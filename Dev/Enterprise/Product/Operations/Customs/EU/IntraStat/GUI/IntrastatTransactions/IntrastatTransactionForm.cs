using System;
using Enterprise.Customs.EU.Intrastat.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.Intrastat.GUI.Transactions
{
	public partial class IntrastatTransactionForm : ZTemplateForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public IntrastatTransactionForm()
		{
			InitializeComponent();
		}

		public IntrastatTransactionForm(CusIntrastatHeader dataSource) : base(dataSource)
		{
			InitializeComponent();
			ControllerID = ControllerIDs.Customs.EU.IntrastatTransactionsController;
			MainTabPage.CaptionResourceString = Res.GetData("0134c6f4-5c4a-444d-bd64-ec36053dc619", "Header");
			LinesTabPage.CaptionResourceString = Res.GetData("c9d4707a-2b22-477c-84ba-27f1fbe386d8", "Lines");
		}

		public override string FormCaption => Transaction.HumanReadableName;

		CusIntrastatHeader Transaction => (CusIntrastatHeader)BusinessEntity;
	}
}
