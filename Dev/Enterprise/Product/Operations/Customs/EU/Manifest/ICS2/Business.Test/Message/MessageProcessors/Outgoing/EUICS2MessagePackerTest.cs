using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	[TestedType(typeof(EUICS2MessagePacker))]
	sealed class EUICS2MessagePackerTest : UniversalCustomsEDIMessagePackerTest<EUICS2MessagePacker>
	{
		public void TestPack()
		{
			CreateCredential(GlbCompany.CurrentCompany);
			var header = Factory.New<AsycudaManifestHeader>();

			var message = Factory.New<EDIMessage>();
			message.EM_LinkTable = "AsycudaManifestHeader";
			message.EM_LinkUniqueID = header.PK;

			var logger = new LoggingInformation();
			var packer = new EUICS2MessagePacker();

			var interchange = Factory.New<EDIInterchange>();

			var errorReason = packer.Pack(message, interchange, logger);

			AssertNotNull(interchange);
			AssertEquals(ZString.Empty, errorReason);
		}

		public void TestMessagesPopulateNewInterchange()
		{
			CreateCredential(GlbCompany.CurrentCompany);
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "HMZ";
			company.GC_Name = "HMY";

			var credential = CreateCredential(company);

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_CustomsProfile = company.GC_Code;

			AssertSame("Precondition", credential, manifestHeader.ICS2Credential);

			var message1 = EUICS2MessageTestHelper.CreateMessage(Factory, MessageTypes.Codes.F24);
			message1.EM_LinkUniqueID = manifestHeader.PK;
			message1.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			message1.EM_GP = credential.PK;

			var message2 = EUICS2MessageTestHelper.CreateMessage(Factory, MessageTypes.Codes.F24);
			message2.EM_LinkUniqueID = manifestHeader.PK;
			message2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			message2.EM_GP = ZGuid.Empty;

			var interchange1 = Factory.New<EDIInterchange>();
			var interchange2 = Factory.New<EDIInterchange>();

			var logger = new LoggingInformation();
			var packer = new EUICS2MessagePacker();
			packer.Pack(message1, interchange1, logger);
			packer.Pack(message2, interchange2, logger);

			Factory.Save();

			message1.Reload();
			message2.Reload();

			CombineAssertions(() =>
			{
				AssertNotNull("Interchange 1 is linked to message 1", interchange1);
				AssertInterchangeDetail(interchange1, message1, company.LicenceKeyIdentifier);

				AssertNotNull("Interchange 2 is linked to message 2", interchange2);
				AssertInterchangeDetail(interchange2, message2, GlbCompany.CurrentCompany.LicenceKeyIdentifier);
			});

			void AssertInterchangeDetail(EDIInterchange interchange, EDIMessage message, string sender)
			{
				AssertEquals("EI_ApplicationCode", "IC2", interchange.EI_ApplicationCode);
				AssertEquals("EI_ReceiveTransmit", "TRX", interchange.EI_ReceiveTransmit);
				AssertEquals("EI_Status", "QUE", interchange.EI_Status);
				AssertEquals("EI_IsActive", true, interchange.EI_IsActive);
				AssertEquals("EI_To", "EUICS2TEST", interchange.EI_To);
				AssertEquals("EI_GB", message.EM_GB, interchange.EI_GB);
				AssertEquals("EI_TransportType", "XTT", interchange.EI_TransportType);
				AssertEquals("EI_InterchangeType", message.EM_MessageType, interchange.EI_InterchangeType);
				AssertEquals("EI_GP", message.EM_GP, interchange.EI_GP);
				AssertEquals("EM_EI", message.EM_EI, interchange.PK);
				AssertEquals("EI_From", sender, interchange.EI_From);
			}
		}

		[TestDate(2023, 05, 13, 12, 0, 0)]
		public void TestInterchangeText_WrapSoapStructure()
		{
			CreateCredential(GlbCompany.CurrentCompany);
			var types = new[] { MessageProviderHelper.RefSysConfigCodes.ICS2SID, MessageProviderHelper.RefSysConfigCodes.ICS2SMS1 };

			types.ForEach(c =>
			{
				var configType = Factory.New<RefSysConfigType>();
				configType.ZRT_ConfigCode = c;
				configType.ZRT_Description = configType.ZRT_LongDescription = $"{c} For Testing";
			});

			var idConfig = Factory.New<RefSysConfig>();
			idConfig.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2SID;
			idConfig.ZRC_StartDate = new ZDateTime(1900, 01, 01);
			idConfig.ZRC_EndDate = new ZDateTime(2079, 06, 06);
			idConfig.ZRC_StringValue = "DE966882266928828";

			var memberStateConfig = Factory.New<RefSysConfig>();
			memberStateConfig.ZRC_ZRT_NKConfigCode = MessageProviderHelper.RefSysConfigCodes.ICS2SMS1;
			memberStateConfig.ZRC_StartDate = new ZDateTime(1900, 01, 01);
			memberStateConfig.ZRC_EndDate = new ZDateTime(2023, 06, 29, 11, 00, 00);
			memberStateConfig.ZRC_StringValue = "DA";

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.SpecificCircumstanceIndicator = "F24";
			manifestHeader.AMA_CustomsProfile = GlbCompany.CurrentCompany.GC_Code;

			var message = EUICS2MessageTestHelper.CreateMessage(Factory, MessageTypes.Codes.F24);
			message.EM_LinkUniqueID = manifestHeader.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;

			var path = "Enterprise.Customs.EU.Manifest.ICS2.Business.Test.Message.MessageProcessors.Outgoing.TestFiles";
			var reader = new Customs.Business.Testing.TestFileReader(typeof(EUICS2MessagePackerTest));

			var eDoc1 = manifestHeader.DocManagerInfo.AddFileOrDocument(reader.GetEmbeddedFileData(path, "TestImage.jpg"), "Invoice1.jpg", "CIV");
			var eDoc2 = manifestHeader.DocManagerInfo.AddFileOrDocument(reader.GetEmbeddedFileData(path, "TestImage.jpeg"), "Invoice2.jpeg", "CIV");

			(eDoc1 as IBusinessObjectInternals).Row[StorageDocsSchema.PK.Name] = new Guid("e7b2d687-216a-45dd-86a6-c71576bc3899");
			(eDoc2 as IBusinessObjectInternals).Row[StorageDocsSchema.PK.Name] = new Guid("90ba2040-916f-4519-bef9-93f2dc3453b3");

			var pkField = typeof(BusinessObject).GetField("pk", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
			pkField.SetValue(eDoc1, new ZGuid("e7b2d687-216a-45dd-86a6-c71576bc3899"));
			pkField.SetValue(eDoc2, new ZGuid("90ba2040-916f-4519-bef9-93f2dc3453b3"));

			message.EM_MessageText = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<IE3TST xmlns=""urn:wco:datamodel:eu:ics2:2"">
    <binaryAttachment>
      <identification>cid:{eDoc1.UniqueKey}</identification>
      <filename>Invoice1.jpg</filename>
      <MIME>image/jpg</MIME>
      <description>TestImage.jpg</description>
    </binaryAttachment>
    <binaryFile>
      <identification>cid:{eDoc2.UniqueKey}</identification>
      <filename>Invoice2.jpeg</filename>
      <MIME>image/jpeg</MIME>
      <description>TestImage.jpeg</description>
    </binaryFile>
</IE3TST>";

			Factory.Save();

			var interchange = Factory.New<EDIInterchange>();
			var packer = new EUICS2MessagePackerForTest();
			packer.Pack(message, interchange, new LoggingInformation());

			var expectedText = reader.GetEmbeddedFileText(path, "InterchangeWithAttachments.mime");
			AssertEquals(expectedText, interchange.EI_InterchangeText);
		}

		[TestDate(2023, 05, 13, 12, 0, 0)]
		public void TestInterchangeText_TestMessage()
		{
			CreateCredential(GlbCompany.CurrentCompany);
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_CustomsProfile = GlbCompany.CurrentCompany.GC_Code;

			var message = EUICS2MessageTestHelper.CreateMessage(Factory, MessageTypes.Codes.F24);
			message.EM_LinkUniqueID = manifestHeader.PK;
			message.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			message.EM_MessageText = ZString.Empty;

			var provider = new EUICS2MessagePacker();
			var interchange = Factory.New<EDIInterchange>();

			provider.Pack(message, interchange, new LoggingInformation());
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals(EDIInterchangeTypeList.Codes.TST, interchange.EI_InterchangeType);
				AssertContains("<Reference URI=\"#Messaging\">", interchange.EI_InterchangeText);
				AssertContains("<Reference URI=\"#Body\">", interchange.EI_InterchangeText);
				AssertNotContains("attachment", interchange.EI_InterchangeText);
			});
		}

		protected override string ApplicationCode => EDIMessage.ApplicationCodes.IC2;

		GlbCompanyCredentialICS2 CreateCredential(GlbCompany company)
		{
			var result = Factory.New<GlbCompanyCredentialICS2>();
			result.GP_Certificate = new Customs.Business.Testing.TestFileReader(typeof(EUICS2MessagePackerTest)).GetEmbeddedFileData("Enterprise.Customs.EU.Manifest.ICS2.Business.Test.Message.MessageProcessors.Outgoing.TestFiles", "escertificate2022_password.pfx");
			result.CurrentDecryptedCertificatePassphrase = "password";
			result.GP_GC = company.PK;
			result.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			result.GP_ExpiryDate = ZDateTime.Today.AddDays(2);

			return result;
		}
	}

	sealed class EUICS2MessagePackerForTest : EUICS2MessagePacker
	{
		protected override SoapHeaderBuilder GetSoapHeaderBuilder(EDIInterchange interchange, IReadOnlyCollection<IBinaryFile> binaryFiles)
		{
			var message = interchange.ContainedMessages[0];
			return new SoapHeaderBuilder(new SoapHeaderProviderForTest(message.EM_LinkedObject as AsycudaManifestHeader, interchange, binaryFiles));
		}
	}

	sealed class SoapHeaderProviderForTest : SoapHeaderProvider
	{
		public SoapHeaderProviderForTest(AsycudaManifestHeader header, EDIInterchange interchange, IReadOnlyCollection<IBinaryFile> binaryFiles)
			: base(header, interchange, binaryFiles)
		{
		}

		protected override string GetMessageIdCore() => $"{ZGuid.BrettsGuid}@{MessageProviderHelper.DomainName}";

		protected override string GetFromPartyValueCore()
		{
			using (ICS2CustomsDataRegistry.Instance.IncludeMemberStateInSiteID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				return $"HYECMT@{MessageProviderHelper.GetICS2SenderEORI(manifestHeader.Factory)}@{MessageProviderHelper.GetICS2SenderMemberState(manifestHeader.Factory, null)}";
			}
		}
	}
}
