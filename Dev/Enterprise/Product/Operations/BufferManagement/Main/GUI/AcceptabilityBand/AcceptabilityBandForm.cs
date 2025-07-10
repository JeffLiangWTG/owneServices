using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI
{
	public partial class AcceptabilityBandForm : ZTemplateForm
	{
		public AcceptabilityBandForm(BMComponentAcceptabilityBand businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();

			MENTTabPage.BindingOrFirstShown += MENTTabPage_BindingOrFirstShown;
		}

		void MENTTabPage_BindingOrFirstShown(object sender, System.EventArgs e)
		{
			var mentControl = ObjectFactory.Get<ZUserControl>("MENTControl");
			mentControl.Dock = System.Windows.Forms.DockStyle.Fill;
			mentControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			mentControl.Name = "mentControl";
			mentControl.TabIndex = 0;
			MENTTabPage.Controls.Add(mentControl);
		}

		protected override bool SupportsEDocs => false;

		protected override bool ShowAuditTab => true;

		public override string FormCaption
		{
			get
			{
				var band = (BMComponentAcceptabilityBand)BusinessEntity;
				if (band != null)
				{
					return band.HumanReadableName;
				}

				return base.FormCaption;
			}
		}

		protected override ContinueWithDelete ShowPreDeleteDialogs()
		{
			var result = base.ShowPreDeleteDialogs();

			if (result == ContinueWithDelete.Yes)
			{
				var usedInBoards = ((BMComponentAcceptabilityBand)BusinessEntity).GetAllBoardsUsingThisAcceptabilityBand().OrderBy(b => b.MB_Name).ToArray();

				if (usedInBoards.Length > 0)
				{
					var boardNames = string.Concat(usedInBoards.Select(b => System.Environment.NewLine + b.MB_Name));
					var message = Res.GetString("AcceptabilityBandForm|DeleteConfirmation|Message", "You are about to delete this record permanently from the section config in following boards. Do you want to proceed?") + boardNames; // An explanation that must be at least 15 characters long
					var dialogResult = Globals.Message.Show(message, Res.GetString("AcceptabilityBandForm|DeleteConfirmation|Caption", "Delete Confirmation"), MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, DialogResult.Cancel);
					result = dialogResult == DialogResult.OK ? ContinueWithDelete.Yes : ContinueWithDelete.No;
				}
			}

			return result;
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && (components != null))
				{
					components.Dispose();
					MENTTabPage.BindingOrFirstShown -= MENTTabPage_BindingOrFirstShown;
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}
	}
}
