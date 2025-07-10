using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using static Enterprise.Accounting.Business.ARAP.Invoicing.UniversalTransactionWrapper;
using CodeDescriptionPair = Enterprise.ZArchitecture.Core.CodeDescriptionPair;
using UniversalTransaction = Enterprise.UniversalDataBuss.DataObjects.Accounting.TransactionInfo;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class UniversalTransactionLineWrapper : NonPersistentBusinessObject
	{
		public UniversalTransactionLineWrapper(PostingJournal universalTransactionLine, UniversalTransaction parentUniversalTransaction, UniversalTransactionWrapper parentUniversalTransactionWrapper)
			: base(parentUniversalTransactionWrapper?.Factory)
		{
			sourceUniversalTransactionLine = universalTransactionLine;
			sourceUniversalTransaction = parentUniversalTransaction;
			parent = parentUniversalTransactionWrapper;

			if (parent != null && parentUniversalTransaction != null)
			{
				parent.JobData.AddXMLData(Job, () => parentUniversalTransaction.ShipmentCollection.GetExactDataObject(Job));
				parent.ConsolData.AddXMLData(Consol, () => parentUniversalTransaction.ShipmentCollection.GetExactDataObject(Consol, ConsolType));
			}
		}

		internal void UpdateMappedCodes(InvoicingLineBase line)
		{
			if (!isMappedCodesUpdated)
			{
				var importer = UniversalTransactionWrapper.GetTransactionImporter();
				importer.TrySetMappedValueDirectlyFromSourceCode(sourceUniversalTransactionLine, line);
				isMappedCodesUpdated = true;
			}
		}
		bool isMappedCodesUpdated;

		internal void ResetIsMappedCodesUpdated()
		{
			isMappedCodesUpdated = false;
		}

		[ResourceStringData("Branch", Caption = "Branch")]
		public ZString Branch => sourceUniversalTransactionLine.GetValueSafe(x => x.Branch).GetValueSafe(x => x.Code).GetValueOrDefault();

		[ResourceStringData("Department", Caption = "Department")]
		public ZString Department => sourceUniversalTransactionLine.GetValueSafe(x => x.Department).GetValueSafe(x => x.Code).GetValueOrDefault();

		public ZString ChargeCode => sourceUniversalTransactionLine.GetValueSafe(x => x.ChargeCode).GetValueSafe(x => x.Code).GetValueOrDefault().MappedValue.GetValueOrDefault();

		[ResourceStringData("ChargeCodeSource", Caption = "Charge Code", ShortCaption = "Charge")]
		public ZString ChargeCodeSource => sourceUniversalTransactionLine.GetValueSafe(x => x.ChargeCode).GetValueSafe(x => x.Code).GetValueOrDefault().SourceValue;

		[ResourceStringData("GLAccount", Caption = "GL Account")]
		public ZString GLAccount => sourceUniversalTransactionLine.GetValueSafe(x => x.GLAccount).GetValueSafe(x => x.AccountCode).GetValueOrDefault();

		[ResourceStringData("Description", Caption = "Description")]
		public ZString Description => sourceUniversalTransactionLine.GetValueSafe(x => x.Description).GetValueOrDefault();

		[ResourceStringData("IsFinalCharge", Caption = "Final")]
		public ZBool IsFinalCharge => sourceUniversalTransactionLine.GetValueSafe(x => x.IsFinalCharge).GetValueOrDefault();

		[ResourceStringData("Sequence", Caption = "Sequence")]
		public ZInt Sequence => sourceUniversalTransactionLine.GetValueSafe(x => x.Sequence).GetValueOrDefault();

		[ResourceStringData("OSCurrency", Caption = "Currency")]
		public ZString OSCurrency => sourceUniversalTransactionLine.GetValueSafe(x => x.OSCurrency).GetValueSafe(x => x.Code).GetValueOrDefault();

		[ResourceStringData("LocalCurrency", Caption = "Local Currency", ShortCaption = "L. Currency")]
		public ZString LocalCurrency => sourceUniversalTransactionLine.GetValueSafe(x => x.LocalCurrency).GetValueSafe(x => x.Code).GetValueOrDefault();

		[ResourceStringData("VATTaxID", Caption = "Tax ID")]
		public ZString VATTaxID => sourceUniversalTransactionLine.GetValueSafe(x => x.VATTaxID).GetValueSafe(x => x.TaxCode).GetValueOrDefault();

		[ResourceStringData("TaxMessageID", Caption = "Tax Message", ShortCaption = "Tax Msg.")]
		public ZString TaxMessageID => sourceUniversalTransactionLine.GetValueSafe(x => x.TaxMessageID).GetValueSafe(x => x.TaxMessageCode).GetValueOrDefault();

		[ResourceStringData("RecoverableGSTVATPercentage", Caption = "Tax Recoverable Percentage", MediumCaption = "Tax Recoverable %", ShortCaption = "Tax Rec. %")]
		public ZDecimal RecoverableGSTVATPercentage => sourceUniversalTransactionLine.GetValueSafe(x => x.RecoverableGSTVATPercentage).GetValueOrDefault();

		[ResourceStringData("WithholdingTaxID", Caption = "WHT ID")]
		public ZString WithholdingTaxID => sourceUniversalTransactionLine.GetValueSafe(x => x.WithholdingTaxID).GetValueSafe(x => x.TaxCode).GetValueOrDefault();

		[ResourceStringData("OSAmount", Caption = "Amount")]
		public ZDecimal OSAmount => sourceUniversalTransactionLine.GetValueSafe(x => x.OSAmount).GetValueOrDefault();

		[ResourceStringData("OSGSTVATAmount", Caption = "Tax")]
		public ZDecimal OSGSTVATAmount => sourceUniversalTransactionLine.GetValueSafe(x => x.OSGSTVATAmount).GetValueOrDefault();

		[ResourceStringData("OSTotalAmount", Caption = "Total")]
		public ZDecimal OSTotalAmount => sourceUniversalTransactionLine.GetValueSafe(x => x.OSTotalAmount).GetValueOrDefault();

		[ResourceStringData("OSWHTAmount", Caption = "WHT Amount")]
		public ZDecimal OSWHTAmount => sourceUniversalTransactionLine.GetValueSafe(x => x.OSWHTAmount).GetValueOrDefault();

		[ResourceStringData("LocalAmount", Caption = "Local Amount", ShortCaption = "L. Amount")]
		public ZDecimal LocalAmount => sourceUniversalTransactionLine.GetValueSafe(x => x.LocalAmount).GetValueOrDefault();

		[ResourceStringData("LocalGSTVATAmount", Caption = "Local Tax", ShortCaption = "L. Tax")]
		public ZDecimal LocalGSTVATAmount => sourceUniversalTransactionLine.GetValueSafe(x => x.LocalGSTVATAmount).GetValueOrDefault();

		[ResourceStringData("LocalTotalAmount", Caption = "Local Total", ShortCaption = "L. Total")]
		public ZDecimal LocalTotalAmount => sourceUniversalTransactionLine.GetValueSafe(x => x.LocalTotalAmount).GetValueOrDefault();

		[ResourceStringData("LocalWHTAmount", Caption = "Local WHT Amount", ShortCaption = "L. WHT")]
		public ZDecimal LocalWHTAmount => sourceUniversalTransactionLine.GetValueSafe(x => x.LocalWHTAmount).GetValueOrDefault();

		[ResourceStringData("SubAccount", Caption = "Sub Account", ShortCaption = "Sub Acc.")]
		public ZString SubAccount => sourceUniversalTransactionLine.GetValueSafe(x => x.SubAccount).GetValueSafe(x => x.Code).GetValueOrDefault();

		[ResourceStringData("SubAccountType", Caption = "Sub Account Type", ShortCaption = "Sub Acc. Type")]
		public ZString SubAccountType => sourceUniversalTransactionLine.GetValueSafe(x => x.SubAccount).GetValueSafe(x => x.Type).GetValueSafe(x => x.Code).GetValueOrDefault();

		[ResourceStringData("Job", Caption = "Job Number", ShortCaption = "Job")]
		public ZString Job => sourceUniversalTransactionLine.GetValueSafe(x => x.Job).GetValueSafe(x => x.Key).GetValueOrDefault();

		[ResourceStringData("SubAccounts", Caption = "Sub Accounts")]
		public ZString SubAccounts
		{
			get
			{
				var subAccounts = new ZStringBuilder();
				var subAccountTypeList = new AccountingMasterFilesConstants.SubAccountTypeList();
				foreach (CodeDescriptionPair subAccountType in subAccountTypeList)
				{
					sourceUniversalTransactionLine.SubAccountCollection?.ForEach(x =>
					{
						if (x.Type?.Code?.ToString() == subAccountType.Code)
						{
							subAccounts.Append(FormattableString.Invariant($"{x.Type.Code}: {x.Code}, "));
						}
					}
					);
				}

				sourceUniversalTransactionLine.SubAccountCollection?.ForEach(x =>
				{
					if (!subAccountTypeList.ToArray().Any(y => y.Code == x.Type?.Code?.ToString()))
					{
						subAccounts.Append(FormattableString.Invariant($"{x.Type?.Code}: {x.Code}, "));
					}
				}
					);
				return subAccounts.ToString().Trim().Trim(',');
			}
		}

		public Job JobHeader
		{
			get
			{
				if (jobHeader == null && !jobNotFoundOrNonJobRelated)
				{
					if (!Job.IsEmpty)
					{
						if (parent.AssociatedJobs.ContainsKey(Job))
						{
							return parent.AssociatedJobs[Job];
						}
						else
						{
							var importer = UniversalTransactionWrapper.GetTransactionImporter();
							jobHeader = (Job)importer.GetJob(Factory, sourceUniversalTransaction, sourceUniversalTransactionLine);

							if (jobHeader != null)
							{
								parent.AssociatedJobs[Job] = jobHeader;
							}
							else
							{
								jobNotFoundOrNonJobRelated = true;
							}
						}
					}
				}

				return jobHeader;
			}
		}
		Job jobHeader;
		bool jobNotFoundOrNonJobRelated;

		[ResourceStringData("Consol", Caption = "Consol Number", ShortCaption = "Consol")]
		public ZString Consol => sourceUniversalTransactionLine.GetValueSafe(x => x.CostSource).GetValueSafe(x => x.Key).GetValueOrDefault();

		ZString ConsolType => sourceUniversalTransactionLine.GetValueSafe(x => x.CostSource).GetValueSafe(x => x.Type).GetValueOrDefault();

		public IJobCostingPlugIn JobConsol
		{
			get
			{
				if (jobConsol == null && !consolNotFoundOrNotConsolRelated)
				{
					var importer = UniversalTransactionWrapper.GetTransactionImporter();

					ZString consolNumber = ZString.Empty;
					ZString? consolType = ZString.Empty;

					if (parent.IsCrossLedger)
					{
						ZString consolInvoiceNumber;
						if (sourceUniversalTransaction.JobInvoiceNumber.TryGetValue(out consolInvoiceNumber) && sourceUniversalTransaction.Job.GetValueSafe(x => x.Key).GetValueOrDefault().IsEmpty)
						{
							consolNumber = InvoicingBase.GetConsolNumberFromConsolidatedInvoiceRef(consolInvoiceNumber);
							consolType = null;
						}
					}
					else
					{
						consolNumber = Consol;
						consolType = ConsolType;
					}

					if (!consolNumber.IsEmpty)
					{
						var key = new ConsolKey(consolNumber, consolType);
						if (parent.AssociatedConsols.ContainsKey(key))
						{
							return parent.AssociatedConsols[key];
						}
						else
						{
							var importedConsol = importer.GetConsol(sourceUniversalTransaction, consolNumber, consolType);
							if (importedConsol != null)
							{
								var consolInRequiredFactory = importedConsol != null ? Factory.Load(importedConsol.TablePrefix, importedConsol.PK) : null;
								jobConsol = consolInRequiredFactory as IJobCostingPlugIn;

								if (jobConsol != null)
								{
									parent.AssociatedConsols[key] = jobConsol;
								}
							}
						}
					}

					if (jobConsol == null)
					{
						consolNotFoundOrNotConsolRelated = true;
					}
				}

				return jobConsol;
			}
		}
		IJobCostingPlugIn jobConsol;
		bool consolNotFoundOrNotConsolRelated;

		[ResourceStringData("UniversalTransactionLineWrapper|JobConsolXMLData", Caption = "Job Summary")]
		public ZString JobConsolXMLData
		{
			get
			{
				var template =
@"{0} {1}
{2}";
				var jobData = Job.IsEmpty ? ZString.Empty : ZString.Format(template, Res.GetString("9FC10E52-CC26-4F0E-81ED-608DA0E42AA9", "Job"), Job, JobXMLDataFormatted);
				var consolData = Consol.IsEmpty ? ZString.Empty : ZString.Format(template, Res.GetString("57A09C3E-7E64-40E3-BB26-FB601E7B286A", "Consol"), Consol, ConsolXMLDataFormatted);

				return NullableExtensions.JoinExcludingEmpty(System.Environment.NewLine + System.Environment.NewLine, new[] { jobData, consolData });
			}
		}

		ZString JobXMLDataFormatted
		{
			get
			{
				var result = ZString.Empty;
				if (parent != null)
				{
					result = parent.JobData.GetXMLDataFormatted(Job);
				}

				return result;
			}
		}

		ZString ConsolXMLDataFormatted
		{
			get
			{
				var result = ZString.Empty;
				if (parent != null)
				{
					result = parent.ConsolData.GetXMLDataFormatted(Consol);
				}

				return result;
			}
		}

		[ResourceStringData("PlaceOfSupply", Caption = "Fixed Place Of Supply", ShortCaption = "FPOS")]
		public ZString PlaceOfSupply => sourceUniversalTransactionLine.GetValueSafe(x => x.PlaceOfSupply).GetValueSafe(x => x.Location?.Code).GetValueOrDefault();

		[ResourceStringData("PlaceOfSupplyType", Caption = "Fixed Place Of Supply Type", ShortCaption = "FPOS Type")]
		public ZString PlaceOfSupplyType => sourceUniversalTransactionLine.GetValueSafe(x => x.PlaceOfSupply).GetValueSafe(x => x.LocationType?.Code).GetValueOrDefault();

		readonly UniversalTransaction sourceUniversalTransaction;
		readonly PostingJournal sourceUniversalTransactionLine;
		readonly UniversalTransactionWrapper parent;
	}
}
