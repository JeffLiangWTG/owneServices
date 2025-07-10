using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using QualifierList = Enterprise.Customs.Business.CusGoodsLocationQualifierList;

namespace Enterprise.Customs.IT.NCTS.GUI;

sealed class CusGoodsLocationLayout : IPanelLayoutProviderWithExtensions
{
	public CusGoodsLocationLayout()
	{
		Layout = CreateCusGoodsLocationLayout();
	}

	PanelLayout Layout { get; }

	PanelLayout IPanelLayoutProvider.Layout => Layout;

	#region IPanelLayoutProviderWithExtensions

	IReadOnlyCollection<ILayoutExtension> IPanelLayoutProviderWithExtensions.Extensions => new[]
	{
		new CusGoodsLocationWebAddressValidationExtension()
	};

	#endregion

	PanelLayout CreateCusGoodsLocationLayout()
	{
		var builder = new CusGoodsLocationLayoutBuilder<CusGoodsLocation>();
		var commonBag = builder.CommonBag;
		var itBag = IT.GUI.CusGoodsLocationControlBag.Instance;

		builder.AddControlBag(itBag);
		builder.AddColumn();

		builder.Add(commonBag.QualifierDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.TypeDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.CustomsOfficeCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.OrganisationFindBox, ControlWidthClass.Auto);
		builder.Add(itBag.OrganizationAddressControl, ControlWidthClass.Long);
		builder.Add(itBag.OverrideCheckBox, ControlWidthClass.Long);
		builder.Add(commonBag.AuthorizationCodeFindBox, ControlWidthClass.Auto);
		builder.Add(itBag.AdditionalIdentifierDropEdit, ControlWidthClass.Auto);
		builder.Add(commonBag.StreetAndNumberWithAddressValidationUserControl, ControlWidthClass.Auto);
		builder.Add(commonBag.CityTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.PostcodeTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.CountryCodeFindBox, ControlWidthClass.Auto);
		builder.Add(commonBag.ContactTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.PhoneTextBox, ControlWidthClass.Auto);
		builder.Add(commonBag.EmailTextBox, ControlWidthClass.Auto);

		builder.AddControlBehaviour<ZDropEdit>(commonBag.QualifierDropEdit, UpdateControlBehaviourAction);
		builder.AddControlBehaviour<ZDropEdit>(commonBag.TypeDropEdit, UpdateControlBehaviourAction);
		builder.AddControlBehaviour<ZCodeFindBox>(commonBag.CustomsOfficeCodeFindBox, UpdateControlBehaviourAction);
		builder.AddControlBehaviour<ZOrganisationFindBox>(commonBag.OrganisationFindBox, UpdateControlBehaviourAction);
		builder.AddControlBehaviour<ZCodeFindBox>(commonBag.AuthorizationCodeFindBox, UpdateControlBehaviourAction);
		builder.AddControlBehaviour<ZDropEdit>(itBag.AdditionalIdentifierDropEdit, UpdateControlBehaviourAction);
		builder.AddControlBehaviour<StreetAndNumberWithAddressValidationControl>(commonBag.StreetAndNumberWithAddressValidationUserControl, UpdateControlBehaviourAction);
		builder.AddControlBehaviour<ZTextBox>(commonBag.PostcodeTextBox, UpdateControlBehaviourAction);
		builder.AddControlBehaviour<ZCodeFindBox>(commonBag.CountryCodeFindBox, UpdateControlBehaviourAction);
		builder.AddControlBehaviour<ZTextBox>(commonBag.ContactTextBox, UpdateControlBehaviourAction);
		builder.AddControlBehaviour<ZTextBox>(commonBag.PhoneTextBox, UpdateControlBehaviourAction);
		builder.AddControlBehaviour<ZTextBox>(commonBag.EmailTextBox, UpdateControlBehaviourAction);
		builder.AddControlBehaviour<ZTextBox>(commonBag.CityTextBox, UpdateControlBehaviourAction);

		builder.SetControlVisibility(commonBag.OrganisationFindBox, QualifierList.Codes.AuthorizationNumber);
		builder.SetControlVisibility(itBag.OrganizationAddressControl, QualifierList.Codes.Address);
		builder.SetControlVisibility(itBag.OverrideCheckBox, QualifierList.Codes.Address);
		builder.SetControlVisibility(itBag.AdditionalIdentifierDropEdit, QualifierList.Codes.AuthorizationNumber);
		builder.SetControlVisibility(commonBag.CountryCodeFindBox, QualifierList.Codes.Address);
		builder.SetControlVisibility(commonBag.ContactTextBox, QualifierList.Codes.AuthorizationNumber, QualifierList.Codes.Address);
		builder.SetControlVisibility(commonBag.PhoneTextBox, QualifierList.Codes.AuthorizationNumber, QualifierList.Codes.Address);
		builder.SetControlVisibility(commonBag.EmailTextBox, QualifierList.Codes.AuthorizationNumber, QualifierList.Codes.Address);

		return builder.Build();
	}

	const int OrganizationAddressControlWidth = 797;

	void UpdateControlBehaviourAction(Control control, CusGoodsLocation cusGoodsLocation)
	{
		control.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.MarkAsScaled(OrganizationAddressControlWidth);
	}
}
