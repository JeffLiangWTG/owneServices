using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Declaration.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.Business.CusEntryHeader;

namespace Enterprise.Customs.GB.Chief.Declaration.Testing
{
	[TestedType(typeof(ChiefEntryDeclarationMessageProcessor))]
	public class ChiefEntryDeclarationMessageProcessorTest : GBAutoSendCustomsMessageProcessorTest
	{
		protected override void SetEntryClearedStatus(CusEntryHeader entry)
		{
			entry.EntryNumber = "123";
			entry.CH_EntryStatus = EntryStatusList.Codes.Clear;
		}

		protected override void PrepareDeclaration(BaseJobDeclaration declaration)
		{
			var dec = (JobDeclaration)declaration;
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			dec.JE_CustomsProfile = "DSK";
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			Factory.Save();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			dec.JE_DeclarationType = "IFD";
			dec.JE_OwnerRef = "OwnerRef";
		}
		protected override void AssertEntryAndMessageResultForEndToEndTest(CusEntryHeader entry)
		{
			CombineAssertions(() =>
			{
				AssertEquals(1, entry.Messages.Count);
				AssertEquals("CH_Status", "AWR", entry.CH_Status);
				AssertEquals("CH_EntryStatus", "AWR", entry.CH_EntryStatus);
				var originalMessage = entry.Messages[0];
				AssertEquals("EM_MessageType", "NEW", originalMessage.EM_MessageType);
				AssertEquals("EM_MessageSubType", "IFD", originalMessage.EM_MessageSubType);
				AssertEquals("EM_Status", "QUE", originalMessage.EM_Status);
				AssertEquals("EM_HeldUntilDate", new ZDateTime(2019, 1, 1, 12, 11, 0), originalMessage.EM_HeldUntilDate);
				AssertNotContains("No <<BOX7>> placeholder", "<<BOX7>>", originalMessage.EM_MessageText);
				AssertContains("<<BOX7>> is replaced by JE_OwnerRef", "'DMS+OWNERREF'", originalMessage.EM_MessageText);
			});
		}

		protected override ZString ExpectedMessageDescription => Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;

		protected override void SetUp()
		{
			base.SetUp();
			GBCustomsDataRegistry.Instance.BadgeCodes.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new BadgeCodeSettingCollection
			{
				new BadgeCodeSetting
				{
					BadgeCode = "DSK",
					RL_PortCode = "GBLBA",
					Direction = "IMP",
					CSPCode = GatewayList.Codes.CCSUKviaNTMsgGW,
					MasterUcrCalculationMode = Registry.MucrGenerationStyles.Codes.Ccsuk,
					ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF,
				}
			});
			GBCustomsDataRegistry.Instance.Credentials.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new CredentialsSettingCollection
			{
				new CredentialsSetting
				{
					BadgeCode = "DSK",
					Printer = "Location",
					Company = "Role",
					Username = "Username",
					Password = "Password"
				}
			});

			GBCustomsDataRegistry.Instance.CcsukLocalHostMnemonic.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "GREY");
			GBCustomsDataRegistry.Instance.MinutesToWaitAfterRoute6AcceptanceToGetClearance.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 10);
		}
	}
}
