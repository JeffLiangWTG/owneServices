using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public sealed class DeclarationOrganizationsLayout : IPanelLayoutProvider
	{
		public DeclarationOrganizationsLayout()
		{
			Layout = CreateDeclarationOrganizationsLayout();
		}

		PanelLayout CreateDeclarationOrganizationsLayout()
		{
			var builder = new DeclarationOrganizationsLayoutBuilder<Business.EMCSJobDeclaration>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.ConsignorDocAddressControl, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.ConsigneeDocAddressControl, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.OwnerDocAddressUserControl, ControlWidthClass.LongNoCaption);

			return builder.Build();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;
	}
}
