using System.Collections;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.MasterFiles.Business;
using Bill = Enterprise.Customs.AU.Declaration.Business.Bill;
using InvoiceHeaderActiveCollection = Enterprise.Customs.AU.Declaration.Business.InvoiceHeaderActiveCollection;
using InvoiceLineCompleteCollection = Enterprise.Customs.AU.Declaration.Business.InvoiceLineCompleteCollection;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocDeclaration : DocBaseJobDeclaration
	{
		protected DocDeclaration(JobDeclaration jobDeclaration, BusinessObjectFactory factoryToWrap)
			: base(jobDeclaration, factoryToWrap)
		{
		}

		public static DocDeclaration New(JobDeclaration jobDeclaration, BusinessObjectFactory factoryToWrap)
		{
			DocDeclaration result = null;

			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(jobDeclaration, factoryToWrap);
			}
			else if (jobDeclaration != null)
			{
				result = new DocDeclaration(jobDeclaration, factoryToWrap);
			}

			return result;
		}

		public DocJobComInvoiceHeader FirstInvoice
		{
			get { return (JobDeclaration.Invoices.Count == 0) ? null : DocJobComInvoiceHeader.New(JobDeclaration.Invoices[0], Factory); }
		}

		#region Overrides

		public override ZString AUCusEntryNumberType
		{
			get
			{
				return JobDeclaration.ExportEntryNumber != null ? JobDeclaration.ExportEntryNumber.CE_EntryType : ZString.Empty;
			}
		}

		protected override DocBaseCusContainerCollection CreateCusContainerCollection(ICusContainerCollection<BaseCusContainer> collectionToWrap)
		{
			return new DocCusContainerCollection(collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceLineCollection CreateJobComInvoiceLineCollection(Enterprise.Customs.Business.InvoiceLineCompleteCollection collectionToWrap)
		{
			return new DocJobComInvoiceLineCollection((InvoiceLineCompleteCollection)collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceGroupHeaderCollection CreateJobComInvoiceGroupHeaderCollection(IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> collectionToWrap)
		{
			return new DocJobComInvoiceGroupHeaderCollection((IJobComInvoiceGroupHeaderCollection<JobComInvoiceGroupHeader>)collectionToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceHeader CreateJobComInvoiceHeader(BaseJobComInvoiceHeader invoiceToWrap)
		{
			return DocJobComInvoiceHeader.New((JobComInvoiceHeader)invoiceToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceGroupHeader CreateJobComInvoiceGroupHeader(BaseJobComInvoiceGroupHeader invoiceGroupToWrap)
		{
			return DocJobComInvoiceGroupHeader.New((JobComInvoiceGroupHeader)invoiceGroupToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceHeaderCollection CreateAllInvoiceHeaderCollection(Enterprise.Customs.Business.InvoiceHeaderActiveCollection collectionToWrap)
		{
			return new DocJobComInvoiceHeaderCollection((InvoiceHeaderActiveCollection)collectionToWrap, Factory);
		}

		#endregion

		#region Wrapper Fields

		public DocJobComInvoiceHeader InvoiceHeader
		{
			get { return (DocJobComInvoiceHeader)InvoiceHeaderInternal; }
		}

		public DocJobComInvoiceGroupHeader ActiveInvoiceHeader
		{
			get { return (DocJobComInvoiceGroupHeader)ActiveInvoiceHeaderGroupInternal; }
		}

		#endregion

		#region ZBool Fields

		public ZBool ForcePrimeEnclosure
		{
			get { return JobDeclaration.JE_ForcePrimeEnclosure; }
		}

		public ZBool NeedsConfirmation
		{
			get { return JobDeclaration.NeedsConfirmation; }
		}

		public ZBool IsSAC
		{
			get { return JobDeclaration.IsSAC; }
		}

		#endregion

		#region ZString Fields

		public ZString MasterBills
		{
			get { return JobDeclaration.MasterBillsCommaSeparated; }
		}

		public ZString DeclarationReferenceOrHashesIfEmpty
		{
			get { return JobDeclaration.DeclarationReferenceOrHashesIfEmpty; }
		}

		public ZString EFTPaymentAdviceDocumentType
		{
			get { return (IsSAC) ? "SAC" : "FID"; }
		}

		#endregion

		#region AUCusEntryHeaderEntryPrintLines

		public ZString[] AUCusEntryHeaderEntryPrintLines
		{
			get { return GetAUCusEntryHeaderEntryPrintLines(false); }
		}

		public ZString[] AUCusEntryHeaderEntryPrintLinesPortrait
		{
			get { return GetAUCusEntryHeaderEntryPrintLines(true); }
		}

		ZString[] GetAUCusEntryHeaderEntryPrintLines(bool isPortrait)
		{
			ArrayList result = new ArrayList();
			foreach (DocCusEntryHeader header in EntryHeaders)
			{
				ZString[] entryPrintLines = header.GetEntryPrintLines(isPortrait);
				result.AddRange(entryPrintLines);
			}
			return (ZString[])result.ToArray(typeof(ZString));
		}

		#endregion

		#region Collections
		public DocJobComInvoiceHeaderCollection AllInvoiceHeaders
		{
			get { return (DocJobComInvoiceHeaderCollection)InvoiceHeadersInternal; }
		}

		public DocCusEntryHeaderCollection EntryHeaders
		{
			get { return new DocCusEntryHeaderCollection(JobDeclaration.CustomsEntryHeaders, Factory); }
		}

		//Need to remove this.
		public DocCusEntryHeaderCollection AUCusEntryHeader
		{
			get { return EntryHeaders; }
		}

		public DocCusContainerCollection Containers
		{
			get { return (DocCusContainerCollection)ContainersInternal; }
		}

		public DocJobComInvoiceLineCollection InvoiceLines
		{
			get { return (DocJobComInvoiceLineCollection)InvoiceLinesInternal; }
		}

		public DocJobComInvoiceLineCollection InvoiceLinesSortedByLineNo
		{
			get { return (DocJobComInvoiceLineCollection)InvoiceLinesSortedByLineNoInternal; }
		}

		public DocJobComInvoiceLineCollection DrawbackInvoiceLinesSortedByLineNo
		{
			get
			{
				DocJobComInvoiceLineCollection result = new DocJobComInvoiceLineCollection(Factory);
				foreach (DocJobComInvoiceLine line in InvoiceLinesSortedByLineNo)
				{
					if (line.DrawbackClaimAmount > 0)
					{
						result.Add(line);
					}
				}
				return result;
			}
		}

		public DocJobComInvoiceLineCollection AllInvoiceLines
		{
			get
			{
				DocJobComInvoiceLineCollection result = new DocJobComInvoiceLineCollection(Factory);
				foreach (DocCusEntryHeader cusEntry in AUCusEntryHeader)
				{
					foreach (DocJobComInvoiceLine line in cusEntry.InvoiceLines)
					{
						result.Add(line);
					}
				}
				result.Sort("SortString", ListSortDirection.Ascending);
				return result;
			}
		}

		public DocJobComInvoiceLineCollection WarehouseInvoiceLines
		{
			get
			{
				DocJobComInvoiceLineCollection result = new DocJobComInvoiceLineCollection(Factory);
				foreach (DocJobComInvoiceLine line in InvoiceLinesSortedByLineNo)
				{
					if (((JobComInvoiceLine)line.WrappedObject).IsGoingIntoBondedWarehouse || JobDeclaration.IsExWarehouse)
					{
						result.Add(line);
					}
				}
				return result;
			}
		}

		// this dirty nasty filthy hack is because the document engine requires a collection to be navigated, never a single business object
		public DocJobInvoicingJobCollection SingleJobInvoicingJob
		{
			get
			{
				DocJobInvoicingJobCollection result = new DocJobInvoicingJobCollection(Factory);
				DocJobInvoicingJob docJob = JobInvoicingJob;
				if (docJob != null)
				{
					result.Add(docJob);
				}
				else
				{
					// this dirty nasty filthy hack is because the document engine doesn't support a zero-element collection here

					var job = (Job)new JobHeader.Loader(new BusinessObjectFactory(), JobDeclaration).TryCreate();
					result.Add(DocJobInvoicingJob.New(job, Factory));
				}
				return result;
			}
		}

		#endregion

		#region Summary Commercial Invoice

		public ZDecimal TotalIncludedBuyingCommission
		{
			get
			{
				if (fTotalIncludedBuyingCommission == -1)
				{
					GetAmountsForAUSpecificCosts();
				}

				return fTotalIncludedBuyingCommission;
			}
		}

		public ZDecimal TotalExcludedBuyingCommission
		{
			get
			{
				if (fTotalExcludedBuyingCommission == -1)
				{
					GetAmountsForAUSpecificCosts();
				}

				return fTotalExcludedBuyingCommission;
			}
		}

		public ZDecimal TotalIncludedOtherCommission
		{
			get
			{
				if (fTotalIncludedOtherCommission == -1)
				{
					GetAmountsForAUSpecificCosts();
				}

				return fTotalIncludedOtherCommission;
			}
		}

		public ZDecimal TotalExcludedOtherCommission
		{
			get
			{
				if (fTotalExcludedOtherCommission == -1)
				{
					GetAmountsForAUSpecificCosts();
				}

				return fTotalExcludedOtherCommission;
			}
		}

		protected ZDecimal fTotalIncludedBuyingCommission = -1;
		protected ZDecimal fTotalExcludedBuyingCommission = -1;
		protected ZDecimal fTotalIncludedOtherCommission = -1;
		protected ZDecimal fTotalExcludedOtherCommission = -1;

		protected void GetAmountsForAUSpecificCosts()
		{
			fTotalIncludedBuyingCommission = 0;
			fTotalExcludedBuyingCommission = 0;
			fTotalIncludedOtherCommission = 0;
			fTotalExcludedOtherCommission = 0;

			foreach (DocJobComInvoiceHeader header in AllInvoiceHeaders)
			{
				fTotalIncludedBuyingCommission += AddAmounts(header.IncludedBuyingCommission, header.InvoiceCurr, header.IncludedBuyingCommissionCurrency);
				fTotalExcludedBuyingCommission += AddAmounts(header.ExcludedBuyingCommission, header.InvoiceCurr, header.ExcludedBuyingCommissionCurrency);
				fTotalIncludedOtherCommission += AddAmounts(header.IncludedOtherCommission, header.InvoiceCurr, header.IncludedOtherCommissionCurrency);
				fTotalExcludedOtherCommission += AddAmounts(header.ExcludedOtherCommission, header.InvoiceCurr, header.ExcludedOtherCommissionCurrency);
			}
		}
		#endregion

		public ZString IsDrawbackDeclarationMethodA
		{
			get { return JobDeclaration.DrawbackHeaderAssesmentMethod == JobDeclaration.DrawbackAssessmentMethods.ActualShipment ? "Y" : ""; }
		}

		public ZString OwnerCode
		{
			get { return JobDeclaration.OwnerCode.Replace(" ", ""); }
		}

		public ZString BrokerageID
		{
			get { return Environment.Env.Registry.BrokerageID; }
		}

		public ZString UserSiteID
		{
			get { return Environment.Env.Registry.AUCustoms.LocalCustomsBranchIdentifier; }
		}

		public ZString BranchBoxNo
		{
			get { return JobDeclaration.BranchBoxNo; }
		}

		public ZString LloydsNo
		{
			get { return JobDeclaration.VesselNumber; }
		}

		public ZString VoyageNo
		{
			get
			{
				var voyageNo = ZString.Empty;
				if (JobDeclaration.IsImportCMR)
				{
					voyageNo = JobDeclaration.JE_VoyageFlightNo;
				}
				else
				{
					if (JobDeclaration.CleanVoyageNumber.IsValid)
					{
						voyageNo = JobDeclaration.CleanVoyageNumber;
					}
					else if (JobDeclaration.JE_VoyageFlightNo.IsValid)
					{
						voyageNo = JobDeclaration.JE_VoyageFlightNo;
					}
				}
				return voyageNo;
			}
		}

		public ZString AddInfoHartHidden
		{
			get
			{
				return (JobDeclaration.AddInfo != null) ? JobDeclaration.AddInfo.ZA_HART_Hidden : ZString.Empty;
			}
		}

		public ZString PaidUnderProtestStatement
		{
			get
			{
				return JobDeclaration.JE_PaidUnderProtestStatement;
			}
		}

		public ZString OutstandingDescription
		{
			get
			{
				var result = ZString.Empty;
				if (JobDeclaration.HasOutstandingAmendment)
				{
					result = "(Amendment detected, NOT Lodged.) ";
				}
				else if (JobDeclaration.HasOutstandingAmendmentNotQueued)
				{
					result = "(Saved without sending amendment, NOT Lodged.) ";
				}
				else if (JobDeclaration.HasOutstandingManualAmendments)
				{
					result = "(Amended through CI, Details may not match ICS.)";
				}
				else if (JobDeclaration.HasOutstandingFailedAmendments)
				{
					result = "(Amendment Failed, Details may not match ICS.)";
				}
				return result;
			}
		}

		public ZBool IsContainerised
		{
			get { return JobDeclaration.IsContainerised; }
		}

		public ZBool IsSOFADeclaration
		{
			get { return JobDeclaration.IsSOFADeclaration; }
		}

		public ZBool IsMail
		{
			get { return JobDeclaration.TransportMode == Core.Constants.TransportModes.Mail; }
		}

		public ZBool ConsignRefNumberEntered
		{
			get { return JobDeclaration.Bills.Cast<Bill>().Any(x => !x.CU_fPartShipConsignmentReference.IsEmpty); }
		}

		public ZString IsDrawbackDeclarationMethodB
		{
			get { return JobDeclaration.DrawbackHeaderAssesmentMethod == JobDeclaration.DrawbackAssessmentMethods.RepresentativeShipment ? "Y" : ""; }
		}

		public ZString IsDrawbackDeclarationMethodC
		{
			get { return JobDeclaration.DrawbackHeaderAssesmentMethod == JobDeclaration.DrawbackAssessmentMethods.Imputation ? "Y" : ""; }
		}

		public ZString IsDrawbackDeclarationAmberReasonC
		{
			get { return JobDeclaration.DrawbackHeaderAmberReasonCode == JobDeclaration.DrawbackAmberReasonTypes.Calculation ? "Y" : ""; }
		}

		public ZString IsDrawbackDeclarationAmberReasonD
		{
			get { return JobDeclaration.DrawbackHeaderAmberReasonCode == JobDeclaration.DrawbackAmberReasonTypes.Declaration ? "Y" : ""; }
		}

		public ZString IsDrawbackDeclarationAmberReasonT
		{
			get { return JobDeclaration.DrawbackHeaderAmberReasonCode == JobDeclaration.DrawbackAmberReasonTypes.Time ? "Y" : ""; }
		}

		public ZString AmberReasonStatement
		{
			get { return JobDeclaration.JE_AmberStatement; }
		}

		public ZString DrawbackCliamContactName
		{
			get { return MasterFiles.Business.GlbStaff.CurrentUser.GS_FullName; }
		}

		public ZString DrawbackContactPhoneNumber
		{
			get { return JobDeclaration.DrawbackContactPhoneNumber_Formatted; }
		}

		public ZString DrawbackContactEMail
		{
			get { return JobDeclaration.DrawbackContactEMail; }
		}

		public ZDecimal TotalDrawbackClaimAmount
		{
			get { return JobDeclaration.TotalDrawbackClaimAmount; }
		}

		public ZDecimal TotalDrawbackMethodAAmount
		{
			get { return JobDeclaration.TotalDrawbackMethodAAmount; }
		}

		public ZDecimal TotalDrawbackMethodBAmount
		{
			get { return JobDeclaration.TotalDrawbackMethodBAmount; }
		}

		public ZDecimal TotalDrawbackMethodCAmount
		{
			get { return JobDeclaration.TotalDrawbackMethodCAmount; }
		}

		public ZString BranchID
		{
			get { return Environment.Env.Registry.AUCustoms.LocalCustomsBranchIdentifier; }
		}

		public ZString IsPaymentPartyBroker
		{
			get { return JobDeclaration.JE_PaymentMethod == JobDeclaration.PaymentMethods.Broker ? "Y" : ""; }
		}

		public ZString IsPaymentPartyDrawbackClaimant
		{
			get { return JobDeclaration.JE_PaymentMethod == JobDeclaration.PaymentMethods.DrawbackClaimant ? "Y" : ""; }
		}

		public ZString IsDrawbackQID283Yes
		{
			get
			{
				CMRCusEntryCPDec cPDec = JobDeclaration.DrawbackQuestions.GetQuestionWithID(283);
				return cPDec != null && cPDec.IsAnswered && cPDec.IsYes ? "Y" : "";
			}
		}

		public ZString IsDrawbackQID283No
		{
			get
			{
				CMRCusEntryCPDec cPDec = JobDeclaration.DrawbackQuestions.GetQuestionWithID(283);
				return cPDec != null && cPDec.IsAnswered && !cPDec.IsYes ? "Y" : "";
			}
		}

		public ZString IsDrawbackQID284Yes
		{
			get
			{
				CMRCusEntryCPDec cPDec = JobDeclaration.DrawbackQuestions.GetQuestionWithID(284);
				return cPDec != null && cPDec.IsAnswered && cPDec.IsYes ? "Y" : "";
			}
		}

		public ZString IsDrawbackQID284No
		{
			get
			{
				CMRCusEntryCPDec cPDec = JobDeclaration.DrawbackQuestions.GetQuestionWithID(284);
				return cPDec != null && cPDec.IsAnswered && !cPDec.IsYes ? "Y" : "";
			}
		}

		public ZString IsDrawbackQID285Yes
		{
			get
			{
				CMRCusEntryCPDec cPDec = JobDeclaration.DrawbackQuestions.GetQuestionWithID(285);
				return cPDec != null && cPDec.IsAnswered && cPDec.IsYes ? "Y" : "";
			}
		}

		public ZString IsDrawbackQID285No
		{
			get
			{
				CMRCusEntryCPDec cPDec = JobDeclaration.DrawbackQuestions.GetQuestionWithID(285);
				return cPDec != null && cPDec.IsAnswered && !cPDec.IsYes ? "Y" : "";
			}
		}

		public ZString IsDrawbackQID286Yes
		{
			get
			{
				CMRCusEntryCPDec cPDec = JobDeclaration.DrawbackQuestions.GetQuestionWithID(286);
				return cPDec != null && cPDec.IsAnswered && cPDec.IsYes ? "Y" : "";
			}
		}

		public ZString IsDrawbackQID286No
		{
			get
			{
				CMRCusEntryCPDec cPDec = JobDeclaration.DrawbackQuestions.GetQuestionWithID(286);
				return cPDec != null && cPDec.IsAnswered && !cPDec.IsYes ? "Y" : "";
			}
		}

		public ZString IsDrawbackQID287Yes
		{
			get
			{
				CMRCusEntryCPDec cPDec = JobDeclaration.DrawbackQuestions.GetQuestionWithID(287);
				return cPDec != null && cPDec.IsAnswered && cPDec.IsYes ? "Y" : "";
			}
		}

		public ZString IsDrawbackQID287No
		{
			get
			{
				CMRCusEntryCPDec cPDec = JobDeclaration.DrawbackQuestions.GetQuestionWithID(287);
				return cPDec != null && cPDec.IsAnswered && !cPDec.IsYes ? "Y" : "";
			}
		}

		public ZString IsDrawbackQID999
		{
			get
			{
				CMRCusEntryCPDec cPDec = JobDeclaration.DrawbackQuestions.GetQuestionWithID(999);
				return cPDec != null && cPDec.IsAnswered && cPDec.IsYes ? "Y" : "";
			}
		}

		#region Implementation

		protected delegate DocDeclaration NewDelegate(JobDeclaration declaration, BusinessObjectFactory factoryToWrap);
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		protected JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)WrappedObject; }
		}

		protected override DocBaseCusEntryHeaderCollection CreateNewEntryHeadersCollection(
			ICusEntryHeaderCollection<Enterprise.Customs.Business.CusEntryHeader> collectionToWrap)
		{
			return new DocCusEntryHeaderCollection(collectionToWrap, Factory);
		}

		#endregion
	}
}
