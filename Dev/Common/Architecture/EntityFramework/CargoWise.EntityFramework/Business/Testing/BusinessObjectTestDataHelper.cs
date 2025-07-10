#if DEBUG
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Schema;
using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.EntityFramework
{
	public interface IBusinessObjectTestDataHelperPropertiesToExclude
	{
		bool ShouldExcludeFromFillWithValidTestData(ZString name);
	}

	public class BusinessObjectTestDataHelper
	{
		readonly bool _generateRandomUniqueStringForProperty;

		public BusinessObjectTestDataHelper(bool generateRandomUniqueStringForProperty = false)
		{
			_generateRandomUniqueStringForProperty = generateRandomUniqueStringForProperty;
		}

		public BusinessObject NewWithValidTestData(BusinessObjectFactory factory, Type bizObjType, TestBusinessObjectKind kind)
		{
			BusinessObject result = null;
			AttributeCollection attributes = TypeDescriptor.GetAttributes(bizObjType);
			DependentBusinessObjectAttribute attribute = attributes[typeof(DependentBusinessObjectAttribute)] as DependentBusinessObjectAttribute;
			if (attribute != null && attribute.MasterType != null)
			{
				BusinessObject master = NewWithValidTestData(factory, attribute.MasterType, kind);
				IBusinessObjectCollection collection = attribute.GetDependentCollection(master);
				if (collection != null)
				{
					result = collection.AddNew();

					if (!bizObjType.IsInstanceOfType(result))
					{
						throw new InvalidOperationException(
							"The collection on property " +
							attribute.MasterType.FullName + "." +
							attribute.DetailRelationshipCollectionProperty +
							" returned element type '" + result.GetType().FullName + "' expected '" + bizObjType.FullName + "'");
					}
				}
			}
			if (result == null)
			{
				EnsureAllNotNullableFKsAccountedFor(ObjectFactory.Get<IApplicationSchemaResolver>(), bizObjType);
				result = factory.New(bizObjType);
			}
			result.TestDataHelper = this;
			result.FillWithValidTestData(kind, Array.Empty<PropertyDescriptor>());

			return result;
		}

		public void FillWithValidTestData(BusinessObject bizObj, TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			((IBusinessObjectInternals)bizObj).IsCopying = true;
			var schemaResolver = ObjectFactory.Get<IApplicationSchemaResolver>();

			try
			{
				if ((kind & TestBusinessObjectKind.PopulateNumbers) != 0)
				{
					PopulateNumbers(bizObj);
				}
				if ((kind & TestBusinessObjectKind.PopulateStrings) != 0)
				{
					PopulateStrings(bizObj);
				}
				if ((kind & TestBusinessObjectKind.PopulateDependentCollections) != 0)
				{
					FillDependentCollectionsWithData(bizObj, kind, propertyPath);
				}
				if ((kind & TestBusinessObjectKind.PopulateManyToManyCollections) != 0)
				{
					FillManyToManyCollectionsWithData(bizObj, kind, propertyPath);
				}
				if ((kind & TestBusinessObjectKind.PopulateRelatedObjects) != 0)
				{
					FillRelatedObjectsWithData(bizObj, kind, propertyPath, true, schemaResolver);
				}
				if ((kind & TestBusinessObjectKind.PopulatePortCodes) != 0)
				{
					PopulatePortCodes(bizObj);
				}
				if ((kind & TestBusinessObjectKind.PopulateDates) != 0)
				{
					PopulateDates(bizObj, true, schemaResolver);
				}
				if ((kind & TestBusinessObjectKind.MinimumRequiredToSave) != 0)
				{
					FillWithValidTestDataForSavableBusinessObject(bizObj, kind, propertyPath, schemaResolver);
				}
			}
			finally
			{
				((IBusinessObjectInternals)bizObj).IsCopying = false;
			}
		}

		#region FillDependentCollectionsWithData / FillRelatedObjectsWithData

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1080:DoNotUseBusinessObjectCollectionIsAssignableFrom", Justification = "Baseline")]
		void FillDependentCollectionsWithData(BusinessObject bizObj, TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(bizObj))
			{
				if ((typeof(IDependentBusinessObjectCollection).IsAssignableFrom(property.PropertyType) || !typeof(BusinessObjectCollection).IsAssignableFrom(property.PropertyType)) &&
					typeof(IBusinessObjectCollection).IsAssignableFrom(property.PropertyType) &&
					property.Attributes[typeof(BusinessObjectTestExclude)] == null)
				{
					IBusinessObjectCollection collection = (IBusinessObjectCollection)property.GetValue(bizObj);
					if (IsDependentCollection(collection))
					{
						FillDependentCollectionWithData(collection, kind, property, propertyPath);
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1080:DoNotUseTypeofIsAssignableFrom", Justification = "Baseline")]
		void FillManyToManyCollectionsWithData(BusinessObject bizObj, TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(bizObj))
			{
				if ((typeof(ManyToManyBusinessObjectCollection).IsAssignableFrom(property.PropertyType) || !typeof(BusinessObjectCollection).IsAssignableFrom(property.PropertyType)) &&
					typeof(IBusinessObjectCollection).IsAssignableFrom(property.PropertyType) &&
					property.Attributes[typeof(BusinessObjectTestExclude)] == null)
				{
					IBusinessObjectCollection collection = (IBusinessObjectCollection)property.GetValue(bizObj);
					if (IsManyToManyCollection(collection))
					{
						FillDependentCollectionWithData(collection, kind, property, propertyPath);
					}
				}
			}
		}

		bool IsDependentCollection(IBusinessObjectCollection collection)
		{
			var legacyCollection = collection as IDependentBusinessObjectCollection;
			var activeCollection = collection as IActiveBusinessObjectCollection;
			return
				(legacyCollection != null && legacyCollection.Master != null) ||
				(activeCollection != null && activeCollection.Relationship is DependentRelationship);
		}

		bool IsManyToManyCollection(IBusinessObjectCollection collection)
		{
			ManyToManyBusinessObjectCollection legacyCollection = collection as ManyToManyBusinessObjectCollection;
			IActiveBusinessObjectCollection activeCollection = collection as IActiveBusinessObjectCollection;
			return
				legacyCollection != null ||
				(activeCollection != null && activeCollection.Relationship is ManyToManyRelationship);
		}

		protected virtual void FillDependentCollectionWithData(IBusinessObjectCollection collection, TestBusinessObjectKind kind, PropertyDescriptor collectionProperty, PropertyDescriptor[] propertyPath)
		{
			if (collection is BusinessObjectCollection legacyCollection)
			{
				legacyCollection.IsManagedForDataRefresh = false;
			}

			if (collection != null &&
				collection.Count == 0 &&
				collection.AllowNew)
			{
				BusinessObject dependent = collection.AddNew();
				if ((kind & TestBusinessObjectKind.PopulateRelationsDeeply) != 0 &&
					!((IList)propertyPath).Contains(collectionProperty) &&
					propertyPath.Length <= 5)
				{
					dependent.FillWithValidTestData(kind, AppendToPropertyPath(propertyPath, collectionProperty));
				}
				else if ((kind & TestBusinessObjectKind.MinimumRequiredToSave) != 0)
				{
					dependent.FillWithValidTestData(TestBusinessObjectKind.MinimumRequiredToSave, AppendToPropertyPath(propertyPath, collectionProperty));
				}
			}
		}

		void FillRelatedObjectsWithData(BusinessObject bizObj, TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath, bool includeNullableProperties, IApplicationSchemaResolver schemaResolver)
		{
			if (!(bizObj is NonPersistentBusinessObject))
			{
				var constraints = DbConstraintReader
					.GetInstance(bizObj.TableName)
					.Constraints
					.GetConstraintsOfType(DbConstraintType.ForeignKey);

				foreach (var constraint in constraints)
				{
					foreach (var columnName in constraint.ColumnNames)
					{
						var propertyInfo = GetPropertyInfo(bizObj, columnName);
						if (propertyInfo != null &&
							propertyInfo.PropertyType == typeof(ZGuid) &&
							!propertyInfo.Value.IsValid)
						{
							if (includeNullableProperties ||
								!schemaResolver.GetSchemaColumn(columnName, propertyInfo.BizObj.TableName).IsNullable)
							{
								PopulateFK(propertyInfo, kind, propertyPath);
							}
						}
					}
				}
			}
		}

		#endregion

		#region PopulateStrings / PopulateNumbers / PopulatePortCodes / PopulateDates

		void PopulateStrings(BusinessObject bizObj)
		{
			foreach (var property in GetNonWrappingPropertyInfo(bizObj))
			{
				if (typeof(ZString).IsAssignableFrom(property.PropertyType) &&
					!property.Name.EndsWith(Schema.Schema.ParentTableCodeColumnSuffix, StringComparison.OrdinalIgnoreCase))
				{
					PopulateString(property);
				}
			}
		}

		protected virtual void PopulateString(ZPropertyInfo property)
		{
			ZWrappedPropertyInfo wrapped = property as ZWrappedPropertyInfo;
			if (!(property is ZPhantomPropertyInfo) && !(wrapped != null && wrapped.InnerInfo is ZPhantomPropertyInfo))
			{
				property.Value = GetTestPropertyStringBasedOnPropertyName(property.Name, property.MaxLength);
			}
		}

		void PopulateNumbers(BusinessObject bizObj)
		{
			foreach (var property in GetNonWrappingPropertyInfo(bizObj))
			{
				if (!property.Name.EndsWith(Schema.Schema.ClusterKeyColumnSuffix, StringComparison.OrdinalIgnoreCase))
				{
					Random random = new Random(CalculateStringHashCodeWithV1Algorithm(property.Name));
					int sampleNumber = random.Next(254) + 1;
					if (typeof(ZInt).IsAssignableFrom(property.PropertyType))
					{
						property.Value = (ZInt)sampleNumber;
					}
					if (typeof(ZDecimal).IsAssignableFrom(property.PropertyType))
					{
						property.Value = (ZDecimal)sampleNumber;
					}
					if (typeof(ZShort).IsAssignableFrom(property.PropertyType))
					{
						property.Value = (ZShort)sampleNumber;
					}
				}
			}
		}

		void PopulatePortCodes(BusinessObject bizObj)
		{
			foreach (var property in GetNonWrappingPropertyInfo(bizObj))
			{
				if (typeof(ZString).IsAssignableFrom(property.PropertyType) && property.Name.IndexOf("_RL_NK") != -1)
				{
					property.Value = GetSamplePortCode(property.Name);
				}
			}
		}

		void PopulateDates(BusinessObject bizObj, bool includeNullableProperties, IApplicationSchemaResolver schemaResolver)
		{
			foreach (var property in GetNonWrappingPropertyInfo(bizObj))
			{
				if (property == null)
				{
					throw new ArgumentNullException("Property");
				}
				if (property.BizObj == null)
				{
					throw new ArgumentNullException("Property.BizObj");
				}
				var schemaColumn = schemaResolver.GetSchemaColumnSafe(property.Name, property.BizObj.TableName);
				var isNullable = (schemaColumn == null || schemaColumn.IsNullable);

				if (property.PropertyType == typeof(ZDateTime) &&
					(!isNullable || includeNullableProperties) &&
					!property.Value.IsValid)
				{
					property.Value = GetSampleDateTime(property.Name);
				}
				else if (property.PropertyType == typeof(ZDate) &&
					(!isNullable || includeNullableProperties) &&
					!property.Value.IsValid)
				{
					property.Value = GetSampleDate(property.Name);
				}
				else if (property.PropertyType == typeof(ZDateTimeOffset) &&
					(!isNullable || includeNullableProperties) &&
					!property.Value.IsValid)
				{
					property.Value = GetSampleDateTimeOffset(property.Name);
				}
				else if (property.PropertyType == typeof(ZTime) &&
					(!isNullable || includeNullableProperties) &&
					!property.Value.IsValid)
				{
					property.Value = GetSampleTime(property.Name);
				}
				else if (property.PropertyType == typeof(ZGeography) &&
						(!isNullable || includeNullableProperties) &&
						!property.Value.IsValid)
				{
					property.Value = GetSampleGeography(property.Name);
				}
			}
		}

		ZDate GetSampleDate(string propertyName)
		{
			var random = new Random(CalculateStringHashCodeWithV1Algorithm(propertyName));
			var portCodeSampleIndex = random.Next(SampleDates.Length);
			return SampleDates[portCodeSampleIndex].Date;
		}

		ZDateTime GetSampleDateTime(string propertyName)
		{
			var random = new Random(CalculateStringHashCodeWithV1Algorithm(propertyName));
			var portCodeSampleIndex = random.Next(SampleDates.Length);
			return SampleDates[portCodeSampleIndex];
		}

		ZDateTimeOffset GetSampleDateTimeOffset(string propertyName)
		{
			var random = new Random(CalculateStringHashCodeWithV1Algorithm(propertyName));
			var portCodeSampleIndex = random.Next(SampleDateOffsets.Length);
			return SampleDateOffsets[portCodeSampleIndex];
		}

		ZTime GetSampleTime(string propertyName)
		{
			var random = new Random(CalculateStringHashCodeWithV1Algorithm(propertyName));
			var portCodeSampleIndex = random.Next(SampleTimes.Length);
			return SampleTimes[portCodeSampleIndex];
		}

		ZString GetSamplePortCode(string propertyName)
		{
			var random = new Random(CalculateStringHashCodeWithV1Algorithm(propertyName));
			var portCodeSampleIndex = random.Next(SamplePortCodes.Length);
			return SamplePortCodes[portCodeSampleIndex];
		}

		ZGeography GetSampleGeography(string propertyName)
		{
			var random = new Random(CalculateStringHashCodeWithV1Algorithm(propertyName));
			var portCodeSampleIndex = random.Next(SampleGeographies.Length);
			return SampleGeographies[portCodeSampleIndex];
		}

		readonly string[] SamplePortCodes = new string[]
		{
			"AUSYD",
			"AUMEL",
			"MYPKG",
			"SGSIN"
		};

		readonly ZDateTime[] SampleDates = new ZDateTime[]
		{
			new ZDateTime(2004, 1, 2),
			new ZDateTime(2004, 3, 4),
			new ZDateTime(2004, 5, 6),
			new ZDateTime(2004, 7, 8),
			new ZDateTime(1998, 1, 2),
			new ZDateTime(1998, 3, 4),
			new ZDateTime(1998, 5, 6),
			new ZDateTime(1998, 7, 8),
		};

		readonly ZDateTimeOffset[] SampleDateOffsets = new ZDateTimeOffset[]
		{
			new ZDateTimeOffset(2004, 1, 2, 1, 2, 3, TimeSpan.Zero),
			new ZDateTimeOffset(2004, 3, 4, 4, 5, 6, TimeSpan.FromHours(1)),
			new ZDateTimeOffset(2004, 5, 6, 7, 8, 9, TimeSpan.FromMinutes(20)),
			new ZDateTimeOffset(2004, 7, 8, 10, 11, 12, TimeSpan.FromHours(14)),
			new ZDateTimeOffset(1998, 1, 2, 13, 14, 15, TimeSpan.FromHours(-14)),
			new ZDateTimeOffset(1998, 3, 4, 17, 18, 19, TimeSpan.FromMinutes(80)),
			new ZDateTimeOffset(1998, 5, 6, 20, 21, 22, TimeSpan.FromMinutes(-80)),
			new ZDateTimeOffset(1998, 7, 8, 23, 24, 25, TimeSpan.FromMinutes(-20)),
		};

		readonly ZTime[] SampleTimes = new ZTime[]
		{
			new ZTime(11, 1),
			new ZTime(11, 3),
			new ZTime(11, 5),
			new ZTime(11, 7),
			new ZTime(22, 1),
			new ZTime(22, 3),
			new ZTime(22, 5),
			new ZTime(22, 7),
		};

		readonly ZGeography[] SampleGeographies = new ZGeography[]
		{
			new ZGeography("-121 48"),
			new ZGeography("-121.1 49"),
			new ZGeography("-121.3 46"),
			new ZGeography("-122 48.1"),
			new ZGeography("-122.8 46.4"),
			new ZGeography("-120 48.1"),
		};

		public static IDisposable TemporarilySetCustomisableStringValue(SetCustomisableStringValueDelegate setCustomisableStringValue)
		{
			var oldSetCustomisableStringValue = setCustomisableStringValue;
			SetCustomisableStringValue = setCustomisableStringValue;

			return new DisposableAction(() => SetCustomisableStringValue = oldSetCustomisableStringValue);
		}

		[ThreadStatic]
		static SetCustomisableStringValueDelegate SetCustomisableStringValue;
		public delegate ZString SetCustomisableStringValueDelegate(string propertyName, int maxLength, ZString value);

		protected ZString GetTestPropertyStringBasedOnPropertyName(string propertyName, int maxLength)
		{
			var result = (propertyName.IndexOf('_') >= 0)
				? StripTablePrefixes(propertyName)
				: propertyName;
			result = ((ZString)result).Substring(0, (maxLength != -1 && result.Length > maxLength) ? maxLength : result.Length);
			if (SetCustomisableStringValue != null)
			{
				result = SetCustomisableStringValue(propertyName, maxLength, result);
			}
			return result;
		}

		string StripTablePrefixes(string propertyName)
		{
			var colNameParts = propertyName.Split('_');
			var i = 0;

			while (i < 2
				&& colNameParts.Length > i
				&& colNameParts[i].Length >= 2
				&& colNameParts[i].Length <= 3
				&& colNameParts[i] == colNameParts[i].ToUpper())
			{
				i++;
			}

			var result = String.Join("_", colNameParts.Skip(i));
			if (result.StartsWith("NK"))
			{
				result = result.Substring(2).ToUpper();
			}

			return result;
		}

		int CalculateStringHashCodeWithV1Algorithm(string s)
		{
			int hash = 5381;
			foreach (char c in s)
			{
				unchecked
				{
					hash = ((hash << 5) + hash) ^ c;
				}
			}

			return hash;
		}

		#endregion

		#region EnsureAllNotNullableFKsAccountedFor

		void EnsureAllNotNullableFKsAccountedFor(IApplicationSchemaResolver schemaResolver, Type bizObjType)
		{
			if (!bizObjType.IsSubclassOf(typeof(NonPersistentBusinessObject)))
			{
				string tableName1 = BusinessObjectFactory.GetTableNameFromType(bizObjType);
				DbConstraint[] fKs = DbConstraintReader.GetInstance(tableName1).Constraints.GetConstraintsOfType(DbConstraintType.ForeignKey);

				foreach (DbConstraint constraint in fKs)
				{
					string fKColumnName = constraint.ColumnNames[0];
					string tableName = BusinessObjectFactory.GetTableNameFromType(bizObjType);
					if (schemaResolver.GetSchemaColumnSafe(fKColumnName, tableName) != null)
					{
						if (FKPropertyNeedsAttribute(schemaResolver, bizObjType, constraint.ForeignTable, fKColumnName))
						{
							string message =
								"You should apply [" + nameof(DependentBusinessObjectAttribute).Replace("Attribute", "") +
								"(typeof(master_type), \"dependent_collection_property_name\")] to business object " + bizObjType.FullName + " or " +
								"[" + nameof(RelatedBusinessObjectAttribute).Replace("Attribute", "") +
								"(\"property_to_access_master\")] to property " + fKColumnName +
								". See <a href=\"http://sharepoint.corporate.cargowise.com/Wiki/Wiki%20Pages/NewWithValidTestData.aspx\"> here</a> for more details";
							ErrorReporter.ReportOnce(message, new ValidTestDataGenerationException(message));
						}
					}
				}
			}
		}

		bool FKPropertyNeedsAttribute(IApplicationSchemaResolver schemaResolver, Type bizObjType, string masterTable, string fkColumnName)
		{
			var tableName = BusinessObjectFactory.GetTableNameFromType(bizObjType);
			if (!schemaResolver.GetSchemaColumn(fkColumnName, tableName).IsNullable &&
				TypeDescriptor.GetProperties(bizObjType)[fkColumnName].Attributes[typeof(RelatedBusinessObjectAttribute)] == null)
			{
				foreach (DependentBusinessObjectAttribute attribute in bizObjType.GetCustomAttributes(typeof(DependentBusinessObjectAttribute), true))
				{
					if (attribute.MasterType != null && masterTable == BusinessObjectFactory.GetTableNameFromType(attribute.MasterType))
					{
						return false;
					}
				}

				return true;
			}

			return false;
		}

		#endregion

		#region FillWithValidTestDataForSavableBusinessObject

		void FillWithValidTestDataForSavableBusinessObject(BusinessObject bizObj, TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath, IApplicationSchemaResolver schemaResolver)
		{
			PopulateDates(bizObj, false, schemaResolver);
			PopulateUniqueStringProperties(bizObj, propertyPath);
			FillRelatedObjectsWithData(bizObj, kind & ~TestBusinessObjectKind.PopulateRelationsDeeply, propertyPath, false, schemaResolver);
		}

		PropertyDescriptor[] AppendToPropertyPath(PropertyDescriptor[] propertyPath, PropertyDescriptor newProperty)
		{
			if (newProperty == null)
			{
				throw new ArgumentNullException(nameof(newProperty));
			}

			PropertyDescriptor[] result = new PropertyDescriptor[propertyPath.Length + 1];
			propertyPath.CopyTo(result, 0);
			result[result.Length - 1] = newProperty;
			return result;
		}

		protected virtual void PopulateUniqueStringProperties(BusinessObject bizObj, PropertyDescriptor[] propertyPath)
		{
			var constraints = DbConstraintReader.GetInstance(bizObj.TableName).Constraints;
			foreach (DbConstraint constraint in constraints.GetConstraintsOfType(DbConstraintType.Unique))
			{
				PopulateConstraintEmptyColumns(constraint, bizObj, propertyPath);
			}

			foreach (DbConstraint constraint in constraints.GetConstraintsOfType(DbConstraintType.Check))
			{
				var withoutSpaces = constraint.Data.Replace(" ", "");

				if (withoutSpaces.IndexOf("<>''") != -1)
				{
					PopulateConstraintEmptyColumns(constraint, bizObj, propertyPath);
				}
				else if (withoutSpaces.IndexOf("(datalength(") == 0 && withoutSpaces.IndexOf(")>0") == -1)
				{
					PopulateConstraintEmptyColumns(constraint, bizObj, propertyPath);
				}
				else if (withoutSpaces.IndexOf("(len(") == 0 && withoutSpaces.IndexOf(")=(") != -1 && withoutSpaces.IndexOf(")=(0)") == -1)
				{
					PopulateConstraintFixedLengthColumns(constraint, bizObj, propertyPath);
				}
			}
		}

		void PopulateConstraintEmptyColumns(DbConstraint constraint, BusinessObject bizObj, PropertyDescriptor[] propertyPath)
		{
			foreach (var columnName in constraint.ColumnNames)
			{
				var property = columnName != bizObj.PKSchemaColumn.Name ? GetPropertyInfo(bizObj, columnName) : null;

				if (
					property != null
					&& property.Value.IsEmpty
					&& property.PropertyType == typeof(ZString))
				{
					PopulateUniqueString(property, propertyPath);
				}
				else if (property == null && !constraint.Data.IsNullOrEmpty()
					&& constraint.Data.IndexOf("OR", 0, StringComparison.OrdinalIgnoreCase) != -1)
				{
					string[] conditions = constraint.Data.Split(new[] { "OR" }, StringSplitOptions.RemoveEmptyEntries);
					foreach (var condition in conditions)
					{
						string withoutSpaces = condition.Replace(" ", "");
						if (withoutSpaces.IndexOf(bizObj.TablePrefix, 0, StringComparison.OrdinalIgnoreCase) == -1
							|| (withoutSpaces.IndexOf("DATALENGTH(", 0, StringComparison.OrdinalIgnoreCase) == -1 && !withoutSpaces.Contains("<>''"))
							|| (withoutSpaces.IndexOf("DATALENGTH(", 0, StringComparison.OrdinalIgnoreCase) != -1 && !withoutSpaces.Contains(">(0)"))
							|| withoutSpaces.IndexOf("AND", 0, StringComparison.OrdinalIgnoreCase) != -1)
						{
							return;
						}
					}

					int indexFrom = conditions[0].IndexOf("[" + bizObj.TablePrefix + "_") + 1;
					int indexTo = conditions[0].IndexOf("]");

					var columnNameOfFirstCondition = constraint.Data.Substring(indexFrom, indexTo - indexFrom);
					property = columnNameOfFirstCondition != bizObj.PKSchemaColumn.Name ? GetPropertyInfo(bizObj, columnNameOfFirstCondition) : null;

					if (property != null && property.Value.IsEmpty	&& property.PropertyType == typeof(ZString))
					{
						PopulateUniqueString(property, propertyPath);
					}
				}
			}
		}

		void PopulateConstraintFixedLengthColumns(DbConstraint constraint, BusinessObject bizObj, PropertyDescriptor[] propertyPath)
		{
			var startIndex = constraint.Data.IndexOf(")=(", StringComparison.Ordinal) + 3;
			var endIndex = constraint.Data.IndexOf(")", startIndex, StringComparison.Ordinal);
			var length = Int32.Parse(constraint.Data.Substring(startIndex, endIndex - startIndex));

			foreach (var columnName in constraint.ColumnNames)
			{
				var property = columnName != bizObj.PKSchemaColumn.Name ? GetPropertyInfo(bizObj, columnName) : null;

				if (
					property != null
					&& property.Value.IsEmpty
					&& property.PropertyType == typeof(ZString))
				{
					PopulateUniqueString(property, propertyPath, length);
				}
			}
		}

		public void PopulateUniqueString(ZPropertyInfo property, PropertyDescriptor[] propertyPath)
		{
			PopulateUniqueString(property, propertyPath, property.MaxLength);
		}

		protected virtual void PopulateUniqueString(ZPropertyInfo property, PropertyDescriptor[] propertyPath, int maxLength)
		{
			property.Value = new ZString(GetUniqueStringForProperty(property, propertyPath, maxLength));
		}

		protected virtual void PopulateFK(ZPropertyInfo fkProperty, TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			var relatedBizObjName = RelatedBusinessObjectAttribute.GetRelatedBizObjName(fkProperty.PropertyDescriptor);
			if (relatedBizObjName != null)
			{
				var relatedProperty = TypeDescriptor.GetProperties(fkProperty.BizObj)[relatedBizObjName]
					?? throw new ValidTestDataGenerationException("Could not find related business object property '" + relatedBizObjName + "'");

				if (BusinessObjectFactory.GetTableNameFromType(relatedProperty.PropertyType).StartsWith("Ref"))
				{
					fkProperty.Value = LoadRandomReferenceFile(fkProperty.BizObj.Factory, fkProperty.Name, relatedProperty.PropertyType);
				}
				if (!fkProperty.Value.IsValid)
				{
					var relatedBusinessObject = fkProperty.BizObj.Factory.New(relatedProperty.PropertyType);
					fkProperty.Value = relatedBusinessObject.PK;
					if ((kind & TestBusinessObjectKind.PopulateRelationsDeeply) != 0 &&
						!((IList)propertyPath).Contains(relatedProperty) &&
						propertyPath.Length <= 5)
					{
						relatedBusinessObject.FillWithValidTestData(kind, AppendToPropertyPath(propertyPath, relatedProperty));
					}
					else if ((kind & TestBusinessObjectKind.MinimumRequiredToSave) != 0)
					{
						relatedBusinessObject.FillWithValidTestData(TestBusinessObjectKind.MinimumRequiredToSave, AppendToPropertyPath(propertyPath, relatedProperty));
					}
				}
			}
		}

		ZGuid LoadRandomReferenceFile(BusinessObjectFactory factory, string propertyName, Type businessObjectType)
		{
			var result = ZGuid.Empty;
			var filter = new ZQuery
			{
				MaximumRows = 30
			};

			var candidateResults = factory.Load(businessObjectType, filter);
			if (candidateResults.Length > 0)
			{
				var index = new Random(CalculateStringHashCodeWithV1Algorithm(propertyName)).Next(candidateResults.Length - 1);
				result = candidateResults[index].PK;
			}
			return result;
		}

		protected virtual string GetUniqueStringForProperty(ZPropertyInfo property, PropertyDescriptor[] propertyPath, int maxLength)
		{
			for (var i = 1; i < 100000; i++)
			{
				if (GetUniqueStringForPropertyWithSeed(property, propertyPath, maxLength, i, out string result))
				{
					return result;
				}
			}

			throw new InvalidOperationException("Called GetUniqueStringForPropertyWithSeed too many times");
		}

		bool GetUniqueStringForPropertyWithSeed(ZPropertyInfo property, PropertyDescriptor[] propertyPath, int maxLength, int initialSeed, out string result)
		{
			if (maxLength == -1)
			{
				throw new ArgumentException("MaxLength must be specified", nameof(maxLength));
			}

			maxLength = Math.Min(4000, maxLength); //No point running out of memory for VARCHAR(Max) fields.

			var builder = new StringBuilder(maxLength);

			if (_generateRandomUniqueStringForProperty)
			{
				lock (RANDOM)
				{
					for (var i = 0; i < maxLength; i++)
					{
						builder.Append(CharactersToUse[RANDOM.Next(CharactersToUse.Length)]);
					}
				}
			}
			else
			{
				var seed = CalculateStringHashCodeWithV1Algorithm(property.Name) ^ initialSeed;
				for (var i = 0; i < propertyPath.Length; i++)
				{
					seed ^= CalculateStringHashCodeWithV1Algorithm(propertyPath[i].Name);
				}

				var random = new Random(seed);
				for (var i = 0; i < maxLength; i++)
				{
					builder.Append(CharactersToUse[random.Next(CharactersToUse.Length - 1)]);
				}
			}
			result = builder.ToString();

			foreach (DataRow row in ((INeedTable)property.BizObj).Table.Rows)
			{
				if (row.RowState != DataRowState.Deleted)
				{
					if (row[property.Name].ToString() == result)
					{
						return false;
					}
				}
			}

			return true;
		}

		const string CharactersToUse = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

		[ThreadSafe(ThreadSafeAttribute.Mechanism.Lock)]
		static readonly Random RANDOM = new Random();

		#endregion

		protected ZPropertyInfo GetPropertyInfo(BusinessObject bizObj, ZString name)
		{
			var info = bizObj.ZPropertyInfoHash.GetPropertySafe(name);
			var excluder = bizObj as IBusinessObjectTestDataHelperPropertiesToExclude;
			return info != null && info.HasSetter && (excluder == null || !excluder.ShouldExcludeFromFillWithValidTestData(name)) ? info : null;
		}

		protected IEnumerable<ZPropertyInfo> GetNonWrappingPropertyInfo(BusinessObject bizObj)
		{
			var excluder = bizObj as IBusinessObjectTestDataHelperPropertiesToExclude;
			foreach (ZPropertyInfo info in bizObj.ZPropertyInfoHash.GetPropertyInfos(PropertyInfoTypes.NonWrapping))
			{
				if (info != null && info.HasSetter && (excluder == null || !excluder.ShouldExcludeFromFillWithValidTestData(info.Name)))
				{
					yield return info;
				}
			}
		}
	}
}
#endif
