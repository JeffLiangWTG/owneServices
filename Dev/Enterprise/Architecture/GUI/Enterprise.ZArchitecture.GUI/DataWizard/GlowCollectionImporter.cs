using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.DataTransfer;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IGlowCollectionImporter
	{
		[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		bool PopulateFromDataRows(IBusinessObjectCollection collection, IEnumerable<ImportPreview> dataRows, MappingDataModel mappingDataModel, GlowLog log, IProgressReporter progressReporter);
	}

	class GlowCollectionImporterCreationSource : IBusinessObjectCreationSource
	{
		public string CreationSourceCode => "ADW";
	}

	class GlowCollectionImporter : IGlowCollectionImporter
	{
		public bool PopulateFromDataRows(IBusinessObjectCollection collection, IEnumerable<ImportPreview> dataRows, MappingDataModel mappingDataModel, GlowLog log, IProgressReporter progressReporter)
		{
			Argument.NotNull(collection, "collection");
			Argument.NotNull(dataRows, "dataRows");
			Argument.NotNull(log, "appendLog");

			var objectsToDelete = new HashSet<BusinessObject>();
			var dataDefinitionName = GlowDataDefinitionReference.FromType(collection.TypeOfElements)?.DataDefinitionName;
			var hasErrors = false;
			var mainLineHeaders = dataRows.FirstOrDefault()?.LineHeaders.First().HeaderValues.ToArray();

			using (CreationStack.PushCreationSource(new GlowCollectionImporterCreationSource()))
			using (DataImportIndicatorService.StartDataImport(collection.Factory))
			using (collection.SuspendAdditionallyForImport())
			using (collection.SuspendListChanged())
			{
				var businessObjectRelationshipFilterProvider = collection as IBusinessObjectRelationshipFilterProvider;
				foreach (var dataRow in dataRows)
				{
					progressReporter.ReportOneItemProcessed();

					var previewLineValues = dataRow.TopLevelLine.PreviewLineValues.ToArray();
					var previewLineDetails = previewLineValues.Where(x => x != null).Select(x => x?.PreviewLineDetails).ToArray();
					var bizObj = GetOrCreateBizo(collection, mainLineHeaders, previewLineDetails, dataDefinitionName, mappingDataModel, log);
					var glowCollectionContext = new GlowCollectionContext(bizObj, mappingDataModel, dataDefinitionName, log);

					using (SupportDataImportingHelper.DataImporting(bizObj))
					{
						// The use of bitwise-or is intentional!
						// we try to import as much as possible regardless of earlier
						// errors. The user can always modify the result in the UI
						hasErrors |= ImportData(glowCollectionContext, dataRow.TopLevelLine.RowIndex, mainLineHeaders, previewLineValues, dataRow.LineHeaders, dataRow.TopLevelLine.ChildLines, businessObjectRelationshipFilterProvider, objectsToDelete);
					}
				}
				foreach (var obj in objectsToDelete)
				{
					if (obj != null)
					{
						var errorMessage = ResString.GetMultilingualString("86C790E0-DA4E-4E72-857C-5ADA7560EFA9", "{0} importing has failed. The incomplete {0} has been deleted as a result", obj.HumanReadableName, obj.ToString()).ToString();
						obj.Delete();
						log.AppendLog(LogType.Error, errorMessage, 0);
					}
				}
			}

			return !hasErrors;
		}

		static bool ImportData(GlowCollectionContext context, int rowIndex, string[] headers, ImportPreviewLineDetails[] values, IEnumerable<ImportPreviewHeader> childLineHeaders, IEnumerable<ImportPreviewLine> childLines, IBusinessObjectRelationshipFilterProvider businessObjectRelationshipFilterProvider, HashSet<BusinessObject> objectsToDelete)
		{
			var hasErrors = false;

			using (var suspender = new SetterValueWithSuspender(context.BusinessObject, headers))
			{
				// The use of bitwise-or is intentional!
				// we try to import as much as possible regardless of earlier
				// errors. The user can always modify the result in the UI
				hasErrors |= ValidateHeaderAndValuesLengthMismatch(headers, values, context.Log);
				hasErrors |= MapBusinessObject(context, rowIndex, headers, values, suspender.SetValue, businessObjectRelationshipFilterProvider, objectsToDelete);
				hasErrors |= ImportChildLines(context, childLineHeaders, childLines, objectsToDelete);
			}

			return hasErrors;
		}

		static bool ImportChildLines(GlowCollectionContext context, IEnumerable<ImportPreviewHeader> lineHeaders, IEnumerable<ImportPreviewLine> childLines, HashSet<BusinessObject> objectsToDelete)
		{
			var hasErrors = false;
			var enterpriseRootType = context.BusinessObject.GetType();
			foreach (var childLine in childLines)
			{
				var colHeader = lineHeaders.Single(h => h.HeaderID == childLine.HeaderID);
				// Removes the first part of the header up to the first dot.
				// ie: RateLine.TL_Calculator => TL_Calculator
				// ie: RateLine.AccChargeCode.AC_Code => AccChargeCode.AC_Code
				var childHeaders = colHeader.HeaderValues.Select(h => h.Substring(childLine.HeaderID.Length + 1)).ToArray();
				var childValues = childLine.PreviewLineValues.ToArray();
				var glowCollectionName = childLine.HeaderID.Split('.').Last();

				MappingDataDefinitionCollection glowCollection;
				try
				{
					glowCollection = context.MappingDataModel.GetCollection(context.DataDefinitionName, glowCollectionName);
				}
				catch (KeyNotFoundException)
				{
					hasErrors = true;
					context.Log.AppendLog(LogType.Error, ResString.GetMultilingualString("e514ea6b-e523-44a3-b872-48fb6c9b08a7", "Could not find element type for collection ID '{0}'.", glowCollectionName).ToString(), 0);
					continue;
				}

				var customImporter = GetCustomImporter(context.BusinessObject);
				if (childLine.ChildLines.Count == 0 && customImporter != null && customImporter.ShouldCustomizeChildrenImport)
				{
					hasErrors |= !customImporter.ImportChildlessChildren(context.BusinessObject, context.Log, childLine.RowIndex, childHeaders, childValues);
				}
				else
				{
					var childCollection = GetChildCollection(context.BusinessObject, enterpriseRootType, context.MappingDataModel, glowCollection);

					if (childCollection != null)
					{
						var businessObjectRelationshipFilterProvider = childCollection as IBusinessObjectRelationshipFilterProvider;
						var item = GetOrCreateBizo(childCollection, childHeaders, childValues.Select(x => x?.PreviewLineDetails).ToArray(), glowCollection.ElementDataDefinitionName, context.MappingDataModel, context.Log);
						var glowCollectionContext = new GlowCollectionContext(item, context.MappingDataModel, glowCollection.ElementDataDefinitionName, context.Log);

						using (SupportDataImportingHelper.DataImporting(item))
						{
							hasErrors |= ImportData(glowCollectionContext, childLine.RowIndex, childHeaders, childValues, lineHeaders, childLine.ChildLines, businessObjectRelationshipFilterProvider, objectsToDelete);
						}
					}
					else
					{
						context.Log.AppendLog(LogType.Warning, ResString.GetMultilingualString(
							"e4eb1ed9-341e-4ce5-965d-d4a3fae1f45a",
							"Importing into this list is not supported '{0}'. Please log an eRequest and WiseTech Global will look at adding this functionality.",
							glowCollectionName).ToString(), 0);
					}
				}
			}

			return hasErrors;
		}

		static IGlowCustomImporter GetCustomImporter(BusinessObject objectWhoseChildrenAreBeingImported)
		{
			var hashTable = (Hashtable)ObjectFactory.Get("GlowCustomImporters"); // Name of an object in the ObjectFactory
			var objectHandle = hashTable[objectWhoseChildrenAreBeingImported.GetType().Name] as ObjectHandle;

			return objectHandle?.GetObject() as IGlowCustomImporter;
		}

		static IBusinessObjectCollection GetChildCollection(BusinessObject rootElement, Type enterpriseRootType, MappingDataModel mappingDataModel, MappingDataDefinitionCollection mappingCollection)
		{
			var tableCode = mappingDataModel.GetTableCode(mappingCollection.ElementDataDefinitionName);
			if (!string.IsNullOrEmpty(tableCode))
			{
				if (tableCode == JobDocAddressSchema.Constants.Prefix)
				{
					var addressCollectionProperty = enterpriseRootType.GetProperties().FirstOrDefault(p => p.PropertyType.GetInterfaces().Contains(typeof(IJobDocAddressDependentCollection)));
					if (addressCollectionProperty != null)
					{
						return addressCollectionProperty.GetValue(rootElement) as IJobDocAddressDependentCollection;
					}

					return null;
				}
				else if (tableCode == CusEntryNumSchema.Constants.Prefix)
				{
					var referenceNumberCollectionProperty = enterpriseRootType.GetProperties().FirstOrDefault(p => p.PropertyType.GetInterfaces().Contains(typeof(Customs.ICusEntryNumAdditionalReferenceCollection)));
					if (referenceNumberCollectionProperty != null)
					{
						return referenceNumberCollectionProperty.GetValue(rootElement) as Customs.ICusEntryNumAdditionalReferenceCollection;
					}

					return null;
				}

				var namedCollection = GetCollectionByName(rootElement, enterpriseRootType, mappingCollection.CollectionName);

				// We cannot compare types directly. The type obtained from a table code can be an abstract type (e.g. CusAddInfo), and collection can have a concrete type of elements.
				if (namedCollection != null && BusinessObjectFactory.GetTableCodeFromType(namedCollection.TypeOfElements) == tableCode)
				{
					return namedCollection;
				}

				var enterpriseItemType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(tableCode);
				return ObjectFactory.Get<IDependentCollectionPropertyHelper>().GetDependentCollection(rootElement, enterpriseItemType, enterpriseRootType);
			}

			return null;
		}

		static IBusinessObjectCollection GetCollectionByName(BusinessObject businessObject, Type businessObjectType, string collectionName)
		{
			var collectionProperty = businessObjectType.GetProperties().FirstOrDefault(p => p.Name == collectionName && typeof(IBusinessObjectCollection).IsAssignableFrom(p.PropertyType));
			return (IBusinessObjectCollection)collectionProperty?.GetValue(businessObject);
		}

		// Note: The 'newObj' being mapped could actually be an already-existing object that was re-used. Currently the
		// reused object gets updated with the incoming values again. If the re-used object was matched based on a
		// single key then the values it has can change to different values. If the re-used object was matched by
		// matching the incoming values then it is updated to the same values it already has and may slow things down
		// unnecessarily.
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		static bool MapBusinessObject(GlowCollectionContext context, int rowIndex, string[] headers, ImportPreviewLineDetails[] values, Action<BusinessObject, string, object> setValue, IBusinessObjectRelationshipFilterProvider businessObjectRelationshipFilterProvider, HashSet<BusinessObject> objectsToDelete)
		{
			var hasErrors = false;
			var navProperties = new List<(string[] propertyPath, ImportPreviewLineDetails importPreviewLineDetails)>();

			// Earlier in the code there's a check for mis-matched lengths. It
			// records an error in the Log and continues. This check here is to
			// ensure that this code can tolerate a mismatch in length without
			// blowing up
			var length = Math.Min(values.Length, headers.Length);

			var wrappedPropertySupporter = context.BusinessObject as IImportWrappedPropertySupporter;
			var customGlowImporter = GetCustomImporter(context.BusinessObject);
			for (var i = 0; i < length; i++)
			{
				var propertyName = headers[i];
				propertyName = wrappedPropertySupporter?.GetWrappedProperty(propertyName) ?? propertyName;

				var previewLineDetails = values[i]?.PreviewLineDetails?.Trim() ?? string.Empty;
				var allowEmptyString = customGlowImporter?.IsEmptyValueAllowed(propertyName) ?? false;
				if (!string.IsNullOrEmpty(previewLineDetails) || allowEmptyString)
				{
					if (!propertyName.Contains("."))
					{
						if (SetValues(context.BusinessObject, propertyName, values[i], rowIndex, setValue, context.Log, customGlowImporter, ref hasErrors))
						{
							objectsToDelete.Add(context.BusinessObject);
						}
					}
					else
					{
						navProperties.Add((propertyName.Split('.'), values[i]));
					}
				}
			}

			GetOrCreateRelatedObj(navProperties, context, customGlowImporter, businessObjectRelationshipFilterProvider, rowIndex, setValue, ref hasErrors);

			return hasErrors;
		}

		static void GetOrCreateRelatedObj(
			List<(string[] propertyPath, ImportPreviewLineDetails importPreviewLineDetails)> navProperties,
			GlowCollectionContext context,
			IGlowCustomImporter customGlowImporter,
			IBusinessObjectRelationshipFilterProvider businessObjectRelationshipFilterProvider,
			int rowIndex,
			Action<BusinessObject, string, object> setValue,
			ref bool hasErrors)
		{
			var newNavProperties = new List<(string[] propertyPath, ImportPreviewLineDetails importPreviewLineDetails)>();
			var matcher = ObjectFactory.Get<IEntityMatcher>();
			var matcherContext = new EntityMatcherContext(context.BusinessObject.Factory, context.MappingDataModel, context.BusinessObject, businessObjectRelationshipFilterProvider);

			foreach (var group in navProperties.GroupBy(n => n.propertyPath.First(), n => (propertyName: string.Join(".", n.propertyPath.Skip(1)), n.importPreviewLineDetails)))
			{
				try
				{
					var relation = context.MappingDataModel.GetRelation(context.DataDefinitionName, group.Key);
					var relatedObj = matcher.GetMatchingBusinessObject(matcherContext, relation.TargetDataDefinitionName, group.Select(n => (n.propertyName, n.importPreviewLineDetails.PreviewLineDetails)), relation.CanCreate);

					if (relation.CanCreate && relatedObj == null)
					{
						relatedObj = CreateNewBusinessObject(context, relation, rowIndex, setValue, customGlowImporter, group, newNavProperties, ref hasErrors);
					}

					SetForeignKey(context.BusinessObject, relation.ForeignKeyProperty, relatedObj, context.Log);
				}
				catch (EntityMatchingException ex)
				{
					context.Log.AppendLog(LogType.Warning, ex.Message, 0);
				}
			}
		}

		static BusinessObject CreateNewBusinessObject(
			GlowCollectionContext context,
			MappingDataDefinitionRelation relation,
			int rowIndex,
			Action<BusinessObject, string, object> setValue,
			IGlowCustomImporter customGlowImporter,
			IGrouping<string, (string propertyName, ImportPreviewLineDetails importPreviewLineDetails)> group,
			List<(string[] propertyPath, ImportPreviewLineDetails importPreviewLineDetails)> newNavProperties,
			ref bool hasErrors)
		{
			var tableCode = context.MappingDataModel.GetTableCode(relation.TargetDataDefinitionName);
			var bizoType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(tableCode);
			var relatedObj = context.BusinessObject.Factory.New(bizoType);

			var wrappedPropertySupporter = relatedObj as IImportWrappedPropertySupporter;

			foreach (var matchingValue in group)
			{
				var propertyName = matchingValue.propertyName;
				propertyName = wrappedPropertySupporter?.GetWrappedProperty(propertyName) ?? propertyName;

				if (!propertyName.Contains("."))
				{
					SetValues(relatedObj, propertyName, matchingValue.importPreviewLineDetails, rowIndex, setValue, context.Log, customGlowImporter, ref hasErrors);
				}
				else
				{
					newNavProperties.Add((propertyName.Split('.'), matchingValue.importPreviewLineDetails));
				}
			}

			return relatedObj;
		}

		static bool SetValues(BusinessObject newObj, string propertyName, ImportPreviewLineDetails importPreviewLineDetails, int rowIndex, Action<BusinessObject, string, object> setValue, GlowLog log, IGlowCustomImporter glowCustomImporter, ref bool hasErrors)
		{
			if (newObj.FindPropertyInfo(propertyName)?.MaxLength != -1 && newObj.FindPropertyInfo(propertyName)?.MaxLength < importPreviewLineDetails.PreviewLineDetails.Length)
			{
				hasErrors = true;
				var errorMessage = ResString.GetMultilingualString("4796EE06-BACF-4754-9BBF-655F9E44A136", "The value '{0}' exceeds character limit for the field '{1}' in Row {2} in Column {3}. Max Length is {4}.", importPreviewLineDetails.PreviewLineDetails, propertyName, rowIndex, importPreviewLineDetails.ColumnIndex, newObj.FindPropertyInfo(propertyName).MaxLength).ToString();
				log.AppendLog(LogType.Error, errorMessage, 0);
				return true;
			}
			try
			{
					if (newObj.FindPropertyInfo(propertyName) != null)
					{
						setValue(newObj, propertyName, TypeConverting.ConvertToType(importPreviewLineDetails.PreviewLineDetails, propertyName, newObj));
					}
					else
					{
						glowCustomImporter.ConvertCustomLine(newObj, importPreviewLineDetails.PreviewLineDetails, log, rowIndex, propertyName);
					}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				hasErrors = true;
				var propertyTypeName = TypeConverting.GetPropertyTypeFriendlyName(newObj, propertyName);
				var errorMessage = propertyTypeName != null ?
					ResString.GetMultilingualString("e0c5f510-2248-43dc-aadd-0c84c2e76b55", "Unable to set value '{0}' to the field '{1}', as this field expects '{2}', error in Row {3} in Column {4}.", importPreviewLineDetails.PreviewLineDetails, propertyName, propertyTypeName, rowIndex, importPreviewLineDetails.ColumnIndex) :
					ResString.GetMultilingualString("679820d9-cf05-4460-9478-f674835d81e1", "{0} does not have property {1} in Row {2} in Column {3}.", newObj.GetType().Name, propertyName, rowIndex, importPreviewLineDetails.ColumnIndex);
				log.AppendLog(LogType.Error, errorMessage, 0);
			}
			return false;
		}

		static void SetForeignKey(BusinessObject newObj, string foreignKeyProperty, BusinessObject relatedObj, GlowLog log)
		{
			var foreignKeyType = newObj.GetPropertyType(foreignKeyProperty);

			if (foreignKeyType == null)
			{
				// todo: 3rd parameter (progress) is not used, it should be removed from GlowLog
				log.AppendLog(LogType.Warning, FormattableString.Invariant($"{newObj.GetType().Name} does not have property {foreignKeyProperty}."), 0); // message is intended primarily for developers
			}
			else if (foreignKeyType == typeof(ZString))
			{
				var foreignKeyString = CodePropertyAttribute.CodeFromBusinessObject(relatedObj);
				if (newObj[foreignKeyProperty] == null || !newObj[foreignKeyProperty].Equals(foreignKeyString))
				{
					newObj[foreignKeyProperty] = foreignKeyString;
				}
			}
			else
			{
				if (newObj[foreignKeyProperty] == null || !newObj[foreignKeyProperty].Equals(relatedObj.PK))
				{
					newObj[foreignKeyProperty] = relatedObj.PK;
				}
			}
		}

		static IBusinessObjectCreationSourceStack CreationStack => ObjectFactory.Get<IBusinessObjectCreationSourceStack>();

		/// <summary>
		/// Returns True if the header and values arrays have different length.
		/// This can typically only happen if either the glow service that converts
		/// the input csv/xls file into a hierarchy has failed, or (most likely)
		/// the user has configured the mapping incorrectly causing the glow service
		/// to get confused.
		///
		/// It is very tempting to stop importing asap, however continuing can
		/// give the user a clue as to where the problem occurred. 
		/// </summary>
		static bool ValidateHeaderAndValuesLengthMismatch(string[] propertyNames, ImportPreviewLineDetails[] propertyValues, GlowLog log)
		{
			if (propertyNames.Length != propertyValues.Length)
			{
				log.AppendLog(
					LogType.Error,
					ResString.GetMultilingualString(
						"62ddce37-8d6f-4e79-9b1a-311b669ae471",
						"There is an issue with your ADAW mapping. Please check that the ‘Map Data From’ and ‘Map Data To’ fields are correct, " +
						"and also other warnings that may help identify the problem. If you recently made changes to the mapping, " +
						"try waiting a few hours or re-importing the mapping."),
					0);
				return true;
			}
			return false;
		}

		#region Matching Bizos for reuse

		static BusinessObject GetOrCreateBizo(IBusinessObjectCollection collection, string[] headers, string[] values, string dataDefinitionName, MappingDataModel mappingDataModel, GlowLog log)
		{
			if (collection is IImportCollectionElementMatchingSupporter matchingSupporter)
			{
				return GetBizoBasedOnKey(matchingSupporter, headers, values, dataDefinitionName)
					?? GetBizoBasedOnColumnMatches(collection, matchingSupporter, headers, values, dataDefinitionName, mappingDataModel, log)
					?? collection.AddNew();
			}

			return collection.AddNew();
		}

		/// <summary>
		/// Attempts to find a bizo using a key.
		/// The key is provided by IImportCollectionElementMatchingSupporter.MatchingColumnName. 
		/// The key is searched in the headers and if found, then its value compared
		/// against the bizos in the collection; and if found, returned.
		/// </summary>
		static BusinessObject GetBizoBasedOnKey(IImportCollectionElementMatchingSupporter matchingSupporter, string[] headers, string[] values, string dataDefinitionName)
		{
			var indexOfKeyForMatching = headers.IndexOf(h => h == matchingSupporter.MatchingColumnName);

			var isIndexOutOfBounds = indexOfKeyForMatching >= values.Length || indexOfKeyForMatching < 0;
			if (isIndexOutOfBounds)
			{
				// Index being out of bounds means that the matching column name key
				// was not in the headers being passed in, so we will not try get
				// a matching bizo - in case it matches a blank string.
				return null;
			}

			var valueOfMatchingColumn = values.GetValue(indexOfKeyForMatching)?.ToString();
			var bizObj = matchingSupporter.GetMatchingBizObject(valueOfMatchingColumn);

			if (bizObj != null)
			{
				matchingSupporter.PrepareForReuse(bizObj);
			}

			return bizObj;
		}

		/// <summary>
		/// Attempts to find a bizo by comparing all the headers/values in the
		/// input against those in the bizos in the collection.
		///
		/// A match is found if all the column/values match.
		/// Note: This accuracy of the match depends wholly by the number of
		/// header/values given. If few header/values are given, then it may
		/// match an unexpected (to the user) bizo.
		///
		/// Note2: Due to defaulting-abilities of Bizos a match may not actually
		/// be found for a bizo that was created on an earlier run of this function
		/// even though the input data is the same.
		/// </summary>
		static BusinessObject GetBizoBasedOnColumnMatches(IBusinessObjectCollection collection, IImportCollectionElementMatchingSupporter matchingSupporter, string[] headers, string[] values, string dataDefinitionName, MappingDataModel mappingDataModel, GlowLog log)
		{
			if (matchingSupporter.FindGenericColumnMatches)
			{
				var matchedBizoResult = GetMatchingBizObjects(collection, mappingDataModel, dataDefinitionName, headers, values);

				// Take(2) is used as the number of matched bizos could be large,
				// and only two are needed here.
				var firstTwoItems = matchedBizoResult.Take(2).ToArray();
				// not using array indices as the array may be a length of 0, 1 or 2
				var firstItem = firstTwoItems.Take(1).SingleOrDefault();
				var secondItem = firstTwoItems.Skip(1).Take(1).SingleOrDefault();

				if (secondItem == null && firstItem != null) // If there is a single match
				{
					if (matchingSupporter.IsGenericColumnMatchingAllowed)
					{
						matchingSupporter.PrepareForReuse(firstItem);
						return firstItem;
					}
					else
					{
						log.AppendLog(LogType.Warning, ResString.GetMultilingualString("8e718299-26db-46e4-ba73-fef895d56858", "{0}: Found a matching {0} but appending to it is not supported. Creating a new {0} instead.", dataDefinitionName), 0);
					}
				}
				else if (secondItem != null) // there are two (or more) matches
				{
					if (matchingSupporter.IsGenericColumnMatchingAllowed)
					{
						log.AppendLog(LogType.Warning, ResString.GetMultilingualString("43f2d778-a675-4e01-a9cd-b15f3577fdf3", "{0}: Found multiple matching {0} but cannot decide which to use. Creating a new {0} instead.", dataDefinitionName), 0);
					}
					else
					{
						log.AppendLog(LogType.Warning, ResString.GetMultilingualString("a208802b-2c38-4ed4-a6c3-f406da024516", "{0}: Found multiple matching {0} but appending to them is not supported. Creating a new {0} instead.", dataDefinitionName), 0);
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Filters the `collection` according to propertyNames and propertyValues
		/// </summary>
		/// <param name="propertyNames">
		/// The property names that are used to match business objects.
		///
		/// Each item in this array may be in the form of
		/// TI_TransitTime or OrgHeader.Code
		/// </param>
		/// <param name="propertyValues">
		/// An array of values that we expect to find on the property
		/// corresponding to the same-indexed propertyName.
		///
		/// ie: Bizo[propertyName[x]] == propertyValue[x]
		/// </param>
		/// <returns>
		/// An enumeration containing the matched Bizos. Will never return null.
		/// </returns>
		static IEnumerable<BusinessObject> GetMatchingBizObjects(
			IBusinessObjectCollection collection,
			MappingDataModel mappingDataModel,
			string dataDefinitionName,
			string[] propertyNames,
			string[] propertyValues)
		{
			if (propertyNames.Length == 0 || propertyNames.Length != propertyValues.Length)
			{
				yield break;
			}

			var propertyNamesAndValues = propertyNames.Zip(propertyValues, (propertyName, value) => new { PropertyName = propertyName, Value = value });
			var splitOnPropertyKind = propertyNamesAndValues.Split((current) => current.PropertyName.Contains("."));
			var directProperties = splitOnPropertyKind.NonMatchingSet.ToList();
			var foreignKeyProperties =
				splitOnPropertyKind.MatchingSet.Select(
					(current) => new { Path = current.PropertyName.Split('.'), Value = current.Value });
			var foreignKeyPropertiesGroupedByForeignTable =
				foreignKeyProperties.GroupBy(
					(current) => current.Path[0],
					(current) => (string.Join(".", current.Path.Skip(1)), current.Value))
				.ToList();

			foreach (BusinessObject bizo in collection)
			{
				var matches = true;

				foreach (var property in directProperties)
				{
					matches &= MatchDirectProperty(bizo, property.PropertyName, property.Value);
					if (!matches)
					{
						break;
					}
				}

				if (!matches)
				{
					continue;
				}

				foreach (var foreignTableMatchCriteria in foreignKeyPropertiesGroupedByForeignTable)
				{
					matches &= MatchForeignKeyProperty(mappingDataModel, dataDefinitionName, bizo, foreignTableMatchCriteria);
					if (!matches)
					{
						break;
					}
				}

				if (matches)
				{
					yield return bizo;
				}
			}
		}

		/// <summary>
		/// Checks that the bizo's reference to a foreign object matches
		/// the foreign objects keys. This requires the foreign object
		/// to exist and be valid
		/// </summary>
		/// <param name="foreignTableMatchCriteria">
		/// This is a mapping from table-name to a list of tuples, each
		/// which consists of a field in the table-name and its value.
		///
		/// Eg: Commodity -> [(RH_Code, GEN), (RH_Description, General)]
		/// </param>
		static bool MatchForeignKeyProperty(
			MappingDataModel mappingDataModel,
			string dataDefinitionName,
			BusinessObject bizo,
			IGrouping<string, (string FieldName, string Value)> foreignTableMatchCriteria)
		{
			var matcher = ObjectFactory.Get<IEntityMatcher>();
			var key = foreignTableMatchCriteria.Key;

			try
			{
				var relation = mappingDataModel.GetRelation(dataDefinitionName, key);
				var foreignKey = bizo[relation.ForeignKeyProperty];

				var matcherContext = new EntityMatcherContext(bizo.Factory, mappingDataModel, bizo);
				var foreignObject = matcher.GetMatchingBusinessObject(matcherContext, relation.TargetDataDefinitionName, foreignTableMatchCriteria);

				// The found foreign object then needs to be compared against the bizo
				// to ensure it refers to the same foreign-object. The bizo only has one
				// thing to compare against. Either a foreign-key of ZGuid type, or a natural key.
				if (foreignKey is ZGuid foreignPK)
				{
					return foreignPK == foreignObject.PK;
				}
				else if (foreignObject is ICodeDescription codeDescription)
				{
					return foreignKey.Equals(codeDescription.Code);
				}
			}
			catch (EntityMatchingException) // no match found
			{
			}

			return false;
		}

		static bool MatchDirectProperty(BusinessObject bizo, string propertyName, string propertyValue)
		{
			try
			{
				var convertedIncomingValue = TypeConverting.ConvertToType(propertyValue, propertyName, bizo);
				return bizo[propertyName].Equals(convertedIncomingValue);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				// When a problem occurs - do not bother reporting it.
				// Instead don't match and a new object will be created instead
				// later, when the value is set for the new object, the error will be seen.
				return false;
			}
		}

		#endregion
	}
}
