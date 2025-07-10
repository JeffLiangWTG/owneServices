using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class CAExportClassificationUserControl : BaseClassificationUserControl
	{
		public CAExportClassificationUserControl()
		{
			InitializeComponent();

			ClassificationTariffUserControlHelper.UpdateTariffFindBoxToGetTariffFromSRDb(
				tariffCodeFindBox,
				"tariffCodeFromSRDbFindBox",
				() => { return ZDateTime.Today; },
				(x) =>
				{
					this.BindingSource.SetBindingMember(x, CusClassification.Schema.CC_FormattedTariffNum);
					CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassification)(null)).CC_FormattedTariffNum);
				}
				);
		}
	}
}
