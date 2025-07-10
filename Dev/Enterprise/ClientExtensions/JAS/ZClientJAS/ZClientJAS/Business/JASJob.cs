using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Client.JAS.Business.JXC.Import;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business
{
	public class JASJob : Job
	{
		public JASJob(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public void AddCharges(params IJobChargeData[] chargesData)
		{
			foreach (IJobChargeData chargeData in chargesData)
			{
				if (chargeData.IsCollect)
				{
					JobCharge jobCharge = FindOrAddNewJobCharge(chargeData);
					jobCharge.JR_OH_SellAccount = LocalChargesPK;
					jobCharge.JR_Desc = chargeData.ChargeDescription;
					SetJobChargeCurrency(jobCharge, chargeData);
					jobCharge.JR_OSSellAmt = chargeData.ChargeAmount;
				}
			}
		}

		JobCharge FindOrAddNewJobCharge(IJobChargeData chargeData)
		{
			JobCharge result = null;

			ZGuid chargeCodePK = GetChargeCode(chargeData);
			foreach (JobCharge existingCharge in Charges)
			{
				if (!existingCharge.IsInDatabase && existingCharge.JR_AC == chargeCodePK && !AddedOrMatchedChargesPKs.Contains(existingCharge.PK))
				{
					result = existingCharge;
					AddedOrMatchedChargesPKs.Add(existingCharge.PK);
					break;
				}
			}

			if (result == null)
			{
				result = Charges.AddNew();
				result.JR_AC = chargeCodePK;
				AddedOrMatchedChargesPKs.Add(result.PK);
			}

			return result;
		}

		ZGuid GetChargeCode(IJobChargeData chargeData)
		{
			ZQuery filter = new ZQuery(AccChargeCodeSchema.AC_Code, chargeData.ChargeCode);
			filter.AddToFilter(AccChargeCodeSchema.AC_GC, JH_GC);
			ZQuery chargeTypeFilter = new ZQuery(AccChargeCodeSchema.AC_ChargeType, Core.Constants.ChargeType.Disbursement);
			chargeTypeFilter.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, Core.Constants.ChargeType.Margin);
			chargeTypeFilter.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_ChargeType, SQLComparisonOperator.Equal, Core.Constants.ChargeType.ManualJobAccrual);
			filter.AddToFilter(chargeTypeFilter);

			AccChargeCode accChargeCode = (AccChargeCode)Factory.LoadTop1(typeof(AccChargeCode), filter);
			return (accChargeCode != null)
				? accChargeCode.PK
				: new ZGuid(JASDataRegistry.Instance.DefaultChargeCodeForAccrualsImportItem.GetValueWithoutFallback(JH_GC.ToGuid(), Guid.Empty, Guid.Empty));
		}

		void SetJobChargeCurrency(JobCharge jobCharge, IJobChargeData chargeData)
		{
			RefCurrency refCurrency = RefCurrency.LoadFromCurrencyCode(jobCharge.Factory, chargeData.Currency);
			jobCharge.JR_RX_NKSellCurrency = (refCurrency != null) ? refCurrency.RX_Code : jobCharge.Branch.Company.LocalCurrency.RX_Code;
		}

		readonly List<ZGuid> AddedOrMatchedChargesPKs = new List<ZGuid>();
	}
}

#region Implementation
#endregion
