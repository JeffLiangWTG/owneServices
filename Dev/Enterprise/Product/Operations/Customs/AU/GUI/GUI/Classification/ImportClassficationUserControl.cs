using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class ImportClassificationUserControl : Customs.GUI.BaseClassificationUserControl
	{
		public ImportClassificationUserControl()
		{
			InitializeComponent();
			InitialiseTariffFindBox();
		}

		void InitialiseTariffFindBox()
		{
			var useCustomsReferenceData = AUCClassWrapper.UseCustomsReferenceData;
			tariffFindBox.Visible = useCustomsReferenceData;
			tariffFindBoxAUCClass.Visible = !useCustomsReferenceData;
			if (useCustomsReferenceData)
			{
				tariffFindBox.GetCountryCode = GetCustomsCountryCode;
				tariffFindBox.GetDataGrouping = GetDataGroupingForUniversalTariff;
			}
		}

		protected string GetCustomsCountryCode() => AUCAHECCWrapper.UseCMRTariffTestData ? AUConstants.RefDataGroupCodes.AustraliaTest : Core.Constants.CountryCodes.Australia;
		protected ZString GetDataGroupingForUniversalTariff() => AUCAHECCWrapper.UseCMRTariffTestData ? AUConstants.RefDataGroupCodes.AustraliaTest : Core.Constants.CountryCodes.Australia;

		public Classification ImportClassification
		{
			get { return CurrentDataItem as Classification; }
		}

		void CMRDefaultCPDecAnswersButton_Click(object sender, System.EventArgs e)
		{
			if (ImportClassification != null)
			{
				ImportClassification.GenerateQuestion();
				ShowCPQAForm();
			}
		}

		void ShowCPQAForm()
		{
			ZFormModaliser.ShowDialogAndDispose(new CPQAClassificationForm(ImportClassification) { Owner = ParentForm });
		}
	}
}
