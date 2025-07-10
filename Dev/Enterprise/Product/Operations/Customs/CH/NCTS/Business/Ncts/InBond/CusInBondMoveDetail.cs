using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CH.NCTS.Business;

public class CusInBondMoveDetail : EU.NCTS.Business.CusInBondMoveDetail
{
	public CusInBondMoveDetail(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override JobDocAddressRequirement GetJobDocAddressRequirement(DocAddressType docAddressType)
	{
		var requirement = new JobDocAddressRequirement(docAddressType);
		requirement.ValidateState = validation => { };
		return requirement;
	}

	public new NctsBill Bill => (NctsBill)base.Bill;

	[LightValidationTestExempt]
	public override ZGuid B9_B0 { get => base.B9_B0; set => base.B9_B0 = value; }

	[LightValidationTestExempt]
	public override ZGuid B9_BM { get => base.B9_BM; set => base.B9_BM = value; }

	public override ZString B9_UnloadedState
	{
		get => base.B9_UnloadedState;
		set
		{
			var oldValue = B9_UnloadedState;
			base.B9_UnloadedState = value;
			if (!IsCopying && oldValue != B9_UnloadedState)
			{
				Bill?.HouseConsignmentDifferences?.MarkAsNeedingValidation();
				Bill?.ArrivalGoodsItems.ForEach(x => x.GoodsItemDifferencesDetails.MarkAsNeedingValidation());
				if (B9_UnloadedState != NctsUnloadedStateList.Codes.MIS)
				{
					Bill?.ClearUnloadingRemarks();
				}
			}
		}
	}

	public override bool ReadOnly
	{
		get => base.ReadOnly;
		set
		{
			base.ReadOnly = value;
			SetAddressesReadOnly();
		}
	}

	void SetAddressesReadOnly()
	{
		ConsignorDocAddress.ReadOnly = true;
		ConsigneeDocAddress.ReadOnly = true;
	}
}
