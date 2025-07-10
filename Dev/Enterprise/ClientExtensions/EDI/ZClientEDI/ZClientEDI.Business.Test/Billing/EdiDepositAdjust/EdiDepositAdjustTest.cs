using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(EdiDepositAdjust))]
	public class EdiDepositAdjustTest : EnterpriseBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var org = Factory.NewWithValidTestData<EDIOrgHeader>();
			var bizo = Factory.NewWithValidTestData<EdiDepositAdjust>();
			bizo.DEA_OH = org.PK;
			return bizo;
		}

		#endregion
	}
}
