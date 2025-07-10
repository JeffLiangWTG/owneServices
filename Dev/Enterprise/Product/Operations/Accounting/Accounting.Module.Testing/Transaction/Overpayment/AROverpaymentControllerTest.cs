using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(AROverpaymentController))]
	class AROverpaymentControllerTest : MiscellaneousTransactionControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.AROverpayment;
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return TestAROVP; }
		}

		protected override void SetupTransactionHeaderRows()
		{
			TestAROVP = Factory.New<AROverpayment>();
			Factory.Save();
		}

		protected AROverpayment TestAROVP;
	}
}
