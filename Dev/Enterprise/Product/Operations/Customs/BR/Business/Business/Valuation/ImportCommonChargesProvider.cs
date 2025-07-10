using System;
using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.BR.Business
{
	public static class ImportCommonChargesProvider
	{
		#region CustomsChargeCodes

		public static IReadOnlyList<ICustomsChargeCode> CommonChargesList
		{
			get
			{
				var charges = new[]
				{
					CustomsChargeCodeProvider.AdditionCharge,
					CustomsChargeCodeProvider.Commission,
					CustomsChargeCodeProvider.DeductionCharge,
					CustomsChargeCodeProvider.Discount,
					CustomsChargeCodeProvider.ExWorks,
					CustomsChargeCodeProvider.ForeignInlandFreight,
					CustomsChargeCodeProvider.LandingCharges,
					CustomsChargeCodeProvider.OtherCharges,
					CustomsChargeCodeProvider.PackingCost
				};
				charges.ForEach(x => x.ParentTypes = ChargeParentTypes.Invoice | ChargeParentTypes.InvoiceLine);
				return charges;
			}
		}

		internal static ICustomsChargeCode[] OverseasFreightCharges => new[] { OverseasFreightCollect, OverseasFreightPrepaid };

		public static CustomsChargeCode OverseasFreightCollect => overseasFreightCollect ?? (overseasFreightCollect = new CustomsChargeCode(ImportCustomsChargeTypeList.Codes.OverseasFreightCollect, ImportCustomsChargeTypeList.Descriptions.OverseasFreightCollect)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = false,
			DistributeBy = ChargeDistributeByList.Codes.NetWeight,
		});
		[ThreadStatic]
		static CustomsChargeCode overseasFreightCollect;

		public static CustomsChargeCode OverseasFreightPrepaid => overseasFreightPrepaid ?? (overseasFreightPrepaid = new CustomsChargeCode(ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid, ImportCustomsChargeTypeList.Descriptions.OverseasFreightPrepaid)
		{
			IsDutiable = false,
			IsDutiableDeemedForThisCharge = true,
			IsVATible = true,
			IsVATibleDeemedForThisCharge = true,
			IsIncludedInITOTIfDeemed = false,
			IsIncludedInITOTDeemedForThisCharge = false,
			DistributeBy = ChargeDistributeByList.Codes.NetWeight,
		});
		[ThreadStatic]
		static CustomsChargeCode overseasFreightPrepaid;

		#endregion
	}
}
