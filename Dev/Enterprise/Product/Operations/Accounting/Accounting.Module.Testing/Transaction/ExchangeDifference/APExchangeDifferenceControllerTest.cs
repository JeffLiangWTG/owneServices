using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Accounting.Module.Testing
{
	[TestedType(typeof(APExchangeDifferenceController))]
	class APExchangeDifferenceControllerTest : MiscellaneousTransactionControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.APExchangeDifference;
		}

		protected override BusinessObject ParentTransactionHeaderRow
		{
			get { return TestAPExDiff; }
		}

		protected override void SetupTransactionHeaderRows()
		{
			TestAPExDiff = Factory.New<APExchangeDifference>();
			Factory.Save();
		}

		protected APExchangeDifference TestAPExDiff;
	}
}
