using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	class AddInfoJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_T2LItemNumber()
		{
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.MergedLines.AddNew();

			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_CL = entryLine.PK;

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
			declaration.JE_MessageType = "IMP";

			CombineAssertions("T2LItemNumber in T2C case", () =>
			{
				invoiceLine.ZG_T2LItemNumber = -1;
				AssertHasMessageErrorContaining(invoiceLine.ZG_T2LItemNumberInfo, "T2L Item number must be from 1 to 999");

				invoiceLine.ZG_T2LItemNumber = 1000;
				AssertHasMessageErrorContaining(invoiceLine.ZG_T2LItemNumberInfo, "T2L Item number must be from 1 to 999");

				invoiceLine.ZG_T2LItemNumber = 0;
				AssertHasMessageErrorContaining(invoiceLine.ZG_T2LItemNumberInfo, "T2L Item number must be from 1 to 999");

				invoiceLine.ZG_T2LItemNumber = 321;
				AssertNoMessageErrorContaining(invoiceLine.ZG_T2LItemNumberInfo, "T2L Item number must be from 1 to 999");
			});

			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
			CombineAssertions("T2LItemNumber in NOT T2C case", () =>
			{
				invoiceLine.ZG_T2LItemNumber = -1;
				AssertNoMessageErrorContaining(invoiceLine.ZG_T2LItemNumberInfo, "T2L Item number must be from 1 to 999");

				invoiceLine.ZG_T2LItemNumber = 1000;
				AssertNoMessageErrorContaining(invoiceLine.ZG_T2LItemNumberInfo, "T2L Item number must be from 1 to 999");

				invoiceLine.ZG_T2LItemNumber = 0;
				AssertNoMessageErrorContaining(invoiceLine.ZG_T2LItemNumberInfo, "T2L Item number must be from 1 to 999");
			});

			declaration.JE_MessageType = "EXP";
			CombineAssertions("T2LItemNumber in T2C case and Export Declaration", () =>
			{
				invoiceLine.ZG_T2LItemNumber = -1;
				AssertNoMessageErrorContaining(invoiceLine.ZG_T2LItemNumberInfo, "T2L Item number must be from 1 to 999");

				invoiceLine.ZG_T2LItemNumber = 1000;
				AssertNoMessageErrorContaining(invoiceLine.ZG_T2LItemNumberInfo, "T2L Item number must be from 1 to 999");

				invoiceLine.ZG_T2LItemNumber = 0;
				AssertNoMessageErrorContaining(invoiceLine.ZG_T2LItemNumberInfo, "T2L Item number must be from 1 to 999");
			});
		}

		public void TestCheckZG_AIEMType()
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			var countryCode = Core.Constants.CountryCodes.Spain;
			var canaryIslandCode = "61";
			helper.CreateCusCodeListCanaryIsland(countryCode, canaryIslandCode, "Test 61");

			var impTariffType = helper.CreateTariffType(countryCode, "IMP");
			var aiemTariffType = helper.CreateTariffType(countryCode, "AIEM");
			Factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(countryCode, impTariffType.PK, "11112222", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			var tariffAIEM = helper.LoadOrCreateNewTariff(countryCode, aiemTariffType.PK, "11112222_AIEM01", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), "desc1");
			helper.CreateTariffRelationship(tariffAIEM.PK, impTariffType.PK, tariff.ZZ1_TariffCode);
			Factory.Save();

			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			declaration.ZG_DestinationState = "ZZ";
			invoiceLine.ZG_AIEMType = "Z";
			AssertNoMessageErrorContaining(invoiceLine.ZG_AIEMTypeInfo, "The code you have selected is not in the list.");

			declaration.ZG_DestinationState = canaryIslandCode;
			invoiceLine.AddInfoValidation.ValidateZG_AIEMType();
			AssertHasMessageErrorContaining(invoiceLine.ZG_AIEMTypeInfo, "The code you have selected is not in the list.");

			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			invoiceLine.ZG_AIEMType = "AIEM01";
			invoiceLine.AddInfoValidation.ValidateZG_AIEMType();
			AssertNoMessageErrorContaining(invoiceLine.ZG_AIEMTypeInfo, "The code you have selected is not in the list.");
		}

		public void TestCheckZG_MethodOfPayment2()
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			var canaryIslandCode = "61";
			helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, canaryIslandCode, "Test 61");

			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			declaration.ZG_DestinationState = "ZZ";
			invoiceLine.ZG_MethodOfPayment2 = "Z";
			AssertNoMessageErrorContaining(invoiceLine.ZG_MethodOfPayment2Info, "The code you have selected is not in the list.");
			declaration.ZG_DestinationState = canaryIslandCode;
			invoiceLine.AddInfoValidation.ValidateZG_MethodOfPayment2();
			AssertHasMessageErrorContaining(invoiceLine.ZG_MethodOfPayment2Info, "The code you have selected is not in the list.");
		}

		public void TestCheckZG_REAProductCode()
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			var canaryIslandCode = "61";
			helper.CreateCusCodeListCanaryIsland(Core.Constants.CountryCodes.Spain, canaryIslandCode, "Test 61");

			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			CombineAssertions(() =>
			{
				declaration.ZG_DestinationState = canaryIslandCode;
				invoiceLine.ZG_REAProductCode = "ZZ";
				invoiceLine.JI_PrimaryPreference = "100";
				invoiceLine.AddInfoValidation.ValidateZG_REAProductCode();
				AssertHasMessageErrorContaining(invoiceLine.ZG_REAProductCodeInfo, "REA Product Code should only be declared when Preference = 085");

				invoiceLine.JI_PrimaryPreference = "085";
				invoiceLine.AddInfoValidation.ValidateZG_REAProductCode();
				AssertNoMessageErrorContaining(invoiceLine.ZG_REAProductCodeInfo, "REA Product Code should only be declared when Preference = 085");

				declaration.ZG_DestinationState = "ZZ";
				Assert("This test need to be completed when adding the List to ZG_REAProductCode", true);

				invoiceLine.JI_PrimaryPreference = "100";
				declaration.ZG_DestinationState = "12";
				invoiceLine.AddInfoValidation.ValidateZG_REAProductCode();
				AssertNoMessageErrorContaining("No message error if destination is not canary island", invoiceLine.ZG_REAProductCodeInfo, "REA Product Code should only be declared when Preference = 085");
			});
		}

		public void TestCheckZG_ExciseExemption()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;

			CombineAssertions(() =>
			{
				invoiceLine.ZG_ExciseCode = ZString.Empty;
				invoiceLine.ZG_ExciseExemption = "B";
				AssertHasWarningContaining(invoiceLine.ZG_ExciseExemptionInfo, "There is no Excise Code to be exempted from.");

				invoiceLine.ZG_ExciseExemption = "0";
				AssertNoNotifications(invoiceLine.ZG_ExciseExemptionInfo);

				invoiceLine.ZG_ExciseCode = "0A0";

				invoiceLine.ZG_ExciseExemption = "Z";
				AssertHasMessageErrorContaining(invoiceLine.ZG_ExciseExemptionInfo, "The code you have selected is not in the list.");

				invoiceLine.ZG_ExciseExemption = "B";
				AssertNoNotifications(invoiceLine.ZG_ExciseExemptionInfo);
			});
		}

		public void TestCheckZG_HasNonRecycledPlastics()
		{
			string warningMessage = AddInfoJobComInvoiceLineValidation.NonRecycledPlasticFeeNotAllowedForProcedureCode;

			CombineAssertions("Import", () =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Import;

				SetDataAndAssertWarningMessage("78232", ZBool.True);
				SetDataAndAssertWarningMessage("43309A", ZBool.True);
				SetDataAndAssertWarningMessage("12234", ZBool.True);
				SetDataAndAssertWarningMessage("09324234", ZBool.True);
				SetDataAndAssertWarningMessage("0071328", ZBool.True);
				SetDataAndAssertWarningMessage("AB290239", ZBool.True);

				SetDataAndAssertNoWarningMessage("", ZBool.True);
				SetDataAndAssertNoWarningMessage("401324", ZBool.True);
				SetDataAndAssertNoWarningMessage("421324", ZBool.True);
				SetDataAndAssertNoWarningMessage("616133", ZBool.True);
				SetDataAndAssertNoWarningMessage("6345455", ZBool.True);
				SetDataAndAssertNoWarningMessage("445822", ZBool.True);
				SetDataAndAssertNoWarningMessage("491233", ZBool.True);
				SetDataAndAssertNoWarningMessage("0787122", ZBool.True);
			});

			CombineAssertions("Export", () =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;

				SetDataAndAssertNoWarningMessage("401324", ZBool.True);
				SetDataAndAssertNoWarningMessage("78232", ZBool.True);
				SetDataAndAssertNoWarningMessage("43309A", ZBool.True);
			});

			void SetDataAndAssertWarningMessage(string cpcValue, ZBool hasRecycledPlastic)
			{
				invoiceLine.JI_Procedure = cpcValue;
				invoiceLine.ZG_HasNonRecycledPlastics = hasRecycledPlastic;
				AssertHasWarningContaining($"Procedure: {cpcValue}, HasRecycledPlastic:{hasRecycledPlastic}", invoiceLine.ZG_HasNonRecycledPlasticsInfo, warningMessage);
			}

			void SetDataAndAssertNoWarningMessage(string cpcValue, ZBool hasRecycledPlastic)
			{
				invoiceLine.ZG_HasNonRecycledPlasticsInfo.ClearAllNotifications();
				invoiceLine.JI_Procedure = cpcValue;
				invoiceLine.ZG_HasNonRecycledPlastics = hasRecycledPlastic;
				AssertNoWarningContaining($"Procedure: {cpcValue}, HasRecycledPlastic:{hasRecycledPlastic}", invoiceLine.ZG_HasNonRecycledPlasticsInfo, warningMessage);
			}
		}

		public void TestCheckZG_HasNonRecycledPlasticsOnProcedureChange()
		{
			string warningMessage = AddInfoJobComInvoiceLineValidation.NonRecycledPlasticFeeNotAllowedForProcedureCode;

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			invoiceLine.ZG_HasNonRecycledPlastics = ZBool.True;
			invoiceLine.JI_Procedure = "806573932";
			AssertHasWarningContaining("Procedure: 806573932, HasRecycledPlastic: True", invoiceLine.ZG_HasNonRecycledPlasticsInfo, warningMessage);

			invoiceLine.JI_Procedure = "40273474";
			AssertNoWarningContaining("Procedure: 40273474, HasRecycledPlastic: True", invoiceLine.ZG_HasNonRecycledPlasticsInfo, warningMessage);

			invoiceLine.JI_Procedure = ZString.Empty;
			AssertNoWarningContaining("Procedure: EMPTY, HasRecycledPlastic: True", invoiceLine.ZG_HasNonRecycledPlasticsInfo, warningMessage);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			invoiceLine.JI_Procedure = "806573932";
			AssertNoWarningContaining("EXP, Procedure: 806573932, HasRecycledPlastic: True", invoiceLine.ZG_HasNonRecycledPlasticsInfo, warningMessage);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
		}

		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
	}
}
