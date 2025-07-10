using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using NctsHeader = Enterprise.Customs.ES.NCTS.Business.NctsHeader;

namespace Enterprise.Customs.ES.NCTS.GUI.Testing
{
	class UnloadedItemDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestSetGoodsItemDifferencesGridsReadOnly_Dynamically()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.BH_HeaderType = NctsMovementType.Codes.Arrival;

			using (var form = new ZForm(header))
			using (var control = new UnloadedItemDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals($"OriginalContainersGrid is not ReadOnly for Arrival", false, control.FindSingle<ZGrid>("OriginalContainersGrid").ReadOnly);
				AssertEquals($"OriginalPackagesGrid is not ReadOnly for Arrival", false, control.FindSingle<ZGrid>("OriginalPackagesGrid").ReadOnly);
				AssertEquals($"OriginalSupportingDocumentsGrid is not ReadOnly for Arrival", false, control.FindSingle<ZGrid>("OriginalSupportingDocumentsGrid").ReadOnly);

				header.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;

				AssertEquals($"OriginalContainersGrid is ReadOnly for DepartureAndArrival", true, control.FindSingle<ZGrid>("OriginalContainersGrid").ReadOnly);
				AssertEquals($"OriginalPackagesGrid is ReadOnly for DepartureAndArrival", true, control.FindSingle<ZGrid>("OriginalPackagesGrid").ReadOnly);
				AssertEquals($"OriginalSupportingDocumentsGrid is ReadOnly for DepartureAndArrival", true, control.FindSingle<ZGrid>("OriginalSupportingDocumentsGrid").ReadOnly);
			}
		}

		public void TestSetGoodsItemDifferencesGridsReadOnly_Arrival()
		{
			AssertControls(false, NctsMovementType.Codes.Arrival);
		}

		public void TestSetGoodsItemDifferencesGridsReadOnly_DepartureAndArrival()
		{
			AssertControls(true, NctsMovementType.Codes.DepartureAndArrival);
		}

		[RequiresSTA]
		public void TestBillOfLadingTextBoxVisibility()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			using (var form = new NctsMovementForm(header))
			using (var control = new UnloadedItemDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var billOfLadingTextBox = (ZTextBox)form.Controls.Find("BillOfLadingTextBox", true).FirstOrDefault();
					AssertEquals("BillOfLadingTextBox is invisible initially.", false, billOfLadingTextBox.Visible);

					header.ESNctsHeader.CEN_PreviousSummaryDeclaration = "TEST";
					AssertEquals("BillOfLadingTextBox is visible when Previous Summary Declaration is filled.", true, billOfLadingTextBox.Visible);
				});
			}
		}

		public void TestSeqNumber()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var line = header.ArrivalMovementHeader.GoodsItems.AddNew();
			line.SupportingDocuments.AddNew();
			using (var form = new NctsMovementForm(header))
			using (var control = new UnloadedItemDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var originalSupportingDocumentsGrid = control.FindSingle<ZGrid>("OriginalSupportingDocumentsGrid");
				var seqNumber = originalSupportingDocumentsGrid.GetColumnStyle(NctsSupportingDocument.Schema.CSI_LineNo);

				AssertNotNull("User control should have Seq.Number column", seqNumber);
			}
		}

		void AssertControls(bool isReadOnly, ZString movementType)
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(movementType);
			using (var form = new ZForm(header))
			using (var control = new UnloadedItemDetailsUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals($"OriginalContainersGrid is ReadOnly for {movementType}", isReadOnly, control.FindSingle<ZGrid>("OriginalContainersGrid").ReadOnly);
					AssertEquals($"OriginalPackagesGrid is ReadOnly for {movementType}", isReadOnly, control.FindSingle<ZGrid>("OriginalPackagesGrid").ReadOnly);
					AssertEquals($"OriginalSupportingDocumentsGrid is ReadOnly for {movementType}", isReadOnly, control.FindSingle<ZGrid>("OriginalSupportingDocumentsGrid").ReadOnly);
				});
			}
		}
	}
}
