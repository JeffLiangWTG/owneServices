using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public partial class OrgSupplierPartTaxUserControl : TaxUserControl, ISupportingInfoUserControls
	{
		public OrgSupplierPartTaxUserControl()
		{
			InitializeComponent();
			RemoveTaxColumnsForOrgSupplierPart();
		}

		string ISupportingInfoUserControls.GridBindingMember => "FilteredInvoiceLines";

		ZGrid ISupportingInfoUserControls.Grid => TaxGrid;

		void RemoveTaxColumnsForOrgSupplierPart()
		{
			try
			{
				TaxGrid.ColumnStyles.RemoveAt(7);
				TaxGrid.ColumnStyles.RemoveAt(5);
				TaxGrid.ColumnStyles.RemoveAt(4);
				TaxGrid.ColumnStyles.RemoveAt(3);
				TaxGrid.ColumnStyles.RemoveAt(2);
				TaxGrid.ColumnStyles.RemoveAt(1);
				var countryCode = this.IsDesignMode() ? ZString.Empty : GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				if (countryCode != Core.Constants.CountryCodes.UnitedKingdom)
				{
					TaxGroupBox.Controls.RemoveByKey("TaxRateGroupBox");
				}
				else
				{
					var rateDuty =
						new ZArchitecture.GUI.ZDropEditColumnStyleInfo
						{
							CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData(EUAddInfoTaxSchema.Constants.G4_RateDuty, "Rate"),
							BindToList = "Data.Lookups.RateDutyList",
							ColumnName = "Data+G4_RateDuty"
						};

					TaxGrid.ColumnStyles.Add(rateDuty);
				}
				TaxGroupBox.Controls.RemoveByKey("TaxBaseQtyCalcEdit");
				TaxGroupBox.Controls.RemoveByKey("TaxAmountCalcEdit");
				TaxGroupBox.Controls.RemoveByKey("TaxBaseAmountCalcEdit");
				TaxGroupBox.Controls.RemoveByKey("TaxBaseQuantityUQDropEdit");
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("OrgSupplierPartTaxUserControl.RemoveTaxColumnsForOrgSupplierPart", "Probably changed number of columns in the tax grid, or renamed controls.", ex);
			}
		}
	}
}
