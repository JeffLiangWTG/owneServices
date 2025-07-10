using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.DocumentEngine.SDF.Testing
{
	class StmSystemDefinedFieldLookupsTest : BusinessObjectLookupsTestCase
	{
		public virtual void TestTypes()
		{
			AssertEquals("Count", 8, Parent.Lookups.Types.Count);
			AssertEquals("GetDescriptionFromCode(\"MUL\")", "Multi-Line Text", Parent.Lookups.Types.GetDescriptionFromCode("MUL"));
			AssertEquals("GetDescriptionFromCode(\"TXT\")", "Text", Parent.Lookups.Types.GetDescriptionFromCode("TXT"));
			AssertEquals("GetDescriptionFromCode(\"DTM\")", "Date Time", Parent.Lookups.Types.GetDescriptionFromCode("DTM"));
			AssertEquals("GetDescriptionFromCode(\"DAT\")", "Date Only", Parent.Lookups.Types.GetDescriptionFromCode("DAT"));
			AssertEquals("GetDescriptionFromCode(\"DEC\")", "Decimal", Parent.Lookups.Types.GetDescriptionFromCode("DEC"));
			AssertEquals("GetDescriptionFromCode(\"INT\")", "Integer", Parent.Lookups.Types.GetDescriptionFromCode("INT"));
			AssertEquals("GetDescriptionFromCode(\"GRD\")", "Grid", Parent.Lookups.Types.GetDescriptionFromCode("GRD"));
			AssertEquals("GetDescriptionFromCode(\"BLN\")", "Boolean", Parent.Lookups.Types.GetDescriptionFromCode("BLN"));
		}

		public void TestDisplayEditRules()
		{
			AssertEquals("Count", 2, Parent.Lookups.DisplayEditRules.Count);
			AssertEquals("GetDescriptionFromCode(\"ING\")", "In Grid", Parent.Lookups.DisplayEditRules.GetDescriptionFromCode("ING"));
			AssertEquals("GetDescriptionFromCode(\"POP\")", "Popup Scroll Box", Parent.Lookups.DisplayEditRules.GetDescriptionFromCode("POP"));
		}

		public void TestValidations()
		{
			AssertEquals("Count", 3, Parent.Lookups.Validations.Count);
			AssertEquals("GetDescriptionFromCode(\"NOV\")", "No Validation", Parent.Lookups.Validations.GetDescriptionFromCode("NOV"));
			AssertEquals("GetDescriptionFromCode(\"VAL\")", "Value Required", Parent.Lookups.Validations.GetDescriptionFromCode("VAL"));
			AssertEquals("GetDescriptionFromCode(\"RVL\")", "Range Value Required", Parent.Lookups.Validations.GetDescriptionFromCode("RVL"));
		}

		#region Defaults

		public virtual void TestDefaults()
		{
			AssertEquals("Count", 0, Parent.Lookups.Defaults.Count);

			DummyBusinessObjectDocumentSupportable documentSupportable = Factory.New<DummyBusinessObjectDocumentSupportable>();
			StmSystemDefinedFieldDependentCollection collection = new StmSystemDefinedFieldDependentCollection(documentSupportable, Factory);
			StmSystemDefinedField field = collection.AddNew();

			AssertDefaultsList(field);
		}

		internal void AssertDefaultsList(AutoStmSystemDefinedField field)
		{
			AssertEquals("Count", 49, field.Lookups.Defaults.Count);

			AssertCodeAndDescription(field.Lookups.Defaults[0], "Z0_SparseVarBinaryMax", "ZBlob");
			AssertCodeAndDescription(field.Lookups.Defaults[1], "Z0_VarBinaryMax", "ZBlob");
			AssertCodeAndDescription(field.Lookups.Defaults[2], "Z0_BitFalse", "ZBool");
			AssertCodeAndDescription(field.Lookups.Defaults[3], "Z0_BitFiltered", "ZBool");
			AssertCodeAndDescription(field.Lookups.Defaults[4], "Z0_BitTrue", "ZBool");
			AssertCodeAndDescription(field.Lookups.Defaults[5], "Z0_Bool", "ZBool");
			AssertCodeAndDescription(field.Lookups.Defaults[6], "Z0_IsSystem", "ZBool");
			AssertCodeAndDescription(field.Lookups.Defaults[7], "Z0_SparseBit", "ZBool");
			AssertCodeAndDescription(field.Lookups.Defaults[8], "Z0_Byte", "ZByte");
			AssertCodeAndDescription(field.Lookups.Defaults[9], "Z0_SparseByte", "ZByte");
			AssertCodeAndDescription(field.Lookups.Defaults[10], "Z0_DateOnly", "ZDate");
			AssertCodeAndDescription(field.Lookups.Defaults[11], "Z0_SparseDate", "ZDate");
			AssertCodeAndDescription(field.Lookups.Defaults[12], "Z0_AnotherDate", "ZDateTime");
			AssertCodeAndDescription(field.Lookups.Defaults[13], "Z0_Date", "ZDateTime");
			AssertCodeAndDescription(field.Lookups.Defaults[14], "Z0_SmallDateTime", "ZDateTime");
			AssertCodeAndDescription(field.Lookups.Defaults[15], "Z0_SparseDateTime", "ZDateTime");
			AssertCodeAndDescription(field.Lookups.Defaults[16], "Z0_SparseSmallDateTime", "ZDateTime");
			AssertCodeAndDescription(field.Lookups.Defaults[17], "Z0_DateTimeOffset", "ZDateTimeOffset");
			AssertCodeAndDescription(field.Lookups.Defaults[18], "Z0_SparseDateTimeOffset", "ZDateTimeOffset");
			AssertCodeAndDescription(field.Lookups.Defaults[19], "Z0_AnotherDecimal", "ZDecimal");
			AssertCodeAndDescription(field.Lookups.Defaults[20], "Z0_Decimal", "ZDecimal");
			AssertCodeAndDescription(field.Lookups.Defaults[21], "Z0_Money", "ZDecimal");
			AssertCodeAndDescription(field.Lookups.Defaults[22], "Z0_SparseDecimal", "ZDecimal");
			AssertCodeAndDescription(field.Lookups.Defaults[23], "Z0_SparseMoney", "ZDecimal");
			AssertCodeAndDescription(field.Lookups.Defaults[24], "Z0_Geography", "ZGeography");
			AssertCodeAndDescription(field.Lookups.Defaults[25], "Z0_Guid", "ZGuid");
			AssertCodeAndDescription(field.Lookups.Defaults[26], "Z0_SparseGuid", "ZGuid");
			AssertCodeAndDescription(field.Lookups.Defaults[27], "Z0_AnotherNumber", "ZInt");
			AssertCodeAndDescription(field.Lookups.Defaults[28], "Z0_Number", "ZInt");
			AssertCodeAndDescription(field.Lookups.Defaults[29], "Z0_SparseNumber", "ZInt");
			AssertCodeAndDescription(field.Lookups.Defaults[30], "Z0_Long", "ZLong");
			AssertCodeAndDescription(field.Lookups.Defaults[31], "Z0_SparseLong", "ZLong");
			AssertCodeAndDescription(field.Lookups.Defaults[32], "Z0_Short", "ZShort");
			AssertCodeAndDescription(field.Lookups.Defaults[33], "Z0_SparseShort", "ZShort");
			AssertCodeAndDescription(field.Lookups.Defaults[34], "Z0_AddInfo", "ZString");
			AssertCodeAndDescription(field.Lookups.Defaults[35], "Z0_Code", "ZString");
			AssertCodeAndDescription(field.Lookups.Defaults[36], "Z0_Description", "ZString");
			AssertCodeAndDescription(field.Lookups.Defaults[37], "Z0_FK_Code", "ZString");
			AssertCodeAndDescription(field.Lookups.Defaults[38], "Z0_NAddInfo", "ZString");
			AssertCodeAndDescription(field.Lookups.Defaults[39], "Z0_NVarChar", "ZString");
			AssertCodeAndDescription(field.Lookups.Defaults[40], "Z0_NVarCharMax", "ZString");
			AssertCodeAndDescription(field.Lookups.Defaults[41], "Z0_SparseChar", "ZString");
			AssertCodeAndDescription(field.Lookups.Defaults[42], "Z0_SparseNVarChar", "ZString");
			AssertCodeAndDescription(field.Lookups.Defaults[43], "Z0_SparseVarChar", "ZString");
			AssertCodeAndDescription(field.Lookups.Defaults[44], "Z0_SparseXml", "ZString");
			AssertCodeAndDescription(field.Lookups.Defaults[45], "Z0_VarCharMax", "ZString");
			AssertCodeAndDescription(field.Lookups.Defaults[46], "Z0_Xml", "ZString");
			AssertCodeAndDescription(field.Lookups.Defaults[47], "Z0_SparseTime", "ZTime");
			AssertCodeAndDescription(field.Lookups.Defaults[48], "Z0_Time", "ZTime");
		}

		void AssertCodeAndDescription(ICodeDescription codeDescription, string expectedCode, string expectedDescription)
		{
			AssertEquals("Code", expectedCode, codeDescription.Code);
			AssertEquals("Description", expectedDescription, codeDescription.Description);
		}

		#region class DummyBusinessObjectDocumentSupportable

		protected class DummyBusinessObjectDocumentSupportable : DummyBusinessObject, IDocumentSupportable
		{
			public DummyBusinessObjectDocumentSupportable(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IDocumentSupportable Members

			public DocumentSupporter DocumentSupporter
			{
				get { return new MockDocSupportBizODocumentSupporter(this); }
			}

			#endregion
		}

		#endregion

		#endregion

		#region Implementation

		internal AutoStmSystemDefinedField Parent
		{
			get
			{
				if (fParent == null)
				{
					fParent = GetNewParent();
				}
				return fParent;
			}
		}

		protected virtual AutoStmSystemDefinedField GetNewParent()
		{
			return Factory.New<StmSystemDefinedField>();
		}

		AutoStmSystemDefinedField fParent;

		#endregion
	}
}
