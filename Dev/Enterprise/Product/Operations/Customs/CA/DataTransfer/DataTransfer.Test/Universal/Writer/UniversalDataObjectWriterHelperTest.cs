using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Common.CA;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	partial class DeclarationDataObjectWriterTest
	{
		public void TestCustomReferenceCCNNumberWriter()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var vendorOrg = Factory.New<OrgHeader>();
			vendorOrg.OH_Code = "IANVENDOR";
			declaration.JE_OH_Supplier = vendorOrg.PK;

			var importerOrg = Factory.New<OrgHeader>();
			importerOrg.OH_Code = "IANIMPORTER";
			declaration.JE_OH_Importer = importerOrg.PK;

			declaration.JE_MessageSubType = B3EntryTypeList.Codes.Confirming;
			declaration.JE_MasterBill = "08112346541";
			declaration.JE_HouseBill = "734646544";
			declaration.JE_WarehouseReleaseDate = new ZDateTime(2015, 9, 28, 0, 33, 0);
			declaration.JE_DateOfFirstArrival = new ZDateTime(2015, 9, 29, 1, 30, 0);
			declaration.CA_EstReleaseDate = new ZDateTime(2015, 9, 30);
			declaration.JE_EntryAuthorisationDate = new ZDateTime(2015, 10, 1);
			declaration.TransactionNumber.AccountSecurityCode = "12345";
			declaration.TransactionNumber.SequentialNumber = "20000001";

			var header = declaration.Invoices.AddNew();
			var ccnH1 = header.CargoControlNumbersList.AddNew();
			ccnH1.J2_ReferenceNumber = "HCN111";
			var ccnH2 = header.CargoControlNumbersList.AddNew();
			ccnH2.J2_ReferenceNumber = "HCN222";

			declaration.AdditionalReferenceNumbers.AddNewIfNotExist(Common.CanadaAdditionalReferenceNumberTypes.Codes.CCN, "80367346464544");
			declaration.AdditionalReferenceNumbers.AddNewIfNotExist(Common.CanadaAdditionalReferenceNumberTypes.Codes.PCN, "801666665555");
			declaration.CargoControlNumbers.AddNew("80367346464544B");

			Factory.SaveForTesting();

			var declarationData = (Shipment)Enterprise.MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			AssertEquals(2, declarationData.CustomsReferenceCollection.Count);
			var ccn1 = declarationData.CustomsReferenceCollection[0];
			var ccn2 = declarationData.CustomsReferenceCollection[1];
			if (ccn1.Reference.Equals("80367346464544"))
			{
				AssertEquals("80367346464544", ccn1.Reference);
				AssertEquals("80367346464544B", ccn2.Reference);
			}
			else
			{
				AssertEquals("80367346464544", ccn2.Reference);
				AssertEquals("80367346464544B", ccn1.Reference);
			}

			var invoiceColl = declarationData.CommercialInfo.CommercialInvoiceCollection;
			AssertEquals(1, invoiceColl.Count);
			AssertEquals(2, invoiceColl[0].CustomsReferenceCollection.Count);

			if (invoiceColl[0].CustomsReferenceCollection[0].Reference.Equals("HCN111"))
			{
				AssertEquals("HCN111", invoiceColl[0].CustomsReferenceCollection[0].Reference);
				AssertEquals("HCN222", invoiceColl[0].CustomsReferenceCollection[1].Reference);
			}
			else
			{
				AssertEquals("HCN222", invoiceColl[0].CustomsReferenceCollection[0].Reference);
				AssertEquals("HCN111", invoiceColl[0].CustomsReferenceCollection[1].Reference);
			}
		}

		public void TestAdditionalAddInfoGroupCollectionSupport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var ccn0 = declaration.ReleaseStatuses.AddNew();
			ccn0.RL_CargoControlNumber = "12345678";
			Factory.SaveForTesting();

			var declarationData = (Shipment)Enterprise.MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);

			CombineAssertions(delegate
			{
				AssertEquals(1, declarationData.AddInfoGroupCollection.Count);

				var addInfoGroupData0 = declarationData.AddInfoGroupCollection[0];
				AssertEquals(CusAddInfoTypeAttribute.Codes.CACCN, addInfoGroupData0.Type.Code.Value);
				var subAddInfoGroupCollection0 = addInfoGroupData0.AddInfoGroupCollection;
				AssertEquals(1, subAddInfoGroupCollection0.Count);
				AssertEquals(3, subAddInfoGroupCollection0[0].AddInfoCollection.Count);
				AssertEquals("12345678", subAddInfoGroupCollection0[0].AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.CargoControlNumber.CCNumber));
				AssertEquals(ZString.Empty, subAddInfoGroupCollection0[0].AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.CargoControlNumber.BillType));
				AssertEquals(ZString.Empty, subAddInfoGroupCollection0[0].AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.CargoControlNumber.BillNumber));
			});
		}

		public void TestUpdateOrganizationAddressCollection()
		{
			var harvestingPartyOrg = CreateOrganisation("harvesting", "ABC#@1");
			var processorOrg = CreateOrganisation("processor", "ABC#@2");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.CA_DFOInd = Customs.Business.YesNoList.Codes.Yes;
			var dfoPGAHeader = invoiceLine.DFOPGAHeader;
			dfoPGAHeader.CA_OA_HarvestingParty = harvestingPartyOrg.MainAddress.PK;
			dfoPGAHeader.CA_OA_Processor = processorOrg.MainAddress.PK;

			Factory.SaveForTesting();

			var declarationData = (Shipment)Enterprise.MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			CombineAssertions(delegate
			{
				var organizationAddressCollection = declarationData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].AddInfoGroupCollection
					.FirstOrDefault(group => group.Type.Code.GetValueOrDefault() == CusAddInfoTypeAttribute.Codes.CADFOPGAHeader).OrganizationAddressCollection;
				AssertEquals(2, organizationAddressCollection.Count);
				AssertNotNull(organizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == Constants.AddressType.HarvestingParty && x.CompanyName.GetValueOrDefault() == harvestingPartyOrg.OH_FullName));
				AssertNotNull(organizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == Constants.AddressType.FoodProcessor && x.CompanyName.GetValueOrDefault() == processorOrg.OH_FullName));
			});
		}
	}
}
