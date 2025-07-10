using Enterprise.Customs.KR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.KR.Module.Testing
{
	sealed class CusReconDeclarationFilterControlTest : ZFilterStripControlTest
	{
		public void TestCusReconDeclarationFilterControl()
		{
			var filterObject = new CusReconDeclarationFilterStripBusinessObject();
			var gridCollection = new CusReconDeclarationCollection(Factory, GlbCompany.CurrentCompany);
			using (var form = new ZForm())
			using (var control = new CusReconDeclarationFilterControl(gridCollection, filterObject))
			{
				form.Controls.Add(control);
				form.Show();

				var index = 0;
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(CusReconDeclaration.CRD_JobReferenceNumber));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(CusReconDeclaration.RefundDeclarationNumber));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(CusReconDeclaration.TotalRefundAmount));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(CusReconDeclaration.MessageStatusDescription));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(CusReconDeclaration.EntryStatusDescription));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(CusReconDeclaration.AcceptedDate));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(CusReconDeclaration.RefundApprovalDate));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(CusReconDeclaration.RefundApprovalNumber));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(CusReconDeclaration.PayerCompanyName));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(CusReconDeclaration.CRD_CustomsOffice));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(CusReconDeclaration.RefundBillCount));
				AssertEquals(control.Grid.Columns[index++].ColumnName, nameof(CusReconDeclaration.CRD_GB_Branch));

				AssertEquals(false, control.Grid.GetColumnStyle(nameof(CusReconDeclaration.CRD_MessageStatus)).IsVisible);
				AssertEquals(false, control.Grid.GetColumnStyle(nameof(CusReconDeclaration.CRD_CustomsStatus)).IsVisible);
				AssertEquals(false, control.Grid.GetColumnStyle(nameof(CusReconDeclaration.CRD_OA_DeclarantAddress)).IsVisible);
				AssertEquals(false, control.Grid.GetColumnStyle(nameof(CusReconDeclaration.OfficeDescription)).IsVisible);
				AssertEquals(false, control.Grid.GetColumnStyle(nameof(CusReconDeclaration.CRD_SystemCreateUser)).IsVisible);
				AssertEquals(false, control.Grid.GetColumnStyle(nameof(CusReconDeclaration.CRD_SystemCreateTimeUtc)).IsVisible);
				AssertEquals(false, control.Grid.GetColumnStyle(nameof(CusReconDeclaration.CRD_SystemLastEditUser)).IsVisible);
				AssertEquals(false, control.Grid.GetColumnStyle(nameof(CusReconDeclaration.CRD_SystemLastEditTimeUtc)).IsVisible);
			}
		}
	}
}
