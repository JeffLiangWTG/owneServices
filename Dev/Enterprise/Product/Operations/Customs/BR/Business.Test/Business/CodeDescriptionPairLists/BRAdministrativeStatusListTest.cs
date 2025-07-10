using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRAdministrativeStatusListTest : TestCase
	{
		public void TestMapToCWCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty", ZString.Empty, BRAdministrativeStatusList.MapToCWCode(ZString.Empty));
				AssertContainsExactElementsInExactOrder("When adding a new code in BRAdministrativeStatusList, please also add it into GetAdministrativeStatusCode",
					new BRAdministrativeStatusList().GetAllCodes(),
					new[] {
						BRAdministrativeStatusList.MapToCWCode("DEFERIDO"),
						BRAdministrativeStatusList.MapToCWCode("DISPENSADO"),
						BRAdministrativeStatusList.MapToCWCode("PENDENTE"),
						BRAdministrativeStatusList.MapToCWCode("EM_PROCESSAMENTO"),
						BRAdministrativeStatusList.MapToCWCode("IMPEDIDO")
				});
			});
		}
	}
}
