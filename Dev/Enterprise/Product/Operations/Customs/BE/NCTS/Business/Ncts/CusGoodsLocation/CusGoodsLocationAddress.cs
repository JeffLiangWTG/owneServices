using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.BE.NCTS.Business;

public class CusGoodsLocationAddress : EU.NCTS.Business.CusGoodsLocationAddress
{
	public CusGoodsLocationAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new CusGoodsLocation GoodsLocation => base.GoodsLocation as CusGoodsLocation;

	NctsArrivalMovementHeader ArrivalMovementHeader => GoodsLocation?.ArrivalMovementHeader;

	public override ZBool E2_AddressOverride
	{
		get => base.E2_AddressOverride;
		set
		{
			var oldValue = E2_AddressOverride;
			base.E2_AddressOverride = value;
			if (!IsCopying && oldValue != E2_AddressOverride)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZGuid E2_OA_Address
	{
		get => base.E2_OA_Address;
		set
		{
			var oldValue = E2_OA_Address;
			base.E2_OA_Address = value;
			if (!IsCopying && oldValue != E2_OA_Address)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_Contact
	{
		get => base.E2_Contact;
		set
		{
			var oldValue = E2_Contact;
			base.E2_Contact = value;
			if (!IsCopying && oldValue != E2_Contact)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_GovRegNum
	{
		get => base.E2_GovRegNum;
		set
		{
			var oldValue = E2_GovRegNum;
			base.E2_GovRegNum = value;
			if (!IsCopying && oldValue != E2_GovRegNum)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_ValidationStatus
	{
		get => base.E2_ValidationStatus;
		set
		{
			var oldValue = E2_ValidationStatus;
			base.E2_ValidationStatus = value;
			if (!IsCopying && oldValue != E2_ValidationStatus)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_RN_NKCountryCode
	{
		get => base.E2_RN_NKCountryCode;
		set
		{
			var oldValue = E2_RN_NKCountryCode;
			base.E2_RN_NKCountryCode = value;
			if (!IsCopying && oldValue != E2_RN_NKCountryCode)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_Phone
	{
		get => base.E2_Phone;
		set
		{
			var oldValue = E2_Phone;
			base.E2_Phone = value;
			if (!IsCopying && oldValue != E2_Phone)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_Fax
	{
		get => base.E2_Fax;
		set
		{
			var oldValue = E2_Fax;
			base.E2_Fax = value;
			if (!IsCopying && oldValue != E2_Fax)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_Mobile
	{
		get => base.E2_Mobile;
		set
		{
			var oldValue = E2_Mobile;
			base.E2_Mobile = value;
			if (!IsCopying && oldValue != E2_Mobile)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_Postcode
	{
		get => base.E2_Postcode;
		set
		{
			var oldValue = E2_Postcode;
			base.E2_Postcode = value;
			if (!IsCopying && oldValue != E2_Postcode)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_CompanyName
	{
		get => base.E2_CompanyName;
		set
		{
			var oldValue = E2_CompanyName;
			base.E2_CompanyName = value;
			if (!IsCopying && oldValue != E2_CompanyName)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_City
	{
		get => base.E2_City;
		set
		{
			var oldValue = E2_City;
			base.E2_City = value;
			if (!IsCopying && oldValue != E2_City)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_Address2
	{
		get => base.E2_Address2;
		set
		{
			var oldValue = E2_Address2;
			base.E2_Address2 = value;
			if (!IsCopying && oldValue != E2_Address2)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_Address1
	{
		get => base.E2_Address1;
		set
		{
			var oldValue = E2_Address1;
			base.E2_Address1 = value;
			if (!IsCopying && oldValue != E2_Address1)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_GovRegNumType
	{
		get => base.E2_GovRegNumType;
		set
		{
			var oldValue = E2_GovRegNumType;
			base.E2_GovRegNumType = value;
			if (!IsCopying && oldValue != E2_GovRegNumType)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZGeography E2_GeoLocation
	{
		get => base.E2_GeoLocation;
		set
		{
			var oldValue = E2_GeoLocation;
			base.E2_GeoLocation = value;
			if (!IsCopying && oldValue != E2_GeoLocation)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZBool E2_IsResidential
	{
		get => base.E2_IsResidential;
		set
		{
			var oldValue = E2_IsResidential;
			base.E2_IsResidential = value;
			if (!IsCopying && oldValue != E2_IsResidential)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_ScreeningStatus
	{
		get => base.E2_ScreeningStatus;
		set
		{
			var oldValue = E2_ScreeningStatus;
			base.E2_ScreeningStatus = value;
			if (!IsCopying && oldValue != E2_ScreeningStatus)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_Email
	{
		get => base.E2_Email;
		set
		{
			var oldValue = E2_Email;
			base.E2_Email = value;
			if (!IsCopying && oldValue != E2_Email)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_AddressType
	{
		get => base.E2_AddressType;
		set
		{
			var oldValue = E2_AddressType;
			base.E2_AddressType = value;
			if (!IsCopying && oldValue != E2_AddressType)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_AddressMap
	{
		get => base.E2_AddressMap;
		set
		{
			var oldValue = E2_AddressMap;
			base.E2_AddressMap = value;
			if (!IsCopying && oldValue != E2_AddressMap)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_AdditionalAddressInformation
	{
		get => base.E2_AdditionalAddressInformation;
		set
		{
			var oldValue = E2_AdditionalAddressInformation;
			base.E2_AdditionalAddressInformation = value;
			if (!IsCopying && oldValue != E2_AdditionalAddressInformation)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_ParentTableCode
	{
		get => base.E2_ParentTableCode;
		set
		{
			var oldValue = E2_ParentTableCode;
			base.E2_ParentTableCode = value;
			if (!IsCopying && oldValue != E2_ParentTableCode)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZGuid E2_ParentID
	{
		get => base.E2_ParentID;
		set
		{
			var oldValue = E2_ParentID;
			base.E2_ParentID = value;
			if (!IsCopying && oldValue != E2_ParentID)
			{
				ArrivalMovementHeader?.MarkAsNeedingValidation();
			}
		}
	}
}
