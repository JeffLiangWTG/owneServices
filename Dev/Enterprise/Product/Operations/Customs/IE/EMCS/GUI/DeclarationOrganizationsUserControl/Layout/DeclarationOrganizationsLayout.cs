using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.EU.EMCS.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IE.EMCS.GUI
{
	public sealed class DeclarationOrganizationsLayout : IPanelLayoutProvider
	{
		public DeclarationOrganizationsLayout()
		{
			Layout = CreateDeclarationMiscDetailsLayout();
		}

		PanelLayout CreateDeclarationMiscDetailsLayout()
		{
			var builder = new DeclarationOrganizationsLayoutBuilder<EMCSJobDeclaration>();
			var commonBag = builder.CommonBag;
			var ieBag = DeclarationOrganizationControlBag.Instance;
			builder.AddControlBag(ieBag);

			builder.AddColumn();
			builder.Add(ieBag.CertificateIdentifierGroupBox, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.ConsignorDocAddressControl, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.ConsigneeDocAddressControl, ControlWidthClass.LongNoCaption);
			builder.Add(commonBag.OwnerDocAddressUserControl, ControlWidthClass.LongNoCaption);

			return builder.Build();
		}

		PanelLayout Layout { get; }

		PanelLayout IPanelLayoutProvider.Layout => Layout;
	}
}
