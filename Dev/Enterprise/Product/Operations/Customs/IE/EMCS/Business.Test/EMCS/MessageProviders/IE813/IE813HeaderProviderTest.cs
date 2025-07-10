using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using CargoWise.Customs.IE.MessageContracts.EMCS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	[TestedType(typeof(IE813HeaderProvider))]
	class IE813HeaderProviderTest : HeaderProviderAbstractTest<IE813HeaderProvider>
	{
		protected override IE813HeaderProvider GetHeaderProvider() => new IE813HeaderProvider(emcsDeclaration);

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
				AssertEquals("No Error for empty", string.Empty, HeaderProvider.InvoiceNumber);
				emcsDeclaration.InvoiceNumber = "INV1234";
				AssertEquals("Returns Correct value", "INV1234", HeaderProvider.InvoiceNumber);
			});
		}

		public void TestInvoiceDate()
		{
			CombineAssertions(() =>
			{
				AssertEquals("No Error for empty", null, HeaderProvider.InvoiceDate);
				var invoiceDate = new ZDateTime(2018, 09, 09, 09, 09, 09);
				emcsDeclaration.InvoiceDate = invoiceDate;
				AssertEquals("Returns Correct value", invoiceDate, HeaderProvider.InvoiceDate);
			});
		}

		public void TestGuarantorType()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.ZG_GuarantorType = ZString.Empty;
				AssertEquals("No Error for empty", string.Empty, HeaderProvider.GuarantorType);
				emcsDeclaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.Consignor;
				AssertEquals("Returns Correct value", "1", HeaderProvider.GuarantorType);
			});
		}

		public void TestGuarantorTrader_Null()
		{
			emcsDeclaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.JointGuaranteeOfTheConsignorOfTheTransporterOfTheOwnerOfTheExciseProductsAndOfTheConsignee;
			AssertNull("Guarantor Trader is null", HeaderProvider.GuarantorTrader);
		}

		public void TestGuarantorTrader_Transporter()
		{
			(ZString exciseNumber, ZString vatNumber) = ("TEN251", "EXC251");

			organisation = GetPartyGuarantorOrg(exciseNumber, vatNumber);

			emcsDeclaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.Transporter;
			emcsDeclaration.TransporterDocumentaryAddress.OrganisationPK = organisation.PK;
			AssertEquals("TraderExciseNumber match for Guarantor Type Transporter", exciseNumber, HeaderProvider.GuarantorTrader.TraderExciseNumber);
			AssertEquals("VatNumber match for Guarantor Type Transporter", vatNumber, HeaderProvider.GuarantorTrader.VatNumber);
		}

		public void TestGuarantorTrader_Owner()
		{
			(ZString exciseNumber, ZString vatNumber) = ("TEN252", "EXC252");

			organisation = GetPartyGuarantorOrg(exciseNumber, vatNumber);

			emcsDeclaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.OwnerOfTheExciseProducts;
			emcsDeclaration.OwnerDocumentaryAddress.OrganisationPK = organisation.PK;
			AssertEquals("TraderExciseNumber match for Guarantor Type Owner", exciseNumber, HeaderProvider.GuarantorTrader.TraderExciseNumber);
			AssertEquals("VatNumber match for Guarantor Type Owner", vatNumber, HeaderProvider.GuarantorTrader.VatNumber);
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

		public void TestTransportDetails_Null()
		{
			AssertEquals("Collections doesn't return null but is empty", false, HeaderProvider.TransportDetails.Any());
		}

		public void TestTransportDetails()
		{
			for (var i = 1; i < 6; i++)
			{
				emcsDeclaration.CusContainers.AddNew();
			}
			AssertEquals("5 records, contents is tested in the provider", 5, HeaderProvider.TransportDetails.Count());
		}

		public void TestNewTransportArrangerTrader()
		{
			// only send the <NewTransportArrangerTrader> segment when ZG_TransportArrangement != 1(Consignor) and != 2 (Consignee).
			// ie. result must be null for Consignor or Consignee so message builder will not generate the element
			AssertNull(HeaderProvider.NewTransportArrangerTrader);

			emcsDeclaration.ZG_TransportArrangement = EMCSTransportArrangementList.Codes.Consignor;
			emcsDeclaration.CarrierAgentDocumentaryAddress.OrganisationPK = GetPartyVatNumberOrg("VAT903").PK;
			var newTransportArrangerTrader = HeaderProvider.NewTransportArrangerTrader;
			AssertNull("Consignor", HeaderProvider.NewTransportArrangerTrader);

			emcsDeclaration.ZG_TransportArrangement = EMCSTransportArrangementList.Codes.Consignee;
			newTransportArrangerTrader = HeaderProvider.NewTransportArrangerTrader;
			AssertNull("Consignee", HeaderProvider.NewTransportArrangerTrader);

			emcsDeclaration.ZG_TransportArrangement = EMCSTransportArrangementList.Codes.Other;
			newTransportArrangerTrader = HeaderProvider.NewTransportArrangerTrader;
			AssertEquals("Other", "VAT903", newTransportArrangerTrader.VatNumber);
		}

		public void TestNewTransporterTrader()
		{
			AssertNull(HeaderProvider.NewTransporterTrader);
			emcsDeclaration.TransporterDocumentaryAddress.OrganisationPK = GetPartyVatNumberOrg("VAT377").PK;
			var newTransporterTrader = HeaderProvider.NewTransporterTrader;
			AssertEquals("VAT377", newTransporterTrader.VatNumber);
		}

		public void TestNewConsigneeTraderNull()
		{
			AssertNull(HeaderProvider.NewConsigneeTrader);
		}

		public void TestNewConsigneeTrader()
		{
			var organisation = GetPartyTraderExciseNumberOrg("TRD821");
			organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "43762894", Core.Constants.CountryCodes.Greece);

			emcsDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExport;

			emcsDeclaration.ImporterDocumentaryAddress.OrganisationPK = organisation.PK;
			var newConsigneeTrader = HeaderProvider.NewConsigneeTrader;
			CombineAssertions(() =>
			{
				AssertEquals("TRD821", newConsigneeTrader.TraderId);
				AssertEquals("GR43762894", newConsigneeTrader.EoriNumber);
			});
		}

		public void TestNewConsigneeTrader_NotMapped()
		{
			emcsDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown;
			emcsDeclaration.ZG_SubmissionType = EMCSSubmissionTypeList.Codes.SubmissionForExport;
			AssertNull(HeaderProvider.NewConsigneeTrader);
		}

		public void TestNewConsigneeTrader_TraderId_NotMapped()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExemptedConsignee;
				AssertNull(HeaderProvider.NewConsigneeTrader);

				emcsDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown;
				AssertNull(HeaderProvider.NewConsigneeTrader);
			});
		}

		public void TestDeliveryPlaceTrader_Null()
		{
			AssertNull(HeaderProvider.DeliveryPlaceTrader);
		}

		public void TestDeliveryPlaceTrader()
		{
			emcsDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationRegisteredConsignee;
			emcsDeclaration.DestinationWarehouseDocumentaryAddress.OrganisationPK = GetPartyVatNumberOrg("UST244").PK;
			headerProvider = null;
			CombineAssertions(() =>
			{
				var deliveryPlaceTrader = HeaderProvider.DeliveryPlaceTrader;
				AssertEquals("UST244", deliveryPlaceTrader.TraderId);
				//Cannot be added to the generic property cache test as setup requirements overlap with another property
				AssertSame("Cached", deliveryPlaceTrader, HeaderProvider.DeliveryPlaceTrader);
			});
		}

		public void TestDeliveryPlaceTrader_NotMapped()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationRegisteredConsignee;
				AssertNull(HeaderProvider.DeliveryPlaceTrader);

				emcsDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown;
				AssertNull(HeaderProvider.DeliveryPlaceTrader);

				emcsDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExport;
				AssertNull(HeaderProvider.DeliveryPlaceTrader);
			});
		}

		public void TestComplementaryInformation()
		{
			emcsDeclaration.SpecialInstructions = "SPECIAL INSTRUCTIONS";
			AssertEquals("SPECIAL INSTRUCTIONS", HeaderProvider.ComplementaryInformation.Text);
		}

		public void TestChangedTransportArrangement()
		{
			emcsDeclaration.ZG_TransportArrangement = ZString.Empty;
			AssertEquals(string.Empty, HeaderProvider.ChangedTransportArrangement);
			emcsDeclaration.ZG_TransportArrangement = EMCSTransportArrangementList.Codes.OwnerOfGoods;
			AssertEquals(EMCSTransportArrangementList.Codes.OwnerOfGoods, HeaderProvider.ChangedTransportArrangement);
		}

		public void TestDeliveryPlaceCustomsOfficeReferenceNumber()
		{
			var officeCode = emcsDeclaration.CustomsOffices.AddNew();
			officeCode.CY_Code = OfficeCodes_EMCS.Codes.OfficeOfDelivery;
			officeCode.CY_Data = "DE01876";
			AssertEquals("DE01876", HeaderProvider.DeliveryPlaceCustomsOfficeReferenceNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();

			organisation = GetPartyTraderIdOrg("TID355");
			var address = organisation.MainAddress;
			address.OA_Language = Core.SharedConstants.Languages.Tamil;
			address.OA_City = "DARWIN";
			address.OA_Address1 = "12 MITCHELL ST";
			address.OA_PostCode = "0800";

			var orgAddress = Factory.New<JobDocAddress>();
			orgAddress.E2_OA_Address = address.PK;
		}
		OrgHeader organisation;

		protected override IE813HeaderProvider GetProvider()
		{
			emcsDeclaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.Transporter;
			emcsDeclaration.TransporterDocumentaryAddress.OrganisationPK = GetPartyGuarantorOrg("TEN251", "EXC251").PK;
			emcsDeclaration.CarrierAgentDocumentaryAddress.OrganisationPK = GetPartyVatNumberOrg("VAT903").PK;
			emcsDeclaration.TransporterDocumentaryAddress.OrganisationPK = GetPartyVatNumberOrg("VAT377").PK;
			emcsDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExport;
			var organisation = GetPartyTraderExciseNumberOrg("TRD821");
			organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "43762894", Core.Constants.CountryCodes.Greece);
			emcsDeclaration.ImporterDocumentaryAddress.OrganisationPK = organisation.PK;
			emcsDeclaration.SpecialInstructions = "SPECIAL INSTRUCTIONS";

			return new IE813HeaderProvider(emcsDeclaration);
		}

		protected override IEnumerable<Expression<System.Func<IE813HeaderProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.GuarantorTrader;
			yield return x => x.NewTransportArrangerTrader;
			yield return x => x.NewTransporterTrader;
			yield return x => x.NewConsigneeTrader;
			yield return x => x.ComplementaryInformation;
		}

		protected new IIE813Header HeaderProvider => base.HeaderProvider;
	}
}
