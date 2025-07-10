using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class InvoiceLineRFPDetailsUserControl : ZUserControl
	{
		public InvoiceLineRFPDetailsUserControl()
		{
			InitializeComponent();
		}

		bool Errata54Enabled => CachedValueHelper.GetValue(ref errata54Enabled, () => UniversalReferenceHelper.Errata54Enabled());
		CachedValue<bool> errata54Enabled;

		public void InitializeEUTariffFindBox(Func<ZDateTime> getEffectiveAssessmentDateForUniversalTariff)
		{
			EUTariffFindBox.GetEffectiveDate = getEffectiveAssessmentDateForUniversalTariff;
		}

		public void SetupVisibility(ZBool isNEXDOCSActive)
		{
			QL_CategoryCodeFindBox.Visible = isNEXDOCSActive;
			QL_SupplimentaryCodeDropEdit.Visible = isNEXDOCSActive;
			QL_SupplimentaryCodeCodeFindBox.Visible = !isNEXDOCSActive;
			EUTariffFindBox.Visible = isNEXDOCSActive;
			ProductConditionGroupBox.Visible = !isNEXDOCSActive && Errata54Enabled;
			ProductConditionGrid.Visible = !isNEXDOCSActive && Errata54Enabled;
			ProductPartDropEdit.Visible = !isNEXDOCSActive && Errata54Enabled;
		}
	}
}
