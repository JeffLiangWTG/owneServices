using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class JobDeclarationForm : EU.GUI.JobDeclarationForm
	{
		public JobDeclarationForm()
			: base()
		{
		}

		public JobDeclarationForm(JobDeclaration declaration)
			: base(declaration)
		{
			recalculateNetPriceMenu = (ZMenuItem)ZFormMenuStrategy.AddActionsMenuItem(this, RecalculateNetPriceMenuName, RecalculateNetPriceMenu_Click);

			ActionsMenuItem.Popup += ActionsMenuItem_Popup;
		}
		readonly ZMenuItem recalculateNetPriceMenu;

		public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

		protected override IEDIMenu GetNewTopLevelMenuCore() => new EDIMenu();

		protected override BaseCustomsBrokerageUserControl GetBrokerageUserControl() => new CustomsBrokerageUserControl();

		void ActionsMenuItem_Popup(object sender, EventArgs e)
		{
			recalculateNetPriceMenu.Enabled = Declaration != null && Declaration.IsImport && Declaration.ZG_IsHighValueOvrd;
		}

		void RecalculateNetPriceMenu_Click(object sender, EventArgs e)
		{
			if (Declaration != null)
			{
				if (Globals.Message.ShowConfirmation(Res.GetString("35BAEB9F-71F5-46D6-8969-8F82367E3D8A",
@"To calculate the Net Price the following fields must be filled:

-Invoice Currency
-[42] Price
-at least one charge code of type DIS

Do you really want to recalculate the Net Price? This will override an existing value!
")
				, RecalculateNetPriceMenuName
				, Res.GetString("4B673861-7E08-4538-B069-EBA3FE383FCE", "yes")
				, MessageBoxIcon.Question) == DialogResult.OK)
				{
					Declaration.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(i => i.CalculateNetPriceIfNeeded(ZBool.True));
				}
			}
		}

		ResourceString RecalculateNetPriceMenuName => ResString.GetMultilingualString("8D205FBC-F21F-495F-AEBC-534FF174E514", "Recalculate Net Price");
	}
}
