using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public class TemporaryStorageBillValidation : EU.Business.CusTempStorage.TemporaryStorageBillValidation
{
	public TemporaryStorageBillValidation(AutoAsycudaBill parent) : base(parent)
	{
	}

	public new TemporaryStorageBill Parent => (TemporaryStorageBill)base.Parent;

	protected override void CheckTypeOfBillDocument()
	{
		base.CheckTypeOfBillDocument();

		if (CheckAnyTRAPrevDocInItems() && !Parent.TypeOfBillDocument.IsEmpty)
		{
			Parent.TypeOfBillDocumentInfo.AddWarning(Res.GetString("B71E6AF0-4EEC-4469-B0C1-185AE8A0C5AE", "Transport documents must be declared either in Header or in Items, but not in both."));
		}
	}

	bool CheckAnyTRAPrevDocInItems() => Parent.PackedItems.Cast<TemporaryStoragePackedItem>().Any(x => x.AdditionalInfos.Cast<TemporaryStorageAdditionalInfo>().Any(y => y.CSI_SubType == AdditionalDocList.Codes.TransportDocuments));

	protected override void CheckConsignorOrgPK()
	{
		base.CheckConsignorOrgPK();

		var parent = Parent;

		if (parent.Header.IsMessageTypeLAM && parent.ABL_OA_Shipper.IsEmpty)
		{
			parent.ConsignorOrgPKInfo.AddWarning(ConsignorEORIWarning);
		}
	}

	protected override void CheckABL_ShipperName()
	{
		base.CheckABL_ShipperName();
		AddConsignorEORIWarningIfFieldEmpty(Parent.ABL_ShipperName, Parent.ABL_ShipperNameInfo);
	}

	protected override void CheckABL_RN_NKShipperCountry()
	{
		base.CheckABL_RN_NKShipperCountry();
		AddConsignorEORIWarningIfFieldEmpty(Parent.ABL_RN_NKShipperCountry, Parent.ABL_RN_NKShipperCountryInfo);
	}

	protected override void CheckABL_ShipperPostcode()
	{
		base.CheckABL_ShipperPostcode();
		AddConsignorEORIWarningIfFieldEmpty(Parent.ABL_ShipperPostcode, Parent.ABL_ShipperPostcodeInfo);
	}

	protected override void CheckABL_ShipperRegNoType()
	{
		base.CheckABL_ShipperRegNoType();
		AddConsignorEORIWarningIfFieldEmpty(Parent.ABL_ShipperRegNoType, Parent.ABL_ShipperRegNoTypeInfo);
	}

	protected override void CheckConsigneeOrgPK()
	{
		base.CheckConsigneeOrgPK();

		var parent = Parent;

		if (parent.Header.IsMessageTypeTSM && parent.ABL_OA_Consignee.IsEmpty)
		{
			parent.ConsigneeOrgPKInfo.AddWarning(ConsigneeEORIWarning);
		}
	}

	protected override void CheckABL_ConsigneeName()
	{
		base.CheckABL_ConsigneeName();
		AddConsigneeEORIWarningIfFieldEmpty(Parent.ABL_ConsigneeName, Parent.ABL_ConsigneeNameInfo);
	}

	protected override void CheckABL_RN_NKConsigneeCountry()
	{
		base.CheckABL_RN_NKConsigneeCountry();
		AddConsigneeEORIWarningIfFieldEmpty(Parent.ABL_RN_NKConsigneeCountry, Parent.ABL_RN_NKConsigneeCountryInfo);
	}

	protected override void CheckABL_ConsigneePostcode()
	{
		base.CheckABL_ConsigneePostcode();
		AddConsigneeEORIWarningIfFieldEmpty(Parent.ABL_ConsigneePostcode, Parent.ABL_ConsigneePostcodeInfo);
	}

	protected override void CheckABL_ConsigneeRegNoType()
	{
		base.CheckABL_ConsigneeRegNoType();
		AddConsigneeEORIWarningIfFieldEmpty(Parent.ABL_ConsigneeRegNoType, Parent.ABL_ConsigneeRegNoTypeInfo);
	}

	void AddConsignorEORIWarningIfFieldEmpty(ZString fieldToCheck, ZPropertyInfo targetInfo)
	{
		var parent = Parent;
		if (parent.Header.IsMessageTypeLAM && fieldToCheck.IsEmpty)
		{
			targetInfo.AddWarning(ConsignorEORIWarning);
		}
	}

	void AddConsigneeEORIWarningIfFieldEmpty(ZString fieldToCheck, ZPropertyInfo targetInfo)
	{
		var parent = Parent;
		if (parent.Header.IsMessageTypeTSM && fieldToCheck.IsEmpty)
		{
			targetInfo.AddWarning(ConsigneeEORIWarning);
		}
	}

	public static string ConsignorEORIWarning => Res.GetString("F1C89DA5-5A1C-4448-AFE2-980CFBCE1E43", "Consignor should be filled to set its EORI to the Owner ID of the lines entered in the Temporary Storage Register.");

	public static string ConsigneeEORIWarning => Res.GetString("6BBB7CB8-1EE8-46F1-AC47-F42F9789E25B", "Consignee should be filled to set its EORI to the Owner ID of the lines entered in the Temporary Storage Register.");
}