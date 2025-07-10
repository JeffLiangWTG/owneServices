using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public class UPEOrgMatchApprovalForm : OrgMatchApprovalForm
	{
		public UPEOrgMatchApprovalForm()
		{
		}

		public UPEOrgMatchApprovalForm(OrgMatchApproval businessEntity)
			: base(businessEntity)
		{
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource != null)
			{
				OwnerCodeBoundTextBox.GetExtension<ILabelCaptionRenderer>().Caption = Res.GetString("UPEOrgMatchApprovalForm|AccountNo", "Account #");
				ZGridColumnInfo ownerCodeColumn = SimilarOrgMatchesModuleButtonGrid.InnerGrid.GetColumnStyle(SimilarOrgMatchForApproval.Schema.OwnerCode);
				ownerCodeColumn.Caption = Res.GetString("UPEOrgMatchApprovalForm|AccountNo", "Account #");
			}
			base.SetDataBinding(dataSource, dataMember);
		}
	}
}
