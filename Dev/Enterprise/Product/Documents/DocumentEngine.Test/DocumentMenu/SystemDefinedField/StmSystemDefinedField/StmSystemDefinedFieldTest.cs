using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.SDF.Testing
{
	[TestedType(typeof(StmSystemDefinedField))]
	sealed class StmSystemDefinedFieldTest : StmSystemDefinedFieldBaseTestCase
	{
		public void TestFieldColumns()
		{
			Field.S1_BusinessContext = "Context 1";
			Field.S1_Order = 80;

			StmSystemDefinedFieldColumn fieldColumn1 = Factory.New<StmSystemDefinedFieldColumn>();
			StmSystemDefinedFieldColumn fieldColumn2 = Factory.New<StmSystemDefinedFieldColumn>();
			StmSystemDefinedFieldColumn fieldColumn3 = Factory.New<StmSystemDefinedFieldColumn>();

			fieldColumn1.S1_BusinessContext = "Context 1";
			fieldColumn2.S1_BusinessContext = "Context 1";
			fieldColumn3.S1_BusinessContext = "Context 2";

			fieldColumn1.S1_Order = 80;
			fieldColumn2.S1_Order = 40;
			fieldColumn3.S1_Order = 20;

			fieldColumn1.S1_OrderColumn = 1;
			fieldColumn2.S1_OrderColumn = 1;
			fieldColumn3.S1_OrderColumn = 1;

			AssertEquals("FieldColumns.Count", 1, Field.FieldColumns.Count);
			AssertEquals("FieldColumns.Contains(FieldColumn1)", true, Field.FieldColumns.Contains(fieldColumn1));

			AssertEquals("Fields.SortInformation.PropertyName", StmSystemDefinedFieldSchema.Constants.S1_OrderColumn, Field.FieldColumns.SortInformation.PropertyName);
			AssertEquals("Fields.SortInformation.Direction", ListSortDirection.Ascending, Field.FieldColumns.SortInformation.Direction);
		}

		public void TestS1_CategoryAndS1_NameTranslatable()
		{
			var bizO = Factory.New<StmSystemDefinedField>();
			bizO.S1_Category = "Boom";
			bizO.S1_Name = "Ding";
			var resKey1 = bizO.S1_CategoryInfo.CustomizableDataResourceStrings.GetMultilingualString(bizO, "Boom").ResourceKey;
			var resKey2 = bizO.S1_NameInfo.CustomizableDataResourceStrings.GetMultilingualString(bizO, "Ding").ResourceKey;
			AssertEquals("Boom", bizO.S1_CategoryMultilingual);
			AssertEquals("Ding", bizO.S1_NameMultilingual);
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.ChineseSimplified))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey1, new ResourceStringData(resKey1, "咚"));
				mockRes.Put(resKey2, new ResourceStringData(resKey2, "叮"));
				AssertEquals("咚", bizO.S1_CategoryMultilingual);
				AssertEquals("叮", bizO.S1_NameMultilingual);
			}
		}

		public void TestFieldCountries()
		{
			Field.S1_BusinessContext = "Chocolate";
			Field.S1_Name = "Coffee";

			StmSystemDefinedFieldCountry fieldCountry1 = Factory.New<StmSystemDefinedFieldCountry>();
			StmSystemDefinedFieldCountry fieldCountry2 = Factory.New<StmSystemDefinedFieldCountry>();

			fieldCountry1.S1_BusinessContext = "Chocolate";
			fieldCountry1.S1_Name = "Coffee";

			fieldCountry2.S1_BusinessContext = "Furry";
			fieldCountry2.S1_Name = "Apples";

			AssertEquals("FieldCountries.Count", 1, Field.FieldCountries.Count);
			AssertEquals("FieldCountries.Contains(FieldCountry1)", true, Field.FieldCountries.Contains(fieldCountry1));
		}

		public void TestFieldColumnsReadOnly()
		{
			StmSystemDefinedField field1 = Factory.New<StmSystemDefinedField>();
			StmSystemDefinedField field2 = Factory.New<StmSystemDefinedField>();

			field1.S1_Type = "GRD";
			field2.S1_Type = "TXT";

			AssertEquals("Field1.FieldColumns.ReadOnly", false, field1.FieldColumns.ReadOnly);
			AssertEquals("Field2.FieldColumns.ReadOnly", true, field2.FieldColumns.ReadOnly);

			field1.S1_Type = "DAT";
			field2.S1_Type = "GRD";
			AssertEquals("Field1.FieldColumns.ReadOnly", true, field1.FieldColumns.ReadOnly);
			AssertEquals("Field2.FieldColumns.ReadOnly", false, field2.FieldColumns.ReadOnly);
		}

		public void TestS1_DefaultInfoReadOnly()
		{
			AssertEquals("S1_DefaultInfo.ReadOnly", false, Field.S1_DefaultInfo.ReadOnly);

			Field.S1_Type = "GRD";
			AssertEquals("S1_DefaultInfo.ReadOnly", true, Field.S1_DefaultInfo.ReadOnly);

			Field.S1_Type = "TXT";
			AssertEquals("S1_DefaultInfo.ReadOnly", false, Field.S1_DefaultInfo.ReadOnly);
		}

		public void TestS1_DefaultIsEmptyIfIsGrid()
		{
			Field.S1_Type = "TXT";
			Field.S1_Default = "DEF";

			Field.S1_Type = "INT";
			AssertEquals("S1_Default", "DEF", Field.S1_Default);

			Field.S1_Type = "GRD";
			AssertEquals("S1_Default", "", Field.S1_Default);
		}

		public void TestDelete()
		{
			StmSystemDefinedFieldColumn fieldColumn1 = Field.FieldColumns.AddNew();
			StmSystemDefinedFieldColumn fieldColumn2 = Field.FieldColumns.AddNew();

			StmSystemDefinedFieldCountry fieldCountry1 = Field.FieldCountries.AddNew();
			StmSystemDefinedFieldCountry fieldCountry2 = Field.FieldCountries.AddNew();

			Factory.Save();
			Field.Delete();
			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			AssertNull("Field should have been deleted.", newFactory.Load(typeof(StmSystemDefinedField), Field.PK));
			AssertNull("FieldColumn1 should have been deleted.", newFactory.Load(typeof(StmSystemDefinedFieldColumn), fieldColumn1.PK));
			AssertNull("FieldColumn2 should have been deleted.", newFactory.Load(typeof(StmSystemDefinedFieldColumn), fieldColumn2.PK));
			AssertNull("FieldCountry1 should have been deleted.", newFactory.Load(typeof(StmSystemDefinedFieldCountry), fieldCountry1.PK));
			AssertNull("FieldCountry2 should have been deleted.", newFactory.Load(typeof(StmSystemDefinedFieldCountry), fieldCountry2.PK));
		}

		public void TestFieldColumnsAreRemovedWhenTypeChanges()
		{
			Field.S1_Type = "GRD";

			StmSystemDefinedFieldColumn fieldColumn1 = Field.FieldColumns.AddNew();
			StmSystemDefinedFieldColumn fieldColumn2 = Field.FieldColumns.AddNew();
			AssertEquals("FieldColumns.Count", 2, Field.FieldColumns.Count);

			Field.S1_Type = "TXT";
			AssertEquals("FieldColumns.Count", 0, Field.FieldColumns.Count);
			AssertEquals("FieldColumn1.IsDeleted", true, fieldColumn1.IsDeleted);
			AssertEquals("FieldColumn1.IsDeleted", true, fieldColumn1.IsDeleted);
		}

		public void TestMultiColumnStyleType()
		{
			Field.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.DateOnly;
			AssertEquals(nameof(FieldType.Date), Field.MultiColumnStyleType);

			Field.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.DateTime;
			AssertEquals(nameof(FieldType.DateTime), Field.MultiColumnStyleType);

			Field.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.Integer;
			AssertEquals(nameof(FieldType.Integer), Field.MultiColumnStyleType);

			Field.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.Decimal;
			AssertEquals(nameof(FieldType.Decimal), Field.MultiColumnStyleType);

			Field.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.MultiLineText;
			AssertEquals(nameof(FieldType.TextMultiLine), Field.MultiColumnStyleType);

			Field.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.Text;
			AssertEquals(nameof(FieldType.Text), Field.MultiColumnStyleType);

			Field.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.Boolean;
			AssertEquals(nameof(FieldType.Boolean), Field.MultiColumnStyleType);
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		protected override Type ExpectedValidationType => typeof(StmSystemDefinedFieldWithOrderAndGridValidation);

		new StmSystemDefinedField Field => (StmSystemDefinedField)base.Field;
	}
}
