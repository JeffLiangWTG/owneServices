using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(ImportJobDeclaration))]
	class ImportJobDeclarationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestImportDataFromDeclarationOfAnotherCountry()
		{
			GlbCompany.CurrentCompany.SetCountry("TW");

			var twCompany = Factory.New<GlbCompany>();
			twCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			twCompany.GC_Code = "CTW";
			var twBranch = twCompany.Branches.AddNew();
			twBranch.GB_RL_NKHomePort = "TWTPE";
			twBranch.GB_Code = "BTP";

			var cnCompany = Factory.New<GlbCompany>();
			cnCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			cnCompany.GC_Code = "CCN";
			var cnBranch = twCompany.Branches.AddNew();
			cnBranch.GB_RL_NKHomePort = "CNSHA";
			cnBranch.GB_Code = "BSH";

			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();
			supplier.OH_IsConsignor = true;
			supplier.OH_RL_NKClosestPort = "TWTPE";
			var supCusCode = supplier.CustomsCodes.AddNew();
			supCusCode.OK_CodeType = "CCD";
			supCusCode.OK_RN_NKCodeCountry = "CN";
			supCusCode.OK_CustomsRegNo = "123456789";

			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			importer.OH_IsConsignee = true;
			importer.OH_RL_NKClosestPort = "CNSHA";
			var impCusCode = importer.CustomsCodes.AddNew();
			impCusCode.OK_CodeType = "CCD";
			impCusCode.OK_RN_NKCodeCountry = "CN";
			impCusCode.OK_CustomsRegNo = "987654321";

			var link = supplier.BuyerLinks.AddNew();
			link.OL_OH_Buyer = importer.PK;
			var linkMode = (OrgSupBuyLinkTrnMode)link.OrgSupBuyLinkTrnModes.First();
			((OrgSupBuyLinkTrnModeAddInfo)linkMode.AddInfo).ZO_CustomsOffice = "2200";
			link.OL_RelatedParty = "Y";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCustomsTariff("2009891200");
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "MANGO JUICE";
			var relatedOrganization = part.RelatedOrganisations.AddNew();
			relatedOrganization.OU_OH = importer.PK;
			relatedOrganization.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "2009891200";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "TWTPE";
			consol.JK_RL_NKDischargePort = "CNSHA";

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = supplier.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = importer.PK;

			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			declaration.JE_JS = shipment.PK;
			declaration.JE_GB = twBranch.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_TotalNoOfPacks = 1;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;

			var entry = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			entry.CH_CEI_Instruction = instruction.PK;
			invoiceLine.JI_CEI = instruction.PK;

			Factory.Save();

			GlbCompany.CurrentCompany.SetCountry("CN");

			var newFactory = new BusinessObjectFactory();
			var decImporter = new ImportJobDeclaration(newFactory);
			decImporter.LoadDeclarationForShipment(shipment.JS_UniqueConsignRef);
			var resultDec = decImporter.CreateDeclarationAgainstShipment() as JobDeclaration;
			var resultInv = resultDec.Invoices.First() as JobComInvoiceHeader;
			var resultInvLine = resultInv.JobComInvoiceLines.First() as JobComInvoiceLine;
			var firstInstruction = resultDec.CustomsEntryInstructions.First<CusEntryInstruction>();
			var secondInstruction = resultDec.CustomsEntryInstructions.Skip<CusEntryInstruction>(1).First();

			CombineAssertions(() =>
			{
				AssertEquals("Test DefaultOriginDistrictIfNeeded", "", resultInvLine.JI_OriginDistrict);
				AssertEquals("Test DefaultDestinationDistrictIfNeeded", "98765", resultInvLine.JI_DestinationDistrict);
				AssertEquals("Test DefaultCountryOfTrade", "TW", resultDec.JE_RN_NKCountryOfTrade);
				AssertEquals("Test DefaultBillOfLadingOnEntryInstructions", "", firstInstruction.BillOfLading);
				AssertEquals("Test UpdateXC_CNTransportMode", "", resultDec.JE_CNTransportMode);
				AssertEquals("Test OriginDefaulter.DefaultPort", "TWN000", resultDec.JE_CNPortOfOrigin);
				AssertEquals("Test FinalDestinationDefaulter.DefaultPort", "CHN000", resultDec.JE_CNPortOfDestination);
				AssertEquals("Test DefaultLastPortBeforeEntryIfNeeded", "TWN000", resultDec.JE_CNLastPortBeforeEntry);
				AssertEquals("Test DefaultValuesFromSupplierImporterLinkTransportMode", "2200", resultDec.JE_CustomsOffice);
				AssertEquals("Test SetDefaultsForInstruction", 1, firstInstruction.CEI_Packages);
				AssertEquals("Test SetDefaultsForInstruction", "22", secondInstruction.CEI_PackageUQ);
				AssertEquals("Test SetDefaultsForInvoiceHeader", "1", resultInv.JZ_SpecialRelationshipConfirm);
				AssertEquals("Test DefaultPreferenceIfNeeded", "MFN", resultInvLine.JI_PrimaryPreference);
				AssertEquals("Test PartSyncManager.Refresh", "2009891200", resultInvLine.JI_Tariff);
			});
		}
	}
}
