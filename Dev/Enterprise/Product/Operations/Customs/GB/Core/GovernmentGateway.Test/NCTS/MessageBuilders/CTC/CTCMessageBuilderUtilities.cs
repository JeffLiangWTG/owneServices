using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;
using Moq;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging.Testing
{
	static class CTCMessageBuilderUtilities
	{
		public static ZString GetEmbeddedResourceFile(ZString embeddedResourceFileName, string path = "Enterprise.Customs.GB.GovernmentGateway.Testing.NCTS.MessageBuilders.CTC.TestFiles.")
		{
			using (var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(path + embeddedResourceFileName))
			using (var sr = new StreamReader(stream))
			{
				return sr.ReadToEnd().TrimEnd(System.Environment.NewLine.ToCharArray());
			}
		}

		public static ICustomsOffice SetupCustomsOffice(ZString referenceNumber, ZString arrivalDateTime)
		{
			var result = new Mock<ICustomsOffice>();
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

		public static IGuarantee SetupGuarantee(ZString type, ZString referenceNumber, ZString otherReference, ZString accessCode, ZDecimal amount, IEnumerable<ZString> notValidForOtherContractingParties)
		{
			var result = new Mock<IGuarantee>();
			result.Setup(m => m.GuaranteeType).Returns(type);
			result.Setup(m => m.GuaranteeReferenceNumber).Returns(referenceNumber);
			result.Setup(m => m.OtherGuaranteeReference).Returns(otherReference);
			result.Setup(m => m.AccessCode).Returns(accessCode);
			result.Setup(m => m.TaxAndDutyLiabiltyAmount).Returns(amount);
			result.Setup(m => m.NotValidForEC).Returns("0");
			result.Setup(m => m.NotValidForOtherContractingParties).Returns((IReadOnlyCollection<ZString>)notValidForOtherContractingParties);
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

		public static IControlResult SetupControlResult(ZString description, ZString controlIndicator, ZString pointerToTheAttribute, ZString correctedValue)
		{
			var resultsOfControlMock = new Mock<IControlResult>();
			resultsOfControlMock.Setup(m => m.ControlIndicator).Returns(controlIndicator);
			resultsOfControlMock.Setup(m => m.PointerToTheAttribute).Returns(pointerToTheAttribute);
			resultsOfControlMock.Setup(m => m.Description).Returns(description);
			resultsOfControlMock.Setup(m => m.DescriptionLNG).Returns("EN-US");
			resultsOfControlMock.Setup(m => m.CorrectedValue).Returns(correctedValue);
			return resultsOfControlMock.Object;
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

		public static IPackage SetupPackage(ZString marksAndNumbers, ZString type, ZLong packages, ZLong pieces, ZBool isBulk, ZBool isUnpacked)
		{
			var result = new Mock<IPackage>();
			result.Setup(m => m.MarksAndNumbersOfPackages).Returns(marksAndNumbers);
			result.Setup(m => m.MarksAndNumbersOfPackagesLanguage).Returns(ZString.Empty);
			result.Setup(m => m.KindOfPackages).Returns(type);
			result.Setup(m => m.NumberOfPackages).Returns(packages);
			result.Setup(m => m.NumberOfPieces).Returns(pieces);
			result.Setup(m => m.IsBulk).Returns(isBulk);
			result.Setup(m => m.IsUnpacked).Returns(isUnpacked);
			return result.Object;
		}

		public static Mock<ITrader> SetupTrader(ZString name, ZString streetAndNumber, ZString postalCode, ZString city, ZString countryCode, ZString nameAndAddressLanguage, ZString tIN, ZString holderIDTIR, ZString representativeCapacity, ZString representativeCapacityLanguage)
		{
			var result = new Mock<ITrader>();
			result.Setup(m => m.Name).Returns(name);
			result.Setup(m => m.StreetAndNumber).Returns(streetAndNumber);
			result.Setup(m => m.PostalCode).Returns(postalCode);
			result.Setup(m => m.City).Returns(city);
			result.Setup(m => m.CountryCode).Returns(countryCode);
			result.Setup(m => m.NameAndAddressLanguage).Returns(nameAndAddressLanguage);
			result.Setup(m => m.TIN).Returns(tIN);
			result.Setup(m => m.HolderIDTIR).Returns(holderIDTIR);
			result.Setup(m => m.RepresentativeCapacity).Returns(representativeCapacity);
			result.Setup(m => m.RepresentativeCapacityLanguage).Returns(representativeCapacityLanguage);
			return result;
		}
	}
}
