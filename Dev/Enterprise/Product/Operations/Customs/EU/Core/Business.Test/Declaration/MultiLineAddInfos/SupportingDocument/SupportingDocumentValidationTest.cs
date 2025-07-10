using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	sealed class SupportingDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_AdditionalDescription()
		{
			var message = "Issuing Authority Name is only declared when 'H1', or 'I1'.";
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = instruction.PK;

			supportingDocument = instruction.SupportingDocuments.AddNew();
			supportingDocument.CSI_AdditionalDescription = "IAN";
			AssertCheckIssuingAuthorityNameForEucdm(nameof(instruction));

			supportingDocument = invoiceHeader.SupportingDocuments.AddNew();
			supportingDocument.CSI_AdditionalDescription = "IAN";
			AssertCheckIssuingAuthorityNameForEucdm(nameof(invoiceHeader));

			supportingDocument = invoiceLine.SupportingDocuments.AddNew();
			supportingDocument.CSI_AdditionalDescription = "IAN";
			AssertCheckIssuingAuthorityNameForEucdm(nameof(invoiceLine));

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				instruction.CEI_Style = EUCommonConstants.ImportDeclarationTypeList.H3;
				supportingDocument = declaration.SupportingDocuments.AddNew();
				supportingDocument.CSI_AdditionalDescription = "IAN";
				AssertNoWarning("parent is not instruction, invoiceHeader or invoiceLine", supportingDocument.CSI_AdditionalDescriptionInfo, message);
			}

			void AssertCheckIssuingAuthorityNameForEucdm(string parent)
			{
				CombineAssertions(parent, () =>
				{
					using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
					{
						declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
						instruction.CEI_Style = EUCommonConstants.ImportDeclarationTypeList.H3;
						supportingDocument.Validation.ValidateCSI_AdditionalDescription();
						AssertNoWarning("IsUCC6 is false", supportingDocument.CSI_AdditionalDescriptionInfo, message);
					}

					using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
					{
						supportingDocument.Validation.ValidateCSI_AdditionalDescription();
						AssertHasWarning("IsUCC6, IsImport, Not H1 or I1", supportingDocument.CSI_AdditionalDescriptionInfo, message);

						declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
						supportingDocument.Validation.ValidateCSI_AdditionalDescription();
						AssertNoWarning("IsImport is false", supportingDocument.CSI_AdditionalDescriptionInfo, message);

						declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
						instruction.CEI_Style = EUCommonConstants.ImportDeclarationTypeList.H1;
						supportingDocument.Validation.ValidateCSI_AdditionalDescription();
						AssertNoWarning("H1", supportingDocument.CSI_AdditionalDescriptionInfo, message);

						instruction.CEI_Style = EUCommonConstants.ImportDeclarationTypeList.I1;
						supportingDocument.Validation.ValidateCSI_AdditionalDescription();
						AssertNoWarning("I1", supportingDocument.CSI_AdditionalDescriptionInfo, message);
					}
				});
			}
		}

		public void TestCheckRuleC0612_EntryInstruction()
		{
			AssertRuleC0612Validation(docLinkedToEntryInstruction: true);
		}

		public void TestCheckRuleC0612_InvoiceHeader()
		{
			AssertRuleC0612Validation(docLinkedToEntryInstruction: false);
		}

		class SupportingDocumentValidationDecider : ISupportingDocumentValidationDecider
		{
			public bool SupportBR20311Rule => true;

			public bool SupportC0612Rule => true;

			public bool ShouldCheckIssuingAuthorityNameForEucdm => true;
		}

		void AssertRuleC0612Validation(bool docLinkedToEntryInstruction)
		{
			var validationDecider = new SupportingDocumentValidationDecider();

			var instructionConfigurationMock = new Mock<InstructionConfiguration>();
			instructionConfigurationMock
				.Protected()
				.Setup<ISupportingDocumentValidationDecider>("GetSupportingDocumentValidationDeciderCore", ItExpr.IsAny<CusEntryInstruction>())
				.Returns(validationDecider);

			var invoiceHeaderConfigurationMock = new Mock<InvoiceHeaderConfiguration>();
			invoiceHeaderConfigurationMock
				.Protected()
				.Setup<ISupportingDocumentValidationDecider>("GetSupportingDocumentValidationDeciderCore", ItExpr.IsAny<JobComInvoiceHeader>())
				.Returns(validationDecider);

			var declarationConfigurationMock = new Mock<DeclarationConfiguration>();
			declarationConfigurationMock.CallBase = true;
			declarationConfigurationMock
				.Protected()
				.Setup<InstructionConfiguration>("GetNewInstructionConfiguration")
				.Returns(instructionConfigurationMock.Object);
			declarationConfigurationMock
				.Protected()
				.Setup<InvoiceHeaderConfiguration>("GetNewInvoiceHeaderConfiguration")
				.Returns(invoiceHeaderConfigurationMock.Object);

			var countryOrGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Factory.ClearCachedValue<DeclarationConfiguration>($"DeclarationConfiguration_{countryOrGrouping}");

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(m => m.GetObject()).Returns(declarationConfigurationMock.Object);

			var declarationConfiguration = new KeyObjectHandleDictionaryObject
			{
				{ countryOrGrouping, objectHandleMock.Object }
			};

			using (ObjectFactory.Substitute(nameof(DeclarationConfiguration), declarationConfiguration))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				SupportingDocument supportingDocument = null;

				if (docLinkedToEntryInstruction)
				{
					var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
					supportingDocument = entryInstruction.SupportingDocuments.AddNew();
				}
				else
				{
					var invoiceHeader = declaration.Invoices.AddNew();
					supportingDocument = invoiceHeader.SupportingDocuments.AddNew();
				}

				var invalidCodeTypes = new string[] {
					CusSupportingInfoTypes.C057,
					CusSupportingInfoTypes.C079,
					CusSupportingInfoTypes.C082,
					CusSupportingInfoTypes.C085,
					CusSupportingInfoTypes.C640,
					CusSupportingInfoTypes.C644,
					CusSupportingInfoTypes.C678,
					CusSupportingInfoTypes.E013,
					CusSupportingInfoTypes.L100,
					CusSupportingInfoTypes.N853,
					CusSupportingInfoTypes.Y120,
					CusSupportingInfoTypes.Y121,
					CusSupportingInfoTypes.Y123,
					CusSupportingInfoTypes.Y124,
					CusSupportingInfoTypes.Y125,
					CusSupportingInfoTypes.Y951,
					CusSupportingInfoTypes.Y986
				};

				foreach (var invalidCode in invalidCodeTypes)
				{
					supportingDocument.CSI_Code = invalidCode;
					AssertHasMessageError($"When CSI_Code = {supportingDocument.CSI_Code}", supportingDocument.CSI_CodeInfo,
						$"[C0612] A CERTEX certificate of type {supportingDocument.CSI_Code} can only be declared on the invoice line level.");
				}
			}
		}

		public void TestCheckRuleBR20311_EntryInstruction()
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			supportingDocument = entryInstruction.SupportingDocuments.AddNew();
			AssertRuleBR20311Validation(supportingDocument);
		}

		public void TestCheckRuleBR20311_InvoiceHeader()
		{
			supportingDocument = invoiceHeader.SupportingDocuments.AddNew();
			AssertRuleBR20311Validation(supportingDocument);
		}

		public void TestCheckRuleBR20311_InvoiceLine()
		{
			AssertRuleBR20311Validation(supportingDocument);
		}

		void AssertRuleBR20311Validation(SupportingDocument document) => CombineAssertions(() =>
		{
			const string message = "The format of Supporting Document Reference Number must be YYYYMMDD when Type is U164, U165, U166 or U167.";
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				document.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.U164;
				document.CSI_ReferenceNumber = "123";
				AssertHasMessageError("Type U164, invalid CSI_ReferenceNumber", document.CSI_ReferenceNumberInfo, message);
				document.CSI_ReferenceNumber = "20230908";
				AssertNoMessageError("Type U164, valid CSI_ReferenceNumber", document.CSI_ReferenceNumberInfo, message);

				document.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.U165;
				document.CSI_ReferenceNumber = "123";
				AssertHasMessageError("Type U165, invalid CSI_ReferenceNumber", document.CSI_ReferenceNumberInfo, message);
				document.CSI_ReferenceNumber = "19981231";
				AssertNoMessageError("Type U165, valid CSI_ReferenceNumber", document.CSI_ReferenceNumberInfo, message);

				document.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.U166;
				document.CSI_ReferenceNumber = "123";
				AssertHasMessageError("Type U166, invalid CSI_ReferenceNumber", document.CSI_ReferenceNumberInfo, message);
				document.CSI_ReferenceNumber = "20080101";
				AssertNoMessageError("Type U166, valid CSI_ReferenceNumber", document.CSI_ReferenceNumberInfo, message);

				document.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.U167;
				document.CSI_ReferenceNumber = "123";
				AssertHasMessageError("Type U167, invalid CSI_ReferenceNumber", document.CSI_ReferenceNumberInfo, message);
				document.CSI_ReferenceNumber = "20211025";
				AssertNoMessageError("Type U167, valid CSI_ReferenceNumber", document.CSI_ReferenceNumberInfo, message);

				document.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N380;
				document.CSI_ReferenceNumber = "123";
				AssertNoMessageError("Type N380, invalid CSI_ReferenceNumber", document.CSI_ReferenceNumberInfo, message);

				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				document.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.U164;
				document.CSI_ReferenceNumber = "456";
				AssertNoMessageError("Type U164, invalid CSI_ReferenceNumber, SupportBR20311Rule false", document.CSI_ReferenceNumberInfo, message);
			}
		});

		public void TestHeaderAggregationWarning()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);
			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
					new string[] { importCodeType }, "UDJC", "Some description", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			supportingDocument = invoiceHeader.SupportingDocuments.AddNew();
			var partialWarning = SupportingDocumentValidation.WarningForAggregateableFields;
			supportingDocument.CSI_Code = "UDJC";

			supportingDocument.CSI_Quantity = 10;
			AssertNoWarningContaining(supportingDocument.CSI_QuantityInfo, partialWarning);
			supportingDocument.CSI_Quantity2 = 20;
			AssertNoWarningContaining(supportingDocument.CSI_Quantity2Info, partialWarning);
			supportingDocument.CSI_Quantity3 = 30;
			AssertNoWarningContaining(supportingDocument.CSI_Quantity3Info, partialWarning);
			supportingDocument.CSI_Value = 40;
			AssertNoWarningContaining(supportingDocument.CSI_ValueInfo, partialWarning);

			var anotherInvoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			supportingDocument.CSI_Quantity = 11;
			AssertHasWarningContaining(supportingDocument.CSI_QuantityInfo, partialWarning);
			supportingDocument.CSI_Quantity2 = 22;
			AssertHasWarningContaining(supportingDocument.CSI_Quantity2Info, partialWarning);
			supportingDocument.CSI_Quantity3 = 33;
			AssertHasWarningContaining(supportingDocument.CSI_Quantity3Info, partialWarning);
			supportingDocument.CSI_Value = 44;
			AssertHasWarningContaining(supportingDocument.CSI_ValueInfo, partialWarning);
		}

		public void TestCheckCSI_QuantityMaxLength_Import()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertCSI_QuantityMaxLength();
		}

		public void TestCheckCSI_QuantityMaxLength_Export()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertCSI_QuantityMaxLength();
		}

		void AssertCSI_QuantityMaxLength()
		{
			var errorMessage = "Supporting documents > Quantity, the maximum allowed number of digits is 16.";

			invoiceHeader = declaration.Invoices.AddNew();
			var supportingDocumentHeader = invoiceHeader.SupportingDocuments.AddNew();

			CombineAssertions("Invoice Header", () =>
			{
				supportingDocumentHeader.CSI_Quantity = 1234567890123456m;
				AssertNoMessageError("CSI_Quantiuty.Length == 16", supportingDocumentHeader.CSI_QuantityInfo, errorMessage);

				supportingDocumentHeader.CSI_Quantity = 1234567890.12345m;
				AssertNoMessageError("CSI_Quantiuty.Length <= 16", supportingDocumentHeader.CSI_QuantityInfo, errorMessage);

				supportingDocumentHeader.CSI_Quantity = 12345678901.12345m;
				AssertNoMessageError("CSI_Quantiuty.Length == 17 (including decimal)", supportingDocumentHeader.CSI_QuantityInfo, errorMessage);

				supportingDocumentHeader.CSI_Quantity = 12345678901.123456m;
				AssertHasMessageError("CSI_Quantiuty.Length == 17 (excluding decimal)", supportingDocumentHeader.CSI_QuantityInfo, errorMessage);

				supportingDocumentHeader.CSI_Quantity = 12345678901234567m;
				AssertHasMessageError("CSI_Quantiuty.Length == 17", supportingDocumentHeader.CSI_QuantityInfo, errorMessage);
			});

			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var supportingDocumentInvLine = invoiceLine.SupportingDocuments.AddNew();
			CombineAssertions("Invoice Line", () =>
			{
				supportingDocumentInvLine.CSI_Quantity = 1234567890123456m;
				AssertNoMessageError("CSI_Quantiuty.Length == 16", supportingDocumentInvLine.CSI_QuantityInfo, errorMessage);

				supportingDocumentInvLine.CSI_Quantity = 1234567890.12345m;
				AssertNoMessageError("CSI_Quantiuty.Length == 16 (including decimal)", supportingDocumentInvLine.CSI_QuantityInfo, errorMessage);

				supportingDocumentInvLine.CSI_Quantity = 12345678901.12345m;
				AssertNoMessageError("CSI_Quantiuty.Length == 17 (including decimal)", supportingDocumentInvLine.CSI_QuantityInfo, errorMessage);

				supportingDocumentInvLine.CSI_Quantity = 12345678901.123456m;
				AssertHasMessageError("CSI_Quantiuty.Length == 17 (excluding decimal)", supportingDocumentInvLine.CSI_QuantityInfo, errorMessage);

				supportingDocumentInvLine.CSI_Quantity = 12345678901234567m;
				AssertHasMessageError("CSI_Quantiuty.Length == 17", supportingDocumentInvLine.CSI_QuantityInfo, errorMessage);
			});
		}

		public void TestTypeCode_Import()
		{
			AssertTypeCodeWorksWithCorrectMessages(MessageTypeList.Codes.Import, "U022", "X006");
		}

		public void TestTypeCode_Export()
		{
			AssertTypeCodeWorksWithCorrectMessages(MessageTypeList.Codes.Import, "U022", "X006");
		}

		public void TestForDuplicates()
		{
			const string expectedErrorMessage = "A row with that document type and reference already exists on this invoice line.";
			CombineAssertions(() =>
			{
				supportingDocument.CSI_Code = "N002";
				supportingDocument.CSI_ReferenceNumber = "DANIEL";
				AssertNoMessageError("Single", supportingDocument.CSI_CodeInfo, expectedErrorMessage);
				var supportingDocument2 = invoiceLine.SupportingDocuments.AddNew();
				supportingDocument2.CSI_ReferenceNumber = "DANIEL";
				supportingDocument2.CSI_Code = "N002";
				AssertHasMessageError("Duplicate", supportingDocument2.CSI_CodeInfo, expectedErrorMessage);
				supportingDocument2.CSI_Code = "U022";
				AssertNoMessageError("Different Code", supportingDocument2.CSI_CodeInfo, expectedErrorMessage);
				supportingDocument2.CSI_ReferenceNumber = "NEW VALUE";
				supportingDocument2.CSI_Code = "N002";
				AssertNoMessageError("Different Reference Number", supportingDocument2.CSI_CodeInfo, expectedErrorMessage);
			});
		}

		public void TestInconsistentUnitOrCurrency()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supDoc1 = declaration.SupportingDocuments.AddNew();
			var supDoc2 = declaration.SupportingDocuments.AddNew();

			supDoc1.CSI_ReferenceNumber = "PermitA";
			supDoc2.CSI_ReferenceNumber = "PermitA";

			supDoc1.CSI_UnitOfQuantity = "KG";
			supDoc2.CSI_UnitOfQuantity = "CM";
			supDoc2.RunPreSaveValidation();
			AssertHasRowMessageError("Same reference, but different units (KG & CM).", supDoc2, SupportingDocumentValidation.InconsistentUnitOrCurrency);

			supDoc2.CSI_UnitOfQuantity = "KG";
			supDoc2.RunPreSaveValidation();
			AssertNoRowMessageError(supDoc2, SupportingDocumentValidation.InconsistentUnitOrCurrency);

			supDoc1.CSI_RX_NKCurrency = "GBP";
			supDoc2.CSI_RX_NKCurrency = "FRF";
			supDoc2.RunPreSaveValidation();
			AssertHasRowMessageError("Same reference, but different currencies (GBP & FRF).", supDoc2, SupportingDocumentValidation.InconsistentUnitOrCurrency);

			supDoc2.CSI_ReferenceNumber = "PermitB";
			supDoc2.RunPreSaveValidation();
			AssertNoRowMessageError(supDoc2, SupportingDocumentValidation.InconsistentUnitOrCurrency);
		}

		public void TestCheckCSI_ReferenceNumberTriggersParentInvoiceLineTariffCheck()
		{
			var currentCountryCode = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(currentCountryCode, "IMP");
			Factory.Save();
			var tariff = helper.CreateTariff(currentCountryCode, tariffType.PK, "08091998", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			declaration.JE_MessageType = "IMP";
			var mockInvoiceLine = Factory.NewMoq<JobComInvoiceLine>();
			mockInvoiceLine.Object.JI_JZ = invoiceHeader.PK;
			var supportingDocument = Factory.New<SupportingDocument>();
			mockInvoiceLine.Object.SupportingDocuments.Add(supportingDocument);

			var mockInvoiceLineValidation = new Mock<JobComInvoiceLineValidation>(mockInvoiceLine.Object);
			mockInvoiceLine.Protected().Setup<Customs.Business.JobComInvoiceLineValidation>("GetNewValidation").Returns(mockInvoiceLineValidation.Object);

			mockInvoiceLineValidation.Protected().Setup("CheckJI_Tariff");
			supportingDocument.Validation.ValidateCSI_ReferenceNumber();
			mockInvoiceLineValidation.Protected().Verify("CheckJI_Tariff", Times.Never());

			mockInvoiceLineValidation.Reset();
			mockInvoiceLine.Object.JI_Tariff = tariff.ZZ1_TariffCode;
			mockInvoiceLineValidation.Protected().Setup("CheckJI_Tariff");
			supportingDocument.Validation.ValidateCSI_ReferenceNumber();
			mockInvoiceLineValidation.VerifyAll();

			mockInvoiceLineValidation.Reset();
			supportingDocument.CSI_Code = ZString.Empty;
			supportingDocument.CSI_ReferenceNumber = ZString.Empty;
			mockInvoiceLineValidation.Protected().Setup("CheckJI_Tariff");
			supportingDocument.Validation.ValidateCSI_ReferenceNumber();
			mockInvoiceLineValidation.VerifyAll();

			mockInvoiceLineValidation.Reset();
			var supportingDocumentWithoutParent = Factory.New<SupportingDocument>();
			AssertNull("Parent is null", supportingDocumentWithoutParent.Parent);
			mockInvoiceLineValidation.Protected().Setup("CheckJI_Tariff");
			supportingDocumentWithoutParent.Validation.ValidateCSI_ReferenceNumber();
			mockInvoiceLineValidation.Protected().Verify("CheckJI_Tariff", Times.Never());

			mockInvoiceLineValidation.Reset();
			var declarationSupportingDocument = declaration.SupportingDocuments.AddNew();
			AssertType<JobDeclaration>("Parent is a JobDeclaration", declarationSupportingDocument.Parent);
			mockInvoiceLineValidation.Protected().Setup("CheckJI_Tariff");
			declarationSupportingDocument.Validation.ValidateCSI_ReferenceNumber();
			mockInvoiceLineValidation.Protected().Verify("CheckJI_Tariff", Times.Never());
		}

		public void TestCheckCSI_ReferenceNumberMandatoryValidationBasedOnSUPCusConditions()
		{
			var currentCountryCode = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(currentCountryCode, "IMP");
			var ctrlType1 = helper.CreateOrGetExistingRefCusConditionType(currentCountryCode, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "TSTC1", "Test Ctrl Condition Type 1");
			Factory.Save();
			var tariff = helper.CreateTariff(currentCountryCode, tariffType.PK, "08091998", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var supValueType = helper.CreateOrGetExistingRefCusConditionValueType(currentCountryCode, Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocument);
			var snrValueType = helper.CreateOrGetExistingRefCusConditionValueType(currentCountryCode, Core.Constants.Customs.Universal.RefCusConditionValueTypes.Codes.SupportingDocumentNoReferenceNumber);

			var condition1 = helper.CreateOrGetExistingRefCusCondition(currentCountryCode, ctrlType1.PK, tariff.PK, "Direction:Import", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(supValueType.PK, condition1.PK, "C000");
			var condition2 = helper.CreateOrGetExistingRefCusCondition(currentCountryCode, ctrlType1.PK, tariff.PK, "Direction:Import", true, false, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			helper.CreateOrGetExistingRefCusConditionValue(snrValueType.PK, condition2.PK, "C001");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			var supportingDocument = invoiceLine.SupportingDocuments.AddNew();
			declaration.JE_MessageType = "IMP";
			invoiceLine.JI_Tariff = "08091998";

			supportingDocument.CSI_Code = "";
			supportingDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageErrorContaining("Empty CSI_Code", supportingDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			supportingDocument.CSI_Code = "C001";
			supportingDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageErrorContaining("Test C001/SNR", supportingDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			supportingDocument.CSI_Code = "C000";
			supportingDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageErrorContaining("Test C000/SUP and empty CSI_ReferenceNumber", supportingDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			supportingDocument.CSI_ReferenceNumber = "123456";
			supportingDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageErrorContaining("Test C000/SUP and CSI_ReferenceNumber filled", supportingDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_Tariff = "";
			supportingDocument.CSI_Code = "C000";
			supportingDocument.CSI_ReferenceNumber = "";
			supportingDocument.Validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageErrorContaining("Test C000/SUP and empty CSI_ReferenceNumber and tariff", supportingDocument.CSI_ReferenceNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckCSI_RX_NKCurrency()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(supportingDocument.CSI_RX_NKCurrencyInfo, "XYZ", "AUD");
		}

		public void TestCheckCSI_Description()
		{
			SupportingDocumentTestHelper.SetupRefCusCodeList(Factory);
			var countryCode = Core.Constants.CountryCodes.UnitedKingdom;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = "CHF";
				var supportingDocument = declaration.SupportingDocuments.AddNew();

				supportingDocument.CSI_Code = "Y057";
				supportingDocument.CSI_Description = "";
				supportingDocument.Validation.ValidateCSI_Description();
				AssertHasWarning(supportingDocument.CSI_DescriptionInfo, SupportingDocumentValidation.EmptyStatementTextWarning);

				supportingDocument.CSI_Code = "Y057";
				supportingDocument.CSI_Description = "Import licence not required";
				supportingDocument.Validation.ValidateCSI_Description();
				AssertNoWarning(supportingDocument.CSI_DescriptionInfo, SupportingDocumentValidation.EmptyStatementTextWarning);

				supportingDocument.CSI_Code = "T123";
				supportingDocument.CSI_Description = "";
				supportingDocument.Validation.ValidateCSI_Description();
				AssertNoWarning(supportingDocument.CSI_DescriptionInfo, SupportingDocumentValidation.EmptyStatementTextWarning);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			supportingDocument = invoiceLine.SupportingDocuments.AddNew();
		}

		SupportingDocument supportingDocument;
		JobComInvoiceLine invoiceLine;
		JobComInvoiceHeader invoiceHeader;
		JobDeclaration declaration;

		void AssertTypeCodeWorksWithCorrectMessages(string messageType, string typeCode, string typeCodeIncorrect)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			if (messageType == MessageTypeList.Codes.Import)
			{
				helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
					new string[] { importCodeType }, typeCode, typeCode + " DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			}
			else
			{
				helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
					new string[] { exportCodeType }, typeCode, typeCode + " DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			}

			Factory.Save();

			declaration.JE_MessageType = messageType;
			supportingDocument.CSI_Code = typeCode;
			AssertNoMessageErrors(supportingDocument.CSI_CodeInfo);
			supportingDocument.CSI_Code = ZString.Empty;
			AssertHasNotifications("Please enter a Type Code.", supportingDocument.CSI_CodeInfo);
			AssertHasMessageErrorContaining(supportingDocument.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			supportingDocument.CSI_Code = typeCodeIncorrect;
			AssertHasMessageErrors(supportingDocument.CSI_CodeInfo);
		}
	}
}
