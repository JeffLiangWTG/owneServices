using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Client.UPE.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Client.UPE.ServiceTask.Testing
{
	[TestedType(typeof(SMSNotificationsServiceTask))]
	class SMSNotificationsServiceTaskTest : ServiceTaskTestCase<SMSNotificationsServiceTask>
	{
		#region UPEBatchProcessor Overrides
		public void TestDefaultSchedule()
		{
			AssertEquals("5minutes", GetHostedServiceAttributes().Single().DefaultScheduleRunEvery);
		}

		public void TestHumanReadableName()
		{
			var attributes = GetHostedServiceAttributes();
			AssertEquals("SMS Notifications", attributes[0].Description);
		}

		public void TestIsEnvironmentDataValid()
		{
			const string expected = "Error: You must set the 'SMS Notification Group' in the registry with a staff group that has at least 1 valid mobile phone number.";
			UPEDataRegistry.Instance.SMSNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "ZZZ");
			Processor.RunTask();
			AssertEquals("When the environment isn't set up for SMS", true, ((TestServiceLogger)Processor.ServiceLogger).ToString().Contains(expected));
			UPEDataRegistry.Instance.SMSNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, NotificationGroup.GG_Code);
			fProcessor = null;
			((TestServiceLogger)Processor.ServiceLogger).ClearLog();
			Processor.RunTask();
			AssertEquals("When the environment is set up for SMS", false, ((TestServiceLogger)Processor.ServiceLogger).ToString().Contains(expected));
		}

		public void TestValidBranch()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = false;
			Processor.RunTask();

			AssertEquals(0, Logger.Count);

			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Processor.RunTask();

			AssertGreaterThan(Logger.Count, 0);
		}

		#endregion
		#region Execute
		[TestDate(2000, 1, 1, 12, 0, 0)]
		public void TestNotifyIfShipmentNotLoadedShortlyBeforeFlightArrival()
		{
			JobVoyage.Sailings[0].Destination.JB_E_ARV = ZDateTime.Now.AddMinutes(181);
			Factory.Save();
			Processor.RunTask();
			AssertSMSSent("No SMS until 3 hours before arrival", null);
			TestDateAttribute.Date = ZDateTime.Now.AddMinutes(2).ToDateTime();
			Processor.RunTask();
			AssertSMSSent("SMS should be sent when no masters have not been loaded 3 hours before arrival", "No masters for flight QF01 have not been loaded, and is scheduled to arrive 01-Jan-00 15:01");
			CusMAWB.CM_ArrivalDate = JobVoyage.Sailings[0].Destination.JB_E_ARV;
			fProcessor = null;
			Factory.Save();
			Processor.RunTask();
			AssertSMSSent("No SMS should be sent once a master has been loaded", null);
		}

		[TestDate(2000, 1, 1, 12, 0, 0)]
		public void TestNotifyIfShipmentNotPrealerted3HoursBeforeFlightArrival()
		{
			UPECusHAWB aSecondHAWB = (UPECusHAWB)CusMAWB.ChildBills.AddNew();
			aSecondHAWB.CS_RL_NKLoadPort = "XXXXX";
			ZDateTime arrivalDateTime = ZDateTime.Now.AddMinutes(181);
			JobVoyage.Sailings[0].Destination.JB_E_ARV = arrivalDateTime;
			CusMAWB.CM_ArrivalDate = arrivalDateTime.Date;
			CusHAWB.CS_MsgStatus = CMRBaseStatuses.Codes.NotSent;
			Factory.Save();
			Processor.RunTask();
			AssertSMSSent("No SMS until 3 hours before arrival", null);
			TestDateAttribute.Date = ZDateTime.Now.AddMinutes(2).ToDateTime();
			Processor.RunTask();
			AssertSMSSent("SMS should be sent when shipments have not been prealerted 3 hours before arrival", "Master 08122222222 has 1 of 2 shipments not pre-alerted, scheduled to arrive 01-Jan-00 15:01");
			fProcessor = null;
			CusHAWB.CS_MsgStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			Factory.Save();
			Processor.RunTask();
			AssertSMSSent("No SMS should be sent once the shipment has been prealerted", null);
		}

		[TestDate(2000, 1, 1, 12, 0, 0)]
		public void TestNotifyIfShipmentNotPrealerted3HoursBeforeFlightArrival_NonUPEBranches()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_Code = "ABC";
			Factory.Save();
			UPECusHAWB aSecondHAWB = (UPECusHAWB)CusMAWB.ChildBills.AddNew();
			aSecondHAWB.CS_RL_NKLoadPort = "XXXXX";
			CusMAWB.CM_GB = branch.PK;
			Factory.Save();
			ZDateTime arrivalDateTime = ZDateTime.Now.AddMinutes(181);
			JobVoyage.Sailings[0].Destination.JB_E_ARV = arrivalDateTime;
			CusMAWB.CM_ArrivalDate = arrivalDateTime.Date;
			CusHAWB.CS_MsgStatus = CMRBaseStatuses.Codes.NotSent;
			Factory.Save();
			Processor.RunTask();
			AssertSMSSent("No SMS until 3 hours before arrival", null);
			TestDateAttribute.Date = ZDateTime.Now.AddMinutes(2).ToDateTime();
			Processor.RunTask();
			AssertSMSSent("No SMS as CusMAWB is not UPE customisation enabled", null);
			fProcessor = null;
			CusHAWB.CS_MsgStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			Factory.Save();
			Processor.RunTask();
			AssertSMSSent("No SMS should be sent at all", null);
		}

		[TestDate(2000, 1, 1, 12, 0, 0)]
		public void TestNotifyForShipmentsWithMessagesNotReceived1HourBeforeFlight()
		{
			Processor.RunTask();
			UPECusHAWB aSecondHAWB = (UPECusHAWB)CusMAWB.ChildBills.AddNew();
			aSecondHAWB.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			ZDateTime arrivalDateTime = ZDateTime.Now.AddMinutes(61);
			JobVoyage.Sailings[0].Destination.JB_E_ARV = arrivalDateTime;
			CusMAWB.CM_ArrivalDate = arrivalDateTime.Date;
			CusHAWB.CS_CustomsStatus = CMRBaseStatuses.Codes.NotSent;
			CusHAWB.CS_MsgStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			Factory.Save();
			Processor.RunTask();
			AssertSMSSent("No SMS until 1 hour before arrival", null);
			TestDateAttribute.Date = ZDateTime.Now.AddMinutes(2).ToDateTime();
			Processor.RunTask();
			AssertSMSSent("SMS should be sent when no response (other than acknowledgement) is received 1 hour before arrival", "Master 08122222222 has 1 of 2 shipments with no response from customs, scheduled to arrive 01-Jan-00 13:01");
			fProcessor = null;
			CusHAWB.CS_CustomsStatus = CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl;
			CusHAWB.CS_MsgStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			Factory.Save();
			Processor.RunTask();
			AssertSMSSent("No SMS should be sent when a response is received", null);
		}

		#endregion
		#region Implementation
		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();
		protected override void SetUpCore()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUpCore();
			NotificationGroup.Factory.Save();
			UPEDataRegistry.Instance.SMSNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, NotificationGroup.GG_Code);
			UPEDataRegistry.Instance.SMSEmailAddressSuffix.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ".fwd@smsprovider.com.au");
		}

		void AssertSMSSent(string errorMessage, string expectedMessage)
		{
			if (expectedMessage != null)
			{
				AssertEquals(errorMessage, 1, Env.OutgoingMailManager.EmailsCreated.Count);
				EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals(errorMessage, expectedMessage, email.Body);
				AssertEquals("SMS providers typically support a maximum of only 160 characters", true, expectedMessage.Length < 160);
				Env.OutgoingMailManager.EmailsCreated.Clear();
			}
			else
			{
				AssertEquals(errorMessage, 0, Env.OutgoingMailManager.EmailsCreated.Count);
			}
		}

		SMSNotificationsServiceTask Processor
		{
			get
			{
				if (fProcessor == null)
				{
					fProcessor = new SMSNotificationsServiceTask(Logger);
				}

				return fProcessor;
			}
		}

		SMSNotificationsServiceTask fProcessor;
		TestServiceLogger Logger
		{
			get
			{
				return logger ?? (logger = new TestServiceLogger());
			}
		}

		TestServiceLogger logger;
		#region NotificationGroup / Recipient
		GlbGroup NotificationGroup
		{
			get
			{
				if (fNotificationGroup == null)
				{
					CreateNotificationGroupAndRecipient();
				}

				return fNotificationGroup;
			}
		}

		GlbGroup fNotificationGroup;
		GlbStaff fRecipient;
		void CreateNotificationGroupAndRecipient()
		{
			fNotificationGroup = Factory.New<GlbGroup>();
			fNotificationGroup.GG_Code = "NGP";
			fRecipient = fNotificationGroup.Staff.AddNew();
			fRecipient.GS_MobilePhone = "0421944317";
			fRecipient.GS_Code = "NSF";
		}

		#endregion
		#region CusMAWB / CusHAWB / JobVoyage
		UPECusMAWB CusMAWB
		{
			get
			{
				if (fCusMAWB == null)
				{
					fCusMAWB = Factory.New<UPECusMAWB>();
					fCusMAWB.CM_MAWB = "08122222222";
					fCusMAWB.CM_FlightNo = "QF01";
					fCusMAWB.CM_RL_NKLoadPort = "AUSYD";
					fCusMAWB.CM_RL_NKDischargePort = "AUMEL";
				}

				return fCusMAWB;
			}
		}

		UPECusMAWB fCusMAWB;
		UPECusHAWB CusHAWB
		{
			get
			{
				if (fCusHAWB == null)
				{
					fCusHAWB = (UPECusHAWB)CusMAWB.ChildBills.AddNew();
					fCusHAWB.CS_HAWB = "1Z1111111111111111";
				}

				return fCusHAWB;
			}
		}

		UPECusHAWB fCusHAWB;
		JobVoyage JobVoyage
		{
			get
			{
				if (fJobVoyage == null)
				{
					fJobVoyage = Factory.New<JobVoyage>();
					fJobVoyage.JV_VoyageFlight = "QF01";
					VoyageOrigin origin = fJobVoyage.Origins.AddNew();
					VoyageDestination destination = fJobVoyage.Destinations.AddNew();
					origin.JA_RL_NKPortOfLoading = "AUSYD";
					destination.JB_RL_NKPortOfDischarge = "AUMEL";
					JobSailing leg = fJobVoyage.Sailings.AddNew();
					leg.JX_JA = origin.PK;
					leg.JX_JB = destination.PK;
				}

				return fJobVoyage;
			}
		}

		JobVoyage fJobVoyage;
		#endregion
		#endregion
	}
}
