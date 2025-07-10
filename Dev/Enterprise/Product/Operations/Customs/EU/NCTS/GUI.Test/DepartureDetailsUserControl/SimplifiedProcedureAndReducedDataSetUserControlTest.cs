using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class SimplifiedProcedureAndReducedDataSetUserControlTest : TestCase
	{
		public void TestSimplifiedNctsProcedureCheckBox()
		{
			var simplifiedNctsProcedureCheckBox = userControl.SimplifiedNctsProcedureCheckBox;
			CombineAssertions(() =>
			{
				AssertEquals("Binding", nameof(NctsHeader.MovementHeader) + "." + nameof(NctsDepartureMovementHeader.IsSimplifiedNctsProcedure), simplifiedNctsProcedureCheckBox.GetBindingMember());
				AssertEquals("TabIndex", 0, simplifiedNctsProcedureCheckBox.TabIndex);
			});
		}

		public void TestReducedDatasetIndicatorCheckBox()
		{
			var reducedDatasetIndicatorCheckBox = userControl.ReducedDatasetIndicatorCheckBox;
			CombineAssertions(() =>
			{
				AssertEquals("Binding", nameof(NctsHeader.MovementHeader) + "." + nameof(NctsDepartureMovementHeader.BM_ReducedDatasetIndicator), reducedDatasetIndicatorCheckBox.GetBindingMember());
				AssertEquals("TabIndex", 1, reducedDatasetIndicatorCheckBox.TabIndex);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new SimplifiedProcedureAndReducedDataSetUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
		SimplifiedProcedureAndReducedDataSetUserControl userControl;
	}
}
