using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceLine))]
	sealed class JobComInvoiceLineBaseOnlyTest : JobComInvoiceLineTest<JobComInvoiceLine>
	{
		public void TestIsIntoRegime()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(currentCountry, "IM", "45", "00", "F06", "Description", "IMP");
			procedure.ZZ6_IntoVATWarehouse = "Y";

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Should return true when ZZ6_IntoVATWarehouse is Y", true, invoiceLine.IsIntoRegime(procedure));
			procedure.ZZ6_IntoVATWarehouse = "N";
			AssertEquals("Should return false when ZZ6_IntoVATWarehouse is N", false, invoiceLine.IsIntoRegime(procedure));
		}

		public void TestGetNewProcedureRegimeDecider()
		{
			var invoiceLine = Factory.New<JobComInvoiceLineForTest>();
			AssertType<ProcedureRegimeDecider>(invoiceLine.GetNewProcedureRegimeDeciderExposed());
		}

		public void TestGetWarningBeforeBeingDeleted()
		{
			const string rowErrorMessage = "The selected Invoice Line has been already declared. Entry lines cannot be deleted from a declared or canceled Entry.";

			invoiceLine.AddRowError(rowErrorMessage);
			CombineAssertions(() =>
			{
				AssertEquals("no linked EntryHeader, invoiceLine has RowError, LockNumberOfEntryLines false", string.Empty, invoiceLine.GetWarningBeforeBeingDeleted());

				var entryInstuction = declaration.CustomsEntryInstructions.AddNew();
				invoiceLine.JI_CEI = entryInstuction.PK;
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstuction.PK;
				AssertEquals("linked EntryHeader, invoiceLine has RowError, LockNumberOfEntryLines false", string.Empty, invoiceLine.GetWarningBeforeBeingDeleted());

				invoiceLine.ClearAllNotifications();
				AssertEquals("linked EntryHeader, invoiceLine has no RowError, LockNumberOfEntryLines false", string.Empty, invoiceLine.GetWarningBeforeBeingDeleted());

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetLockNumberOfEntryLinesForRegisteredEntryConfiguration(declaration, configurationValue: true))
				{
					var message = entryHeader.Messages.AddNew();
					message.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
					AssertEquals("linked EntryHeader, invoiceLine has no RowError, LockNumberOfEntryLines true as IsWaitingForResponse", rowErrorMessage, invoiceLine.GetWarningBeforeBeingDeleted());

					message.Delete();
					entryHeader.EntryNumber = "123456";
					AssertEquals("linked EntryHeader, invoiceLine has no RowError, LockNumberOfEntryLines true as HasBeenLodgedAtCustoms", rowErrorMessage, invoiceLine.GetWarningBeforeBeingDeleted());
				}
			});
		}

		public void TestJI_TaxOrFeeDetailAndJI_TaxOrFeeDetailEntityUpdatedWhenVatCacheKeyChanged()
		{
			SetUpReferenceData();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.JE_MessageType = "IMP";
			invoiceLine.JI_Tariff = "99999999";
			var taxOrFeeDetailEntityList = invoiceLine.Lookups.TaxOrFeeDetailEntities;

			var taxOrFeeDetailEntity_VT1_Add1 = taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT1" && x.AdditionalCode == "ADD1");
			var taxOrFeeDetailEntity_VT2_Add1 = taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT2" && x.AdditionalCode == "ADD1");
			var taxOrFeeDetailEntity_VT2_EMPTY = taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT2" && x.AdditionalCode == "");

			invoiceLine.JI_TaxOrFeeDetail = ZGuid.Empty;
			invoiceLine.JI_ZZF_NKTaxType = "VT1";
			invoiceLine.JI_SupplementaryCode1 = "ADD1";
			invoiceLine.OnLoaded();
			AssertEquals("Prerequisite: JI_TaxOrFeeDetail has value.", taxOrFeeDetailEntity_VT1_Add1.PK, invoiceLine.JI_TaxOrFeeDetail);
			AssertEquals("Prerequisite: JI_TaxOrFeeDetailEntity is not null.", taxOrFeeDetailEntity_VT1_Add1.PK, invoiceLine.JI_TaxOrFeeDetailEntity.PK);

			invoiceLine.JI_Tariff = "11111111";
			taxOrFeeDetailEntity_VT2_Add1 = invoiceLine.Lookups.TaxOrFeeDetailEntities.FirstOrDefault(x => x.VATCode == "VT2" && x.AdditionalCode == "ADD1");
			AssertNull("JI_TaxOrFeeDetailEntity should be cleared vatCacheKey(e.g. JI_Tariff) changed.", invoiceLine.JI_TaxOrFeeDetailEntity);
			invoiceLine.JI_TaxOrFeeDetail = taxOrFeeDetailEntity_VT2_Add1.PK;
			AssertEquals("JI_TaxOrFeeDetailEntity should be updated when a new JI_TaxOrFeeDetail is entered.", taxOrFeeDetailEntity_VT2_Add1.PK, invoiceLine.JI_TaxOrFeeDetailEntity.PK);

			void SetUpReferenceData()
			{
				var startDate = ZDateTime.Today.AddDays(-2);
				var endDate = ZDateTime.Today.AddDays(2);
				var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				var helper = new UniversalReferenceTestDataHelper(Factory);

				helper.CreateNewOrGetExistingDataGrouping(currentCountry).ZZZ_ZZZ_Grouping = helper.CreateNewOrGetExistingDataGrouping("EUN").PK;

				var impTariffTypePK = helper.CreateNewOrGetExistingTariffType("EUN", "IMP").PK;
				Factory.Save();
				var tariff = helper.CreateTariff(currentCountry, impTariffTypePK, "99999999", ZDateTime.BrettsBirthday,
					endDate, "Alpha Bravo");
				var tariff2 = helper.CreateTariff(currentCountry, impTariffTypePK, "11111111", startDate,
					endDate, "Alpha Bravo");
				helper.CreateTaxOrFee("VT1", 0.02m, currentCountry, description: "VAT One");
				helper.CreateTaxOrFee("VT2", 0.01m, currentCountry, description: "VAT Two");

				helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT1", startDate: startDate,
					endDate: endDate, additionalCode: "ADD1");
				helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT2", startDate: startDate,
					endDate: endDate, additionalCode: "ADD1");
				helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT2", startDate: startDate,
					endDate: endDate, additionalCode: "");
				helper.CreateNewOrGetExistingVATApplicability(tariff2, currentCountry, "VT2", startDate: startDate,
					endDate: endDate, additionalCode: "ADD1");

				helper.CreateCusCodeType("ADDIN", "Additional Code");
				helper.CreateCusCodeList(currentCountry, "ADDIN", "ADD1", "Test Additional Code 1",
					startDate: startDate, endDate: endDate);

				Factory.Save();
			}
		}

		public void TestJI_Calc_RequestedProcedure()
		{
			invoiceLine.JI_Procedure = "XX123";
			AssertEquals("Should return first 2 characters of JI_Procedure", "XX", invoiceLine.JI_Calc_RequestedProcedure);
		}

		public void TestSetFirst2CharactersOfJI_Procedure()
		{
			invoiceLine.JI_Procedure = "XX123";
			invoiceLine.SetFirst2CharactersOfJI_Procedure("AA");
			AssertEquals("AA123", invoiceLine.JI_Procedure);
		}

		public void TestZG_CountryOfDispatch()
		{
			AssertEquals("[UCC 5/8] Dispatch", DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.ZG_CountryOfDispatch)).Caption);
		}

		public void TestSupplementaryCodeIsSet_WhenJI_ZZF_NKTaxTypeChanges_WhenUsingSingleVatField()
		{
			var invoiceLineConfigurationMock = new Mock<InvoiceLineConfiguration>();
			invoiceLineConfigurationMock.Protected()
				.Setup<ZBool>("UseMultipleVatFieldsCore")
				.Returns(false);

			using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(Factory, "GetNewInvoiceLineConfiguration", invoiceLineConfigurationMock.Object, null))
			{
				SetUpReferenceData();

				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = "IMP";
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "99999999";
				AssertEquals(declaration.Configuration.InvoiceLineConfiguration.UseMultipleVatFields, false);
				CombineAssertions(() =>
				{
					invoiceLine.JI_ZZF_NKTaxType = "IT1";
					var invoiceLineType = invoiceLine.GetType();
					var supplementaryCode1 = (SupplementaryCode)invoiceLineType.GetProperty("SupplementaryCode1", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(invoiceLine);
					AssertEquals(
						"If there is no VAT supplementary code existing, and JI_SupplementaryCode1 is empty, the VAT additional code should be filled in the JI_SupplementaryCode1.",
						"Add1-LV", $"{invoiceLine.JI_SupplementaryCode1}-{supplementaryCode1.CY_Data}");

					invoiceLine.JI_SupplementaryCode1 = "S001";
					invoiceLine.JI_ZZF_NKTaxType = "IT2";
					var supplementaryCode2 = (SupplementaryCode)invoiceLineType.GetProperty("SupplementaryCode2", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(invoiceLine);
					AssertEquals(
						"If there is no VAT supplementary code existing, and JI_SupplementaryCode1 is filled while JI_SupplementaryCode2 is empty, the VAT additional code should be filled in the JI_SupplementaryCode2.",
						"Add2-LV", $"{invoiceLine.JI_SupplementaryCode2}-{supplementaryCode2.CY_Data}");

					invoiceLine.JI_SupplementaryCode1 = "S001";
					invoiceLine.JI_SupplementaryCode2 = "S002";
					invoiceLine.JI_ZZF_NKTaxType = "IT1";
					var invLineAdditionalSupplementaryCodes = invoiceLine.AdditionalSupplementaryCodes;
					AssertEquals(
						"If there is no VAT supplementary code existing, and JI_SupplementaryCode1 and JI_SupplementaryCode2 are both filled while AdditionalSupplementaryCodes is empty, the VAT additional code should be filled in the AdditionalSupplementaryCodes.",
						"Add1-LV", $"{invLineAdditionalSupplementaryCodes.AsString}-{string.Join(",", invoiceLine.AdditionalSupplementaryCodes.Cast<SupplementaryCode>().OrderBy(x => x.CY_Order).Select(x => x.CY_Data))}");

					invoiceLine.JI_SupplementaryCode1 = "S001";
					invoiceLine.JI_SupplementaryCode2 = "S002";
					invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					invoiceLine.AdditionalSupplementaryCodes.AddNew("S003");
					invoiceLine.JI_ZZF_NKTaxType = "IT2";
					AssertEquals(
						"If there is no VAT supplementary code existing, and JI_SupplementaryCode1 and JI_SupplementaryCode2 are both filled while AdditionalSupplementaryCodes is filled with a code, the VAT additional code should be appended to the end of AdditionalSupplementaryCodes.",
						"S003,Add2-,LV", $"{invLineAdditionalSupplementaryCodes.AsString}-{string.Join(",", invoiceLine.AdditionalSupplementaryCodes.Cast<SupplementaryCode>().OrderBy(x => x.CY_Order).Select(x => x.CY_Data))}");

					invoiceLine.JI_SupplementaryCode1 = "Add2";
					invoiceLine.JI_SupplementaryCode2 = "S002";
					invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					invoiceLine.AdditionalSupplementaryCodes.AddNew("S003");
					invoiceLine.JI_ZZF_NKTaxType = "IT1";
					supplementaryCode1 = (SupplementaryCode)invoiceLineType.GetProperty("SupplementaryCode1", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(invoiceLine);
					AssertEquals(
						"If there is a VAT supplementary code existing at JI_SupplementaryCode1, it will be replaced with new VAT additional code.",
						"Add1-LV", $"{invoiceLine.JI_SupplementaryCode1}-{supplementaryCode1.CY_Data}");

					invoiceLine.JI_SupplementaryCode1 = "S001";
					invoiceLine.JI_SupplementaryCode2 = "Add1";
					invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					invoiceLine.AdditionalSupplementaryCodes.AddNew("S003");
					invoiceLine.JI_ZZF_NKTaxType = "IT2";
					supplementaryCode2 = (SupplementaryCode)invoiceLineType.GetProperty("SupplementaryCode2", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(invoiceLine);
					AssertEquals(
						"If there is a VAT supplementary code existing at JI_SupplementaryCode2, it will be replaced with new VAT additional code.",
						"Add2-LV", $"{invoiceLine.JI_SupplementaryCode2}-{supplementaryCode2.CY_Data}");

					invoiceLine.JI_SupplementaryCode1 = "S001";
					invoiceLine.JI_SupplementaryCode2 = "S002";
					invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					invoiceLine.AdditionalSupplementaryCodes.AddNew("Add2");
					invoiceLine.JI_ZZF_NKTaxType = "IT1";
					AssertEquals(
						"If there is a VAT supplementary code existing within AdditionalSupplementaryCodes as the only one code, it will be replaced with new VAT additional code.",
						"Add1-LV", $"{invLineAdditionalSupplementaryCodes.AsString}-{string.Join(",", invoiceLine.AdditionalSupplementaryCodes.Cast<SupplementaryCode>().OrderBy(x => x.CY_Order).Select(x => x.CY_Data))}");

					invoiceLine.JI_SupplementaryCode1 = "S001";
					invoiceLine.JI_SupplementaryCode2 = "S002";
					invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					invoiceLine.AdditionalSupplementaryCodes.AddNew("Add1");
					invoiceLine.AdditionalSupplementaryCodes.AddNew("S003");
					invoiceLine.JI_ZZF_NKTaxType = "IT2";
					AssertEquals(
						"If there is a VAT supplementary code existing within AdditionalSupplementaryCodes as one of the members, it will be replaced with new VAT additional code.",
						"S003,Add2-,LV", $"{invLineAdditionalSupplementaryCodes.AsString}-{string.Join(",", invoiceLine.AdditionalSupplementaryCodes.Cast<SupplementaryCode>().OrderBy(x => x.CY_Order).Select(x => x.CY_Data))}");

					invoiceLine.JI_SupplementaryCode1 = "S001";
					invoiceLine.JI_SupplementaryCode2 = "S002";
					invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					invoiceLine.AdditionalSupplementaryCodes.AddNew("S003");
					invoiceLine.JI_ZZF_NKTaxType = "IT3";
					AssertEquals(
						"If there is no VAT supplementary code existing, when JI_ZZF_NKTaxType is set to a new value with empty VAT additional code, nothing should be done.",
						"S003,S001,S002-,,", $"{string.Join(",", invoiceLine.SupplementaryCodes.Select(c => c.CY_Code))}-{string.Join(",", invoiceLine.SupplementaryCodes.Select(c => c.CY_Data))}");

					invoiceLine.JI_ZZF_NKTaxType = string.Empty;
					invoiceLine.JI_SupplementaryCode1 = "Add2";
					invoiceLine.JI_SupplementaryCode2 = "S002";
					invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					invoiceLine.AdditionalSupplementaryCodes.AddNew("S003");
					invoiceLine.JI_ZZF_NKTaxType = "IT3";
					supplementaryCode1 = (SupplementaryCode)invoiceLineType.GetProperty("SupplementaryCode1", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(invoiceLine);
					AssertEquals(
						"If there is a VAT supplementary code existing, when JI_ZZF_NKTaxType is set to a new value with empty additional code, the VAT additional code should be removed.",
						ZString.Empty, invoiceLine.JI_SupplementaryCode1);
					AssertNull(
						"If there is a VAT supplementary code existing, when JI_ZZF_NKTaxType is set to a new value with empty additional code, the VAT additional code should be removed.",
						supplementaryCode1);
					AssertEquals(
						"If there is a VAT supplementary code existing, when JI_ZZF_NKTaxType is set to a new value with empty additional code, the VAT additional code should be removed.",
						"S003,S002-,", $"{string.Join(",", invoiceLine.SupplementaryCodes.Select(c => c.CY_Code))}-{string.Join(",", invoiceLine.SupplementaryCodes.Select(c => c.CY_Data))}");

					invoiceLine.JI_ZZF_NKTaxType = string.Empty;
					invoiceLine.JI_SupplementaryCode1 = "S001";
					invoiceLine.JI_SupplementaryCode2 = "S002";
					invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					invoiceLine.AdditionalSupplementaryCodes.AddNew("Add2");
					invoiceLine.AdditionalSupplementaryCodes.AddNew("S004");
					invoiceLine.JI_ZZF_NKTaxType = "IT3";
					AssertEquals(
						"If there is a VAT supplementary code existing, when JI_ZZF_NKTaxType is set to a new value with empty additional code, the VAT additional code should be removed.",
						1, invoiceLine.AdditionalSupplementaryCodes.Count);
					AssertEquals(
						"If there is a VAT supplementary code existing, when JI_ZZF_NKTaxType is set to a new value with empty additional code, the VAT additional code should be removed.",
						"S004,S001,S002-,,", $"{string.Join(",", invoiceLine.SupplementaryCodes.Select(c => c.CY_Code))}-{string.Join(",", invoiceLine.SupplementaryCodes.Select(c => c.CY_Data))}");

					invoiceLine.JI_ZZF_NKTaxType = string.Empty;
					invoiceLine.JI_SupplementaryCode1 = "Add1";
					invoiceLine.JI_SupplementaryCode2 = "Add2";
					invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					invoiceLine.AdditionalSupplementaryCodes.AddNew("S004");
					invoiceLine.JI_ZZF_NKTaxType = "IT4";
					supplementaryCode1 = (SupplementaryCode)invoiceLineType.GetProperty("SupplementaryCode1", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(invoiceLine);
					supplementaryCode2 = (SupplementaryCode)invoiceLineType.GetProperty("SupplementaryCode2", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(invoiceLine);
					AssertEquals(
						"If there are multiple VAT supplementary codes existing, when JI_ZZF_NKTaxType is set to a new value with empty additional code, all previous VAT additional codes should be removed.",
						"Add4-LV", $"{invoiceLine.JI_SupplementaryCode1}-{supplementaryCode1.CY_Data}");
					AssertEquals(
						"If there are multiple VAT supplementary codes existing, when JI_ZZF_NKTaxType is set to a new value with empty additional code, all previous VAT additional codes should be removed.",
						ZString.Empty, invoiceLine.JI_SupplementaryCode2);
					AssertNull(
						"If there are multiple VAT supplementary codes existing, when JI_ZZF_NKTaxType is set to a new value with empty additional code, all previous VAT additional codes should be removed.",
						supplementaryCode2);
					AssertEquals(
						"If there are multiple VAT supplementary codes existing, when JI_ZZF_NKTaxType is set to a new value with empty additional code, all previous VAT additional codes should be removed.",
						"S004,Add4-,LV", $"{string.Join(",", invoiceLine.SupplementaryCodes.Select(c => c.CY_Code))}-{string.Join(",", invoiceLine.SupplementaryCodes.Select(c => c.CY_Data))}");

					invoiceLine.JI_ZZF_NKTaxType = string.Empty;
					invoiceLine.JI_SupplementaryCode1 = "S001";
					invoiceLine.JI_SupplementaryCode2 = "Add1";
					invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					invoiceLine.AdditionalSupplementaryCodes.AddNew("Add2");
					invoiceLine.AdditionalSupplementaryCodes.AddNew("S004");
					Factory.Save();
					invoiceLine.JI_ZZF_NKTaxType = "IT4";
					Factory.Save();
					AssertEquals(
						"If there are multiple VAT supplementary codes existing, when JI_ZZF_NKTaxType is set to a new value with empty additional code, all previous VAT additional codes should be removed.",
						1, invoiceLine.AdditionalSupplementaryCodes.Count);
					AssertEquals(
						"If there are multiple VAT supplementary codes existing, when JI_ZZF_NKTaxType is set to a new value with empty additional code, all previous VAT additional codes should be removed.",
						"S004,S001,Add4-,,LV", $"{string.Join(",", invoiceLine.SupplementaryCodes.Select(c => c.CY_Code))}-{string.Join(",", invoiceLine.SupplementaryCodes.Select(c => c.CY_Data))}");

					invoiceLine.JI_ZZF_NKTaxType = string.Empty;
					invoiceLine.JI_SupplementaryCode1 = ZString.Empty;
					invoiceLine.JI_SupplementaryCode2 = "S002";
					invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					invoiceLine.AdditionalSupplementaryCodes.AddNew("Add1");
					invoiceLine.AdditionalSupplementaryCodes.AddNew("Add2");
					Factory.Save();
					invoiceLine.JI_ZZF_NKTaxType = "IT4";
					Factory.Save();
					AssertEquals(
						"If there are multiple VAT supplementary codes existing, when JI_ZZF_NKTaxType is set to a new value with empty additional code, all previous VAT additional codes should be removed.",
						0, invoiceLine.AdditionalSupplementaryCodes.Count);
					AssertEquals(
						"If there are multiple VAT supplementary codes existing, when JI_ZZF_NKTaxType is set to a new value with empty additional code, all previous VAT additional codes should be removed.",
						"Add4,S002-LV,", $"{string.Join(",", invoiceLine.SupplementaryCodes.Select(c => c.CY_Code))}-{string.Join(",", invoiceLine.SupplementaryCodes.Select(c => c.CY_Data))}");
				});
			}

			void SetUpReferenceData()
			{
				var startDate = ZDateTime.Today.AddDays(-2);
				var endDate = ZDateTime.Today.AddDays(2);
				var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				var helper = new UniversalReferenceTestDataHelper(Factory);

				helper.CreateNewOrGetExistingDataGrouping(currentCountry).ZZZ_ZZZ_Grouping = helper.CreateNewOrGetExistingDataGrouping("EUN").PK;

				var impTariffTypePK = helper.CreateNewOrGetExistingTariffType("EUN", "IMP").PK;
				Factory.Save();
				var tariff = helper.CreateTariff(currentCountry, impTariffTypePK, "99999999", ZDateTime.BrettsBirthday,
					endDate, "Alpha Bravo");

				helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "IT1", additionalCode: "Add1", startDate, endDate);
				helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "IT2", additionalCode: "Add2", startDate, endDate);
				helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "IT3", additionalCode: "", startDate, endDate);
				helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "IT4", additionalCode: "Add4", startDate, endDate);

				helper.CreateTaxOrFee("IT1", 0.02m, currentCountry);
				helper.CreateTaxOrFee("IT2", 0.01m, currentCountry);
				helper.CreateTaxOrFee("IT3", 9999m, currentCountry);
				helper.CreateTaxOrFee("IT4", 99m, currentCountry);
				Factory.Save();
			}
		}

		public void TestSupplementaryCodeIsSet_WhenJI_TaxOrFeeDetailChanges_WhenUsingMultipleVatFields()
		{
			var invoiceLineConfigurationMock = new Mock<InvoiceLineConfiguration>();
			invoiceLineConfigurationMock.Protected()
				.Setup<ZBool>("UseMultipleVatFieldsCore")
				.Returns(true);

			using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(Factory, "GetNewInvoiceLineConfiguration", invoiceLineConfigurationMock.Object, null))
			{
				SetUpReferenceData();

				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_MessageType = "IMP";
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Tariff = "99999999";
				var taxOrFeeDetailEntityList = invoiceLine.Lookups.TaxOrFeeDetailEntities;

				CombineAssertions(() =>
				{
					var taxOrFeeDetailEntity =
						taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT1" && x.AdditionalCode == "ADD1");
					invoiceLine.JI_TaxOrFeeDetail = ZGuid.Empty;
					invoiceLine.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
					var invoiceLineType = invoiceLine.GetType();
					var supplementaryCode1 = (SupplementaryCode)invoiceLineType.GetProperty("SupplementaryCode1", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(invoiceLine);
					AssertEquals(
						"If there is no VAT supplementary code existing, and JI_SupplementaryCode1 is empty, the VAT additional code should be filled in the JI_SupplementaryCode1.",
						"ADD1-LV", $"{invoiceLine.JI_SupplementaryCode1}-{supplementaryCode1.CY_Data}");

					invoiceLine.JI_SupplementaryCode1 = "S001";
					taxOrFeeDetailEntity =
						taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT2" && x.AdditionalCode == "ADD2");
					invoiceLine.JI_TaxOrFeeDetail = ZGuid.Empty;
					invoiceLine.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
					var supplementaryCode2 = (SupplementaryCode)invoiceLineType.GetProperty("SupplementaryCode2", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(invoiceLine);
					AssertEquals(
						"If there is no VAT supplementary code existing, and JI_SupplementaryCode1 is filled while JI_SupplementaryCode2 is empty, the VAT additional code should be filled in the JI_SupplementaryCode2.",
						"ADD2-LV", $"{invoiceLine.JI_SupplementaryCode2}-{supplementaryCode2.CY_Data}");

					invoiceLine.JI_SupplementaryCode1 = "S001";
					invoiceLine.JI_SupplementaryCode2 = "S002";
					taxOrFeeDetailEntity =
						taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT1" && x.AdditionalCode == "ADD1");
					invoiceLine.JI_TaxOrFeeDetail = ZGuid.Empty;
					invoiceLine.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
					var invLineAdditionalSupplementaryCodes = invoiceLine.AdditionalSupplementaryCodes as BaseSupplementaryCodeCollection<SupplementaryCode>;
					AssertEquals(
						"If there is no VAT supplementary code existing, and JI_SupplementaryCode1 and JI_SupplementaryCode2 are both filled while AdditionalSupplementaryCodes is empty, the VAT additional code should be filled in the AdditionalSupplementaryCodes.",
						"ADD1-LV", $"{invLineAdditionalSupplementaryCodes.AsString}-{string.Join(",", invoiceLine.AdditionalSupplementaryCodes.Cast<SupplementaryCode>().OrderBy(x => x.CY_Order).Select(x => x.CY_Data))}");

					invoiceLine.JI_SupplementaryCode1 = "S001";
					invoiceLine.JI_SupplementaryCode2 = "S002";
					invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					invoiceLine.AdditionalSupplementaryCodes.AddNew("S003");
					taxOrFeeDetailEntity =
						taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT2" && x.AdditionalCode == "ADD2");
					invoiceLine.JI_TaxOrFeeDetail = ZGuid.Empty;
					invoiceLine.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
					AssertEquals(
						"If there is no VAT supplementary code existing, and JI_SupplementaryCode1 and JI_SupplementaryCode2 are both filled while AdditionalSupplementaryCodes is filled with a code, the VAT additional code should be appended to the end of AdditionalSupplementaryCodes.",
						"S003,ADD2-,LV", $"{invLineAdditionalSupplementaryCodes.AsString}-{string.Join(",", invoiceLine.AdditionalSupplementaryCodes.Cast<SupplementaryCode>().OrderBy(x => x.CY_Order).Select(x => x.CY_Data))}");

					invoiceLine.JI_SupplementaryCode1 = "ADD2";
					invoiceLine.JI_SupplementaryCode2 = "S002";
					invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					invoiceLine.AdditionalSupplementaryCodes.AddNew("S003");
					taxOrFeeDetailEntity =
						taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT1" && x.AdditionalCode == "ADD1");
					invoiceLine.JI_TaxOrFeeDetail = ZGuid.Empty;
					invoiceLine.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
					supplementaryCode1 = (SupplementaryCode)invoiceLineType.GetProperty("SupplementaryCode1", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(invoiceLine);
					AssertEquals(
						"If there is a VAT supplementary code existing at JI_SupplementaryCode1, it will be replaced with new VAT additional code.",
						"ADD1-LV", $"{invoiceLine.JI_SupplementaryCode1}-{supplementaryCode1.CY_Data}");

					invoiceLine.JI_SupplementaryCode1 = "ADD2";
					invoiceLine.JI_SupplementaryCode2 = "S002";
					invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					invoiceLine.AdditionalSupplementaryCodes.AddNew("S003");
					taxOrFeeDetailEntity =
						taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT2" && x.AdditionalCode == "ADD1");
					invoiceLine.JI_TaxOrFeeDetail = ZGuid.Empty;
					invoiceLine.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
					supplementaryCode1 = (SupplementaryCode)invoiceLineType.GetProperty("SupplementaryCode1", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(invoiceLine);
					AssertEquals(
						"If there is a VAT supplementary code existing at JI_SupplementaryCode1, it will be replaced with new VAT additional code.",
						"ADD1-LV", $"{invoiceLine.JI_SupplementaryCode1}-{supplementaryCode1.CY_Data}");

					invoiceLine.JI_SupplementaryCode1 = "S001";
					invoiceLine.JI_SupplementaryCode2 = "ADD1";
					invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					invoiceLine.AdditionalSupplementaryCodes.AddNew("S003");
					taxOrFeeDetailEntity =
						taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT2" && x.AdditionalCode == "ADD2");
					invoiceLine.JI_TaxOrFeeDetail = ZGuid.Empty;
					invoiceLine.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
					supplementaryCode2 = (SupplementaryCode)invoiceLineType.GetProperty("SupplementaryCode2", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(invoiceLine);
					AssertEquals(
						"If there is a VAT supplementary code existing at JI_SupplementaryCode2, it will be replaced with new VAT additional code.",
						"ADD2-LV", $"{invoiceLine.JI_SupplementaryCode2}-{supplementaryCode2.CY_Data}");

					invoiceLine.JI_SupplementaryCode1 = "S001";
					invoiceLine.JI_SupplementaryCode2 = "S002";
					invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					invoiceLine.AdditionalSupplementaryCodes.AddNew("ADD2");
					taxOrFeeDetailEntity =
						taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT1" && x.AdditionalCode == "ADD1");
					invoiceLine.JI_TaxOrFeeDetail = ZGuid.Empty;
					invoiceLine.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
					AssertEquals(
						"If there is a VAT supplementary code existing within AdditionalSupplementaryCodes as the only one code, it will be replaced with new VAT additional code.",
						"ADD1-LV", $"{invLineAdditionalSupplementaryCodes.AsString}-{string.Join(",", invoiceLine.AdditionalSupplementaryCodes.Cast<SupplementaryCode>().OrderBy(x => x.CY_Order).Select(x => x.CY_Data))}");

					invoiceLine.JI_SupplementaryCode1 = "S001";
					invoiceLine.JI_SupplementaryCode2 = "S002";
					invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					invoiceLine.AdditionalSupplementaryCodes.AddNew("ADD1");
					invoiceLine.AdditionalSupplementaryCodes.AddNew("S003");
					taxOrFeeDetailEntity =
						taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT2" && x.AdditionalCode == "ADD2");
					invoiceLine.JI_TaxOrFeeDetail = ZGuid.Empty;
					invoiceLine.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
					AssertEquals(
						"If there is a VAT supplementary code existing within AdditionalSupplementaryCodes as one of the members, it will be replaced with new VAT additional code.",
						"S003,ADD2-,LV", $"{invLineAdditionalSupplementaryCodes.AsString}-{string.Join(",", invoiceLine.AdditionalSupplementaryCodes.Cast<SupplementaryCode>().OrderBy(x => x.CY_Order).Select(x => x.CY_Data))}");

					invoiceLine.JI_SupplementaryCode1 = "S001";
					invoiceLine.JI_SupplementaryCode2 = "S002";
					invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					invoiceLine.AdditionalSupplementaryCodes.AddNew("S003");
					taxOrFeeDetailEntity =
						taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT3" && x.AdditionalCode == "");
					invoiceLine.JI_TaxOrFeeDetail = ZGuid.Empty;
					invoiceLine.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
					AssertEquals(
						"If there is no VAT supplementary code existing, when JI_TaxOrFeeDetail is set to a new value with empty VAT additional code, nothing should be done.",
						"S003,S001,S002-,,", $"{string.Join(",", invoiceLine.SupplementaryCodes.Select(c => c.CY_Code))}-{string.Join(",", invoiceLine.SupplementaryCodes.Select(c => c.CY_Data))}");

					invoiceLine.JI_ZZF_NKTaxType = string.Empty;
					invoiceLine.JI_SupplementaryCode1 = "ADD2";
					invoiceLine.JI_SupplementaryCode2 = "S002";
					invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					invoiceLine.AdditionalSupplementaryCodes.AddNew("S003");
					taxOrFeeDetailEntity =
						taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT3" && x.AdditionalCode == "");
					invoiceLine.JI_TaxOrFeeDetail = ZGuid.Empty;
					invoiceLine.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
					supplementaryCode1 = (SupplementaryCode)invoiceLineType.GetProperty("SupplementaryCode1", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(invoiceLine);
					AssertEquals(
						"If there is a VAT supplementary code existing, when JI_TaxOrFeeDetail is set to a new value with empty additional code, the VAT additional code should be removed.",
						ZString.Empty, invoiceLine.JI_SupplementaryCode1);
					AssertNull(
						"If there is a VAT supplementary code existing, when JI_TaxOrFeeDetail is set to a new value with empty additional code, the VAT additional code should be removed.",
						supplementaryCode1);
					AssertEquals(
						"If there is a VAT supplementary code existing, when JI_TaxOrFeeDetail is set to a new value with empty additional code, the VAT additional code should be removed.",
						"S003,S002-,", $"{string.Join(",", invoiceLine.SupplementaryCodes.Select(c => c.CY_Code))}-{string.Join(",", invoiceLine.SupplementaryCodes.Select(c => c.CY_Data))}");

					invoiceLine.JI_ZZF_NKTaxType = string.Empty;
					invoiceLine.JI_SupplementaryCode1 = "S001";
					invoiceLine.JI_SupplementaryCode2 = "S002";
					invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					invoiceLine.AdditionalSupplementaryCodes.AddNew("ADD2");
					invoiceLine.AdditionalSupplementaryCodes.AddNew("S004");
					taxOrFeeDetailEntity =
						taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT3" && x.AdditionalCode == "");
					invoiceLine.JI_TaxOrFeeDetail = ZGuid.Empty;
					invoiceLine.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
					AssertEquals(
						"If there is a VAT supplementary code existing, when JI_TaxOrFeeDetail is set to a new value with empty additional code, the VAT additional code should be removed.",
						1, invoiceLine.AdditionalSupplementaryCodes.Count);
					AssertEquals(
						"If there is a VAT supplementary code existing, when JI_TaxOrFeeDetail is set to a new value with empty additional code, the VAT additional code should be removed.",
						"S004,S001,S002-,,", $"{string.Join(",", invoiceLine.SupplementaryCodes.Select(c => c.CY_Code))}-{string.Join(",", invoiceLine.SupplementaryCodes.Select(c => c.CY_Data))}");

					invoiceLine.JI_ZZF_NKTaxType = string.Empty;
					invoiceLine.JI_SupplementaryCode1 = "ADD1";
					invoiceLine.JI_SupplementaryCode2 = "ADD2";
					invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					invoiceLine.AdditionalSupplementaryCodes.AddNew("S004");
					taxOrFeeDetailEntity =
						taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT4" && x.AdditionalCode == "ADD4");
					invoiceLine.JI_TaxOrFeeDetail = ZGuid.Empty;
					invoiceLine.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
					supplementaryCode1 = (SupplementaryCode)invoiceLineType.GetProperty("SupplementaryCode1", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(invoiceLine);
					supplementaryCode2 = (SupplementaryCode)invoiceLineType.GetProperty("SupplementaryCode2", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(invoiceLine);
					AssertEquals(
						"If there are multiple VAT supplementary codes existing, when JI_TaxOrFeeDetail is set to a new value with empty additional code, all previous VAT additional codes should be removed.",
						"ADD4-LV", $"{invoiceLine.JI_SupplementaryCode1}-{supplementaryCode1.CY_Data}");
					AssertEquals(
						"If there are multiple VAT supplementary codes existing, when JI_TaxOrFeeDetail is set to a new value with empty additional code, all previous VAT additional codes should be removed.",
						ZString.Empty, invoiceLine.JI_SupplementaryCode2);
					AssertNull(
						"If there are multiple VAT supplementary codes existing, when JI_TaxOrFeeDetail is set to a new value with empty additional code, all previous VAT additional codes should be removed.",
						supplementaryCode2);
					AssertEquals(
						"If there are multiple VAT supplementary codes existing, when JI_TaxOrFeeDetail is set to a new value with empty additional code, all previous VAT additional codes should be removed.",
						"S004,ADD4-,LV", $"{string.Join(",", invoiceLine.SupplementaryCodes.Select(c => c.CY_Code))}-{string.Join(",", invoiceLine.SupplementaryCodes.Select(c => c.CY_Data))}");

					invoiceLine.JI_ZZF_NKTaxType = string.Empty;
					invoiceLine.JI_SupplementaryCode1 = "S001";
					invoiceLine.JI_SupplementaryCode2 = "ADD1";
					invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					invoiceLine.AdditionalSupplementaryCodes.AddNew("ADD2");
					invoiceLine.AdditionalSupplementaryCodes.AddNew("S004");
					Factory.Save();
					taxOrFeeDetailEntity =
						taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT4" && x.AdditionalCode == "ADD4");
					invoiceLine.JI_TaxOrFeeDetail = ZGuid.Empty;
					invoiceLine.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
					Factory.Save();
					AssertEquals(
						"If there are multiple VAT supplementary codes existing, when JI_TaxOrFeeDetail is set to a new value with empty additional code, all previous VAT additional codes should be removed.",
						1, invoiceLine.AdditionalSupplementaryCodes.Count);
					AssertEquals(
						"If there are multiple VAT supplementary codes existing, when JI_TaxOrFeeDetail is set to a new value with empty additional code, all previous VAT additional codes should be removed.",
						"S004,S001,ADD4-,,LV", $"{string.Join(",", invoiceLine.SupplementaryCodes.Select(c => c.CY_Code))}-{string.Join(",", invoiceLine.SupplementaryCodes.Select(c => c.CY_Data))}");

					invoiceLine.JI_ZZF_NKTaxType = string.Empty;
					invoiceLine.JI_SupplementaryCode1 = ZString.Empty;
					invoiceLine.JI_SupplementaryCode2 = "S002";
					invoiceLine.AdditionalSupplementaryCodes.RemoveAndDeleteAll();
					invoiceLine.AdditionalSupplementaryCodes.AddNew("ADD1");
					invoiceLine.AdditionalSupplementaryCodes.AddNew("ADD2");
					Factory.Save();
					taxOrFeeDetailEntity =
						taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT4" && x.AdditionalCode == "ADD4");
					invoiceLine.JI_TaxOrFeeDetail = ZGuid.Empty;
					invoiceLine.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;
					Factory.Save();
					AssertEquals(
						"If there are multiple VAT supplementary codes existing, when JI_TaxOrFeeDetail is set to a new value with empty additional code, all previous VAT additional codes should be removed.",
						0, invoiceLine.AdditionalSupplementaryCodes.Count);
					AssertEquals(
						"If there are multiple VAT supplementary codes existing, when JI_TaxOrFeeDetail is set to a new value with empty additional code, all previous VAT additional codes should be removed.",
						"ADD4,S002-LV,", $"{string.Join(",", invoiceLine.SupplementaryCodes.Select(c => c.CY_Code))}-{string.Join(",", invoiceLine.SupplementaryCodes.Select(c => c.CY_Data))}");
				});
			}
			void SetUpReferenceData()
			{
				var startDate = ZDateTime.Today.AddDays(-2);
				var endDate = ZDateTime.Today.AddDays(2);
				var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				var helper = new UniversalReferenceTestDataHelper(Factory);

				helper.CreateNewOrGetExistingDataGrouping(currentCountry).ZZZ_ZZZ_Grouping = helper.CreateNewOrGetExistingDataGrouping("EUN").PK;

				var impTariffTypePK = helper.CreateNewOrGetExistingTariffType("EUN", "IMP").PK;
				Factory.Save();
				var tariff = helper.CreateTariff(currentCountry, impTariffTypePK, "99999999", ZDateTime.BrettsBirthday,
					endDate, "Alpha Bravo");

				helper.CreateTaxOrFee("VT1", 0.02m, currentCountry, description: "VAT One");
				helper.CreateTaxOrFee("VT2", 0.01m, currentCountry, description: "VAT Two");
				helper.CreateTaxOrFee("VT3", 0.03m, currentCountry, description: "VAT Three");
				helper.CreateTaxOrFee("VT3", 0.04m, currentCountry, description: "VAT Four");

				helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT1", startDate: startDate,
					endDate: endDate, additionalCode: "ADD1");
				helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT2", startDate: startDate,
					endDate: endDate, additionalCode: "ADD2");
				helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT2", startDate: startDate,
					endDate: endDate, additionalCode: "ADD1");
				helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT3", startDate: startDate,
					endDate: endDate, additionalCode: "");
				helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT4", startDate: startDate,
					endDate: endDate, additionalCode: "ADD4");

				helper.CreateCusCodeType("ADDIN", "Additional Code");
				helper.CreateCusCodeList(currentCountry, "ADDIN", "ADD1", "Test Additional Code 1",
					startDate: startDate, endDate: endDate);
				helper.CreateCusCodeList(currentCountry, "ADDIN", "ADD2", "Test Additional Code 2",
					startDate: startDate, endDate: endDate);
				helper.CreateCusCodeList(currentCountry, "ADDIN", "ADD4", "Test Additional Code 4",
					startDate: startDate, endDate: endDate);

				Factory.Save();
			}
		}

		public void TestGetCusProcedureReturnsCorrectCPCForCountry()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes.UnitedKingdom, "A", "11", "", "   ",
				"One", "EXP", group: "IFD");
			var procedure2 = helper.CreateRefCusProcedure(Core.Constants.CountryCodes._EUTemplateCountryName_, "A", "22", "", "   ", "Two",
				"EXP", group: "IFD");

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedKingdom);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = "CHF";
			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew();
			CombineAssertions(() =>
			{
				AssertNull(invoiceLine.CusProcedure);
				invoiceLine.JI_Procedure = "11";
				AssertEquals(procedure1, invoiceLine.CusProcedure);
				invoiceLine.JI_Procedure = "22";
				AssertNull(invoiceLine.CusProcedure);
			});

			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes._EUTemplateCountryName_);
			declaration = Factory.New<JobDeclaration>();
			header = declaration.Invoices.AddNew();
			invoiceLine = header.InvoiceLines.AddNew();
			CombineAssertions(() =>
			{
				AssertNull(invoiceLine.CusProcedure);
				invoiceLine.JI_Procedure = "11";
				AssertNull(invoiceLine.CusProcedure);
				invoiceLine.JI_Procedure = "22";
				AssertEquals(procedure2, invoiceLine.CusProcedure);
			});
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Analyzer suggests BaseSupplementaryCode.Loader, which is less readible")]
		public void TestISupplementaryCodeSupporterProperties()
		{
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var currentCountryCode = Core.Constants.CountryCodes.Latvia;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tradeGroup = helper.CreateTradeGroup(currentCountryCode, "STANDARD", startDate, endDate);
			var tariffType = helper.CreateNewOrGetExistingTariffType(currentCountryCode, "EXP");
			var dutyRateType = helper.CreateNewOrGetExistingRateType(currentCountryCode, Universal.Constants.RateTypes.Duty, "Duty");
			Factory.Save();
			var tariff1 = helper.CreateTariff(currentCountryCode, tariffType.PK, "11111111", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1));
			var tariff2 = helper.CreateTariff(currentCountryCode, tariffType.PK, "11111112", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1));
			var tariff3 = helper.CreateTariff(currentCountryCode, tariffType.PK, "11111113", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1));

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "ADDCD");
			helper.CreateCusCodeList(currentCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "TS1", startDate, endDate);
			helper.CreateCusCodeList(currentCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "TS2", startDate, endDate);
			helper.CreateCusCodeList(currentCountryCode, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes, "TS3", startDate, endDate);

			helper.CreateNewOrGetExistingVATApplicability(tariff1, currentCountryCode, "IT1", additionalCode: "TS1", startDate, endDate);

			var ctrlType = helper.CreateOrGetExistingRefCusConditionType(currentCountryCode, Core.Constants.Customs.Universal.RefCusConditionTypes.ConditionClass.Control, "TSTC", "Test Ctrl Condition Type");
			var condition = helper.CreateOrGetExistingRefCusCondition("C1", ctrlType.PK, tariff2.PK, "Direction:Export", true, false, startDate, endDate);

			helper.CreateCusApplicability(condition, tradeGroup, startDate, endDate, "TS2");

			var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "RC1", dutyRateType.PK);

			var rate = helper.CreateRate(tariff3, rateCode.PK, startDate, endDate, "0", dataGrouping: "R1");
			helper.CreateCusApplicability(rate, tradeGroup, startDate, endDate, "TS3");

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			var invoiceLineMock = Factory.NewMoq<JobComInvoiceLine>();
			invoiceLineMock.Protected().Setup<bool>("UseUniversalTariffCore").Returns(true);

			var invoiceLine = invoiceLineMock.Object;
			invoiceLine.JI_JZ = invoice.PK;
			invoiceLine.JI_Tariff = "11111111";
			var supporter = invoiceLine as ISupplementaryCodeSupporter;

			var additionalSupplementaryCode = invoiceLine.AdditionalSupplementaryCodes.AddNew();
			additionalSupplementaryCode.CY_Code = "1";
			var loader = new SupplementaryCode.Loader(Factory);
			var supplementaryCode1 = loader.LoadOrCreate<SupplementaryCode, JobComInvoiceLine>(invoiceLine, 1);

			CombineAssertions(() =>
			{
				AssertEquals("SupplementaryCodesFieldType", nameof(FieldType.TextDropEdit), supporter.SupplementaryCodesFieldType);
				AssertArrayEqualsByElements("SupplementaryCodes", new ZGuid[] { additionalSupplementaryCode.PK, supplementaryCode1.PK }, supporter.SupplementaryCodes.Select(x => x.PK).ToArray());
				AssertEquals("Tariff", tariff1.PK, supporter.Tariff.PK);
				AssertType<RateSelectionCriteria<JobComInvoiceLine>>("RateSelectionCriteria", supporter.RateSelectionCriteria);
				AssertEquals("GetCountryCodeForSupplementaryCodeProvider", Core.Constants.CountryCodes.Latvia, supporter.GetCountryCodeForCodeProvider());
				AssertEquals("CachedListOfAdditionalCodeDescriptions", "TS1, TS2, TS3", supporter.CachedListOfAdditionalCodeDescriptions.CodesAsString);
				AssertEquals("GetCountryCodeFromAdditionalCode", "LV", supporter.GetCountryCodeFromAdditionalCode("TS1"));
				invoiceLine.JI_Tariff = "11111112";
				AssertEquals("GetCountryCodeFromAdditionalCode", "C1", supporter.GetCountryCodeFromAdditionalCode("TS2"));
				invoiceLine.JI_Tariff = "11111113";
				AssertEquals("GetCountryCodeFromAdditionalCode", "R1", supporter.GetCountryCodeFromAdditionalCode("TS3"));
			});

			var invoiceLineMock2 = Factory.NewMoq<JobComInvoiceLine>();
			invoiceLineMock2.Protected().Setup<bool>("UseUniversalTariffCore").Returns(false);
			var invoiceLine2 = invoiceLineMock2.Object;
			invoiceLine2.JI_JZ = invoice.PK;
			var supporter2 = invoiceLine2 as ISupplementaryCodeSupporter;

			var additionalSupplementaryCode2 = invoiceLine2.AdditionalSupplementaryCodes.AddNew();
			additionalSupplementaryCode2.CY_Code = "1";
			var supplementaryCode2 = loader.LoadOrCreate<SupplementaryCode, JobComInvoiceLine>(invoiceLine2, 1);

			CombineAssertions(() =>
			{
				AssertEquals("SupplementaryCodesFieldType", nameof(FieldType.Text), supporter2.SupplementaryCodesFieldType);
				AssertArrayEqualsByElements("SupplementaryCodes", new ZGuid[] { additionalSupplementaryCode2.PK, supplementaryCode2.PK }, supporter2.SupplementaryCodes.Select(x => x.PK).ToArray());
				AssertEquals("Tariff", null, supporter2.Tariff);
				AssertType<RateSelectionCriteria<JobComInvoiceLine>>("RateSelectionCriteria", supporter2.RateSelectionCriteria);
				AssertEquals("GetCountryCodeForSupplementaryCodeProvider", Core.Constants.CountryCodes.Latvia, supporter2.GetCountryCodeForCodeProvider());
				AssertEquals("CachedListOfAdditionalCodeDescriptions", "TS1, TS2, TS3", supporter2.CachedListOfAdditionalCodeDescriptions.CodesAsString);
				AssertEquals("GetCountryCodeFromAdditionalCode", ZString.Empty, supporter2.GetCountryCodeFromAdditionalCode("TS1"));
				AssertEquals("GetCountryCodeFromAdditionalCode", ZString.Empty, supporter2.GetCountryCodeFromAdditionalCode("TS2"));
				AssertEquals("GetCountryCodeFromAdditionalCode", ZString.Empty, supporter2.GetCountryCodeFromAdditionalCode("TS3"));
			});
		}

		public void TestAntiDumpingRateSelectionCriteria()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_SupplementaryCode1 = "1";
			invoiceLine.JI_SupplementaryCode2 = "2";

			var antiDumpingCriteria = invoiceLine.AntiDumpingRateSelectionCriteria;
			AssertNotNull("AntiDumpingRateSelectionCriteria", antiDumpingCriteria);
			AssertType<RateSelectionCriteriaNoPrimaryPreference<JobComInvoiceLine>>("AntiDumpingRateSelectionCriteria type", antiDumpingCriteria);
			CombineAssertions("AntiDumpingRateSelectionCriteria", () =>
			{
				AssertEquals("RateType", "ADD", antiDumpingCriteria.RateType);
				AssertEquals("RateCode", "", antiDumpingCriteria.RateCode);
				AssertEquals("PrimaryReference", "", antiDumpingCriteria.PrimaryPreference);
				AssertArrayEqualsByElements("AdditionalCodes", new ZString[] { "1", "2" }, antiDumpingCriteria.AdditionalCodes.ToArray());
			});
		}

		public void TestCountervailingRateSelectionCriteria()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_SupplementaryCode1 = "1";
			invoiceLine.JI_SupplementaryCode2 = "2";

			var countervailingCriteria = invoiceLine.CountervailingRateSelectionCriteria;
			AssertNotNull("CountervailingRateSelectionCriteria", countervailingCriteria);
			AssertType<RateSelectionCriteriaNoPrimaryPreference<JobComInvoiceLine>>("CountervailingRateSelectionCriteria type", countervailingCriteria);
			CombineAssertions("CountervailingRateSelectionCriteria", () =>
			{
				AssertEquals("RateType", "CVD", countervailingCriteria.RateType);
				AssertEquals("RateCode", "", countervailingCriteria.RateCode);
				AssertEquals("PrimaryReference", "", countervailingCriteria.PrimaryPreference);
				AssertArrayEqualsByElements("AdditionalCodes", new ZString[] { "1", "2" }, countervailingCriteria.AdditionalCodes.ToArray());
			});
		}

		public void TestEffectiveSupportingDocumentsFallsBack()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();

			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN,
				new string[] { importCodeType, exportCodeType }, "RPTI", "RPTI DESC", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			AssertEquals(0, invoiceLine.EffectiveSupportingDocuments().Count);
			var supportDocument = invoiceHeader.SupportingDocuments.AddNew();
			supportDocument.CSI_Code = "RPTI";
			AssertEquals(0, invoiceLine.EffectiveSupportingDocuments().Count);

			supportDocument = invoiceLine.SupportingDocuments.AddNew();
			supportDocument.CSI_Code = "RPTI";
			AssertEquals(1, invoiceLine.EffectiveSupportingDocuments().Count);
		}

		public void TestAdditionalInfosValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportAdditionalInfoValidationDecider>(((IAdditionalInfosProviderWithValidationDecider)invoiceLine).ValidationDecider); // change to IAdditionalInfosProviderWithValidationDecider
			}
		}

		public void TestSupportingDocumentsValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportSupportingDocumentValidationDecider>(((ISupportingDocumentsProviderWithValidationDecider)invoiceLine).ValidationDecider);
			}
		}

		public void TestPreviousDocumentValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportPreviousDocumentValidationDecider>(((IPreviousDocumentsProviderWithValidationDecider)invoiceLine).ValidationDecider);
			}
		}

		public void TestCusFiscalReferenceValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportCusFiscalReferenceValidationDecider>(((ICusFiscalReferenceProviderWithValidationDecider)invoiceLine).ValidationDecider);
			}
		}

		public void TestCusAuthorizationUsageValidationDecider()
		{
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportCusAuthorizationUsageValidationDecider>(((ICusAuthorizationUsageProviderWithValidationDecider)invoiceLine).ValidationDecider);
			}
		}

		public void TestJI_NationalAdditionalCode1()
		{
			CombineAssertions(() =>
			{
				AssertResourceStringDataAttribute(
					typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_NationalAdditionalCode1),
					"Nat. Code 1",
					"National Code 1",
					"National Additional Code 1",
					"[18 09 060 000] National Additional Code 1",
					JobDeclaration.CaptionKeyExportUCC6
					);

				AssertResourceStringDataAttribute(
					typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_NationalAdditionalCode1),
					"Nat. Code 1",
					"National Code 1",
					"National Additional Code 1"
					);

				var info = Factory.New<JobComInvoiceLine>().JI_NationalAdditionalCode1Info;
				AssertEquals("MaxLength", 4, info.GetAttribute<MaxLengthAttribute>().MaxLength);
			});
		}

		public void TestJI_NationalAdditionalCode2()
		{
			CombineAssertions(() =>
			{
				AssertResourceStringDataAttribute(
					typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_NationalAdditionalCode2),
					"Nat. Code 2",
					"National Code 2",
					"National Additional Code 2",
					"[18 09 060 000] National Additional Code 2",
					JobDeclaration.CaptionKeyExportUCC6
					);

				AssertResourceStringDataAttribute(
					typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_NationalAdditionalCode2),
					"Nat. Code 2",
					"National Code 2",
					"National Additional Code 2"
					);

				var info = Factory.New<JobComInvoiceLine>().JI_NationalAdditionalCode2Info;
				AssertEquals("MaxLength", 4, info.GetAttribute<MaxLengthAttribute>().MaxLength);
			});
		}

		public void TestNationalAdditionalCodes()
		{
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var code1 = invoiceLine.NationalAdditionalCodes.AddNew();
			code1.CY_Code = "Code1";
			code1.CY_Order = 1;
			var code2 = invoiceLine.NationalAdditionalCodes.AddNew();
			code2.CY_Code = "Code2";
			code2.CY_Order = 2;
			AssertEquals("NationalAdditionalCodes", "Code1,Code2", invoiceLine.JI_NationalAdditionalCodes);
		}

		public void TestJI_NationalAdditionalCodes()
		{
			CombineAssertions(() =>
			{
				AssertResourceStringDataAttribute(
					typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_NationalAdditionalCodes),
					"Nat. Add. Codes",
					"National Add. Codes",
					"National Additional Codes",
					"[18 09 060 000] National Additional Codes",
					JobDeclaration.CaptionKeyExportUCC6
					);

				AssertResourceStringDataAttribute(
					typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_NationalAdditionalCodes),
					"Nat. Add. Codes",
					"National Add. Codes",
					"National Additional Codes"
					);
			});
		}

		public void TestJI_SupplementaryCode1()
		{
			CombineAssertions(() =>
			{
				AssertResourceStringDataAttribute(
					typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_SupplementaryCode1),
					"Add. Code 1",
					null,
					"Additional Code 1",
					"[18 09 059 000] TARIC Additional Code 1",
					JobDeclaration.CaptionKeyExportUCC6
					);

				AssertResourceStringDataAttribute(
					typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_SupplementaryCode1),
					caption: "Sup. Code 1",
					fullDescription: "Supplementary Code 1"
					);

				var info = Factory.New<JobComInvoiceLine>().JI_SupplementaryCode1Info;
				AssertEquals("MaxLength", 15, info.GetAttribute<MaxLengthAttribute>().MaxLength);
			});
		}

		public void TestJI_SupplementaryCode2()
		{
			CombineAssertions(() =>
			{
				AssertResourceStringDataAttribute(
					typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_SupplementaryCode2),
					"Add. Code 2",
					null,
					"Additional Code 2",
					"[18 09 059 000] TARIC Additional Code 2",
					JobDeclaration.CaptionKeyExportUCC6
					);

				AssertResourceStringDataAttribute(
					typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_SupplementaryCode2),
					caption: "Sup. Code 2",
					fullDescription: "Supplementary Code 2"
					);

				var info = Factory.New<JobComInvoiceLine>().JI_SupplementaryCode2Info;
				AssertEquals("MaxLength", 15, info.GetAttribute<MaxLengthAttribute>().MaxLength);
			});
		}

		public void TestJI_AdditionalSupplements()
		{
			CombineAssertions(() =>
			{
				AssertResourceStringDataAttribute(
					typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_AdditionalSupplements),
					caption: "Additional Codes",
					fullDescription: "[18 09 059 000] TARIC Additional Codes",
					multipleKey: JobDeclaration.CaptionKeyExportUCC6
					);

				AssertResourceStringDataAttribute(
					typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.JI_AdditionalSupplements),
					"Add. Sup. Codes",
					"Additional Sup. Codes",
					"Additional Supplementary Codes"
					);
			});
		}

		void AssertResourceStringDataAttribute(
			Type typeToCheck, string propertyName,
			string shortCaption = null,
			string mediumCaption = null,
			string caption = null,
			string fullDescription = null,
			string multipleKey = null
			)
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeToCheck, propertyName, false,
					x => x.ShortCaption == shortCaption &&
					x.MediumCaption == mediumCaption &&
					x.Caption == caption &&
					x.FullDescription == fullDescription &&
					x.MultipleKey == multipleKey
					);
		}

		public void TestIAdditionalInfosProvider()
		{
			AssertSame(invoiceLine.AdditionalInfos, ((IAdditionalInfosProvider)invoiceLine).AdditionalInfos);
		}

		public void TestRelatedIndicator_Caption()
		{
			AssertEquals("Party relationship, whether there is price influence or not", DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.RelatedIndicator)).Caption);
		}

		public void TestZG_RelatedIndicator2_Caption()
		{
			AssertEquals("Restrictions as to the disposal or use of the goods by the buyer in accordance with Article 70(3)(a) of the Code", DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.ZG_RelatedIndicator2)).Caption);
		}

		public void TestZG_RelatedIndicator3_Caption()
		{
			AssertEquals("Sale or price is subject to some condition or consideration in accordance with Article 70(3)(b) of the Code", DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.ZG_RelatedIndicator3)).Caption);
		}

		public void TestZG_RelatedIndicator4_Caption()
		{
			AssertEquals("The sale is subject to an arrangement under which part of the proceeds of any subsequent resale, disposal or use accrues directly or indirectly to the seller", DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.ZG_RelatedIndicator4)).Caption);
		}

		public void TestZG_RegionOfDestination_NonUcc6Caption()
		{
			AssertResourceStringDataAttribute(
				typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.ZG_RegionOfDestination),
				"Reg./Dest.",
				"Reg. of Dest.",
				"Region of Destination"
				);
		}

		public void TestZG_RegionOfDestination_ImportUCC6Caption()
		{
			AssertResourceStringDataAttribute(
				typeof(JobComInvoiceLine), nameof(JobComInvoiceLine.ZG_RegionOfDestination),
				shortCaption: "Dest. Region",
				caption: "Region of Destination",
				fullDescription: "[16 04 001 000] Region of Destination",
				multipleKey: JobDeclaration.CaptionKeyImportUCC6
				);
		}

		public void TestJI_TaxOrFeeDetail_PropertyInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertEquals(ZGuid.Empty, invoiceLine.JI_TaxOrFeeDetail);
			var property = DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_TaxOrFeeDetailInfo);
			AssertEquals("Caption", "VAT", property.Caption);
			AssertEquals("JI_TaxOrFeeDetail should not be read only.", false, invoiceLine.JI_TaxOrFeeDetailInfo.ReadOnly);
		}

		public void TestJI_TaxOrFeeDetail_ShouldBeMatchedWhenReloaded()
		{
			SetUpReferenceData();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.JE_MessageType = "IMP";
			invoiceLine.JI_Tariff = "99999999";
			var taxOrFeeDetailEntityList = invoiceLine.Lookups.TaxOrFeeDetailEntities;

			var taxOrFeeDetailEntity_VT1_Add1 = taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT1" && x.AdditionalCode == "ADD1");
			var taxOrFeeDetailEntity_VT2_Add1 = taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT2" && x.AdditionalCode == "ADD1");
			var taxOrFeeDetailEntity_VT2_EMPTY = taxOrFeeDetailEntityList.FirstOrDefault(x => x.VATCode == "VT2" && x.AdditionalCode == "");

			invoiceLine.JI_TaxOrFeeDetail = ZGuid.Empty;
			invoiceLine.JI_ZZF_NKTaxType = "VT1";
			invoiceLine.JI_SupplementaryCode1 = "ADD1";
			invoiceLine.OnLoaded();
			AssertEquals("When JI_TaxOrFeeDetail assigned to Empty, JI_TaxOrFeeDetail can be get according to JI_ZZF_NKTaxType and entered SupplementaryCodes.", invoiceLine.JI_TaxOrFeeDetail, taxOrFeeDetailEntity_VT1_Add1.PK);

			invoiceLine.JI_TaxOrFeeDetail = ZGuid.Empty;
			invoiceLine.JI_ZZF_NKTaxType = "VT2";
			invoiceLine.JI_SupplementaryCode1 = "ADD1";
			invoiceLine.OnLoaded();
			AssertEquals("When JI_TaxOrFeeDetail assigned to Empty, JI_TaxOrFeeDetail can be get according to JI_ZZF_NKTaxType and entered SupplementaryCodes.", invoiceLine.JI_TaxOrFeeDetail, taxOrFeeDetailEntity_VT2_Add1.PK);

			invoiceLine.JI_TaxOrFeeDetail = ZGuid.Empty;
			invoiceLine.JI_ZZF_NKTaxType = "VT2";
			invoiceLine.JI_SupplementaryCode1 = "";
			invoiceLine.OnLoaded();
			AssertEquals("When JI_TaxOrFeeDetail assigned to Empty, and SupplementaryCodes not entered, JI_TaxOrFeeDetail can be get by JI_ZZF_NKTaxType and empty additional code.", invoiceLine.JI_TaxOrFeeDetail, taxOrFeeDetailEntity_VT2_EMPTY.PK);

			void SetUpReferenceData()
			{
				var startDate = ZDateTime.Today.AddDays(-2);
				var endDate = ZDateTime.Today.AddDays(2);
				var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				var helper = new UniversalReferenceTestDataHelper(Factory);

				helper.CreateNewOrGetExistingDataGrouping(currentCountry).ZZZ_ZZZ_Grouping = helper.CreateNewOrGetExistingDataGrouping("EUN").PK;

				var impTariffTypePK = helper.CreateNewOrGetExistingTariffType("EUN", "IMP").PK;
				Factory.Save();
				var tariff = helper.CreateTariff(currentCountry, impTariffTypePK, "99999999", ZDateTime.BrettsBirthday,
					endDate, "Alpha Bravo");

				helper.CreateTaxOrFee("VT1", 0.02m, currentCountry, description: "VAT One");
				helper.CreateTaxOrFee("VT2", 0.01m, currentCountry, description: "VAT Two");

				helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT1", startDate: startDate,
					endDate: endDate, additionalCode: "ADD1");
				helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT2", startDate: startDate,
					endDate: endDate, additionalCode: "ADD1");
				helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT2", startDate: startDate,
					endDate: endDate, additionalCode: "");

				helper.CreateCusCodeType("ADDIN", "Additional Code");
				helper.CreateCusCodeList(currentCountry, "ADDIN", "ADD1", "Test Additional Code 1",
					startDate: startDate, endDate: endDate);

				Factory.Save();
			}
		}

		public void TestJI_ZZF_NKTaxType_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertEquals("JI_ZZF_NKTaxType should not be read only.", false, invoiceLine.JI_ZZF_NKTaxTypeInfo.ReadOnly);
		}

		public void TestShouldCheckMissingPreviousDocuments()
		{
			AssertEquals(true, invoiceLine.ShouldCheckMissingPreviousDocuments);
		}

		public void TestJI_RelatedIndicator2_ReadOnly()
		{
			AssertRelatedIndicatorReadOnly(invoice.RelatedIndicator2Info, invoiceLine.RelatedIndicator2Info);
		}

		public void TestJI_RelatedIndicator3_ReadOnly()
		{
			AssertRelatedIndicatorReadOnly(invoice.RelatedIndicator3Info, invoiceLine.RelatedIndicator3Info);
		}

		public void TestJI_RelatedIndicator4_ReadOnly()
		{
			AssertRelatedIndicatorReadOnly(invoice.RelatedIndicator4Info, invoiceLine.RelatedIndicator4Info);
		}

		public void TestRelatedIndicatorA()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_RelatedIndicator = RelatedIndicatorList.Codes.No;
				AssertEquals("No", false, invoiceLine.RelatedIndicator);
				invoiceLine.JI_RelatedIndicator = RelatedIndicatorList.Codes.Yes;
				AssertEquals("Yes", true, invoiceLine.RelatedIndicator);
				invoiceLine.JI_RelatedIndicator = "Z";
				AssertEquals("Invalid", false, invoiceLine.RelatedIndicator);
			});
		}

		public void TestRelatedIndicator_ReadOnly()
		{
			AssertRelatedIndicatorReadOnly(invoice.RelatedIndicatorInfo, invoiceLine.RelatedIndicatorInfo);
		}

		public void TestMultipleKeysToUse()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Interfaced;
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					AssertSequencesEqual("MultipleKeysToUse - UCC6 - Export", new[] { JobDeclaration.CaptionKeyExportUCC6 }, invoiceLine.MultipleKeysToUse);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					AssertSequencesEqual("MultipleKeysToUse - UCC6 - Import", new[] { JobDeclaration.CaptionKeyImportUCC6 }, invoiceLine.MultipleKeysToUse);
					declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
					AssertSequencesEqual("MultipleKeysToUse - UCC6 - MiscellaneousCustoms", new[] { JobDeclaration.CaptionKeyUCC }, invoiceLine.MultipleKeysToUse);
				}
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
				{
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					AssertSequencesEqual("MultipleKeysToUse - Non-UCC6 - Export", new[] { JobDeclaration.CaptionKeySAD }, invoiceLine.MultipleKeysToUse);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					AssertSequencesEqual("MultipleKeysToUse - Non-UCC6 - Import", new[] { JobDeclaration.CaptionKeySAD }, invoiceLine.MultipleKeysToUse);
				}
			});
		}

		public void TestRelatedIndicator2_ReadOnly()
		{
			AssertRelatedIndicatorReadOnly(invoice.RelatedIndicator2Info, invoiceLine.RelatedIndicator2Info);
		}

		public void TestRelatedIndicator3_ReadOnly()
		{
			AssertRelatedIndicatorReadOnly(invoice.RelatedIndicator3Info, invoiceLine.RelatedIndicator3Info);
		}

		public void TestRelatedIndicator4_ReadOnly()
		{
			AssertRelatedIndicatorReadOnly(invoice.RelatedIndicator4Info, invoiceLine.RelatedIndicator4Info);
		}

		public void TestJI_CountryOfOrigin_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			CombineAssertions("Export UCC6", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					var info = invoiceLine.JI_CountryOfOriginInfo;
					AssertEquals("HumanReadableName", "Country/Region of Origin", info.HumanReadableName);
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, invoiceLine.MultipleKeysToUse, "Country/Region of Origin", shortCaption: "Ctry./Rgn. of Orig.", mediumCaption: "Ctry./Rgn. of Origin", fullDescription: "Country/Region of Origin of the goods being moved.");
				}
			});

			CombineAssertions("Import UCC6", () =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					var info = invoiceLine.JI_CountryOfOriginInfo;
					AssertEquals("HumanReadableName", "Country of Origin", info.HumanReadableName);
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, invoiceLine.MultipleKeysToUse, "Country of Origin", shortCaption: "Origin", fullDescription: "[16 08 001 000] Country of origin");
				}
			});

			CombineAssertions("Not UCC6", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
				{
					var info = invoiceLine.JI_CountryOfOriginInfo;
					AssertEquals("HumanReadableName", "[34] Goods Origin", info.HumanReadableName);
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, invoiceLine.MultipleKeysToUse, "[34] Goods Origin", shortCaption: "Origin", mediumCaption: "[34] Origin", fullDescription: "Country/Region of Origin of the goods being moved.");
				}
			});
		}

		public void TestJI_RN_NKCountryOfExport_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			CombineAssertions("ExportUCC6", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					var info = invoiceLine.JI_RN_NKCountryOfExportInfo;
					AssertEquals("HumanReadableName", "Country/Region of Export", info.HumanReadableName);
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, invoiceLine.MultipleKeysToUse, "Country/Region of Export", shortCaption: "Ctry./Rgn. of Exp.", mediumCaption: "Ctry./Rgn. of Export", fullDescription: "Country/Region of Export of the goods being moved.");
				}
			});

			CombineAssertions("Not ExportUCC6", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
				{
					var info = invoiceLine.JI_RN_NKCountryOfExportInfo;
					AssertEquals("HumanReadableName", "Country/Region Of Export", info.HumanReadableName);
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, invoiceLine.MultipleKeysToUse, "Country/Region Of Export", "Ctry./Rgn. of Exp.");
				}
			});
		}

		public void TestJI_CustomsQuantity_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			CombineAssertions("ExportUCC6", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					var info = invoiceLine.JI_CustomsQuantityInfo;
					AssertEquals("HumanReadableName", "Net Weight in KG", info.HumanReadableName);
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, invoiceLine.MultipleKeysToUse, "Net Weight in KG");
				}
			});

			CombineAssertions("Not ExportUCC6", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
				{
					var info = invoiceLine.JI_CustomsQuantityInfo;
					AssertEquals("HumanReadableName", "[38] Customs Qty", info.HumanReadableName);
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, invoiceLine.MultipleKeysToUse, "[38] Customs Qty");
				}
			});
		}

		public void TestZG_CountryOfDestination_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			CombineAssertions("ExportUCC6", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					var info = invoiceLine.ZG_CountryOfDestinationInfo;
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, invoiceLine.MultipleKeysToUse, "Country of Destination", shortCaption: "Destination", mediumCaption: "Destination Country", fullDescription: "Country of Destination of the goods being moved.");
				}
			});

			CombineAssertions("Not ExportUCC6", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
				{
					var info = invoiceLine.ZG_CountryOfDestinationInfo;
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, invoiceLine.MultipleKeysToUse, "[UCC 5/8] Destination", shortCaption: "[UCC 5/8] Dest.");
				}
			});
		}

		public void TestDelete_Taxes()
		{
			CombineAssertions(() =>
			{
				var invoiceLine = Factory.New<JobComInvoiceLine>();
				var tax1 = invoiceLine.Taxes.AddNew();
				var tax2 = invoiceLine.Taxes.AddNew();
				AssertEquals("Has Records", 2, invoiceLine.Taxes.Count);
				invoiceLine.Delete();
				AssertEquals("tax1 deleted", true, tax1.IsDeleted);
				AssertEquals("tax2 deleted", true, tax2.IsDeleted);
			});
		}

		public void TestDelete_FiscalReferences()
		{
			CombineAssertions(() =>
			{
				var invoiceLine = Factory.New<JobComInvoiceLine>();
				var fiscalReference1 = invoiceLine.FiscalReferences.AddNew();
				var fiscalReference2 = invoiceLine.FiscalReferences.AddNew();
				AssertEquals("Has Records", 2, invoiceLine.FiscalReferences.Count);
				invoiceLine.Delete();
				AssertEquals("fiscalReference1 deleted", true, fiscalReference1.IsDeleted);
				AssertEquals("fiscalReference1 deleted", true, fiscalReference2.IsDeleted);
			});
		}

		public void TestJI_CustomsValue_Export()
		{
			// For exports, charges default to being distributed by mass, not value.
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
			var invoiceHeader = groupHeader.JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_IncoTerm = Core.Constants.IncoTerms.CostInsuranceAndFreight;
			invoiceHeader.JZ_InvoiceAmount = 1200.00m;
			invoiceHeader.JZ_Weight = 1200m;
			invoiceHeader.JZ_WeightUQ = Core.Constants.Weight.Kilograms;
			var currCode = declaration.LocalCurrencyCode;
			invoiceHeader.JZ_RX_NKInvoice_Currency = currCode;

			var charge1 = invoiceHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 160.00m, currCode);
			charge1.J7_IsIncludedInITOT = true;
			charge1.J7_IsDutiable = true;

			var charge2 = invoiceHeader.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasInsurance, 40.00m, currCode);
			charge2.J7_IsIncludedInITOT = true;
			charge2.J7_IsDutiable = false;

			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 1200m;
			invoiceLine.JI_Weight = 1200m;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;
			declaration.ResumeApportionment();
			AssertEquals(1160m, invoiceLine.JI_CustomsValue);
		}

		public void TestIsContainerisedMode_ULD()
		{
			invoiceLine.JI_ContainerMode = Core.Constants.ContainerModes.ULD;
			AssertEquals("JI_ContainerMode = 'ULD'", true, invoiceLine.IsContainerisedMode);
		}

		public void TestJI_ValuationCode_ResourceData()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_ValuationCodeInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[43] Valuation Method", resourceStringData.Caption);
				AssertEquals("MediumCaption", "Valuation Method", resourceStringData.MediumCaption);
				AssertEquals("ShortCaption", "Val. Method", resourceStringData.ShortCaption);
			});
		}

		public void TestJI_LinePrice_ResourceData()
		{
			CombineAssertions(() =>
			{
				var linePriceResourceData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_LinePriceInfo);
				AssertEquals("Caption", "[42] Price", linePriceResourceData.Caption);
				AssertEquals("Full Description", "Line price for line item.", linePriceResourceData.FullDescription);
			});
		}

		public void TestJI_CustomsSecondQuantity_Caption()
		{
			AssertEquals("[41] Supp. Qty", DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_CustomsSecondQuantityInfo).Caption);
		}

		public void TestJI_CustomsThirdQuantity_ResourceData()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_CustomsThirdQuantityInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[44] Third Qty", resourceStringData.Caption);
				AssertEquals("MediumCaption", "Third Qty", resourceStringData.MediumCaption);
				AssertEquals("ShortCaption", "3rd Qty", resourceStringData.ShortCaption);
			});
		}

		public void TestJI_CustomsFourthUnitQtyReadOnly()
		{
			AssertEquals("Default ReadOnly value of JI_CustomsFourthUnitQty is false", false, invoiceLine.JI_CustomsFourthUnitQtyInfo.ReadOnly);
		}

		public void TestJI_CustomsFifthUnitQtyReadOnly()
		{
			AssertEquals("Default ReadOnly value of JI_CustomsFifthUnitQty is false", false, invoiceLine.JI_CustomsFifthUnitQtyInfo.ReadOnly);
		}

		public void TestJI_Weight_ResourceData()
		{
			CombineAssertions(() =>
			{
				var weightResourceData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_WeightInfo);
				AssertEquals("Caption", "[35] Gross Weight", weightResourceData.Caption);
				AssertEquals("MediumCaption", "[35] GWT", weightResourceData.MediumCaption);
				AssertEquals("Short Caption", "GWT", weightResourceData.ShortCaption);
			});
		}

		public void TestJI_FormattedProcedure()
		{
			invoiceLine.JI_FormattedProcedure = "123.4567";
			AssertEquals("123.4567", invoiceLine.JI_FormattedProcedure);
		}

		public void TestJI_FormattedProcedure_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(invoiceLine.JI_FormattedProcedureInfo, invoiceLine.MultipleKeysToUse, "Procedure", shortCaption: string.Empty, mediumCaption: string.Empty, fullDescription: "[11 09 000 000] Procedure");
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(invoiceLine.JI_FormattedProcedureInfo, invoiceLine.MultipleKeysToUse, "[37] Procedure Code", shortCaption: "CPC", mediumCaption: "[37] CPC", fullDescription: string.Empty);
				}
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
				{
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(invoiceLine.JI_FormattedProcedureInfo, invoiceLine.MultipleKeysToUse, "[37] Procedure Code", shortCaption: "CPC", mediumCaption: "[37] CPC", fullDescription: string.Empty);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(invoiceLine.JI_FormattedProcedureInfo, invoiceLine.MultipleKeysToUse, "[37] Procedure Code", shortCaption: "CPC", mediumCaption: "[37] CPC", fullDescription: string.Empty);
				}
			});
		}

		public void TestJI_Procedure_Caption()
		{
			CombineAssertions(() =>
			{
				var weightResourceData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_ProcedureInfo);
				AssertEquals("Caption", "[37] Procedure Code", weightResourceData.Caption);
				AssertEquals("MediumCaption", "[37] CPC", weightResourceData.MediumCaption);
				AssertEquals("Short Caption", "CPC", weightResourceData.ShortCaption);
			});
		}

		public void TestAdditionalProcedureCodesAsString_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(invoiceLine.AdditionalProcedureCodesAsStringInfo, invoiceLine.MultipleKeysToUse, "Additional Procedures", shortCaption: string.Empty, mediumCaption: "Add. Procedures", fullDescription: "[11 10 000 000] Additional Procedures");
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(invoiceLine.AdditionalProcedureCodesAsStringInfo, invoiceLine.MultipleKeysToUse, "Additional Procedure Codes", shortCaption: "Add. CPCs", mediumCaption: "Add. Procedure Codes", fullDescription: string.Empty);
				}
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
				{
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(invoiceLine.AdditionalProcedureCodesAsStringInfo, invoiceLine.MultipleKeysToUse, "Additional Procedure Codes", shortCaption: "Add. CPCs", mediumCaption: "Add. Procedure Codes", fullDescription: string.Empty);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(invoiceLine.AdditionalProcedureCodesAsStringInfo, invoiceLine.MultipleKeysToUse, "Additional Procedure Codes", shortCaption: "Add. CPCs", mediumCaption: "Add. Procedure Codes", fullDescription: string.Empty);
				}
			});
		}

		public void TestProcedureMustBeEnteredForAdditionalProceduresSelectionErrorMessage()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					AssertEquals("UCC6 - Export", "To select additional procedure codes the procedure/CPC must be filled in.", invoiceLine.ProcedureMustBeEnteredForAdditionalProceduresSelectionErrorMessage);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					AssertEquals("UCC6 - Import", "To select additional procedure codes (Box 37.2) the CPC must be filled in", invoiceLine.ProcedureMustBeEnteredForAdditionalProceduresSelectionErrorMessage);
				}
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
				{
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					AssertEquals("Non-UCC6 - Export", "To select additional procedure codes (Box 37.2) the CPC must be filled in", invoiceLine.ProcedureMustBeEnteredForAdditionalProceduresSelectionErrorMessage);
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					AssertEquals("Non-UCC6 - Import", "To select additional procedure codes (Box 37.2) the CPC must be filled in", invoiceLine.ProcedureMustBeEnteredForAdditionalProceduresSelectionErrorMessage);
				}
			});
		}

		public void TestJI_PrimaryPreference_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			CombineAssertions("Import UCC6", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					var info = invoiceLine.JI_PrimaryPreferenceInfo;
					AssertEquals("HumanReadableName", "Preference", info.HumanReadableName);
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, invoiceLine.MultipleKeysToUse, "Preference", fullDescription: "[14 11 001 000] Preference");
				}
			});

			CombineAssertions("Not UCC6", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
				{
					var info = invoiceLine.JI_PrimaryPreferenceInfo;
					AssertEquals("HumanReadableName", "Preference", info.HumanReadableName);
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, invoiceLine.MultipleKeysToUse, "Preference", shortCaption: "Pref. Code");
				}
			});
		}

		public void TestJI_AdditionalSupplements_Caption()
		{
			var property = DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_AdditionalSupplementsInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Additional Supplementary Codes", property.Caption);
				AssertEquals("MediumCaption", "Additional Sup. Codes", property.MediumCaption);
				AssertEquals("ShortCaption", "Add. Sup. Codes", property.ShortCaption);
			});
		}

		public void TestJI_ZZF_NKTaxType_Caption()
		{
			AssertEquals("VAT", DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_ZZF_NKTaxTypeInfo).Caption);
		}

		public void TestTaxOrFeeAndSupplementaryCodeAreSetWhenJI_TaxOrFeeDetailChanges()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			AssertEquals(ZString.Empty, invoiceLine.JI_ZZF_NKTaxType);
			AssertEquals(ZString.Empty, invoiceLine.JI_SupplementaryCode1);

			var taxOrFeeDetailEntity = new TaxOrFeeDetailEntity();
			taxOrFeeDetailEntity.VATCode = "RED";
			taxOrFeeDetailEntity.Description = "Reduced, V904, A505";
			taxOrFeeDetailEntity.AdditionalCode = "V904";
			taxOrFeeDetailEntity.Category = "A505";
			invoiceLine.Lookups.TaxOrFeeDetailEntities.Add(taxOrFeeDetailEntity);
			invoiceLine.JI_TaxOrFeeDetail = taxOrFeeDetailEntity.PK;

			AssertEquals("RED", invoiceLine.JI_ZZF_NKTaxType);
			AssertEquals("V904", invoiceLine.JI_SupplementaryCode1);
		}

		[TestDate(2021, 7, 21)]
		public void TestGetEffectiveVATApplicabilities()
		{
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(currentCountry).ZZZ_ZZZ_Grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN).PK;
			var impTariffTypePK = helper.CreateNewOrGetExistingTariffType(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Enterprise.Customs.Business.UniversalReferenceConstants.CusTariffTypes.ImportTariff).PK;
			Factory.Save();

			helper.CreateTaxOrFee("VT1", 0.02m, currentCountry, description: "VAT One");
			helper.CreateTaxOrFee("VT2", 0.01m, currentCountry, description: "VAT Two");
			helper.CreateTaxOrFee("VT3", 9999m, currentCountry, description: "VAT Three");

			var tariff = helper.CreateTariff(currentCountry, impTariffTypePK, "99999999", ZDateTime.BrettsBirthday, ZDateTime.Today, "Alpha Bravo", compositeKey: "99...99.99");
			helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT1", startDate: startDate, endDate: endDate, additionalCode: "Z001");
			helper.CreateNewOrGetExistingVATApplicability(tariff, currentCountry, "VT2", startDate: startDate, endDate: endDate);
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "99999999";

			AssertContainsExactElementsInAnyOrder("Effective Vat Applicabilites", new ZString[] { "VT1", "VT2" }, invoiceLine.GetEffectiveVATApplicabilities().Select(x => x.ZX5_ZZF_NKTaxOrFeeCode).ToArray());
		}

		public void TestGetEffectiveValueToReturn()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = Factory.New<JobComInvLineForTest>();
			invoiceLine.JI_JZ = invoice.PK;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var instructionLine = Factory.New<JobComInvLineForTest>();
			invoiceLine.JI_CEI = instruction.PK;

			CombineAssertions(() =>
			{
				invoice.JZ_Description = "Header Description";
				AssertEquals("Get value from Header", "Header Description", invoiceLine.GetInvoiceEffectiveValueToReturnIfNeeded(ZString.Empty, JobComInvLineForTest.Schema.JI_Description, JobComInvoiceHeader.Schema.JZ_Description));
				AssertEquals("Get value from itself", "Line Description", invoiceLine.GetInvoiceEffectiveValueToReturnIfNeeded(new ZString("Line Description"), JobComInvLineForTest.Schema.JI_Description, JobComInvoiceHeader.Schema.JZ_Description));

				instruction.CEI_Description = "Instruction Description";
				AssertEquals("Get value from Instruction", "Instruction Description", invoiceLine.GetEntryInstructionEffectiveValueToReturnIfNeeded(ZString.Empty, JobComInvLineForTest.Schema.JI_Description, CusEntryInstruction.Schema.CEI_Description));
				AssertEquals("Get value from itself", "Line Description", invoiceLine.GetEntryInstructionEffectiveValueToReturnIfNeeded(new ZString("Line Description"), JobComInvLineForTest.Schema.JI_Description, CusEntryInstruction.Schema.CEI_Description));

				declaration.JE_GoodsDescription = "Declaration Description";
				AssertEquals("Get value from Declaration", "Declaration Description", invoiceLine.GetDeclarationEffectiveValueToReturnIfNeeded(ZString.Empty, JobComInvLineForTest.Schema.JI_Description, JobDeclaration.Schema.JE_GoodsDescription));
				AssertEquals("Get value from itself", "Line Description", invoiceLine.GetDeclarationEffectiveValueToReturnIfNeeded(new ZString("Line Description"), JobComInvLineForTest.Schema.JI_Description, JobDeclaration.Schema.JE_GoodsDescription));
			});
		}

		public void TestGetEffectiveValueToSet()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = Factory.New<JobComInvLineForTest>();
			invoiceLine.JI_JZ = invoice.PK;

			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var instructionLine = Factory.New<JobComInvLineForTest>();
			invoiceLine.JI_CEI = instruction.PK;

			CombineAssertions(() =>
			{
				invoice.JZ_Description = "Header Description";
				AssertEquals("Pass value same with parent will return default value", ZString.Empty, invoiceLine.GetEffectiveValueToSetCompareToInvoice(new ZString("Header Description"), JobComInvoiceHeader.Schema.JZ_Description));
				AssertEquals("Pass value different from parent will return itself", "Line Description", invoiceLine.GetEffectiveValueToSetCompareToInvoice(new ZString("Line Description"), JobComInvoiceHeader.Schema.JZ_Description));

				instruction.CEI_Description = "Instruction Description";
				AssertEquals("Pass value same with parent will return default value", ZString.Empty, invoiceLine.GetEffectiveValueToSetCompareToEntryInstruction(new ZString("Instruction Description"), CusEntryInstruction.Schema.CEI_Description));
				AssertEquals("Pass value same with parent will return default value", "Line Description", invoiceLine.GetEffectiveValueToSetCompareToEntryInstruction(new ZString("Line Description"), CusEntryInstruction.Schema.CEI_Description));

				declaration.JE_GoodsDescription = "Declaration Description";
				AssertEquals("Pass value same with parent will return default value", ZString.Empty, invoiceLine.GetEffectiveValueToSetCompareToDeclaration(new ZString("Declaration Description"), JobDeclaration.Schema.JE_GoodsDescription));
				AssertEquals("Pass value same with parent will return default value", "Line Description", invoiceLine.GetEffectiveValueToSetCompareToDeclaration(new ZString("Line Description"), JobDeclaration.Schema.JE_GoodsDescription));
			});
		}

		public void TestZG_ValueAdjustmentCode_MaxLength()
		{
			AssertEquals(1, InvoiceLine.ZG_ValueAdjustmentCodeInfo.MaxLength);
		}

		public void TestFiscalReferences()
		{
			CombineAssertions(() =>
			{
				var fiscalReferences = InvoiceLine.FiscalReferences;
				AssertEquals("IsRegisteredEditableChildObject", true, InvoiceLine.IsRegisteredEditableChildObject(fiscalReferences));
				AssertSame("Cached", fiscalReferences, InvoiceLine.FiscalReferences);
			});
		}

		public void TestCustomsUnitDefaultingStrategy_AttachingInvoiceLineToInvoiceHeader()
		{
			var invoiceLine = Factory.New<JobComInvoiceLineForTest>();
			AssertType<UniversalRateCustomsUnitDefaultingStrategy<JobComInvoiceLine>>(invoiceLine.GetCustomsUnitDefaultingStrategyExposed());
		}

		public void TestZG_CountryOfSupply_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var info = invoiceLine.ZG_CountryOfSupplyInfo;

			CombineAssertions("Import UCC6", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, invoiceLine.MultipleKeysToUse, "Pref. Origin", shortCaption: "Pref. Ctry.", fullDescription: "[16 09 001 000] Country of preferential origin");
				}
			});

			CombineAssertions("Not UCC6", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
				{
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, invoiceLine.MultipleKeysToUse, "Country of Supply");
				}
			});
		}

		public void TestZG_CountryOfSupplyMaxLength() => AssertEquals(2, InvoiceLine.ZG_CountryOfSupplyInfo.MaxLength);

		public void TestCusSupplyChainActorReferences()
		{
			CombineAssertions(() =>
			{
				var cusSupplyChainActorReferences = InvoiceLine.CusSupplyChainActorReferences;
				AssertEquals("IsRegisteredEditableChildObject", true, InvoiceLine.IsRegisteredEditableChildObject(cusSupplyChainActorReferences));
				AssertType<CusSupplyChainActorReferenceCollection<CusSupplyChainActorReference>>("Type", cusSupplyChainActorReferences);
				AssertSame("Cached", cusSupplyChainActorReferences, InvoiceLine.CusSupplyChainActorReferences);
			});
		}

		public void TestIPreviousDocumentsProviderMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			invoiceLine.SupportingDocuments.AddNew();
			var previousDocumentsProvider = (IPreviousDocumentsProvider)invoiceLine;

			AssertNotNull("IPreviousDocumentsProvider.PreviousDocuments must be not null", previousDocumentsProvider.PreviousDocuments);
			AssertSame("IPreviousDocumentsProvider.PreviousDocuments must be the same of PreviousDocuments", invoiceLine.PreviousDocuments, previousDocumentsProvider.PreviousDocuments);
		}

		public void TestCusAuthorizationUsages()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			AssertType<CusAuthorizationUsageCollection<CusAuthorizationUsage, JobComInvoiceLine>>(invoiceLine.CusAuthorizationUsages);
		}

		public void TestBuyer()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var jobDocAddress = Factory.New<JobDocAddress>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			jobDocAddress.E2_AddressType = DocAddressTypes.Codes.BuyingParty;
			jobDocAddress.OrganisationPK = orgHeader.PK;
			jobDocAddress.E2_ParentTableCode = "JI";
			jobDocAddress.E2_ParentID = invoiceLine.PK;
			AssertEquals("Buying party is correctly retrieved", invoiceLine.BuyerDocAddress.OrganisationPK, jobDocAddress.OrganisationPK);
		}

		public void TestSeller()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			var jobDocAddress = Factory.New<JobDocAddress>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			jobDocAddress.E2_AddressType = DocAddressTypes.Codes.SellingParty;
			jobDocAddress.OrganisationPK = orgHeader.PK;
			jobDocAddress.E2_ParentTableCode = "JI";
			jobDocAddress.E2_ParentID = invoiceLine.PK;
			AssertEquals("Selling party is correctly retrieved", invoiceLine.SellerDocAddress.OrganisationPK, jobDocAddress.OrganisationPK);
		}

		public void TestStatisticalValueSTA()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = dec.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = dec.LocalCurrencyCode;
			invoice.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			invoice.JZ_InvoiceAmount = 1000m;
			invoiceLine.JI_LinePrice = 1000m;

			AssertEquals(invoiceLine.JI_Calc_StatisticalValue, 1000m);
			invoiceLine.ZG_StatisticalValueManualOverride = false;

			var insuranceCharge = invoiceLine.Charges.AddNew();
			insuranceCharge.J7_ChargeType = OverseasInsuranceCode;
			insuranceCharge.J7_RX_NKCurrency = dec.LocalCurrencyCode;
			insuranceCharge.J7_Amount = 100m;
			Assert("Pre-Req - insurance is STAT-able", insuranceCharge.J7_IsStatisticalValueApplicable);

			AssertEquals("Stat value  = 1000+100= 1100", 1100m, invoiceLine.JI_Calc_StatisticalValue);
			AssertEquals(1100m, invoiceLine.JI_Calc_StatisticalBasisExcludingSTACharge);

			var nonStatCharge = invoiceLine.Charges.AddNew();
			nonStatCharge.J7_ChargeType = ChargeTypeList.Codes.StatisticalValue;
			nonStatCharge.J7_RX_NKCurrency = dec.LocalCurrencyCode;
			nonStatCharge.J7_Amount = 50m;
			nonStatCharge.J7_IsStatisticalValueApplicable = true;

			AssertEquals(1150m, invoiceLine.JI_Calc_StatisticalValue);
			AssertEquals(1100m, invoiceLine.JI_Calc_StatisticalBasisExcludingSTACharge);
		}

		public void TestEmptyTaxTypeIfNotImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

			AssertEquals("[PRE-CONDITION] invoiceLine.IsImport", true, invoiceLine.IsImport);
			invoiceLine.JI_ZZF_NKTaxType = "VAT";
			invoiceLine.EmptyTaxTypeIfNotImport();
			AssertEquals("JI_ZZF_NKTaxType", "VAT", invoiceLine.JI_ZZF_NKTaxType);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("[PRE-CONDITION] invoiceLine.IsImport", false, invoiceLine.IsImport);
			invoiceLine.JI_ZZF_NKTaxType = "VEX";
			invoiceLine.EmptyTaxTypeIfNotImport();
			AssertEquals("JI_ZZF_NKTaxType", "", invoiceLine.JI_ZZF_NKTaxType);
		}

		public void TestCaptionOfPaymentProperties()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			CombineAssertions("Payment Properties Caption", () =>
			{
				AssertCaption(invoiceLine.ZG_CommercialPaymentCodeInfo, "Payment Code", "Payment Code");
				AssertCaption(invoiceLine.ZG_CommercialPaymentAmountInfo, "Amount", "Amount");
				AssertCaption(invoiceLine.ZG_CommercialPaymentNumberInfo, "Payment Reference", "Payment Reference");
				AssertCaption(invoiceLine.ZG_CommercialPaymentDateInfo, "Payment Ref.Date", "Payment Ref.Date");
			});
		}

		void AssertCaption(ZPropertyInfo info, string shortCaption, string caption)
		{
			var propertyData = DataBoundResourceStrings.GetDataForProperty(info);
			AssertEquals($"{info.Name} Caption", caption, propertyData.Caption);
			AssertEquals($"{info.Name} ShortCaption", shortCaption, propertyData.ShortCaption);
		}

		public void TestZG_CommercialPaymentNumberMaxLength()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			AssertEquals("ZG_CommercialPaymentNumber", 20, invoiceLine.ZG_CommercialPaymentNumberInfo.MaxLength);
		}

		public void TestJI_Tariff_ResourceData()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(invoiceLine.JI_TariffInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[33] Tariff", resourceStringData.Caption);
				AssertEquals("ShortCaption", "Tariff", resourceStringData.ShortCaption);
			});
		}

		public void TestJI_TariffMaxLength() => AssertEquals(16, Factory.New<JobComInvoiceLine>().JI_TariffInfo.MaxLength);

		public void TestCusProcedureSupportsCommaSeparatedShipmentType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure1 = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, "A", "11", "", "   ", "One", "IMP,EXP", group: "IFD");
			var procedure2 = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, "A", "22", "", "   ", "Two", "EXP, MSC", group: "IFD");

			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var invoiceLine = header.InvoiceLines.AddNew();

			invoiceLine.JI_Procedure = "11";

			declaration.JE_MessageType = "EXP";
			CombineAssertions("When Message Type is EXP and Procedure: 11", () =>
			{
				AssertNotNull(nameof(invoiceLine.CusProcedure), invoiceLine.CusProcedure);
				AssertEquals(nameof(invoiceLine.CusProcedure), procedure1.PK, invoiceLine.CusProcedure.PK);
			});

			declaration.JE_MessageType = "IMP";
			CombineAssertions("When Message Type is IMP and Procedure: 11", () =>
			{
				AssertNotNull(nameof(invoiceLine.CusProcedure), invoiceLine.CusProcedure);
				AssertEquals(nameof(invoiceLine.CusProcedure), procedure1.PK, invoiceLine.CusProcedure.PK);
			});

			declaration.JE_MessageType = "MSC";
			AssertNull("When Message Type is MSC and Procedure: 11", invoiceLine.CusProcedure);

			invoiceLine.JI_Procedure = "22";
			declaration.JE_MessageType = "MSC";
			CombineAssertions("When Message Type is MSC and Procedure: 22", () =>
			{
				AssertNotNull(nameof(invoiceLine.CusProcedure), invoiceLine.CusProcedure);
				AssertEquals(nameof(invoiceLine.CusProcedure), procedure2.PK, invoiceLine.CusProcedure.PK);
			});
		}

		public void TestCheckJI_CustomsUnitQty_IsReadOnly()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType("EUN", "IMP");
			Factory.Save();

			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "11111111", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1));
			helper.CreateTariffUOM(tariff1, Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "CU1");
			helper.CreateTariffUOM(tariff1, Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, "CU2");
			helper.CreateTariffUOM(tariff1, Universal.Constants.UnitOfMeasureTypes.CustomsUOM3Type, "CU3");

			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "22222222", ZDateTime.BrettsBirthday, ZDateTime.Today.AddDays(1));
			helper.CreateTariffUOM(tariff2, Universal.Constants.UnitOfMeasureTypes.ClassificationUOMType, "RX1");

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();

			var invoiceLineMock = Factory.NewMoq<JobComInvoiceLine>();
			invoiceLineMock.Protected().Setup<bool>("UseUniversalTariffCore").Returns(true);
			invoiceLineMock.Setup(i => i.UniversalTariff).Returns(tariff1);

			var invoiceLine = invoiceLineMock.Object;
			invoiceLine.JI_JZ = invoice.PK;
			CombineAssertions(() =>
			{
				invoiceLine.JI_Tariff = tariff1.ZZ1_TariffCode;
				AssertEquals("Universal Tariff has a tariff unit of measure - so returns true", true, invoiceLine.JI_CustomsUnitQty_ReadOnly);

				invoiceLineMock.Setup(i => i.UniversalTariff).Returns(tariff2);
				invoiceLine.JI_Tariff = tariff2.ZZ1_TariffCode;
				AssertEquals("Universal Tariff does not has a tariff unit of measure - so returns false", false, invoiceLine.JI_CustomsUnitQty_ReadOnly);
			});
		}

		public void TestAuthorisationAreDeletedWhenParentInvoiceLineIsDeleted()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var inv = dec.Invoices.AddNew();
			inv.FillWithValidTestData();
			var invLine = inv.InvoiceLines.AddNew();
			invLine.FillWithValidTestData();

			var auth = invLine.CusAuthorizationUsages.AddNew();
			auth.FillWithValidTestData();
			Factory.Save();

			invLine.Delete();
			AssertEquals(true, auth.IsDeleted);
			AssertNoExceptionThrown(() => Factory.Save());
		}

		public void TestBuyerJobDocAddressAdditionalValidationType()
		{
			AssertType<InvoiceLineTraderJobDocAddressValidation>("BuyerJobDocAddressAdditionalValidation type", InvoiceLine.BuyerDocAddress.AdditionalValidation);
		}

		public void TestSellerJobDocAddressAdditionalValidationType()
		{
			AssertType<InvoiceLineTraderJobDocAddressValidation>("SellerJobDocAddressAdditionalValidation type", InvoiceLine.SellerDocAddress.AdditionalValidation);
		}

		public void TestSuspendCalculateFromNetWeightToCustomsQty()
		{
			invoiceLine.JI_CustomsQuantity = 99m;
			invoiceLine.JI_CustomsUnitQty = "ASV";

			using (invoiceLine.SuspendCalculateFromNetWeightToCustomsQty())
			{
				invoiceLine.JI_NetWeight = 1m;
				invoiceLine.JI_NetWeightUQ = "KG";
			}

			CombineAssertions(() =>
			{
				AssertEquals("JI_CustomsQuantity", 99m, invoiceLine.JI_CustomsQuantity);
				AssertEquals("JI_CustomsUnitQty", "ASV", invoiceLine.JI_CustomsUnitQty);
			});
		}

		#region IUcc6ValueProvider

		public void TestIUcc6ValueProvider_IsUCC6()
		{
			IUcc6ValueProvider ucc6ValueProvider = InvoiceLine;

			AssertEquals("Non-UCC6", false, ucc6ValueProvider.IsUCC6);

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(Declaration, true))
			{
				AssertEquals("UCC6", true, ucc6ValueProvider.IsUCC6);
			}
		}

		public void TestIUcc6ValueProvider_IsExport()
		{
			IUcc6ValueProvider ucc6ValueProvider = InvoiceLine;
			Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertEquals("EXP - IsExport", true, ucc6ValueProvider.IsExport);

			Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertEquals("IMP - IsExport", false, ucc6ValueProvider.IsExport);
		}

		public void TestIUcc6ValueProvider_IsImport()
		{
			IUcc6ValueProvider ucc6ValueProvider = InvoiceLine;
			Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
			AssertEquals("EXP - IsImport", false, ucc6ValueProvider.IsImport);

			Declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
			AssertEquals("IMP - IsImport", true, ucc6ValueProvider.IsImport);
		}

		#endregion

		public void TestShouldCreateCusPackagePivotFromInvoiceQuantityForSinglePackageType()
		{
			var dec = Factory.NewWithValidTestData<JobDeclaration>();
			var inv = dec.Invoices.AddNew();
			inv.FillWithValidTestData();
			var invLine = inv.InvoiceLines.AddNew();
			invLine.FillWithValidTestData();

			invLine.JI_InvoiceQuantity = 10m;
			invLine.JI_InvoiceUQ = "PK";

			var pack = dec.Packages.AddNew();
			pack.CW_PackType = "PK";
			Assert(invLine.ShouldCreateCusPackagePivotFromInvoiceQuantityForSinglePackageType().shouldCreate);

			invLine.JI_InvoiceUQ = "AAA";
			pack.CW_PackType = "PK";
			Assert(!invLine.ShouldCreateCusPackagePivotFromInvoiceQuantityForSinglePackageType().shouldCreate);
		}

		public void TestIsPreviousEntryNumberVisible()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France");
			var procedure = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, "A", "12", "34", "567", "One", EU.Business.MessageTypeList.Codes.Import, intoWarehouse: false, outOfWarehouse: false);

			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				declaration.SetSupportsBondedWarehousingForTesting(true);
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Procedure = "1234567";

				AssertEquals("Previous entry No. is not visible when procedure is nor into warehouse nor out of warehouse.", expected: false, invoiceLine.IsPreviousEntryNumberVisible);

				procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
				procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
				AssertEquals("Previous entry No. is visible when procedure is out of warehouse.", expected: true, invoiceLine.IsPreviousEntryNumberVisible);

				procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
				procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.No;
				AssertEquals("Previous entry No. is not visible when procedure is into warehouse.", expected: false, invoiceLine.IsPreviousEntryNumberVisible);

				procedure.ZZ6_IntoInwardProcessing = WarehouseMoveStatus.Codes.No;
				procedure.ZZ6_OutOfInwardProcessing = WarehouseMoveStatus.Codes.Yes;
				AssertEquals("Previous entry No. is visible when procedure is out of inward warehouse.", expected: true, invoiceLine.IsPreviousEntryNumberVisible);

				procedure.ZZ6_IntoInwardProcessing = WarehouseMoveStatus.Codes.Yes;
				procedure.ZZ6_OutOfInwardProcessing = WarehouseMoveStatus.Codes.No;
				AssertEquals("Previous entry No. is not visible when procedure is into inward warehouse.", expected: false, invoiceLine.IsPreviousEntryNumberVisible);

				procedure.ZZ6_IntoOutwardProcessing = WarehouseMoveStatus.Codes.No;
				procedure.ZZ6_OutofOutwardProcessing = WarehouseMoveStatus.Codes.Yes;
				AssertEquals("Previous entry No. is visible when procedure is out of outward warehouse.", expected: true, invoiceLine.IsPreviousEntryNumberVisible);

				procedure.ZZ6_IntoOutwardProcessing = WarehouseMoveStatus.Codes.Yes;
				procedure.ZZ6_OutofOutwardProcessing = WarehouseMoveStatus.Codes.No;
				AssertEquals("Previous entry No. is not visible when procedure is into outward warehouse.", expected: false, invoiceLine.IsPreviousEntryNumberVisible);
			}
		}

		public void TestIsBondedWhsVisible()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France");
			var procedure = helper.CreateRefCusProcedure(GlbCompany.CurrentCompany.Country.Code, "A", "12", "34", "567", "One", EU.Business.MessageTypeList.Codes.Import, intoWarehouse: false, outOfWarehouse: false);

			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				declaration.SetSupportsBondedWarehousingForTesting(true);
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Procedure = "1234567";

				AssertEquals("Bonded warehouse quantity is not visible when procedure is nor into warehouse nor into VAT warehouse nor out of warehouse.", expected: false, invoiceLine.IsBondedWhsQuantityVisible);

				procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
				procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.No;
				AssertEquals("Bonded warehouse quantity is visible when procedure is into warehouse.", expected: true, invoiceLine.IsBondedWhsQuantityVisible);

				procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.No;
				procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
				AssertEquals("Boned warehouse quantity is visible when procedure is out of warehouse.", expected: true, invoiceLine.IsBondedWhsQuantityVisible);

				procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.No;
				procedure.ZZ6_IntoInwardProcessing = WarehouseMoveStatus.Codes.No;
				procedure.ZZ6_OutOfInwardProcessing = WarehouseMoveStatus.Codes.Yes;
				AssertEquals("Boned warehouse quantity is visible when procedure is out of inward warehouse.", expected: true, invoiceLine.IsBondedWhsQuantityVisible);

				procedure.ZZ6_IntoInwardProcessing = WarehouseMoveStatus.Codes.Yes;
				procedure.ZZ6_OutOfInwardProcessing = WarehouseMoveStatus.Codes.No;
				AssertEquals("Boned warehouse quantity is visible when procedure is into inward warehouse.", expected: true, invoiceLine.IsBondedWhsQuantityVisible);

				procedure.ZZ6_IntoInwardProcessing = WarehouseMoveStatus.Codes.No;
				procedure.ZZ6_IntoOutwardProcessing = WarehouseMoveStatus.Codes.No;
				procedure.ZZ6_OutofOutwardProcessing = WarehouseMoveStatus.Codes.Yes;
				AssertEquals("Boned warehouse quantity is visible when procedure is out of outward warehouse.", expected: true, invoiceLine.IsBondedWhsQuantityVisible);

				procedure.ZZ6_IntoOutwardProcessing = WarehouseMoveStatus.Codes.Yes;
				procedure.ZZ6_OutofOutwardProcessing = WarehouseMoveStatus.Codes.No;
				AssertEquals("Boned warehouse quantity is visible when procedure is into outward warehouse.", expected: true, invoiceLine.IsBondedWhsQuantityVisible);

				procedure.ZZ6_IntoOutwardProcessing = WarehouseMoveStatus.Codes.No;
				procedure.ZZ6_OutofOutwardProcessing = WarehouseMoveStatus.Codes.No;
				procedure.ZZ6_IntoVATWarehouse = WarehouseMoveStatus.Codes.Yes;
				AssertEquals("Boned warehouse quantity is visible when procedure is into VAT warehouse.", expected: true, invoiceLine.IsBondedWhsQuantityVisible);
			}
		}

		public void TestShouldNotHaveTraders()
		{
			var declaration = Factory.New<JobDeclaration>();
			var header = declaration.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();

			AssertEquals("No message error should be added when JI_Procedure and CEI_SubStyle is not set.", false, line.ShouldNotHaveTraders());

			line.JI_FormattedProcedure = "5800000";
			AssertEquals("No message error should be added when ProcedureCode is other than 51, 53 or 71.", false, line.ShouldNotHaveTraders());

			line.JI_FormattedProcedure = "5100000";
			AssertEquals("Message error should be added for Procedure Code 51.", true, line.ShouldNotHaveTraders());

			line.JI_FormattedProcedure = "5300000";
			AssertEquals("Message error should be added for Procedure Code 53.", true, line.ShouldNotHaveTraders());

			line.JI_FormattedProcedure = "7100000";
			AssertEquals("Message error should be added for Procedure Code 71.", true, line.ShouldNotHaveTraders());

			line.JI_FormattedProcedure = "0000F15";
			AssertEquals("Message error should be added for Concession F15.", true, line.ShouldNotHaveTraders());

			line.JI_FormattedProcedure = "0000F16";
			AssertEquals("No message error should be added when concession is other than F15.", false, line.ShouldNotHaveTraders());

			var cei = declaration.CustomsEntryInstructions.AddNew();
			line.JI_CEI = cei.PK;

			cei.CEI_SubStyle = "D";
			AssertEquals("No message error should be added when SubStyle is other than C or F.", false, line.ShouldNotHaveTraders());

			cei.CEI_SubStyle = "C";
			AssertEquals("Message error should be added for SubStyle C.", true, line.ShouldNotHaveTraders());

			cei.CEI_SubStyle = "F";
			AssertEquals("Message error should be added for SubStyle F.", true, line.ShouldNotHaveTraders());
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;

		void AssertRelatedIndicatorReadOnly(ZPropertyInfo invoicePropertyInfo, ZPropertyInfo invoiceLinePropertyInfo)
		{
			CombineAssertions(() =>
			{
				invoicePropertyInfo.Value = ZBool.False;
				invoiceLinePropertyInfo.Value = ZBool.False;
				AssertEquals("Invoice false", false, invoiceLinePropertyInfo.ReadOnly);
				invoicePropertyInfo.Value = ZBool.True;
				AssertEquals("Invoice true", true, invoiceLinePropertyInfo.ReadOnly);
				invoiceLinePropertyInfo.Value = ZBool.True;
				AssertEquals("InvoiceLine true", false, invoiceLinePropertyInfo.ReadOnly);
			});
		}

		class JobComInvoiceLineForTest : JobComInvoiceLine
		{
			public JobComInvoiceLineForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategyExposed() => GetCustomsUnitDefaultingStrategy();

			public Customs.Business.ProcedureRegimeDecider GetNewProcedureRegimeDeciderExposed() => base.GetNewProcedureRegimeDecider();
		}
	}
}
