using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.SDF;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.AE.Business.Testing;

public class ManifestMapperTest : TestCaseWithFactory
{
	[TestDate(2013, 4, 16)]
	public void TestMapInstalmentNumber()
	{
		ZString expectedImport = @"""VOY"",""LCO"",""ACOD1"",""Vessel1"",""FL111"",""AEZZZ"",""16-Apr-2013"",""Bookin"",""MFI"",""2"",""1""
""BOL"",""HOUSEBILL01"",""LCO"",""ACOD1"",""ISZZZ"",""ISZZZ"",""AEZZZ"",""UAZZZ"",""16-Apr-2013"",""5BXH3P60"",""T"",""S"","""","""",""F"",""N"","""",""FCL/FCL"",""IS"","""","""","""","""","""","""","""",""Mr Consignee"",""CONSIGNEE ADDRESS"",""AE"",""CCD1"",""Mr Consignor"",""CONSIGNOR ADDRESS ICELAND"",""CCD3"",""Mr Notify"",""NOTIFY ADDRESS UKRAINE"","""","""","""","""","""","""",""shipment1 marks and numbers"",""HCode1"",""PackLine1 Description"",""60"",""Package"",""PKG"","""","""",""2"",""3"",""8.5"",""333.000"",""8924.000"",""33.000"",""150"",""1111.110"",""10"",""N"",""FOB"",""""
""CTR"",""CTR0199999"",""9"",""5.5"",""SEAL01""
""CON"",""1"",""PackLine1 Marks and Numbers"",""PackLine1 Description"","""",""HCode1"",""10"",""Pallet"",""PLT"",""10"",""111.000"",""11.000"",""Y"",""IMO"",""DG001"",""+111"",""C"","""",""N"",""0"",""0"",""C""
""CON"",""2"",""PackLine2 Marks and Numbers"",""PackLine2 Description"","""",""HCode2"",""20"",""Drum"",""DRM"",""0"",""111.000"",""11.000"",""Y"",""IMO"",""DG001"",""+122"",""C"","""",""N"",""0"",""0"",""C""
""CTR"",""CTR0299999"",""9"",""3.0"",""SEAL02""
""CON"",""1"",""PackLine3 Marks and Numbers"",""PackLine3 Description"","""",""HCode3"",""30"",""Package"",""PKG"",""0"",""111.000"",""11.000"",""Y"",""IMO"",""DG001"",""+133"","""","""",""N"",""0"",""0"",""""
""BOL"",""HOUSEBILL02"",""LCO"",""ACOD1"",""UAZZZ"",""ISZZZ"",""AEZZZ"",""AEZZZ"",""16-Apr-2013"",""HS7UO25M"",""I"","""","""","""",""L"",""Y"","""",""LCL/LCL"",""UA"","""","""","""","""","""","""","""",""Mr Consignee"",""CONSIGNEE ADDRESS"",""AE"",""CCD1"",""Mr Consignor"",""CONSIGNOR ADDRESS ICELAND"",""CCD3"",""Mr Notify"",""NOTIFY ADDRESS UKRAINE"","""","""","""","""","""","""",""shipment2 marks and numbers"",""HCode4"",""PackLine4 Description"",""90"",""Package"",""PKG"",""CTR0199999"",""9"",""2"",""4"",""11.1"",""222.000"",""0.000"",""22.000"",""99"",""1001.010"",""0"",""N"",""XXX"",""""
""CTR"",""CTR0199999"",""9"",""5.5"",""SEAL01""
""CON"",""1"",""PackLine5 Marks and Numbers"",""PackLine5 Description"","""",""HCode5"",""50"",""Cradle"",""CRD"",""0"",""111.000"",""11.000"",""Y"",""IMO"",""DG002"",""+155"",""C"","""",""N"",""0"",""0"",""C""
""CTR"",""CTR0399999"",""9"",""5.5"",""SEAL03""
""CON"",""1"",""PackLine4 Marks and Numbers"",""PackLine4 Description"","""",""HCode4"",""40"",""Keg"",""KEG"",""0"",""111.000"",""11.000"",""Y"",""IMO"",""DG001"",""+144"",""C"","""",""N"",""0"",""0"",""C""
""END"",""2"","""",""""";
		RefUNLOCO load = CreateNewPort(Constants.CountryCodes.Iceland);
		RefUNLOCO discharge = CreateNewPort(Constants.CountryCodes.UnitedArabEmirates);
		var consol = CreateConsolForTest(load, discharge);
		var num = consol.Numbers.AddNew();
		num.CE_EntryType = UnitedArabEmiratesAdditionalReferenceNumberTypes.Codes.UAEInstalmentNumber;
		num.CE_EntryNum = "2";
		AssertMap(consol, expectedImport);
	}

	[TestDate(2008, 8, 8)]
	public void TestMapImport()
	{
		ZString expectedImport = @"""VOY"",""LCO"",""ACOD1"",""Vessel1"",""FL111"",""AEZZZ"",""08-Aug-2008"",""Bookin"",""MFI"",""1"",""1""
""BOL"",""HOUSEBILL01"",""LCO"",""ACOD1"",""ISZZZ"",""ISZZZ"",""AEZZZ"",""UAZZZ"",""08-Aug-2008"",""5BXH3P60"",""T"",""S"","""","""",""F"",""N"","""",""FCL/FCL"",""IS"","""","""","""","""","""","""","""",""Mr Consignee"",""CONSIGNEE ADDRESS"",""AE"",""CCD1"",""Mr Consignor"",""CONSIGNOR ADDRESS ICELAND"",""CCD3"",""Mr Notify"",""NOTIFY ADDRESS UKRAINE"","""","""","""","""","""","""",""shipment1 marks and numbers"",""HCode1"",""PackLine1 Description"",""60"",""Package"",""PKG"","""","""",""2"",""3"",""8.5"",""333.000"",""8924.000"",""33.000"",""150"",""1111.110"",""10"",""N"",""FOB"",""""
""CTR"",""CTR0199999"",""9"",""5.5"",""SEAL01""
""CON"",""1"",""PackLine1 Marks and Numbers"",""PackLine1 Description"","""",""HCode1"",""10"",""Pallet"",""PLT"",""10"",""111.000"",""11.000"",""Y"",""IMO"",""DG001"",""+111"",""C"","""",""N"",""0"",""0"",""C""
""CON"",""2"",""PackLine2 Marks and Numbers"",""PackLine2 Description"","""",""HCode2"",""20"",""Drum"",""DRM"",""0"",""111.000"",""11.000"",""Y"",""IMO"",""DG001"",""+122"",""C"","""",""N"",""0"",""0"",""C""
""CTR"",""CTR0299999"",""9"",""3.0"",""SEAL02""
""CON"",""1"",""PackLine3 Marks and Numbers"",""PackLine3 Description"","""",""HCode3"",""30"",""Package"",""PKG"",""0"",""111.000"",""11.000"",""Y"",""IMO"",""DG001"",""+133"","""","""",""N"",""0"",""0"",""""
""BOL"",""HOUSEBILL02"",""LCO"",""ACOD1"",""UAZZZ"",""ISZZZ"",""AEZZZ"",""AEZZZ"",""08-Aug-2008"",""HS7UO25M"",""I"","""","""","""",""L"",""Y"","""",""LCL/LCL"",""UA"","""","""","""","""","""","""","""",""Mr Consignee"",""CONSIGNEE ADDRESS"",""AE"",""CCD1"",""Mr Consignor"",""CONSIGNOR ADDRESS ICELAND"",""CCD3"",""Mr Notify"",""NOTIFY ADDRESS UKRAINE"","""","""","""","""","""","""",""shipment2 marks and numbers"",""HCode4"",""PackLine4 Description"",""90"",""Package"",""PKG"",""CTR0199999"",""9"",""2"",""4"",""11.1"",""222.000"",""0.000"",""22.000"",""99"",""1001.010"",""0"",""N"",""XXX"",""""
""CTR"",""CTR0199999"",""9"",""5.5"",""SEAL01""
""CON"",""1"",""PackLine5 Marks and Numbers"",""PackLine5 Description"","""",""HCode5"",""50"",""Cradle"",""CRD"",""0"",""111.000"",""11.000"",""Y"",""IMO"",""DG002"",""+155"",""C"","""",""N"",""0"",""0"",""C""
""CTR"",""CTR0399999"",""9"",""5.5"",""SEAL03""
""CON"",""1"",""PackLine4 Marks and Numbers"",""PackLine4 Description"","""",""HCode4"",""40"",""Keg"",""KEG"",""0"",""111.000"",""11.000"",""Y"",""IMO"",""DG001"",""+144"",""C"","""",""N"",""0"",""0"",""C""
""END"",""2"","""",""""";
		RefUNLOCO load = CreateNewPort(Constants.CountryCodes.Iceland);
		RefUNLOCO dishcarge = CreateNewPort(Constants.CountryCodes.UnitedArabEmirates);
		AssertMap(CreateConsolForTest(load, dishcarge), expectedImport);
	}

	[TestDate(2008, 8, 8)]
	public void TestIncoTermMapping()
	{
		ZString expected = @"""VOY"",""LCO"",""ACOD1"",""Vessel1"",""FL111"",""AEZZZ"",""08-Aug-2008"",""Bookin"",""MFI"",""1"",""1""
""BOL"",""HOUSEBILL01"",""LCO"",""ACOD1"",""ISZZZ"",""ISZZZ"",""AEZZZ"",""UAZZZ"",""08-Aug-2008"",""5BXH3P60"",""T"",""S"","""","""",""F"",""N"","""",""FCL/FCL"",""IS"","""","""","""","""","""","""","""",""Mr Consignee"",""CONSIGNEE ADDRESS"",""AE"",""CCD1"",""Mr Consignor"",""CONSIGNOR ADDRESS ICELAND"",""CCD3"",""Mr Notify"",""NOTIFY ADDRESS UKRAINE"","""","""","""","""","""","""",""shipment1 marks and numbers"",""HCode1"",""PackLine1 Description"",""60"",""Package"",""PKG"","""","""",""2"",""3"",""8.5"",""333.000"",""8924.000"",""33.000"",""150"",""1111.110"",""10"",""N"",""C&F"",""""
""CTR"",""CTR0199999"",""9"",""5.5"",""SEAL01""
""CON"",""1"",""PackLine1 Marks and Numbers"",""PackLine1 Description"","""",""HCode1"",""10"",""Pallet"",""PLT"",""10"",""111.000"",""11.000"",""Y"",""IMO"",""DG001"",""+111"",""C"","""",""N"",""0"",""0"",""C""
""CON"",""2"",""PackLine2 Marks and Numbers"",""PackLine2 Description"","""",""HCode2"",""20"",""Drum"",""DRM"",""0"",""111.000"",""11.000"",""Y"",""IMO"",""DG001"",""+122"",""C"","""",""N"",""0"",""0"",""C""
""CTR"",""CTR0299999"",""9"",""3.0"",""SEAL02""
""CON"",""1"",""PackLine3 Marks and Numbers"",""PackLine3 Description"","""",""HCode3"",""30"",""Package"",""PKG"",""0"",""111.000"",""11.000"",""Y"",""IMO"",""DG001"",""+133"","""","""",""N"",""0"",""0"",""""
""BOL"",""HOUSEBILL02"",""LCO"",""ACOD1"",""UAZZZ"",""ISZZZ"",""AEZZZ"",""AEZZZ"",""08-Aug-2008"",""HS7UO25M"",""I"","""","""","""",""L"",""Y"","""",""LCL/LCL"",""UA"","""","""","""","""","""","""","""",""Mr Consignee"",""CONSIGNEE ADDRESS"",""AE"",""CCD1"",""Mr Consignor"",""CONSIGNOR ADDRESS ICELAND"",""CCD3"",""Mr Notify"",""NOTIFY ADDRESS UKRAINE"","""","""","""","""","""","""",""shipment2 marks and numbers"",""HCode4"",""PackLine4 Description"",""90"",""Package"",""PKG"",""CTR0199999"",""9"",""2"",""4"",""11.1"",""222.000"",""0.000"",""22.000"",""99"",""1001.010"",""0"",""N"",""FOB"",""""
""CTR"",""CTR0199999"",""9"",""5.5"",""SEAL01""
""CON"",""1"",""PackLine5 Marks and Numbers"",""PackLine5 Description"","""",""HCode5"",""50"",""Cradle"",""CRD"",""0"",""111.000"",""11.000"",""Y"",""IMO"",""DG002"",""+155"",""C"","""",""N"",""0"",""0"",""C""
""CTR"",""CTR0399999"",""9"",""5.5"",""SEAL03""
""CON"",""1"",""PackLine4 Marks and Numbers"",""PackLine4 Description"","""",""HCode4"",""40"",""Keg"",""KEG"",""0"",""111.000"",""11.000"",""Y"",""IMO"",""DG001"",""+144"",""C"","""",""N"",""0"",""0"",""C""
""END"",""2"","""",""""";
		RefUNLOCO load = CreateNewPort(Constants.CountryCodes.Iceland);
		RefUNLOCO dishcarge = CreateNewPort(Constants.CountryCodes.UnitedArabEmirates);
		var consol = CreateConsolForTest(load, dishcarge);
		consol.Shipments[0].JS_INCO = Constants.IncoTerms.CostAndFreight;
		consol.Shipments[1].JS_INCO = Constants.IncoTerms.FreeOnBoard;
		AssertMap(consol, expected);
		expected = @"""VOY"",""LCO"",""ACOD1"",""Vessel1"",""FL111"",""AEZZZ"",""08-Aug-2008"",""Bookin"",""MFI"",""1"",""1""
""BOL"",""HOUSEBILL01"",""LCO"",""ACOD1"",""ISZZZ"",""ISZZZ"",""AEZZZ"",""UAZZZ"",""08-Aug-2008"",""5BXH3P60"",""T"",""S"","""","""",""F"",""N"","""",""FCL/FCL"",""IS"","""","""","""","""","""","""","""",""Mr Consignee"",""CONSIGNEE ADDRESS"",""AE"",""CCD1"",""Mr Consignor"",""CONSIGNOR ADDRESS ICELAND"",""CCD3"",""Mr Notify"",""NOTIFY ADDRESS UKRAINE"","""","""","""","""","""","""",""shipment1 marks and numbers"",""HCode1"",""PackLine1 Description"",""60"",""Package"",""PKG"","""","""",""2"",""3"",""8.5"",""333.000"",""8924.000"",""33.000"",""150"",""1111.110"",""10"",""N"",""CIF"",""""
""CTR"",""CTR0199999"",""9"",""5.5"",""SEAL01""
""CON"",""1"",""PackLine1 Marks and Numbers"",""PackLine1 Description"","""",""HCode1"",""10"",""Pallet"",""PLT"",""10"",""111.000"",""11.000"",""Y"",""IMO"",""DG001"",""+111"",""C"","""",""N"",""0"",""0"",""C""
""CON"",""2"",""PackLine2 Marks and Numbers"",""PackLine2 Description"","""",""HCode2"",""20"",""Drum"",""DRM"",""0"",""111.000"",""11.000"",""Y"",""IMO"",""DG001"",""+122"",""C"","""",""N"",""0"",""0"",""C""
""CTR"",""CTR0299999"",""9"",""3.0"",""SEAL02""
""CON"",""1"",""PackLine3 Marks and Numbers"",""PackLine3 Description"","""",""HCode3"",""30"",""Package"",""PKG"",""0"",""111.000"",""11.000"",""Y"",""IMO"",""DG001"",""+133"","""","""",""N"",""0"",""0"",""""
""BOL"",""HOUSEBILL02"",""LCO"",""ACOD1"",""UAZZZ"",""ISZZZ"",""AEZZZ"",""AEZZZ"",""08-Aug-2008"",""HS7UO25M"",""I"","""","""","""",""L"",""Y"","""",""LCL/LCL"",""UA"","""","""","""","""","""","""","""",""Mr Consignee"",""CONSIGNEE ADDRESS"",""AE"",""CCD1"",""Mr Consignor"",""CONSIGNOR ADDRESS ICELAND"",""CCD3"",""Mr Notify"",""NOTIFY ADDRESS UKRAINE"","""","""","""","""","""","""",""shipment2 marks and numbers"",""HCode4"",""PackLine4 Description"",""90"",""Package"",""PKG"",""CTR0199999"",""9"",""2"",""4"",""11.1"",""222.000"",""0.000"",""22.000"",""99"",""1001.010"",""0"",""N"",""XXX"",""""
""CTR"",""CTR0199999"",""9"",""5.5"",""SEAL01""
""CON"",""1"",""PackLine5 Marks and Numbers"",""PackLine5 Description"","""",""HCode5"",""50"",""Cradle"",""CRD"",""0"",""111.000"",""11.000"",""Y"",""IMO"",""DG002"",""+155"",""C"","""",""N"",""0"",""0"",""C""
""CTR"",""CTR0399999"",""9"",""5.5"",""SEAL03""
""CON"",""1"",""PackLine4 Marks and Numbers"",""PackLine4 Description"","""",""HCode4"",""40"",""Keg"",""KEG"",""0"",""111.000"",""11.000"",""Y"",""IMO"",""DG001"",""+144"",""C"","""",""N"",""0"",""0"",""C""
""END"",""2"","""",""""";
		consol.Shipments[0].JS_INCO = Constants.IncoTerms.CostInsuranceAndFreight;
		consol.Shipments[1].JS_INCO = Constants.IncoTerms.DeliveredDutyPaid;
		AssertMap(consol, expected);
	}

	[TestDate(2008, 8, 8)]
	public void TestParties()
	{
		ZString expected = "\"VOY\",\"LCO\",\"ACOD1\",\"Vessel1\",\"FL111\",\"AEZZZ\",\"08-Aug-2008\",\"Bookin\",\"MFI\",\"1\",\"1\"\r\n\"BOL\",\"HOUSEBILL01\",\"LCO\",\"ACOD1\",\"ISZZZ\",\"ISZZZ\",\"AEZZZ\",\"UAZZZ\",\"08-Aug-2008\",\"5BXH3P60\",\"T\",\"S\",\"\",\"\",\"F\",\"N\",\"\",\"FCL/FCL\",\"IS\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"CONSIGNOR1\",\"CONSIGNORADDRESS1\",\"AE\",\"\",\"CONSIGNEE1\",\"CONSIGNEEADDRESS1\",\"\",\"NOTIFY1\",\"NOTIFYADDRESS1\",\"\",\"\",\"\",\"\",\"\",\"\",\"shipment1 marks and numbers\",\"\",\"\",\"0\",\"Pallet\",\"PLT\",\"\",\"\",\"0\",\"0\",\"0.0\",\"0.000\",\"0.000\",\"0.000\",\"0\",\"0.000\",\"0\",\"N\",\"FOB\",\"\"\r\n\"END\",\"1\",\"\",\"\"";
		var load = CreateNewPort(Constants.CountryCodes.Iceland);
		var discharge = CreateNewPort(Constants.CountryCodes.UnitedArabEmirates);
		var origin = CreateNewPort(Constants.CountryCodes.Ukraine);
		var consol = CreateNewConsol("LCO", "ACOD1", Constants.TransportModes.Sea, load.Code, discharge.Code, "Vessel1", "FL111", ZDateTime.Now, "BookingRef");
		var shipment1 = CreateNewShipment(consol, "HOUSEBILL01", null, null, null, load.Code, origin.Code, ZDateTime.Now, Constants.ContainerModes.FCL, "shipment1 marks and numbers", 2000, Constants.IncoTerms.FreeOnBoard);
		Factory.Save();
		consol.Shipments[0].ConsigneeDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
		consol.Shipments[0].ConsigneeDocumentaryAddress.E2_AddressOverride = true;
		consol.Shipments[0].ConsigneeDocumentaryAddress.E2_CompanyName = "CONSIGNEE1";
		consol.Shipments[0].ConsigneeDocumentaryAddress.E2_Address1 = "CONSIGNEEADDRESS1";
		consol.Shipments[0].ConsignorDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
		consol.Shipments[0].ConsignorDocumentaryAddress.E2_AddressOverride = true;
		consol.Shipments[0].ConsignorDocumentaryAddress.E2_CompanyName = "CONSIGNOR1";
		consol.Shipments[0].ConsignorDocumentaryAddress.E2_Address1 = "CONSIGNORADDRESS1";
		consol.Shipments[0].NotifyPartyDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
		consol.Shipments[0].NotifyPartyDocumentaryAddress.E2_AddressOverride = true;
		consol.Shipments[0].NotifyPartyDocumentaryAddress.E2_CompanyName = "NOTIFY1";
		consol.Shipments[0].NotifyPartyDocumentaryAddress.E2_Address1 = "NOTIFYADDRESS1";
		AssertMap(consol, expected);
	}

	public void TestMapWithoutData()
	{
		ZString expectedWithoutData = "\"VOY\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"MFI\",\"1\",\"0\"\r\n\"BOL\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"T\",\"\",\"\",\"\",\"L\",\"N\",\"\",\"LCL/LCL\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"0\",\"Pallet\",\"PLT\",\"\",\"\",\"1\",\"0\",\"0.0\",\"0.000\",\"0.000\",\"0.000\",\"0\",\"0.000\",\"0\",\"N\",\"\",\"\"\r\n\"CTR\",\"\",\"\",\"0.0\",\"\"\r\n\"CON\",\"1\",\"\",\"\",\"\",\"\",\"0\",\"Pallet\",\"PLT\",\"0\",\"0.000\",\"0.000\",\"N\",\"\",\"\",\"0\",\"\",\"D\",\"N\",\"0\",\"0\",\"\"\r\n\"END\",\"1\",\"\",\"\"";
		ForwardingConsol consol = Factory.New<ForwardingConsol>();
		ForwardingShipment shipment = consol.Shipments.AddNew();
		CommonContainer c = consol.Containers.AddNew();
		shipment.OuterPackLines.AddNew().Containers.Add(c);
		ManifestLine root = new ManifestMapper().Map(consol);
		AssertEquals("Mapped Consol", expectedWithoutData, root.ToString().Trim());
		AssertNotNull(root);
	}

	[TestDate(2008, 8, 8)]
	public void TestMapTransShipment()
	{
		ZString expectedTransShipment = @"""VOY"",""LCO"",""ACOD1"",""Vessel1"",""FL111"",""AEZZZ"",""08-Aug-2008"",""Bookin"",""MFI"",""1"",""1""
""BOL"",""HOUSEBILL01"",""LCO"",""ACOD1"",""ISZZZ"",""ISZZZ"",""AEZZZ"",""UAZZZ"",""08-Aug-2008"",""5BXH3P60"",""T"",""S"","""","""",""F"",""N"","""",""FCL/FCL"",""IS"","""","""","""","""","""","""","""",""Mr Consignee"",""CONSIGNEE ADDRESS"",""AE"",""CCD1"",""Mr Consignor"",""CONSIGNOR ADDRESS ICELAND"",""CCD3"",""Mr Notify"",""NOTIFY ADDRESS UKRAINE"","""","""","""","""","""","""",""shipment1 marks and numbers"",""HCode1"",""PackLine1 Description"",""60"",""Package"",""PKG"","""","""",""2"",""3"",""8.5"",""333.000"",""8924.000"",""33.000"",""150"",""1111.110"",""10"",""N"",""FOB"",""""
""CTR"",""CTR0199999"",""9"",""5.5"",""SEAL01""
""CON"",""1"",""PackLine1 Marks and Numbers"",""PackLine1 Description"","""",""HCode1"",""10"",""Pallet"",""PLT"",""10"",""111.000"",""11.000"",""Y"",""IMO"",""DG001"",""+111"",""C"","""",""N"",""0"",""0"",""C""
""CON"",""2"",""PackLine2 Marks and Numbers"",""PackLine2 Description"","""",""HCode2"",""20"",""Drum"",""DRM"",""0"",""111.000"",""11.000"",""Y"",""IMO"",""DG001"",""+122"",""C"","""",""N"",""0"",""0"",""C""
""CTR"",""CTR0299999"",""9"",""3.0"",""SEAL02""
""CON"",""1"",""PackLine3 Marks and Numbers"",""PackLine3 Description"","""",""HCode3"",""30"",""Package"",""PKG"",""0"",""111.000"",""11.000"",""Y"",""IMO"",""DG001"",""+133"","""","""",""N"",""0"",""0"",""""
""BOL"",""HOUSEBILL02"",""LCO"",""ACOD1"",""UAZZZ"",""ISZZZ"",""AEZZZ"",""AEZZZ"",""08-Aug-2008"",""HS7UO25M"",""I"","""","""","""",""L"",""Y"","""",""LCL/LCL"",""UA"","""","""","""","""","""","""","""",""Mr Consignee"",""CONSIGNEE ADDRESS"",""AE"",""CCD1"",""Mr Consignor"",""CONSIGNOR ADDRESS ICELAND"",""CCD3"",""Mr Notify"",""NOTIFY ADDRESS UKRAINE"","""","""","""","""","""","""",""shipment2 marks and numbers"",""HCode4"",""PackLine4 Description"",""90"",""Package"",""PKG"",""CTR0199999"",""9"",""2"",""4"",""11.1"",""222.000"",""0.000"",""22.000"",""99"",""1001.010"",""0"",""N"",""XXX"",""""
""CTR"",""CTR0199999"",""9"",""5.5"",""SEAL01""
""CON"",""1"",""PackLine5 Marks and Numbers"",""PackLine5 Description"","""",""HCode5"",""50"",""Cradle"",""CRD"",""0"",""111.000"",""11.000"",""Y"",""IMO"",""DG002"",""+155"",""C"","""",""N"",""0"",""0"",""C""
""CTR"",""CTR0399999"",""9"",""5.5"",""SEAL03""
""CON"",""1"",""PackLine4 Marks and Numbers"",""PackLine4 Description"","""",""HCode4"",""40"",""Keg"",""KEG"",""0"",""111.000"",""11.000"",""Y"",""IMO"",""DG001"",""+144"",""C"","""",""N"",""0"",""0"",""C""
""END"",""2"","""",""""";
		var load = CreateNewPort(Constants.CountryCodes.Iceland);
		var discharge = CreateNewPort(Constants.CountryCodes.UnitedArabEmirates);
		var origin = CreateNewPort(Constants.CountryCodes.Ukraine);
		var consignor = CreateNewOrg("Mr Consignor", "Consignor address", load.Code, "CCD1");
		var consignee = CreateNewOrg("Mr Consignee", "Consignee address", discharge.Code, "CCD2");
		var notify = CreateNewOrg("Mr Notify", "Notify address", origin.Code, "CCD3");
		var consol = CreateNewConsol("LCO", "ACOD1", Constants.TransportModes.Sea, load.Code, discharge.Code, "Vessel1", "FL111", ZDateTime.Now, "BookingRef");
		var shipment1 = CreateNewShipment(consol, "HOUSEBILL01", consignor, consignee, notify, load.Code, origin.Code, ZDateTime.Now, Constants.ContainerModes.FCL, "shipment1 marks and numbers", 2000, Constants.IncoTerms.FreeOnBoard);
		var shipment2 = CreateNewShipment(consol, "HOUSEBILL02", consignor, consignee, notify, origin.Code, discharge.Code, ZDateTime.Now, Constants.ContainerModes.LCL, "shipment2 marks and numbers", 1100m, Constants.IncoTerms.FreeCarrier);
		var container1 = CreateNewContainer(consol, GetRefContainer("40FR"), "CTR01", 900m, "SEAL01", 36.6m, false);
		var container2 = CreateNewContainer(consol, GetRefContainer("20NOR"), "CTR02", 1100m, "SEAL02", -50.0m, false);
		var container3 = CreateNewContainer(consol, GetRefContainer("40FR"), "CTR03", 3330m, "SEAL03", 0, true);
		var subs = Factory.New<UNDGSubstance>();
		subs.DG_UNNO = "DG00";
		subs.DG_Variant = "1";
		subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
		var subs2 = Factory.New<UNDGSubstance>();
		subs2.DG_UNNO = "DG00";
		subs2.DG_Variant = "2";
		subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
		var line11 = CreateNewPackLine(shipment1, "DG001", "IMO1", 111m, "PackLine1 Marks and Numbers", "PackLine1 Description", "HCode1", 10, Constants.PkgUnit.Pallet);
		var line12 = CreateNewPackLine(shipment1, "DG001", "IMO2", 122m, "PackLine2 Marks and Numbers", "PackLine2 Description", "HCode2", 20, Constants.PkgUnit.Drum);
		var line13 = CreateNewPackLine(shipment1, "DG001", "IMO3", 133m, "PackLine3 Marks and Numbers", "PackLine3 Description", "HCode3", 30, Constants.PkgUnit.Package);
		var line21 = CreateNewPackLine(shipment2, "DG001", "IMO4", 144m, "PackLine4 Marks and Numbers", "PackLine4 Description", "HCode4", 40, Constants.PkgUnit.Keg);
		var line22 = CreateNewPackLine(shipment2, "DG002", "IMO5", 155m, "PackLine5 Marks and Numbers", "PackLine5 Description", "HCode5", 50, Constants.PkgUnit.Cradle);
		line11.Containers.RemoveAll();
		line12.Containers.RemoveAll();
		line13.Containers.RemoveAll();
		line21.Containers.RemoveAll();
		line22.Containers.RemoveAll();
		line11.Containers.Add(container1);
		line12.Containers.Add(container1);
		line13.Containers.Add(container2);
		line21.Containers.Add(container3);
		line22.Containers.Add(container1);
		shipment1.JS_OuterPacks = 150;
		shipment2.JS_OuterPacks = 99;
		shipment1.JS_ActualChargeable = 1111.11m;
		shipment2.JS_ActualChargeable = 1001.01m;
		Factory.Save();
		AssertMap(consol, expectedTransShipment);
	}

	[TestDate(2013, 01, 01)]
	public void TestIsTransShipment()
	{
		ZString expectedTransShipment = "\"VOY\",\"LCO\",\"ACOD2\",\"Vessel1\",\"FL111\",\"AEJEA\",\"01-Jan-2013\",\"Bookin\",\"MFI\",\"1\",\"1\"\n\"BOL\",\"HOUSEBILL01\",\"LCO\",\"ACOD2\",\"USLAX\",\"USLAX\",\"AEJEA\",\"AEJEA\",\"01-Jan-2013\",\"5BXH3P60\",\"I\",\"\",\"\",\"\",\"F\",\"N\",\"\",\"FCL/FCL\",\"US\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"Mr Consignee\",\"CONSIGNEE ADDRESS\",\"AE\",\"CCD1\",\"Mr Consignor\",\"CONSIGNOR ADDRESS CA UNITED STATES\",\"CCD3\",\"Mr Notify\",\"NOTIFY ADDRESS\",\"\",\"\",\"\",\"\",\"\",\"\",\"shipment1 marks and numbers\",\"\",\"\",\"0\",\"Pallet\",\"PLT\",\"\",\"\",\"0\",\"0\",\"0.0\",\"0.000\",\"0.000\",\"0.000\",\"0\",\"0.000\",\"0\",\"N\",\"FOB\",\"\"\n\"BOL\",\"HOUSEBILL02\",\"LCO\",\"ACOD2\",\"USLAX\",\"USLAX\",\"AEJEA\",\"AESHJ\",\"01-Jan-2013\",\"HS7UO25M\",\"I\",\"\",\"\",\"\",\"L\",\"N\",\"\",\"LCL/LCL\",\"US\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"Mr Consignee\",\"CONSIGNEE ADDRESS\",\"AE\",\"CCD1\",\"Mr Consignor\",\"CONSIGNOR ADDRESS CA UNITED STATES\",\"CCD3\",\"Mr Notify\",\"NOTIFY ADDRESS\",\"\",\"\",\"\",\"\",\"\",\"\",\"shipment2 marks and numbers\",\"\",\"\",\"0\",\"Pallet\",\"PLT\",\"\",\"\",\"0\",\"0\",\"0.0\",\"0.000\",\"0.000\",\"0.000\",\"0\",\"0.000\",\"0\",\"N\",\"XXX\",\"\"\n\"BOL\",\"HOUSEBILL03\",\"LCO\",\"ACOD2\",\"USLAX\",\"USLAX\",\"AEJEA\",\"AESHJ\",\"01-Jan-2013\",\"SAJ7AG48\",\"T\",\"S\",\"\",\"\",\"L\",\"N\",\"\",\"LCL/LCL\",\"US\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"Mr Consignee\",\"CONSIGNEE ADDRESS\",\"AE\",\"CCD1\",\"Mr Consignor\",\"CONSIGNOR ADDRESS CA UNITED STATES\",\"CCD3\",\"Mr Notify\",\"NOTIFY ADDRESS\",\"\",\"\",\"\",\"\",\"\",\"\",\"shipment2 marks and numbers\",\"\",\"\",\"0\",\"Pallet\",\"PLT\",\"\",\"\",\"0\",\"0\",\"0.0\",\"0.000\",\"0.000\",\"0.000\",\"0\",\"0.000\",\"0\",\"N\",\"XXX\",\"\"\n\"BOL\",\"HOUSEBILL04\",\"LCO\",\"ACOD2\",\"USLAX\",\"USLAX\",\"AEJEA\",\"AESHJ\",\"01-Jan-2013\",\"3RTLUU3U\",\"I\",\"\",\"\",\"\",\"L\",\"N\",\"\",\"LCL/LCL\",\"US\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"Mr Consignee\",\"CONSIGNEE ADDRESS\",\"AE\",\"CCD1\",\"Mr Consignor\",\"CONSIGNOR ADDRESS CA UNITED STATES\",\"CCD3\",\"Mr Notify\",\"NOTIFY ADDRESS\",\"\",\"\",\"\",\"\",\"\",\"\",\"shipment2 marks and numbers\",\"\",\"\",\"0\",\"Pallet\",\"PLT\",\"\",\"\",\"0\",\"0\",\"0.0\",\"0.000\",\"0.000\",\"0.000\",\"0\",\"0.000\",\"0\",\"N\",\"XXX\",\"\"\n\"BOL\",\"HOUSEBILL05\",\"LCO\",\"ACOD2\",\"USLAX\",\"USLAX\",\"AEJEA\",\"AESHJ\",\"01-Jan-2013\",\"F84YF72H\",\"I\",\"\",\"\",\"\",\"L\",\"N\",\"\",\"LCL/LCL\",\"US\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"Mr Consignee\",\"CONSIGNEE ADDRESS\",\"AE\",\"CCD1\",\"Mr Consignor\",\"CONSIGNOR ADDRESS CA UNITED STATES\",\"CCD3\",\"Mr Notify\",\"NOTIFY ADDRESS\",\"\",\"\",\"\",\"\",\"\",\"\",\"shipment2 marks and numbers\",\"\",\"\",\"0\",\"Pallet\",\"PLT\",\"\",\"\",\"0\",\"0\",\"0.0\",\"0.000\",\"0.000\",\"0.000\",\"0\",\"0.000\",\"0\",\"N\",\"XXX\",\"\"\n\"BOL\",\"HOUSEBILL06\",\"LCO\",\"ACOD2\",\"USLAX\",\"USLAX\",\"AEJEA\",\"AESHJ\",\"01-Jan-2013\",\"QPFCZL12\",\"T\",\"S\",\"\",\"\",\"L\",\"N\",\"\",\"LCL/LCL\",\"US\",\"\",\"\",\"\",\"\",\"\",\"\",\"\",\"Mr Consignee\",\"CONSIGNEE ADDRESS\",\"AE\",\"CCD1\",\"Mr Consignor\",\"CONSIGNOR ADDRESS CA UNITED STATES\",\"CCD3\",\"Mr Notify\",\"NOTIFY ADDRESS\",\"\",\"\",\"\",\"\",\"\",\"\",\"shipment2 marks and numbers\",\"\",\"\",\"0\",\"Pallet\",\"PLT\",\"\",\"\",\"0\",\"0\",\"0.0\",\"0.000\",\"0.000\",\"0.000\",\"0\",\"0.000\",\"0\",\"N\",\"XXX\",\"\"\n\"END\",\"6\",\"\",\"\"";
		var consol = CreateNewConsol("LCO", "ACOD1", Constants.TransportModes.Sea, "USLAX", "AEJEA", "Vessel1", "FL111", ZDateTime.Now, "BookingRef");
		var consignor = CreateNewOrg("Mr Consignor", "Consignor address", "USLAX", "CCD1");
		var consignee = CreateNewOrg("Mr Consignee", "Consignee address", "AESHJ", "CCD2");
		var notify = CreateNewOrg("Mr Notify", "Notify address", "AESHJ", "CCD3");
		CreateNewShipment(consol, "HOUSEBILL01", consignor, consignee, notify, "USLAX", "AEJEA", ZDateTime.Now, Constants.ContainerModes.FCL, "shipment1 marks and numbers", 2000, Constants.IncoTerms.FreeOnBoard);
		CreateNewShipment(consol, "HOUSEBILL02", consignor, consignee, notify, "USLAX", "AESHJ", ZDateTime.Now, Constants.ContainerModes.LCL, "shipment2 marks and numbers", 1100m, Constants.IncoTerms.FreeCarrier);
		var shipment = CreateNewShipment(consol, "HOUSEBILL03", consignor, consignee, notify, "USLAX", "AESHJ", ZDateTime.Now, Constants.ContainerModes.LCL, "shipment2 marks and numbers", 1100m, Constants.IncoTerms.FreeCarrier);
		var transport = shipment.Transports.AddNew();
		transport.JW_TransportMode = Constants.TransportModes.Sea;
		transport.JW_RL_NKLoadPort = "AEJEA";
		transport.JW_RL_NKDiscPort = "AESHJ";
		shipment = CreateNewShipment(consol, "HOUSEBILL04", consignor, consignee, notify, "USLAX", "AESHJ", ZDateTime.Now, Constants.ContainerModes.LCL, "shipment2 marks and numbers", 1100m, Constants.IncoTerms.FreeCarrier);
		transport = shipment.Transports.AddNew();
		transport.JW_TransportMode = Constants.TransportModes.Air;
		transport.JW_RL_NKLoadPort = "AEJEA";
		transport.JW_RL_NKDiscPort = "AESHJ";
		shipment = CreateNewShipment(consol, "HOUSEBILL05", consignor, consignee, notify, "USLAX", "AESHJ", ZDateTime.Now, Constants.ContainerModes.LCL, "shipment2 marks and numbers", 1100m, Constants.IncoTerms.FreeCarrier);
		transport = shipment.Transports.AddNew();
		transport.JW_TransportMode = Constants.TransportModes.Sea;
		transport.JW_RL_NKLoadPort = "AEJEA";
		transport.JW_RL_NKDiscPort = "USLAX";
		shipment = CreateNewShipment(consol, "HOUSEBILL06", consignor, consignee, notify, "USLAX", "AESHJ", ZDateTime.Now, Constants.ContainerModes.LCL, "shipment2 marks and numbers", 1100m, Constants.IncoTerms.FreeCarrier);
		var exportConsol = CreateNewConsol("LCOD2", "ACOD2", Constants.TransportModes.Sea, "AEJEA", "AESHJ", "Vessel2", "FL112", ZDateTime.Now, "BookingRef1");
		shipment.Consols.Add(exportConsol);
		Factory.Save();
		AssertMap(consol, expectedTransShipment);
	}

	internal ForwardingConsol CreateConsolForTest(RefUNLOCO load, RefUNLOCO discharge)
	{
		RefUNLOCO origin = CreateNewPort(Constants.CountryCodes.Ukraine);
		OrgHeader consignor = CreateNewOrg("Mr Consignor", "Consignor address", load.Code, "CCD1");
		OrgHeader consignee = CreateNewOrg("Mr Consignee", "Consignee address", discharge.Code, "CCD2");
		OrgHeader notify = CreateNewOrg("Mr Notify", "Notify address", origin.Code, "CCD3");
		ForwardingConsol consol = CreateNewConsol("LCO", "ACOD1", Constants.TransportModes.Sea, load.Code, discharge.Code, "Vessel1", "FL111", ZDateTime.Now, "BookingRef");
		ForwardingShipment shipment1 = CreateNewShipment(consol, "HOUSEBILL01", consignor, consignee, notify, load.Code, origin.Code, ZDateTime.Now, Constants.ContainerModes.FCL, "shipment1 marks and numbers", 2000, Constants.IncoTerms.FreeOnBoard);
		ForwardingShipment shipment2 = CreateNewShipment(consol, "HOUSEBILL02", consignor, consignee, notify, origin.Code, discharge.Code, ZDateTime.Now, Constants.ContainerModes.LCL, "shipment2 marks and numbers", 1100m, Constants.IncoTerms.FreeCarrier);
		CommonContainer container1 = CreateNewContainer(consol, GetRefContainer("40FR"), "CTR01", 900m, "SEAL01", 36.6m, false);
		CommonContainer container2 = CreateNewContainer(consol, GetRefContainer("20NOR"), "CTR02", 1100m, "SEAL02", -50.0m, false);
		CommonContainer container3 = CreateNewContainer(consol, GetRefContainer("40FR"), "CTR03", 3330m, "SEAL03", 0, true);
		var subs = Factory.New<UNDGSubstance>();
		subs.DG_UNNO = "DG00";
		subs.DG_Variant = "1";
		subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
		var subs2 = Factory.New<UNDGSubstance>();
		subs2.DG_UNNO = "DG00";
		subs2.DG_Variant = "2";
		subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
		PackLine line11 = CreateNewPackLine(shipment1, "DG001", "IMO1", 111m, "PackLine1 Marks and Numbers", "PackLine1 Description", "HCode1", 10, Constants.PkgUnit.Pallet);
		PackLine line12 = CreateNewPackLine(shipment1, "DG001", "IMO2", 122m, "PackLine2 Marks and Numbers", "PackLine2 Description", "HCode2", 20, Constants.PkgUnit.Drum);
		PackLine line13 = CreateNewPackLine(shipment1, "DG001", "IMO3", 133m, "PackLine3 Marks and Numbers", "PackLine3 Description", "HCode3", 30, Constants.PkgUnit.Package);
		PackLine line21 = CreateNewPackLine(shipment2, "DG001", "IMO4", 144m, "PackLine4 Marks and Numbers", "PackLine4 Description", "HCode4", 40, Constants.PkgUnit.Keg);
		PackLine line22 = CreateNewPackLine(shipment2, "DG002", "IMO5", 155m, "PackLine5 Marks and Numbers", "PackLine5 Description", "HCode5", 50, Constants.PkgUnit.Cradle);
		line11.Containers.RemoveAll();
		line12.Containers.RemoveAll();
		line13.Containers.RemoveAll();
		line21.Containers.RemoveAll();
		line22.Containers.RemoveAll();
		line11.Containers.Add(container1);
		line12.Containers.Add(container1);
		line13.Containers.Add(container2);
		line21.Containers.Add(container3);
		line22.Containers.Add(container1);
		shipment1.JS_OuterPacks = 150;
		shipment2.JS_OuterPacks = 99;
		shipment1.JS_ActualChargeable = 1111.11m;
		shipment2.JS_ActualChargeable = 1001.01m;
		Factory.Save();
		return consol;
	}

	void AssertMap(ForwardingConsol consol, ZString expectedResult)
	{
		ManifestMapper mapper = new ManifestMapper();
		ManifestLine root = mapper.Map(consol);
		AssertNotNull(root);
		AssertMultilineASCIIEquals("Mapped Consol", expectedResult, root.ToString().Trim());
		root = mapper.Map(consol);
		AssertMultilineASCIIEquals("Mapped Consol 2-nd time", expectedResult, root.ToString().Trim());
	}

	RefContainer GetRefContainer(ZString codeStartsWith)
	{
		return Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, SQLComparisonOperator.StartsWith, codeStartsWith));
	}

	public void TestSlacIndicator()
	{
		TestCaseHelper.ClearTable(StmSystemDefinedField.Schema.TableName);
		ManifestMapper mapper = new ManifestMapper();
		RefUNLOCO load = CreateNewPort(Constants.CountryCodes.Iceland);
		RefUNLOCO dishcarge = CreateNewPort(Constants.CountryCodes.UnitedArabEmirates);
		ForwardingConsol consol = CreateConsolForTest(load, dishcarge);
		AssertEquals(2, consol.Shipments.Count);
		ForwardingShipment shipment = consol.Shipments[0];
		AssertEquals("SclaIndicator is not set", "N", mapper.SlacIndicator(shipment));
		StmSystemDefinedFieldCollection fieldsCollection = new StmSystemDefinedFieldCollection(Factory);
		StmSystemDefinedField shipperLoadAndCount = fieldsCollection.AddNew();
		shipperLoadAndCount.S1_Name = "Shipper Load And Count";
		shipperLoadAndCount.S1_BusinessContext = nameof(BusinessContext.Shipment);
		Factory.Save();
		AssertEquals("ForwardingShipment", BusinessContext.Shipment, shipment.DocumentSupporter.BusinessContext);
		DocumentNote note = DocumentNote.LoadNote(shipment);
		AssertEquals("SDF Count", 1, note.FilteredSystemDefinedFieldWrappers.Count);
		AssertEquals("Field Name", "Shipper Load And Count", note.FilteredSystemDefinedFieldWrappers[0].S1_Name);
		AssertEquals("Field Value", "", note.FilteredSystemDefinedFieldWrappers[0].S1_Value);
		note.FilteredSystemDefinedFieldWrappers[0].S1_Value = "VALUE";
		Factory.Save();
		ForwardingShipment shipment2 = new BusinessObjectFactory().Load<ForwardingShipment>(shipment.PK);
		AssertEquals("SclaIndicator is set", "Y", mapper.SlacIndicator(shipment2));
	}

	#region Implementation
	internal RefUNLOCO CreateNewPort(ZString countryCode)
	{
		RefUNLOCO port = Factory.New<RefUNLOCO>();
		port.Code = countryCode + "ZZZ";
		port.RL_RN_NKCountryCode = countryCode;
		Factory.Save();
		return port;
	}

	ForwardingConsol CreateNewConsol(ZString lineCode, ZString voyageAgentCode, ZString transportMode, string loadPort, string dischargePort, ZString vessel, ZString voyageFlight, ZDateTime eTA, ZString rotationNumber)
	{
		ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
		SetCustomValues(GlbCompany.CurrentCompany.OrgProxy, OrgCusCode.CodeTypes.CarrierCode, lineCode);
		Env.Registry.AECustoms.CourierID = voyageAgentCode;
		consol.JK_TransportMode = transportMode;
		consol.JK_RL_NKDischargePort = dischargePort;
		consol.JK_RL_NKLoadPort = loadPort;
		AssertEquals(1, consol.Transports.Count);
		Transport transport = consol.Transports[0];
		transport.JW_Vessel = vessel;
		transport.JW_VoyageFlight = voyageFlight;
		transport.JW_ETA = eTA;
		consol.JK_UniqueConsignRef = "C" + (++consignRef).ToString().PadLeft(5, '0');
		var rotationCusEntryNum = consol.Numbers.AddNewIfNotExist(UnitedArabEmiratesAdditionalReferenceNumberTypes.Codes.RotationNumber, rotationNumber);
		rotationCusEntryNum.CE_RN_NKCountryCode = Constants.CountryCodes.UnitedArabEmirates;
		return consol;
	}

	CommonContainer CreateNewContainer(ForwardingConsol consol, RefContainer refContainer, ZString containerNumber, ZDecimal tareWeightInKG, ZString sealNumber, ZDecimal pointTemperature, bool groupage)
	{
		ForwardingContainer container = consol.Containers.AddNew();
		container.JC_ContainerNum = containerNumber.PadRight(11, '9');
		container.JC_SealNum = sealNumber;
		container.JC_TareWeight = tareWeightInKG;
		container.JC_GrossWeightUQ = Constants.Weight.Kilograms;
		container.JC_SetPointTemp = pointTemperature;
		container.JC_SetPointTempUnit = "C";
		container.JC_RC = refContainer.PK;
		if (groupage)
		{
			container.JC_ContainerMode = Constants.ContainerModes.Groupage;
		}

		return container;
	}

	PackLine CreateNewPackLine(CommonShipment shipment, ZString subs, ZString iMOClass, ZDecimal flashPoint, ZString marksAndNumbers, ZString description, ZString harmonisedCode, ZInt packageCount, ZString packType)
	{
		PackLine line = shipment.OuterPackLines.AddNew();
		line.JL_MarksAndNumbers = marksAndNumbers;
		line.JL_Description = description;
		line.JL_HarmonisedCode = harmonisedCode;
		line.JL_PackageCount = packageCount;
		line.JL_F3_NKPackType = packType;
		var dgItem = line.UNDGs.AddNew();
		var unno = subs.SubstringSafe(0, UNDGSubstanceSchema.DG_UNNO.MaxLength);
		var variant = subs.SubstringSafe(UNDGSubstanceSchema.DG_UNNO.MaxLength, 1);
		var substance = UNDGSubstanceLoader.LoadSubstances(Factory, unno, variant, UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO).FirstOrDefault();
		substance.DG_Class = iMOClass;
		dgItem.LinkDefault(substance);
		dgItem.DI_DGFlashPoint = flashPoint;
		line.JL_ActualWeight = 111m;
		line.JL_ActualWeightUQ = Constants.Weight.Kilograms;
		line.JL_ActualVolume = 11m;
		line.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
		return line;
	}

	ForwardingShipment CreateNewShipment(ForwardingConsol consol, ZString houseBill, OrgHeader consignee, OrgHeader consignor, OrgHeader notify, ZString origin, ZString destination, ZDateTime shippedOnBoardDate, ZString packMode, ZString marksAndNumbers, ZDecimal actualChargeable, ZString iNCO)
	{
		ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
		if (consignor != null)
		{
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
		}

		if (consignee != null)
		{
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
		}

		if (notify != null)
		{
			shipment.NotifyPartyDocumentaryAddress.OrganisationPK = notify.PK;
		}

		shipment.JS_HouseBill = houseBill;
		shipment.JS_RL_NKOrigin = origin;
		shipment.JS_RL_NKDestination = destination;
		shipment.JS_ShippedOnBoardDate = shippedOnBoardDate;
		shipment.JS_PackingMode = packMode;
		shipment.JS_INCO = iNCO;
		shipment.JS_MarksAndNumbers = marksAndNumbers;
		consol.Shipments.Add(shipment);
		return shipment;
	}

	OrgHeader CreateNewOrg(ZString fullName, ZString address, ZString portCode, ZString cCDCode)
	{
		OrgHeader org = Factory.New<OrgHeader>();
		org.OH_FullName = fullName;
		org.MainAddress.OA_Address1 = address;
		org.OH_RL_NKClosestPort = portCode;
		if (!cCDCode.IsEmpty)
		{
			SetCustomValues(org, OrgCusCode.CodeTypes.CustomsClientCode, cCDCode);
		}

		return org;
	}

	void SetCustomValues(OrgHeader org, ZString customCode, ZString value)
	{
		org.CustomsCodes.AddNew(customCode, value, GlbCompany.CurrentCompany.Country);
	}

	ZInt consignRef;
	#endregion
}
