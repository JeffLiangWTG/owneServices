using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public class TemporaryStorageHeaderGuaranteeValidation : CommonGuaranteeValidation
{
	public TemporaryStorageHeaderGuaranteeValidation(TemporaryStorageHeaderGuarantee parent) : base(parent)
	{
	}

	protected new TemporaryStorageHeaderGuarantee Parent => (TemporaryStorageHeaderGuarantee)base.Parent;

	protected override void CheckPW_BondNumber()
	{
		base.CheckPW_BondNumber();
		CheckBondNumberAndBondAmountEmpty(Parent.PW_BondNumber.IsEmpty, Parent.PW_BondNumberInfo);
	}

	protected override void CheckPW_BondAmount()
	{
		base.CheckPW_BondAmount();
		CheckBondNumberAndBondAmountEmpty(Parent.PW_BondAmount.IsEmpty, Parent.PW_BondAmountInfo);
	}

	void CheckBondNumberAndBondAmountEmpty(ZBool guaranteeFieldEmpty, ZPropertyInfo guaranteeFieldInfo)
	{
		var guarantee = Parent;
		var header = guarantee.TemporaryStorageHeader;
		var destinationGoodsLocation = header?.DestinationGoodsLocation?.Address?.AuthorisationNumber ?? ZString.Empty;
		var premises = EU.Business.TemporaryStorageHelper.GetManagedPremises(guarantee.Factory, CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse, destinationGoodsLocation);
		if (header != null && header.IsMessageTypeG5V1Expedition && guaranteeFieldEmpty && premises != null && IsDeclarantAndConsigneeEquals())
		{
			guaranteeFieldInfo.AddMessageError(BondNumberAndBondAmountEmptyError);
		}

		bool IsDeclarantAndConsigneeEquals()
		{
			var declarant = header.Declarant;
			var consignee = header.Bills.FirstOrDefault()?.Consignee;
			if (declarant != null && consignee != null)
			{
				return declarant.Header.OH_Code == consignee.Header.OH_Code;
			}
			else
			{
				return false;
			}
		}
	}

	static string BondNumberAndBondAmountEmptyError => Res.GetString("0778BE9D-43FB-4A6E-A3F6-CD35AB307F0C", "For G5 Expedition declarations where the Declarant is the owner of the Destination Goods Location, a Guarantee Reference Number and Liability Amount must be supplied.");
}
