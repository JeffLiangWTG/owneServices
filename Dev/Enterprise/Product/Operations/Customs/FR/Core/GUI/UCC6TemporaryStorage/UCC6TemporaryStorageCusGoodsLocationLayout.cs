using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.UCC6TemporaryStorage
{
	internal class UCC6TemporaryStorageCusGoodsLocationLayout : IPanelLayoutProviderWithExtensions
	{
		public UCC6TemporaryStorageCusGoodsLocationLayout()
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
			var builder = new CusGoodsLocationLayoutBuilder<EU.Business.CusGoodsLocation>();
			var commonBag = builder.CommonBag;

			builder.AddColumn();
			builder.Add(commonBag.QualifierDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.TypeDropEdit, ControlWidthClass.Long);
			builder.Add(commonBag.OrganisationFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.EoriNumberTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.AuthorizationCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.AdditionalIdentifierTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.StreetAndNumberWithAddressValidationUserControl, ControlWidthClass.Long);
			builder.Add(commonBag.CityTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.PostcodeTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.CountryCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.UnlocoCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.CustomsOfficeCodeFindBox, ControlWidthClass.Long);
			builder.Add(commonBag.GeoLocationLatitudeTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.GeoLocationLongitudeTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.ContactTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.PhoneTextBox, ControlWidthClass.Long);
			builder.Add(commonBag.EmailTextBox, ControlWidthClass.Long);

			SetControlVisibilityShouldBeVisibleDependingOnType(commonBag.AdditionalIdentifierTextBox, AdditionalIdentifierTextBoxDictionnaryOfValidScenariiForAdditionalIdentifierTextBox(), builder);

			builder.SetControlVisibility(commonBag.StreetAndNumberWithAddressValidationUserControl,
				CusGoodsLocationQualifierList.Codes.Address,
				CusGoodsLocationQualifierList.Codes.PostcodeAddress);

			builder.SetCaption(commonBag.StreetAndNumberWithAddressValidationUserControl, GetStreetAndNumberTextBoxCaption, l => l.CGL_QualifierInfo);

			return builder.Build();
		}

		Dictionary<ZString, ZString> AdditionalIdentifierTextBoxDictionnaryOfValidScenariiForAdditionalIdentifierTextBox()
		{
			var result = new Dictionary<ZString, ZString>();
			result.Add(CusGoodsLocationQualifierList.Codes.EoriNumber, ZString.Empty);
			result.Add(CusGoodsLocationQualifierList.Codes.AuthorizationNumber, ZString.Empty);
			result.Add(CusGoodsLocationQualifierList.Codes.PostcodeAddress, ZString.Empty);
			result.Add(CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier, CusGoodsLocationTypeList.Codes.DesignatedLocation);
			return result;
		}
	
		void SetControlVisibilityShouldBeVisibleDependingOnType(ControlReference control, Dictionary<ZString, ZString> pairOfQualifierAndType, CusGoodsLocationLayoutBuilder<EU.Business.CusGoodsLocation> builder)
		{
			builder.SetVisibility(control, l => l.CGL_Qualifier.In(pairOfQualifierAndType.Keys) && pairOfQualifierAndType.TryGetValue(l.CGL_Qualifier, out ZString type) && (type.IsEmpty || l.CGL_Type == type), GetvisibilityDependencies());
		}

		Func<CusGoodsLocation, ZPropertyInfo>[] GetvisibilityDependencies() =>
			new Func<CusGoodsLocation, ZPropertyInfo>[]
			{
				goods => goods.CGL_QualifierInfo, goods => goods.CGL_TypeInfo
			};

		ResourceStringData GetStreetAndNumberTextBoxCaption(CusGoodsLocation location)
		{
			switch (location.CGL_Qualifier)
			{
				case CusGoodsLocationQualifierList.Codes.PostcodeAddress:
					return Res.GetData("539FF835-18B6-4565-B710-A59EEC57D949", "House Number");
				case CusGoodsLocationQualifierList.Codes.Address:
					return Res.GetData("43A0AD8F-EB93-4AE0-8AF3-1A0DCAF38054", "Street + Number");
				default:
					return new ResourceStringData();
			}
		}
	}
}
