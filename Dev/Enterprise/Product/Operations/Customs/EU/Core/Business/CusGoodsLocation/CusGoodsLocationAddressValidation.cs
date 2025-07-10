using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business
{
	public class CusGoodsLocationAddressValidation : JobDocAddressValidation
	{
		public CusGoodsLocationAddressValidation(CusGoodsLocationAddress parent)
			: base(parent)
		{
		}

		new CusGoodsLocationAddress Parent => (CusGoodsLocationAddress)base.Parent;
		CusGoodsLocation GoodsLocation => Parent.GoodsLocation;

		protected override void CheckE2_Email()
		{
			new AddressValidation().CheckEmail(Parent.E2_EmailInfo);
		}

		protected override void CheckE2_GovRegNum()
		{
			base.CheckE2_GovRegNum();

			if (Parent.E2_GovRegNum.IsEmpty && GoodsLocation is CusGoodsLocation goodsLocation)
			{
				var qualifier = goodsLocation.CGL_Qualifier.ToUpperInvariant();
				switch (qualifier)
				{
					case CusGoodsLocationQualifierList.Codes.AuthorizationNumber:
						if (ApplyC0065Rule)
						{
							Parent.E2_GovRegNumInfo.AddMessageError(CusGoodsLocationValidationHelper.YouHaveNotEnteredMessageWithQualifier(Res.GetString("DAEB6319-8DF1-4A54-839A-5BA1C34859C1", "Location: Authorization No."), qualifier, ValidationRuleCodeConstants.Codes.C0065));
						}
						break;
					case CusGoodsLocationQualifierList.Codes.EoriNumber:
						Parent.E2_GovRegNumInfo.AddMessageError(CusGoodsLocationValidationHelper.YouHaveNotEnteredMessageWithQualifier(Parent.E2_GovRegNumInfo.HumanReadableName, qualifier, ValidationRuleCodeConstants.Codes.C0064));
						break;
				}
			}
		}

		protected override void CheckE2_AdditionalAddressInformation()
		{
			base.CheckE2_AdditionalAddressInformation();

			if ((Parent.E2_AdditionalAddressInformation.IsEmpty || Parent.E2_AdditionalAddressInformation == ZGuid.Empty.ToString())
				&& GoodsLocation is CusGoodsLocation goodsLocation && goodsLocation.Parent is TemporaryStorageHeader)
			{
				var qualifier = goodsLocation.CGL_Qualifier.ToUpperInvariant();
				switch (qualifier)
				{
					case CusGoodsLocationQualifierList.Codes.AuthorizationNumber:
						if (ApplyC0065Rule)
						{
							Parent.E2_AdditionalAddressInformationInfo.AddMessageError(CusGoodsLocationValidationHelper.YouHaveNotEnteredMessageWithQualifier(LocationOrganizationPropertyDescription, qualifier, ValidationRuleCodeConstants.Codes.C0065));
						}
						break;
					case CusGoodsLocationQualifierList.Codes.EoriNumber:
						Parent.E2_AdditionalAddressInformationInfo.AddMessageError(CusGoodsLocationValidationHelper.YouHaveNotEnteredMessageWithQualifier(LocationOrganizationPropertyDescription, qualifier, ValidationRuleCodeConstants.Codes.C0064));
						break;
				}
			}
		}
		protected virtual bool ApplyC0065Rule => true;

		string LocationOrganizationPropertyDescription => Res.GetString("7EDB3EA4-1738-471B-9741-6B5D1CB34ADD", "Location: Organization");

		protected override void CheckE2_GeoLocation()
		{
			base.CheckE2_GeoLocation();

			var parent = Parent;
			ValidateCalculatedProperty(parent.E2_LatitudeInfo);
			ValidateCalculatedProperty(parent.E2_LongitudeInfo);
		}

		protected virtual void CheckE2_Latitude()
		{
			var parent = Parent;
			if (parent.E2_Latitude.IsEmpty
				&& GoodsLocation is CusGoodsLocation goodsLocation
				&& goodsLocation.CGL_Qualifier.EqualsIgnoringCase(CusGoodsLocationQualifierList.Codes.GnssCoordinates))
			{
				parent.E2_LatitudeInfo.AddMessageError(CusGoodsLocationValidationHelper.YouHaveNotEnteredMessageWithQualifier(Parent.E2_LatitudeInfo.HumanReadableName, GoodsLocation.CGL_Qualifier, ValidationRuleCodeConstants.Codes.C0063, goodsLocation.Validation.IncludeRuleCode));
			}
		}

		protected virtual void CheckE2_Longitude()
		{
			var parent = Parent;
			if (parent.E2_Longitude.IsEmpty
				&& GoodsLocation is CusGoodsLocation goodsLocation
				&& goodsLocation.CGL_Qualifier.EqualsIgnoringCase(CusGoodsLocationQualifierList.Codes.GnssCoordinates))
			{
				parent.E2_LongitudeInfo.AddMessageError(CusGoodsLocationValidationHelper.YouHaveNotEnteredMessageWithQualifier(Parent.E2_LongitudeInfo.HumanReadableName, GoodsLocation.CGL_Qualifier, ValidationRuleCodeConstants.Codes.C0063, goodsLocation.Validation.IncludeRuleCode));
			}
		}

		protected override void CheckE2_Postcode()
		{
			base.CheckE2_Postcode();

			if (GoodsLocation is CusGoodsLocation goodsLocation)
			{
				if (goodsLocation.CGL_Qualifier.EqualsIgnoringCase(CusGoodsLocationQualifierList.Codes.Address))
				{
					var parent = Parent;
					var info = parent.E2_PostcodeInfo;
					var country = parent.Country ?? GlbBranch.CurrentBranch.Country;
					AddressValidation.CheckPostCode(info, country, parent.ValidationSection, parent.ValidationStatus);
				}
			}
		}

		protected override bool IsAddressOverridenValidationEnabled => false;

		AddressValidation AddressValidation => addressValidation ?? (addressValidation = new AddressValidation());
		AddressValidation addressValidation;
	}
}
