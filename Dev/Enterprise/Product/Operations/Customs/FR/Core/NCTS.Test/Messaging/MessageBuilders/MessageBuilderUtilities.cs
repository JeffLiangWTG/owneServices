using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.DataTransfer.Phase4.Testing;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Moq;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	static class MessageBuilderUtilities
	{
		public static void DepartureSetUp(NctsHeader nctsHeader)
		{
			var factory = nctsHeader.Factory;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			NctsHeaderDataObjectWriterTest.SetupNctsHeaderForDeparture(factory, nctsHeader);

			var declarantOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			nctsHeader.Declarant.OrganisationPK = declarantOrgHeader.PK;

			var orgCusAccount = factory.NewWithValidTestData<OrgCusAccount>();
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DTA;
			orgCusAccount.CZ_Account = "12345678";
			orgCusAccount.CZ_OH = declarantOrgHeader.PK;

			var cusCode = declarantOrgHeader.CustomsCodes.AddNew();
			cusCode.OK_RN_NKCodeCountry = "FR";
			cusCode.OK_CodeType = OrgCusCode.FranceCodeTypes.TVA;
			cusCode.OK_CustomsRegNo = "0123456789002";

			var orgAddress = declarantOrgHeader.Addresses.AddNew();
			cusCode.OK_OA_PremisesAddress = orgAddress.PK;
		}

		public static ZString GetEmbeddedResourceFile(ZString embeddedResourceFile)
		{
			using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Enterprise.Customs.FR.NCTS.Testing.Messaging.MessageBuilders.TestFiles." + embeddedResourceFile))
			using (var sr = new StreamReader(stream))
			{
				return sr.ReadToEnd().TrimEnd(System.Environment.NewLine.ToCharArray());
			}
		}

		public static void AssertFullEnvelopeXml(NctsMessageFunctionSet how, NctsHeader nctsHeader, string schemaId, string partyId, string messageRoot)
		{
			var ec = new ErrorCollector();
			nctsHeader.Branch.Company.GC_CustomsRegistrationNo = "DANIEL";

			var principal = nctsHeader.Factory.New<OrgHeader>();
			nctsHeader.Principal.E2_OA_Address = principal.MainAddress.PK;
			var principalAccount = principal.DeltaAgreementNumberCollection.AddNew();
			principalAccount.CZ_Code = OrgCusAccountCodeList.Codes.DTA;
			principalAccount.CZ_Type = OrgCusAccountDeltaTTypeList.Codes.TR;
			principalAccount.CZ_RepresentativeID = "PRN-REP";

			var declarant = nctsHeader.Factory.New<OrgHeader>();
			nctsHeader.Declarant.E2_OA_Address = declarant.MainAddress.PK;
			var declarantAccount = declarant.DeltaAgreementNumberCollection.AddNew();
			declarantAccount.CZ_Code = OrgCusAccountCodeList.Codes.DTA;
			declarantAccount.CZ_Type = OrgCusAccountDeltaTTypeList.Codes.TR;
			declarantAccount.CZ_RepresentativeID = "DEC-REP";

			var builder = new NctsDTMessageBuilder();
			var fullXml = builder.NativeMessage(nctsHeader, how, ec);
			NUnit.Framework.Assertion.AssertContains($@"<Message>
  <EnveloppeMessage>
    <schemaID>{schemaId}</schemaID>
    <schemaVersion>1.0</schemaVersion>
    <partyId>{partyId}</partyId>
    <transactionId>00000001</transactionId>
    <numseq>0</numseq>
    <refdos>NCT00000001</refdos>
  </EnveloppeMessage>
  <Declaration>
    <{messageRoot}", fullXml);
			NUnit.Framework.Assertion.AssertNotContains("No FR customs or blank namespace", "xmlns=", fullXml);
			NUnit.Framework.Assertion.AssertNotContains("No accent ê in message", "ê", fullXml);
			NUnit.Framework.Assertion.AssertNotContains("No accent û in message", "û", fullXml);
			NUnit.Framework.Assertion.AssertNotContains("No accent é in message", "é", fullXml);
			var message1 = nctsHeader.Messages.AddNew();
			fullXml = builder.NativeMessage(nctsHeader, how, ec);
			NUnit.Framework.Assertion.AssertContains($@"<numseq>0</numseq>", fullXml);

			var message2 = nctsHeader.Messages.AddNew();
			message2.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			fullXml = builder.NativeMessage(nctsHeader, how, ec);
			NUnit.Framework.Assertion.AssertContains($@"<numseq>0</numseq>", fullXml);

			var message3ForDeparture = nctsHeader.Messages.AddNew();
			message3ForDeparture.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message3ForDeparture.EM_Status = EDIMessage.Status.Acknowledged;
			message3ForDeparture.EM_MessageType = "015";
			message3ForDeparture.EM_MessageSubType = "DT";
			var message3ForArrival = nctsHeader.Messages.AddNew();
			message3ForArrival.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message3ForArrival.EM_Status = EDIMessage.Status.Acknowledged;
			message3ForArrival.EM_MessageType = "007";
			message3ForArrival.EM_MessageSubType = "DT";
			fullXml = builder.NativeMessage(nctsHeader, how, ec);
			NUnit.Framework.Assertion.AssertContains($@"<numseq>1</numseq>", fullXml);

			var message4ForDeparture = nctsHeader.Messages.AddNew();
			message4ForDeparture.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message4ForDeparture.EM_Status = EDIMessage.Status.Acknowledged;
			message4ForDeparture.EM_MessageType = "15F";
			message4ForDeparture.EM_MessageSubType = "DT";
			var message4ForArrival = nctsHeader.Messages.AddNew();
			message4ForArrival.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
			message4ForArrival.EM_Status = EDIMessage.Status.Acknowledged;
			message4ForArrival.EM_MessageType = "007";
			message4ForArrival.EM_MessageSubType = "DT";

			fullXml = builder.NativeMessage(nctsHeader, how, ec);
			NUnit.Framework.Assertion.AssertContains($@"<numseq>2</numseq>", fullXml);

			nctsHeader.Messages.RemoveAll();
		}

		public static EU.NCTS.Messaging.ICustomsOffice SetupCustomsOffice(ZString referenceNumber, ZString arrivalDateTime)
		{
			var result = new Mock<EU.NCTS.Messaging.ICustomsOffice>();
			result.Setup(m => m.ReferenceNumber).Returns(referenceNumber);
			result.Setup(m => m.ArrivalTime).Returns(arrivalDateTime);
			return result.Object;
		}

		public static ISealID SetupSeal(ZString sealNumber)
		{
			var result = new Mock<ISealID>();
			result.Setup(m => m.SealIdentity).Returns(sealNumber);
			result.Setup(m => m.SealIdentityLanguage).Returns(ZString.Empty);
			return result.Object;
		}

		public static IGuarantee SetupGuarantee(ZString type, ZString referenceNumber, ZString otherReference, ZString accessCode, ZDecimal amount)
		{
			var result = new Mock<IGuarantee>();
			result.Setup(m => m.GuaranteeType).Returns(type);
			result.Setup(m => m.GuaranteeReferenceNumber).Returns(referenceNumber);
			result.Setup(m => m.OtherGuaranteeReference).Returns(otherReference);
			result.Setup(m => m.AccessCode).Returns(accessCode);
			result.Setup(m => m.TaxAndDutyLiabiltyAmount).Returns(amount);
			result.Setup(m => m.NotValidForEC).Returns("0");
			result.Setup(m => m.NotValidForOtherContractingParties).Returns((IReadOnlyCollection<ZString>)Enumerable.Empty<ZString>());
			return result.Object;
		}

		public static IPreviousAdministrativeReference SetupPreviousAdministrativeReference(ZString type, ZString reference)
		{
			var result = new Mock<IPreviousAdministrativeReference>();
			result.Setup(m => m.PreviousDocumentType).Returns(type);
			result.Setup(m => m.PreviousDocumentReference).Returns(reference);
			result.Setup(m => m.PreviousDocumentReferenceLanguage).Returns(ZString.Empty);
			result.Setup(m => m.ComplementOfInformation).Returns(ZString.Empty);
			result.Setup(m => m.ComplementOfInformationLanguage).Returns(ZString.Empty);
			return result.Object;
		}

		public static IProducedDocumentCertificate SetupDocumentCertificate(ZString type, ZString reference, ZString information)
		{
			var result = new Mock<IProducedDocumentCertificate>();
			result.Setup(m => m.DocumentType).Returns(type);
			result.Setup(m => m.DocumentReference).Returns(reference);
			result.Setup(m => m.DocumentReferenceLanguage).Returns(ZString.Empty);
			result.Setup(m => m.ComplementOfInformation).Returns(information);
			result.Setup(m => m.ComplementOfInformationLanguage).Returns(ZString.Empty);
			return result.Object;
		}

		public static IStatement SetupSpecialMention(ZString text, ZString statement, ZString exportFromEC, ZString exportCountry)
		{
			var result = new Mock<IStatement>();
			result.Setup(m => m.StatementText).Returns(text);
			result.Setup(m => m.Statement).Returns(statement);
			result.Setup(m => m.ExportFromEC).Returns(new ZBool(exportFromEC));
			result.Setup(m => m.ExportFromCountry).Returns(exportCountry);
			return result.Object;
		}

		public static IPackage SetupPackage(ZString marksAndNumbers, ZString type, ZLong packages, ZLong pieces, bool isBulk, bool isUnpacked)
		{
			var result = new Mock<IPackage>();
			result.Setup(m => m.MarksAndNumbersOfPackages).Returns(marksAndNumbers);
			result.Setup(m => m.MarksAndNumbersOfPackagesLanguage).Returns(ZString.Empty);
			result.Setup(m => m.KindOfPackages).Returns(type);
			result.Setup(m => m.NumberOfPackages).Returns(packages);
			result.Setup(m => m.NumberOfUnits).Returns(isUnpacked ? pieces : packages);
			result.Setup(m => m.NumberOfPieces).Returns(pieces);
			result.Setup(m => m.IsBulk).Returns(isBulk);
			result.Setup(m => m.IsUnpacked).Returns(isUnpacked);
			return result.Object;
		}
	}
}
