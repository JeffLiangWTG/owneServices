using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocJobInvoicingJob : DocumentWrapper
	{
		DocJobInvoicingJob(Job job, BusinessObjectFactory factoryToWrap)
			: base(job, factoryToWrap)
		{
		}

		public static DocJobInvoicingJob New(BusinessObjectFactory factory, ZGuid pK)
		{
			return New(factory.Load<Job>(pK), factory);
		}

		public static DocJobInvoicingJob New(BusinessObjectFactory factory, ZGuid pK, ZBool isConsolDoc, ZBool isProfitLossDoc)
		{
			DocJobInvoicingJob job = New(factory.Load<Job>(pK), factory);
			if (job != null)
			{
				job.isConsolDoc = isConsolDoc;
				job.isProfitLossDoc = isProfitLossDoc;
			}

			return job;
		}

		public static DocJobInvoicingJob New(Job job, BusinessObjectFactory factoryToWrap)
		{
			if (job == null)
			{
				return null;
			}
			else
			{
				return factoryToWrap.GetCachedValue(job.PK.ToStringKey(), () => new DocJobInvoicingJob(job, factoryToWrap), CacheStalenessPolicy.StaleOnFactorySave);
			}
		}

		public static DocJobInvoicingJob New(JobDocumentPrintItem jobPrintItem, BusinessObjectFactory factoryToWrap)
		{
			if (jobPrintItem == null || jobPrintItem.Job == null)
			{
				return null;
			}
			else
			{
				DocJobInvoicingJob jobWrapper = DocJobInvoicingJob.New(jobPrintItem.Job, factoryToWrap);
				jobWrapper.fPrintChargeSummary = jobPrintItem.PrintChargeSummary;
				jobWrapper.fPrintProfitRecognitionByDateSummary = jobPrintItem.PrintProfitRecognitionByDateSummary;
				jobWrapper.fPrintChargeDetail = jobPrintItem.PrintChargeDetail;
				jobWrapper.fPrintARInvoiceAnalysis = jobPrintItem.PrintARInvoiceAnalysis;
				jobWrapper.fPrintAPInvoiceAnalysis = jobPrintItem.PrintAPInvoiceAnalysis;
				jobWrapper.fPrintJobRevenueJournalAnalysis = jobPrintItem.PrintJobRevenueJournalAnalysis;
				jobWrapper.isConsolDoc = false;
				jobWrapper.isProfitLossDoc = jobPrintItem.IsProfitLossDoc;
				return jobWrapper;
			}
		}

		public override string ToString()
		{
			return JobNum;
		}

		public ZGuid JobPK
		{
			get { return Job.PK; }
		}

		public DocJobInvoicingJobChargeCollection Charges
		{
			get
			{
				DocJobInvoicingJobChargeCollection result = new DocJobInvoicingJobChargeCollection(Factory);
				foreach (Charge charge in Job.Charges)
				{
					if (!IsProfitLossDoc || IsAllowedtoViewCharge(charge))
					{
						result.Add(DocJobInvoicingJobCharge.New(charge, Factory));
					}
				}
				return result;
			}
		}

		public DocJobInvoicingJobChargeCollection ChargesForProfitShare
		{
			get
			{
				DocJobInvoicingJobChargeCollection result = new DocJobInvoicingJobChargeCollection(Factory);
				foreach (Charge charge in Job.Charges)
				{
					if (!IsProfitLossDoc || IsAllowedtoViewCharge(charge))
					{
						result.AddRange(DocJobInvoicingJobCharge.NewForJobProfit(charge, Factory));
					}
				}
				return result;
			}
		}

		public DocProfitLossSummaryLineCollection AllProfitLossSummaryLines
		{
			get
			{
				if (fAllProfitLossSummaryLines == null)
				{
					fAllProfitLossSummaryLines = new DocProfitLossSummaryLineCollection(Factory);
					foreach (ProfitLossSummaryDetailView line in ProfitLoss.ProfitLossSummaryFilteredDetails)
					{
						fAllProfitLossSummaryLines.Add(DocProfitLossSummaryLine.New(line, Factory));
					}
				}
				return fAllProfitLossSummaryLines;
			}
		}
		DocProfitLossSummaryLineCollection fAllProfitLossSummaryLines;

		public DocJobInvoicingJobChargeCollection ChargesDueAgentOrProfitShared
		{
			get
			{
				DocJobInvoicingJobChargeCollection result = new DocJobInvoicingJobChargeCollection(Factory);
				foreach (Charge charge in Job.Charges)
				{
					if (!charge.IsProfitShareCharge && (charge.CostDueOverseasAgent || charge.JR_IsIncludedInProfitShare))
					{
						result.Add(DocJobInvoicingJobCharge.New(charge, Factory));
					}
				}

				return result;
			}
		}

		public DocJobInvoicingJobChargeCollection ChargesNotIncludingCustomsDisbursement
		{
			get
			{
				DocJobInvoicingJobChargeCollection result = new DocJobInvoicingJobChargeCollection(Factory);
				foreach (Charge charge in Job.Charges)
				{
					if (charge.ChargeCode.PK != RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value)
					{
						result.Add(DocJobInvoicingJobCharge.New(charge, Factory));
					}
				}
				return result;
			}
		}

		public DocJobInvoicingJobChargeCollection ChargesNotIncludingCustomsDisbursementOrZeroCharges
		{
			get
			{
				DocJobInvoicingJobChargeCollection result = new DocJobInvoicingJobChargeCollection(Factory);
				foreach (Charge charge in Job.Charges)
				{
					if (charge.ChargeCode.PK != RatingDataRegistry.Instance.CustomsDisbursementChargeCode.Value &&
						charge.JR_OSSellAmt != 0)
					{
						result.Add(DocJobInvoicingJobCharge.New(charge, Factory));
					}
				}
				return result;
			}
		}

		public DocJobExchangeRateCollection ExchangeRates
		{
			get
			{
				DocJobExchangeRateCollection result = new DocJobExchangeRateCollection(Factory);

				foreach (ExchangeRate rate in Job.ExchangeRates)
				{
					result.Add(DocJobExchangeRate.New(rate, Factory));
				}

				return result;
			}
		}

		public DocJobExchangeRateCollection ExchangeRatesCodeSorted
		{
			get
			{
				DocJobExchangeRateCollection result = ExchangeRates;
				result.Sort(new SortInfo("CurrencyCode", ListSortDirection.Ascending));
				return result;
			}
		}

		public ZString LocalCurrency
		{
			get { return Job.JH_LocalCurrency; }
		}

		public ZString Code
		{
			get { return JobNum; }
		}

		public ZString Description
		{
			get { return Job.JH_JobNum; }
		}

		public ZDateTime ActualJCL
		{
			get { return Job.JH_A_JCL; }
		}

		public ZDateTime ActualJOP
		{
			get { return Job.JH_A_JOP; }
		}

		public ZDecimal AgentChargesCFX
		{
			get { return Job.JH_AgentChargesCFX; }
		}

		public ZGuid ParentID
		{
			get { return Job.JH_ParentID; }
		}

		public ZString ParentTableCode
		{
			get { return Job.JH_ParentTableCode; }
		}

		public DocBranch Branch
		{
			get { return DocBranch.New(Job.Branch, Factory); }
		}

		public DocDepartment Department
		{
			get { return DocDepartment.New(Job.Department, Factory); }
		}

		public ZString HoldReason
		{
			get { return Job.JH_HoldReason; }
		}

		public ZString JobNum
		{
			get { return Job.JH_JobNum; }
		}

		public ZString JobLocalReference
		{
			get { return Job.JH_JobLocalReference; }
		}

		public ZDecimal LocalChargesCFX
		{
			get { return Job.JH_LocalChargesCFX; }
		}

		public DocOrganisation AgentCollect
		{
			get { return DocOrganisation.New(Job.AgentCollect, Factory); }
		}

		public DocOrganisation LocalCharges
		{
			get { return DocOrganisation.New(Job.LocalCharges, Factory); }
		}

		public ZBool SingleAgentsInvoicePerConsol
		{
			get { return Job.JH_SingleAgentsInvoicePerConsol; }
		}

		public ZString Status
		{
			get { return Job.JH_Status; }
		}

		public ZString StatusDescription
		{
			get
			{
				ZString result = ZString.Empty;

				CodeDescriptionPairList statusList = new JobHeaderStatusList();
				result = statusList.GetDescriptionFromCode(Job.JH_Status).ToUpper();

				return result;
			}
		}

		public ZShort UniqueJobInvoiceNumber
		{
			get { return Job.JH_UniqueJobInvoiceNumber; }
		}

		public ZString Operator
		{
			get { return (Job.RepOps != null) ? Job.RepOps.GS_Code : ZString.Empty; }
		}

		public ZString SalesRep
		{
			get { return (Job.RepSales != null) ? Job.RepSales.GS_Code : ZString.Empty; }
		}

		public ZString JobProfitHeading
		{
			get { return Res.GetString("5054951d-7eee-473c-92aa-80eeac68385e", "{0} {1} Job Profit", TransportMode, ContainerMode); }
		}

		public ZString TransportMode
		{
			get
			{
				if (!transportMode.HasValue)
				{
					transportMode = InvoicingSupporter != null ? InvoicingSupporter.TransportMode : ZString.Empty;
				}

				return transportMode.Value;
			}
		}
		ZString? transportMode;

		public ZString ContainerMode
		{
			get
			{
				if (!containerMode.HasValue)
				{
					containerMode = InvoicingSupporter != null ? InvoicingSupporter.ContainerMode : ZString.Empty;
				}

				return containerMode.Value;
			}
		}
		ZString? containerMode;

		public ZString QuoteNumber
		{
			get { return Job.JH_TH_NKQuoteNumber; }
		}

		public ZDecimal LocalClientCFX
		{
			get { return Job.JH_LocalChargesCFX; }
		}

		public ZDecimal OverseasAgentCFX
		{
			get { return Job.JH_AgentChargesCFX; }
		}

		public ZString JobType
		{
			get
			{
				if (!jobType.HasValue)
				{
					jobType = InvoicingSupporter != null && InvoicingSupporter.ConsumerType != null ? (ZString)InvoicingSupporter.ConsumerType.Code : ZString.Empty;
				}

				return jobType.Value;
			}
		}
		ZString? jobType;

		public ZString Origin
		{
			get
			{
				if (!origin.HasValue)
				{
					origin = InvoicingSupporter != null && InvoicingSupporter.Origin != null ? InvoicingSupporter.Origin.RL_Code : ZString.Empty;
				}

				return origin.Value;
			}
		}
		ZString? origin;

		public ZString Destination
		{
			get
			{
				if (!destination.HasValue)
				{
					destination = InvoicingSupporter != null && InvoicingSupporter.Destination != null ? InvoicingSupporter.Destination.RL_Code : ZString.Empty;
				}

				return destination.Value;
			}
		}
		ZString? destination;

		IJobInvoicingSupporter InvoicingSupporter
		{
			get
			{
				Job.InitializeParentFromGenericJobWithoutSettingDefaults();
				return Job.PlugInData != null ? Job.PlugInData.InvoicingSupporter : null;
			}
		}

		public ZString JobExchangeRatesAsString
		{
			get
			{
				ZString result = ZString.Empty;

				int decimals = GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;
				foreach (DocJobExchangeRate rate in ExchangeRatesCodeSorted)
				{
					result += rate.CurrencyCode + " ";
					result += rate.BuyRate.ToString(decimals) + " ";
					result += rate.OrgType + " ";
					result += rate.Organization + " ";

					if (rate.IsCfxApplied)
					{
						result += Res.GetString("A083E2B9-5772-4C36-8C94-2246B43870D0", "CFX ") + rate.CfxPercent + " ";
						result += Res.GetString("C4C1CFB9-5D4B-498E-8057-63BCA9445F1D", "CFX Min. ") + rate.CfxMinimum;
					}

					result += System.Environment.NewLine;
				}

				return result;
			}
		}

		public ZBool IsConsolDoc
		{
			get { return isConsolDoc; }
		}

		ZBool isConsolDoc;

		public ZBool IsProfitLossDoc
		{
			get { return isProfitLossDoc; }
		}

		ZBool isProfitLossDoc;

		public ZBool PrintChargeSummary
		{
			get { return fPrintChargeSummary; }
		}

		ZBool fPrintChargeSummary;

		public ZBool PrintProfitRecognitionByDateSummary
		{
			get { return fPrintProfitRecognitionByDateSummary; }
		}

		ZBool fPrintProfitRecognitionByDateSummary;

		public ZBool PrintChargeDetail
		{
			get { return fPrintChargeDetail; }
		}

		ZBool fPrintChargeDetail;

		public ZBool PrintARInvoiceAnalysis
		{
			get { return fPrintARInvoiceAnalysis; }
		}

		ZBool fPrintARInvoiceAnalysis;

		public ZBool PrintAPInvoiceAnalysis
		{
			get { return fPrintAPInvoiceAnalysis; }
		}

		public ZString LocalTaxTitle
		{
			get { return GlbCompany.CurrentCompany.GC_IsGSTRegistered ? (NoResString)"Local(Excl Tax)" : (NoResString)"Local Value"; }
		}

		ZBool fPrintAPInvoiceAnalysis;

		public ZBool PrintJobRevenueJournalAnalysis
		{
			get { return fPrintJobRevenueJournalAnalysis; }
		}

		ZBool fPrintJobRevenueJournalAnalysis;

		#region Job Profit Document

		#region IsAllowedtoViewCharge

		bool IsAllowedtoViewCharge(Charge charge)
		{
			return IsConsolDoc ? charge.IsAllowedToViewCosts : charge.IsAllowedToViewThisCharge;
		}

		#endregion

		public DocJobInvoicingJobChargeCollection RevenueMovementsRecognized
		{
			get
			{
				if (fRevenueMovementsRecognized == null)
				{
					fRevenueMovementsRecognized = new DocJobInvoicingJobChargeCollection(Factory);
					foreach (Charge charge in Job.Charges)
					{
						if (IsAllowedtoViewCharge(charge) && (!charge.JR_LocalSellAmt.IsEmpty && IsRevenue(charge.ARLine) && !charge.ARLine.AL_ReverseDate.IsEmpty) || (!charge.JR_LocalCostAmt.IsEmpty && IsJobRevenueJournal(charge.APLine) && !charge.APLine.AL_ReverseDate.IsEmpty))
						{
							fRevenueMovementsRecognized.Add(DocJobInvoicingJobCharge.New(charge, Factory));
						}
					}
				}
				return fRevenueMovementsRecognized;
			}
		}
		DocJobInvoicingJobChargeCollection fRevenueMovementsRecognized;

		public DocJobInvoicingJobChargeCollection RevenueMovementsRecognizedForProfitShare
		{
			get
			{
				if (fRevenueMovementsRecognizedForProfitShare == null)
				{
					fRevenueMovementsRecognizedForProfitShare = new DocJobInvoicingJobChargeCollection(Factory);
					foreach (Charge charge in Job.Charges)
					{
						if (IsAllowedtoViewCharge(charge) && (!charge.JR_LocalSellAmt.IsEmpty && IsRevenue(charge.ARLine) && !charge.ARLine.AL_ReverseDate.IsEmpty) || (!charge.JR_LocalCostAmt.IsEmpty && IsJobRevenueJournal(charge.APLine) && !charge.APLine.AL_ReverseDate.IsEmpty))
						{
							fRevenueMovementsRecognizedForProfitShare.AddRange(DocJobInvoicingJobCharge.NewForJobPofit(charge, charge.ARLine, Factory));
						}
					}
				}
				return fRevenueMovementsRecognizedForProfitShare;
			}
		}
		DocJobInvoicingJobChargeCollection fRevenueMovementsRecognizedForProfitShare;

		public DocJobInvoicingJobChargeCollection CostMovementsRecognized
		{
			get
			{
				if (fCostMovementsRecognized == null)
				{
					fCostMovementsRecognized = new DocJobInvoicingJobChargeCollection(Factory);
					foreach (Charge charge in Job.Charges)
					{
						if (IsAllowedtoViewCharge(charge) && !charge.JR_LocalCostAmt.IsEmpty && IsCostOnly(charge.APLine) && !charge.APLine.AL_ReverseDate.IsEmpty)
						{
							fCostMovementsRecognized.Add(DocJobInvoicingJobCharge.New(charge, Factory));
						}
					}
				}
				return fCostMovementsRecognized;
			}
		}
		DocJobInvoicingJobChargeCollection fCostMovementsRecognized;

		public DocJobInvoicingJobChargeCollection CostMovementsRecognizedForProfitShare
		{
			get
			{
				if (fCostMovementsRecognizedForProfitShare == null)
				{
					fCostMovementsRecognizedForProfitShare = new DocJobInvoicingJobChargeCollection(Factory);
					foreach (Charge charge in Job.Charges)
					{
						if (IsAllowedtoViewCharge(charge) && !charge.JR_LocalCostAmt.IsEmpty && IsCostOnly(charge.APLine) && !charge.APLine.AL_ReverseDate.IsEmpty)
						{
							fCostMovementsRecognizedForProfitShare.AddRange(DocJobInvoicingJobCharge.NewForJobPofit(charge, charge.APLine, Factory));
						}
					}
				}
				return fCostMovementsRecognizedForProfitShare;
			}
		}
		DocJobInvoicingJobChargeCollection fCostMovementsRecognizedForProfitShare;

		#region wrapper and collection for WIP and ACR movements calculation

		public class WIPACRDummyLine : DocumentWrapper
		{
			protected WIPACRDummyLine(AccTransactionLines line, BusinessObjectFactory factoryToWrap)
				: base(line, factoryToWrap)
			{
			}

			public static WIPACRDummyLine New(AccTransactionLines line, BusinessObjectFactory factoryToWrap)
			{
				return (line == null) ? null : new WIPACRDummyLine(line, factoryToWrap);
			}

			public ZDecimal LineAmount { set; get; }

			public ZDateTime Date { set; get; }
		}

		public class WIPACRMovementsRecognized : DocumentWrapperCollection<WIPACRDummyLine>
		{
			public WIPACRMovementsRecognized(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}

		#endregion

		IJobProfitLoss ProfitLoss
		{
			get
			{
				if (profitLoss == null && Job.ProfitLoss != null && Job.ProfitLoss.Count > 0)
				{
					profitLoss = Job.ProfitLoss[0];
				}
				return profitLoss;
			}
		}
		IJobProfitLoss profitLoss;

		////WIP's created (by AL_PostDate) MINUS WIP's REVERSED (by AL_ReversedDate)
		public WIPACRMovementsRecognized WIPMovementsRecognized
		{
			get
			{
				if (fWIPMovementsRecognized == null)
				{
					fWIPMovementsRecognized = new WIPACRMovementsRecognized(Factory);
					foreach (AccTransactionLines line in FilteredLineBizObjCollection)
					{
						if (line.AL_LineType == ZArchitecture.Core.TransactionLineTypes.WIP && line.AL_PostDate.Date != line.AL_ReverseDate.Date)
						{
							//created WIPs
							if (!line.AL_PostDate.IsEmpty)
							{
								WIPACRDummyLine createdWIP = WIPACRDummyLine.New(line, Factory);
								createdWIP.LineAmount = line.AL_LineAmount;
								createdWIP.Date = line.AL_PostDate.Date;
								fWIPMovementsRecognized.Add(createdWIP);
							}
							//reversed WIPs (maybe the same)
							if (!line.AL_ReverseDate.IsEmpty)
							{
								WIPACRDummyLine reversedWIP = WIPACRDummyLine.New(line, Factory);
								reversedWIP.LineAmount = -line.AL_LineAmount;
								reversedWIP.Date = line.AL_ReverseDate.Date;
								fWIPMovementsRecognized.Add(reversedWIP);
							}
						}
					}
				}
				return fWIPMovementsRecognized;
			}
		}
		WIPACRMovementsRecognized fWIPMovementsRecognized;
		public WIPACRMovementsRecognized ACRMovementsRecognized
		{
			get
			{
				if (fACRMovementsRecognized == null)
				{
					fACRMovementsRecognized = new WIPACRMovementsRecognized(Factory);
					foreach (AccTransactionLines line in FilteredLineBizObjCollection)
					{
						if (line.AL_LineType == ZArchitecture.Core.TransactionLineTypes.Accrual && line.AL_PostDate.Date != line.AL_ReverseDate.Date)
						{
							if (!line.AL_PostDate.IsEmpty)
							{
								WIPACRDummyLine createdACR = WIPACRDummyLine.New(line, Factory);
								createdACR.LineAmount = line.AL_LineAmount;
								createdACR.Date = line.AL_PostDate.Date;
								fACRMovementsRecognized.Add(createdACR);
							}
							if (!line.AL_ReverseDate.IsEmpty)
							{
								WIPACRDummyLine reversedACR = WIPACRDummyLine.New(line, Factory);
								reversedACR.LineAmount = -line.AL_LineAmount;
								reversedACR.Date = line.AL_ReverseDate.Date;
								fACRMovementsRecognized.Add(reversedACR);
							}
						}
					}
				}
				return fACRMovementsRecognized;
			}
		}
		WIPACRMovementsRecognized fACRMovementsRecognized;

		public ZDecimal TotalRevenueMovementsRecognized
		{
			get
			{
				return ProfitLoss.TotalRevenueRecognized;
			}
		}

		public ZDecimal TotalCostMovementsRecognized
		{
			get
			{
				return ProfitLoss.TotalCostRecognized * -1;
			}
		}

		public ZDecimal TotalWIPMovementsRecognized
		{
			get
			{
				return ProfitLoss.TotalWIPRecognized;
			}
		}

		public ZDecimal TotalACRMovementsRecognized
		{
			get
			{
				return ProfitLoss.TotalAccrualRecognized * -1;
			}
		}

		public ZDecimal TotalJobProfitRecognizedInGL
		{
			get
			{
				return TotalRevenueMovementsRecognized + TotalWIPMovementsRecognized - TotalCostMovementsRecognized - TotalACRMovementsRecognized;
			}
		}

		public ZDecimal TotalJobProfitNotRecognizedInGL
		{
			get
			{
				return RevenueNotRecognized + WIPNotRecognized - CostNotRecognized - ACRNotRecognized;
			}
		}

		public DocJobLineDetailCollection AllLines
		{
			get
			{
				if (fAllLines == null)
				{
					fAllLines = new DocJobLineDetailCollection(Factory);

					foreach (AccTransactionLines line in FilteredLineBizObjCollection)
					{
						if (!IsCancelled(line))
						{
							DocJobLineDetail lineWrapper = DocJobLineDetail.New(line, Factory);
							fAllLines.Add(lineWrapper);
						}
					}
				}

				return fAllLines;
			}
		}

		DocJobLineDetailCollection fAllLines;

		public DocJobLineDetailCollection ARLines
		{
			get
			{
				if (fARLines == null)
				{
					fARLines = new DocJobLineDetailCollection(Factory);

					foreach (AccTransactionLines line in FilteredLineBizObjCollection)
					{
						if (IsRevenue(line) && !IsCancelled(line))
						{
							DocJobLineDetail lineWrapper = DocJobLineDetail.New(line, Factory);
							fARLines.Add(lineWrapper);
						}
					}
				}

				return fARLines;
			}
		}

		DocJobLineDetailCollection fARLines;

		public DocJobLineDetailCollection ARLinesForTaxExpense
		{
			get
			{
				if (fARLinesForTaxExpense == null)
				{
					fARLinesForTaxExpense = new DocJobLineDetailCollection(Factory);

					foreach (AccTransactionLines line in FilteredLineBizObjCollection)
					{
						if (IsRevenue(line) && !IsCancelled(line))
						{
							AddTaxExpenseLineIfApplicable(line, fARLinesForTaxExpense);
						}
					}
				}

				return fARLinesForTaxExpense;
			}
		}

		DocJobLineDetailCollection fARLinesForTaxExpense;
		public ZBool DisplayNoARInvoicesMessage
		{
			get { return ARLines.Count == 0; }
		}

		public DocJobLineDetailCollection APLines
		{
			get
			{
				if (fAPLines == null)
				{
					fAPLines = new DocJobLineDetailCollection(Factory);

					foreach (AccTransactionLines line in FilteredLineBizObjCollection)
					{
						if (IsCostOnly(line) && !IsCancelled(line))
						{
							DocJobLineDetail lineWrapper = DocJobLineDetail.New(line, Factory);
							fAPLines.Add(lineWrapper);
						}
					}
				}

				return fAPLines;
			}
		}

		DocJobLineDetailCollection fAPLines;

		public DocJobLineDetailCollection APLinesForTaxExpense
		{
			get
			{
				if (fAPLinesForTaxExpense == null)
				{
					fAPLinesForTaxExpense = new DocJobLineDetailCollection(Factory);

					foreach (AccTransactionLines line in FilteredLineBizObjCollection)
					{
						if (IsCostOnly(line) && !IsCancelled(line))
						{
							AddTaxExpenseLineIfApplicable(line, fAPLinesForTaxExpense);
						}
					}
				}

				return fAPLinesForTaxExpense;
			}
		}

		DocJobLineDetailCollection fAPLinesForTaxExpense;

		void AddTaxExpenseLineIfApplicable(AccTransactionLines line, DocJobLineDetailCollection collection)
		{
			if (line.IsTaxExpense)
			{
				var lineToDisplay = DocJobLineDetail.New(line, Factory);
				lineToDisplay.IsTaxExpense = true;
				collection.Add(lineToDisplay);
			}
		}

		public DocJobLineDetailCollection APExcludeJRJLines
		{
			get
			{
				if (fAPExcludeJRJLines == null)
				{
					fAPExcludeJRJLines = GetLinesExcludeJRJLines(APLines);
				}
				return fAPExcludeJRJLines;
			}
		}

		DocJobLineDetailCollection fAPExcludeJRJLines;

		public DocJobLineDetailCollection APExcludeJRJLinesForProfitShare
		{
			get
			{
				if (fAPExcludeJRJLinesForProfitShare == null)
				{
					fAPExcludeJRJLinesForProfitShare = GetLinesExcludeJRJLines(APLines);
					fAPExcludeJRJLinesForProfitShare.AddRange(GetLinesExcludeJRJLines(APLinesForTaxExpense));
				}
				return fAPExcludeJRJLinesForProfitShare;
			}
		}

		DocJobLineDetailCollection fAPExcludeJRJLinesForProfitShare;

		public DocJobLineDetailCollection ARExcludeJRJLines
		{
			get
			{
				if (fARExcludeJRJLines == null)
				{
					fARExcludeJRJLines = GetLinesExcludeJRJLines(ARLines);
				}
				return fARExcludeJRJLines;
			}
		}

		DocJobLineDetailCollection fARExcludeJRJLines;

		public DocJobLineDetailCollection ARExcludeJRJLinesForProfitShare
		{
			get
			{
				if (fARExcludeJRJLinesForProfitShare == null)
				{
					fARExcludeJRJLinesForProfitShare = GetLinesExcludeJRJLines(ARLines);
					fARExcludeJRJLinesForProfitShare.AddRange(GetLinesExcludeJRJLines(ARLinesForTaxExpense));
				}
				return fARExcludeJRJLinesForProfitShare;
			}
		}

		DocJobLineDetailCollection fARExcludeJRJLinesForProfitShare;

		DocJobLineDetailCollection GetLinesExcludeJRJLines(DocJobLineDetailCollection lines)
		{
			var result = new DocJobLineDetailCollection(Factory);

			foreach (var line in lines.Cast<DocJobLineDetail>().Where(x => x.TransactionHeader == null || (x.TransactionHeader != null && x.TransactionHeader.TransactionType != TransactionTypes.JobRevenueJournal)))
			{
				result.Add(line);
			}

			return result;
		}

		public DocJobLineDetailCollection JRJLines
		{
			get
			{
				if (fJRJLines == null)
				{
					fJRJLines = GetJRJLines(AllLines);
				}
				return fJRJLines;
			}
		}

		DocJobLineDetailCollection fJRJLines;

		DocJobLineDetailCollection GetJRJLines(DocJobLineDetailCollection lines)
		{
			var result = new DocJobLineDetailCollection(Factory);

			foreach (var line in lines.Cast<DocJobLineDetail>().Where(x => x.TransactionHeader != null && x.TransactionHeader.TransactionType == TransactionTypes.JobRevenueJournal))
			{
				result.Add(line);
			}

			return result;
		}

		public ZBool DisplayNoAPInvoicesMessage
		{
			get { return APLines.Count == 0; }
		}

		public ZBool DisplayNoARExcludeJRJInvoicesMessage
		{
			get { return ARExcludeJRJLines.Count == 0; }
		}

		public ZBool DisplayNoAPExcludeJRJInvoicesMessage
		{
			get { return APExcludeJRJLines.Count == 0; }
		}

		internal AccTransactionLinesCollection LineBizObjCollection
		{
			get
			{
				if (fLineBizObjCollection == null)
				{
					var jobsToLookup = new[] { JobPK }.Union(((Job)WrappedObject).ChildJobPKs);
					var sqlFilter = new ZQuery(AccTransactionLinesSchema.AL_JH, jobsToLookup);
					sqlFilter.AddToFilter(AccTransactionLinesSchema.AL_GC, Job.JH_GC);
					var lLineBizObjCollection = new AccTransactionLinesCollection(Factory, sqlFilter);
					lLineBizObjCollection.Load();
					fLineBizObjCollection = lLineBizObjCollection;
				}

				return fLineBizObjCollection;
			}
		}

		AccTransactionLinesCollection fLineBizObjCollection;

		internal AccTransactionLinesCollection FilteredLineBizObjCollection
		{
			get
			{
				if (filteredLineBizObjCollection == null)
				{
					filteredLineBizObjCollection = new AccTransactionLinesCollection(Factory);
					filteredLineBizObjCollection.AddRange(LineBizObjCollection.Cast<AccTransactionLines>().Where(line => IsAllowedToViewlineFromCurrentBranch(Job, line.Branch, line.Department)));
				}

				return filteredLineBizObjCollection;
			}
		}

		AccTransactionLinesCollection filteredLineBizObjCollection;

		protected internal bool IsCancelled(AccTransactionLines line)
		{
			bool result = false;

			if (IsAccrualOrWip(line))
			{
				result = (!line.AL_ReverseDate.IsEmpty && line.AL_ReverseDate.IsValid);
			}
			else if (IsCostOrRevenue(line) && line.TransactionHeader != null)
			{
				result = line.TransactionHeader.AH_IsCancelled;
			}

			return result;
		}

		protected internal bool IsAccrualOrWip(AccTransactionLines line)
		{
			return line.AL_LineType == ZArchitecture.Core.TransactionLineTypes.WIP ||
				line.AL_LineType == ZArchitecture.Core.TransactionLineTypes.Accrual;
		}

		protected internal bool IsCostOrRevenue(AccTransactionLines line)
		{
			return IsRevenue(line) || IsCostOrJobRevenueJournal(line);
		}

		protected internal bool IsRevenue(AccTransactionLines line)
		{
			return line != null && line.AL_LineType == ZArchitecture.Core.TransactionLineTypes.Revenue;
		}

		bool IsJobRevenueJournal(AccTransactionLines line)
		{
			return line != null && line.AL_LineType == TransactionLineTypes.Revenue && line.TransactionHeader != null && line.TransactionHeader.AH_TransactionType == TransactionTypes.JobRevenueJournal;
		}

		protected internal bool IsCostOrJobRevenueJournal(AccTransactionLines line)
		{
			return line != null && (line.AL_LineType == TransactionLineTypes.Cost ||
				IsJobRevenueJournal(line));
		}

		protected internal bool IsCostOnly(AccTransactionLines line)
		{
			return line != null && line.AL_LineType == ZArchitecture.Core.TransactionLineTypes.Cost;
		}

		public ZDecimal JH_TotalRevenue
		{
			get
			{
				return Job.JH_TotalRevenue;
			}
		}

		public ZDecimal TotalRevenue
		{
			get
			{
				return ProfitLoss.TotalRevenue;
			}
		}

		public ZDecimal TotalWip
		{
			get
			{
				return ProfitLoss.TotalWIP;
			}
		}

		public ZDecimal TotalCost
		{
			get
			{
				return ProfitLoss.TotalCost * -1;
			}
		}

		public ZDecimal TotalAccrual
		{
			get
			{
				return ProfitLoss.TotalAccrual * -1;
			}
		}

		public ZDecimal TotalIncome
		{
			get { return TotalRevenue + TotalWip; }
		}

		public ZDecimal TotalExpense
		{
			get { return TotalCost + TotalAccrual; }
		}

		public ZDecimal TotalRealisedAmount
		{
			get { return TotalRevenue - TotalCost; }
		}

		public ZDecimal TotalEstimatedAmount
		{
			get { return TotalWip - TotalAccrual; }
		}

		public ZDecimal TotalProfit
		{
			get { return TotalIncome - TotalExpense; }
		}

		public ZString RevenueRecognitionDates
		{
			get { return Job.RevenueRecognitionDates; }
		}

		public ZDecimal RevenueNotRecognized
		{
			get
			{
				return ProfitLoss.TotalRevenueNotRecognized;
			}
		}

		public ZDecimal CostNotRecognized
		{
			get
			{
				return ProfitLoss.TotalCostNotRecognized * -1;
			}
		}

		public ZDecimal WIPNotRecognized
		{
			get
			{
				return ProfitLoss.TotalWIPNotRecognized;
			}
		}

		public ZDecimal ACRNotRecognized
		{
			get
			{
				return ProfitLoss.TotalAccrualNotRecognized * -1;
			}
		}

		#endregion

		#region Profit/Cost Margin

		public ZDecimal ProfitCostMargin
		{
			get
			{
				return TotalExpense != 0 ? (TotalProfit / TotalExpense) : 0;
			}
		}

		#endregion

		#region Profit/Rev Margin

		public ZDecimal ProfitRevMargin
		{
			get
			{
				return TotalIncome != 0 ? (TotalProfit / TotalIncome) : 0;
			}
		}

		#endregion

		#region Shipment, Declaration, Load List & Transport Wrapper

		public ZBool IsShipment
		{
			get { return Job.JH_ParentTableCode == JobShipmentSchema.Constants.Prefix; }
		}

		public DocShipment Shipment
		{
			get
			{
				DocShipment result = null;

				if (IsShipment)
				{
					var shipment = Factory.Load<ForwardingShipment>(Job.JH_ParentID);
					if (shipment != null)
					{
						result = DocShipment.New(shipment, Factory);
					}
				}

				return result;
			}
		}

		public ZBool IsDeclaration
		{
			get { return Job.JH_ParentTableCode == JobDeclarationSchema.Constants.Prefix; }
		}

		public DocBaseJobDeclaration Declaration
		{
			get
			{
				DocBaseJobDeclaration result = null;

				if (IsDeclaration)
				{
					var declaration = Factory.Load<BaseJobDeclaration>(Job.JH_ParentID);
					if (declaration != null)
					{
						result = DocBaseJobDeclaration.New(declaration, Factory);
					}
				}

				return result;
			}
		}

		public ZBool IsLoadList
		{
			get { return Job.JH_ParentTableCode == JobConsolSchema.Constants.Prefix; }
		}

		public DocLoadListConsol LoadList
		{
			get
			{
				if (IsLoadList)
				{
					var loadListConsol = Factory.Load<CFSLoadListConsol>(Job.JH_ParentID);
					if (loadListConsol != null)
					{
						return DocLoadListConsol.New(loadListConsol, Factory);
					}
				}

				return null;
			}
		}

		public ZBool IsTransport
		{
			get { return Job.JH_ParentTableCode == JobCartageSchema.Constants.Prefix; }
		}

		public DocCommonCartage Cartage
		{
			get
			{
				DocCommonCartage result = null;

				if (IsTransport)
				{
					var transport = Factory.Load<CommonCartage>(Job.JH_ParentID);
					if (transport != null)
					{
						result = DocCommonCartage.New(transport, Factory);
					}
				}

				return result;
			}
		}

		#endregion

		#region Implementation

		Job Job
		{
			get { return (Job)WrappedObject; }
		}

		bool IsAllowedToViewlineFromCurrentBranch(Job job, GlbBranch branch, GlbDepartment department)
		{
			var result = true;
			var haslineViewingRestriction = true;
			if (IsConsolDoc)
			{
				haslineViewingRestriction = job != null && !job.IsAllowedToViewConsolCostsFromOtherBranchesOrDepartments;
			}
			else
			{
				haslineViewingRestriction = job != null && !job.IsAllowedToViewChargesFromOtherBranchesOrDepartments;
			}
			if (haslineViewingRestriction && branch != null && department != null)
			{
				if (branch != GlbBranch.CurrentBranch || department != GlbDepartment.CurrentDepartment)
				{
					result = AllowedToLogin(branch, department);
				}
			}
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Combining Cache key, not related to GUI")]
		bool AllowedToLogin(GlbBranch branch, GlbDepartment department)
		{
			return Factory.GetCachedValue("Login BRN:" + branch.GB_Code + " DEP:" + department.GE_Code, delegate
			{
				var security = new UserLoginController().GetSecurityForUser(GlbStaff.CurrentUser.GS_LoginName, branch.PK.ToGuid(), department.PK.ToGuid());
				return security.Login.IsAllowed;
			});
		}

		#endregion
	}
}
