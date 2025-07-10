using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE429;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.DocumentWrappers;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument.Testing;

class IDD429WrapperTest : Enterprise.DocumentWrappers.Testing.DocBaseWrapperTest
{
	protected override DocBaseWrapper GetNewDocumentWrapper()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.Messages.Add(message);
		return IDD429Wrapper.New(message);
	}

	public void TestDeclarationAcceptanceDateTime()
	{
		messageObject.DeclarationStatus.StateDateTime = "2024-01-02";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("DeclarationAcceptanceDateTime", "02/01/2024 00:00:00", wrapper.DeclarationAcceptanceDateTime);
	}

	public void TestReleaseDateTime()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.EntryInstruction.CEI_Style = DeltaIEImportDeclarationTypeList.Codes.I1;
		messageObject.DeclarationStatus.StateDateTime = "2024-01-02";
		var wrapper = new IDD429Wrapper(message, entryHeader, Factory);
		AssertEquals("ReleaseDateTime", ZString.Empty, wrapper.ReleaseDateTime);

		entryHeader.EntryInstruction.CEI_Style = DeltaIEImportDeclarationTypeList.Codes.H1;
		wrapper = new IDD429Wrapper(message, entryHeader, Factory);
		AssertEquals("ReleaseDateTime", "02/01/2024 00:00:00", wrapper.ReleaseDateTime);
	}

	public void TestRelatedRelaseDateTime()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		entryHeader.EntryInstruction.CEI_Style = DeltaIEImportDeclarationTypeList.Codes.H1;

		var message1 = Factory.New<FREDIMessage>();
		message1.EM_ReceiveTransmit = "TRX";
		message1.EM_MessageSubType = "433";
		message1.EM_ApplicationCode = EDIMessage.ApplicationCodes.FRCustomsMessage;
		message1.EM_Status = EDIMessage.Status.Queued;
		message1.MessageNumberStrategy = new FRMessageNumberStrategy(Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
		message1.EM_MessageText = "&lt;&lt;MSGNO PLACEHOLDER&gt;&gt; + <<MSGNO PLACEHOLDER>>";
		entryHeader.Messages.Add(message1);
		Factory.Save();

		entryHeader.Messages.Add(message);
		Factory.Save();
		message.EM_ReceiveTransmit = "RCV";
		messageObject.ImportOperation.ReleaseDate = "2024-01-02";
		var wrapper = new IDD429Wrapper(message, entryHeader, Factory);
		AssertEquals("RelatedRelaseDateTime", ZString.Empty, wrapper.RelatedRelaseDateTime);

		entryHeader.EntryInstruction.CEI_Style = DeltaIEImportDeclarationTypeList.Codes.I1;
		wrapper = new IDD429Wrapper(message, entryHeader, Factory);
		AssertEquals("RelatedRelaseDateTime", "02/01/2024 00:00:00", wrapper.RelatedRelaseDateTime);
	}

	[TestDate(2024, 01, 02, 03, 04, 05)]
	public void TestPrintDateTime()
	{
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("PrintDateTime", "02/01/2024 03:04:05", wrapper.PrintDateTime);
	}

	public void TestDeclarationStatus()
	{
		var wrapper = new IDD429Wrapper(message, Factory);
		messageObject.DeclarationStatus.State = "BONAENLEVER";
		AssertEquals("DeclarationStatus", "BON A ENLEVER", wrapper.DeclarationStatus);

		messageObject.DeclarationStatus.State = "TESTABCD";
		AssertEquals("DeclarationStatus", "TESTABCD", wrapper.DeclarationStatus);
	}

	public void TestDeclarationType()
	{
		messageObject.ImportOperation.DeclarationType = "DeclarationType";
		messageObject.ImportOperation.AdditionalDeclarationType = "AdditionalDeclarationType";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("DeclarationType", "DeclarationType AdditionalDeclarationType", wrapper.DeclarationType);
	}

	public void TestAdditionalDeclarationType()
	{
		messageObject.ImportOperation.AdditionalDeclarationType = "AdditionalDeclarationType";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("AdditionalDeclarationType should be empty", ZString.Empty, wrapper.AdditionalDeclarationType);
	}

	public void TestSupervisingCustomsOffice()
	{
		messageObject.SupervisingCustomsOffice = new MScoType();
		messageObject.SupervisingCustomsOffice.ReferenceNumber = "SupervisingCustomsOffice";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("SupervisingCustomsOffice", "SupervisingCustomsOffice", wrapper.SupervisingCustomsOffice);
	}

	public void TestPresentationCustomsOffice()
	{
		messageObject.CustomsOfficeOfPresentation.ReferenceNumber = "PresentationCustomsOffice";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("PresentationCustomsOffice", "PresentationCustomsOffice", wrapper.PresentationCustomsOffice);
	}

	public void TestImporterName()
	{
		messageObject.Importer.Name = "ImporterName";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("ImporterName", "ImporterName", wrapper.ImporterName);
	}

	public void TestImporterEORI()
	{
		messageObject.Importer.IdentificationNumber = "ImporterEORI";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("ImporterEORI", "ImporterEORI", wrapper.ImporterEORI);
	}

	public void TestImporterAddress()
	{
		messageObject.Importer.Address = new MAddressType01();
		messageObject.Importer.Address.StreetAndNumber = "ImporterAddress";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("ImporterAddress", "ImporterAddress", wrapper.ImporterAddress);
	}

	public void TestImporterPostCode()
	{
		messageObject.Importer.Address = new MAddressType01();
		messageObject.Importer.Address.Postcode = "ImporterPostCode";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("ImporterPostCode", "ImporterPostCode", wrapper.ImporterPostCode);
	}

	public void TestImporterCity()
	{
		messageObject.Importer.Address = new MAddressType01();
		messageObject.Importer.Address.City = "ImporterCity";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("ImporterCity", "ImporterCity", wrapper.ImporterCity);
	}

	public void TestImporterCountry()
	{
		messageObject.Importer.Address = new MAddressType01();
		messageObject.Importer.Address.Country = "ImporterCountry";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("ImporterCountry", "ImporterCountry", wrapper.ImporterCountry);
	}

	public void TestDeclarantName()
	{
		messageObject.Declarant.Name = "DeclarantName";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("DeclarantName", "DeclarantName", wrapper.DeclarantName);
	}

	public void TestDeclarantEORI()
	{
		messageObject.Declarant.IdentificationNumber = "DeclarantEORI";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("DeclarantEORI", "DeclarantEORI", wrapper.DeclarantEORI);
	}

	public void TestDeclarantAddress()
	{
		messageObject.Declarant.Address = new MAddressType01();
		messageObject.Declarant.Address.StreetAndNumber = "DeclarantAddress";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("DeclarantAddress", "DeclarantAddress", wrapper.DeclarantAddress);
	}

	public void TestDeclarantPostCode()
	{
		messageObject.Declarant.Address = new MAddressType01();
		messageObject.Declarant.Address.Postcode = "DeclarantPostCode";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("DeclarantPostCode", "DeclarantPostCode", wrapper.DeclarantPostCode);
	}

	public void TestDeclarantCity()
	{
		messageObject.Declarant.Address = new MAddressType01();
		messageObject.Declarant.Address.City = "DeclarantCity";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("DeclarantCity", "DeclarantCity", wrapper.DeclarantCity);
	}

	public void TestDeclarantCountry()
	{
		messageObject.Declarant.Address = new MAddressType01();
		messageObject.Declarant.Address.Country = "DeclarantCountry";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("DeclarantCountry", "DeclarantCountry", wrapper.DeclarantCountry);
	}

	public void TestRepresentativeEORI()
	{
		messageObject.Representative = new MRepresentativeType();
		messageObject.Representative.IdentificationNumber = "RepresentativeEORI";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("RepresentativeEORI", "RepresentativeEORI", wrapper.RepresentativeEORI);
	}

	public void TestRepresentativeType()
	{
		messageObject.Representative = new MRepresentativeType();
		messageObject.Representative.Status = "RepresentativeType";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("RepresentativeType", "RepresentativeType", wrapper.RepresentativeType);
	}

	public void TestRepresentativeContactName()
	{
		messageObject.Representative = new MRepresentativeType();
		messageObject.Representative.ContactPerson = new MContactPersonType();
		messageObject.Representative.ContactPerson.Name = "Rep Contact Name";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("RepresentativeContactName", "Rep Contact Name", wrapper.RepresentativeContactName);
	}

	public void TestRepresentativeContactPhoneNumber()
	{
		messageObject.Representative = new MRepresentativeType();
		messageObject.Representative.ContactPerson = new MContactPersonType();
		messageObject.Representative.ContactPerson.PhoneNumber = "123456789";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("RepresentativeContactPhoneNumber", "123456789", wrapper.RepresentativeContactPhoneNumber);
	}

	public void TestRepresentativeContactEMail()
	{
		messageObject.Representative = new MRepresentativeType();
		messageObject.Representative.ContactPerson = new MContactPersonType();
		messageObject.Representative.ContactPerson.EMailAddress = "rep@example.com";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("RepresentativeContactEMail", "rep@example.com", wrapper.RepresentativeContactEMail);
	}

	public void TestDeclarantContactName()
	{
		messageObject.Declarant.ContactPerson = new MContactPersonType();
		messageObject.Declarant.ContactPerson.Name = "Declarant Contact Name";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("DeclarantContactName", "Declarant Contact Name", wrapper.DeclarantContactName);
	}

	public void TestDeclarantContactPhoneNumber()
	{
		messageObject.Declarant.ContactPerson = new MContactPersonType();
		messageObject.Declarant.ContactPerson.PhoneNumber = "987654321";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("DeclarantContactPhoneNumber", "987654321", wrapper.DeclarantContactPhoneNumber);
	}

	public void TestDeclarantContactEMail()
	{
		messageObject.Declarant.ContactPerson = new MContactPersonType();
		messageObject.Declarant.ContactPerson.EMailAddress = "declarant@example.com";
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("DeclarantContactEMail", "declarant@example.com", wrapper.DeclarantContactEMail);
	}

	public void TestAuthorizationType()
	{
		var authorisation = new MAuthorisationType01();
		authorisation.Type = "ABC";
		messageObject.Authorisation = new Collection<MAuthorisationType01>();
		messageObject.Authorisation.Add(authorisation);
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("AuthorizationType", "ABC", wrapper.AuthorizationType);
	}

	public void TestAuthorizationNumber()
	{
		var authorisation = new MAuthorisationType01();
		authorisation.ReferenceNumber = "ABC";
		messageObject.Authorisation = new Collection<MAuthorisationType01>();
		messageObject.Authorisation.Add(authorisation);
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("AuthorizationNumber", "ABC", wrapper.AuthorizationNumber);
	}

	public void TestAuthorizationHolder()
	{
		var authorisation = new MAuthorisationType01();
		authorisation.HolderOfTheAuthorisation = "ABC";
		messageObject.Authorisation = new Collection<MAuthorisationType01>();
		messageObject.Authorisation.Add(authorisation);
		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("AuthorizationHolder", "ABC", wrapper.AuthorizationHolder);
	}

	public void TestGuaranteeNumber()
	{
		var guaranteeType = new MGuaranteeReferenceType();
		guaranteeType.GRN = "GRN12345";
		var guarantee = new MGuaranteeType();
		guarantee.GuaranteeReference.Add(guaranteeType);
		messageObject.Guarantee = new Collection<MGuaranteeType>();
		messageObject.Guarantee.Add(guarantee);
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("GuaranteeNumber", "GRN12345", wrapper.GuaranteeNumber);
	}

	public void TestGuaranteeOffice()
	{
		var guaranteeType = new MGuaranteeReferenceType();
		guaranteeType.CustomsOfficeOfGuarantee = new MCustomsOfficeOfGuaranteeType();
		guaranteeType.CustomsOfficeOfGuarantee.ReferenceNumber = "ABC12345";
		var guarantee = new MGuaranteeType();
		guarantee.GuaranteeReference.Add(guaranteeType);
		messageObject.Guarantee = new Collection<MGuaranteeType>();
		messageObject.Guarantee.Add(guarantee);
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("GuaranteeOffice", "ABC12345", wrapper.GuaranteeOffice);
	}

	public void TestAmountToBeCovered()
	{
		var guaranteeType = new MGuaranteeReferenceType();
		guaranteeType.AmountToBeCovered = 123;
		var guarantee = new MGuaranteeType();
		guarantee.GuaranteeReference.Add(guaranteeType);
		messageObject.Guarantee = new Collection<MGuaranteeType>();
		messageObject.Guarantee.Add(guarantee);
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("AmountToBeCovered", "123", wrapper.AmountToBeCovered);
	}

	public void TestGuaranteedAmount()
	{
		messageObject.GeneralTaxation = new GeneralTaxationType();
		messageObject.GeneralTaxation.TotalPayableTaxAmount = new TotalPayableTaxAmountType();
		messageObject.GeneralTaxation.TotalPayableTaxAmount.AmountUsed = new AmountUsedType();
		messageObject.GeneralTaxation.TotalPayableTaxAmount.AmountUsed.GuaranteedAmount = 100.02;
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("GuaranteedAmount", "100.02", wrapper.GuaranteedAmount);
	}

	public void TestUnguaranteedAmount()
	{
		messageObject.GeneralTaxation = new GeneralTaxationType();
		messageObject.GeneralTaxation.TotalPayableTaxAmount = new TotalPayableTaxAmountType();
		messageObject.GeneralTaxation.TotalPayableTaxAmount.AmountUsed = new AmountUsedType();
		messageObject.GeneralTaxation.TotalPayableTaxAmount.AmountUsed.UnguaranteedAmount = 50.02;
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("UnguaranteedAmount", "50.02", wrapper.UnguaranteedAmount);
	}

	public void TestTotalPayableTaxAmount()
	{
		messageObject.GeneralTaxation = new GeneralTaxationType();
		messageObject.GeneralTaxation.TotalPayableTaxAmount = new TotalPayableTaxAmountType();
		messageObject.GeneralTaxation.TotalPayableTaxAmount.TotalPayableTaxAmount = 150.02;
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("TotalPayableTaxAmount","150.02", wrapper.TotalPayableTaxAmount);
	}

	public void TestMRN()
	{
		messageObject.ImportOperation.MRN = "123456789";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("MRN", "123456789", wrapper.MRN);
	}

	public void TestTotalItemsCount()
	{
		goodsShipment.GoodsShipmentItem.Add(new MGoodsShipmentItemType04FR());
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("TotalItemsCount", "1", wrapper.TotalItemsCount);
	}

	public void TestAgreementNumber()
	{
		goodsShipment.AdditionalReference = new List<MAdditionalReferenceType>();
		var reference1 = new MAdditionalReferenceType();
		reference1.Type = "2DEC";
		reference1.ReferenceNumber = "123456";
		var reference2 = new MAdditionalReferenceType();
		reference1.Type = "1DEC";
		reference1.ReferenceNumber = "654321";
		goodsShipment.AdditionalReference.Add(reference1);
		goodsShipment.AdditionalReference.Add(reference2);
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("AgreementNumber", "654321", wrapper.AgreementNumber);
	}

	public void TestGoodsLocationType()
	{
		goodsShipment.Consignment.LocationOfGoods = new MLocationOfGoodsType01();
		goodsShipment.Consignment.LocationOfGoods.TypeOfLocation = "GoodsLocationType";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("GoodsLocationType", "GoodsLocationType", wrapper.GoodsLocationType);
	}

	public void TestGoodsLocationAddress()
	{
		goodsShipment.Consignment.LocationOfGoods = new MLocationOfGoodsType01();
		goodsShipment.Consignment.LocationOfGoods.Address = new MAddressType01();
		goodsShipment.Consignment.LocationOfGoods.Address.StreetAndNumber = "123 Main St";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("If the QualifierOfIdentification is not equal to 'Z', it should return an empty string.", ZString.Empty, wrapper.GoodsLocationAddress);

		goodsShipment.Consignment.LocationOfGoods.QualifierOfIdentification = Enterprise.Customs.Business.CusGoodsLocationQualifierList.Codes.Address;

		AssertEquals("GoodsLocationAddress", "123 Main St", wrapper.GoodsLocationAddress);
	}

	public void TestGoodsLocationPostCode()
	{
		goodsShipment.Consignment.LocationOfGoods = new MLocationOfGoodsType01();
		goodsShipment.Consignment.LocationOfGoods.Address = new MAddressType01();
		goodsShipment.Consignment.LocationOfGoods.Address.Postcode = "10001";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("If the QualifierOfIdentification is not equal to 'Z', it should return an empty string.", ZString.Empty, wrapper.GoodsLocationAddress);

		goodsShipment.Consignment.LocationOfGoods.QualifierOfIdentification = Enterprise.Customs.Business.CusGoodsLocationQualifierList.Codes.Address;

		AssertEquals("GoodsLocationPostCode", "10001", wrapper.GoodsLocationPostCode);
	}

	public void TestGoodsLocationCity()
	{
		goodsShipment.Consignment.LocationOfGoods = new MLocationOfGoodsType01();
		goodsShipment.Consignment.LocationOfGoods.Address = new MAddressType01();
		goodsShipment.Consignment.LocationOfGoods.Address.City = "New York";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("If the QualifierOfIdentification is not equal to 'Z', it should return an empty string.", ZString.Empty, wrapper.GoodsLocationAddress);

		goodsShipment.Consignment.LocationOfGoods.QualifierOfIdentification = Enterprise.Customs.Business.CusGoodsLocationQualifierList.Codes.Address;

		AssertEquals("GoodsLocationCity", "New York", wrapper.GoodsLocationCity);
	}

	public void TestGoodsLocationCountry()
	{
		goodsShipment.Consignment.LocationOfGoods = new MLocationOfGoodsType01();
		goodsShipment.Consignment.LocationOfGoods.Address = new MAddressType01();
		goodsShipment.Consignment.LocationOfGoods.Address.Country = "USA";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("If the QualifierOfIdentification is not equal to 'Z', it should return an empty string.", ZString.Empty, wrapper.GoodsLocationAddress);

		goodsShipment.Consignment.LocationOfGoods.QualifierOfIdentification = Enterprise.Customs.Business.CusGoodsLocationQualifierList.Codes.Address;

		AssertEquals("GoodsLocationCountry", "USA", wrapper.GoodsLocationCountry);
	}

	public void TestSupplierName()
	{
		goodsShipment.Exporter = new MExporterType();
		goodsShipment.Exporter.Name = "Supplier Inc.";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("SupplierName", "Supplier Inc.", wrapper.SupplierName);
	}

	public void TestSupplierEORI()
	{
		goodsShipment.Exporter = new MExporterType();
		goodsShipment.Exporter.IdentificationNumber = "EORI123456";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("SupplierEORI", "EORI123456", wrapper.SupplierEORI);
	}

	public void TestSupplierAddress()
	{
		goodsShipment.Exporter = new MExporterType();
		goodsShipment.Exporter.Address = new MAddressType01();
		goodsShipment.Exporter.Address.StreetAndNumber = "456 Elm St";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("SupplierAddress", "456 Elm St", wrapper.SupplierAddress);
	}

	public void TestSupplierPostCode()
	{
		goodsShipment.Exporter = new MExporterType();
		goodsShipment.Exporter.Address = new MAddressType01();
		goodsShipment.Exporter.Address.Postcode = "20002";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("SupplierPostCode", "20002", wrapper.SupplierPostCode);
	}

	public void TestSupplierCity()
	{
		goodsShipment.Exporter = new MExporterType();
		goodsShipment.Exporter.Address = new MAddressType01();
		goodsShipment.Exporter.Address.City = "Washington";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("SupplierCity", "Washington", wrapper.SupplierCity);
	}

	public void TestSupplierCountry()
	{
		goodsShipment.Exporter = new MExporterType();
		goodsShipment.Exporter.Address = new MAddressType01();
		goodsShipment.Exporter.Address.Country = "USA";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("SupplierCountry", "USA", wrapper.SupplierCountry);
	}

	public void TestConsigneeName()
	{
		goodsShipment.Consignee = new MConsigneeType();
		goodsShipment.Consignee.Name = "Consignee Ltd.";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("ConsigneeName", "Consignee Ltd.", wrapper.ConsigneeName);
	}

	public void TestConsigneeEori()
	{
		goodsShipment.Consignee = new MConsigneeType();
		goodsShipment.Consignee.IdentificationNumber = "EORI789123";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("ConsigneeEori", "EORI789123", wrapper.ConsigneeEori);
	}

	public void TestSupplyChainActorEORI()
	{
		goodsShipment.AdditionalSupplyChainActor = new List<MAdditionalSupplyChainActorType>
		{
			new MAdditionalSupplyChainActorType { IdentificationNumber = "EORI456789" }
		};
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("SupplyChainActorEORI", "EORI456789", wrapper.SupplyChainActorEORI);
	}

	public void TestSupplyChainActorRole()
	{
		goodsShipment.AdditionalSupplyChainActor = new List<MAdditionalSupplyChainActorType>
		{
			new MAdditionalSupplyChainActorType { Role = "Transporter" }
		};
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("SupplyChainActorRole", "Transporter", wrapper.SupplyChainActorRole);
	}

	public void TestCountryOfDestination()
	{
		goodsShipment.Destination = new MDestinationType();
		goodsShipment.Destination.CountryOfDestination = "Germany";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("CountryOfDestination", "Germany", wrapper.CountryOfDestination);
	}

	public void TestCountryOfDispatch()
	{
		goodsShipment.CountryOfDispatch = new MCountryOfDispatchType();
		goodsShipment.CountryOfDispatch.CountryOfDispatch = "France";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("CountryOfDispatch", "France", wrapper.CountryOfDispatch);
	}

	public void TestIncotermCode()
	{
		goodsShipment.DeliveryTerms = new MDeliveryTermsType();
		goodsShipment.DeliveryTerms.IncotermCode = "FOB";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("IncotermCode", "FOB", wrapper.IncotermCode);
	}

	public void TestIncotermUNLOCO()
	{
		goodsShipment.DeliveryTerms = new MDeliveryTermsType();
		goodsShipment.DeliveryTerms.UNLOCODE = "DEHAM";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("IncotermUNLOCO", "DEHAM", wrapper.IncotermUNLOCO);
	}

	public void TestIncotermLocation()
	{
		goodsShipment.DeliveryTerms = new MDeliveryTermsType();
		goodsShipment.DeliveryTerms.Location = "Hamburg";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("IncotermLocation", "Hamburg", wrapper.IncotermLocation);
	}

	public void TestIncotermCountry()
	{
		goodsShipment.DeliveryTerms = new MDeliveryTermsType();
		goodsShipment.DeliveryTerms.Country = "Germany";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("IncotermCountry", "Germany", wrapper.IncotermCountry);
	}

	public void TestIncotermText()
	{
		goodsShipment.DeliveryTerms = new MDeliveryTermsType();
		goodsShipment.DeliveryTerms.Text = "Free on Board";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("IncotermText", "Free on Board", wrapper.IncotermText);
	}

	public void TestWarehouseType()
	{
		goodsShipment.Warehouse = new MWarehouseType();
		goodsShipment.Warehouse.Type = "Cold Storage";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("WarehouseType", "Cold Storage", wrapper.WarehouseType);
	}

	public void TestTotalGrossMass()
	{
		goodsShipment.Warehouse = new MWarehouseType();
		goodsShipment.Consignment.GrossMass = 1000;
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("TotalGrossMass", "1000", wrapper.TotalGrossMass);
	}

	public void TestIsContainerised()
	{
		goodsShipment.Warehouse = new MWarehouseType();
		goodsShipment.Consignment.ContainerIndicator = "Yes";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("IsContainerised", "Yes", wrapper.IsContainerised);
	}

	public void TestContainerId()
	{
		goodsShipment.Consignment.TransportEquipment = new List<MTransportEquipmentType>
		{
			new MTransportEquipmentType { ContainerIdentificationNumber = "CONT123456" }
		};
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("ContainerId", "CONT123456", wrapper.ContainerId);
	}

	public void TestTransportMeansNationalityAtBorder()
	{
		goodsShipment.Consignment.ActiveBorderTransportMeans = new MActiveBorderTransportMeansType();
		goodsShipment.Consignment.ActiveBorderTransportMeans.Nationality = "USA";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("TransportMeansNationalityAtBorder", "USA", wrapper.TransportMeansNationalityAtBorder);
	}

	public void TestInlandModeOfTransport()
	{
		goodsShipment.Consignment.InlandModeOfTransport = "Truck";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("InlandModeOfTransport", "Truck", wrapper.InlandModeOfTransport);
	}

	public void TestModeOfTransportAtTheBorder()
	{
		goodsShipment.Consignment.ModeOfTransportAtTheBorder = "Air";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("ModeOfTransportAtTheBorder", "Air", wrapper.ModeOfTransportAtTheBorder);
	}

	public void TestArrivalTransportMeansIdType()
	{
		goodsShipment.Consignment.ArrivalTransportMeans = new MArrivalTransportMeansType();
		goodsShipment.Consignment.ArrivalTransportMeans.TypeOfIdentification = "ID Card";
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("ArrivalTransportMeansIdType", "ID Card", wrapper.ArrivalTransportMeansIdType);
	}

	public void TestSpecialMentions()
	{
		goodsShipment.AdditionalInformation = new List<MAdditionalInformationType>()
		{
			new MAdditionalInformationType { Code = "ABC" },
			new MAdditionalInformationType { Code = "DEF" }
		};
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("SpecialMentions", "ABC - DEF", wrapper.SpecialMentions);
	}

	public void TestFiscalReferences()
	{
		goodsShipment.AdditionalFiscalReference = new List<MAdditionalFiscalReferenceType>()
		{
			new MAdditionalFiscalReferenceType { FiscalReferenceIdentificationNumber = "ABC" },
			new MAdditionalFiscalReferenceType { FiscalReferenceIdentificationNumber = "DEF" }
		};
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("FiscalReferences", "ABC - DEF", wrapper.FiscalReferences);
	}

	public void TestAdditionalReferences()
	{
		goodsShipment.AdditionalReference = new List<MAdditionalReferenceType>()
		{
			new MAdditionalReferenceType { Type = "ABC" ,ReferenceNumber = "123" },
			new MAdditionalReferenceType { Type = "DEF" ,ReferenceNumber = "456" }
		};
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("AdditionalReferences", "ABC 123 - DEF 456", wrapper.AdditionalReferences);
	}

	public void TestPreviousDocuments()
	{
		goodsShipment.PreviousDocument = new List<MPreviousDocumentType04>()
		{
			new MPreviousDocumentType04 { Type = "ABC" ,ReferenceNumber = "123" },
			new MPreviousDocumentType04 { Type = "DEF" ,ReferenceNumber = "456" }
		};
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("PreviousDocuments", "ABC 123 - DEF 456", wrapper.PreviousDocuments);
	}

	public void TestSupportingDocuments()
	{
		goodsShipment.SupportingDocument = new List<MSupportingDocumentType02>()
		{
			new MSupportingDocumentType02 { Type = "ABC" ,ReferenceNumber = "123", DocumentLineItemNumber = "1" },
			new MSupportingDocumentType02 { Type = "DEF" ,ReferenceNumber = "456" }
		};
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("SupportingDocuments", "ABC 123(Fiche d'Imputation) - DEF 456", wrapper.SupportingDocuments);
	}

	public void TestTransportDocuments()
	{
		goodsShipment.Consignment.TransportDocument = new List<MTransportDocumentType>()
		{
			new MTransportDocumentType { Type = "ABC" ,ReferenceNumber = "123" },
			new MTransportDocumentType { Type = "DEF" ,ReferenceNumber = "456" }
		};
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("TransportDocuments", "ABC 123 - DEF 456", wrapper.TransportDocuments);
	}

	public void TestImputationSheets()
	{
		var supportingDocument1 = new List<MSupportingDocumentType01FR>
		{
			new MSupportingDocumentType01FR { Type = "A01" ,Quantity = 0, Amount = 0 },
			new MSupportingDocumentType01FR { Type = "A02" ,Quantity = 1, Amount = 0 },
		};

		var supportingDocument2 = new List<MSupportingDocumentType01FR>
		{
			new MSupportingDocumentType01FR { Type = "A03" ,Quantity = 0, Amount = 1 },
		};

		goodsShipment.GoodsShipmentItem = new List<MGoodsShipmentItemType04FR>()
		{
			new MGoodsShipmentItemType04FR { SupportingDocument = supportingDocument1 },
			new MGoodsShipmentItemType04FR { SupportingDocument = supportingDocument2 }
		};
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("ImputationSheets", 2, wrapper.ImputationSheets.Count);
		AssertEquals("ImputationSheets", "A02", (wrapper.ImputationSheets.First() as IDDImputationSheet429Wrapper).DocumentType);
	}

	public void TestItems()
	{
		goodsShipment.GoodsShipmentItem = new List<MGoodsShipmentItemType04FR>()
		{
			new MGoodsShipmentItemType04FR { NatureOfTransaction = "A01" },
			new MGoodsShipmentItemType04FR { NatureOfTransaction = "A02" }
		};
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("Items", 2, wrapper.Items.Count);
		AssertEquals("Items", "A01", wrapper.Items[0].NatureOfTransaction);
		AssertContainsExactElementsInExactOrder(new [] { "1", "2" }, wrapper.Items.Select(item => ((IDD429ItemWrapper)item).ItemNumber));
	}

	public void TestNatureOfTransaction()
	{
		goodsShipment.NatureOfTransaction = "NatureOfTransaction";

		var wrapper = new IDD429Wrapper(message, Factory);
		AssertEquals("NatureOfTransaction", "NatureOfTransaction", wrapper.NatureOfTransaction);
	}

	public void TestModeOfPayment()
	{
		goodsShipment.GoodsShipmentItem = new List<MGoodsShipmentItemType04FR>()
		{
			new MGoodsShipmentItemType04FR
			{
				DeclarationGoodsItemNumber = "2",
			},
			new MGoodsShipmentItemType04FR
			{
				DeclarationGoodsItemNumber = "1",
				Commodity = new MCommodityType04FR()
				{
					CalculationOfTaxes = new MCalculationOfTaxesType01FR()
					{
						DutiesAndTaxe = new List<MDutiesAndTaxesType01FR>()
						{
							new MDutiesAndTaxesType01FR
							{
								MethodOfPayment = "MethodOfPayment"
							}
						}
					}
				}
			}
		};
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("ModeOfPayment", "MethodOfPayment", wrapper.ModeOfPayment);
	}

	public void TestReverseChargeVAT()
	{
		messageObject.GeneralTaxation.VAT.ReverseChargeVATAmount = 123d;
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("123", wrapper.ReverseChargeVAT);
	}

	public void TestVATAmount()
	{
		messageObject.GeneralTaxation.VAT.VATamountAI2 = 321d;
		var wrapper = new IDD429Wrapper(message, Factory);

		AssertEquals("321", wrapper.VATAmountAI2);
	}

	DeltaIEFREDIMessage GetMessage()
	{
		var message = Factory.New<DeltaIEFREDIMessage>();
		message.EM_MessageSubType = "429";
		message.EM_ApplicationCode = EDIMessage.ApplicationCodes.FRCustomsMessage;
		message.EM_Status = EDIMessage.Status.Queued;
		message.MessageNumberStrategy = new FRMessageNumberStrategy(Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
		message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.DocumentWrappers.Testing.TestFiles.DeltaIE_IE429ResponseMessage.json");

		return message;
	}

	protected override void SetUp()
	{
		base.SetUp();
		resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		message = GetMessage();
		messageObject = (message.MessageDataObject as IE429MessageDataObject).ResponseMessage;
		goodsShipment = new MGoodsShipmentType05FR();
		messageObject.GoodsShipment.Add(goodsShipment);
	}

	DeltaIEFREDIMessage message;
	CC429BType messageObject;
	MGoodsShipmentType05FR goodsShipment;
	Lazy<EmbeddedResourceRetriever> resourceRetriever;
}
