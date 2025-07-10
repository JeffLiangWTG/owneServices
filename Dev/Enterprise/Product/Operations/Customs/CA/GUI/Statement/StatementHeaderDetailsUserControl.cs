using System;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class StatementHeaderDetailsUserControl : ZUserControl
	{
		public StatementHeaderDetailsUserControl()
		{
			InitializeComponent();
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			var header = DataSource as CusStatementHeader;

			if (header != null)
			{
				var isMonthlyStatement = header.B2_IsMonthlyStatement;
				ImporterGuidFindBox.CaptionResourceString = ImporterLabel(header);
				if (isMonthlyStatement)
				{
					StatementAmountCalcEdit.CaptionResourceString = Res.GetData("D18886DE-CA81-46D4-830E-42B42709E724", "Statement Total");
				}

				if (header.IsCARMDailyNotice)
				{
					GrandTotalGroupBox.Visible = false;
					CARMGrandTotalGroupBox.Visible = true;
				}
				else
				{
					AccountingDateDateEdit.Visible = !isMonthlyStatement;
					GrandTotalGroupBox.Visible = !isMonthlyStatement;
					CARMGrandTotalGroupBox.Visible = false;
				}
			}
		}

		ResourceStringData ImporterLabel(CusStatementHeader header)
		{
			switch (header.B2_StatementType)
			{
				case CusStatementHeaderTypes.Codes.Broker:
					return Res.GetData("77db0107-dfb0-4351-a406-6c8c6d0b40d3", "Broker");
				default:
					return Res.GetData("d52e5605-f469-4593-a689-eeadf249843a", "Importer");
			}
		}
	}
}
