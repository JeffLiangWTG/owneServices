using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Manifest.Business;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.ICS.UniversalDataTransfer.Testing;

class SASAsycudaManifestDataObjectReaderHelperTest : TestCaseWithUniversalObjectFactory
{
	public void TestFillCustomsOffices()
	{
		ASYCUDA.Business.Testing.ZZDataTestHelper.SetupZZ(new BusinessObjectFactory(), Core.Constants.CountryCodes.UnitedKingdom);
		var header = Factory.New<AsycudaManifestHeaderSSForTest>();
		header.AMA_TransportMode = "ROA";
		header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
		header.AMA_ManifestType = ICSManifestTypes.Codes.SAS;
		header.MasterBill.ABL_BillNumber = "MAN00001";

		var office1 = header.EUCustomsOffices.AddNew();
		office1.CY_Code = "OOF";
		office1.CY_Data = "GB000001";
		office1.CY_Date = new ZDateTime(2025, 01, 01);
		var office2 = header.EUCustomsOffices.AddNew();
		office2.CY_Code = "OOF";
		office2.CY_Data = "GB000002";
		office2.CY_Date = new ZDateTime(2025, 01, 02);
		Factory.SaveForTesting();

		var help = new AsycudaManifestDataObjectReaderTestHelper();
		var usAirLocalPort1 = help.GetAirLocalPort1(Core.Constants.CountryCodes.UnitedKingdom);
		var sgAirLocalPort1 = help.GetAirLocalPort1(Core.Constants.CountryCodes.UnitedKingdom);
		var usFirstArrival = new UNLOCO() { Code = usAirLocalPort1.RL_Code };
		var sgFirstArrival = new UNLOCO() { Code = sgAirLocalPort1.RL_Code };
		var arrivalTime = ZDateTime.Today.AddDays(1);
		var departureTime = ZDateTime.Today.AddDays(3);

		var headerDataObject = help.SetupManifestHeader("MAN00001", usFirstArrival, sgFirstArrival, arrivalTime, departureTime, "", ICSManifestTypes.Codes.SAS);
		headerDataObject.TransportMode = new CodeDescriptionPair { Code = GBSSTransportTypeList.Codes.RoadFreight, Description = GBSSTransportTypeList.Descriptions.RoadFreight };
		headerDataObject.VoyageFlightNo = "NEW001";
		headerDataObject.SetCustomsReferenceCollection(() => new List<CustomsReference>());
		var newReference = new CustomsReference {
			Type = new CodeDescriptionPair { Code = EU.Business.CusCodeDataTypeList.Codes.OfficeCode },
			SubType = new CodeDescriptionPair35Char { Code = OfficeCodes_ICS.Codes.OfficeOfSubsequentEntry },
			Reference = "GB000003",
			DateCollection = new List<Date> { new Date { Type = DateType.DateAtOffice, Value = new ZDateTime(2025, 01, 03) } }
		};
		headerDataObject.CustomsReferenceCollection.Add(newReference);

		var reader = new ASYCUDA.Business.UniversalDataTransfer.AsycudaManifestHeaderDataObjectReader(headerDataObject, new TestErrorLogger(), Factory);
		var readerHeaderBO = reader.ReadIntoBusinessObject();

		Factory.SaveForTesting();

		var headerBo = (AsycudaManifestHeaderSSForTest)readerHeaderBO;

		var query = new ZQuery(CusCodeDataSchema.CY_ParentID, headerBo.PK);
		var cusData = Factory.Load<CusCodeData>(query);

		AssertNotNull(headerBo);

		AssertEquals("EUCustomsOffices", 1, headerBo.EUCustomsOffices.Count);
		var office = headerBo.EUCustomsOffices[0];
		AssertEquals("CY_Type", EU.Business.CusCodeDataTypeList.Codes.OfficeCode, office.CY_Type);
		AssertEquals("CY_Code", OfficeCodes_ICS.Codes.OfficeOfSubsequentEntry, office.CY_Code);
		AssertEquals("CY_Data", "GB000003", office.CY_Data);
		AssertEquals("CY_Date", new ZDateTime(2025, 01, 03), office.CY_Date);
	}

	public void TestFillVoyageFlightNoCore_WhenVoyageFlightNoIsNull()
	{
		var header1 = Factory.BOFactory.New<AsycudaManifestHeaderSS>();
		header1.AMA_TransportMode = GBSSTransportTypeList.Codes.RoadFreight;
		header1.AMA_Voyage = "EXISTING";
		var helper = new SASAsycudaManifestDataObjectReaderHelper(Factory.BOFactory);
		var logger = new TestErrorLogger();
		helper.FillVoyageFlightNo(null, Core.Constants.TransportModes.Road, logger, header1);
		AssertEquals("AMA_Voyage", header1.AMA_Voyage, "EXISTING");
	}

	public void TestFillVoyageFlightNoCore_TransportMode_RoadFreight()
	{
		var header1 = Factory.BOFactory.New<AsycudaManifestHeaderSS>();
		header1.AMA_TransportMode = GBSSTransportTypeList.Codes.RoadFreight;
		var helper = new SASAsycudaManifestDataObjectReaderHelper(Factory.BOFactory);
		var logger = new TestErrorLogger();
		helper.FillVoyageFlightNo("NEW001", GBSSTransportTypeList.Codes.RoadFreight, logger, header1);
		AssertNotEquals("AMA_Voyage", header1.AMA_Voyage, "NEW001");
		AssertEquals("AMA_VehicleRegistration", header1.AMA_VehicleRegistration, "NEW001");
	}

	public void TestFillVoyageFlightNoCore_TransportMode_RoroAccompanied()
	{
		var header1 = Factory.BOFactory.New<AsycudaManifestHeaderSS>();
		header1.AMA_TransportMode = GBSSTransportTypeList.Codes.RoroAccompanied;
		var helper = new SASAsycudaManifestDataObjectReaderHelper(Factory.BOFactory);
		var logger = new TestErrorLogger();
		helper.FillVoyageFlightNo("NEW001", GBSSTransportTypeList.Codes.RoroAccompanied, logger, header1);
		AssertNotEquals("AMA_Voyage", header1.AMA_Voyage, "NEW001");
		AssertEquals("AMA_VehicleRegistration", header1.AMA_VehicleRegistration, "NEW001");
	}

	public void TestFillVoyageFlightNoCore_TransportMode_RoroUnaccompanied()
	{
		var header1 = Factory.BOFactory.New<AsycudaManifestHeaderSS>();
		header1.AMA_TransportMode = GBSSTransportTypeList.Codes.RoroUnaccompanied;
		var helper = new SASAsycudaManifestDataObjectReaderHelper(Factory.BOFactory);
		var logger = new TestErrorLogger();
		helper.FillVoyageFlightNo("NEW001", GBSSTransportTypeList.Codes.RoroUnaccompanied, logger, header1);
		AssertNotEquals("AMA_Voyage", header1.AMA_Voyage, "NEW001");
		AssertEquals("AMA_VehicleRegistration", header1.AMA_VehicleRegistration, "NEW001");
	}

	public void TestFillVoyageFlightNoCore_TransportMode_SeaFreight()
	{
		var header1 = Factory.BOFactory.New<AsycudaManifestHeaderSS>();
		header1.AMA_TransportMode = GBSSTransportTypeList.Codes.SeaFreight;
		var helper = new SASAsycudaManifestDataObjectReaderHelper(Factory.BOFactory);
		var logger = new TestErrorLogger();
		helper.FillVoyageFlightNo("NEW001", GBSSTransportTypeList.Codes.SeaFreight, logger, header1);
		AssertEquals("AMA_Voyage", header1.AMA_Voyage, "NEW001");
		AssertNotEquals("AMA_VehicleRegistration", header1.AMA_VehicleRegistration, "NEW001");
	}

	public void TestNoExceptionWhenNoCustomsReferenceCollection()
	{
		var header = Factory.BOFactory.New<AsycudaManifestHeaderSS>();
		var helper = new SASAsycudaManifestDataObjectReaderHelper(Factory.BOFactory);
		var logger = new TestErrorLogger();
		var shipment = new Shipment();
		AssertNull("Pre-requisite", shipment.CustomsReferenceCollection);
		AssertNoExceptionThrown("FillManifestSpecificData", () => helper.FillManifestSpecificData(shipment, logger, header, Factory));
	}

	public class AsycudaManifestHeaderSSForTest : AsycudaManifestHeaderSS
	{
		public AsycudaManifestHeaderSSForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[ChildEditable(true)]
		public new IcsOfficeCodeCollection EUCustomsOffices => GetCustomsOffices();
	}
}
