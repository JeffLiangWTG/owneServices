using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.SADH;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.SADH
{
	public partial class SADHEntryForm : Customs.GUI.SADH.SADHEntryForm
	{
		public SADHEntryForm(SADHFormDataManager formDataManager)
			: base(formDataManager)
		{
			InitializeTariffFindBox();
			FormRightBorderLabel.AllowOverlap(Section33Panel);
			Section36Panel.AllowOverlap(Section33Panel);
		}

		void InitializeTariffFindBox()
		{
			var tariffFindBox = new Universal.GUI.TariffFindBox();
			tariffFindBox.GetCountryCode = () => this.IsDesignMode() ? ZString.Empty : MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			tariffFindBox.GetTariffType = GetTariffType;
			tariffFindBox.BindTo = "D1_CommodityCode";
			tariffFindBox.Name = "Section33TariffFindBox";
			tariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 18, isInStandardDpi: true);
			tariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(295, 20, isInStandardDpi: true);
			tariffFindBox.TabIndex = 5;
			Section33Panel.Controls.Add(tariffFindBox);
		}

		ZString GetTariffType()
		{
			return TariffFormatter.GetTariffType(FormDataManager.FormData.IsExport);
		}
	}
}
