using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE815HeaderProvider))]
	sealed class IE815HeaderProviderTest : HeaderProviderAbstractTest<IE815HeaderProvider>
	{
		public void TestJourneyTime()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default value is correct", "H01", HeaderProvider.JourneyTime);
				emcsDeclaration.JourneyTimeNumericPart = 11;
				emcsDeclaration.JourneyTimeFormatPart = JourneyTimeUnitList.Codes.Days;
				AssertEquals("Modified Value is Correct", "D11", HeaderProvider.JourneyTime);
			});
		}

		public void TestTransportModeCode()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("AIR", "4", HeaderProvider.TransportModeCode);

				emcsDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("SEA", "1", HeaderProvider.TransportModeCode);

				emcsDeclaration.JE_TransportMode = TransportTypeList.Codes.Mail;
				AssertEquals("MAI", "5", HeaderProvider.TransportModeCode);

				emcsDeclaration.JE_TransportMode = Core.Constants.TransportModes.Road;
				AssertEquals("ROA", "3", HeaderProvider.TransportModeCode);

				emcsDeclaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
				AssertEquals("RAI", "2", HeaderProvider.TransportModeCode);

				emcsDeclaration.JE_TransportMode = Core.Constants.TransportModes.FixedTransportInstallations;
				AssertEquals("FIX", "7", HeaderProvider.TransportModeCode);

				emcsDeclaration.JE_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
				AssertEquals("IWT", "8", HeaderProvider.TransportModeCode);

				emcsDeclaration.JE_TransportMode = Core.Constants.TransportModes.Other;
				AssertEquals("OTH", "0", HeaderProvider.TransportModeCode);

				emcsDeclaration.JE_TransportMode = "ZZZ";
				AssertEquals("Invalid ZZZ", string.Empty, HeaderProvider.TransportModeCode);
			});
		}

		public void TestInvoiceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("empty InvoiceNumber", string.Empty, HeaderProvider.InvoiceNumber);

				emcsDeclaration.InvoiceNumber = "INV1234";
				AssertEquals("InvoiceNumber not empty", "INV1234", HeaderProvider.InvoiceNumber);
			});
		}

		public void TestInvoiceDate()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for empty", null, HeaderProvider.InvoiceDate);
				var invoiceDate = new DateTime(2018, 09, 09, 09, 09, 09);
				emcsDeclaration.InvoiceDate = invoiceDate;
				AssertEquals("Returns Correct value", invoiceDate, HeaderProvider.InvoiceDate);
			});
		}

		public void TestDestinationTypeCode()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.JE_MessageSubType = ZString.Empty;
				AssertEquals("No Error for empty", string.Empty, HeaderProvider.DestinationTypeCode);
				emcsDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationTaxWarehouse;
				AssertEquals("Returns Correct value", EMCSDestinationTypeList.Codes.DestinationTaxWarehouse, HeaderProvider.DestinationTypeCode);
			});
		}

		public void TestGuarantorType()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.ZG_GuarantorType = ZString.Empty;
				AssertEquals("No Error for empty", string.Empty, HeaderProvider.GuarantorType);
				emcsDeclaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.GuarantorNotRequiredQualifyingMemberStateToEuMovements;
				AssertEquals("Returns Correct value", "5", HeaderProvider.GuarantorType);
			});
		}

		public void TestGuarantorTraders_Empty()
		{
			AssertEquals(true, HeaderProvider.GuarantorTraders.Count == 0);
		}

		public void TestTransportDetails_Null()
		{
			AssertEquals("IEnumerables don't return null", false, HeaderProvider.TransportDetails.Any());
		}

		public void TestTransportDetails()
		{
			CombineAssertions(() =>
			{
				for (var i = 1; i < 6; i++)
				{
					emcsDeclaration.CusContainers.AddNew();
				}
				var transportDetails = HeaderProvider.TransportDetails;
				AssertEquals("5 records, contents is tested in the provider", 5, transportDetails.Count);
			});
		}

		public void TestDispatchImportOfficeReferenceNumber()
		{
			AssertEquals(string.Empty, HeaderProvider.DispatchImportOfficeReferenceNumber);
			var dispatchOffice = emcsDeclaration.CustomsOffices[0];
			dispatchOffice.CY_Code = OfficeCodes_EMCS.Codes.OfficeOfDispatch;
			dispatchOffice.CY_Data = "DispOfce";
			AssertEquals("DispOfce", HeaderProvider.DispatchImportOfficeReferenceNumber);
		}

		public void TestCompetentAuthorityDispatchOfficeReferenceNumber()
		{
			AssertEquals(string.Empty, HeaderProvider.CompetentAuthorityDispatchOfficeReferenceNumber);
			var competentOffice = emcsDeclaration.CustomsOffices[0];
			competentOffice.CY_Code = OfficeCodes_EMCS.Codes.CompetentAuthorityOfDispatch;
			competentOffice.CY_Data = "AuthOfce";
			AssertEquals("AuthOfce", HeaderProvider.CompetentAuthorityDispatchOfficeReferenceNumber);
		}

		public void TestConsigneeTrader_Null()
		{
			AssertNull(HeaderProvider.ConsigneeTrader);
		}

		public void TestConsigneeTrader_Provided()
		{
			emcsDeclaration.ImporterDocumentaryAddress.OrganisationPK = GetPartyTraderExciseNumberOrg("TEN190").PK;
			var consigneeTrader = HeaderProvider.ConsigneeTrader;
			AssertEquals("TraderID is Trader Excise Number", "TEN190", consigneeTrader.TraderId);
		}

		public void TestConsigneeTrader_NullWhenUnknownDestinationConsigneeUnknownAndSubmissionForExport()
		{
			emcsDeclaration.ImporterDocumentaryAddress.OrganisationPK = GetPartyTraderExciseNumberOrg("TEN190").PK;
			emcsDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown;
			emcsDeclaration.ZG_SubmissionType = EMCSSubmissionTypeList.Codes.SubmissionForExport;
			AssertNull(HeaderProvider.ConsigneeTrader);
		}

		public void TestConsignorTraderNull()
		{
			AssertNull(HeaderProvider.ConsignorTrader);
		}

		public void TestConsignorTrader()
		{
			emcsDeclaration.SupplierDocumentaryAddress.OrganisationPK = GetPartyTraderExciseNumberOrg("TRD539").PK;
			var consignorTrader = HeaderProvider.ConsignorTrader;
			AssertEquals("TRD539", consignorTrader.TraderExciseNumber);
		}

		public void TestPlaceOfDispatchTrader_Null()
		{
			AssertNull(HeaderProvider.PlaceOfDispatchTrader);
		}

		public void TestPlaceOfDispatchTrader_IsSupplierWhenWarehouseEmpty()
		{
			emcsDeclaration.SupplierDocumentaryAddress.OrganisationPK = GetPartyTraderIdOrg("TWH874").PK;
			AssertEquals("TWH874", HeaderProvider.PlaceOfDispatchTrader.ReferenceOfTaxWarehouse);
			AssertEquals("en", HeaderProvider.PlaceOfDispatchTrader.Language);
		}

		public void TestPlaceOfDispatchTrader_IsWarehouse()
		{
			emcsDeclaration.SupplierDocumentaryAddress.OrganisationPK = GetPartyTraderIdOrg("TWH874").PK;
			emcsDeclaration.DispatchWarehouseDocumentaryAddress.OrganisationPK = GetPartyTraderIdOrg("TWH923").PK;
			AssertEquals("TWH923", HeaderProvider.PlaceOfDispatchTrader.ReferenceOfTaxWarehouse);
			AssertEquals("en", HeaderProvider.PlaceOfDispatchTrader.Language);
		}

		public void TestPlaceOfDispatchTrader_IsNullWhenOriginTypeNotTaxWarehouse()
		{
			emcsDeclaration.SupplierDocumentaryAddress.OrganisationPK = GetPartyTraderIdOrg("TWH874").PK;
			emcsDeclaration.DispatchWarehouseDocumentaryAddress.OrganisationPK = GetPartyTraderIdOrg("TWH923").PK;
			emcsDeclaration.ZG_OriginType = EMCSOriginTypeList.Codes.Import;

			AssertNull(HeaderProvider.PlaceOfDispatchTrader);
		}

		public void TestDeliveryPlaceTrader_Null()
		{
			AssertNull(HeaderProvider.DeliveryPlaceTrader);
		}

		public void TestDeliveryPlaceTrader_FallbackImporter()
		{
			emcsDeclaration.ImporterDocumentaryAddress.OrganisationPK = GetPartyTraderIdOrg("TID938").PK;
			AssertEquals("TID938", HeaderProvider.DeliveryPlaceTrader.TraderId);
			AssertEquals("en", HeaderProvider.DeliveryPlaceTrader.Language);
		}

		public void TestDeliveryPlaceTrader_DestinationWarehouse()
		{
			emcsDeclaration.DestinationWarehouseDocumentaryAddress.OrganisationPK = GetPartyTraderIdOrg("TID349").PK;
			AssertEquals("TID349", HeaderProvider.DeliveryPlaceTrader.TraderId);
			AssertEquals("en", HeaderProvider.DeliveryPlaceTrader.Language);
		}

		public void TestDeliveryPlaceTrader_NullWhenDestinationRegisteredConsignee() => AssertDeliveryPlaceTraderNull(EMCSDestinationTypeList.Codes.DestinationRegisteredConsignee);

		public void TestDeliveryPlaceTrader_NullWhenDestinationExport() => AssertDeliveryPlaceTraderNull(EMCSDestinationTypeList.Codes.DestinationExport);

		public void TestDeliveryPlaceTrader_NullWhenUnknownDestinationConsigneeUnknown() => AssertDeliveryPlaceTraderNull(EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown);

		public void TestTransportArrangerTraderNull()
		{
			AssertNull(HeaderProvider.TransportArrangerTrader);
		}

		public void TestTransportArrangerTrader()
		{
			emcsDeclaration.CarrierAgentDocumentaryAddress.OrganisationPK = GetPartyVatNumberOrg("VAT438").PK;
			emcsDeclaration.ZG_TransportArrangement = EMCSTransportArrangementList.Codes.Other;
			AssertEquals("VAT438", HeaderProvider.TransportArrangerTrader.VatNumber);
		}

		public void TestTransportArrangerTraderNullWhenTransportArrangerConsignor() => AssertTransportArrangerTraderNull(EMCSTransportArrangementList.Codes.Consignor);

		public void TestTransportArrangerTraderNullWhenTransportArrangerConsignee() => AssertTransportArrangerTraderNull(EMCSTransportArrangementList.Codes.Consignee);

		public void TestFirstTransporterTrader()
		{
			emcsDeclaration.TransporterDocumentaryAddress.OrganisationPK = GetPartyVatNumberOrg("VAT398").PK;
			AssertEquals("VAT398", HeaderProvider.FirstTransporterTrader.VatNumber);
		}

		public void TestFirstTransporterTraderNull()
		{
			AssertNull(HeaderProvider.FirstTransporterTrader);
		}

		public void TestComplementConsigneeMemberStateCode()
		{
			AssertEquals(string.Empty, HeaderProvider.ComplementConsigneeMemberStateCode);
			emcsDeclaration.ZG_CCTMSA = Core.Constants.CountryCodes.Austria;
			AssertEquals(Core.Constants.CountryCodes.Austria, HeaderProvider.ComplementConsigneeMemberStateCode);
		}

		public void TestComplementConsigneeSerialNumberOfCertificateOfExemption()
		{
			AssertEquals(string.Empty, HeaderProvider.ComplementConsigneeSerialNumberOfCertificateOfExemption);
			emcsDeclaration.ZG_CertOfExemption = "COE";
			AssertEquals("COE", HeaderProvider.ComplementConsigneeSerialNumberOfCertificateOfExemption);
		}

		public void TestDeferredSubmissionFlag()
		{
			AssertEquals("0", HeaderProvider.DeferredSubmissionFlag);
			emcsDeclaration.ZG_DeferredSubmission = "1";
			AssertEquals("1", HeaderProvider.DeferredSubmissionFlag);
		}

		public void TestSubmissionMessageType()
		{
			emcsDeclaration.ZG_SubmissionType = ZString.Empty;
			AssertEquals(string.Empty, HeaderProvider.SubmissionMessageType);
			emcsDeclaration.ZG_SubmissionType = EMCSSubmissionTypeList.Codes.StandardSubmission;
			AssertEquals(EMCSSubmissionTypeList.Codes.StandardSubmission, HeaderProvider.SubmissionMessageType);
		}

		public void TestTransportArrangement()
		{
			emcsDeclaration.ZG_TransportArrangement = ZString.Empty;
			AssertEquals(string.Empty, HeaderProvider.TransportArrangement);
			emcsDeclaration.ZG_TransportArrangement = EMCSTransportArrangementList.Codes.Consignor;
			AssertEquals(EMCSTransportArrangementList.Codes.Consignor, HeaderProvider.TransportArrangement);
		}

		public void TestDispatchDateTime()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty is not valid", null, HeaderProvider.DispatchDateTime);
				var dateAtOrigin = new ZDateTime(2018, 09, 09, 12, 12, 12);
				emcsDeclaration.JE_DateAtOrigin = dateAtOrigin;
				AssertEquals("Entered value is returned", dateAtOrigin, HeaderProvider.DispatchDateTime);
			});
		}

		public void TestOriginType()
		{
			emcsDeclaration.ZG_OriginType = ZString.Empty;
			AssertEquals(string.Empty, HeaderProvider.OriginType);
			emcsDeclaration.ZG_OriginType = EMCSOriginTypeList.Codes.TaxWarehouse;
			AssertEquals(EMCSOriginTypeList.Codes.TaxWarehouse, HeaderProvider.OriginType);
		}

		public void TestLocalReferenceNumber()
		{
			emcsDeclaration.JE_DeclarationReference = "E00001001";
			AssertEquals("E00001001", HeaderProvider.LocalReferenceNumber);
		}

		public void TestImportSadNumbers_Null()
		{
			AssertEquals("IEnumerables don't return null", false, HeaderProvider.ImportSadNumbers.Any());
		}

		public void TestImportSadNumbers()
		{
			var expectedSadNumbers = new List<string>();
			for (var i = 1; i < 11; i++)
			{
				var sad = emcsDeclaration.ImportSADNumbers.AddNew();
				var description = "SAD" + i;
				sad.CSI_Description = description;
				expectedSadNumbers.Add(description);
			}
			AssertContainsExactElementsInAnyOrder("Correct codes in collection", expectedSadNumbers, HeaderProvider.ImportSadNumbers);
		}

		public void TestDocuments_Null()
		{
			AssertEquals("IEnumerables don't return null", false, HeaderProvider.Documents.Any());
		}

		public void TestDocuments()
		{
			for (var i = 1; i < 6; i++)
			{
				emcsDeclaration.Documents.AddNew();
			}
			AssertEquals("Correct record count", 5, HeaderProvider.Documents.Count);
		}

		public void TestComplementaryInformation()
		{
			emcsDeclaration.SpecialInstructions = "OTHER DESCRIPTION";
			AssertEquals("OTHER DESCRIPTION", HeaderProvider.ComplementaryInformation.Text);
		}

		public void TestLines_Null()
		{
			AssertEquals("IEnumerables don't return null", false, HeaderProvider.Lines.Any());
		}

		public void TestLines()
		{
			var invoiceHeader = emcsDeclaration.InvoiceHeader;
			invoiceHeader.InvoiceLines.AddNew();
			invoiceHeader.InvoiceLines.AddNew();
			AssertEquals("Correct number of Lines returned", 2, HeaderProvider.Lines.Count);
		}

		public void TestDeliveryPlaceCustomsOfficeReferenceNumber()
		{
			var officeCode = emcsDeclaration.CustomsOffices.AddNew();
			officeCode.CY_Code = OfficeCodes_EMCS.Codes.OfficeOfDelivery;
			officeCode.CY_Data = "DE01876";
			AssertEquals("DE01876", HeaderProvider.DeliveryPlaceCustomsOfficeReferenceNumber);
		}

		protected override IE815HeaderProvider GetHeaderProvider() => new IE815HeaderProvider(emcsDeclaration);

		protected override IE815HeaderProvider GetProvider()
		{
			emcsDeclaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.Transporter;
			emcsDeclaration.TransporterDocumentaryAddress.OrganisationPK = GetPartyGuarantorOrg("TEN251", "EXC251").PK;
			emcsDeclaration.ImporterDocumentaryAddress.OrganisationPK = GetPartyTraderExciseNumberOrg("TEN190").PK;
			emcsDeclaration.SupplierDocumentaryAddress.OrganisationPK = GetPartyTraderExciseNumberOrg("TRD539").PK;
			emcsDeclaration.DispatchWarehouseDocumentaryAddress.OrganisationPK = GetPartyTraderIdOrg("TWH923").PK;
			emcsDeclaration.DestinationWarehouseDocumentaryAddress.OrganisationPK = GetPartyTraderIdOrg("TID349").PK;
			emcsDeclaration.CarrierAgentDocumentaryAddress.OrganisationPK = GetPartyVatNumberOrg("VAT438").PK;
			emcsDeclaration.SpecialInstructions = "OTHER DESCRIPTION";
			return new IE815HeaderProvider(emcsDeclaration);
		}

		protected override IEnumerable<Expression<Func<IE815HeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.GuarantorTraders;
			yield return x => x.ConsigneeTrader;
			yield return x => x.ConsignorTrader;
			yield return x => x.PlaceOfDispatchTrader;
			yield return x => x.DeliveryPlaceTrader;
			yield return x => x.TransportArrangerTrader;
			yield return x => x.FirstTransporterTrader;
			yield return x => x.ComplementaryInformation;
		}

		new IIE815Header HeaderProvider => base.HeaderProvider;

		void AssertDeliveryPlaceTraderNull(string messageSubType)
		{
			emcsDeclaration.JE_MessageSubType = messageSubType;
			emcsDeclaration.ImporterDocumentaryAddress.OrganisationPK = GetPartyTraderIdOrg("TID938").PK;
			AssertNull(HeaderProvider.DeliveryPlaceTrader);
		}

		void AssertTransportArrangerTraderNull(ZString transportArrangement)
		{
			emcsDeclaration.ZG_TransportArrangement = transportArrangement;
			AssertNull(HeaderProvider.TransportArrangerTrader);
		}
	}
}
