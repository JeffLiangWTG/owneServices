using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class DUAImportDeliveryConditionsWrapperTest : WrapperHelperTest<DUAImportDeliveryConditionsWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Null CusEntryHeader", () => new DUAImportDeliveryConditionsWrapper(null));
		}

		public void TestCode()
		{
			CombineAssertions(() =>
			{
				declaration.JE_ShipmentIncoTerm = HeaderData.TermsOfDeliveryDeclarationCode;
				AssertEquals("Expected filled Code with JE_ShipmentIncoTerm", HeaderData.TermsOfDeliveryDeclarationCode, wrapper.Code);

				invoiceHeader.JZ_IncoTerm = HeaderData.TermsOfDeliveryCode;
				AssertEquals("Expected filled Code with JZ_IncoTerm (even when JE_ShipmentIncoTerm is declared)", HeaderData.TermsOfDeliveryCode, wrapper.Code);
			});
		}

		public void TestPlace()
		{
			CombineAssertions(() =>
			{
				declaration.JE_ShipmentIncoTermPlace = HeaderData.DeliveryLocationDeclaration;
				AssertEquals("Expected filled Place with JE_ShipmentIncoTermPlace", HeaderData.DeliveryLocationDeclaration, wrapper.Place);

				invoiceHeader.JZ_IncoTermPlace = HeaderData.DeliveryLocation;
				AssertEquals("Expected filled Place with JZ_IncoTermPlace (even when JE_ShipmentIncoTermPlace is declared)", HeaderData.DeliveryLocation, wrapper.Place);
			});
		}

		public void TestZoneIndicator()
		{
			declaration.ZG_AgreedPlaceCode = HeaderData.LocationId;
			AssertEquals("Expected filled ZoneIndicator", HeaderData.LocationId, wrapper.ZoneIndicator);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.InvoiceLines.AddNew();

			AssertEquals("Merge done", true, declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer()));

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = new DUAImportDeliveryConditionsWrapper(entryHeader);
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoiceHeader;
		CusEntryHeader entryHeader;
		DUAImportDeliveryConditionsWrapper wrapper;

		protected override DUAImportDeliveryConditionsWrapper GetProvider() => wrapper;
	}
}
