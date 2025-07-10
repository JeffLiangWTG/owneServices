using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	public class MessageHostedServiceRequirementTest : TestCaseWithFactory
	{
		public void TestCheckCLSetupSMSMessageSendingConfig_True()
		{
			CLCustomsDataRegistry.Instance.CLSMSMessageSending.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, CreateSMSMessageSending());
			Factory.Save();

			AssertEquals(ZString.Empty, MessageHostedServiceRequirement.CheckCLSetupSMSMessageSendingConfig());
		}

		public void TestCheckCLSetupSMSMessageSendingConfig_False()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = CountryCodes.Chile;
			Factory.Save();

			AssertEquals("There is no SMS Message Sending configuration on Chilean companies.", MessageHostedServiceRequirement.CheckCLSetupSMSMessageSendingConfig());
		}

		CLSMSMessageSending CreateSMSMessageSending()
		{
			var sms = new CLSMSMessageSending(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);

			sms.MachineName = "Machine Name";
			sms.ApplicationNodePassword = "1234";
			sms.RunningIntervalInSeconds = 15;
			sms.SendFolder = @"D:\Folders\SendFolder";
			sms.UnknownFolder = @"D:\Folders\UnknownFolder";
			sms.InvalidFolder = @"D:\Folders\InvalidFolder";
			sms.RejectedFolder = @"D:\Folders\RejectedFolder";
			sms.ReceiveFolder = @"D:\Folders\ReceiveFolder";
			sms.AcceptedFolder = @"D:\Folders\AcceptedFolder";

			return sms;
		}
	}
}
