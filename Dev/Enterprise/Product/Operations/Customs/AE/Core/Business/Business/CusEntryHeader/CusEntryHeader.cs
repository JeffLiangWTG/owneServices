using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Registry.Business.Customs;

namespace Enterprise.Customs.AE.Business;

public class CusEntryHeader : TypeSafeCusEntryHeader, Integration.Customs.AE.ICusEntryHeader
{
	public CusEntryHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public override bool HasBeenWithdrawn
	{
		get { return false; }
	}

	#region MergedLines
	public new ICusEntryLineCollection<CusEntryLine> MergedLines
	{
		get { return (CusEntryLineCollection<CusEntryLine>)base.MergedLines; }
	}

	protected override ICusEntryLineCollection<Customs.Business.CusEntryLine> GetMergedLineCollection()
	{
		return new CusEntryLineCollection<CusEntryLine>(this);
	}
	#endregion

		#region GetEntryChargeTypeList
		protected override EntryChargeTypeList GetEntryChargeTypeList()
		{
			return Factory.GetCachedValue<Registry.EntryChargeTypeList>();
		}
		#endregion

	#region Override Method

	protected override ZDecimal GetTotalChargeValueFor(EntryChargeType chargeTypeElement, ZString methodOfPaymentCode)
	{
		string chargeType = MappingChargeTypes(chargeTypeElement.Code);
		chargeType = chargeType == FeeTypeList.Codes.A00 ? chargeType : FeeTypeList.Codes.B00;

		ZDecimal result = 0m;
		foreach (CusEntryLine entryLine in MergedLines)
		{
			result += entryLine.Fees.GetAmount(chargeType);
		}

		return result;
	}

	ZString MappingChargeTypes(ZString chargeType)
	{
		var result = ZString.Empty;
		switch (chargeType.ToUpper())
		{
			case ChargeTypesList.Codes.DTY:
				result = FeeTypeList.Codes.A00;
				break;
			case ChargeTypesList.Codes.VAT:
				result = FeeTypeList.Codes.B00;
				break;
			default:
				result = chargeType;
				break;
		}
		return result;
	}

	#endregion
}
