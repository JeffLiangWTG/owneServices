using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class DispatchInstructionNumberLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAdditionalReferenceNumberTypes()
		{
			var number = Factory.New<DispatchInstructionNumber>();
			var list = number.Lookups.AdditionalReferenceNumberTypes;
			AssertType<DispatchInstructionDocumentTypes>(list);
			AssertSame(list, number.Lookups.AdditionalReferenceNumberTypes);
		}
	}
}
