using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ES.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	sealed class DeclarationDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			using (var control = new DeclarationDetailsUserControl())
			{
				AssertEquals(typeof(NctsDepartureMovementHeader), control.BindingSource.DataSourceType);
			}
		}

		public void TestAcceptanceDateDateEdit()
		{
			using (var control = new DeclarationDetailsUserControl())
			{
				var acceptanceDateDateEdit = control.AcceptanceDateDateEdit;
				CombineAssertions(() =>
				{
					AssertType<ZDateEdit>(acceptanceDateDateEdit);
					AssertEquals("BindTo", $"{nameof(NctsDepartureMovementHeader.Header)}.{nameof(NctsHeader.AcceptanceDate)}", acceptanceDateDateEdit.BindTo);
				});
			}
		}

		public void TestCircuitTextBox()
		{
			using (var control = new DeclarationDetailsUserControl())
			{
				var circuitTextBox = control.CircuitTextBox;
				CombineAssertions(() =>
				{
					AssertType<ZTextBox>(circuitTextBox);
					AssertEquals("BindTo", $"{nameof(NctsDepartureMovementHeader.Header)}.{nameof(NctsHeader.Circuit)}", circuitTextBox.BindTo);
				});
			}
		}

		public void TestClearanceNumberTextBox()
		{
			using (var control = new DeclarationDetailsUserControl())
			{
				var clearanceNumberTextBox = control.ClearanceNumberTextBox;
				CombineAssertions(() =>
				{
					AssertType<ZTextBox>(clearanceNumberTextBox);
					AssertEquals("BindTo", $"{nameof(NctsDepartureMovementHeader.Header)}.{nameof(NctsHeader.ClearanceReferenceNumber)}", clearanceNumberTextBox.BindTo);
				});
			}
		}

		public void TestClearanceDateDateEdit()
		{
			using (var control = new DeclarationDetailsUserControl())
			{
				var clearanceDateDateEdit = control.ClearanceDateDateEdit;
				CombineAssertions(() =>
				{
					AssertType<ZDateEdit>(clearanceDateDateEdit);
					AssertEquals("BindTo", $"{nameof(NctsDepartureMovementHeader.Header)}.{nameof(NctsHeader.ClearanceDate)}", clearanceDateDateEdit.BindTo);
				});
			}
		}

		public void TestArrivalLimitDateEdit()
		{
			using (var control = new DeclarationDetailsUserControl())
			{
				var arrivalLimitDateEdit = control.ArrivalLimitDateEdit;
				CombineAssertions(() =>
				{
					AssertType<ZDateEdit>(arrivalLimitDateEdit);
					AssertEquals("BindTo", $"{nameof(NctsDepartureMovementHeader.Header)}.{nameof(NctsHeader.ArrivalLimit)}", arrivalLimitDateEdit.BindTo);
				});
			}
		}
	}
}
