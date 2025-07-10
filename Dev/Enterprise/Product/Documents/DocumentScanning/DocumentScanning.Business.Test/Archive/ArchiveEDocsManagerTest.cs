using System;
using System.Collections;
using System.IO;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	[TestedType(typeof(ArchiveEDocsManager))]
	sealed class ArchiveeDocsManagerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCreateAchiveListWithShipmentsAndNotIncludingRelatedEDocs()
		{
			SetupOrganisations();

			BusinessObject testShipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			testShipment["JS_UniqueConsignRef"] = "MagicString";

			AssertEquals("relatedobjects", 0, ((DocManagerInfo)testShipment["DocManagerInfo"]).RelatedObjects.Length);

			((JobDocAddress)testShipment["ConsigneeDocumentaryAddress"]).OrganisationPK = Consignee.PK;
			AssertEquals("relatedobjects", 1, ((DocManagerInfo)testShipment["DocManagerInfo"]).RelatedObjects.Length);

			BusinessObject arInvoice = MasterFactory.NewWithValidTestData(ObjectFactory.GetType(typeof(IARInvoice)));
			arInvoice[AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef] = testShipment["JS_UniqueConsignRef"];
			AssertEquals("relatedobjects", 2, ((DocManagerInfo)testShipment["DocManagerInfo"]).RelatedObjects.Length);

			StorageMain invoiceStorageMain = CreateStorageMainWithEDocs(Core.Constants.DocManagerCodes.ReceivableInvoice, arInvoice.PK, 1);

			Manager.SA_IncludeRelatedEDocs = false;
			Manager.SA_Organisation = ((OrgHeader)testShipment["Consignee"]).PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the correct Consignee", 0, Manager.ListToArchive.Count);
		}

		public void TestCreateAchiveListWithShipmentsAndRelatedEDocs()
		{
			SetupOrganisations();

			BusinessObject testShipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			AssertEquals("relatedobjects", 0, ((DocManagerInfo)testShipment["DocManagerInfo"]).RelatedObjects.Length);
			testShipment["JS_UniqueConsignRef"] = "MagicString";

			((JobDocAddress)testShipment["ConsigneeDocumentaryAddress"]).OrganisationPK = Consignee.PK;
			AssertEquals("relatedobjects", 1, ((DocManagerInfo)testShipment["DocManagerInfo"]).RelatedObjects.Length);

			BusinessObject arInvoice = MasterFactory.NewWithValidTestData(ObjectFactory.GetType<IARInvoice>());
			arInvoice[AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef] = testShipment["JS_UniqueConsignRef"];
			AssertEquals("relatedobjects", 2, ((DocManagerInfo)testShipment["DocManagerInfo"]).RelatedObjects.Length);

			StorageMain invoiceStorageMain = CreateStorageMainWithEDocs(Core.Constants.DocManagerCodes.ReceivableInvoice, arInvoice.PK, 1);
			StorageMain shipmentStorageMain = CreateStorageMainWithEDocs(Core.Constants.DocManagerCodes.Shipment, testShipment.PK, 1);

			Manager.SA_Organisation = ((OrgHeader)testShipment["Consignee"]).PK;
			AssertEquals("SA_IncludeRelatedEDocs", Manager.SA_IncludeRelatedEDocs, true);
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the correct Consignee", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipment (As Consignee, November)", true, Manager.ListToArchive.Contains(invoiceStorageMain.PK));
		}

		public void TestAgencyForwardingSeparation()
		{
			SetupShipments();
			SetupAgencyShipments();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;

			SetOnlySearchType(Manager, Enterprise.Core.Constants.DocManagerCodes.AgencyShipment);

			Manager.CreateArchiveList();
			AssertEquals("Should contain the agency shipment", true, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Should not contain the non-agency shipment", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeNovemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			SetOnlySearchType(Manager, Enterprise.Core.Constants.DocManagerCodes.Shipment);

			Manager.CreateArchiveList();
			AssertEquals("Should not contain the agency shipment", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Should contain the non-agency shipment", true, Manager.ListToArchive.Contains(ShipmentWithConsigneeNovemberMain.PK));
		}

		#region Shipment tests

		public void TestCreateArchiveListWithShipmentsConsigneeOnly()
		{
			SetupShipments();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the correct Consignee", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipment (As Consignee, November)", true, Manager.ListToArchive.Contains(ShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipment (As Consignee, December)", true, Manager.ListToArchive.Contains(ShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct shipment (As Consignor, November)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct shipment (As Consignor, December)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct shipment (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(ShipmentWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipment (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(ShipmentWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithShipmentsConsigneeAndConsignor()
		{
			SetupShipments();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = true;
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the matching org as Consignee or Consignor", 4, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipments (As Consignee, November)", true, Manager.ListToArchive.Contains(ShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignee, December)", true, Manager.ListToArchive.Contains(ShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, November)", true, Manager.ListToArchive.Contains(ShipmentWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, December)", true, Manager.ListToArchive.Contains(ShipmentWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(ShipmentWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(ShipmentWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithShipmentsETADateConstrained()
		{
			SetupShipments();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;
			Manager.SA_ETAFrom = new ZDateTime(2003, 11, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the matching org and matching dates", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipments (As Consignee, November)", true, Manager.ListToArchive.Contains(ShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignee, December)", true, Manager.ListToArchive.Contains(ShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, November)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, December)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(ShipmentWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(ShipmentWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETATo = new ZDateTime(2003, 11, 30);
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the matching org and matching dates", 1, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipments (As Consignee, November)", true, Manager.ListToArchive.Contains(ShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignee, December)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, November)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, December)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(ShipmentWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(ShipmentWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithShipmentsETADateConstrainedConsignorAndConsignee()
		{
			SetupShipments();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = true;
			Manager.SA_ETAFrom = new ZDateTime(2003, 11, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the matching org and matching dates", 4, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipments (As Consignee, November)", true, Manager.ListToArchive.Contains(ShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignee, December)", true, Manager.ListToArchive.Contains(ShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, November)", true, Manager.ListToArchive.Contains(ShipmentWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, December)", true, Manager.ListToArchive.Contains(ShipmentWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(ShipmentWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(ShipmentWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETATo = new ZDateTime(2003, 11, 30);
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the matching org and matching dates", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipments (As Consignee, November)", true, Manager.ListToArchive.Contains(ShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignee, December)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, November)", true, Manager.ListToArchive.Contains(ShipmentWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, December)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(ShipmentWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(ShipmentWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithShipmentsETDDateConstrained()
		{
			SetupShipments();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;
			Manager.SA_ETDFrom = new ZDateTime(2003, 12, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the matching org and matching dates", 1, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipments (As Consignee, November)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignee, December)", true, Manager.ListToArchive.Contains(ShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, November)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, December)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(ShipmentWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(ShipmentWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETDTo = new ZDateTime(2003, 12, 09);
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the matching org and matching dates", 0, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipments (As Consignee, November)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignee, December)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, November)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, December)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(ShipmentWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(ShipmentWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithShipmentsETDDateConstrainedConsignorAndConsignee()
		{
			SetupShipments();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = true;
			Manager.SA_ETDFrom = new ZDateTime(2003, 12, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the matching org and matching dates", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipments (As Consignee, November)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignee, December)", true, Manager.ListToArchive.Contains(ShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, November)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, December)", true, Manager.ListToArchive.Contains(ShipmentWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(ShipmentWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(ShipmentWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETDTo = new ZDateTime(2003, 12, 09);
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the matching org and matching dates", 0, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipments (As Consignee, November)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignee, December)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, November)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, December)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(ShipmentWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(ShipmentWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithShipmentsETAandETDDateConstrained()
		{
			SetupShipments();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;
			Manager.SA_ETDFrom = new ZDateTime(2003, 11, 01);
			Manager.SA_ETAFrom = new ZDateTime(2003, 11, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the matching org and matching dates", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipments (As Consignee, November)", true, Manager.ListToArchive.Contains(ShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignee, December)", true, Manager.ListToArchive.Contains(ShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, November)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, December)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(ShipmentWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(ShipmentWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETDTo = new ZDateTime(2003, 11, 30);
			Manager.SA_ETATo = new ZDateTime(2003, 11, 30);
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the matching org and matching dates", 1, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipments (As Consignee, November)", true, Manager.ListToArchive.Contains(ShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignee, December)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, November)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, December)", false, Manager.ListToArchive.Contains(ShipmentWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(ShipmentWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(ShipmentWithNoMatchingConsigneeDecemberMain.PK));
		}

		[TestDate(2008, 8, 8)]
		public void TestCreateArchiveListWithShipments_JobClosed_From_To_DateConstrained()
		{
			SetupShipments();
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = ShipmentWithConsigneeNovember.PK;
			job.JH_ParentTableCode = "JS";
			Factory.Save();
			AssertJobClosed(job, ShipmentWithConsigneeNovemberMain);
		}

		#endregion

		#region Agency Shipment tests

		public void TestCreateArchiveListWithAgencyShipmentsConsigneeOnly()
		{
			SetupAgencyShipments();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the correct Consignee", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipment (As Consignee, November)", true, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipment (As Consignee, December)", true, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct shipment (As Consignor, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct shipment (As Consignor, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct shipment (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipment (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithAgencyShipmentsConsigneeAndConsignor()
		{
			SetupAgencyShipments();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = true;
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the matching org as Consignee or Consignor", 4, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipments (As Consignee, November)", true, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignee, December)", true, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, November)", true, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, December)", true, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithAgencyShipmentsETADateConstrained()
		{
			SetupAgencyShipments();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;
			Manager.SA_ETAFrom = new ZDateTime(2003, 11, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the matching org and matching dates", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipments (As Consignee, November)", true, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignee, December)", true, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETATo = new ZDateTime(2003, 11, 30);
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the matching org and matching dates", 1, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipments (As Consignee, November)", true, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignee, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithAgencyShipmentsETADateConstrainedConsignorAndConsignee()
		{
			SetupAgencyShipments();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = true;
			Manager.SA_ETAFrom = new ZDateTime(2003, 11, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the matching org and matching dates", 4, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipments (As Consignee, November)", true, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignee, December)", true, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, November)", true, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, December)", true, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETATo = new ZDateTime(2003, 11, 30);
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the matching org and matching dates", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipments (As Consignee, November)", true, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignee, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, November)", true, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithAgencyShipmentsETDDateConstrained()
		{
			SetupAgencyShipments();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;
			Manager.SA_ETDFrom = new ZDateTime(2003, 12, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the matching org and matching dates", 1, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipments (As Consignee, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignee, December)", true, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETDTo = new ZDateTime(2003, 12, 09);
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the matching org and matching dates", 0, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipments (As Consignee, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignee, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithAgencyShipmentsETDDateConstrainedConsignorAndConsignee()
		{
			SetupAgencyShipments();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = true;
			Manager.SA_ETDFrom = new ZDateTime(2003, 12, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the matching org and matching dates", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipments (As Consignee, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignee, December)", true, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, December)", true, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETDTo = new ZDateTime(2003, 12, 09);
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the matching org and matching dates", 0, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipments (As Consignee, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignee, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithAgencyShipmentsETAandETDDateConstrained()
		{
			SetupAgencyShipments();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;
			Manager.SA_ETDFrom = new ZDateTime(2003, 11, 01);
			Manager.SA_ETAFrom = new ZDateTime(2003, 11, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the matching org and matching dates", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipments (As Consignee, November)", true, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignee, December)", true, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETDTo = new ZDateTime(2003, 11, 30);
			Manager.SA_ETATo = new ZDateTime(2003, 11, 30);
			Manager.CreateArchiveList();
			AssertEquals("Element count of shipments with the matching org and matching dates", 1, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct shipments (As Consignee, November)", true, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignee, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct shipments (As Consignor, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(AgencyShipmentWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct shipments (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(AgencyShipmentWithNoMatchingConsigneeDecemberMain.PK));
		}

		[TestDate(2008, 8, 8)]
		public void TestCreateArchiveListWithAgencyShipments_JobClosed_From_To_DateConstrained()
		{
			SetupAgencyShipments();
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = AgencyShipmentWithConsigneeNovember.PK;
			job.JH_ParentTableCode = "JS";
			Factory.Save();
			AssertJobClosed(job, AgencyShipmentWithConsigneeNovemberMain);
		}

		#endregion

		#region Consol Tests

		public void TestCreateArchiveListWithConsolsConsigneeOnly()
		{
			SetupConsols();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;
			Manager.CreateArchiveList();
			AssertEquals("Element count of Consols with the correct Consignee", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Consol (As Consignee, November)", true, Manager.ListToArchive.Contains(ConsolWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Consol (As Consignee, December)", true, Manager.ListToArchive.Contains(ConsolWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Consol (As Consignor, November)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Consol (As Consignor, December)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Consol (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(ConsolWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Consol (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(ConsolWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithConsolsConsigneeAndConsignor()
		{
			SetupConsols();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = true;
			Manager.CreateArchiveList();
			AssertEquals("Element count of Consols with the matching org as Consignee or Consignor", 4, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Consols (As Consignee, November)", true, Manager.ListToArchive.Contains(ConsolWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignee, December)", true, Manager.ListToArchive.Contains(ConsolWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignor, November)", true, Manager.ListToArchive.Contains(ConsolWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignor, December)", true, Manager.ListToArchive.Contains(ConsolWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Consols (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(ConsolWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Consols (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(ConsolWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithConsolsETADateConstrained()
		{
			SetupConsols();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;
			Manager.SA_ETAFrom = new ZDateTime(2003, 11, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Consols with the matching org and matching dates", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Consols (As Consignee, November)", true, Manager.ListToArchive.Contains(ConsolWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignee, December)", true, Manager.ListToArchive.Contains(ConsolWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignor, November)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignor, December)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Consols (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(ConsolWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Consols (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(ConsolWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETATo = new ZDateTime(2003, 11, 30);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Consols with the matching org and matching dates", 1, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Consols (As Consignee, November)", true, Manager.ListToArchive.Contains(ConsolWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignee, December)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignor, November)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignor, December)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Consols (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(ConsolWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Consols (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(ConsolWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithConsolsETADateConstrainedConsignorAndConsignee()
		{
			SetupConsols();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = true;
			Manager.SA_ETAFrom = new ZDateTime(2003, 11, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Consols with the matching org and matching dates", 4, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Consols (As Consignee, November)", true, Manager.ListToArchive.Contains(ConsolWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignee, December)", true, Manager.ListToArchive.Contains(ConsolWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignor, November)", true, Manager.ListToArchive.Contains(ConsolWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignor, December)", true, Manager.ListToArchive.Contains(ConsolWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Consols (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(ConsolWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Consols (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(ConsolWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETATo = new ZDateTime(2003, 11, 30);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Consols with the matching org and matching dates", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Consols (As Consignee, November)", true, Manager.ListToArchive.Contains(ConsolWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignee, December)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignor, November)", true, Manager.ListToArchive.Contains(ConsolWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignor, December)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Consols (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(ConsolWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Consols (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(ConsolWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithConsolsETDDateConstrained()
		{
			SetupConsols();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;
			Manager.SA_ETDFrom = new ZDateTime(2003, 12, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Consols with the matching org and matching dates", 1, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Consols (As Consignee, November)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignee, December)", true, Manager.ListToArchive.Contains(ConsolWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignor, November)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignor, December)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Consols (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(ConsolWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Consols (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(ConsolWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETDTo = new ZDateTime(2003, 12, 09);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Consols with the matching org and matching dates", 0, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Consols (As Consignee, November)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignee, December)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignor, November)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignor, December)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Consols (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(ConsolWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Consols (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(ConsolWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithConsolsETDDateConstrainedConsignorAndConsignee()
		{
			SetupConsols();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = true;
			Manager.SA_ETDFrom = new ZDateTime(2003, 12, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Consols with the matching org and matching dates", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Consols (As Consignee, November)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignee, December)", true, Manager.ListToArchive.Contains(ConsolWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignor, November)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignor, December)", true, Manager.ListToArchive.Contains(ConsolWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Consols (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(ConsolWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Consols (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(ConsolWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETDTo = new ZDateTime(2003, 12, 09);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Consols with the matching org and matching dates", 0, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Consols (As Consignee, November)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignee, December)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignor, November)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignor, December)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Consols (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(ConsolWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Consols (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(ConsolWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithConsolsETAandETDDateConstrained()
		{
			SetupConsols();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;
			Manager.SA_ETDFrom = new ZDateTime(2003, 11, 01);
			Manager.SA_ETAFrom = new ZDateTime(2003, 11, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Consols with the matching org and matching dates", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Consols (As Consignee, November)", true, Manager.ListToArchive.Contains(ConsolWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignee, December)", true, Manager.ListToArchive.Contains(ConsolWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignor, November)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignor, December)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Consols (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(ConsolWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Consols (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(ConsolWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETDTo = new ZDateTime(2003, 11, 30);
			Manager.SA_ETATo = new ZDateTime(2003, 11, 30);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Consols with the matching org and matching dates", 1, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Consols (As Consignee, November)", true, Manager.ListToArchive.Contains(ConsolWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignee, December)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignor, November)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Consols (As Consignor, December)", false, Manager.ListToArchive.Contains(ConsolWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Consols (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(ConsolWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Consols (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(ConsolWithNoMatchingConsigneeDecemberMain.PK));
		}

		[TestDate(2008, 8, 8)]
		public void TestCreateArchiveListWithConsols_JobClosed_From_To_DateConstrained()
		{
			SetupConsols();
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = ConsolWithConsigneeNovember.PK;
			job.JH_ParentTableCode = "JK";
			Factory.Save();
			AssertJobClosed(job, ConsolWithConsigneeNovemberMain);
		}

		#endregion

		#region Transport Tests

		public void TestCreateArchiveListWithTransportsConsigneeOnly()
		{
			SetupTransportJobs();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;
			Manager.CreateArchiveList();
			AssertEquals("Element count of Transports with the correct Consignee", 4, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Transport Job (As Consignee, November)", true, Manager.ListToArchive.Contains(TransportWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Transport Job (As Consignee, December)", true, Manager.ListToArchive.Contains(TransportWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Transport Job (As Consignor, November)", true, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Transport Job (As Consignor, December)", true, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Transport Job (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(TransportWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Transport Job (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(TransportWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithTransportsConsigneeAndConsignor()
		{
			SetupTransportJobs();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = true;
			Manager.CreateArchiveList();
			AssertEquals("Element count of Transports with the matching org as Consignee or Consignor", 4, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Transport Jobs (As Consignee, November)", true, Manager.ListToArchive.Contains(TransportWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignee, December)", true, Manager.ListToArchive.Contains(TransportWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignor, November)", true, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignor, December)", true, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(TransportWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(TransportWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithTransportsETADateConstrained()
		{
			SetupTransportJobs();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;
			Manager.SA_ETAFrom = new ZDateTime(2003, 11, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Transports with the matching org and matching dates", 4, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Transport Jobs (As Consignee, November)", true, Manager.ListToArchive.Contains(TransportWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignee, December)", true, Manager.ListToArchive.Contains(TransportWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignor, November)", true, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignor, December)", true, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(TransportWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(TransportWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETATo = new ZDateTime(2003, 11, 30);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Transports with the matching org and matching dates", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Transport Jobs (As Consignee, November)", true, Manager.ListToArchive.Contains(TransportWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignee, December)", false, Manager.ListToArchive.Contains(TransportWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignor, November)", true, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignor, December)", false, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(TransportWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(TransportWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithTransportsETADateConstrainedConsignorAndConsignee()
		{
			SetupTransportJobs();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = true;
			Manager.SA_ETAFrom = new ZDateTime(2003, 11, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Transports with the matching org and matching dates", 4, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Transport Jobs (As Consignee, November)", true, Manager.ListToArchive.Contains(TransportWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignee, December)", true, Manager.ListToArchive.Contains(TransportWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignor, November)", true, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignor, December)", true, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(TransportWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(TransportWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETATo = new ZDateTime(2003, 11, 30);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Transports with the matching org and matching dates", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Transport Jobs (As Consignee, November)", true, Manager.ListToArchive.Contains(TransportWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignee, December)", false, Manager.ListToArchive.Contains(TransportWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignor, November)", true, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignor, December)", false, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(TransportWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(TransportWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithTransportsETDDateConstrained()
		{
			SetupTransportJobs();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;
			Manager.SA_ETDFrom = new ZDateTime(2003, 12, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Transports with the matching org and matching dates", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Transport Jobs (As Consignee, November)", false, Manager.ListToArchive.Contains(TransportWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignee, December)", true, Manager.ListToArchive.Contains(TransportWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignor, November)", false, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignor, December)", true, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(TransportWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(TransportWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETDTo = new ZDateTime(2003, 12, 09);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Transports with the matching org and matching dates", 0, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Transport Jobs (As Consignee, November)", false, Manager.ListToArchive.Contains(TransportWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignee, December)", false, Manager.ListToArchive.Contains(TransportWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignor, November)", false, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignor, December)", false, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(TransportWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(TransportWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithTransportsETDDateConstrainedConsignorAndConsignee()
		{
			SetupTransportJobs();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = true;
			Manager.SA_ETDFrom = new ZDateTime(2003, 12, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Transports with the matching org and matching dates", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Transport Jobs (As Consignee, November)", false, Manager.ListToArchive.Contains(TransportWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignee, December)", true, Manager.ListToArchive.Contains(TransportWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignor, November)", false, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignor, December)", true, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(TransportWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(TransportWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETDTo = new ZDateTime(2003, 12, 09);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Transports with the matching org and matching dates", 0, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Transport Jobs (As Consignee, November)", false, Manager.ListToArchive.Contains(TransportWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignee, December)", false, Manager.ListToArchive.Contains(TransportWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignor, November)", false, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignor, December)", false, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(TransportWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(TransportWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithTransportsETAandETDDateConstrained()
		{
			SetupTransportJobs();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;
			Manager.SA_ETDFrom = new ZDateTime(2003, 11, 01);
			Manager.SA_ETAFrom = new ZDateTime(2003, 11, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Transports with the matching org and matching dates", 4, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Transport Jobs (As Consignee, November)", true, Manager.ListToArchive.Contains(TransportWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignee, December)", true, Manager.ListToArchive.Contains(TransportWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignor, November)", true, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignor, December)", true, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(TransportWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(TransportWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETDTo = new ZDateTime(2003, 11, 30);
			Manager.SA_ETATo = new ZDateTime(2003, 11, 30);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Transports with the matching org and matching dates", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Transport Jobs (As Consignee, November)", true, Manager.ListToArchive.Contains(TransportWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignee, December)", false, Manager.ListToArchive.Contains(TransportWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignor, November)", true, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (As Consignor, December)", false, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(TransportWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Transport Jobs (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(TransportWithNoMatchingConsigneeDecemberMain.PK));
		}

		[TestDate(2008, 8, 8)]
		public void TestCreateArchiveListWithTransports_JobClosed_From_To_DateConstrained()
		{
			SetupTransportJobs();
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = TransportWithConsigneeNovember.PK;
			job.JH_ParentTableCode = "JJ";
			Factory.Save();
			AssertJobClosed((JobHeader)(TransportWithConsigneeNovember)["Job"], TransportWithConsigneeNovemberMain);
		}

		#endregion

		#region Declaration Tests

		public void TestCreateArchiveListWithDeclarationsConsigneeOnly()
		{
			SetupDeclarations();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;
			Manager.CreateArchiveList();
			AssertEquals("Element count of Declarations with the correct Consignee", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Declaration (As Consignee, November)", true, Manager.ListToArchive.Contains(DeclarationWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Declaration (As Consignee, December)", true, Manager.ListToArchive.Contains(DeclarationWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Declaration (As Consignor, November)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Declaration (As Consignor, December)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Declaration (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(DeclarationWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Declaration (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(DeclarationWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithDeclarationsConsigneeAndConsignor()
		{
			SetupDeclarations();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = true;
			Manager.CreateArchiveList();
			AssertEquals("Element count of Declarations with the matching org as Consignee or Consignor", 4, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Declarations (As Consignee, November)", true, Manager.ListToArchive.Contains(DeclarationWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignee, December)", true, Manager.ListToArchive.Contains(DeclarationWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignor, November)", true, Manager.ListToArchive.Contains(DeclarationWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignor, December)", true, Manager.ListToArchive.Contains(DeclarationWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Declarations (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(DeclarationWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(DeclarationWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithDeclarationsETADateConstrained()
		{
			SetupDeclarations();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;
			Manager.SA_ETAFrom = new ZDateTime(2003, 11, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Declarations with the matching org and matching dates", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Declarations (As Consignee, November)", true, Manager.ListToArchive.Contains(DeclarationWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignee, December)", true, Manager.ListToArchive.Contains(DeclarationWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignor, November)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignor, December)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Declarations (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(DeclarationWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(DeclarationWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETATo = new ZDateTime(2003, 11, 30);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Declarations with the matching org and matching dates", 1, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Declarations (As Consignee, November)", true, Manager.ListToArchive.Contains(DeclarationWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignee, December)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignor, November)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignor, December)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Declarations (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(DeclarationWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(DeclarationWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithDeclarationsETADateConstrainedConsignorAndConsignee()
		{
			SetupDeclarations();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = true;
			Manager.SA_ETAFrom = new ZDateTime(2003, 11, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Declarations with the matching org and matching dates", 4, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Declarations (As Consignee, November)", true, Manager.ListToArchive.Contains(DeclarationWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignee, December)", true, Manager.ListToArchive.Contains(DeclarationWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignor, November)", true, Manager.ListToArchive.Contains(DeclarationWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignor, December)", true, Manager.ListToArchive.Contains(DeclarationWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Declarations (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(DeclarationWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(DeclarationWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETATo = new ZDateTime(2003, 11, 30);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Declarations with the matching org and matching dates", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Declarations (As Consignee, November)", true, Manager.ListToArchive.Contains(DeclarationWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignee, December)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignor, November)", true, Manager.ListToArchive.Contains(DeclarationWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignor, December)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Declarations (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(DeclarationWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(DeclarationWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithDeclarationsETDDateConstrained()
		{
			SetupDeclarations();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;
			Manager.SA_ETDFrom = new ZDateTime(2003, 12, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Declarations with the matching org and matching dates", 1, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Declarations (As Consignee, November)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignee, December)", true, Manager.ListToArchive.Contains(DeclarationWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignor, November)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignor, December)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Declarations (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(DeclarationWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(DeclarationWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETDTo = new ZDateTime(2003, 12, 09);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Declarations with the matching org and matching dates", 0, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Declarations (As Consignee, November)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignee, December)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignor, November)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignor, December)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Declarations (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(DeclarationWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(DeclarationWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithDeclarationsETDDateConstrainedConsignorAndConsignee()
		{
			SetupDeclarations();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = true;
			Manager.SA_ETDFrom = new ZDateTime(2003, 12, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Declarations with the matching org and matching dates", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Declarations (As Consignee, November)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignee, December)", true, Manager.ListToArchive.Contains(DeclarationWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignor, November)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignor, December)", true, Manager.ListToArchive.Contains(DeclarationWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Declarations (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(DeclarationWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(DeclarationWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETDTo = new ZDateTime(2003, 12, 09);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Declarations with the matching org and matching dates", 0, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Declarations (As Consignee, November)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignee, December)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignor, November)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignor, December)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Declarations (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(DeclarationWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(DeclarationWithNoMatchingConsigneeDecemberMain.PK));
		}

		public void TestCreateArchiveListWithDeclarationsETAandETDDateConstrained()
		{
			SetupDeclarations();

			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;
			Manager.SA_ETDFrom = new ZDateTime(2003, 11, 01);
			Manager.SA_ETAFrom = new ZDateTime(2003, 11, 01);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Declarations with the matching org and matching dates", 2, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Declarations (As Consignee, November)", true, Manager.ListToArchive.Contains(DeclarationWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignee, December)", true, Manager.ListToArchive.Contains(DeclarationWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignor, November)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignor, December)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Declarations (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(DeclarationWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(DeclarationWithNoMatchingConsigneeDecemberMain.PK));

			Manager.ListToArchive.RemoveAll();
			Manager.SA_ETDTo = new ZDateTime(2003, 11, 30);
			Manager.SA_ETATo = new ZDateTime(2003, 11, 30);
			Manager.CreateArchiveList();
			AssertEquals("Element count of Declarations with the matching org and matching dates", 1, Manager.ListToArchive.Count);
			AssertEquals("Contains the correct Declarations (As Consignee, November)", true, Manager.ListToArchive.Contains(DeclarationWithConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignee, December)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeDecemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignor, November)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeAsConsignorNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (As Consignor, December)", false, Manager.ListToArchive.Contains(DeclarationWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("Contains the correct Declarations (No match Consignee or Consignor, November)", false, Manager.ListToArchive.Contains(DeclarationWithNoMatchingConsigneeNovemberMain.PK));
			AssertEquals("Contains the correct Declarations (No match Consignee or Consignor, December)", false, Manager.ListToArchive.Contains(DeclarationWithNoMatchingConsigneeDecemberMain.PK));
		}

		[TestDate(2008, 8, 8)]
		public void TestCreateArchiveListWithDeclarations_JobClosed_From_To_DateConstrained()
		{
			SetupDeclarations();
			JobHeader job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentID = DeclarationWithConsigneeNovember.PK;
			job.JH_ParentTableCode = "JE";
			Factory.Save();
			AssertJobClosed(job, DeclarationWithConsigneeNovemberMain);
		}

		#endregion

		public void TestCreateArchiveListIncludeShipments()
		{
			SetupConsols();
			SetupDeclarations();
			SetupShipments();
			SetupTransportJobs();

			Manager.SA_Organisation = Consignee.PK;

			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;

			Manager.SearchTypes["Shipment"].IsFilterOn = true;
			Manager.SearchTypes["Consol"].IsFilterOn = false;
			Manager.SearchTypes["Declaration"].IsFilterOn = false;
			Manager.SearchTypes["Transport Job"].IsFilterOn = false;

			Manager.CreateArchiveList();

			AssertEquals("Only 2 mains found - shipment ones searching consignee only", 2, Manager.ListToArchive.Count);
			AssertEquals("Shipment mains only", true, Manager.ListToArchive.Contains(ShipmentWithConsigneeDecemberMain.PK));
			AssertEquals("Shipment mains only", true, Manager.ListToArchive.Contains(ShipmentWithConsigneeNovemberMain.PK));
		}

		public void TestCreateArchiveListIncludeConsols()
		{
			SetupConsols();
			SetupDeclarations();
			SetupShipments();
			SetupTransportJobs();

			Manager.SA_Organisation = Consignee.PK;

			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;

			Manager.SearchTypes["Shipment"].IsFilterOn = false;
			Manager.SearchTypes["Consol"].IsFilterOn = true;
			Manager.SearchTypes["Declaration"].IsFilterOn = false;
			Manager.SearchTypes["Transport Job"].IsFilterOn = false;

			Manager.CreateArchiveList();

			AssertEquals("Only 2 mains found - consol ones searching consignee only", 2, Manager.ListToArchive.Count);
			AssertEquals("Consol mains only", true, Manager.ListToArchive.Contains(ConsolWithConsigneeDecemberMain.PK));
			AssertEquals("Consol mains only", true, Manager.ListToArchive.Contains(ConsolWithConsigneeNovemberMain.PK));
		}

		public void TestCreateArchiveListIncludeDeclarations()
		{
			SetupConsols();
			SetupDeclarations();
			SetupShipments();
			SetupTransportJobs();

			Manager.SA_Organisation = Consignee.PK;

			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;

			Manager.SearchTypes["Shipment"].IsFilterOn = false;
			Manager.SearchTypes["Consol"].IsFilterOn = false;
			Manager.SearchTypes["Declaration"].IsFilterOn = true;
			Manager.SearchTypes["Transport Job"].IsFilterOn = false;

			Manager.CreateArchiveList();

			AssertEquals("Only 2 mains found - declaration ones searching consignee only", 2, Manager.ListToArchive.Count);
			AssertEquals("Declaration mains only", true, Manager.ListToArchive.Contains(DeclarationWithConsigneeDecemberMain.PK));
			AssertEquals("Declaration mains only", true, Manager.ListToArchive.Contains(DeclarationWithConsigneeNovemberMain.PK));
		}

		public void TestCreateArchiveListIncludeTransportJobs()
		{
			SetupConsols();
			SetupDeclarations();
			SetupShipments();
			SetupTransportJobs();

			Manager.SA_Organisation = Consignee.PK;

			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;

			Manager.SearchTypes["Shipment"].IsFilterOn = false;
			Manager.SearchTypes["Consol"].IsFilterOn = false;
			Manager.SearchTypes["Declaration"].IsFilterOn = false;
			Manager.SearchTypes["Transport Job"].IsFilterOn = true;

			Manager.CreateArchiveList();

			AssertEquals("4 mains found - transport jobs matching any org", 4, Manager.ListToArchive.Count);
			AssertEquals("transport jobs mains only", true, Manager.ListToArchive.Contains(TransportWithConsigneeDecemberMain.PK));
			AssertEquals("transport jobs mains only", true, Manager.ListToArchive.Contains(TransportWithConsigneeNovemberMain.PK));
			AssertEquals("transport jobs mains only", true, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorDecemberMain.PK));
			AssertEquals("transport jobs mains only", true, Manager.ListToArchive.Contains(TransportWithConsigneeAsConsignorNovemberMain.PK));
		}

		StorageMain CreateStorageMainWithEDocs(string docManagerCode, ZGuid parentPK, int numberOfDocsToCreate)
		{
			StorageMain main = MasterFactory.New<StorageMain>();
			main.SM_Type = docManagerCode;
			main.SM_ParentFK = parentPK;
			main.SM_DB = 1;

			for (int i = 0; i < numberOfDocsToCreate; i++)
			{
				StorageDocsBase newElement = null;

				if (i % 2 != 0)
				{
					newElement = main.Documents.AddNew();
					newElement.SC_ImageData = SmallTifBytes;
				}
				else
				{
					newElement = main.AddFileOrDocument(SamplePdfBytes, new AddFileOrDocumentDto
					{
						FileName = "Sample.pdf",
						DocumentType = "CIV",
					});
				}

				newElement.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
				newElement.SC_Desc = "test document " + i.ToString();
				newElement.SC_IsPublished = true;
			}

			// not published document
			StorageDocs document = main.Documents.AddNew();
			document.SC_DocType = Core.Constants.RefDocTypes.MiscellaneousDocument;
			document.SC_Desc = "unpublished test document ";
			document.SC_ImageData = SmallTifBytes;

			MasterFactory.Save();

			return main;
		}

		void AssertJobClosed(JobHeader job, StorageMain expectedStorage)
		{
			AssertJobClosedArchiveList(m =>
			{
				job.Logs.CancelAll();
				CloseJob(job);
				m.SA_JobClosedFrom = ZDateTime.Now;
			}, m =>
			{
				AssertEquals("Element count of shipments with the matching org and matching dates", 1, m.ListToArchive.Count);
				AssertEquals("Contains the correct shipments", true, m.ListToArchive.Contains(expectedStorage.PK));
			});

			AssertJobClosedArchiveList(m =>
			{
				job.Logs.CancelAll();
				var to = ZDateTime.Now;
				TestDateAttribute.Date = ZDateTime.Now.AddDays(-3).ToDateTime();
				CloseJob(job);
				m.SA_JobClosedTo = to;
			}, m =>
			{
				AssertEquals("Element count of shipments with the matching org and matching dates", 1, m.ListToArchive.Count);
				AssertEquals("Contains the correct shipments", true, m.ListToArchive.Contains(expectedStorage.PK));
			});

			AssertJobClosedArchiveList(m =>
			{
				job.Logs.CancelAll();
				CloseJob(job);
				m.SA_JobClosedTo = ZDateTime.Now;
			}, m =>
			{
				AssertEquals("Element count of shipments with the matching org and matching dates", 1, m.ListToArchive.Count);
				AssertEquals("Contains the correct shipments", true, m.ListToArchive.Contains(expectedStorage.PK));
			});
		}

		void AssertJobClosedArchiveList(Action<ArchiveEDocsManager> action, Action<ArchiveEDocsManager> assert)
		{
			Manager.ListToArchive.RemoveAll();
			Manager.SA_Organisation = Consignee.PK;
			Manager.SA_IncludeConsignee = true;
			Manager.SA_IncludeConsignor = false;

			Manager.SA_JobClosedFrom = ZDateTime.Empty;
			Manager.SA_JobClosedTo = ZDateTime.Empty;

			action(Manager);
			Manager.CreateArchiveList();
			assert(Manager);
		}

		void CloseJob(BusinessObject business)
		{
			StmALog log = business.GetLogs().AddNew(Events.JobClose, "Test");
			business.Factory.Save();
			Assert("log.IsInDatabase", log.IsInDatabase);
			Assert("log.SL_PostedTimeUtc is set", !log.SL_PostedTimeUtc.IsEmpty);
		}

		public void TestListToArchive()
		{
			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			StorageMain main = MasterFactory.New<StorageMain>();
			main.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			main.SM_ParentFK = shipment.PK;

			StorageDocs document1 = main.Documents.AddNew();
			document1.SC_ImageData = TestTifBytes;
			StorageDocs document2 = main.Documents.AddNew();
			document2.SC_ImageData = TestTifBytes;
			StorageDocs document3 = main.Documents.AddNew();
			document3.SC_ImageData = TestTifBytes;

			AssertNotNull("Can't be null", Manager.ListToArchive);
			Manager.ListToArchive.Add(main);
			AssertEquals("should now have one element", 1, Manager.ListToArchive.Count);
		}

		public void TestTotalSize()
		{
			AssertEquals("TotalSize is 0 when nothing scheduled for archive", "0", Manager.TotalSize);

			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			StorageMain main = MasterFactory.New<StorageMain>();
			main.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			main.SM_ParentFK = shipment.PK;
			main.SM_DB = 1;

			StorageDocs document1 = main.Documents.AddNew();
			document1.SC_ImageData = TestTifBytes;

			Manager.ListToArchive.Add(main);
			MasterFactory.Save();
			AssertEquals("Total Size is 0 because none of the docs are published", "0", Manager.TotalSize);

			StorageFile file1 = main.Files.AddNew();
			file1.SC_ImageData = SamplePdfBytes;
			file1.SC_IsPublished = true;

			Manager.ListToArchive.Remove(main);
			Manager.ListToArchive.Add(main);
			MasterFactory.Save();
			AssertEquals("TotalSize is now 1", "1", Manager.TotalSize);

			Manager.ListToArchive.Remove(main);
			MasterFactory.Save();
			AssertEquals("After removing element, TotalSize is now 0", "0", Manager.TotalSize);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTotalSizeDoesNotUseExtraMemory()
		{
			AssertEquals("TotalSize is 0 when nothing scheduled for archive", "0", Manager.TotalSize);

			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			StorageMain main = MasterFactory.New<StorageMain>();
			main.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			main.SM_ParentFK = shipment.PK;
			main.SM_DB = 1;

			StorageFile file = main.Files.AddNew();
			file.SC_ImageData = DocumentUtilities.GetFileAsBytes(Path.Combine(BaseSourcePath, TestDocsHelper.TestDocsPath, @"1MB.dat"));
			file.SC_IsPublished = true;

			Manager.ListToArchive.Add(main);
			MasterFactory.Save();

			long memoryUsedBefore = GC.GetTotalMemory(true);
			string totalSize = Manager.TotalSize;
			long memoryUsedAfter = GC.GetTotalMemory(true);

			AssertGreaterThanOrEqualTo("Memory usage before should be greater than or equal to after in Megabytes", memoryUsedBefore / (1024 * 1024), memoryUsedAfter / (1024 * 1024));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ArchiveEDocsManager(MasterFactory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			MasterFactory = new DocumentFactoryProvider().GetFactory(Factory);
			Manager = new ArchiveEDocsManager(new DocumentFactoryProvider().GetFactory(Factory));
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		byte[] TestTifBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Test.tif");

		byte[] SmallTifBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.small.tif");

		byte[] SamplePdfBytes => resourceRetriever.Value.GetBytes("Enterprise.DocumentScanning.Business.Test.TestDocs.Sample.PDF");

		static void SetOnlySearchType(ArchiveEDocsManager manager, string code)
		{
			foreach (SearchType searchType in manager.SearchTypes)
			{
				searchType.IsFilterOn = (searchType.AssemblyData.DocManagerCode == code);
			}
		}

		#region Setup Methods

		void SetupOrganisations()
		{
			Consignee = MasterFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "H"));
			NotConsignee = MasterFactory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "L"));
		}

		void SetupShipments()
		{
			SetupOrganisations();

			ShipmentWithConsigneeNovember = CreateShipment(null, Consignee, new ZDateTime(2003, 11, 10), new ZDateTime(2003, 11, 15));
			ShipmentWithConsigneeNovemberMain = CreateStorageMainWithEDocs(Core.Constants.DocManagerCodes.Shipment, ShipmentWithConsigneeNovember.PK, 3);

			ShipmentWithConsigneeDecember = CreateShipment(null, Consignee, new ZDateTime(2003, 12, 10), new ZDateTime(2003, 12, 15));
			ShipmentWithConsigneeDecemberMain = CreateStorageMainWithEDocs(Core.Constants.DocManagerCodes.Shipment, ShipmentWithConsigneeDecember.PK, 3);

			ShipmentWithConsigneeAsConsignorNovember = CreateShipment(Consignee, null, new ZDateTime(2003, 11, 10), new ZDateTime(2003, 11, 15));
			ShipmentWithConsigneeAsConsignorNovemberMain = CreateStorageMainWithEDocs(Core.Constants.DocManagerCodes.Shipment, ShipmentWithConsigneeAsConsignorNovember.PK, 3);

			ShipmentWithConsigneeAsConsignorDecember = CreateShipment(Consignee, null, new ZDateTime(2003, 12, 10), new ZDateTime(2003, 12, 15));
			ShipmentWithConsigneeAsConsignorDecemberMain = CreateStorageMainWithEDocs(Core.Constants.DocManagerCodes.Shipment, ShipmentWithConsigneeAsConsignorDecember.PK, 3);

			ShipmentWithNoMatchingConsigneeNovember = CreateShipment(null, NotConsignee, new ZDateTime(2003, 11, 10), new ZDateTime(2003, 11, 15));
			ShipmentWithNoMatchingConsigneeNovemberMain = CreateStorageMainWithEDocs(Core.Constants.DocManagerCodes.Shipment, ShipmentWithNoMatchingConsigneeNovember.PK, 3);

			ShipmentWithNoMatchingConsigneeDecember = CreateShipment(null, NotConsignee, new ZDateTime(2003, 12, 10), new ZDateTime(2003, 12, 15));
			ShipmentWithNoMatchingConsigneeDecemberMain = CreateStorageMainWithEDocs(Core.Constants.DocManagerCodes.Shipment, ShipmentWithNoMatchingConsigneeDecember.PK, 3);
		}

		void SetupAgencyShipments()
		{
			SetupOrganisations();

			AgencyShipmentWithConsigneeNovember = CreateAgencyShipment(null, Consignee, new ZDateTime(2003, 11, 10), new ZDateTime(2003, 11, 15));
			AgencyShipmentWithConsigneeNovemberMain = CreateStorageMainWithEDocs(Core.Constants.DocManagerCodes.AgencyShipment, AgencyShipmentWithConsigneeNovember.PK, 3);

			AgencyShipmentWithConsigneeDecember = CreateAgencyShipment(null, Consignee, new ZDateTime(2003, 12, 10), new ZDateTime(2003, 12, 15));
			AgencyShipmentWithConsigneeDecemberMain = CreateStorageMainWithEDocs(Core.Constants.DocManagerCodes.AgencyShipment, AgencyShipmentWithConsigneeDecember.PK, 3);

			AgencyShipmentWithConsigneeAsConsignorNovember = CreateAgencyShipment(Consignee, null, new ZDateTime(2003, 11, 10), new ZDateTime(2003, 11, 15));
			AgencyShipmentWithConsigneeAsConsignorNovemberMain = CreateStorageMainWithEDocs(Core.Constants.DocManagerCodes.AgencyShipment, AgencyShipmentWithConsigneeAsConsignorNovember.PK, 3);

			AgencyShipmentWithConsigneeAsConsignorDecember = CreateAgencyShipment(Consignee, null, new ZDateTime(2003, 12, 10), new ZDateTime(2003, 12, 15));
			AgencyShipmentWithConsigneeAsConsignorDecemberMain = CreateStorageMainWithEDocs(Core.Constants.DocManagerCodes.AgencyShipment, AgencyShipmentWithConsigneeAsConsignorDecember.PK, 3);

			AgencyShipmentWithNoMatchingConsigneeNovember = CreateAgencyShipment(null, NotConsignee, new ZDateTime(2003, 11, 10), new ZDateTime(2003, 11, 15));
			AgencyShipmentWithNoMatchingConsigneeNovemberMain = CreateStorageMainWithEDocs(Core.Constants.DocManagerCodes.AgencyShipment, AgencyShipmentWithNoMatchingConsigneeNovember.PK, 3);

			AgencyShipmentWithNoMatchingConsigneeDecember = CreateAgencyShipment(null, NotConsignee, new ZDateTime(2003, 12, 10), new ZDateTime(2003, 12, 15));
			AgencyShipmentWithNoMatchingConsigneeDecemberMain = CreateStorageMainWithEDocs(Core.Constants.DocManagerCodes.AgencyShipment, AgencyShipmentWithNoMatchingConsigneeDecember.PK, 3);
		}

		void SetProperty(object transportsCollection, SchemaColumn column, object value)
		{
			IBusinessObjectCollection transports = (IBusinessObjectCollection)transportsCollection;
			((BusinessObject)((IList)transports)[0])[column] = value;
		}

		void SetupConsols()
		{
			SetupOrganisations();
			SetupSailings();

			ConsolWithConsigneeNovember = (BusinessObject)MasterFactory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			ConsolWithConsigneeNovember[JobConsolSchema.JK_OA_ReceivingForwarderAddress] = Consignee.MainAddress.PK;
			SetProperty(ConsolWithConsigneeNovember["Transports"], JobConsolTransportSchema.JW_JX, NovemberSailing.PK);
			ConsolWithConsigneeNovemberMain = CreateStorageMainWithEDocs("CON", ConsolWithConsigneeNovember.PK, 3);

			ConsolWithConsigneeDecember = (BusinessObject)MasterFactory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			ConsolWithConsigneeDecember[JobConsolSchema.JK_OA_ReceivingForwarderAddress] = Consignee.MainAddress.PK;
			SetProperty(ConsolWithConsigneeDecember["Transports"], JobConsolTransportSchema.JW_JX, DecemberSailing.PK);
			ConsolWithConsigneeDecemberMain = CreateStorageMainWithEDocs("CON", ConsolWithConsigneeDecember.PK, 3);

			ConsolWithConsigneeAsConsignorNovember = (BusinessObject)MasterFactory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			ConsolWithConsigneeAsConsignorNovember[JobConsolSchema.JK_OA_SendingForwarderAddress] = Consignee.MainAddress.PK;
			SetProperty(ConsolWithConsigneeAsConsignorNovember["Transports"], JobConsolTransportSchema.JW_JX, NovemberSailing.PK);
			ConsolWithConsigneeAsConsignorNovemberMain = CreateStorageMainWithEDocs("CON", ConsolWithConsigneeAsConsignorNovember.PK, 3);

			ConsolWithConsigneeAsConsignorDecember = (BusinessObject)MasterFactory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			ConsolWithConsigneeAsConsignorDecember[JobConsolSchema.JK_OA_SendingForwarderAddress] = Consignee.MainAddress.PK;
			SetProperty(ConsolWithConsigneeAsConsignorDecember["Transports"], JobConsolTransportSchema.JW_JX, DecemberSailing.PK);
			ConsolWithConsigneeAsConsignorDecemberMain = CreateStorageMainWithEDocs("CON", ConsolWithConsigneeAsConsignorDecember.PK, 3);

			ConsolWithNoMatchingConsigneeNovember = (BusinessObject)MasterFactory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			ConsolWithNoMatchingConsigneeNovember[JobConsolSchema.JK_OA_ReceivingForwarderAddress] = NotConsignee.MainAddress.PK;
			SetProperty(ConsolWithNoMatchingConsigneeNovember["Transports"], JobConsolTransportSchema.JW_JX, NovemberSailing.PK);
			ConsolWithNoMatchingConsigneeNovemberMain = CreateStorageMainWithEDocs("CON", ConsolWithNoMatchingConsigneeNovember.PK, 3);

			ConsolWithNoMatchingConsigneeDecember = (BusinessObject)MasterFactory.New<Enterprise.Integration.Forwarding.IForwardingConsol>();
			ConsolWithNoMatchingConsigneeDecember[JobConsolSchema.JK_OA_ReceivingForwarderAddress] = NotConsignee.MainAddress.PK;
			SetProperty(ConsolWithNoMatchingConsigneeDecember["Transports"], JobConsolTransportSchema.JW_JX, DecemberSailing.PK);
			ConsolWithNoMatchingConsigneeDecemberMain = CreateStorageMainWithEDocs("CON", ConsolWithNoMatchingConsigneeDecember.PK, 3);
		}

		void SetupTransportJobs()
		{
			SetupOrganisations();
			SetupSailings();

			OrgAddress addressConsignee = Consignee.MainAddress;
			addressConsignee.OA_Address1 = "Somewhere";

			OrgAddress addressNotConsignee = NotConsignee.MainAddress;
			addressNotConsignee.OA_Address1 = "SomewhereElse";

			TransportWithConsigneeNovember = (BusinessObject)MasterFactory.New<ICommonCartage>();
			((JobDocAddress)TransportWithConsigneeNovember["FirstDocAddress"]).E2_OA_Address = addressConsignee.PK;
			TransportWithConsigneeNovember["JJ_JX_Sailing"] = NovemberSailing.PK;
			TransportWithConsigneeNovemberMain = CreateStorageMainWithEDocs("TRN", TransportWithConsigneeNovember.PK, 3);

			TransportWithConsigneeDecember = (BusinessObject)MasterFactory.New<ICommonCartage>();
			((JobDocAddress)TransportWithConsigneeDecember["FirstDocAddress"]).E2_OA_Address = addressConsignee.PK;
			TransportWithConsigneeDecember["JJ_JX_Sailing"] = DecemberSailing.PK;
			TransportWithConsigneeDecemberMain = CreateStorageMainWithEDocs("TRN", TransportWithConsigneeDecember.PK, 3);

			TransportWithConsigneeAsConsignorNovember = (BusinessObject)MasterFactory.New<ICommonCartage>();
			new JobHeader.Loader((IJobHeaderParent)TransportWithConsigneeAsConsignorNovember).TryLoadOrCreate();
			TransportWithConsigneeAsConsignorNovember["LocalClientAddressPK"] = Consignee.MainAddress.PK;
			TransportWithConsigneeAsConsignorNovember["JJ_JX_Sailing"] = NovemberSailing.PK;
			TransportWithConsigneeAsConsignorNovemberMain = CreateStorageMainWithEDocs("TRN", TransportWithConsigneeAsConsignorNovember.PK, 3);

			TransportWithConsigneeAsConsignorDecember = (BusinessObject)MasterFactory.New<ICommonCartage>();
			new JobHeader.Loader((IJobHeaderParent)TransportWithConsigneeAsConsignorDecember).TryLoadOrCreate();
			TransportWithConsigneeAsConsignorDecember["LocalClientAddressPK"] = Consignee.MainAddress.PK;
			TransportWithConsigneeAsConsignorDecember["JJ_JX_Sailing"] = DecemberSailing.PK;
			TransportWithConsigneeAsConsignorDecemberMain = CreateStorageMainWithEDocs("TRN", TransportWithConsigneeAsConsignorDecember.PK, 3);

			TransportWithNoMatchingConsigneeNovember = (BusinessObject)MasterFactory.New<ICommonCartage>();
			((JobDocAddress)TransportWithNoMatchingConsigneeNovember["FirstDocAddress"]).E2_OA_Address = addressNotConsignee.PK;
			TransportWithNoMatchingConsigneeNovember["JJ_JX_Sailing"] = NovemberSailing.PK;
			TransportWithNoMatchingConsigneeNovemberMain = CreateStorageMainWithEDocs("TRN", TransportWithNoMatchingConsigneeNovember.PK, 3);

			TransportWithNoMatchingConsigneeDecember = (BusinessObject)MasterFactory.New<ICommonCartage>();
			((JobDocAddress)TransportWithNoMatchingConsigneeDecember["FirstDocAddress"]).E2_OA_Address = addressNotConsignee.PK;
			TransportWithNoMatchingConsigneeDecember["JJ_JX_Sailing"] = DecemberSailing.PK;
			TransportWithNoMatchingConsigneeDecemberMain = CreateStorageMainWithEDocs("TRN", TransportWithNoMatchingConsigneeDecember.PK, 3);
		}

		void SetupDeclarations()
		{
			SetupOrganisations();
			DeclarationWithConsigneeNovember = (BusinessObject)MasterFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			DeclarationWithConsigneeNovember[JobDeclarationSchema.JE_OH_Importer] = Consignee.PK;
			DeclarationWithConsigneeNovember[JobDeclarationSchema.JE_ExportDate] = new ZDateTime(2003, 11, 10);
			DeclarationWithConsigneeNovember[JobDeclarationSchema.JE_DateOfArrival] = new ZDateTime(2003, 11, 15);
			DeclarationWithConsigneeNovemberMain = CreateStorageMainWithEDocs("DEC", DeclarationWithConsigneeNovember.PK, 3);

			DeclarationWithConsigneeDecember = (BusinessObject)MasterFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			DeclarationWithConsigneeDecember[JobDeclarationSchema.JE_OH_Importer] = Consignee.PK;
			DeclarationWithConsigneeDecember[JobDeclarationSchema.JE_ExportDate] = new ZDateTime(2003, 12, 10);
			DeclarationWithConsigneeDecember[JobDeclarationSchema.JE_DateOfArrival] = new ZDateTime(2003, 12, 15);
			DeclarationWithConsigneeDecemberMain = CreateStorageMainWithEDocs("DEC", DeclarationWithConsigneeDecember.PK, 3);

			DeclarationWithConsigneeAsConsignorNovember = (BusinessObject)MasterFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			DeclarationWithConsigneeAsConsignorNovember[JobDeclarationSchema.JE_OH_Supplier] = Consignee.PK;
			DeclarationWithConsigneeAsConsignorNovember[JobDeclarationSchema.JE_ExportDate] = new ZDateTime(2003, 11, 10);
			DeclarationWithConsigneeAsConsignorNovember[JobDeclarationSchema.JE_DateOfArrival] = new ZDateTime(2003, 11, 15);
			DeclarationWithConsigneeAsConsignorNovemberMain = CreateStorageMainWithEDocs("DEC", DeclarationWithConsigneeAsConsignorNovember.PK, 3);

			DeclarationWithConsigneeAsConsignorDecember = (BusinessObject)MasterFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			DeclarationWithConsigneeAsConsignorDecember[JobDeclarationSchema.JE_OH_Supplier] = Consignee.PK;
			DeclarationWithConsigneeAsConsignorDecember[JobDeclarationSchema.JE_ExportDate] = new ZDateTime(2003, 12, 10);
			DeclarationWithConsigneeAsConsignorDecember[JobDeclarationSchema.JE_DateOfArrival] = new ZDateTime(2003, 12, 15);
			DeclarationWithConsigneeAsConsignorDecemberMain = CreateStorageMainWithEDocs("DEC", DeclarationWithConsigneeAsConsignorDecember.PK, 3);

			DeclarationWithNoMatchingConsigneeNovember = (BusinessObject)MasterFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			DeclarationWithNoMatchingConsigneeNovember[JobDeclarationSchema.JE_OH_Importer] = NotConsignee.PK;
			DeclarationWithNoMatchingConsigneeNovember[JobDeclarationSchema.JE_ExportDate] = new ZDateTime(2003, 11, 10);
			DeclarationWithNoMatchingConsigneeNovember[JobDeclarationSchema.JE_DateOfArrival] = new ZDateTime(2003, 11, 15);
			DeclarationWithNoMatchingConsigneeNovemberMain = CreateStorageMainWithEDocs("DEC", DeclarationWithNoMatchingConsigneeNovember.PK, 3);

			DeclarationWithNoMatchingConsigneeDecember = (BusinessObject)MasterFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			DeclarationWithNoMatchingConsigneeDecember[JobDeclarationSchema.JE_OH_Importer] = NotConsignee.PK;
			DeclarationWithNoMatchingConsigneeDecember[JobDeclarationSchema.JE_ExportDate] = new ZDateTime(2003, 12, 10);
			DeclarationWithNoMatchingConsigneeDecember[JobDeclarationSchema.JE_DateOfArrival] = new ZDateTime(2003, 12, 15);
			DeclarationWithNoMatchingConsigneeDecemberMain = CreateStorageMainWithEDocs("DEC", DeclarationWithNoMatchingConsigneeDecember.PK, 3);
		}

		void SetupSailings()
		{
			NovemberSailing = CreateSailing(new ZDateTime(2003, 11, 10), new ZDateTime(2003, 11, 15), "NOV");
			DecemberSailing = CreateSailing(new ZDateTime(2003, 12, 10), new ZDateTime(2003, 12, 15), "DEC");
		}

		BusinessObject CreateSailing(ZDateTime eTD, ZDateTime eTA, ZString voyageID)
		{
			BusinessObject voyage = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.IJobVoyage>();
			voyage[JobVoyageSchema.JV_VoyageFlight] = voyageID;
			BusinessObject sailing = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.IJobSailing>();
			BusinessObject origin = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.IVoyageOrigin>();
			BusinessObject destination = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.IVoyageDestination>();
			origin["JA_E_DEP"] = eTD;
			origin["JA_JV"] = voyage.PK;
			destination["JB_E_ARV"] = eTA;
			destination["JB_JV"] = voyage.PK;
			sailing["JX_JA"] = origin.PK;
			sailing["JX_JB"] = destination.PK;
			return sailing;
		}

		BusinessObject CreateShipment(OrgHeader consignor, OrgHeader consignee, ZDateTime etd, ZDateTime eta)
		{
			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();

			if (consignor != null)
			{
				((JobDocAddress)shipment["ConsignorDocumentaryAddress"]).OrganisationPK = consignor.PK;
			}

			if (consignee != null)
			{
				((JobDocAddress)shipment["ConsigneeDocumentaryAddress"]).OrganisationPK = consignee.PK;
			}

			shipment[JobShipmentSchema.JS_E_DEP] = etd;
			shipment[JobShipmentSchema.JS_E_ARV] = eta;
			shipment[JobShipmentSchema.JS_IsShipping] = false;

			return shipment;
		}

		BusinessObject CreateAgencyShipment(OrgHeader consignor, OrgHeader consignee, ZDateTime etd, ZDateTime eta)
		{
			BusinessObject shipment = (BusinessObject)MasterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();

			if (consignor != null)
			{
				((JobDocAddress)shipment["ConsignorDocumentaryAddress"]).OrganisationPK = consignor.PK;
			}

			if (consignee != null)
			{
				((JobDocAddress)shipment["ConsigneeDocumentaryAddress"]).OrganisationPK = consignee.PK;
			}

			shipment[JobShipmentSchema.JS_E_DEP] = etd;
			shipment[JobShipmentSchema.JS_E_ARV] = eta;
			shipment[JobShipmentSchema.JS_IsShipping] = true;

			return shipment;
		}

		#endregion

		DocumentFactory MasterFactory;
		ArchiveEDocsManager Manager;

		OrgHeader Consignee;
		OrgHeader NotConsignee;

		BusinessObject NovemberSailing;
		BusinessObject DecemberSailing;

		BusinessObject ShipmentWithConsigneeNovember;
		BusinessObject ShipmentWithConsigneeDecember;
		BusinessObject ShipmentWithConsigneeAsConsignorNovember;
		BusinessObject ShipmentWithConsigneeAsConsignorDecember;
		BusinessObject ShipmentWithNoMatchingConsigneeNovember;
		BusinessObject ShipmentWithNoMatchingConsigneeDecember;
		StorageMain ShipmentWithConsigneeNovemberMain;
		StorageMain ShipmentWithConsigneeDecemberMain;
		StorageMain ShipmentWithConsigneeAsConsignorNovemberMain;
		StorageMain ShipmentWithConsigneeAsConsignorDecemberMain;
		StorageMain ShipmentWithNoMatchingConsigneeNovemberMain;
		StorageMain ShipmentWithNoMatchingConsigneeDecemberMain;

		BusinessObject AgencyShipmentWithConsigneeNovember;
		BusinessObject AgencyShipmentWithConsigneeDecember;
		BusinessObject AgencyShipmentWithConsigneeAsConsignorNovember;
		BusinessObject AgencyShipmentWithConsigneeAsConsignorDecember;
		BusinessObject AgencyShipmentWithNoMatchingConsigneeNovember;
		BusinessObject AgencyShipmentWithNoMatchingConsigneeDecember;
		StorageMain AgencyShipmentWithConsigneeNovemberMain;
		StorageMain AgencyShipmentWithConsigneeDecemberMain;
		StorageMain AgencyShipmentWithConsigneeAsConsignorNovemberMain;
		StorageMain AgencyShipmentWithConsigneeAsConsignorDecemberMain;
		StorageMain AgencyShipmentWithNoMatchingConsigneeNovemberMain;
		StorageMain AgencyShipmentWithNoMatchingConsigneeDecemberMain;

		BusinessObject ConsolWithConsigneeNovember;
		BusinessObject ConsolWithConsigneeDecember;
		BusinessObject ConsolWithConsigneeAsConsignorNovember;
		BusinessObject ConsolWithConsigneeAsConsignorDecember;
		BusinessObject ConsolWithNoMatchingConsigneeNovember;
		BusinessObject ConsolWithNoMatchingConsigneeDecember;
		StorageMain ConsolWithConsigneeNovemberMain;
		StorageMain ConsolWithConsigneeDecemberMain;
		StorageMain ConsolWithConsigneeAsConsignorNovemberMain;
		StorageMain ConsolWithConsigneeAsConsignorDecemberMain;
		StorageMain ConsolWithNoMatchingConsigneeNovemberMain;
		StorageMain ConsolWithNoMatchingConsigneeDecemberMain;

		BusinessObject TransportWithConsigneeNovember;
		BusinessObject TransportWithConsigneeDecember;
		BusinessObject TransportWithConsigneeAsConsignorNovember;
		BusinessObject TransportWithConsigneeAsConsignorDecember;
		BusinessObject TransportWithNoMatchingConsigneeNovember;
		BusinessObject TransportWithNoMatchingConsigneeDecember;
		StorageMain TransportWithConsigneeNovemberMain;
		StorageMain TransportWithConsigneeDecemberMain;
		StorageMain TransportWithConsigneeAsConsignorNovemberMain;
		StorageMain TransportWithConsigneeAsConsignorDecemberMain;
		StorageMain TransportWithNoMatchingConsigneeNovemberMain;
		StorageMain TransportWithNoMatchingConsigneeDecemberMain;

		BusinessObject DeclarationWithConsigneeNovember;
		BusinessObject DeclarationWithConsigneeDecember;
		BusinessObject DeclarationWithConsigneeAsConsignorNovember;
		BusinessObject DeclarationWithConsigneeAsConsignorDecember;
		BusinessObject DeclarationWithNoMatchingConsigneeNovember;
		BusinessObject DeclarationWithNoMatchingConsigneeDecember;
		StorageMain DeclarationWithConsigneeNovemberMain;
		StorageMain DeclarationWithConsigneeDecemberMain;
		StorageMain DeclarationWithConsigneeAsConsignorNovemberMain;
		StorageMain DeclarationWithConsigneeAsConsignorDecemberMain;
		StorageMain DeclarationWithNoMatchingConsigneeNovemberMain;
		StorageMain DeclarationWithNoMatchingConsigneeDecemberMain;
	}
}
