using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.CN.Business.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.Customs.CN.DataTransfer.Universal.Testing
{
	public class DeclarationDataObjectWriterTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestPopulateContainerSize()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();

			var refContainer = Factory.New<RefContainer>();
			refContainer.RC_Code = "C-LD";
			var cnRefMap = refContainer.CodeMapCollection.AddNew();
			cnRefMap.RCM_RN_NKCountry = "CN";
			cnRefMap.RCM_Code = "12";

			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_RC = refContainer.PK;

			var writer = new CNJobDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var shipment = writer.GetDataObject(declaration);

			AssertEquals(1, shipment.ContainerCollection.Count);
			AssertEquals("CustomsContainerSize.Code", "12", shipment.ContainerCollection[0].CustomsContainerSize.Code);
		}

		public void TestPopulateExtraOrganisation()
		{
			var buyer1 = Factory.New<OrgHeader>();
			buyer1.OH_FullName = "Buyer 1";
			var address1 = buyer1.Addresses.AddNew();
			address1.OA_OH = buyer1.PK;
			address1.CompanyName = "Buyer 1";

			var buyer2 = Factory.New<OrgHeader>();
			buyer2.OH_FullName = "Buyer 2";
			var buyer2Contact = buyer2.Contacts.AddNew();
			buyer2Contact.OC_ContactName = "JOHN SMITH";
			buyer2Contact.OC_IsActive = true;
			var allocation = buyer2Contact.Allocations.AddNew();
			allocation.PC_Type = OrgConstants.ContactAllocationType.CNCUS;
			var phone1 = buyer2Contact.PhoneContactItems.AddNew();
			phone1.OI_ContactItemType = OrgContactItemTypes.Codes.Phone;
			phone1.OI_Description = PhoneContactItemDescriptionList.Codes.Work;
			phone1.OI_Address_Formatted = "1234567";
			var address2 = buyer2.Addresses.AddNew();
			address2.OA_OH = buyer2.PK;
			address2.CompanyName = "Buyer 2";

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_FullName = "Manufacturer";
			var address3 = manufacturer.Addresses.AddNew();
			address3.OA_OH = manufacturer.PK;
			address3.CompanyName = "Manufacturer";

			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			declaration.JE_OH_Buyer = buyer2.PK;
			declaration.BuyerDocAddress.OrganisationPK = buyer1.PK;

			declaration.ManufacturerDocumentaryAddress.OrganisationPK = manufacturer.PK;

			var writer = new CNJobDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var shipment = writer.GetDataObject(declaration);

			AssertOrganizationAddress("BuyerDocumentaryAddress", "Buyer 1");
			AssertOrganizationAddress("Manufacturer", "Manufacturer");
			AssertOrganizationAddress("Declarant", GlbCompany.CurrentCompany.OrgProxy.OH_FullName);

			void AssertOrganizationAddress(string addressType, ZString? expectedCompanyName)
			{
				AssertEquals(addressType, expectedCompanyName, shipment.OrganizationAddressCollection.SingleOrDefault(add => add.AddressType.Value == addressType)?.CompanyName);
			}
		}

		public void TestPopulateAttachedDocumentCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var eDoc = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Test1.pdf", "TST");
			var attachment = instruction.Attachments.AddNew();
			attachment.EDoc = eDoc.UniqueKey;
			attachment.AttachmentType = "00000001";
			attachment.AttachmentNumber = "123456789012";
			var entry = declaration.ActiveEntryHeaders.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;

			var writer = new CNJobDeclarationDataObjectWriterPopulateAttachedDocumentCollection(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			using (var shipment = writer.GetDataObject(declaration))
			{
				AssertEquals(1, shipment.AttachedDocumentCollection.Count);

				var attachedDocument = shipment.AttachedDocumentCollection[0];
				AssertEquals("FileName", "Test1.pdf", attachedDocument.FileName);
				AssertEquals("ImageData", 1, attachedDocument.ImageData.Length);
				AssertEquals("Type.Code", "TST", attachedDocument.Type.Code);
				AssertEquals("Type.Description", "", attachedDocument.Type.Description);
			}
		}

		public void TestPopulateCusAgentCertificates()
		{
			var broker = Factory.New<GlbStaff>();
			broker.GS_Code = "TBR";
			var brkCrt = broker.Certificates.AddNew();
			brkCrt.XZ_Type = "BRK";
			brkCrt.XZ_RN_NKCountryOfIssuance = "CN";
			brkCrt.XZ_Comment = "Broker Name";
			var cnoCrt = broker.Certificates.AddNew();
			cnoCrt.XZ_Type = "CNO";
			cnoCrt.XZ_RN_NKCountryOfIssuance = "CN";
			cnoCrt.XZ_Comment = "CNO Name";

			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			var writer = new CNJobDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var shipment = writer.GetDataObject(declaration);
			AssertAddInfo(shipment, "BrokerNumber", null);
			AssertAddInfo(shipment, "BrokerName", null);
			AssertAddInfo(shipment, "OperatorCardID", null);
			AssertAddInfo(shipment, "OperatorName", null);

			declaration.JE_GS_NKCusAgent = "TBR";
			shipment = writer.GetDataObject(declaration);
			AssertAddInfo(shipment, "BrokerNumber", "");
			AssertAddInfo(shipment, "BrokerName", null);
			AssertAddInfo(shipment, "OperatorCardID", "");
			AssertAddInfo(shipment, "OperatorName", null);

			brkCrt.XZ_RefNumber = "BRK001";
			cnoCrt.XZ_RefNumber = "CNO001";
			shipment = writer.GetDataObject(declaration);
			AssertAddInfo(shipment, "BrokerNumber", "BRK001");
			AssertAddInfo(shipment, "BrokerName", "Broker Name");
			AssertAddInfo(shipment, "OperatorCardID", "CNO001");
			AssertAddInfo(shipment, "OperatorName", "CNO Name");
		}

		public void TestPopulateOverriddenAddresses()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var docAddress = declaration.SupplierDocumentaryAddress;
			docAddress.E2_AddressOverride = true;
			docAddress.OverseasPartyCodeType = "AEO";
			docAddress.OverseasPartyCode = "SG444";

			docAddress = declaration.ImporterDocumentaryAddress;
			docAddress.E2_AddressOverride = true;
			docAddress.SocialCreditCode = "111";
			docAddress.CustomsCode = "444";

			docAddress = declaration.BuyerDocAddress;
			docAddress.E2_AddressOverride = true;
			docAddress.CustomsCode = "222";

			var writer = new CNJobDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			var shipment = writer.GetDataObject(declaration);

			AssertRegistrationNumber("SupplierDocumentaryAddress", "AEO", "Authorized Economic Operator", "SG", "Singapore", "444");
			AssertRegistrationNumber("ImporterDocumentaryAddress", "USC", "Unified Social Credit Identifier", "CN", "China", "111");
			AssertRegistrationNumber("ImporterDocumentaryAddress", "CCD", "Customs Client Code", "CN", "China", "444");
			AssertRegistrationNumber("BuyerDocumentaryAddress", "CCD", "Customs Client Code", "CN", "China", "222");

			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			docAddress = declaration.ManufacturerDocumentaryAddress;
			docAddress.E2_AddressOverride = true;
			docAddress.CIQCode = "333";
			shipment = writer.GetDataObject(declaration);
			AssertRegistrationNumber("Manufacturer", "CIQ", "China Import-Export Inspection and Quarantine Code", "CN", "China", "333");

			void AssertRegistrationNumber(string addressType, string numbertType, ZString? expectedTypeDesc, ZString? expectedCountryCode, ZString? expectedCountryName, ZString? expectedValue)
			{
				var address = shipment.OrganizationAddressCollection.SingleOrDefault(add => add.AddressType.Value == addressType);
				var number = address.RegistrationNumberCollection.SingleOrDefault(x => x.Type.Code.Value == numbertType);
				CombineAssertions(addressType, () =>
				{
					AssertEquals("Type.Description", expectedTypeDesc, number.Type.Description.Value);
					AssertEquals("CountryOfIssue.Code", expectedCountryCode, number.CountryOfIssue.Code);
					AssertEquals("CountryOfIssue.Name", expectedCountryName, number.CountryOfIssue.Name);
					AssertEquals("Value", expectedValue, number.Value);
				});
			}
		}

		public void TestPopulateDestinationParty()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "CNC";
			var branch1 = company.Branches.AddNew();
			branch1.GB_Code = "CN1";
			var branch2 = company.Branches.AddNew();
			branch2.GB_Code = "CN2";
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "ENT";
			registrationKey.ServerCodeForTest = "SVR";

			Factory.SaveForTesting();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GC = company.PK;
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var writer = new CNJobDeclarationDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));

			using (CNSWClientSettingCheckerTest.TemporarilySetCNSWClientSetting(Factory.BOFactory, company.PK.ToGuid(), Guid.Empty))
			using (CNSWClientSettingCheckerTest.TemporarilySetCNSWClientSetting(Factory.BOFactory, Guid.Empty, branch1.PK.ToGuid()))
			{
				declaration.JE_GB = branch1.PK;
				AssertAddInfo(writer.GetDataObject(declaration), Constants.AddInfoKeys.JobDeclaration.DestinationParty, "ENTCNCSVRCN1_CSW");

				declaration.JE_GB = branch2.PK;
				AssertAddInfo(writer.GetDataObject(declaration), Constants.AddInfoKeys.JobDeclaration.DestinationParty, "ENTCNCSVR_CSW");
			}
		}

		void AssertAddInfo(Shipment shipment, string key, ZString? expectedValue)
		{
			AssertEquals(key, expectedValue, shipment.AddInfoCollection.SingleOrDefault(item => item.Key.Value == key)?.Value);
		}

		class CNJobDeclarationDataObjectWriterPopulateAttachedDocumentCollection : CNJobDeclarationDataObjectWriter
		{
			internal CNJobDeclarationDataObjectWriterPopulateAttachedDocumentCollection(IDataWritingManager manager) : base(manager) { ShouldPopulateAttachedDocumentCollection = true; }
		}

		protected override void SetUp()
		{
			setupCreator = ((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			base.SetUp();
		}
		IDisposable setupCreator;

		protected override void TearDown()
		{
			base.TearDown();
			if (setupCreator != null)
			{
				setupCreator.Dispose();
				setupCreator = null;
			}
		}
	}
}
