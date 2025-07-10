using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.BR.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI
{
	public partial class LPCOForm : ZTemplateForm
	{
		public LPCOForm() : base()
		{
			if (!this.IsDesignMode())
			{
				throw new InvalidOperationException("This constructor is only for the designer. Please use the one that takes a business object.");
			}
		}

		public LPCOForm(CusLPCOHeader header) : base(header)
		{
			InitializeComponent();
			PlugIns.AddJobInvoicing(header.InvoicingSupporter);
			WorkflowTabPage.Initialize(header);
			var messagingMenutItem = new LPCOMessagingMenu(header);
			MainMenu.MenuItems.Add(MainMenu.MenuItems.IndexOf(HelpMenuItem), messagingMenutItem);
		}

		public override string FormCaption => Res.GetString("0beff361-fa4a-4d40-ae99-ef883dec6b4a", "LPCO {0} - {1}", BusinessEntity?.PermitHolder?.OH_Code ?? ZString.Empty, BusinessEntity.CPH_Number);

		protected new CusLPCOHeader BusinessEntity => (CusLPCOHeader)base.BusinessEntity;
	}
}
