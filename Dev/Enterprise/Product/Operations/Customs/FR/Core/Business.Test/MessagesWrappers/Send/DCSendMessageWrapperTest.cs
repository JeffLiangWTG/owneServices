using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessagesWrappers.Common;
using Enterprise.Customs.FR.Messaging.Interfaces;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Send.Testing
{
	public abstract class DCSendMessageWrapperTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			var entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var messageSendingObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(entryHeader);
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>(() => GetMessageWrapper(null, itemErrorCollector));
				AssertExceptionThrown<ArgumentNullException>(() => GetMessageWrapper(messageSendingObject, null));
				AssertExceptionThrown<ArgumentOutOfRangeException>(() => GetMessageWrapper(messageSendingObject, itemErrorCollector));
			});
		}

		public void TestIsIntoWarehouseProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			RefCusProcedure procedure;
			if (declaration.IsExport)
			{
				procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "EX", "10", "71", "F61", "", "EXP", "10P");
			}
			else
			{
				procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.France, "IM", "40", "71", "F61", "", "IMP", "40P");
			}

			procedure.ZZ6_IntoWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.No;
			Factory.Save();

			declaration.JE_MessageType = MessageType;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = cusEntryHeader.MergedLines.AddNew();
			invoiceLine1.JI_CL = entryLine.PK;
			invoiceLine2.JI_CL = entryLine.PK;

			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(cusEntryHeader);
			var wrapper = GetMessageWrapper(messageObject, itemErrorCollector);
			AssertEquals("Wrapper HasIntoWarehouseProcedure should be false because none of the lines has a procedure with ZZ6_IntoWarehouse flagged.", false, wrapper.HasIntoWarehouseProcedure);

			if (declaration.IsExport)
			{
				invoiceLine2.JI_Procedure = "1071F61";
			}
			else
			{
				invoiceLine2.JI_Procedure = "4071F61";
			}
			AssertEquals("Wrapper HasIntoWarehouseProcedure should be true because at least one of the lines has a procedure with ZZ6_IntoWarehouse flagged.", true, wrapper.HasIntoWarehouseProcedure);
		}

		public virtual void TestIsOfficeOfLodgementDifferentFromOfficeOfExit()
		{
			if (declaration.IsExport)
			{
				var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
				var entryLine1 = cusEntryHeader.MergedLines.AddNew();
				var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(cusEntryHeader);
				var wrapper = GetMessageWrapper(messageObject, itemErrorCollector);

				declaration.JE_CustomsOffice = "FRXXXXX";
				var officeOfExit = declaration.CustomsOffices.Cast<EuOfficeCode>().First(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfExit);
				officeOfExit.CY_Data = "FRXXXXX";
				AssertEquals(false, declaration.IsOfficeOfLodgementDifferentFromOfficeOfExit);
				AssertEquals(false, wrapper.IsOfficeOfLodgementDifferentFromOfficeOfExit);

				officeOfExit.CY_Data = "FRYYYYY";
				AssertEquals(true, declaration.IsOfficeOfLodgementDifferentFromOfficeOfExit);
				AssertEquals(true, wrapper.IsOfficeOfLodgementDifferentFromOfficeOfExit);
			}
			else
			{
				Assert("JE_ExportExitType is not valid for import.", true);
			}
		}

		public void TestMetaDatas()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => { new MetaDataWrapper(null, null); });
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = MessageType;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_BGMReference = "8461132";
			var entryLine1 = cusEntryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_AdValoremTariff = "5603939040";
			var entryLine2 = cusEntryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			var wrapper = new MetaDataWrapper(cusEntryHeader, MessageSubType);
			AssertEquals(Application, wrapper.Application);
			AssertEquals(cusEntryHeader.CH_BGMReference, wrapper.DeclarationReference);
			AssertEquals(MessageType, wrapper.EntryNumberType);
		}

		public void TestMessageEnvelope()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => { new EntryMessageEnvelopeWrapper(null, null); });

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_MessageType = "IMP";

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var orgCusAccount = Factory.New<OrgCusAccount>();
			orgCusAccount.CZ_Code = OrgCusAccountCodeList.Codes.DGI;
			orgCusAccount.CZ_Account = "TESTACC";
			orgCusAccount.CZ_RepresentativeID = "TESTREPID";
			orgCusAccount.CZ_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgCusAccount.CZ_OH = importer.PK;
			declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_CustomsProfile = "TESTACC";

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_BGMReference = "8461132";
			var entryLine1 = cusEntryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_AdValoremTariff = "5603939040";
			var entryLine2 = cusEntryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			var wrapper = new EntryMessageEnvelopeWrapper(cusEntryHeader, MessageSubType);
			AssertEquals(SchemaID, wrapper.SchemaID);
			AssertEquals(SchemaVersion, wrapper.SchemaVersion);
			AssertEquals("TESTREPID", wrapper.PartnerId);
			AssertEquals(cusEntryHeader.CorrelationID, wrapper.TransactionId);
			AssertEquals((short)0, wrapper.NumSeq);
		}

		public void TestArticlesImport()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => { new ArticleWrapper(null, null); });
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = cusEntryHeader.MergedLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			entryLine1.CL_AdValoremTariff = "5603939040";
			var entryLine2 = cusEntryHeader.MergedLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			var invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			invoiceLine.JI_LineNo = 1;
			invoiceLine.JI_Weight = 10;
			invoiceLine.JI_WeightUQ = Core.Constants.Weight.Kilograms;

			entryLine1.InvoiceLines.Add(invoiceLine);

			var wrapper = new ArticleWrapper(cusEntryHeader, entryLine1);
			AssertEquals(entryLine1.CL_LineNumber, wrapper.EntryNumber);
			AssertNotNull(wrapper.ShippingIdReference);
			AssertNotNull(wrapper.AlternateCalcValue);
			AssertNotNull(wrapper.AlternateCalcValue.CalcValue);
			AssertNotNull(wrapper.AlternateCalcValue.Motivation);
			AssertEquals(entryLine1.CL_AdValoremTariff.ToString(), wrapper.TariffCode);
			AssertNotNull(wrapper.Observation);
			AssertNotNull(wrapper.CETariffAdditionalCodes);
			AssertNotNull(wrapper.FRTariffAdditionalCodes);
			AssertNotNull(wrapper.PartDispos);
			AssertEquals(invoiceLine.JI_Weight, wrapper.GrossWeight);
		}

		public void TestAlternateCalcValueWrapperCusProcedure()
		{
			var nullDeclaration = Factory.New<JobDeclaration>();
			var nullEntryHeader = nullDeclaration.CustomsEntryHeaders.AddNew();
			nullEntryHeader = null;
			AssertExceptionThrown(typeof(ArgumentNullException), () => { new AlternateCalcValueWrapper(nullEntryHeader, null); });

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			AssertNoExceptionThrown("Wrapper must be instantiated successfully when no EntryInstruction for EntryHeader", () => { new AlternateCalcValueWrapper(cusEntryHeader, itemErrorCollector); });
			var wrapper = new AlternateCalcValueWrapper(cusEntryHeader, itemErrorCollector);
			AssertEquals("CalcValue must be empty when no EntryInstruction for EntryHeader", ZString.Empty, wrapper.CalcValue);
			AssertEquals("Motivation must be empty when no EntryInstruction for EntryHeader", ZString.Empty, wrapper.Motivation);

			var jobDeclaration = cusEntryHeader.Declaration;
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			cusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;

			cusEntryHeader.EntryInstruction.ZG_BypassCode = "I";
			cusEntryHeader.EntryInstruction.ZG_BypassReason = "MAN";

			wrapper = new AlternateCalcValueWrapper(cusEntryHeader, itemErrorCollector);
			AssertEquals(cusEntryHeader.EntryInstruction.ZG_BypassCode, wrapper.CalcValue);
			AssertEquals(cusEntryHeader.EntryInstruction.ZG_BypassReason, wrapper.Motivation);
		}

		public void TestAlternateCalcValueWrapperArticles()
		{
			var cusEntryLine = Factory.New<CusEntryLine>();
			cusEntryLine = null;

			AssertExceptionThrown(typeof(ArgumentNullException), () => { new AlternateCalcValueWrapper(cusEntryLine); });

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = cusEntryHeader.MergedLines.AddNew();

			entryLine1.RandomLine.JI_TariffBypassCode = "3";
			entryLine1.RandomLine.JI_TariffBypassReason = "BAT";

			var wrapper = new AlternateCalcValueWrapper(entryLine1);
			AssertEquals(entryLine1.RandomLine.JI_TariffBypassCode, wrapper.CalcValue);
			AssertEquals(entryLine1.RandomLine.JI_TariffBypassReason, wrapper.Motivation);
		}

		public void TestLiquidation()
		{
			var cusEntryHeader = SetupEntryAndFees();
			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(cusEntryHeader);
			var wrapper = GetMessageWrapper(messageObject, itemErrorCollector);
			AssertEquals(ExpectedLiquidationCount, wrapper.Liquidation.Count());
		}

		public void TestHeader()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => { new HeaderWrapper(null); });

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_BGMReference = "8461132";

			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(cusEntryHeader);
			messageObject.MessageType = EntryActionCodeList.Codes.VAL;

			var wrapper = new HeaderWrapper(messageObject);

			AssertNotNull(wrapper.Motivation);
			AssertNotNull(wrapper.Motivation.Comment);
			AssertNotNull(wrapper.Motivation.Motivation);
			AssertNotNull(wrapper.Motivation.NewDestination);
			AssertNotNull(wrapper.Motivation.RegularJustification);
			AssertEquals(EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.VAL, declaration.IsDeltaC).ToString(), wrapper.ActionCode);
			AssertEquals(cusEntryHeader.CH_BGMReference, wrapper.References.OwnerDeclarationIdentification);
			AssertEquals(cusEntryHeader.EntryNumber, wrapper.References.CusDeclarationNumber);
		}

		public void TestHeaderWithInvalidation()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => { new HeaderWrapper(null); });

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_BGMReference = "8461132";

			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(cusEntryHeader);
			messageObject.MessageType = EntryActionCodeList.Codes.INV;
			messageObject.VOCReason = "BLABLA VOCREASON";

			var wrapper = new HeaderWrapper(messageObject);
			AssertNotNull(wrapper.Motivation);
			AssertNotNullOrEmpty(wrapper.Motivation.Motivation);
			AssertNotNull(wrapper.Motivation.Comment);
			AssertNotNull(wrapper.Motivation.NewDestination);
			AssertNotNull(wrapper.Motivation.RegularJustification);
			AssertEquals(EntryActionCodeList.GetMessageCodeNumber(EntryActionCodeList.Codes.INV, declaration.IsDeltaC).ToString(), wrapper.ActionCode);
			AssertEquals(cusEntryHeader.CH_BGMReference, wrapper.References.OwnerDeclarationIdentification);
			AssertEquals(cusEntryHeader.EntryNumber, wrapper.References.CusDeclarationNumber);
		}

		public virtual void TestCusProcedureType()
		{
			var declaration = CreateJobDeclaration();
			var invoice = declaration.Invoices.AddNew();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(entryHeader);

			CombineAssertions(() =>
			{
				AssertCusProcedureType<ANTProcedureWrapper>(messageObject, EntryActionCodeList.Codes.ANT);
				AssertCusProcedureType<VALProcedureWrapper>(messageObject, EntryActionCodeList.Codes.VAL);
				AssertCusProcedureType<MAPProcedureWrapper>(messageObject, EntryActionCodeList.Codes.MAP);
				AssertCusProcedureType<ANAProcedureWrapper>(messageObject, EntryActionCodeList.Codes.ANA);
				AssertCusProcedureType<VAAProcedureWrapper>(messageObject, EntryActionCodeList.Codes.VAA);
				AssertCusProcedureType<EAVProcedureWrapper>(messageObject, EntryActionCodeList.Codes.EAV);
				AssertCusProcedureType<INVProcedureWrapper>(messageObject, EntryActionCodeList.Codes.INV);
				AssertCusProcedureType<CMPProcedureWrapper>(messageObject, EntryActionCodeList.Codes.CMP);
				AssertCusProcedureType<RPSProcedureWrapper>(messageObject, EntryActionCodeList.Codes.RPS);
				AssertCusProcedureType<RECProcedureWrapper>(messageObject, EntryActionCodeList.Codes.REC);
				AssertCusProcedureType<VARProcedureWrapper>(messageObject, EntryActionCodeList.Codes.VAR);
				AssertCusProcedureType<ANRProcedureWrapper>(messageObject, EntryActionCodeList.Codes.ANR);
			});
		}

		protected void AssertCusProcedureType<T>(MessageSending.DeltaGJobDeclarationMessageSendingObject messageObject, ZString messageType)
		{
			messageObject.MessageType = messageType;
			var messageWrapper = GetMessageWrapper(messageObject, itemErrorCollector);
			AssertType<T>(messageWrapper.CusProcedure);
		}

		public void TestCusProcedure()
		{
			const string officeCode = "FR000001";

			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();

			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			declaration.JE_PaymentMethod = "R";

			declaration.JE_LocationOfGoods = "34796082500052/1";
			declaration.JE_CustomsOffice = officeCode;

			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			cusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine1 = cusEntryHeader.AllEntryLines.AddNew();
			var entryLine2 = cusEntryHeader.AllEntryLines.AddNew();
			var invoice = declaration.Invoices.AddNew();
			entryInstruction.ZG_TransNature = "11";

			var messageObject = new MessageSending.DeltaGJobDeclarationMessageSendingObject(cusEntryHeader);
			messageObject.MessageType = EntryActionCodeList.Codes.ANT;
			var messageWrapper = new DCSendExpMessageWrapper(messageObject, itemErrorCollector);
			var procedureWrapper = messageWrapper.CusProcedure;

			#region Assertion

			AssertNotNull(procedureWrapper.AlternateCalcValue);
			AssertNotNull(procedureWrapper.AlternateCalcValue.CalcValue);
			AssertNotNull(procedureWrapper.AlternateCalcValue.Motivation);
			AssertEquals("", procedureWrapper.ProcedureType.ToString());
			AssertEquals(Core.Constants.CountryCodes.France, procedureWrapper.EntryStyle);
			AssertNotNull(procedureWrapper.EntryStyleCode);
			AssertEquals("2", procedureWrapper.ArticleCount.ToString());
			AssertNotNull(procedureWrapper.EstimatedAssessmentDate);
			AssertNotNull(procedureWrapper.EstimatedAssessmentHour);
			AssertEquals(0, procedureWrapper.PackageCount);
			AssertEquals(declaration.JE_LocationOfGoods, procedureWrapper.AgreedGoodsLocation);
			AssertNotNull(procedureWrapper.ClearanceLocation);
			AssertEquals(entryInstruction.ZG_TransNature, procedureWrapper.TransactionNature);
			AssertNotNull(procedureWrapper.ArrivalState);
			AssertNotNull(procedureWrapper.Office);
			AssertEquals(officeCode, procedureWrapper.Office.OfficeOfLodgement);
			AssertNotNull(procedureWrapper.Office.VisitingOffice);
			AssertEquals(declaration.JE_PaymentMethod, procedureWrapper.PaymentMode);
			AssertEquals("", procedureWrapper.EntryStyleCode);

			#endregion
		}

		CusEntryHeader SetupEntryAndFees()
		{
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader.CH_BGMReference = "8461132";
			var charge1 = cusEntryHeader.Charges.AddNew();
			charge1.C1_ChargeType = "A01";
			charge1.C1_ChargeAmount = 10m;
			charge1.C1_MethodOfPayment = "1";
			var charge2 = cusEntryHeader.ConfirmedCharges.AddNew();
			charge2.C1_ChargeType = "A02";
			charge2.C1_ChargeAmount = 20m;
			charge2.C1_MethodOfPayment = "2";
			var charge3 = cusEntryHeader.ConfirmedCharges.AddNew();
			charge3.C1_ChargeType = "A03";
			charge3.C1_ChargeAmount = 30m;
			charge3.C1_MethodOfPayment = "3";

			var cusEntryLine = cusEntryHeader.MergedLines.AddNew();
			cusEntryLine.CL_LineNumber = 1;

			var fee1 = cusEntryLine.Fees.AddNew();
			fee1.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Override;
			fee1.CF_ChargeType = "B01";
			fee1.CF_MethodOfCalculation = "KGM";
			fee1.CF_Rate = 5;
			fee1.CF_BaseValue = 10;
			fee1.CF_ChargeAmount = 20;
			fee1.CF_MethodOfPayment = "A";
			var fee2 = cusEntryLine.Fees.AddNew();
			fee2.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;
			var fee3 = cusEntryLine.Fees.AddNew();
			fee3.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Override;
			fee3.CF_ChargeType = "A01";
			fee3.CF_MethodOfCalculation = "KLT";
			fee3.CF_Rate = 2;
			fee3.CF_BaseValue = 30;
			fee3.CF_ChargeAmount = 40;
			fee3.CF_MethodOfPayment = "B";

			var confirmedfee1 = cusEntryLine.ConfirmedFees.AddNew();
			confirmedfee1.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Override;
			confirmedfee1.CF_ChargeType = "B01";
			confirmedfee1.CF_MethodOfCalculation = "KGM";
			confirmedfee1.CF_Rate = 5;
			confirmedfee1.CF_BaseValue = 10;
			confirmedfee1.CF_ChargeAmount = 20;
			confirmedfee1.CF_MethodOfPayment = "A";
			var confirmedfee2 = cusEntryLine.ConfirmedFees.AddNew();
			confirmedfee2.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Additional;
			var confirmedfee3 = cusEntryLine.ConfirmedFees.AddNew();
			confirmedfee3.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Override;
			confirmedfee3.CF_ChargeType = "A01";
			confirmedfee3.CF_MethodOfCalculation = "KLT";
			confirmedfee3.CF_Rate = 2;
			confirmedfee3.CF_BaseValue = 30;
			confirmedfee3.CF_ChargeAmount = 40;
			confirmedfee3.CF_MethodOfPayment = "B";
			var confirmedfee4 = cusEntryLine.ConfirmedFees.AddNew();
			confirmedfee4.CF_RateOverrideReasonCode = RateOverrideReasonList.Codes.Override;
			confirmedfee4.CF_ChargeType = "A02";
			confirmedfee4.CF_MethodOfCalculation = "KLT";
			confirmedfee4.CF_Rate = 2;
			confirmedfee4.CF_BaseValue = 30;
			confirmedfee4.CF_ChargeAmount = 40;
			confirmedfee4.CF_MethodOfPayment = "B";

			return cusEntryHeader;
		}

		protected abstract IDeclarationImportExport GetMessageWrapper(MessageSending.DeltaGJobDeclarationMessageSendingObject objectToSend, ErrorCollector itemErrorCollector);
		protected abstract ZString Application { get; }
		protected abstract ZString SchemaID { get; }
		protected abstract ZString SchemaVersion { get; }
		protected abstract ZString MessageType { get; }
		protected abstract ZString DeltaMode { get; }
		protected abstract ZString MessageSubType { get; }
		protected abstract ZInt ExpectedLiquidationCount { get; }

		protected JobDeclaration CreateJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();

			var declarantAddress = Factory.New<OrgAddress>();
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "OHCODE";
			orgHeader.OH_IsWarehouseClient = true;

			declarantAddress.OA_OH = orgHeader.PK;
			declarantAddress.OA_Address1 = "Eugene Leroy Street ";
			declaration.JE_OA_DeclarantAddress = declarantAddress.PK;

			declaration.JE_MessageType = MessageType;
			declaration.JE_DeltaMode = DeltaMode;

			return declaration;
		}

		protected override void SetUp()
		{
			base.SetUp();
			itemErrorCollector = new ErrorCollector();
			declaration = CreateJobDeclaration();
		}

		protected JobDeclaration declaration;
		protected ErrorCollector itemErrorCollector;
	}
}
