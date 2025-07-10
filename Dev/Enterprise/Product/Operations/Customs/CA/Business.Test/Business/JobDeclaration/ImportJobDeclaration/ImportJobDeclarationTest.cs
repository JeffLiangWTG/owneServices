using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(ImportJobDeclaration))]
	sealed class ImportJobDeclarationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestShowShipmentRelatedField()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.SuppressShipmentRelatedFields = false;
			AssertEquals(true, declaration.ShowShipmentRelatedFields);

			declaration.SuppressShipmentRelatedFields = true;
			AssertEquals(false, declaration.ShowShipmentRelatedFields);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(true, declaration.ShowShipmentRelatedFields);

			declaration.SuppressShipmentRelatedFields = false;
			AssertEquals(true, declaration.ShowShipmentRelatedFields);
		}

		public void TestShowShipmentRelatedFieldsOrSea()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportModeCodeList.Codes.Sea;
			declaration.SuppressShipmentRelatedFields = false;
			AssertEquals("should true when sea", true, declaration.ShowShipmentRelatedFieldsOrSea);

			declaration.JE_TransportMode = TransportModeCodeList.Codes.Road;
			AssertEquals("should false when not sea", true, declaration.ShowShipmentRelatedFieldsOrSea);

			declaration.SuppressShipmentRelatedFields = true;
			AssertEquals("should true when field flag set up", false, declaration.ShowShipmentRelatedFieldsOrSea);
		}

		public void TestCreateDecFromOtherCountry()
		{
			GlbCompany.CurrentCompany.SetCountry("US");

			var otherCompany = Factory.New<GlbCompany>();
			otherCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			otherCompany.GC_Code = "XYZ";
			var newBranch = otherCompany.Branches.AddNew();
			newBranch.GB_RL_NKHomePort = "USCHI";
			newBranch.GB_Code = "QAZ";

			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_IsConsignee = true;
			importer.OH_RL_NKClosestPort = "CAYVR";

			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();
			supplier.OH_IsConsignor = true;
			supplier.OH_RL_NKClosestPort = "AUSYD";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "CAYVR";

			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = importer.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;
			shipment.JS_OuterPacks = 1;

			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_JS = shipment.PK;
			declaration.JE_DeclarationReference = shipment.JS_UniqueConsignRef;
			declaration.JE_GB = newBranch.PK;

			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry("CA");

			var factory3 = new BusinessObjectFactory();
			var decImporter = new ImportJobDeclaration(factory3);
			decImporter.LoadDeclarationForShipment(shipment.JS_UniqueConsignRef);
			var result = decImporter.CreateDeclarationAgainstShipment();

			AssertEquals("JE_MessageType should be IMP", "IMP", result.JE_MessageType);
			AssertEquals("Merge should be NON", "NON", result.JE_MergeBy);
			AssertEquals("JE_PaymentMethod should be DEF", PaymentPartyCodeDescriptionList.Codes.Default, result.JE_PaymentMethod);
		}
	}
}
