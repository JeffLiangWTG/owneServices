using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentWrappers.Customs.EU;
using Enterprise.DocumentWrappers.Customs.EU.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using ESCusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH.Testing
{
	[MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Spain)]
	abstract class ESDocSADHTest : DocSADHTest
	{
		public void TestShowBox2SupplierCountryCodeES()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ESCusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var wrapper = GetNewDocSADH(entryHeader);
			AssertEquals(false, wrapper.ShowBox2SupplierCountryCode);
		}

		public void TestShowBox8ImporterCountryCodeES()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			ESCusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var wrapper = GetNewDocSADH(entryHeader);
			AssertEquals(false, wrapper.ShowBox8ImporterCountryCode);
		}

		public void TestBox6TotalNoOfPackages_OnlyPackages()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			CombineAssertions(() =>
			{
				DocSADH wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("Total Packages in Box 6 is empty", "0", wrapper.Box6TotalNoOfPacks);
				var cw = declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
				cw.CW_PackQty = 1;
				var pack1 = (InvoiceLinePackagePivot)invoiceLine.PackagesPivot.AddPivotFor(cw);
				pack1.CHC_NumberOfPacks = 5;
				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("Total Packages in Box 6 has the correct value", "5", wrapper.Box6TotalNoOfPacks);
				var cw2 = declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
				cw2.CW_PackQty = 1;
				var pack2 = (InvoiceLinePackagePivot)invoiceLine.PackagesPivot.AddPivotFor(cw2);
				pack2.CHC_NumberOfPacks = 5;
				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("Total Packages in Box 6 has the correct value", "10", wrapper.Box6TotalNoOfPacks);
			});
		}

		public void TestBox6TotalNoOfPackages_OnlyVehicles()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryHeader = declaration.CustomsEntryHeaders[0];

			CombineAssertions(() =>
			{
				DocSADH wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("Total Packages in Box 6 is empty", "0", wrapper.Box6TotalNoOfPacks);

				var vehicle = invoiceLine.Vehicles.AddNew();
				vehicle.CVH_VehicleIdentificationNumber = "Vin1";
				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("Total Packages in Box 6 has the correct value", "1", wrapper.Box6TotalNoOfPacks);

				var vehicle2 = invoiceLine2.Vehicles.AddNew();
				vehicle2.CVH_VehicleIdentificationNumber = "Vin2";
				var vehicle3 = invoiceLine3.Vehicles.AddNew();
				vehicle3.CVH_VehicleIdentificationNumber = "Vin3";
				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("Total Packages in Box 6 has the correct value", "3", wrapper.Box6TotalNoOfPacks);
			});
		}

		public void TestBox6TotalNoOfPackages_PackagesAndVehicles()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
			var invoiceLine4 = invoiceHeader.InvoiceLines.AddNew();

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryHeader = declaration.CustomsEntryHeaders[0];

			CombineAssertions(() =>
			{
				DocSADH wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("Total Packages in Box 6 is empty", "0", wrapper.Box6TotalNoOfPacks);

				var cw = declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
				cw.CW_PackQty = 1;
				var pack1 = (InvoiceLinePackagePivot)invoiceLine.PackagesPivot.AddPivotFor(cw);
				pack1.CHC_NumberOfPacks = 5;
				var cw2 = declaration.Bills.AddNew().PackingGroups.AddNew().Packages.AddNew();
				cw2.CW_PackQty = 1;
				var pack2 = (InvoiceLinePackagePivot)invoiceLine.PackagesPivot.AddPivotFor(cw2);
				pack2.CHC_NumberOfPacks = 5;

				var vehicle2 = invoiceLine2.Vehicles.AddNew();
				vehicle2.CVH_VehicleIdentificationNumber = "Vin1";
				var vehicle3 = invoiceLine3.Vehicles.AddNew();
				vehicle3.CVH_VehicleIdentificationNumber = "Vin2";
				var vehicle4 = invoiceLine4.Vehicles.AddNew();
				vehicle4.CVH_VehicleIdentificationNumber = "Vin3";
				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("Total Packages in Box 6 has the correct value", "13", wrapper.Box6TotalNoOfPacks);
			});
		}

		public void TestBox7ReferenceNumberES()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			var wrapper = GetNewDocSADH(entryHeader);

			CombineAssertions(() =>
			{
				AssertEquals("Box7ReferenceNumber Empty if Declarant Reference is empty", ZString.Empty, wrapper.Box7ReferenceNumber);

				declaration.JE_OwnerRef = "Declarant Reference";
				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("Box7ReferenceNumber has the value of Declarant Reference", "Declarant Reference", wrapper.Box7ReferenceNumber);
			});
		}

		public void TestBox18IdentityOfTransportAtDeparture_ImportES()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.EU.Business.MessageTypeList.Codes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = GetNewDocSADH(entryHeader);

			declaration.ZG_Box18TransportID = "RAIL 1234";
			AssertEquals("declaration.ZG_Box18TransportID", "RAIL 1234", wrapper.Box18IdentityOfTransportAtDeparture);
		}

		public void TestBox18IdentityOfTransportAtDeparture_ExportES()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Enterprise.Customs.EU.Business.MessageTypeList.Codes.Export;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = GetNewDocSADH(entryHeader);

			declaration.ZG_Box18TransportID = "RAIL 1234";
			AssertEquals("declaration.ZG_Box18TransportID", "RAIL 1234", wrapper.Box18IdentityOfTransportAtDeparture);
		}

		public void TestBox20IncoTermAgreedPlaceCodeAndCode2()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			CombineAssertions("No Incoterm information at all", () =>
			{
				DocSADH wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("Box20AgreedPlaceCode", ZString.Empty, wrapper.Box20AgreedPlaceCode);
				AssertEquals("Box20AgreedPlaceCode2 last box", ZString.Empty, wrapper.Box20AgreedPlaceCode2);
			});

			declaration.ZG_AgreedPlaceCode = "3";

			CombineAssertions("IncoTerm Information available for declaration", () =>
			{
				DocSADH wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("Box20AgreedPlaceCode", ExpectedBox20AgreedPlaceCode, wrapper.Box20AgreedPlaceCode);
				AssertEquals("Box20AgreedPlaceCode2 last box", ExpectedBox20AgreedPlaceCode2, wrapper.Box20AgreedPlaceCode2);
			});

			invoiceHeader.ZG_AgreedPlaceCode = "1";

			CombineAssertions("IncoTerm Information available for declaration and Invoiceheader", () =>
			{
				DocSADH wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("Box20AgreedPlaceCode", ExpectedBox20AgreedPlaceCode, wrapper.Box20AgreedPlaceCode);
				AssertEquals("Box20AgreedPlaceCode2 last box", ExpectedBox20AgreedPlaceCode2, wrapper.Box20AgreedPlaceCode2);
			});
		}

		public void TestBox20IncoTermES()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			var wrapper = GetNewDocSADH(entryHeader);

			CombineAssertions(() =>
			{
				AssertEquals("Box20ShipmentIncoTerm empty when all incoterms are empty", "", wrapper.Box20ShipmentIncoTerm);

				declaration.JE_ShipmentIncoTerm = "DAP";
				AssertEquals("Box20ShipmentIncoTerm with declaration incoterm when header one is empty", "DAP", wrapper.Box20ShipmentIncoTerm);

				invoiceHeader.JZ_IncoTerm = "EXW";
				AssertEquals("Box20ShipmentIncoTerm with header incoterm when not empty", "EXW", wrapper.Box20ShipmentIncoTerm);
			});
		}

		public void TestBox20AgreedPlaceES()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "BLT";
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			ESCusEntryHeader entryHeader = declaration.CustomsEntryHeaders[0];

			var wrapper = GetNewDocSADH(entryHeader);

			CombineAssertions(() =>
			{
				AssertEquals("Box20AgreedPlace empty when all incoterms are empty", "", wrapper.Box20AgreedPlace);

				declaration.JE_ShipmentIncoTermPlace = "SYDNEY";
				AssertEquals("Box20AgreedPlace with declaration incoterm when header one is empty", "SYDNEY", wrapper.Box20AgreedPlace);

				invoiceHeader.JZ_IncoTermPlace = "MADRID";
				AssertEquals("Box20AgreedPlace with header incoterm when not empty", "MADRID", wrapper.Box20AgreedPlace);
			});
		}

		public void TestBox30LocationOfGoods()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			AssertBox30LocationOfGoodsES(declaration, entryHeader, "", "", "");
			AssertBox30LocationOfGoodsES(declaration, entryHeader, "LHR", "", "LHR");
			AssertBox30LocationOfGoodsES(declaration, entryHeader, "LHR", "BAC", "LHRLHRBAC");
			AssertBox30LocationOfGoodsES(declaration, entryHeader, "AA", "A", "AAAAA");

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var goodsLocation = entryInstruction.GoodsLocation;
			goodsLocation.Address.AuthorisationNumber = "9999000002";
			AssertBox30LocationOfGoodsES(declaration, entryHeader, "", "", "9999000002");
			goodsLocation.Address.AuthorisationNumber = "9999000003";
			AssertBox30LocationOfGoodsES(declaration, entryHeader, "LHR", "", "9999000003");
			goodsLocation.Address.AuthorisationNumber = "9999000004";
			AssertBox30LocationOfGoodsES(declaration, entryHeader, "LHR", "BAC", "9999000004");
			goodsLocation.Address.AuthorisationNumber = null;
			AssertBox30LocationOfGoodsES(declaration, entryHeader, "AA", "A", "AAAAA");
		}

		protected void AssertBox30LocationOfGoodsES(JobDeclaration declaration, ESCusEntryHeader entryHeader, ZString locationOfGoods, ZString shedCode, ZString expectedAssertion)
		{
			declaration.JE_LocationOfGoods = locationOfGoods;
			if (!shedCode.IsEmpty)
			{
				declaration.SubLocation = shedCode;
			}
			DocSADH wrapper = GetNewDocSADH(entryHeader);
			AssertEquals(expectedAssertion, wrapper.Box30LocationOfGoods);
		}

		public void TestBoxDSignature()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			CombineAssertions(() =>
			{
				DocSADH wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("Box D is empty because there is no MRN", ZString.Empty, wrapper.BoxDSignature);

				entryHeader.MovementReferenceNumber = "MRN-TEST";

				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("Box D is notempty because there is MRN", "AUTENTICACION INFORMATICA, ART. 98 R/UE 952/2013", wrapper.BoxDSignature);
			});
		}

		public void TestBox54Place()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				JobDeclaration declaration = Factory.New<JobDeclaration>();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();

				declaration.JE_CustomsOffice = customsOfficeCode;

				DocSADH wrapper = GetNewDocSADH(entryHeader);

				var place54 = wrapper.Box54Place;
				CombineAssertions(() =>
				{
					AssertEquals("Box54Place is correct", attributeValue, place54);
					AssertEquals("Cached Box54Place", wrapper.Box54Place, place54);
				});
			}
		}

		public void TestBox54NameOfDeclarantAndRepresentative()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = GetDeclarationType;
			declaration.JE_OA_DeclarantAddress = ZGuid.Empty;
			declaration.JE_GB = ZGuid.Empty;

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			OrgHeader declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_FullName = "Declarant S.A.";
			OrgAddress declarantOA = declarant.Addresses.AddNewMainAddress();

			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "Importer S.A.";

			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "Supplier S.A.";

			var wrapper = GetNewDocSADH(entryHeader);
			CombineAssertions(() =>
			{
				AssertEquals("Empty if there is no Declarant or Importer/Supplier", ZString.Empty, wrapper.Box54NameOfDeclarantAndRepresentative);

				declaration.JE_OH_Importer = importer.PK;
				declaration.JE_OH_Supplier = supplier.PK;
				var expectedBox54IfNoDeclarant = declaration.IsImport() ? declaration.Importer.OH_FullName : declaration.Supplier.OH_FullName;
				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("If no Declarant then Importer for Imports or Exporter for Export", expectedBox54IfNoDeclarant, wrapper.Box54NameOfDeclarantAndRepresentative);

				declaration.JE_OA_DeclarantAddress = declarantOA.PK;
				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("If Declarant then Declarant Full Name", "Declarant S.A.", wrapper.Box54NameOfDeclarantAndRepresentative);
			});
		}

		public void TestBox54UserDetailsES()
		{
			var declaration = Factory.New<JobDeclaration>();

			var branch = Factory.New<GlbBranch>();
			branch.GB_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;
			branch.GB_RL_NKHomePort = "ESMAD";
			branch.GB_BranchName = "Taric";
			branch.GB_Phone = "91 554 1006";
			branch.GB_Fax = "91 554 1006";
			declaration.JE_GB = branch.PK;

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "AZ";
			staff.GS_FullName = "Ana Zayat Test";
			staff.GS_LoginName = "aztest";
			staff.StaffPlainTextPassword = "1234";
			staff.GS_WorkPhone = "91 554 1123";
			staff.GS_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;

			ESCusEntryHeader entryHeader = declaration.CustomsEntryHeaders.AddNew();
			CombineAssertions(() =>
			{
				var wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("entryHeader.Box54SignatoryNameAndPosition: No Message and CusAgent", "", wrapper.Box54SignatoryNameAndPosition);
				AssertEquals("entryHeader.Box54SignatoryContactDetails: No Message and CusAgent", "Tel:+34 915 54 10 06", wrapper.Box54SignatoryContactDetails);

				wrapper = GetNewDocSADH(entryHeader);
				declaration.JE_GS_NKCusAgent = staff.GS_Code;
				AssertEquals("entryHeader.Box54SignatoryNameAndPosition: has CusAgent", "Ana Zayat Test", wrapper.Box54SignatoryNameAndPosition);
				AssertEquals("entryHeader.Box54SignatoryContactDetails: has CusAgent", "Tel:+34 915 54 10 06  Direct:+34 915 54 11 23", wrapper.Box54SignatoryContactDetails);

				var cert = staff.Certificates.AddNew();
				cert.XZ_Type = "NID";
				cert.XZ_RefNumber = "12345678A";

				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals("entryHeader.Box54SignatoryNameAndPosition: has CusAgent and Certificate", "Ana Zayat Test", wrapper.Box54SignatoryNameAndPosition);
				AssertEquals("entryHeader.Box54SignatoryContactDetails: has CusAgent and Certificate", "NIF:12345678A", wrapper.Box54SignatoryContactDetails);
			});
		}

		[TestDate(1995, 02, 16)]
		public void TestBox54Date()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			DocSADH wrapper = GetNewDocSADH(entryHeader);
			AssertEquals("Box54Date should have ES format", "16-02-1995", wrapper.Box54Date);
		}

		public void TestBoxABarcode()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();

			CombineAssertions(() =>
			{
				var wrapper = GetNewDocSADH(entryHeader);
				AssertEquals($"{nameof(ESDocSADH.BoxABarcode)} is empty because there is no MRN", ZString.Empty, wrapper.BoxABarcode);

				entryHeader.MovementReferenceNumber = "MRN-TEST";

				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals($"When MRN is set, {nameof(ESDocSADH.BoxABarcode)}", "*MRN-TEST*", wrapper.BoxABarcode);

				entryHeader.EntryNumber = "ENUMBER";

				wrapper = GetNewDocSADH(entryHeader);
				AssertEquals($"When EntryNumber is set, {nameof(ESDocSADH.BoxABarcode)}", "*ENUMBER*", wrapper.BoxABarcode);
			});
		}

		protected abstract DocSADH GetNewDocSADH(ESCusEntryHeader entryHeader);

		public void TestBox54Details_Box54NameOfDeclarantAndRepresentativeES()
		{
			var broker = CreateBroker("F_N");
			var (declaration, entryHeader) = CreateEntryHeader(broker.GS_Code);

			var wrapper = GetNewDocSADH((ESCusEntryHeader)entryHeader);
			ZString expectedBrokers = "EDI CUSTOMS BROKERS";

			CombineAssertions(() =>
			{
				declaration.JE_DeclarantType = Enterprise.Customs.ES.Business.Declaration.ESRepresentationTypeList.Codes._2Direct;
				AssertEquals("entryHeader.Box54NameOfDeclarantAndRepresentative for Representation " + Enterprise.Customs.ES.Business.Declaration.ESRepresentationTypeList.Codes._2Direct, expectedBrokers, wrapper.Box54NameOfDeclarantAndRepresentative);

				declaration.JE_DeclarantType = Enterprise.Customs.ES.Business.Declaration.ESRepresentationTypeList.Codes._1Auto;
				AssertEquals("entryHeader.Box54NameOfDeclarantAndRepresentative for Representation " + Enterprise.Customs.ES.Business.Declaration.ESRepresentationTypeList.Codes._1Auto, expectedBrokers, wrapper.Box54NameOfDeclarantAndRepresentative);

				declaration.JE_DeclarantType = Enterprise.Customs.ES.Business.Declaration.ESRepresentationTypeList.Codes._3Indirect;
				AssertEquals("entryHeader.Box54NameOfDeclarantAndRepresentative for Representation " + Enterprise.Customs.ES.Business.Declaration.ESRepresentationTypeList.Codes._3Indirect, expectedBrokers, wrapper.Box54NameOfDeclarantAndRepresentative);

				var oh = Factory.New<OrgHeader>();
				oh.OH_FullName = "Daniel Declaring party";
				var oa = oh.Addresses.AddNewMainAddress();
				declaration.JE_OA_DeclarantAddress = oa.PK;
				declaration.JE_DeclarantType = Enterprise.Customs.ES.Business.Declaration.ESRepresentationTypeList.Codes._2Direct;
				AssertEquals("entryHeader.Box54NameOfDeclarantAndRepresentative for Representation " + Enterprise.Customs.ES.Business.Declaration.ESRepresentationTypeList.Codes._2Direct, (ZString)"Daniel Declaring party", wrapper.Box54NameOfDeclarantAndRepresentative);
			});
		}

		public void TestBox54Details_Box54PlaceES()
		{
			var (_, entryHeader) = CreateEntryHeader();
			var wrapper = GetNewDocSADH((ESCusEntryHeader)entryHeader);
			AssertEquals("entryHeader.Box54Place", ZString.Empty, wrapper.Box54Place);
		}

		public void TestIsIndirectExportES()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = "EXP";
				declaration.CustomsOffices.RemoveAndDeleteAll();
				var cusOffice = declaration.CustomsOffices.AddNew();
				cusOffice.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfExit;
				cusOffice.CY_Data = "FR000025";
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();

				var wrapper = GetNewDocSADH(entryHeader);

				AssertEquals(true, wrapper.IsIndirectExport);
			}
		}

		protected abstract ZString ExpectedBox20AgreedPlaceCode { get; }

		protected abstract ZString ExpectedBox20AgreedPlaceCode2 { get; }

		protected abstract ZString GetDeclarationType { get; }

		protected override ZString CountrySpecificCurrency => "EUR";

		protected override ZString ExpectedEadBarCode => ZString.Empty;

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
			var refCusCodeListCustoms = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Spain, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, customsOfficeCode, "Office Description" + customsOfficeCode, ZDateTime.Today.AddYears(-2), ZDateTime.Today.AddYears(2));
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeListCustoms.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.City, attributeValue);
			Factory.Save();
		}

		readonly ZString customsOfficeCode = "ES009999";
		readonly ZString attributeValue = "MADRID";
	}
}
