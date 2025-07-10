using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	partial class CombinedDataObjectReaderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestImportRelatedIndicatorWhenValueIsYes()
		{
			AssertRelatedIndicatorImportProcess(relatedIndicator: new CodeDescriptionPair() { Code = "Y", Description = "Yes" }, expectedOutputJZ_RelatedIndicator: "Y");
		}

		public void TestImportRelatedIndicatorWhenValueIsNo()
		{
			AssertRelatedIndicatorImportProcess(relatedIndicator: new CodeDescriptionPair() { Code = "N", Description = "No" }, expectedOutputJZ_RelatedIndicator: "N");
		}

		public void TestImportRelatedIndicatorWithAnyOtherValue()
		{
			AssertRelatedIndicatorImportProcess(relatedIndicator: new CodeDescriptionPair() { Code = "D", Description = "Departure" }, expectedOutputJZ_RelatedIndicator: "N");
		}

		public void TestImportRelatedIndicatorWithNullValue()
		{
			AssertRelatedIndicatorImportProcess(relatedIndicator: null, expectedOutputJZ_RelatedIndicator: "N");
		}

		void AssertRelatedIndicatorImportProcess(CodeDescriptionPair relatedIndicator, ZString expectedOutputJZ_RelatedIndicator)
		{
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, customsInterface))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
				var universalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					CommercialInfo = new CommercialInfo()
					{
						CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>()
					{
						new CommercialInvoiceHeader
						{
							RelatedIndicator  = relatedIndicator
						}
					}
					}
				};

				var universalMessage = GetQueuedUniversalShipmentMessage(universalShipment);
				new UniversalMessageProcessingManager(new ServiceTaskLogForTesting()).Process(universalMessage);
				AssertEquals("UXML message status", EDIMessageStatusList.Codes.ProcessedOK, universalMessage.EM_Status);

				var generatedDeclaration = new JobDeclarationDataObjectReader(universalShipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject();
				AssertEquals("UXML generated invoice JZ_RelatedIndicator", expectedOutputJZ_RelatedIndicator, generatedDeclaration.Invoices[0].JZ_RelatedIndicator);
			}
		}

		public void TestImportSupplyChainActors()
		{
			var existingDeclaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(existingDeclaration, "IsUCC6Core", true))
			{
				AssertEquals("OrganizationsSupport", true, existingDeclaration.Configuration.InvoiceLineConfiguration.OrganizationsSupport(existingDeclaration));
				existingDeclaration.JE_MasterBill = "MB123";
				var existingInvoice = existingDeclaration.Invoices.AddNew();
				existingInvoice.JZ_InvoiceNumber = "INV123ABC";
				var existingInvoiceline = existingInvoice.InvoiceLines.AddNew();
				existingInvoiceline.JI_MatchingKey = "INVLINE123ABC";
				var existingActor1 = existingInvoiceline.CusSupplyChainActorReferences.AddNew();
				existingActor1.CFR_Code = "CS";
				existingActor1.CFR_Reference = "REF123";
				existingActor1.CFR_OA_Owner = Org3.MainAddress.PK;

				var existingActor2 = existingInvoiceline.CusSupplyChainActorReferences.AddNew();
				existingActor2.CFR_Code = "MF";
				existingActor2.CFR_Reference = "REF657";
				existingActor2.CFR_OA_Owner = Org2.MainAddress.PK;

				var existingActor3 = existingInvoiceline.CusSupplyChainActorReferences.AddNew();
				existingActor3.CFR_Code = "WH";
				existingActor3.CFR_Reference = "ZZZ999";
				existingActor3.CFR_OA_Owner = Org1.MainAddress.PK;

				Factory.SaveForTesting();

				var shipment = CreateShipment(existingDeclaration.JE_DeclarationReference);
				shipment.CommercialInfo = new CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
					{
					new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
					{
						InvoiceNumber = "INV123ABC",
					}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
						new DataObjectList<CommercialInvoiceLine>(new[]
						{
							new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
							{
								DataImportMatchingKey = "INVLINE123ABC",
								CustomsReferenceCollection = CreateSupplyChainActors(),
							}
						})
						{
							Content = CollectionContent.Partial,
						}))
				})
					{
						Content = CollectionContent.Partial,
					}
				};

				CombineAssertions(() =>
				{
					var declaration = GetDeclarationFromShipment(shipment);
					AssertSame(declaration, existingDeclaration);
					AssertEquals("Invoice Header Count", 1, declaration.Invoices.Count);
					var invoice = declaration.Invoices[0];
					AssertSame(existingInvoice, invoice);
					AssertEquals("Invoice Line Count", 1, invoice.InvoiceLines.Count);
					var invoiceLine = invoice.InvoiceLines[0];
					AssertSame(existingInvoiceline, invoiceLine);
					AssertEquals("Supply chain actor count", 2, invoiceLine.CusSupplyChainActorReferences.Count);
					var actor1 = invoiceLine.CusSupplyChainActorReferences[0];
					var actor2 = invoiceLine.CusSupplyChainActorReferences[1];
					if (existingActor1.CFR_Reference != "REF123")
					{
						actor1 = invoiceLine.CusSupplyChainActorReferences[1];
						actor2 = invoiceLine.CusSupplyChainActorReferences[0];
					}
					AssertSame(existingActor1, actor1);
					AssertSame(existingActor2, actor2);
					AssertEquals("existingActor3.IsDeleted - not matched based on CFR_Code", true, existingActor3.IsDeleted);

					AssertEquals("Actor 1 Role", "CS", actor1.CFR_Code);
					AssertEquals("Actor 1 Reference", "REF123", actor1.CFR_Reference);
					AssertEquals("Actor 1 Owner", Org1.MainAddress.PK, actor1.CFR_OA_Owner);

					AssertEquals("Actor 2 Role", "MF", actor2.CFR_Code);
					AssertEquals("Actor 2 Reference", "REF987", actor2.CFR_Reference);
					AssertEquals("Actor 2 Owner", Org2.MainAddress.PK, actor2.CFR_OA_Owner);
				});
			}
		}

		public void TestDoNotImportSupplyChainActorsWhenNotSupported()
		{
			var existingDeclaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(existingDeclaration, "IsUCC6Core", false))
			{
				AssertEquals("OrganizationsSupport", false, existingDeclaration.Configuration.InvoiceLineConfiguration.OrganizationsSupport(existingDeclaration));
				existingDeclaration.JE_MasterBill = "MB123";
				var existingInvoice = existingDeclaration.Invoices.AddNew();
				existingInvoice.JZ_InvoiceNumber = "INV123ABC";
				var existingInvoiceline = existingInvoice.InvoiceLines.AddNew();
				existingInvoiceline.JI_MatchingKey = "INVLINE123ABC";
				var existingActor3 = existingInvoiceline.CusSupplyChainActorReferences.AddNew();
				existingActor3.CFR_Code = "WH";
				existingActor3.CFR_Reference = "ZZZ999";
				existingActor3.CFR_OA_Owner = Org1.MainAddress.PK;

				Factory.SaveForTesting();

				var shipment = CreateShipment(existingDeclaration.JE_DeclarationReference);
				shipment.CommercialInfo = new CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
					{
					new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
					{
						InvoiceNumber = "INV123ABC",
					}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
						new DataObjectList<CommercialInvoiceLine>(new[]
						{
							new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
							{
								DataImportMatchingKey = "INVLINE123ABC",
								CustomsReferenceCollection = CreateSupplyChainActors(),
							}
						})
						{
							Content = CollectionContent.Partial,
						}))
				})
					{
						Content = CollectionContent.Partial,
					}
				};

				CombineAssertions(() =>
				{
					var declaration = GetDeclarationFromShipment(shipment);
					AssertSame(declaration, existingDeclaration);
					AssertEquals("Invoice Header Count", 1, declaration.Invoices.Count);
					var invoice = declaration.Invoices[0];
					AssertSame(existingInvoice, invoice);
					AssertEquals("Invoice Line Count", 1, invoice.InvoiceLines.Count);
					var invoiceLine = invoice.InvoiceLines[0];
					AssertSame(existingInvoiceline, invoiceLine);
					AssertEquals("Supply chain actor count", 1, invoiceLine.CusSupplyChainActorReferences.Count);
					AssertSame(existingActor3, invoiceLine.CusSupplyChainActorReferences[0]);
					AssertEquals("should not be deleted when not supported", false, existingActor3.IsDeleted);
				});
			}
		}

		public void TestImportSupplyChainActors_CustomsReferenceCollectionWithEmptyData()
		{
			var existingDeclaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(existingDeclaration, "IsUCC6Core", true))
			{
				AssertEquals("OrganizationsSupport", true, existingDeclaration.Configuration.InvoiceLineConfiguration.OrganizationsSupport(existingDeclaration));
				existingDeclaration.JE_MasterBill = "MB123";
				var existingInvoice = existingDeclaration.Invoices.AddNew();
				existingInvoice.JZ_InvoiceNumber = "INV123ABC";
				var existingInvoiceline = existingInvoice.InvoiceLines.AddNew();
				existingInvoiceline.JI_MatchingKey = "INVLINE123ABC";
				var existingActor1 = existingInvoiceline.CusSupplyChainActorReferences.AddNew();
				existingActor1.CFR_Code = "CS";
				existingActor1.CFR_Reference = "REF123";
				existingActor1.CFR_OA_Owner = Org3.MainAddress.PK;

				Factory.SaveForTesting();

				var shipment = CreateShipment(existingDeclaration.JE_DeclarationReference);
				shipment.CommercialInfo = new CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
					{
					new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
					{
						InvoiceNumber = "INV123ABC",
					}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
						new DataObjectList<CommercialInvoiceLine>(new[]
						{
							new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
							{
								DataImportMatchingKey = "INVLINE123ABC",
								CustomsReferenceCollection = new List<CustomsReference>(new[] { new CustomsReference { Type = new CodeDescriptionPair() { Code = CusReferenceTypeList.Codes.SupplyChainActor } } }),
							}
						})
						{
							Content = CollectionContent.Partial,
						}))
				})
					{
						Content = CollectionContent.Partial,
					}
				};

				CombineAssertions(() =>
				{
					var declaration = GetDeclarationFromShipment(shipment);
					AssertSame(declaration, existingDeclaration);
					AssertEquals("Invoice Header Count", 1, declaration.Invoices.Count);
					var invoice = declaration.Invoices[0];
					AssertSame(existingInvoice, invoice);
					AssertEquals("Invoice Line Count", 1, invoice.InvoiceLines.Count);
					var invoiceLine = invoice.InvoiceLines[0];
					AssertSame(existingInvoiceline, invoiceLine);
					AssertEquals("Supply chain actor count", 0, invoiceLine.CusSupplyChainActorReferences.Count);
					AssertEquals("existingActor1.IsDeleted - not matched based on CFR_Code", true, existingActor1.IsDeleted);
				});
			}
		}

		public void TestImportSupplyChainActors_CustomsReferenceCollectionWithNoSupplyChainActor()
		{
			var existingDeclaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(existingDeclaration, "IsUCC6Core", true))
			{
				AssertEquals("OrganizationsSupport", true, existingDeclaration.Configuration.InvoiceLineConfiguration.OrganizationsSupport(existingDeclaration));
				existingDeclaration.JE_MasterBill = "MB123";
				var existingInvoice = existingDeclaration.Invoices.AddNew();
				existingInvoice.JZ_InvoiceNumber = "INV123ABC";
				var existingInvoiceline = existingInvoice.InvoiceLines.AddNew();
				existingInvoiceline.JI_MatchingKey = "INVLINE123ABC";
				var existingActor1 = existingInvoiceline.CusSupplyChainActorReferences.AddNew();
				existingActor1.CFR_Code = "CS";
				existingActor1.CFR_Reference = "REF123";
				existingActor1.CFR_OA_Owner = Org3.MainAddress.PK;

				Factory.SaveForTesting();

				var shipment = CreateShipment(existingDeclaration.JE_DeclarationReference);
				shipment.CommercialInfo = new CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
					{
					new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
					{
						InvoiceNumber = "INV123ABC",
					}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
						new DataObjectList<CommercialInvoiceLine>(new[]
						{
							new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance)
							{
								DataImportMatchingKey = "INVLINE123ABC",
								CustomsReferenceCollection = new List<CustomsReference>(new[] { new CustomsReference { Type = new CodeDescriptionPair() { Code = CusReferenceTypeList.Codes.FiscalReference }, Reference = "REF12" } }),
							}
						})
						{
							Content = CollectionContent.Partial,
						}))
				})
					{
						Content = CollectionContent.Partial,
					}
				};

				CombineAssertions(() =>
				{
					var declaration = GetDeclarationFromShipment(shipment);
					AssertSame(declaration, existingDeclaration);
					AssertEquals("Invoice Header Count", 1, declaration.Invoices.Count);
					var invoice = declaration.Invoices[0];
					AssertSame(existingInvoice, invoice);
					AssertEquals("Invoice Line Count", 1, invoice.InvoiceLines.Count);
					var invoiceLine = invoice.InvoiceLines[0];
					AssertSame(existingInvoiceline, invoiceLine);
					AssertEquals("Supply chain actor count", 1, invoiceLine.CusSupplyChainActorReferences.Count);
					AssertSame(existingActor1, invoiceLine.CusSupplyChainActorReferences[0]);
					AssertEquals("existingActor1.IsDeleted - should not touch since xml doesn't contain data", false, existingActor1.IsDeleted);

					AssertEquals("Actor 1 Role", "CS", existingActor1.CFR_Code);
					AssertEquals("Actor 1 Reference", "REF123", existingActor1.CFR_Reference);
					AssertEquals("Actor 1 Owner", Org3.MainAddress.PK, existingActor1.CFR_OA_Owner);
				});
			}
		}

		UniversalShipment CreateShipment(ZString? declarationReference = null)
		{
			base.SetUp();
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
			dataContext.CodesMappedToTarget = true;
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, declarationReference);
			return new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				WayBillNumber = "MB123",
				WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
			};
		}

		JobDeclaration GetDeclarationFromShipment(UniversalShipment shipment)
		{
			return new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject();
		}

		List<CustomsReference> CreateSupplyChainActors()
		{
			var result = new List<CustomsReference>();
			var actor1Address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			actor1Address.OrganizationCode = Org1.OH_Code;

			var actor1 = new CustomsReference();
			actor1.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			actor1.Type = new CodeDescriptionPair() { Code = CusReferenceTypeList.Codes.SupplyChainActor };
			actor1.SubType = new CodeDescriptionPair35Char() { Code = "CS" };
			actor1.Reference = "REF123";
			actor1.Owner = actor1Address;
			result.Add(actor1);

			var actor2Address = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			actor2Address.OrganizationCode = Org2.OH_Code;

			var actor2 = new CustomsReference();
			actor2.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			actor2.Type = new CodeDescriptionPair() { Code = CusReferenceTypeList.Codes.SupplyChainActor };
			actor2.SubType = new CodeDescriptionPair35Char() { Code = "MF" };
			actor2.Reference = "REF987";
			actor2.Owner = actor2Address;
			result.Add(actor2);

			return result;
		}

		OrgHeader Org1
		{
			get
			{
				if (org1 == null)
				{
					org1 = Factory.NewWithValidTestData<OrgHeader>();
					org1.OH_FullName = "Test Company 1";
					org1.OH_Code = "TESTCO1";
					var org1Address = org1.MainAddress;
					org1Address.FillWithValidTestData();
					org1Address.Address1 = "123 Test Street";
					org1Address.Postcode = "A12B3C4";
				}
				return org1;
			}
		}
		OrgHeader org1;

		OrgHeader Org2
		{
			get
			{
				if (org2 == null)
				{
					org2 = Factory.NewWithValidTestData<OrgHeader>();
					org2.OH_FullName = "Test Company 2";
					org2.OH_Code = "TESTCO2";
					var org2Address = org2.MainAddress;
					org2Address.FillWithValidTestData();
					org2Address.Address1 = "456 Test Street";
					org2Address.Postcode = "X98Y7Z6";
				}
				return org2;
			}
		}
		OrgHeader org2;

		OrgHeader Org3
		{
			get
			{
				if (org3 == null)
				{
					org3 = Factory.NewWithValidTestData<OrgHeader>();
					org3.OH_FullName = "Test Company 3";
					org3.OH_Code = "TESTCO3";
					var org3Address = org3.MainAddress;
					org3Address.FillWithValidTestData();
					org3Address.Address1 = "789 Test Street";
					org3Address.Postcode = "C4DE56";
				}
				return org3;
			}
		}
		OrgHeader org3;
	}
}
