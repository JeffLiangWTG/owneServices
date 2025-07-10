using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class AccQueryClaimFilterControl : ZFilterStripControl
	{
		public AccQueryClaimFilterControl()
		{
			InitializeComponent();
		}

		public AccQueryClaimFilterControl(IBusinessObjectCollection gridCollection, AccQueryClaimFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();

			var creditorDebtorColumn = FilteredGrid.ColumnStyles.OfType<ZGridColumnInfo>().FirstOrDefault(column => column.ColumnName == "AY_OH_Debtor");
			if (creditorDebtorColumn != null)
			{
				creditorDebtorColumn.Caption = CreditorDebtorText;
			}
		}

		ZString CreditorDebtorText => ((AccQueryClaimFilterBusinessObject)FilterBusinessObject).CreditorDebtorCaption;
	}
}
