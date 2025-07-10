using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.IO;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Customs.HK.ServiceTasks.Testing
{
	[TestedType(typeof(MessageSenderServiceTask))]
	sealed class MessageSenderServiceTaskTest : ServiceTaskTestCase<MessageSenderServiceTask>
	{
		public void TestRunForAllHKCompanies()
		{
			using (Registry.Business.eHubMessagingRegistry.Instance.SendHKISACEViaEHub.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var tempTraxonOutPath = TestHelper.CreateDirectory();
				var tempTraxonOutPath1 = TestHelper.CreateDirectory();

				try
				{
					var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
					currentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.HongKong;
					var company2 = Factory.NewWithValidTestData<GlbCompany>();
					company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.HongKong;
					var branch2 = company2.Branches.AddNew();
					branch2.GB_Code = "HK2";
					Factory.Save();

					TestHelper.SetUpRegistry(GlbCompany.CurrentCompany.PK.ToGuid(), "RHKAGT021332880/HKG81", "PRDAGENT027", tempTraxonOutPath);
					TestHelper.SetUpRegistry(company2.PK.ToGuid(), "RHKAGT021332880/HKG82", "PRDAGENT028", tempTraxonOutPath1);

					var interchange = Factory.New<EDIInterchange>();
					interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.Traxon;
					interchange.EI_Status = EDIInterchange.Status.Queued;
					interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
					interchange.EI_HeaderText = "HEADERTEXT";
					interchange.EI_BodyText = "BODYTEXT";
					interchange.EI_FooterText = "FOOTERTEXT";
					interchange.EI_InterchangeNum = "12345";
					interchange.EI_GB = Enterprise.MasterFiles.Business.GlbBranch.CurrentBranch.PK;

					var interchange2 = Factory.New<EDIInterchange>();
					interchange2.EI_ApplicationCode = EDIInterchange.ApplicationCodes.Traxon;
					interchange2.EI_Status = EDIInterchange.Status.Queued;
					interchange2.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
					interchange2.EI_HeaderText = "HEADERTEXT";
					interchange2.EI_BodyText = "BODYTEXT";
					interchange2.EI_FooterText = "FOOTERTEXT";
					interchange2.EI_InterchangeNum = "12346";
					interchange2.EI_GB = branch2.PK;
					Factory.Save();

					new MessageSenderServiceTask().RunTask();
					interchange.Reload();
					interchange2.Reload();

					AssertEquals("InterchangeState", EDIInterchange.Status.Sent, interchange.EI_Status);
					AssertEquals("InterchangeState", EDIInterchange.Status.Sent, interchange2.EI_Status);
				}
				finally
				{
					TempDirectory.DeleteDirectory(new DirectoryInfo(tempTraxonOutPath));
					TempDirectory.DeleteDirectory(new DirectoryInfo(tempTraxonOutPath1));
				}
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIMessageSchema.Constants.TableName,
						"HK Customs messages outbound",
						EDIMessageSchema.Constants.EM_Status + "=" + EDIMessage.Status.Queued,
						EDIMessageSchema.Constants.EM_ReceiveTransmit + "=" + EDIMessage.Direction.Transmit,
						EDIMessageSchema.Constants.EM_IsActive + "=Y",
						EDIMessageSchema.Constants.EM_ApplicationCode + "=" + EDIMessage.ApplicationCodes.Traxon,
						EDIMessageSchema.Constants.EM_HeldUntilDate + " IS PASTORNULL"),
				};
			}
		}
	}
}
