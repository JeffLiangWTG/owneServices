using CargoWise.IO;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor.Testing;
using Enterprise.Messaging.Integration;
using NUnit.Framework;
using WTG.TestHelpers.Xml;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(ILMessagePacker))]
	sealed class ILMessagePackerTest : UniversalCustomsEDIMessagePackerTest<ILMessagePacker>
	{
		public void TestPack()
		{
			var messageText = new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("Manifest_1171.xml"));

			var jobDeclaration = Factory.New<JobDeclaration>();
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();

			var message = Factory.New<EDIMessage>();
			message.EM_LinkTable = CusEntryHeader.Schema.TableName;
			message.EM_LinkUniqueID = entryHeader.PK;
			message.EM_MessageText = messageText;

			var logger = new LoggingInformation();
			var packer = new ILMessagePacker();
			var interchange = Factory.New<ILEDIInterchange>();
			var errorReason = packer.Pack(message, interchange, logger);

			AssertNotNull(interchange);
			AssertEquals(ZString.Empty, errorReason);
		}

		public void TestMessagesPopulateNewInterchange()
		{
			var messageText = new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("Manifest_1171.xml"));

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "HMZ";
			company.GC_Name = "HMY";

			var credential = CreateCredential(company);

			var jobDeclaration = Factory.New<JobDeclaration>();
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();

			var message1 = ILBusinessTestHelper.CreateMessage(Factory, "120");
			message1.EM_LinkedObject = entryHeader;
			message1.EM_GP = credential.PK;
			message1.EM_ApplicationCode = "ILC";
			message1.EM_MessageText = messageText;

			var message2 = ILBusinessTestHelper.CreateMessage(Factory, "110");
			message2.EM_LinkedObject = entryHeader;
			message2.EM_GP = ZGuid.Empty;
			message2.EM_ApplicationCode = "ILC";
			message2.EM_MessageText = messageText;

			var logger = new LoggingInformation();
			var packer = new ILMessagePacker();
			var interchange1 = Factory.New<ILEDIInterchange>();
			var interchange2 = Factory.New<ILEDIInterchange>();
			var errorReason1 = packer.Pack(message1, interchange1, logger);
			var errorReason2 = packer.Pack(message2, interchange2, logger);

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

			void AssertInterchangeDetail(ILEDIInterchange interchange, EDIMessage message, string sender)
			{
				AssertEquals("EI_ApplicationCode", ILEDIInterchange.ApplicationCodes.ILCustoms, interchange.EI_ApplicationCode);
				AssertEquals("EI_ReceiveTransmit", EDIMessage.Direction.Transmit, interchange.EI_ReceiveTransmit);
				AssertEquals("EI_Status", EDIMessage.Status.Queued, interchange.EI_Status);
				AssertEquals("EI_IsActive", true, interchange.EI_IsActive);
				AssertEquals("EI_GB", message.EM_GB, interchange.EI_GB);
				AssertEquals("EI_TransportType", EDIInterchangeTransportTypeList.Codes.xT, interchange.EI_TransportType);
				AssertEquals("EI_InterchangeType", message.EM_MessageType, interchange.EI_InterchangeType);
				AssertEquals("EI_GP", message.EM_GP, interchange.EI_GP);
				AssertEquals("EM_EI", message.EM_EI, interchange.PK);
				AssertEquals("EI_From", sender, interchange.EI_From);
			}
		}

		[TestDate(2023, 05, 13, 12, 0, 0)]
		public void TestInterchangeText_WrapSoapStructure()
		{
			var factory = Factory;
			var credential = CreateCredential(GlbCompany.CurrentCompany);

			var messageText = new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("Manifest_1171.xml"));

			var jobDeclaration = factory.New<JobDeclaration>();
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();

			var message = ILBusinessTestHelper.CreateMessage(factory, "MAN");
			message.EM_LinkedObject = entryHeader;
			message.EM_MessageText = messageText;
			message.EM_MessageSubType = "170";
			message.EM_GP = credential.PK;

			Factory.Save();

			var packer = new ILMessagePackerForTest();
			var interchange = Factory.New<ILEDIInterchange>();
			var errorReason = packer.Pack(message, interchange, new LoggingInformation());

			var expectedText = new EmbeddedResourceRetriever().GetString(ILBusinessTestHelper.GetEmbeddedResourcePath("ILInterchangeMessage.xml"));
			AssertNullOrEmpty("errorReason", errorReason);
			var interchangeText = interchange.EI_BodyData.ToUTF8();
			XmlComparison.CompareAndAssertXml(expectedText, interchangeText, "Both XMLs should be equal");
		}

		protected override string ApplicationCode => EDIMessage.ApplicationCodes.ILCustoms;

		GlbCompanyCredentialICS2 CreateCredential(GlbCompany glbCompany)
		{
			var result = Factory.New<GlbCompanyCredentialICS2>();
			result.GP_GC = glbCompany.PK;
			result.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			result.GP_ExpiryDate = ZDateTime.Today.AddDays(2);
			result.GP_MailBoxID = "560038416";

			return result;
		}
	}

	sealed class ILMessagePackerForTest : ILMessagePacker
	{
		protected override ZGuid CreateNewSessionGUID() => ZGuid.BrettsGuid;
	}
}
