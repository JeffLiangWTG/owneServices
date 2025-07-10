using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;
using Moq;
using NUnit.Framework;
using static CargoWise.Customs.DE.MessageContracts.MessageSchema.AESMessageSchema;
using static Enterprise.Customs.DE.Messaging.Testing.MessageBuilderTestHelper;

namespace Enterprise.Customs.DE.Messaging.AESVersion3_0.Testing
{
	[TestTimeZone]
	sealed class EXPDATMessageBuilderTest : AESMessageBuilderTest<EXPDATMessageBuilder, CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDF>
	{
		[ExpectNoExceptions]
		public void TestMessageIdentification()
		{
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.messageIdentification, Is.EqualTo("<<SENDERS REFERENCE PLACE HOLDER>>"));
		}

		[ExpectNoExceptions]
		public void TestMessageGroup()
		{
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.messageGroup.ToString(), Is.EqualTo("EXP"));
		}

		[ExpectNoExceptions]
		public void TestMessageType()
		{
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.messageType.ToString(), Is.EqualTo("DEXPDF"));
		}

		[ExpectNoExceptions]
		public void TestMessageSender_Abnormal()
		{
			messageHeader.Setup(m => m.InterchangeSender)
				.Returns(MockPartyId(
							GetLongString("E", EoriCodeMaxLength + 1),
							GetLongString("B", EoriBranchCodeMaxLength + 1)).Object);
			messageHeader.Setup(m => m.AuthorizationNumber).Returns(GetLongString("A", AuthorisationNumberMaxLength + 1));

			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.MessageSender.identificationNumber, Is.EqualTo(GetLongString("E", EoriCodeMaxLength)), "identificationNumber");
				NUnit.Framework.Assert.That(message.MessageSender.subsidiaryNumber, Is.EqualTo(GetLongString("B", EoriBranchCodeMaxLength)), "subsidiaryNumber");
				NUnit.Framework.Assert.That(message.MessageSender.authenticationNumber, Is.EqualTo(GetLongString("A", AuthorisationNumberMaxLength)), "authenticationNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestExportOperation()
		{
			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.ExportOperation.LRN, Is.EqualTo("LRN0001"), "LRN");
				NUnit.Framework.Assert.That(message.ExportOperation.declarationType, Is.EqualTo("T1"), "declarationType");
				NUnit.Framework.Assert.That(message.ExportOperation.exportDeclarationType.ToString(), Is.EqualTo("Item00000110"), "exportDeclarationType");
				NUnit.Framework.Assert.That(message.ExportOperation.partyConstellation.ToString(), Is.EqualTo("Item0010"), "partyConstellation");
				NUnit.Framework.Assert.That(message.ExportOperation.declarationSubmissionDateAndTime, Is.EqualTo(new DateTime(2021, 8, 12, 9, 53, 11, DateTimeKind.Utc)), "declarationSubmissionDateAndTime");
				NUnit.Framework.Assert.That(message.ExportOperation.decisiveDate, Is.EqualTo(new DateTime(2021, 1, 1)), "decisiveDate");
				NUnit.Framework.Assert.That(message.ExportOperation.decisiveDateSpecified, Is.EqualTo(true), "decisiveDateSpecified");
				NUnit.Framework.Assert.That(message.ExportOperation.exitDate, Is.EqualTo(new DateTime(2021, 2, 1)), "exitDate");
				NUnit.Framework.Assert.That(message.ExportOperation.exitDateSpecified, Is.EqualTo(true), "exitDateSpecified");
				NUnit.Framework.Assert.That(message.ExportOperation.presentationStartDateAndTime, Is.EqualTo(new DateTime(2021, 3, 1, 1, 3, 0, DateTimeKind.Utc)), "presentationStartDateAndTime");
				NUnit.Framework.Assert.That(message.ExportOperation.presentationStartDateAndTimeSpecified, Is.EqualTo(true), "presentationStartDateAndTimeSpecified");
				NUnit.Framework.Assert.That(message.ExportOperation.loadingEndDateAndTime, Is.EqualTo(new DateTime(2021, 4, 1, 0, 3, 0, DateTimeKind.Utc)), "loadingEndDateAndTime");
				NUnit.Framework.Assert.That(message.ExportOperation.loadingEndDateAndTimeSpecified, Is.EqualTo(true), "loadingEndDateAndTimeSpecified");
				NUnit.Framework.Assert.That(message.ExportOperation.security, Is.EqualTo("2"), "security");
				NUnit.Framework.Assert.That(message.ExportOperation.specificCircumstanceIndicator, Is.EqualTo("A"), "specificCircumstanceIndicator");
				NUnit.Framework.Assert.That(message.ExportOperation.totalAmountInvoiced, Is.EqualTo(2.22m), "totalAmountInvoiced");
				NUnit.Framework.Assert.That(message.ExportOperation.totalAmountInvoicedSpecified, Is.EqualTo(true), "totalAmountInvoicedSpecified");
				NUnit.Framework.Assert.That(message.ExportOperation.invoiceCurrency, Is.EqualTo("EUR"), "invoiceCurrency");
			});
		}

		[ExpectNoExceptions]
		public void TestExportOperation_Abnormal()
		{
			mockHeader.Setup(m => m.LocalReferenceNumber).Returns(GetLongString("L", LocalReferenceNumberMaxLength + 1));
			mockHeader.Setup(m => m.DeclarationType).Returns(GetLongString("D", DeclarationTypeMaxLength + 1));
			mockHeader.Setup(m => m.ExportDeclarationType).Returns(string.Empty);
			mockHeader.Setup(m => m.PartyConstellation).Returns(string.Empty);
			mockHeader.Setup(m => m.DecisiveDate).Returns(ZDate.Invalid);
			mockHeader.Setup(m => m.ExitDate).Returns(ZDate.Invalid);
			mockHeader.Setup(m => m.PresentationStartDateAndTimeUtc).Returns(default(DateTime));
			mockHeader.Setup(m => m.LoadingEndDateAndTimeUtc).Returns(default(DateTime));
			mockHeader.Setup(m => m.Security).Returns(GetLongString("S", SecurityMaxLength + 1));
			mockHeader.Setup(m => m.SpecificCircumstanceIndicator).Returns(GetLongString("P", SpecificCircumstanceIndicatorMaxLength3 + 1));
			mockHeader.Setup(m => m.InvoiceAmount).Returns(0);
			mockHeader.Setup(m => m.Currency).Returns(GetLongString("C", CurrencyMaxLength + 1));

			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.ExportOperation.LRN, Is.EqualTo(GetLongString("L", LocalReferenceNumberMaxLength)), "LRN");
				NUnit.Framework.Assert.That(message.ExportOperation.declarationType, Is.EqualTo(GetLongString("D", DeclarationTypeMaxLength)), "declarationType");
				NUnit.Framework.Assert.That(message.ExportOperation.exportDeclarationType.ToString(), Is.EqualTo("Item00000100"), "exportDeclarationType");
				NUnit.Framework.Assert.That(message.ExportOperation.partyConstellation.ToString(), Is.EqualTo("Item0000"), "partyConstellation");
				NUnit.Framework.Assert.That(message.ExportOperation.decisiveDateSpecified, Is.EqualTo(false), "decisiveDateSpecified");
				NUnit.Framework.Assert.That(message.ExportOperation.exitDateSpecified, Is.EqualTo(false), "exitDateSpecified");
				NUnit.Framework.Assert.That(message.ExportOperation.presentationStartDateAndTimeSpecified, Is.EqualTo(false), "presentationStartDateAndTimeSpecified");
				NUnit.Framework.Assert.That(message.ExportOperation.loadingEndDateAndTimeSpecified, Is.EqualTo(false), "loadingEndDateAndTimeSpecified");
				NUnit.Framework.Assert.That(message.ExportOperation.security, Is.EqualTo(GetLongString("S", SecurityMaxLength)), "security");
				NUnit.Framework.Assert.That(message.ExportOperation.specificCircumstanceIndicator, Is.EqualTo(GetLongString("P", SpecificCircumstanceIndicatorMaxLength3)), "specificCircumstanceIndicator");
				NUnit.Framework.Assert.That(message.ExportOperation.totalAmountInvoicedSpecified, Is.EqualTo(true), "totalAmountInvoicedSpecified");
				NUnit.Framework.Assert.That(message.ExportOperation.invoiceCurrency, Is.EqualTo(GetLongString("C", CurrencyMaxLength)), "invoiceCurrency");
			});
		}

		[ExpectNoExceptions]
		public void TestExportOperation_AmountAndCurrency()
		{
			mockHeader.Setup(m => m.InvoiceAmountAndCurrencySpecified).Returns(false);

			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.ExportOperation.totalAmountInvoicedSpecified, Is.EqualTo(false), "totalAmountInvoicedSpecified");
				NUnit.Framework.Assert.That(message.ExportOperation.invoiceCurrency, Is.EqualTo(default(string)), "invoiceCurrency");
			});
		}

		[ExpectNoExceptions]
		public void TestAuthorisation()
		{
			var message = messageBuilder.GenerateMessage();
			var element1 = message.Authorisation[0];
			var element2 = message.Authorisation[1];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element1.sequenceNumber, Is.EqualTo("1"), "element1 sequenceNumber");
				NUnit.Framework.Assert.That(element1.type.ToString(), Is.EqualTo("C512"), "element1 type");
				NUnit.Framework.Assert.That(element1.referenceNumber, Is.EqualTo("1234567890"), "element1 referenceNumber");

				NUnit.Framework.Assert.That(element2.sequenceNumber, Is.EqualTo("2"), "element2 sequenceNumber");
				NUnit.Framework.Assert.That(element2.type.ToString(), Is.EqualTo("C514"), "element2 type");
				NUnit.Framework.Assert.That(element2.referenceNumber, Is.EqualTo("1234567891"), "element2 referenceNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestAuthorisation_Abnormal()
		{
			var authorisations = new[]
			{
				MockAuthorisation("!@#", GetLongString("R", AuthorisationReferenceNumberMaxLength + 1)).Object
			};
			mockHeader.Setup(m => m.Authorisations).Returns(authorisations);

			var message = messageBuilder.GenerateMessage();
			var element = message.Authorisation[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element.sequenceNumber, Is.EqualTo("1"), "sequenceNumber");
				NUnit.Framework.Assert.That(element.type.ToString(), Is.EqualTo("C019"), "type");
				NUnit.Framework.Assert.That(element.referenceNumber, Is.EqualTo(GetLongString("R", AuthorisationReferenceNumberMaxLength)), "referenceNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestCustomsOfficeOfPresentation()
		{
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsOfficeOfPresentation.referenceNumber, Is.EqualTo("DE0001"));
		}

		[ExpectNoExceptions]
		public void TestCustomsOfficeOfPresentation_Abnormal()
		{
			mockHeader.Setup(m => m.CustomsOfficeOfPresentation).Returns(GetLongString("P", CustomsOfficeReferenceNumberMaxLength + 1));

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsOfficeOfPresentation.referenceNumber, Is.EqualTo(GetLongString("P", CustomsOfficeReferenceNumberMaxLength)));
		}

		[ExpectNoExceptions]
		public void TestCustomsOfficeOfExport()
		{
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsOfficeOfExport.referenceNumber, Is.EqualTo("DE0002"));
		}

		[ExpectNoExceptions]
		public void TestCustomsOfficeOfExport_Abnormal()
		{
			mockHeader.Setup(m => m.ExportCustomsOffice).Returns(GetLongString("P", CustomsOfficeReferenceNumberMaxLength + 1));

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsOfficeOfExport.referenceNumber, Is.EqualTo(GetLongString("P", CustomsOfficeReferenceNumberMaxLength)));
		}

		[ExpectNoExceptions]
		public void TestCustomsOfficeOfSupplement()
		{
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsOfficeOfSupplement.referenceNumber, Is.EqualTo("DE0003"));
		}

		[ExpectNoExceptions]
		public void TestCustomsOfficeOfSupplement_Abnormal()
		{
			mockHeader.Setup(m => m.SupplementaryDeclarationCustomsOffice).Returns(GetLongString("P", CustomsOfficeReferenceNumberMaxLength + 1));

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsOfficeOfSupplement.referenceNumber, Is.EqualTo(GetLongString("P", CustomsOfficeReferenceNumberMaxLength)));
		}

		[ExpectNoExceptions]
		public void TestCustomsOfficeOfExitDeclared()
		{
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsOfficeOfExitDeclared.referenceNumber, Is.EqualTo("DE0004"));
		}

		[ExpectNoExceptions]
		public void TestCustomsOfficeOfExitDeclared_Abnormal()
		{
			mockHeader.Setup(m => m.IntendedExitCustomsOffice).Returns(GetLongString("P", CustomsOfficeReferenceNumberMaxLength + 1));

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsOfficeOfExitDeclared.referenceNumber, Is.EqualTo(GetLongString("P", CustomsOfficeReferenceNumberMaxLength)));
		}

		[ExpectNoExceptions]
		public void TestCustomsOfficeOfExitActual()
		{
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsOfficeOfExitActual.referenceNumber, Is.EqualTo("DE0005"));
		}

		[ExpectNoExceptions]
		public void TestCustomsOfficeOfExitActual_Abnormal()
		{
			mockHeader.Setup(m => m.ActualExitCustomsOffice).Returns(GetLongString("P", CustomsOfficeReferenceNumberMaxLength + 1));

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.CustomsOfficeOfExitActual.referenceNumber, Is.EqualTo(GetLongString("P", CustomsOfficeReferenceNumberMaxLength)));
		}

		[ExpectNoExceptions]
		public void TestContractualPartner_Null()
		{
			mockHeader.Setup(m => m.ContractualPartner).Returns((IAESParty)null);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.ContractualPartner, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFContractualPartner)));
		}

		[ExpectNoExceptions]
		public void TestContractualPartner()
		{
			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.ContractualPartner.identificationNumber, Is.EqualTo("DEEOR001"), "identificationNumber");
				NUnit.Framework.Assert.That(message.ContractualPartner.subsidiaryNumber, Is.EqualTo("0001"), "subsidiaryNumber");
				NUnit.Framework.Assert.That(message.ContractualPartner.name, Is.EqualTo(default(string)), "name");
				NUnit.Framework.Assert.That(message.ContractualPartner.Address, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFContractualPartnerAddress)), "Address - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestContractualPartner_Abnormal()
		{
			var contractualPartner = MockParty(GetLongString("E", EoriCodeMaxLength + 1)
				, GetLongString("B", EoriBranchCodeMaxLength + 1)).Object;
			mockHeader.Setup(m => m.ContractualPartner).Returns(contractualPartner);

			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.ContractualPartner.identificationNumber, Is.EqualTo(GetLongString("E", EoriCodeMaxLength)), "identificationNumber");
				NUnit.Framework.Assert.That(message.ContractualPartner.subsidiaryNumber, Is.EqualTo(GetLongString("B", EoriBranchCodeMaxLength)), "subsidiaryNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestContractualPartner_TCU()
		{
			var contractualPartner = MockParty(string.Empty
			, string.Empty
			, tcuNumber: "DETCU001"
			).Object;
			mockHeader.Setup(m => m.ContractualPartner).Returns(contractualPartner);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.ContractualPartner.identificationNumber, Is.EqualTo("DETCU001"), "identificationNumber");
		}

		[ExpectNoExceptions]
		public void TestContractualPartner_TCU_Abnormal()
		{
			var contractualPartner = MockParty(tcuNumber: GetLongString("T", EoriCodeMaxLength + 1)).Object;
			mockHeader.Setup(m => m.ContractualPartner).Returns(contractualPartner);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.ContractualPartner.identificationNumber, Is.EqualTo(GetLongString("T", EoriCodeMaxLength)), "identificationNumber");
		}

		[ExpectNoExceptions]
		public void TestContractualPartner_NoEOROrTCU()
		{
			var contractualPartner = MockParty(string.Empty
			, string.Empty
			, "ContractualPartner"
			, "ContractualPartner Address1"
			, "ContractualPartner City"
			, "123456789"
			, "DE"
			, address2: "ContractualPartner Address2"
			).Object;
			mockHeader.Setup(m => m.ContractualPartner).Returns(contractualPartner);

			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.ContractualPartner.identificationNumber, Is.EqualTo(default(string)), "identificationNumber - should be [null]");
				NUnit.Framework.Assert.That(message.ContractualPartner.subsidiaryNumber, Is.EqualTo(default(string)), "subsidiaryNumber - should be [null]");
				NUnit.Framework.Assert.That(message.ContractualPartner.name, Is.EqualTo("ContractualPartner"), "name");
				NUnit.Framework.Assert.That(message.ContractualPartner.Address.streetAndNumber, Is.EqualTo("ContractualPartner Address1ContractualPartner Address2"), "streetAndNumber");
				NUnit.Framework.Assert.That(message.ContractualPartner.Address.postcode, Is.EqualTo("123456789"), "postcode");
				NUnit.Framework.Assert.That(message.ContractualPartner.Address.city, Is.EqualTo("ContractualPartner City"), "city");
				NUnit.Framework.Assert.That(message.ContractualPartner.Address.country, Is.EqualTo("DE"), "country");
			});
		}

		[ExpectNoExceptions]
		public void TestContractualPartner_NoEOROrTCU_Abnormal()
		{
			var contractualPartner = MockParty(string.Empty
				, string.Empty
				, GetLongString("N", PartyNameMaxLength70 + 1)
				, GetLongString("A", StreetAndNumberMaxLength + 1)
				, GetLongString("C", AddressCityMaxLength + 1)
				, GetLongString("P", AddressPostcodeMaxLength + 1)
				, GetLongString("O", AddressCountryMaxLength + 1)
				, address2: GetLongString("A", StreetAndNumberMaxLength + 1)
				).Object;
			mockHeader.Setup(m => m.ContractualPartner).Returns(contractualPartner);

			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.ContractualPartner.name, Is.EqualTo(GetLongString("N", PartyNameMaxLength70)), "name");
				NUnit.Framework.Assert.That(message.ContractualPartner.Address.streetAndNumber, Is.EqualTo(GetLongString("A", StreetAndNumberMaxLength)), "streetAndNumber");
				NUnit.Framework.Assert.That(message.ContractualPartner.Address.postcode, Is.EqualTo(GetLongString("P", AddressPostcodeMaxLength)), "postcode");
				NUnit.Framework.Assert.That(message.ContractualPartner.Address.city, Is.EqualTo(GetLongString("C", AddressCityMaxLength)), "city");
				NUnit.Framework.Assert.That(message.ContractualPartner.Address.country, Is.EqualTo(GetLongString("O", AddressCountryMaxLength)), "country");
			});
		}

		[ExpectNoExceptions]
		public void TestExporter_Null()
		{
			mockHeader.Setup(m => m.Exporter).Returns((IAESParty)null);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Exporter, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFExporter)));
		}

		[ExpectNoExceptions]
		public void TestExporter()
		{
			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.Exporter.identificationNumber, Is.EqualTo("DEEOR002"), "identificationNumber");
				NUnit.Framework.Assert.That(message.Exporter.subsidiaryNumber, Is.EqualTo("0002"), "subsidiaryNumber");
				NUnit.Framework.Assert.That(message.Exporter.name, Is.EqualTo(default(string)), "name");
				NUnit.Framework.Assert.That(message.Exporter.Address, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFExporterAddress)), "Address - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestExporter_Abnormal()
		{
			var exporter = MockParty(GetLongString("E", EoriCodeMaxLength + 1)
				, GetLongString("B", EoriBranchCodeMaxLength + 1)
				).Object;
			mockHeader.Setup(m => m.Exporter).Returns(exporter);

			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.Exporter.identificationNumber, Is.EqualTo(GetLongString("E", EoriCodeMaxLength)), "identificationNumber");
				NUnit.Framework.Assert.That(message.Exporter.subsidiaryNumber, Is.EqualTo(GetLongString("B", EoriBranchCodeMaxLength)), "subsidiaryNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestExporter_NoEOR()
		{
			var exporter = MockParty(string.Empty
				, string.Empty
				, "Exporter"
				, "Exporter Address1"
				, "Exporter City"
				, "123456789"
				, "DE"
				, address2: "Exporter Address2"
				).Object;
			mockHeader.Setup(m => m.Exporter).Returns(exporter);

			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.Exporter.identificationNumber, Is.EqualTo(default(string)), "identificationNumber - should be [null]");
				NUnit.Framework.Assert.That(message.Exporter.subsidiaryNumber, Is.EqualTo(default(string)), "subsidiaryNumber - should be [null]");
				NUnit.Framework.Assert.That(message.Exporter.name, Is.EqualTo("Exporter"), "name");
				NUnit.Framework.Assert.That(message.Exporter.Address.streetAndNumber, Is.EqualTo("Exporter Address1Exporter Address2"), "streetAndNumber");
				NUnit.Framework.Assert.That(message.Exporter.Address.postcode, Is.EqualTo("123456789"), "postcode");
				NUnit.Framework.Assert.That(message.Exporter.Address.city, Is.EqualTo("Exporter City"), "city");
				NUnit.Framework.Assert.That(message.Exporter.Address.country, Is.EqualTo("DE"), "country");
			});
		}

		[ExpectNoExceptions]
		public void TestExporter_NoEOR_Abnormal()
		{
			var exporter = MockParty(string.Empty
				, string.Empty
				, GetLongString("N", PartyNameMaxLength70 + 1)
				, GetLongString("A", StreetAndNumberMaxLength + 1)
				, GetLongString("C", AddressCityMaxLength + 1)
				, GetLongString("P", AddressPostcodeMaxLength + 1)
				, GetLongString("O", AddressCountryMaxLength + 1)
				, address2: GetLongString("A", StreetAndNumberMaxLength + 1)
				).Object;
			mockHeader.Setup(m => m.Exporter).Returns(exporter);

			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.Exporter.name, Is.EqualTo(GetLongString("N", PartyNameMaxLength70)), "name");
				NUnit.Framework.Assert.That(message.Exporter.Address.streetAndNumber, Is.EqualTo(GetLongString("A", StreetAndNumberMaxLength)), "streetAndNumber");
				NUnit.Framework.Assert.That(message.Exporter.Address.postcode, Is.EqualTo(GetLongString("P", AddressPostcodeMaxLength)), "postcode");
				NUnit.Framework.Assert.That(message.Exporter.Address.city, Is.EqualTo(GetLongString("C", AddressCityMaxLength)), "city");
				NUnit.Framework.Assert.That(message.Exporter.Address.country, Is.EqualTo(GetLongString("O", AddressCountryMaxLength)), "country");
			});
		}

		[ExpectNoExceptions]
		public void TestDeclarant_Null()
		{
			mockHeader.Setup(m => m.Declarant).Returns((IAESParty)null);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Declarant, Is.Not.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFDeclarant)));
		}

		[ExpectNoExceptions]
		public void TestDeclarant()
		{
			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.Declarant.identificationNumber, Is.EqualTo("DEEOR003"), "identificationNumber");
				NUnit.Framework.Assert.That(message.Declarant.subsidiaryNumber, Is.EqualTo("0003"), "subsidiaryNumber");
				NUnit.Framework.Assert.That(message.Declarant.name, Is.EqualTo(default(string)), "name");
				NUnit.Framework.Assert.That(message.Declarant.Address, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFDeclarantAddress)), "Address - should be [null]");
				NUnit.Framework.Assert.That(message.Declarant.ContactPerson.name, Is.EqualTo("VIC"), "ContactPerson name");
				NUnit.Framework.Assert.That(message.Declarant.ContactPerson.phoneNumber, Is.EqualTo("1324333"), "ContactPerson phoneNumber");
				NUnit.Framework.Assert.That(message.Declarant.ContactPerson.eMailAddress, Is.EqualTo("a@123.com"), "ContactPerson eMailAddress");
			});
		}

		[ExpectNoExceptions]
		public void TestDeclarant_Abnormal()
		{
			var declarant = MockParty(GetLongString("E", EoriCodeMaxLength + 1)
						, GetLongString("B", EoriBranchCodeMaxLength + 1)
						, contactPerson: MockContactPerson(string.Empty
							, GetLongString("M", ContactNameMaxLength70 + 1)
							, GetLongString("H", ContactPhoneNumberMaxLength + 1)
							, string.Empty
							, GetLongString("I", ContactMailAddressMaxLength + 1)
							).Object
						).Object;
			mockHeader.Setup(m => m.Declarant).Returns(declarant);

			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.Declarant.identificationNumber, Is.EqualTo(GetLongString("E", EoriCodeMaxLength)), "identificationNumber");
				NUnit.Framework.Assert.That(message.Declarant.subsidiaryNumber, Is.EqualTo(GetLongString("B", EoriBranchCodeMaxLength)), "subsidiaryNumber");
				NUnit.Framework.Assert.That(message.Declarant.ContactPerson.name, Is.EqualTo(GetLongString("M", ContactNameMaxLength70)), "ContactPerson name");
				NUnit.Framework.Assert.That(message.Declarant.ContactPerson.phoneNumber, Is.EqualTo(GetLongString("H", ContactPhoneNumberMaxLength)), "ContactPerson phoneNumber");
				NUnit.Framework.Assert.That(message.Declarant.ContactPerson.eMailAddress, Is.EqualTo(GetLongString("I", ContactMailAddressMaxLength)), "ContactPerson eMailAddress");
			});
		}

		[ExpectNoExceptions]
		public void TestDeclarant_NoEOR()
		{
			var declarant = MockParty(string.Empty
			, string.Empty
			, "Declarant"
			, "Declarant Address1"
			, "Declarant City"
			, "123456789"
			, "DE"
			, address2: "Declarant Address2"
		).Object;
			mockHeader.Setup(m => m.Declarant).Returns(declarant);

			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.Declarant.identificationNumber, Is.EqualTo(default(string)), "identificationNumber - should be [null]");
				NUnit.Framework.Assert.That(message.Declarant.subsidiaryNumber, Is.EqualTo(default(string)), "subsidiaryNumber - should be [null]");
				NUnit.Framework.Assert.That(message.Declarant.name, Is.EqualTo("Declarant"), "name");
				NUnit.Framework.Assert.That(message.Declarant.Address.streetAndNumber, Is.EqualTo("Declarant Address1Declarant Address2"), "streetAndNumber");
				NUnit.Framework.Assert.That(message.Declarant.Address.postcode, Is.EqualTo("123456789"), "postcode");
				NUnit.Framework.Assert.That(message.Declarant.Address.city, Is.EqualTo("Declarant City"), "city");
				NUnit.Framework.Assert.That(message.Declarant.Address.country, Is.EqualTo("DE"), "country");
			});
		}

		[ExpectNoExceptions]
		public void TestDeclarant_NoEOR_Abnormal()
		{
			var declarant = MockParty(string.Empty
						, string.Empty
						, GetLongString("N", PartyNameMaxLength70 + 1)
						, GetLongString("A", AddressLineMaxLength + 1)
						, GetLongString("C", AddressCityMaxLength + 1)
						, GetLongString("P", AddressPostcodeMaxLength + 1)
						, GetLongString("O", AddressCountryMaxLength + 1)
						, address2: GetLongString("A", StreetAndNumberMaxLength + 1)
						).Object;
			mockHeader.Setup(m => m.Declarant).Returns(declarant);

			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.Declarant.name, Is.EqualTo(GetLongString("N", PartyNameMaxLength70)), "name");
				NUnit.Framework.Assert.That(message.Declarant.Address.streetAndNumber, Is.EqualTo(GetLongString("A", AddressLineMaxLength)), "streetAndNumber");
				NUnit.Framework.Assert.That(message.Declarant.Address.postcode, Is.EqualTo(GetLongString("P", AddressPostcodeMaxLength)), "postcode");
				NUnit.Framework.Assert.That(message.Declarant.Address.city, Is.EqualTo(GetLongString("C", AddressCityMaxLength)), "city");
				NUnit.Framework.Assert.That(message.Declarant.Address.country, Is.EqualTo(GetLongString("O", AddressCountryMaxLength)), "country");
			});
		}

		[ExpectNoExceptions]
		public void TestDeclarant_NoContact()
		{
			var declarant = MockParty("DEEOR003", "0003").Object;
			mockHeader.Setup(m => m.Declarant).Returns(declarant);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Declarant.ContactPerson, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFDeclarantContactPerson)));
		}

		[ExpectNoExceptions]
		public void TestRepresentative_Null()
		{
			mockHeader.Setup(m => m.Representative).Returns((IAESParty)null);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Representative, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFRepresentative)));
		}

		[ExpectNoExceptions]
		public void TestRepresentative()
		{
			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.Representative.identificationNumber, Is.EqualTo("DEEOR004"), "identificationNumber");
				NUnit.Framework.Assert.That(message.Representative.subsidiaryNumber, Is.EqualTo("0004"), "subsidiaryNumber");
				NUnit.Framework.Assert.That(message.Representative.ContactPerson.name, Is.EqualTo("VIC"), "name");
				NUnit.Framework.Assert.That(message.Representative.ContactPerson.phoneNumber, Is.EqualTo("1324333"), "phoneNumber");
				NUnit.Framework.Assert.That(message.Representative.ContactPerson.eMailAddress, Is.EqualTo("a@123.com"), "eMailAddress");
			});
		}

		[ExpectNoExceptions]
		public void TestRepresentative_Abnormal()
		{
			var representative = MockParty(GetLongString("E", EoriCodeMaxLength + 1)
						, GetLongString("B", EoriBranchCodeMaxLength + 1)
						, contactPerson: MockContactPerson(string.Empty
							, GetLongString("M", ContactNameMaxLength70 + 1)
							, GetLongString("H", ContactPhoneNumberMaxLength + 1)
							, string.Empty
							, GetLongString("I", ContactMailAddressMaxLength + 1)
							).Object
						).Object;
			mockHeader.Setup(m => m.Representative).Returns(representative);

			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.Representative.identificationNumber, Is.EqualTo(GetLongString("E", EoriCodeMaxLength)), "identificationNumber");
				NUnit.Framework.Assert.That(message.Representative.subsidiaryNumber, Is.EqualTo(GetLongString("B", EoriBranchCodeMaxLength)), "subsidiaryNumber");
				NUnit.Framework.Assert.That(message.Representative.ContactPerson.name, Is.EqualTo(GetLongString("M", ContactNameMaxLength70)), "name");
				NUnit.Framework.Assert.That(message.Representative.ContactPerson.phoneNumber, Is.EqualTo(GetLongString("H", ContactPhoneNumberMaxLength)), "phoneNumber");
				NUnit.Framework.Assert.That(message.Representative.ContactPerson.eMailAddress, Is.EqualTo(GetLongString("I", ContactMailAddressMaxLength)), "eMailAddress");
			});
		}

		[ExpectNoExceptions]
		public void TestRepresentative_ContactIsNull()
		{
			var representative = MockParty("DEEOR004", "0004").Object;
			mockHeader.Setup(m => m.Representative).Returns(representative);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Representative.ContactPerson, Is.Not.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFRepresentativeContactPerson)));
		}

		[ExpectNoExceptions]
		public void TestSubContractor_Null()
		{
			mockHeader.Setup(m => m.SubContractor).Returns((IAESParty)null);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.SubContractor, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFSubContractor)));
		}

		[ExpectNoExceptions]
		public void TestSubContractor()
		{
			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.SubContractor.identificationNumber, Is.EqualTo("DEEOR005"), "identificationNumber");
				NUnit.Framework.Assert.That(message.SubContractor.subsidiaryNumber, Is.EqualTo("0005"), "subsidiaryNumber");
				NUnit.Framework.Assert.That(message.SubContractor.name, Is.EqualTo(default(string)), "name");
				NUnit.Framework.Assert.That(message.SubContractor.Address, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFSubContractorAddress)), "Address - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestSubContractor_Abnormal()
		{
			var subContractor = MockParty(GetLongString("E", EoriCodeMaxLength + 1)
				, GetLongString("B", EoriBranchCodeMaxLength + 1)
				).Object;
			mockHeader.Setup(m => m.SubContractor).Returns(subContractor);

			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.SubContractor.identificationNumber, Is.EqualTo(GetLongString("E", EoriCodeMaxLength)), "identificationNumber");
				NUnit.Framework.Assert.That(message.SubContractor.subsidiaryNumber, Is.EqualTo(GetLongString("B", EoriBranchCodeMaxLength)), "subsidiaryNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestSubContractor_NoEOR()
		{
			var subContractor = MockParty(string.Empty
			, string.Empty
			, "SubContractor"
			, "SubContractor Address1"
			, "SubContractor City"
			, "123456789"
			, "DE"
			, address2: "SubContractor Address2"
			).Object;
			mockHeader.Setup(m => m.SubContractor).Returns(subContractor);

			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.SubContractor.identificationNumber, Is.EqualTo(default(string)), "identificationNumber - should be [null]");
				NUnit.Framework.Assert.That(message.SubContractor.subsidiaryNumber, Is.EqualTo(default(string)), "subsidiaryNumber - should be [null]");
				NUnit.Framework.Assert.That(message.SubContractor.name, Is.EqualTo("SubContractor"), "name");
				NUnit.Framework.Assert.That(message.SubContractor.Address.streetAndNumber, Is.EqualTo("SubContractor Address1SubContractor Address2"), "streetAndNumber");
				NUnit.Framework.Assert.That(message.SubContractor.Address.postcode, Is.EqualTo("123456789"), "postcode");
				NUnit.Framework.Assert.That(message.SubContractor.Address.city, Is.EqualTo("SubContractor City"), "city");
				NUnit.Framework.Assert.That(message.SubContractor.Address.country, Is.EqualTo("DE"), "country");
			});
		}

		[ExpectNoExceptions]
		public void TestSubContractor_NoEOR_Abnormal()
		{
			var subContractor = MockParty(string.Empty
				, string.Empty
				, GetLongString("N", PartyNameMaxLength70 + 1)
				, GetLongString("A", StreetAndNumberMaxLength + 1)
				, GetLongString("C", AddressCityMaxLength + 1)
				, GetLongString("P", AddressPostcodeMaxLength + 1)
				, GetLongString("O", AddressCountryMaxLength + 1)
				, address2: GetLongString("A", StreetAndNumberMaxLength + 1)
				).Object;
			mockHeader.Setup(m => m.SubContractor).Returns(subContractor);

			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(message.SubContractor.identificationNumber, Is.EqualTo(default(string)), "identificationNumber - should be [null]");
				NUnit.Framework.Assert.That(message.SubContractor.subsidiaryNumber, Is.EqualTo(default(string)), "subsidiaryNumber - should be [null]");
				NUnit.Framework.Assert.That(message.SubContractor.name, Is.EqualTo(GetLongString("N", PartyNameMaxLength70)), "name");
				NUnit.Framework.Assert.That(message.SubContractor.Address.streetAndNumber, Is.EqualTo(GetLongString("A", StreetAndNumberMaxLength)), "streetAndNumber");
				NUnit.Framework.Assert.That(message.SubContractor.Address.postcode, Is.EqualTo(GetLongString("P", AddressPostcodeMaxLength)), "postcode");
				NUnit.Framework.Assert.That(message.SubContractor.Address.city, Is.EqualTo(GetLongString("C", AddressCityMaxLength)), "city");
				NUnit.Framework.Assert.That(message.SubContractor.Address.country, Is.EqualTo(GetLongString("O", AddressCountryMaxLength)), "country");
			});
		}

		[ExpectNoExceptions]
		public void TestNatureOfTransaction()
		{
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.natureOfTransaction, Is.EqualTo("31"));
		}

		[ExpectNoExceptions]
		public void TestNatureOfTransaction_Abnormal()
		{
			mockHeader.Setup(m => m.TransactionType).Returns(GetLongString("N", TransactionTypeMaxLength + 1));

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.natureOfTransaction, Is.EqualTo(GetLongString("N", TransactionTypeMaxLength)));
		}

		[ExpectNoExceptions]
		public void TestCountryOfExport()
		{
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.countryOfExport, Is.EqualTo("DE"));
		}

		[ExpectNoExceptions]
		public void TestCountryOfExport_Abnormal()
		{
			mockHeader.Setup(m => m.ExportCountry).Returns(GetLongString("O", CountryCodeMaxLength + 1));

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.countryOfExport, Is.EqualTo(GetLongString("O", CountryCodeMaxLength)));
		}

		[ExpectNoExceptions]
		public void TestCountryOfDestination()
		{
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.countryOfDestination, Is.EqualTo("CN"));
		}

		[ExpectNoExceptions]
		public void TestCountryOfDestination_Abnormal()
		{
			mockHeader.Setup(m => m.DestinationCountry).Returns(GetLongString("O", CountryCodeMaxLength + 1));

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.countryOfDestination, Is.EqualTo(GetLongString("O", CountryCodeMaxLength)));
		}

		[ExpectNoExceptions]
		public void TestAdditionalSupplyChainActor()
		{
			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.AdditionalSupplyChainActor;
			var element1 = elements[0];
			var element2 = elements[1];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element1.sequenceNumber, Is.EqualTo("1"), "element1 sequenceNumber");
				NUnit.Framework.Assert.That(element1.role, Is.EqualTo("AAA"), "element1 role");
				NUnit.Framework.Assert.That(element1.identificationNumber, Is.EqualTo("1234567890"), "element1 identificationNumber");

				NUnit.Framework.Assert.That(element2.sequenceNumber, Is.EqualTo("2"), "element2 sequenceNumber");
				NUnit.Framework.Assert.That(element2.role, Is.EqualTo("BBB"), "element2 role");
				NUnit.Framework.Assert.That(element2.identificationNumber, Is.EqualTo("1234567891"), "element2 identificationNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestAdditionalSupplyChainActor_Abnormal()
		{
			var additionalSupplyChainActors = new[]
			{
				MockSupplyChainActor(GetLongString("R", SupplyChainActorRoleMaxLength + 1), GetLongString("I", SupplyChainActorIdentificationNumberMaxLength + 1)).Object
			};
			mockHeader.Setup(m => m.AdditionalSupplyChainActors).Returns(additionalSupplyChainActors);

			var message = messageBuilder.GenerateMessage();
			var element = message.GoodsShipment.AdditionalSupplyChainActor[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element.sequenceNumber, Is.EqualTo("1"), "sequenceNumber");
				NUnit.Framework.Assert.That(element.role, Is.EqualTo(GetLongString("R", SupplyChainActorRoleMaxLength)), "role");
				NUnit.Framework.Assert.That(element.identificationNumber, Is.EqualTo(GetLongString("I", SupplyChainActorIdentificationNumberMaxLength)), "identificationNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestDeliveryTerms()
		{
			var deliveryTerms = MockDeliveryTerms("FOB", location: "Any location").Object;
			mockHeader.Setup(m => m.DeliveryTerms).Returns(deliveryTerms);
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.DeliveryTerms.incotermCode, Is.EqualTo("FOB"), "unchanged incotermCode");
			NUnit.Framework.Assert.That(message.GoodsShipment.DeliveryTerms.location, Is.EqualTo("Any location"), "unchanged location");
		}

		[ExpectNoExceptions]
		public void TestDeliveryTerms_Abnormal()
		{
			CombineAssertions(() =>
			{
				var deliveryTerms = MockDeliveryTerms(GetLongString("I", IncotermCodeMaxLength + 1), GetLongString("L", DeliveryTermsLocationMaxLength + 1)).Object;
				mockHeader.Setup(m => m.DeliveryTerms).Returns(deliveryTerms);

				var message = messageBuilder.GenerateMessage();
				NUnit.Framework.Assert.That(message.GoodsShipment.DeliveryTerms.incotermCode, Is.EqualTo(GetLongString("I", IncotermCodeMaxLength)), "incotermCode");
				NUnit.Framework.Assert.That(message.GoodsShipment.DeliveryTerms.location, Is.EqualTo(GetLongString("L", DeliveryTermsLocationMaxLength)), "location");
				NUnit.Framework.Assert.That(message.GoodsShipment.DeliveryTerms.text, Is.EqualTo(default(string)), "text - should be [null]");

				deliveryTerms = MockDeliveryTerms("XXX", "LOC1").Object;
				mockHeader.Setup(m => m.DeliveryTerms).Returns(deliveryTerms);

				message = messageBuilder.GenerateMessage();
				NUnit.Framework.Assert.That(message.GoodsShipment.DeliveryTerms.text, Is.EqualTo(default(string)), "IncotermCode is 'XXX', text");
			});
		}

		[ExpectNoExceptions]
		public void TestOutwardProcessing()
		{
			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				var reimportElements = message.GoodsShipment.OutwardProcessing.Reimport;
				var reimportElement1 = reimportElements[0];
				var reimportElement2 = reimportElements[1];
				NUnit.Framework.Assert.That(reimportElement1.sequenceNumber, Is.EqualTo("1"), "reimportElement1 sequenceNumber");
				NUnit.Framework.Assert.That(reimportElement1.country, Is.EqualTo("DE"), "reimportElement1 country");

				NUnit.Framework.Assert.That(reimportElement2.sequenceNumber, Is.EqualTo("2"), "reimportElement2 sequenceNumber");
				NUnit.Framework.Assert.That(reimportElement2.country, Is.EqualTo("US"), "reimportElement2 country");

				var identificationMeansElements = message.GoodsShipment.OutwardProcessing.IdentificationMeans;
				var identificationMeansElement1 = identificationMeansElements[0];
				var identificationMeansElement2 = identificationMeansElements[1];
				NUnit.Framework.Assert.That(identificationMeansElement1.sequenceNumber, Is.EqualTo("1"), "identificationMeansElement1 sequenceNumber");
				NUnit.Framework.Assert.That(identificationMeansElement1.type.ToString(), Is.EqualTo("A"), "identificationMeansElement1 type");
				NUnit.Framework.Assert.That(identificationMeansElement1.description, Is.EqualTo("A Description"), "identificationMeansElement1 description");

				NUnit.Framework.Assert.That(identificationMeansElement2.sequenceNumber, Is.EqualTo("2"), "identificationMeansElement2 sequenceNumber");
				NUnit.Framework.Assert.That(identificationMeansElement2.type.ToString(), Is.EqualTo("B"), "identificationMeansElement2 type");
				NUnit.Framework.Assert.That(identificationMeansElement2.description, Is.EqualTo("B Description"), "identificationMeansElement2 description");

				var productElements = message.GoodsShipment.OutwardProcessing.Product;
				var productElement1 = productElements[0];
				var productElement2 = productElements[1];
				NUnit.Framework.Assert.That(productElement1.sequenceNumber, Is.EqualTo("1"), "productElement1 sequenceNumber");
				NUnit.Framework.Assert.That(productElement1.Commodity.descriptionOfGoods, Is.EqualTo("Goods Description 1"), "productElement1 descriptionOfGoods");
				NUnit.Framework.Assert.That(productElement1.Commodity.CommodityCode.harmonizedSystemSubHeadingCode, Is.EqualTo("123456"), "productElement1 harmonizedSystemSubHeadingCode");
				NUnit.Framework.Assert.That(productElement1.Commodity.CommodityCode.combinedNomenclatureCode, Is.EqualTo("78"), "productElement1 combinedNomenclatureCode");

				NUnit.Framework.Assert.That(productElement2.sequenceNumber, Is.EqualTo("2"), "productElement2 sequenceNumber");
				NUnit.Framework.Assert.That(productElement2.Commodity.descriptionOfGoods, Is.EqualTo("Goods Description 2"), "productElement2 descriptionOfGoods");
				NUnit.Framework.Assert.That(productElement2.Commodity.CommodityCode.harmonizedSystemSubHeadingCode, Is.EqualTo("234567"), "productElement2 harmonizedSystemSubHeadingCode");
				NUnit.Framework.Assert.That(productElement2.Commodity.CommodityCode.combinedNomenclatureCode, Is.EqualTo("90"), "productElement2 combinedNomenclatureCode");
			});
		}

		[ExpectNoExceptions]
		public void TestOutwardProcessing_Abnormal()
		{
			var outwardProcessing = MockOutwardProcessing(new string[] { GetLongString("C", CountryCodeMaxLength + 1) }
					, new[]
					{
							MockIdentificationMean("!@#", GetLongString("D", IdentificationMeansDescriptionMaxLength + 1)).Object
					}
					, new[]
					{
							MockProduct(string.Empty,
								GetLongString("D", GoodsDescriptionMaxLength512 + 1),
								GetLongString("H", HarmonizedSystemSubHeadingCodeMaxLength + 1),
								GetLongString("C", CombinedNomenclatureCodeMaxLength2 + 1)).Object
					}).Object;
			mockHeader.Setup(m => m.OutwardProcessing).Returns(outwardProcessing);

			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				var reimportElement = message.GoodsShipment.OutwardProcessing.Reimport[0];
				NUnit.Framework.Assert.That(reimportElement.sequenceNumber, Is.EqualTo("1"), "reimportElement sequenceNumber");
				NUnit.Framework.Assert.That(reimportElement.country, Is.EqualTo(GetLongString("C", CountryCodeMaxLength)), "reimportElement country");

				var identificationMeansElement = message.GoodsShipment.OutwardProcessing.IdentificationMeans[0];
				NUnit.Framework.Assert.That(identificationMeansElement.sequenceNumber, Is.EqualTo("1"), "identificationMeansElement sequenceNumber");
				NUnit.Framework.Assert.That(identificationMeansElement.type.ToString(), Is.EqualTo("A"), "identificationMeansElement type");
				NUnit.Framework.Assert.That(identificationMeansElement.description, Is.EqualTo(GetLongString("D", IdentificationMeansDescriptionMaxLength)), "identificationMeansElement description");

				var productElement = message.GoodsShipment.OutwardProcessing.Product[0];
				NUnit.Framework.Assert.That(productElement.sequenceNumber, Is.EqualTo("1"), "productElement sequenceNumber");
				NUnit.Framework.Assert.That(productElement.Commodity.descriptionOfGoods, Is.EqualTo(GetLongString("D", GoodsDescriptionMaxLength512)), "productElement descriptionOfGoods");
				NUnit.Framework.Assert.That(productElement.Commodity.CommodityCode.harmonizedSystemSubHeadingCode, Is.EqualTo(GetLongString("H", HarmonizedSystemSubHeadingCodeMaxLength)), "productElement harmonizedSystemSubHeadingCode");
				NUnit.Framework.Assert.That(productElement.Commodity.CommodityCode.combinedNomenclatureCode, Is.EqualTo(GetLongString("C", CombinedNomenclatureCodeMaxLength2)), "productElement combinedNomenclatureCode");
			});
		}

		[ExpectNoExceptions]
		public void TestPreviousDocument()
		{
			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.PreviousDocument;
			var element1 = elements[0];
			var element2 = elements[1];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element1.sequenceNumber, Is.EqualTo("1"), "element1 sequenceNumber");
				NUnit.Framework.Assert.That(element1.type, Is.EqualTo("1234"), "element1 type");
				NUnit.Framework.Assert.That(element1.qualifier, Is.EqualTo("567"), "element1 qualifier");
				NUnit.Framework.Assert.That(element1.referenceNumber, Is.EqualTo("Reference1"), "element1 referenceNumber");

				NUnit.Framework.Assert.That(element2.sequenceNumber, Is.EqualTo("2"), "element2 sequenceNumber");
				NUnit.Framework.Assert.That(element2.type, Is.EqualTo("2345"), "element2 type");
				NUnit.Framework.Assert.That(element2.qualifier, Is.EqualTo("678"), "element2 qualifier");
				NUnit.Framework.Assert.That(element2.referenceNumber, Is.EqualTo("Reference2"), "element2 referenceNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestPreviousDocument_Abnormal()
		{
			var previousDocuments = new[] { MockPreviousDocument(string.Empty
							, GetLongString("R", DocumentReferenceNumberMaxLength + 1)
							, string.Empty
							, GetLongString("T", DocumentTypeMaxLength + 1)
							, GetLongString("Q", DocumentQualifierMaxLength + 1)
							).Object };
			mockHeader.Setup(m => m.PreviousDocuments).Returns(previousDocuments);

			var message = messageBuilder.GenerateMessage();
			var element = message.GoodsShipment.PreviousDocument[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element.sequenceNumber, Is.EqualTo("1"), "sequenceNumber");
				NUnit.Framework.Assert.That(element.type, Is.EqualTo(GetLongString("T", DocumentTypeMaxLength)), "type");
				NUnit.Framework.Assert.That(element.qualifier, Is.EqualTo(GetLongString("Q", DocumentQualifierMaxLength)), "qualifier");
				NUnit.Framework.Assert.That(element.referenceNumber, Is.EqualTo(GetLongString("R", DocumentReferenceNumberMaxLength)), "referenceNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestSupportingDocument()
		{
			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.SupportingDocument;
			var element1 = elements[0];
			var element2 = elements[1];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element1.sequenceNumber, Is.EqualTo("1"), "element1 sequenceNumber");
				NUnit.Framework.Assert.That(element1.type, Is.EqualTo("Typ1"), "element1 type");
				NUnit.Framework.Assert.That(element1.qualifier, Is.EqualTo("Qu1"), "element1 qualifier");
				NUnit.Framework.Assert.That(element1.referenceNumber, Is.EqualTo("Reference1"), "element1 referenceNumber");
				NUnit.Framework.Assert.That(element1.documentLineItemNumber, Is.EqualTo("1"), "element1 documentLineItemNumber");
				NUnit.Framework.Assert.That(element1.issuingAuthorityName, Is.EqualTo("Name1"), "element1 issuingAuthorityName");
				NUnit.Framework.Assert.That(element1.issuingDate, Is.EqualTo(new DateTime(2021, 01, 01)), "element1 issuingDate");
				NUnit.Framework.Assert.That(element1.issuingDateSpecified, Is.EqualTo(true), "element1 issuingDateSpecified");
				NUnit.Framework.Assert.That(element1.validityDate, Is.EqualTo(new DateTime(2021, 12, 31)), "element1 validityDate");
				NUnit.Framework.Assert.That(element1.validityDateSpecified, Is.EqualTo(true), "element1 validityDateSpecified");

				NUnit.Framework.Assert.That(element2.sequenceNumber, Is.EqualTo("2"), "element2 sequenceNumber");
				NUnit.Framework.Assert.That(element2.type, Is.EqualTo("Typ2"), "element2 type");
				NUnit.Framework.Assert.That(element2.qualifier, Is.EqualTo("Qu2"), "element2 qualifier");
				NUnit.Framework.Assert.That(element2.referenceNumber, Is.EqualTo("Reference2"), "element2 referenceNumber");
				NUnit.Framework.Assert.That(element2.documentLineItemNumber, Is.EqualTo("2"), "element2 documentLineItemNumber");
				NUnit.Framework.Assert.That(element2.issuingAuthorityName, Is.EqualTo("Name2"), "element2 issuingAuthorityName");
				NUnit.Framework.Assert.That(element2.issuingDate, Is.EqualTo(new DateTime(2022, 01, 01)), "element2 issuingDate");
				NUnit.Framework.Assert.That(element2.issuingDateSpecified, Is.EqualTo(true), "element2 issuingDateSpecified");
				NUnit.Framework.Assert.That(element2.validityDate, Is.EqualTo(new DateTime(2022, 12, 31)), "element2 validityDate");
				NUnit.Framework.Assert.That(element2.validityDateSpecified, Is.EqualTo(true), "element2 validityDateSpecified");
			});
		}

		[ExpectNoExceptions]
		public void TestSupportingDocument_Abnormal()
		{
			var supportingDocuments = new[]
			{
				MockDocument(GetLongString("Q", DocumentQualifierMaxLength + 1)
				, GetLongString("T", DocumentTypeMaxLength + 1)
				, GetLongString("R", DocumentReferenceNumberMaxLength + 1)
				, string.Empty
				, string.Empty
				, null
				, null
				, 0M
				, string.Empty
				, 0M
				, 0
				, GetLongString("A", DocumentAdditionalDescriptionMaxLength + 1)
				).Object
			};
			mockHeader.Setup(m => m.SupportingDocuments).Returns(supportingDocuments);

			var message = messageBuilder.GenerateMessage();
			var element = message.GoodsShipment.SupportingDocument[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element.sequenceNumber, Is.EqualTo("1"), "sequenceNumber");
				NUnit.Framework.Assert.That(element.type, Is.EqualTo(GetLongString("T", DocumentTypeMaxLength)), "type");
				NUnit.Framework.Assert.That(element.qualifier, Is.EqualTo(GetLongString("Q", DocumentQualifierMaxLength)), "qualifier");
				NUnit.Framework.Assert.That(element.referenceNumber, Is.EqualTo(GetLongString("R", DocumentReferenceNumberMaxLength)), "referenceNumber");
				NUnit.Framework.Assert.That(element.documentLineItemNumber, Is.EqualTo(default(string)), "documentLineItemNumber - should be [null]");
				NUnit.Framework.Assert.That(element.issuingAuthorityName, Is.EqualTo(GetLongString("A", DocumentAdditionalDescriptionMaxLength)), "issuingAuthorityName");
				NUnit.Framework.Assert.That(element.issuingDateSpecified, Is.EqualTo(false), "issuingDateSpecified");
				NUnit.Framework.Assert.That(element.validityDateSpecified, Is.EqualTo(false), "validityDateSpecified");
			});
		}

		[ExpectNoExceptions]
		public void TestAdditionalReference()
		{
			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.AdditionalReference;
			var element1 = elements[0];
			var element2 = elements[1];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element1.sequenceNumber, Is.EqualTo("1"), "element1 sequenceNumber");
				NUnit.Framework.Assert.That(element1.type, Is.EqualTo("Tpr1"), "element1 type");
				NUnit.Framework.Assert.That(element1.qualifier, Is.EqualTo("Qu1"), "element1 qualifier");
				NUnit.Framework.Assert.That(element1.referenceNumber, Is.EqualTo("Reference1"), "element1 referenceNumber");

				NUnit.Framework.Assert.That(element2.sequenceNumber, Is.EqualTo("2"), "element2 sequenceNumber");
				NUnit.Framework.Assert.That(element2.type, Is.EqualTo("Tpr2"), "element2 type");
				NUnit.Framework.Assert.That(element2.qualifier, Is.EqualTo("Qu2"), "element2 qualifier");
				NUnit.Framework.Assert.That(element2.referenceNumber, Is.EqualTo("Reference2"), "element2 referenceNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestAdditionalReference_Abnormal()
		{
			var additionalReferences = new[]
			{
				MockAdditionalInfo(type: GetLongString("T", DocumentTypeMaxLength + 1)
				, qualifier: GetLongString("Q", DocumentQualifierMaxLength + 1)
				, referenceNumber: GetLongString("R", DocumentReferenceNumberMaxLength + 1)
				).Object,
			};
			mockHeader.Setup(m => m.AdditionalReferences).Returns(additionalReferences);

			var message = messageBuilder.GenerateMessage();
			var element = message.GoodsShipment.AdditionalReference[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element.sequenceNumber, Is.EqualTo("1"), "sequenceNumber");
				NUnit.Framework.Assert.That(element.type, Is.EqualTo(GetLongString("T", DocumentTypeMaxLength)), "type");
				NUnit.Framework.Assert.That(element.qualifier, Is.EqualTo(GetLongString("Q", DocumentQualifierMaxLength)), "qualifier");
				NUnit.Framework.Assert.That(element.referenceNumber, Is.EqualTo(GetLongString("R", DocumentReferenceNumberMaxLength)), "referenceNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestAdditionalInformation()
		{
			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.AdditionalInformation;
			var element1 = elements[0];
			var element2 = elements[1];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element1.sequenceNumber, Is.EqualTo("1"), "element1 sequenceNumber");
				NUnit.Framework.Assert.That(element1.code, Is.EqualTo("Ftpi1"), "element1 code");
				NUnit.Framework.Assert.That(element1.text, Is.EqualTo("Complement1"), "element1 text");

				NUnit.Framework.Assert.That(element2.sequenceNumber, Is.EqualTo("2"), "element2 sequenceNumber");
				NUnit.Framework.Assert.That(element2.code, Is.EqualTo("Ftpi2"), "element2 code");
				NUnit.Framework.Assert.That(element2.text, Is.EqualTo("Complement2"), "element2 text");
			});
		}

		[ExpectNoExceptions]
		public void TestAdditionalInformation_Abnormal()
		{
			var additionalInformations = new[]
			{
				MockAdditionalInfo(fullType: GetLongString("F", AdditionalInformationCodeMaxLength + 1)
				, complement: GetLongString("C", AdditionalInformationTextMaxLength + 1)
				).Object,
			};
			mockHeader.Setup(m => m.AdditionalInformations).Returns(additionalInformations);

			var message = messageBuilder.GenerateMessage();
			var element = message.GoodsShipment.AdditionalInformation[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element.sequenceNumber, Is.EqualTo("1"), "sequenceNumber");
				NUnit.Framework.Assert.That(element.code, Is.EqualTo(GetLongString("F", AdditionalInformationCodeMaxLength)), "code");
				NUnit.Framework.Assert.That(element.text, Is.EqualTo(GetLongString("C", AdditionalInformationTextMaxLength)), "text");
			});
		}

		[ExpectNoExceptions]
		public void TestConsignment()
		{
			var message = messageBuilder.GenerateMessage();
			var consignment = message.GoodsShipment.Consignment;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignment.containerIndicator.ToString(), Is.EqualTo("Item0"), "containerIndicator");
				NUnit.Framework.Assert.That(consignment.inlandModeOfTransport, Is.EqualTo("1"), "inlandModeOfTransport");
				NUnit.Framework.Assert.That(consignment.modeOfTransportAtTheBorder, Is.EqualTo("1"), "modeOfTransportAtTheBorder");
				NUnit.Framework.Assert.That(consignment.grossMass, Is.EqualTo(200.55m), "grossMass");
				NUnit.Framework.Assert.That(consignment.referenceNumberUCR, Is.EqualTo("CRN00001"), "referenceNumberUCR");
				NUnit.Framework.Assert.That(consignment.registrationNumberExternal, Is.EqualTo("RN000001"), "registrationNumberExternal");
			});
		}

		[ExpectNoExceptions]
		public void TestConsignment_Abnormal()
		{
			mockHeader.Setup(m => m.InlandTransportMeansMode).Returns(GetLongString("I", TransportMeansModeMaxLength + 1));
			mockHeader.Setup(m => m.BorderTransportMeansMode).Returns(GetLongString("B", TransportMeansModeMaxLength + 1));
			mockHeader.Setup(m => m.CommercialReferenceNumber).Returns(GetLongString("C", ReferenceNumberMaxLength + 1));
			mockHeader.Setup(m => m.RegistrationNumber).Returns(GetLongString("R", RegistrationNumberMaxLength + 1));

			var message = messageBuilder.GenerateMessage();
			var consignment = message.GoodsShipment.Consignment;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignment.inlandModeOfTransport, Is.EqualTo(GetLongString("I", TransportMeansModeMaxLength)), "inlandModeOfTransport");
				NUnit.Framework.Assert.That(consignment.modeOfTransportAtTheBorder, Is.EqualTo(GetLongString("B", TransportMeansModeMaxLength)), "modeOfTransportAtTheBorder");
				NUnit.Framework.Assert.That(consignment.referenceNumberUCR, Is.EqualTo(GetLongString("C", ReferenceNumberMaxLength)), "referenceNumberUCR");
				NUnit.Framework.Assert.That(consignment.registrationNumberExternal, Is.EqualTo(GetLongString("R", RegistrationNumberMaxLength)), "registrationNumberExternal");
			});
		}

		[ExpectNoExceptions]
		public void TestCarrier_Null()
		{
			mockHeader.Setup(m => m.Carrier).Returns((IPartyID)null);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.Carrier, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentConsignmentCarrier)));
		}

		[ExpectNoExceptions]
		public void TestCarrier()
		{
			var message = messageBuilder.GenerateMessage();
			var carrier = message.GoodsShipment.Consignment.Carrier;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(carrier.identificationNumber, Is.EqualTo("DEEOR006"), "identificationNumber");
				NUnit.Framework.Assert.That(carrier.subsidiaryNumber, Is.EqualTo("0006"), "subsidiaryNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestCarrier_Abnormal()
		{
			var party = MockParty(GetLongString("E", EoriCodeMaxLength + 1)
				, GetLongString("B", EoriBranchCodeMaxLength + 1)
				).Object;
			mockHeader.Setup(m => m.Carrier).Returns(party);

			var message = messageBuilder.GenerateMessage();
			var carrier = message.GoodsShipment.Consignment.Carrier;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(carrier.identificationNumber, Is.EqualTo(GetLongString("E", EoriCodeMaxLength)), "identificationNumber");
				NUnit.Framework.Assert.That(carrier.subsidiaryNumber, Is.EqualTo(GetLongString("B", EoriBranchCodeMaxLength)), "subsidiaryNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestCarrier_TCU()
		{
			var party = MockParty(string.Empty
				, "0006"
				, tcuNumber: "DETCU006"
				).Object;
			mockHeader.Setup(m => m.Carrier).Returns(party);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.Carrier.identificationNumber, Is.EqualTo("DETCU006"));
		}

		[ExpectNoExceptions]
		public void TestCarrier_TCU_Abnormal()
		{
			var party = MockParty(string.Empty
				, string.Empty
				, tcuNumber: GetLongString("T", EoriCodeMaxLength + 1)
				).Object;
			mockHeader.Setup(m => m.Carrier).Returns(party);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.Carrier.identificationNumber, Is.EqualTo(GetLongString("T", EoriCodeMaxLength)));
		}

		[ExpectNoExceptions]
		public void TestConsignor_Null()
		{
			mockHeader.Setup(m => m.Consignor).Returns((IAESParty)null);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.Consignor, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentConsignmentConsignor)));
		}

		[ExpectNoExceptions]
		public void TestConsignor()
		{
			var message = messageBuilder.GenerateMessage();
			var consignor = message.GoodsShipment.Consignment.Consignor;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignor.identificationNumber, Is.EqualTo("DEEOR007"), "identificationNumber");
				NUnit.Framework.Assert.That(consignor.subsidiaryNumber, Is.EqualTo("0007"), "subsidiaryNumber");
				NUnit.Framework.Assert.That(consignor.name, Is.EqualTo(default(string)), "name");
				NUnit.Framework.Assert.That(consignor.Address, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentConsignmentConsignorAddress)), "Address - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestConsignor_Abnormal()
		{
			var party = MockParty(GetLongString("E", EoriCodeMaxLength + 1)
				, GetLongString("B", EoriBranchCodeMaxLength + 1)).Object;
			mockHeader.Setup(m => m.Consignor).Returns(party);

			var message = messageBuilder.GenerateMessage();
			var consignor = message.GoodsShipment.Consignment.Consignor;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignor.identificationNumber, Is.EqualTo(GetLongString("E", EoriCodeMaxLength)), "identificationNumber");
				NUnit.Framework.Assert.That(consignor.subsidiaryNumber, Is.EqualTo(GetLongString("B", EoriBranchCodeMaxLength)), "subsidiaryNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestConsignor_TCU()
		{
			var consignor = MockParty(string.Empty
			, string.Empty
			, tcuNumber: "DETCU007"
			).Object;
			mockHeader.Setup(m => m.Consignor).Returns(consignor);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.Consignor.identificationNumber, Is.EqualTo("DETCU007"));
		}

		[ExpectNoExceptions]
		public void TestConsignor_TCU_Abnormal()
		{
			var consignor = MockParty(tcuNumber: GetLongString("T", EoriCodeMaxLength + 1)).Object;
			mockHeader.Setup(m => m.Consignor).Returns(consignor);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.Consignor.identificationNumber, Is.EqualTo(GetLongString("T", EoriCodeMaxLength)));
		}

		[ExpectNoExceptions]
		public void TestConsignor_NoEOROrTCU()
		{
			var party = MockParty(string.Empty
			, string.Empty
			, "Consignor"
			, "Consignor Address1"
			, "Consignor City"
			, "123456789"
			, "DE"
			, address2: "Consignor Address2"
			).Object;
			mockHeader.Setup(m => m.Consignor).Returns(party);

			var message = messageBuilder.GenerateMessage();
			var consignor = message.GoodsShipment.Consignment.Consignor;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignor.identificationNumber, Is.EqualTo(default(string)), "identificationNumber - should be [null]");
				NUnit.Framework.Assert.That(consignor.subsidiaryNumber, Is.EqualTo(default(string)), "subsidiaryNumber - should be [null]");
				NUnit.Framework.Assert.That(consignor.name, Is.EqualTo("Consignor"), "name");
				NUnit.Framework.Assert.That(consignor.Address.streetAndNumber, Is.EqualTo("Consignor Address1Consignor Address2"), "streetAndNumber");
				NUnit.Framework.Assert.That(consignor.Address.postcode, Is.EqualTo("123456789"), "postcode");
				NUnit.Framework.Assert.That(consignor.Address.city, Is.EqualTo("Consignor City"), "city");
				NUnit.Framework.Assert.That(consignor.Address.country, Is.EqualTo("DE"), "country");
			});
		}

		[ExpectNoExceptions]
		public void TestConsignor_NoEOROrTCU_Abnormal()
		{
			var party = MockParty(string.Empty
				, string.Empty
				, GetLongString("N", PartyNameMaxLength70 + 1)
				, GetLongString("A", StreetAndNumberMaxLength + 1)
				, GetLongString("C", AddressCityMaxLength + 1)
				, GetLongString("P", AddressPostcodeMaxLength + 1)
				, GetLongString("O", AddressCountryMaxLength + 1)
				, address2: GetLongString("A", StreetAndNumberMaxLength + 1)
				).Object;
			mockHeader.Setup(m => m.Consignor).Returns(party);

			var message = messageBuilder.GenerateMessage();
			var consignor = message.GoodsShipment.Consignment.Consignor;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignor.name, Is.EqualTo(GetLongString("N", PartyNameMaxLength70)), "name");
				NUnit.Framework.Assert.That(consignor.Address.streetAndNumber, Is.EqualTo(GetLongString("A", StreetAndNumberMaxLength)), "streetAndNumber");
				NUnit.Framework.Assert.That(consignor.Address.postcode, Is.EqualTo(GetLongString("P", AddressPostcodeMaxLength)), "postcode");
				NUnit.Framework.Assert.That(consignor.Address.city, Is.EqualTo(GetLongString("C", AddressCityMaxLength)), "city");
				NUnit.Framework.Assert.That(consignor.Address.country, Is.EqualTo(GetLongString("O", AddressCountryMaxLength)), "country");
			});
		}

		[ExpectNoExceptions]
		public void TestConsignee_Null()
		{
			mockHeader.Setup(m => m.Consignee).Returns((IAESParty)null);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.Consignee, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentConsignmentConsignee)));
		}

		[ExpectNoExceptions]
		public void TestConsignee()
		{
			var message = messageBuilder.GenerateMessage();
			var consignee = message.GoodsShipment.Consignment.Consignee;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignee.identificationNumber, Is.EqualTo("DEEOR008"), "identificationNumber");
				NUnit.Framework.Assert.That(consignee.subsidiaryNumber, Is.EqualTo("0008"), "subsidiaryNumber");
				NUnit.Framework.Assert.That(consignee.name, Is.EqualTo(default(string)), "name");
				NUnit.Framework.Assert.That(consignee.Address, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentConsignmentConsigneeAddress)), "Address - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestConsignee_Abnormal()
		{
			var party = MockParty(GetLongString("E", EoriCodeMaxLength + 1)
				, GetLongString("B", EoriBranchCodeMaxLength + 1)).Object;
			mockHeader.Setup(m => m.Consignee).Returns(party);

			var message = messageBuilder.GenerateMessage();
			var consignee = message.GoodsShipment.Consignment.Consignee;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignee.identificationNumber, Is.EqualTo(GetLongString("E", EoriCodeMaxLength)), "identificationNumber");
				NUnit.Framework.Assert.That(consignee.subsidiaryNumber, Is.EqualTo(GetLongString("B", EoriBranchCodeMaxLength)), "subsidiaryNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestConsignee_TCU()
		{
			var consignee = MockParty(string.Empty
			, string.Empty
			, tcuNumber: "DETCU008"
			).Object;
			mockHeader.Setup(m => m.Consignee).Returns(consignee);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.Consignee.identificationNumber, Is.EqualTo("DETCU008"));
		}

		[ExpectNoExceptions]
		public void TestConsignee_TCU_Abnormal()
		{
			var consignee = MockParty(tcuNumber: GetLongString("T", EoriCodeMaxLength + 1)).Object;
			mockHeader.Setup(m => m.Consignee).Returns(consignee);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.Consignee.identificationNumber, Is.EqualTo(GetLongString("T", EoriCodeMaxLength)));
		}

		[ExpectNoExceptions]
		public void TestConsignee_NoEOROrTCU()
		{
			var party = MockParty(string.Empty
			, string.Empty
			, "Consignee"
			, "Consignee Address1"
			, "Consignee City"
			, "123456789"
			, "DE"
			, address2: "Consignee Address2"
			).Object;
			mockHeader.Setup(m => m.Consignee).Returns(party);

			var message = messageBuilder.GenerateMessage();
			var consignee = message.GoodsShipment.Consignment.Consignee;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignee.identificationNumber, Is.EqualTo(default(string)), "identificationNumber - should be [null]");
				NUnit.Framework.Assert.That(consignee.subsidiaryNumber, Is.EqualTo(default(string)), "subsidiaryNumber - should be [null]");
				NUnit.Framework.Assert.That(consignee.name, Is.EqualTo("Consignee"), "name");
				NUnit.Framework.Assert.That(consignee.Address.streetAndNumber, Is.EqualTo("Consignee Address1Consignee Address2"), "streetAndNumber");
				NUnit.Framework.Assert.That(consignee.Address.postcode, Is.EqualTo("123456789"), "postcode");
				NUnit.Framework.Assert.That(consignee.Address.city, Is.EqualTo("Consignee City"), "city");
				NUnit.Framework.Assert.That(consignee.Address.country, Is.EqualTo("DE"), "country");
			});
		}

		[ExpectNoExceptions]
		public void TestConsignee_NoEOROrTCU_Abnormal()
		{
			var party = MockParty(string.Empty
				, string.Empty
				, GetLongString("N", PartyNameMaxLength70 + 1)
				, GetLongString("A", StreetAndNumberMaxLength + 1)
				, GetLongString("C", AddressCityMaxLength + 1)
				, GetLongString("P", AddressPostcodeMaxLength + 1)
				, GetLongString("O", AddressCountryMaxLength + 1)
				, address2: GetLongString("A", StreetAndNumberMaxLength + 1)
				).Object;
			mockHeader.Setup(m => m.Consignee).Returns(party);

			var message = messageBuilder.GenerateMessage();
			var consignee = message.GoodsShipment.Consignment.Consignee;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignee.name, Is.EqualTo(GetLongString("N", PartyNameMaxLength70)), "name");
				NUnit.Framework.Assert.That(consignee.Address.streetAndNumber, Is.EqualTo(GetLongString("A", StreetAndNumberMaxLength)), "streetAndNumber");
				NUnit.Framework.Assert.That(consignee.Address.postcode, Is.EqualTo(GetLongString("P", AddressPostcodeMaxLength)), "postcode");
				NUnit.Framework.Assert.That(consignee.Address.city, Is.EqualTo(GetLongString("C", AddressCityMaxLength)), "city");
				NUnit.Framework.Assert.That(consignee.Address.country, Is.EqualTo(GetLongString("O", AddressCountryMaxLength)), "country");
			});
		}

		[ExpectNoExceptions]
		public void TestTransportEquipment()
		{
			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.Consignment.TransportEquipment;
			var element1 = elements[0];
			var element2 = elements[1];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element1.sequenceNumber, Is.EqualTo("1"), "element1 sequenceNumber");
				NUnit.Framework.Assert.That(element1.containerIdentificationNumber, Is.EqualTo("Reference1"), "element1 containerIdentificationNumber");
				NUnit.Framework.Assert.That(element1.numberOfSeals, Is.EqualTo("0"), "element1 numberOfSeals");
				NUnit.Framework.Assert.That(element1.Seal[0].sequenceNumber, Is.EqualTo("1"), "element1 Seal[0].sequenceNumber");
				NUnit.Framework.Assert.That(element1.Seal[0].identifier, Is.EqualTo("1001"), "element1 Seal[0].identifier");
				NUnit.Framework.Assert.That(element1.Seal[1].sequenceNumber, Is.EqualTo("2"), "element1 Seal[1].sequenceNumber");
				NUnit.Framework.Assert.That(element1.Seal[1].identifier, Is.EqualTo("1002"), "element1 Seal[1].identifier");
				NUnit.Framework.Assert.That(element1.GoodsReference[0].sequenceNumber, Is.EqualTo("1"), "element1 GoodsReference[0].sequenceNumber");
				NUnit.Framework.Assert.That(element1.GoodsReference[0].declarationGoodsItemNumber, Is.EqualTo("1"), "element1 GoodsReference[0].declarationGoodsItemNumber");
				NUnit.Framework.Assert.That(element1.GoodsReference[1].sequenceNumber, Is.EqualTo("2"), "element1 GoodsReference[1].sequenceNumber");
				NUnit.Framework.Assert.That(element1.GoodsReference[1].declarationGoodsItemNumber, Is.EqualTo("3"), "element1 GoodsReference[1].declarationGoodsItemNumber");

				NUnit.Framework.Assert.That(element2.sequenceNumber, Is.EqualTo("2"), "element2 sequenceNumber");
				NUnit.Framework.Assert.That(element2.containerIdentificationNumber, Is.EqualTo("Reference2"), "element2 containerIdentificationNumber");
				NUnit.Framework.Assert.That(element2.numberOfSeals, Is.EqualTo("0"), "element2 numberOfSeals");
				NUnit.Framework.Assert.That(element2.Seal[0].sequenceNumber, Is.EqualTo("1"), "element2 Seal[0].sequenceNumber");
				NUnit.Framework.Assert.That(element2.Seal[0].identifier, Is.EqualTo("2001"), "element2 Seal[0].identifier");
				NUnit.Framework.Assert.That(element2.Seal[1].sequenceNumber, Is.EqualTo("2"), "element2 Seal[1].sequenceNumber");
				NUnit.Framework.Assert.That(element2.Seal[1].identifier, Is.EqualTo("2002"), "element2 Seal[1].identifier");
				NUnit.Framework.Assert.That(element2.GoodsReference[0].sequenceNumber, Is.EqualTo("1"), "element2 GoodsReference[0].sequenceNumber");
				NUnit.Framework.Assert.That(element2.GoodsReference[0].declarationGoodsItemNumber, Is.EqualTo("2"), "element2 GoodsReference[0].declarationGoodsItemNumber");
				NUnit.Framework.Assert.That(element2.GoodsReference[1].sequenceNumber, Is.EqualTo("2"), "element2 GoodsReference[1].sequenceNumber");
				NUnit.Framework.Assert.That(element2.GoodsReference[1].declarationGoodsItemNumber, Is.EqualTo("4"), "element2 GoodsReference[1].declarationGoodsItemNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestTransportEquipment_Abnormal()
		{
			var transportEquipments = new[]
			{
				MockTransportEquipment(GetLongString("C", ContainerIdentificationNumberMaxLength + 1)
				, new string[] { GetLongString("S", SealIdentityMaxLength + 1) }
				, new int[] { 1 }).Object,
			};
			mockHeader.Setup(m => m.TransportEquipments).Returns(transportEquipments);

			var message = messageBuilder.GenerateMessage();
			var element = message.GoodsShipment.Consignment.TransportEquipment[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element.sequenceNumber, Is.EqualTo("1"), "sequenceNumber");
				NUnit.Framework.Assert.That(element.containerIdentificationNumber, Is.EqualTo(GetLongString("C", ContainerIdentificationNumberMaxLength)), "containerIdentificationNumber");
				NUnit.Framework.Assert.That(element.numberOfSeals, Is.EqualTo("0"), "numberOfSeals");
				NUnit.Framework.Assert.That(element.Seal[0].sequenceNumber, Is.EqualTo("1"), "Seal[0].sequenceNumber");
				NUnit.Framework.Assert.That(element.Seal[0].identifier, Is.EqualTo(GetLongString("S", SealIdentityMaxLength)), "Seal[0].identifier");
			});
		}

		[ExpectNoExceptions]
		public void TestLocationOfGoods()
		{
			var message = messageBuilder.GenerateMessage();
			var locationOfGoods = message.GoodsShipment.Consignment.LocationOfGoods;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(locationOfGoods.typeOfLocation.ToString(), Is.EqualTo("B"), "typeOfLocation");
				NUnit.Framework.Assert.That(locationOfGoods.qualifierOfIdentification.ToString(), Is.EqualTo("V"), "qualifierOfIdentification");
				NUnit.Framework.Assert.That(locationOfGoods.authorisationNumber, Is.EqualTo("AU00001"), "authorisationNumber");
				NUnit.Framework.Assert.That(locationOfGoods.additionalIdentifier, Is.EqualTo("AI01"), "additionalIdentifier");
				NUnit.Framework.Assert.That(locationOfGoods.UNLocode, Is.EqualTo("UNL001"), "UNLocoder");
				NUnit.Framework.Assert.That(locationOfGoods.GNSS.latitude, Is.EqualTo("100.23"), "latitude");
				NUnit.Framework.Assert.That(locationOfGoods.GNSS.longitude, Is.EqualTo("60.37"), "longitude");
				NUnit.Framework.Assert.That(locationOfGoods.Address.complementOfInformation, Is.EqualTo("AdditionalInformation"), "complementOfInformation");
				NUnit.Framework.Assert.That(locationOfGoods.Address.streetAndNumber, Is.EqualTo("LocationOfGoods Address1LocationOfGoods Address2"), "streetAndNumber");
				NUnit.Framework.Assert.That(locationOfGoods.Address.postcode, Is.EqualTo("123456789"), "postcode");
				NUnit.Framework.Assert.That(locationOfGoods.Address.city, Is.EqualTo("LocationOfGoods City"), "city");
				NUnit.Framework.Assert.That(locationOfGoods.Address.country, Is.EqualTo("DE"), "country");
				NUnit.Framework.Assert.That(locationOfGoods.ContactPerson.name, Is.EqualTo("VIC"), "name");
				NUnit.Framework.Assert.That(locationOfGoods.ContactPerson.phoneNumber, Is.EqualTo("1324333"), "phoneNumber");
				NUnit.Framework.Assert.That(locationOfGoods.ContactPerson.eMailAddress, Is.EqualTo("a@123.com"), "eMailAddress");
			});
		}

		[ExpectNoExceptions]
		public void TestLocationOfGoods_Abnormal()
		{
			var locationOfGoodsParty = MockPartyDocAddress(GetLongString("A", AddressAdditionalAddressInfoMaxLength + 1)
				, GetLongString("D", StreetAndNumberMaxLength + 1)
				, GetLongString("D", StreetAndNumberMaxLength + 1)
				, GetLongString("P", AddressPostcodeMaxLength + 1)
				, GetLongString("C", AddressCityMaxLength + 1)
				, GetLongString("O", CountryCodeMaxLength + 1)
				).Object;
			var locationOfGoodsContactPerson = MockContactPerson(string.Empty
				, GetLongString("N", ContactNameMaxLength70 + 1)
				, GetLongString("H", ContactPhoneNumberMaxLength + 1)
				, string.Empty
				, GetLongString("I", ContactMailAddressMaxLength + 1)
				).Object;
			mockHeader.Setup(m => m.TypeOfLocation).Returns("!");
			mockHeader.Setup(m => m.QualifierOfIdentification).Returns("@");
			mockHeader.Setup(m => m.AuthorisationNumber).Returns(GetLongString("T", AuthorisationNumberMaxLength35 + 1));
			mockHeader.Setup(m => m.AdditionalIdentifier).Returns(GetLongString("I", AdditionalIdentifierMaxLength + 1));
			mockHeader.Setup(m => m.UNLocode).Returns(GetLongString("U", UNLocodeMaxLength + 1));
			mockHeader.Setup(m => m.LocationOfGoodsParty).Returns(locationOfGoodsParty);
			mockHeader.Setup(m => m.LocationOfGoodsContactPerson).Returns(locationOfGoodsContactPerson);

			var message = messageBuilder.GenerateMessage();
			var locationOfGoods = message.GoodsShipment.Consignment.LocationOfGoods;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(locationOfGoods.typeOfLocation.ToString(), Is.EqualTo("A"), "typeOfLocation");
				NUnit.Framework.Assert.That(locationOfGoods.qualifierOfIdentification.ToString(), Is.EqualTo("U"), "qualifierOfIdentification");
				NUnit.Framework.Assert.That(locationOfGoods.authorisationNumber, Is.EqualTo(GetLongString("T", AuthorisationNumberMaxLength35)), "authorisationNumber");
				NUnit.Framework.Assert.That(locationOfGoods.additionalIdentifier, Is.EqualTo(GetLongString("I", AdditionalIdentifierMaxLength)), "additionalIdentifier");
				NUnit.Framework.Assert.That(locationOfGoods.UNLocode, Is.EqualTo(GetLongString("U", UNLocodeMaxLength)), "UNLocoder");
				NUnit.Framework.Assert.That(locationOfGoods.Address.complementOfInformation, Is.EqualTo(GetLongString("A", AddressAdditionalAddressInfoMaxLength)), "complementOfInformation");
				NUnit.Framework.Assert.That(locationOfGoods.Address.streetAndNumber, Is.EqualTo(GetLongString("D", StreetAndNumberMaxLength)), "streetAndNumber");
				NUnit.Framework.Assert.That(locationOfGoods.Address.postcode, Is.EqualTo(GetLongString("P", AddressPostcodeMaxLength)), "postcode");
				NUnit.Framework.Assert.That(locationOfGoods.Address.city, Is.EqualTo(GetLongString("C", AddressCityMaxLength)), "city");
				NUnit.Framework.Assert.That(locationOfGoods.Address.country, Is.EqualTo(GetLongString("O", CountryCodeMaxLength)), "country");
				NUnit.Framework.Assert.That(locationOfGoods.ContactPerson.name, Is.EqualTo(GetLongString("N", ContactNameMaxLength70)), "name");
				NUnit.Framework.Assert.That(locationOfGoods.ContactPerson.phoneNumber, Is.EqualTo(GetLongString("H", ContactPhoneNumberMaxLength)), "phoneNumber");
				NUnit.Framework.Assert.That(locationOfGoods.ContactPerson.eMailAddress, Is.EqualTo(GetLongString("I", ContactMailAddressMaxLength)), "eMailAddress");
			});
		}

		[ExpectNoExceptions]
		public void TestLocationOfGoods_NoAddressOrContact()
		{
			mockHeader.Setup(m => m.LocationOfGoodsParty).Returns((IPartyDocAddress)null);
			mockHeader.Setup(m => m.LocationOfGoodsContactPerson).Returns((IAESPartyContactPerson)null);

			var message = messageBuilder.GenerateMessage();
			var locationOfGoods = message.GoodsShipment.Consignment.LocationOfGoods;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(locationOfGoods.Address, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentConsignmentLocationOfGoodsAddress)), "Address - should be [null]");
				NUnit.Framework.Assert.That(locationOfGoods.ContactPerson, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentConsignmentLocationOfGoodsContactPerson)), "ContactPerson - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestLocationOfGoodsSpecified()
		{
			mockHeader.Setup(m => m.LocationOfGoodsSpecified).Returns(false);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.LocationOfGoods, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentConsignmentLocationOfGoods)));
		}

		[ExpectNoExceptions]
		public void TestLocationOfGoods_GNSSSpecified()
		{
			mockHeader.Setup(m => m.GNSSSpecified).Returns(false);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.LocationOfGoods.GNSS, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentConsignmentLocationOfGoodsGNSS)));
		}

		[ExpectNoExceptions]
		public void TestDepartureTransportMeans()
		{
			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.Consignment.DepartureTransportMeans;
			var element1 = elements[0];
			var element2 = elements[1];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element1.sequenceNumber, Is.EqualTo("1"), "element1 sequenceNumber");
				NUnit.Framework.Assert.That(element1.typeOfIdentification, Is.EqualTo("T1"), "element1 typeOfIdentification");
				NUnit.Framework.Assert.That(element1.identificationNumber, Is.EqualTo("0001"), "element1 identificationNumber");
				NUnit.Framework.Assert.That(element1.nationality, Is.EqualTo("DE"), "element1 nationality");

				NUnit.Framework.Assert.That(element2.sequenceNumber, Is.EqualTo("2"), "element2 sequenceNumber");
				NUnit.Framework.Assert.That(element2.typeOfIdentification, Is.EqualTo("T2"), "element2 typeOfIdentification");
				NUnit.Framework.Assert.That(element2.identificationNumber, Is.EqualTo("0002"), "element2 identificationNumber");
				NUnit.Framework.Assert.That(element2.nationality, Is.EqualTo("AU"), "element2 nationality");
			});
		}

		[ExpectNoExceptions]
		public void TestDepartureTransportMeans_Abnormal()
		{
			var departureTransportMeans = new[]
			{
				MockDepartureTransportMeans(GetLongString("T", TransportMeansTypeMaxLength + 1)
				, GetLongString("I", TransportMeansIdentityMaxLength + 1)
				, GetLongString("N", TransportMeansNationalityMaxLength + 1)
				).Object,
			};
			mockHeader.Setup(m => m.DepartureTransportMeans).Returns(departureTransportMeans);

			var message = messageBuilder.GenerateMessage();
			var element = message.GoodsShipment.Consignment.DepartureTransportMeans[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element.sequenceNumber, Is.EqualTo("1"), "sequenceNumber");
				NUnit.Framework.Assert.That(element.typeOfIdentification, Is.EqualTo(GetLongString("T", TransportMeansTypeMaxLength)), "typeOfIdentification");
				NUnit.Framework.Assert.That(element.identificationNumber, Is.EqualTo(GetLongString("I", TransportMeansIdentityMaxLength)), "identificationNumber");
				NUnit.Framework.Assert.That(element.nationality, Is.EqualTo(GetLongString("N", TransportMeansNationalityMaxLength)), "nationality");
			});
		}

		[ExpectNoExceptions]
		public void TestCountryOfRoutingOfConsignment()
		{
			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.Consignment.CountryOfRoutingOfConsignment;
			var element1 = elements[0];
			var element2 = elements[1];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element1.sequenceNumber, Is.EqualTo("1"), "element1 sequenceNumber");
				NUnit.Framework.Assert.That(element1.country, Is.EqualTo("CN"), "element1 country");

				NUnit.Framework.Assert.That(element2.sequenceNumber, Is.EqualTo("2"), "element2 sequenceNumber");
				NUnit.Framework.Assert.That(element2.country, Is.EqualTo("US"), "element2 country");
			});
		}

		[ExpectNoExceptions]
		public void TestCountryOfRoutingOfConsignment_Abnormal()
		{
			mockHeader.Setup(m => m.ItineraryCountries).Returns(new ZString[] { GetLongString("C", CountryCodeMaxLength + 1) });

			var message = messageBuilder.GenerateMessage();
			var element = message.GoodsShipment.Consignment.CountryOfRoutingOfConsignment[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element.sequenceNumber, Is.EqualTo("1"), "sequenceNumber");
				NUnit.Framework.Assert.That(element.country, Is.EqualTo(GetLongString("C", CountryCodeMaxLength)), "country");
			});
		}

		[ExpectNoExceptions]
		public void TestActiveBorderTransportMeans()
		{
			var message = messageBuilder.GenerateMessage();
			var activeBorderTransportMeans = message.GoodsShipment.Consignment.ActiveBorderTransportMeans;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(activeBorderTransportMeans.typeOfIdentification, Is.EqualTo("10"), "typeOfIdentification");
				NUnit.Framework.Assert.That(activeBorderTransportMeans.identificationNumber, Is.EqualTo("1234567890"), "identificationNumber");
				NUnit.Framework.Assert.That(activeBorderTransportMeans.nationality, Is.EqualTo("AU"), "nationality");
			});
		}

		[ExpectNoExceptions]
		public void TestActiveBorderTransportMeans_Abnormal()
		{
			mockHeader.Setup(m => m.BorderTransportMeansType).Returns(GetLongString("T", TransportMeansTypeMaxLength + 1));
			mockHeader.Setup(m => m.BorderTransportMeansIdentity).Returns(GetLongString("I", TransportMeansIdentityMaxLength + 1));
			mockHeader.Setup(m => m.BorderTransportMeansNationality).Returns(GetLongString("N", TransportMeansNationalityMaxLength + 1));

			var message = messageBuilder.GenerateMessage();
			var activeBorderTransportMeans = message.GoodsShipment.Consignment.ActiveBorderTransportMeans;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(activeBorderTransportMeans.typeOfIdentification, Is.EqualTo(GetLongString("T", TransportMeansTypeMaxLength)), "typeOfIdentification");
				NUnit.Framework.Assert.That(activeBorderTransportMeans.identificationNumber, Is.EqualTo(GetLongString("I", TransportMeansIdentityMaxLength)), "identificationNumber");
				NUnit.Framework.Assert.That(activeBorderTransportMeans.nationality, Is.EqualTo(GetLongString("N", TransportMeansNationalityMaxLength)), "nationality");
			});
		}

		[ExpectNoExceptions]
		public void TestTransportDocument()
		{
			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.Consignment.TransportDocument;
			var element1 = elements[0];
			var element2 = elements[1];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element1.sequenceNumber, Is.EqualTo("1"), "element1 sequenceNumber");
				NUnit.Framework.Assert.That(element1.type, Is.EqualTo("Tpt1"), "element1 type");
				NUnit.Framework.Assert.That(element1.qualifier, Is.EqualTo("Qu1"), "element1 qualifier");
				NUnit.Framework.Assert.That(element1.referenceNumber, Is.EqualTo("Reference1"), "element1 referenceNumber");

				NUnit.Framework.Assert.That(element2.sequenceNumber, Is.EqualTo("2"), "element2 sequenceNumber");
				NUnit.Framework.Assert.That(element2.type, Is.EqualTo("Tpt2"), "element2 type");
				NUnit.Framework.Assert.That(element2.qualifier, Is.EqualTo("Qu2"), "element2 qualifier");
				NUnit.Framework.Assert.That(element2.referenceNumber, Is.EqualTo("Reference2"), "element2 referenceNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestTransportDocument_Abnormal()
		{
			var transportEquipments = new[]
			{
				MockAdditionalInfo(type: GetLongString("T", DocumentTypeMaxLength + 1)
				, qualifier: GetLongString("Q", DocumentQualifierMaxLength + 1)
				, referenceNumber: GetLongString("R", DocumentReferenceNumberMaxLength + 1)
				).Object
			};
			mockHeader.Setup(m => m.TransportDocuments).Returns(transportEquipments);

			var message = messageBuilder.GenerateMessage();
			var element = message.GoodsShipment.Consignment.TransportDocument[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element.sequenceNumber, Is.EqualTo("1"), "sequenceNumber");
				NUnit.Framework.Assert.That(element.type, Is.EqualTo(GetLongString("T", DocumentTypeMaxLength)), "type");
				NUnit.Framework.Assert.That(element.qualifier, Is.EqualTo(GetLongString("Q", DocumentQualifierMaxLength)), "qualifier");
				NUnit.Framework.Assert.That(element.referenceNumber, Is.EqualTo(GetLongString("R", DocumentReferenceNumberMaxLength)), "referenceNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestTransportCharges()
		{
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.TransportCharges.methodOfPayment, Is.EqualTo("B"));
		}

		[ExpectNoExceptions]
		public void TestTransportCharges_Abnormal()
		{
			mockHeader.Setup(m => m.TransportChargesPaymentMethod).Returns(GetLongString("T", TransportChargesPaymentMethodMaxLength + 1));

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.Consignment.TransportCharges.methodOfPayment, Is.EqualTo(GetLongString("T", TransportChargesPaymentMethodMaxLength)));
		}

		[ExpectNoExceptions]
		public void TestItem()
		{
			var message = messageBuilder.GenerateMessage();
			var item = message.GoodsShipment.GoodsItem[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(item.sequenceNumber, Is.EqualTo("1"), "sequenceNumber");
				NUnit.Framework.Assert.That(item.declarationGoodsItemNumber, Is.EqualTo("1"), "declarationGoodsItemNumber");
				NUnit.Framework.Assert.That(item.statisticalValue, Is.EqualTo(1.11M), "statisticalValue");
				NUnit.Framework.Assert.That(item.statisticalValueSpecified, Is.EqualTo(true), "statisticalValueSpecified");
				NUnit.Framework.Assert.That(item.natureOfTransaction, Is.EqualTo("12"), "natureOfTransaction");
				NUnit.Framework.Assert.That(item.countryOfExport, Is.EqualTo("SG"), "countryOfExport");
				NUnit.Framework.Assert.That(item.countryOfDestination, Is.EqualTo("TR"), "countryOfDestination");
				NUnit.Framework.Assert.That(item.referenceNumberUCR, Is.EqualTo("CRN0001"), "referenceNumberUCR");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Abnormal()
		{
			mockLine.Setup(m => m.StatisticalValueSpecified).Returns(false);
			mockLine.Setup(m => m.TransactionType).Returns(GetLongString("T", TransportChargesPaymentMethodMaxLength + 1));
			mockLine.Setup(m => m.CountryOfExport).Returns(GetLongString("E", CountryCodeMaxLength + 1));
			mockLine.Setup(m => m.CountryOfDestination).Returns(GetLongString("D", CountryCodeMaxLength + 1));
			mockLine.Setup(m => m.CommercialReferenceNumber).Returns(GetLongString("R", ReferenceNumberMaxLength + 1));

			var message = messageBuilder.GenerateMessage();
			var item = message.GoodsShipment.GoodsItem[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(item.statisticalValueSpecified, Is.EqualTo(false), "statisticalValueSpecified");
				NUnit.Framework.Assert.That(item.natureOfTransaction, Is.EqualTo(GetLongString("T", TransactionTypeMaxLength)), "natureOfTransaction");
				NUnit.Framework.Assert.That(item.countryOfExport, Is.EqualTo(GetLongString("E", CountryCodeMaxLength)), "countryOfExport");
				NUnit.Framework.Assert.That(item.countryOfDestination, Is.EqualTo(GetLongString("D", CountryCodeMaxLength)), "countryOfDestination");
				NUnit.Framework.Assert.That(item.referenceNumberUCR, Is.EqualTo(GetLongString("R", ReferenceNumberMaxLength)), "referenceNumberUCR");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Authorisation()
		{
			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.GoodsItem[0].Authorisation;
			var element1 = elements[0];
			var element2 = elements[1];
			var element3 = elements[2];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element1.sequenceNumber, Is.EqualTo("1"), "element1 sequenceNumber");
				NUnit.Framework.Assert.That(element1.type.ToString(), Is.EqualTo("C516"), "element1 type");
				NUnit.Framework.Assert.That(element1.referenceNumber, Is.EqualTo("Reference1"), "element1 referenceNumber");
				NUnit.Framework.Assert.That(element1.holderOfAuthorisation, Is.EqualTo(default(string)), "element1 holderOfAuthorisation");

				NUnit.Framework.Assert.That(element2.sequenceNumber, Is.EqualTo("2"), "element2 sequenceNumber");
				NUnit.Framework.Assert.That(element2.type.ToString(), Is.EqualTo("C626"), "element2 type");
				NUnit.Framework.Assert.That(element2.referenceNumber, Is.EqualTo("Reference2"), "element2 referenceNumber");
				NUnit.Framework.Assert.That(element2.holderOfAuthorisation, Is.EqualTo("Detail2"), "element2 holderOfAuthorisation");

				NUnit.Framework.Assert.That(element3.sequenceNumber, Is.EqualTo("3"), "element3 sequenceNumber");
				NUnit.Framework.Assert.That(element3.type.ToString(), Is.EqualTo("C627"), "element3 type");
				NUnit.Framework.Assert.That(element3.referenceNumber, Is.EqualTo("Reference3"), "element3 referenceNumber");
				NUnit.Framework.Assert.That(element3.holderOfAuthorisation, Is.EqualTo("Detail3"), "element3 holderOfAuthorisation");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Authorisation_Abnormal()
		{
			var authorisations = new[]
			{
				MockAdditionalInfo(type: "!@#"
				, referenceNumber: GetLongString("R", AuthorisationReferenceNumberMaxLength + 1)
				).Object
			};
			mockLine.Setup(m => m.Authorisations).Returns(authorisations);

			var message = messageBuilder.GenerateMessage();
			var element = message.GoodsShipment.GoodsItem[0].Authorisation[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element.sequenceNumber, Is.EqualTo("1"), "sequenceNumber");
				NUnit.Framework.Assert.That(element.type.ToString(), Is.EqualTo("C019"), "type");
				NUnit.Framework.Assert.That(element.referenceNumber, Is.EqualTo(GetLongString("R", AuthorisationReferenceNumberMaxLength)), "referenceNumber");
				NUnit.Framework.Assert.That(element.holderOfAuthorisation, Is.EqualTo(default(string)), "holderOfAuthorisation");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Procedure()
		{
			var message = messageBuilder.GenerateMessage();
			var procedure = message.GoodsShipment.GoodsItem[0].Procedure;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(procedure.requestedProcedure, Is.EqualTo("12"), "requestedProcedure");
				NUnit.Framework.Assert.That(procedure.previousProcedure, Is.EqualTo("34"), "previousProcedure");
				NUnit.Framework.Assert.That(procedure.AdditionalProcedure.sequenceNumber, Is.EqualTo("1"), "sequenceNumber");
				NUnit.Framework.Assert.That(procedure.AdditionalProcedure.additionalProcedure, Is.EqualTo("567"), "additionalProcedure");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Procedure_Abnormal()
		{
			mockLine.Setup(m => m.RequestedProcedure).Returns(GetLongString("R", RequestedProcedureMaxLength + 1));
			mockLine.Setup(m => m.PreviousProcedure).Returns(GetLongString("P", PreviousProcedureMaxlength + 1));
			mockLine.Setup(m => m.AdditionalProcedure).Returns(GetLongString("A", AdditionalProcedureMaxLength + 1));

			var message = messageBuilder.GenerateMessage();
			var procedure = message.GoodsShipment.GoodsItem[0].Procedure;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(procedure.requestedProcedure, Is.EqualTo(GetLongString("R", RequestedProcedureMaxLength)), "requestedProcedure");
				NUnit.Framework.Assert.That(procedure.previousProcedure, Is.EqualTo(GetLongString("P", PreviousProcedureMaxlength)), "previousProcedure");
				NUnit.Framework.Assert.That(procedure.AdditionalProcedure.additionalProcedure, Is.EqualTo(GetLongString("A", AdditionalProcedureMaxLength)), "additionalProcedure");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Procedure_AdditionalProcedure_Null()
		{
			mockLine.Setup(m => m.AdditionalProcedure).Returns(ZString.Empty);

			var message = messageBuilder.GenerateMessage();
			var procedure = message.GoodsShipment.GoodsItem[0].Procedure;
			NUnit.Framework.Assert.That(procedure.AdditionalProcedure, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentGoodsItemProcedureAdditionalProcedure)));
		}

		[ExpectNoExceptions]
		public void TestItem_Consignor_Null()
		{
			mockLine.Setup(m => m.Consignor).Returns((IAESParty)null);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem[0].Consignor, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentGoodsItemConsignor)));
		}

		[ExpectNoExceptions]
		public void TestItem_Consignor()
		{
			var message = messageBuilder.GenerateMessage();
			var consignor = message.GoodsShipment.GoodsItem[0].Consignor;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignor.identificationNumber, Is.EqualTo("DEEOR007"), "identificationNumber");
				NUnit.Framework.Assert.That(consignor.subsidiaryNumber, Is.EqualTo("0007"), "subsidiaryNumber");
				NUnit.Framework.Assert.That(consignor.name, Is.EqualTo(default(string)), "name");
				NUnit.Framework.Assert.That(consignor.Address, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentGoodsItemConsignorAddress)), "Address - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Consignor_Abnormal()
		{
			var party = MockParty(GetLongString("E", EoriCodeMaxLength + 1)
				, GetLongString("B", EoriBranchCodeMaxLength + 1)).Object;
			mockLine.Setup(m => m.Consignor).Returns(party);

			var message = messageBuilder.GenerateMessage();
			var consignor = message.GoodsShipment.GoodsItem[0].Consignor;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignor.identificationNumber, Is.EqualTo(GetLongString("E", EoriCodeMaxLength)), "identificationNumber");
				NUnit.Framework.Assert.That(consignor.subsidiaryNumber, Is.EqualTo(GetLongString("B", EoriBranchCodeMaxLength)), "subsidiaryNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Consignor_TCU()
		{
			var party = MockParty(string.Empty
			, string.Empty
			, tcuNumber: "DETCU007"
			).Object;
			mockLine.Setup(m => m.Consignor).Returns(party);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem[0].Consignor.identificationNumber, Is.EqualTo("DETCU007"));
		}

		[ExpectNoExceptions]
		public void TestItem_Consignor_TCU_Abnormal()
		{
			var party = MockParty(tcuNumber: GetLongString("T", EoriCodeMaxLength + 1)).Object;
			mockLine.Setup(m => m.Consignor).Returns(party);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem[0].Consignor.identificationNumber, Is.EqualTo(GetLongString("T", EoriCodeMaxLength)));
		}

		[ExpectNoExceptions]
		public void TestItem_Consignor_NoEOROrTCU()
		{
			var party = MockParty(string.Empty
			, string.Empty
			, "Consignor"
			, "Consignor Address1"
			, "Consignor City"
			, "123456789"
			, "DE"
			, address2: "Consignor Address2"
			).Object;
			mockLine.Setup(m => m.Consignor).Returns(party);

			var message = messageBuilder.GenerateMessage();
			var consignor = message.GoodsShipment.GoodsItem[0].Consignor;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignor.identificationNumber, Is.EqualTo(default(string)), "identificationNumber - should be [null]");
				NUnit.Framework.Assert.That(consignor.subsidiaryNumber, Is.EqualTo(default(string)), "subsidiaryNumber - should be [null]");
				NUnit.Framework.Assert.That(consignor.name, Is.EqualTo("Consignor"), "name");
				NUnit.Framework.Assert.That(consignor.Address.streetAndNumber, Is.EqualTo("Consignor Address1Consignor Address2"), "streetAndNumber");
				NUnit.Framework.Assert.That(consignor.Address.postcode, Is.EqualTo("123456789"), "postcode");
				NUnit.Framework.Assert.That(consignor.Address.city, Is.EqualTo("Consignor City"), "city");
				NUnit.Framework.Assert.That(consignor.Address.country, Is.EqualTo("DE"), "country");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Consignor_NoEOROrTCU_Abnormal()
		{
			var party = MockParty(string.Empty
				, string.Empty
				, GetLongString("N", PartyNameMaxLength70 + 1)
				, GetLongString("A", StreetAndNumberMaxLength + 1)
				, GetLongString("C", AddressCityMaxLength + 1)
				, GetLongString("P", AddressPostcodeMaxLength + 1)
				, GetLongString("O", AddressCountryMaxLength + 1)
				, address2: GetLongString("A", StreetAndNumberMaxLength + 1)
				).Object;
			mockLine.Setup(m => m.Consignor).Returns(party);

			var message = messageBuilder.GenerateMessage();
			var consignor = message.GoodsShipment.GoodsItem[0].Consignor;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignor.name, Is.EqualTo(GetLongString("N", PartyNameMaxLength70)), "name");
				NUnit.Framework.Assert.That(consignor.Address.streetAndNumber, Is.EqualTo(GetLongString("A", StreetAndNumberMaxLength)), "streetAndNumber");
				NUnit.Framework.Assert.That(consignor.Address.postcode, Is.EqualTo(GetLongString("P", AddressPostcodeMaxLength)), "postcode");
				NUnit.Framework.Assert.That(consignor.Address.city, Is.EqualTo(GetLongString("C", AddressCityMaxLength)), "city");
				NUnit.Framework.Assert.That(consignor.Address.country, Is.EqualTo(GetLongString("O", AddressCountryMaxLength)), "country");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Consignee_Null()
		{
			mockLine.Setup(m => m.Consignee).Returns((IAESParty)null);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem[0].Consignee, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentGoodsItemConsignee)));
		}

		[ExpectNoExceptions]
		public void TestItem_Consignee()
		{
			var message = messageBuilder.GenerateMessage();
			var consignee = message.GoodsShipment.GoodsItem[0].Consignee;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignee.identificationNumber, Is.EqualTo("DEEOR008"), "identificationNumber");
				NUnit.Framework.Assert.That(consignee.subsidiaryNumber, Is.EqualTo("0008"), "subsidiaryNumber");
				NUnit.Framework.Assert.That(consignee.name, Is.EqualTo(default(string)), "name");
				NUnit.Framework.Assert.That(consignee.Address, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentGoodsItemConsigneeAddress)), "Address - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Consignee_Abnormal()
		{
			var party = MockParty(GetLongString("E", EoriCodeMaxLength + 1)
				, GetLongString("B", EoriBranchCodeMaxLength + 1)).Object;
			mockLine.Setup(m => m.Consignee).Returns(party);

			var message = messageBuilder.GenerateMessage();
			var consignee = message.GoodsShipment.GoodsItem[0].Consignee;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignee.identificationNumber, Is.EqualTo(GetLongString("E", EoriCodeMaxLength)), "identificationNumber");
				NUnit.Framework.Assert.That(consignee.subsidiaryNumber, Is.EqualTo(GetLongString("B", EoriBranchCodeMaxLength)), "subsidiaryNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Consignee_TCU()
		{
			var party = MockParty(string.Empty
			, string.Empty
			, tcuNumber: "DETCU008"
			).Object;
			mockLine.Setup(m => m.Consignee).Returns(party);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem[0].Consignee.identificationNumber, Is.EqualTo("DETCU008"));
		}

		[ExpectNoExceptions]
		public void TestItem_Consignee_TCU_Abnormal()
		{
			var party = MockParty(tcuNumber: GetLongString("T", EoriCodeMaxLength + 1)).Object;
			mockLine.Setup(m => m.Consignee).Returns(party);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem[0].Consignee.identificationNumber, Is.EqualTo(GetLongString("T", EoriCodeMaxLength)));
		}

		[ExpectNoExceptions]
		public void TestItem_Consignee_NoEOROrTCU()
		{
			var party = MockParty(string.Empty
			, string.Empty
			, "Consignee"
			, "Consignee Address1"
			, "Consignee City"
			, "123456789"
			, "DE"
			, address2: "Consignee Address2"
			).Object;
			mockLine.Setup(m => m.Consignee).Returns(party);

			var message = messageBuilder.GenerateMessage();
			var consignee = message.GoodsShipment.GoodsItem[0].Consignee;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignee.identificationNumber, Is.EqualTo(default(string)), "identificationNumber - should be [null]");
				NUnit.Framework.Assert.That(consignee.subsidiaryNumber, Is.EqualTo(default(string)), "subsidiaryNumber - should be [null]");
				NUnit.Framework.Assert.That(consignee.name, Is.EqualTo("Consignee"), "name");
				NUnit.Framework.Assert.That(consignee.Address.streetAndNumber, Is.EqualTo("Consignee Address1Consignee Address2"), "streetAndNumber");
				NUnit.Framework.Assert.That(consignee.Address.postcode, Is.EqualTo("123456789"), "postcode");
				NUnit.Framework.Assert.That(consignee.Address.city, Is.EqualTo("Consignee City"), "city");
				NUnit.Framework.Assert.That(consignee.Address.country, Is.EqualTo("DE"), "country");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Consignee_NoEOROrTCU_Abnormal()
		{
			var party = MockParty(string.Empty
				, string.Empty
				, GetLongString("N", PartyNameMaxLength70 + 1)
				, GetLongString("A", StreetAndNumberMaxLength + 1)
				, GetLongString("C", AddressCityMaxLength + 1)
				, GetLongString("P", AddressPostcodeMaxLength + 1)
				, GetLongString("O", AddressCountryMaxLength + 1)
				, address2: GetLongString("A", StreetAndNumberMaxLength + 1)
				).Object;
			mockLine.Setup(m => m.Consignee).Returns(party);

			var message = messageBuilder.GenerateMessage();
			var consignee = message.GoodsShipment.GoodsItem[0].Consignee;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(consignee.name, Is.EqualTo(GetLongString("N", PartyNameMaxLength70)), "name");
				NUnit.Framework.Assert.That(consignee.Address.streetAndNumber, Is.EqualTo(GetLongString("A", StreetAndNumberMaxLength)), "streetAndNumber");
				NUnit.Framework.Assert.That(consignee.Address.postcode, Is.EqualTo(GetLongString("P", AddressPostcodeMaxLength)), "postcode");
				NUnit.Framework.Assert.That(consignee.Address.city, Is.EqualTo(GetLongString("C", AddressCityMaxLength)), "city");
				NUnit.Framework.Assert.That(consignee.Address.country, Is.EqualTo(GetLongString("O", AddressCountryMaxLength)), "country");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_AdditionalSupplyChainActor()
		{
			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.GoodsItem[0].AdditionalSupplyChainActor;
			var element1 = elements[0];
			var element2 = elements[1];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element1.sequenceNumber, Is.EqualTo("1"), "element1 sequenceNumber");
				NUnit.Framework.Assert.That(element1.role, Is.EqualTo("AAA"), "element1 role");
				NUnit.Framework.Assert.That(element1.identificationNumber, Is.EqualTo("1234567890"), "element1 identificationNumber");

				NUnit.Framework.Assert.That(element2.sequenceNumber, Is.EqualTo("2"), "element2 sequenceNumber");
				NUnit.Framework.Assert.That(element2.role, Is.EqualTo("BBB"), "element2 troleype");
				NUnit.Framework.Assert.That(element2.identificationNumber, Is.EqualTo("1234567891"), "element2 identificationNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_AdditionalSupplyChainActor_Abnormal()
		{
			var additionalSupplyChainActors = new[]
			{
				MockSupplyChainActor(GetLongString("R", SupplyChainActorRoleMaxLength)
				, GetLongString("I", SupplyChainActorIdentificationNumberMaxLength)
				).Object,
			};
			mockLine.Setup(m => m.AdditionalSupplyChainActors).Returns(additionalSupplyChainActors);

			var message = messageBuilder.GenerateMessage();
			var element = message.GoodsShipment.GoodsItem[0].AdditionalSupplyChainActor[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element.sequenceNumber, Is.EqualTo("1"), "sequenceNumber");
				NUnit.Framework.Assert.That(element.role, Is.EqualTo(GetLongString("R", SupplyChainActorRoleMaxLength)), "role");
				NUnit.Framework.Assert.That(element.identificationNumber, Is.EqualTo(GetLongString("I", SupplyChainActorIdentificationNumberMaxLength)), "identificationNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Origin()
		{
			var message = messageBuilder.GenerateMessage();
			var origin = message.GoodsShipment.GoodsItem[0].Origin;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(origin.countryOfOrigin, Is.EqualTo("DE"), "countryOfOrigin");
				NUnit.Framework.Assert.That(origin.regionOfDispatch, Is.EqualTo("03"), "regionOfDispatch");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Origin_Abnormal()
		{
			mockLine.Setup(m => m.CountryOfOrigin).Returns(GetLongString("C", CountryCodeMaxLength + 1));
			mockLine.Setup(m => m.OriginFederalState).Returns(GetLongString("O", RegionOfDispatchMaxLength + 1));

			var message = messageBuilder.GenerateMessage();
			var origin = message.GoodsShipment.GoodsItem[0].Origin;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(origin.countryOfOrigin, Is.EqualTo(GetLongString("C", CountryCodeMaxLength)), "countryOfOrigin");
				NUnit.Framework.Assert.That(origin.regionOfDispatch, Is.EqualTo(GetLongString("O", RegionOfDispatchMaxLength)), "regionOfDispatch");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Commodity()
		{
			var message = messageBuilder.GenerateMessage();
			var commodity = message.GoodsShipment.GoodsItem[0].Commodity;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(commodity.descriptionOfGoods, Is.EqualTo("Goods Description 1"), "descriptionOfGoods");
				NUnit.Framework.Assert.That(commodity.cusCode, Is.EqualTo("CC001"), "cusCode");
				NUnit.Framework.Assert.That(commodity.CommodityCode.harmonizedSystemSubHeadingCode, Is.EqualTo("HSSC01"), "harmonizedSystemSubHeadingCode");
				NUnit.Framework.Assert.That(commodity.CommodityCode.combinedNomenclatureCode, Is.EqualTo("C1"), "combinedNomenclatureCode");

				NUnit.Framework.Assert.That(commodity.CommodityCode.TARICAdditionalCode.Length, Is.EqualTo(5), "TARICAdditionalCode is allowed to have 5 elements");
				NUnit.Framework.Assert.That(commodity.CommodityCode.TARICAdditionalCode[0].sequenceNumber, Is.EqualTo("1"), "TARICAdditionalCode[0].sequenceNumber");
				NUnit.Framework.Assert.That(commodity.CommodityCode.TARICAdditionalCode[0].taricAdditionalCode, Is.EqualTo("TF01"), "TARICAdditionalCode[0].taricAdditionalCode");
				NUnit.Framework.Assert.That(commodity.CommodityCode.TARICAdditionalCode[1].sequenceNumber, Is.EqualTo("2"), "TARICAdditionalCode[1].sequenceNumber");
				NUnit.Framework.Assert.That(commodity.CommodityCode.TARICAdditionalCode[1].taricAdditionalCode, Is.EqualTo("TF02"), "TARICAdditionalCode[1].taricAdditionalCode");
				NUnit.Framework.Assert.That(commodity.CommodityCode.TARICAdditionalCode[2].sequenceNumber, Is.EqualTo("3"), "TARICAdditionalCode[2].sequenceNumber");
				NUnit.Framework.Assert.That(commodity.CommodityCode.TARICAdditionalCode[2].taricAdditionalCode, Is.EqualTo("TF03"), "TARICAdditionalCode[2].taricAdditionalCode");
				NUnit.Framework.Assert.That(commodity.CommodityCode.TARICAdditionalCode[3].sequenceNumber, Is.EqualTo("4"), "TARICAdditionalCode[3].sequenceNumber");
				NUnit.Framework.Assert.That(commodity.CommodityCode.TARICAdditionalCode[3].taricAdditionalCode, Is.EqualTo("TF04"), "TARICAdditionalCode[3].taricAdditionalCode");
				NUnit.Framework.Assert.That(commodity.CommodityCode.TARICAdditionalCode[4].sequenceNumber, Is.EqualTo("5"), "TARICAdditionalCode[4].sequenceNumber");
				NUnit.Framework.Assert.That(commodity.CommodityCode.TARICAdditionalCode[4].taricAdditionalCode, Is.EqualTo("TF05"), "TARICAdditionalCode[4].taricAdditionalCode");

				var dangerousGoods = commodity.DangerousGoods.Single();
				NUnit.Framework.Assert.That(dangerousGoods.sequenceNumber, Is.EqualTo("1"), "DangerousGoods[0].sequenceNumber");
				NUnit.Framework.Assert.That(dangerousGoods.UNNumber, Is.EqualTo("DG01"), "DangerousGoods[0].UNNumber");

				NUnit.Framework.Assert.That(commodity.GoodsMeasure.grossMass, Is.EqualTo(2.23m), "grossMass");
				NUnit.Framework.Assert.That(commodity.GoodsMeasure.netMass, Is.EqualTo(3.32m), "netMass");
				NUnit.Framework.Assert.That(commodity.GoodsMeasure.supplementaryUnits, Is.EqualTo(4.16m), "supplementaryUnits");
				NUnit.Framework.Assert.That(commodity.GoodsMeasure.supplementaryUnitsSpecified, Is.EqualTo(true), "supplementaryUnitsSpecified");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Commodity_Abnormal()
		{
			mockLine.Setup(m => m.GoodsDescription).Returns(GetLongString("D", GoodsDescriptionMaxLength + 1));
			mockLine.Setup(m => m.CusCode).Returns(GetLongString("C", CusCodeMaxLength + 1));
			mockLine.Setup(m => m.HarmonizedSystemSubHeadingCode).Returns(GetLongString("H", HarmonizedSystemSubHeadingCodeMaxLength + 1));
			mockLine.Setup(m => m.CombinedNomenclatureCode).Returns(GetLongString("N", CombinedNomenclatureCodeMaxLength2 + 1));
			mockLine.Setup(m => m.TaricFirstAdditionalCode).Returns(GetLongString("F", TaricAdditionalCodeMaxLength + 1));
			mockLine.Setup(m => m.TaricSecondAdditionalCode).Returns(GetLongString("S", TaricAdditionalCodeMaxLength + 1));
			mockLine.Setup(m => m.DangerousGoodsCodes).Returns(new string[] { GetLongString("G", DangerousGoodsCodeMaxLength + 1) });
			mockLine.Setup(m => m.SupplementaryQuantity).Returns(0M);
			mockLine.Setup(m => m.GrossMass).Returns(1.123M);
			mockLine.Setup(m => m.NetMass).Returns(2.234M);

			var message = messageBuilder.GenerateMessage();
			var commodity = message.GoodsShipment.GoodsItem[0].Commodity;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(commodity.descriptionOfGoods, Is.EqualTo(GetLongString("D", GoodsDescriptionMaxLength)), "descriptionOfGoods");
				NUnit.Framework.Assert.That(commodity.cusCode, Is.EqualTo(GetLongString("C", CusCodeMaxLength)), "cusCode");
				NUnit.Framework.Assert.That(commodity.CommodityCode.harmonizedSystemSubHeadingCode, Is.EqualTo(GetLongString("H", HarmonizedSystemSubHeadingCodeMaxLength)), "harmonizedSystemSubHeadingCode");
				NUnit.Framework.Assert.That(commodity.CommodityCode.combinedNomenclatureCode, Is.EqualTo(GetLongString("N", CombinedNomenclatureCodeMaxLength2)), "combinedNomenclatureCode");

				NUnit.Framework.Assert.That(commodity.CommodityCode.TARICAdditionalCode[0].taricAdditionalCode, Is.EqualTo(GetLongString("F", TaricAdditionalCodeMaxLength)), "TARICAdditionalCode[0].taricAdditionalCode");
				NUnit.Framework.Assert.That(commodity.CommodityCode.TARICAdditionalCode[1].taricAdditionalCode, Is.EqualTo(GetLongString("S", TaricAdditionalCodeMaxLength)), "TARICAdditionalCode[1].taricAdditionalCode");

				NUnit.Framework.Assert.That(commodity.DangerousGoods[0].UNNumber, Is.EqualTo(GetLongString("G", DangerousGoodsCodeMaxLength)), "DangerousGoods[0].UNNumber");

				NUnit.Framework.Assert.That(commodity.GoodsMeasure.supplementaryUnitsSpecified, Is.EqualTo(false), "supplementaryUnitsSpecified");
				NUnit.Framework.Assert.That(commodity.GoodsMeasure.grossMass, Is.EqualTo(1.123M), "grossMass");
				NUnit.Framework.Assert.That(commodity.GoodsMeasure.netMass, Is.EqualTo(2.234M), "netMass");
			});
		}

		[ExpectNoExceptions]
		public void TestTARICAdditionalCode_AllEmpty()
		{
			mockLine.Setup(m => m.TaricFirstAdditionalCode).Returns(ZString.Empty);
			mockLine.Setup(m => m.TaricSecondAdditionalCode).Returns(ZString.Empty);
			mockLine.Setup(m => m.TaricOtherAdditionalCodes).Returns((IReadOnlyCollection<ZString>)Enumerable.Empty<ZString>());

			var message = messageBuilder.GenerateMessage();
			var commodity = message.GoodsShipment.GoodsItem[0].Commodity;
			NUnit.Framework.Assert.That(commodity.CommodityCode.TARICAdditionalCode.Any(), Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestTARICAdditionalCode_ExistsNotEmpty()
		{
			mockLine.Setup(m => m.TaricFirstAdditionalCode).Returns(ZString.Empty);
			mockLine.Setup(m => m.TaricSecondAdditionalCode).Returns("4321");
			mockLine.Setup(m => m.TaricOtherAdditionalCodes).Returns((IReadOnlyCollection<ZString>)Enumerable.Empty<ZString>());

			var message = messageBuilder.GenerateMessage();
			var commodity = message.GoodsShipment.GoodsItem[0].Commodity;
			var taricCode = commodity.CommodityCode.TARICAdditionalCode.Single();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(taricCode.sequenceNumber, Is.EqualTo("1"));
				NUnit.Framework.Assert.That(taricCode.taricAdditionalCode, Is.EqualTo("4321"));
			});
		}

		[ExpectNoExceptions]
		public void TestTARICAdditionalCode_TaricOtherAdditionalCodesSpecifiedIsFalse()
		{
			CombineAssertions(() =>
			{
				mockLine.Setup(m => m.TaricOtherAdditionalCodesSpecified).Returns(false);
				var message = messageBuilder.GenerateMessage();
				var commodity = message.GoodsShipment.GoodsItem[0].Commodity;
				NUnit.Framework.Assert.That(commodity.CommodityCode.TARICAdditionalCode.Length, Is.EqualTo(2), "TARICAdditionalCode can only have up to 2 elements");
				NUnit.Framework.Assert.That(commodity.CommodityCode.TARICAdditionalCode[0].sequenceNumber, Is.EqualTo("1"), "TARICAdditionalCode[0].sequenceNumber");
				NUnit.Framework.Assert.That(commodity.CommodityCode.TARICAdditionalCode[0].taricAdditionalCode, Is.EqualTo("TF01"), "TARICAdditionalCode[0].taricAdditionalCode");
				NUnit.Framework.Assert.That(commodity.CommodityCode.TARICAdditionalCode[1].sequenceNumber, Is.EqualTo("2"), "TARICAdditionalCode[1].sequenceNumber");
				NUnit.Framework.Assert.That(commodity.CommodityCode.TARICAdditionalCode[1].taricAdditionalCode, Is.EqualTo("TF02"), "TARICAdditionalCode[1].taricAdditionalCode");
			});
		}

		[ExpectNoExceptions]
		public void TestDangerousGoods_Empty()
		{
			mockLine.Setup(m => m.DangerousGoodsCodes).Returns(Array.Empty<string>());

			var message = messageBuilder.GenerateMessage();
			var commodity = message.GoodsShipment.GoodsItem[0].Commodity;
			NUnit.Framework.Assert.That(commodity.DangerousGoods.Any(), Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestItem_Packaging()
		{
			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.GoodsItem[0].Packaging;
			var element1 = elements[0];
			var element2 = elements[1];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element1.sequenceNumber, Is.EqualTo("1"), "element1 sequenceNumber");
				NUnit.Framework.Assert.That(element1.typeOfPackages, Is.EqualTo("1A"), "element1 typeOfPackages");
				NUnit.Framework.Assert.That(element1.numberOfPackages, Is.EqualTo("2"), "element1 numberOfPackages");
				NUnit.Framework.Assert.That(element1.shippingMarks, Is.EqualTo("1234567890"), "element1 shippingMarks");
				NUnit.Framework.Assert.That(element1.PackageReference, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentGoodsItemPackagingPackageReference)), "element1 PackageReference - should be [null]");

				NUnit.Framework.Assert.That(element2.sequenceNumber, Is.EqualTo("2"), "element2 sequenceNumber");
				NUnit.Framework.Assert.That(element2.typeOfPackages, Is.EqualTo("2C"), "element2 troleype");
				NUnit.Framework.Assert.That(element2.numberOfPackages, Is.EqualTo(default(string)), "element2 numberOfPackages - should be [null]");
				NUnit.Framework.Assert.That(element2.shippingMarks, Is.EqualTo("1234567891"), "element2 shippingMarks");
				NUnit.Framework.Assert.That(element2.PackageReference.declarationGoodsItemNumber, Is.EqualTo("2"), "element2 PackageReference");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Packaging_Abnormal()
		{
			var packages = new[]
			{
				MockPackage(2, GetLongString("K", PackageKindMaxLength2 + 1), GetLongString("N", PackageMarksNumbersMaxLength + 1), 0, true).Object,
			};
			mockLine.Setup(m => m.Packages).Returns(packages);

			var message = messageBuilder.GenerateMessage();
			var element = message.GoodsShipment.GoodsItem[0].Packaging[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element.typeOfPackages, Is.EqualTo(GetLongString("K", PackageKindMaxLength2)), "typeOfPackages");
				NUnit.Framework.Assert.That(element.shippingMarks, Is.EqualTo(GetLongString("N", PackageMarksNumbersMaxLength)), "shippingMarks");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Packaging_MultiplePackagesSameShippingMarks()
		{
			mockLine.Setup(l => l.Packages).Returns(new[]
			{
				MockPackage(2, "1A", "1234567890", 0, true).Object,
				MockPackage(5, "2C", "1234567891", 2, true).Object,
				MockPackage(2, "2C", "1234567891", 2, true).Object,
				MockPackage(1, "2C", "1234567891", 2, true).Object,
			});

			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.GoodsItem[0].Packaging;

			var element1 = elements[0];
			var element2 = elements[1];

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(elements.Length, Is.EqualTo(2), "Number of mapped packages");

				NUnit.Framework.Assert.That(element1.sequenceNumber, Is.EqualTo("1"), "element1 sequenceNumber");
				NUnit.Framework.Assert.That(element1.typeOfPackages, Is.EqualTo("1A"), "element1 typeOfPackages");
				NUnit.Framework.Assert.That(element1.numberOfPackages, Is.EqualTo("2"), "element1 numberOfPackages");
				NUnit.Framework.Assert.That(element1.shippingMarks, Is.EqualTo("1234567890"), "element1 shippingMarks");
				NUnit.Framework.Assert.That(element1.PackageReference, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentGoodsItemPackagingPackageReference)), "element1 PackageReference - should be [null]");

				NUnit.Framework.Assert.That(element2.sequenceNumber, Is.EqualTo("2"), "element2 sequenceNumber");
				NUnit.Framework.Assert.That(element2.typeOfPackages, Is.EqualTo("2C"), "element2 typeOfPackages");
				NUnit.Framework.Assert.That(element2.numberOfPackages, Is.EqualTo("8"), "element2 numberOfPackages");
				NUnit.Framework.Assert.That(element2.shippingMarks, Is.EqualTo("1234567891"), "element2 shippingMarks");
				NUnit.Framework.Assert.That(element2.PackageReference, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentGoodsItemPackagingPackageReference)), "element2 PackageReference - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Packaging_MultiplePackagesSameShippingMarks_PackagesWithZeroQuantity()
		{
			mockLine.Setup(l => l.Packages).Returns(new[]
			{
				MockPackage(2, "1A", "1234567890", 0, true).Object,
				MockPackage(5, "2C", "1234567891", 2, true).Object,
				MockPackage(0, "2C", "1234567891", 2, true).Object,
				MockPackage(0, "2C", "1234567891", 2, true).Object,
			});

			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.GoodsItem[0].Packaging;

			var element1 = elements[0];
			var element2 = elements[1];

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(elements.Length, Is.EqualTo(2), "Number of mapped packages");

				NUnit.Framework.Assert.That(element1.sequenceNumber, Is.EqualTo("1"), "element1 sequenceNumber");
				NUnit.Framework.Assert.That(element1.typeOfPackages, Is.EqualTo("1A"), "element1 typeOfPackages");
				NUnit.Framework.Assert.That(element1.numberOfPackages, Is.EqualTo("2"), "element1 numberOfPackages");
				NUnit.Framework.Assert.That(element1.shippingMarks, Is.EqualTo("1234567890"), "element1 shippingMarks");
				NUnit.Framework.Assert.That(element1.PackageReference, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentGoodsItemPackagingPackageReference)), "element1 PackageReference - should be [null]");

				NUnit.Framework.Assert.That(element2.sequenceNumber, Is.EqualTo("2"), "element2 sequenceNumber");
				NUnit.Framework.Assert.That(element2.typeOfPackages, Is.EqualTo("2C"), "element2 typeOfPackages");
				NUnit.Framework.Assert.That(element2.numberOfPackages, Is.EqualTo("5"), "element2 numberOfPackages");
				NUnit.Framework.Assert.That(element2.shippingMarks, Is.EqualTo("1234567891"), "element2 shippingMarks");
				NUnit.Framework.Assert.That(element2.PackageReference, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentGoodsItemPackagingPackageReference)), "element2 PackageReference - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Packaging_MultiplePackagesSameShippingMarksAndType()
		{
			mockLine.Setup(l => l.Packages).Returns(new[]
			{
				MockPackage(2, "1A", "1234567891", 0, true).Object,
				MockPackage(5, "2C", "1234567891", 2, true).Object,
				MockPackage(0, "2C", "1234567891", 2, true).Object,
				MockPackage(0, "2C", "1234567891", 2, true).Object,
			});

			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.GoodsItem[0].Packaging;

			var element1 = elements[0];
			var element2 = elements[1];

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(elements.Length, Is.EqualTo(2), "Number of mapped packages");

				NUnit.Framework.Assert.That(element1.sequenceNumber, Is.EqualTo("1"), "element1 sequenceNumber");
				NUnit.Framework.Assert.That(element1.typeOfPackages, Is.EqualTo("1A"), "element1 typeOfPackages");
				NUnit.Framework.Assert.That(element1.numberOfPackages, Is.EqualTo("2"), "element1 numberOfPackages");
				NUnit.Framework.Assert.That(element1.shippingMarks, Is.EqualTo("1234567891"), "element1 shippingMarks");
				NUnit.Framework.Assert.That(element1.PackageReference, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentGoodsItemPackagingPackageReference)), "element1 PackageReference - should be [null]");

				NUnit.Framework.Assert.That(element2.sequenceNumber, Is.EqualTo("2"), "element2 sequenceNumber");
				NUnit.Framework.Assert.That(element2.typeOfPackages, Is.EqualTo("2C"), "element2 typeOfPackages");
				NUnit.Framework.Assert.That(element2.numberOfPackages, Is.EqualTo("5"), "element2 numberOfPackages");
				NUnit.Framework.Assert.That(element2.shippingMarks, Is.EqualTo("1234567891"), "element2 shippingMarks");
				NUnit.Framework.Assert.That(element2.PackageReference, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentGoodsItemPackagingPackageReference)), "element2 PackageReference - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_PreviousDocument()
		{
			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.GoodsItem[0].PreviousDocument;
			var element1 = elements[0];
			var element2 = elements[1];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element1.sequenceNumber, Is.EqualTo("1"), "element1 sequenceNumber");
				NUnit.Framework.Assert.That(element1.type, Is.EqualTo("1234"), "element1 type");
				NUnit.Framework.Assert.That(element1.qualifier, Is.EqualTo("567"), "element1 qualifier");
				NUnit.Framework.Assert.That(element1.referenceNumber, Is.EqualTo("Reference1"), "element1 referenceNumber");
				NUnit.Framework.Assert.That(element1.goodsItemNumber, Is.EqualTo("1"), "element1 goodsItemNumber");
				NUnit.Framework.Assert.That(element1.measurementUnitAndQualifier, Is.EqualTo("UN1"), "element1 measurementUnitAndQualifier");
				NUnit.Framework.Assert.That(element1.quantity, Is.EqualTo(1.12M), "element1 quantity");
				NUnit.Framework.Assert.That(element1.quantitySpecified, Is.EqualTo(true), "element1 quantitySpecified");
				NUnit.Framework.Assert.That(element1.complementOfInformation, Is.EqualTo("Complement1"), "element1 complementOfInformation");

				NUnit.Framework.Assert.That(element2.sequenceNumber, Is.EqualTo("2"), "element2 sequenceNumber");
				NUnit.Framework.Assert.That(element2.type, Is.EqualTo("2345"), "element2 type");
				NUnit.Framework.Assert.That(element2.qualifier, Is.EqualTo("678"), "element2 qualifier");
				NUnit.Framework.Assert.That(element2.referenceNumber, Is.EqualTo("Reference2"), "element2 referenceNumber");
				NUnit.Framework.Assert.That(element2.goodsItemNumber, Is.EqualTo("2"), "element2 goodsItemNumber");
				NUnit.Framework.Assert.That(element2.measurementUnitAndQualifier, Is.EqualTo("UN2"), "element2 measurementUnitAndQualifier");
				NUnit.Framework.Assert.That(element2.quantity, Is.EqualTo(2.34M), "element2 quantity");
				NUnit.Framework.Assert.That(element2.quantitySpecified, Is.EqualTo(true), "element2 quantitySpecified");
				NUnit.Framework.Assert.That(element2.complementOfInformation, Is.EqualTo("Complement2"), "element2 complementOfInformation");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_PreviousDocument_Abnormal()
		{
			var previousDocuments = new[] { MockPreviousDocument(string.Empty
							, GetLongString("R", DocumentReferenceNumberMaxLength + 1)
							, GetLongString("C", DocumentComplementMaxLength + 1)
							, GetLongString("T", DocumentTypeMaxLength + 1)
							, GetLongString("Q", DocumentQualifierMaxLength + 1)
							, 0
							, GetLongString("U", MeasurementUnitAndQualifierMaxLength + 1)
							, 0
							).Object };
			mockLine.Setup(m => m.PreviousDocuments).Returns(previousDocuments);

			var message = messageBuilder.GenerateMessage();
			var element = message.GoodsShipment.GoodsItem[0].PreviousDocument[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element.sequenceNumber, Is.EqualTo("1"), "sequenceNumber");
				NUnit.Framework.Assert.That(element.type, Is.EqualTo(GetLongString("T", DocumentTypeMaxLength)), "type");
				NUnit.Framework.Assert.That(element.qualifier, Is.EqualTo(GetLongString("Q", DocumentQualifierMaxLength)), "qualifier");
				NUnit.Framework.Assert.That(element.referenceNumber, Is.EqualTo(GetLongString("R", DocumentReferenceNumberMaxLength)), "referenceNumber");
				NUnit.Framework.Assert.That(element.goodsItemNumber, Is.EqualTo(default(string)), "goodsItemNumber - should be [null]");
				NUnit.Framework.Assert.That(element.measurementUnitAndQualifier, Is.EqualTo(GetLongString("U", MeasurementUnitAndQualifierMaxLength)), "measurementUnitAndQualifier");
				NUnit.Framework.Assert.That(element.complementOfInformation, Is.EqualTo(GetLongString("C", DocumentComplementMaxLength)), "complementOfInformation");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_PreviousDocument_MeasurementUnitAndQualifierEmpty()
		{
			var previousDocuments = new[] { MockPreviousDocument(string.Empty
				, GetLongString("R", DocumentReferenceNumberMaxLength + 1)
				, GetLongString("C", DocumentComplementMaxLength + 1)
				, GetLongString("T", DocumentTypeMaxLength + 1)
				, GetLongString("Q", DocumentQualifierMaxLength + 1)
				, 0
				, string.Empty
				, 0
			).Object };
			mockLine.Setup(m => m.PreviousDocuments).Returns(previousDocuments);

			var message = messageBuilder.GenerateMessage();
			var element = message.GoodsShipment.GoodsItem[0].PreviousDocument[0];
			NUnit.Framework.Assert.That(element.measurementUnitAndQualifier, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestItem_PreviousDocument_QuantitySpecified()
		{
			mockLine.Setup(l => l.PreviousDocuments).Returns(new[]
			{
				MockPreviousDocument(string.Empty, "Reference1", "Complement1", "1234", "567", 1, "UN1", 0).Object,
				MockPreviousDocument(string.Empty, "Reference2", "Complement2", "1234", "567", 1, "", 1).Object
			});

			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.GoodsItem[0].PreviousDocument;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(elements[0].quantitySpecified, Is.EqualTo(false), "Quantity = 0");
				NUnit.Framework.Assert.That(elements[1].quantitySpecified, Is.EqualTo(false), "MeasurementUnitAndQualifier is empty");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_SupportingDocument()
		{
			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.GoodsItem[0].SupportingDocument;
			var element1 = elements[0];
			var element2 = elements[1];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element1.sequenceNumber, Is.EqualTo("1"), "element1 sequenceNumber");
				NUnit.Framework.Assert.That(element1.type, Is.EqualTo("Typ1"), "element1 type");
				NUnit.Framework.Assert.That(element1.qualifier, Is.EqualTo("Qu1"), "element1 qualifier");
				NUnit.Framework.Assert.That(element1.referenceNumber, Is.EqualTo("Reference1"), "element1 referenceNumber");
				NUnit.Framework.Assert.That(element1.documentLineItemNumber, Is.EqualTo("1"), "element1 documentLineItemNumber");
				NUnit.Framework.Assert.That(element1.complementOfInformation, Is.EqualTo("Complement1"), "element1 complementOfInformation");
				NUnit.Framework.Assert.That(element1.detail, Is.EqualTo("Detail1"), "element1 detail");
				NUnit.Framework.Assert.That(element1.issuingAuthorityName, Is.EqualTo("Name1"), "element1 issuingAuthorityName");
				NUnit.Framework.Assert.That(element1.issuingDate, Is.EqualTo(new DateTime(2021, 01, 01)), "element1 issuingDate");
				NUnit.Framework.Assert.That(element1.issuingDateSpecified, Is.EqualTo(true), "element1 issuingDateSpecified");
				NUnit.Framework.Assert.That(element1.validityDate, Is.EqualTo(new DateTime(2021, 12, 31)), "element1 validityDate");
				NUnit.Framework.Assert.That(element1.validityDateSpecified, Is.EqualTo(true), "element1 validityDateSpecified");
				NUnit.Framework.Assert.That(element1.measurementUnitAndQualifier, Is.EqualTo("MUQ1"), "element1 measurementUnitAndQualifier");
				NUnit.Framework.Assert.That(element1.complementaryUnit.ToString(), Is.EqualTo("kg"), "element1 complementaryUnit");
				NUnit.Framework.Assert.That(element1.complementaryUnitSpecified, Is.EqualTo(true), "element1 complementaryUnitSpecified");
				NUnit.Framework.Assert.That(element1.quantity, Is.EqualTo(8M), "element1 quantity");
				NUnit.Framework.Assert.That(element1.quantitySpecified, Is.EqualTo(true), "element1 quantitySpecified");
				NUnit.Framework.Assert.That(element1.currency, Is.EqualTo("CU1"), "element1 currency");
				NUnit.Framework.Assert.That(element1.amount, Is.EqualTo(1.12M), "element1 amount");
				NUnit.Framework.Assert.That(element1.amountSpecified, Is.EqualTo(true), "element1 amountSpecified");

				NUnit.Framework.Assert.That(element2.sequenceNumber, Is.EqualTo("2"), "element2 sequenceNumber");
				NUnit.Framework.Assert.That(element2.type, Is.EqualTo("Typ2"), "element2 type");
				NUnit.Framework.Assert.That(element2.qualifier, Is.EqualTo("Qu2"), "element2 qualifier");
				NUnit.Framework.Assert.That(element2.referenceNumber, Is.EqualTo("Reference2"), "element2 referenceNumber");
				NUnit.Framework.Assert.That(element2.documentLineItemNumber, Is.EqualTo("2"), "element2 documentLineItemNumber");
				NUnit.Framework.Assert.That(element2.complementOfInformation, Is.EqualTo("Complement2"), "element2 complementOfInformation");
				NUnit.Framework.Assert.That(element2.detail, Is.EqualTo("Detail2"), "element2 detail");
				NUnit.Framework.Assert.That(element2.issuingAuthorityName, Is.EqualTo("Name2"), "element2 issuingAuthorityName");
				NUnit.Framework.Assert.That(element2.issuingDate, Is.EqualTo(new DateTime(2022, 01, 01)), "element2 issuingDate");
				NUnit.Framework.Assert.That(element2.issuingDateSpecified, Is.EqualTo(true), "element2 issuingDateSpecified");
				NUnit.Framework.Assert.That(element2.validityDate, Is.EqualTo(new DateTime(2022, 12, 31)), "element2 validityDate");
				NUnit.Framework.Assert.That(element2.validityDateSpecified, Is.EqualTo(true), "element2 validityDateSpecified");
				NUnit.Framework.Assert.That(element2.measurementUnitAndQualifier, Is.EqualTo("MUQ2"), "element2 measurementUnitAndQualifier");
				NUnit.Framework.Assert.That(element2.complementaryUnit.ToString(), Is.EqualTo("km"), "element2 complementaryUnit");
				NUnit.Framework.Assert.That(element2.complementaryUnitSpecified, Is.EqualTo(true), "element2 complementaryUnitSpecified");
				NUnit.Framework.Assert.That(element2.quantity, Is.EqualTo(10M), "element2 quantity");
				NUnit.Framework.Assert.That(element2.quantitySpecified, Is.EqualTo(true), "element2 quantitySpecified");
				NUnit.Framework.Assert.That(element2.currency, Is.EqualTo("CU2"), "element2 currency");
				NUnit.Framework.Assert.That(element2.amount, Is.EqualTo(2.34M), "element2 amount");
				NUnit.Framework.Assert.That(element2.amountSpecified, Is.EqualTo(true), "element2 amountSpecified");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_SupportingDocument_Abnormal()
		{
			var supportingDocuments = new[] { MockDocument(GetLongString("Q", DocumentQualifierMaxLength + 1)
							, GetLongString("T", DocumentTypeMaxLength + 1)
							, GetLongString("R", DocumentReferenceNumberMaxLength + 1)
							, GetLongString("C", DocumentComplementMaxLength + 1)
							, GetLongString("D", DocumentDetailMaxLength + 1)
							, null
							, null
							, 0M
							, "!@#"
							, 0M
							, 0
							, GetLongString("N", DocumentAdditionalDescriptionMaxLength + 1)
							, GetLongString("U", MeasurementUnitAndQualifierMaxLength + 1)
							, GetLongString("Y", CurrencyMaxLength + 1)
							).Object };
			mockLine.Setup(m => m.Documents).Returns(supportingDocuments);

			var message = messageBuilder.GenerateMessage();
			var element = message.GoodsShipment.GoodsItem[0].SupportingDocument[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element.sequenceNumber, Is.EqualTo("1"), "sequenceNumber");
				NUnit.Framework.Assert.That(element.type, Is.EqualTo(GetLongString("T", DocumentTypeMaxLength)), "type");
				NUnit.Framework.Assert.That(element.qualifier, Is.EqualTo(GetLongString("Q", DocumentQualifierMaxLength)), "qualifier");
				NUnit.Framework.Assert.That(element.referenceNumber, Is.EqualTo(GetLongString("R", DocumentReferenceNumberMaxLength)), "referenceNumber");
				NUnit.Framework.Assert.That(element.documentLineItemNumber, Is.EqualTo(default(string)), "documentLineItemNumber - should be [null]");
				NUnit.Framework.Assert.That(element.complementOfInformation, Is.EqualTo(GetLongString("C", DocumentComplementMaxLength)), "complementOfInformation");
				NUnit.Framework.Assert.That(element.detail, Is.EqualTo(GetLongString("D", DocumentDetailMaxLength)), "detail");
				NUnit.Framework.Assert.That(element.issuingAuthorityName, Is.EqualTo(GetLongString("N", DocumentAdditionalDescriptionMaxLength)), "issuingAuthorityName");
				NUnit.Framework.Assert.That(element.issuingDateSpecified, Is.EqualTo(false), "issuingDateSpecified");
				NUnit.Framework.Assert.That(element.validityDateSpecified, Is.EqualTo(false), "validityDateSpecified");
				NUnit.Framework.Assert.That(element.measurementUnitAndQualifier, Is.EqualTo(GetLongString("U", MeasurementUnitAndQualifierMaxLength)), "measurementUnitAndQualifier");
				NUnit.Framework.Assert.That(element.complementaryUnitSpecified, Is.EqualTo(false), "complementaryUnitSpecified");
				NUnit.Framework.Assert.That(element.currency, Is.EqualTo(GetLongString("Y", CurrencyMaxLength)), "currency");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_SupportingDocument_MeasurementUnitAndQualifierEmpty()
		{
			var supportingDocuments = new[] { MockDocument(GetLongString("Q", DocumentQualifierMaxLength + 1)
				, GetLongString("T", DocumentTypeMaxLength + 1)
				, GetLongString("R", DocumentReferenceNumberMaxLength + 1)
				, GetLongString("C", DocumentComplementMaxLength + 1)
				, GetLongString("D", DocumentDetailMaxLength + 1)
				, default(DateTime)
				, default(DateTime)
				, 0M
				, "!@#"
				, 0M
				, 0
				, GetLongString("N", DocumentAdditionalDescriptionMaxLength + 1)
				, string.Empty
				, GetLongString("Y", CurrencyMaxLength + 1)
			).Object };
			mockLine.Setup(m => m.Documents).Returns(supportingDocuments);

			var message = messageBuilder.GenerateMessage();
			var element = message.GoodsShipment.GoodsItem[0].SupportingDocument[0];
			NUnit.Framework.Assert.That(element.measurementUnitAndQualifier, Is.EqualTo(default(string)));
		}

		[ExpectNoExceptions]
		public void TestItem_SupportingDocument_QuantitySpecified()
		{
			mockLine.Setup(l => l.Documents).Returns(new[]
			{
				MockDocument("Qu1", "Typ1", "Reference1", "Complement1", "Detail1", new DateTime(2022, 01, 01), new DateTime(2022, 12, 31),
					2.34M, "km", 0, 2, "Name1", "MUQ1", "AUD").Object,
				MockDocument("Qu2", "Typ2", "Reference2", "Complement2", "Detail2", new DateTime(2022, 01, 01), new DateTime(2022, 12, 31),
					2.34M, "div", 10, 2, "Name2", "KG", "AUD").Object,
				MockDocument("Qu3", "Typ3", "Reference3", "Complement3", "Detail3", new DateTime(2022, 01, 01), new DateTime(2022, 12, 31),
					2.34M, "div", 10, 2, "Name3", "", "AUD").Object,
				MockDocument("Qu4", "Typ4", "Reference4", "Complement4", "Detail4", new DateTime(2022, 01, 01), new DateTime(2022, 12, 31),
				2.34M, "ltAnlage", 10, 2, "Name4", "", "AUD").Object,
				MockDocument("Qu5", "Typ5", "Reference5", "Complement5", "Detail5", new DateTime(2022, 01, 01), new DateTime(2022, 12, 31),
					2.34M, "Mio", 10, 2, "Name5", "", "AUD").Object
			});

			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.GoodsItem[0].SupportingDocument;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(elements[0].quantitySpecified, Is.EqualTo(false), "Quantity = 0");
				NUnit.Framework.Assert.That(elements[1].quantitySpecified, Is.EqualTo(true), "MeasurementUnitAndQualifier <> ''");
				NUnit.Framework.Assert.That(elements[2].quantitySpecified, Is.EqualTo(false), "ComplementaryUnit = 'div'");
				NUnit.Framework.Assert.That(elements[3].quantitySpecified, Is.EqualTo(false), "ComplementaryUnit = 'ltAnlage'");
				NUnit.Framework.Assert.That(elements[4].quantitySpecified, Is.EqualTo(true), "ComplementaryUnit <> ('div', 'ltAnlage')");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_SupportingDocument_AmountSpecified()
		{
			mockLine.Setup(l => l.Documents).Returns(new[]
			{
				MockDocument("Qu1", "Typ1", "Reference1", "Complement1", "Detail1", new DateTime(2022, 01, 01), new DateTime(2022, 12, 31),
					0, "km", 10, 2, "Name1", "MUQ1", "AUD").Object,
				MockDocument("Qu2", "Typ2", "Reference2", "Complement2", "Detail2", new DateTime(2022, 01, 01), new DateTime(2022, 12, 31),
					2.34M, "km", 10, 2, "Name2", "MUQ2", "").Object
			});

			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.GoodsItem[0].SupportingDocument;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(elements[0].amount, Is.EqualTo(0M), "amount is 0 if Amount = 0");
				NUnit.Framework.Assert.That(elements[0].amountSpecified, Is.EqualTo(true), "amountSpecified is true if Amount = 0 and Currency isn't empty");

				NUnit.Framework.Assert.That(elements[1].amountSpecified, Is.EqualTo(false), "Currency is empty");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_AdditionalReference()
		{
			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.GoodsItem[0].AdditionalReference;
			var element1 = elements[0];
			var element2 = elements[1];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element1.sequenceNumber, Is.EqualTo("1"), "element1 sequenceNumber");
				NUnit.Framework.Assert.That(element1.type, Is.EqualTo("Tpr1"), "element1 type");
				NUnit.Framework.Assert.That(element1.qualifier, Is.EqualTo("Qu1"), "element1 qualifier");
				NUnit.Framework.Assert.That(element1.referenceNumber, Is.EqualTo("Reference1"), "element1 referenceNumber");
				NUnit.Framework.Assert.That(element1.detail, Is.EqualTo("Detail1"), "element1 detail");
				NUnit.Framework.Assert.That(element1.currency, Is.EqualTo("CU1"), "element1 currency");
				NUnit.Framework.Assert.That(element1.amount, Is.EqualTo(1.12M), "element1 amount");
				NUnit.Framework.Assert.That(element1.amountSpecified, Is.EqualTo(true), "element1 amountSpecified");

				NUnit.Framework.Assert.That(element2.sequenceNumber, Is.EqualTo("2"), "element2 sequenceNumber");
				NUnit.Framework.Assert.That(element2.type, Is.EqualTo("Tpr2"), "element2 type");
				NUnit.Framework.Assert.That(element2.qualifier, Is.EqualTo("Qu2"), "element2 qualifier");
				NUnit.Framework.Assert.That(element2.referenceNumber, Is.EqualTo("Reference2"), "element2 referenceNumber");
				NUnit.Framework.Assert.That(element2.detail, Is.EqualTo("Detail2"), "element2 detail");
				NUnit.Framework.Assert.That(element2.currency, Is.EqualTo("CU2"), "element2 currency");
				NUnit.Framework.Assert.That(element2.amount, Is.EqualTo(2.34M), "element2 amount");
				NUnit.Framework.Assert.That(element2.amountSpecified, Is.EqualTo(true), "element2 amountSpecified");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_AdditionalReference_Abnormal()
		{
			var additionalReferences = new[]
			{
				MockAdditionalInfo(type: GetLongString("T", DocumentTypeMaxLength + 1)
				, qualifier: GetLongString("Q", DocumentQualifierMaxLength + 1)
				, referenceNumber: GetLongString("R", DocumentReferenceNumberMaxLength + 1)
				, detail: GetLongString("D", DocumentDetailMaxLength + 1)
				, currency: GetLongString("Y", CurrencyMaxLength + 1)
				, amount: 0
				).Object,
			};
			mockLine.Setup(m => m.AdditionalReferences).Returns(additionalReferences);

			var message = messageBuilder.GenerateMessage();
			var element = message.GoodsShipment.GoodsItem[0].AdditionalReference[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element.sequenceNumber, Is.EqualTo("1"), "sequenceNumber");
				NUnit.Framework.Assert.That(element.type, Is.EqualTo(GetLongString("T", DocumentTypeMaxLength)), "type");
				NUnit.Framework.Assert.That(element.qualifier, Is.EqualTo(GetLongString("Q", DocumentQualifierMaxLength)), "qualifier");
				NUnit.Framework.Assert.That(element.referenceNumber, Is.EqualTo(GetLongString("R", DocumentReferenceNumberMaxLength)), "referenceNumber");
				NUnit.Framework.Assert.That(element.detail, Is.EqualTo(GetLongString("D", DocumentDetailMaxLength)), "detail");
				NUnit.Framework.Assert.That(element.currency, Is.EqualTo(GetLongString("Y", CurrencyMaxLength)), "currency");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_AdditionalReference_AmountSpecified()
		{
			var additionalReferences = new[]
			{
				MockAdditionalInfo(string.Empty, "Tpr1", "Qu1", "Reference1", "Complement1", "Detail1", "CU1", 0M).Object,
				MockAdditionalInfo(string.Empty, "Tpr2", "Qu2", "Reference2", "Complement2", "Detail2", string.Empty, 2.34M).Object
			};
			mockLine.Setup(m => m.AdditionalReferences).Returns(additionalReferences);

			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.GoodsItem[0].AdditionalReference;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(elements[0].amount, Is.EqualTo(0M), "amount is 0 if Amount = 0");
				NUnit.Framework.Assert.That(elements[0].amountSpecified, Is.EqualTo(true), "amountSpecified is true if Amount = 0 and Currency isn't empty");
				NUnit.Framework.Assert.That(elements[1].amountSpecified, Is.EqualTo(false), "Currency is empty");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_AdditionalInformation()
		{
			var message = messageBuilder.GenerateMessage();
			var elements = message.GoodsShipment.GoodsItem[0].AdditionalInformation;
			var element1 = elements[0];
			var element2 = elements[1];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element1.sequenceNumber, Is.EqualTo("1"), "element1 sequenceNumber");
				NUnit.Framework.Assert.That(element1.code, Is.EqualTo("Ftpi1"), "element1 code");
				NUnit.Framework.Assert.That(element1.text, Is.EqualTo("Complement1"), "element1 text");

				NUnit.Framework.Assert.That(element2.sequenceNumber, Is.EqualTo("2"), "element2 sequenceNumber");
				NUnit.Framework.Assert.That(element2.code, Is.EqualTo("Ftpi2"), "element2 code");
				NUnit.Framework.Assert.That(element2.text, Is.EqualTo("Complement2"), "element2 text");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_AdditionalInformation_Abnormal()
		{
			var additionalInformations = new[]
			{
				MockAdditionalInfo(fullType: GetLongString("T", AdditionalInformationCodeMaxLength + 1)
				, complement: GetLongString("C", AdditionalInformationTextMaxLength + 1)
				).Object,
			};
			mockLine.Setup(m => m.AdditionalInformations).Returns(additionalInformations);

			var message = messageBuilder.GenerateMessage();
			var element = message.GoodsShipment.GoodsItem[0].AdditionalInformation[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(element.sequenceNumber, Is.EqualTo("1"), "sequenceNumber");
				NUnit.Framework.Assert.That(element.code, Is.EqualTo(GetLongString("T", AdditionalInformationCodeMaxLength)), "code");
				NUnit.Framework.Assert.That(element.text, Is.EqualTo(GetLongString("C", AdditionalInformationTextMaxLength)), "text");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_TransportCharges()
		{
			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem[0].TransportCharges.methodOfPayment, Is.EqualTo("A"));
		}

		[ExpectNoExceptions]
		public void TestItem_TransportCharges_Abnormal()
		{
			mockLine.Setup(m => m.TransportChargesPaymentMethod).Returns(GetLongString("P", TransportChargesPaymentMethodMaxLength + 1));

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem[0].TransportCharges.methodOfPayment, Is.EqualTo(GetLongString("P", TransportChargesPaymentMethodMaxLength)));
		}

		[ExpectNoExceptions]
		public void TestItem_OutwardProcessing()
		{
			var message = messageBuilder.GenerateMessage();
			var outwardProcessing = message.GoodsShipment.GoodsItem[0].OutwardProcessing;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(outwardProcessing.replacement.ToString(), Is.EqualTo("Item1"), "replacement");
				NUnit.Framework.Assert.That(outwardProcessing.reimportDate, Is.EqualTo(new DateTime(2021, 08, 23)), "reimportDate");
				NUnit.Framework.Assert.That(outwardProcessing.reimportDateSpecified, Is.EqualTo(true), "reimportDateSpecified");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_OutwardProcessing_Abnormal()
		{
			mockLine.Setup(m => m.OutwardProcessingReplacement).Returns("!@#");
			mockLine.Setup(m => m.OutwardProcessingReimportDate).Returns(default(DateTime));

			var message = messageBuilder.GenerateMessage();
			var outwardProcessing = message.GoodsShipment.GoodsItem[0].OutwardProcessing;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(outwardProcessing.replacement.ToString(), Is.EqualTo("Item0"), "replacement");
				NUnit.Framework.Assert.That(outwardProcessing.reimportDateSpecified, Is.EqualTo(false), "reimportDateSpecified");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_OutwardProcessing_OutwardProcessingReplacementIsEmpty()
		{
			mockLine.Setup(m => m.OutwardProcessingReplacement).Returns(string.Empty);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem[0].OutwardProcessing, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentGoodsItemOutwardProcessing)));
		}

		[ExpectNoExceptions]
		public void TestItem_ProcedureTransference_CustomsWarehousing()
		{
			var message = messageBuilder.GenerateMessage();
			var customsWarehousing = message.GoodsShipment.GoodsItem[0].ProcedureTransference.CustomsWarehousing;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(customsWarehousing.LRN, Is.EqualTo("WLRN001"), "LRN");
				NUnit.Framework.Assert.That(customsWarehousing.Authorisation.type.ToString(), Is.EqualTo("C518"), "type");
				NUnit.Framework.Assert.That(customsWarehousing.Authorisation.referenceNumber, Is.EqualTo("CWA0001"), "referenceNumber");

				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].sequenceNumber, Is.EqualTo("1"), "GoodsReference[0].sequenceNumber");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].accessViaATLAS.ToString(), Is.EqualTo("Item0"), "GoodsReference[0].accessViaATLAS");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].MRN, Is.EqualTo("MRN001"), "GoodsReference[0].MRN");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].registrationNumber, Is.EqualTo(default(string)), "GoodsReference[0].registrationNumber");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].goodsItemNumber, Is.EqualTo("1"), "GoodsReference[0].goodsItemNumber");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].usualTreatment.ToString(), Is.EqualTo("Item0"), "GoodsReference[0].usualTreatment");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].complementOfInformation, Is.EqualTo("Complement1"), "GoodsReference[0].complementOfInformation");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].Commodity.CommodityCode.harmonizedSystemSubHeadingCode, Is.EqualTo("HSSH01"), "GoodsReference[0].harmonizedSystemSubHeadingCode");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].Commodity.CommodityCode.combinedNomenclatureCode, Is.EqualTo("C1"), "GoodsReference[0].combinedNomenclatureCode");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].Commodity.CommodityCode.taricCode, Is.EqualTo("T1"), "GoodsReference[0].taricCode");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].Commodity.CommodityCode.NationalAdditionalCode.nationalAdditionalCode, Is.EqualTo("A"), "GoodsReference[0].nationalAdditionalCode");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].Commodity.GoodsReduction.measurementUnit, Is.EqualTo("042"), "GoodsReference[0].GoodsReduction.measurementUnit");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].Commodity.GoodsReduction.qualifier, Is.EqualTo("A"), "GoodsReference[0].GoodsReduction.qualifier");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].Commodity.GoodsReduction.quantity, Is.EqualTo(1.12M), "GoodsReference[0].GoodsReduction.quantity");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].Commodity.GoodsReductionAfterTreatment.measurementUnit, Is.EqualTo("204"), "GoodsReference[0].GoodsReductionAfterTreatment.measurementUnit");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].Commodity.GoodsReductionAfterTreatment.qualifier, Is.EqualTo("B"), "GoodsReference[0].GoodsReductionAfterTreatment.qualifier");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].Commodity.GoodsReductionAfterTreatment.quantity, Is.EqualTo(2.23M), "GoodsReference[0].GoodsReductionAfterTreatment.quantity");

				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[1].sequenceNumber, Is.EqualTo("2"), "GoodsReference[1].sequenceNumber");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[1].accessViaATLAS.ToString(), Is.EqualTo("Item1"), "GoodsReference[1].accessViaATLAS");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[1].MRN, Is.EqualTo("MRN002"), "GoodsReference[1].MRN");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[1].registrationNumber, Is.EqualTo(default(string)), "GoodsReference[1].registrationNumber");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[1].goodsItemNumber, Is.EqualTo("2"), "GoodsReference[1].goodsItemNumber");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[1].usualTreatment.ToString(), Is.EqualTo("Item1"), "GoodsReference[1].usualTreatment");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[1].complementOfInformation, Is.EqualTo("Complement2"), "GoodsReference[1].complementOfInformation");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[1].Commodity.CommodityCode.harmonizedSystemSubHeadingCode, Is.EqualTo("HSSH02"), "GoodsReference[1].harmonizedSystemSubHeadingCode");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[1].Commodity.CommodityCode.combinedNomenclatureCode, Is.EqualTo("C2"), "GoodsReference[1].combinedNomenclatureCode");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[1].Commodity.CommodityCode.taricCode, Is.EqualTo("T2"), "GoodsReference[1].taricCode");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[1].Commodity.CommodityCode.NationalAdditionalCode.nationalAdditionalCode, Is.EqualTo("B"), "GoodsReference[1].nationalAdditionalCode");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[1].Commodity.GoodsReduction.measurementUnit, Is.EqualTo("019"), "GoodsReference[1].GoodsReduction.measurementUnit");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[1].Commodity.GoodsReduction.qualifier, Is.EqualTo("C"), "GoodsReference[1].GoodsReduction.qualifier");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[1].Commodity.GoodsReduction.quantity, Is.EqualTo(3.34M), "GoodsReference[1].GoodsReduction.quantity");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[1].Commodity.GoodsReductionAfterTreatment.measurementUnit, Is.EqualTo("202"), "GoodsReference[1].GoodsReductionAfterTreatment.measurementUnit");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[1].Commodity.GoodsReductionAfterTreatment.qualifier, Is.EqualTo("D"), "GoodsReference[1].GoodsReductionAfterTreatment.qualifier");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[1].Commodity.GoodsReductionAfterTreatment.quantity, Is.EqualTo(4.45M), "GoodsReference[1].GoodsReductionAfterTreatment.quantity");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_ProcedureTransference_CustomsWarehousing_Abnormal()
		{
			var warehousingAuthorisation = MockAuthorisation("!@#", GetLongString("A", AuthorisationReferenceNumberMaxLength + 1)).Object;
			var warehouseProcedures = new[]
			{
				MockWarehouseProcedure(1
					, GetLongString("R", PreviousProcedureRegistrationNumberMaxLength + 1)
					, "0"
					, string.Empty
					, "0"
					, GetLongString("C", PreviousProcedureComplementMaxlength + 1)
					, MockAmount(GetLongString("Q", CargoWise.Customs.DE.MessageContracts.MessageSchema.Common.AmountQualifierMaxLength + 1), GetLongString("U", CargoWise.Customs.DE.MessageContracts.MessageSchema.Common.MeasurementUnitMaxLength + 1), 1.1234m).Object
					, MockAmount(GetLongString("L", CargoWise.Customs.DE.MessageContracts.MessageSchema.Common.AmountQualifierMaxLength + 1), GetLongString("E", CargoWise.Customs.DE.MessageContracts.MessageSchema.Common.MeasurementUnitMaxLength + 1), 2.23m).Object
					, GetLongString("M", MRNMaxLength + 1)
					, GetLongString("H", HarmonizedSystemSubHeadingCodeMaxLength + 1)
					, GetLongString("C", CombinedNomenclatureCodeMaxLength2 + 1)
					, GetLongString("T", TaricCodeMaxLength + 1)
					, GetLongString("N", NationalAdditionalCodeMaxLength + 1)
					).Object
			};
			mockLine.Setup(m => m.WarehouseLocalReferenceNumber).Returns(GetLongString("W", WarehouseLocalReferenceNumberMaxLength + 1));
			mockLine.Setup(m => m.CustomsWarehousingAuthorisation).Returns(warehousingAuthorisation);
			mockLine.Setup(m => m.WarehouseProcedures).Returns(warehouseProcedures);

			var message = messageBuilder.GenerateMessage();
			var customsWarehousing = message.GoodsShipment.GoodsItem[0].ProcedureTransference.CustomsWarehousing;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(customsWarehousing.LRN, Is.EqualTo(GetLongString("W", WarehouseLocalReferenceNumberMaxLength)), "LRN");
				NUnit.Framework.Assert.That(customsWarehousing.Authorisation.type.ToString(), Is.EqualTo("C517"), "type");
				NUnit.Framework.Assert.That(customsWarehousing.Authorisation.referenceNumber, Is.EqualTo(GetLongString("A", AuthorisationReferenceNumberMaxLength)), "referenceNumber");

				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].MRN, Is.EqualTo(GetLongString("M", MRNMaxLength)), "MRN");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].complementOfInformation, Is.EqualTo(GetLongString("C", PreviousProcedureComplementMaxlength)), "complementOfInformation");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].Commodity.CommodityCode.harmonizedSystemSubHeadingCode, Is.EqualTo(GetLongString("H", HarmonizedSystemSubHeadingCodeMaxLength)), "harmonizedSystemSubHeadingCode");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].Commodity.CommodityCode.combinedNomenclatureCode, Is.EqualTo(GetLongString("C", CombinedNomenclatureCodeMaxLength2)), "combinedNomenclatureCode");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].Commodity.CommodityCode.taricCode, Is.EqualTo(GetLongString("T", TaricCodeMaxLength)), "taricCode");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].Commodity.CommodityCode.NationalAdditionalCode.nationalAdditionalCode, Is.EqualTo(GetLongString("N", NationalAdditionalCodeMaxLength)), "nationalAdditionalCode");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].Commodity.GoodsReduction.measurementUnit, Is.EqualTo(GetLongString("U", CargoWise.Customs.DE.MessageContracts.MessageSchema.Common.MeasurementUnitMaxLength)), "GoodsReduction.measurementUnit");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].Commodity.GoodsReduction.qualifier, Is.EqualTo(GetLongString("Q", CargoWise.Customs.DE.MessageContracts.MessageSchema.Common.AmountQualifierMaxLength)), "GoodsReduction.qualifier");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].Commodity.GoodsReduction.quantity, Is.EqualTo(1.123M), "GoodsReduction.quantity");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].Commodity.GoodsReductionAfterTreatment.measurementUnit, Is.EqualTo(GetLongString("E", CargoWise.Customs.DE.MessageContracts.MessageSchema.Common.MeasurementUnitMaxLength)), "GoodsReductionAfterTreatment.measurementUnit");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].Commodity.GoodsReductionAfterTreatment.qualifier, Is.EqualTo(GetLongString("L", CargoWise.Customs.DE.MessageContracts.MessageSchema.Common.AmountQualifierMaxLength)), "GoodsReductionAfterTreatment.qualifier");
				NUnit.Framework.Assert.That(customsWarehousing.GoodsReference[0].Commodity.GoodsReductionAfterTreatment.quantity, Is.EqualTo(2.23M), "GoodsReductionAfterTreatment.quantity");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_ProcedureTransference_CustomsWarehousing_NotWarehouseProcedure()
		{
			mockLine.Setup(m => m.IsWarehouseProcedure).Returns(false);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem[0].ProcedureTransference.CustomsWarehousing, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentGoodsItemProcedureTransferenceCustomsWarehousing)));
		}

		[ExpectNoExceptions]
		public void TestItem_ProcedureTransference_CustomsWarehousing_AuthorisationIsNull()
		{
			mockLine.Setup(m => m.CustomsWarehousingAuthorisation).Returns((IAuthorisation)null);

			var message = messageBuilder.GenerateMessage();
			var customsWarehousing = message.GoodsShipment.GoodsItem[0].ProcedureTransference.CustomsWarehousing;
			NUnit.Framework.Assert.That(customsWarehousing.Authorisation, Is.Not.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentGoodsItemProcedureTransferenceCustomsWarehousingAuthorisation)));
		}

		[ExpectNoExceptions]
		public void TestItem_ProcedureTransference_CustomsWarehousing_MRNIsEmpty()
		{
			CombineAssertions(() =>
			{
				var warehouseProcedures = new[]
				{
					MockWarehouseProcedure(1
					,"REG 1"
					, "0"
					, "0049"
					, "0"
					, "Complement1"
					, MockAmount("A", "042", 1.12m).Object
					, MockAmount("B", "204", 2.23m).Object
					, string.Empty
					, "HSSH01"
					, "C1"
					, "T1"
					, "A"
					).Object,
				};
				mockLine.Setup(m => m.WarehouseProcedures).Returns(warehouseProcedures);

				var message = CreateMessageBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem[0].ProcedureTransference.CustomsWarehousing.GoodsReference[0].registrationNumber, Is.EqualTo("REG 1"), "registrationNumber");

				warehouseProcedures = new[]
				{
					MockWarehouseProcedure(1
					, GetLongString("R", PreviousProcedureRegistrationNumberMaxLength + 1)
					, "0"
					, "0049"
					, "0"
					, "Complement1"
					, MockAmount("A", "042", 1.12m).Object
					, MockAmount("B", "204", 2.23m).Object
					, string.Empty
					, "HSSH01"
					, "C1"
					, "T1"
					, "A"
					).Object,
				};
				mockLine.Setup(m => m.WarehouseProcedures).Returns(warehouseProcedures);
				message = CreateMessageBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem[0].ProcedureTransference.CustomsWarehousing.GoodsReference[0].registrationNumber, Is.EqualTo(GetLongString("R", PreviousProcedureRegistrationNumberMaxLength)), "Long registrationNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_ProcedureTransference_InwardProcessing()
		{
			var message = messageBuilder.GenerateMessage();
			var inwardProcessing = message.GoodsShipment.GoodsItem[0].ProcedureTransference.InwardProcessing;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(inwardProcessing.simplyGrantedAuthorisation.ToString(), Is.EqualTo("Item0"), "simplyGrantedAuthorisation");
				NUnit.Framework.Assert.That(inwardProcessing.Authorisation.type.ToString(), Is.EqualTo("C601"), "type");
				NUnit.Framework.Assert.That(inwardProcessing.Authorisation.referenceNumber, Is.EqualTo("IPA0001"), "Authorisation.referenceNumber");
				NUnit.Framework.Assert.That(inwardProcessing.CustomsOfficeOfSupervision, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentGoodsItemProcedureTransferenceInwardProcessingCustomsOfficeOfSupervision)), "CustomsOfficeOfSupervision - should be [null]");

				NUnit.Framework.Assert.That(inwardProcessing.GoodsReference[0].sequenceNumber, Is.EqualTo("1"), "GoodsReference[0].sequenceNumber");
				NUnit.Framework.Assert.That(inwardProcessing.GoodsReference[0].accessViaATLAS.ToString(), Is.EqualTo("Item0"), "GoodsReference[0].accessViaATLAS");
				NUnit.Framework.Assert.That(inwardProcessing.GoodsReference[0].MRN, Is.EqualTo("MRN001"), "GoodsReference[0].MRN");
				NUnit.Framework.Assert.That(inwardProcessing.GoodsReference[0].registrationNumber, Is.EqualTo(default(string)), "GoodsReference[0].registrationNumber");
				NUnit.Framework.Assert.That(inwardProcessing.GoodsReference[0].goodsItemNumber, Is.EqualTo("1"), "GoodsReference[0].goodsItemNumber");
				NUnit.Framework.Assert.That(inwardProcessing.GoodsReference[0].Commodity.goodsRelatedData, Is.EqualTo("GRI001"), "GoodsReference[0].goodsRelatedData");

				NUnit.Framework.Assert.That(inwardProcessing.GoodsReference[1].sequenceNumber, Is.EqualTo("2"), "GoodsReference[1].sequenceNumber");
				NUnit.Framework.Assert.That(inwardProcessing.GoodsReference[1].accessViaATLAS.ToString(), Is.EqualTo("Item1"), "GoodsReference[1].accessViaATLAS");
				NUnit.Framework.Assert.That(inwardProcessing.GoodsReference[1].MRN, Is.EqualTo("MRN002"), "GoodsReference[1].MRN");
				NUnit.Framework.Assert.That(inwardProcessing.GoodsReference[1].registrationNumber, Is.EqualTo(default(string)), "GoodsReference[1].registrationNumber");
				NUnit.Framework.Assert.That(inwardProcessing.GoodsReference[1].goodsItemNumber, Is.EqualTo("2"), "GoodsReference[1].goodsItemNumber");
				NUnit.Framework.Assert.That(inwardProcessing.GoodsReference[1].Commodity.goodsRelatedData, Is.EqualTo("GRI002"), "GoodsReference[1].goodsRelatedData");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_ProcedureTransference_InwardProcessing_Abnormal()
		{
			var inwardProcessingAuthorisation = MockAuthorisation("!@#", GetLongString("A", AuthorisationReferenceNumberMaxLength + 1)).Object;
			var inwardProcessingProcedures = new[]
			{
				MockInwardProcessingProcedure(1
					, string.Empty
					, "0"
					, GetLongString("G", GoodsRelatedInformationMaxLength + 1)
					, GetLongString("M", MRNMaxLength + 1)
					).Object
			};
			mockLine.Setup(m => m.InwardProcessingAuthorisation).Returns(inwardProcessingAuthorisation);
			mockLine.Setup(m => m.InwardProcessingProcedures).Returns(inwardProcessingProcedures);

			var message = messageBuilder.GenerateMessage();
			var inwardProcessing = message.GoodsShipment.GoodsItem[0].ProcedureTransference.InwardProcessing;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(inwardProcessing.Authorisation.type.ToString(), Is.EqualTo("C601"), "type");
				NUnit.Framework.Assert.That(inwardProcessing.Authorisation.referenceNumber, Is.EqualTo(GetLongString("A", AuthorisationReferenceNumberMaxLength)), "Authorisation.referenceNumber");

				NUnit.Framework.Assert.That(inwardProcessing.GoodsReference[0].MRN, Is.EqualTo(GetLongString("M", MRNMaxLength)), "MRN");
				NUnit.Framework.Assert.That(inwardProcessing.GoodsReference[0].Commodity.goodsRelatedData, Is.EqualTo(GetLongString("G", GoodsRelatedInformationMaxLength)), "goodsRelatedData");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_ProcedureTransference_InwardProcessing_NotInwardProcessingProcedure()
		{
			mockLine.Setup(m => m.IsInwardProcessingProcedure).Returns(false);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem[0].ProcedureTransference.InwardProcessing, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentGoodsItemProcedureTransferenceInwardProcessing)));
		}

		[ExpectNoExceptions]
		public void TestItem_ProcedureTransference_InwardProcessingAuthorisationIsNull()
		{
			mockLine.Setup(m => m.InwardProcessingAuthorisation).Returns((IAuthorisation)null);

			var message = messageBuilder.GenerateMessage();
			var inwardProcessing = message.GoodsShipment.GoodsItem[0].ProcedureTransference.InwardProcessing;
			NUnit.Framework.Assert.That(inwardProcessing.Authorisation, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentGoodsItemProcedureTransferenceInwardProcessingAuthorisation)));
		}

		[ExpectNoExceptions]
		public void TestItem_ProcedureTransference_InwardProcessing_SimplyGrantedAuthorisationIs1()
		{
			CombineAssertions(() =>
			{
				mockLine.Setup(m => m.InwardProcessingSimplyGrantedAuthorisation).Returns("1");
				var message = CreateMessageBuilder().GenerateMessage();
				var inwardProcessing = message.GoodsShipment.GoodsItem[0].ProcedureTransference.InwardProcessing;
				NUnit.Framework.Assert.That(inwardProcessing.Authorisation, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentGoodsItemProcedureTransferenceInwardProcessingAuthorisation)), "Authorisation - should be [null]");
				NUnit.Framework.Assert.That(inwardProcessing.CustomsOfficeOfSupervision.referenceNumber, Is.EqualTo("DE00567"), "CustomsOfficeOfSupervision.referenceNumber");

				mockLine.Setup(m => m.InwardProcessingCustomsOfficeOfSupervision).Returns(GetLongString("C", CustomsOfficeReferenceNumberMaxLength + 1));
				message = CreateMessageBuilder().GenerateMessage();
				inwardProcessing = message.GoodsShipment.GoodsItem[0].ProcedureTransference.InwardProcessing;
				NUnit.Framework.Assert.That(inwardProcessing.CustomsOfficeOfSupervision.referenceNumber, Is.EqualTo(GetLongString("C", CustomsOfficeReferenceNumberMaxLength)), "Long CustomsOfficeOfSupervision.referenceNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_ProcedureTransference_InwardProcessing_SimplyGrantedAuthorisationIsEmpty()
		{
			mockLine.Setup(m => m.InwardProcessingSimplyGrantedAuthorisation).Returns(string.Empty);

			var message = messageBuilder.GenerateMessage();
			var inwardProcessing = message.GoodsShipment.GoodsItem[0].ProcedureTransference.InwardProcessing;
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(inwardProcessing.simplyGrantedAuthorisation.ToString(), Is.EqualTo("Item0"), "simplyGrantedAuthorisation");
				NUnit.Framework.Assert.That(inwardProcessing.Authorisation, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentGoodsItemProcedureTransferenceInwardProcessingAuthorisation)), "Authorisation - should be [null]");
				NUnit.Framework.Assert.That(inwardProcessing.CustomsOfficeOfSupervision, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentGoodsItemProcedureTransferenceInwardProcessingCustomsOfficeOfSupervision)), "CustomsOfficeOfSupervision - should be [null]");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_ProcedureTransference_InwardProcessing_MRNIsEmpty()
		{
			CombineAssertions(() =>
			{
				var inwardProcessingProcedures = new[]
				{
					MockInwardProcessingProcedure(1
					,"REG 1"
					, "0"
					, "GRI001"
					, string.Empty
					).Object
				};
				mockLine.Setup(m => m.InwardProcessingProcedures).Returns(inwardProcessingProcedures);

				var message = CreateMessageBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem[0].ProcedureTransference.InwardProcessing.GoodsReference[0].registrationNumber, Is.EqualTo("REG 1"), "registrationNumber");

				inwardProcessingProcedures = new[]
				{
					MockInwardProcessingProcedure(1
					, GetLongString("R", PreviousProcedureRegistrationNumberMaxLength + 1)
					, "0"
					, "GRI001"
					, string.Empty
					).Object
				};
				mockLine.Setup(m => m.InwardProcessingProcedures).Returns(inwardProcessingProcedures);
				message = CreateMessageBuilder().GenerateMessage();
				NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem[0].ProcedureTransference.InwardProcessing.GoodsReference[0].registrationNumber, Is.EqualTo(GetLongString("R", PreviousProcedureRegistrationNumberMaxLength)), "Long registrationNumber");
			});
		}

		[ExpectNoExceptions]
		public void TestItem_ProcedureTransference_ProcedureTransferenceSpecifiedIsFalse()
		{
			mockLine.Setup(m => m.ProcedureTransferenceSpecified).Returns(false);

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem[0].ProcedureTransference, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPDFGoodsShipmentGoodsItemProcedureTransference)));
		}

		[ExpectNoExceptions]
		public void TestDangerousGoodsCodes()
		{
			var expectedDangerousGoodsCodes = new string[] { "1001", "1002", "1003" };
			mockLine.Setup(m => m.DangerousGoodsCodes).Returns(expectedDangerousGoodsCodes);
			var message = messageBuilder.GenerateMessage();
			CombineAssertions(() =>
			{
				var actualDangerousGoodsCodes = message.GoodsShipment.GoodsItem[0].Commodity.DangerousGoods;
				NUnit.Framework.Assert.That(actualDangerousGoodsCodes.Length, Is.EqualTo(3), "Three DangerousGoodsCodes should exist on the message");
				int i = 1;

				foreach (var dangerousGoodsCode in expectedDangerousGoodsCodes)
				{
					var dangerousGoodsCodeForAssert = actualDangerousGoodsCodes.Single(x => x.UNNumber.Equals(dangerousGoodsCode));
					NUnit.Framework.Assert.That(dangerousGoodsCodeForAssert.sequenceNumber, Is.EqualTo(i.ToString()), $"Sequence number for {dangerousGoodsCode}");
					i++;
				}
			});
		}

		[ExpectNoExceptions]
		public void TestItem_Multiple()
		{
			var mockLine2 = new Mock<IEXPDATLine>();
			mockLine2.Setup(l => l.LineNumber).Returns(2);
			mockLine2.Setup(l => l.StatisticalValueSpecified).Returns(true);
			mockLine2.Setup(l => l.StatisticalValue).Returns(1.11m);
			mockLine2.Setup(l => l.TransactionType).Returns("12");
			mockLine2.Setup(l => l.CountryOfExport).Returns("SG");
			mockLine2.Setup(l => l.CountryOfDestination).Returns("TR");
			mockLine2.Setup(l => l.CommercialReferenceNumber).Returns("CRN0001");
			mockLine2.Setup(l => l.Authorisations).Returns(Array.Empty<IReference>());
			mockLine2.Setup(l => l.RequestedProcedure).Returns("12");
			mockLine2.Setup(l => l.PreviousProcedure).Returns("34");
			mockLine2.Setup(l => l.AdditionalProcedure).Returns("567");
			mockLine2.Setup(l => l.Consignor).Returns((IAESParty)null);
			mockLine2.Setup(l => l.Consignee).Returns((IAESParty)null);
			mockLine2.Setup(l => l.AdditionalSupplyChainActors).Returns(Array.Empty<ISupplyChainActor>());
			mockLine2.Setup(l => l.CountryOfOrigin).Returns("DE");
			mockLine2.Setup(l => l.OriginFederalState).Returns("03");
			mockLine2.Setup(l => l.GoodsDescription).Returns("Goods Description 1");
			mockLine2.Setup(l => l.CusCode).Returns("CC001");
			mockLine2.Setup(l => l.HarmonizedSystemSubHeadingCode).Returns("HSSC01");
			mockLine2.Setup(l => l.CombinedNomenclatureCode).Returns("C1");
			mockLine2.Setup(l => l.TaricFirstAdditionalCode).Returns("TF01");
			mockLine2.Setup(l => l.TaricSecondAdditionalCode).Returns("TF02");
			mockLine2.Setup(l => l.TaricOtherAdditionalCodes).Returns(new ZString[] { "TF03", "TF04", "TF05" });
			mockLine2.Setup(l => l.TaricOtherAdditionalCodesSpecified).Returns(true);
			mockLine2.Setup(l => l.DangerousGoodsCodes).Returns(new string[] { "DG01" });
			mockLine2.Setup(l => l.GrossMass).Returns(2.23);
			mockLine2.Setup(l => l.NetMass).Returns(3.32);
			mockLine2.Setup(l => l.SupplementaryQuantity).Returns(4.16m);
			mockLine2.Setup(l => l.Packages).Returns(Array.Empty<IPackage>());
			mockLine2.Setup(l => l.PreviousDocuments).Returns(Array.Empty<IPreviousDocument>());
			mockLine2.Setup(l => l.Documents).Returns(Array.Empty<ISupportingDocument>());
			mockLine2.Setup(l => l.AdditionalReferences).Returns(Array.Empty<IReference>());
			mockLine2.Setup(l => l.AdditionalInformations).Returns(Array.Empty<IReference>());
			mockLine2.Setup(l => l.TransportChargesPaymentMethod).Returns("A");
			mockLine2.Setup(l => l.IsWarehouseProcedure).Returns(true);
			mockLine2.Setup(l => l.OutwardProcessingReplacement).Returns("1");
			mockLine2.Setup(l => l.OutwardProcessingReimportDate).Returns(new DateTime(2021, 08, 23));
			mockLine2.Setup(l => l.WarehouseLocalReferenceNumber).Returns("WLRN001");
			mockLine2.Setup(l => l.CustomsWarehousingAuthorisation).Returns((IAuthorisation)null);
			mockLine2.Setup(l => l.WarehouseProcedures).Returns(Array.Empty<IWarehouseProcedure>());
			mockLine2.Setup(l => l.IsInwardProcessingProcedure).Returns(true);
			mockLine2.Setup(l => l.InwardProcessingSimplyGrantedAuthorisation).Returns("1");
			mockLine2.Setup(l => l.InwardProcessingAuthorisation).Returns((IAuthorisation)null);
			mockLine2.Setup(l => l.InwardProcessingCustomsOfficeOfSupervision).Returns("DE00567");
			mockLine2.Setup(l => l.InwardProcessingProcedures).Returns(Array.Empty<IInwardProcessingProcedure>());
			mockLine2.Setup(m => m.ProcedureTransferenceSpecified).Returns(true);
			mockHeader.Setup(m => m.Lines).Returns(new[] { mockLine.Object, mockLine2.Object });

			var message = messageBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.GoodsShipment.GoodsItem.Length, Is.EqualTo(2));
		}

		protected override string GetExpectedMessageVersion() => "F.1.10";

		protected override string GetCompleteMessageFileName() => "TestEXPDATMessage";

		protected override void SetUp()
		{
			base.SetUp();

			SetUpHeader();
			SetUpLine();
			mockHeader.Setup(m => m.Lines).Returns(new[] { mockLine.Object });
			messageHeader.Setup(m => m.AESHeader).Returns(mockHeader.Object);
			messageBuilder = CreateMessageBuilder();
		}

		EXPDATMessageBuilder CreateMessageBuilder()
		{
			return new EXPDATMessageBuilder(messageHeader.Object);
		}

		Mock<IEXPDATHeader> mockHeader;
		Mock<IEXPDATLine> mockLine;

		void SetUpHeader()
		{
			var submissionDateAndTimeMock = new Mock<IDateAndTime>();
			submissionDateAndTimeMock.Setup(dt => dt.DateAndTime).Returns(new DateTime(2021, 08, 12, 09, 53, 11, DateTimeKind.Utc));

			mockHeader = new Mock<IEXPDATHeader>();
			mockHeader.Setup(h => h.LocalReferenceNumber).Returns("LRN0001");
			mockHeader.Setup(h => h.DeclarationType).Returns("T1");
			mockHeader.Setup(h => h.ExportDeclarationType).Returns("00000110");
			mockHeader.Setup(h => h.PartyConstellation).Returns("0010");
			mockHeader.Setup(h => h.SubmissionDateAndTimeUtc).Returns(submissionDateAndTimeMock.Object);
			mockHeader.Setup(h => h.DecisiveDate).Returns(new ZDate(2021, 01, 01));
			mockHeader.Setup(h => h.ExitDate).Returns(new ZDate(2021, 02, 01));
			mockHeader.Setup(h => h.PresentationStartDateAndTimeUtc).Returns(new DateTime(2021, 03, 01, 01, 03, 00, DateTimeKind.Utc));
			mockHeader.Setup(h => h.LoadingEndDateAndTimeUtc).Returns(new DateTime(2021, 04, 01, 00, 03, 00, DateTimeKind.Utc));
			mockHeader.Setup(h => h.Security).Returns("2");
			mockHeader.Setup(h => h.InvoiceAmountAndCurrencySpecified).Returns(true);
			mockHeader.Setup(h => h.SpecificCircumstanceIndicator).Returns("A");
			mockHeader.Setup(h => h.InvoiceAmount).Returns(2.22M);
			mockHeader.Setup(h => h.Currency).Returns("EUR");
			mockHeader.Setup(h => h.Authorisations).Returns((IReadOnlyCollection<IAuthorisation>)GetAuthorisations());
			mockHeader.Setup(h => h.CustomsOfficeOfPresentation).Returns("DE0001");
			mockHeader.Setup(h => h.ExportCustomsOffice).Returns("DE0002");
			mockHeader.Setup(h => h.SupplementaryDeclarationCustomsOffice).Returns("DE0003");
			mockHeader.Setup(h => h.IntendedExitCustomsOffice).Returns("DE0004");
			mockHeader.Setup(h => h.ActualExitCustomsOffice).Returns("DE0005");
			mockHeader.Setup(h => h.ContractualPartner).Returns(GetContractualPartner());
			mockHeader.Setup(h => h.Exporter).Returns(GetExporter());
			mockHeader.Setup(h => h.Declarant).Returns(GetDeclarant());
			mockHeader.Setup(h => h.Representative).Returns(GetRepresentative());
			mockHeader.Setup(h => h.SubContractor).Returns(GetSubContractor());
			mockHeader.Setup(h => h.TransactionType).Returns("31");
			mockHeader.Setup(h => h.ExportCountry).Returns("DE");
			mockHeader.Setup(h => h.DestinationCountry).Returns("CN");
			mockHeader.Setup(h => h.AdditionalSupplyChainActors)
				.Returns((IReadOnlyCollection<ISupplyChainActor>)GetAdditionalSupplyChainActors());
			mockHeader.Setup(h => h.DeliveryTerms).Returns(GetDeliveryTerms());
			mockHeader.Setup(h => h.OutwardProcessing).Returns(GetOutwardProcessing());
			mockHeader.Setup(h => h.PreviousDocuments)
				.Returns((IReadOnlyCollection<IPreviousDocument>)GetPreviousDocuments());
			mockHeader.Setup(h => h.SupportingDocuments)
				.Returns((IReadOnlyCollection<ISupportingDocument>)GetSupportingDocuments());
			mockHeader.Setup(h => h.AdditionalReferences)
				.Returns((IReadOnlyCollection<IReference>)GetAdditionalReferences());
			mockHeader.Setup(h => h.AdditionalInformations)
				.Returns((IReadOnlyCollection<IReference>)GetAdditionalInformations());
			mockHeader.Setup(h => h.IsContainerized).Returns(ZBool.False);
			mockHeader.Setup(h => h.ContainerIndicatorSpecified).Returns(true);
			mockHeader.Setup(h => h.InlandTransportMeansMode).Returns("1");
			mockHeader.Setup(h => h.BorderTransportMeansMode).Returns("1");
			mockHeader.Setup(h => h.TotalGrossMass).Returns(200.55m);
			mockHeader.Setup(h => h.CommercialReferenceNumber).Returns("CRN00001");
			mockHeader.Setup(h => h.RegistrationNumber).Returns("RN000001");
			mockHeader.Setup(h => h.Carrier).Returns(GetCarrier());
			mockHeader.Setup(h => h.Consignor).Returns(GetConsignor());
			mockHeader.Setup(h => h.Consignee).Returns(GetConsignee());
			mockHeader.Setup(h => h.TransportEquipments)
				.Returns((IReadOnlyCollection<ITransportEquipment>)GetTransportEquipments());
			mockHeader.Setup(h => h.LocationOfGoodsSpecified).Returns(true);
			mockHeader.Setup(h => h.TypeOfLocation).Returns("B");
			mockHeader.Setup(h => h.QualifierOfIdentification).Returns("V");
			mockHeader.Setup(h => h.AuthorisationNumber).Returns("AU00001");
			mockHeader.Setup(h => h.AdditionalIdentifier).Returns("AI01");
			mockHeader.Setup(h => h.UNLocode).Returns("UNL001");
			mockHeader.Setup(h => h.GNSSSpecified).Returns(true);
			mockHeader.Setup(h => h.GNSSLatitude).Returns(100.23);
			mockHeader.Setup(h => h.GNSSLongitude).Returns(60.37);
			mockHeader.Setup(h => h.LocationOfGoodsParty).Returns(GetLocationOfGoodsParty());
			mockHeader.Setup(h => h.LocationOfGoodsContactPerson).Returns(GetContactPerson());
			mockHeader.Setup(h => h.DepartureTransportMeans)
				.Returns((IReadOnlyCollection<IDepartureTransportMeans>)GetDepartureTransportMeans());
			mockHeader.Setup(h => h.ItineraryCountries).Returns(new ZString[] { "CN", "US" });
			mockHeader.Setup(h => h.ActiveBorderTransportMeansSpecified).Returns(true);
			mockHeader.Setup(h => h.BorderTransportMeansType).Returns("10");
			mockHeader.Setup(h => h.BorderTransportMeansIdentity).Returns("1234567890");
			mockHeader.Setup(h => h.BorderTransportMeansNationality).Returns("AU");
			mockHeader.Setup(h => h.TransportDocuments)
				.Returns((IReadOnlyCollection<IReference>)GetTransportDocuments());
			mockHeader.Setup(m => m.TransportChargesPaymentMethod).Returns("B");

			IEnumerable<IAuthorisation> GetAuthorisations()
			{
				return new[]
				{
					MockAuthorisation("C512", "1234567890").Object,
					MockAuthorisation("C514", "1234567891").Object,
				};
			}

			IAESParty GetContractualPartner()
			{
				return MockParty("DEEOR001"
				, "0001"
				, "ContractualPartner"
				, "ContractualPartner Address1"
				, "ContractualPartner City"
				, "123456789"
				, "DE"
				, tcuNumber: "DETCU001"
				, address2: "ContractualPartner Address2"
				).Object;
			}

			IAESParty GetExporter()
			{
				return MockParty("DEEOR002"
				, "0002"
				, "Exporter"
				, "Exporter Address1"
				, "Exporter City"
				, "123456789"
				, "DE"
				, address2: "Exporter Address2"
				).Object;
			}

			IAESParty GetDeclarant()
			{
				return MockParty("DEEOR003"
				, "0003"
				, "Declarant"
				, "Declarant Address1"
				, "Declarant City"
				, "123456789"
				, "DE"
				, contactPerson: GetContactPerson()
				, address2: "Exporter Address2"
				).Object;
			}

			IAESParty GetRepresentative()
			{
				return MockParty("DEEOR004"
				, "0004"
				, contactPerson: GetContactPerson()
				).Object;
			}

			IAESParty GetSubContractor()
			{
				return MockParty("DEEOR005"
				, "0005"
				, "SubContractor"
				, "SubContractor Address1"
				, "SubContractor City"
				, "123456789"
				, "DE"
				, address2: "SubContractor Address2"
				).Object;
			}

			IDeliveryTerms GetDeliveryTerms()
			{
				return MockDeliveryTerms("CFR", "LOC1").Object;
			}

			IOutwardProcessing GetOutwardProcessing()
			{
				return MockOutwardProcessing(new string[] { "DE", "US" }, GetIdentificationMeans(), GetProducts()).Object;
			}

			IReadOnlyCollection<IIdentificationMeans> GetIdentificationMeans()
			{
				return new[]
				{
					MockIdentificationMean("A", "A Description").Object,
					MockIdentificationMean("B", "B Description").Object
				};
			}

			IReadOnlyCollection<IProduct> GetProducts()
			{
				return new[]
				{
					MockProduct("39220000", "Goods Description 1", "123456", "78").Object,
					MockProduct("48880000", "Goods Description 2", "2345678", "90").Object
				};
			}

			IAESParty GetCarrier()
			{
				return MockParty("DEEOR006", "0006").Object;
			}

			IAESParty GetConsignor()
			{
				return MockParty("DEEOR007"
				, "0007"
				, "Consignor"
				, "Consignor Address1"
				, "Consignor City"
				, "123456789"
				, "DE"
				, address2: "Consignor Address2"
				).Object;
			}

			IAESParty GetConsignee()
			{
				return MockParty("DEEOR008"
				, "0008"
				, "Consignee"
				, "Consignee Address1"
				, "Consignee City"
				, "123456789"
				, "DE"
				, address2: "Consignee Address2"
			  ).Object;
			}

			IEnumerable<ITransportEquipment> GetTransportEquipments()
			{
				return new[]
				{
					MockTransportEquipment("Reference1", new string[] { "1001", "1002" }, new int[] { 1, 3 }).Object,
					MockTransportEquipment("Reference2", new string[] { "2001", "2002" }, new int[] { 2, 4 }).Object,
				};
			}

			IPartyDocAddress GetLocationOfGoodsParty()
			{
				return MockPartyDocAddress("AdditionalInformation", "LocationOfGoods Address1", "LocationOfGoods Address2", "123456789", "LocationOfGoods City", "DE").Object;
			}

			IEnumerable<IDepartureTransportMeans> GetDepartureTransportMeans()
			{
				return new[]
				{
					MockDepartureTransportMeans("T1", "0001", "DE").Object,
					MockDepartureTransportMeans("T2", "0002", "AU").Object,
				};
			}

			IEnumerable<IReference> GetTransportDocuments()
			{
				return new[]
				{
					MockAdditionalInfo(type: "Tpt1", qualifier: "Qu1", referenceNumber: "Reference1").Object,
					MockAdditionalInfo(type: "Tpt2", qualifier: "Qu2", referenceNumber: "Reference2").Object
				};
			}
		}

		void SetUpLine()
		{
			mockLine = new Mock<IEXPDATLine>();
			mockLine.Setup(l => l.LineNumber).Returns(1);
			mockLine.Setup(l => l.StatisticalValueSpecified).Returns(true);
			mockLine.Setup(l => l.StatisticalValue).Returns(1.11m);
			mockLine.Setup(l => l.TransactionType).Returns("12");
			mockLine.Setup(l => l.CountryOfExport).Returns("SG");
			mockLine.Setup(l => l.CountryOfDestination).Returns("TR");
			mockLine.Setup(l => l.CommercialReferenceNumber).Returns("CRN0001");
			mockLine.Setup(l => l.Authorisations).Returns((IReadOnlyCollection<IReference>)GetAuthorisations());
			mockLine.Setup(l => l.RequestedProcedure).Returns("12");
			mockLine.Setup(l => l.PreviousProcedure).Returns("34");
			mockLine.Setup(l => l.AdditionalProcedure).Returns("567");
			mockLine.Setup(l => l.Consignor).Returns(GetConsignor());
			mockLine.Setup(l => l.Consignee).Returns(GetConsignee());
			mockLine.Setup(l => l.AdditionalSupplyChainActors).Returns((IReadOnlyCollection<ISupplyChainActor>)GetAdditionalSupplyChainActors());
			mockLine.Setup(l => l.CountryOfOrigin).Returns("DE");
			mockLine.Setup(l => l.OriginFederalState).Returns("03");
			mockLine.Setup(l => l.GoodsDescription).Returns("Goods Description 1");
			mockLine.Setup(l => l.CusCode).Returns("CC001");
			mockLine.Setup(l => l.HarmonizedSystemSubHeadingCode).Returns("HSSC01");
			mockLine.Setup(l => l.CombinedNomenclatureCode).Returns("C1");
			mockLine.Setup(l => l.TaricFirstAdditionalCode).Returns("TF01");
			mockLine.Setup(l => l.TaricSecondAdditionalCode).Returns("TF02");
			mockLine.Setup(l => l.TaricOtherAdditionalCodes).Returns(new ZString[] { "TF03", "TF04", "TF05" });
			mockLine.Setup(l => l.TaricOtherAdditionalCodesSpecified).Returns(true);
			mockLine.Setup(l => l.DangerousGoodsCodes).Returns(new string[] { "DG01" });
			mockLine.Setup(l => l.GrossMass).Returns(2.23);
			mockLine.Setup(l => l.NetMass).Returns(3.32);
			mockLine.Setup(l => l.SupplementaryQuantity).Returns(4.16m);
			mockLine.Setup(l => l.Packages).Returns((IReadOnlyCollection<IPackage>)GetPackages());
			mockLine.Setup(l => l.PreviousDocuments).Returns((IReadOnlyCollection<IPreviousDocument>)GetPreviousDocuments());
			mockLine.Setup(l => l.Documents).Returns((IReadOnlyCollection<ISupportingDocument>)GetSupportingDocuments());
			mockLine.Setup(l => l.AdditionalReferences).Returns((IReadOnlyCollection<IReference>)GetAdditionalReferences());
			mockLine.Setup(l => l.AdditionalInformations).Returns((IReadOnlyCollection<IReference>)GetAdditionalInformations());
			mockLine.Setup(l => l.TransportChargesPaymentMethod).Returns("A");
			mockLine.Setup(l => l.OutwardProcessingReplacement).Returns("1");
			mockLine.Setup(l => l.OutwardProcessingReimportDate).Returns(new DateTime(2021, 08, 23));
			mockLine.Setup(l => l.IsWarehouseProcedure).Returns(true);
			mockLine.Setup(l => l.WarehouseLocalReferenceNumber).Returns("WLRN001");
			mockLine.Setup(l => l.CustomsWarehousingAuthorisation).Returns(MockAuthorisation("C518", "CWA0001").Object);
			mockLine.Setup(l => l.WarehouseProcedures).Returns((IReadOnlyCollection<IWarehouseProcedure>)GetWarehouseProcedures());
			mockLine.Setup(l => l.IsInwardProcessingProcedure).Returns(true);
			mockLine.Setup(l => l.InwardProcessingSimplyGrantedAuthorisation).Returns("0");
			mockLine.Setup(l => l.InwardProcessingAuthorisation).Returns(MockAuthorisation("C601", "IPA0001").Object);
			mockLine.Setup(l => l.InwardProcessingCustomsOfficeOfSupervision).Returns("DE00567");
			mockLine.Setup(l => l.InwardProcessingProcedures).Returns(GetInwardProcessingProcedures());
			mockLine.Setup(m => m.ProcedureTransferenceSpecified).Returns(true);

			IEnumerable<IReference> GetAuthorisations()
			{
				return new[]
				{
					MockAdditionalInfo(type: "C516", referenceNumber: "Reference1", detail: "Detail1").Object,
					MockAdditionalInfo(type: "C626", referenceNumber: "Reference2", detail: "Detail2").Object,
					MockAdditionalInfo(type: "C627", referenceNumber: "Reference3", detail: "Detail3").Object,
				};
			}

			IEnumerable<IPackage> GetPackages()
			{
				return new[]
				{
					MockPackage(2, "1A", "1234567890", 0, true).Object,
					MockPackage(0, "2C", "1234567891", 2, false).Object,
				};
			}

			IEnumerable<IWarehouseProcedure> GetWarehouseProcedures()
			{
				return new[]
				{
					MockWarehouseProcedure(1
					,"REG 1"
					, "0"
					, "0049"
					, "0"
					, "Complement1"
					, MockAmount("A", "042", 1.12m).Object
					, MockAmount("B", "204", 2.23m).Object
					, "MRN001"
					, "HSSH01"
					, "C1"
					, "T1"
					, "A"
					).Object,
					MockWarehouseProcedure(2
					,"REG 2"
					, "1"
					, "0035"
					, "1"
					, "Complement2"
					, MockAmount("C", "019", 3.34m).Object
					, MockAmount("D", "202", 4.45m).Object
					, "MRN002"
					, "HSSH02"
					, "C2"
					, "T2"
					, "B"
					).Object,
				};
			}

			IInwardProcessingProcedure[] GetInwardProcessingProcedures()
			{
				return new[]
				{
					MockInwardProcessingProcedure(1
					,"REG 1"
					, "0"
					, "GRI001"
					, "MRN001"
					).Object,
					MockInwardProcessingProcedure(2
					,"REG 2"
					, "1"
					, "GRI002"
					, "MRN002"
					).Object,
				};
			}
		}

		IAESPartyContactPerson GetContactPerson()
		{
			return MockContactPerson("DEV"
			, "VIC"
			, "1324333"
			, "3334"
			, "a@123.com"
			).Object;
		}

		IAESParty GetConsignor()
		{
			return MockParty("DEEOR007"
			, "0007"
			, "Consignor"
			, "Consignor Address1"
			, "Consignor City"
			, "123456789"
			, "DE"
			, address2: "Consignor Address2"
			).Object;
		}

		IAESParty GetConsignee()
		{
			return MockParty("DEEOR008"
			, "0008"
			, "Consignee"
			, "Consignee Address1"
			, "Consignee City"
			, "123456789"
			, "DE"
			, address2: "Consignee Address2"
			).Object;
		}

		IEnumerable<ISupplyChainActor> GetAdditionalSupplyChainActors()
		{
			return new[]
			{
				MockSupplyChainActor("AAA", "1234567890").Object,
				MockSupplyChainActor("BBB", "1234567891").Object,
			};
		}

		IEnumerable<IPreviousDocument> GetPreviousDocuments()
		{
			return new[]
			{
				MockPreviousDocument(string.Empty, "Reference1", "Complement1", "1234", "567", 1, "UN1", 1.12m).Object,
				MockPreviousDocument(string.Empty, "Reference2", "Complement2", "2345", "678", 2, "UN2", 2.34m).Object,
			};
		}

		IEnumerable<ISupportingDocument> GetSupportingDocuments()
		{
			return new[]
			{
				MockDocument("Qu1"
				, "Typ1"
				, "Reference1"
				, "Complement1"
				, "Detail1"
				, new DateTime(2021, 01, 01)
				, new DateTime(2021, 12, 31)
				, 1.12M
				, "KG"
				, 8
				, 1
				, "Name1"
				, "MUQ1"
				, "CU1"
				).Object,
				MockDocument("Qu2"
				, "Typ2"
				, "Reference2"
				, "Complement2"
				, "Detail2"
				, new DateTime(2022, 01, 01)
				, new DateTime(2022, 12, 31)
				, 2.34M
				, "km"
				, 10
				, 2
				, "Name2"
				, "MUQ2"
				, "CU2"
				).Object
			};
		}

		IEnumerable<IReference> GetAdditionalReferences()
		{
			return new[]
			{
				MockAdditionalInfo(string.Empty, "Tpr1", "Qu1", "Reference1", "Complement1", "Detail1", "CU1", 1.12m).Object,
				MockAdditionalInfo(string.Empty, "Tpr2", "Qu2", "Reference2", "Complement2", "Detail2", "CU2", 2.34m).Object
			};
		}

		IEnumerable<IReference> GetAdditionalInformations()
		{
			return new[]
			{
				MockAdditionalInfo(fullType: "Ftpi1", complement: "Complement1").Object,
				MockAdditionalInfo(fullType: "Ftpi2", complement: "Complement2").Object,
			};
		}
	}
}
