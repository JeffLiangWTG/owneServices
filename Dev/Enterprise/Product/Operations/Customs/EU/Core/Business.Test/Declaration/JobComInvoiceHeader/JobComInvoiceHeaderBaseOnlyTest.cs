using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.Metadata.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Resources;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceHeader))]
	sealed class JobComInvoiceHeaderBaseOnlyTest : JobComInvoiceHeaderTest<JobDeclaration, JobComInvoiceHeader, JobComInvoiceLine>
	{
		public void TestUpdateDefaultSupportingDocumentsForJZ_InvoiceNumber() => CombineAssertions(() =>
		{
			var header = Factory.New<JobDeclaration>().Invoices.AddNew();
			var document1 = header.SupportingDocuments.AddNew("N830", "1");
			header.JZ_InvoiceNumber = "123";
			AssertEquals("No updated as type not N380 and not N325", "1", document1.CSI_ReferenceNumber);
			var document2 = header.SupportingDocuments.AddNew("N380", "2");
			header.JZ_InvoiceNumber = "456";
			AssertEquals("No updated as type not N380 and not N325", "1", document1.CSI_ReferenceNumber);
			AssertEquals("Updated as type is only N380", "456", document2.CSI_ReferenceNumber);
			document2.CSI_Code = "N325";
			header.JZ_InvoiceNumber = "789";
			AssertEquals("Updated as type is only N325", "789", document2.CSI_ReferenceNumber);
			var document3 = header.SupportingDocuments.AddNew("N380", "3");
			header.JZ_InvoiceNumber = "111";
			AssertEquals("Not updated as both type N380 N325 used", "789", document2.CSI_ReferenceNumber);
			AssertEquals("Not updated as both type N380 N325 used", "3", document3.CSI_ReferenceNumber);
			document2.CSI_Code = "N380";
			header.JZ_InvoiceNumber = "222";
			AssertEquals("Not updated as type N380 happens twice", "789", document2.CSI_ReferenceNumber);
			AssertEquals("Not updated as type N380 happens twice", "3", document3.CSI_ReferenceNumber);
			document2.CSI_Code = "N325";
			document2.CSI_Code = "N325";
			header.JZ_InvoiceNumber = "222";
			AssertEquals("Not updated as type N325 happens twice", "789", document2.CSI_ReferenceNumber);
			AssertEquals("Not updated as type N325 happens twice", "3", document3.CSI_ReferenceNumber);
		});

		public void TestTypeDecider()
		{
			AssertType<JobComInvoiceHeaderTypeDecider>("TypeDecider", JobComInvoiceHeader.TypeDecider);
		}

		public void TestEffectiveValueManager()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			AssertNotNull("EffectiveValueManager should not be null", invoiceHeader.EffectiveValueManager);
		}

		public void TestIAdditionalInfosProvider()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			AssertSame(invoiceHeader.AdditionalInfos, ((IAdditionalInfosProvider)invoiceHeader).AdditionalInfos);
		}

		public void TestJZ_OH_Supplier_CaptionKeySAD()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_OH_SupplierInfo, JobDeclaration.CaptionKeySAD);
				AssertEquals("Caption", "Supplier", captionResourceString.Caption);
			});
		}

		public void TestJZ_OH_Supplier_CaptionKeyUCC()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_OH_SupplierInfo, JobDeclaration.CaptionKeyUCC);
				AssertEquals("Caption", "[3/1] Supplier", captionResourceString.Caption);
				AssertEquals("ShortCaption", "Supplier", captionResourceString.ShortCaption);
			});
		}

		public void TestJZ_InvoiceAmount_CaptionKeySAD()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_InvoiceAmountInfo, JobDeclaration.CaptionKeySAD);
				AssertEquals("Caption", "[22] Inv. Amount", captionResourceString.Caption);
				AssertEquals("FullDescription", "The total amount of the invoice and its currency.", captionResourceString.FullDescription);
			});
		}

		public void TestJZ_InvoiceAmount_CaptionKeyUCC()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_InvoiceAmountInfo, JobDeclaration.CaptionKeyUCC);
				AssertEquals("Caption", "[4/11] Invoice Amount", captionResourceString.Caption);
				AssertEquals("MediumCaption", "[4/11] Inv. Amount", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Inv. Amount", captionResourceString.ShortCaption);
			});
		}

		public void TestJZ_InvoiceAmount_CaptionKeyExportUCC6()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_InvoiceAmountInfo, JobDeclaration.CaptionKeyExportUCC6);
			AssertEquals("JZ_InvoiceAmount.Caption", "Invoice Amount", captionResourceString.Caption);
			AssertEquals("JZ_InvoiceAmount.MediumCaption", "Inv. Amount", captionResourceString.MediumCaption);
			AssertEquals("JZ_InvoiceAmount.ShortCaption", "Inv. Amount", captionResourceString.ShortCaption);
		}

		public void TestJZ_InvoiceAmount_CaptionKeyImportUCC6()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_InvoiceAmountInfo, JobDeclaration.CaptionKeyImportUCC6);
			AssertEquals("JZ_InvoiceAmount.Caption", "Invoice Amount", captionResourceString.Caption);
			AssertEquals("JZ_InvoiceAmount.MediumCaption", "Inv. Amount", captionResourceString.MediumCaption);
			AssertEquals("JZ_InvoiceAmount.ShortCaption", "Inv. Amount", captionResourceString.ShortCaption);
		}

		public void TestJZ_InvoiceCurrExRate_CaptionKeySAD()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_InvoiceCurrExRateInfo, JobDeclaration.CaptionKeySAD);
				AssertEquals("Caption", "Exchange Rate", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Exch. Rate", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Ex. Rate", captionResourceString.ShortCaption);
			});
		}

		public void TestJZ_InvoiceCurrExRate_CaptionKeyUCC()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_InvoiceCurrExRateInfo, JobDeclaration.CaptionKeyUCC);
				AssertEquals("Caption", "[4/15] Exchange Rate", captionResourceString.Caption);
				AssertEquals("MediumCaption", "[4/15] Exch. Rate", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Exch. Rate", captionResourceString.ShortCaption);
			});
		}

		public void TestJZ_InvoiceCurrExRate_CaptionKeyExportUCC6()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_InvoiceCurrExRateInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("Caption", "Exchange Rate", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Exch. Rate", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Ex. Rate", captionResourceString.ShortCaption);
				AssertEquals("FullDescription", "[14 09 000 000] Exchange Rate", captionResourceString.FullDescription);
			});
		}

		public void TestJZ_InvoiceCurrExRate_CaptionKeyImportUCC6()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_InvoiceCurrExRateInfo, JobDeclaration.CaptionKeyImportUCC6);
				AssertEquals("Caption", "Exchange Rate", captionResourceString.Caption);
				AssertEquals("MediumCaption", "Exch. Rate", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Ex. Rate", captionResourceString.ShortCaption);
				AssertEquals("FullDescription", "[14 09 000 000] Exchange Rate", captionResourceString.FullDescription);
			});
		}

		public void TestJZ_ValuationCode_CaptionSAD()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_ValuationCodeInfo, JobDeclaration.CaptionKeySAD);
				AssertEquals("Caption", "[24] Tran. Nature", captionResourceString.Caption);
				AssertEquals("FullDescription", "The nature of the transaction.", captionResourceString.FullDescription);
			});
		}

		public void TestJZ_ValuationCode_CaptionUCC()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_ValuationCodeInfo, JobDeclaration.CaptionKeyUCC);
				AssertEquals("Caption", "[8/5] Transaction Nature", captionResourceString.Caption);
				AssertEquals("MediumCaption", "[8/5] Tran. Nature", captionResourceString.MediumCaption);
				AssertEquals("ShortCaption", "Tran. Nature", captionResourceString.ShortCaption);
			});
		}

		public void TestJZ_IncoTerm_Caption()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_IncoTermInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("Incoterm", captionResourceString.Caption);
				AssertEquals("[14 01 035 000] Incoterm Code", captionResourceString.FullDescription);

				captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_IncoTermInfo, JobDeclaration.CaptionKeyImportUCC6);
				AssertEquals("Incoterm", captionResourceString.Caption);
				AssertEquals("[14 01 035 000] Incoterm Code", captionResourceString.FullDescription);
			});
		}

		public void TestJZ_RX_NKInvoice_Currency_Caption()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_RX_NKInvoice_CurrencyInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("Invoice Currency", captionResourceString.Caption);
				AssertEquals("Inv. Currency", captionResourceString.MediumCaption);
				AssertEquals("Currency", captionResourceString.ShortCaption);
				AssertEquals("[14 05 000 000] Invoice Currency", captionResourceString.FullDescription);

				captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_RX_NKInvoice_CurrencyInfo, JobDeclaration.CaptionKeyImportUCC6);
				AssertEquals("Invoice Currency", captionResourceString.Caption);
				AssertEquals("Inv. Currency", captionResourceString.MediumCaption);
				AssertEquals("Currency", captionResourceString.ShortCaption);
				AssertEquals("[14 05 000 000] Invoice Currency", captionResourceString.FullDescription);
			});
		}

		public void TestJZ_ValuationCode_CaptionImportUCC6()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_ValuationCodeInfo, JobDeclaration.CaptionKeyImportUCC6);
				AssertEquals("Caption", "Nature of Transaction", captionResourceString.Caption);
				AssertEquals("FullDescription", "[99 05 000 000]  Nature of transaction", captionResourceString.FullDescription);
			});
		}

		public void TestJZ_Weight_Caption_Default()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_WeightInfo, string.Empty);
			AssertEquals("Caption", "Inv. Gross Weight", captionResourceString.Caption);
		}

		public void TestJZ_JZ_Weight_CaptionImportUCC6()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_WeightInfo, JobDeclaration.CaptionKeyImportUCC6);
				AssertEquals("Caption", "Inv. Gross Weight", captionResourceString.Caption);
				AssertEquals("FullDescription", "[18 04 001 000]  Gross mass", captionResourceString.FullDescription);
			});
		}

		public void TestJZ_NetWeight_Caption_Default()
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_NetWeightInfo, string.Empty);
			AssertEquals("Caption", "Inv. Net Weight", captionResourceString.Caption);
		}

		public void TestJZ_NetWeight_CaptionImportUCC6()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_NetWeightInfo, JobDeclaration.CaptionKeyImportUCC6);
				AssertEquals("Caption", "Inv. Net Weight", captionResourceString.Caption);
				AssertEquals("FullDescription", "[18 01 001 000]  Invoice Net mass", captionResourceString.FullDescription);
			});
		}

		public void TestAddInfo_IsConnectedtoAutoClass()
		{
			AssertEquals(true, typeof(JobComInvoiceHeader).IsSubclassOf(typeof(AutoJobComInvoiceHeader)));
		}

		public void TestAddInfo_IsAutoGenerated()
		{
			AssertNotNull(typeof(AutoJobComInvoiceHeader).GetCustomAttribute<AutoGeneratedSourceCodeAttribute>());
		}

		public void TestAddInfo_HasUseAddInfoPropertyDescriptorsTrue()
		{
			AssertNotNull(typeof(AutoJobComInvoiceHeader).GetCustomAttribute<PropertyDescriptorCollectionAttribute>());
		}

		public void TestAddInfoType()
		{
			AssertEquals(typeof(AddInfoJobComInvoiceHeader), JobComInvoiceHeader.AddInfoType);
		}

		public void TestMaxSupportingDocuments()
		{
			AssertEquals("MaxSupportingDocuments default value", -1, invoice.MaxSupportingDocuments);
		}

		public void TestGetSupportingDocumentsMaxCountReduction()
		{
			AssertEquals("The functions returns 0", 0, invoice.GetSupportingDocumentsMaxCountReduction());
		}

		public void TestSupportingDocumentsValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportSupportingDocumentValidationDecider>(((ISupportingDocumentsProviderWithValidationDecider)invoice).ValidationDecider);
			}
		}

		public void TestAdditionalInfosValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportAdditionalInfoValidationDecider>(((IAdditionalInfosProviderWithValidationDecider)invoice).ValidationDecider);
			}
		}

		public void TestPreviousDocumentValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportPreviousDocumentValidationDecider>(((IPreviousDocumentsProviderWithValidationDecider)invoice).ValidationDecider);
			}
		}

		public void TestAddingSupportingDocumentAutomaticallyEnabled()
		{
			AssertEquals("AddingSupportingDocumentAutomaticallyEnabled", ZBool.True, invoice.AddingSupportingDocumentAutomaticallyEnabled);
		}

		public void TestNeedAtLeastOneInvoiceSupportingDocument_Import()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			var supportingDocument = invoice.SupportingDocuments.AddNew();

			CombineAssertions(() =>
			{
				supportingDocument.CSI_Code = "AAA";
				AssertEquals("AAA", ZBool.True, invoice.NeedAtLeastOneInvoiceSupportingDocument);

				supportingDocument.CSI_Code = "N380";
				AssertEquals("N380", ZBool.False, invoice.NeedAtLeastOneInvoiceSupportingDocument);

				supportingDocument.CSI_Code = "D008";
				AssertEquals("D008", ZBool.False, invoice.NeedAtLeastOneInvoiceSupportingDocument);
			});
		}

		public void TestNeedAtLeastOneInvoiceSupportingDocument_Export()
		{
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			var supportingDocument = invoice.SupportingDocuments.AddNew();

			CombineAssertions(() =>
			{
				supportingDocument.CSI_Code = "D008";
				AssertEquals("D008", ZBool.True, invoice.NeedAtLeastOneInvoiceSupportingDocument);

				supportingDocument.CSI_Code = "N935";
				AssertEquals("N935", ZBool.False, invoice.NeedAtLeastOneInvoiceSupportingDocument);
			});
		}

		public void TestSetDefaultInvoiceDate()
		{
			AssertEquals("For EU, JZ_InvoiceDate should not default to a value", ZDateTime.Empty, invoice.JZ_InvoiceDate);
		}

		public void TestICanBeImportOrExport()
		{
			var importOrExport = invoice as ICanBeImportOrExport;
			CombineAssertions(() =>
			{
				AssertEquals("Level", UniversalReferenceConstants.RefCusCodeListLevelType.Both, importOrExport.Level);
				AssertEquals("Country Code", CountryCodes.Latvia, importOrExport.TrueCountryCode);
				AssertEquals("Data Grouping", CountryCodes.Latvia, importOrExport.DataGroupingCode);
			});
		}

		public void TestRelatedIndicator()
		{
			CombineAssertions(() =>
			{
				invoice.JZ_RelatedIndicator = RelatedIndicatorList.Codes.No;
				AssertEquals("No", false, invoice.RelatedIndicator);
				invoice.JZ_RelatedIndicator = RelatedIndicatorList.Codes.Yes;
				AssertEquals("Yes", true, invoice.RelatedIndicator);
				invoice.JZ_RelatedIndicator = "Z";
				AssertEquals("Invalid", false, invoice.RelatedIndicator);
			});
		}

		public void TestRelatedIndicator_ReadOnly()
		{
			CombineAssertions(() =>
			{
				invoice.RelatedIndicator = false;
				AssertEquals("No Invoice Lines", false, invoice.RelatedIndicatorInfo.ReadOnly);
				var line = invoice.InvoiceLines.AddNew();
				line.RelatedIndicator = true;
				AssertEquals("Invoice Line with RelatedIndictor", true, invoice.RelatedIndicatorInfo.ReadOnly);
				invoice.RelatedIndicator = true;
				AssertEquals("Invoice RelatedIndictor", false, invoice.RelatedIndicatorInfo.ReadOnly);
			});
		}

		public void TestSuspendAddingDefaultInvoiceDocument()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_ApplicationCode = "BLT";

			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "123";
			invoice1.JZ_InvoiceDate = ZDateTime.Today;
			CombineAssertions("Default Supporting document in Invoice1", () =>
			{
				AssertEquals("Count", 1, invoice1.SupportingDocuments.Count);
				var supportingDocument = invoice1.SupportingDocuments[0];
				AssertEquals("Default Document", "N380", supportingDocument.CSI_Code);
			});

			var invoice2 = declaration.Invoices.AddNew();
			using (invoice2.SuspendAddingSupportingDocumentAutomatically())
			{
				invoice2.JZ_InvoiceNumber = "124";
				invoice2.JZ_InvoiceDate = ZDateTime.Today;
				CombineAssertions("Default Supporting document in Invoice2 when suspended", () =>
				{
					AssertEquals("Count", 0, invoice2.SupportingDocuments.Count);
				});
			}

			CombineAssertions("Default Supporting document in Invoice2 after updating Invoice number and not suspended", () =>
			{
				invoice2.JZ_InvoiceNumber = "125";
				AssertEquals("Count", 1, invoice2.SupportingDocuments.Count);
				var supportingDocument = invoice2.SupportingDocuments[0];
				AssertEquals("Default Document", "N380", supportingDocument.CSI_Code);
			});
		}

		public void TestJZ_IncoTerm_ValueChanged_AgreedPlaceCodeSupportEnabled() => AssertJZ_IncoTerm_ValueChanged(true);

		public void TestJZ_IncoTerm_ValueChanged_AgreedPlaceCodeSupportDisabled() => AssertJZ_IncoTerm_ValueChanged(false);

		public void TestIncoTermPlace_ReadOnly_AgreedPlaceCodeSupportEnabled() => AssertIncoTermPlace_ReadOnly(true);

		public void TestIncoTermPlace_ReadOnly_AgreedPlaceCodeSupportDisabled() => AssertIncoTermPlace_ReadOnly(false);

		public void TestAgreedPlaceCodeSupportAndVisible_AgreedPlaceCodeSupportEnabled() => AssertAgreedPlaceCodeSupportAndVisible(true);

		public void TestAgreedPlaceCodeSupportAndVisible_AgreedPlaceCodeSupportDisabled() => AssertAgreedPlaceCodeSupportAndVisible(false);

		public void TestClearIncoTermPlacesIfNeeded()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				CombineAssertions(() =>
				{
					invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
					invoiceHeader.IncoTermsAgreedPlace = "TestIncotermAgreedPlace";
					AssertEquals("IncoTermsAgreedPlace value should be set", "TestIncotermAgreedPlace", invoiceHeader.IncoTermsAgreedPlace);

					invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CarriageAndInsurancePaidTo;
					AssertEquals("IncoTermsAgreedPlace value should be emptied", ZString.Empty, invoiceHeader.IncoTermsAgreedPlace);

					invoiceHeader.ZG_AgreedPlaceCode = "Test";
					invoiceHeader.JZ_IncoTermPlace = "TestIncoTermPlace";
					AssertEquals("JZ_IncoTermPlace value should be set", "TestIncoTermPlace", invoiceHeader.JZ_IncoTermPlace);
					AssertEquals("ZG_AgreedPlaceCode value should be set", "Test", invoiceHeader.ZG_AgreedPlaceCode);
					invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.Other;
					AssertEquals("JZ_IncoTermPlace value should be emptied", ZString.Empty, invoiceHeader.JZ_IncoTermPlace);
					AssertEquals("ZG_AgreedPlaceCode value should be emptied", ZString.Empty, invoiceHeader.ZG_AgreedPlaceCode);
				});
			}
		}

		public void TestIncoTermPlaceCaption()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_IncoTermPlaceInfo, JobDeclaration.CaptionKeySAD);
				AssertEquals("Non UCC6 Caption", "Incoterm Place", captionResourceString.Caption);

				captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_IncoTermPlaceInfo, JobDeclaration.CaptionKeyUCC);
				AssertEquals("UCC6 Caption", "Agreed Place", captionResourceString.Caption);

				captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_IncoTermPlaceInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("UCC6 Export Caption", "Agreed Place", captionResourceString.Caption);
			});
		}

		public void TestBuyerOrgPK_Caption()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().BuyerOrgPKInfo, string.Empty);
				AssertEquals("Non UCC6 Caption", "Buyer", captionResourceString.Caption);

				captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().BuyerOrgPKInfo, JobDeclaration.CaptionKeyImportUCC6);
				AssertEquals("UCC6 Import Caption", "Buyer", captionResourceString.Caption);
				AssertEquals("UCC6 Import FullDescription", "[13 09 016 000] Buyer Name", captionResourceString.FullDescription);

				captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().BuyerOrgPKInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("UCC6 Export Caption", "Buyer", captionResourceString.Caption);
				AssertEquals("UCC6 Export FullDescription", "[13 09 016 000] Buyer Name", captionResourceString.FullDescription);
			});
		}

		public void TestJZ_OA_BuyerAddress_Caption()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_OA_BuyerAddressInfo, string.Empty);
				AssertEquals("Non UCC6 Caption", "Buyer Address", captionResourceString.Caption);

				captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_OA_BuyerAddressInfo, JobDeclaration.CaptionKeyImportUCC6);
				AssertEquals("UCC6 Import Caption", "Buyer Address", captionResourceString.Caption);
				AssertEquals("UCC6 Import FullDescription", "[13 09 018 000] Buyer's Address", captionResourceString.FullDescription);

				captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_OA_BuyerAddressInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("UCC6 Export Caption", "Buyer Address", captionResourceString.Caption);
				AssertEquals("UCC6 Export FullDescription", "[13 09 018 000] Buyer's Address", captionResourceString.FullDescription);
			});
		}

		public void TestSellerOrgPK_Caption()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().SellerOrgPKInfo, string.Empty);
				AssertEquals("Non UCC6 Caption", "Seller", captionResourceString.Caption);

				captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().SellerOrgPKInfo, JobDeclaration.CaptionKeyImportUCC6);
				AssertEquals("UCC6 Import Caption", "Seller", captionResourceString.Caption);
				AssertEquals("UCC6 Import FullDescription", "[13 08 016 000] Seller Name", captionResourceString.FullDescription);

				captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().SellerOrgPKInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("UCC6 Export Caption", "Seller", captionResourceString.Caption);
				AssertEquals("UCC6 Export FullDescription", "[13 08 016 000] Seller Name", captionResourceString.FullDescription);
			});
		}

		public void TestJZ_OA_SellerAddress_Caption()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_OA_SellerAddressInfo, string.Empty);
				AssertEquals("Non UCC6 Caption", "Seller Address", captionResourceString.Caption);

				captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_OA_SellerAddressInfo, JobDeclaration.CaptionKeyImportUCC6);
				AssertEquals("UCC6 Import Caption", "Seller Address", captionResourceString.Caption);
				AssertEquals("UCC6 Import FullDescription", "[13 08 018 000] Seller's Address", captionResourceString.FullDescription);

				captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_OA_SellerAddressInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("UCC6 Export Caption", "Seller Address", captionResourceString.Caption);
				AssertEquals("UCC6 Export FullDescription", "[13 08 018 000] Seller's Address", captionResourceString.FullDescription);
			});
		}

		public void TestExporterOrgPKInfo_Caption()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().ExporterOrgPKInfo, string.Empty);
				AssertEquals("Non UCC6 Caption", "Exporter", captionResourceString.Caption);

				captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().ExporterOrgPKInfo, JobDeclaration.CaptionKeyImportUCC6);
				AssertEquals("UCC6 Import Caption", "Exporter", captionResourceString.Caption);
				AssertEquals("UCC6 Import FullDescription", "[13 01 016 000] Exporter Name", captionResourceString.FullDescription);

				captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().ExporterOrgPKInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("UCC6 Export Caption", "Exporter", captionResourceString.Caption);
				AssertEquals("UCC6 Export FullDescription", "[13 01 016 000] Exporter Name", captionResourceString.FullDescription);
			});
		}

		public void TestJZ_OA_ExporterAddress_Caption()
		{
			CombineAssertions(() =>
			{
				var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_OA_ExporterAddressInfo, string.Empty);
				AssertEquals("Non UCC6 Caption", "Exporter Address", captionResourceString.Caption);

				captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_OA_ExporterAddressInfo, JobDeclaration.CaptionKeyImportUCC6);
				AssertEquals("UCC6 Import Caption", "Exporter Address", captionResourceString.Caption);
				AssertEquals("UCC6 Import FullDescription", "[13 01 018 000] Exporter's Address", captionResourceString.FullDescription);

				captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(Factory.New<JobComInvoiceHeader>().JZ_OA_ExporterAddressInfo, JobDeclaration.CaptionKeyExportUCC6);
				AssertEquals("UCC6 Export Caption", "Exporter Address", captionResourceString.Caption);
				AssertEquals("UCC6 Export FullDescription", "[13 01 018 000] Exporter's Address", captionResourceString.FullDescription);
			});
		}

		#region IUcc6ValueProvider

		public void TestIUcc6ValueProvider_IsUCC6()
		{
			IUcc6ValueProvider ucc6ValueProvider = invoice;

			AssertEquals("Non-UCC6", false, ucc6ValueProvider.IsUCC6);

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				AssertEquals("UCC6", true, ucc6ValueProvider.IsUCC6);
			}
		}

		public void TestIUcc6ValueProvider_IsExport()
		{
			IUcc6ValueProvider ucc6ValueProvider = invoice;
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertEquals("EXP - IsExport", true, ucc6ValueProvider.IsExport);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertEquals("IMP - IsExport", false, ucc6ValueProvider.IsExport);
		}

		public void TestIUcc6ValueProvider_IsImport()
		{
			IUcc6ValueProvider ucc6ValueProvider = invoice;
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertEquals("EXP - IsImport", false, ucc6ValueProvider.IsImport);

			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertEquals("IMP - IsImport", true, ucc6ValueProvider.IsImport);
		}

		#endregion

		void AssertJZ_IncoTerm_ValueChanged(bool agreedPlaceCodeSupport)
		{
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetAgreedPlaceCodeSupport(declaration, agreedPlaceCodeSupport))
				{
					invoice.ZG_AgreedPlaceCode = "abc";
					invoice.JZ_IncoTerm = IncoTerms.CarriageAndInsurancePaidTo;
					AssertEquals("IncoTerm is not Other", "abc", invoice.ZG_AgreedPlaceCode);

					invoice.JZ_IncoTerm = IncoTerms.Other;
					AssertEquals("IncoTerm is Other", agreedPlaceCodeSupport ? ZString.Empty : new ZString("abc"), invoice.ZG_AgreedPlaceCode);
				}
			});
		}

		void AssertIncoTermPlace_ReadOnly(bool agreedPlaceCodeSupport)
		{
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetAgreedPlaceCodeSupport(declaration, agreedPlaceCodeSupport))
				{
					AssertEquals("Invalid Unloco", false, invoice.JZ_IncoTermPlaceInfo.ReadOnly);
					invoice.ZG_AgreedPlaceCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).Code;
					AssertEquals("Valid Unloco", agreedPlaceCodeSupport, invoice.JZ_IncoTermPlaceInfo.ReadOnly);
				}
			});
		}

		void AssertAgreedPlaceCodeSupportAndVisible(bool agreedPlaceCodeSupport)
		{
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetAgreedPlaceCodeSupport(declaration, agreedPlaceCodeSupport))
				{
					invoice.JZ_IncoTerm = IncoTerms.Other;
					AssertEquals("IncoTerms is Other", ZBool.False, invoice.AgreedPlaceCodeSupportAndVisible);

					invoice.JZ_IncoTerm = IncoTerms.CarriageAndInsurancePaidTo;
					AssertEquals("IncoTerms is not Other", agreedPlaceCodeSupport, invoice.AgreedPlaceCodeSupportAndVisible);
				}
			});
		}

		protected override Type ExpectedMetadataType => typeof(BaseJobComInvoiceHeader);

		protected override BusinessObject GetNewBusinessObject() => invoice;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => factory.New<JobDeclaration>().Invoices.AddNew();

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoice = declaration.Invoices.AddNew();
		}
		new JobDeclaration declaration;
		JobComInvoiceHeader invoice;
	}
}
