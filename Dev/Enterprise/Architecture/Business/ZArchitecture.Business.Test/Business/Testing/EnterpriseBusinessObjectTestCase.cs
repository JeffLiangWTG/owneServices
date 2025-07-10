using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestsSubclassesOf(
			typeof(EnterpriseBusinessObject),
			new Type[] { typeof(TestExcludeBusinessObjectsAllHaveTestCasesAttribute), typeof(TestedAsNonPersistentBusinessObjectAttribute) },
			new Type[] { typeof(IDocumentWrapper), typeof(NonPersistentBusinessObject) },
			ExcludeClientDlls = true)]
	public abstract class EnterpriseBusinessObjectTestCase : PersistentBusinessObjectTestCase
	{
		public void TestIAddInfoWithSyncPropertySupporterCorrectlySet()
		{
			AssertIfImplement<IAddInfoWithSyncPropertySupporter>(AssertIAddInfoWithSyncPropertySupporterCorrectlySet);
		}

		void AssertIAddInfoWithSyncPropertySupporterCorrectlySet(IAddInfoWithSyncPropertySupporter supporter)
		{
			var bizObj = (BusinessObject)supporter;
			var syncAddInfos = supporter.GetSyncAddInfos();
			if (syncAddInfos.Length > 0)
			{
				CombineAssertions(() =>
				{
					Factory.Save();
					var bizObjType = bizObj.GetType();
					foreach (var syncAddInfo in syncAddInfos)
					{
						foreach ((IZType testValue, IZType expectedValueResult) in GetValuesForAddInfoWithSyncPropertyTest(syncAddInfo.addInfoName, syncAddInfo.addInfoValueType, syncAddInfo.info))
						{
							var fastSearchName = syncAddInfo.fastSearchName;
							var newFactory = new BusinessObjectFactory { RefreshEnabled = false, NameForDebugging = $"{syncAddInfo.addInfoName} - {testValue}" };
							bizObj = newFactory.Load(bizObjType, bizObj.PK);
							var addInfoProperty = ((IAddInfoWithSyncPropertySupporter)bizObj).AddInfo.AddInfoProperty;
							bizObj[syncAddInfo.info.Name] = testValue;
							newFactory.Save();
							if (testValue.IsDefault)
							{
								AssertNotContains("AddInfo property should not have data", $"*{syncAddInfo.addInfoName}=", "*" + addInfoProperty.Value.ToString());
								if (!string.IsNullOrEmpty(fastSearchName))
								{
									AssertNull($"No GenAddOnColumn for '{fastSearchName}'", GetGenAddOnColumnBizObj(newFactory, bizObj.PK, fastSearchName));
								}
							}
							else
							{
								AssertContains("AddInfo property should have data", $"*{syncAddInfo.addInfoName}={expectedValueResult.GetStringRepresentation()}*", "*" + addInfoProperty.Value.ToString() + "*");
								if (!string.IsNullOrEmpty(fastSearchName))
								{
									var genAddOnColumnBizObj = GetGenAddOnColumnBizObj(newFactory, bizObj.PK, fastSearchName);
									AssertEquals(fastSearchName + " - XA_Type", GetGenAddOnColumnType(expectedValueResult), genAddOnColumnBizObj[GenAddOnColumnSchema.XA_Type]);
									AssertEquals(fastSearchName + " - XA_Data", GetGenAddOnColumnDataString(expectedValueResult), genAddOnColumnBizObj[GenAddOnColumnSchema.XA_Data]);
									bizObj[syncAddInfo.info.Name] = testValue.Default;
									newFactory.Save();
								}
							}
						}
					}
				});
			}
		}

		string GetGenAddOnColumnDataString(IZType value)
		{
			if (value is ZDate date)
			{
				return date.ToISO8601ShortDateString();
			}
			else if (value is ZDateTime dateTime)
			{
				return dateTime.SqlFormat;
			}
			else if (value is ZDateTimeOffset dateTimeOffset)
			{
				return dateTimeOffset.SqlFormat;
			}
			else
			{
				return value.ToString();
			}
		}

		string GetGenAddOnColumnType(IZType value)
		{
			if (value is ZBool)
			{
				return "BOO";
			}
			else if (value is ZByte)
			{
				return "BYT";
			}
			else if (value is ZDate)
			{
				return "DTE";
			}
			else if (value is ZDateTime)
			{
				return "DAT";
			}
			else if (value is ZDecimal)
			{
				return "DEC";
			}
			else if (value is ZGuid)
			{
				return "GUI";
			}
			else if (value is ZInt)
			{
				return "INT";
			}
			else if (value is ZShort)
			{
				return "SHO";
			}
			else if (value is ZString)
			{
				return "STR";
			}

			throw new ArgumentException("Unsupported Type : " + value.GetType().Name, nameof(value));
		}

		BusinessObject GetGenAddOnColumnBizObj(BusinessObjectFactory factory, ZGuid parentID, ZString name)
		{
			var query = new ZQuery(GenAddOnColumnSchema.XA_ParentID, parentID);
			query.AddToFilter(GenAddOnColumnSchema.XA_Name, name);
			return factory.LoadTop1(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(GenAddOnColumnSchema.Constants.Prefix), query);
		}

		protected virtual IEnumerable<(IZType testValue, IZType expectedValueResult)> GetValuesForAddInfoWithSyncPropertyTest(string addInfoName, Type addInfoValueType, ZPropertyInfo info)
		{
			var value = info.Value.Default;
			yield return (value, null);
			if (value is ZDate)
			{
				value = new ZDate(2021, 12, 25);
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
			}
			else if (value is ZDateTime)
			{
				value = new ZDateTime(2021, 12, 25, 15, 45, 35);
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
				value = ZDateTime.MaxSmallDateTime;
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
				value = ZDateTime.MinSmallDateTimeValue;
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
			}
			else if (value is ZDateTimeOffset)
			{
				value = new ZDateTimeOffset(2021, 12, 25, 15, 45, 35);
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
			}
			else if (value is ZString)
			{
				value = new ZString("A");
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
				value = new ZString("1");
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
			}
			else if (value is ZDecimal)
			{
				value = new ZDecimal(10.12m);
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
				value = GetMaxOrMinValue(info, false);
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
				value = GetMaxOrMinValue(info, true);
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
			}
			else if (value is ZByte)
			{
				value = new ZByte(2);
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
				value = new ZByte(byte.MaxValue);
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
				value = new ZByte(byte.MinValue);
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
			}
			else if (value is ZInt)
			{
				value = new ZInt(2);
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
				value = new ZInt(int.MaxValue);
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
				value = new ZInt(int.MinValue);
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
			}
			else if (value is ZLong)
			{
				value = new ZLong(2);
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
				value = new ZLong(long.MaxValue);
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
				value = new ZLong(long.MinValue);
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
			}
			else if (value is ZShort)
			{
				value = new ZShort(2);
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
				value = new ZShort(short.MaxValue);
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
				value = new ZShort(short.MinValue);
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
			}
			else if (value is ZGeography)
			{
				value = ZGeography.CreatePoint(1, 2);
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
			}
			else if (value is ZGuid)
			{
				value = new ZGuid("D7341193-24B5-40D4-B2D2-5C3C0DC3E3B5");
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
			}
			else if (value is ZBool)
			{
				value = ZBool.True;
				yield return (value, GetExpectedResultValueForAddInfoWithSyncPropertyTest(value, addInfoValueType));
			}
			else
			{
				throw new NotSupportedException($"Value of type {value.GetType().FullName} is not supported in AddInfo conversion");
			}
		}

		ZDecimal GetMaxOrMinValue(ZPropertyInfo info, bool isMin)
		{
			var schemaColumn = (SchemaDecimalColumn)info.BizObj.PKSchemaColumn.TableSchema.GetSchemaColumn(info.Name);
			var precision = schemaColumn.Precision;
			var scale = schemaColumn.Scale;
			return ZDecimal.Parse("".PadRight(precision - scale, '9') + "." + "".PadRight(scale, '9')) * (isMin ? -1 : 1);
		}

		IZType GetExpectedResultValueForAddInfoWithSyncPropertyTest(IZType value, Type addInfoValueType)
		{
			IZType resultValue = null;
			try
			{
				resultValue = (IZType)Activator.CreateInstance(addInfoValueType, value);
			}
			catch (Exception)
			{
				resultValue = GetExpectedResultValueForAddInfoWithSyncPropertyTestCore(value, addInfoValueType);
			}
			return resultValue;
		}

		protected virtual IZType GetExpectedResultValueForAddInfoWithSyncPropertyTestCore(IZType value, Type addInfoValueType)
		{
			throw new NotSupportedException($@"Conversion from {value.GetType().FullName}({value.ToString()}) to {addInfoValueType.FullName} is not currectly supporting in the test.
If you want to enable support for it then override {nameof(GetExpectedResultValueForAddInfoWithSyncPropertyTestCore)} and Enterprise.Customs.Business.BaseAddInfo");
		}

		public void TestUniqueIndexFailureHandlerForAddInfoChild()
		{
			AssertIfImplement<IAddInfoChildUniqueIndexFailureHandlerSupporter>(AssertUniqueIndexFailureHandlerForAddInfoChild);
		}

		static void AssertUniqueIndexFailureHandlerForAddInfoChild(IAddInfoChildUniqueIndexFailureHandlerSupporter bizObj)
		{
			var handlerExpectedType = typeof(AddInfoChildUniqueIndexFailureHandler);
			var handler = GetUniqueIndexFailureHandler((BusinessObject)bizObj, handlerExpectedType);
			AssertNotNull("Should have a Handler sub class from " + handlerExpectedType, handler);
		}

		public void TestUniqueClusterkeyIndexFailureHandlerForAddInfoChild()
		{
			AssertIfImplement<IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter>(AssertUniqueClusterkeyIndexFailureHandlerForAddInfoChild);
		}

		static void AssertUniqueClusterkeyIndexFailureHandlerForAddInfoChild(IAddInfoChildUniqueClusterKeyIndexFailureHandlerSupporter bizObj)
		{
			var handlerExpectedType = typeof(AddInfoChildUniqueClusterKeyIndexFailureHandler);
			var handler = GetUniqueIndexFailureHandler((BusinessObject)bizObj, handlerExpectedType);
			AssertNotNull("Should have a Handler sub class from " + handlerExpectedType, handler);
		}

		static IUniqueIndexFailureHandler GetUniqueIndexFailureHandler(BusinessObject bizObj, Type handlerExpectedType)
		{
			return ((IEnumerable<IUniqueIndexFailureHandler>)bizObj.GetType().GetProperty("UniqueIndexFailureHandlers", BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(bizObj, null)).FirstOrDefault(x => handlerExpectedType.IsAssignableFrom(x.GetType()));
		}

		public void TestIAddInfoChildSupporterCorrectlySet()
		{
			AssertIfImplement<IAddInfoChildSupporter>(AssertIAddInfoChildSupporterCorrectlySet);
		}

		static void AssertIAddInfoChildSupporterCorrectlySet(IAddInfoChildSupporter supporter)
		{
			CombineAssertions(() =>
			{
				var addInfoChild = supporter.AddInfoChild;
				if (addInfoChild == null || supporter.ChildForeignKeyColumn == null)
				{
					var bizObj = (BusinessObject)supporter;
					var tableName = bizObj.TableName;
					var splitTableName = Regex.Split(tableName, @"(?<!^)(?=[A-Z])");
					if (splitTableName.Length > 1)
					{
						var countryCode = StaticCurrentFetcher.Instance.CurrentCompany.GC_RN_NKCountryCode;
						var childAddInfoTabbleName = splitTableName[0] + countryCode + string.Join("", splitTableName.Skip(1));
						var childAddInfoTableSchema = EnterpriseSchema.GetTableSchema(childAddInfoTabbleName);
						if (childAddInfoTableSchema != null)
						{
							var bizObjType = supporter.GetType();
							(Type childAddInfoType, bool matchUsingPrefix) = GetChildAddInfoType(childAddInfoTableSchema, supporter.GetType());
							if (childAddInfoType != null)
							{
								var foreignKeySuffix = "_" + bizObj.TablePrefix;
								var childForeignKeyColumn = childAddInfoTableSchema.All.OfType<SchemaGuidColumn>().FirstOrDefault(x => x.Name.Contains(foreignKeySuffix))?.Name;
								if (childForeignKeyColumn == null)
								{
									childForeignKeyColumn = $"foreign key to {tableName}";
								}
								else
								{
									childForeignKeyColumn = $"{childAddInfoTableSchema.TableName}.{childForeignKeyColumn}";
								}
								var message = new ZStringBuilder($"{bizObjType.FullName} has a child leaf table '{childAddInfoType.FullName}'.");
								message.Append($"Please ensure that {typeof(IAddInfoChildSupporter).FullName}.{nameof(IAddInfoChildSupporter.AddInfoChild)} return {childAddInfoType.Name}.");
								message.Append($"Please ensure that {typeof(IAddInfoChildSupporter).FullName}.{nameof(IAddInfoChildSupporter.ChildForeignKeyColumn)} return {childForeignKeyColumn}.");
								if (!matchUsingPrefix)
								{
									message.Append($"Please ensure that there is a mapping for '{childAddInfoTableSchema.PK.ColumnPrefix}' in EnterpriseBusinessObjectPrefixTypes.");
								}
								TestCase.Fail(message.ToStringWithNewLineBetweenAppends());
							}
						}
					}
				}
				if (addInfoChild != null)
				{
					var handlerSupporterType = typeof(IAddInfoChildUniqueIndexFailureHandlerSupporter);
					var addInfoChildType = addInfoChild.GetType();
					if (!handlerSupporterType.IsAssignableFrom(addInfoChildType))
					{
						TestCase.Fail($"{addInfoChildType.FullName} needs to implement {handlerSupporterType.FullName}.");
					}
				}
			});
		}

		static (Type childAddInfoType, bool matchUsingPrefix) GetChildAddInfoType(ITableSchema tableSchema, Type bizObjType)
		{
			var matchUsingPrefix = true;
			var childAddInfoType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(tableSchema.PK.ColumnPrefix, false);
			if (childAddInfoType == null)
			{
				matchUsingPrefix = false;
				var tableName = tableSchema.TableName;
				var autoTableName = "Auto" + tableName;
				foreach (var type in bizObjType.Assembly.GetTypes())
				{
					if (type.Name == tableName)
					{
						childAddInfoType = type;
						break;
					}
					else if (childAddInfoType == null && type.BaseType?.Name == autoTableName)
					{
						childAddInfoType = type;
					}
				}
			}

			return (childAddInfoType, matchUsingPrefix);
		}

		public virtual void TestCorrectlyDeleteChildrenIfSupporterInterfacesIsUsed()
		{
			var businessObjectType = GetExpectedBusinessObjectType();
			var checkICusSupportingInfoTypeSupporter = typeof(Customs.ICusSupportingInfoTypeSupporter).IsAssignableFrom(businessObjectType);
			var checkICusAddInfoTypeSupporter = typeof(Customs.ICusAddInfoTypeSupporter).IsAssignableFrom(businessObjectType);
			var checkICusCodeDataTypeSupporter = typeof(Customs.ICusCodeDataTypeSupporter).IsAssignableFrom(businessObjectType);
			if (checkICusSupportingInfoTypeSupporter || checkICusAddInfoTypeSupporter || checkICusCodeDataTypeSupporter)
			{
				var bizObj = GetNewBusinessObjectForDeleteTest(Factory);
				var pk = bizObj.PK;
				var prefix = bizObj.TablePrefix;
				var children = new List<BusinessObject>();
				if (bizObj is Customs.ICusSupportingInfoTypeSupporter supportingInfoTypeSupporter)
				{
					children.AddRange(CreateChildren(CusSupportingInfoSchema.CSI_ParentTableCode, CusSupportingInfoSchema.CSI_ParentID, supportingInfoTypeSupporter.GetCusSupportingInfoTypes()?.Values));
				}
				if (bizObj is Customs.ICusAddInfoTypeSupporter addInfoTypeSupporter)
				{
					children.AddRange(CreateChildren(CusAddInfoSchema.B7_ParentTableCode, CusAddInfoSchema.B7_ParentID, addInfoTypeSupporter.GetCusAddInfoTypes()?.Values));
				}
				if (bizObj is Customs.ICusCodeDataTypeSupporter codeDataTypeSupporter)
				{
					children.AddRange(CreateChildren(CusCodeDataSchema.CY_ParentTableCode, CusCodeDataSchema.CY_ParentID, codeDataTypeSupporter.GetCusCodeDataTypes()?.Values));
				}

				if (children.Count > 0)
				{
					bizObj.Delete();
					var listOfChildrenNotDeleted = string.Join("\r\n", children.Where(x => !x.IsDeleted).Select(x => x.GetType().FullName));
					if (listOfChildrenNotDeleted.Length > 0)
					{
						Fail($@"Calling {bizObj.GetType().FullName}.Delete() did not delete the following BusinessObjects:
{listOfChildrenNotDeleted}

Please ensure that the Delete() will call deletion of those BusinessObjects.
Or make sure that the following code is called:

			if (!IsDeleted)
			{{
				FetchForLoadChildEditableObjectsIfNeeded();
				this.DeleteAllCusAddInfoCodeDataAndSupportingInfoChildrenIfSupported();
			}}
");
					}
				}

				IEnumerable<BusinessObject> CreateChildren(SchemaStringColumn parentTableCodeColumn, SchemaGuidColumn pareintIDColumn, IEnumerable<Type> childTypes)
				{
					if (childTypes != null)
					{
						foreach (var childType in childTypes.Where(x => !x.IsAbstract))
						{
							var child = Factory.New(childType);
							child[parentTableCodeColumn] = prefix;
							child[pareintIDColumn] = pk;
							yield return child;
						}
					}
				}
			}

			Assert("All is good", true);
		}

		protected void AssertIfImplement<T>(Action<T> assertData)
			where T : class
		{
			if (typeof(T).IsAssignableFrom(GetExpectedBusinessObjectType())
				&& GetNewBusinessObjectForDeleteTest(Factory) is T bizObj)
			{
				assertData(bizObj);
			}
			Assert("All is good", true);
		}

		[ExpectNoExceptions]
		public void TestIUniversalXMLNoteParentShouldOnlyGetNoteTypesfromMetadata()
		{
			var universalXMLNoteParent = CachedBusinessObject as IUniversalXMLNoteParent;

			if (universalXMLNoteParent != null)
			{
				if (CachedBusinessObject.GetNoteTypesWithoutUsingMetadata().Count > 0)
				{
					var errorMessage = string.Format(@"Type '{0}' has implemented interface 'IUniversalXMLNoteParent'. It should only get note types from metadata. 
Check if its implementation or its ancestors' implementation override NoteTypesCore. If yes, they should define metadata classes and move note types definition into their metadata classes.", CachedBusinessObject.GetType().Name);
					Fail(errorMessage);
				}
			}
		}

		#region Note Types Not Null

		public virtual void TestNotesTypesIsNotNull()
		{
			AssertNotNull(CachedBusinessObject.NoteTypes);
		}

		protected new EnterpriseBusinessObject CachedBusinessObject
		{
			get { return (EnterpriseBusinessObject)base.CachedBusinessObject; }
		}

		#endregion

		#region Properties - Action Field Attribute Usage

		public void TestActionFieldAttributeUsage()
		{
			Type nearestConcreteAncestor = NearestConcreteAncestor(ExpectedBusinessObjectType, typeof(BusinessObject));
			IDictionary<string, IList<string>> brokenProperties = new SortedDictionary<string, IList<string>>();

			List<string> errors = new List<string>();
			foreach (PropertyInfo propertyInfo in ExpectedBusinessObjectType.GetProperties())
			{
				if (propertyInfo.GetIndexParameters().Length > 0)
				{
					continue;
				}

				if (!propertyInfo.DeclaringType.IsSubclassOf(nearestConcreteAncestor))
				{
					continue;
				}

				CheckActionFieldUsage(propertyInfo, errors);
				CheckActionFieldFollowUsage(propertyInfo, errors);

				if (errors.Count > 0)
				{
					string caption = propertyInfo.DeclaringType.FullName + "." + propertyInfo.Name;
					brokenProperties.Add(caption, errors);
					errors = new List<string>();
				}
			}

			AssertGroupedErrorList("These properties have problems", brokenProperties);
		}

		void CheckActionFieldUsage(PropertyInfo info, IList<string> errors)
		{
			ActionFieldAttribute actionField = GetAttribute<ActionFieldAttribute>(info, errors);
			if (actionField != null)
			{
				if (!typeof(IZType).IsAssignableFrom(info.PropertyType))
				{
					errors.Add(string.Format("[ActionField(...)] can only be applied to IZType properties,\nbut this property returns a {0}", info.PropertyType.FullName));
				}
				else if (!Enum.IsDefined(typeof(ActionFieldType), actionField.FieldType))
				{
					errors.Add(string.Format("[ActionField(FieldType = (ActionFieldType){0}, ...)] has an invalid field type", (int)actionField.FieldType));
				}
				else if (!IsFieldTypeValidForPropertyType(actionField.FieldType, info.PropertyType))
				{
					errors.Add(string.Format("[ActionField(FieldType = ActionFieldType.{0}, ...)] is not valid for {1} properties", actionField.FieldType, info.PropertyType.FullName));
				}

				if (!info.CanWrite)
				{
					errors.Add("[ActionField(...)] can only be applied to properties with setters");
				}
			}
		}
		void CheckActionFieldFollowUsage(PropertyInfo info, IList<string> errors)
		{
			ActionFieldFollowAttribute actionFieldFollow = GetAttribute<ActionFieldFollowAttribute>(info, errors);

			if (actionFieldFollow != null)
			{
				Type returnType = ActionFieldFollowAttribute.GetReturnType(info);

				if (!typeof(BusinessObject).IsAssignableFrom(returnType) && !typeof(IBusinessObjectCollection).IsAssignableFrom(returnType))
				{
					errors.Add(string.Format("[ActionFieldFollow(...)] can only be applied to BusinessObject or IBusinessObjectCollection properties,\r\nbut this property returns a {0}", info.PropertyType.FullName));
				}

				if (!info.PropertyType.IsAssignableFrom(returnType))
				{
					errors.Add(string.Format("[ActionFieldFollow(...)] defines a return type that is incompatable with the property type.\r\nReturn Type: {0}\r\nProperty Type: {1}", returnType.FullName, info.PropertyType.FullName));
				}

				if (!info.CanRead)
				{
					errors.Add("[ActionFieldFollow(...)] can only be applied to properties with getters");
				}
			}
		}

		bool IsFieldTypeValidForPropertyType(ActionFieldType fieldType, Type propertyType)
		{
			switch (fieldType)
			{
				case ActionFieldType.Auto:
				case ActionFieldType.Hidden:
					return propertyType == typeof(ZString)
						|| propertyType == typeof(ZGuid)
						|| propertyType == typeof(ZDateTime)
						|| propertyType == typeof(ZDateTimeOffset)
						|| propertyType == typeof(ZDate)
						|| propertyType == typeof(ZBool)
						|| propertyType == typeof(ZDecimal)
						|| propertyType == typeof(ZInt)
						|| propertyType == typeof(ZShort)
						|| propertyType == typeof(ZByte);

				case ActionFieldType.Text:
				case ActionFieldType.Code:
				case ActionFieldType.NKModule:
					return propertyType == typeof(ZString);

				case ActionFieldType.PKModule:
				case ActionFieldType.Address:
					return propertyType == typeof(ZGuid);

				case ActionFieldType.DateTime:
					return propertyType == typeof(ZDateTime);

				case ActionFieldType.Date:
					return propertyType == typeof(ZDate);

				case ActionFieldType.Boolean:
					return propertyType == typeof(ZBool);

				case ActionFieldType.Numeric:
					return propertyType == typeof(ZDecimal)
						|| propertyType == typeof(ZInt)
						|| propertyType == typeof(ZShort)
						|| propertyType == typeof(ZByte);

				default:
					return false;
			}
		}

		T GetAttribute<T>(PropertyInfo info, IList<string> errors)
			where T : Attribute
		{
			try
			{
				return (T)Attribute.GetCustomAttribute(info, typeof(T));
			}
			catch (Exception ex)
			{
				errors.Add(string.Format("exception caught trying to get a '{0}' attribute\n\n{1}", typeof(T).FullName, ex.ToString()));
				return null;
			}
		}

		#endregion

		#region EventDatePropertyAttribute

		public void TestEventDatePropertyAttribute_AppliedCorrectlyToProperties()
		{
			foreach (PropertyDescriptor property in ZCustomTypeDescriptor.GetProperties(BusinessObject.GetType()))
			{
				EventDatePropertyAttribute attr = (EventDatePropertyAttribute)property.Attributes[typeof(EventDatePropertyAttribute)];
				if (attr != null)
				{
					ZPropertyInfo propertyInfo = BusinessObject.ZPropertyInfoHash[property.Name];
					Event eventType = Events.All[attr.EventType];

					AssertNotNull("An invalid event type was specified ('" + attr.EventType + "')", eventType);
					AssertEventDatePropertyAttributeAppliedCorrectly(GetLogParentForEventDateProperty(), propertyInfo, eventType, attr.EstimateActual, attr.ShouldOnlyUpdateEmptyDate);
				}
			}
			Assert(true);
		}

		protected virtual void AssertEventDatePropertyAttributeAppliedCorrectly(BusinessObject logParent, ZPropertyInfo property, Event eventType, EstimateActual estimateActual, bool updateBizObjOnlyWhenEmpty)
		{
			if (property.PropertyDescriptor != null &&
				property.PropertyDescriptor.Attributes[typeof(BusinessObjectTestExclude)] == null &&
				estimateActual != EstimateActual.MilestoneEstimateOnly)
			{
				AssertEventLogCreatedFromDateProperty(logParent, property, eventType, estimateActual);
			}
		}

		void AssertEventLogCreatedFromDateProperty(BusinessObject logParent, ZPropertyInfo property, Event eventType, EstimateActual estimateActual)
		{
			var isEstimate = estimateActual == EstimateActual.Estimate;
			property.Value = new ZDateTime(2000, 1, 1);

			Factory.Save();

			Func<StmALog, bool> additionalFilter = l => l.SL_IsEstimate == isEstimate;
			StmALog log = logParent.GetLogs().MostRecentLogByEventTime(eventType, additionalFilter);
			AssertNotNull(string.Format("Setting date value on {0}.{1} should have created log event type '{2}'", GetExpectedBusinessObjectType().FullName, property.Name, eventType.Code), log);
			AssertEquals("SL_SE_NKEvent set on new log from date property attribute", eventType.Code, log.SL_SE_NKEvent);
			AssertEquals("SL_EventTime set on new log from date property value", new ZDateTime(2000, 1, 1), log.SL_EventTime);
			AssertEquals("SL_IsEstimate set on new log from date property value", estimateActual == EstimateActual.Estimate, log.SL_IsEstimate);
		}

		protected virtual BusinessObject GetLogParentForEventDateProperty()
		{
			return BusinessObject;
		}

		#endregion

		#region Workflow supportable business objects

		public void TestWorkflowSupportableBusinessObject()
		{
			var bizo = BusinessObject as IWorkflowProviderCore;

			if (bizo != null && !WorkflowSupportableTableNames.Instance.GetTableNames().Contains(BusinessObject.TableName))
			{
				Fail(string.Format(CultureInfo.InvariantCulture, "Table name [{0}] not found in Enterprise.ZArchitecture.Business.EventManagement.WorkflowSupportableTableNames list. Please add the table name [{0}] into the list.", BusinessObject.TableName));
			}
			else
			{
				Assert(true);
			}
		}

		#endregion

		#region IDocManagerSupport

		public virtual void TestCanExportEDocViaUniversalXml()
		{
			var tester = ObjectFactory.Get<IDocManagerDataContextTester>();
			tester.ExportEdocsViaUniversalXml(BusinessObject);
			Assert(true);
		}

		#endregion

		#region Metadata

		[ExpectNoExceptions]
		public void TestMetadata()
		{
			var metadata = CachedBusinessObject.Metadata;
			AssertNotNull(metadata);
			AssertType(ExpectedMetadataType, metadata);
		}

		protected virtual Type ExpectedMetadataType
		{
			get
			{
				return ObjectFactory.Get<IEnterpriseBusinessObjectMetadata>().GetType();
			}
		}

		#endregion
	}
}
