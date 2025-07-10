using System.Linq;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.CA.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	partial class DeclarationDataObjectWriterTest
	{
		public void TestExportDFOPGAHeaderOrganisations()
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

			invoiceLine.CA_NRCanInd = Customs.Business.YesNoList.Codes.Yes;
			var nrCanPGAHeader = invoiceLine.NRCanPGAHeader;
			var lpco = nrCanPGAHeader.LPCOViews.AddNew();
			lpco.CLP_ApplicantType = LPCOHolderPartyTypeCodes.Codes.Other;
			lpco.CLP_OA_Applicant = harvestingPartyOrg.MainAddress.PK;

			var machineManufacturer = CreateOrganisation("machine", "ABC#@3");
			invoiceLine.CA_ECCCInd = Customs.Business.YesNoList.Codes.Yes;
			var ecccPGAHeader = invoiceLine.ECCCPGAHeader;
			invoiceLine.ECCCPGAHeader.CA_MachineManufacturer = machineManufacturer.MainAddress.PK;

			Factory.SaveForTesting();

			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			CombineAssertions(delegate
			{
				var organizationAddressCollection = declarationData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].AddInfoGroupCollection
					.FirstOrDefault(group => group.Type.Code.GetValueOrDefault() == CusAddInfoTypeAttribute.Codes.CADFOPGAHeader).OrganizationAddressCollection;
				AssertEquals(2, organizationAddressCollection.Count);
				AssertNotNull(organizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == Constants.AddressType.HarvestingParty && x.CompanyName.GetValueOrDefault() == harvestingPartyOrg.OH_FullName));
				AssertNotNull(organizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == Constants.AddressType.FoodProcessor && x.CompanyName.GetValueOrDefault() == processorOrg.OH_FullName));

				var nrCANGroup = declarationData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].AddInfoGroupCollection.FirstOrDefault(group => group.Type.Code.GetValueOrDefault() == CusAddInfoTypeAttribute.Codes.CANRCanPGAHeader);
				organizationAddressCollection = nrCANGroup.AddInfoGroupCollection.FirstOrDefault(group => group.Type.Code.GetValueOrDefault() == Constants.AddInfoKeys.CusCALPCO.CusAddInfoType).OrganizationAddressCollection;
				AssertEquals(1, organizationAddressCollection.Count);
				AssertNotNull(organizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == Constants.AddressType.LPCOApplicant && x.CompanyName.GetValueOrDefault() == harvestingPartyOrg.OH_FullName));

				var ecccGroup = declarationData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].AddInfoGroupCollection.FirstOrDefault(group => group.Type.Code.GetValueOrDefault() == CusAddInfoTypeAttribute.Codes.CAECCCPGAHeader);
				var organizationAddress = ecccGroup.OrganizationAddressCollection.FirstOrDefault(group => group.AddressType.ToString() == Constants.AddressType.ECCCMachineManufacturer);
				AssertNotNull(organizationAddress);
				AssertEquals(organizationAddress.CompanyName, machineManufacturer.OH_FullName);
			});
		}

		public void TestECCCPGAHeaderOrganisations()
		{
			var engineLocation = CreateOrganisation("EGNLOC#", "EE#@1");
			var evidenceOfConfirmityLocation = CreateOrganisation("EVCOMLOC@", "CC#@2");
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)invoice.InvoiceLines.AddNew();
			invoiceLine.CA_ECCCInd = Customs.Business.YesNoList.Codes.Yes;
			var eccPGAHeader = invoiceLine.ECCCPGAHeader;
			eccPGAHeader.CA_OA_EngineLocation = engineLocation.MainAddress.PK;
			eccPGAHeader.CA_OA_EvidenceOfConformityLocation = evidenceOfConfirmityLocation.MainAddress.PK;

			Factory.SaveForTesting();

			var declarationData = (Shipment)MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			CombineAssertions(delegate
			{
				var organizationAddressCollection = declarationData.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].AddInfoGroupCollection
					.FirstOrDefault(group => group.Type.Code.GetValueOrDefault() == CusAddInfoTypeAttribute.Codes.CAECCCPGAHeader).OrganizationAddressCollection;
				AssertEquals(2, organizationAddressCollection.Count);
				AssertNotNull(organizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == Constants.AddressType.ECCCEngineLocation && x.CompanyName.GetValueOrDefault() == engineLocation.OH_FullName));
				AssertNotNull(organizationAddressCollection.FirstOrDefault(x => x.AddressType.GetValueOrDefault() == Constants.AddressType.ECCCEvidenceOfConfirmityLocation && x.CompanyName.GetValueOrDefault() == evidenceOfConfirmityLocation.OH_FullName));
			});
		}
	}
}
