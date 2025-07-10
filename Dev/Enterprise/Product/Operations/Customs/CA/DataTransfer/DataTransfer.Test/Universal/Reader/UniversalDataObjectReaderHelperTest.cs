using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Common.CA;
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
		public void TestAdditionalAddInfoGroupCollectionSupport()
		{
			var declarationDataObject = SetupDeclaration(CAJobMessageTypeList.Codes.Import, B3EntryTypeList.Codes.Confirming);
			declarationDataObject.SetAddInfoGroupCollection(() => new List<AddInfoGroup>()
			{
				new AddInfoGroup()
				{
					Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.CACCN },
					AddInfoCollection = new List<AddInfo>()
					{
						new AddInfo() { Key = "CCNInfoNumber", Value = "123456789" }
					},
					AddInfoGroupCollection = new List<AddInfoGroup>()
					{
						new AddInfoGroup()
						{
							Type = new CodeDescriptionPair() { Code = Constants.AddInfoKeys.CargoControlNumber.Type },
							AddInfoCollection = new List<AddInfo>()
							{
								new AddInfo() { Key = Constants.AddInfoKeys.CargoControlNumber.CCNumber, Value = "123456789" },
								new AddInfo() { Key = Constants.AddInfoKeys.CargoControlNumber.BillType, Value = ZString.Empty },
								new AddInfo() { Key = Constants.AddInfoKeys.CargoControlNumber.BillNumber, Value = ZString.Empty }
							}
						}
					}
				}
			});

			Factory.SaveForTesting();

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(delegate
			{
				var newDeclarationBO = new BusinessObjectFactory().Load<JobDeclaration>(declarationBO.PK);
				AssertEquals(1, newDeclarationBO.ReleaseStatuses.Count);

				var cargoControlNumber0 = newDeclarationBO.ReleaseStatuses[0];
				AssertEquals("123456789", cargoControlNumber0.RL_CargoControlNumber);
				AssertEquals(ZGuid.Empty, cargoControlNumber0.RL_Bill);
			});
		}

		public void TestGetNewAddInfoDataObjectReader()
		{
			var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));
			var dfoIndAddInfo = new AddInfo();
			dfoIndAddInfo.Key = "DFOInd";
			dfoIndAddInfo.Value = YesNoList.Codes.Yes;
			var dfoDataObject = new AddInfoGroup();
			dfoDataObject.Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.CADFOPGAHeader };
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
		}

		public void TestImportInvoiceLineExemptCode()
		{
			var taxType = new AddInfo();
			taxType.Key = "TaxType";
			taxType.Value = DutyAndTaxTypes.Codes.ADD;
			var exemptCode = new AddInfo();
			exemptCode.Key = "ExemptCode";
			exemptCode.Value = "52";
			var dutyAndTaxes = new AddInfoGroup();
			dutyAndTaxes.Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.CADutyAndTax };
			dutyAndTaxes.AddInfoCollection = new List<AddInfo>(new[]
			{
				exemptCode,
				taxType
			});
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
									HarmonisedCode = "3333333333",
									AddInfoGroupCollection = new List<AddInfoGroup>(new[]
									{
										dutyAndTaxes
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
			AssertEquals(1, invoiceLine.DutiesAndTaxes.Count);
			var duty = invoiceLine.DutiesAndTaxes[0];
			AssertEquals(DutyAndTaxTypes.Codes.ADD, duty.C1_TaxType);
			AssertEquals("52", duty.C1_ExemptCode);
		}
	}
}
