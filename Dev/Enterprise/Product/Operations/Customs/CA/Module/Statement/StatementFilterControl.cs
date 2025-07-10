using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Module
{
	public partial class StatementFilterControl : ZFilterStripControl<StatementFilterStrip>
	{
		public StatementFilterControl()
		{
			InitializeComponent();
		}

		public StatementFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();

			if (filterBusinessObject != null)
			{
				if (filterBusinessObject.GetType() == typeof(ARLStatementOfAccountFilterStripBusinessObject))
				{
					var accountingDate = grid.GetColumnStyle(CusStatementHeader.Schema.B2_ProcessDate);
					if (accountingDate != null)
					{
						accountingDate.IsUnavailable = true;
					}

					var statementAmountColumn = grid.GetColumnStyle(CusStatementHeader.Schema.B2_StatementAmount);
					if (statementAmountColumn != null)
					{
						statementAmountColumn.CaptionResourceString = Enterprise.Customs.CA.Module.Res.GetData("DE1BD576-94F3-491E-ACF1-B87AF666DDC7", "Statement Total");
					}
				}
				else if (filterBusinessObject.GetType() == typeof(DailyNoticeReconciliationFilterStripBusinessObject))
				{
					var paymentDueDate = grid.GetColumnStyle(CusStatementHeader.Schema.B2_DueDate);
					if (paymentDueDate != null)
					{
						paymentDueDate.IsUnavailable = true;
					}
				}
			}
		}
	}
}
