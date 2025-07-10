using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.EMCS.Testing
{
	[TestedType(typeof(EMCSDeclarationWrapper))]
	class EMCSDeclarationWrapperTest : TestCaseWithFactory
	{
		public void TestWrapper()
		{
			AssertExceptionThrown<ArgumentException>(() => new EMCSDeclarationWrapper(null, Factory));
		}

		public void TestLines()
		{
			declaration = Factory.New<EMCSJobDeclaration>();
			EMCSDeclarationWrapperTestHelper.AddInvoiceLine(declaration, isMainPack: true, "Main Pack", 15.5m, 10.4m, "Main Tariff", Core.Constants.CountryCodes.Portugal);
			EMCSDeclarationWrapperTestHelper.AddInvoiceLine(declaration, isMainPack: false, "Other Pack", 15.5m, 10.4m, "Other Tariff", Core.Constants.CountryCodes.Portugal);
			wrapper = EMCSDeclarationWrapper.New(declaration, Factory);
			AssertEquals(2, wrapper.Lines.Count);
			AssertEquals("Main Tariff", wrapper.Lines[0].Box19CommodityCode);
			AssertEquals("Other Tariff", wrapper.Lines[1].Box19CommodityCode);
		}

		public virtual void TestWrapperProperties()
		{
			EMCSDeclarationWrapperTestHelper.SetupReferenceTestData(Factory);

			declaration = Factory.New<EMCSJobDeclaration>();
			declaration.JE_DeclarationReference = "Foo";
			declaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.Consignor;
			declaration.JE_DateAtOrigin = ZDateTime.BrettsBirthday;
			declaration.InvoiceNumber = "INV0001";
			declaration.InvoiceDate = ZDateTime.BrettsBirthday;
			declaration.JourneyTimeNumericPart = 11;
			declaration.JourneyTimeFormatPart = JourneyTimeUnitList.Codes.Days;
			declaration.EADNumber = "EADNUM1234";
			declaration.JE_GoodsOrigin = Core.Constants.CountryCodes.Portugal;
			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.UnitedKingdom;
			wrapper = EMCSDeclarationWrapper.New(declaration, Factory);

			CombineAssertions(() =>
			{
				AssertSupplierDocumentaryAddress();
				AssertEquals(nameof(wrapper.Box03ReferenceNumber), declaration.JE_DeclarationReference, wrapper.Box03ReferenceNumber);
				AssertEquals(nameof(wrapper.Box05CommercialUse), string.Empty, wrapper.Box05CommercialUse);
				AssertEquals(nameof(wrapper.Box06CommercialUse), string.Empty, wrapper.Box06CommercialUse);
				AssertImporterDocumentaryAddress();
				AssertDeliveryWarehouseDocumentaryAddress();
				AssertOffice(EuOfficeCodesTypes.Codes.CompetentAuthorityOfDispatch);
				AssertTransporterDocumentaryAddress();
				AssertEquals(nameof(wrapper.Box10Guarantee), EMCSGuarantorTypeList.Codes.Consignor, wrapper.Box10Guarantee);
				AssertOtherTransportDetails();
				AssertEquals(nameof(wrapper.Box12CommercialUse), string.Empty, wrapper.Box12CommercialUse);
				AssertEquals(nameof(wrapper.Box13CommercialUse), string.Empty, wrapper.Box13CommercialUse);
				AssertProprietor();
				AssertEquals(nameof(wrapper.Box15CommercialUse), string.Empty, wrapper.Box15CommercialUse);
				AssertEquals(nameof(wrapper.Box16DateOfRemoval), ZDateTime.BrettsBirthday.ToString(EMCSDeclarationWrapperHelper.DateFormat), wrapper.Box16DateOfRemoval);
				AssertEquals(nameof(wrapper.Box17CommercialUse), string.Empty, wrapper.Box17CommercialUse);

				declaration.InvoiceLines.DeleteAll();
				AssertItem(1);
				AssertItem(2);
				AssertItem(3);

				AssertAdditionalInformation();
				AssertSignatory();
				AssertEquals(nameof(wrapper.Box05InvoiceNumber), declaration.InvoiceNumber, wrapper.Box05InvoiceNumber);
				AssertEquals(nameof(wrapper.Box06InvoiceDate), ZDateTime.BrettsBirthday.ToString(EMCSDeclarationWrapperHelper.DateFormat), wrapper.Box06InvoiceDate);
				AssertDispatchWarehouseDocumentaryAddress();
				AssertGuarantor();
				AssertEquals(nameof(wrapper.Box12CountryDispatch), declaration.JE_GoodsOrigin, wrapper.Box12CountryDispatch);
				AssertEquals(nameof(wrapper.Box13CountryDestination), declaration.JE_GoodsDestination, wrapper.Box13CountryDestination);
				AssertRepresentative();
				AssertOffice(EuOfficeCodesTypes.Codes.OfficeOfDispatch);
				AssertEquals(nameof(wrapper.Box17JourneyTime), declaration.JourneyTimeNumericPart + declaration.JourneyTimeFormatPart, wrapper.Box17JourneyTime);
				AssertEquals(nameof(wrapper.BoxZSubmissionReference), string.Empty, wrapper.BoxZSubmissionReference);
				AssertImportSAD();
				AssertEquals(nameof(wrapper.BoxZThirdCountryOrigin), string.Empty, wrapper.BoxZThirdCountryOrigin);
				AssertOffice(EuOfficeCodesTypes.Codes.OfficeOfDelivery);
				AssertEquals(nameof(wrapper.BoxZDateOfArrival), string.Empty, wrapper.BoxZDateOfArrival);
				AssertEquals(nameof(wrapper.BoxZGlobalConclusion), string.Empty, wrapper.BoxZGlobalConclusion);
				AssertEquals(nameof(wrapper.BoxZAdministrativeReferenceCode), declaration.EADNumber, wrapper.BoxZAdministrativeReferenceCode);
			});
		}

		void AssertSupplierDocumentaryAddress()
		{
			var party = new PartyAddressTestData();
			var org = EMCSDeclarationWrapperTestHelper.AddOrganisation(declaration, DocAddressType.SupplierDocumentaryAddress, party);
			AssertEquals(nameof(wrapper.Box01ConsignorNameAndAddress), party.ExpectedDocumentaryAddress, wrapper.Box01ConsignorNameAndAddress);
			AssertEquals(nameof(wrapper.Box02ConsignorExciseNumber), party.ExciseCode, wrapper.Box02ConsignorExciseNumber);
			AssertContains(nameof(wrapper.Box01ConsignorEori), party.EORINumber, wrapper.Box01ConsignorEori);

			var partyOverride = new PartyAddressTestData() { OverrideAddress = true };
			EMCSDeclarationWrapperTestHelper.AddJobDocAddress(declaration, org, DocAddressType.SupplierDocumentaryAddress, partyOverride);
			AssertEquals("Address override", expected: true, declaration.SupplierDocumentaryAddress.E2_AddressOverride);
			AssertEquals(nameof(wrapper.Box01ConsignorNameAndAddress), partyOverride.ExpectedDocumentaryAddress, wrapper.Box01ConsignorNameAndAddress);
			AssertEquals(nameof(wrapper.Box02ConsignorExciseNumber), partyOverride.ExciseCodeOverride, wrapper.Box02ConsignorExciseNumber);
			AssertEquals(nameof(wrapper.Box01ConsignorEori), partyOverride.ExciseCodeOverride, wrapper.Box01ConsignorEori);
		}

		void AssertImporterDocumentaryAddress()
		{
			var party = new PartyAddressTestData();
			var org = EMCSDeclarationWrapperTestHelper.AddOrganisation(declaration, DocAddressType.ImporterDocumentaryAddress, party);
			AssertEquals(nameof(wrapper.Box04ConsigneeExciseNumber), party.ExciseCode, wrapper.Box04ConsigneeExciseNumber);
			AssertEquals(nameof(wrapper.Box07ConsigneeNameAddressAndNumber), party.ExpectedImporterDocumentaryAddress, wrapper.Box07ConsigneeNameAddressAndNumber);
			AssertEquals(nameof(wrapper.Box07ConsigneeNameAndAddress), party.ExpectedDocumentaryAddress, wrapper.Box07ConsigneeNameAndAddress);
			AssertContains(nameof(wrapper.Box07ConsigneeEori), party.EORINumber, wrapper.Box07ConsigneeEori);

			var partyOverride = new PartyAddressTestData() { OverrideAddress = true };
			EMCSDeclarationWrapperTestHelper.AddJobDocAddress(declaration, org, DocAddressType.ImporterDocumentaryAddress, partyOverride);
			AssertEquals("Address override", expected: true, declaration.ImporterDocumentaryAddress.E2_AddressOverride);
			AssertEquals(nameof(wrapper.Box04ConsigneeExciseNumber), partyOverride.ExciseCodeOverride, wrapper.Box04ConsigneeExciseNumber);
			AssertEquals(nameof(wrapper.Box07ConsigneeNameAddressAndNumber), partyOverride.ExpectedImporterDocumentaryAddress, wrapper.Box07ConsigneeNameAddressAndNumber);
			AssertEquals(nameof(wrapper.Box07ConsigneeNameAndAddress), partyOverride.ExpectedDocumentaryAddress, wrapper.Box07ConsigneeNameAndAddress);
			AssertEquals(nameof(wrapper.Box07ConsigneeEori), partyOverride.ExciseCodeOverride, wrapper.Box07ConsigneeEori);
		}

		void AssertDeliveryWarehouseDocumentaryAddress()
		{
			var party = new PartyAddressTestData();
			var org = EMCSDeclarationWrapperTestHelper.AddOrganisation(declaration, DocAddressType.DestinationWarehouse, party);
			AssertEquals(nameof(wrapper.Box07APlaceOfDelivery), party.ExpectedDocumentaryAddress, wrapper.Box07APlaceOfDelivery);
			AssertContains(nameof(wrapper.Box07APlaceOfDeliveryEori), party.EORINumber, wrapper.Box07APlaceOfDeliveryEori);

			var partyOverride = new PartyAddressTestData() { OverrideAddress = true };
			EMCSDeclarationWrapperTestHelper.AddJobDocAddress(declaration, org, DocAddressType.DestinationWarehouse, partyOverride);
			AssertEquals("Address override", expected: true, declaration.DestinationWarehouseDocumentaryAddress.E2_AddressOverride);
			AssertEquals(nameof(wrapper.Box07APlaceOfDelivery), partyOverride.ExpectedDocumentaryAddress, wrapper.Box07APlaceOfDelivery);
			AssertEquals(nameof(wrapper.Box07APlaceOfDeliveryEori), partyOverride.ExciseCodeOverride, wrapper.Box07APlaceOfDeliveryEori);
		}

		void AssertOffice(string codeType)
		{
			var office = new CustomsOfficeTestData(codeType);
			EMCSDeclarationWrapperTestHelper.AddCustomsOffice(declaration, office);
			switch (codeType)
			{
				case EuOfficeCodesTypes.Codes.CompetentAuthorityOfDispatch:
					AssertEquals(nameof(wrapper.Box08CustomsOffice), office.ExpectedCustomsOffice, wrapper.Box08CustomsOffice);
					break;
				case EuOfficeCodesTypes.Codes.OfficeOfDispatch:
					AssertEquals(nameof(wrapper.Box15DispatchOffice), office.ExpectedCustomsOffice, wrapper.Box15DispatchOffice);
					break;
				case EuOfficeCodesTypes.Codes.OfficeOfDelivery:
					AssertEquals(nameof(wrapper.BoxZDestinationOffice), office.ExpectedCustomsOffice, wrapper.BoxZDestinationOffice);
					break;
			}
		}

		void AssertTransporterDocumentaryAddress()
		{
			var party = new PartyAddressTestData();
			var org = EMCSDeclarationWrapperTestHelper.AddOrganisation(declaration, DocAddressType.Transporter, party);
			AssertEquals(nameof(wrapper.Box09Transporter), party.ExpectedDocumentaryAddress, wrapper.Box09Transporter);
			AssertContains(nameof(wrapper.Box09TransporterEori), party.EORINumber, wrapper.Box09TransporterEori);

			var partyOverride = new PartyAddressTestData() { OverrideAddress = true };
			EMCSDeclarationWrapperTestHelper.AddJobDocAddress(declaration, org, DocAddressType.Transporter, partyOverride);
			AssertEquals("Address override", expected: true, declaration.TransporterDocumentaryAddress.E2_AddressOverride);
			AssertEquals(nameof(wrapper.Box09Transporter), partyOverride.ExpectedDocumentaryAddress, wrapper.Box09Transporter);
			AssertEquals(nameof(wrapper.Box09TransporterEori), partyOverride.ExciseCodeOverride, wrapper.Box09TransporterEori);
		}

		void AssertOtherTransportDetails()
		{
			EMCSDeclarationWrapperTestHelper.AddContainer(declaration, "1", "CO0001", "SEAL1", "SI0001");
			EMCSDeclarationWrapperTestHelper.AddContainer(declaration, "2", "CO0002", "SEAL2", "SI0002");
			var expected = $"CO0001{EMCSDeclarationWrapperHelper.Delimiter}1{EMCSDeclarationWrapperHelper.Delimiter}SEAL1{EMCSDeclarationWrapperHelper.Delimiter}SI0001{System.Environment.NewLine}CO0002{EMCSDeclarationWrapperHelper.Delimiter}2{EMCSDeclarationWrapperHelper.Delimiter}SEAL2{EMCSDeclarationWrapperHelper.Delimiter}SI0002";
			AssertEquals(nameof(wrapper.Box11OtherTransportDetails), expected, wrapper.Box11OtherTransportDetails);
		}

		void AssertProprietor()
		{
			var party = new PartyAddressTestData();
			var org = EMCSDeclarationWrapperTestHelper.AddOrganisation(declaration, DocAddressType.OwnerOfGoods, party);
			AssertEquals(nameof(wrapper.Box14Proprietor), party.ExpectedDocumentaryAddress, wrapper.Box14Proprietor);

			var partyOverride = new PartyAddressTestData() { OverrideAddress = true };
			EMCSDeclarationWrapperTestHelper.AddJobDocAddress(declaration, org, DocAddressType.OwnerOfGoods, partyOverride);
			AssertEquals("Address override", expected: true, declaration.SupplierDocumentaryAddress.E2_AddressOverride);
			AssertEquals(nameof(wrapper.Box14Proprietor), partyOverride.ExpectedDocumentaryAddress, wrapper.Box14Proprietor);
		}

		void AssertAdditionalInformation()
		{
			EMCSDeclarationWrapperTestHelper.AddInvoiceLine(declaration, 0m);
			EMCSDeclarationWrapperTestHelper.AddInvoiceLine(declaration, 0m);
			EMCSDeclarationWrapperTestHelper.AddInvoiceLine(declaration, 0m);
			EMCSDeclarationWrapperTestHelper.AddInvoiceLine(declaration, 1m);
			AssertEquals(nameof(wrapper.Box25AdditionalInformation), string.Empty, wrapper.Box25AdditionalInformation);
			declaration.InvoiceLines[2].ZG_SizeOfProducer = 2m;
			var expectedText = $"It is hereby certified that the beer described has been produced by an independent small brewery with a production in the previous year of {2} hectolitres.";
			AssertEquals(nameof(wrapper.Box25AdditionalInformation), expectedText, wrapper.Box25AdditionalInformation);
		}

		void AssertSignatory()
		{
			var party = new PartyAddressTestData();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = EMCSDeclarationWrapperTestHelper.AddOrganisation(declaration, DocAddressType.None, party).PK;
			Factory.Save();
			var expected = $"{party.Name}{EMCSDeclarationWrapperHelper.Delimiter}{party.Phone}";
			AssertEquals(nameof(wrapper.Box26ASignatoryCompanyAndPhone), expected, wrapper.Box26ASignatoryCompanyAndPhone);

			GlbStaff.CurrentUser.GS_FullName = "Niels Bohr";
			AssertEquals(nameof(wrapper.Box26BSignatoryName), "Niels Bohr", wrapper.Box26BSignatoryName);
			var staff = EMCSDeclarationWrapperTestHelper.AddStaff(Factory, "AR1", "Antonio");
			declaration.JE_GS_NKCusAgent = staff.GS_Code;
			AssertEquals(nameof(wrapper.Box26BSignatoryName), staff.GS_FullName, wrapper.Box26BSignatoryName);

			expected = $"{party.City}{EMCSDeclarationWrapperHelper.Delimiter}{ZDateTime.Now.ToString(EMCSDeclarationWrapperHelper.DateFormat)}";
			AssertEquals(nameof(wrapper.Box26CSignatoryPlaceAndDate), expected, wrapper.Box26CSignatoryPlaceAndDate);

			AssertEquals(nameof(wrapper.Box26DSignatorySignature), string.Empty, wrapper.Box26DSignatorySignature);
		}

		void AssertDispatchWarehouseDocumentaryAddress()
		{
			var party = new PartyAddressTestData();
			var org = EMCSDeclarationWrapperTestHelper.AddOrganisation(declaration, DocAddressType.DispatchWarehouse, party);
			AssertEquals(nameof(wrapper.Box08DispatcherNameAddress), party.ExpectedDocumentaryAddress, wrapper.Box08DispatcherNameAddress);
			AssertContains(nameof(wrapper.Box08DispatcherEORI), party.EORINumber, wrapper.Box08DispatcherEORI);

			var partyOverride = new PartyAddressTestData() { OverrideAddress = true };
			EMCSDeclarationWrapperTestHelper.AddJobDocAddress(declaration, org, DocAddressType.DispatchWarehouse, partyOverride);
			AssertEquals("Address override", expected: true, declaration.DispatchWarehouseDocumentaryAddress.E2_AddressOverride);
			AssertEquals(nameof(wrapper.Box08DispatcherNameAddress), partyOverride.ExpectedDocumentaryAddress, wrapper.Box08DispatcherNameAddress);
			AssertEquals(nameof(wrapper.Box08DispatcherEORI), party.EORINumber, wrapper.Box08DispatcherEORI);
		}

		void AssertImportSAD()
		{
			EMCSDeclarationWrapperTestHelper.AddSADNumbers(declaration, new string[] { "SAD1", string.Empty, "SAD2", "SAD3", string.Empty });
			AssertEquals(nameof(wrapper.BoxZImportSAD), "SAD1,SAD2,SAD3", wrapper.BoxZImportSAD);
		}

		void AssertItem(int lineNumber)
		{
			var box = string.Empty;
			switch (lineNumber)
			{
				case 1:
					box = "A";
					break;
				case 2:
					box = "B";
					break;
				case 3:
					box = "C";
					break;
			}

			var box18 = string.Format("Box18{0}PackagesMarksAndDescription", box);
			var box19 = string.Format("Box19{0}CommodityCode", box);
			var box20 = string.Format("Box20{0}Quantity", box);
			var box21 = string.Format("Box21{0}GrossMass", box);
			var box22 = string.Format("Box22{0}NetMass", box);
			var box23 = string.Format("Box23{0}CustomsStatus", box);
			var box24Label = string.Format("Box24{0}SoldInWarehouseLabel", box);
			var box24 = string.Format("Box24{0}SoldInWarehouse", box);
			var box241Label = string.Format("Box24{0}ProducedInUKLabel", box);
			var box241 = string.Format("Box24{0}ProducedInUK", box);

			var invoiceLine = EMCSDeclarationWrapperTestHelper.AddInvoiceLine(declaration, isMainPack: true, $"Description {lineNumber}", 15.5m, 10.4m, $"{lineNumber}23456789", Core.Constants.CountryCodes.Portugal);
			var package1 = new PackageTestData(invoiceLine.JI_NDescription, 100, "BX", $"Shipping Mark {lineNumber}", invoiceLine.ZG_AlcoholicStrength, invoiceLine.ZG_Density, countable: true);
			var package2 = new PackageTestData(invoiceLine.JI_NDescription, 1, "PL", "Shipping Mark", invoiceLine.ZG_AlcoholicStrength, invoiceLine.ZG_Density, countable: false);
			EMCSDeclarationWrapperTestHelper.AddPackages(invoiceLine, new[] { package1, package2 });

			var expected = $"{package1.GetExpected()}{System.Environment.NewLine}{package2.GetExpected()}";
			AssertEquals(box18, expected, wrapper.GetPropertyValue(box18));

			AssertEquals(box19, invoiceLine.JI_Tariff, wrapper.GetPropertyValue(box19));

			invoiceLine.JI_CustomsQuantity = new Random().NextDouble();
			invoiceLine.JI_CustomsUnitQty = "3";
			expected = $"{invoiceLine.JI_CustomsQuantity}{EMCSDeclarationWrapperHelper.Delimiter}{invoiceLine.CustomsUnitQtyDescription}";
			AssertEquals(box20, expected, wrapper.GetPropertyValue(box20));
			invoiceLine.JI_CustomsQuantity = new Random().NextDouble();
			invoiceLine.JI_CustomsUnitQty = "5";
			expected = $"{invoiceLine.JI_CustomsQuantity}";
			AssertEquals(box20, expected, wrapper.GetPropertyValue(box20));

			invoiceLine.JI_Weight = new Random().NextDouble();
			invoiceLine.JI_WeightUQ = "KG";
			expected = $"{invoiceLine.JI_Weight}{EMCSDeclarationWrapperHelper.Delimiter}{invoiceLine.JI_WeightUQ}";
			AssertEquals(box21, expected, wrapper.GetPropertyValue(box21));

			invoiceLine.JI_NetWeight = new Random().NextDouble();
			invoiceLine.JI_NetWeightUQ = "KG";
			expected = $"{invoiceLine.JI_NetWeight}{EMCSDeclarationWrapperHelper.Delimiter}{invoiceLine.JI_NetWeightUQ}";
			AssertEquals(box22, expected, wrapper.GetPropertyValue(box22));

			AssertEquals(box23, string.Empty, wrapper.GetPropertyValue(box23));

			AssertEquals(box24Label, "24a\r\nSold in\r\nwarehouse?", wrapper.GetPropertyValue(box24Label));
			AssertEquals(box24, ZBool.False.ToYesNoString(), wrapper.GetPropertyValue(box24));

			AssertEquals(box241Label, "UK produced?", wrapper.GetPropertyValue(box241Label));
			AssertEquals(box241, ZBool.False.ToYesNoString(), wrapper.GetPropertyValue(box241));
			invoiceLine.ZG_Origin = Core.Constants.CountryCodes.UnitedKingdom;
			AssertEquals(box241, ZBool.True.ToYesNoString(), wrapper.GetPropertyValue(box241));
		}

		void AssertGuarantor()
		{
			declaration.SupplierDocumentaryAddress.Delete();
			var party = new PartyAddressTestData();
			var org = EMCSDeclarationWrapperTestHelper.AddOrganisation(declaration, DocAddressType.SupplierDocumentaryAddress, party);
			AssertEquals(nameof(wrapper.Box10Guarantor), party.ExpectedDocumentaryAddress, wrapper.Box10Guarantor);

			var partyOverride = new PartyAddressTestData() { OverrideAddress = true };
			EMCSDeclarationWrapperTestHelper.AddJobDocAddress(declaration, org, DocAddressType.SupplierDocumentaryAddress, partyOverride);
			AssertEquals("Address override", expected: true, declaration.SupplierDocumentaryAddress.E2_AddressOverride);
			AssertEquals(nameof(wrapper.Box10Guarantor), partyOverride.ExpectedDocumentaryAddress, wrapper.Box10Guarantor);
		}

		void AssertRepresentative()
		{
			var expectedRepresentative = EMCSDeclarationWrapperHelper.GetRepresentative(declaration);
			AssertEquals(nameof(wrapper.Box14Representative), expectedRepresentative, wrapper.Box14Representative);
		}

		EMCSJobDeclaration declaration;
		EMCSDeclarationWrapper wrapper;
	}
}
