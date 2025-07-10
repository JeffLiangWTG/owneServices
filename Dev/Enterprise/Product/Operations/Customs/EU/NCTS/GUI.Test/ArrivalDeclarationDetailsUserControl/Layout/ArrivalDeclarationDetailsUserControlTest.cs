using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.GUI.Testing
{
	[TestedType(typeof(ArrivalDeclarationDetailsUserControl))]
	sealed class ArrivalDeclarationDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsHeader), control.BindingSource.DataSourceType);
		}

		public void TestTransactionStatus()
		{
			AssertType<ZDropEdit>(control.PhaseDropEdit);
		}

		public void TestStatusDropEdit()
		{
			AssertType<ZDropEdit>(control.StatusDropEdit);
		}

		public void TestMessageStatusDropEdit()
		{
			AssertType<ZDropEdit>(control.MessageStatusDropEdit);
		}

		public void TestSeparatorLabel()
		{
			AssertType<ZLabel>(control.SeparatorLabel);
		}

		public void TestReleaseDateEdit() => CombineAssertions(() =>
		{
			AssertEquals("Binding", nameof(NctsHeader.MovementReferenceIssueDate), control.ReleaseDateEdit.GetBindingMember());
			AssertType<ZDateEdit>("Type", control.ReleaseDateEdit);
		});

		public void TestAcceptanceDateEdit() => CombineAssertions(() =>
		{
			AssertEquals("Binding", $"{nameof(NctsHeader.ArrivalMovementHeader)}.{nameof(NctsArrivalMovementHeader.BM_EntryDate)}", control.AcceptanceDateEdit.GetBindingMember());
			AssertType<ZDateEdit>("Type", control.AcceptanceDateEdit);
			AssertEquals("Read-only", expected: true, control.AcceptanceDateEdit.ReadOnly);
		});

		protected override void SetUp()
		{
			base.SetUp();
			control = new ArrivalDeclarationDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		ArrivalDeclarationDetailsUserControl control;
	}
}
