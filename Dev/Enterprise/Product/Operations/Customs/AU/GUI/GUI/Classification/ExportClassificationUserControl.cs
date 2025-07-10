using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class ExportClassificationUserControl : Customs.GUI.BaseClassificationUserControl
	{
		public ExportClassificationUserControl()
		{
			InitializeComponent();
			InitialiseTariffFindBox();

			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.Classification";
		}

		void InitialiseTariffFindBox()
		{
			var useCustomsReferenceData = AUCAHECCWrapper.EnableCWRefForAHECC;
			tariffFindBox.Visible = useCustomsReferenceData;
			tariffFindBoxAHECC.Visible = !useCustomsReferenceData;
			if (useCustomsReferenceData)
			{
				tariffFindBox.GetCountryCode = GetCustomsCountryCode;
				tariffFindBox.GetDataGrouping = GetDataGroupingForUniversalTariff;
			}
		}

		protected string GetCustomsCountryCode() => AUCAHECCWrapper.UseCMRTariffTestData ? AUConstants.RefDataGroupCodes.AustraliaTest : Core.Constants.CountryCodes.Australia;
		protected ZString GetDataGroupingForUniversalTariff() => AUCAHECCWrapper.UseCMRTariffTestData ? AUConstants.RefDataGroupCodes.AustraliaTest : Core.Constants.CountryCodes.Australia;
	}
}

