using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.Base.Testing
{
	[TestsSubclassesOf(typeof(DocBaseCusContainer))]
	public abstract class DocBaseCusContainerAbstractTest<T, TWrapper> : DocumentWrapperTestCase
			where T : BaseCusContainer
			where TWrapper : DocBaseCusContainer
	{
		public void TestToString()
		{
			ContainerInternal.CO_ContainerNumber = "Container";
			AssertEquals("ToString()", ContainerInternal.CO_ContainerNumber, ContainerWrapperInternal.ToString());
		}

		#region Abstract

		protected abstract TWrapper CreateContainerWrapper(T containerInternal, BaseJobDeclaration declarationInternal);

		#endregion

		#region Cartage Advice

		public void TestEmailSubjectNumber()
		{
			ContainerInternal.CO_ContainerNumber = "C000101";
			AssertEquals("Email Subject is container number", "C000101", ContainerWrapperInternal.EmailSubjectNumber);
		}

		public void TestIsImportMessage()
		{
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForExport;
			AssertEquals("IsImportMessage", ZBool.False, ContainerWrapperInternal.IsImportMessage);
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForImport;
			AssertEquals("IsImportMessage", ZBool.True, ContainerWrapperInternal.IsImportMessage);
		}

		public void TestIsExportMessage()
		{
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForImport;
			AssertEquals("IsExportMessage", ZBool.False, ContainerWrapperInternal.IsExportMessage);
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForExport;
			AssertEquals("IsExportMessage", ZBool.True, ContainerWrapperInternal.IsExportMessage);
		}

		public void TestAvailableDate()
		{
			Assert("ContainerAvailable", ContainerWrapperInternal.AvailableDate.IsEmpty);

			ZDateTime containerAvailable = new ZDateTime(2004, 02, 02);
			ContainerInternal.FCLAvailable = containerAvailable;
			AssertEquals("ContainerAvailable", containerAvailable, ContainerWrapperInternal.AvailableDate);

			ZDateTime declarationAvailable = new ZDateTime(2004, 03, 03);
			DeclarationInternal.DocsAndCartage.JP_FCLAvailable = declarationAvailable;
			AssertEquals("ContainerAvailable", containerAvailable, ContainerWrapperInternal.AvailableDate);

			ContainerInternal.FCLAvailable = ZDateTime.Empty;
			DeclarationInternal.DocsAndCartage.JP_FCLAvailable = declarationAvailable;
			AssertEquals("ContainerAvailable", declarationAvailable, ContainerWrapperInternal.AvailableDate);
		}

		public void TestStorageCommenceDate()
		{
			Assert("ContainerStorageCommenceDate", ContainerWrapperInternal.StorageCommenceDate.IsEmpty);

			ZDateTime containerStorage = new ZDateTime(2004, 02, 02);
			ContainerInternal.ArrivalCTOStorageStartDate = containerStorage;
			AssertEquals("ContainerStorageCommenceDate", containerStorage, ContainerWrapperInternal.StorageCommenceDate);

			ZDateTime declarationStorage = new ZDateTime(2004, 03, 03);
			DeclarationInternal.DocsAndCartage.JP_FCLStorageCommences = declarationStorage;
			AssertEquals("ContainerStorageCommenceDate", containerStorage, ContainerWrapperInternal.StorageCommenceDate);

			ContainerInternal.ArrivalCTOStorageStartDate = ZDateTime.Empty;
			DeclarationInternal.DocsAndCartage.JP_FCLStorageCommences = declarationStorage;
			AssertEquals("ContainerStorageCommenceDate", declarationStorage, ContainerWrapperInternal.StorageCommenceDate);
		}

		public void TestEstimatedPickup()
		{
			Assert("ContainerEstimatedPickup", ContainerWrapperInternal.EstimatedPickup.IsEmpty);

			ZDateTime declarationEstimatedPickup = new ZDateTime(2004, 02, 02);
			DeclarationInternal.DocsAndCartage.JP_EstimatedPickup = declarationEstimatedPickup;
			AssertEquals("ContainerEstimatedPickup", declarationEstimatedPickup, ContainerWrapperInternal.EstimatedPickup);

			ZDateTime containerEstimatedPickup = new ZDateTime(2004, 03, 03);
			ContainerInternal.DepartureEstimatedPickup = containerEstimatedPickup;
			AssertEquals("ContainerEstimatedPickup", containerEstimatedPickup, ContainerWrapperInternal.EstimatedPickup);
		}

		public void TestPickupRequiredBy()
		{
			Assert("ContainerPickupRequiredBy", ContainerWrapperInternal.PickupRequiredBy.IsEmpty);

			ZDateTime declarationPickupRequiredBy = new ZDateTime(2004, 02, 02);
			DeclarationInternal.DocsAndCartage.JP_PickupRequiredBy = declarationPickupRequiredBy;
			AssertEquals("DecPickupRequiredBy", declarationPickupRequiredBy, ContainerWrapperInternal.PickupRequiredBy);

			ZDateTime containerPickupRequiredBy = new ZDateTime(2004, 03, 03);
			ContainerInternal.EmptyRequired = containerPickupRequiredBy;
			AssertEquals("ContainerPickupRequiredBy", containerPickupRequiredBy, ContainerWrapperInternal.PickupRequiredBy);
		}

		#region Addresses

		public void TestJourneyOnePickUpAddress()
		{
			AssertNull("JourneyOnePickUpAddress", ContainerWrapperInternal.JourneyOnePickUpAddress);

			SetupDeclarationForAddressTests(JobMessageTypeCodeForExport);
			AssertEquals("JourneyOnePickUpAddress", DeclarationWrapper.JourneyOnePickUpAddress.Address1, ContainerWrapperInternal.JourneyOnePickUpAddress.Address1);

			OrgAddress cp = CreateContainerParkAddress("CP");
			ContainerInternal.JobContainer.JC_OA_DepartureContainerYardAddress = cp.PK;
			AssertNotEquals("JourneyOnePickUpAddress", DeclarationWrapper.JourneyOnePickUpAddress.Address1, ContainerWrapperInternal.JourneyOnePickUpAddress.Address1);
			AssertEquals("JourneyOnePickUpAddress", cp.OA_Address1, ContainerWrapperInternal.JourneyOnePickUpAddress.Address1);

			SetupDeclarationForAddressTests(JobMessageTypeCodeForImport);
			AssertEquals("JourneyOnePickUpAddress", DeclarationWrapper.JourneyOnePickUpAddress.Address1, ContainerWrapperInternal.JourneyOnePickUpAddress.Address1);
		}

		public void TestJourneyOneDeliverToAddress()
		{
			AssertNull("JourneyOneDeliverToAddress", ContainerWrapperInternal.JourneyOneDeliverToAddress);

			SetupDeclarationForAddressTests(JobMessageTypeCodeForExport);
			AssertEquals("JourneyOneDeliverToAddress", DeclarationWrapper.JourneyOneDeliverToAddress.Address1, ContainerWrapperInternal.JourneyOneDeliverToAddress.Address1);

			SetupDeclarationForAddressTests(JobMessageTypeCodeForImport);
			AssertEquals("JourneyOneDeliverToAddress", DeclarationWrapper.JourneyOneDeliverToAddress.Address1, ContainerWrapperInternal.JourneyOneDeliverToAddress.Address1);

			SetupDeclarationForAddressTests(JobMessageTypeCodeForExport);
			DeclarationInternal.SupplierPickupAddress.E2_OA_Address = Guid.Empty;
			AssertEquals("JourneyOneDeliverToAddress fallback", DeclarationWrapper.JourneyOneDeliverToAddress.Address1, ContainerWrapperInternal.JourneyOneDeliverToAddress.Address1);

			SetupDeclarationForAddressTests(JobMessageTypeCodeForImport);
			DeclarationInternal.ImporterDeliveryAddress.E2_OA_Address = Guid.Empty;
			AssertEquals("JourneyOneDeliverToAddress fallback", DeclarationWrapper.JourneyOneDeliverToAddress.Address1, ContainerWrapperInternal.JourneyOneDeliverToAddress.Address1);
		}

		public void TestJourneyTwoPickUpAddress()
		{
			AssertNull("TestJourneyTwoPickUpAddress", ContainerWrapperInternal.JourneyTwoPickUpAddress);

			SetupDeclarationForAddressTests(JobMessageTypeCodeForExport);
			AssertEquals("TestJourneyTwoPickUpAddress", DeclarationWrapper.JourneyTwoPickUpAddress.Address1, ContainerWrapperInternal.JourneyTwoPickUpAddress.Address1);

			SetupDeclarationForAddressTests(JobMessageTypeCodeForImport);
			AssertEquals("TestJourneyTwoPickUpAddress", DeclarationWrapper.JourneyTwoPickUpAddress.Address1, ContainerWrapperInternal.JourneyTwoPickUpAddress.Address1);

			SetupDeclarationForAddressTests(JobMessageTypeCodeForExport);
			DeclarationInternal.ImporterDeliveryAddress.E2_OA_Address = Guid.Empty;
			AssertEquals("TestJourneyTwoPickUpAddress fallback", DeclarationWrapper.JourneyTwoPickUpAddress.Address1, ContainerWrapperInternal.JourneyTwoPickUpAddress.Address1);

			SetupDeclarationForAddressTests(JobMessageTypeCodeForImport);
			DeclarationInternal.SupplierPickupAddress.E2_OA_Address = Guid.Empty;
			AssertEquals("TestJourneyTwoPickUpAddress fallback", DeclarationWrapper.JourneyTwoPickUpAddress.Address1, ContainerWrapperInternal.JourneyTwoPickUpAddress.Address1);
		}

		public void TestJourneyTwoDeliverToAddress()
		{
			AssertNull("JourneyTwoDeliverToAddress", ContainerWrapperInternal.JourneyTwoDeliverToAddress);

			SetupDeclarationForAddressTests(JobMessageTypeCodeForExport);
			AssertEquals("JourneyTwoDeliverToAddress", DeclarationWrapper.JourneyTwoDeliverToAddress.Address1, ContainerWrapperInternal.JourneyTwoDeliverToAddress.Address1);

			SetupDeclarationForAddressTests(JobMessageTypeCodeForImport);
			AssertEquals("JourneyTwoDeliverToAddress", DeclarationWrapper.JourneyTwoDeliverToAddress.Address1, ContainerWrapperInternal.JourneyTwoDeliverToAddress.Address1);

			OrgAddress cp = CreateContainerParkAddress("CP");
			ContainerInternal.JobContainer.JC_OA_ArrivalContainerYardAddress = cp.PK;
			AssertNotEquals("JourneyTwoDeliverToAddress", DeclarationWrapper.JourneyTwoDeliverToAddress.Address1, ContainerWrapperInternal.JourneyTwoDeliverToAddress.Address1);
			AssertEquals("JourneyTwoDeliverToAddress", cp.OA_Address1, ContainerWrapperInternal.JourneyTwoDeliverToAddress.Address1);
		}

		#endregion

		#region Contacts

		public void TestJourneyOnePickUpContactDetails()
		{
			AssertEquals("JourneyOnePickUpContactName", ZString.Empty, ContainerWrapperInternal.JourneyOnePickUpContactName);
			AssertEquals("JourneyOnePickUpContactPhone", ZString.Empty, ContainerWrapperInternal.JourneyOnePickUpContactPhone);

			SetupDeclarationForAddressTests(JobMessageTypeCodeForExport);
			AssertEquals("JourneyOnePickUpContactName", "ContainerParkName", ContainerWrapperInternal.JourneyOnePickUpContactName);
			AssertEquals("JourneyOnePickUpContactPhone", "ContainerParkPhone", ContainerWrapperInternal.JourneyOnePickUpContactPhone);

			OrgAddress cp = CreateContainerParkAddress("CP");
			ContainerInternal.JobContainer.JC_OA_DepartureContainerYardAddress = cp.PK;
			AssertEquals("JourneyOnePickUpContactName", "CPName", ContainerWrapperInternal.JourneyOnePickUpContactName);
			AssertEquals("JourneyOnePickUpContactPhone", "CPPhone", ContainerWrapperInternal.JourneyOnePickUpContactPhone);

			SetupDeclarationForAddressTests(JobMessageTypeCodeForImport);
			AssertEquals("JourneyOnePickUpContactName", "CTOName", ContainerWrapperInternal.JourneyOnePickUpContactName);
			AssertEquals("JourneyOnePickUpContactPhone", "CTOPhone", ContainerWrapperInternal.JourneyOnePickUpContactPhone);
		}

		public void TestJourneyOneDeliverToContactDetails()
		{
			AssertEquals("JourneyOneDeliverToContactName", ZString.Empty, ContainerWrapperInternal.JourneyOneDeliverToContactName);
			AssertEquals("JourneyOneDeliverToContactPhone", ZString.Empty, ContainerWrapperInternal.JourneyOneDeliverToContactPhone);

			SetupDeclarationForAddressTests(JobMessageTypeCodeForExport);
			AssertEquals("JourneyOneDeliverToContactName", "PickupName", ContainerWrapperInternal.JourneyOneDeliverToContactName);
			AssertEquals("JourneyOneDeliverToContactPhone", "PickupPhone", ContainerWrapperInternal.JourneyOneDeliverToContactPhone);

			SetupDeclarationForAddressTests(JobMessageTypeCodeForImport);
			AssertEquals("JourneyOneDeliverToContactName", "DeliveryName", ContainerWrapperInternal.JourneyOneDeliverToContactName);
			AssertEquals("JourneyOneDeliverToContactPhone", "DeliveryPhone", ContainerWrapperInternal.JourneyOneDeliverToContactPhone);

			SetupDeclarationForAddressTests(JobMessageTypeCodeForExport);
			DeclarationInternal.SupplierPickupAddress.E2_OA_Address = Guid.Empty;
			AssertEquals("JourneyOneDeliverToContactName fallback", "ConsignorName", ContainerWrapperInternal.JourneyOneDeliverToContactName);
			AssertEquals("JourneyOneDeliverToContactPhone fallback", "ConsignorPhone", ContainerWrapperInternal.JourneyOneDeliverToContactPhone);

			SetupDeclarationForAddressTests(JobMessageTypeCodeForImport);
			DeclarationInternal.ImporterDeliveryAddress.E2_OA_Address = Guid.Empty;
			AssertEquals("JourneyOneDeliverToContactName fallback", "ConsigneeName", ContainerWrapperInternal.JourneyOneDeliverToContactName);
			AssertEquals("JourneyOneDeliverToContactPhone fallback", "ConsigneePhone", ContainerWrapperInternal.JourneyOneDeliverToContactPhone);
		}

		public void TestJourneyTwoPickUpContactDetails()
		{
			AssertEquals("JourneyTwoPickUpContactName", ZString.Empty, ContainerWrapperInternal.JourneyTwoPickUpContactName);
			AssertEquals("JourneyTwoPickUpContactPhone", ZString.Empty, ContainerWrapperInternal.JourneyTwoPickUpContactPhone);

			SetupDeclarationForAddressTests(JobMessageTypeCodeForExport);
			AssertEquals("JourneyTwoPickUpContactName", "PickupName", ContainerWrapperInternal.JourneyTwoPickUpContactName);
			AssertEquals("JourneyTwoPickUpContactPhone", "PickupPhone", ContainerWrapperInternal.JourneyTwoPickUpContactPhone);

			SetupDeclarationForAddressTests(JobMessageTypeCodeForImport);
			AssertEquals("JourneyTwoPickUpContactName", "DeliveryName", ContainerWrapperInternal.JourneyTwoPickUpContactName);
			AssertEquals("JourneyTwoPickUpContactPhone", "DeliveryPhone", ContainerWrapperInternal.JourneyTwoPickUpContactPhone);

			SetupDeclarationForAddressTests(JobMessageTypeCodeForExport);
			DeclarationInternal.SupplierPickupAddress.E2_OA_Address = Guid.Empty;
			AssertEquals("JourneyTwoPickUpContactName fallback", "ConsignorName", ContainerWrapperInternal.JourneyTwoPickUpContactName);
			AssertEquals("JourneyTwoPickUpContactPhone fallback", "ConsignorPhone", ContainerWrapperInternal.JourneyTwoPickUpContactPhone);

			SetupDeclarationForAddressTests(JobMessageTypeCodeForImport);
			DeclarationInternal.ImporterDeliveryAddress.E2_OA_Address = Guid.Empty;
			AssertEquals("JourneyTwoPickUpContactName fallback", "ConsigneeName", ContainerWrapperInternal.JourneyTwoPickUpContactName);
			AssertEquals("JourneyTwoPickUpContactPhone fallback", "ConsigneePhone", ContainerWrapperInternal.JourneyTwoPickUpContactPhone);
		}

		public void TestJourneyTwoDeliverToContactDetails()
		{
			AssertEquals("JourneyTwoDeliverToContactName", ZString.Empty, ContainerWrapperInternal.JourneyTwoDeliverToContactName);
			AssertEquals("JourneyTwoDeliverToContactPhone", ZString.Empty, ContainerWrapperInternal.JourneyTwoDeliverToContactPhone);

			SetupDeclarationForAddressTests(JobMessageTypeCodeForExport);
			AssertEquals("JourneyTwoDeliverToContactName", "CTOName", ContainerWrapperInternal.JourneyTwoDeliverToContactName);
			AssertEquals("JourneyTwoDeliverToContactPhone", "CTOPhone", ContainerWrapperInternal.JourneyTwoDeliverToContactPhone);

			SetupDeclarationForAddressTests(JobMessageTypeCodeForImport);
			AssertEquals("JourneyTwoDeliverToContactName", "ContainerParkName", ContainerWrapperInternal.JourneyTwoDeliverToContactName);
			AssertEquals("JourneyTwoDeliverToContactPhone", "ContainerParkPhone", ContainerWrapperInternal.JourneyTwoDeliverToContactPhone);

			OrgAddress cp = CreateContainerParkAddress("CP");
			ContainerInternal.JobContainer.JC_OA_ArrivalContainerYardAddress = cp.PK;
			AssertEquals("JourneyTwoDeliverToContactName", "CPName", ContainerWrapperInternal.JourneyTwoDeliverToContactName);
			AssertEquals("JourneyTwoDeliverToContactPhone", "CPPhone", ContainerWrapperInternal.JourneyTwoDeliverToContactPhone);
		}

		#region Journey Pickup and Delivery ContactName and Phone

		#region JourneyOne

		public void TestJourneyOnePickUpContactName()
		{
			SetupDeclarationForAddressTests(JobMessageTypeCodeForExport);
			JobDocAddress journeyOnePickupDocAddress = (JobDocAddress)ContainerWrapperInternal.JourneyOnePickUpAddress.WrappedObject;
			journeyOnePickupDocAddress.E2_Contact = "DepotName";
			AssertEquals("JourneyOnePickUpContactName", "DepotName", ContainerWrapperInternal.JourneyOnePickUpContactName);
			journeyOnePickupDocAddress.E2_Contact = "CTOName";
			AssertEquals("JourneyOnePickUpContactName", "CTOName", ContainerWrapperInternal.JourneyOnePickUpContactName);

			OrgAddress cp = CreateContainerParkAddress("CP");
			ContainerInternal.JobContainer.JC_OA_DepartureContainerYardAddress = cp.PK;
			AssertEquals("JourneyOnePickUpContactName", "CPName", ContainerWrapperInternal.JourneyOnePickUpContactName);
		}

		public void TestJourneyOnePickUpContactPhone()
		{
			SetupDeclarationForAddressTests(JobMessageTypeCodeForExport);
			JobDocAddress journeyOnePickupDocAddress = (JobDocAddress)ContainerWrapperInternal.JourneyOnePickUpAddress.WrappedObject;
			journeyOnePickupDocAddress.E2_Contact = "ContainerParkName2";
			AssertEquals("JourneyOnePickUpContactPhone", "ContainerParkPhone2", ContainerWrapperInternal.JourneyOnePickUpContactPhone);
			journeyOnePickupDocAddress.E2_Contact = "ContainerParkName";
			AssertEquals("JourneyOnePickUpContactPhone", "ContainerParkPhone", ContainerWrapperInternal.JourneyOnePickUpContactPhone);

			OrgAddress cp = CreateContainerParkAddress("CP");
			ContainerInternal.JobContainer.JC_OA_DepartureContainerYardAddress = cp.PK;
			AssertEquals("JourneyOnePickUpContactPhone", "CPPhone", ContainerWrapperInternal.JourneyOnePickUpContactPhone);
		}

		public void TestJourneyOneDeliverToContactName()
		{
			SetupDeclarationForAddressTests(JobMessageTypeCodeForImport);
			ContainerWrapperInternal.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			JobDocAddress journeyOneDeliverToDocAddress = (JobDocAddress)ContainerWrapperInternal.JourneyOneDeliverToAddress.WrappedObject;
			journeyOneDeliverToDocAddress.E2_Contact = "DepotName";
			AssertEquals("JourneyOneDeliverToContactName", "DepotName", ContainerWrapperInternal.JourneyOneDeliverToContactName);
			journeyOneDeliverToDocAddress.E2_Contact = "CTOName";
			AssertEquals("JourneyOneDeliverToContactName", "CTOName", ContainerWrapperInternal.JourneyOneDeliverToContactName);
		}

		public void TestJourneyOneDeliverToContactPhone()
		{
			SetupDeclarationForAddressTests(JobMessageTypeCodeForImport);
			ContainerWrapperInternal.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			JobDocAddress journeyOneDeliverToDocAddress = (JobDocAddress)ContainerWrapperInternal.JourneyOneDeliverToAddress.WrappedObject;
			journeyOneDeliverToDocAddress.E2_Contact = "DeliveryName2";
			AssertEquals("JourneyOneDeliverToContactPhone", "DeliveryPhone2", ContainerWrapperInternal.JourneyOneDeliverToContactPhone);
			journeyOneDeliverToDocAddress.E2_Contact = "DeliveryName";
			AssertEquals("JourneyOneDeliverToContactPhone", "DeliveryPhone", ContainerWrapperInternal.JourneyOneDeliverToContactPhone);
		}
		#endregion

		#region Journey Two

		public void TestJourneyTwoPickUpContactName()
		{
			SetupDeclarationForAddressTests(JobMessageTypeCodeForExport);
			ContainerWrapperInternal.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			JobDocAddress journeyTwoPickupDocAddress = (JobDocAddress)ContainerWrapperInternal.JourneyTwoPickUpAddress.WrappedObject;
			journeyTwoPickupDocAddress.E2_Contact = "DepotName";
			AssertEquals("JourneyTwoPickUpContactName", "DepotName", ContainerWrapperInternal.JourneyTwoPickUpContactName);
			journeyTwoPickupDocAddress.E2_Contact = "CTOName";
			AssertEquals("JourneyTwoPickUpContactName", "CTOName", ContainerWrapperInternal.JourneyTwoPickUpContactName);
		}

		public void TestJourneyTwoPickUpContactPhone()
		{
			SetupDeclarationForAddressTests(JobMessageTypeCodeForExport);
			ContainerWrapperInternal.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			JobDocAddress journeyTwoPickupDocAddress = (JobDocAddress)ContainerWrapperInternal.JourneyTwoPickUpAddress.WrappedObject;
			journeyTwoPickupDocAddress.E2_Contact = "PickupName2";
			AssertEquals("JourneyTwoPickUpContactPhone", "PickupPhone2", ContainerWrapperInternal.JourneyTwoPickUpContactPhone);
			journeyTwoPickupDocAddress.E2_Contact = "PickupName";
			AssertEquals("JourneyTwoPickUpContactPhone", "PickupPhone", ContainerWrapperInternal.JourneyTwoPickUpContactPhone);
		}

		public void TestJourneyTwoDeliverToContactName()
		{
			SetupDeclarationForAddressTests(JobMessageTypeCodeForImport);
			JobDocAddress journeyTwoDeliverToDocAddress = (JobDocAddress)ContainerWrapperInternal.JourneyTwoDeliverToAddress.WrappedObject;
			journeyTwoDeliverToDocAddress.E2_Contact = "DepotName";
			AssertEquals("JourneyTwoDeliverToContactName", "DepotName", ContainerWrapperInternal.JourneyTwoDeliverToContactName);
			journeyTwoDeliverToDocAddress.E2_Contact = "CTOName";
			AssertEquals("JourneyTwoDeliverToContactName", "CTOName", ContainerWrapperInternal.JourneyTwoDeliverToContactName);

			OrgAddress cp = CreateContainerParkAddress("CP");
			ContainerInternal.JobContainer.JC_OA_ArrivalContainerYardAddress = cp.PK;
			AssertEquals("JourneyTwoDeliverToContactName", "CPName", ContainerWrapperInternal.JourneyTwoDeliverToContactName);
		}

		public void TestJourneyTwoDeliverToContactPhone()
		{
			SetupDeclarationForAddressTests(JobMessageTypeCodeForImport);
			JobDocAddress journeyTwoDeliverToDocAddress = (JobDocAddress)ContainerWrapperInternal.JourneyTwoDeliverToAddress.WrappedObject;
			journeyTwoDeliverToDocAddress.E2_Contact = "ContainerParkName2";
			AssertEquals("JourneyTwoDeliverToContactPhone", "ContainerParkPhone2", ContainerWrapperInternal.JourneyTwoDeliverToContactPhone);
			journeyTwoDeliverToDocAddress.E2_Contact = "ContainerParkName";
			AssertEquals("JourneyTwoDeliverToContactPhone", "ContainerParkPhone", ContainerWrapperInternal.JourneyTwoDeliverToContactPhone);

			OrgAddress cp = CreateContainerParkAddress("CP");
			ContainerInternal.JobContainer.JC_OA_ArrivalContainerYardAddress = cp.PK;
			AssertEquals("JourneyTwoDeliverToContactPhone", "CPPhone", ContainerWrapperInternal.JourneyTwoDeliverToContactPhone);
		}

		#endregion

		#endregion

		#endregion

		void SetupDeclarationForAddressTests(string messageType)
		{
			DeclarationInternal.JE_MessageType = messageType;
			DeclarationInternal.JE_TransportMode = DeclarationInternal.TransportModeSeaCodeForTesting;
			DeclarationInternal.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			ContainerInternal.CO_FCL_LCL_AIR = "FCL";

			var depotOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var depotContact = depotOrg.Contacts.AddNew();
			var depotAddress = depotOrg.Addresses.AddNew();
			depotContact.OC_Phone = "DepotPhone";
			depotContact.OC_ContactName = "DepotName";
			depotAddress.OA_Address1 = "abcd";
			depotAddress.OA_Phone = "123";
			depotContact.Documents.AddNew().OD_DocumentGroup = ContactType.LocalTransport.Code;
			var depotContact2 = depotOrg.Contacts.AddNew();
			depotContact2.OC_Phone = "DepotPhone2";
			depotContact2.OC_ContactName = "DepotName2";

			var ctoOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var ctoContact = ctoOrg.Contacts.AddNew();
			var ctoAddress = ctoOrg.Addresses.AddNew();
			ctoContact.OC_Phone = "CTOPhone";
			ctoContact.OC_ContactName = "CTOName";
			ctoAddress.OA_Address1 = "efgh";
			ctoAddress.OA_Phone = "234";
			ctoContact.Documents.AddNew().OD_DocumentGroup = ContactType.LocalTransport.Code;
			var cTOContact2 = ctoOrg.Contacts.AddNew();
			cTOContact2.OC_Phone = "CTOPhone2";
			cTOContact2.OC_ContactName = "CTOName2";

			var deliveryOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var deliveryContact = deliveryOrg.Contacts.AddNew();
			var deliveryAddress = deliveryOrg.Addresses.AddNew();
			deliveryContact.OC_Phone = "DeliveryPhone";
			deliveryContact.OC_ContactName = "DeliveryName";
			deliveryAddress.OA_Address1 = "ijkl";
			deliveryAddress.OA_Phone = "345";
			deliveryContact.Documents.AddNew().OD_DocumentGroup = ContactType.LocalTransport.Code;
			var deliveryContact2 = deliveryOrg.Contacts.AddNew();
			deliveryContact2.OC_Phone = "DeliveryPhone2";
			deliveryContact2.OC_ContactName = "DeliveryName2";

			var pickupOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var pickupContact = pickupOrg.Contacts.AddNew();
			var pickupAddress = pickupOrg.Addresses.AddNew();
			pickupContact.OC_Phone = "PickupPhone";
			pickupContact.OC_ContactName = "PickupName";
			pickupAddress.OA_Address1 = "mnop";
			pickupAddress.OA_Phone = "456";
			pickupContact.Documents.AddNew().OD_DocumentGroup = ContactType.LocalTransport.Code;
			var pickupContact2 = pickupOrg.Contacts.AddNew();
			pickupContact2.OC_Phone = "PickupPhone2";
			pickupContact2.OC_ContactName = "PickupName2";

			var containerParkOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var containerParkContact = containerParkOrg.Contacts.AddNew();
			var containerParkAddress = containerParkOrg.Addresses.AddNew();
			containerParkContact.OC_Phone = "ContainerParkPhone";
			containerParkContact.OC_ContactName = "ContainerParkName";
			containerParkAddress.OA_Address1 = "ContPark";
			containerParkAddress.OA_Phone = "0123";
			containerParkContact.Documents.AddNew().OD_DocumentGroup = ContactType.LocalTransport.Code;
			var containerParkContact2 = containerParkOrg.Contacts.AddNew();
			containerParkContact2.OC_Phone = "ContainerParkPhone2";
			containerParkContact2.OC_ContactName = "ContainerParkName2";

			var consignee = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var consigneeContact = consignee.Contacts.AddNew();
			var consigneeAddress = consignee.Addresses.AddNew();
			consigneeContact.OC_Phone = "ConsigneePhone";
			consigneeContact.OC_ContactName = "ConsigneeName";
			consigneeAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Delivery);
			consigneeAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Delivery);
			consigneeAddress.OA_Address1 = "qrst";
			consigneeAddress.OA_Phone = "567";
			consigneeContact.Documents.AddNew().OD_DocumentGroup = ContactType.LocalTransport.Code;
			var consigneeContact2 = consignee.Contacts.AddNew();
			consigneeContact2.OC_Phone = "ConsigneePhone2";
			consigneeContact2.OC_ContactName = "ConsigneeName2";

			var consignor = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			var consignorContact = consignor.Contacts.AddNew();
			var consignorAddress = consignor.Addresses.AddNew();
			consignorContact.OC_Phone = "ConsignorPhone";
			consignorContact.OC_ContactName = "ConsignorName";
			consignorAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			consignorAddress.AddressCapability.SetIsMainAddress(OrgConstants.AddressType.Pickup);
			consignorAddress.OA_Address1 = "uvwx";
			consignorAddress.OA_Phone = "678";
			consignorContact.Documents.AddNew().OD_DocumentGroup = ContactType.LocalTransport.Code;
			var consignorContact2 = consignor.Contacts.AddNew();
			consignorContact2.OC_Phone = "ConsignorPhone2";
			consignorContact2.OC_ContactName = "ConsignorName2";

			DeclarationInternal.DepotDocAddress.E2_OA_Address = depotAddress.PK;
			DeclarationInternal.ContainerTerminalOperatorDocAddress.E2_OA_Address = ctoAddress.PK;
			DeclarationInternal.ImporterDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
			DeclarationInternal.SupplierPickupAddress.E2_OA_Address = pickupAddress.PK;
			DeclarationInternal.JE_OH_Importer = consignee.PK;
			DeclarationInternal.JE_OH_Supplier = consignor.PK;
			DeclarationInternal.ContainerYardDocAddress.E2_OA_Address = containerParkAddress.PK;

			CreateWrappersForTest();
		}

		OrgAddress CreateContainerParkAddress(string prefix)
		{
			OrgHeader containerParkOrg = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			OrgContact containerParkContact = containerParkOrg.Contacts.AddNew();
			OrgAddress containerParkAddress = containerParkOrg.Addresses.AddNew();
			containerParkContact.OC_Phone = prefix + "Phone";
			containerParkContact.OC_ContactName = prefix + "Name";
			containerParkAddress.OA_Address1 = prefix + "addy";
			containerParkAddress.OA_Phone = "0123";
			containerParkContact.Documents.AddNew().OD_DocumentGroup = ContactType.LocalTransport.Code;
			OrgContact containerParkContact2 = containerParkOrg.Contacts.AddNew();
			containerParkContact2.OC_Phone = prefix + "Phone2";
			containerParkContact2.OC_ContactName = prefix + "Name2";

			return containerParkAddress;
		}

		public void TestAddressesWithWareHousing()
		{
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForExport;
			ContainerInternal.CO_FCL_LCL_AIR = "FCL";
			AssertEquals("AddressesWithWareHousing should be", 0, ContainerWrapperInternal.AddressesWithWareHousing.Count);

			var j1PickUpAddress = Factory.NewWithValidTestData<OrgAddress>();
			var j1J2ExporterAddress = Factory.NewWithValidTestData<OrgAddress>();
			var j2DeliverToAddress = Factory.NewWithValidTestData<OrgAddress>();

			DeclarationInternal.ContainerYardDocAddress.E2_OA_Address = j1PickUpAddress.PK;
			DeclarationInternal.SupplierPickupAddress.E2_OA_Address = j1J2ExporterAddress.PK;
			DeclarationInternal.ContainerTerminalOperatorDocAddress.E2_OA_Address = j2DeliverToAddress.PK;

			AssertEquals("AddressesWithWareHousing should be", 0, ContainerWrapperInternal.AddressesWithWareHousing.Count);

			j1PickUpAddress.OA_ForkLift = ZBool.True;
			j1J2ExporterAddress.OA_DockLeveler = ZBool.True;
			j2DeliverToAddress.OA_DockLeveler = ZBool.True;
			AssertEquals("AddressesWithWareHousing should have 3, 1 is repeated", 3, ContainerWrapperInternal.AddressesWithWareHousing.Count);

			System.Collections.Hashtable hashtable = new System.Collections.Hashtable();

			hashtable.Add(j1PickUpAddress.PK, j1PickUpAddress);
			hashtable.Add(j1J2ExporterAddress.PK, j1J2ExporterAddress);
			hashtable.Add(j2DeliverToAddress.PK, j2DeliverToAddress);
			foreach (DocDocAddress dAddress in ContainerWrapperInternal.AddressesWithWareHousing)
			{
				Assert("is in col", hashtable.ContainsKey(((JobDocAddress)dAddress.WrappedObject).Address.PK));
			}
		}

		public void TestDropOffEmpty()
		{
			AssertEquals("DropOffEmpty", ZDateTime.Empty, ContainerWrapperInternal.DropOffEmpty);

			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForExport;
			AssertEquals("DropOffEmpty", ContainerInternal.EmptyRequired, ContainerWrapperInternal.DropOffEmpty);

			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForImport;
			AssertEquals("DropOffEmpty", ZDateTime.Empty, ContainerWrapperInternal.DropOffEmpty);
		}

		public void TestReturnEmpty()
		{
			AssertEquals("ReturnEmpty", ZDateTime.Empty, ContainerWrapperInternal.ReturnEmpty);

			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForImport;
			ContainerInternal.EmptyReturnedBy = new ZDateTime(2004, 04, 04);
			AssertEquals("ReturnEmpty", ContainerInternal.EmptyReturnedBy, ContainerWrapperInternal.ReturnEmpty);

			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForExport;
			ContainerInternal.EmptyReturnedBy = new ZDateTime(2004, 03, 03);
			AssertEquals("ReturnEmpty", ZDateTime.Empty, ContainerWrapperInternal.ReturnEmpty);
		}

		public void TestArrivalCartageAdvised()
		{
			ZDateTime arrivalCartageAdvised = new ZDateTime(2004, 02, 02);
			ContainerInternal.ArrivalCartageAdvised = arrivalCartageAdvised;
			AssertEquals("ArrivalCartageAdvised", arrivalCartageAdvised, ContainerWrapperInternal.ArrivalCartageAdvised);
		}

		public void TestArrivalCartageComplete()
		{
			ZDateTime arrivalCartageComplete = new ZDateTime(2004, 02, 02);
			ContainerInternal.ArrivalCartageComplete = arrivalCartageComplete;
			AssertEquals("ArrivalCartageComplete", arrivalCartageComplete, ContainerWrapperInternal.ArrivalCartageComplete);
		}

		public void TestContainerAvailable()
		{
			ZDateTime containerAvailable = new ZDateTime(2004, 02, 02);
			ContainerInternal.FCLAvailable = containerAvailable;
			AssertEquals("ContainerAvailable", containerAvailable, ContainerWrapperInternal.ContainerAvailable);
		}

		public void TestDepartureCartageAdvised()
		{
			ZDateTime departureCartageAdvised = new ZDateTime(2004, 02, 02);
			ContainerInternal.DepartureCartageAdvised = departureCartageAdvised;
			AssertEquals("DepartureCartageAdvised", departureCartageAdvised, ContainerWrapperInternal.DepartureCartageAdvised);
		}

		public void TestDepartureCartageComplete()
		{
			ZDateTime departureCartageComplete = new ZDateTime(2004, 02, 02);
			ContainerInternal.DepartureCartageComplete = departureCartageComplete;
			AssertEquals("DepartureCartageComplete", departureCartageComplete, ContainerWrapperInternal.DepartureCartageComplete);
		}

		public void TestContainerParkEmptyReturnGateIn()
		{
			var containerParkEmptyReturnGateIn = new ZDateTime(2004, 02, 02);
			ContainerInternal.ContainerYardEmptyReturnGateIn = containerParkEmptyReturnGateIn;
			AssertEquals("ContainerParkEmptyReturnGateIn", containerParkEmptyReturnGateIn, ContainerWrapperInternal.ContainerParkEmptyReturnGateIn);
		}

		public void TestEmptyRequired()
		{
			ZDateTime emptyRequired = new ZDateTime(2004, 02, 02);
			ContainerInternal.EmptyRequired = emptyRequired;
			AssertEquals("EmptyRequired", emptyRequired, ContainerWrapperInternal.EmptyRequired);
		}

		public void TestEmptyReturnedBy()
		{
			ZDateTime emptyReturnedBy = new ZDateTime(2004, 02, 02);
			ContainerInternal.EmptyReturnedBy = emptyReturnedBy;
			AssertEquals("EmptyReturnedBy", emptyReturnedBy, ContainerWrapperInternal.EmptyReturnedBy);
		}

		public void TestEstimatedDelivery()
		{
			ZDateTime estimatedDelivery = new ZDateTime(2004, 02, 02);
			ContainerInternal.ArrivalEstimatedDelivery = estimatedDelivery;
			AssertEquals("EstimatedDelivery", estimatedDelivery, ContainerWrapperInternal.EstimatedDelivery);
		}

		public void TestSlotArrivalDetails()
		{
			AssertEquals("Empty", ZString.Empty, ContainerWrapperInternal.SlotArrivalDetails);
			ContainerInternal.ArrivalSlotReference = "ARR-REF";
			AssertEquals("SlotArrivalDetails Should be", "ARR-REF", ContainerWrapperInternal.SlotArrivalDetails);

			ZDateTime arrivalTime = ZDateTime.Now.AddDays(1);
			ContainerInternal.ArrivalSlotDateTime = arrivalTime;

			AssertEquals("Incorrect Container Slot Details found for arrival.", "ARR-REF / " + arrivalTime.ToShortDateString(), ContainerWrapperInternal.SlotArrivalDetails);
		}

		public void TestSlotDepartureDetails()
		{
			AssertEquals("Empty", ZString.Empty, ContainerWrapperInternal.SlotDepartureDetails);
			ContainerInternal.DepartureSlotReference = "DEP-REF";
			AssertEquals("SlotDepartureDetails Should be", "DEP-REF", ContainerWrapperInternal.SlotDepartureDetails);

			ZDateTime departureTime = ZDateTime.Now.AddDays(1);
			ContainerInternal.DepartureSlotDateTime = departureTime;

			AssertEquals("Incorrect Container Slot Details found for Departure.", "DEP-REF / " + departureTime.ToShortDateString(), ContainerWrapperInternal.SlotDepartureDetails);
		}

		public void TestIDocCartageAdviceDates()
		{
			DeclarationInternal.JE_DeliveryOrPickupRequiredBy = new ZDateTime(2011, 1, 1);
			DeclarationInternal.DocsAndCartage.JP_FCLAvailable = new ZDateTime(2011, 1, 5);
			DeclarationInternal.JE_EstimatedDeliveryOrPickup = new ZDateTime(2011, 2, 1);
			DeclarationInternal.DocsAndCartage.JP_FCLStorageCommences = new ZDateTime(2011, 2, 5);

			DeclarationInternal.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			var container = DeclarationInternal.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;

			AssertEquals("CartageAvailableDate", new ZDateTime(2011, 1, 5), ContainerWrapperInternal.CartageAvailableDate);
			AssertEquals("CartageStorageCommenceDate", new ZDateTime(2011, 2, 5), ContainerWrapperInternal.CartageStorageCommenceDate);
		}

		public void TestCartageCutOffandReceivalDates()
		{
			Transport transport1 = DeclarationInternal.Transports.AddNew();
			transport1.JW_ETD = new ZDateTime(2011, 2, 1);
			transport1.JW_ETA = new ZDateTime(2011, 2, 15);
			transport1.JW_TerminalCutOff = new ZDateTime(2011, 1, 18);
			transport1.JW_DepotCutOff = new ZDateTime(2011, 1, 20);
			transport1.JW_TerminalReceivalCommences = new ZDateTime(2011, 1, 22);
			transport1.JW_DepotReceivalCommences = new ZDateTime(2011, 1, 25);

			Transport transport2 = DeclarationInternal.Transports.AddNew();
			transport2.JW_ETD = new ZDateTime(2012, 1, 1);
			transport2.JW_ETA = new ZDateTime(2012, 1, 15);
			transport2.JW_TerminalCutOff = new ZDateTime(2012, 1, 30);
			transport2.JW_DepotCutOff = new ZDateTime(2012, 1, 30);
			transport2.JW_TerminalReceivalCommences = new ZDateTime(2012, 1, 22);
			transport2.JW_DepotReceivalCommences = new ZDateTime(2012, 1, 25);

			DeclarationInternal.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			var container = DeclarationInternal.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			AssertEquals("CartageReceivalDate", new ZDateTime(2011, 1, 22), DeclarationWrapper.CartageReceivalDate);
			AssertEquals("CartageCutOffDate", new ZDateTime(2011, 1, 18), DeclarationWrapper.CartageCutOffDate);

			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.LCL;
			AssertEquals("CartageReceivalDate", new ZDateTime(2011, 1, 25), DeclarationWrapper.CartageReceivalDate);
			AssertEquals("CartageCutOffDate", new ZDateTime(2011, 1, 20), DeclarationWrapper.CartageCutOffDate);
		}

		#endregion

		#region ZString Fields

		public void TestContainerImportDORelease()
		{
			ContainerInternal.JobContainer.JC_ContainerImportDORelease = "DOREL";
			AssertEquals("ContainerImportDORelease", "DOREL", ContainerWrapperInternal.ContainerImportDORelease);
		}

		public void TestContainerNumber()
		{
			ContainerInternal.CO_ContainerNumber = "Container";
			AssertEquals("ContainerNumber", ContainerInternal.CO_ContainerNumber, ContainerWrapperInternal.ContainerNumber);
		}

		public void TestContainerSize()
		{
			ContainerInternal.CO_ContainerSize = "CC";
			AssertEquals("ContainerSize", ContainerInternal.CO_ContainerSize, ContainerWrapperInternal.ContainerSize);
		}

		public void TestContainerUQ()
		{
			ContainerInternal.CO_ContainerUQ = "UQ";
			AssertEquals("ContainerUQ", ContainerInternal.CO_ContainerUQ, ContainerWrapperInternal.ContainerUQ);
		}

		public void TestCustomAttrib1()
		{
			ContainerInternal.CO_CustomAttrib1 = "CustomAttrib1";
			AssertEquals("CustomAttrib1", ContainerInternal.CO_CustomAttrib1, ContainerWrapperInternal.CustomAttrib1);
		}

		public void TestType()
		{
			ContainerInternal.CO_FCL_LCL_AIR = "FCL";
			AssertEquals("FCL_LCL_AIR", ContainerInternal.CO_FCL_LCL_AIR, ContainerWrapperInternal.Type);
		}

		public void TestContainerMode()
		{
			ContainerInternal.CO_FCL_LCL_AIR = "AIR";
			AssertEquals("FCL_LCL_AIR", "AIR", ContainerWrapperInternal.ContainerMode);
		}

		public void TestSeal()
		{
			ContainerInternal.CO_Seal = "Seal";
			AssertEquals("Seal", ContainerInternal.CO_Seal, ContainerWrapperInternal.SealNumber);
		}

		public void TestCommodityCode()
		{
			ContainerInternal.RH_NKContainerCommodityCode = "";
			AssertEquals("Commodity", "", ContainerWrapperInternal.CommodityCode);

			var commCode = Factory.New<RefCommodityCode>();
			commCode.RH_Code = "TEST";
			ContainerInternal.RH_NKContainerCommodityCode = commCode.RH_Code;
			AssertEquals("Commodity is ", "TEST", ContainerWrapperInternal.CommodityCode);
		}

		public void TestFormattedWeight()
		{
			ContainerInternal.CO_Weight = 0.0M;
			AssertEquals("Formatted Weight", "", ContainerWrapperInternal.FormattedWeight);

			ContainerInternal.CO_Weight = 111.2M;
			AssertEquals("Formatted Weight", "111.20", ContainerWrapperInternal.FormattedWeight);
		}
		#endregion

		#region ZDateTime Fields

		public void TestCustomDate1()
		{
			ZDateTime customDate1 = new ZDateTime(2004, 04, 04);
			ContainerInternal.CO_CustomDate1 = customDate1;
			AssertEquals("CustomDate1", customDate1, ContainerWrapperInternal.CustomDate1);
		}

		#endregion

		#region ZDecimal Fields

		public void TestCustomDecimal1()
		{
			ContainerInternal.CO_CustomDecimal1 = 12.34M;
			AssertEquals("CustomDecimal1", ContainerInternal.CO_CustomDecimal1, ContainerWrapperInternal.CustomDecimal1);
		}

		public void TestWeight()
		{
			ContainerInternal.CO_Weight = 12.34M;
			AssertEquals("Weight", ContainerInternal.CO_Weight, ContainerWrapperInternal.TotalAllocatedJobWeight);
		}

		#endregion

		#region ZBool Fields

		public void TestCustomFlag1()
		{
			ContainerInternal.CO_CustomFlag1 = ZBool.False;
			Assert("!CustomFlag1", !ContainerWrapperInternal.CustomFlag1);

			ContainerInternal.CO_CustomFlag1 = ZBool.True;
			Assert("CustomFlag1", ContainerWrapperInternal.CustomFlag1);
		}

		public void TestPrintAsContainer()
		{
			AssertEquals("Print as container", ZBool.True, ContainerWrapperInternal.PrintAsContainers);
		}

		#endregion

		#region ZShort Fields

		public void TestContainerCount()
		{
			AssertEquals("Container count should be 1", (ZShort)1, ContainerWrapperInternal.ContainerCount);
		}

		#endregion

		#region Wrapper Fields

		public void TestRefContainer()
		{
			AssertNull("RefContainer", ContainerWrapperInternal.Container);

			ContainerInternal.CO_RC = Factory.LoadTop1(typeof(RefContainer), new ZQuery()).PK;
			AssertNotNull("RefContainer", ContainerWrapperInternal.Container);
			AssertEquals("RefContainer is of type DocRefContainer", typeof(DocRefContainer), ContainerWrapperInternal.Container.GetType());
		}

		#endregion

		#region IDocContainer

		#region Headings

		public void TestJourneyOnePickUpHeading()
		{
			//No Dates
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForExport;
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForExport;
			AssertEquals("JourneyOnePickUpHeading: Export, No date", "PICKUP EMPTY", ContainerWrapperInternal.JourneyOnePickUpHeading);
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForImport;
			AssertEquals("JourneyOnePickUpHeading: Import, No date", "PICKUP FULL", ContainerWrapperInternal.JourneyOnePickUpHeading);

			//+ Dates
			ContainerInternal.JobContainer.JC_ReleaseNum = "12345";
			ContainerInternal.ArrivalSlotReference = "67890";
			ContainerInternal.ArrivalSlotDateTime = ZDateTime.Now;
			ContainerInternal.DepartureEstimatedPickup = ZDateTime.Now.AddDays(1);
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForExport;
			AssertEquals("JourneyOnePickUpHeading: LCL Export + Date", "PICKUP EMPTY REF. 12345 DATE " + ContainerInternal.DepartureEstimatedPickup.ToLongTimeString(), ContainerWrapperInternal.JourneyOnePickUpHeading);
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForImport;
			AssertEquals("JourneyOnePickUpHeading: LCL Import + Date", "PICKUP FULL SLOT REF. 67890 / " + ContainerInternal.ArrivalSlotDateTime.ToLongTimeString(), ContainerWrapperInternal.JourneyOnePickUpHeading);
		}

		public void TestJourneyOneDeliverToHeading()
		{
			//No Dates
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForExport;
			AssertEquals("JourneyOneDeliveryToHeading: Export, No date", "DELIVER TO EMPTY", ContainerWrapperInternal.JourneyOneDeliverToHeading);
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForImport;
			AssertEquals("JourneyOneDeliveryToHeading: Import, No date", "DELIVER TO FULL", ContainerWrapperInternal.JourneyOneDeliverToHeading);

			//+ Dates
			ContainerInternal.EmptyRequired = ZDateTime.Now;
			ContainerInternal.ArrivalEstimatedDelivery = ZDateTime.Now.AddDays(1);
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForExport;
			AssertEquals("JourneyOneDeliveryToHeading: LCL Export + Date", "DELIVER TO EMPTY DATE " + ContainerInternal.EmptyRequired.ToLongTimeString(), ContainerWrapperInternal.JourneyOneDeliverToHeading);
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForImport;
			AssertEquals("JourneyOneDeliveryToHeading: LCL Import + Date", "DELIVER TO FULL DATE " + ContainerInternal.ArrivalEstimatedDelivery.ToLongTimeString(), ContainerWrapperInternal.JourneyOneDeliverToHeading);
		}

		public void TestJourneyTwoPickUpHeading()
		{
			//No Dates
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForExport;
			AssertEquals("JourneyTwoPickUpHeading: Export, No date", "PICKUP FULL", ContainerWrapperInternal.JourneyTwoPickUpHeading);
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForImport;
			AssertEquals("JourneyTwoPickUpHeading: Import, No date", "PICKUP EMPTY", ContainerWrapperInternal.JourneyTwoPickUpHeading);

			//+ Dates
			ContainerInternal.DepartureEstimatedPickup = ZDateTime.Now;
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForExport;
			AssertEquals("JourneyTwoPickUpHeading: Export + Date", "PICKUP FULL DATE " + ContainerInternal.DepartureEstimatedPickup.ToLongTimeString(), ContainerWrapperInternal.JourneyTwoPickUpHeading);
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForImport;
			AssertEquals("JourneyTwoPickUpHeading: Import + Date", "PICKUP EMPTY", ContainerWrapperInternal.JourneyTwoPickUpHeading);
		}

		public void TestJourneyTwoDeliverToHeading()
		{
			//No Dates
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForExport;
			AssertEquals("JourneyTwoDeliverToHeading: Export, No date", "DELIVER TO FULL", ContainerWrapperInternal.JourneyTwoDeliverToHeading);
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForImport;
			AssertEquals("JourneyTwoDeliverToHeading: Import, No date", "DELIVER TO EMPTY", ContainerWrapperInternal.JourneyTwoDeliverToHeading);

			//+ Dates
			ContainerInternal.EmptyReturnedBy = ZDateTime.Now;
			ContainerInternal.DepartureSlotReference = "67890";
			ContainerInternal.DepartureSlotDateTime = ZDateTime.Now.AddDays(1);
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForExport;
			AssertEquals("JourneyTwoDeliverToHeading: LCL Export + Date", "DELIVER TO FULL SLOT REF. 67890 / " + ContainerInternal.DepartureSlotDateTime.ToLongTimeString(), ContainerWrapperInternal.JourneyTwoDeliverToHeading);
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForImport;
			AssertEquals("JourneyTwoDeliverToHeading: LCL Import + Date", "DELIVER TO EMPTY DATE " + ContainerInternal.EmptyReturnedBy.ToLongTimeString(), ContainerWrapperInternal.JourneyTwoDeliverToHeading);
		}

		#endregion

		public void TestIsEmptyLeg()
		{
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForExport;
			AssertEquals("Journey One, Export: Should be True", true, ContainerWrapperInternal.IsEmptyLeg(true));
			AssertEquals("Journey Two, Export: Should be False", false, ContainerWrapperInternal.IsEmptyLeg(false));

			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForImport;
			AssertEquals("Journey One, Import: Should be False", false, ContainerWrapperInternal.IsEmptyLeg(true));
			AssertEquals("Journey Two, Import: Should be True", true, ContainerWrapperInternal.IsEmptyLeg(false));
		}

		public void TestIsFullLeg()
		{
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForExport;
			AssertEquals("Journey One, Export: Should be True", false, ContainerWrapperInternal.IsFullLeg(true));
			AssertEquals("Journey Two, Export: Should be False", true, ContainerWrapperInternal.IsFullLeg(false));

			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForImport;
			AssertEquals("Journey One, Import: Should be False", true, ContainerWrapperInternal.IsFullLeg(true));
			AssertEquals("Journey Two, Import: Should be True", false, ContainerWrapperInternal.IsFullLeg(false));
		}

		public void TestConsignee()
		{
			AssertNull("No Consignee", ContainerWrapperInternal.Consignee);

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			consignee.OH_FullName = "Consignee";
			DeclarationInternal.JE_OH_Importer = consignee.PK;

			AssertEquals("Consignee should be consignee", "Consignee", ContainerWrapperInternal.Consignee.Name);
		}

		public void TestConsignor()
		{
			AssertNull("No Consignor", ContainerWrapperInternal.Consignor);

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			consignor.OH_FullName = "Consignor";
			DeclarationInternal.JE_OH_Supplier = consignor.PK;

			AssertEquals("Consignor should be consignor", "Consignor", ContainerWrapperInternal.Consignor.Name);
		}
		#endregion

		#region JobContainer

		public void TestGrossWeightUQ()
		{
			CommonContainer container = Factory.Load<CommonContainer>(ContainerInternal.CO_JC);
			container.JC_GrossWeightUQ = "UQ";
			AssertEquals("GrossWeightUQ", "UQ", ContainerWrapperInternal.GrossWeightUQ);
		}
		public void TestVolumeCapacityUQ()
		{
			ZString volumeCapacityUQ = new ZString("UQ");
			ContainerInternal.VolumeCapacityUQ = volumeCapacityUQ;
			AssertEquals("VolumeCapacityUQ", volumeCapacityUQ, ContainerWrapperInternal.VolumeCapacityUQ);
		}

		public void TestWeightCapacityUQ()
		{
			ZString weightCapacityUQ = new ZString("UQ");
			ContainerInternal.WeightCapacityUQ = weightCapacityUQ;
			AssertEquals("WeightCapacityUQ", weightCapacityUQ, ContainerWrapperInternal.WeightCapacityUQ);
		}
		public void TestNetWeight()
		{
			AssertEquals("NetWeight", ContainerInternal.NetWeight, ContainerWrapperInternal.NetWeight);
		}
		public void TestTareWeight()
		{
			ZDecimal tareWeight = new ZDecimal(1);
			ContainerInternal.TareWeight = tareWeight;
			AssertEquals("TareWeight", tareWeight, ContainerWrapperInternal.TareWeight);
		}

		public void TestDunnageWeight()
		{
			ZDecimal dunnageWeight = new ZDecimal(1);
			ContainerInternal.DunnageWeight = dunnageWeight;
			AssertEquals("DunnageWeight", dunnageWeight, ContainerWrapperInternal.DunnageWeight);
		}

		public void TestGrossWeight()
		{
			CommonContainer container = Factory.Load<CommonContainer>(ContainerInternal.CO_JC);
			container.JC_GrossWeight = 1.23m;
			AssertEquals("GrossWeight", 1.23m, ContainerWrapperInternal.GrossWeight);
		}

		public void TestVolumeCapacity()
		{
			ZDecimal volumeCapacity = new ZDecimal(1);
			ContainerInternal.VolumeCapacity = volumeCapacity;
			AssertEquals("VolumeCapacity", volumeCapacity, ContainerWrapperInternal.VolumeCapacity);
		}

		public void TestWeightCapacity()
		{
			ZDecimal weightCapacity = new ZDecimal(1);
			ContainerInternal.WeightCapacity = weightCapacity;
			AssertEquals("WeightCapacity", weightCapacity, ContainerWrapperInternal.WeightCapacity);
		}

		public void TestTotalHeight()
		{
			ZDecimal totalHeight = new ZDecimal(1);
			ContainerInternal.TotalHeight = totalHeight;
			AssertEquals("TotalHeight", totalHeight, ContainerWrapperInternal.TotalHeight);
		}

		public void TestTotalLength()
		{
			ZDecimal totalLength = new ZDecimal(1);
			ContainerInternal.TotalLength = totalLength;
			AssertEquals("TotalLength", totalLength, ContainerWrapperInternal.TotalLength);
		}

		public void TestTotalWidth()
		{
			ZDecimal totalWidth = new ZDecimal(1);
			ContainerInternal.TotalWidth = totalWidth;
			AssertEquals("TotalWidth", totalWidth, ContainerWrapperInternal.TotalWidth);
		}
		#endregion

		#region IDocServicesParent Members

		public void TestConsolNumber()
		{
			DeclarationInternal.JE_DeclarationReference = "DECREF4";
			AssertEquals("Consol Number", "DECREF4", ServicesParentWrapper.ConsolNumber);
		}

		public void TestGoodsDescription()
		{
			DeclarationInternal.JE_GoodsDescription = "bananas";
			AssertEquals("Goods Description", "BANANAS", ServicesParentWrapper.GoodsDescription.ToUpper());
		}

		public void TestPackages()
		{
			DeclarationInternal.JE_TotalNoOfPacks = 4;
			DeclarationInternal.JE_TotalNoOfPacksPackType = "PKG";
			AssertEquals("Packages", "4 PKG (OUTER)", ServicesParentWrapper.Packages);
		}

		public void TestMasterBillNum()
		{
			DeclarationInternal.JE_MasterBill = "ME334";
			AssertEquals("MasterBillNum", "ME334", ServicesParentWrapper.MasterBillNum);
		}

		public void TestMasterBillHeading()
		{
			DeclarationInternal.JE_TransportMode = ZString.Empty;
			AssertEquals("MasterBillHeading", "MASTER BILL", ServicesParentWrapper.MasterBillHeading);
		}

		public void TestHouseBill()
		{
			DeclarationInternal.JE_HouseBill = "HOUSE2222";
			AssertEquals("HouseBill", "HOUSE2222", ServicesParentWrapper.HouseBill);
		}

		public void TestHouseBillHeading()
		{
			DeclarationInternal.JE_TransportMode = ZString.Empty;
			AssertEquals("HouseBillHeading", "HOUSE BILL", ServicesParentWrapper.HouseBillHeading);
		}

		public void TestContext()
		{
			AssertEquals("CUSCONTAINER", ServicesParentWrapper.Context);
		}

		public void TestContainerNumbers()
		{
			AssertEquals("", ServicesParentWrapper.ContainerNumbers);
		}

		public void TestTransportInfo()
		{
			DeclarationInternal.JE_TransportMode = DeclarationInternal.TransportModeSeaCodeForTesting;
			DeclarationInternal.JE_VoyageFlightNo = "Voyage";
			AssertEquals("Transport", "Voyage", ServicesParentWrapper.TransportInfo);
		}

		public void TestETD()
		{
			DeclarationInternal.JE_DateAtOrigin = ZDateTime.Today;
			AssertEquals("ETD is Date at Origin", DeclarationInternal.JE_DateAtOrigin, ServicesParentWrapper.ETD);
		}

		public void TestETA()
		{
			DeclarationInternal.JE_DateAtFinalDestination = ZDateTime.Today;
			AssertEquals("ETA is Date at Final Destination", DeclarationInternal.JE_DateAtFinalDestination, ServicesParentWrapper.ETA);
		}

		public void TestWeightForRequestForService()
		{
			CommonContainer container = Factory.Load<CommonContainer>(ContainerInternal.CO_JC);
			container.JC_GrossWeight = 500m;
			AssertEquals("Weight", "500", ServicesParentWrapper.Weight);
		}

		public void TestVolume()
		{
			AssertEquals("", ServicesParentWrapper.Volume);
		}

		public void TestWeightUnit()
		{
			CommonContainer container = Factory.Load<CommonContainer>(ContainerInternal.CO_JC);
			container.JC_GrossWeightUQ = "KG";
			AssertEquals("Weight", "KG", ServicesParentWrapper.WeightUnit);
		}

		public void TestVolumeUnit()
		{
			AssertEquals("", ServicesParentWrapper.VolumeUnit);
		}

		public void TestPortOfLoading()
		{
			DeclarationInternal.JE_RL_NKPortOfLoading = "AUSYD";
			AssertEquals("Port of Loading", "AUSYD", ServicesParentWrapper.PortOfLoading.ToString());
		}

		public void TestPortOfDischarge()
		{
			DeclarationInternal.JE_RL_NKPortOfArrival = "BRRIO";
			AssertEquals("Port of Discharge", "BRRIO", ServicesParentWrapper.PortOfDischarge.ToString());
		}

		public void TestOwnerRefAndOrderRef()
		{
			DeclarationInternal.JE_MessageType = JobMessageTypeCodeForImport;
			DeclarationInternal.DocsAndCartage.JP_OrderItemsAsString = "Orders";
			DeclarationInternal.JE_OwnerRef = "Owner's Ref 123";
			AssertEquals("Test with order items as string", "Owner's Ref 123 Orders", ServicesParentWrapper.OwnerRefAndOrderRef);

			DeclarationInternal.DocsAndCartage.JP_OrderItemsAsString = "Order123";
			DeclarationInternal.JE_OwnerRef = "123";
			AssertEquals("Test with order items as string", "Order123", ServicesParentWrapper.OwnerRefAndOrderRef);
		}

		public void TestOwnerRefAndOrderRefHeading()
		{
			AssertEquals("ORDER NUMBERS / REFERENCE", ServicesParentWrapper.OwnerRefAndOrderRefHeading);
		}

		#endregion

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				ContainerWrapperInternal
			};
		}

		#region Implementation

		protected T ContainerInternal;
		protected TWrapper ContainerWrapperInternal;

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return CreateContainerWrapper(ContainerInternal, DeclarationInternal);
		}

		protected BaseJobDeclaration DeclarationInternal;
		protected DocBaseJobDeclaration DeclarationWrapper;
		protected IDocServicesParent ServicesParentWrapper;

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry(TestingCountry);
			ContainerInternal = GetNewContainer();
			DeclarationInternal = Factory.NewWithValidTestData<BaseJobDeclaration>();
			DeclarationInternal.JE_TransportMode = Core.Constants.TransportModes.Sea;
			DeclarationInternal.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			DeclarationInternal.CusContainers.Add(ContainerInternal);
			Factory.Save();
			CreateWrappersForTest();
			base.SetUp();
		}

		protected virtual T GetNewContainer()
		{
			return Factory.New<T>();
		}

		void CreateWrappersForTest()
		{
			ContainerWrapperInternal = CreateContainerWrapper(ContainerInternal, DeclarationInternal);
			DeclarationWrapper = DocBaseJobDeclaration.New(DeclarationInternal, Factory);
			ServicesParentWrapper = ContainerWrapperInternal;
		}

		protected virtual ZString JobMessageTypeCodeForImport
		{
			get { return JobMessageTypeList.Codes.Import; }
		}

		protected virtual ZString JobMessageTypeCodeForExport
		{
			get { return JobMessageTypeList.Codes.Export; }
		}

		#endregion
	}
}
