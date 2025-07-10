using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	[TestedType(typeof(MassUpdateWizard))]
	sealed class MassUpdateWizardTest : NonPersistentBusinessObjectTestCase
	{
		public void TestGetBindingList()
		{
			Helper.Z0_GuidBindToListForTesting = CurrencyList;
			Helper.Z0_FK_CodeBindToListForTesting = PairList;
			MassUpdateWizard wizard = new MassUpdateWizard(Helper.CollectionInfo);
			AssertNull(wizard.GetBindingList(DummyBizoSchema.Constants.Z0_VarCharMax));
			AssertEquals(CurrencyList, wizard.GetBindingList(DummyBizoSchema.Constants.Z0_Guid));
			AssertEquals(PairList, wizard.GetBindingList(DummyBizoSchema.Constants.Z0_FK_Code));
		}

		public void TestGetModuleId()
		{
			Helper.Z0_GuidBindToListForTesting = CurrencyList;
			Helper.Z0_FK_CodeBindToListForTesting = PairList;
			Helper.Z0_GuidModuleIDForTesting = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			MassUpdateWizard wizard = new MassUpdateWizard(Helper.CollectionInfo);
			AssertNull(wizard.GetModuleId(DummyBizoSchema.Constants.Z0_VarCharMax));
			AssertEquals(ModuleIDs.RefCurrency, wizard.GetModuleId(DummyBizoSchema.Constants.Z0_Guid));
			AssertEquals(ModuleIDs.NotAssigned, wizard.GetModuleId(DummyBizoSchema.Constants.Z0_FK_Code));
		}

		public void TestGetFieldType()
		{
			Helper.Z0_GuidBindToListForTesting = CurrencyList;
			Helper.Z0_FK_CodeBindToListForTesting = PairList;
			MassUpdateWizard wizard = new MassUpdateWizard(Helper.CollectionInfo);
			Dictionary<string, FieldType> expectedResult = new Dictionary<string, FieldType>();
			expectedResult.Add("", FieldType.Text);
			expectedResult.Add(DummyBizoSchema.Constants.Z0_Number, FieldType.Integer);
			expectedResult.Add(DummyBizoSchema.Constants.Z0_VarCharMax, FieldType.TextMultiLine);
			expectedResult.Add(DummyBizoSchema.Constants.Z0_Bool, FieldType.Boolean);
			expectedResult.Add(DummyBizoSchema.Constants.Z0_Short, FieldType.Integer);
			expectedResult.Add(DummyBizoSchema.Constants.Z0_AnotherDecimal, FieldType.Decimal);
			expectedResult.Add(DummyBizoSchema.Constants.Z0_Date, FieldType.DateTime);
			expectedResult.Add(DummyBizoSchema.Constants.Z0_DateTimeOffset, FieldType.DateTimeOffset);
			expectedResult.Add(DummyBizoSchema.Constants.Z0_Geography, FieldType.Geography);
			expectedResult.Add(DummyBizoSchema.Constants.Z0_Guid, FieldType.GuidDropEdit);
			expectedResult.Add(DummyBizoSchema.Constants.Z0_FK_Code, FieldType.TextDropEdit);
			expectedResult.Add(DummyBizoSchema.Constants.Z0_Byte, FieldType.Byte);
			expectedResult.Add(DummyBizoSchema.Constants.Z0_Time, FieldType.Time);

			foreach (KeyValuePair<string, FieldType> pair in expectedResult)
			{
				AssertEquals(pair.Key, pair.Value, wizard.GetFieldType(pair.Key));
			}

			Helper.Z0_GuidBindToListForTesting = new DummyBusinessObjectCollection(Factory);
			Helper.Z0_FK_CodeBindToListForTesting = currencyList;
			Helper.Z0_GuidModuleIDForTesting = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCurrency;
			wizard = new MassUpdateWizard(Helper.CollectionInfo);

			expectedResult.Clear();
			expectedResult.Add(DummyBizoSchema.Constants.Z0_Guid, FieldType.Guid);
			expectedResult.Add(DummyBizoSchema.Constants.Z0_FK_Code, FieldType.TextCodeFindBox);

			foreach (KeyValuePair<string, FieldType> pair in expectedResult)
			{
				AssertEquals(pair.Key, pair.Value, wizard.GetFieldType(pair.Key));
			}
		}

		public void TestGetMaxLength()
		{
			MassUpdateWizard wizard = new MassUpdateWizard(Helper.CollectionInfo);
			Dictionary<string, int> expectedResult = new Dictionary<string, int>();
			expectedResult.Add("", -1);
			expectedResult.Add(DummyBizoSchema.Constants.Z0_Number, -1);
			expectedResult.Add(DummyBizoSchema.Constants.Z0_Bool, -1);
			expectedResult.Add(DummyBizoSchema.Constants.Z0_Short, -1);
			expectedResult.Add(DummyBizoSchema.Constants.Z0_Decimal, -1);
			expectedResult.Add(DummyBizoSchema.Constants.Z0_Date, -1);
			expectedResult.Add(DummyBizoSchema.Constants.Z0_Guid, -1);
			expectedResult.Add(DummyBizoSchema.Constants.Z0_FK_Code, DummyBusinessObject.Schema.Z0_FK_CodeMaxLength);
			expectedResult.Add(DummyBizoSchema.Constants.Z0_Byte, -1);
			expectedResult.Add(DummyBizoSchema.Constants.Z0_DateTimeOffset, -1);
			expectedResult.Add(DummyBizoSchema.Constants.Z0_Geography, -1);
			expectedResult.Add(DummyBizoSchema.Constants.Z0_Time, -1);

			foreach (KeyValuePair<string, int> pair in expectedResult)
			{
				AssertEquals(pair.Key, pair.Value, wizard.GetMaxLength(pair.Key));
			}
		}

		public void TestFieldMaxLength()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Collection.AddNew();
			var collectionInfo = new ImportCollectionInfoImpl(dummy.Collection)
					{
						new ImportPropertyInfoImpl<DummyChildBusinessObject>(DummyBizoSchema.Constants.Z0_Number) { HeaderText = "Very very very very very very very very very long string" },
					};
			MassUpdateWizard wizard = new MassUpdateWizard(collectionInfo);
			AssertEquals("Z0_Number (Very very very very very very very very very long string)".Length, wizard.FieldInfo.MaxLength);
		}

		public void TestFieldMaxLengthWithNoField()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Collection.AddNew();
			var collectionInfo = new ImportCollectionInfoImpl(dummy.Collection);
			MassUpdateWizard wizard = new MassUpdateWizard(collectionInfo);
			AssertEquals(68, wizard.Field_MaxLength);
		}

		public void TestGetBindToField()
		{
			MassUpdateWizard wizard = new MassUpdateWizard(Helper.CollectionInfo);
			Dictionary<Type, string> expectedResult = new Dictionary<Type, string>();
			expectedResult.Add(typeof(ZInt), MassUpdateWizard.Schema.NewZIntValue);
			expectedResult.Add(typeof(ZString), MassUpdateWizard.Schema.NewZStringValue);
			expectedResult.Add(typeof(ZBool), MassUpdateWizard.Schema.NewZBoolValue);
			expectedResult.Add(typeof(ZShort), MassUpdateWizard.Schema.NewZShortValue);
			expectedResult.Add(typeof(ZDecimal), MassUpdateWizard.Schema.NewZDecimalValue);
			expectedResult.Add(typeof(ZDate), MassUpdateWizard.Schema.NewZDateTimeValue);
			expectedResult.Add(typeof(ZDateTime), MassUpdateWizard.Schema.NewZDateTimeValue);
			expectedResult.Add(typeof(ZDateTimeOffset), MassUpdateWizard.Schema.NewZDateTimeOffsetValue);
			expectedResult.Add(typeof(ZGeography), MassUpdateWizard.Schema.NewZGeographyValue);
			expectedResult.Add(typeof(ZGuid), MassUpdateWizard.Schema.NewZGuidValue);
			expectedResult.Add(typeof(ZByte), MassUpdateWizard.Schema.NewZByteValue);
			expectedResult.Add(typeof(ZTime), MassUpdateWizard.Schema.NewZTimeValue);

			AssertEquals(MassUpdateWizard.Schema.NewZStringValue, wizard.GetBindToField(null));

			foreach (KeyValuePair<Type, string> pair in expectedResult)
			{
				AssertEquals(pair.Value, wizard.GetBindToField(pair.Key));
			}
		}

		public void TestReadOnlyFieldShouldNotCheckMaxLength()
		{
			var collection = new DummyBusinessObjectCollection(Factory);
			var bizObj1 = Factory.New<DummyBusinessObject>();
			bizObj1.Z0_FK_Code = ZString.Empty;
			bizObj1.Z0_FK_Code_MaxLength = 0;
			collection.Add(bizObj1);

			var collectionInfo = new ImportCollectionInfoImpl(collection)
			{
				new ImportPropertyInfoImpl<DummyBusinessObject>(DummyBusinessObject.Schema.Z0_FK_Code)
			};

			var wizard = new MassUpdateWizard(collectionInfo);
			wizard.Field = string.Format("{0} ({0})", DummyBusinessObject.Schema.Z0_FK_Code);
			AssertNoExceptionThrown(() => wizard.NewZStringValue = string.Format("Text ({0})", MassUpdateWizard.Schema.NewZStringValue));
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestLoadBizObjsToUpdate_OverlappingFilters()
		{
			ZGuid pk1 = ZGuid.NewZGuid();
			DummyChildBusinessObjectCollection collection = (DummyChildBusinessObjectCollection)Helper.CollectionInfo.Collection;
			collection.RemoveAndDeleteAll();
			DummyChildBusinessObject child1 = CreateChild(collection, 1, "ABC", false, 68, 3.3, new ZDateTime(2009, 2, 14), pk1, "CODE1", (ZByte)23, "HELLO1", ZDateTimeOffset.Empty, ZGeography.Empty, ZTime.Empty);
			DummyChildBusinessObject child2 = CreateChild(collection, 2, "ABC", false, 68, 3.3, new ZDateTime(2009, 2, 14), pk1, "CODE2", (ZByte)23, "HELLO2", ZDateTimeOffset.Invalid, ZGeography.Invalid, ZTime.Invalid);
			DummyChildBusinessObject child3 = CreateChild(collection, 1, "DEF", false, 68, 3.3, new ZDateTime(2009, 2, 14), pk1, "CODE3", (ZByte)23, "HELLO3", new ZDateTimeOffset(2003, 5, 5, 8, 9, 10, TimeSpan.FromHours(5)), new ZGeography("POINT (-122 48)"), new ZTime(1,2));
			DummyChildBusinessObject child4 = CreateChild(collection, 2, "DEF", true, 68, 3.3, new ZDateTime(2009, 2, 14), pk1, "CODE4", (ZByte)23, "HELLO4", new ZDateTimeOffset(2004, 5, 5, 8, 9, 10, TimeSpan.FromHours(10)), new ZGeography("POINT (-121 49.4)"), new ZTime(4,5));

			MassUpdateWizard wizard = new MassUpdateWizard(Helper.CollectionInfo);
			AssertEquals(0, wizard.BizObjsToUpdate.Count);
			wizard.LoadBizObjsToUpdate();
			AssertEquals(4, wizard.BizObjsToUpdate.Count);
			AssertEquals(child1.Z0_FK_Code, wizard.BizObjsToUpdate[0][DummyBizoSchema.Constants.Z0_FK_Code]);
			AssertEquals(child2.Z0_FK_Code, wizard.BizObjsToUpdate[1][DummyBizoSchema.Constants.Z0_FK_Code]);
			AssertEquals(child3.Z0_FK_Code, wizard.BizObjsToUpdate[2][DummyBizoSchema.Constants.Z0_FK_Code]);
			AssertEquals(child4.Z0_FK_Code, wizard.BizObjsToUpdate[3][DummyBizoSchema.Constants.Z0_FK_Code]);

			MassUpdateMatchingFilter filter1 = wizard.MatchingFilters.AddNew();
			filter1.Field = "Text (" + DummyBizoSchema.Constants.Z0_VarCharMax + ")";
			filter1.Operator = MassUpdateMatchingFilterOperatorList.Codes.Equal;
			filter1.FieldValue = "ABC";
			MassUpdateMatchingFilter filter2 = wizard.MatchingFilters.AddNew();
			filter2.Field = "Text (" + DummyBizoSchema.Constants.Z0_VarCharMax + ")";
			filter2.Operator = MassUpdateMatchingFilterOperatorList.Codes.Equal;
			filter2.FieldValue = "DEF";
			wizard.LoadBizObjsToUpdate();
			AssertEquals(4, wizard.BizObjsToUpdate.Count);
			AssertEquals(child1.Z0_FK_Code, wizard.BizObjsToUpdate[0][DummyBizoSchema.Constants.Z0_FK_Code]);
			AssertEquals(child2.Z0_FK_Code, wizard.BizObjsToUpdate[1][DummyBizoSchema.Constants.Z0_FK_Code]);
			AssertEquals(child3.Z0_FK_Code, wizard.BizObjsToUpdate[2][DummyBizoSchema.Constants.Z0_FK_Code]);
			AssertEquals(child4.Z0_FK_Code, wizard.BizObjsToUpdate[3][DummyBizoSchema.Constants.Z0_FK_Code]);

			filter1.Operator = MassUpdateMatchingFilterOperatorList.Codes.NotEqualTo;
			filter2.Operator = MassUpdateMatchingFilterOperatorList.Codes.NotEqualTo;
			wizard.LoadBizObjsToUpdate();
			AssertEquals(0, wizard.BizObjsToUpdate.Count);

			filter1.Field = "NText (" + DummyBizoSchema.Constants.Z0_NVarCharMax + ")";
			filter1.Operator = MassUpdateMatchingFilterOperatorList.Codes.Equal;
			filter1.FieldValue = "HELLO1";
			filter2.Field = "NText (" + DummyBizoSchema.Constants.Z0_NVarCharMax + ")";
			filter2.Operator = MassUpdateMatchingFilterOperatorList.Codes.Equal;
			filter2.FieldValue = "HELLO2";

			wizard.LoadBizObjsToUpdate();
			AssertEquals(2, wizard.BizObjsToUpdate.Count);
			AssertEquals(child1.Z0_FK_Code, wizard.BizObjsToUpdate[0][DummyBizoSchema.Constants.Z0_FK_Code]);
			AssertEquals(child2.Z0_FK_Code, wizard.BizObjsToUpdate[1][DummyBizoSchema.Constants.Z0_FK_Code]);

			filter1.Operator = MassUpdateMatchingFilterOperatorList.Codes.NotEqualTo;
			filter2.Operator = MassUpdateMatchingFilterOperatorList.Codes.NotEqualTo;
			wizard.LoadBizObjsToUpdate();
			AssertEquals(2, wizard.BizObjsToUpdate.Count);
			AssertEquals(child3.Z0_FK_Code, wizard.BizObjsToUpdate[0][DummyBizoSchema.Constants.Z0_FK_Code]);
			AssertEquals(child4.Z0_FK_Code, wizard.BizObjsToUpdate[1][DummyBizoSchema.Constants.Z0_FK_Code]);
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestLoadBizObjsToUpdate()
		{
			ZGuid pk1 = ZGuid.NewZGuid();
			DummyChildBusinessObjectCollection collection = (DummyChildBusinessObjectCollection)Helper.CollectionInfo.Collection;
			collection.RemoveAndDeleteAll();
			DummyChildBusinessObject child1 = CreateChild(collection, 1, "ABC", false, 68, 3.3, new ZDateTime(2009, 2, 14), pk1, "CODE1", (ZByte)23, "HELLO", ZDateTimeOffset.Empty, ZGeography.Empty, ZTime.Empty);
			DummyChildBusinessObject child2 = CreateChild(collection, 2, "ABC", false, 68, 3.3, new ZDateTime(2009, 2, 14), pk1, "CODE2", (ZByte)23, "HELLO", ZDateTimeOffset.Invalid, ZGeography.Invalid, ZTime.Invalid);
			DummyChildBusinessObject child3 = CreateChild(collection, 1, "DEF", false, 68, 3.3, new ZDateTime(2009, 2, 14), pk1, "CODE3", (ZByte)23, "HELLO", new ZDateTimeOffset(2003, 5, 5, 8, 9, 10, TimeSpan.FromHours(5)), new ZGeography("POINT (-122 48)"), new ZTime(1,2));
			DummyChildBusinessObject child4 = CreateChild(collection, 2, "DEF", true, 68, 3.3, new ZDateTime(2009, 2, 14), pk1, "CODE4", (ZByte)23, "HELLO", new ZDateTimeOffset(2004, 5, 5, 8, 9, 10, TimeSpan.FromHours(10)), new ZGeography("POINT (-121 49.4)"), new ZTime(4,5));

			MassUpdateWizard wizard = new MassUpdateWizard(Helper.CollectionInfo);
			AssertEquals(0, wizard.BizObjsToUpdate.Count);
			wizard.LoadBizObjsToUpdate();
			AssertEquals(4, wizard.BizObjsToUpdate.Count);
			AssertEquals(child1.Z0_FK_Code, wizard.BizObjsToUpdate[0][DummyBizoSchema.Constants.Z0_FK_Code]);
			AssertEquals(child2.Z0_FK_Code, wizard.BizObjsToUpdate[1][DummyBizoSchema.Constants.Z0_FK_Code]);
			AssertEquals(child3.Z0_FK_Code, wizard.BizObjsToUpdate[2][DummyBizoSchema.Constants.Z0_FK_Code]);
			AssertEquals(child4.Z0_FK_Code, wizard.BizObjsToUpdate[3][DummyBizoSchema.Constants.Z0_FK_Code]);
			MassUpdateMatchingFilter filter1 = wizard.MatchingFilters.AddNew();
			filter1.Field = "Text (" + DummyBizoSchema.Constants.Z0_VarCharMax + ")";
			filter1.Operator = MassUpdateMatchingFilterOperatorList.Codes.Equal;
			filter1.FieldValue = "ABC";
			wizard.LoadBizObjsToUpdate();
			AssertEquals(2, wizard.BizObjsToUpdate.Count);
			AssertEquals(child1.Z0_FK_Code, wizard.BizObjsToUpdate[0][DummyBizoSchema.Constants.Z0_FK_Code]);
			AssertEquals(child2.Z0_FK_Code, wizard.BizObjsToUpdate[1][DummyBizoSchema.Constants.Z0_FK_Code]);

			filter1.Operator = MassUpdateMatchingFilterOperatorList.Codes.NotEqualTo;
			wizard.LoadBizObjsToUpdate();
			AssertEquals(2, wizard.BizObjsToUpdate.Count);
			AssertEquals(child3.Z0_FK_Code, wizard.BizObjsToUpdate[0][DummyBizoSchema.Constants.Z0_FK_Code]);
			AssertEquals(child4.Z0_FK_Code, wizard.BizObjsToUpdate[1][DummyBizoSchema.Constants.Z0_FK_Code]);

			MassUpdateMatchingFilter filter2 = wizard.MatchingFilters.AddNew();
			filter2.Field = "Number (" + DummyBizoSchema.Constants.Z0_Number + ")";
			filter2.FieldValue = "2";

			MassUpdateMatchingFilter filter3 = wizard.MatchingFilters.AddNew();
			filter3.Field = "Bool (" + DummyBizoSchema.Constants.Z0_Bool + ")";
			filter3.FieldValue = ZBool.True.ToString();

			MassUpdateMatchingFilter filter4 = wizard.MatchingFilters.AddNew();
			filter4.Field = "Short (" + DummyBizoSchema.Constants.Z0_Short + ")";
			filter4.FieldValue = "68";

			MassUpdateMatchingFilter filter5 = wizard.MatchingFilters.AddNew();
			filter5.Field = "Decimal (" + DummyBizoSchema.Constants.Z0_Decimal + ")";
			filter5.FieldValue = "3.3";

			MassUpdateMatchingFilter filter6 = wizard.MatchingFilters.AddNew();
			filter6.Field = "Date (" + DummyBizoSchema.Constants.Z0_Date + ")";
			filter6.FieldValue = new ZDateTime(2009, 2, 14).ToString();

			MassUpdateMatchingFilter filter7 = wizard.MatchingFilters.AddNew();
			filter7.Field = "Guid (" + DummyBizoSchema.Constants.Z0_Guid + ")";
			filter7.FieldValue = pk1.ToString();

			MassUpdateMatchingFilter filter8 = wizard.MatchingFilters.AddNew();
			filter8.Field = "Byte (" + DummyBizoSchema.Constants.Z0_Byte + ")";
			filter8.FieldValue = "23";

			MassUpdateMatchingFilter filter9 = wizard.MatchingFilters.AddNew();
			filter9.Field = "NText (" + DummyBizoSchema.Constants.Z0_NVarCharMax + ")";
			filter9.FieldValue = "HELLO";

			MassUpdateMatchingFilter filter10 = wizard.MatchingFilters.AddNew();
			filter10.Field = "Date (With Offset) (" + DummyBizoSchema.Constants.Z0_DateTimeOffset + ")";
			filter10.FieldValue = new ZDateTime(2004, 5, 5, 8, 9, 10).ToString();

			MassUpdateMatchingFilter filter11 = wizard.MatchingFilters.AddNew();
			filter11.Field = "Geography (" + DummyBizoSchema.Constants.Z0_Geography + ")";
			filter11.FieldValue = new ZGeography("POINT (-121 49.4)").ToString();

			MassUpdateMatchingFilter filter12 = wizard.MatchingFilters.AddNew();
			filter12.Field = "Time (" + DummyBizoSchema.Constants.Z0_Time + ")";
			filter12.FieldValue = new ZTime(4, 5).ToString();

			wizard.LoadBizObjsToUpdate();
			AssertEquals(1, wizard.BizObjsToUpdate.Count);
			AssertEquals(child4.Z0_FK_Code, wizard.BizObjsToUpdate[0][DummyBizoSchema.Constants.Z0_FK_Code]);

			wizard.Field = "Decimal (" + DummyBizoSchema.Constants.Z0_AnotherDecimal + ")";
			wizard.NewZDecimalValue = 100;
			wizard.Modifier = MassUpdateModifierList.Codes.Add;
			wizard.LoadBizObjsToUpdate();
			AssertEquals(1, wizard.BizObjsToUpdate.Count);
			AssertEquals(103.3m, wizard.BizObjsToUpdate[0][DummyBizoSchema.Constants.Z0_AnotherDecimal]);

			wizard.NewZDecimalValue = 2;
			wizard.Modifier = MassUpdateModifierList.Codes.Subtract;
			wizard.LoadBizObjsToUpdate();
			AssertEquals(1, wizard.BizObjsToUpdate.Count);
			AssertEquals(1.3m, wizard.BizObjsToUpdate[0][DummyBizoSchema.Constants.Z0_AnotherDecimal]);

			wizard.NewZDecimalValue = 3;
			wizard.Modifier = MassUpdateModifierList.Codes.Multiply;
			wizard.LoadBizObjsToUpdate();
			AssertEquals(1, wizard.BizObjsToUpdate.Count);
			AssertEquals(9.9m, wizard.BizObjsToUpdate[0][DummyBizoSchema.Constants.Z0_AnotherDecimal]);

			wizard.NewZDecimalValue = 3;
			wizard.Modifier = MassUpdateModifierList.Codes.Divide;
			wizard.LoadBizObjsToUpdate();
			AssertEquals(1, wizard.BizObjsToUpdate.Count);
			AssertEquals(1.1m, wizard.BizObjsToUpdate[0][DummyBizoSchema.Constants.Z0_AnotherDecimal]);
		}

		DummyChildBusinessObject CreateChild(DummyChildBusinessObjectCollection collection, ZInt numberValue, ZString textValue, ZBool booleanValue,
			ZShort shortValue, ZDecimal decimalValue, ZDateTime dateValue, ZGuid guidValue, ZString fkCodeValue, ZByte byteValue, ZString ntextValue,
			ZDateTimeOffset dateTimeOffsetValue, ZGeography geographyValue, ZTime timeValue)
		{
			DummyChildBusinessObject child = collection.AddNew();
			child.Z0_Number = numberValue;
			child.Z0_VarCharMax = textValue;
			child.Z0_Bool = booleanValue;
			child.Z0_Short = shortValue;
			child.Z0_AnotherDecimal = decimalValue;
			child.Z0_Date = dateValue;
			child.Z0_Guid = guidValue;
			child.Z0_FK_Code = fkCodeValue;
			child.Z0_Byte = byteValue;
			child.Z0_NVarCharMax = ntextValue;
			child.Z0_DateTimeOffset = dateTimeOffsetValue;
			child.Z0_Geography = geographyValue;
			child.Z0_Time = timeValue;
			return child;
		}

		[TestTimeZoneUNLOCO("AUSYD")]
		public void TestUpdate()
		{
			ZGuid pk1 = ZGuid.NewZGuid();
			DummyChildBusinessObjectCollection collection = (DummyChildBusinessObjectCollection)Helper.CollectionInfo.Collection;
			collection.RemoveAndDeleteAll();
			DummyChildBusinessObject child1 = CreateChild(collection, 1, "ABC", false, 68, 3.3, new ZDateTime(2009, 2, 14), pk1, "CODE1", (ZByte)23, "HELLO", ZDateTimeOffset.Empty, ZGeography.Empty, ZTime.Empty);
			DummyChildBusinessObject child2 = CreateChild(collection, 2, "ABC", false, 68, 3.3, new ZDateTime(2009, 2, 14), pk1, "CODE2", (ZByte)23, "HELLO", ZDateTimeOffset.Invalid, ZGeography.Invalid, ZTime.Invalid);
			child2.Z0_NVarCharMax_ReadOnly = true;
			DummyChildBusinessObject child3 = CreateChild(collection, 1, "DEF", false, 68, 3.3, new ZDateTime(2009, 2, 14), pk1, "CODE3", (ZByte)23, "HELLO", new ZDateTimeOffset(2003, 5, 5, 8, 9, 10, TimeSpan.FromHours(5)), new ZGeography("POINT (-122 48)"), new ZTime(1,2));
			DummyChildBusinessObject child4 = CreateChild(collection, 2, "DEF", true, 68, 3.3, new ZDateTime(2009, 2, 14), pk1, "CODE4", (ZByte)23, "HELLO", new ZDateTimeOffset(2004, 5, 5, 8, 9, 10, TimeSpan.Zero), new ZGeography("POINT (-121 49.4)"), new ZTime(4,5));

			MassUpdateWizard wizard = new MassUpdateWizard(Helper.CollectionInfo);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			MassUpdateMatchingFilter filter1 = wizard.MatchingFilters.AddNew();
			filter1.Field = "Text (" + DummyBizoSchema.Constants.Z0_VarCharMax + ")";
			filter1.FieldValue = "ZZZ";
			wizard.Update();
			AssertEquals("Cannot Update till all errors are fixed.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(true, wizard.HasErrors());

			wizard.Field = "NText (" + DummyBizoSchema.Constants.Z0_NVarCharMax + ")";
			wizard.NewZStringValue = "BYE";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			wizard.Update();
			AssertEquals(false, wizard.HasErrors());
			AssertEquals("0 record(s) matched; 0 were updated; 0 were 'Read Only'.", UnitTestUserNotification.Instance.LastMessage.Text);

			filter1.FieldValue = "ABC";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			wizard.Update();
			AssertEquals("2 record(s) matched; 1 were updated; 1 were 'Read Only'.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(child1.Z0_NVarCharMax, "BYE");
			AssertEquals(child2.Z0_NVarCharMax, "HELLO");

			filter1.Field = "Bool (" + DummyBizoSchema.Constants.Z0_Bool + ")";
			filter1.Operator = MassUpdateMatchingFilterOperatorList.Codes.NotEqualTo;
			filter1.FieldValue = ZBool.True.ToString();
			wizard.NewZStringValue = "HOWDY";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			wizard.Update();
			AssertEquals("3 record(s) matched; 2 were updated; 1 were 'Read Only'.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(child1.Z0_NVarCharMax, "HOWDY");
			AssertEquals(child2.Z0_NVarCharMax, "HELLO");
			AssertEquals(child3.Z0_NVarCharMax, "HOWDY");

			//Because this is always done from the GUI, expected business logic will be in place to change offset (if desired). So we won't apply our own logic (of using the offset from the previous value whenever possible).
			wizard.Field = "Date (With Offset) (" + DummyBizoSchema.Constants.Z0_DateTimeOffset + ")";
			wizard.NewZDateTimeOffsetValue = new ZDateTimeOffset(2002, 3, 1, 12, 1, 2, TimeSpan.FromHours(11));
			wizard.MatchingFilters.RemoveAndDeleteAll();
			wizard.Update();
			AssertEquals("4 record(s) matched; 4 were updated; 0 were 'Read Only'.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(child1.Z0_DateTimeOffset, new ZDateTimeOffset(2002, 3, 1, 12, 1, 2, TimeSpan.FromHours(11)));
			AssertEquals(child2.Z0_DateTimeOffset, new ZDateTimeOffset(2002, 3, 1, 12, 1, 2, TimeSpan.FromHours(11)));
			AssertEquals(child3.Z0_DateTimeOffset, new ZDateTimeOffset(2002, 3, 1, 12, 1, 2, TimeSpan.FromHours(11)));
			AssertEquals(child4.Z0_DateTimeOffset, new ZDateTimeOffset(2002, 3, 1, 12, 1, 2, TimeSpan.FromHours(11)));

			wizard.Field = "Geography (" + DummyBizoSchema.Constants.Z0_Geography + ")";
			wizard.NewZGeographyValue = new ZGeography("POINT (-123 44)");
			wizard.MatchingFilters.RemoveAndDeleteAll();
			wizard.Update();
			AssertEquals("4 record(s) matched; 4 were updated; 0 were 'Read Only'.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(child1.Z0_Geography, new ZGeography("POINT (-123 44)"));
			AssertEquals(child2.Z0_Geography, new ZGeography("POINT (-123 44)"));
			AssertEquals(child3.Z0_Geography, new ZGeography("POINT (-123 44)"));
			AssertEquals(child4.Z0_Geography, new ZGeography("POINT (-123 44)"));
		}

		public void TestUpdateNumericValueWithCalculationModifier()
		{
			var collection = (DummyChildBusinessObjectCollection)Helper.CollectionInfo.Collection;
			collection.RemoveAndDeleteAll();
			var child1 = CreateChild(collection, 10, "Item 1", false, 10, 10.001, ZDateTime.Empty, ZGuid.Empty, "CODE1", ZByte.Zero, ZString.Empty, ZDateTimeOffset.Empty, ZGeography.Empty, ZTime.Empty);
			var child2 = CreateChild(collection, 20, "Item 2", false, 20, 20.001, ZDateTime.Empty, ZGuid.Empty, "CODE2", ZByte.Zero, ZString.Empty, ZDateTimeOffset.Empty, ZGeography.Empty, ZTime.Empty);

			var wizard = new MassUpdateWizard(Helper.CollectionInfo);

			// Integer calculation
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			wizard.Field = "Number (" + DummyBizoSchema.Constants.Z0_Number + ")";
			wizard.NewZIntValue = 100;
			wizard.Modifier = MassUpdateModifierList.Codes.Add;
			wizard.Update();
			AssertEquals("2 record(s) matched; 2 were updated; 0 were 'Read Only'.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(110, child1.Z0_Number);
			AssertEquals(120, child2.Z0_Number);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			wizard.NewZIntValue = 100;
			wizard.Modifier = MassUpdateModifierList.Codes.Subtract;
			wizard.Update();
			AssertEquals("2 record(s) matched; 2 were updated; 0 were 'Read Only'.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(10, child1.Z0_Number);
			AssertEquals(20, child2.Z0_Number);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			wizard.NewZIntValue = 5;
			wizard.Modifier = MassUpdateModifierList.Codes.Multiply;
			wizard.Update();
			AssertEquals("2 record(s) matched; 2 were updated; 0 were 'Read Only'.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(50, child1.Z0_Number);
			AssertEquals(100, child2.Z0_Number);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			wizard.NewZIntValue = 5;
			wizard.Modifier = MassUpdateModifierList.Codes.Divide;
			wizard.Update();
			AssertEquals("2 record(s) matched; 2 were updated; 0 were 'Read Only'.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(10, child1.Z0_Number);
			AssertEquals(20, child2.Z0_Number);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			wizard.NewZIntValue = 3;
			wizard.Modifier = MassUpdateModifierList.Codes.Divide;
			wizard.Update();
			AssertEquals("2 record(s) matched; 2 were updated; 0 were 'Read Only'.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(3, child1.Z0_Number);
			AssertEquals(7, child2.Z0_Number);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			wizard.NewZIntValue = 999;
			wizard.Modifier = MassUpdateModifierList.Codes.Overwrite;
			wizard.Update();
			AssertEquals("2 record(s) matched; 2 were updated; 0 were 'Read Only'.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(999, child1.Z0_Number);
			AssertEquals(999, child2.Z0_Number);

			// Decimal calculation
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			wizard.Field = "Decimal (" + DummyBizoSchema.Constants.Z0_AnotherDecimal + ")";
			wizard.NewZDecimalValue = 100;
			wizard.Modifier = MassUpdateModifierList.Codes.Add;
			wizard.Update();
			AssertEquals("2 record(s) matched; 2 were updated; 0 were 'Read Only'.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(110.001m, child1.Z0_AnotherDecimal);
			AssertEquals(120.001m, child2.Z0_AnotherDecimal);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			wizard.NewZDecimalValue = 100;
			wizard.Modifier = MassUpdateModifierList.Codes.Subtract;
			wizard.Update();
			AssertEquals("2 record(s) matched; 2 were updated; 0 were 'Read Only'.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(10.001m, child1.Z0_AnotherDecimal);
			AssertEquals(20.001m, child2.Z0_AnotherDecimal);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			wizard.NewZDecimalValue = 5;
			wizard.Modifier = MassUpdateModifierList.Codes.Multiply;
			wizard.Update();
			AssertEquals("2 record(s) matched; 2 were updated; 0 were 'Read Only'.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(50.005m, child1.Z0_AnotherDecimal);
			AssertEquals(100.005m, child2.Z0_AnotherDecimal);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			wizard.NewZDecimalValue = 5;
			wizard.Modifier = MassUpdateModifierList.Codes.Divide;
			wizard.Update();
			AssertEquals("2 record(s) matched; 2 were updated; 0 were 'Read Only'.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(10.001m, child1.Z0_AnotherDecimal);
			AssertEquals(20.001m, child2.Z0_AnotherDecimal);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			wizard.NewZDecimalValue = 3;
			wizard.Modifier = MassUpdateModifierList.Codes.Divide;
			wizard.Update();
			AssertEquals("2 record(s) matched; 2 were updated; 0 were 'Read Only'.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(3.334m, child1.Z0_AnotherDecimal);
			AssertEquals(6.667m, child2.Z0_AnotherDecimal);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			wizard.NewZDecimalValue = 999;
			wizard.Modifier = MassUpdateModifierList.Codes.Overwrite;
			wizard.Update();
			AssertEquals("2 record(s) matched; 2 were updated; 0 were 'Read Only'.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(999m, child1.Z0_AnotherDecimal);
			AssertEquals(999m, child2.Z0_AnotherDecimal);
		}

		public void TestModifierFallbackToOverwriteForNonDecimalValue()
		{
			var wizard = new MassUpdateWizard(Helper.CollectionInfo);

			wizard.Field = "Number (" + DummyBizoSchema.Constants.Z0_Number + ")";
			wizard.Modifier = MassUpdateModifierList.Codes.Add;
			AssertEquals(MassUpdateModifierList.Codes.Add, wizard.Modifier);
			AssertEquals(false, wizard.IsModifierReadOnly);

			wizard.Field = "Decimal (" + DummyBizoSchema.Constants.Z0_AnotherDecimal + ")";
			wizard.Modifier = MassUpdateModifierList.Codes.Add;
			AssertEquals(MassUpdateModifierList.Codes.Add, wizard.Modifier);
			AssertEquals(false, wizard.IsModifierReadOnly);

			wizard.Field = "Text (" + DummyBizoSchema.Constants.Z0_VarCharMax + ")";
			AssertEquals(MassUpdateModifierList.Codes.Overwrite, wizard.Modifier);
			AssertEquals(true, wizard.IsModifierReadOnly);
		}

		public void TestNumericFieldsValidation()
		{
			var wizard = new MassUpdateWizard(Helper.CollectionInfo);

			wizard.Modifier = MassUpdateModifierList.Codes.Divide;
			wizard.NewZIntValue = 0;
			AssertHasErrors("Not allow to divide by zero", wizard.NewZIntValueInfo);
			wizard.NewZIntValue = 10;
			AssertNoErrors("Allow any non-zero values", wizard.NewZIntValueInfo);
			wizard.Modifier = MassUpdateModifierList.Codes.Overwrite;
			wizard.NewZIntValue = 0;
			AssertNoErrors("Allow to overwrite using zero", wizard.NewZIntValueInfo);

			wizard.Modifier = MassUpdateModifierList.Codes.Divide;
			wizard.NewZDecimalValue = 0;
			AssertHasErrors("Not allow to divide by zero", wizard.NewZDecimalValueInfo);
			wizard.NewZDecimalValue = 10;
			AssertNoErrors("Allow any non-zero values", wizard.NewZDecimalValueInfo);
			wizard.Modifier = MassUpdateModifierList.Codes.Overwrite;
			wizard.NewZDecimalValue = 0;
			AssertNoErrors("Allow to overwrite using zero", wizard.NewZDecimalValueInfo);
		}

		public void TestUpdateMultiControlColumns()
		{
			//Z0_NVarCharMax expects a string of a guid when Z0_BitFiltered is true, otherwise it just expects a string.
			//Similar to how ConsigneeNameOrPK works, just without the conversion of guid to the string code as well.
			//The wizard will use the behaviour of the first BizObj in the list, when deciding which type to use. Otherwise it's too difficult. (So it will work intuitively for collections that are all one or all other type.)
			ZGuid pk1 = ZGuid.NewZGuid();
			DummyChildBusinessObjectCollection collection = (DummyChildBusinessObjectCollection)Helper.CollectionInfo.Collection;
			collection.RemoveAndDeleteAll();
			DummyChildBusinessObject child1 = CreateChild(collection, 1, "ABC", true, 68, 3.3, new ZDateTime(2009, 2, 14), pk1, "CODE1", (ZByte)23, "d92343ad-cf5d-499b-a518-55e903d562f2", ZDateTimeOffset.Empty, ZGeography.Empty, ZTime.Empty);
			child1.Z0_BitFiltered = true;
			DummyChildBusinessObject child2 = CreateChild(collection, 2, "ABC", true, 68, 3.3, new ZDateTime(2009, 2, 14), pk1, "CODE2", (ZByte)23, "9c27e792-b3da-4505-9725-4f11c02f07dd", ZDateTimeOffset.Invalid, ZGeography.Invalid, ZTime.Invalid);
			child2.Z0_BitFiltered = true;
			DummyChildBusinessObject child3 = CreateChild(collection, 3, "DEF", true, 68, 3.3, new ZDateTime(2009, 2, 14), pk1, "CODE3", (ZByte)23, "NAME1", new ZDateTimeOffset(2003, 5, 5, 8, 9, 10, TimeSpan.FromHours(5)), new ZGeography("POINT (-122 48)"), new ZTime(1,2));
			child3.Z0_BitFiltered = false;
			DummyChildBusinessObject child4 = CreateChild(collection, 4, "DEF", true, 68, 3.3, new ZDateTime(2009, 2, 14), pk1, "CODE4", (ZByte)23, "NAME2", new ZDateTimeOffset(2004, 5, 5, 8, 9, 10, TimeSpan.Zero), new ZGeography("POINT (-121 49.4)"), new ZTime(4,5));
			child4.Z0_BitFiltered = false;

			AssertEquals(child1, collection[0]);
			AssertEquals(true, collection[0].Z0_BitFiltered);

			MassUpdateWizard wizard = new MassUpdateWizard(Helper.CollectionInfo);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			wizard.Field = "NText (" + DummyBizoSchema.Constants.Z0_NVarCharMax + ")";
			wizard.NewZGuidValue = new ZGuid("96374db6-b38e-4021-8516-bc8bdb1ee522");
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			wizard.Update();
			AssertEquals(false, wizard.HasErrors());
			AssertEquals("4 record(s) matched; 2 were updated; 0 were 'Read Only'.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(child1.Z0_NVarCharMax, "96374db6-b38e-4021-8516-bc8bdb1ee522");
			AssertEquals(child2.Z0_NVarCharMax, "96374db6-b38e-4021-8516-bc8bdb1ee522");
			AssertEquals(child3.Z0_NVarCharMax, "NAME1");
			AssertEquals(child4.Z0_NVarCharMax, "NAME2");

			collection.Sort("Z0_BitFiltered", ListSortDirection.Ascending);
			AssertEquals(false, collection[0].Z0_BitFiltered);
			wizard = new MassUpdateWizard(Helper.CollectionInfo);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			wizard.Field = "NText (" + DummyBizoSchema.Constants.Z0_NVarCharMax + ")";
			wizard.NewZStringValue = "NAME3";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			wizard.Update();
			AssertEquals(false, wizard.HasErrors());
			AssertEquals("4 record(s) matched; 2 were updated; 0 were 'Read Only'.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(child1.Z0_NVarCharMax, "96374db6-b38e-4021-8516-bc8bdb1ee522");
			AssertEquals(child2.Z0_NVarCharMax, "96374db6-b38e-4021-8516-bc8bdb1ee522");
			AssertEquals(child3.Z0_NVarCharMax, "NAME3");
			AssertEquals(child4.Z0_NVarCharMax, "NAME3");
		}

		public void TestLoadBizObjsToUpdateForShowMatched()
		{
			ZGuid pk1 = ZGuid.NewZGuid();
			DummyChildBusinessObjectCollection collection = (DummyChildBusinessObjectCollection)Helper.CollectionInfo.Collection;
			collection.RemoveAndDeleteAll();
			DummyChildBusinessObject child1 = CreateChild(collection, 1, "ABC", true, 68, 3.3, new ZDateTime(2009, 2, 14), pk1, "CODE1", (ZByte)23, "d92343ad-cf5d-499b-a518-55e903d562f2", ZDateTimeOffset.Empty, ZGeography.Empty, ZTime.Empty);
			child1.Z0_BitFiltered = true;
			DummyChildBusinessObject child2 = CreateChild(collection, 2, "ABC", true, 68, 3.3, new ZDateTime(2009, 2, 14), pk1, "CODE2", (ZByte)23, "9c27e792-b3da-4505-9725-4f11c02f07dd", ZDateTimeOffset.Invalid, ZGeography.Invalid, ZTime.Invalid);
			child2.Z0_BitFiltered = true;
			DummyChildBusinessObject child3 = CreateChild(collection, 3, "DEF", true, 68, 3.3, new ZDateTime(2009, 2, 14), pk1, "CODE3", (ZByte)23, "NAME1", new ZDateTimeOffset(2003, 5, 5, 8, 9, 10, TimeSpan.FromHours(5)), new ZGeography("POINT (-122 48)"), new ZTime(12));
			child3.Z0_BitFiltered = false;
			DummyChildBusinessObject child4 = CreateChild(collection, 4, "DEF", true, 68, 3.3, new ZDateTime(2009, 2, 14), pk1, "CODE4", (ZByte)23, "NAME2", new ZDateTimeOffset(2004, 5, 5, 8, 9, 10, TimeSpan.Zero), new ZGeography("POINT (-121 49.4)"), new ZTime(13, 1));
			child4.Z0_BitFiltered = false;

			MassUpdateWizard wizard = new MassUpdateWizard(Helper.CollectionInfo);

			wizard.Field = "NText (" + DummyBizoSchema.Constants.Z0_NVarCharMax + ")";
			wizard.NewZGuidValue = new ZGuid("96374db6-b38e-4021-8516-bc8bdb1ee522");
			wizard.LoadBizObjsToUpdate();
			AssertEquals("96374db6-b38e-4021-8516-bc8bdb1ee522", wizard.BizObjsToUpdate[0][DummyBizoSchema.Constants.Z0_NVarCharMax]);
			AssertEquals("96374db6-b38e-4021-8516-bc8bdb1ee522", wizard.BizObjsToUpdate[1][DummyBizoSchema.Constants.Z0_NVarCharMax]);
			AssertEquals("NAME1", wizard.BizObjsToUpdate[2][DummyBizoSchema.Constants.Z0_NVarCharMax]);
			AssertEquals("NAME2", wizard.BizObjsToUpdate[3][DummyBizoSchema.Constants.Z0_NVarCharMax]);
		}

		public void TestNullAddNew()
		{
			var importCollectionInfoMock = new Mock<IImportCollectionInfo>();
			importCollectionInfoMock.Setup(m => m.Collection)
				.Returns(new DummyCollection(Factory));
			var wizard = new MassUpdateWizard(importCollectionInfoMock.Object);
			AssertEquals(0, wizard.FieldList.Count);
			importCollectionInfoMock.VerifyAll();
		}

		public void TestUpdate_WithPropertyValuesLongerThanMaxLength_ShouldTruncateValues()
		{
			var collection = (DummyChildBusinessObjectCollection)Helper.CollectionInfo.Collection;
			collection.RemoveAndDeleteAll();
			var child1 = CreateChild(collection, 1, "ABC", false, 68, 3.3, new ZDateTime(2009, 2, 14), ZGuid.NewZGuid(), "CODE1", (ZByte)23, "HELLO", ZDateTimeOffset.Empty, ZGeography.Empty, ZTime.Empty);
			var wizard = new MassUpdateWizard(Helper.CollectionInfo);
			wizard.NewZStringValue = "1234567890";
			wizard.Field = "FK_Code (" + DummyBizoSchema.Constants.Z0_FK_Code + ")";

			var filter1 = wizard.MatchingFilters.AddNew();
			filter1.Field = "FK_Code (" + DummyBizoSchema.Constants.Z0_FK_Code + ")";
			filter1.FieldValue = "CODE1";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			wizard.ClearAllNotifications();

			wizard.Update();

			AssertEquals("1 record(s) matched; 1 were updated; 0 were 'Read Only'.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Should have truncated value to fit in field", child1.Z0_FK_Code, "12345");
		}

		public void TestMatchingFilterOperatorList()
		{
			var expectedDefaultOperators = new[]
			{
				MassUpdateMatchingFilterOperatorList.Codes.Equal,
				MassUpdateMatchingFilterOperatorList.Codes.NotEqualTo
			};
			var expectedNumericOperators = new[]
			{
				MassUpdateMatchingFilterOperatorList.Codes.Equal,
				MassUpdateMatchingFilterOperatorList.Codes.NotEqualTo,
				MassUpdateMatchingFilterOperatorList.Codes.GreaterThan,
				MassUpdateMatchingFilterOperatorList.Codes.LessThan
			};
			var expectedStringOperators = new[]
			{
				MassUpdateMatchingFilterOperatorList.Codes.Equal,
				MassUpdateMatchingFilterOperatorList.Codes.NotEqualTo,
				MassUpdateMatchingFilterOperatorList.Codes.Contains
			};

			var wizard = new MassUpdateWizard(Helper.CollectionInfo);
			var filter1 = wizard.MatchingFilters.AddNew();
			filter1.Field = "Text (" + DummyBizoSchema.Constants.Z0_VarCharMax + ")";
			AssertContainsExactElementsInExactOrder(expectedStringOperators, filter1.OperatorList.GetAllCodes());

			var filter2 = wizard.MatchingFilters.AddNew();
			filter2.Field = "Number (" + DummyBizoSchema.Constants.Z0_Number + ")";
			AssertContainsExactElementsInExactOrder(expectedNumericOperators, filter2.OperatorList.GetAllCodes());

			var filter3 = wizard.MatchingFilters.AddNew();
			filter3.Field = "Bool (" + DummyBizoSchema.Constants.Z0_Bool + ")";
			AssertContainsExactElementsInExactOrder(expectedDefaultOperators, filter3.OperatorList.GetAllCodes());

			var filter4 = wizard.MatchingFilters.AddNew();
			filter4.Field = "Short (" + DummyBizoSchema.Constants.Z0_Short + ")";
			AssertContainsExactElementsInExactOrder(expectedNumericOperators, filter4.OperatorList.GetAllCodes());

			var filter5 = wizard.MatchingFilters.AddNew();
			filter5.Field = "Decimal (" + DummyBizoSchema.Constants.Z0_AnotherDecimal + ")";
			AssertContainsExactElementsInExactOrder(expectedNumericOperators, filter5.OperatorList.GetAllCodes());

			var filter6 = wizard.MatchingFilters.AddNew();
			filter6.Field = "Date (" + DummyBizoSchema.Constants.Z0_Date + ")";
			AssertContainsExactElementsInExactOrder(expectedDefaultOperators, filter6.OperatorList.GetAllCodes());

			var filter7 = wizard.MatchingFilters.AddNew();
			filter7.Field = "Guid (" + DummyBizoSchema.Constants.Z0_Guid + ")";
			AssertContainsExactElementsInExactOrder(expectedDefaultOperators, filter7.OperatorList.GetAllCodes());

			var filter8 = wizard.MatchingFilters.AddNew();
			filter8.Field = "Byte (" + DummyBizoSchema.Constants.Z0_Byte + ")";
			AssertContainsExactElementsInExactOrder(expectedNumericOperators, filter8.OperatorList.GetAllCodes());

			var filter9 = wizard.MatchingFilters.AddNew();
			filter9.Field = "NText (" + DummyBizoSchema.Constants.Z0_NVarCharMax + ")";
			AssertContainsExactElementsInExactOrder(expectedStringOperators, filter9.OperatorList.GetAllCodes());

			var filter10 = wizard.MatchingFilters.AddNew();
			filter10.Field = "Date (With Offset) (" + DummyBizoSchema.Constants.Z0_DateTimeOffset + ")";
			AssertContainsExactElementsInExactOrder(expectedDefaultOperators, filter10.OperatorList.GetAllCodes());

			var filter11 = wizard.MatchingFilters.AddNew();
			filter11.Field = "Geography (" + DummyBizoSchema.Constants.Z0_Geography + ")";
			AssertContainsExactElementsInExactOrder(expectedDefaultOperators, filter11.OperatorList.GetAllCodes());
		}

		public void TestMatchingFilterForStringContains()
		{
			var collection = (DummyChildBusinessObjectCollection)Helper.CollectionInfo.Collection;
			collection.RemoveAndDeleteAll();
			var child1 = CreateChild(collection, 1, "Pure Water (BEST)", false, 0, 0, ZDateTime.Empty, ZGuid.Empty, "CODE1", ZByte.Zero, ZString.Empty, ZDateTimeOffset.Empty, ZGeography.Empty, ZTime.Empty);
			var child2 = CreateChild(collection, 2, "Flour (BEST)", false, 0, 0, ZDateTime.Empty, ZGuid.Empty, "CODE2", ZByte.Zero, ZString.Empty, ZDateTimeOffset.Empty, ZGeography.Empty, ZTime.Empty);

			var wizard = new MassUpdateWizard(Helper.CollectionInfo);
			var filter = wizard.MatchingFilters.AddNew();
			filter.Field = "Text (" + DummyBizoSchema.Constants.Z0_VarCharMax + ")";
			filter.Operator = MassUpdateMatchingFilterOperatorList.Codes.Contains;

			filter.FieldValue = "WATER";
			wizard.LoadBizObjsToUpdate();
			AssertEquals(1, wizard.BizObjsToUpdate.Count);
			AssertEquals(child1.Z0_FK_Code, wizard.BizObjsToUpdate[0][DummyBizoSchema.Constants.Z0_FK_Code]);

			filter.FieldValue = "water";
			wizard.LoadBizObjsToUpdate();
			AssertEquals(1, wizard.BizObjsToUpdate.Count);
			AssertEquals(child1.Z0_FK_Code, wizard.BizObjsToUpdate[0][DummyBizoSchema.Constants.Z0_FK_Code]);

			filter.FieldValue = "our";
			wizard.LoadBizObjsToUpdate();
			AssertEquals(1, wizard.BizObjsToUpdate.Count);
			AssertEquals(child2.Z0_FK_Code, wizard.BizObjsToUpdate[0][DummyBizoSchema.Constants.Z0_FK_Code]);

			filter.FieldValue = "best";
			wizard.LoadBizObjsToUpdate();
			AssertEquals(2, wizard.BizObjsToUpdate.Count);
			AssertEquals(child1.Z0_FK_Code, wizard.BizObjsToUpdate[0][DummyBizoSchema.Constants.Z0_FK_Code]);
			AssertEquals(child2.Z0_FK_Code, wizard.BizObjsToUpdate[1][DummyBizoSchema.Constants.Z0_FK_Code]);

			filter.FieldValue = "ABC";
			wizard.LoadBizObjsToUpdate();
			AssertEquals(0, wizard.BizObjsToUpdate.Count);
		}

		public void TestMatchingFilterForNumericComparison()
		{
			var collection = (DummyChildBusinessObjectCollection)Helper.CollectionInfo.Collection;
			collection.RemoveAndDeleteAll();
			var child1 = CreateChild(collection, 10, "Item 1", false, 40, 123.4, ZDateTime.Empty, ZGuid.Empty, "CODE1", ZByte.Zero, ZString.Empty, ZDateTimeOffset.Empty, ZGeography.Empty, ZTime.Empty);
			var child2 = CreateChild(collection, 20, "Item 2", false, 30, 234.5, ZDateTime.Empty, ZGuid.Empty, "CODE2", ZByte.Zero, ZString.Empty, ZDateTimeOffset.Empty, ZGeography.Empty, ZTime.Empty);

			var wizard = new MassUpdateWizard(Helper.CollectionInfo);
			var filter = wizard.MatchingFilters.AddNew();
			filter.Field = "Number (" + DummyBizoSchema.Constants.Z0_Number + ")";
			filter.FieldValue = "15";

			filter.Operator = MassUpdateMatchingFilterOperatorList.Codes.LessThan;
			wizard.LoadBizObjsToUpdate();
			AssertEquals(1, wizard.BizObjsToUpdate.Count);
			AssertEquals(child1.Z0_FK_Code, wizard.BizObjsToUpdate[0][DummyBizoSchema.Constants.Z0_FK_Code]);

			filter.Operator = MassUpdateMatchingFilterOperatorList.Codes.GreaterThan;
			wizard.LoadBizObjsToUpdate();
			AssertEquals(1, wizard.BizObjsToUpdate.Count);
			AssertEquals(child2.Z0_FK_Code, wizard.BizObjsToUpdate[0][DummyBizoSchema.Constants.Z0_FK_Code]);

			filter.Field = "Short (" + DummyBizoSchema.Constants.Z0_Short + ")";
			filter.FieldValue = "35";

			filter.Operator = MassUpdateMatchingFilterOperatorList.Codes.LessThan;
			wizard.LoadBizObjsToUpdate();
			AssertEquals(1, wizard.BizObjsToUpdate.Count);
			AssertEquals(child2.Z0_FK_Code, wizard.BizObjsToUpdate[0][DummyBizoSchema.Constants.Z0_FK_Code]);

			filter.Operator = MassUpdateMatchingFilterOperatorList.Codes.GreaterThan;
			wizard.LoadBizObjsToUpdate();
			AssertEquals(1, wizard.BizObjsToUpdate.Count);
			AssertEquals(child1.Z0_FK_Code, wizard.BizObjsToUpdate[0][DummyBizoSchema.Constants.Z0_FK_Code]);

			filter.Field = "Decimal (" + DummyBizoSchema.Constants.Z0_AnotherDecimal + ")";
			filter.FieldValue = "156.7";

			filter.Operator = MassUpdateMatchingFilterOperatorList.Codes.LessThan;
			wizard.LoadBizObjsToUpdate();
			AssertEquals(1, wizard.BizObjsToUpdate.Count);
			AssertEquals(child1.Z0_FK_Code, wizard.BizObjsToUpdate[0][DummyBizoSchema.Constants.Z0_FK_Code]);

			filter.Operator = MassUpdateMatchingFilterOperatorList.Codes.GreaterThan;
			wizard.LoadBizObjsToUpdate();
			AssertEquals(1, wizard.BizObjsToUpdate.Count);
			AssertEquals(child2.Z0_FK_Code, wizard.BizObjsToUpdate[0][DummyBizoSchema.Constants.Z0_FK_Code]);
		}

		class DummyCollection : IBusinessObjectCollection, ICancelAddNew
		{
			public DummyCollection(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			public IDisposable SuspendListChanged()
			{
				return DisposableAction.NoAction;
			}

			public IDisposable SuspendAdditionallyForImport()
			{
				return DisposableAction.NoAction;
			}

			#region IBusinessObjectCollection Members

			public void AddGuidListMapping(string propertyName, string listName)
			{
				throw new NotImplementedException();
			}

			public void AddIsTime(string propertyName)
			{
				throw new NotImplementedException();
			}

			public BusinessObject AddNew()
			{
				return null;
			}

			public void AddRange(IEnumerable businessObjects)
			{
				throw new NotImplementedException();
			}

			public void ApplySort(SortInfo sort)
			{
				throw new NotImplementedException();
			}

			public ZQuery CompleteFilter
			{
				get { throw new NotImplementedException(); }
			}

			public ZQuery RelationshipFilter
			{
				get { throw new NotImplementedException(); }
			}

			public bool Contains(ZGuid pk)
			{
				throw new NotImplementedException();
			}

			public bool Contains(BusinessObject businessObject)
			{
				throw new NotImplementedException();
			}

			public void Delete(BusinessObject businessObject)
			{
				throw new NotImplementedException();
			}

			public ISortable Elements
			{
				get { throw new NotImplementedException(); }
			}

			public IBusinessObjectCollectionFetchStrategy FetchStrategy
			{
				get { throw new NotImplementedException(); }
			}

			public BusinessObject[] Find(ZQuery filter)
			{
				throw new NotImplementedException();
			}

			public BusinessObject FindByPK(ZGuid pk)
			{
				throw new NotImplementedException();
			}

			public IComparer GetComparerForSort(PropertyDescriptor property, ListSortDirection direction)
			{
				throw new NotImplementedException();
			}

			public Type GetTypeOfElementsFromPK(ZGuid pk)
			{
				throw new NotImplementedException();
			}

			public int IndexOf(IBusiness bizObj, int startIndex, int countToSearchFromStartIndex)
			{
				throw new NotImplementedException();
			}

			public bool IsLoaded
			{
				get { throw new NotImplementedException(); }
			}

			public bool ReadOnly
			{
				get { throw new NotImplementedException(); }
			}

			public void Remove(BusinessObject businessObject)
			{
				throw new NotImplementedException();
			}

			public void RemoveFromRelationship(BusinessObject businessObject)
			{
				throw new NotImplementedException();
			}

			public event EventHandler SortChanged
			{
				remove { }
				add { }
			}

			public SortInfo SortInformation
			{
				get { throw new NotImplementedException(); }
			}

			public BusinessObject[] ToArray()
			{
				throw new NotImplementedException();
			}

			public Type TypeOfElements
			{
				get { throw new NotImplementedException(); }
			}

			PropertyDescriptor IBusinessObjectCollection.ListPropertyDescriptor { get; set; }
			object IBusinessObjectCollection.Parent { get; set; }

			#endregion

			#region IBusiness Members

			public bool CanContinueWithSave
			{
				get { throw new NotImplementedException(); }
			}

			public IBusiness[] Children
			{
				get { throw new NotImplementedException(); }
			}

			public void Delete()
			{
				throw new NotImplementedException();
			}

			bool IBusiness.CanDeleteForDataRefresh => throw new NotImplementedException();
			void IBusiness.DeleteForDataRefresh() => throw new NotImplementedException();

			public BusinessObjectFactory Factory
			{
				get { return factory; }
			}
			readonly BusinessObjectFactory factory;

			public ZString HumanReadableName
			{
				get { throw new NotImplementedException(); }
			}

			public bool IsValidationSuspended
			{
				get { throw new NotImplementedException(); }
			}

			public void MarkAsNeedingValidationIncludingChildren()
			{
				throw new NotImplementedException();
			}

			public void NotifyRegisteredChildEditable()
			{
				throw new NotImplementedException();
			}

			public void ResumeValidation()
			{
				throw new NotImplementedException();
			}

			public void RunPreSaveValidation()
			{
				throw new NotImplementedException();
			}

			public void RunPreSaveValidationFetch(bool executeHints)
			{
				throw new NotImplementedException();
			}

			public void SuspendValidation()
			{
				throw new NotImplementedException();
			}

			bool IBusiness.IgnoreValidationSuspended
			{
				get
				{
					throw new NotImplementedException();
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			public string TableName
			{
				get { throw new NotImplementedException(); }
			}

			public void ValidateIfQuickAndImprovesPreSaveValidationPerformance()
			{
				throw new NotImplementedException();
			}

			#endregion

			#region IBusinessObjectState Members

			public void ClearHasChangesIncludingChildren()
			{
				throw new NotImplementedException();
			}

			public void DecrementReadOnlyIncludingChildren(bool decrementToZero)
			{
				throw new NotImplementedException();
			}

			public bool HasChanges
			{
				get
				{
					throw new NotImplementedException();
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			public event EventHandler<HasChangesChangedEventArgs> HasChangesChanged
			{
				remove { }
				add { }
			}

			public bool HasChangesNotIncludingChildren
			{
				get { throw new NotImplementedException(); }
			}

			public void IncrementReadOnlyIncludingChildren()
			{
				throw new NotImplementedException();
			}

			public bool IsInDatabase => throw new NotImplementedException();

			public bool IsInDatabaseIncludingChildren
			{
				get { throw new NotImplementedException(); }
			}

			public uint LastChangeNumber
			{
				get { throw new NotImplementedException(); }
			}

			public event EventHandler<NotificationsChangedEventArgs> NotificationsChanged
			{
				remove { }
				add { }
			}

			public void RefreshBindingIncludingChildren()
			{
				throw new NotImplementedException();
			}

			public event EventHandler UpdatedByDataRefreshIncludingChildren
			{
				remove { }
				add { }
			}

			#endregion

			#region IBindingList Members

			public void AddIndex(PropertyDescriptor property)
			{
				throw new NotImplementedException();
			}

			object IBindingList.AddNew()
			{
				return null;
			}

			public bool AllowEdit
			{
				get { throw new NotImplementedException(); }
			}

			public bool AllowNew
			{
				get { throw new NotImplementedException(); }
			}

			public bool AllowRemove
			{
				get { throw new NotImplementedException(); }
			}

			public void ApplySort(PropertyDescriptor property, ListSortDirection direction)
			{
				throw new NotImplementedException();
			}

			public int Find(PropertyDescriptor property, object key)
			{
				throw new NotImplementedException();
			}

			public bool IsSorted
			{
				get { throw new NotImplementedException(); }
			}

			public event ListChangedEventHandler ListChanged
			{
				remove { }
				add { }
			}

			public void RemoveIndex(PropertyDescriptor property)
			{
				throw new NotImplementedException();
			}

			public void RemoveSort()
			{
				throw new NotImplementedException();
			}

			public ListSortDirection SortDirection
			{
				get { throw new NotImplementedException(); }
			}

			public PropertyDescriptor SortProperty
			{
				get { throw new NotImplementedException(); }
			}

			public bool SupportsChangeNotification
			{
				get { throw new NotImplementedException(); }
			}

			public bool SupportsSearching
			{
				get { throw new NotImplementedException(); }
			}

			public bool SupportsSorting
			{
				get { throw new NotImplementedException(); }
			}

			#endregion

			#region IList Members

			public int Add(object value)
			{
				throw new NotImplementedException();
			}

			public void Clear()
			{
				throw new NotImplementedException();
			}

			public bool Contains(object value)
			{
				throw new NotImplementedException();
			}

			public int IndexOf(object value)
			{
				throw new NotImplementedException();
			}

			public void Insert(int index, object value)
			{
				throw new NotImplementedException();
			}

			public bool IsFixedSize
			{
				get { throw new NotImplementedException(); }
			}

			public bool IsReadOnly
			{
				get { throw new NotImplementedException(); }
			}

			public void Remove(object value)
			{
				throw new NotImplementedException();
			}

			public void RemoveAt(int index)
			{
				throw new NotImplementedException();
			}

			public object this[int index]
			{
				get
				{
					throw new NotImplementedException();
				}
				set
				{
					throw new NotImplementedException();
				}
			}

			#endregion

			#region ICollection Members

			public void CopyTo(Array array, int index)
			{
				throw new NotImplementedException();
			}

			public int Count
			{
				get { return 0; }
			}

			public bool IsSynchronized
			{
				get { throw new NotImplementedException(); }
			}

			public object SyncRoot
			{
				get { throw new NotImplementedException(); }
			}

			#endregion

			#region IEnumerable Members

			public IEnumerator GetEnumerator()
			{
				throw new NotImplementedException();
			}

			#endregion

			#region INotificationProvider Members

			public INotificationType GetHighestSeverityNotificationType()
			{
				throw new NotImplementedException();
			}

			public bool HasNotifications(INotificationType type)
			{
				throw new NotImplementedException();
			}

			public bool HasNotifications()
			{
				throw new NotImplementedException();
			}

			public IEnumerable<INotification> Notifications
			{
				get { throw new NotImplementedException(); }
			}

			#endregion

			#region IIdentified Members

			public ZGuid Identifier
			{
				get { throw new NotImplementedException(); }
			}

			#endregion

			#region ISortable Members

			public void ApplySort(IComparer comparer)
			{
				throw new NotImplementedException();
			}

			#endregion

			#region ICancelAddNew Members

			public void CancelNew(int itemIndex)
			{
			}

			public void EndNew(int itemIndex)
			{
			}

			#endregion
		}

		IBusinessObjectCollection CurrencyList
		{
			get { return currencyList ?? (currencyList = (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<MasterFiles.Integration.IRefCurrencyCollection>(), new object[] { Factory })); }
		}
		IBusinessObjectCollection currencyList;

		CodeDescriptionPairList PairList
		{
			get
			{
				if (pairList == null)
				{
					pairList = new CodeDescriptionPairList();
					pairList.AddPair("ABC", "ABC Noodle");
					pairList.AddPair("DEF", "DEF Spaghetti");
					pairList.AddPair("GHI", "GHI Rice");
				}
				return pairList;
			}
		}
		CodeDescriptionPairList pairList;

		protected override BusinessObject GetNewBusinessObject()
		{
			return new MassUpdateWizard(Helper.CollectionInfo);
		}

		MassUpdateWizardTestHelper Helper
		{
			get { return helper ?? (helper = new MassUpdateWizardTestHelper(Factory)); }
		}
		MassUpdateWizardTestHelper helper;
	}
}
