using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	partial class DeclarationDataObjectReaderTest
	{
		public void TestImportDFOOrganisations()
		{
			var harvestingPartyOrg = CreateOrganisation("harvesting", "ABC#@1");
			var processorOrg = CreateOrganisation("processor", "ABC#@2");

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var dfoIndAddInfo = new AddInfo();
			dfoIndAddInfo.Key = "DFOInd";
			dfoIndAddInfo.Value = YesNoList.Codes.Yes;
			var dfoDataObject = new AddInfoGroup(DefaultDataObjectWriterStrategy.TestInstance);
			dfoDataObject.Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.CADFOPGAHeader };
			dfoDataObject.AddOrgAddress(writeManager, harvestingPartyOrg, Constants.AddressType.HarvestingParty);
			dfoDataObject.AddOrgAddress(writeManager, processorOrg, Constants.AddressType.FoodProcessor);
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				OwnerRef = "OW234#$#",
				MessageType = new CodeDescriptionPair() { Code = "IMP" },

				CommercialInfo = new CommercialInfo()
				{
					Name = "GROUP",
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "INV23",
						}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
							new DataObjectList<CommercialInvoiceLine>(new[]
							{
								new CommercialInvoiceLine()
								{
									LineNo = 1,
									AddInfoCollection = new List<AddInfo>(new[]
									{
										dfoIndAddInfo
									}),
									AddInfoGroupCollection = new List<AddInfoGroup>(new[]
									{
										dfoDataObject
									})
								}
							})))
					})
				}
			};

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
			ProcessMessage(message, new ServiceTaskLogForTesting());
			var declaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_OwnerRef, "OW234#$#"));
			AssertEquals("declaration.InvoiceLines.Count", 1, declaration.InvoiceLines.Count);
			var invoiceLine = declaration.InvoiceLines[0];

			var header = invoiceLine.DFOPGAHeader;
			AssertNotNull("DFOPGAHeader", header);
			AssertEquals(harvestingPartyOrg.MainAddress.PK, header.CA_OA_HarvestingParty);
			AssertEquals(processorOrg.MainAddress.PK, header.CA_OA_Processor);
		}
	}
}
