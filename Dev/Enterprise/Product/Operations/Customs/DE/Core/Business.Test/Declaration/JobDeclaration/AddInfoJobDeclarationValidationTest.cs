using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	sealed class AddInfoJobDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJE_VATClaimBack()
		{
			declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_VATClaimBackInfo, "X", YesNoList.Codes.Yes);
		}

		public void TestCheckZG_VATDeferType()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				declaration.ZG_VATDeferType = "XXX";
				AssertNoNotifications(declaration.ZG_VATDeferTypeInfo);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
				declaration.ZG_VATDeferType = "XXX";
				AssertHasMessageErrorContaining(declaration.ZG_VATDeferTypeInfo, ListValidation.InvalidCodeMessageError);

				declaration.ZG_VATDeferType = declaration.Lookups.PaymentPartyList[0].Code;
				AssertNoMessageErrorContaining(declaration.ZG_VATDeferTypeInfo, ListValidation.InvalidCodeMessageError);

				declaration.ZG_VATDeferType = ZString.Empty;
				declaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.E;
				declaration.AddInfoValidation.ValidateZG_VATDeferType();
				AssertHasMessageErrorContaining(declaration.ZG_VATDeferTypeInfo, MandatoryValidation.YouHaveNotEntered);
				declaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.F;
				declaration.AddInfoValidation.ValidateZG_VATDeferType();
				AssertHasMessageErrorContaining(declaration.ZG_VATDeferTypeInfo, MandatoryValidation.YouHaveNotEntered);
				declaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.G;
				declaration.AddInfoValidation.ValidateZG_VATDeferType();
				AssertHasMessageErrorContaining(declaration.ZG_VATDeferTypeInfo, MandatoryValidation.YouHaveNotEntered);
				declaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.Z;
				declaration.AddInfoValidation.ValidateZG_VATDeferType();
				AssertHasMessageErrorContaining(declaration.ZG_VATDeferTypeInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.ZG_MethodOfPayment = "A";
				declaration.AddInfoValidation.ValidateZG_VATDeferType();
				AssertNoMessageErrorContaining(declaration.ZG_VATDeferTypeInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_PaymentMethod = DeferralPaymentPartyList.Codes.Declarant;
				declaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.Z;
				declaration.AddInfoValidation.ValidateZG_VATDeferType();
				AssertNoMessageErrorContaining(declaration.ZG_VATDeferTypeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckZG_VATAccountNumber()
		{
			declaration.JE_OA_DeclarantAddress = Factory.CreateDeferralParty("10", "1111").MainAddress.PK;

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.ZG_VATDeferType = DeferralPaymentPartyList.Codes.Declarant;
			declaration.ZG_VATDeferNumber = ZString.Empty;
			AssertHasMessageErrorContaining(declaration.ZG_VATDeferNumberInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.ZG_VATDeferNumber = "XXX";
			AssertNoMessageErrorContaining(declaration.ZG_VATDeferNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(declaration.ZG_VATDeferNumberInfo, ListValidation.InvalidCodeMessageError);

			declaration.ZG_VATDeferNumber = "1111";
			AssertNoNotifications(declaration.ZG_VATDeferNumberInfo);

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.ZG_VATDeferType = DeferralPaymentPartyList.Codes.Declarant;
			declaration.ZG_VATDeferNumber = ZString.Empty;
			AssertNoNotifications(declaration.ZG_VATDeferNumberInfo);
		}

		public void TestCheckZG_AgreedPlaceCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: grouping);

			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "IncoTerm Key");
			helper.CreateNewOrGetExistingCusCodeList("EUN", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "1", "Test Code", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				declaration.ZG_AgreedPlaceCode = "X";
				AssertHasMessageError(declaration.ZG_AgreedPlaceCodeInfo, ListValidation.InvalidCodeMessageError);
				declaration.ZG_AgreedPlaceCode = "1";
				AssertNoMessageError(declaration.ZG_AgreedPlaceCodeInfo, ListValidation.InvalidCodeMessageError);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				declaration.ZG_AgreedPlaceCode = "X";
				AssertNoMessageError(declaration.ZG_AgreedPlaceCodeInfo, ListValidation.InvalidCodeMessageError);
				declaration.ZG_AgreedPlaceCode = "1";
				AssertNoMessageError(declaration.ZG_AgreedPlaceCodeInfo, ListValidation.InvalidCodeMessageError);
			}
		}

		public void TestCheckZG_AgreedPlaceCode_WarehouseAdjustment()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;
			declaration.AddInfoValidation.ValidateZG_AgreedPlaceCode();
			AssertNoNotifications(declaration.ZG_AgreedPlaceCodeInfo);
		}

		public void TestCheckZG_SpecificCircumstanceIndicator()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.ZG_SpecificCircumstanceIndicatorInfo, "@", SpecificCircumstanceIndicatorForUCCList.Codes.A20);
		}

		public void TestCheckZG_SpecificCircumstanceIndicator_WarehouseAdjustment()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;
			declaration.AddInfoValidation.ValidateZG_SpecificCircumstanceIndicator();
			AssertNoNotifications(declaration.ZG_SpecificCircumstanceIndicatorInfo);
		}

		public void TestCheckZG_StatisticsGoodsStatus()
		{
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(declaration.JE_StatisticStatusInfo, "XX", StatisticStatusCodeList.Codes.C04);
		}

		public void TestCheckZG_BorderTransportMeans_Mandatory_Import()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.ZG_BorderTransportMeansInfo);
		}

		public void TestCheckZG_BorderTransportMeans_ListValidation_Import()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.ZG_BorderTransportMeansInfo, "XX", ImportBorderTransportMeansList.Codes.Without);
		}

		public void TestCheckZG_BorderTransportMeans_Mandatory_Export()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			CombineAssertions(() =>
			{
				foreach (var transportMode in new[] { "", TransportTypeList.Codes.Rail, TransportTypeList.Codes.Mail, TransportTypeList.Codes.FixedTransportInstallations })
				{
					declaration.JE_TransportMode = transportMode;
					declaration.AddInfoValidation.ValidateZG_BorderTransportMeans();
					AssertNoMessageErrorContaining($"TransportMode is {transportMode}", declaration.ZG_BorderTransportMeansInfo, MandatoryValidation.YouHaveNotEntered);
				}

				foreach (var transportMode in new[] { TransportTypeList.Codes.Sea, TransportTypeList.Codes.Road, TransportTypeList.Codes.Air, TransportTypeList.Codes.OwnPropulsion, TransportTypeList.Codes.InlandWaterwayTransport })
				{
					declaration.JE_TransportMode = transportMode;
					ValidationTestHelper.AssertYouHaveNotEnteredMessageError(declaration.ZG_BorderTransportMeansInfo, MandatoryValidation.YouHaveNotEntered, $"TransportMode is {transportMode}");
				}
			});
		}

		public void TestCheckZG_BorderTransportMeans_ListValidation_Export()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			ValidationTestHelper.AssertInvalidCodeMessageError(declaration.ZG_BorderTransportMeansInfo, "XX", ExportBorderTransportMeansList.Codes._10);
		}

		public void TestCheckZG_BorderTransportMeans_WarehouseAdjustment()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;
			declaration.AddInfoValidation.ValidateZG_BorderTransportMeans();
			AssertNoNotifications(declaration.ZG_BorderTransportMeansInfo);
		}

		public void TestCheckZG_BorderTransportMeans_StockMovement()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;

			declaration.AddInfoValidation.ValidateZG_BorderTransportMeans();
			AssertNoNotifications(declaration.ZG_BorderTransportMeansInfo);
		}

		public void TestCheckZG_Box18TransportID()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.AddInfoValidation.ValidateZG_Box18TransportID();
			AssertNoMessageErrorContaining(declaration.ZG_Box18TransportIDInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.AddInfoValidation.ValidateZG_Box18TransportID();
			AssertHasMessageErrorContaining(declaration.ZG_Box18TransportIDInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.ZG_Box18TransportID = "1";
			AssertNoMessageErrorContaining(declaration.ZG_Box18TransportIDInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckZG_Box18TransportID_WarehouseAdjustment()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;
			declaration.AddInfoValidation.ValidateZG_Box18TransportID();
			AssertNoNotifications(declaration.ZG_Box18TransportIDInfo);
		}

		public void TestCheckZG_Box18TransportID_StockMovement()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;

			declaration.AddInfoValidation.ValidateZG_Box18TransportID();
			AssertNoNotifications(declaration.ZG_Box18TransportIDInfo);
		}

		public void TestCheckJE_PresentationStartDate_WarehouseAdjustment()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;
			declaration.Validation.ValidateJE_PresentationStartDate();
			AssertNoNotifications(declaration.JE_PresentationStartDateInfo);
		}

		public void TestCheckJE_PresentationEndDate_WarehouseAdjustment()
		{
			declaration.JE_MessageType = Common.DE.DEJobMessageTypeList.Codes.WarehouseAdjustment;
			declaration.Validation.ValidateJE_PresentationEndDate();
			AssertNoNotifications(declaration.JE_PresentationEndDateInfo);
		}

		public void TestCheckZG_MethodOfPayment_Export()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			ValidationTestHelper.AssertFieldIsNotMandatory(declaration.ZG_MethodOfPaymentInfo);
		}

		public void TestCheckZG_MethodOfPayment_Import()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				declaration.AddInfoValidation.ValidateZG_MethodOfPayment();
				AssertNoMessageErrorContaining("No entry instructions", declaration.ZG_MethodOfPaymentInfo, MandatoryValidation.YouHaveNotEntered);

				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.EZA;
				declaration.AddInfoValidation.ValidateZG_MethodOfPayment();
				AssertHasMessageErrorContaining("Entry instructions are all 'EZA'", declaration.ZG_MethodOfPaymentInfo, MandatoryValidation.YouHaveNotEntered);

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_Style = ImportDeclarationTypeList.Codes.EAV;
				declaration.AddInfoValidation.ValidateZG_MethodOfPayment();
				AssertNoMessageErrorContaining("Not all entry instructions are 'EZA'", declaration.ZG_MethodOfPaymentInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckZG_MethodOfPayment_Import_ListValidation()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var dateInFuture = ZDate.Today.AddDays(4);
			var dateInPast = ZDate.Today.AddDays(-4);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany");

			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Payment Methods");

			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "A", "Valid by Date And Code", dateInPast, dateInFuture);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "L", "Not Valid by Code L", dateInPast, dateInFuture);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "S", "Not Valid by Code S", dateInPast, dateInFuture);

			Factory.Save();

			CombineAssertions(() =>
			{
				declaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.A;
				AssertNoMessageErrorContaining(declaration.ZG_MethodOfPaymentInfo, ListValidation.InvalidCodeMessageError);
				declaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.L;
				AssertHasMessageErrorContaining(declaration.ZG_MethodOfPaymentInfo, ListValidation.InvalidCodeMessageError);
				declaration.ZG_MethodOfPayment = UniversalReferenceConstants.MethodOfPaymentTypes.S;
				AssertHasMessageErrorContaining(declaration.ZG_MethodOfPaymentInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckZG_IsHighValueOvrd()
		{
			SetUpForCheckZG_IsHighValueOvrd();

			CombineAssertions(() =>
			{
				declaration.ZG_IsHighValueOvrd = ZBool.False;
				declaration.AddInfoValidation.ValidateZG_IsHighValueOvrd();
				AssertHasMessageError("When false", declaration.ZG_IsHighValueOvrdInfo, "D.V.1 Flag must be set to true if the sum of all Customs Values on Inv. Lines >=20.000,00€.");
				declaration.ZG_IsHighValueOvrd = ZBool.True;
				declaration.AddInfoValidation.ValidateZG_IsHighValueOvrd();
				AssertNoNotifications("When true", declaration.ZG_IsHighValueOvrdInfo);

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				declaration.ZG_IsHighValueOvrd = ZBool.False;
				declaration.AddInfoValidation.ValidateZG_IsHighValueOvrd();
				AssertNoNotifications("Export: when false", declaration.ZG_IsHighValueOvrdInfo);
			});
		}

		public void TestCheckZG_IsHighValueOvrd_AVABR()
		{
			SetUpForCheckZG_IsHighValueOvrd_AVABR();

			CombineAssertions(() =>
			{
				declaration.ZG_IsHighValueOvrd = ZBool.False;
				declaration.AddInfoValidation.ValidateZG_IsHighValueOvrd();
				AssertNoNotifications("When true", declaration.ZG_IsHighValueOvrdInfo);

				declaration.CustomsEntryInstructions[0].CEI_Style = ImportDeclarationTypeList.Codes.AAV;
				declaration.AddInfoValidation.ValidateZG_IsHighValueOvrd();
				AssertHasMessageError("When false", declaration.ZG_IsHighValueOvrdInfo, "D.V.1 Flag must be set to true if the sum of all Customs Values on Inv. Lines >=20.000,00€.");
			});
		}

		public void TestCheckZG_IsHighValueOvrd_NormalInwardProcessing()
		{
			SetUpForCheckZG_IsHighValueOvrd();

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.EAV;
			declaration.ZG_IsHighValueOvrd = ZBool.False;
			declaration.AddInfoValidation.ValidateZG_IsHighValueOvrd();
			AssertNoNotifications("When false", declaration.ZG_IsHighValueOvrdInfo);
		}

		public void TestCheckZG_IsHighValueOvrd_StockMovement()
		{
			SetUpForCheckZG_IsHighValueOvrd();

			declaration.ZG_IsHighValueOvrd = ZBool.False;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.LUZ;
			declaration.AddInfoValidation.ValidateZG_IsHighValueOvrd();
			AssertNoNotifications(declaration.ZG_IsHighValueOvrdInfo);
		}

		void SetUpForCheckZG_IsHighValueOvrd()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("DV1", 20000.0m, "DE", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1));
			Factory.Save();

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_InvoiceAmount = 21000m;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 11000m;

			AssertEquals("Pre-Req - invoiceLine.JI_CustomsValue = 10000", 10000m, invoiceLine.JI_CustomsValue);
			AssertEquals("Pre-Req - invoiceLine2.JI_CustomsValue = 11000", 11000m, invoiceLine2.JI_CustomsValue);
		}

		void SetUpForCheckZG_IsHighValueOvrd_AVABR()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateTaxOrFee("DV1", 20000.0m, "DE", ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1));
			Factory.Save();

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			invoice.JZ_InvoiceAmount = 21000m;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 11000m;

			AssertEquals("Pre-Req - invoiceLine.JI_CustomsValue = 10000", 10000m, invoiceLine.JI_CustomsValue);
			AssertEquals("Pre-Req - invoiceLine2.JI_CustomsValue = 11000", 11000m, invoiceLine2.JI_CustomsValue);
		}

		public void TestCheckZG_BorderTransportMeans_AVABR()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			declaration.AddInfoValidation.ValidateZG_BorderTransportMeans();
			AssertNoNotifications(declaration.ZG_BorderTransportMeansInfo);
		}

		public void TestCheckZG_Box18TransportID_AVABR()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ImportDeclarationTypeList.Codes.AVABR;
			declaration.AddInfoValidation.ValidateZG_Box18TransportID();
			AssertNoNotifications(declaration.ZG_Box18TransportIDInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}

		JobDeclaration declaration;
	}
}
