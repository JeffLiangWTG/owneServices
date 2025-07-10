using Enterprise.Customs.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class CustomsContainersWithTrackingUserControl : BaseCustomsCusContainersWithTrackingUserControl
	{
		public CustomsContainersWithTrackingUserControl()
			: base()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				SetupNewColumns();
			}
		}

		void SetupNewColumns()
		{
			ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo countryOfRegistrationDropEditColumnStyle = new ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo containerSizeZTextBoxColumnStyle = new ZArchitecture.ZTextBoxColumnStyleInfo();

			this.SuspendLayout();

			countryOfRegistrationDropEditColumnStyle.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			countryOfRegistrationDropEditColumnStyle.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ZModuleButtonGrid|650ca24d-8813-4592-9a1c-630dd3949a58", "Reg. Country/Region", "Country/Region of Container Registration", "");
			countryOfRegistrationDropEditColumnStyle.ColumnName = "CA_RN_NKCountryOfRegistration";
			this.CusContainersBoundGrid.ColumnStyles.Add(countryOfRegistrationDropEditColumnStyle);

			containerSizeZTextBoxColumnStyle.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			containerSizeZTextBoxColumnStyle.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ZModuleButtonGrid|8b9159dc-0df5-43e1-9010-b0549bee815b", "ISO Size", "Cn. ISO Size", "Container ISO Size Code", "");
			containerSizeZTextBoxColumnStyle.ColumnName = "CA_ContainerSizeOrISOCode";
			this.CusContainersBoundGrid.ColumnStyles.Add(containerSizeZTextBoxColumnStyle);

			this.ResumeLayout();
		}
	}
}
