namespace Enterprise.DocumentEngine.SDF.Testing
{
	sealed class StmSystemDefinedFieldColumnLookupsTest : StmSystemDefinedFieldLookupsTest
	{
		public override void TestTypes()
		{
			AssertEquals("Count", 7, Parent.Lookups.Types.Count);
			AssertEquals("GetDescriptionFromCode(\"MUL\")", "Multi-Line Text", Parent.Lookups.Types.GetDescriptionFromCode("MUL"));
			AssertEquals("GetDescriptionFromCode(\"TXT\")", "Text", Parent.Lookups.Types.GetDescriptionFromCode("TXT"));
			AssertEquals("GetDescriptionFromCode(\"DTM\")", "Date Time", Parent.Lookups.Types.GetDescriptionFromCode("DTM"));
			AssertEquals("GetDescriptionFromCode(\"DAT\")", "Date Only", Parent.Lookups.Types.GetDescriptionFromCode("DAT"));
			AssertEquals("GetDescriptionFromCode(\"DEC\")", "Decimal", Parent.Lookups.Types.GetDescriptionFromCode("DEC"));
			AssertEquals("GetDescriptionFromCode(\"INT\")", "Integer", Parent.Lookups.Types.GetDescriptionFromCode("INT"));
			AssertEquals("GetDescriptionFromCode(\"BLN\")", "Boolean", Parent.Lookups.Types.GetDescriptionFromCode("BLN"));
		}

		public override void TestDefaults()
		{
			AssertEquals("Count", 0, Parent.Lookups.Defaults.Count);

			DummyBusinessObjectDocumentSupportable documentSupportable = Factory.New<DummyBusinessObjectDocumentSupportable>();
			StmSystemDefinedFieldDependentCollection collection = new StmSystemDefinedFieldDependentCollection(documentSupportable, Factory);
			StmSystemDefinedField field = collection.AddNew();

			AssertDefaultsList(field.FieldColumns.AddNew());
		}

		#region Implementation

		protected override AutoStmSystemDefinedField GetNewParent()
		{
			return Factory.New<StmSystemDefinedFieldColumn>();
		}

		#endregion
	}
}
