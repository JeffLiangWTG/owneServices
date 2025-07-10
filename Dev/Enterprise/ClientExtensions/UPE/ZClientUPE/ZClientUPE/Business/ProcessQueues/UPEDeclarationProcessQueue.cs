using System;
using System.Collections;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class UPEDeclarationQueue : UPEProcessQueue
	{
		public UPEDeclarationQueue(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override UPECusHAWB FirstUPECusHAWB
		{
			get { return (Declaration != null) ? Declaration.FirstCusHAWB : null; }
		}

		public override GlbBranch ParentBranch => Declaration?.Branch;

		public UPEJobDeclaration Declaration
		{
			get { return (UPEJobDeclaration)base.ParentBusinessObject; }
		}

		public override UPECusHAWB[] GetFinalisedCusHAWBs()
		{
			UPECusHAWB[] result;

			if (Declaration != null && IsCustomsQueueCompleted)
			{
				UPECusHAWB[] allCusHAWBs = Declaration.GetAssociatedHAWBs();
				ArrayList finalisedCusHAWBs = new ArrayList();
				foreach (UPECusHAWB hAWB in allCusHAWBs)
				{
					if (hAWB.CurrentQueue.IsCustomsQueueCompleted && hAWB.CurrentQueue.IsCommercialQueueCompleted)
					{
						finalisedCusHAWBs.Add(hAWB);
					}
				}

				result = (UPECusHAWB[])finalisedCusHAWBs.ToArray(typeof(UPECusHAWB));
			}
			else
			{
				result = Array.Empty<UPECusHAWB>();
			}

			return result;
		}

		#region AutoDeliverAlternateBrokerDocumentPack

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			RequiresAutoDeliverAlternateBrokerDocumentPackOnFactorySaved = RequiresAutoDeliverAlternateBrokerDocumentPack();
		}
		bool RequiresAutoDeliverAlternateBrokerDocumentPackOnFactorySaved;

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				if (RequiresAutoDeliverAlternateBrokerDocumentPackOnFactorySaved)
				{
					RequiresAutoDeliverAlternateBrokerDocumentPackOnFactorySaved = false;
					GetAlternateBrokerDocumentPackAutoDelivery().Deliver();
				}
			}
		}

		bool RequiresAutoDeliverAlternateBrokerDocumentPack()
		{
			return Declaration != null
				   && Declaration.AlternateBroker != null
				   && Declaration.Importer != null
				   && !Declaration.Importer.IsITFChargableForThisImporter
				   && Declaration.Callout != null
				   && Declaration.Callout.FinanceFreightChargeIncludingGST == 0m
				   && CustomsQueueNameHasChanges
				   && ((IList)CustomsQueueCodeDescriptionPairList.CompletedQueueNames).Contains((string)P4_CustomsQueue);
		}

		bool CustomsQueueNameHasChanges
		{
			get
			{
				return
					(IsInDatabase && P4_CustomsQueueInfo.HasChanges) ||
					(!IsInDatabase && !P4_CustomsQueue.IsEmpty);
			}
		}

 UPEAlternateBrokerDocumentPackAutoDelivery GetAlternateBrokerDocumentPackAutoDelivery()
		{
			return new UPEAlternateBrokerDocumentPackAutoDelivery(Declaration);
		}

		#endregion

		#region ADP Scoring

		public override ZString P4_CustomsQueue
		{
			get { return base.P4_CustomsQueue; }
			set
			{
				base.P4_CustomsQueue = value;
				if (IsInDatabase && !Env.CurrentUser.IsBatchProcessor && P4_CustomsQueueInfo.HasChanges)
				{
					ZString originalQueue = (ZString)P4_CustomsQueueInfo.OriginalValue;
					if (originalQueue == DeclarationQueueCodeDescriptionPairList.Codes.Classification &&
						P4_CustomsQueue == DeclarationQueueCodeDescriptionPairList.Codes.Lodgement)
					{
						ADPScoring.T4_SubmissionCount++;
					}
					else if (
						originalQueue == DeclarationQueueCodeDescriptionPairList.Codes.Lodgement &&
						P4_CustomsQueue == DeclarationQueueCodeDescriptionPairList.Codes.Classification)
					{
						ADPScoring.T4_SubmissionErrorCount++;
					}
				}
			}
		}

		UPEADPScoring ADPScoring
		{
			get
			{
				UPEADPScoring.Loader loader = new UPEADPScoring.Loader(Factory);
				return loader.LoadOrCreate();
			}
		}

		#endregion

		#region New MAWB Alert

		void NotifyMAWBClassificationGroup()
		{
			try
			{
				if (IsInDatabase &&
					P4_CustomsQueueInfo.HasChanges &&
					P4_CustomsQueue == AutoDeclarationQueueCodeDescriptionPairList.Codes.Classification)
				{
					UPECusMAWB mAWB = Factory.LoadTop1<UPECusMAWB>(new ZQuery(CusMAWBSchema.CM_MAWB, Declaration.JE_MasterBill));

					ZQuery emailSentQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.EmailSent.Code);
					emailSentQuery.AddToFilter(StmALogSchema.SL_Reference, MAWBClassificationReference);
					if (mAWB != null && mAWB.Logs.GetAllLogs().Find(emailSentQuery).Length == 0)
					{
						EmailMAWBClassificationGroup(mAWB);
					}
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("{E4CCC6B1-A839-449e-A69F-102411C30648}", ex.Message, ex);
			}
		}

		void EmailMAWBClassificationGroup(UPECusMAWB mAWB)
		{
			EmailDef email = new EmailDef();
			email.Subject = string.Format(NotificationMessage, mAWB.CM_MAWB);
			email.Body = string.Format(NotificationMessage, mAWB.CM_MAWB);
			try
			{
				Env.OutgoingMailManager.Create(Factory, email, UPEDataRegistry.Instance.MAWBFirstMovedToClassificationNotificationGroup, GroupSourceLocator.GetFromRegistryItem(UPEDataRegistry.Instance.MAWBFirstMovedToClassificationNotificationGroupItem));
			}
			catch (EmailSendFailedException)
			{
				// usually thrown if there are no staff members in the recipient group
			}
			mAWB.Logs.AddNew(Events.EmailSent, MAWBClassificationReference);
		}
		const string NotificationMessage = "MAWB: '{0}' has been moved to the CLS queue for the first time.";
		const string MAWBClassificationReference = "MAWB moved to Classification Queue";

		#endregion

		#region Overrides

		protected override string UPEProcessQueueType
		{
			get { return this.GetType().Name; }
		}

		protected override Type ParentBusinessObjectType
		{
			get { return typeof(UPEJobDeclaration); }
		}

		protected override ProcessQueueLookups GetNewLookups()
		{
			return new UPEDeclarationQueueLookups(this);
		}

		protected override ProcessQueueValidation GetNewValidation()
		{
			return new UPEDeclarationQueueValidation(this);
		}

		public override void OnSaving()
		{
			base.OnSaving();
			NotifyMAWBClassificationGroup();
		}

		#endregion

		public override string ReferenceCode
		{
			get { return referenceCode; }
		}
		const string referenceCode = "CUS";
	}
}
