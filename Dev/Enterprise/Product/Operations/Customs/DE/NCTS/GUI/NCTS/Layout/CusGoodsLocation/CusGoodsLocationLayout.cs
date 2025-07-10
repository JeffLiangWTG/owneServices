using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;
using CusGoodsLocation = Enterprise.Customs.DE.NCTS.Business.CusGoodsLocation;

namespace Enterprise.Customs.DE.NCTS.GUI
{
	public class CusGoodsLocationLayout : IPanelLayoutProviderWithExtensions
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
			var deBag = CusGoodsLocationControlBag.Instance;

			builder.AddControlBag(deBag);

			builder.AddColumn();
			builder.Add(commonBag.QualifierDropEdit, ControlWidthClass.Long);
			builder.Add(deBag.OrganisationFindBox, ControlWidthClass.Long);
			builder.Add(deBag.AuthorizationCodeFindBox, ControlWidthClass.Long);
			builder.Add(deBag.AdditionalIdentifierDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.ContactTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.PhoneTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.EmailTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.UnlocoCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.GeoLocationLatitudeTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.GeoLocationLongitudeTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.StreetAndNumberWithAddressValidationUserControl, ControlWidthClass.Long);
			builder.Add(commonBag.CityTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.PostcodeTextBox, ControlWidthClass.Long);

			builder.SetVisibility(deBag.OrganisationFindBox, (x) => x.ParentIsArrivalMovementHeader && x.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber, x => x.CGL_QualifierInfo, x => x.CGL_TypeInfo);
			builder.SetVisibility(deBag.AuthorizationCodeFindBox, (x) => x.ParentIsArrivalMovementHeader && x.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber, x => x.CGL_QualifierInfo, x => x.CGL_TypeInfo);
			builder.SetControlVisibility(deBag.AdditionalIdentifierDropEdit, CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
			builder.SetControlVisibility(commonBag.ContactTextBox, CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
			builder.SetControlVisibility(commonBag.PhoneTextBox, CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
			builder.SetControlVisibility(commonBag.EmailTextBox, CusGoodsLocationQualifierList.Codes.AuthorizationNumber);
			builder.SetControlVisibility(commonBag.StreetAndNumberWithAddressValidationUserControl, CusGoodsLocationQualifierList.Codes.Address);
			builder.SetControlVisibility(commonBag.PostcodeTextBox, CusGoodsLocationQualifierList.Codes.Address);

			return builder.Build();
		}
	}
}
