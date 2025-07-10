using CargoWise.Application;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	public class IsraelComplianceReportDefaultValueTest : TestCase
	{
		[TestDate(2024, 5, 9, 8, 12, 4, 123)]
		public void TestGetReferenceNumber()
		{
			AssertEquals("240509081204123", GetIsraelComplianceReportDefaultValue.GetReferenceNumber());
		}

		IComplianceReportDefaultValue GetIsraelComplianceReportDefaultValue => ((ObjectFactory.Get<IGlobalAccountingCountryFactory>()).GetCountryFactory(Core.Constants.CountryCodes.Israel) as IInstanceProvider<IComplianceReportDefaultValue>).Get();
	}
}
