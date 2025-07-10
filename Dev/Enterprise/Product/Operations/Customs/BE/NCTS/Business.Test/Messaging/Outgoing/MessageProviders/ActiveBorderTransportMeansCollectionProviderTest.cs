using System.Linq;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	class ActiveBorderTransportMeansCollectionProviderTest : TestCaseWithFactory
	{
		public void TestNoAdditionalTransportMeans()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var depHeader = header.MovementHeader;
			depHeader.BM_CustomsOfficeAtBorder = "BE112233";
			depHeader.BM_ConveyanceNumber = "CN000";
			depHeader.BM_ActiveBorderIdentificationType = "41";
			depHeader.BM_TOLCarrierID = "ABC1234";
			depHeader.BM_RN_NKTOLCarrierNationality = Core.Constants.CountryCodes.Finland;

			var provider = new ActiveBorderTransportMeansCollectionProvider(depHeader);
			AssertEquals("Count", 1, provider.Count);

			var element = provider.First();
			AssertTransportMeans(string.Empty, element, 1, "BE112233", 41, "ABC1234", Core.Constants.CountryCodes.Finland, "CN000");
		}

		public void TestAdditionalTransportMeansOnly()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var depHeader = header.MovementHeader;
			var means = depHeader.AdditionalTransportAtBorderList.AddNew();
			means.TPM_CustomsOffice = "BE112234";
			means.TPM_TypeOfIdentification = "21";
			means.TPM_IdentificationNumber = "XYZ123";
			means.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.Denmark;
			means.TPM_ReferenceNumber = "CN001";

			var provider = new ActiveBorderTransportMeansCollectionProvider(depHeader);
			AssertEquals("Count", 1, provider.Count);

			var element = provider.First();
			AssertTransportMeans(string.Empty, element, 1, "BE112234", 21, "XYZ123", Core.Constants.CountryCodes.Denmark, "CN001");
		}

		public void TestWithAdditionalTransportMeans()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var depHeader = header.MovementHeader;
			depHeader.BM_CustomsOfficeAtBorder = "BE112200";
			depHeader.BM_ConveyanceNumber = "CN000";
			depHeader.BM_ActiveBorderIdentificationType = "41";
			depHeader.BM_TOLCarrierID = "ABC1234";
			depHeader.BM_RN_NKTOLCarrierNationality = Core.Constants.CountryCodes.Finland;

			var means = depHeader.AdditionalTransportAtBorderList.AddNew();
			means.TPM_CustomsOffice = "BE112201";
			means.TPM_TypeOfIdentification = "21";
			means.TPM_IdentificationNumber = "XYZ123";
			means.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.Denmark;
			means.TPM_ReferenceNumber = "CN001";

			var provider = new ActiveBorderTransportMeansCollectionProvider(depHeader);
			AssertEquals("Count", 2, provider.Count);

			using var enumerator = provider.GetEnumerator();
			_ = enumerator.MoveNext();
			AssertTransportMeans("First item", enumerator.Current, 1, "BE112200", 41, "ABC1234", Core.Constants.CountryCodes.Finland, "CN000");
			_ = enumerator.MoveNext();
			AssertTransportMeans("Second item", enumerator.Current, 2, "BE112201", 21, "XYZ123", Core.Constants.CountryCodes.Denmark, "CN001");
		}

		public void TestWithTwoAdditionalTransportMeans()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);

			var depHeader = header.MovementHeader;
			depHeader.BM_CustomsOfficeAtBorder = "BE112233";
			depHeader.BM_ConveyanceNumber = "R1";
			depHeader.BM_ActiveBorderIdentificationType = "20";
			depHeader.BM_TOLCarrierID = "AB120";
			depHeader.BM_RN_NKTOLCarrierNationality = Core.Constants.CountryCodes.Norway;

			var means1 = depHeader.AdditionalTransportAtBorderList.AddNew();
			means1.TPM_CustomsOffice = "BE112232";
			means1.TPM_TypeOfIdentification = "21";
			means1.TPM_IdentificationNumber = "AB121";
			means1.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.Netherlands;
			means1.TPM_ReferenceNumber = "R2";

			var means2 = depHeader.AdditionalTransportAtBorderList.AddNew();
			means2.TPM_CustomsOffice = "BE112231";
			means2.TPM_TypeOfIdentification = "22";
			means2.TPM_IdentificationNumber = "AB122";
			means2.TPM_RN_NKTransportNationality = Core.Constants.CountryCodes.Liechtenstein;
			means2.TPM_ReferenceNumber = "R3";

			var provider = new ActiveBorderTransportMeansCollectionProvider(depHeader);
			AssertEquals("Count", 3, provider.Count);

			using var enumerator = provider.GetEnumerator();
			_ = enumerator.MoveNext();
			AssertTransportMeans("First item", enumerator.Current, 1, "BE112233", 20, "AB120", Core.Constants.CountryCodes.Norway, "R1");
			_ = enumerator.MoveNext();
			AssertTransportMeans("Second item", enumerator.Current, 2, "BE112232", 21, "AB121", Core.Constants.CountryCodes.Netherlands, "R2");
			_ = enumerator.MoveNext();
			AssertTransportMeans("Third item", enumerator.Current, 3, "BE112231", 22, "AB122", Core.Constants.CountryCodes.Liechtenstein, "R3");
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
