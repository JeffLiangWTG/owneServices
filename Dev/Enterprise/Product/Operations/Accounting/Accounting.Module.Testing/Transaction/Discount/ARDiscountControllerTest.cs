using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARDiscountController))]
	class ARDiscountControllerTest : MiscellaneousTransactionControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ARDiscount;
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return TestARDiscount; }
		}

		protected override void SetupTransactionHeaderRows()
		{
			TestARDiscount = Factory.New<ARDiscount>();
			Factory.Save();
		}

		protected ARDiscount TestARDiscount;
	}
}
