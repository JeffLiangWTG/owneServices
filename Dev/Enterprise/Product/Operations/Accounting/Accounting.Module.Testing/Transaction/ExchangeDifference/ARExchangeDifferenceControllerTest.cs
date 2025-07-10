using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(ARExchangeDifferenceController))]
	class ARExchangeDifferenceControllerTest : MiscellaneousTransactionControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ARExchangeDifference;
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return TestARExDiff; }
		}

		protected override void SetupTransactionHeaderRows()
		{
			TestARExDiff = Factory.New<ARExchangeDifference>();
			Factory.Save();
		}

		protected ARExchangeDifference TestARExDiff;
	}
}
