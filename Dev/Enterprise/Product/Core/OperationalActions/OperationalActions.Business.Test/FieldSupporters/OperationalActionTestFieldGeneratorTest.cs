using System;
using System.Data;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	internal sealed class OperationalActionTestFieldGeneratorTest : TestCaseWithFactory
	{
		public void TestStringFields()
		{
			AssertTextField(DummyBizoSchema.Constants.Z0_VarCharMax, false, 100);
			AssertTextField(DummyBizoSchema.Constants.Z0_Description, false, DummyBizoSchema.Z0_Description.MaxLength);
			AssertTextField(DummyForGeneration.Schema.Z0_OA_TextFieldThatLooksLikeAModuleField, false, 35);
			AssertTextField(DummyForGeneration.Schema.Z0_URL, false, 250);
			AssertTextField(DummyForGeneration.Schema.NotRN, false, 2);
			AssertNKModuleField(DummyForGeneration.Schema.Z0_RL_ParentShown, false, 5, ModuleIDs.RefUNLOCO, typeof(RefUNLOCOCollection));
			AssertNoField(DummyForGeneration.Schema.Z0_RL_ParentHidden);
			AssertCodeField(DummyBizoSchema.Constants.Z0_Code, false, OLookUpEditType.TransportType);
			AssertCodeField(DummyForGeneration.Schema.Z0_OtherCode, false, typeof(CodedList));
			AssertCodeField(DummyForGeneration.Schema.Z0_F3_NKPackType, false, RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(Factory));
			AssertNKModuleField(DummyForGeneration.Schema.Z0_NKOrigin, true, RefUNLOCOSchema.RL_Code.MaxLength, ModuleIDs.RefUNLOCO, typeof(RefUNLOCOCollection));
			AssertNKModuleField(DummyForGeneration.Schema.Z0_RL_NKLoad, false, RefUNLOCOSchema.RL_Code.MaxLength, ModuleIDs.RefUNLOCO, typeof(RefUNLOCOCollection));
		}

		public void TestDateFields()
		{
			AssertDateField(DummyBizoSchema.Constants.Z0_Date, false, ZDateTimePickerFormat.Long);
			AssertDateField(DummyBizoSchema.Constants.Z0_SmallDateTime, false, ZDateTimePickerFormat.Long, true);
			AssertDateField(DummyBizoSchema.Constants.Z0_AnotherDate, false, ZDateTimePickerFormat.Short);
			AssertDateTimeOffsetField(DummyBizoSchema.Constants.Z0_DateTimeOffset, false);
			AssertDateOnlyField(DummyBizoSchema.Constants.Z0_DateOnly, false);
		}

		public void TestGuidFields()
		{
			AssertPKModuleField(DummyForGeneration.Schema.Z0_OH_Consignor, false, ModuleIDs.Organisation, typeof(OrganisationsFindBoxCollection));
			AssertPKModuleField(DummyForGeneration.Schema.ContactPK, false, ModuleIDs.OrgContacts, typeof(OrgContactCollection));
			AssertAddressField(DummyForGeneration.Schema.Z0_OA_ObviousAddressField, false, AddressType.OFC, typeof(OrganisationsFindBoxCollection));
			AssertAddressField(DummyForGeneration.Schema.Z0_ExplicitAddress, false, AddressType.DLV, typeof(OrganisationsFindBoxCollection));
		}

		public void TestBooleanFields()
		{
			AssertBooleanField(DummyForGeneration.Schema.Z0_Bool, false);
			AssertBooleanField(DummyForGeneration.Schema.IsFlagged, true);
		}

		public void TestNumericFields()
		{
			AssertNumericField(DummyForGeneration.Schema.Z0_Byte, false, byte.MinValue, byte.MaxValue);
			AssertNumericField(DummyForGeneration.Schema.Z0_Short, false, short.MinValue, short.MaxValue);
			AssertNumericField(DummyForGeneration.Schema.Z0_Number, false, int.MinValue, int.MaxValue);
			AssertNumericField(DummyForGeneration.Schema.Z0_Decimal, false, decimal.MinValue, decimal.MaxValue, 18, 0);
			AssertNumericField(DummyForGeneration.Schema.Z0_Money, false, decimal.MinValue, decimal.MaxValue, 19, 4);
			AssertNumericField(DummyForGeneration.Schema.Z0_AnotherDecimal, false, decimal.MinValue, decimal.MaxValue, 18, 3);
			AssertNumericField(DummyForGeneration.Schema.Z0_CustomDecimal, false, -100, 100, 3, 0);
		}

		public void TestGeographyFields()
		{
			AssertGeographyField(DummyForGeneration.Schema.Z0_Geography, false);
		}

		public void TestCustomFields()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			template.P0_Name = "Template";
			var customField1 = template.GenCustomColumnDefinitions.AddNew();
			customField1.XC_Name = "customFieldBoolean";
			customField1.XC_Type = AddOnColumnDataType.Codes.Boolean;
			var customField2 = template.GenCustomColumnDefinitions.AddNew();
			customField2.XC_Name = "customFieldDate";
			customField2.XC_Type = AddOnColumnDataType.Codes.Datetime;
			var customField3 = template.GenCustomColumnDefinitions.AddNew();
			customField3.XC_Name = "customFieldDecimal";
			customField3.XC_Type = AddOnColumnDataType.Codes.Decimal;
			var customField4 = template.GenCustomColumnDefinitions.AddNew();
			customField4.XC_Name = "customFieldInteger";
			customField4.XC_Type = AddOnColumnDataType.Codes.Integer;
			var customField5 = template.GenCustomColumnDefinitions.AddNew();
			customField5.XC_Name = "customFieldString";
			customField5.XC_Type = AddOnColumnDataType.Codes.String;
			var customField6 = template.GenCustomColumnDefinitions.AddNew();
			customField6.XC_Name = "customFieldDateTime";
			customField6.XC_Type = AddOnColumnDataType.Codes.Datetime;
			var addOnRuleForDateTime = Factory.NewWithValidTestData<GenCustomAddOnRule>();
			addOnRuleForDateTime.SetRules(new DateTimeFormatRule()
			{ Format = KDateTimeFormat.Long, IsEnabled = true });
			addOnRuleForDateTime.XR_Code = "DateTimeRule";
			addOnRuleForDateTime.XR_Description = "DateTime Rule";
			addOnRuleForDateTime.XR_IsSystemDefined = false;
			customField6.XC_XR = addOnRuleForDateTime.PK;
			var customField7 = template.GenCustomColumnDefinitions.AddNew();
			customField7.XC_Name = "customFieldCodeList";
			customField7.XC_Type = AddOnColumnDataType.Codes.String;
			var addOnRuleForCodeList = Factory.NewWithValidTestData<GenCustomAddOnRule>();
			var rule = new InvalidCodeRule()
			{ IsEnabled = true };
			var list = new CodeDescriptionPairList();
			list.AddPair("A1", "AAA 111");
			list.AddPair("B1", "BBB 111");
			list.AddPair("C1", "CCC 111");
			rule.List = list;
			addOnRuleForCodeList.SetRules(rule);
			addOnRuleForCodeList.XR_Code = "CodeListRule";
			addOnRuleForCodeList.XR_Description = "CodeList Rule";
			addOnRuleForCodeList.XR_IsSystemDefined = false;
			customField7.XC_XR = addOnRuleForCodeList.PK;
			Factory.Save();
			AssertBooleanCustomField("cUstomFieldBoolean", false);
			AssertDateCustomField("cusTomFieldDate", false);
			AssertZDecimalCustomField("CUstomFieldDecimal", false, decimal.MinValue, decimal.MaxValue, 9, 2);
			AssertIntCustomField("customFIeldInteger", false, int.MinValue, int.MaxValue, 10, 0);
			AssertTextCustomField("customFIeldString", false, GenCustomAddOnValue.Schema.XV_DataMaxLength);
			AssertDateTimeCustomField("cusTomFieldDateTime", false, ZDateTimePickerFormat.Long);
			AssertCodeCustomField("customFIeldCodeList", false, list.MaxCodeLength, list.Count, list.ElementsAsString);
		}

		public void TestGlbStaffActivityTrackingStatusFieldIsHidden()
		{
			PropertyInfo path = ReflectionHelper.GetPropertyInfo(typeof(GlbStaff), GlbStaff.Schema.GS_ActivityTrackingStatus);
			AssertNotNull(path);
			OperationalActionFieldSupporter activityTrackingStatusField = Generator.CreateField(new PropertyInfo[] { path });
			AssertNull(activityTrackingStatusField);

			PropertyInfo[] paths = ReflectionHelper.FieldTextToPath(typeof(GlbStaff), GlbStaff.Schema.GS_ActivityTrackingStatus);
			AssertNull(paths);
		}

		public void TestGlbStaffIsSystemAccountAndIsDeveloperFieldIsHidden()
		{
			var path = ReflectionHelper.GetPropertyInfo(typeof(GlbStaff), GlbStaff.Schema.GS_IsSystemAccount);
			AssertNotNull(path);
			var isSystemAccountField = Generator.CreateField(new PropertyInfo[] { path });
			AssertNull(isSystemAccountField);

			var paths = ReflectionHelper.FieldTextToPath(typeof(GlbStaff), GlbStaff.Schema.GS_IsSystemAccount);
			AssertNull(paths);

			var path1 = ReflectionHelper.GetPropertyInfo(typeof(GlbStaff), GlbStaff.Schema.GS_IsDeveloper);
			AssertNotNull(path1);
			var isDeveloperField = Generator.CreateField(new PropertyInfo[] { path1 });
			AssertNull(isDeveloperField);

			var paths1 = ReflectionHelper.FieldTextToPath(typeof(GlbStaff), GlbStaff.Schema.GS_IsDeveloper);
			AssertNull(paths1);
		}

		#region Implementation
		OperationalActionFieldGenerator Generator
		{
			get
			{
				return generator ?? (generator = new OperationalActionFieldGenerator());
			}
		}

		OperationalActionFieldGenerator generator;
		void AssertTextField(string key, bool readOnly, int maxLength)
		{
			OperationalActionTextFieldSupporter fieldSupporter = AssertField<OperationalActionTextFieldSupporter>(key);
			CombineAssertions(delegate
			{
				AssertEquals(string.Format("'{0}' should have the correct readonlyness.", key), readOnly, fieldSupporter.ReadOnly);
				AssertEquals(string.Format("'{0}' should have the correct max length", key), maxLength, fieldSupporter.MaxLength);
			});
		}

		void AssertCodeField(string key, bool readOnly, OLookUpEditType lookup)
		{
			OperationalActionCodeFieldSupporter fieldSupporter = AssertField<OperationalActionCodeFieldSupporter>(key);
			CombineAssertions(delegate
			{
				AssertEquals(string.Format("'{0}' should have the correct readonlyness.", key), readOnly, fieldSupporter.ReadOnly);
				AssertEquals(string.Format("'{0}' should have the correct list", key), new CodeDescriptionPairList(lookup).CodesAsString, new CodeDescriptionPairList(fieldSupporter.List).CodesAsString);
			});
		}

		void AssertCodeField(string key, bool readOnly, Type listType)
		{
			OperationalActionCodeFieldSupporter fieldSupporter = AssertField<OperationalActionCodeFieldSupporter>(key);
			CombineAssertions(delegate
			{
				AssertEquals(string.Format("'{0}' should have the correct readonlyness.", key), readOnly, fieldSupporter.ReadOnly);
				ReadOnlyCodeDescriptionPairList expectedList = (ReadOnlyCodeDescriptionPairList)Activator.CreateInstance(listType);
				AssertEquals(string.Format("'{0}' should have the correct list", key), expectedList.CodesAsString, new CodeDescriptionPairList(fieldSupporter.List).CodesAsString);
			});
		}

		void AssertCodeField(string key, bool readOnly, ReadOnlyCodeDescriptionPairList expectedList)
		{
			OperationalActionCodeFieldSupporter fieldSupporter = AssertField<OperationalActionCodeFieldSupporter>(key);
			CombineAssertions(delegate
			{
				AssertEquals(string.Format("'{0}' should have the correct readonlyness.", key), readOnly, fieldSupporter.ReadOnly);
				AssertEquals(string.Format("'{0}' should have the correct list", key), expectedList.CodesAsString, new CodeDescriptionPairList(fieldSupporter.List).CodesAsString);
			});
		}

		void AssertDateField(string key, bool readOnly, ZDateTimePickerFormat format, bool isDuration = false)
		{
			OperationalActionDateTimeFieldSupporter fieldSupporter = AssertField<OperationalActionDateTimeFieldSupporter>(key);
			CombineAssertions(delegate
			{
				AssertEquals(string.Format("'{0}' should have the correct readonlyness.", key), readOnly, fieldSupporter.ReadOnly);
				AssertEquals(string.Format("'{0}' should have the correct format", key), format, fieldSupporter.Format);
				AssertEquals(string.Format("'{0}' Should have the correct duration-ness", key), isDuration, fieldSupporter.IsDuration);
			});
		}

		void AssertDateOnlyField(string key, bool readOnly)
		{
			var fieldSupporter = AssertField<OperationalActionDateFieldSupporter>(key);
			CombineAssertions(delegate
			{
				AssertEquals(string.Format("'{0}' should have the correct readonlyness.", key), readOnly, fieldSupporter.ReadOnly);
			});
		}

		void AssertDateTimeOffsetField(string key, bool readOnly)
		{
			OperationalActionDateTimeOffsetFieldSupporter fieldSupporter = AssertField<OperationalActionDateTimeOffsetFieldSupporter>(key);
			CombineAssertions(delegate
			{
				AssertEquals(string.Format("'{0}' should have the correct readonlyness.", key), readOnly, fieldSupporter.ReadOnly);
			});
		}

		void AssertPKModuleField(string key, bool readOnly, ModuleIdentifier moduleID, Type collectionType)
		{
			OperationalActionPKModuleFieldSupporter fieldSupporter = AssertField<OperationalActionPKModuleFieldSupporter>(key);
			CombineAssertions(delegate
			{
				AssertEquals(string.Format("'{0}' should have the correct readonlyness.", key), readOnly, fieldSupporter.ReadOnly);
				BusinessObjectFactory factory = new BusinessObjectFactory();
				IBusinessObjectCollection collection = fieldSupporter.ConstructCollection(factory);
				AssertNotNull(string.Format("'{0}'.ConstructCollection should not return null", key), collection);
				if (collection != null)
				{
					AssertEquals(string.Format("'{0}'.ConstructCollection should return the correct type of collection", key), collectionType, collection.GetType());
					AssertEquals(string.Format("'{0}'.ConstructCollection should return a collection with the correct factory", key), factory, collection.Factory);
					AssertEquals(string.Format("'{0}'.ConstructCollection should return a collection with the correct module", key), moduleID, ZMetaData.GetModuleId(collection));
				}
			});
		}

		void AssertNKModuleField(string key, bool readOnly, int maxLength, ModuleIdentifier moduleID, Type collectionType)
		{
			OperationalActionNKModuleFieldSupporter fieldSupporter = AssertField<OperationalActionNKModuleFieldSupporter>(key);
			CombineAssertions(delegate
			{
				AssertEquals(string.Format("'{0}' should have the correct readonlyness.", key), readOnly, fieldSupporter.ReadOnly);
				AssertEquals(string.Format("'{0}' should have the correct max length", key), maxLength, fieldSupporter.MaxLength);
				BusinessObjectFactory factory = new BusinessObjectFactory();
				IBusinessObjectCollection collection = fieldSupporter.ConstructCollection(factory);
				AssertNotNull(string.Format("'{0}'.ConstructCollection should not return null", key), collection);
				if (collection != null)
				{
					AssertEquals(string.Format("'{0}'.ConstructCollection should return the correct type of collection", key), collectionType, collection.GetType());
					AssertEquals(string.Format("'{0}'.ConstructCollection should return a collection with the correct factory", key), factory, collection.Factory);
					AssertEquals(string.Format("'{0}'.ConstructCollection should return a collection with the correct module", key), moduleID, ZMetaData.GetModuleId(collection));
				}
			});
		}

		void AssertAddressField(string key, bool readOnly, AddressType defaultAddressType, Type collectionType)
		{
			OperationalActionAddressFieldSupporter fieldSupporter = AssertField<OperationalActionAddressFieldSupporter>(key);
			CombineAssertions(delegate
			{
				AssertEquals(string.Format("'{0}' should have the correct readonlyness.", key), readOnly, fieldSupporter.ReadOnly);
				AssertEquals(string.Format("'{0}' should have the correct default address type", key), defaultAddressType, fieldSupporter.DefaultAddressType);
				BusinessObjectFactory factory = new BusinessObjectFactory();
				IBusinessObjectCollection collection = fieldSupporter.ConstructCollection(factory);
				AssertNotNull(string.Format("'{0}'.ConstructCollection should not return null", key), collection);
				if (collection != null)
				{
					AssertEquals(string.Format("'{0}'.ConstructCollection should return the correct type of collection", key), collectionType, collection.GetType());
					AssertEquals(string.Format("'{0}'.ConstructCollection should return a collection with the correct factory", key), factory, collection.Factory);
				}
			});
		}

		void AssertBooleanField(string key, bool readOnly)
		{
			OperationalActionBooleanFieldSupporter fieldSupporter = AssertField<OperationalActionBooleanFieldSupporter>(key);
			AssertEquals(string.Format("'{0}' should have the correct readonlyness.", key), readOnly, fieldSupporter.ReadOnly);
		}

		void AssertNumericField(string key, bool readOnly, int min, int max)
		{
			int precision = Math.Max(CalculatePrecision(min), CalculatePrecision(max));
			AssertNumericField(key, readOnly, min, max, precision, 0);
		}

		void AssertNumericField(string key, bool readOnly, decimal min, decimal max, int precision, int scale)
		{
			OperationalActionNumericFieldSupporter fieldSupporter = AssertField<OperationalActionNumericFieldSupporter>(key);
			CombineAssertions(delegate
			{
				AssertEquals(string.Format("'{0}' should have the correct readonlyness.", key), readOnly, fieldSupporter.ReadOnly);
				AssertEquals(string.Format("'{0}' should have the correct min value.", key), min, fieldSupporter.MinValue);
				AssertEquals(string.Format("'{0}' should have the correct max value.", key), max, fieldSupporter.MaxValue);
				AssertEquals(string.Format("'{0}' should have the correct precision.", key), precision, fieldSupporter.Precision);
				AssertEquals(string.Format("'{0}' should have the correct scale.", key), scale, fieldSupporter.Scale);
			});
		}

		void AssertGeographyField(string key, bool readOnly)
		{
			OperationalActionGeographyFieldSupporter fieldSupporter = AssertField<OperationalActionGeographyFieldSupporter>(key);
			AssertEquals(string.Format("'{0}' should have the correct readonlyness.", key), readOnly, fieldSupporter.ReadOnly);
		}

		void AssertNoField(string key)
		{
			PropertyInfo[] path = ReflectionHelper.FieldTextToPath(typeof(DummyForGeneration), key);
			OperationalActionFieldSupporter fieldSupporter = Generator.CreateField(path);
			AssertNull(fieldSupporter);
		}

		void AssertBooleanCustomField(string key, bool readOnly)
		{
			var fieldSupporter = AssertCustomField<OperationalActionBooleanFieldSupporter>(key);
			AssertEquals(string.Format("'{0}' should have the correct readonlyness.", key), readOnly, fieldSupporter.ReadOnly);
		}

		void AssertDateCustomField(string key, bool readOnly)
		{
			var fieldSupporter = AssertCustomField<OperationalActionDateFieldSupporter>(key);
			AssertEquals(string.Format("'{0}' should have the correct readonlyness.", key), readOnly, fieldSupporter.ReadOnly);
		}

		void AssertDateTimeCustomField(string key, bool readOnly, ZDateTimePickerFormat dateTimeFormat)
		{
			var fieldSupporter = AssertCustomField<OperationalActionDateTimeFieldSupporter>(key);
			CombineAssertions(delegate
			{
				AssertEquals(string.Format("'{0}' should have the correct readonlyness.", key), readOnly, fieldSupporter.ReadOnly);
				AssertEquals(string.Format("'{0}' should have the correct format", key), dateTimeFormat, fieldSupporter.Format);
			});
		}

		void AssertZDecimalCustomField(string key, bool readOnly, decimal min, decimal max, int precision, int scale)
		{
			var fieldSupporter = AssertCustomField<OperationalActionNumericFieldSupporter>(key);
			CombineAssertions(delegate
			{
				AssertEquals(string.Format("'{0}' should have the correct readonlyness.", key), readOnly, fieldSupporter.ReadOnly);
				AssertEquals(string.Format("'{0}' should have the correct min value.", key), min, fieldSupporter.MinValue);
				AssertEquals(string.Format("'{0}' should have the correct max value.", key), max, fieldSupporter.MaxValue);
				AssertEquals(string.Format("'{0}' should have the correct precision.", key), precision, fieldSupporter.Precision);
				AssertEquals(string.Format("'{0}' should have the correct scale.", key), scale, fieldSupporter.Scale);
			});
		}

		void AssertIntCustomField(string key, bool readOnly, decimal min, decimal max, int precision, int scale)
		{
			var fieldSupporter = AssertCustomField<OperationalActionNumericFieldSupporter>(key);
			CombineAssertions(delegate
			{
				AssertEquals(string.Format("'{0}' should have the correct readonlyness.", key), readOnly, fieldSupporter.ReadOnly);
				AssertEquals(string.Format("'{0}' should have the correct min value.", key), min, fieldSupporter.MinValue);
				AssertEquals(string.Format("'{0}' should have the correct max value.", key), max, fieldSupporter.MaxValue);
				AssertEquals(string.Format("'{0}' should have the correct precision.", key), precision, fieldSupporter.Precision);
				AssertEquals(string.Format("'{0}' should have the correct scale.", key), scale, fieldSupporter.Scale);
			});
		}

		void AssertTextCustomField(string key, bool readOnly, int maxLength)
		{
			var fieldSupporter = AssertCustomField<OperationalActionTextFieldSupporter>(key);
			CombineAssertions(delegate
			{
				AssertEquals(string.Format("'{0}' should have the correct readonlyness.", key), readOnly, fieldSupporter.ReadOnly);
				AssertEquals(string.Format("'{0}' should have the correct max length", key), maxLength, fieldSupporter.MaxLength);
			});
		}

		void AssertCodeCustomField(string key, bool readOnly, int maxLength, int listCount, string elementsAsString)
		{
			var fieldSupporter = AssertCustomField<OperationalActionCodeFieldSupporter>(key);
			CombineAssertions(() =>
			{
				AssertEquals($"'{key}' should have the correct readonlyness.", readOnly, fieldSupporter.ReadOnly);
				AssertEquals($"'{key}' should have the correct list count.", listCount, fieldSupporter.List.Count);
				AssertEquals($"'{key}' should have the correct code max length", maxLength, fieldSupporter.MaxLength);
				AssertEquals($"'{key}' should have the correct elements", elementsAsString, fieldSupporter.List.ElementsAsString);
			});
		}

		T AssertField<T>(string key)
			where T : OperationalActionFieldSupporter
		{
			PropertyInfo[] path = ReflectionHelper.FieldTextToPath(typeof(DummyForGeneration), key);
			OperationalActionFieldSupporter fieldSupporter = Generator.CreateField(path);
			AssertNotNull(string.Format("Expecting to find '{0}'", key), fieldSupporter);
			AssertEquals(string.Format("'{0}' should be of the correct type.", key), typeof(T), fieldSupporter.GetType());
			return (T)fieldSupporter;
		}

		T AssertCustomField<T>(string key)
			where T : OperationalActionFieldSupporter
		{
			var fieldSupporter = Generator.CreateField(key, DummyWorkflowDescriptor.Instance.Code);
			AssertNotNull(string.Format("Expecting to find '{0}'", key), fieldSupporter);
			AssertEquals(string.Format("'{0}' should be of the correct type.", key), typeof(T), fieldSupporter.GetType());
			return (T)fieldSupporter;
		}

		int CalculatePrecision(int value)
		{
			int result = 1;
			while (value > 9)
			{
				value /= 10;
				result++;
			}

			return result;
		}

		#endregion
		#region Dummy Classes
		internal sealed class CodedList : ReadOnlyCodeDescriptionPairList
		{
			public CodedList()
			{
				Elements.Add(new CodeDescriptionPair("A", "Alpha"));
				Elements.Add(new CodeDescriptionPair("B", "Betai"));
				Elements.Add(new CodeDescriptionPair("C", "Gamma"));
				Elements.Add(new CodeDescriptionPair("D", "Delta"));
			}
		}

		internal sealed class DummyForGeneration : DummyBusinessObject
		{
			public new class Schema : DummyBusinessObject.Schema
			{
				public const string Z0_CustomDecimal = "Z0_CustomDecimal";
				public const string Z0_RL_ParentHidden = "Z0_RL_ParentHidden";
				public const string Z0_RL_ParentShown = "Z0_RL_ParentShown";
				public const string Z0_RL_NKLoad = "Z0_RL_NKLoad";
				public const string Z0_OH_Consignor = "Z0_OH_Consignor";
				public const string Z0_NKOrigin = "Z0_NKOrigin";
				public const string ContactPK = "ContactPK";
				public const string Z0_OA_TextFieldThatLooksLikeAModuleField = "Z0_OA_TextFieldThatLooksLikeAModuleField";
				public const string Z0_URL = "Z0_URL";
				public const string Z0_OtherCode = "Z0_OtherCode";
				public const string Z0_F3_NKPackType = "Z0_F3_NKPackType";
				public const string Z0_OA_ObviousAddressField = "Z0_OA_ObviousAddressField";
				public const string Z0_ExplicitAddress = "Z0_ExplicitAddress";
				public const string NotRN = "NotRN";
				public const string IsFlagged = "IsFlagged";
			}

			public DummyForGeneration(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ZBool IsFlagged
			{
				get
				{
					return ZBool.False;
				}
			}

			[ActionField(FieldType = ActionFieldType.Numeric, MinValue = -100, MaxValue = 100, MaxLength = 3, Scale = 0)]
			public ZDecimal Z0_CustomDecimal
			{
				get
				{
					return 0;
				}

				set
				{
				}
			}

			public ZString Z0_RL_ParentHidden
			{
				get
				{
					return "";
				}

				set
				{
				}
			}

			[ActionField]
			public ZString Z0_RL_ParentShown
			{
				get
				{
					return "";
				}

				set
				{
				}
			}

			public ZString Z0_RL_NKLoad
			{
				get
				{
					return "";
				}

				set
				{
				}
			}

			[ActionField(CollectionType = typeof(RefUNLOCOCollection), ReadOnly = true)]
			public ZString Z0_NKOrigin
			{
				get
				{
					return "";
				}

				set
				{
				}
			}

			public ZGuid Z0_OH_Consignor
			{
				get
				{
					return ZGuid.Empty;
				}

				set
				{
				}
			}

			[ActionField(FieldType = ActionFieldType.Text, MaxLength = 35)]
			public ZString Z0_OA_TextFieldThatLooksLikeAModuleField
			{
				get
				{
					return ZString.Empty;
				}

				set
				{
				}
			}

			[ActionField(FieldType = ActionFieldType.Text, MaxLength = 250)]
			public ZString Z0_URL
			{
				get
				{
					return ZString.Empty;
				}

				set
				{
				}
			}

			[ActionField(CollectionType = typeof(OrgContactCollection))]
			public ZGuid ContactPK
			{
				get
				{
					return ZGuid.Empty;
				}

				set
				{
				}
			}

			[ActionField(MaxLength = 2)]
			public ZString NotRN
			{
				get
				{
					return ZString.Empty;
				}

				set
				{
				}
			}

			[ActionField(DateTimeFormat = ZDateTimePickerFormat.Long)]
			public override ZDateTime Z0_Date
			{
				get
				{
					return base.Z0_Date;
				}

				set
				{
					base.Z0_Date = value;
				}
			}

			[ActionField(DateTimeFormat = ZDateTimePickerFormat.Short)]
			public override ZDateTime Z0_AnotherDate
			{
				get
				{
					return base.Z0_AnotherDate;
				}

				set
				{
					base.Z0_AnotherDate = value;
				}
			}

			[ActionField()]
			public override ZDate Z0_DateOnly
			{
				get
				{
					return base.Z0_DateOnly;
				}

				set
				{
					base.Z0_DateOnly = value;
				}
			}

			[ZDateTimeDurationValue]
			public override ZDateTime Z0_SmallDateTime
			{
				get => base.Z0_SmallDateTime;
				set => base.Z0_SmallDateTime = value;
			}

			[ActionField(LookUpEditType = OLookUpEditType.TransportType)]
			public override ZString Z0_Code
			{
				get
				{
					return base.Z0_Code;
				}

				set
				{
					base.Z0_Code = value;
				}
			}

			[ActionField(CollectionType = typeof(CodedList))]
			public ZString Z0_OtherCode
			{
				get
				{
					return ZString.Empty;
				}

				set
				{
				}
			}

			public ZString Z0_F3_NKPackType
			{
				get
				{
					return ZString.Empty;
				}

				set
				{
				}
			}

			[ActionField(MaxLength = 100)]
			public override ZString Z0_VarCharMax
			{
				get
				{
					return base.Z0_VarCharMax;
				}

				set
				{
					base.Z0_VarCharMax = value;
				}
			}

			public ZGuid Z0_OA_ObviousAddressField
			{
				get
				{
					return ZGuid.Empty;
				}

				set
				{
				}
			}

			[ActionField(FieldType = ActionFieldType.Address, DefaultAddressType = AddressType.DLV)]
			public ZGuid Z0_ExplicitAddress
			{
				get
				{
					return ZGuid.Empty;
				}

				set
				{
				}
			}
		}
		#endregion
	}
}
