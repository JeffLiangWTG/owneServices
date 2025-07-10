using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	sealed class DeclarationDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsDepartureMovementHeader), control.BindingSource.DataSourceType);
		}

		public void TestMrnTextBox()
		{
			var mrnTextBox = control.MrnTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>(mrnTextBox);
				AssertEquals("BindTo", $"{nameof(NctsDepartureMovementHeader.Header)}.{nameof(NctsHeader.MovementReferenceNumber)}", mrnTextBox.BindTo);
			});
		}

		public void TestDepartureStatusDropEdit()
		{
			var departureStatusDropEdit = control.DepartureStatusDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>(departureStatusDropEdit);
				AssertEquals("BindTo", nameof(NctsDepartureMovementHeader.BM_CustomsStatus), departureStatusDropEdit.BindTo);
			});
		}

		public void TestMessageStatusDropEdit()
		{
			var messageStatusDropEdit = control.MessageStatusDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>(messageStatusDropEdit);
				AssertEquals("BindTo", nameof(NctsDepartureMovementHeader.BM_MessageStatus), messageStatusDropEdit.BindTo);
			});
		}

		public void TestPhaseStatusDropEdit()
		{
			var phaseStatusDropEdit = control.PhaseStatusDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>(phaseStatusDropEdit);
				AssertEquals("BindTo", nameof(NctsDepartureMovementHeader.BM_Phase), phaseStatusDropEdit.BindTo);
			});
		}

		public void TestReleaseDateEdit()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Binding", $"{nameof(NctsDepartureMovementHeader.Header)}.{nameof(NctsHeader.MovementReferenceIssueDate)}", control.ReleaseDateEdit.GetBindingMember());
				AssertType<ZDateEdit>("Type", control.ReleaseDateEdit);
			});
		}

		public void TestAcceptanceDateEdit()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Binding", nameof(NctsDepartureMovementHeader.BM_EntryDate), control.AcceptanceDateEdit.GetBindingMember());
				AssertType<ZDateEdit>("Type", control.AcceptanceDateEdit);
				AssertEquals("Read-only", expected: true, control.AcceptanceDateEdit.ReadOnly);
			});
		}

		public void TestActivationDeadlineDateEdit()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Binding", $"{nameof(NctsDepartureMovementHeader.Header)}.{nameof(NctsHeader.MovementReferenceExpiryDate)}", control.ActivationDeadlineDateEdit.GetBindingMember());
				AssertType<ZDateEdit>("Type", control.ActivationDeadlineDateEdit);
				AssertEquals("Read-only", expected: true, control.ActivationDeadlineDateEdit.ReadOnly);
			});
		}

		protected override void SetUp()
		{
			control = new DeclarationDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		DeclarationDetailsUserControl control;
	}
}
