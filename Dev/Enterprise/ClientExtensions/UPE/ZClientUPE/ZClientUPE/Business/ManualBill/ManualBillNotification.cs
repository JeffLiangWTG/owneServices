using System;
using CargoWise.Common;
using Enterprise.Client.UPE.Business.BISI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Business
{
	public class ManualBillNotification
	{
		public ManualBillNotification(UPEJobDeclaration declaration)
		{
			this.Declaration = declaration;
		}

		public virtual bool ShouldDoManualBillActivitiesOnSave
		{
			get
			{
				try
				{
					return HasChargesBeenAmendedSinceBISIUpload;
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					ErrorReporter.ReportOnce("ManualBillNotification.ShouldDoManualBillActivitiesOnSave", ex.Message, ex);
					return false;
				}
			}
		}

		public void DoManualBillActivities()
		{
			try
			{
				SendEmail();
				CreateNote();
				CompleteFinanceQueues();
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				ErrorReporter.ReportOnce("ManualBillNotification.DoManualBillActivities", ex.Message, ex);
			}
		}

		#region SendEmail

		void SendEmail()
		{
			EmailDef email = new EmailDef();

			email.Subject = "Manual Bill - " + Declaration.JE_HouseBill;
			email.Body = NotificationString;

			SendEmailToBillingNotificationGroup(email);
		}

		void SendEmailToBillingNotificationGroup(EmailDef email)
		{
			GlbGroup recipientGroup = Declaration.Factory.Load<GlbGroup>(UPEDataRegistry.Instance.ManualbillNotificationGroup);
			if (recipientGroup != null)
			{
				Env.OutgoingMailManager.CreateAndSave(email, recipientGroup.PK.ToGuid(), GroupSourceLocator.GetFromGroup(recipientGroup));
			}
		}

		#endregion

		#region CreateNote

		void CreateNote()
		{
			NoteToBeSaved = Declaration.Notes.AddNew();
			NoteToBeSaved.ST_Description = UPEPredefinedNoteTypes.Instance.ManualBillNote.Description;
			NoteToBeSaved.ST_NoteText = NotificationString;
		}
		StmNote NoteToBeSaved;

		#endregion

		#region CompleteFinanceQueues

		void CompleteFinanceQueues()
		{
			foreach (UPECusHAWB cusHAWB in Declaration.RelatedCusHAWBs)
			{
				if (cusHAWB.CurrentQueue.P4_Status != ReasonCodeDescriptionPairList.Codes._C1_SubsequentSplitShipment)
				{
					cusHAWB.CurrentQueue.P4_QueueName = CommercialQueueCodeDescriptionPairList.Codes.Completed;
				}
			}
		}

		#endregion

		#region Implementation

		readonly UPEJobDeclaration Declaration;

		protected bool HasChargesBeenAmendedSinceBISIUpload
		{
			get
			{
				foreach (UPECusHAWB cusHAWB in Declaration.RelatedCusHAWBs)
				{
					if (HasChargesBeenAmendedSinceBISIUploadForCusHAWB(cusHAWB))
					{
						return true;
					}
				}
				return false;
			}
		}

		bool HasChargesBeenAmendedSinceBISIUploadForCusHAWB(UPECusHAWB cusHAWB)
		{
			IShipmentData shipmentData = cusHAWB;

			bool result = false;
			if (cusHAWB.BISIUploadedShipmentHeader != null)
			{
				result = shipmentData.ChargesData.Count != cusHAWB.BISIUploadedShipmentHeader.Charges.Count;
				if (!result)
				{
					foreach (ShipmentChargeData currentCharge in shipmentData.ChargesData)
					{
						ClientBISIShipmentCharge uploadedCharge = cusHAWB.BISIUploadedShipmentHeader.Charges.FindByChargeType(currentCharge.TypeCode);
						if (uploadedCharge == null || currentCharge.GrossAmount != uploadedCharge.T9_GrossAmount)
						{
							result = true;
						}
					}
				}
			}
			return result;
		}

		string NotificationString
		{
			get
			{
				string result = @"
Formal Declaration has been amended and manual billing is required.
A manual bill is required.

Tracking number: " + Declaration.JE_HouseBill + @"
Master Air Waybill number: " + Declaration.JE_MasterBill + @"

Charges:
";
				foreach (ShipmentChargeData charge in Declaration.ChargesData)
				{
					result += charge.TypeCode + " = $" + charge.GrossAmount.ToString(2) + "\r\n";
				}
				return result;
			}
		}

		#endregion
	}
}
