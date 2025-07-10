using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

public class DeclarationAESConsignmentWrapperTest : WrapperHelperTest<DeclarationAESConsignmentWrapper>
{
	public void TestModeOfTransportAtBorder()
	{
		CombineAssertions(() =>
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected filled ModeOfTransportAtBorder Sea (1)", "1", wrapper.ModeOfTransportAtBorder);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected empty ModeOfTransportAtBorder when entry instruction is B", ZString.Empty, wrapper.ModeOfTransportAtBorder);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected filled ModeOfTransportAtBorder Rail (2)", "2", wrapper.ModeOfTransportAtBorder);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected empty ModeOfTransportAtBorder when entry instruction is C and isComplementaryCWithMRN is false", ZString.Empty, wrapper.ModeOfTransportAtBorder);

			wrapper = GetWrapper(entryHeader, isComplementaryCWithMRN: true);
			AssertEquals("Expected filled ModeOfTransportAtBorder Rail (2) when EntryInstruction is C but isComplementaryCWithMRN is true", "2", wrapper.ModeOfTransportAtBorder);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected filled ModeOfTransportAtBorder Road (3)", "3", wrapper.ModeOfTransportAtBorder);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected filled ModeOfTransportAtBorder Air (4)", "4", wrapper.ModeOfTransportAtBorder);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected filled ModeOfTransportAtBorder Mail (5)", "5", wrapper.ModeOfTransportAtBorder);

			declaration.JE_TransportMode = Core.Constants.TransportModes.FixedTransportInstallations;
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected filled ModeOfTransportAtBorder FixedTransportInstallations (7)", "7", wrapper.ModeOfTransportAtBorder);

			declaration.JE_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected filled ModeOfTransportAtBorder InlandWaterwayTransport (8)", "8", wrapper.ModeOfTransportAtBorder);

			declaration.JE_TransportMode = Core.Constants.TransportModes.OwnPropulsion;
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected filled ModeOfTransportAtBorder OwnPropulsion (9)", "9", wrapper.ModeOfTransportAtBorder);
		});
	}

	public void TestGrossMass()
	{
		CombineAssertions(() =>
		{
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine.JI_Weight = 1.1234m;
			AssertEquals("Expected filled GrossMass with 1 invoice line", 1.123m, wrapper.GrossMass);

			var entryLine2 = entryHeader.MergedLines.AddNew();
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine2.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			invoiceLine2.JI_Weight = 2.3211m;
			invoiceLine2.JI_CL = entryLine2.PK;
			AssertEquals("Expected filled GrossMass with 2 invoice lines", 3.444m, wrapper.GrossMass);
		});
	}

	public void TestReferenceNumberUCR()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty ReferenceNumberUCR when Details/Shipment Details/Declarant's Ref is empty", ZString.Empty, wrapper.ReferenceNumberUCR);

			declaration.JE_OwnerRef = "reference";
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected filled ReferenceNumberUCR when Details/Shipment Details/Declarant's Ref is filled", "reference", wrapper.ReferenceNumberUCR);
		});
	}

	public void TestNullCarrier()
	{
		declaration.JE_OH_ShippingLine = ZGuid.Empty;
		AssertExceptionThrown<NullReferenceException>(() => wrapper.Carrier.ToString());
	}

	public void TestCarrier()
	{
		CombineAssertions(() =>
		{
			var orgAddress = Factory.New<OrgAddress>();
			var orgHeader = Factory.New<OrgHeader>();
			orgAddress.OA_OH = orgHeader.PK;
			declaration.JE_OH_ShippingLine = orgHeader.PK;

			var carrier = wrapper.Carrier;
			AssertNotNull("Expected filled Carrier", carrier);
			AssertSame("Cached Carrier", wrapper.Carrier, carrier);

			declaration.JE_OA_DeclarantAddress = orgAddress.PK;
			wrapper = GetWrapper(entryHeader);
			AssertNull("Expected empty Carrier when it is the same as the Declarant", wrapper.Carrier);
		});
	}

	public void TestNullConsignor()
	{
		AssertExceptionThrown<NullReferenceException>("shouldDeclareConsignorInConsignment flag is false so Consignor is null", () => wrapper.Consignee.ToString());

		wrapper = GetWrapper(entryHeader, shouldDeclareConsignorInConsignment: true);
		invoiceLine.JI_OA_ExporterAddress = ZGuid.Empty;
		AssertExceptionThrown<NullReferenceException>("shouldDeclareConsignorInConsignment flag is true but consignor is not declared so Consignor is null", () => wrapper.Consignee.ToString());
	}

	public void TestConsignor()
	{
		CombineAssertions(() =>
		{
			var orgHeader = Factory.New<OrgHeader>();
			invoiceLine.JI_OA_ExporterAddress = orgHeader.MainAddress.PK;
			wrapper = GetWrapper(entryHeader, shouldDeclareConsignorInConsignment: true);
			var consignor = wrapper.Consignor;
			AssertNotNull("Expected filled Consignor when shouldDeclareConsignorInConsignment flag is true and only one line for goods exists", wrapper.Consignor);
			AssertSame("Cached Consignor", wrapper.Consignor, consignor);
		});
	}

	public void TestNullConsignee()
	{
		AssertExceptionThrown<NullReferenceException>("shouldDeclareConsigneeInConsignment flag is false so Consignee is null", () => wrapper.Consignee.ToString());

		wrapper = GetWrapper(entryHeader, shouldDeclareConsigneeInConsignment: true);
		declaration.JE_OH_Importer = ZGuid.Empty;
		AssertExceptionThrown<NullReferenceException>("shouldDeclareConsigneeInConsignment flag is true but importer is not declared so Consignee is null", () => wrapper.Consignee.ToString());
	}

	public void TestConsignee_DontSendImporterId()
	{
		var orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "Declaration orgHeader";
		var orgAddressMain = orgHeader.MainAddress;
		orgAddressMain.OA_Address1 = "Address Main 1";

		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address Other 1";

		declaration.ImporterDocumentaryAddress.E2_OA_Address = orgAddress.PK;
		declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader.PK;

		CombineAssertions(() =>
		{
			declaration.ZG_DontSendImporterId = false;
			wrapper = GetWrapper(entryHeader, shouldDeclareConsigneeInConsignment: true);
			var consignee = wrapper.Consignee;
			AssertEquals("Expected Consignee's Name filled when there is no valid CusCode for id even when ZG_DontSendImporterId is false", "Declaration orgHeader", consignee.Name);
			AssertEquals("Expected Consignee's Id empty when there is no valid CusCode for id even when ZG_DontSendImporterId is false", ZString.Empty, consignee.Id);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
			wrapper = GetWrapper(entryHeader, shouldDeclareConsigneeInConsignment: true);
			consignee = wrapper.Consignee;
			AssertEquals("Expected Consignee's Name empty when there is a valid CusCode for id and ZG_DontSendImporterId is false", ZString.Empty, consignee.Name);
			AssertEquals("Expected Consignee's Id filled when there is a valid CusCode for id and ZG_DontSendImporterId is false", "GB333333333", consignee.Id);

			declaration.ZG_DontSendImporterId = true;
			wrapper = GetWrapper(entryHeader, shouldDeclareConsigneeInConsignment: true);
			consignee = wrapper.Consignee;
			AssertEquals("Expected Consignee's Name filled when there is a valid CusCode for id but ZG_DontSendImporterId is true", "Declaration orgHeader", consignee.Name);
			AssertEquals("Expected Consignee's Id empty when there is a valid CusCode for id but ZG_DontSendImporterId is true", ZString.Empty, consignee.Id);
		});
	}

	public void TestConsignee()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusMapType(RefCusMapTypeList.Codes.EUCTY, MapDirectionList.Codes.BTH, "Description", false);
		helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "RS", "XS", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
		helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "MQ", "FR", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
		Factory.Save();

		var orgHeader1 = Factory.New<OrgHeader>();
		orgHeader1.OH_FullName = "Declaration orgHeader";
		orgHeader1.OH_Code = "Code1";
		var orgAddressMain1 = orgHeader1.MainAddress;
		orgAddressMain1.OA_Address1 = "Address Main 1";

		var orgAddress1 = Factory.New<OrgAddress>();
		orgAddress1.OA_OH = orgHeader1.PK;
		orgAddress1.OA_Address1 = "Address Other 1";

		var orgHeader2 = Factory.New<OrgHeader>();
		orgHeader2.OH_FullName = "Header orgHeader";
		orgHeader2.OH_Code = "Code2";
		var orgAddressMain2 = orgHeader2.MainAddress;
		orgAddressMain2.OA_Address1 = "Address Main 2";

		var orgAddress2 = Factory.New<OrgAddress>();
		orgAddress2.OA_OH = orgHeader2.PK;
		orgAddress2.OA_Address1 = "Address Other 2";

		CombineAssertions(() =>
		{
			declaration.ImporterDocumentaryAddress.E2_OA_Address = orgAddress1.PK;
			declaration.ImporterDocumentaryAddress.OrganisationPK = orgHeader1.PK;
			wrapper = GetWrapper(entryHeader, shouldDeclareConsigneeInConsignment: true);
			var consignee = wrapper.Consignee;
			AssertNotNull("Expected filled Consignee with declaration's importer when shouldDeclareConsigneeInConsignment flag is true but randomLine has importer empty", consignee);
			AssertEquals("Expected declaration importer in Consignee", "Declaration orgHeader", consignee.Name);
			AssertEquals("Expected declaration importer in Consignee with correct Address", "Address Other 1", consignee.Address.Address);
			AssertSame("Cached Consignee", wrapper.Consignee, consignee);

			invoiceHeader.JZ_OH_Buyer = orgHeader2.PK;
			invoiceHeader.JZ_OA_BuyerAddress = orgAddress2.PK;
			wrapper = GetWrapper(entryHeader, shouldDeclareConsigneeInConsignment: true);
			consignee = wrapper.Consignee;
			AssertNotNull("Expected filled Consignee when shouldDeclareConsigneeInConsignment flag is true and randomLine has importer declared", consignee);
			AssertEquals("Expected invoiceheader importer in Consignee", "Header orgHeader", consignee.Name);
			AssertEquals("Expected invoiceheader importer in Consignee with correct Address", "Address Other 2", consignee.Address.Address);

			invoiceHeader.JZ_OA_BuyerAddress = ZGuid.Empty;
			wrapper = GetWrapper(entryHeader, shouldDeclareConsigneeInConsignment: true);
			consignee = wrapper.Consignee;
			AssertNotNull("Expected filled Consignee when shouldDeclareConsigneeInConsignment flag is true and randomLine has importer declared (without address declared)", consignee);
			AssertEquals("Expected invoiceheader importer in Consignee (without address declared)", "Header orgHeader", consignee.Name);
			AssertEquals("Expected invoiceheader importer in Consignee with correct Address (without address declared)", "Address Main 2", consignee.Address.Address);
		});

		CombineAssertions("Consignee.Address.Country should be using default territory if present.", () =>
		{
			invoiceHeader.JZ_OH_Buyer = orgHeader2.PK;
			invoiceHeader.JZ_OA_BuyerAddress = orgAddress2.PK;

			orgAddress2.OA_RN_NKCountryCode = "RS";
			wrapper = GetWrapper(entryHeader, shouldDeclareConsigneeInConsignment: true);
			AssertEquals("Expected filled Country with Default Territory (XS)", "XS", wrapper.Consignee.Address.Country);

			orgAddress2.OA_RN_NKCountryCode = "ES";
			wrapper = GetWrapper(entryHeader, shouldDeclareConsigneeInConsignment: true);
			AssertEquals("Expected filled Country with given code since there is no Default Territory", "ES", wrapper.Consignee.Address.Country);

			orgAddress2.OA_RN_NKCountryCode = "MQ";
			wrapper = GetWrapper(entryHeader, shouldDeclareConsigneeInConsignment: true);
			AssertEquals("Expected filled Country with Default Territory (FR)", "FR", wrapper.Consignee.Address.Country);
		});
	}

	public void TestTransportEquipment()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty TransportEquipment", 0, wrapper.TransportEquipment.Count);

			declaration.JE_ContainerMode = "ULD";
			var package1 = declaration.Packages.AddNew();
			var container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT1";
			container1.CO_Seal = "seal11";
			package1.CW_ContainerNoOrEquipmentNo = container1.CO_ContainerNumber;
			invoiceLine.PackagesPivot.AddPivotFor(package1);
			wrapper = GetWrapper(entryHeader);
			var transportEquipment = wrapper.TransportEquipment;
			AssertEquals("Expected filled TransportEquipment", 1, transportEquipment.Count);
			AssertSame("Cached TransportEquipment", wrapper.TransportEquipment, transportEquipment);
		});
	}

	public void TestLocationOfGoods()
	{
		var locationOfGoods = wrapper.LocationOfGoods;
		CombineAssertions(() =>
		{
			AssertNotNull("Expected filled LocationOfGoods", locationOfGoods);
			AssertSame("Cached LocationOfGoods", wrapper.LocationOfGoods, locationOfGoods);
		});
	}

	public void TestDepartureTransportMeans()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusMapType(RefCusMapTypeList.Codes.EUCTY, MapDirectionList.Codes.BTH, "Description", false);
		helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "RS", "XS", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
		helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "MQ", "FR", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
		Factory.Save();

		declaration.CustomsOffices.RemoveAndDeleteAll();
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty DepartureTransportMeans when no data declared", 0, wrapper.DepartureTransportMeans.Count);

			declaration.JE_TransportMeans = "00";
			declaration.ZG_Box18TransportID = "identification";
			declaration.ZG_Box18TransportNationality = "ES";

			wrapper = GetWrapper(entryHeader);
			var departureTransportMeans = wrapper.DepartureTransportMeans;

			AssertEquals("Expected empty DepartureTransportMeans when CustomsOfficeOfExport and CustomsOffice of Exit are the same (empty)", 0, wrapper.DepartureTransportMeans.Count);

			declaration.JE_CustomsOffice = "ES009999";

			var customsOfficeExit = declaration.CustomsOffices.AddNew();
			customsOfficeExit.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit;
			customsOfficeExit.CY_Data = "FR008889";

			wrapper = GetWrapper(entryHeader);
			departureTransportMeans = wrapper.DepartureTransportMeans;
			AssertEquals("Expected filled DepartureTransportMeans", 1, departureTransportMeans.Count);
			AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);

			var departureTransportMeansObject = departureTransportMeans.First();
			AssertEquals("Expected filled DepartureTransportMeans correctly, SequenceNumber", "1", departureTransportMeansObject.SequenceNumber);
			AssertEquals("Expected filled DepartureTransportMeans correctly, TransportMode", "00", departureTransportMeansObject.TransportMode);
			AssertEquals("Expected filled DepartureTransportMeans correctly, TransportId", "identification", departureTransportMeansObject.TransportId);
			AssertEquals("Expected filled DepartureTransportMeans correctly, TransportNationality", "ES", departureTransportMeansObject.TransportNationality);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected empty DepartureTransportMeans when EntryInstruction is B", 0, wrapper.DepartureTransportMeans.Count);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
			wrapper = GetWrapper(entryHeader, isComplementaryCWithMRN: true);
			AssertEquals("Expected filled DepartureTransportMeans when EntryInstruction is C but isComplementaryCWithMRN is true", 1, wrapper.DepartureTransportMeans.Count);

			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected empty DepartureTransportMeans when EntryInstruction is C and isComplementaryCWithMRN is false", 0, wrapper.DepartureTransportMeans.Count);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.Y;
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected filled DepartureTransportMeans when EntryInstruction is Y", 1, wrapper.DepartureTransportMeans.Count);

			declaration.JE_CustomsOffice = "FR008889";
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected filled DepartureTransportMeans when customsOfficeExit = JE_CustomsOffice", 0, wrapper.DepartureTransportMeans.Count);
		});

		CombineAssertions("DepartureTransportMeans.TransportNationality should be using default territory if present.", () =>
		{
			declaration.JE_CustomsOffice = "ES009999";

			declaration.ZG_Box18TransportNationality = "RS";
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected filled Country with Default Territory (XS)", "XS", wrapper.DepartureTransportMeans.First().TransportNationality);

			declaration.ZG_Box18TransportNationality = "ES";
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected filled Country with given code since there is no Default Territory", "ES", wrapper.DepartureTransportMeans.First().TransportNationality);

			declaration.ZG_Box18TransportNationality = "MQ";
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected filled Country with Default Territory (FR)", "FR", wrapper.DepartureTransportMeans.First().TransportNationality);
		});
	}

	public void TestROADepartureTransportMeans()
	{
		declaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.Road;
		declaration.ZG_Box18TransportID = "Box18";

		CombineAssertions(() =>
		{
			AssertEquals("Expected empty DepartureTransportMeans when no data declared", 0, wrapper.DepartureTransportMeans.Count);

			declaration.JE_TransportMeans = "00";
			declaration.JE_TransportIDInland = "identification";
			declaration.JE_RN_NKTransportNationalityInland = "ES";

			declaration.JE_CustomsOffice = "ES009999";

			var customsOfficeExit = declaration.CustomsOffices.AddNew();
			customsOfficeExit.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit;
			customsOfficeExit.CY_Data = "FR008889";

			wrapper = GetWrapper(entryHeader);
			var departureTransportMeans = wrapper.DepartureTransportMeans;
			AssertEquals("Expected filled DepartureTransportMeans", 1, departureTransportMeans.Count);
			AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);

			var departureTransportMeansObject = departureTransportMeans.First();
			AssertEquals("Expected filled DepartureTransportMeans correctly, SequenceNumber", "1", departureTransportMeansObject.SequenceNumber);
			AssertEquals("Expected filled DepartureTransportMeans correctly, TransportMode", "00", departureTransportMeansObject.TransportMode);
			AssertEquals("Expected filled DepartureTransportMeans correctly, TransportId", "identification", departureTransportMeansObject.TransportId);
			AssertEquals("Expected filled DepartureTransportMeans correctly, TransportNationality", "ES", departureTransportMeansObject.TransportNationality);

			declaration.JE_Trailer1RegNo = "identification2";
			declaration.JE_RN_NKTrailer1Nationality = "FR";

			wrapper = GetWrapper(entryHeader);
			departureTransportMeans = wrapper.DepartureTransportMeans;
			AssertEquals("Expected filled DepartureTransportMeans", 2, departureTransportMeans.Count);
			AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);

			departureTransportMeansObject = departureTransportMeans.ElementAt(1);
			AssertEquals("Expected filled DepartureTransportMeans correctly, SequenceNumber", "2", departureTransportMeansObject.SequenceNumber);
			AssertEquals("Expected filled DepartureTransportMeans correctly, TransportMode", "31", departureTransportMeansObject.TransportMode);
			AssertEquals("Expected filled DepartureTransportMeans correctly, TransportId", "identification2", departureTransportMeansObject.TransportId);
			AssertEquals("Expected filled DepartureTransportMeans correctly, TransportNationality", "FR", departureTransportMeansObject.TransportNationality);

			declaration.JE_Trailer2RegNo = "identification3";
			declaration.JE_RN_NKTrailer2Nationality = "IT";

			wrapper = GetWrapper(entryHeader);
			departureTransportMeans = wrapper.DepartureTransportMeans;
			AssertEquals("Expected filled DepartureTransportMeans", 3, departureTransportMeans.Count);
			AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);

			departureTransportMeansObject = departureTransportMeans.ElementAt(2);
			AssertEquals("Expected filled DepartureTransportMeans correctly, SequenceNumber", "3", departureTransportMeansObject.SequenceNumber);
			AssertEquals("Expected filled DepartureTransportMeans correctly, TransportMode", "31", departureTransportMeansObject.TransportMode);
			AssertEquals("Expected filled DepartureTransportMeans correctly, TransportId", "identification3", departureTransportMeansObject.TransportId);
			AssertEquals("Expected filled DepartureTransportMeans correctly, TransportNationality", "IT", departureTransportMeansObject.TransportNationality);
		});
	}

	public void TestRAIDepartureTransportMeans()
	{
		declaration.JE_TransportModeInland = Customs.Business.TransportTypeList.Codes.Rail;

		CombineAssertions(() =>
		{
			AssertEquals("Expected empty DepartureTransportMeans when no data declared", 0, wrapper.DepartureTransportMeans.Count);

			declaration.JE_TransportMeans = "00";
			declaration.ZG_Box18TransportID = "identification";
			declaration.ZG_Box18TransportNationality = "ES";

			declaration.JE_CustomsOffice = "ES009999";

			var customsOfficeExit = declaration.CustomsOffices.AddNew();
			customsOfficeExit.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit;
			customsOfficeExit.CY_Data = "FR008889";

			wrapper = GetWrapper(entryHeader);
			var departureTransportMeans = wrapper.DepartureTransportMeans;
			AssertEquals("Expected filled DepartureTransportMeans", 1, departureTransportMeans.Count);
			AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);

			var departureTransportMeansObject = departureTransportMeans.First();
			AssertEquals("Expected filled DepartureTransportMeans correctly, SequenceNumber", "1", departureTransportMeansObject.SequenceNumber);
			AssertEquals("Expected filled DepartureTransportMeans correctly, TransportMode", "00", departureTransportMeansObject.TransportMode);
			AssertEquals("Expected filled DepartureTransportMeans correctly, TransportId", "identification", departureTransportMeansObject.TransportId);
			AssertEquals("Expected filled DepartureTransportMeans correctly, TransportNationality", "ES", departureTransportMeansObject.TransportNationality);

			declaration.JE_Trailer1RegNo = "identification2";
			declaration.JE_RN_NKTrailer1Nationality = "FR";

			wrapper = GetWrapper(entryHeader);
			departureTransportMeans = wrapper.DepartureTransportMeans;
			AssertEquals("Expected filled DepartureTransportMeans", 2, departureTransportMeans.Count);
			AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);

			departureTransportMeansObject = departureTransportMeans.ElementAt(1);
			AssertEquals("Expected filled DepartureTransportMeans correctly, SequenceNumber", "2", departureTransportMeansObject.SequenceNumber);
			AssertEquals("Expected filled DepartureTransportMeans correctly, TransportMode", "20", departureTransportMeansObject.TransportMode);
			AssertEquals("Expected filled DepartureTransportMeans correctly, TransportId", "identification2", departureTransportMeansObject.TransportId);
			AssertEquals("Expected filled DepartureTransportMeans correctly, TransportNationality", "FR", departureTransportMeansObject.TransportNationality);

			var inlandTransports1 = declaration.InlandTransports.AddNew();
			inlandTransports1.CY_Data = "identification3";
			inlandTransports1.Nationality = "IT";

			var inlandTransports2 = declaration.InlandTransports.AddNew();
			inlandTransports2.CY_Data = "identification4";
			inlandTransports2.Nationality = "DE";

			wrapper = GetWrapper(entryHeader);
			departureTransportMeans = wrapper.DepartureTransportMeans;
			AssertEquals("Expected filled DepartureTransportMeans", 4, departureTransportMeans.Count);
			AssertSame("Cached DepartureTransportMeans", wrapper.DepartureTransportMeans, departureTransportMeans);

			departureTransportMeansObject = departureTransportMeans.ElementAt(2);
			AssertEquals("Expected filled DepartureTransportMeans correctly, SequenceNumber", "3", departureTransportMeansObject.SequenceNumber);
			AssertEquals("Expected filled DepartureTransportMeans correctly, TransportMode", "20", departureTransportMeansObject.TransportMode);
			AssertEquals("Expected filled DepartureTransportMeans correctly, TransportId", "identification3", departureTransportMeansObject.TransportId);
			AssertEquals("Expected filled DepartureTransportMeans correctly, TransportNationality", "IT", departureTransportMeansObject.TransportNationality);

			departureTransportMeansObject = departureTransportMeans.ElementAt(3);
			AssertEquals("Expected filled DepartureTransportMeans correctly, SequenceNumber", "4", departureTransportMeansObject.SequenceNumber);
			AssertEquals("Expected filled DepartureTransportMeans correctly, TransportMode", "20", departureTransportMeansObject.TransportMode);
			AssertEquals("Expected filled DepartureTransportMeans correctly, TransportId", "identification4", departureTransportMeansObject.TransportId);
			AssertEquals("Expected filled DepartureTransportMeans correctly, TransportNationality", "DE", departureTransportMeansObject.TransportNationality);
		});
	}

	public void TestCountryOfRoutingOfConsignments()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty CountryOfRoutingOfConsignments when no data declared", 0, wrapper.CountryOfRoutingOfConsignments.Count);

			var trans1 = declaration.Transports.AddNew();
			var trans2 = declaration.Transports.AddNew();
			var trans3 = declaration.Transports.AddNew();
			var trans4 = declaration.Transports.AddNew();
			trans1.JW_RL_NKLoadPort = "ESMAD";
			trans1.JW_RL_NKDiscPort = "ITBOL";
			trans2.JW_RL_NKLoadPort = "ITBOL";
			trans2.JW_RL_NKDiscPort = "FRACO";
			trans3.JW_RL_NKLoadPort = "FRACO";
			trans3.JW_RL_NKDiscPort = "ESTFN";
			trans4.JW_RL_NKLoadPort = "ESTFN";
			trans4.JW_RL_NKDiscPort = "CNSHA";

			declaration.JE_RL_NKOrigin = "ESMAD";
			declaration.JE_RL_NKFinalDestination = "CNSHA";

			var expectedCountryListFor = new String[] { "ES", "IT", "FR", "ES", "CN" };
			List<String> result = new List<String>();

			entryHeader.ZG_UCC6Version = 1;
			entryInstruction.IncludeRoutingSecurityData = true;
			wrapper = GetWrapper(entryHeader);
			var countryOfRoutingOfConsignments = wrapper.CountryOfRoutingOfConsignments;
			foreach (CommonCountryOfRoutingOfConsignmentWrapper country in countryOfRoutingOfConsignments)
			{
				result.Add(country.CountryOfRouting);
			}
			AssertArrayEqualsByElements("Expected filled CountryCodes when IncludeRoutingSecurityData is true", expectedCountryListFor, result.ToArray());
			AssertEquals("Expected filled CountryOfRoutingOfConsignments when IncludeRoutingSecurityData is true", 5, countryOfRoutingOfConsignments.Count);
			AssertSame("Cached CountryOfRoutingOfConsignments", wrapper.CountryOfRoutingOfConsignments, countryOfRoutingOfConsignments);

			entryInstruction.IncludeRoutingSecurityData = false;
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected empty CountryOfRoutingOfConsignments when IncludeRoutingSecurityData is false", 0, wrapper.CountryOfRoutingOfConsignments.Count);
		});
	}

	public void TestActiveBorderTransportMeans()
	{
		CombineAssertions(() =>
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertNull("Expected null ActiveBorderTransportMeans when no data declared (even if TransportMode is declared)", wrapper.ActiveBorderTransportMeans);

			declaration.ZG_BorderTransportMeans = "00";
			wrapper = GetWrapper(entryHeader);
			var activeBorderTransportMeans = wrapper.ActiveBorderTransportMeans;
			AssertNotNull("Expected filled ActiveBorderTransportMeans when TransportMode is not empty", activeBorderTransportMeans);
			AssertSame("Cached ActiveBorderTransportMeans", wrapper.ActiveBorderTransportMeans, activeBorderTransportMeans);

			declaration.JE_TransportMode = ZString.Empty;
			wrapper = GetWrapper(entryHeader);
			AssertNull("Expected null ActiveBorderTransportMeans when TransportMode is empty", wrapper.ActiveBorderTransportMeans);
		});

		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusMapType(RefCusMapTypeList.Codes.EUCTY, MapDirectionList.Codes.BTH, "Description", false);
		helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "RS", "XS", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
		helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "MQ", "FR", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
		Factory.Save();

		CombineAssertions("ActiveBorderTransportMeans.TransportNationality should use default territory if present.", () =>
		{
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;

			declaration.JE_RN_NKTransportNationality = "RS";
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected filled Country with Default Territory (XS)", "XS", wrapper.ActiveBorderTransportMeans.TransportNationality);

			declaration.JE_RN_NKTransportNationality = "ES";
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected filled Country with given code since there is no Default Territory", "ES", wrapper.ActiveBorderTransportMeans.TransportNationality);

			declaration.JE_RN_NKTransportNationality = "MQ";
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected filled Country with Default Territory (FR)", "FR", wrapper.ActiveBorderTransportMeans.TransportNationality);
		});
	}
	
	public void TestActiveBorderTransportMeansInProvisionalDeclarations()
	{
		declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
		declaration.ZG_BorderTransportMeans = "00";

		CombineAssertions("ActiveBorderTransportMeans should not be sent in provisional declarations when modeOfTransportAtTheBorder is not sent", () =>
		{
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			wrapper = GetWrapper(entryHeader);
			Assert("Pre-Requisite: With SubStyle B, ModeOfTransportAtBorder is empty", wrapper.ModeOfTransportAtBorder.IsEmpty);
			AssertNull("With SubStyle B, ActiveBorderTransportMeans should be null", wrapper.ActiveBorderTransportMeans);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			wrapper = GetWrapper(entryHeader);
			Assert("Pre-Requisite: With SubStyle A, ModeOfTransportAtBorder is not empty", !wrapper.ModeOfTransportAtBorder.IsEmpty);
			AssertNotNull("With SubStyle A, ActiveBorderTransportMeans should be filled", wrapper.ActiveBorderTransportMeans);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
			wrapper = GetWrapper(entryHeader);
			Assert("Pre-Requisite: With SubStyle C, ModeOfTransportAtBorder is empty", wrapper.ModeOfTransportAtBorder.IsEmpty);
			AssertNull("With SubStyle C without MRN, ActiveBorderTransportMeans should be null", wrapper.ActiveBorderTransportMeans);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
			wrapper = GetWrapper(entryHeader, isComplementaryCWithMRN: true);
			Assert("Pre-Requisite: With SubStyle C with MRN, ModeOfTransportAtBorder is not empty", !wrapper.ModeOfTransportAtBorder.IsEmpty);
			AssertNotNull("With SubStyle C with MRN, ActiveBorderTransportMeans should be filled", wrapper.ActiveBorderTransportMeans);
		});
	}

	public void TestTransportDocuments()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty TransportDocuments list", 0, wrapper.TransportDocuments.Count);

			var supdoc1 = declaration.AdditionalInfos.AddNew();
			supdoc1.CSI_Code = "9001";
			supdoc1.CSI_SubType = "TRA";

			var supdoc1INF = declaration.AdditionalInfos.AddNew();
			supdoc1INF.CSI_Code = "Y001";
			supdoc1INF.CSI_SubType = "INF";

			var supdoc2 = entryInstruction.AdditionalInfos.AddNew();
			supdoc2.CSI_Code = "9002";
			supdoc2.CSI_SubType = "TRA";

			var supdoc2INF = entryInstruction.AdditionalInfos.AddNew();
			supdoc2INF.CSI_Code = "Y002";
			supdoc2INF.CSI_SubType = "INF";

			var supdoc3 = invoiceHeader.AdditionalInfos.AddNew();
			supdoc3.CSI_Code = "9003";
			supdoc3.CSI_SubType = "TRA";

			var supdoc3INF = invoiceHeader.AdditionalInfos.AddNew();
			supdoc3INF.CSI_Code = "Y003";
			supdoc3INF.CSI_SubType = "INF";

			var supdoc4 = invoiceLine.AdditionalInfos.AddNew();
			supdoc4.CSI_Code = "9004";
			supdoc4.CSI_SubType = "TRA";

			var supdoc5 = invoiceLine.AdditionalInfos.AddNew();
			supdoc5.CSI_Code = "9005";
			supdoc5.CSI_SubType = "TRA";

			var supdoc6 = invoiceLine.AdditionalInfos.AddNew();
			supdoc6.CSI_Code = "Y004";
			supdoc6.CSI_SubType = "INF";

			using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, true))
			{
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected empty TransportDocuments list when declared but transitionPeriod is true", 0, wrapper.TransportDocuments.Count);
			}

			using (TemporarilyClearDeclarationConfigurationAndThenSetIsTransitionPeriodAES30(declaration, false))
			{
				wrapper = GetWrapper(entryHeader);
				var documents = wrapper.TransportDocuments;
				AssertEquals("Expected filled TransportDocuments with number 9001 for Misc)", true, documents.Any(x => x.Name == "9001"));
				AssertEquals("Expected filled TransportDocuments with number 9002 for EntryInstruction)", true, documents.Any(x => x.Name == "9002"));
				AssertEquals("Expected filled TransportDocuments with number 9003 for Header)", true, documents.Any(x => x.Name == "9003"));
				AssertEquals("Expected filled TransportDocuments with number 9004 for Lines)", true, documents.Any(x => x.Name == "9004"));
				AssertEquals("Expected filled TransportDocuments with number 9005 for Lines)", true, documents.Any(x => x.Name == "9005"));

				AssertEquals("Expected filled TransportDocuments when transition period is false (included those that that have subType TRA and are in declaration, entryInstruction, invoiceHeader and invoiceLine)", 5, documents.Count);
				AssertSame("Cached TransportDocuments", wrapper.TransportDocuments, documents);
			}
		});
	}

	public void TestTransportChargesMoP()
	{
		CombineAssertions(() =>
		{
			invoiceHeader.ZG_TransportChargesMethodOfPayment = "A";
			AssertEquals("Expected filled TransportChargesMoP when EntryInstruction is not B nor C", "A", wrapper.TransportChargesMoP);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected empty TransportChargesMoP when EntryInstruction is B", ZString.Empty, wrapper.TransportChargesMoP);

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
			wrapper = GetWrapper(entryHeader, isComplementaryCWithMRN: true);
			AssertEquals("Expected filled TransportChargesMoP when EntryInstruction is C but isComplementaryCWithMRN is true", "A", wrapper.TransportChargesMoP);

			wrapper = GetWrapper(entryHeader);
			AssertEquals("Expected empty TransportChargesMoP when EntryInstruction is C and isComplementaryCWithMRN is false", ZString.Empty, wrapper.TransportChargesMoP);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.FillWithValidTestData();
		declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

		declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = invoiceHeader.InvoiceLines.AddNew();
		invoiceLine.JI_Tariff = "11";

		var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
		AssertEquals("Merge done", true, mergeResult);

		entryHeader = declaration.CustomsEntryHeaders[0];

		wrapper = GetWrapper(entryHeader);
	}

	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;
	CusEntryHeader entryHeader;
	DeclarationAESConsignmentWrapper wrapper;

	DeclarationAESConsignmentWrapper GetWrapper(CusEntryHeader entryheader, bool shouldDeclareConsignorInConsignment = false, bool shouldDeclareConsigneeInConsignment = false, bool isComplementaryCWithMRN = false) => new DeclarationAESConsignmentWrapper(entryheader, shouldDeclareConsignorInConsignment, shouldDeclareConsigneeInConsignment, isComplementaryCWithMRN);

	protected override DeclarationAESConsignmentWrapper GetProvider() => wrapper;
}
