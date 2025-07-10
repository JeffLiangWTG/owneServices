using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class ProcessRelatedNumberLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAdditionalReferenceNumberTypes()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var number = declaration.ProcessRelatedNumbers.AddNew();

			var list = number.Lookups.AdditionalReferenceNumberTypes;
			AssertType<ProcessRelatedTypeList>(list);
			AssertContainsExactElementsInAnyOrder(new string[] { "ADM" }, list.GetAllCodes());
			AssertSame(list, number.Lookups.AdditionalReferenceNumberTypes);

			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			list = number.Lookups.AdditionalReferenceNumberTypes;
			AssertContainsExactElementsInAnyOrder(new string[] { "ADM", "JUD", "PRE", "EJD" }, list.GetAllCodes());
			AssertSame(list, number.Lookups.AdditionalReferenceNumberTypes);
		}
	}
}
