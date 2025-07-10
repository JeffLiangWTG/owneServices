using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(SpecialBusinessIdentifier))]
	class SpecialBusinessIdentifierTesting : Customs.Business.Testing.CusCodeDataTest<SpecialBusinessIdentifier>
	{
		public void TestSupportsNotes()
		{
			AssertEquals("SupportsNotes should be false", false, ((SpecialBusinessIdentifier)BusinessObject).SupportsNotes);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			return declaration.CustomsEntryInstructions.AddNew().SpecialBusinessIdentifiers.AddNew();
		}
	}
}
