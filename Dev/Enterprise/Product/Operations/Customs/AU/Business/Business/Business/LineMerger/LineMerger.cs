using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// This class is responsible for merging invoice lines to Customs Entry Lines
	/// </summary>
	public class LineMerger : Customs.Business.LineMerger
	{
		public LineMerger(JobDeclaration declaration)
			: base(declaration)
		{
		}

		new JobDeclaration Declaration
		{
			get { return base.Declaration as JobDeclaration; }
		}

		protected internal EntryCreationStrategy[] GetEntryCreationStrategiesInternal() => GetEntryCreationStrategies();
		protected override EntryCreationStrategy[] GetEntryCreationStrategies()
		{
			if (Declaration.IsImportCMR || Declaration.IsEXPDeclaration)
			{
				if (Declaration.IsSAC)
				{
					return new EntryCreationStrategy[] { new CMRSACEntryCreationStrategy(this) };
				}
				else
				{
					return new EntryCreationStrategy[] { new CMREntryCreationStrategy(this) };
				}
			}
			else
			{
				if (Globals.IsTest)
				{
					return new EntryCreationStrategy[] { new EdificeEntryCreationStrategy(this) };
				}
				else
				{
					return System.Array.Empty<EntryCreationStrategy>();
				}
			}
		}

		protected override void OnMerging()
		{
			base.OnMerging();
			CacheCPDecQuestions(Declaration);
		}

		internal static void CacheCPDecQuestions(JobDeclaration declaration)
		{
			declaration.CachedQuestions.ClearQuestions();

			if (declaration.IsMergeDone)
			{
				foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
				{
					if (invoiceLine.CusEntryLine != null)
					{
						declaration.CachedQuestions.CacheAnsweredQuestions(invoiceLine, invoiceLine.CusEntryLine.Questions);
					}
				}
			}
		}

		public bool WillThereBeMultipleEntryHeaders
		{
			get
			{
				bool result = false;
				MergeKey headerKey = null;
				EntryCreationStrategy[] strategies = GetEntryCreationStrategies();

				if (strategies != null)
				{
					foreach (EntryCreationStrategy entryCreation in strategies)
					{
						foreach (BaseJobComInvoiceLine invoiceLine in Declaration.InvoiceLines)
						{
							MergeKey key = entryCreation.GetKeyForHeader(invoiceLine);
							if (headerKey != null && headerKey != key)
							{
								result = true;
								break;
							}
							headerKey = key;
						}
						if (result)
						{
							break;
						}
					}
				}
				return result;
			}
		}

		#region Implementation

		protected internal void PerformCountrySpecificOperationAfterMergeBeforeCalculateDutyInternal() => PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty();
		protected override void PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty()
		{
			base.PerformCountrySpecificOperationAfterMergeBeforeCalculateDuty();
			if (Declaration.IsImportCMR)
			{
				GenerateQuestions();
				if (Declaration.SubmitWeeklyNilReturnN30 && !Declaration.ActiveEntryHeaders.Any())
				{
					Declaration.ActiveEntryHeaders.AddNew();
				}
			}
		}

		protected
#if DEBUG
 virtual
#endif
 void GenerateQuestions()
		{
			Declaration.CPQAManager.GenerateQuestions(false, false);
		}

		protected override void CalculateDuties()
		{
			CusEntryHeader[] activeEntryHeaders;

			var consolidatedEntry = ConsolidatedDeclaration.GetConsolidatedDeclaration(Declaration);
			if (consolidatedEntry != null && consolidatedEntry.CRD_JE_LeadDeclaration == Declaration.PK)
			{
				activeEntryHeaders = consolidatedEntry.JobDeclarations.SelectMany(x => x.ActiveEntryHeaders.Cast<CusEntryHeader>()).ToArray();
			}
			else
			{
				activeEntryHeaders = Declaration.ActiveEntryHeaders.ToArray<CusEntryHeader>();
			}

			try
			{
				foreach (CusEntryHeader header in activeEntryHeaders)
				{
					header.IsCalculatingDuty = true;
					header.ResetCachedValuesForDutyCalculation();

					foreach (CusEntryLine line in header.MergedLines)
					{
						AggregateAddInfoAmountAndStoreIntoFeeTable(line);
						line.UpdateLineTILV();
					}
				}

				new DutyCalculationManager().Calculate(activeEntryHeaders);
			}
			finally
			{
				foreach (CusEntryHeader header in activeEntryHeaders)
				{
					header.IsCalculatingDuty = false;
				}
			}
		}

		void AggregateAddInfoAmountAndStoreIntoFeeTable(CusEntryLine entryLine)
		{
			InvoiceLinesForEntryLineCollection invoiceLines = entryLine.InvoiceLines;
			ZDecimal countervailingSecurityAmount = 0m;
			ZDecimal dumpingSecurityAmount = 0m;
			ZDecimal countervailingDutyAmount = 0m;
			ZDecimal interimCountervailingDuty = 0m;
			ZDecimal dumpingDuty = 0m;
			ZDecimal interimDumpingDuty = 0m;
			ZDecimal duty = 0m;
			ZDecimal standardDutyOverriden = 0m;

			foreach (JobComInvoiceLine invoiceLine in invoiceLines)
			{
				countervailingSecurityAmount += invoiceLine.AddInfo.ZA_CSA;
				dumpingSecurityAmount += invoiceLine.AddInfo.ZA_DSA;
				countervailingDutyAmount += invoiceLine.AddInfo.ZA_CVD;
				interimCountervailingDuty += invoiceLine.AddInfo.ZA_ICV;
				dumpingDuty += invoiceLine.AddInfo.ZA_DMP;
				interimDumpingDuty += invoiceLine.AddInfo.ZA_IDP;
				duty += invoiceLine.AddInfo.ZA_DTY;
				standardDutyOverriden += invoiceLine.AddInfo.ZA_STD;
			}

			PopulateFeeForEntryLine(entryLine, countervailingSecurityAmount, CusEntryChargeTypeList.Codes.CountervailingSecurityAmount);
			PopulateFeeForEntryLine(entryLine, dumpingSecurityAmount, CusEntryChargeTypeList.Codes.DumpingSecurityAmount);
			PopulateFeeForEntryLine(entryLine, countervailingDutyAmount, CusEntryChargeTypeList.Codes.CountervailingDuty);
			PopulateFeeForEntryLine(entryLine, interimCountervailingDuty, CusEntryChargeTypeList.Codes.InterimCountervailingDuty);
			PopulateFeeForEntryLine(entryLine, dumpingDuty, CusEntryChargeTypeList.Codes.DumpingDuty);
			PopulateFeeForEntryLine(entryLine, interimDumpingDuty, CusEntryChargeTypeList.Codes.InterimAntiDumpingDuty);
			PopulateFeeForEntryLine(entryLine, standardDutyOverriden, CusEntryChargeTypeList.Codes.StandardDutyOverriden);
			if (!Declaration.IsImportCMR)//CMR uses the same code as normally calculated duty, DTY
			{
				PopulateFeeForEntryLine(entryLine, duty, CusEntryChargeTypeList.Codes.DutyOverride);
			}
		}

		void PopulateFeeForEntryLine(CusEntryLine entryLine, ZDecimal amount, ZString chargeType)
		{
			if (amount > 0)
			{
				CusEntryLineFee lineFee = entryLine.Fees.GetOrAddFeeByFeeType(chargeType);
				lineFee.CF_ChargeAmount = amount;
			}
		}

		#endregion
	}
}
