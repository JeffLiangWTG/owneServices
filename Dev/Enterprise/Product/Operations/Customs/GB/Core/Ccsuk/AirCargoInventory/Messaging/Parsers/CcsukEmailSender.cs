using System;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.Parsers
{
	public class CcsukEmailSender
	{
		public enum ToWhom
		{
			StaffAndOrCustomsGroupBasedOnRegistry,
			CustomsGroupOnly,
			ItDepartmentAndCustomsGroup
		}

		public CcsukEmailSender(BusinessObjectFactory businessObjectFactory, ToWhom whom, BusinessObject job, GlbStaff staff = null)
		{
			this.businessObjectFactory = businessObjectFactory;
			this.job = job;
			this.Staff = staff;
			if (job != null && staff == null)
			{
				Staff = TryReallyHardToGetStaffFromJobOrLinkedJob(job);
			}
			this.whom = whom;
		}

		/// <summary>
		/// Don't forget to Save() your factory, either do so yourself or let base message processor do so.  But this won't do so on its own.
		/// </summary> 
		public void SendEmail(string subject, string body, IRegistryItem notificationRegistryItem, ZString optionalRegistryItemSubCode, Guid company, Guid branch, Guid dept)
		{
			var link = GetLinkForJob(job);
			if (!link.IsEmpty)
			{
				body = string.Format("{0} <p> To open the job, click here: <a href='{1}'>{2}</a></p>", body, link, job.HumanReadableName);
			}
			SendEmailWithoutIndividualJobLink(subject, body, notificationRegistryItem, optionalRegistryItemSubCode, company, branch, dept);
		}

		/// <summary>
		/// Don't forget to Save() your factory, either do so yourself or let base message processor do so.  But this won't do so on its own.
		/// </summary> 
		public void SendEmailWithoutIndividualJobLink(string subject, string body, IRegistryItem notificationRegistryItem, ZString optionalRegistryItemSubCode, Guid company, Guid branch, Guid dept)
		{
			HtmlNotificationEmailSender emailSender = new HtmlNotificationEmailSender();
			EmailDef email = emailSender.CreateEmail(subject, body);
			var notifier = new Customs.Business.EmailSender(new LoggingInformation());

			if (whom == ToWhom.StaffAndOrCustomsGroupBasedOnRegistry && Staff == null)
			{
				whom = ToWhom.CustomsGroupOnly;
			}

			if (whom == ToWhom.StaffAndOrCustomsGroupBasedOnRegistry)
			{
				notifier.SendNotification(email,
												Staff,
												GBCustomsDataRegistry.Instance.CustomsResponseNotifications,
												GBCustomsDataRegistry.Instance.GetRegistryItemGuid(notificationRegistryItem, optionalRegistryItemSubCode, company, branch, dept),
												notificationRegistryItem,
												businessObjectFactory
												);
			}
			else if (whom == ToWhom.CustomsGroupOnly)
			{
				notifier.SendNotification(
												email,
												GBCustomsDataRegistry.Instance.GetRegistryItemGuid(notificationRegistryItem, optionalRegistryItemSubCode, company, branch, dept),
												notificationRegistryItem,
												businessObjectFactory
											);
			}
			else if (whom == ToWhom.ItDepartmentAndCustomsGroup)
			{
				if (EnvProxy.IsHostedWithCargowise)
				{
					if (IsProdcutionDatabase)
					{
						var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
						var newSubject = "CCSUK hosted connection/login failure. Ent=" + registrationKey.EnterpriseCode + ", LicDB=" + registrationKey.DatabaseName + " Srvr=" + registrationKey.ServerCode + ", Host=" + System.Environment.MachineName + ", DB=" + Db.Connection.ServerNameReportedByDatabase;
						var ipsInRegistry = GBCustomsDataRegistry.Instance.CcsukIpAddresses.Value.AllAsString();
						Env.OutgoingCustomsMailManager.CreateAndSaveSimple(
																			newSubject,
																			newSubject + "\r\n\r\nIP addresses in registry:\r\n\r\n" + ipsInRegistry + "\r\n\r\n\r\n" + body,
																			GBCustomsDataRegistry.Instance.HostedCcsukAlertsEmailAddress.Value
																			);
					}
				}
				else
				{
					notifier.SendNotification(
													email,
													GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationCcsukErrors, optionalRegistryItemSubCode, company, branch, dept),
													notificationRegistryItem,
													businessObjectFactory
												);
				}
			}
		}

		bool IsProdcutionDatabase => ObjectFactory.Get<IProductRegistration>()?.Key?.DatabaseType == DatabaseTypes.Codes.Production;

		public static ZString GetLinkForJob(BusinessObject job)
		{
			if (job == null)
			{
				return string.Empty;
			}
			var pk = job.PK.ToGuid();

			ControllerID controllerId = null;
			if (job is CusHAWB)
			{
				controllerId = ControllerIDs.Customs.GB.CcsukAirInventoryHouse;
			}
			else if (job is CusMAWB)
			{
				controllerId = ControllerIDs.Customs.GB.CcsukAirInventory;
			}
			else if (job is GenralEdiMessage || job is EDIMessage)
			{
				controllerId = ControllerIDs.Customs.GB.CcsukGenralMessage;
			}
			else if (job is ForwardingConsol)
			{
				controllerId = ControllerIDs.JobConsol;
			}
			else if (job is ForwardingShipment)
			{
				controllerId = ControllerIDs.JobShipment;
			}
			else if (job is EU.Business.Declaration.CusEntryHeader)
			{
				controllerId = ControllerIDs.Customs.JobDeclaration;
				pk = ((EU.Business.Declaration.CusEntryHeader)job).Declaration.PK.ToGuid();
			}
			else if (job is ICcsukCusAwb)
			{
				var iawb = job as ICcsukCusAwb;
				controllerId = iawb.ModuleControllerId;
				pk = iawb.PK.ToGuid();
			}
			if (controllerId != null)
			{
				try
				{
					return ObjectFactory.Get<IShowEditFormUrlCreator>().Create(controllerId, pk);
				}
				catch (NullReferenceException) { } // If your test DB is not properly licenced, the above will throw an exception. But ignore it. 
			}
			return string.Empty;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		GlbStaff TryReallyHardToGetStaffFromJobOrLinkedJob(BusinessObject bizObj)
		{
			GlbStaff staff = null;
			if (bizObj is ICcsukCusAwb)
			{
				var awb = (ICcsukCusAwb)bizObj;
				staff = awb.UserInChargeOfJob;  // Get from CS_JE or from last message. Assumes CS_JE is set properly and that it's a house. 
				if (staff == null)
				{
					Customs.Business.BaseJobDeclaration dec = null;
					ForwardingShipment shipment = null;
					var hawb = awb as CusHAWB;
					var mawb = awb as CusMAWB;
					if (hawb != null)
					{
						if (hawb.Shipment != null)
						{
							shipment = hawb.Shipment;
						}
					}
					else if (mawb != null)
					{
						if (mawb != null)
						{
							if (mawb.IsBasic)
							{
								if (mawb.Consol != null && mawb.Consol.IsDirect && mawb.Consol.Shipments.Count > 0)
								{
									shipment = mawb.Consol.Shipments[0];  // A direct consol can have no more than 1 shipment
								}
								if (shipment == null)
								{
									shipment = mawb.MasterLevelHouseHelper.Shipment;
									dec = mawb.MasterLevelHouseHelper.Declaration;
								}
							}
						}
					}
					else if (bizObj is SplitConsignment)
					{
						dec = ((SplitConsignment)bizObj).OwnDeclaration;
					}

					if (shipment != null && dec == null)
					{
						dec = (Customs.Business.BaseJobDeclaration)shipment.GetDeclarationFor(awb.Branch.Company.PK);
					}
					if (dec != null)
					{
						staff = dec.CusAgent;
					}
				}
			}
			else if (bizObj is EU.Business.Declaration.CusEntryHeader)
			{
				staff = ((EU.Business.Declaration.CusEntryHeader)bizObj).Declaration.CusAgent;
			}
			else if (bizObj is Business.Declaration.JobDeclaration)
			{
				staff = ((Business.Declaration.JobDeclaration)bizObj).CusAgent;
			}
			else if (bizObj is EDIMessage)
			{
				staff = ((EDIMessage)bizObj).UserWhoQueuedThisRecord;
			}
			return staff;
		}

		readonly BusinessObjectFactory businessObjectFactory;
		ToWhom whom;
		public readonly GlbStaff Staff;
		readonly BusinessObject job;
	}
}
