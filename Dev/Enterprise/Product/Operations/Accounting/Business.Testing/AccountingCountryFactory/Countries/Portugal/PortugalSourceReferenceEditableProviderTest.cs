using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class PortugalSourceReferenceEditableProviderTest : TestCaseWithFactory
	{
		public void TestCheckReferenceSourceIsEditable()
		{
			var result = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Core.Constants.CountryCodes.Portugal) as IInstanceProvider<ISourceReferenceEditableProvider>).Get();

			AssertNotNull(result);
			AssertEquals(true, result.CheckReferenceSourceIsEditable(true, "TCM"));
			AssertEquals(true, result.CheckReferenceSourceIsEditable(true, "LCR"));
			AssertEquals("CheckReferenceSourceIsEditable will be false when compliance sub type is not TCM or LCR.", false, result.CheckReferenceSourceIsEditable(true, "TDM"));
			AssertEquals("CheckReferenceSourceIsEditable will be false when it is not reverse transaction.", false, result.CheckReferenceSourceIsEditable(false, "TCM"));
		}
	}
}
