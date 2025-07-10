using System;
using CargoWise.Types;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Testing;
using Enterprise.DocumentEngine.SDF;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Bill = Enterprise.Customs.NZ.Business.Declaration.Bill;
using BillTypeList = Enterprise.Customs.NZ.Business.Declaration.BillTypeList;
using JobMessageTypeList = Enterprise.Customs.NZ.Business.JobMessageTypeList;

namespace Enterprise.DocumentWrappers.Customs.NZ.Testing
{
	abstract class DocDeclarationTest : DocBaseJobDeclarationAbstractTest<JobDeclaration, DocDeclaration>
	{
		[TestDateIncremental(0, 0, 1, 1)]
		public void TestDeclarantIsLastSenderNotOriginalDeclarant()
		{
			DocDeclaration docDeclaration = DocDeclaration.New(Declaration, Factory);
			AssertEquals("If no Broker set on Declaration and no Messages should be Current User.", GlbStaff.CurrentUser.GS_FullName, docDeclaration.DeclarantName);

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "XXX";
			staff.GS_FullName = staff.GS_Code + " - " + Guid.NewGuid().ToString();
			Declaration.JE_GS_NKCusAgent = staff.GS_Code;
			AssertEquals("If no messages should be Broker set on Declaration.", staff.GS_FullName, docDeclaration.DeclarantName);

			staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "YYY";
			staff.GS_FullName = staff.GS_Code + " - " + Guid.NewGuid().ToString();
			EDIMessage message = Declaration.CusEntryHeader.Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_SystemCreateTimeUtc = ZDateTime.Now;
			message.EM_SystemCreateUser = staff.GS_Code;
			AssertEquals("Should be Staff Member who sent the message.", staff.GS_FullName, docDeclaration.DeclarantName);

			staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ZZZ";
			staff.GS_FullName = staff.GS_Code + " - " + Guid.NewGuid().ToString();
			message = Declaration.CusEntryHeader.Messages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_SystemCreateTimeUtc = ZDateTime.Now;
			message.EM_SystemCreateUser = staff.GS_Code;
			AssertEquals("Should be Staff Member who sent the most RECENT message.", staff.GS_FullName, docDeclaration.DeclarantName);
		}

		public void TestDefaultHoldingPremisesWhenImporterIsATFFacility()
		{
			Declaration.JE_OH_Importer = Factory.New<OrgHeader>().PK;
			OrgHeader importer = Declaration.Importer;
			DocDeclaration declaration = DocDeclaration.New(Declaration, Factory);
			Assert(declaration.DefaultHoldingPremises == null);

			OrgCusCode code = importer.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "");
			code.OK_OA_PremisesAddress = Factory.New<OrgAddress>().PK;
			OrgAddress add = code.PremisesAddress;
			declaration = DocDeclaration.New(Declaration, Factory);
			Assert(declaration.DefaultHoldingPremises != null);
		}

		public void TestMultipleATFCodesChosesCorrectOne()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "JJJIMP";
			importer.OH_FullName = "JJ Johnson Imports Pty. Ltd.";

			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_OH_Importer = importer.PK;
			jobDeclaration.JE_ATFOtherInfoValue = "1273T";
			Factory.Save();

			var docDeclaration = DocDeclaration.New(jobDeclaration, Factory);
			Assert(docDeclaration.DefaultHoldingPremises == null);

			var importerAddress1 = importer.Addresses.AddNew();
			importerAddress1.OA_Address1 = "100 Smith St.";
			importerAddress1.OA_PostCode = "2030";
			importerAddress1.OA_City = "Sydney";
			importerAddress1.OA_State = "NSW";
			var atfCode1 = importer.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "1273T", "NZ");
			atfCode1.OK_OA_PremisesAddress = importerAddress1.PK;

			docDeclaration = DocDeclaration.New(jobDeclaration, Factory);
			Assert(docDeclaration.DefaultHoldingPremises != null);
			AssertEquals("JJ Johnson Imports Pty. Ltd. 100 Smith St.  Sydney", docDeclaration.DefaultHoldingPremises.CompanyNameAddress1Address2City);

			var importerAddress2 = importer.Addresses.AddNew();
			importerAddress2.OA_Address1 = "Unit 3, 57 Cargo Rd.";
			importerAddress2.OA_Address2 = "Botany";
			importerAddress2.OA_PostCode = "2250";
			importerAddress2.OA_City = "Sydney";
			importerAddress2.OA_State = "NSW";

			var atfCode2 = importer.CustomsCodes.AddNew(OrgCusCode.NZCodeTypes.ApprovedTransitionalFacility, "5982K", "NZ");
			atfCode2.OK_OA_PremisesAddress = importerAddress2.PK;
			var addr2 = atfCode2.PremisesAddress;
			jobDeclaration.JE_ATFOtherInfoValue = "5982K";
			docDeclaration = DocDeclaration.New(jobDeclaration, Factory);
			Assert(docDeclaration.DefaultHoldingPremises != null);
			AssertEquals("JJ Johnson Imports Pty. Ltd.", docDeclaration.DefaultHoldingPremises.CompanyName);
			AssertEquals("DefaultHoldingPremises should recognise the use of the second ATF code", "Unit 3, 57 Cargo Rd.", docDeclaration.DefaultHoldingPremises.Address1);
			AssertEquals("DefaultHoldingPremises should recognise the use of the second ATF code", "Botany", docDeclaration.DefaultHoldingPremises.Address2);
			AssertEquals("JJ Johnson Imports Pty. Ltd. Unit 3, 57 Cargo Rd. Botany Sydney", docDeclaration.DefaultHoldingPremises.CompanyNameAddress1Address2City);
		}

		public void TestLocalCustomsClientCode()
		{
			AssertEquals("", DeclarationWrapper.LocalCustomsClientCode);
			GlbCompany.CurrentCompany.OrgProxy.LocalCustomsClientCode = "LCC1";
			AssertEquals("LCC1", DeclarationWrapper.LocalCustomsClientCode);
		}

		public void TestDefaultHoldingPremisesWhenHasDepotAddress()
		{
			DocDeclaration dec = DocDeclaration.New(Declaration, Factory);
			Assert(dec.DefaultHoldingPremises == null);
			Declaration.DepotDocAddress.E2_OA_Address = Factory.New<OrgAddress>().PK;
			dec = DocDeclaration.New(Declaration, Factory);
			Assert(dec.DefaultHoldingPremises != null);
		}

		public void TestNZDocsMAFCoverSheet()
		{
			AssertEquals("DeclarationWrapper.MAFCS.GetType()", typeof(DocMAFCoverSheet), DeclarationWrapper.MAFCS.GetType());
		}

		public override void TestTransportModeDescription()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("TransportModeDescription", "Air", DeclarationWrapper.TransportModeDescription);

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("TransportModeDescription", "Sea", DeclarationWrapper.TransportModeDescription);
		}

		public override void TestPaymentMethod()
		{
			Declaration.JE_PaymentMethod = Enterprise.Customs.NZ.Business.PaymentMethodList.Codes.CashPaidByBroker;
			AssertEquals("PaymentMethod", Enterprise.Customs.NZ.Business.PaymentMethodList.Descriptions.CashPaidByBroker, DeclarationWrapper.PaymentMethod);
		}

		public void TestOriginalEntryNumber()
		{
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Completion;
			Declaration.JE_OriginalEntryNumber = "ABC";
			AssertEquals("OriginalEntruNumber", "ABC", DeclarationWrapper.OriginalEntryNumber);

			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_OriginalEntryNumber = "XYZ";
			AssertEquals("OriginalEntruNumber", "", DeclarationWrapper.OriginalEntryNumber);
		}

		public void TestCountryOfDestination()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			Declaration.JE_RL_NKFinalDestination = "SGSIN";
			AssertEquals("Pre-condition: value defaulting from JE_RL_NKFinalDestination", "SG - Singapore", DeclarationWrapper.CountryOfDestination);

			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.TSW;
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMP_ZZZHK";
			importer.OH_IsConsignee = true;
			importer.OH_RL_NKClosestPort = "HKHKG";
			Declaration.JE_OH_Importer = importer.PK;

			var deliveryTo = Factory.New<OrgHeader>();
			deliveryTo.OH_Code = "IMP_ZZZCN";
			deliveryTo.OH_IsConsignee = true;
			deliveryTo.OH_RL_NKClosestPort = "CNJEM";

			AssertEquals("value defaults for TSW from Importer if no Delivery Destination has been entered.", "HK - Hong Kong", DeclarationWrapper.CountryOfDestination);

			var deliveryAddress = Declaration.DeliveryDestinationPartyDocAddress.OrganisationPK = deliveryTo.PK;
			AssertEquals("value defaults for TSW from Delivery Destination if it has been entered.", "CN - China", DeclarationWrapper.CountryOfDestination);

			Declaration.JE_RL_NKFinalDestination = "SGSIN";
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			AssertEquals("Value defaults from standard location of JE_RL_NKFinalDestination for non-TSW declarations", "SG - Singapore", DeclarationWrapper.CountryOfDestination);
		}

		public override void TestPortOfFirstArrival()
		{
			Assert("Port of first arrival is not used in NZ", true);
		}

		public override void TestDateOfFirstArrival()
		{
			Assert("Functionality not required in NZ", true);
		}

		public void TestDocContainerAndPackageInfos()
		{
			Assert("Just to make sure the object gets instantiated", DeclarationWrapper.DocContainerAndPackageInfos != null);
		}

		public void TestMultiplePackageAndTypeForDocuments()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "AE", "AM", "AP", "AT", "BG", "FX", "5H");

			Declaration.DisableDefaultPackingInformation = true;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;

			Bill houseBill = Declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "TestHB1";

			Enterprise.Customs.Business.BasePackingGroup packingGroup = (Declaration.PackingGroups.Count > 0) ? Declaration.PackingGroups[0] : Declaration.PackingGroups.AddNew();
			packingGroup.CR_CU_HouseBill = houseBill.PK;
			packingGroup.CR_CO_Container = ZGuid.Empty;

			Package pack1 = Declaration.Packages.AddNew();
			pack1.CW_CR_HouseContainer = packingGroup.PK;
			pack1.CW_PackQty = 11;
			pack1.CW_PackType = "AE";

			Package pack2 = Declaration.Packages.AddNew();
			pack2.CW_CR_HouseContainer = packingGroup.PK;
			pack2.CW_PackQty = 22;
			pack2.CW_PackType = "AM";

			Package pack3 = Declaration.Packages.AddNew();
			pack3.CW_CR_HouseContainer = packingGroup.PK;
			pack3.CW_PackQty = 33;
			pack3.CW_PackType = "AP";

			Package pack4 = Declaration.Packages.AddNew();
			pack4.CW_CR_HouseContainer = packingGroup.PK;
			pack4.CW_PackQty = 44;
			pack4.CW_PackType = "AT";

			Package pack5 = Declaration.Packages.AddNew();
			pack5.CW_CR_HouseContainer = packingGroup.PK;
			pack5.CW_PackQty = 55;
			pack5.CW_PackType = "BG";

			Package pack6 = Declaration.Packages.AddNew();
			pack6.CW_CR_HouseContainer = packingGroup.PK;
			pack6.CW_PackQty = 66;
			pack6.CW_PackType = "FX";

			Package pack7 = Declaration.Packages.AddNew();
			pack7.CW_CR_HouseContainer = packingGroup.PK;
			pack7.CW_PackQty = 77;
			pack7.CW_PackType = "5H";

			AssertEquals("Pre-condition: Declaration.Packages.Count", 7, Declaration.Packages.Count);
			Assert("Make sure the object gets instantiated", DeclarationWrapper.DocContainerAndPackageInfos != null);
			AssertEquals("Declaration.Packages", "11 AE, 22 AM, 33 AP, 44 AT, 55 BG, 66 FX, 77 5H", DeclarationWrapper.DocContainerAndPackageInfos[0].PackagesAndType);
		}

		public void TestPackageAndContainersOverflow()
		{
			UniversalReferenceHelperTest.InitialiseUNEPackageTypeList(Factory, "AE", "AM", "AP", "AT", "BG", "FX", "5H");

			Declaration.DisableDefaultPackingInformation = true;
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Normal;
			Declaration.JE_TransportMode = JobTransportModeList.Codes.Air;
			AssertEquals("Pre-condition", false, DeclarationWrapper.HasOverflowContainersAndPackaging);

			Bill houseBill = Declaration.Bills.AddNew();
			houseBill.CU_BillType = BillTypeList.Codes.HouseBill;
			houseBill.CU_HouseBill = "TestHB1";

			CusContainer container1 = Declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "CONT1";
			container1.CO_ContainerSize = "23";
			container1.CO_FCL_LCL_AIR = "LCL";

			CusContainer container2 = Declaration.CusContainers.AddNew();
			container2.CO_ContainerNumber = "CONT2";
			container2.CO_ContainerSize = "23";
			container2.CO_FCL_LCL_AIR = "LCL";

			CusContainer container3 = Declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "CONT3";
			container3.CO_ContainerSize = "23";
			container3.CO_FCL_LCL_AIR = "LCL";

			CusContainer container4 = Declaration.CusContainers.AddNew();
			container4.CO_ContainerNumber = "CONT4";
			container4.CO_ContainerSize = "23";
			container4.CO_FCL_LCL_AIR = "LCL";

			CusContainer container5 = Declaration.CusContainers.AddNew();
			container5.CO_ContainerNumber = "CONT5";
			container5.CO_ContainerSize = "23";
			container5.CO_FCL_LCL_AIR = "LCL";

			CusContainer container6 = Declaration.CusContainers.AddNew();
			container6.CO_ContainerNumber = "CONT6";
			container6.CO_ContainerSize = "23";
			container6.CO_FCL_LCL_AIR = "LCL";

			Enterprise.Customs.Business.BasePackingGroup packingGroup = (Declaration.PackingGroups.Count > 0) ? Declaration.PackingGroups[0] : Declaration.PackingGroups.AddNew();
			packingGroup.CR_CU_HouseBill = houseBill.PK;
			packingGroup.CR_CO_Container = container1.PK;

			Package pack1 = Declaration.Packages.AddNew();
			pack1.CW_CR_HouseContainer = packingGroup.PK;
			pack1.CW_PackQty = 11;
			pack1.CW_PackType = "AE";

			Package pack2 = Declaration.Packages.AddNew();
			pack2.CW_CR_HouseContainer = packingGroup.PK;
			pack2.CW_PackQty = 22;
			pack2.CW_PackType = "AM";

			Package pack3 = Declaration.Packages.AddNew();
			pack3.CW_CR_HouseContainer = packingGroup.PK;
			pack3.CW_PackQty = 33;
			pack3.CW_PackType = "AP";

			Package pack4 = Declaration.Packages.AddNew();
			pack4.CW_CR_HouseContainer = packingGroup.PK;
			pack4.CW_PackQty = 44;
			pack4.CW_PackType = "AT";

			Package pack5 = Declaration.Packages.AddNew();
			pack5.CW_CR_HouseContainer = packingGroup.PK;
			pack5.CW_PackQty = 55;
			pack5.CW_PackType = "BG";

			Package pack6 = Declaration.Packages.AddNew();
			pack6.CW_CR_HouseContainer = packingGroup.PK;
			pack6.CW_PackQty = 66;
			pack6.CW_PackType = "FX";

			Package pack7 = Declaration.Packages.AddNew();
			pack7.CW_CR_HouseContainer = packingGroup.PK;
			pack7.CW_PackQty = 77;
			pack7.CW_PackType = "5H";

			var packingGroup2 = Declaration.PackingGroups.AddNew();
			packingGroup2.CR_CU_HouseBill = houseBill.PK;
			packingGroup2.CR_CO_Container = container2.PK;

			var packingGroup3 = Declaration.PackingGroups.AddNew();
			packingGroup3.CR_CU_HouseBill = houseBill.PK;
			packingGroup3.CR_CO_Container = container3.PK;

			var packingGroup4 = Declaration.PackingGroups.AddNew();
			packingGroup4.CR_CU_HouseBill = houseBill.PK;
			packingGroup4.CR_CO_Container = container4.PK;
			AssertEquals("Pre-condition - 4 container/packing group lines can appear on Entry Print document", false, DeclarationWrapper.HasOverflowContainersAndPackaging);

			var packingGroup5 = Declaration.PackingGroups.AddNew();
			packingGroup5.CR_CU_HouseBill = houseBill.PK;
			packingGroup5.CR_CO_Container = container5.PK;

			var packingGroup6 = Declaration.PackingGroups.AddNew();
			packingGroup6.CR_CU_HouseBill = houseBill.PK;
			packingGroup6.CR_CO_Container = container6.PK;

			AssertEquals("Pre-condition: Declaration.Packages.Count", 7, Declaration.Packages.Count);
			Assert("Make sure the object gets instantiated", DeclarationWrapper.DocContainerAndPackageInfos != null);
			AssertEquals("Declaration.Packages", "11 AE, 22 AM, 33 AP, 44 AT, 55 BG, 66 FX, 77 5H", DeclarationWrapper.DocContainerAndPackageInfos[0].PackagesAndType);

			AssertEquals("HasOverflowContainersAndPackaging", true, DeclarationWrapper.HasOverflowContainersAndPackaging);
			Assert("DocContainerAndPackageOverflow object should get instantiated", DeclarationWrapper.DocContainerAndPackageOverflow != null);
			AssertEquals("DocContainerAndPackageOverflow should have two lines", 2, DeclarationWrapper.DocContainerAndPackageOverflow.Count);
			AssertEquals("Overlfow container 1", "CONT5", DeclarationWrapper.DocContainerAndPackageOverflow[0].ContainerNumber);
			AssertEquals("Overlfow container 2", "CONT6", DeclarationWrapper.DocContainerAndPackageOverflow[1].ContainerNumber);
		}
		public void TestMiscellaneousDescriptionsCollection()
		{
			Declaration.JE_GoodsDescription = "GOODS";
			Declaration.DisableDefaultPackingInformation = true;
			SetupPackages();
			StmNote note = Declaration.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.MarksAndNumbers.Description;
			note.ST_NoteText = "MarksAndNumbers";

			Factory.Save();

			AssertNotNull(DeclarationWrapper.MiscellaneousDescriptions);
			Assert(DeclarationWrapper.MiscellaneousDescriptions.Count > 0);
			Assert("Misc Desc contains Goods Descr", DeclarationWrapper.MiscellaneousDescriptions[0].Description.IndexOf("GOODS") > -1);
			Assert("Misc Desc contains Marks and Numbers", DeclarationWrapper.MiscellaneousDescriptions[0].Description.IndexOf("MarksAndNumbers") > -1);
			Assert("Misc Desc contains PackageInfo", DeclarationWrapper.MiscellaneousDescriptions[0].Description.IndexOf(GetExpectedPackagesInfoString()) > -1);
		}
		protected abstract void SetupPackages();

		protected abstract ZString GetExpectedPackagesInfoString();

		public void TestEntryStyle()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_MessageSubType = JobMessageSubTypeList.Codes.Periodic;
			AssertEquals(JobMessageSubTypeList.Descriptions.Periodic, DeclarationWrapper.EntryStyle);
		}

		public void TestCustomsDeliveryInstructions()
		{
			Declaration.CustomsDeliveryInstructions = "Get Milk.";
			AssertEquals("Delivery Instructions should map.", "Get Milk.", DeclarationWrapper.CustomsDeliveryInstructions);
		}

		public void CustomsDeliveryInstructionsWithITR()
		{
			Declaration.CustomsDeliveryInstructions = "Response Status: ITR-International Transhipment Approved";
			AssertEquals("Delivery Instructions should still map for instructions with ITR", "Response Status: ITR-International Transhipment Approved", DeclarationWrapper.CustomsDeliveryInstructions);
		}

		public void TestVoyageNo()
		{
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			Declaration.JE_VoyageFlightNo = "VOY 222";

			AssertEquals("VoyageNo for Sea should be voyage number", "VOY 222", DeclarationWrapper.VoyageNo);

			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			Declaration.JE_VoyageFlightNo = "QF123";
			AssertEquals("VoyageNo for Air should be empty", "", DeclarationWrapper.VoyageNo);
		}

		public void TestCraftFlight()
		{
			ZString nKVessel = ZArchitecture.Core.Utilities.GetFieldFromRandomRowInTable(RefVesselSchema.Constants.RV_Code, RefVesselSchema.Constants.TableName).ToString();
			Declaration.JE_TransportMode = Declaration.TransportModeSeaCodeForTesting;
			Declaration.JE_VesselName = nKVessel;
			Declaration.JE_VoyageFlightNo = "VOY 222";

			AssertEquals("CraftFlight for Sea should be vessel name", nKVessel, DeclarationWrapper.CraftFlight);

			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			Declaration.JE_VoyageFlightNo = "FLY 888";
			AssertEquals("CraftFlight for Sea should be the flight number", "FLY 888", DeclarationWrapper.CraftFlight);
		}

		public void TestAgent()
		{
			if (Declaration.Branch != null && Declaration.Branch.OrgProxy != null)
			{
				AssertEquals("Agent should be Organisation name", Declaration.Branch.OrgProxy.OH_FullName, DeclarationWrapper.Agent);
			}
			else
			{
				AssertEquals("Agent should be empty", ZString.Empty, DeclarationWrapper.Agent);
			}
		}

		public void TestCasperCode()
		{
			OrgHeader supplier = GetSupplierWithCustomsCode(OrgCusCode.CodeTypes.CustomsClientCode);
			Declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("Casper Code:", "CustomsCode", DeclarationWrapper.CasperCode);
		}

		public void TestGSTRegNo()
		{
			OrgHeader supplier = GetSupplierWithCustomsCode(OrgCusCode.CodeTypes.GSTCode);
			Declaration.JE_OH_Supplier = supplier.PK;
			AssertEquals("GST Reg. No.:", "CustomsCode", DeclarationWrapper.GSTRegNo);
		}

		public void TestVoyageOrFlight()
		{
			ZDateTime currentDate = new ZDateTime(2006, 2, 2);
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			Declaration.JE_DateOfArrival = currentDate;
			Declaration.JE_TransportMode = Declaration.TransportModeAirCodeForTesting;
			Declaration.JE_VoyageFlightNo = "FLY 888";

			AssertEquals("Flight details should include date", "FLY 888/02-Feb-06", DeclarationWrapper.FlightOrVoyage);
		}

		public void TestIntendedShipmentMonth()
		{
			ZDateTime exportDate = new ZDateTime(2004, 04, 02);
			Declaration.JE_ExportDate = exportDate;
			AssertEquals("IntendedShipmentMonth", "April", DeclarationWrapper.IntendedShipmentMonth);
		}

		public void TestImporterCustomsClientCode()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "IMP_ZZZNZ";
			importer.OH_IsConsignee = true;

			var clientCode = importer.CustomsCodes.AddNew();
			clientCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			clientCode.OK_CustomsRegNo = "51352368J";
			clientCode.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			var supplier = GetSupplierWithCustomsCode(OrgCusCode.CodeTypes.SupplierCode);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Declaration.JE_OH_Supplier = importer.PK;
			Declaration.JE_OH_Importer = supplier.PK;
			AssertEquals("ImporterClientCode on Export job should return the Importer(Supplier) CSC code:", "CustomsCode", DeclarationWrapper.ImporterClientCode);

			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_OH_Supplier = supplier.PK;
			Declaration.JE_OH_Importer = importer.PK;
			AssertEquals("ImporterClientCode on Export job should return the Importer CCD code:", "51352368J", DeclarationWrapper.ImporterClientCode);
		}

		#region Implementation

		protected override string TestingCountry
		{
			get { return Enterprise.Core.Constants.CountryCodes.NewZealand; }
		}

		#region GetSupplierWithCustomsCode
		OrgHeader GetSupplierWithCustomsCode(ZString code)
		{
			var result = Factory.New<OrgHeader>();
			result.OH_Code = "NEWSUPPLIER";
			result.OH_IsConsignor = true;

			OrgCusCode cCD = result.CustomsCodes.AddNew();
			cCD.OK_CodeType = code;
			cCD.OK_CustomsRegNo = "CustomsCode";
			cCD.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			return result;
		}
		#endregion

		#region CreateNewStmSystemDefinedField
		protected void CreateNewStmSystemDefinedField(ZString fieldName)
		{
			TestCaseHelper.ClearTable(StmSystemDefinedField.Schema.TableName);

			StmSystemDefinedFieldCollection collection = new StmSystemDefinedFieldCollection(Factory);
			StmSystemDefinedField field1 = collection.AddNew();
			field1.S1_Name = fieldName;
			field1.S1_BusinessContext = Declaration.DocumentSupporter.BusinessContext.ToString();

			Factory.Save();
		}
		#endregion
		#endregion
	}
}
