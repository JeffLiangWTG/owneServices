using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusGoodsLocationAddress : EU.Business.CusGoodsLocationAddress, ISupportMultipleResourceStringData
	{
		public CusGoodsLocationAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new CusGoodsLocationAddressValidation Validation => (CusGoodsLocationAddressValidation)base.Validation;

		protected override JobDocAddressValidation GetNewValidation() => new CusGoodsLocationAddressValidation(this);

		public new CusGoodsLocation GoodsLocation => base.GoodsLocation as CusGoodsLocation;

		NctsCommonMovementHeader DepartureMovementHeader => GoodsLocation?.DepartureMovementHeader;

		protected ZBool IsPhase5 => DepartureMovementHeader?.IsPhase5 ?? false;

		public override ZBool E2_AddressOverride
		{
			get => base.E2_AddressOverride;
			set
			{
				base.E2_AddressOverride = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid E2_OA_Address
		{
			get => base.E2_OA_Address;
			set
			{
				base.E2_OA_Address = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		[MaxLength(70)]
		public override ZString E2_Contact
		{
			get => base.E2_Contact;
			set
			{
				base.E2_Contact = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("02343635-3B9D-4BE8-90BE-389B57C97562", Caption = "EORI Number", MultipleKey = CusGoodsLocationQualifierList.Codes.EoriNumber)]
		[ResourceStringData("13DC7123-85F8-468C-B46F-14B30628CD02", Caption = "Authorization Number", MultipleKey = CusGoodsLocationQualifierList.Codes.AuthorizationNumber)]
		public override ZString E2_GovRegNum
		{
			get => base.E2_GovRegNum;
			set
			{
				base.E2_GovRegNum = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid IdentificationHolderPK
		{
			get
			{
				var result = base.IdentificationHolderPK;
				if (result.IsEmpty)
				{
					result = GoodsLocation?.Header?.Consignor?.Organisation?.PK ?? ZGuid.Empty;
				}
				return result;
			}
			set
			{
				base.IdentificationHolderPK = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString E2_ValidationStatus
		{
			get => base.E2_ValidationStatus;
			set
			{
				base.E2_ValidationStatus = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString E2_RN_NKCountryCode
		{
			get => base.E2_RN_NKCountryCode;
			set
			{
				base.E2_RN_NKCountryCode = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString E2_Phone
		{
			get => base.E2_Phone;
			set
			{
				base.E2_Phone = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString E2_Fax
		{
			get => base.E2_Fax;
			set
			{
				base.E2_Fax = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString E2_Mobile
		{
			get => base.E2_Mobile;
			set
			{
				base.E2_Mobile = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString E2_Postcode
		{
			get => base.E2_Postcode;
			set
			{
				base.E2_Postcode = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString E2_CompanyName
		{
			get => base.E2_CompanyName;
			set
			{
				base.E2_CompanyName = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString E2_City
		{
			get => base.E2_City;
			set
			{
				base.E2_City = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString E2_Address2
		{
			get => base.E2_Address2;
			set
			{
				base.E2_Address2 = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString E2_Address1
		{
			get => base.E2_Address1;
			set
			{
				base.E2_Address1 = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString E2_GovRegNumType
		{
			get => base.E2_GovRegNumType;
			set
			{
				base.E2_GovRegNumType = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGeography E2_GeoLocation
		{
			get => base.E2_GeoLocation;
			set
			{
				base.E2_GeoLocation = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZBool E2_IsResidential
		{
			get => base.E2_IsResidential;
			set
			{
				base.E2_IsResidential = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString E2_ScreeningStatus
		{
			get => base.E2_ScreeningStatus;
			set
			{
				base.E2_ScreeningStatus = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString E2_Email
		{
			get => base.E2_Email;
			set
			{
				base.E2_Email = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString E2_AddressType
		{
			get => base.E2_AddressType;
			set
			{
				base.E2_AddressType = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString E2_AddressMap
		{
			get => base.E2_AddressMap;
			set
			{
				base.E2_AddressMap = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString E2_AdditionalAddressInformation
		{
			get => base.E2_AdditionalAddressInformation;
			set
			{
				base.E2_AdditionalAddressInformation = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString E2_ParentTableCode
		{
			get => base.E2_ParentTableCode;
			set
			{
				base.E2_ParentTableCode = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		public override ZGuid E2_ParentID
		{
			get => base.E2_ParentID;
			set
			{
				base.E2_ParentID = value;
				if (IsPhase5)
				{
					DepartureMovementHeader?.MarkAsNeedingValidation();
				}
			}
		}

		protected override bool AuthorisationNumberReadOnly => base.AuthorisationNumberReadOnly
			&& GoodsLocation.CGL_Qualifier != CusGoodsLocationQualifierList.Codes.AuthorizationNumber;

		IReadOnlyList<string> ISupportMultipleResourceStringData.MultipleKeysToUse => GoodsLocation is CusGoodsLocation goodsLocation ? new[] { goodsLocation.CGL_Qualifier.ToString() } : Array.Empty<string>();
	}
}
