using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	[TestedType(typeof(IE825SplitDetailProvider))]
	sealed class IE825SplitDetailProviderTest : SplitProviderProviderAbstractTest<IE825SplitDetailProvider>
	{
		public void TestLocalReferenceNumber()
		{
			AssertEquals(nameof(SplitProvider.LocalReferenceNumber), "E00001001", SplitProvider.LocalReferenceNumber);
		}

		public void TestJourneyTime()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default value is correct", "H01", SplitProvider.JourneyTime);
				emcsDeclaration.JourneyTimeNumericPart = 11;
				emcsDeclaration.JourneyTimeFormatPart = JourneyTimeUnitList.Codes.Days;
				AssertEquals("Modified Value is Correct", "D11", SplitProvider.JourneyTime);
			});
		}

		public void TestChangedTransportArrangement()
		{
			emcsDeclaration.ZG_TransportArrangement = ZString.Empty;
			AssertEquals(nameof(SplitProvider.ChangedTransportArrangement), string.Empty, SplitProvider.ChangedTransportArrangement);
			emcsDeclaration.ZG_TransportArrangement = EMCSTransportArrangementList.Codes.OwnerOfGoods;
			AssertEquals(SplitProvider.ChangedTransportArrangement, EMCSTransportArrangementList.Codes.OwnerOfGoods, SplitProvider.ChangedTransportArrangement);
		}

		public void TestChangedTransportArrangementSpecified()
		{
			emcsDeclaration.ZG_TransportArrangement = ZString.Empty;
			AssertEquals(nameof(SplitProvider.ChangedTransportArrangementSpecified), false, SplitProvider.ChangedTransportArrangementSpecified);
			emcsDeclaration.ZG_TransportArrangement = EMCSTransportArrangementList.Codes.Consignor;
			AssertEquals(nameof(SplitProvider.ChangedTransportArrangementSpecified), true, SplitProvider.ChangedTransportArrangementSpecified);
		}

		public void TestDestinationChangedSplittingDestinationTypeCode()
		{
			CombineAssertions(() =>
			{
				emcsDeclaration.JE_MessageSubType = ZString.Empty;
				AssertEquals("No Error for empty", string.Empty, SplitProvider.DestinationChangedSplittingDestinationTypeCode);
				emcsDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationTaxWarehouse;
				AssertEquals("Returns Correct value", EMCSDestinationTypeList.Codes.DestinationTaxWarehouse, SplitProvider.DestinationChangedSplittingDestinationTypeCode);
			});
		}

		public void TestNewConsigneeTrader()
		{
			AssertNull(SplitProvider.NewConsigneeTrader);
			var organisation = GetPartyTraderExciseNumberOrg("TRD821");
			organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "43762894", Constants.CountryCodes.Portugal);
			emcsDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExport;
			emcsDeclaration.ImporterDocumentaryAddress.OrganisationPK = organisation.PK;
			var newConsigneeTrader = SplitProvider.NewConsigneeTrader;
			CombineAssertions(() =>
			{
				AssertEquals("TRD821", newConsigneeTrader.TraderId);
				AssertEquals("PT43762894", newConsigneeTrader.EoriNumber);
			});
		}

		public void TestDeliveryPlaceTrader()
		{
			AssertNull(nameof(SplitProvider.DeliveryPlaceTrader), SplitProvider.DeliveryPlaceTrader);
			emcsDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationRegisteredConsignee;
			emcsDeclaration.DestinationWarehouseDocumentaryAddress.OrganisationPK = GetPartyVatNumberOrg("UST244").PK;
			splitProvider = null;
			CombineAssertions(() =>
			{
				var deliveryPlaceTrader = SplitProvider.DeliveryPlaceTrader;
				AssertEquals(nameof(deliveryPlaceTrader.TraderId), "UST244", deliveryPlaceTrader.TraderId);
				//Cannot be added to the generic property cache test as setup requirements overlap with another property
				AssertSame("Cached", deliveryPlaceTrader, SplitProvider.DeliveryPlaceTrader);
			});
		}

		public void TestDeliveryPlaceCustomsOfficeReferenceNumber()
		{
			var officeCode = emcsDeclaration.CustomsOffices.AddNew();
			officeCode.CY_Code = OfficeCodes_EMCS.Codes.OfficeOfDelivery;
			officeCode.CY_Data = "GB01876";
			AssertEquals(nameof(SplitProvider.DeliveryPlaceCustomsOfficeReferenceNumber), "GB01876", SplitProvider.DeliveryPlaceCustomsOfficeReferenceNumber);
		}

		public void TestNewTransportArrangerTrader()
		{
			AssertNull(SplitProvider.NewTransportArrangerTrader);
			emcsDeclaration.CarrierAgentDocumentaryAddress.OrganisationPK = GetPartyVatNumberOrg("VAT903").PK;
			var newTransportArrangerTrader = SplitProvider.NewTransportArrangerTrader;
			AssertEquals(nameof(newTransportArrangerTrader.VatNumber), "VAT903", newTransportArrangerTrader.VatNumber);
		}

		public void TestNewTransporterTrader()
		{
			AssertNull(nameof(SplitProvider.NewTransporterTrader), SplitProvider.NewTransporterTrader);
			emcsDeclaration.TransporterDocumentaryAddress.OrganisationPK = GetPartyVatNumberOrg("VAT377").PK;
			var newTransporterTrader = SplitProvider.NewTransporterTrader;
			AssertEquals(nameof(newTransporterTrader.VatNumber), "VAT377", newTransporterTrader.VatNumber);
		}

		public void TestTransportDetails()
		{
			for (var i = 1; i < 6; i++)
			{
				emcsDeclaration.CusContainers.AddNew();
			}
			AssertEquals("5 records, contents is tested in the provider", 5, SplitProvider.TransportDetails.Count);
		}

		public void TestLines()
		{
			emcsInvoiceHeader.InvoiceLines.AddNew();
			emcsInvoiceHeader.InvoiceLines.AddNew();
			AssertEquals("Lines returned", 2, SplitProvider.Lines.Count);
		}

		protected override IE825SplitDetailProvider GetSplitProvider() => new IE825SplitDetailProvider(emcsInvoiceHeader);

		protected override IE825SplitDetailProvider GetProvider() => new IE825SplitDetailProvider(emcsInvoiceHeader);

		protected override void SetUp()
		{
			base.SetUp();
			emcsDeclaration.JE_DeclarationReference = "E00001001";
		}

		protected override IEnumerable<Expression<Func<IE825SplitDetailProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.NewConsigneeTrader;
			yield return x => x.DeliveryPlaceTrader;
			yield return x => x.NewTransportArrangerTrader;
			yield return x => x.NewTransporterTrader;
			yield return x => x.TransportDetails;
			yield return x => x.Lines;
		}
	}
}
