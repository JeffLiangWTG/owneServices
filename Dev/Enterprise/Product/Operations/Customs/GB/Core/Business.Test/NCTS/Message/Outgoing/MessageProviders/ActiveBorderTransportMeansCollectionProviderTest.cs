using System.Linq;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	class ActiveBorderTransportMeansCollectionProviderTest : TestCaseWithFactory
	{
		public void TestNoAdditionalTransportMeans()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var depHeader = header.MovementHeader;
			depHeader.BM_CustomsOfficeAtBorder = "GB000011";
			depHeader.BM_ConveyanceNumber = "CN000";
			depHeader.BM_ActiveBorderIdentificationType = "41";
			depHeader.BM_TOLCarrierID = "ABC1234";
			depHeader.BM_RN_NKTOLCarrierNationality = Core.Constants.CountryCodes.Finland;

			var provider = new ActiveBorderTransportMeansCollectionProvider(depHeader);
			AssertEquals("Count", 1, provider.Count);

			var element = provider.First();
			AssertTransportMeans(string.Empty, element, 1, "GB000011", 41, "ABC1234", Core.Constants.CountryCodes.Finland, "CN000");
		}

		public void TestAdditionalTransportMeansOnly()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var depHeader = header.MovementHeader;
			var means = depHeader.AdditionalTransportAtBorderList.AddNew();
			means.TPM_CustomsOffice = "GB000012";
			means.TPM_TypeOfIdentification = "21";
			means.TPM_IdentificationNumber = "XYZ123";
			means.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.Denmark;
			means.TPM_ReferenceNumber = "CN001";

			var provider = new ActiveBorderTransportMeansCollectionProvider(depHeader);
			AssertEquals("Count", 1, provider.Count);

			var element = provider.First();
			AssertTransportMeans(string.Empty, element, 1, "GB000012", 21, "XYZ123", Core.Constants.CountryCodes.Denmark, "CN001");
		}

		public void TestWithAdditionalTransportMeans()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var depHeader = header.MovementHeader;
			depHeader.BM_CustomsOfficeAtBorder = "GB000011";
			depHeader.BM_ConveyanceNumber = "CN000";
			depHeader.BM_ActiveBorderIdentificationType = "41";
			depHeader.BM_TOLCarrierID = "ABC1234";
			depHeader.BM_RN_NKTOLCarrierNationality = Core.Constants.CountryCodes.Finland;

			var means = depHeader.AdditionalTransportAtBorderList.AddNew();
			means.TPM_CustomsOffice = "GB000012";
			means.TPM_TypeOfIdentification = "21";
			means.TPM_IdentificationNumber = "XYZ123";
			means.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.Denmark;
			means.TPM_ReferenceNumber = "CN001";

			var provider = new ActiveBorderTransportMeansCollectionProvider(depHeader);
			AssertEquals("Count", 2, provider.Count);

			using var enumerator = provider.GetEnumerator();
			_ = enumerator.MoveNext();
			AssertTransportMeans("First item", enumerator.Current, 1, "GB000011", 41, "ABC1234", Core.Constants.CountryCodes.Finland, "CN000");
			_ = enumerator.MoveNext();
			AssertTransportMeans("Second item", enumerator.Current, 2, "GB000012", 21, "XYZ123", Core.Constants.CountryCodes.Denmark, "CN001");
		}

		public void TestWithTwoAdditionalTransportMeans()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var depHeader = header.MovementHeader;
			depHeader.BM_CustomsOfficeAtBorder = "GB000021";
			depHeader.BM_ConveyanceNumber = "R1";
			depHeader.BM_ActiveBorderIdentificationType = "20";
			depHeader.BM_TOLCarrierID = "AB120";
			depHeader.BM_RN_NKTOLCarrierNationality = Core.Constants.CountryCodes.Norway;

			var means1 = depHeader.AdditionalTransportAtBorderList.AddNew();
			means1.TPM_CustomsOffice = "GB000022";
			means1.TPM_TypeOfIdentification = "21";
			means1.TPM_IdentificationNumber = "AB121";
			means1.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.Netherlands;
			means1.TPM_ReferenceNumber = "R2";

			var means2 = depHeader.AdditionalTransportAtBorderList.AddNew();
			means2.TPM_CustomsOffice = "GB000023";
			means2.TPM_TypeOfIdentification = "22";
			means2.TPM_IdentificationNumber = "AB122";
			means2.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.Liechtenstein;
			means2.TPM_ReferenceNumber = "R3";

			var provider = new ActiveBorderTransportMeansCollectionProvider(depHeader);
			AssertEquals("Count", 3, provider.Count);

			using var enumerator = provider.GetEnumerator();
			_ = enumerator.MoveNext();
			AssertTransportMeans("First item", enumerator.Current, 1, "GB000021", 20, "AB120", Core.Constants.CountryCodes.Norway, "R1");
			_ = enumerator.MoveNext();
			AssertTransportMeans("Second item", enumerator.Current, 2, "GB000022", 21, "AB121", Core.Constants.CountryCodes.Netherlands, "R2");
			_ = enumerator.MoveNext();
			AssertTransportMeans("Third item", enumerator.Current, 3, "GB000023", 22, "AB122", Core.Constants.CountryCodes.Liechtenstein, "R3");
		}

		void AssertTransportMeans(string message, IActiveBorderTransportMeans testElement, int sequenceNumber, string customsOfficeAtBorderReferenceNumber, int typeOfIdentification, string identificationNumber, string nationality, string conveyanceReferenceNumber)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("SequenceNumber", sequenceNumber, testElement.SequenceNumber);
				AssertEquals("CustomsOfficeAtBorderReferenceNumber", customsOfficeAtBorderReferenceNumber, testElement.CustomsOfficeAtBorderReferenceNumber);
				AssertEquals("TypeOfIdentification", typeOfIdentification, testElement.TypeOfIdentification);
				AssertEquals("IdentificationNumber", identificationNumber, testElement.IdentificationNumber);
				AssertEquals("Nationality", nationality, testElement.Nationality);
				AssertEquals("ConveyanceReferenceNumber", conveyanceReferenceNumber, testElement.ConveyanceReferenceNumber);
			});
		}
	}
}
