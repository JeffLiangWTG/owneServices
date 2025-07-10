using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APDiscountController))]
	class APDiscountControllerTest : MiscellaneousTransactionControllerTest
	{
		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return TestAPDiscount; }
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.APDiscount;
		}

		protected override void SetupTransactionHeaderRows()
		{
			TestAPDiscount = Factory.New<APDiscount>();
			Factory.Save();
		}

		protected APDiscount TestAPDiscount;
	}
}
