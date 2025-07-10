using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing;
using Enterprise.Customs.GB.ICS.Business;
using Enterprise.Customs.GB.ICS.CodeDescriptionPairLists;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.GB.ICS.UniversalDataTransfer.Testing;

public class SASAsycudaManifestHeaderDataObjectWriterTest : AsycudaWriterTestHelper
{
	public void TestExportVoyageFlightNo_For_RoadFreight()
	{
		using (((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator())
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
			manifestHeader.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
			manifestHeader.AMA_ManifestType = ICSManifestTypes.Codes.SAS;
			manifestHeader.AMA_TransportMode = GBSSTransportTypeList.Codes.RoadFreight;
			manifestHeader.AMA_Voyage = "VOY001";
			manifestHeader.AMA_VehicleRegistration = "REG001";

			Factory.SaveForTesting();

			var writer = new SASAsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeaderSS>(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, manifestHeader)));
			var headerData = writer.GetDataObject(manifestHeader);
			AssertEquals("REG001", headerData.VoyageFlightNo);
		}
	}

	public void TestExportVoyageFlightNo_For_RoroAccompanied()
	{
		using (((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator())
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
			manifestHeader.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
			manifestHeader.AMA_ManifestType = ICSManifestTypes.Codes.SAS;
			manifestHeader.AMA_TransportMode = GBSSTransportTypeList.Codes.RoroAccompanied;
			manifestHeader.AMA_Voyage = "VOY001";
			manifestHeader.AMA_VehicleRegistration = "REG001";

			Factory.SaveForTesting();

			var writer = new SASAsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeaderSS>(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, manifestHeader)));
			var headerData = writer.GetDataObject(manifestHeader);
			AssertEquals("REG001", headerData.VoyageFlightNo);
		}
	}

	public void TestExportVoyageFlightNo_For_RoroUnaccompanied()
	{
		using (((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator())
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
			manifestHeader.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
			manifestHeader.AMA_ManifestType = ICSManifestTypes.Codes.SAS;
			manifestHeader.AMA_TransportMode = GBSSTransportTypeList.Codes.RoroUnaccompanied;
			manifestHeader.AMA_Voyage = "VOY001";
			manifestHeader.AMA_VehicleRegistration = "REG001";

			Factory.SaveForTesting();

			var writer = new SASAsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeaderSS>(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, manifestHeader)));
			var headerData = writer.GetDataObject(manifestHeader);
			AssertEquals("REG001", headerData.VoyageFlightNo);
		}
	}

	public void TestExportVoyageFlightNo_For_Sea()
	{
		using (((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator())
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
			manifestHeader.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
			manifestHeader.AMA_ManifestType = ICSManifestTypes.Codes.SAS;
			manifestHeader.AMA_TransportMode = GBSSTransportTypeList.Codes.SeaFreight;
			manifestHeader.AMA_Voyage = "VOY001";
			manifestHeader.AMA_VehicleRegistration = "REG001";

			Factory.SaveForTesting();

			var writer = new SASAsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeaderSS>(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, manifestHeader)));
			var headerData = writer.GetDataObject(manifestHeader);
			AssertEquals("VOY001", headerData.VoyageFlightNo);
		}
	}

	public void TestExportCustomsOffices()
	{
		using (((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator())
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeaderSS>();
			manifestHeader.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.ShippingLine;
			manifestHeader.AMA_ManifestType = ICSManifestTypes.Codes.SAS;
			manifestHeader.EUCustomsOffices.RemoveAndDeleteAll();
			var office = manifestHeader.EUCustomsOffices.AddNew();
			office.CY_Code = "OOA";
			office.CY_Data = "GB000001";
			office.CY_Date = new ZDateTime(2025, 01, 01);

			Factory.SaveForTesting();

			var writer = new SASAsycudaManifestHeaderDataObjectWriter<AsycudaManifestHeaderSS>(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, manifestHeader)));
			var headerData = writer.GetDataObject(manifestHeader);
			AssertEquals(1, headerData.CustomsReferenceCollection.Count);
		}
	}
}
