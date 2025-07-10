using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class PanamaEInvoicingRequeuePivotsStatusProviderTest : TestCaseWithFactory
	{
		public void TestGetEligibleForRequeuingStatus()
		{
			AssertArrayEqualsByElements(new List<ZString> { Constants.EInvoicingPivotState.Failed, Constants.EInvoicingPivotState.BatchedWithError, Constants.EInvoicingPivotState.Sent, Constants.EInvoicingPivotState.Delivered }.ToArray(), GetInstance().GetEligibleForRequeuingStatus().ToArray());
		}

		public void TestGetSecurityConstraintStatus()
		{
			AssertArrayEqualsByElements(new List<ZString> { Constants.EInvoicingPivotState.Sent, Constants.EInvoicingPivotState.Delivered }.ToArray(), GetInstance().GetSecurityConstraintStatus().ToArray());
		}

		IEInvoicingRequeuePivotsStatusProvider GetInstance() => (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Panama) as IInstanceProvider<IEInvoicingRequeuePivotsStatusProvider>).Get();
	}
}
