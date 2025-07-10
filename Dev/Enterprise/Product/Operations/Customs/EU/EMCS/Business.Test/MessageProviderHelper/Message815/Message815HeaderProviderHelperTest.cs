using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class Message815HeaderProviderHelperTest : TestCaseWithFactory
	{
		public void TestJourneyTime()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default value is correct", "H01", helper.JourneyTime);
				emcsDeclaration.JourneyTimeNumericPart = 11;
				emcsDeclaration.JourneyTimeFormatPart = JourneyTimeUnitList.Codes.Days;
				AssertEquals("Modified Value is Correct", "D11", helper.JourneyTime);
			});
		}

		public void TestTransportModeCode()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("AIR", "4", helper.TransportModeCode);

				emcsDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				AssertEquals("SEA", "1", helper.TransportModeCode);

				emcsDeclaration.JE_TransportMode = TransportTypeList.Codes.Mail;
				AssertEquals("MAI", "5", helper.TransportModeCode);

				emcsDeclaration.JE_TransportMode = Core.Constants.TransportModes.Road;
				AssertEquals("ROA", "3", helper.TransportModeCode);

				emcsDeclaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
				AssertEquals("RAI", "2", helper.TransportModeCode);

				emcsDeclaration.JE_TransportMode = Core.Constants.TransportModes.FixedTransportInstallations;
				AssertEquals("FIX", "7", helper.TransportModeCode);

				emcsDeclaration.JE_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
				AssertEquals("IWT", "8", helper.TransportModeCode);

				emcsDeclaration.JE_TransportMode = Core.Constants.TransportModes.Other;
				AssertEquals("OTH", "0", helper.TransportModeCode);

				emcsDeclaration.JE_TransportMode = "ZZZ";
				AssertEquals("Invalid ZZZ", string.Empty, helper.TransportModeCode);
			});
		}

		public void TestInvoiceDate()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for empty", null, helper.InvoiceDate);
				var invoiceDate = new DateTime(2018, 09, 09, 09, 09, 09);
				emcsDeclaration.InvoiceDate = invoiceDate;
				AssertEquals("Returns Correct value", invoiceDate, helper.InvoiceDate);
			});
		}

		public void TestDestinationTypeCode()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.JE_MessageSubType = ZString.Empty;
				AssertEquals("No Error for empty", string.Empty, helper.DestinationTypeCode);
				emcsDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationTaxWarehouse;
				AssertEquals("Returns Correct value", EMCSDestinationTypeList.Codes.DestinationTaxWarehouse, helper.DestinationTypeCode);
			});
		}

		public void TestGuarantorType()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.ZG_GuarantorType = ZString.Empty;
				AssertEquals("No Error for empty", string.Empty, helper.GuarantorType);
				emcsDeclaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.GuarantorNotRequiredQualifyingMemberStateToEuMovements;
				AssertEquals("Returns Correct value", "5", helper.GuarantorType);
			});
		}

		public void TestDispatchImportOfficeReferenceNumber()
		{
			AssertEquals(string.Empty, helper.DispatchImportOfficeReferenceNumber);
			var dispatchOffice = emcsDeclaration.CustomsOffices[0];
			dispatchOffice.CY_Code = OfficeCodes_EMCS.Codes.OfficeOfDispatch;
			dispatchOffice.CY_Data = "DispOfce";
			AssertEquals("DispOfce", helper.DispatchImportOfficeReferenceNumber);
		}

		public void TestCompetentAuthorityDispatchOfficeReferenceNumber()
		{
			AssertEquals(string.Empty, helper.CompetentAuthorityDispatchOfficeReferenceNumber);
			var competentOffice = emcsDeclaration.CustomsOffices[0];
			competentOffice.CY_Code = OfficeCodes_EMCS.Codes.CompetentAuthorityOfDispatch;
			competentOffice.CY_Data = "AuthOfce";
			AssertEquals("AuthOfce", helper.CompetentAuthorityDispatchOfficeReferenceNumber);
		}

		public void TestComplementConsigneeMemberStateCode()
		{
			AssertEquals(string.Empty, helper.ComplementConsigneeMemberStateCode);
			emcsDeclaration.ZG_CCTMSA = Core.Constants.CountryCodes.Austria;
			AssertEquals(Core.Constants.CountryCodes.Austria, helper.ComplementConsigneeMemberStateCode);
		}

		public void TestComplementConsigneeSerialNumberOfCertificateOfExemption()
		{
			AssertEquals(string.Empty, helper.ComplementConsigneeSerialNumberOfCertificateOfExemption);
			emcsDeclaration.ZG_CertOfExemption = "COE";
			AssertEquals("COE", helper.ComplementConsigneeSerialNumberOfCertificateOfExemption);
		}

		public void TestDeferredSubmissionFlag()
		{
			emcsDeclaration.ZG_DeferredSubmission = "0";
			AssertEquals("0", helper.DeferredSubmissionFlag);
		}

		public void TestSubmissionMessageType()
		{
			emcsDeclaration.ZG_SubmissionType = ZString.Empty;
			AssertEquals(string.Empty, helper.SubmissionMessageType);
			emcsDeclaration.ZG_SubmissionType = EMCSSubmissionTypeList.Codes.StandardSubmission;
			AssertEquals(EMCSSubmissionTypeList.Codes.StandardSubmission, helper.SubmissionMessageType);
		}

		public void TestTransportArrangement()
		{
			emcsDeclaration.ZG_TransportArrangement = ZString.Empty;
			AssertEquals(string.Empty, helper.TransportArrangement);
			emcsDeclaration.ZG_TransportArrangement = EMCSTransportArrangementList.Codes.Consignor;
			AssertEquals(EMCSTransportArrangementList.Codes.Consignor, helper.TransportArrangement);
		}

		public void TestDispatchDateTime()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Empty is not valid", null, helper.DispatchDateTime);
				var dateAtOrigin = new ZDateTime(2018, 09, 09, 12, 12, 12);
				emcsDeclaration.JE_DateAtOrigin = dateAtOrigin;
				AssertEquals("Entered value is returned", dateAtOrigin, helper.DispatchDateTime);
			});
		}

		public void TestOriginType()
		{
			emcsDeclaration.ZG_OriginType = ZString.Empty;
			AssertEquals(string.Empty, helper.OriginType);
			emcsDeclaration.ZG_OriginType = EMCSOriginTypeList.Codes.TaxWarehouse;
			AssertEquals(EMCSOriginTypeList.Codes.TaxWarehouse, helper.OriginType);
		}

		public void TestLocalReferenceNumber()
		{
			emcsDeclaration.JE_OwnerRef = "B123456";
			AssertEquals("B123456", helper.LocalReferenceNumber);
		}

		public void TestImportSadNumbers_Null()
		{
			AssertEquals("IEnumerables don't return null", false, helper.ImportSadNumbers.Any());
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
			AssertContainsExactElementsInAnyOrder("Correct codes in collection", expectedSadNumbers, helper.ImportSadNumbers);
		}

		public void TestDeliveryPlaceCustomsOfficeReferenceNumber()
		{
			var officeCode = emcsDeclaration.CustomsOffices.AddNew();
			officeCode.CY_Code = OfficeCodes_EMCS.Codes.OfficeOfDelivery;
			officeCode.CY_Data = "DE01876";
			AssertEquals("DE01876", helper.DeliveryPlaceCustomsOfficeReferenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			emcsDeclaration = Factory.New<EMCSJobDeclaration>();
			helper = new Message815HeaderProviderHelper(emcsDeclaration);
		}
		EMCSJobDeclaration emcsDeclaration;
		Message815HeaderProviderHelper helper;
	}
}
