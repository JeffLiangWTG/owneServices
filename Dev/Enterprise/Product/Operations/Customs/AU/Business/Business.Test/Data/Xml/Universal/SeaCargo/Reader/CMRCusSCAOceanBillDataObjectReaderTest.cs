using System.IO;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.DataTransfer.Universal.DataReaderExtensions;
using Enterprise.Customs.DataTransfer.Universal.SeaManifest.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2011_11;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRCusSCAOceanBillDataObjectReader))]
	partial class CMRCusSCAOceanBillDataObjectReaderTest : CusSCAOceanBillDataObjectReaderTest<CusSCAOceanBill, CusSCAHouse, CusSCAContainer, CusSCAPivot>
	{
		public void TestImportArrivalDateFromTransportLeg()
		{
			var logger = new DummyLogger();
			var shipment = ReadXMLIntoShipment(logger, OceanBillShipmentXML);
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.SeaOceanBill, null);
			shipment.DataContext = dataContext;
			shipment.DateCollection.Clear();
			shipment.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			var leg1 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransportMode = TransportMode.Sea,
				LegOrder = 1,
				PortOfLoading = new UNLOCO()
				{ Code = "NZAKL" },
				PortOfDischarge = new UNLOCO()
				{ Code = "USLAX" },
				ActualArrival = ZDate.Today.AddDays(-10),
			};
			var leg2 = new TransportLeg(DefaultDataObjectWriterStrategy.TestInstance)
			{
				TransportMode = TransportMode.Sea,
				LegOrder = 2,
				PortOfLoading = new UNLOCO()
				{ Code = "USLAX" },
				PortOfDischarge = new UNLOCO()
				{ Code = "AUSYD" },
				ActualArrival = ZDate.Today.AddDays(-5),
			};
			shipment.TransportLegCollection.Add(leg1);
			shipment.TransportLegCollection.Add(leg2);
			Factory.SaveForTesting();
			var message = GetQueuedUniversalShipmentMessage(shipment);
			var serviceTaskLog = new ServiceTaskLogForTesting();
			var manager = new UniversalMessageProcessingManager(serviceTaskLog);
			manager.Process(message);
			var oceanBillQuery = new ZQuery(CusSCAOceanBillSchema.CB_OceanBill, "TEST OCEAN BILL");
			var oceanBillBO = Factory.LoadTop1<CusSCAOceanBill>(oceanBillQuery);
			AssertEquals(ZDate.Today.AddDays(-5), oceanBillBO.CB_DateOfArrival);
		}

		public void TestNotUpdateOceanBillWithActiveHouseBill()
		{
			var logger = new TestErrorLogger();
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(OceanBillShipmentXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
			}

			var oceanBill = CreateOceanBillMatchingXML();
			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_HouseBill = "1";
			houseBill.CA_MessageStatus = CMRBaseStatuses.Codes.AwaitingResponseToOriginal;
			Factory.SaveForTesting();
			BusinessObject bizObj = null;
			GetReader(shipment, null, logger, Factory).ReadIntoBusinessObject(ref bizObj);
			AssertContains("Error - Cannot populate CusSCAOceanBill", logger.Logs);
		}

		public void TestUpdateCustomizedField()
		{
			var logger = new TestErrorLogger();
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(OceanBillShipmentXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
			}

			var oceanBill = CreateOceanBillMatchingXML();
			var houseBill = oceanBill.HouseBills.AddNew();
			houseBill.CA_HouseBill = "1";
			Factory.SaveForTesting();
			BusinessObject bizObj = null;
			GetReader(shipment, null, logger, Factory).ReadIntoBusinessObject(ref bizObj);
			AssertNotNull(bizObj);
			AssertEquals("oceanBill.Test1", "Test1Value", oceanBill.GetUserDefinedValue<ZString>("Test1"));
			AssertEquals("oceanBill.Test2", 111111, oceanBill.GetUserDefinedValue<ZInt>("Test2"));
			AssertEquals("houseBill.Test3", "Test3Value", houseBill.GetUserDefinedValue<ZString>("Test3"));
			AssertEquals("houseBill.Test4", "Test4Value", houseBill.GetUserDefinedValue<ZString>("Test4"));
			AssertEquals("houseBill.Test5", 222222, houseBill.GetUserDefinedValue<ZInt>("Test5"));
		}

		public void TestCheckTheFieldsIsChangedAfterCargoReporting_SkipValueSetterIfNewValueIsNull()
		{
			var logger = new TestErrorLogger();
			BusinessObject oceanBillBO = null;

			var shipment = ReadXMLIntoShipment(logger, OceanBillShipmentXML);
			GetReader(shipment, null, logger, Factory).ReadIntoBusinessObject(ref oceanBillBO);

			var existingOceanBill = oceanBillBO as CusSCAOceanBill;
			var housebill = existingOceanBill.HouseBills.AddNew();
			housebill.CA_MessageStatus = "ACA";
			Factory.SaveForTesting();

			shipment.OrganizationAddressCollection.Clear();
			shipment.AdditionalReferenceCollection.Clear();
			shipment.Branch = null;

			var responsiblePartyID = shipment.GetResponsiblePartyID(logger, Factory);
			AssertNull("Pre-condition: incoming responsible party Id is null", responsiblePartyID);

			GetReader(shipment, null, logger, Factory).ReadIntoBusinessObject(ref oceanBillBO);
			Assert("Should not add error message if new value is null", !logger.HasErrors());
		}

		public void TestCanFindExistingOceanBillWithUpdatedShipmentHouseBillNumber()
		{
			var logger = new TestErrorLogger();
			var jobShipment = Factory.New<ForwardingShipment>();
			jobShipment.JS_ShipmentType = "HVL";
			jobShipment.JS_UniqueConsignRef = "JS001";
			var genPivot = Factory.New<GenPivot>();
			genPivot.Relation1ID = jobShipment.HVLVConsignmentHeader.PK;
			genPivot.XX_Relation1TableCode = "HCH";
			genPivot.XX_RelationType = "HVL";
			var oceanBill = CreateOceanBillMatchingXML();
			genPivot.Relation2Object = oceanBill;
			Factory.SaveForTesting();

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			using (var stream = (SubStreamableStream)new MemoryStream(Encoding.UTF8.GetBytes(OceanBillShipmentXML)))
			{
				ObjectFactory.Get<IXmlReader>().ReadXML(shipment, stream, logger);
			}

			var hvlvShipperConsolidation = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			hvlvShipperConsolidation.DataContext = new DataContext();
			hvlvShipperConsolidation.DataContext.AddDataSource(DataContextType.SeaOceanBill, "JS001");
			hvlvShipperConsolidation.WayBillNumber = "HOUSEBILL2";

			BusinessObject targetBO = null;
			GetReader(shipment, hvlvShipperConsolidation, logger, Factory).ReadIntoBusinessObject(ref targetBO);
			AssertEquals("Existing Ocean Bill was updated", oceanBill, targetBO);
			AssertEquals("HOUSEBILL2", oceanBill.CB_MasterHouseBill);
		}

		protected override ITopLevelDataObjectReader GetReader(Shipment dataObject, Shipment hvlv, IXmlImportLogger logger, UniversalObjectFactory factory)
		{
			return new CMRCusSCAOceanBillDataObjectReader(dataObject, hvlv, logger, factory);
		}

		protected override ZString ApplicationCodeMatchingXML => Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

		protected override string OceanBillShipmentXML
		{
			get { return FileReader.GetEmbeddedFileText(TestFilesPath, "OceanBillShipment1.xml"); }
		}

		TestFileReader FileReader => fileReader ?? (fileReader = new TestFileReader(typeof(CMRCusSCAOceanBillDataObjectReaderTest)));
		TestFileReader fileReader;

		const string TestFilesPath = "Enterprise.Customs.AU.Declaration.Business.Testing.Data.Xml.Universal.SeaCargo.TestFiles";
	}
}
