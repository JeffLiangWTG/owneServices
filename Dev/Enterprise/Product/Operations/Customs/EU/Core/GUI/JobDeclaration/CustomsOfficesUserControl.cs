using System.Linq;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class CustomsOfficesUserControl : ZUserControl
	{
		public CustomsOfficesUserControl()
		{
			InitializeComponent();
		}

		public virtual void HandleDeclarationControlVisibilityChanged()
		{
			CustomsOfficesGrid.Visible = CustomsOfficesGridVisible;
			CustomsOfficeFindBox.Visible = Helper?.MainOffice != null;
			if (IsOverrideLabelByFriendlyName)
			{
				CustomsOfficeFindBox.Extensions.Get<ILabelCaptionRenderer>().Caption = Helper?.MainOffice?.FriendlyName ?? Res.GetString("dc90deb3-1c45-44a7-87a1-f8e1d466a61b", "Customs Office");
			}
		}

		public bool IsOverrideLabelByFriendlyName => IsOverrideLabelByFriendlyNameCore;

		protected virtual bool IsOverrideLabelByFriendlyNameCore => true;

		public JobDeclaration JobDeclaration => (JobDeclaration)CurrentDataItem;

		Business.CustomsOfficeRequirementHelper Helper => JobDeclaration?.CustomsOfficeRequirementHelper;

		protected virtual bool CustomsOfficesGridVisible => Helper?.OtherRequirements.Any() ?? false;
	}
}
