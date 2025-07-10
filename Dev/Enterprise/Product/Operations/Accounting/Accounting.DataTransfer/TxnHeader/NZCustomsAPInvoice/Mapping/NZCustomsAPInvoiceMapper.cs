using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public class NZCustomsAPInvoiceMapper
	{
		public NZCustomsAPInvoiceMapper(BusinessObjectFactory factory, INotifications notifications)
		{
			Factory = factory;
			Notifications = notifications;

			foreach (EntryChargeTypeSetting typeAndCode in RatingDataRegistry.Instance.EntryChargeTypesAndCodes.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				if (typeAndCode.ChargeType == NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.GST)
				{
					RegistryGSTChargeCode = Factory.Load<AccChargeCode>(typeAndCode.AC_ChargeCode);
				}
				else if (typeAndCode.ChargeType == NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.EntryFee)
				{
					RegistryEntryFeeChargeCode = Factory.Load<AccChargeCode>(typeAndCode.AC_ChargeCode);
				}
				else if (typeAndCode.ChargeType == NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.Duty)
				{
					RegistryDutyChargeCode = Factory.Load<AccChargeCode>(typeAndCode.AC_ChargeCode);
				}
			}
		}

		public void MapJobChargeLines(JobHeader job, NZCustomsAPInvoiceDataRow row, Xsd.TxnHeader invoice)
		{
			NZCustomsAPInvoiceJobWrapper jobWrapper = new NZCustomsAPInvoiceJobWrapper(job);
			ZDecimal totalDutyAndEntryFee = Math.Abs(row.NetChargesExclGST);
			ZDecimal totalGST = Math.Abs(row.GST);
			fChargesToImport = null;

			foreach (JobCharge charge in jobWrapper.ValidChargeLines)
			{
				if (RegistryEntryFeeChargeCode != null && charge.JR_AC == RegistryEntryFeeChargeCode.PK)
				{
					totalDutyAndEntryFee = GetRemainderAfterUpdate(NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.EntryFee, invoice, jobWrapper, RegistryEntryFeeChargeCode.AC_Code, totalDutyAndEntryFee, charge.JR_LocalCostAmt);
				}
				else if (RegistryDutyChargeCode != null && charge.JR_AC == RegistryDutyChargeCode.PK)
				{
					totalDutyAndEntryFee = GetRemainderAfterUpdate(NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.Duty, invoice, jobWrapper, RegistryDutyChargeCode.AC_Code, totalDutyAndEntryFee, charge.JR_LocalCostAmt);
				}
			}

			if (totalDutyAndEntryFee > 0)
			{
				if (ChargesToImport[NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.EntryFee] == null)
				{
					if (ChargesToImport[NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.Duty] == null)
					{
						NotifyWarning(jobWrapper.JobNumber, Res.GetString("44baf8bf-e9cf-4baf-b432-2e3f7cca2a4a", "This job does not have Entry Fee or Duty allocated. By default the imported amount will be allocated to Default Disbursement Charge Code"));
						UpdateTxnLine(NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.Default, invoice, jobWrapper, DefaultChargeCode.AC_Code, totalDutyAndEntryFee);
					}
					else
					{
						if (RegistryDutyChargeCode != null)
						{
							NotifyWarning(jobWrapper.JobNumber, Res.GetString("bc14f9b1-b66a-4677-abb6-75bb46a0bc17", "Unable to allocate the excess amount to Entry Fee. It will be defaulted to Duty."));
							UpdateTxnLine(NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.Duty, invoice, jobWrapper, RegistryDutyChargeCode.AC_Code, totalDutyAndEntryFee);
						}
						else
						{
							NotifyWarning(jobWrapper.JobNumber, Res.GetString("2118c4d4-21f5-48c0-bd66-9e3c93ed5440", "Unable to allocate the excess amount to Duty because it is not setup in the Registry. It will be defaulted to the Default Disbursement Charge Code"));
							UpdateTxnLine(NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.Default, invoice, jobWrapper, DefaultChargeCode.AC_Code, totalDutyAndEntryFee);
						}
					}
				}
				else
				{
					if (RegistryDutyChargeCode != null)
					{
						NotifyWarning(jobWrapper.JobNumber, Res.GetString("8ac20cd3-5bc6-49f4-86ff-48bcde29631d", "Amount 1 in file higher than accruals. ${0} allocated to Duty.", totalDutyAndEntryFee.ToString("0.00")));
						UpdateTxnLine(NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.Duty, invoice, jobWrapper, RegistryDutyChargeCode.AC_Code, totalDutyAndEntryFee);
					}
					else
					{
						NotifyWarning(jobWrapper.JobNumber, Res.GetString("2118c4d4-21f5-48c0-bd66-9e3c93ed5440", "Unable to allocate the excess amount to Duty because it is not setup in the Registry. It will be defaulted to the Default Disbursement Charge Code"));
						UpdateTxnLine(NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.Default, invoice, jobWrapper, DefaultChargeCode.AC_Code, totalDutyAndEntryFee);
					}
				}
			}

			if (ChargesToImport[NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.EntryFee] != null)
			{
				IBaseJobDeclaration declaration = job.Parent as IBaseJobDeclaration;
				if (declaration == null && job.Parent is IForwardingShipment shipment)
				{
					declaration = shipment.GetDeclarationFor(job.JH_GC) as IBaseJobDeclaration;
				}
				var taxDate = declaration?.DateForDutyRate;
				if (!taxDate.HasValue || !taxDate.Value.IsValid)
				{
					taxDate = ZDate.Today;
				}

				ZDecimal gstValue = ChargesToImport[NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.EntryFee].OsInvoiceAmtExclTax.Value * (GSTTaxRate.GetRate(taxDate.Value) / 100m);
				gstValue = NZCustomsEntryFeeTaxCalculator.GetEntryFeeGST(ChargesToImport[NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.EntryFee].OsInvoiceAmtExclTax.Value, gstValue);

				if (RegistryGSTChargeCode != null)
				{
					UpdateTxnLine(NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.GST, invoice, jobWrapper, RegistryGSTChargeCode.AC_Code, totalGST + gstValue);
				}
				else
				{
					NotifyWarning(jobWrapper.JobNumber, Res.GetString("9997607a-8bd6-4100-b080-ac37205cf613", "GST Charge Code was not specified in the Registry. The GST amount will be allocated to the Default Disbursement Charge Code."));
					UpdateTxnLine(NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.Default, invoice, jobWrapper, DefaultChargeCode.AC_Code, totalGST + gstValue);
				}
			}
			else
			{
				if (RegistryGSTChargeCode != null)
				{
					NotifyWarning(jobWrapper.JobNumber, Res.GetString("cd415a5c-3f20-435a-9273-b29181faa726", "Unable to work out the GST for Entry Fee because there is no Entry Fee on this job. The imported value will be imported as GST."));
					UpdateTxnLine(NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.GST, invoice, jobWrapper, RegistryGSTChargeCode.AC_Code, totalGST);
				}
				else
				{
					NotifyWarning(jobWrapper.JobNumber, Res.GetString("9997607a-8bd6-4100-b080-ac37205cf613", "GST Charge Code was not specified in the Registry. The GST amount will be allocated to the Default Disbursement Charge Code."));
					UpdateTxnLine(NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.Default, invoice, jobWrapper, DefaultChargeCode.AC_Code, totalGST);
				}
			}

			// The following is needed because the Xsd marks it as negative even when you set it as possitive.
			if ((row.NetChargesExclGST + row.GST) < 0)
			{
				foreach (KeyValuePair<ZString, Xsd.TxnLine> charge in ChargesToImport)
				{
					if (charge.Value != null)
					{
						charge.Value.OsInvoiceAmtExclTax.Value = -1 * charge.Value.OsInvoiceAmtExclTax.Value;
					}
				}
			}
		}

		#region Implementation

		protected readonly INotifications Notifications;
		protected readonly BusinessObjectFactory Factory;
		protected readonly AccChargeCode RegistryDutyChargeCode;
		protected readonly AccChargeCode RegistryEntryFeeChargeCode;
		protected readonly AccChargeCode RegistryGSTChargeCode;

		void NotifyWarning(ZString jobNumber, ZString msg)
		{
			Notifications.Notify(new WarningNotification(WarningType.Warning, Res.GetString("6cbc9898-138b-480b-a251-090dc1dff3ad", @"Job Number '{0}': {1}", jobNumber, msg)));
		}

		void CreateNewTxnLine(ZString chargeType, Xsd.TxnHeader invoice, NZCustomsAPInvoiceJobWrapper job, ZString chargeCode)
		{
			ChargesToImport[chargeType] = invoice.TxnLines.AddNew();
			ChargesToImport[chargeType].Branch = job.BranchCode;
			ChargesToImport[chargeType].Department = job.DepartmentCode;
			ChargesToImport[chargeType].ConsolOrJobNo = job.JobNumber;
			ChargesToImport[chargeType].ChargeCode = chargeCode;
			ChargesToImport[chargeType].OsInvoiceAmtExclTax.CurrencyCode = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
		}

		void UpdateTxnLine(ZString chargeType, Xsd.TxnHeader invoice, NZCustomsAPInvoiceJobWrapper job, ZString chargeCode, ZDecimal amount)
		{
			if (ChargesToImport[chargeType] == null)
			{
				CreateNewTxnLine(chargeType, invoice, job, chargeCode);
			}

			ChargesToImport[chargeType].OsInvoiceAmtExclTax.Value -= amount; // this is because the Xsd marks it as negative even when you set it as possitive.
		}

		ZDecimal GetRemainderAfterUpdate(ZString chargeType, Xsd.TxnHeader invoice, NZCustomsAPInvoiceJobWrapper job, ZString chargeCode, ZDecimal currentRemainder, ZDecimal amount)
		{
			if (ChargesToImport[chargeType] == null)
			{
				CreateNewTxnLine(chargeType, invoice, job, chargeCode);
			}

			if (currentRemainder > amount)
			{
				ChargesToImport[chargeType].OsInvoiceAmtExclTax.Value -= amount;
				currentRemainder -= amount; // this is because the Xsd marks it as negative even when you set it as possitive.
			}
			else
			{
				ChargesToImport[chargeType].OsInvoiceAmtExclTax.Value -= currentRemainder;
				currentRemainder = 0;
			}

			return currentRemainder;
		}

		#region DefaultChargeCode
		AccChargeCode fDefaultChargeCode;
#if DEBUG
		public
#endif
		AccChargeCode DefaultChargeCode
		{
			get
			{
				if (fDefaultChargeCode == null)
				{
					fDefaultChargeCode = Factory.Load<AccChargeCode>(RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value);
				}
				return fDefaultChargeCode;
			}
		}
		#endregion

		#region GST Tax Rate
		AccTaxRate fGSTTaxRate;
#if DEBUG
		public
#endif
		AccTaxRate GSTTaxRate
		{
			get
			{
				if (fGSTTaxRate == null)
				{
					fGSTTaxRate = AccTaxRate.Helper.FindTaxRate(Factory, AccTaxRate.Helper.MainGSTTaxRegistryID, Env.CurrentCompany.PK);
				}
				return fGSTTaxRate;
			}
		}
		#endregion

		#region ChargesToImport
		Dictionary<ZString, Xsd.TxnLine> fChargesToImport;
		Dictionary<ZString, Xsd.TxnLine> ChargesToImport
		{
			get
			{
				if (fChargesToImport == null)
				{
					fChargesToImport = new Dictionary<ZString, Xsd.TxnLine>();
					fChargesToImport.Add(NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.Duty, null);
					fChargesToImport.Add(NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.EntryFee, null);
					fChargesToImport.Add(NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.GST, null);
					fChargesToImport.Add(NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.Default, null);
				}
				return fChargesToImport;
			}
		}
		#endregion

		#endregion
	}
}
