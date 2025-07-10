using CargoWise.EntityFramework;
using Enterprise.Customs.Common.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.EU.GUI
{
	[SuppressCheckControlModuleId]
	[FormBasherTestPopupExclude]
	public partial class TariffFindBox : ZCodeFindBox
	{
		public string ParameterForediTariff
		{
			get;
			set;
		}

		protected override IFindBoxPopup GetNewPopupForm()
		{
			return BorderWiseTariffFindBoxProvider.GetTariffFindBoxWrapper(ParameterForediTariff ?? "E", () => base.GetNewPopupForm());
		}

		protected override IFindBoxListProvider ListProvider
		{
			get { return new TariffFindBoxListProvider(); }
		}

		protected override void ShowEditOrViewForm()
		{
			//Because JI_Procedure/JI_FormattedTariff/CI_FormattedTariffNum/D1_CommodityCode/CC_FormattedTariffNum/CC_ProcedureCode/TariffNumber/CPCCode has no ListAttribute and when it's bound to a control it has no list bound with it, this can never succeed anyway, so we override it to do nothing. See WI00078618
		}

		protected override bool RequiresList
		{
			get { return false; }
		}
	}
}
