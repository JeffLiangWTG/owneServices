using System;
using CargoWise.Types;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public partial class TaxUserControl : BaseCustomsEntryUserControl
	{
		public TaxUserControl()
		{
			InitializeComponent();
			ShowHideCountrySpecificColumns();
			SetPropertiesAfterInit();
		}

		protected virtual void SetPropertiesAfterInit()
		{
		}

		void ShowHideCountrySpecificColumns()
		{
			var countryCode = DesignModeFinder.IsDesigning ? ZString.Empty : MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			ShowHideGbSpecificColumns(countryCode == Core.Constants.CountryCodes.UnitedKingdom);
		}

		void ShowHideGbSpecificColumns(bool isGB)
		{
			zTextBoxColumnStyleInfoForMethodOfCalculation.IsVisible = !isGB;
		}

		internal void ChangeParent(string parent)
		{
			new ControlRebinder().Rebind(this, "FilteredInvoiceLines", parent);
		}

		protected virtual Type DeclarationType => typeof(Business.Declaration.JobDeclaration);
	}
}
