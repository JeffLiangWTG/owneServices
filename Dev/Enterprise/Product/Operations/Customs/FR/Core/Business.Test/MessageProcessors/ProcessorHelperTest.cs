using System;
using System.Xml;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessageProcessors.Testing
{
	class ProcessorHelperTest : TestCaseWithFactory
	{
		public void TestSendEmail()
		{
			var group = SetUpStaffAndGroup();

			Factory.Save();
			FRCustomsDataRegistry.Instance.DeltaTResponseNotificationGroup.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, new Enterprise.Registry.Business.Customs.ManifestGroupNotification("ESG", group.PK, false));

			var declaration = Factory.New<JobDeclaration>();
			var entry = declaration.CustomsEntryHeaders.AddNew();
			entry.FillWithValidTestData();
			entry.CH_SystemCreateUser = "~BB";
			declaration.JE_DeclarationReference = "dec";

			var body = "test";

			ProcessorHelper.SendEmail(entry.Factory, entry.CH_SystemCreateUser, true, body, "New DCG response received. Declaration: dec Reference: entry", FRCustomsDataRegistry.Instance.DeltaGResponseNotificationGroup);

			var mails = Env.OutgoingCustomsMailManager.EmailsCreated;
			CombineAssertions(() =>
			{
				AssertEquals(1, mails.Count);
				AssertEquals("New DCG response received. Declaration: dec Reference: entry", mails[0].Subject);
				AssertEquals(@"test", mails[0].Body);
			});
		}

		public void TestGetElementTextByTagNameWithDefaultValue()
		{
			var xmlText = @"<Message><MyTag>Test</MyTag></Message>";

			var xmlDoc = new XmlDocument();
			xmlDoc.LoadXml(xmlText);

			AssertEquals("Test", ProcessorHelper.GetElementTextByTagNameWithDefaultValue(xmlDoc, "MyTag"));
			AssertEquals(ZString.Empty, ProcessorHelper.GetElementTextByTagNameWithDefaultValue(xmlDoc, "UnavailableTag"));
		}

		GlbGroup SetUpStaffAndGroup()
		{
			var staff = Factory.New<GlbStaff>();
			staff.FillWithValidTestData();
			staff.GS_Code = "~BB";
			staff.GS_EmailAddress = "test@cargowise.com";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			var groupLink = Factory.NewWithValidTestData<GlbGroupLink>();
			groupLink.GK_GG = group.PK;
			groupLink.GK_GS = staff.PK;
			group.Staff.Load();
			return group;
		}

		public void TestAdaptMessageToExpectedFormat()
		{
			var messageText = @"<Message>
  <EnveloppeMessage>
	<schemaID>DELTAT_MESSAGE_CC029B</schemaID>
	<schemaVersion>1.0</schemaVersion>
	<partyId>33159700500064</partyId>
	<transactionId>HYEDFRCMT+DT_DEC+00050271</transactionId>
	<numseq>0</numseq>
  </EnveloppeMessage>
  <ReponseDeclaration>
	<CC029B>
		<SynIdeMES1>UNOC</SynIdeMES1>
		<SynVerNumMES2>3</SynVerNumMES2>
		<MesSenMES3>NTA.FR</MesSenMES3>
		<MesRecMES6>OPE.FR</MesRecMES6>
		<DatOfPreMES9>190411</DatOfPreMES9>
		<TimOfPreMES10>1128</TimOfPreMES10>
		<IntConRefMES11>JNeZ6B4EA9GERr</IntConRefMES11>
		<TesIndMES18>0</TesIndMES18>
		<MesIdeMES19>BH_JOBREFERENCE</MesIdeMES19>
		<MesTypMES20>CC029B</MesTypMES20>
		<HEAHEA>
			<RefNumHEA4>LRNScenario1b</RefNumHEA4>
			<DocNumHEA5>18FR00400000000273</DocNumHEA5>
		</HEAHEA>
</CC029B>
</ReponseDeclaration>
</Message>";

			var textResult = ProcessorHelper.AdaptMessageToExpectedFormat(messageText);

			AssertContains("http://www.w3.org/2001/XMLSchema-instance", textResult);
			AssertContains(@"<HEAHEA xmlns="""">", textResult);
		}
	}
}
