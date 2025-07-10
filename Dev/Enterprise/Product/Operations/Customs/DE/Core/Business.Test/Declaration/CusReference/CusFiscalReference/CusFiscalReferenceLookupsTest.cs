using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class CusFiscalReferenceLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCodeList()
		{
			var list = lookups.CodeList;
			CombineAssertions(() =>
			{
				AssertEquals("CodeAsString", "FR1, FR2, FR3, FR5", list.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<FiscalReferenceCodeList>(), list);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var fiscalReference = entryInstruction.FiscalReferences.AddNew();
			lookups = new CusFiscalReferenceLookups(fiscalReference);
		}
		CusFiscalReferenceLookups lookups;
	}
}
