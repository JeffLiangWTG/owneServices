using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business;
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
		public void TestImportBothOldAndNewAddInfoSchemaDataObjectReaderForHC()
		{
			//var harvestingPartyOrg = CreateOrganisation("harvesting", "ABC#@1");
			//var processorOrg = CreateOrganisation("processor", "ABC#@2");

			//var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var hcAddInfo = new AddInfo();
			hcAddInfo.Key = "HCInd";
			hcAddInfo.Value = YesNoList.Codes.Yes;
			var hcDataObject = new AddInfoGroup();
			hcDataObject.Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.CAHCPGAHeader, Description = "Health Canada PGA" };

			var addInfoMDEInd = new AddInfo { Key = "MDEProgramInd", Value = "Y" };
			var addInfoMDECategory = new AddInfo { Key = "CategoryMDE", Value = "DD33" };
			var addInfoMDEIntendedUseCode = new AddInfo { Key = "IntendedUseCodeMDE", Value = "II22" };
			var addInfoNHPInd = new AddInfo { Key = "NHPProgramInd", Value = "Y" };
			var addInfoNHPCategory = new AddInfo { Key = "CategoryNHP", Value = "PP33" };
			var addInfoNHPIntendedUseCode = new AddInfo { Key = "IntendedUseCodeNHP", Value = "HH22" };
			var addInfoCategory = new AddInfo { Key = "Category", Value = "OOLD" };
			var addInfoIntendedUseCode = new AddInfo { Key = "IntendedUseCode", Value = "IOLD" };

			var caHCCollection = new List<AddInfo>();
			caHCCollection.Add(addInfoMDEInd);
			caHCCollection.Add(addInfoNHPInd);
			caHCCollection.Add(addInfoMDECategory);
			caHCCollection.Add(addInfoMDEIntendedUseCode);
			caHCCollection.Add(addInfoNHPCategory);
			caHCCollection.Add(addInfoNHPIntendedUseCode);
			caHCCollection.Add(addInfoCategory);
			caHCCollection.Add(addInfoIntendedUseCode);
			hcDataObject.AddInfoCollection = caHCCollection;
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			var declarationDataObject = new Shipment()
			{
				DataContext = dataContext,
				OwnerRef = "OWNEROH",
				MessageType = new CodeDescriptionPair() { Code = "IMP" },

				CommercialInfo = new CommercialInfo()
				{
					Name = "GROUP",
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "IHD1",
						}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
							new DataObjectList<CommercialInvoiceLine>(new[]
							{
								new CommercialInvoiceLine()
								{
									LineNo = 1,
									AddInfoCollection = new List<AddInfo>(new [] { hcAddInfo }),
									AddInfoGroupCollection = new List<AddInfoGroup>(new[] { hcDataObject })
								}
							})))
					})
				}
			};

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
			ProcessMessage(message, new ServiceTaskLogForTesting());
			var declaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_OwnerRef, "OWNEROH"));
			AssertEquals("declaration.InvoiceLines.Count", 1, declaration.InvoiceLines.Count);
			var invoiceLine = declaration.InvoiceLines[0];
			var header = invoiceLine.HCPGAHeader;
			AssertNotNull("HCPGAHeader", header);
			AssertEquals("MDE Category DD33", "DD33", header.CA_CategoryMDE);
			AssertEquals("MDE IntendedUseCode II22", "II22", header.CA_IntendedUseCodeMDE);
			AssertEquals("NHP Category PP33", "PP33", header.CA_CategoryNHP);
			AssertEquals("NHP IntendedUseCode HH22", "HH22", header.CA_IntendedUseCodeNHP);
		}

		public void TestImportOldAddInfoSchemaForHC()
		{
			//var harvestingPartyOrg = CreateOrganisation("harvesting", "ABC#@1");
			//var processorOrg = CreateOrganisation("processor", "ABC#@2");

			//var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var hcAddInfo = new AddInfo();
			hcAddInfo.Key = "HCInd";
			hcAddInfo.Value = YesNoList.Codes.Yes;
			var hcDataObject = new AddInfoGroup();
			hcDataObject.Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.CAHCPGAHeader, Description = "Health Canada PGA" };

			var addInfoMDEInd = new AddInfo { Key = "MDEProgramInd", Value = "Y" };
			var addInfoNHPInd = new AddInfo { Key = "NHPProgramInd", Value = "Y" };
			var addInfoCategory = new AddInfo { Key = "Category", Value = "COLD" };
			var addInfoIntendedUseCode = new AddInfo { Key = "IntendedUseCode", Value = "IOLD" };

			var caHCCollection = new List<AddInfo>();
			caHCCollection.Add(addInfoMDEInd);
			caHCCollection.Add(addInfoNHPInd);
			caHCCollection.Add(addInfoCategory);
			caHCCollection.Add(addInfoIntendedUseCode);
			hcDataObject.AddInfoCollection = caHCCollection;
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			var declarationDataObject = new Shipment()
			{
				DataContext = dataContext,
				OwnerRef = "OWNEROH",
				MessageType = new CodeDescriptionPair() { Code = "IMP" },

				CommercialInfo = new CommercialInfo()
				{
					Name = "GROUP",
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "IHD1",
						}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
							new DataObjectList<CommercialInvoiceLine>(new[]
							{
								new CommercialInvoiceLine()
								{
									LineNo = 1,
									AddInfoCollection = new List<AddInfo>(new [] { hcAddInfo }),
									AddInfoGroupCollection = new List<AddInfoGroup>(new[] { hcDataObject })
								}
							})))
					})
				}
			};

			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
			ProcessMessage(message, new ServiceTaskLogForTesting());
			var declaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_OwnerRef, "OWNEROH"));
			AssertEquals("declaration.InvoiceLines.Count", 1, declaration.InvoiceLines.Count);
			var invoiceLine = declaration.InvoiceLines[0];
			var header = invoiceLine.HCPGAHeader;
			AssertNotNull("HCPGAHeader", header);
			AssertEquals("MDE Category COLD", "COLD", header.CA_CategoryMDE);
			AssertEquals("MDE IntendedUseCode IOLD", "IOLD", header.CA_IntendedUseCodeMDE);
			AssertEquals("NHP Category COLD", "COLD", header.CA_CategoryNHP);
			AssertEquals("NHP IntendedUseCode IOLD", "IOLD", header.CA_IntendedUseCodeNHP);
		}
	}
}
