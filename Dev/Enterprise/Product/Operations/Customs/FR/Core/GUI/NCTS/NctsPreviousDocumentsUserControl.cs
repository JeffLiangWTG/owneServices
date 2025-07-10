using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	public partial class NctsPreviousDocumentsUserControl : EU.NCTS.GUI.PreviousDocumentsUserControl
	{
		public NctsPreviousDocumentsUserControl()
		{
			InitializeComponent();
			PrevDocsReferenceTextBox.AllowOverlap(PrevDocsReferenceCodeFindBox);
		}

		protected override void InitializeGridLayout()
		{
			base.InitializeGridLayout();
			using (PreviousDocumentsGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				PreviousDocumentsGrid.ColumnStyles.Remove(PreviousDocumentsGrid.GetColumnStyle(NctsPreviousDocument.Schema.CSI_SubType));
				PreviousDocumentsGrid.ColumnStyles.Add(new ZTextBoxColumnStyleInfo()
				{
					ColumnName = NctsPreviousDocument.Schema.CSI_Description,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80),
					CharacterCasing = CharacterCasing.Normal
				});
			}
		}

		protected void PrevDocsTypeDropEdit_TextChanged(object sender, EventArgs e)
		{
			var prevDocsType = sender as ZDropEdit;
			if (prevDocsType != null)
			{
				PrevDocsReferenceCodeFindBox.Visible = ShouldHaveACodeFindBoxForReference(prevDocsType.Text);
				PrevDocsReferenceTextBox.Visible = !PrevDocsReferenceCodeFindBox.Visible;
			}
		}

		protected bool ShouldHaveACodeFindBoxForReference(ZString newValueForCSICode) => newValueForCSICode == PreviousDocumentCodeList.Codes._337;
	}
}
