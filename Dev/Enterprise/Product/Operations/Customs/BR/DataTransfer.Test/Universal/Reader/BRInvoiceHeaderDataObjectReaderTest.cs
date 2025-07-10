using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BR.DataTransfer.Universal.Testing
{
	public class BRInvoiceHeaderDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestComplementaryExport()
		{
			#region Setup

			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			var declarationDataObject = new Shipment()
			{
				DataContext = dataContext,
				OwnerRef = "@#1&*2343#@",
				MessageType = new CodeDescriptionPair() { Code = BRJobMessageTypeList.Codes.Export },
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
									PartNo = "CAR1234",
									AddInfoCollection = new List<AddInfo>()
									{
										new AddInfo() { Key = "ComplementaryDescriptionExport", Value = "Test" },
									}
								}
							})))
						})
				}
			};

			Factory.SaveForTesting();

			#endregion Setup

			var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
			ProcessMessage(message, new ServiceTaskLogForTesting());
			var declaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_OwnerRef, "@#1&*2343#@"));

			AssertEquals("declaration.InvoiceLines.Count", 1, declaration.InvoiceLines.Count);
			var invoiceLine = declaration.InvoiceLines[0];
			AssertNotNull("Excepted: invoiceline is not null", invoiceLine);
			AssertEquals("ComplementaryDescriptionExport", "Test", invoiceLine.ComplementaryDescription);
		}

		public void TestFillAddInfoGroupCollection()
		{
			#region Setup

			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			var declarationDataObject = new Shipment()
			{
				DataContext = dataContext,
				OwnerRef = "@#1&*2343#@",
				MessageType = new CodeDescriptionPair() { Code = BRJobMessageTypeList.Codes.Export },
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
									PartNo = "CAR1234",
									AddInfoCollection = new List<AddInfo>()
									{
										new AddInfo() { Key = "ComplementaryDescriptionExport", Value = "Test" },
									},
									AddInfoGroupCollection = CreateDrawbackSuspensionInfoGroup().ToList(),
								}
							})))
					})
				}
			};

			Factory.SaveForTesting();

			#endregion Setup

			var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
			ProcessMessage(message, new ServiceTaskLogForTesting());
			var declaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_OwnerRef, "@#1&*2343#@"));

			CombineAssertions(() =>
			{
				AssertEquals("declaration.InvoiceLines.Count", 1, declaration.InvoiceLines.Count);

				var invoiceLine = declaration.InvoiceLines[0];
				AssertNotNull("Excepted: invoiceline is not null", invoiceLine);
				AssertEquals("SuspensionDrawbackCollection should be", 1, invoiceLine.SuspensionDrawbackCollection?.Count);

				var suspensionDrawback = invoiceLine.SuspensionDrawbackCollection[0];
				AssertEquals("CSI_ReferenceNumber should be ", "123", suspensionDrawback.CSI_ReferenceNumber);
				AssertEquals("CSI_ReferenceNumber2 should be ", "123", suspensionDrawback.CSI_ReferenceNumber2);
				AssertEquals("CSI_LineNo should be ", 1, suspensionDrawback.CSI_LineNo);
				AssertEquals("CSI_Quantity should be ", 10m, suspensionDrawback.CSI_Quantity);
				AssertEquals("CSI_SubType should be ", "1", suspensionDrawback.CSI_SubType);
				AssertEquals("CSI_Tariff should be ", "2", suspensionDrawback.CSI_Tariff);
				AssertEquals("CSI_Value should be ", 3m, suspensionDrawback.CSI_Value);

				AssertEquals("SuspensionDrawbackInvoiceCollection should be ", 1, suspensionDrawback.SuspensionDrawbackInvoiceCollection?.Count);
				var suspensionDrawbackInvoice = suspensionDrawback.SuspensionDrawbackInvoiceCollection[0];
				AssertEquals("CSI_ReferenceNumber should be ", "123", suspensionDrawbackInvoice.CSI_ReferenceNumber);
				AssertEquals("CSI_Quantity should be ", 2m, suspensionDrawbackInvoice.CSI_Quantity);
				AssertEquals("CSI_Value should be ", 2m, suspensionDrawbackInvoice.CSI_Value);
				AssertEquals("CSI_DateOfIssue should be ", new ZDateTime(2023, 1, 2), suspensionDrawbackInvoice.CSI_DateOfIssue);

				AssertEquals("SuspensionDrawbackImportEntryDocumentCollection should be ", 1, suspensionDrawback.SuspensionDrawbackImportEntryDocumentCollection?.Count);
				var suspensionDrawbackImportEntryDocument = suspensionDrawback.SuspensionDrawbackImportEntryDocumentCollection[0];
				AssertEquals("CSI_ReferenceNumber should be ", "123", suspensionDrawbackImportEntryDocument.CSI_ReferenceNumber);
				AssertEquals("CSI_Quantity should be ", 2m, suspensionDrawbackImportEntryDocument.CSI_Quantity);
				AssertEquals("CSI_Value should be ", 20m, suspensionDrawbackImportEntryDocument.CSI_Value);
				AssertEquals("CSI_SubType should be ", "1", suspensionDrawbackImportEntryDocument.CSI_SubType);
				AssertEquals("CSI_LineNo should be ", 2, suspensionDrawbackImportEntryDocument.CSI_LineNo);
			});
		}

		public void TestFillCustomsReferenceCollection()
		{
			#region Setup

			eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			var declarationDataObject = new Shipment()
			{
				DataContext = dataContext,
				OwnerRef = "@#1&*2343#@",
				MessageType = new CodeDescriptionPair() { Code = BRJobMessageTypeList.Codes.Export },
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
									PartNo = "CAR1234",
									CustomsReferenceCollection = CreateCustomsReferenceCollection().ToList(),
								}
							})))
					})
				}
			};

			Factory.SaveForTesting();

			#endregion Setup

			var message = GetQueuedUniversalShipmentMessage(declarationDataObject);
			ProcessMessage(message, new ServiceTaskLogForTesting());
			var declaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_OwnerRef, "@#1&*2343#@"));

			CombineAssertions(() =>
			{
				AssertEquals("declaration.InvoiceLines.Count", 1, declaration.InvoiceLines.Count);

				var invoiceLine = declaration.InvoiceLines[0];
				AssertNotNull("Excepted: invoiceline is not null", invoiceLine);
				AssertEquals("LPCOJobComInvLineRefsCollection should be", 2, invoiceLine.LPCOJobComInvLineRefsCollection.Count);

				var lpco = invoiceLine.LPCOJobComInvLineRefsCollection[0];
				AssertEquals("JG_ReferenceNumber should be ", "1", lpco.JG_ReferenceNumber);
				var lpco1 = invoiceLine.LPCOJobComInvLineRefsCollection[1];
				AssertEquals("JG_ReferenceNumber should be ", "2", lpco1.JG_ReferenceNumber);
			});
		}

		IEnumerable<CustomsReference> CreateCustomsReferenceCollection()
		{
			yield return new CustomsReference()
			{
				Type = new CodeDescriptionPair { Code = Constants.JobComInvLineRefsType.Codes.LPCO },
				Reference = "1"
			};
			yield return new CustomsReference()
			{
				Type = new CodeDescriptionPair { Code = Constants.JobComInvLineRefsType.Codes.LPCO },
				Reference = "2"
			};
		}

		IEnumerable<AddInfoGroup> CreateDrawbackSuspensionInfoGroup()
		{
			yield return new AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = Constants.AddInfoKeys.AddInfoGroupTypeCodes.SuspensionDrawback, Description = CusSupportingInfoTypeList.Descriptions.SuspensionDrawback },
				AddInfoCollection = new List<AddInfo>(new[]
					{
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawback.CNPJBeneficiary, Value = "123" },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawback.CANumber, Value = "123" },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawback.CALineItemNumber, Value = "1" },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawback.QuantityUsed, Value =  "10" },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawback.CATypeOfConcessionAct, Value = "1" },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawback.TariffOfTheCAImportItem, Value = "2" },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawback.ForeignExchangeHedgedVMLE, Value = "3" },
					}),
				AddInfoGroupCollection = CreateAddInfoGroupCollectionForSuspensionDrawback().ToList()
			};
		}

		IEnumerable<AddInfoGroup> CreateAddInfoGroupCollectionForSuspensionDrawback()
		{
			yield return new AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = Constants.AddInfoKeys.AddInfoGroupTypeCodes.SuspensionDrawbackInvoice, Description = CusSupportingInfoTypeList.Descriptions.SuspensionDrawbackInvoice },
				AddInfoCollection = new List<AddInfo>(new[]
				{
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawbackInvoice.InvoiceNumber, Value = "123" },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawbackInvoice.Quantity, Value = "2" },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawbackInvoice.TradingCurrencyValue, Value = "2" },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawbackInvoice.Date, Value = "2023-01-02T00:00:00" }
					}),
			};
			yield return new AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = Constants.AddInfoKeys.AddInfoGroupTypeCodes.SuspensionDrawbackImportEntryDocument, Description = CusSupportingInfoTypeList.Descriptions.SuspensionDrawbackImportEntryDocument },
				AddInfoCollection = new List<AddInfo>(new[]
				{
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawbackImportEntryDocument.ImportEntry, Value = "123" },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawbackImportEntryDocument.Quantity, Value = "2" },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawbackImportEntryDocument.Value, Value = "20" },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawbackImportEntryDocument.Category, Value = "1" },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawbackImportEntryDocument.EntryLine, Value = "2" }
					}),
			};
		}

		public void TestCreateAdditionalLineTariffDetailDataObjectReader()
		{
			var logger = new TestErrorLogger();
			var currentCompanyHelper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Brazil, Core.Constants.CountryCodes.Brazil);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			var invoiceData = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			var reader = new BRInvoiceHeaderDataObjectReaderForTesting(declaration.TopGroupInvoice, invoiceData, logger, currentCompanyHelper);
			AssertType<BRAdditionalLineTariffDetailDataObjectReader>(reader.GetAdditionalLineTariffDetailDataObjectReader(new AdditionalLineTariffDetail(), logger, Factory, currentCompanyHelper, invoiceLine));
		}

		class BRInvoiceHeaderDataObjectReaderForTesting : BRInvoiceHeaderDataObjectReader
		{
			public BRInvoiceHeaderDataObjectReaderForTesting(BaseJobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, Shipment topLevelObject = null, ILandedCostDataReader landedCostDataReader = null) : base(groupHeader, invoiceDataObject, logger, helper, topLevelObject, landedCostDataReader)
			{
			}

			public AdditionalLineTariffDetailDataObjectReader GetAdditionalLineTariffDetailDataObjectReader(AdditionalLineTariffDetail dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, UniversalDataObjectReaderHelper helper, Customs.Business.IAdditionalLineTariffDetailParent parent)
			{
				return base.CreateAdditionalLineTariffDetailDataObjectReader(dataObject, logger, factory, helper, parent);
			}
		}
	}
}
