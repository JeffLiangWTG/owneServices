using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.AccountingPresentationProviders;
using Enterprise.Accounting.Business.Presentation;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Accounting.Module
{
	public partial class EnquiryFilterControl : ZFilterStripControl
	{
		[CodeAlive("Provides a list of filtered grid column layouts")]
		protected enum FilteredGridColumnLayoutContext
		{
			AP, AR
		}

		public EnquiryFilterControl()
		{
			InitializeComponent();
		}

		public EnquiryFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				SetupControls();
	SetupColumns();
				PerformSearch += delegate { ((APEnquiryFilterBusinessObject)filterBusinessObject).ResetInformationalFields(); };
			}
		}

		#region GUI Setup

		protected override void BindCore()
		{
			base.BindCore();

			BindingSource.SetBindingMember(FilteredGrid, "");
			BindingSource.SetDataBinding(FilterBusinessObject, "");
		}

		void SetupControls()
		{
			ZCalcEditColumnStyleInfo info = (ZCalcEditColumnStyleInfo)FilteredGrid.GetColumnStyle(AccTransactionHeaderSchema.Constants.AH_ExchangeRate);
			info.Decimals = GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;

			DisbursementTermsTextBox.Visible = HasDisbursementFields;
		}

		void SetupColumns()
		{
			if (!GlbCompany.CurrentCompany.Country.HasGovtTaxInvoice)
			{
				RemoveGridColumn(AccTransactionHeader.Schema.AH_TransactionReference);
			}

			if (!GlbCompany.CurrentCompany.Country.SupportComplianceSubType)
			{
				RemoveGridColumn(AccTransactionHeader.Schema.AH_ComplianceSubType);
			}

			if (!EnquiryFilterControlPresentationProvider.IsTaxBranchColumnAvailable())
			{
				RemoveGridColumn(AccTransactionHeader.Schema.AH_GB_TaxBranch);
			}
		}

		void RemoveGridColumn(ZString columnName)
		{
			foreach (Core.Forms.ZGridColumnInfo columnInfo in grid.ColumnStyles)
			{
				if (columnInfo.ColumnName == columnName)
				{
					grid.ColumnStyles.Remove(columnInfo);
					break;
				}
			}
		}

		protected virtual bool HasDisbursementFields
		{
			get { return false; }
		}

		protected override ZFilterStrip NewZFilterStrip()
		{
			return new TransactionModuleFilterStrip();
		}

		#endregion

		#region Grid Sizing

		protected override void HandleGridSizing()
		{
			if (FilteredGrid != null && ReadOnlyDetailsPanel != null)
			{
				if (FilteredGrid.Left != 0 || FilteredGrid.Right != ClientRectangle.Right || FilteredGrid.Bottom != ClientRectangle.Bottom)
				{
					FilteredGrid.Location = ControlDpiScalingHelper.NewScaledPoint(0, ControlDpiScalingHelper.UnscaleFromCurrentDpiY(FilteredGrid.Top));
					ControlDpiScalingHelper.SetHeight(FilteredGrid, ClientSize.Height - FilteredGrid.Top - ReadOnlyDetailsPanel.Height, false);
					ControlDpiScalingHelper.SetWidth(FilteredGrid, ClientSize.Width, false);
				}
			}
		}

		#endregion

		IEnquiryFilterControlPresentationProvider EnquiryFilterControlPresentationProvider => enquiryFilterControlPresentationProvider ?? (enquiryFilterControlPresentationProvider = ObjectFactory.Get<IAccountingPresentationProviderFactory>().GetEnquiryFilterControlPresentationProvider());
		IEnquiryFilterControlPresentationProvider enquiryFilterControlPresentationProvider;
	}
}
