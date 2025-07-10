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
		public void TestECCCManufacturer()
		{
			var machineManufacturer = CreateOrganisation("machineManufacturer", "ABC#@1");

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var ecccIndAddInfo = new AddInfo();
			ecccIndAddInfo.Key = "ECCCInd";
			ecccIndAddInfo.Value = YesNoList.Codes.Yes;
			var ecccDataObject = new AddInfoGroup(DefaultDataObjectWriterStrategy.TestInstance);
			ecccDataObject.Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.CAECCCPGAHeader };
			ecccDataObject.AddOrgAddress(writeManager, machineManufacturer, Constants.AddressType.ECCCMachineManufacturer);
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
										ecccIndAddInfo
									}),
									AddInfoGroupCollection = new List<AddInfoGroup>(new[]
									{
										ecccDataObject
									}),
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

			var header = invoiceLine.ECCCPGAHeader;
			AssertNotNull("ECCCPGAHeader", header);
			AssertEquals(machineManufacturer.MainAddress.PK, header.CA_MachineManufacturer);
		}

		public void TestECCCEngineProcessCodeXE02()
		{
			var engineLocation = CreateOrganisation("EngineLoc", "EGL#@1");
			var evidenceOfConfirmityLocation = CreateOrganisation("EvidenceLoc", "ECL#$2");

			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var ecccIndAddInfo = new AddInfo();
			ecccIndAddInfo.Key = "ECCCInd";
			ecccIndAddInfo.Value = YesNoList.Codes.Yes;

			var modelAddInfo = new AddInfo { Key = "ModelOfEngine", Value = "ENGINEMODEL" };
			var processCodeAddInfo = new AddInfo { Key = "ProcessCode", Value = "XE02" };
			var replacementEnginesAddInfo = new AddInfo { Key = "ReplacementEngines", Value = "Y" };
			var veeProgramIndAddInfo = new AddInfo { Key = "VEEProgramIndAddInfo", Value = "Y" };

			var ecccDataObject = new AddInfoGroup(DefaultDataObjectWriterStrategy.TestInstance);
			ecccDataObject.Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.CAECCCPGAHeader };
			ecccDataObject.AddOrgAddress(writeManager, engineLocation, Constants.AddressType.ECCCEngineLocation);
			ecccDataObject.AddOrgAddress(writeManager, evidenceOfConfirmityLocation, Constants.AddressType.ECCCEvidenceOfConfirmityLocation);
			ecccDataObject.AddInfoCollection = new List<AddInfo>(new[] { modelAddInfo, processCodeAddInfo, replacementEnginesAddInfo, veeProgramIndAddInfo });

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				OwnerRef = "ENGINE#$#",
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
										ecccIndAddInfo
									}),
									AddInfoGroupCollection = new List<AddInfoGroup>(new[]
									{
										ecccDataObject
									}),
								}
							})))
					})
				}
			};
			Factory.SaveForTesting();

			var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
			ProcessMessage(message, new ServiceTaskLogForTesting());
			var declaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_OwnerRef, "ENGINE#$#"));
			AssertEquals("declaration.InvoiceLines.Count", 1, declaration.InvoiceLines.Count);
			var invoiceLine = declaration.InvoiceLines[0];

			var header = invoiceLine.ECCCPGAHeader;
			AssertNotNull("ECCCPGAHeader", header);
			AssertEquals("XE02", header.CA_ProcessCode);
			AssertEquals(engineLocation.MainAddress.PK, header.CA_OA_EngineLocation);
			AssertEquals(evidenceOfConfirmityLocation.MainAddress.PK, header.CA_OA_EvidenceOfConformityLocation);
		}
	}
}
