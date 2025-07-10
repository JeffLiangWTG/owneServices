using System;
using System.Collections.Generic;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Response.IE428;
using CargoWise.IO;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.DocumentWrappers;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.DocumentWrappers.ImportDeclarationDocument.Testing;

class IDD428WrapperTest : Enterprise.DocumentWrappers.Testing.DocBaseWrapperTest
{
	protected override DocBaseWrapper GetNewDocumentWrapper()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.Messages.Add(message);
		return IDD428Wrapper.New(message);
	}

	public void TestDeclarationAcceptanceDateTime()
	{
		messageObject.DeclarationStatus.StateDateTime = "2024-01-02";
		var wrapper = new IDD428Wrapper(message, Factory);
		AssertEquals("DeclarationAcceptanceDateTime", "02/01/2024 00:00:00", wrapper.DeclarationAcceptanceDateTime);
	}

	public void TestDeclarationStatus()
	{
		var wrapper = new IDD428Wrapper(message, Factory);
		messageObject.DeclarationStatus.State = "BONAENLEVER";
		AssertEquals("DeclarationStatus", "BON A ENLEVER", wrapper.DeclarationStatus);

		messageObject.DeclarationStatus.State = "TESTABCD";
		AssertEquals("DeclarationStatus", "TESTABCD", wrapper.DeclarationStatus);
	}

	public void TestSupervisingCustomsOffice()
	{
		messageObject.SupervisingCustomsOffice = new MScoType();
		messageObject.SupervisingCustomsOffice.ReferenceNumber = "SupervisingCustomsOffice";
		var wrapper = new IDD428Wrapper(message, Factory);
		AssertEquals("SupervisingCustomsOffice", "SupervisingCustomsOffice", wrapper.SupervisingCustomsOffice);
	}

	[TestDate(2024, 01, 02, 03, 04, 05)]
	public void TestPrintDateTime()
	{
		var wrapper = new IDD428Wrapper(message, Factory);
		AssertEquals("PrintDateTime", "02/01/2024 03:04:05", wrapper.PrintDateTime);
	}

	public void TestDeclarantName()
	{
		messageObject.Declarant = new MDeclarantType();
		messageObject.Declarant.Name = "DeclarantName";
		var wrapper = new IDD428Wrapper(message, Factory);
		AssertEquals("DeclarantName", "DeclarantName", wrapper.DeclarantName);
	}

	public void TestDeclarantEORI()
	{
		messageObject.Declarant = new MDeclarantType();
		messageObject.Declarant.IdentificationNumber = "DeclarantEORI";
		var wrapper = new IDD428Wrapper(message, Factory);
		AssertEquals("DeclarantEORI", "DeclarantEORI", wrapper.DeclarantEORI);
	}

	public void TestDeclarantAddress()
	{
		messageObject.Declarant = new MDeclarantType();
		messageObject.Declarant.Address = new MAddressType01();
		messageObject.Declarant.Address.StreetAndNumber = "DeclarantAddress";
		var wrapper = new IDD428Wrapper(message, Factory);
		AssertEquals("DeclarantAddress", "DeclarantAddress", wrapper.DeclarantAddress);
	}

	public void TestDeclarantPostCode()
	{
		messageObject.Declarant = new MDeclarantType();
		messageObject.Declarant.Address = new MAddressType01();
		messageObject.Declarant.Address.Postcode = "DeclarantPostCode";
		var wrapper = new IDD428Wrapper(message, Factory);
		AssertEquals("DeclarantPostCode", "DeclarantPostCode", wrapper.DeclarantPostCode);
	}

	public void TestDeclarantCity()
	{
		messageObject.Declarant = new MDeclarantType();
		messageObject.Declarant.Address = new MAddressType01();
		messageObject.Declarant.Address.City = "DeclarantCity";
		var wrapper = new IDD428Wrapper(message, Factory);
		AssertEquals("DeclarantCity", "DeclarantCity", wrapper.DeclarantCity);
	}

	public void TestDeclarantCountry()
	{
		messageObject.Declarant = new MDeclarantType();
		messageObject.Declarant.Address = new MAddressType01();
		messageObject.Declarant.Address.Country = "DeclarantCountry";
		var wrapper = new IDD428Wrapper(message, Factory);
		AssertEquals("DeclarantCountry", "DeclarantCountry", wrapper.DeclarantCountry);
	}

	public void TestRepresentativeEORI()
	{
		messageObject.Representative = new MRepresentativeType();
		messageObject.Representative.IdentificationNumber = "RepresentativeEORI";
		var wrapper = new IDD428Wrapper(message, Factory);
		AssertEquals("RepresentativeEORI", "RepresentativeEORI", wrapper.RepresentativeEORI);
	}

	public void TestRepresentativeType()
	{
		messageObject.Representative = new MRepresentativeType();
		messageObject.Representative.Status = "RepresentativeType";
		var wrapper = new IDD428Wrapper(message, Factory);
		AssertEquals("RepresentativeType", "RepresentativeType", wrapper.RepresentativeType);
	}

	public void TestRepresentativeContactName()
	{
		messageObject.Representative = new MRepresentativeType();
		messageObject.Representative.ContactPerson = new MContactPersonType();
		messageObject.Representative.ContactPerson.Name = "Rep Contact Name";
		var wrapper = new IDD428Wrapper(message, Factory);
		AssertEquals("RepresentativeContactName", "Rep Contact Name", wrapper.RepresentativeContactName);
	}

	public void TestRepresentativeContactPhoneNumber()
	{
		messageObject.Representative = new MRepresentativeType();
		messageObject.Representative.ContactPerson = new MContactPersonType();
		messageObject.Representative.ContactPerson.PhoneNumber = "123456789";
		var wrapper = new IDD428Wrapper(message, Factory);
		AssertEquals("RepresentativeContactPhoneNumber", "123456789", wrapper.RepresentativeContactPhoneNumber);
	}

	public void TestRepresentativeContactEMail()
	{
		messageObject.Representative = new MRepresentativeType();
		messageObject.Representative.ContactPerson = new MContactPersonType();
		messageObject.Representative.ContactPerson.EMailAddress = "rep@example.com";
		var wrapper = new IDD428Wrapper(message, Factory);
		AssertEquals("RepresentativeContactEMail", "rep@example.com", wrapper.RepresentativeContactEMail);
	}

	public void TestDeclarantContactName()
	{
		messageObject.Declarant = new MDeclarantType();
		messageObject.Declarant.ContactPerson = new MContactPersonType();
		messageObject.Declarant.ContactPerson.Name = "Declarant Contact Name";
		var wrapper = new IDD428Wrapper(message, Factory);
		AssertEquals("DeclarantContactName", "Declarant Contact Name", wrapper.DeclarantContactName);
	}

	public void TestDeclarantContactPhoneNumber()
	{
		messageObject.Declarant = new MDeclarantType();
		messageObject.Declarant.ContactPerson = new MContactPersonType();
		messageObject.Declarant.ContactPerson.PhoneNumber = "987654321";
		var wrapper = new IDD428Wrapper(message, Factory);
		AssertEquals("DeclarantContactPhoneNumber", "987654321", wrapper.DeclarantContactPhoneNumber);
	}

	public void TestDeclarantContactEMail()
	{
		messageObject.Declarant = new MDeclarantType();
		messageObject.Declarant.ContactPerson = new MContactPersonType();
		messageObject.Declarant.ContactPerson.EMailAddress = "declarant@example.com";
		var wrapper = new IDD428Wrapper(message, Factory);
		AssertEquals("DeclarantContactEMail", "declarant@example.com", wrapper.DeclarantContactEMail);
	}

	public void TestCRN()
	{
		messageObject.ImportOperation = new MCciOperationType02();
		messageObject.ImportOperation.CustomsRegistrationNumber = "123456789";
		var wrapper = new IDD428Wrapper(message, Factory);

		AssertEquals("CRN", "123456789", wrapper.CRN);
	}

	public void TestMRN()
	{
		messageObject.ImportOperation = new MCciOperationType02();
		messageObject.ImportOperation.MRN = "123456789";
		var wrapper = new IDD428Wrapper(message, Factory);

		AssertEquals("MRN", "123456789", wrapper.MRN);
	}

	public void TestAgreementNumber()
	{
		goodsShipment.AdditionalReference = new List<MAdditionalReferenceType>();
		var reference1 = new MAdditionalReferenceType();
		reference1.Type = "2DEC";
		reference1.ReferenceNumber = "123456";
		var reference2 = new MAdditionalReferenceType();
		reference2.Type = "1DEC";
		reference2.ReferenceNumber = "654321";
		goodsShipment.AdditionalReference.Add(reference1);
		goodsShipment.AdditionalReference.Add(reference2);
		var wrapper = new IDD428Wrapper(message, Factory);

		AssertEquals("AgreementNumber", "654321", wrapper.AgreementNumber);
	}

	public void TestAdditionalReferences()
	{
		goodsShipment.AdditionalReference = new List<MAdditionalReferenceType>()
		{
			new MAdditionalReferenceType { Type = "ABC" ,ReferenceNumber = "123" },
			new MAdditionalReferenceType { Type = "DEF" ,ReferenceNumber = "456" }
		};
		var wrapper = new IDD428Wrapper(message, Factory);

		AssertEquals("AdditionalReferences", "ABC 123 - DEF 456", wrapper.AdditionalReferences);
	}

	public void TestItems()
	{
		goodsShipment.GoodsShipmentItem = new List<MGoodsShipmentItemType05FR>()
		{
			new MGoodsShipmentItemType05FR { StatisticalValue = 1 },
			new MGoodsShipmentItemType05FR { StatisticalValue = 2 }
		};
		var wrapper = new IDD428Wrapper(message, Factory);

		AssertEquals("Items", 2, wrapper.Items.Count);
		AssertEquals("Items", "1", wrapper.Items[0].StatisticalValue);
		AssertContainsExactElementsInExactOrder(new[] { "1", "2" }, wrapper.Items.Select(item => ((IDD428ItemWrapper)item).ItemNumber));
	}

	public void TestItemsCount()
	{
		goodsShipment.GoodsShipmentItem = new List<MGoodsShipmentItemType05FR>()
		{
			new MGoodsShipmentItemType05FR { StatisticalValue = 1 },
			new MGoodsShipmentItemType05FR { StatisticalValue = 2 }
		};
		var wrapper = new IDD428Wrapper(message, Factory);

		AssertEquals("ItemsCount", "2", wrapper.ItemsCount);
	}

	DeltaIEFREDIMessage GetMessage()
	{
		var message = Factory.New<DeltaIEFREDIMessage>();
		message.EM_MessageSubType = "428";
		message.EM_ApplicationCode = EDIMessage.ApplicationCodes.FRCustomsMessage;
		message.EM_Status = EDIMessage.Status.Queued;
		message.MessageNumberStrategy = new FRMessageNumberStrategy(Factory, FREDIMessage.ApplicationCodes.FRCustomsMessage);
		message.EM_MessageText = resourceRetriever.Value.GetString("Enterprise.Customs.FR.DocumentWrappers.Testing.TestFiles.DeltaIE_IE428ResponseMessage.json");

		return message;
	}

	protected override void SetUp()
	{
		base.SetUp();

		resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		message = GetMessage();
		messageObject = (message.MessageDataObject as IE428MessageDataObject).ResponseMessage;
		goodsShipment = new MGoodsShipmentType07FR();
		messageObject.GoodsShipment = goodsShipment;
	}

	DeltaIEFREDIMessage message;
	CC428BType messageObject;
	MGoodsShipmentType07FR goodsShipment;
	Lazy<EmbeddedResourceRetriever> resourceRetriever;
}
