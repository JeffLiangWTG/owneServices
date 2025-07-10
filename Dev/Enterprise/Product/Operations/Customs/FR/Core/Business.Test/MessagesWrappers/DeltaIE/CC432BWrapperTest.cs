using System.Collections.ObjectModel;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	sealed class CC432BWrapperTest : Customs.Business.Testing.DataProviderTestCase<CC432BWrapper>
	{
		public void TestCustomsOfficeOfPresentation()
		{
			var customsOfficeOfPresentation = Provider.CustomsOfficeOfPresentation;
			CombineAssertions("CC432BWrapper.CustomsOfficeOfPresentation", () =>
			{
				AssertType<CustomsOfficeOfPresentationWrapper>("CustomsOfficeOfPresentation should be of type CustomsOfficeOfPresentationWrapper", customsOfficeOfPresentation);
				AssertEquals("CustomsOfficeOfPresentation should be captured from declration.JE_CustomsOffice", "XD", customsOfficeOfPresentation.ReferenceNumber);
			});
		}

		public void TestDeclarant()
		{
			var declarant = Provider.Declarant;
			CombineAssertions("CC432BWrapper.Declarant", () =>
			{
				AssertType<DeclarantWrapper>("Declarant should be of type DeclarantWrapper.", declarant);
				AssertEquals("Declarant.IdentificationNumber should be captured from the EORI of declarant.", "FR20230525", declarant.IdentificationNumber);
				AssertNull("Declarant.Name should be null when EORI is set for declarant.", declarant.Name);
				AssertNull("Declarant.Address shoudle be null when EORI is set for declarant.", declarant.Address);
				AssertEquals("Name", Provider.Declarant.ContactPerson.Name);
				AssertEquals("emailAddress", Provider.Declarant.ContactPerson.EMailAddress);
				AssertEquals("staffPhone", Provider.Declarant.ContactPerson.PhoneNumber);
			});
		}

		public void TestDeclarant_ShouldContainAddress_WhenNoEORI()
		{
			var alternativeProvider = GetAlternativeProvider_ThatDeclarantHasNoEORI();
			var declarant = alternativeProvider.Declarant;
			CombineAssertions("CC432BWrapper.Declarant without EORI", () =>
			{
				AssertEquals("Declarant.IdentificationNumber should be empty when no EORI exists for declarant.", string.Empty, declarant.IdentificationNumber);
				AssertEquals("Declarant.Name should be captured from the full name of declarant.", "SpongeBob", declarant.Name);
				OrganisationAddressWrapperTest.AssertOrganisationAddressWrapper(declarant.Address, Core.Constants.CountryCodes.France, "Pacific", "Bikini Bottom, BigPineapple", "23456");
			});

			CC432BWrapper GetAlternativeProvider_ThatDeclarantHasNoEORI()
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();

				var declarantOrg = CreateDeclarant();
				declarantOrg.CustomsCodes.RemoveAndDeleteAll();
				declaration.JE_OA_DeclarantAddress = declarantOrg.MainAddress.PK;

				var merger = new LineMerger(declaration);
				merger.DoMerge();
				var entryHeader = declaration.CustomsEntryHeaders[0];

				return CC432BWrapper.New(entryHeader);
			}
		}

		public void TestGoodsShipment()
		{
			var goodsShipment = Provider.GoodsShipment;
			CombineAssertions("CC432BWrapper.GoodsShipment", () =>
			{
				AssertType<GoodsShipmentWrapper>("GoodsShipment should be of type GoodsShipmentWrapper.", goodsShipment);
				AssertType<ConsignmentWrapper>("GoodsShipment.Consignment should be of type ConsignmentWrapper", goodsShipment.Consignment);
				AssertType<Collection<IGoodsShipmentPreviousDocument>>("GoodsShipment.PreviousDocument should be of type Collection<GoodsShipmentPreviousDocumentWrapper>", goodsShipment.PreviousDocument);
				AssertType<Collection<IGoodsShipmentItem>>("GoodsShipment.GoodsShipmentItem should be of type Collection<GoodsShipmentItemWrapper>", goodsShipment.GoodsShipmentItem);
			});
		}

		public void TestImportOperation()
		{
			var importOperation = Provider.ImportOperation;
			CombineAssertions("CC432BWrapper.ImportOperation", () =>
			{
				AssertType<CC432BCciOperationWrapper>("ImportOperation should be of type CC432BCciOperationWrapper.", importOperation);
				AssertEquals("ImportOperation.CustomsRegistrationNumber should be captured from entryHeader.CRN", "CRN #", importOperation.CustomsRegistrationNumber);
				AssertEquals("ImportOperation.LRN should be captured from entryHeader.CorrelationID", "LRN #", importOperation.LRN);
			});
		}

		public void TestRepresentative()
		{
			var representative = Provider.Representative;
			CombineAssertions("CC432BWrapper.Representative", () =>
			{
				AssertType<RepresentativeWrapper>("Representative should be of type RepresentativeWrapper.", representative);
				AssertEquals("Representative.Status should be captured from RepresentationTypeNo of the declaration.", "3", representative.Status);
				AssertEquals("IdentificationNumber should equal representative EORI number.", "FR98765432", representative.IdentificationNumber);
				AssertEquals("Name", Provider.Representative.ContactPerson.Name);
				AssertEquals("staffPhone", Provider.Representative.ContactPerson.PhoneNumber);
				AssertEquals("emailAddress", Provider.Representative.ContactPerson.EMailAddress);
			});
		}

		protected override CC432BWrapper GetProvider()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "CUS";
			staff.GS_FullName = "Name";
			staff.GS_WorkPhone = "staffPhone";
			var email = staff.EmailAddresses.AddNew();
			email.GSE_EmailAddress = "emailAddress";
			email.GSE_Type = Core.Constants.EmailFromAddressTypes.Codes.Main;
			Factory.Save();

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_GS_NKCusAgent = "CUS";

			declaration.JE_DeclarantType = RepresentationTypeList.Codes.IND;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
			declaration.JE_CustomsOffice = "XD";

			var representative = CreateRepresentative();
			declaration.JE_OA_Representative = representative.MainAddress.PK;

			var declarant = CreateDeclarant();
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.FillWithValidTestData();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.FillWithValidTestData();

			var merger = new LineMerger(declaration);
			merger.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.CorrelationID = "LRN #";
			entryHeader.CRN = "CRN #";
			Factory.Save();

			return CC432BWrapper.New(entryHeader);
		}

		OrgHeader CreateRepresentative()
		{
			var representative = Factory.NewWithValidTestData<OrgHeader>();
			representative.CustomsCodes.RemoveAndDeleteAll();
			representative.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "98765432", Core.Constants.CountryCodes.France);
			return representative;
		}

		OrgHeader CreateDeclarant()
		{
			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_FullName = "SpongeBob";
			declarant.CustomsCodes.RemoveAndDeleteAll();
			declarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "20230525", Core.Constants.CountryCodes.France);
			declarant.MainAddress.OA_Address1 = "Bikini Bottom";
			declarant.MainAddress.OA_Address2 = "BigPineapple";
			declarant.MainAddress.OA_PostCode = "23456";
			declarant.MainAddress.OA_City = "Pacific";
			declarant.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			return declarant;
		}
	}
}
