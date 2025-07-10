using System;
using System.Collections;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public abstract class AirOceanMessageProcessor : JXCMessageProcessor
	{
		public AirOceanMessageProcessor(JXCRecord[] records)
			: base(records)
		{
		}

		protected void ProcessREFRRecords(JASForwardingShipment shipment, int startingIndex)
		{
			REFRRecord[] records = (REFRRecord[])GetChildrenRecords(startingIndex, typeof(REFRRecord));
			foreach (REFRRecord record in records)
			{
				record.UpdateShipment(shipment);
			}
		}

		protected void ProcessSHMKRecords(JASForwardingShipment shipment, int startingIndex)
		{
			SHMKRecord[] records = (SHMKRecord[])GetChildrenRecords(startingIndex, typeof(SHMKRecord));
			foreach (SHMKRecord record in records)
			{
				record.UpdateShipment(shipment);
			}
		}

		protected bool ProcessDummyShipment(JASForwardingConsol consol, int bodyRecordIndex, INotifications notificationSubscriber)
		{
			bool result = false;

			DummyHouseRecord dummyHouseRecord = BodyRecords[bodyRecordIndex] as DummyHouseRecord;
			if (dummyHouseRecord != null)
			{
				JASForwardingShipment shipment = dummyHouseRecord.LoadOrCreateShipment(consol, notificationSubscriber);
				if (!shipment.IsInDatabase || JASDataRegistry.Instance.EnableAutoUpdateOnImport)
				{
					dummyHouseRecord.UpdateShipment(shipment, notificationSubscriber);
					notificationSubscriber.Notify(new BusinessObjectCreatedOrUpdatedNotification(shipment));
				}
				else
				{
					NotifyShipmentNotUpdated(shipment, notificationSubscriber);
				}
				result = true;
			}

			return result;
		}

		protected JXCRecord[] GetChildrenRecords(int startingIndex, Type recordType)
		{
			ArrayList result = new ArrayList();

			for (int i = startingIndex; i < BodyRecords.Length; i++)
			{
				JXCRecord record = BodyRecords[i];
				if (record.GetType() == recordType)
				{
					result.Add(BodyRecords[i]);
				}

				if (IsHouseLevelRecord(record))
				{
					break;
				}
			}

			return (JXCRecord[])result.ToArray(recordType);
		}

		protected void NotifyConsolNotUpdated(JASForwardingConsol consol, INotifications notificationSubscriber)
		{
			if (notificationSubscriber != null)
			{
				string warningMessage = string.Format("{0} already exist and the automatic update feature is disabled in the registry settings. Consol will not be updated.", consol.HumanReadableName);
				WarningNotification warningNotification = new WarningNotification(warningMessage);
				notificationSubscriber.Notify(warningNotification);
			}
		}

		protected void NotifyShipmentNotUpdated(JASForwardingShipment shipment, INotifications notificationSubscriber)
		{
			if (notificationSubscriber != null)
			{
				string warningMessage = string.Format("{0} already exist and the automatic update feature is disabled in the registry settings. Shipment will not be updated.", shipment.HumanReadableName);
				WarningNotification warningNotification = new WarningNotification(warningMessage);
				notificationSubscriber.Notify(warningNotification);
			}
		}

		#region Loading and Creating Job

		protected JASJob LoadOrCreateJob(JASForwardingShipment shipment, GlbBranch branch, IJobChargeData[] chargesData, INotifications notificationSubscriber)
		{
			JASJob result = null;

			if (branch == null)
			{
				branch = GetBranchFromForwarderDetails(shipment);
			}

			if (branch != null)
			{
				ZQuery filter = new ZQuery(JobHeaderSchema.JH_GC, branch.Company.PK);
				filter.AddToFilter(JobHeaderSchema.JH_ParentID, shipment.PK);
				filter.AddToFilter(JobHeaderSchema.JH_ParentTableCode, JobShipmentSchema.Constants.Prefix);
				result = (JASJob)shipment.Factory.LoadTop1(typeof(Job), filter);

				if (result == null)
				{
					result = CreateJob(shipment, branch, chargesData, notificationSubscriber);
				}

				if (result != null)
				{
					if (JobAlreadyExistAndHasNonDefaultCharges(result))
					{
						WarningNotification warning = new WarningNotification("Cannot create new Invoice for this Shipment Job as it already has existing charges");
						notificationSubscriber.Notify(warning);
						EmailJobChargesCannotBeCreated(shipment, result.Charges, chargesData, "");
						result = null;
					}
				}
			}
			else
			{
				WarningNotification warning = new WarningNotification("Cannot create Invoice for the Shipment Job as the Invoice branch cannot be determined from the Receiving Forwarder");
				notificationSubscriber.Notify(warning);
			}

			return result;
		}

		protected JASJob LoadOrCreateJob(JASForwardingShipment shipment, IJobChargeData[] chargesData, INotifications notificationSubscriber)
		{
			return LoadOrCreateJob(shipment, null, chargesData, notificationSubscriber);
		}

		bool JobAlreadyExistAndHasNonDefaultCharges(JASJob job)
		{
			foreach (JobCharge charge in job.Charges)
			{
				if (charge.IsInDatabase)
				{
					return true;
				}
			}

			return false;
		}

		JASJob CreateJob(JASForwardingShipment shipment, GlbBranch branch, IJobChargeData[] chargesData, INotifications notificationSubscriber)
		{
			var loader = new Job.Loader(shipment);
			JASJob result = (JASJob)loader.TryCreateWithMutex();
			if (result != null)
			{
				result.JH_GB = branch.PK;
				result.JH_GC = branch.GB_GC;
				SetDepartment(result, shipment);
			}
			else
			{
				string errorMessage = loader.GetJobCreationError();
				ErrorNotification error = new ErrorNotification(JASDataErrorNotificationType.CannotCreateNewShipmentJob, errorMessage);
				notificationSubscriber.Notify(error);
				EmailJobChargesCannotBeCreated(shipment, null, chargesData, errorMessage);
			}

			return result;
		}

		void EmailJobChargesCannotBeCreated(JASForwardingShipment shipment, ChargeCollection charges, IJobChargeData[] chargesData, string additionalMessage)
		{
			string mailSubject = "Cannot create Invoices for " + shipment.HumanReadableName;
			StringBuilder mailBody = new StringBuilder();

			if (charges == null)
			{
				mailBody.Append("Invoice cannot be created due to the following reason: ");
				mailBody.Append(additionalMessage);
				mailBody.Append("\r\n\r\n");
			}
			else
			{
				mailBody.Append("The below charges already attached to the shipment:\r\n");
				foreach (JobCharge charge in charges)
				{
					ZString currencyCode = (charge.SellCurrency != null) ? charge.JR_RX_NKSellCurrency : ZString.Empty;
					AppendJobChargeNotificationLine(mailBody, charge.ChargeCode.AC_Code, charge.JR_Desc, currencyCode, charge.JR_OSSellAmt);
				}
				mailBody.Append("\r\n");
			}

			mailBody.Append("The below charges are extracted from the JXC file:\r\n");
			foreach (IJobChargeData chargeData in chargesData)
			{
				AppendJobChargeNotificationLine(mailBody, chargeData.ChargeCode, chargeData.ChargeDescription, chargeData.Currency, chargeData.ChargeAmount);
			}
			mailBody.Append("\r\nPlease update manually.");

			JASMailSender.SendEmail(JASDataRegistry.Instance.JXCFinancialMessagingNotificationGroupPK, JASDataRegistry.Instance.JXCFinancialMessagingNotificationGroupItem, mailSubject, mailBody.ToString(), null);
		}

		protected
 JASMailSender JASMailSender
		{
			get
			{
				if (fJASMailSender == null)
				{
					fJASMailSender = GetNewJASMailSender();
				}
				return fJASMailSender;
			}
		}

		protected virtual
 JASMailSender GetNewJASMailSender()
		{
			return new JASMailSender();
		}

		void AppendJobChargeNotificationLine(StringBuilder builder, ZString chargeCode, ZString desc, ZString currency, ZDecimal amount)
		{
			builder.AppendFormat("{0, -12}  {1, -50}  {2, -3}  {3, -15}\r\n", chargeCode.Left(12), desc.Left(50), currency.Left(3), amount);
		}

		void SetDepartment(Job job, JASForwardingShipment shipment)
		{
			if (job.JH_GE.IsEmpty)
			{
				var defaultDept = (GlbDepartment)shipment.Factory.LoadFromNaturalKey(typeof(GlbDepartment), GlbDepartmentSchema.GE_Code, "BRN")
					?? (GlbDepartment)shipment.Factory.LoadTop1(typeof(GlbDepartment), new ZQuery());
				job.JH_GE = defaultDept.PK;
			}
		}

		protected GlbBranch GetBranchFromForwarderDetails(JASForwardingShipment shipment)
		{
			GlbBranch result = null;

			JASOrgHeader receivingForwarder = (shipment.Consols.Count > 0)
				? (JASOrgHeader)shipment.Consols[0].ReceivingForwarder
				: JASOrgHeader.FindOrgHeaderByOfficeAndNettingCode(shipment.Factory, HEADRecord.DestinationOfficeCode, HEADRecord.DestinationNettingCode);

			if (receivingForwarder != null)
			{
				ZQuery orgProxyQuery = new ZQuery(GlbBranchSchema.GB_OH_OrgProxy, receivingForwarder.PK);
				result = shipment.Factory.LoadTop1<GlbBranch>(orgProxyQuery);
				if (result == null)
				{
					result = receivingForwarder.Branch;
				}
			}

			return result;
		}

		JASMailSender fJASMailSender;

		#endregion

		protected abstract bool ProcessStandardShipment(IFactoryProvider factoryProvider, int bodyRecordIndex, INotifications notificationSubscriber);
		protected abstract bool IsHouseLevelRecord(JXCRecord record);
	}
}
