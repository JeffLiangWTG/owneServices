using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Overpayment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APOverpaymentController))]
	class APOverpaymentControllerTest : MiscellaneousTransactionControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.APOverpayment;
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return TestAPOVP; }
		}

		protected override void SetupTransactionHeaderRows()
		{
			TestAPOVP = Factory.New<APOverpayment>();
			Factory.Save();
		}

		protected APOverpayment TestAPOVP;
	}
}
