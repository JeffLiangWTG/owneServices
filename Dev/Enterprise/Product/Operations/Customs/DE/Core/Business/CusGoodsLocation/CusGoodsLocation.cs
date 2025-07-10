using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business
{
	public class CusGoodsLocation : EU.Business.CusGoodsLocation, Integration.Customs.DE.ICusGoodsLocation
	{
		public CusGoodsLocation(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override int AdditionalIdentifierMaxLength => 35;

		[MaxLength(nameof(LoadingPlaceMaxLenght))]
		[ResourceStringData("29850207-65B3-489F-809E-46F1C8A83B71", Caption = "Loading Place")]
		public ZString LoadingPlace
		{
			get => Address?.E2_CompanyName ?? ZString.Empty;
			set
			{
				if (LoadingPlace != value)
				{
					Address.E2_CompanyName = value.Left(LoadingPlaceInfo.MaxLength);
					if (!IsValidationSuspended)
					{
						Validation.ValidateLoadingPlace();
					}
					LoadingPlaceInfo.RefreshBinding();
				}
			}
		}

		int LoadingPlaceMaxLenght => Address.E2_CompanyNameInfo.MaxLength;

		public ZPropertyInfo LoadingPlaceInfo => GetZPropertyInfo(nameof(LoadingPlace));

		void SetDefaultValuesIfNeeded()
		{
			var entryInstruction = Parent as Declaration.CusEntryInstruction;
			if (entryInstruction != null)
			{
				var supplierPickupAddress = entryInstruction.JobDeclaration.SupplierPickupAddress;
				if (supplierPickupAddress != null)
				{
					var supplierAddress = Factory.Load<OrgAddress>(supplierPickupAddress.E2_OA_Address);
					if (CGL_Qualifier == Customs.Business.CusGoodsLocationQualifierList.Codes.Address
						|| CGL_Qualifier == Customs.Business.CusGoodsLocationQualifierList.Codes.GnssCoordinates)
					{
						var addressIsOverridden = supplierPickupAddress.E2_AddressOverride;

						Address.E2_CompanyName = addressIsOverridden ? supplierPickupAddress.E2_CompanyName : (supplierPickupAddress.Organisation?.OH_FullName ?? ZString.Empty);
						Address.E2_Address1 = addressIsOverridden ? supplierPickupAddress.E2_Address1 : (supplierAddress?.OA_Address1 ?? ZString.Empty);
						Address.E2_Address2 = addressIsOverridden ? supplierPickupAddress.E2_Address2 : (supplierAddress?.OA_Address2 ?? ZString.Empty);
						Address.E2_City = addressIsOverridden ? supplierPickupAddress.E2_City : (supplierAddress?.OA_City ?? ZString.Empty);
						Address.E2_Postcode = addressIsOverridden ? supplierPickupAddress.E2_Postcode : (supplierAddress?.OA_PostCode ?? ZString.Empty);
						Address.E2_RN_NKCountryCode = addressIsOverridden ? supplierPickupAddress.E2_RN_NKCountryCode : (supplierAddress?.OA_RN_NKCountryCode ?? ZString.Empty);
						Address.E2_GeoLocation = supplierPickupAddress.E2_GeoLocation;
					}
					else
					{
						Address.E2_CompanyName = ZString.Empty;
						Address.E2_Address1 = ZString.Empty;
						Address.E2_Address2 = ZString.Empty;
						Address.E2_City = ZString.Empty;
						Address.E2_Postcode = ZString.Empty;
						Address.E2_RN_NKCountryCode = ZString.Empty;
						Address.E2_GeoLocation = ZGeography.Empty;
					}

					if (CGL_Qualifier == Customs.Business.CusGoodsLocationQualifierList.Codes.UnLocode)
					{
						CGL_AdditionalIdentifier = supplierAddress?.OA_RL_NKRelatedPortCode ?? ZString.Empty;
					}
					else
					{
						CGL_AdditionalIdentifier = ZString.Empty;
					}
				}
			}
		}

		public override ZString CGL_Qualifier
		{
			get => base.CGL_Qualifier;
			set
			{
				var oldValue = CGL_Qualifier;
				base.CGL_Qualifier = value;
				if (!IsCopying && oldValue != CGL_Qualifier)
				{
					SetDefaultValuesIfNeeded();
					SetTypeOfLocation(value);
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusGoodsLocationLookups.AdditionalIdentifierList))]
		public override ZString CGL_AdditionalIdentifier
		{
			get => base.CGL_AdditionalIdentifier;
			set => base.CGL_AdditionalIdentifier = value;
		}

		void SetTypeOfLocation(string qualifierOfIdentification)
		{
			string result = string.Empty;
			switch (qualifierOfIdentification)
			{
				case Customs.Business.CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier:
					result = "A";
					break;
				case Customs.Business.CusGoodsLocationQualifierList.Codes.AuthorizationNumber:
					result = "B";
					break;
				case Customs.Business.CusGoodsLocationQualifierList.Codes.UnLocode:
				case Customs.Business.CusGoodsLocationQualifierList.Codes.GnssCoordinates:
				case Customs.Business.CusGoodsLocationQualifierList.Codes.Address:
					result = "D";
					break;
			}
			CGL_Type = result;
		}

		protected override Type AddressType => typeof(CusGoodsLocationAddress);

		protected override EU.Business.CusGoodsLocationAddress LoadOrCreateCusGoodsLocationAddress() => (CusGoodsLocationAddress)base.LoadOrCreateCusGoodsLocationAddress();

		protected override Customs.Business.CusGoodsLocationLookups GetNewLookups() => new CusGoodsLocationLookups(this);

		public new CusGoodsLocationLookups Lookups => (CusGoodsLocationLookups)base.Lookups;

		protected override Customs.Business.CusGoodsLocationValidation GetNewValidation() => new CusGoodsLocationValidation(this);

		public new CusGoodsLocationValidation Validation => (CusGoodsLocationValidation)base.Validation;
	}
}
