using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.ExitControl.Business;

public class CusGoodsLocationAddress : EU.Business.CusGoodsLocationAddress
{
	public CusGoodsLocationAddress(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new CusGoodsLocation GoodsLocation => base.GoodsLocation as CusGoodsLocation;

	public CusExitReport CusExitReport => GoodsLocation?.CusExitReport;

	public ZBool IsUCC6 => CusExitReport?.IsUCC6 ?? false;

	public override ZBool E2_AddressOverride
	{
		get => base.E2_AddressOverride;
		set
		{
			base.E2_AddressOverride = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZGuid E2_OA_Address
	{
		get => base.E2_OA_Address;
		set
		{
			base.E2_OA_Address = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_Contact
	{
		get => base.E2_Contact;
		set
		{
			base.E2_Contact = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_GovRegNum
	{
		get => base.E2_GovRegNum;
		set
		{
			base.E2_GovRegNum = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_ValidationStatus
	{
		get => base.E2_ValidationStatus;
		set
		{
			base.E2_ValidationStatus = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_RN_NKCountryCode
	{
		get => base.E2_RN_NKCountryCode;
		set
		{
			base.E2_RN_NKCountryCode = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_Phone
	{
		get => base.E2_Phone;
		set
		{
			base.E2_Phone = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_Fax
	{
		get => base.E2_Fax;
		set
		{
			base.E2_Fax = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_Mobile
	{
		get => base.E2_Mobile;
		set
		{
			base.E2_Mobile = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_Postcode
	{
		get => base.E2_Postcode;
		set
		{
			base.E2_Postcode = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_CompanyName
	{
		get => base.E2_CompanyName;
		set
		{
			base.E2_CompanyName = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_City
	{
		get => base.E2_City;
		set
		{
			base.E2_City = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_Address2
	{
		get => base.E2_Address2;
		set
		{
			base.E2_Address2 = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_Address1
	{
		get => base.E2_Address1;
		set
		{
			base.E2_Address1 = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_GovRegNumType
	{
		get => base.E2_GovRegNumType;
		set
		{
			base.E2_GovRegNumType = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZGeography E2_GeoLocation
	{
		get => base.E2_GeoLocation;
		set
		{
			base.E2_GeoLocation = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZBool E2_IsResidential
	{
		get => base.E2_IsResidential;
		set
		{
			base.E2_IsResidential = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_ScreeningStatus
	{
		get => base.E2_ScreeningStatus;
		set
		{
			base.E2_ScreeningStatus = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_Email
	{
		get => base.E2_Email;
		set
		{
			base.E2_Email = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_AddressType
	{
		get => base.E2_AddressType;
		set
		{
			base.E2_AddressType = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_AddressMap
	{
		get => base.E2_AddressMap;
		set
		{
			base.E2_AddressMap = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_AdditionalAddressInformation
	{
		get => base.E2_AdditionalAddressInformation;
		set
		{
			base.E2_AdditionalAddressInformation = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString E2_ParentTableCode
	{
		get => base.E2_ParentTableCode;
		set
		{
			base.E2_ParentTableCode = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZGuid E2_ParentID
	{
		get => base.E2_ParentID;
		set
		{
			base.E2_ParentID = value;
			if (!IsCopying && IsUCC6)
			{
				CusExitReport?.MarkAsNeedingValidation();
			}
		}
	}
}
