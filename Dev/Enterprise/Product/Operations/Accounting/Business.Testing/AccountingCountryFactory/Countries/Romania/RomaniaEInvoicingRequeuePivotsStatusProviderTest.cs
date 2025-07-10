using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing;

public class RomaniaEInvoicingRequeuePivotsStatusProviderTest : TestCaseWithFactory
{
	public void TestGetEligibleForRequeuingStatus()
	{
		AssertArrayEqualsByElements(new List<ZString> { EInvoicingPivotState.Failed, EInvoicingPivotState.BatchedWithError, EInvoicingPivotState.Sent, EInvoicingPivotState.Delivered }.ToArray(), GetInstance().GetEligibleForRequeuingStatus().ToArray());
	}

	public void TestGetSecurityConstraintStatus()
	{
		AssertArrayEqualsByElements(new List<ZString> { EInvoicingPivotState.Sent, EInvoicingPivotState.Delivered }.ToArray(), GetInstance().GetSecurityConstraintStatus().ToArray());
	}

	IEInvoicingRequeuePivotsStatusProvider GetInstance() => (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(Constants.CountryCodes.Romania) as IInstanceProvider<IEInvoicingRequeuePivotsStatusProvider>).Get();
}
