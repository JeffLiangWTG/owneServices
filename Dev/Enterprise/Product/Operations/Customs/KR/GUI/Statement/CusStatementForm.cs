using CargoWise.Windows.UI;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class Statements : ZTemplateForm, IPostingButtonsProvider
	{
		public Statements(CusStatementHeader header)
			: base(header)
		{
			this.statementHeader = header;
			InitializeComponent();
			SetMainTabControlCaption();
		}
		readonly CusStatementHeader statementHeader;
		public override string FormCaption
		{
			get
			{
				string caption = Res.GetString("37A5B281-E100-4259-AF7D-1A962ACDB9A8", "Statement");

				if (!this.IsDesignMode() && statementHeader != null)
				{
					caption = statementHeader.B2_StatementType == StatementHeaderTypeList.Codes.CustomsDisbursementBill
								? Res.GetString("F4F8D7DA-C737-4C0E-BB7E-E328115A1C42", "Customs Individual Disbursement Bills") : caption;
					caption += " - " + statementHeader.FormattedStatementNumber;
				}
				return caption;
			}
		}

		void SetMainTabControlCaption()
		{
			if (statementHeader != null)
			{
				if (statementHeader.B2_StatementType == StatementHeaderTypeList.Codes.CustomsDisbursementBill)
				{
					MainTabPage.CaptionResourceString = Res.GetData("6049EE6F-1F2F-4FD8-A76C-B3732AD8CF52", "Bills");
				}
			}
		}

		#region IPostingButtonsProvider Members

		bool IPostingButtonsProvider.IsPostOnly
		{
			get { return true; }
			set { }
		}

		#endregion
	}
}
