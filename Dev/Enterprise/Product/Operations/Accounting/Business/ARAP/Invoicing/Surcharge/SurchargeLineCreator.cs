using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public interface ISurchargeLineCreator
	{
		void AddSurchargeLine(InvoicingBase invoice);
	}

	public class SurchargeLineCreator : ISurchargeLineCreator
	{
		void ISurchargeLineCreator.AddSurchargeLine(InvoicingBase invoice)
		{
			if (ShouldAddSurchargeLine(invoice))
			{
				var surchargeCalculator = ObjectFactory.Get<ISurchargeCalculator>();

				var calculationDataList = surchargeCalculator.GetSurchargeCalculationData(invoice).ToList();
				if (calculationDataList.Any())
				{
					var factory = invoice.Factory;
					var prevIgnoreValidationSuspended = invoice.IgnoreValidationSuspended;
					using (new DisposableAction(() => invoice.IgnoreValidationSuspended = false, () => invoice.IgnoreValidationSuspended = prevIgnoreValidationSuspended))
					{
						foreach (var data in calculationDataList)
						{
							var basisLines = invoice.Lines.Where(x => data.LinePKs.Contains(x.PK)).Cast<InvoicingLineBase>().ToList();
							var supplyType = basisLines.AllSame(x => x.AL_SupplyType) ? basisLines.First().AL_SupplyType : ZString.Empty;
							var placeOfSupply = basisLines.AllSame(x => x.AL_PlaceOfSupply) ? basisLines.First().AL_PlaceOfSupply : ZString.Empty;
							var basisLineBranches = basisLines.Select(x => x.AL_GB);

							var chargeCode = factory.Load<AccChargeCode>(data.ChargePK);
							var isNONChargeType = (chargeCode?.AC_ChargeType ?? CargoWise.Types.ZString.Empty) == Enterprise.Core.Constants.ChargeType.NonAccrual;
							var shouldAddCharge = !isNONChargeType && data.JobPK.IsValid;
							Job job = null;
							if (data.JobPK.IsValid)
							{
								job = factory.Load<Job>(data.JobPK);
							}

							var line = (InvoicingLineBase)invoice.Lines.AddNew();
							line.SetContext(BusinessContext.SkipTaxIdAndTaxMessageMappingValidation);
							line.SetContext(BusinessContext.SurchargeLine);

							if (shouldAddCharge)
							{
								line.AL_JH = data.JobPK;
							}

							line.GenericCharge = data.ChargePK;

							line.AL_GB = GetBranch(job?.JH_GB, basisLineBranches);
							line.AL_GE = GetDepartment(job?.JH_GE);
							line.AL_OH = invoice.AH_OH;
							line.AL_GB_TaxBranch = invoice.AH_GB_TaxBranch;

							line.AL_SupplyType = supplyType;

							if (!placeOfSupply.IsEmpty)
							{
								line.AL_PlaceOfSupply = placeOfSupply;
							}

							line.AL_RX_NKTransactionCurrency = invoice.AH_RX_NKTransactionCurrency;

							if (InvoiceTypeCalculationProvider.IsDeferredInvoiceType(invoice.AH_TransactionCategory))
							{
								line.AL_ExchangeRate = invoice.GetExchangeRateFromJobExRateConfig(job);
							}
							else
							{
								line.AL_ExchangeRate = invoice.AH_ExchangeRate;
							}

							line.AL_OSExTaxAmount = data.CalculatedAmount;

							Charge charge = null;
							if (shouldAddCharge)
							{
								charge = CreateCharge(line);
							}

							var shouldAddSurchargeToDesc = calculationDataList.Count(x => x.JobPK == line.AL_JH && x.ChargePK == line.AL_AC) > 1;
							var descSurchargeCode = $" ({data.SurchargeCode})";
							if (shouldAddSurchargeToDesc)
							{
								if (charge != null)
								{
									charge.JR_Desc += descSurchargeCode;
									line.AL_Desc = charge.JR_Desc;
								}
								else
								{
									line.AL_Desc += descSurchargeCode;
								}
							}

							line.RunPreSaveValidation();
						}
					}
				}
			}
		}

		ZGuid GetBranch(ZGuid? jobBranchPK, IEnumerable<ZGuid> basisLineBranches)
		{
			ZGuid branchPK;

			if (basisLineBranches.AllSame() || AccountingConfigurationRegistry.Instance.ReceivableEnforceBranchLevelPosting.Value.EnableBranchLevelPosting)
			{
				branchPK = basisLineBranches.First();
			}
			else
			{
				if (jobBranchPK.HasValue)
				{
					branchPK = jobBranchPK.Value;
				}
				else
				{
					branchPK = GlbBranch.CurrentBranch.PK;
				}
			}

			return branchPK;
		}

		ZGuid GetDepartment(ZGuid? jobDeptPK)
		{
			ZGuid deptPK;

			if (jobDeptPK.HasValue)
			{
				deptPK = jobDeptPK.Value;
			}
			else
			{
				deptPK = GlbDepartment.CurrentDepartment.PK;
			}

			return deptPK;
		}

		Charge CreateCharge(InvoicingLineBase line)
		{
			var header = line.TransactionHeader;
			var charge = line.Factory.New<Charge>();
			using (charge.SetTempContext(JobInvoicingBusinessContext.SuspendJobChargeCalculationTrigger))
			using (charge.TaxAmountAdjustmentSuspender.GetSuspender())
			using (charge.GetValidationSuspender())
			{
				charge.JR_JH = line.AL_JH;
				charge.JR_AC = line.AL_AC;
				charge.JR_GB = line.AL_GB;
				charge.JR_GE = line.AL_GE;
				charge.JR_OH_SellAccount = header.AH_OH;
				charge.JR_RX_NKSellCurrency = line.AL_RX_NKTransactionCurrency;
				charge.JR_OSSellAmt = line.AL_OSExTaxAmount_DBSigned;
				charge.SetSellTaxDateSafe(line.AL_TaxDate);
				charge.JR_AW_SellWHTRate = line.AL_AW;
				charge.JR_AL_ARLine = line.PK;
				charge.ARLine.SetContext(BusinessContext.SkipTaxIdAndTaxMessageMappingValidation);
				charge.JR_LineCFX = 0m;
				charge.JR_OSSellExRate = line.AL_ExchangeRate;
				charge.JR_LocalSellAmt = line.AL_LineAmount;
				charge.JR_OA_SellInvoiceAddress = header.AH_OA_InvoiceAddressOverride;
				charge.JR_OC_SellInvoiceContact = header.AH_OC_InvoiceContactOverride;
				charge.JR_SellGovtChargeCode = line.AL_GovtChargeCode;
				charge.JR_InvoiceType = header.AH_TransactionCategory;
				charge.JR_SellSupplyType = line.AL_SupplyType;
				charge.JR_GB_SellTaxBranch = line.AL_GB_TaxBranch;

				if (!line.AL_PlaceOfSupply.IsEmpty)
				{
					charge.JR_SellPlaceOfSupply = line.AL_PlaceOfSupply;
				}

				charge.JR_AT_SellGSTRate = line.AL_AT;
				charge.JR_A9_SellVATClass = line.AL_A9_VATClass;
			}

			return charge;
		}

		bool ShouldAddSurchargeLine(InvoicingBase invoicingBase) => !invoicingBase.IsInDatabase &&
																	!invoicingBase.IsReverseTransaction &&
																	!invoicingBase.IsAmendingTransaction &&
																	invoicingBase.AH_Ledger == LedgerTypes.AccountsReceivable &&
																	invoicingBase.AH_TransactionType == TransactionTypes.Invoice &&
																	invoicingBase.Header != null;

#if DEBUG
		public bool ShouldAddSurchargeLine_ExposedForTestOnly(InvoicingBase invoicingBase) => ShouldAddSurchargeLine(invoicingBase);
#endif

	}
}
