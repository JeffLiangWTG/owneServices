using CargoWise.Definitions;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsHeaderDocumentSupporter))]
	public class NctsHeaderDocumentSupporterTest : DocumentSupporterTest
	{
		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			return nctsHeader;
		}

		public void TestDocumentSupporterType()
		{
			var supporter = nctsHeader.DocumentSupporter;
			AssertType(typeof(NctsHeaderDocumentSupporter), supporter);
		}

		public void TestGetFilterValueForNCTSTransitDocumentsSupport()
		{
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var documentSupporter = nctsHeader.DocumentSupporter;

			using (NctsConfigurationTestHelper.TemporarilySetNctsHeaderConfiguration(Factory, docDataPlugInSupportForDepartureMovement: true))
			{
				AssertEquals("When DocDataPlugIn is supported, GetFilterValue", "Y", documentSupporter.GetFilterValue(DocumentFilters.NCTSTransitDocumentsSupport));
			}

			using (NctsConfigurationTestHelper.TemporarilySetNctsHeaderConfiguration(Factory, docDataPlugInSupportForDepartureMovement: false))
			{
				AssertEquals("When DocDataPlugIn is not supported, GetFilterValue", "N", documentSupporter.GetFilterValue(DocumentFilters.NCTSTransitDocumentsSupport));
			}
		}

		public void TestGetFilterValueForNonSupportedDocumentFilter()
		{
			AssertNull("For any non supported DocumentFilter, GetFilterValue", nctsHeader.DocumentSupporter.GetFilterValue(DocumentFilters.SGAIRSHP));
		}

		public void TestGetMenuTemplateFilterValue()
		{
			// Checks that our supporter is applicable to TEMPLATE PIVOTs with the named filter
			var supporter = nctsHeader.DocumentSupporter;
			var filterType = MenuTemplateFilterType.Security;
			AssertEquals("N", supporter.GetMenuTemplateFilterValue(filterType, null));
			nctsHeader = NctsHeaderWithSecurity();
			supporter = nctsHeader.DocumentSupporter;
			AssertEquals("Y", supporter.GetMenuTemplateFilterValue(filterType, null));
		}

		NctsHeader NctsHeaderWithSecurity()
		{
			nctsHeader.BH_FTZMove = true;
			NCTSTestHelper.CreateJobDocAddressForTest(Factory, "COS", nctsHeader.SecurityConsignor);
			AssertEquals("Added security at header level", true, nctsHeader.IsSecurityDeclaration);
			return nctsHeader;
		}

		public void TestBusinessContext()
		{
			var supporter = nctsHeader.DocumentSupporter;
			AssertEquals(BusinessContext.CusInBondHeader, supporter.BusinessContext);
		}

		public void TestGetSupportedDataContexts()
		{
			var supporter = nctsHeader.DocumentSupporter;
			AssertEquals(true, supporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.EuNcts)));
			AssertEquals(true, supporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GenericFreightJob)));
			AssertEquals(true, supporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GenericFreightJobInvoice)));
			AssertEquals(true, supporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.EuNcts5TAD)));
			AssertEquals(false, supporter.IsDataContextSupported(new DataContextValueForTesting(Core.Constants.DataContext.GbCcsuk)));
		}

		public void TestGetDocumentWrappersInternal()
		{
			var supporter = nctsHeader.DocumentSupporter;
			var wrappers = supporter.GetDocumentWrappers(Core.Constants.DataContext.EuNcts, null);
			var wrapper = wrappers[0];
			AssertEquals("NctsHeaderDocumentWrapper", wrapper.GetType().Name);
			AssertEquals(nctsHeader, wrapper.WrappedObject);

			wrappers = supporter.GetDocumentWrappers(Core.Constants.DataContext.EuNcts5TAD, null);
			wrapper = wrappers[0];
			AssertEquals("Phase5NctsHeaderTADDocumentWrapper", wrapper.GetType().Name);
			AssertEquals(nctsHeader, wrapper.WrappedObject);

			nctsHeader = NctsHeaderWithSecurity();
			supporter = nctsHeader.DocumentSupporter;
			wrappers = supporter.GetDocumentWrappers(Core.Constants.DataContext.EuNcts, null);
			wrapper = wrappers[0];
			AssertEquals("SecurityNctsHeaderDocumentWrapper", wrapper.GetType().Name);
			AssertEquals(nctsHeader, wrapper.WrappedObject);
		}

		public void TestGetContactOrganisation()
		{
			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_JobNum = "J001";
			job.JH_ParentID = nctsHeader.PK;
			foreach (var type in new[]
			{
				ContactType.Consignee,
				ContactType.Consignor,
				ContactType.Declarant,
				ContactType.Warehouse,
				ContactType.TransportServices,
				ContactType.Payables,
				ContactType.Receivables,
				ContactType.LocalClient,
				ContactType.ImportBroker,
				ContactType.ExportBroker,
				ContactType.Principal
			})
			{
				AssertOrganisationContact(type);
			}
		}

		void AssertOrganisationContact(ContactType contactType)
		{
			using (Globals.SetIsUserInteractiveForTest(true))
			{
				var org = Factory.New<OrgHeader>();
				org.OH_FullName = "Some Enquiry Org";
				org.MainAddress.OA_Address1 = "1 Street";
				org.OH_RL_NKClosestPort = "AUSYD";
				org.MainAddress.OA_Address1 = "Main Address";
				org.MainAddress.OA_Fax = "234234";
				org.MainAddress.OA_Email = "blah@blah.com";

				var org1 = Factory.New<OrgHeader>();
				org1.OH_FullName = "Some Test Org1";
				org1.MainAddress.OA_Address1 = "2 Street";
				org1.OH_RL_NKClosestPort = "GBLHR";

				var contact = org1.Contacts.AddNew();
				contact.OC_IsActive = ZBool.True;
				contact.OC_ContactName = "Unit Tester";
				contact.OC_Email = "unit.tester@cargowise.com";

				var org2 = Factory.New<OrgHeader>();
				org2.OH_FullName = "Unit Test Org2";
				org2.MainAddress.OA_Address1 = "3 Street";
				org2.OH_RL_NKClosestPort = "AUSYD";
				var orgAddress = org2.Addresses.AddNew();
				orgAddress.OA_Address1 = "ADD1";

				var emailContact = org2.Contacts.AddNew();
				emailContact.OC_IsActive = ZBool.True;
				emailContact.OC_ContactName = "Sir Ken Robinson";
				emailContact.OC_Email = "sir.ken.robinson@cargowise.com";
				emailContact.OC_Fax = "11111111";

				var emailDocument = emailContact.Documents.AddNew();
				emailDocument.OD_DocumentGroup = contactType.Code;
				emailDocument.OD_DeliverBy = Core.Constants.ContactNotifyModes.Email;
				emailDocument.OD_AttachmentType = AttachmentTypeList.Codes.Pdf;
				emailDocument.OD_FilterShipmentMode = "ALL";
				emailDocument.OD_FilterDirection = "ALL";
				emailDocument.OD_CarbonCopyRecipientsAsString = "cc1@qq.com, cc2@qq.com";
				emailDocument.OD_BlindCarbonCopyRecipientsAsString = "bcc1@qq.com, bcc2@qq.com";

				if (contactType == ContactType.Consignee)
				{
					nctsHeader.Consignee.OrganisationPK = org2.PK;
				}
				else if (contactType == ContactType.Consignor)
				{
					nctsHeader.Consignor.OrganisationPK = org2.PK;
				}
				else if (contactType == ContactType.Declarant)
				{
					nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
					nctsHeader.MovementHeader.Representative.OrganisationPK = org2.PK;
				}
				else if (contactType == ContactType.Warehouse)
				{
					nctsHeader.MovementHeader.BM_OA_WarehouseAddress = orgAddress.PK;
				}
				else if (contactType == ContactType.TransportServices || contactType == ContactType.LocalTransport || contactType == ContactType.ShippingLine)
				{
					nctsHeader.MovementHeader.Carrier.OrganisationPK = org2.PK;
				}
				else if (contactType == ContactType.Receivables || contactType == ContactType.LocalClient || contactType == ContactType.Payables)
				{
					if (contactType == ContactType.Payables)
					{
						var job = LoadOrCreateJobHeader(nctsHeader);
						job.JH_ParentTableCode = CusInBondHeaderSchema.Constants.Prefix;
						job.LocalChargesPK = org2.PK;
						job.MarkAsInactive();
					}
					if (contactType == ContactType.Receivables || contactType == ContactType.LocalClient)
					{
						var job = LoadOrCreateJobHeader(nctsHeader);
						nctsHeader.Job.LocalChargesPK = org2.PK;
					}
				}
				else if (contactType == ContactType.ImportBroker)
				{
					AssertNull(nctsHeader.Shipment);
					nctsHeader.Company.GC_OH_OrgProxy = org2.PK;
				}
				else if (contactType == ContactType.ExportBroker)
				{
					nctsHeader.BH_ParentID = Factory.New<ForwardingShipment>().PK;
					nctsHeader.Shipment.JS_OH_ExportBroker = org2.PK;
					nctsHeader.Shipment.JS_RL_NKOrigin = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
					nctsHeader.Shipment.JS_RL_NKDestination = "MYPKG";
				}
				else if (contactType == ContactType.Principal)
				{
					nctsHeader.Principal.E2_OA_Address = org2.MainAddress.PK;
				}
				Factory.Save();

				var attachmentTypes = new CodeDescriptionPairList();
				attachmentTypes.AddPair(AttachmentTypeList.Codes.Xls, AttachmentTypeList.Descriptions.Xls);
				attachmentTypes.AddPair(AttachmentTypeList.Codes.Pdf, AttachmentTypeList.Descriptions.Pdf);
				attachmentTypes.AddPair(AttachmentTypeList.Codes.Tif, AttachmentTypeList.Descriptions.Tif);

				var menuItem = new Mock<IStmMenuItem>();

				menuItem.Setup(m => m.SU_ContactType).Returns(contactType.Code);
				menuItem.Setup(m => m.SU_MenuName).Returns("Test Document");
				menuItem.Setup(m => m.SU_IsDocPack).Returns(true);
				menuItem.Setup(m => m.AttachmentTypes).Returns(attachmentTypes);

				var documentSupporter = new NctsHeaderDocumentSupporter(nctsHeader);

				var autoDelivery = new DocAutoDelivery();
				var contacts = autoDelivery.GetDeliveryContacts(menuItem.Object, documentSupporter);
				AssertEquals("There should be 1 contact.", 1, contacts.Count);
				AssertEquals("Sir Ken Robinson", contacts[0].Name);
				AssertEquals("sir.ken.robinson@cargowise.com", contacts[0].Email);
				menuItem.Verify(m => m.SU_ContactType, Times.AtLeastOnce);
				menuItem.Verify(m => m.SU_MenuName, Times.AtLeastOnce);
				menuItem.Verify(m => m.AttachmentTypes, Times.AtLeastOnce);
				contacts.DeleteAll();
			}
			JobHeader LoadOrCreateJobHeader(IJobHeaderParent parent) => new JobHeader.Loader(parent).TryLoadOrCreate();
		}

		public void TestGetJobHeaderForeignKeyLink()
		{
			AssertNull("NctsHeader has no JobHeader associated", NctsHeaderDocumentSupporter.GetJobHeaderForeignKeyLink(nctsHeader));

			var job = Factory.NewJobForTesting<JobHeader>();
			job.JH_JobNum = "J001";
			job.JH_ParentID = nctsHeader.PK;
			job.JH_ParentTableCode = CusInBondHeaderSchema.Constants.Prefix;
			Factory.Save();
			AssertEquals("NctsHeader has one JobHeader associated", job, NctsHeaderDocumentSupporter.GetJobHeaderForeignKeyLink(nctsHeader));
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS4;
		}

		NctsHeader nctsHeader;
	}
}
