using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class PortugalComplianceSubTypeEditableProviderTest : TestCaseWithFactory
	{
		public void TestCheckComplianceSubTypeIsEditable()
		{
			var result = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Core.Constants.CountryCodes.Portugal) as IInstanceProvider<IComplianceSubTypeEditableProvider>).Get();

			AssertNotNull(result);
			AssertEquals(true, result.CheckComplianceSubTypeIsEditable(true));
			AssertEquals("CheckComplianceSubTypeIsEditable is false when is not reverse transaction", false, result.CheckComplianceSubTypeIsEditable(false));
		}
	}
}
