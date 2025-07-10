using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.Shared;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class ConsolidatedDeclaration : Customs.Business.ConsolidatedDeclaration
		, Integration.Customs.AU.IConsolidatedDeclaration
		, IAddInfo
		, ICPQAAttachee
	{
		public ConsolidatedDeclaration(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CRD_ApplicationCode = ApplicationCodes.CMR;
		}

		public override FilterBusinessObjectDefaults DefaultAttachDeclarationFilter => DefaultConsolidatedDeclarationFilter.GetDefaultConsolidatedDeclarationFilter(LeadDeclaration);

		protected override Customs.Business.CusReconBase.CusReconDeclarationValidation GetNewValidation()
		{
			return new ConsolidatedDeclarationValidation(this);
		}

		protected override IConsolidatedJobDeclarationCollection<BaseJobDeclaration> CreateNewJobDeclarationCollection() => new ConsolidatedJobDeclarationCollection<JobDeclaration>(this);

		[List(nameof(LeadDeclaration) + "." + nameof(BaseJobDeclaration.Lookups) + "." + nameof(JobDeclarationLookups.MessageSubTypeList))]
		public ZString EntryStyle => JobDeclarations.IsCongruentOn(dec => dec.JE_DateOfArrival, dec => dec.JE_VoyageFlightNo) ? LeadDeclaration?.JE_MessageSubType ?? ZString.Empty : ZString.Empty;
		public ZPropertyInfo EntryStyleInfo => GetZPropertyInfo(nameof(EntryStyle));

		[List(nameof(LeadDeclaration) + "." + nameof(BaseJobDeclaration.Lookups) + "." + nameof(JobDeclarationLookups.Vessels))]
		public ZString VesselName => LeadDeclaration?.JE_VesselName ?? ZString.Empty;
		public ZPropertyInfo VesselNameInfo => GetZPropertyInfo(nameof(VesselName));

		public ZString VoyageFlightNo => LeadDeclaration?.JE_VoyageFlightNo ?? ZString.Empty;
		public ZPropertyInfo VoyageFlightNoInfo => GetZPropertyInfo(nameof(VoyageFlightNo));

		[ResourceStringData("D6380B12-B824-4E43-97BD-D48A56F2C797", Caption = "Customs Status", ShortCaption = "Status")]
		public ZString CustomsStatusDescription
		{
			get
			{
				if (ConsolidatedEntryStatusList.Codes.AppliedToConsolidation.Equals(LeadDeclaration?.JE_EntryStatus) && CRD_CustomsStatus.IsEmpty)
				{
					return ZString.Empty;
				}
				else
				{
					return (LeadDeclaration as JobDeclaration)?.JE_EntryStatusDescription ?? ZString.Empty;
				}
			}
		}
		public ZPropertyInfo CustomsStatusDescriptionInfo => GetZPropertyInfo(nameof(CustomsStatusDescription));

		[ResourceStringData("FCCB4B7E-3014-4F16-AFFB-594144B220F8", Caption = "Entry Number", ShortCaption = "Entry Num")]
		public ZString DeclarationNumber
		{
			get
			{
				return (LeadDeclaration as JobDeclaration)?.DeclarationNumber ?? ZString.Empty;
			}
		}
		public ZPropertyInfo DeclarationNumberInfo => GetZPropertyInfo(nameof(DeclarationNumber));

		[ResourceStringData("11B76AAB-230D-46B3-90A2-B62C21934D19", Caption = "Message Status", ShortCaption = "Msg. Status")]
		public ZString MessageStatusDescription
		{
			get
			{
				return (LeadDeclaration as JobDeclaration)?.JE_MessageStatusDescriptionIncludingOustandingAmendments ?? ZString.Empty;
			}
		}
		public ZPropertyInfo MessageStatusDescriptionInfo => GetZPropertyInfo(nameof(MessageStatusDescription));

		#region TAndI

		/// <summary>
		/// Stored TILV or Sum of Declaration TILVs.
		/// </summary>
		public ZDecimal TAndI => Factory.GetValue(ref fTAndI, GetTAndI);

		ZDecimal GetTAndI()
		{
			var result = AddInfo.TILVInAUD;
			if (result.IsEmpty)
			{
				foreach (JobDeclaration jobDeclaration in JobDeclarations)
				{
					result += jobDeclaration.EntryHeader.TAndI;
				}
			}
			return result;
		}

		public void UpdateTILV(string tilv)
		{
			AddInfo.ZA_TILV = tilv;
			fTAndI = null;
			TAndIInfo.RefreshBinding();
		}

		CachedProperty<ZDecimal> fTAndI;

		public ZPropertyInfo TAndIInfo => GetZPropertyInfo(nameof(TAndI));

		#endregion

		#region IAddInfo Members

		public AUAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new AUAddInfo(this, CRD_AddInfoInfo);
					RegisterEditableChildObject(fAddInfo);
				}
				return fAddInfo;
			}
		}
		protected AUAddInfo fAddInfo;

		[BusinessObjectTestExclude()]
		public override ZString CRD_AddInfo
		{
			get { return base.CRD_AddInfo; }
			set
			{
				value = value.Trim(AUAddInfo.SeperationCharacter);
				if (CRD_AddInfo != value)
				{
					base.CRD_AddInfo = value;
					using (AddInfo.GetValidationSuspender())
					{
						AddInfo.LoadPropertiesFromString(CRD_AddInfo);
					}
				}
			}
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			using (AddInfo.SuspendSettingHasChanges())
			{
				AddInfo.LoadPropertiesFromString(CRD_AddInfo);
			}
		}

		ZDateTime IAggregatedAddInfo.DateOfValuation => ZDateTime.Today;
		ZDateTime IAggregatedAddInfo.EffectiveDutyDate => ZDateTime.Today;
		ZString IAggregatedAddInfo.AggregatedZA_ORG => ZString.Empty;
		ZString IAggregatedAddInfo.AggregatedZA_PRF => ZString.Empty;
		IZType IAggregatedAddInfo.AggregatedValue(string propertyName) => (IZType)AddInfo[propertyName];
		bool IAggregatedAddInfo.IsCopying => IsCopying;

		#endregion

		#region ICPQAAttachee Members

		SchemaGuidColumn ICPQAAttachee.FKColumnInCusEntryCPDecTable
		{
			get { return CPQAAttacheeWrapper.FKColumnInCusEntryCPDecTable; }
		}

		[ChildEditable(true)]
		public CMRCusEntryCPDecCollection Questions
		{
			get { return CPQAAttacheeWrapper.Questions; }
		}

		ZDateTime ICPQAAttachee.SelectionDate
		{
			get { return CPQAAttacheeWrapper.SelectionDate; }
		}

		CPQAAttacheeWrapper CPQAAttacheeWrapper
		{
			get
			{
				if (fCPQAAttacheeWrapper == null)
				{
					fCPQAAttacheeWrapper = new CPQAAttacheeWrapper(this);
				}
				return fCPQAAttacheeWrapper;
			}
		}
		CPQAAttacheeWrapper fCPQAAttacheeWrapper;

		#endregion

		public override void CalculateHeaderFees()
		{
			var headers = JobDeclarations.Cast<JobDeclaration>().Select(x => x.EntryHeader);
			new DutyCalculationManager().Calculate(headers);
		}

		public void DeriveConsolidatedStatusAndImportAggregateDeclaration(JobDeclaration aggregateDeclaration, bool deriveStatus)
		{
			// Copy messages from aggregateDeclaration's entryHeader to this Consolidated Declaration
			ImportMessages(aggregateDeclaration);
			Messages.Reload(false);

			// Update JE_MessageStatus of aggregateDeclaration, CH_Status and ZA_PaymentStatus_HiddenInfo of aggregateDeclaration's entryHeader
			if (deriveStatus)
			{
				aggregateDeclaration.EntryHeader.DeriveConsolidatedStatus();
			}

			// Copy properties of aggregateDeclaration to the linked declarations, including entryHeader's details
			ImportAggregateDeclaration(aggregateDeclaration, importMessages: false);
			RefreshStatusDescription();
		}

		void RefreshStatusDescription()
		{
			(LeadDeclaration as JobDeclaration)?.SetMessageStatusDescription();
			CustomsStatusDescriptionInfo.RefreshBinding();
			MessageStatusDescriptionInfo.RefreshBinding();
		}

		public static ConsolidatedDeclaration LoadFromRef(BusinessObjectFactory factory, ZString uniqueRef)
		{
			return (!uniqueRef.IsEmpty) ? factory.LoadTop1<ConsolidatedDeclaration>(new ZQuery(CusReconDeclarationSchema.CRD_JobReferenceNumber, uniqueRef)) : null;
		}

		public void DequeueScheduledMessages()
		{
			CancelQueuedMessageLogs();

			foreach (JobDeclaration dec in JobDeclarations)
			{
				dec.DequeueScheduledMessages();
			}
		}

		public void CancelQueuedMessageLogs()
		{
			var dsmLogs = new LogsForNominatedEvent(this.GetLogs(), Events.DeferredScheduledMessage);
			var activeQueuedEntryLogs = dsmLogs.Find(x => !x.IsCancelled && x.SL_EventTime >= ZDateTime.Now && JobDeclaration.IsQueuedMessageLogReference(x.SL_Reference.Trim())).ToArray();
			foreach (var log in activeQueuedEntryLogs)
			{
				log.Cancel();
			}
		}

		protected override void SyncJobDeclarationsStatusCore()
		{
			var leadDeclaration = (JobDeclaration)LeadDeclaration;
			var leadEntryHeader = leadDeclaration.EntryHeader;
			if (leadEntryHeader != null)
			{
				var entryNumber = leadEntryHeader.CusEntryNumber;

				var leadDeclarationNumber = ZString.Empty;
				if (entryNumber != null && !entryNumber.IsInDatabase && leadDeclaration.DeclarationNumber == entryNumber.CE_EntryNum)
				{
					leadDeclarationNumber = LeadDeclaration.DeclarationNumber;
				}

				var setConsolidatedEntryMemberID = leadEntryHeader.ConsolidatedEntryMemberID.IsEmpty && !leadDeclarationNumber.IsEmpty;
				var consolidatedEntryMemberID = ZShort.Zero;
				if (setConsolidatedEntryMemberID)
				{
					leadEntryHeader.ConsolidatedEntryMemberID = ++consolidatedEntryMemberID;
				}

				foreach (JobDeclaration dec in JobDeclarations)
				{
					if (dec.PK != leadDeclaration.PK)
					{
						if (setConsolidatedEntryMemberID)
						{
							dec.EntryHeader.ConsolidatedEntryMemberID = ++consolidatedEntryMemberID;
						}

						if (!leadDeclarationNumber.IsEmpty)
						{
							dec.EntryHeader.EntryNumber = leadDeclarationNumber;
						}

						if (leadDeclaration.HasChanges)
						{
							dec.JE_EDITransmitDate = leadDeclaration.JE_EDITransmitDate;
							if (leadDeclaration.JE_PaymentMethodInfo.HasChanges)
							{
								dec.JE_PaymentMethod = leadDeclaration.JE_PaymentMethod;
							}
						}

						if (leadEntryHeader.HasChanges)
						{
							var entryHeader = dec.EntryHeader;
							entryHeader.CH_CustomsDeliveryInstructions = leadEntryHeader.CH_CustomsDeliveryInstructions;
							entryHeader.CH_EntryStatus = leadEntryHeader.CH_EntryStatus;
							entryHeader.CH_Status = leadEntryHeader.CH_Status;
							entryHeader.AddInfo.ZA_PaymentStatus_Hidden = leadEntryHeader.AddInfo.ZA_PaymentStatus_Hidden;
							entryHeader.AddInfo.ZA_ScheduledPaymentDate_Hidden = leadEntryHeader.AddInfo.ZA_ScheduledPaymentDate_Hidden;
						}
					}
				}
			}
		}

		public void UpdateQuestionsFromAggregateDeclaration(JobDeclaration aggregateDeclaration)
		{
			UpdateLodgementQuestionsFromAggregateDeclaration(aggregateDeclaration);

			var aggregatedEntryLines = aggregateDeclaration.EntryHeader.AllEntryLines;

			foreach (JobDeclaration jobDeclaration in JobDeclarations)
			{
				var entryHeader = jobDeclaration.EntryHeader;
				foreach (var entryLine in entryHeader.AllEntryLines)
				{
					if (aggregatedEntryLines.FindByPK(entryLine.PK) is CusEntryLine aggregatedLine)
					{
						foreach (CMRCusEntryCPDec question in entryLine.Questions)
						{
							var answer = aggregatedLine.Questions.GetQuestionWithID(question.ON_CPDecNum);
							if (answer != null)
							{
								question.ON_AnswerCode = answer.ON_AnswerCode;
								question.ON_Permit = answer.ON_Permit;
							}
						}
					}
				}
			}
		}

		protected override void OnAggregateDeclarationBuilt(BaseJobDeclaration aggregateDeclaration)
		{
			var auAggregateDeclaration = (JobDeclaration)aggregateDeclaration;
			var auAggregateEntryHeader = auAggregateDeclaration.EntryHeader;
			var readonlyFactory = auAggregateDeclaration.Factory;
			var leadEntryHeader = readonlyFactory.Load<JobDeclaration>(LeadDeclaration.PK).EntryHeader;

			var leadEntryNumber = leadEntryHeader.CusEntryNumber;
			if (leadEntryNumber != null)
			{
				leadEntryNumber.SuspendMarkingAsNeedingValidation();
				leadEntryNumber.SuspendValidation();
				leadEntryNumber.Parent = auAggregateEntryHeader;
			}

			auAggregateEntryHeader.Logs.RemoveAndDeleteAll();
			MoveAllBizO(leadEntryHeader.Logs.GetAllLogs(), auAggregateEntryHeader.Logs.GetAllLogs());

			var allEntryLines = auAggregateEntryHeader.AllEntryLines;
			foreach (var entryLine in allEntryLines)
			{
				var aggregateLineNumber = entryLine.ZA_AggregateEntryLineNumber;
				if (!aggregateLineNumber.IsEmpty)
				{
					// Sync original line number with aggregate line number
					entryLine.CL_LineNumber = aggregateLineNumber;
				}
				else
				{
					// Set CL_LineNumber to 0 so it will be recalculated by LineNumberAssigner
					entryLine.CL_LineNumber = 0;
				}
			}

			// Recalculate and assign new aggregate line numbers where CL_LineNumber == 0 or CL_LineNumber > CH_HighestLineNumber
			// Then store them into the line addinfo
			var lineNumberAssigner = new LineNumberAssigner(auAggregateEntryHeader);
			lineNumberAssigner.Execute();
			foreach (var entryLine in auAggregateEntryHeader.MergedLines)
			{
				entryLine.ZA_AggregateEntryLineNumber = entryLine.CL_LineNumber;
			}

			auAggregateEntryHeader.AddInfo.ZA_TILV = AddInfo.ZA_TILV;

			CopyChargesToAggregateDeclaration(auAggregateDeclaration);
			CopyLodgementQuestionsToAggregateDeclaration(auAggregateDeclaration);
		}

		protected override void MergeDeclarationIntoAggregateDeclaration(BaseJobDeclaration jobDeclaration, BaseJobDeclaration aggregateDeclaration, Customs.Business.CusEntryHeader aggregateEntryHeader)
		{
			var auDeclaration = jobDeclaration as JobDeclaration;
			var auAggregateEntryHeader = aggregateEntryHeader as CusEntryHeader;

			foreach (var entryLine in auDeclaration.EntryHeader.AllEntryLines)
			{
				foreach (CMRCusEntryCPDec question in entryLine.Questions)
				{
					question.EntryLineDescriptionPrefix = auDeclaration.JE_DeclarationReference + " ";
				}
			}

			base.MergeDeclarationIntoAggregateDeclaration(jobDeclaration, aggregateDeclaration, aggregateEntryHeader);

			MoveAllBizO(auDeclaration.EntryHeader.DeletedPackingGroups, auAggregateEntryHeader.DeletedPackingGroups);
			if (auDeclaration.PK != CRD_JE_LeadDeclaration)
			{
				auAggregateEntryHeader.CH_HighestLineNumber += auDeclaration.EntryHeader.CH_HighestLineNumber;
			}
			aggregateDeclaration.Invoices.ForEach(x => x.ReloadInvoiceLines());
		}

		void CopyChargesToAggregateDeclaration(JobDeclaration auAggregateDeclaration)
		{
			var totalDeferredDuty = ZDecimal.Zero;
			var destinationCharges = auAggregateDeclaration.EntryHeader.Charges;

			foreach (JobDeclaration jobDeclaration in JobDeclarations)
			{
				var sourceCharges = jobDeclaration.EntryHeader.Charges;
				totalDeferredDuty += sourceCharges.GetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount);

				if (jobDeclaration.PK == CRD_JE_LeadDeclaration)
				{
					foreach (var charge in sourceCharges)
					{
						destinationCharges.SetAmount(charge.C1_ChargeType, charge.C1_ChargeAmount);
					}
				}
			}

			destinationCharges.SetAmount(CusEntryChargeTypeList.Codes.DutyDeferredAmount, totalDeferredDuty);
		}

		public void CopyLodgementQuestionsToAggregateDeclaration(JobDeclaration aggregateDeclaration)
		{
			var destinationQuestions = aggregateDeclaration.EntryHeader.Questions;

			foreach (CMRCusEntryCPDec sourceQuestion in Questions)
			{
				var destinationQuestion = destinationQuestions.AddNew();
				destinationQuestion.CopyPersistentValuesFrom(sourceQuestion, CopyCPDecQuestionCloneArgs);
			}

			aggregateDeclaration.AddInfo.ZA_CPQuestionGenDate_Hidden = AddInfo.ZA_CPQuestionGenDate_Hidden;
		}

		void UpdateLodgementQuestionsFromAggregateDeclaration(JobDeclaration aggregateDeclaration)
		{
			var sourceQuestions = aggregateDeclaration.EntryHeader.Questions;
			var destinationQuestions = Questions;

			foreach (var destinationQuestion in destinationQuestions.Cast<CMRCusEntryCPDec>().ToArray())
			{
				if (sourceQuestions.GetQuestionWithID(destinationQuestion.ON_CPDecNum) == null)
				{
					destinationQuestions.RemoveAndDelete(destinationQuestion);
				}
			}

			foreach (CMRCusEntryCPDec sourceQuestion in sourceQuestions)
			{
				var destinationQuestion = destinationQuestions.GetQuestionWithID(sourceQuestion.ON_CPDecNum);
				if (destinationQuestion == null)
				{
					destinationQuestion = destinationQuestions.AddNew();
					destinationQuestion.CopyPersistentValuesFrom(sourceQuestion, CopyCPDecQuestionCloneArgs);
				}
				else
				{
					destinationQuestion.ON_AnswerCode = sourceQuestion.ON_AnswerCode;
				}
			}

			AddInfo.ZA_CPQuestionGenDate_Hidden = aggregateDeclaration.AddInfo.ZA_CPQuestionGenDate_Hidden;
		}

		BusinessObjectCloneArgs CopyCPDecQuestionCloneArgs => copyCPDecQuestionCloneArgs ??= new BusinessObjectCloneArgs(columnNamesToExcludeFromCopy: new[]
			{
				nameof(CMRCusEntryCPDec.ON_ParentTableCode), nameof(CMRCusEntryCPDec.ON_ParentID),
				nameof(CMRCusEntryCPDec.ON_CL), nameof(CMRCusEntryCPDec.ON_CH), nameof(CMRCusEntryCPDec.ON_JE)
			});
		BusinessObjectCloneArgs copyCPDecQuestionCloneArgs;

		#region PaymentStatus

		[BusinessObjectTestExclude]
		[ReadOnlyMember(nameof(IsPaymentStatusReadOnly))]
		[MaxLength(30)]
		public ZString PaymentStatus
		{
			get
			{
				CusEntryHeader entryHeader = (CusEntryHeader)LeadDeclaration?.CustomsEntryHeaders[0];
				return entryHeader != null ? PaymentStatusList.GetDescriptionFromCode(entryHeader.AddInfo.ZA_PaymentStatus_Hidden) : ZString.Empty;
			}
		}

		protected bool IsPaymentStatusReadOnly => true;

		CMREntryPaymentStatusList PaymentStatusList
		{
			get
			{
				if (fPaymentStatusList == null)
				{
					fPaymentStatusList = new CMREntryPaymentStatusList();
				}
				return fPaymentStatusList;
			}
		}
		CMREntryPaymentStatusList fPaymentStatusList;

		#endregion

		protected override void OnAggregateDeclarationImported(BaseJobDeclaration aggregateDeclaration)
		{
			var auAggregateDeclaration = (JobDeclaration)aggregateDeclaration;
			if (auAggregateDeclaration.IsQueuedEntryLodgement)
			{
				var transmitDate = auAggregateDeclaration.JE_EDITransmitDate.Date;
				var messageType = auAggregateDeclaration.JE_MessageStatus == CustomsEntryStatus.ScheduledLodgeWithPayment.Code ? CMRMessageTypes.LodgeWithPay : CMRMessageTypes.LodgeWithoutPay;
				IMDMessageManager.CreateScheduledLodgementMessageLog(this, messageType, transmitDate);
			}
			else if (auAggregateDeclaration.IsQueuedEntryPayment)
			{
				var transmitTime = auAggregateDeclaration.EntryHeader.ScheduledPaymentDate.ToDateTime();
				IMDMessageManager.CreateScheduledLodgementMessageLog(this, CMRMessageTypes.Payment, transmitTime);
			}

			var aggregateEntryHeader = auAggregateDeclaration.EntryHeader;
			foreach (var entryLineToImport in aggregateEntryHeader.MergedLines)
			{
				var entryLineToSave = Factory.Load<CusEntryLine>(entryLineToImport.PK);
				entryLineToSave.ZA_AggregateEntryLineNumber = entryLineToImport.ZA_AggregateEntryLineNumber;
			}

			foreach (PackingGroup packingGroupToImport in aggregateEntryHeader.PackingGroups)
			{
				var packingGroupToSave = Factory.Load<PackingGroup>(packingGroupToImport.PK);
				packingGroupToSave.CR_HouseContainerNumber = packingGroupToImport.CR_HouseContainerNumber;
			}

			foreach (JobDeclaration declaration in JobDeclarations)
			{
				declaration.EntryHeader.AddInfo.ZA_HighHouseContPivotNo_Hidden = aggregateEntryHeader.AddInfo.ZA_HighHouseContPivotNo_Hidden;
			}
		}

		protected override Type[] PermittedNewObjectTypes => base.PermittedNewObjectTypes.Concat(new[] { typeof(CMRCusEntryCPDec) }).ToArray();

		#region IDocumentSupportable Override

		protected override DocumentSupporter CreateNewDocumentSupporter()
		{
			return new ConsolidatedDeclarationDocumentSupporter(this);
		}

		#endregion

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			var jobDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			jobDeclaration.ActiveEntryHeaders.AddNew();
			jobDeclaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			JobDeclarations.Add(jobDeclaration);
			CRD_JE_LeadDeclaration = jobDeclaration.PK;
		}

#endif

	}
}
