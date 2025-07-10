using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	class EntryFeePaymentPartyUnderstanderTest : TestCaseWithFactory
	{
		public void TestUkDisbursementDependsUponDefermentCodesOnDec()
		{
			var logger = new DetailedLoggerForTest();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var currentCountry = Env.CurrentCompany.Country.Code;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry);
			Factory.Save();

			var dut = helper.CreateNewOrGetExistingRateType(currentCountry, RefCusRateTypes.Dty, "Duty");
			var cvd = helper.CreateNewOrGetExistingRateType(currentCountry, RefCusRateTypes.CountervailingDuty, "Countervailing Duty");
			var add = helper.CreateNewOrGetExistingRateType(currentCountry, RefCusRateTypes.AntiDumpingDuty, "Anti-Dumping Duty");
			dut.ZZR_IsPayable = true;
			cvd.ZZR_IsPayable = true;
			add.ZZR_IsPayable = true;
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dut.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.ProvisionalAntiDumpingDuty, add.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, add.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DefinitiveCountervailingDuty, cvd.PK);
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.ProvisionalCountervailingDuty, cvd.PK);
			Factory.Save();

			var fasMop = "X";
			var deferredMop = "Y";
			var defermentAccountMop = "E";
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method of Payment");
			var mop1 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, deferredMop, "Deferred", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(mop1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AutoRating, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Autorating_SometimesBrokerSeeBox48);
			var mop2 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, fasMop, "FAS", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(mop2.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AutoRating, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Autorating_AlwaysBroker);  // FAS is always apid by broker
			var mop3 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, defermentAccountMop, "Deferred (deferment account)", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(mop3.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AutoRating, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Autorating_SometimesBrokerSeeBox48);
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			var cusEntryheader = dec.CustomsEntryHeaders.AddNew();
			var cusEntryLine = cusEntryheader.MergedLines.AddNew();
			var lineFeeA00deferred = cusEntryLine.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 60m);
			var lineFeeA00fas = cusEntryLine.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 9m);
			var lineFeeB00deferred = cusEntryLine.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.Vat, 71m);
			var lineFeeB00fas = cusEntryLine.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.Vat, 8m);
			var lineFeeA30fas = cusEntryLine.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, 1m);
			var lineFeeA40 = cusEntryLine.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.DefunctUseA00InsteadCustomsDutyOnAgriculturalProducts, 1m);  // Not configured in the RefZZ DB, so won't be found by the charge getter
			var lineFeeXYZ = cusEntryLine.Fees.AddOrUpdate("XYZ", 89m);  // fake fee

			lineFeeA00deferred.CF_MethodOfPayment = deferredMop;
			lineFeeA00fas.CF_MethodOfPayment = fasMop;
			lineFeeB00deferred.CF_MethodOfPayment = deferredMop;
			lineFeeB00fas.CF_MethodOfPayment = fasMop;
			lineFeeA30fas.CF_MethodOfPayment = fasMop;

			var chargesGetter = CargoWise.Common.ServiceLocator.GetService<ICustomsCharges>(dec);
			var charges = chargesGetter.GetCustomsCharges(logger);
			AssertEquals("A00, B00, A30 will be mapped to DTY, VAT and ADD, Charges found should be 3.", 3, charges.Length);
			AssertEquals("Find DTY", true, cusEntryheader.EntryChargeTypeList.ContainsCode(RefCusRateTypes.Dty));
			AssertEquals("Find VAT", true, cusEntryheader.EntryChargeTypeList.ContainsCode(RefCusRateTypes.Vat));
			AssertEquals("Find ADD", true, cusEntryheader.EntryChargeTypeList.ContainsCode(RefCusRateTypes.AntiDumpingDuty));
			Assert("DTY Log", logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee DTY with MoP=X is always paid by broker - include in rating"));
			Assert("VAT Log", logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee VAT with MoP=X is always paid by broker - include in rating"));
			Assert("ADD Log", logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == "Fee ADD with MoP=X is always paid by broker - include in rating"));
			logger.Logs.Clear();

			var autoRatingInfo = dec.RatingAdapter as IAutoRatingCustomsInfo;
			AssertNotNull(autoRatingInfo);

			dec.ZG_VATDeferType = "";
			AssertEquals("When VAT deferment is undefined, VAT is not deferred, so broker pays VA", true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, deferredMop, logger));
			AssertEquals("Broker always pays FAS charges - VAT (B00)", true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, fasMop, logger));
			Assert("VAT Log", logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == $"Deferred Fee VAT with no deferment, with MoP={deferredMop}, include for autorating"));
			logger.Logs.Clear();

			dec.JE_PaymentMethod = "";
			AssertEquals("When other deferment is undefined, other fees are not deferred, so broker pays fee for duty", true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Dty, deferredMop, logger));
			AssertEquals("When other deferment is undefined, other fees are not deferred, so broker pays fee for add (a known fee)", true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.AntiDumpingDuty, deferredMop, logger));
			AssertEquals("When other deferment is undefined, other fees are not deferred, so broker pays fee for XYZ (an unknown fee - asserting that when we do not recognise a fee, we still make someone pay it, when FAS'd)",
							true, cusEntryheader.IsFeePaidByBroker("XYZ", fasMop, logger));
			Assert("DTY Log", logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == $"Deferred Fee DTY with 1DAN=, with MoP={deferredMop}, include for rating"));
			Assert("ADD Log", logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == $"Deferred Fee ADD with 1DAN=, with MoP={deferredMop}, include for rating"));
			logger.Logs.Clear();

			dec.ZG_VATDeferType = EU.Business.DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14; //A
			dec.JE_PaymentMethod = EU.Business.DefermentMethodList.Codes.ConsigneesAccountStandingAuthority; //Anything except A
			AssertEquals("When VAT is deferred to broker's account, broker pays fee for VAT (a known fee)", true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, deferredMop, logger));
			Assert("VAT Log", logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == $"Deferred Fee VAT with 1DAN=C and 2DAN=A, with MoP={deferredMop}, include for rating"));
			logger.Logs.Clear();

			dec.JE_PaymentMethod = EU.Business.DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14; //A
			AssertEquals("When duty is deferred to broker's account, broker pays fee for duty (a known fee)", true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Dty, deferredMop, logger));
			AssertEquals("When duty is deferred to broker's account, broker pays fee for all fees, EVEN those unrecognised", true, cusEntryheader.IsFeePaidByBroker("XYZ", fasMop, logger));

			dec.ZG_VATDeferType = EU.Business.DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;  //B
			AssertEquals("When VAT is deferred to CONSIGNEE's account, broker DOESN'T pay fee for VAT", false, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, deferredMop, logger));

			dec.JE_PaymentMethod = EU.Business.DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;  //B
			AssertEquals("When duty is deferred to CONSIGNEE's account, broker DOESN'T pay fee for duty", false, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Dty, deferredMop, logger));
			AssertEquals("When duty is deferred to CONSIGNEE's account, broker SHOULD pay fee for an unknown fee", true, cusEntryheader.IsFeePaidByBroker("XYZ", fasMop, logger));

			AssertEquals("Broker still always pas FAS'd fees", true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Duty, fasMop, logger));

			logger.Logs.Clear();
			// For CS00107640: when VAT is not explictly deferred, it must fall back to Other deferment
			dec.ZG_VATDeferType = "";  // undefined
			dec.JE_PaymentMethod = EU.Business.DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14;  //A
			AssertEquals("When duty is deferred to broker's account and VAT deferment is not defined, broker pays fee for VAT too", true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, deferredMop, logger));
			dec.JE_PaymentMethod = EU.Business.DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority;  //B
			AssertEquals("When duty is deferred to consignee's account and VAT deferment is not defined, broker doesn't pays fee for VAT", false, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Vat, deferredMop, logger));
			Assert("VAT Log", logger.Logs.Exists(x => x.Item1 == LogType.Information && x.Item2 == $"Deferred Fee VAT with 1DAN=A and no 2DAN, with MoP={deferredMop}, include for rating"));
			logger.Logs.Clear();

			// For WI00533234: CDS - Taxes are autorating even when on a customers deferment
			dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;

			cusEntryheader = dec.CustomsEntryHeaders.AddNew();
			cusEntryLine = cusEntryheader.MergedLines.AddNew();

			var lineFeeA00defermentAccount = cusEntryLine.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 200m);
			lineFeeA00defermentAccount.CF_MethodOfPayment = defermentAccountMop;

			chargesGetter = CargoWise.Common.ServiceLocator.GetService<ICustomsCharges>(dec);
			charges = chargesGetter.GetCustomsCharges(logger);

			AssertEquals("Prerequisite: declaration default data grouping is CDS", GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services, dec.GetDefaultDataGroupingCode());
			AssertEquals("A00 will be mapped to DTY, Charges found should be 1.", 1, charges.Length);
			AssertEquals("Find DTY", true, cusEntryheader.EntryChargeTypeList.ContainsCode(RefCusRateTypes.Dty));
			logger.Logs.Clear();

			autoRatingInfo = dec.RatingAdapter as IAutoRatingCustomsInfo;
			AssertNotNull(autoRatingInfo);

			dec.JE_PaymentMethod = EU.Business.DefermentMethodList.Codes.ConsigneesAccountSpecificAuthority; //B
			AssertEquals("When duty is deferred to broker's account with 1DAN=B and MoP=E, broker DOESN'T pay fee for duty", false, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Dty, defermentAccountMop, logger));

			dec.JE_PaymentMethod = EU.Business.DefermentMethodList.Codes.DeclarantsAccountOrAccountBelongingToTheTraderByNoInBox14; //A
			AssertEquals("When duty is deferred to broker's account with 1DAN=A and MoP=E, broker pays fee for duty", true, cusEntryheader.IsFeePaidByBroker(RefCusRateTypes.Dty, defermentAccountMop, logger));
		}

		public void TestDutyNotBilledForOtherAccount()
		{
			var logger = new DetailedLoggerForTest();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var gb = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom);
			const string grouping = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			helper.CreateNewOrGetExistingDataGrouping(grouping, Registry.Business.DeclarationApplicationCodeList.Descriptions.Customs_Declaration_Services, gb);
			var dut = helper.CreateNewOrGetExistingRateType(grouping, RefCusRateTypes.Dty, "Duty");
			dut.ZZR_IsPayable = true;
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dut.PK);
			const string mop = "P";
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method of Payment");
			var mop0 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, mop, mop, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(mop0.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AutoRating, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Autorating_AlwaysBroker);
			var mop1 = helper.CreateNewOrGetExistingCusCodeList(grouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, mop, mop, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(mop1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AutoRating, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Autorating_SeeDataElement83);

			const string eori = "GB1234";
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, GlbBranch.CurrentBranch.OrgProxy.PK));
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eori, ZString.Empty);
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = grouping;
			dec.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var header = dec.CustomsEntryHeaders.AddNew();
			var line = header.MergedLines.AddNew();
			var fee = line.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 99.99m);
			fee.CF_MethodOfPayment = mop;

			AssertEquals("Pre-requisite: Submitter's org should specify the expected EORI", eori, dec.Branch.OrgProxy.GetEuIdentificationNumber());

			AssertEquals("Duty not charged to broker if no guarantee given", false, header.IsFeePaidByBroker(RefCusRateTypes.Dty, mop, logger));
			AssertEquals("Log item", $"Fee DTY with MoP={mop} excluded because no cash guarantee is given", logger.Logs[0].Item2);

			const string otherEori = "GB5678";
			AssertNotEquals("Pre-requisite: EORI != Other EORI", eori, otherEori);

			var guarantee = dec.Guarantees.AddNew();
			guarantee.PW_BondType = "G";
			guarantee.PW_BondNumber = otherEori;
			guarantee.PW_Password = "Y";
			logger.Logs.Clear();
			AssertEquals("Duty not charged to broker if guarantee is specified with other account", false, header.IsFeePaidByBroker(RefCusRateTypes.Dty, mop, logger));
			AssertEquals("Log item", $"Fee DTY with MoP={mop} excluded because the submitter's EORI {eori} does not match the guarantee {otherEori}", logger.Logs[0].Item2);
			guarantee.PW_BondNumber2 = otherEori;
			AssertEquals("Duty not charged to broker if guarantee is specified with other account (Ref&GRN)", false, header.IsFeePaidByBroker(RefCusRateTypes.Dty, mop, logger));
			guarantee.PW_BondNumber = ZString.Empty;
			AssertEquals("Duty not charged to broker if guarantee is specified with other account (GRN)", false, header.IsFeePaidByBroker(RefCusRateTypes.Dty, mop, logger));
			guarantee.PW_BondNumber2 = ZString.Empty;
			guarantee.PW_HolderIdentification = otherEori;
			AssertEquals("Duty not charged to broker if guarantee is specified with other account (HolderIdentification)", false, header.IsFeePaidByBroker(RefCusRateTypes.Dty, mop, logger));
			guarantee.PW_HolderIdentification = ZString.Empty;

			guarantee.PW_BondNumber = eori;
			logger.Logs.Clear();
			AssertEquals("Duty is charged to broker if guarantee is specified with own account", true, header.IsFeePaidByBroker(RefCusRateTypes.Dty, mop, logger));
			AssertEquals("Log item", $"Fee DTY with MoP={mop} included because the submitter's EORI {eori} matches the guarantee {eori}", logger.Logs[0].Item2);

			guarantee.PW_BondNumber2 = eori;
			AssertEquals("Duty is charged to broker if guarantee is specified with own account (Ref&GRN)", true, header.IsFeePaidByBroker(RefCusRateTypes.Dty, mop, logger));
			guarantee.PW_BondNumber = ZString.Empty;
			AssertEquals("Duty is charged to broker if guarantee is specified with own account (GRN)", true, header.IsFeePaidByBroker(RefCusRateTypes.Dty, mop, logger));
			guarantee.PW_BondNumber2 = ZString.Empty;
			guarantee.PW_HolderIdentification = eori;
			AssertEquals("Duty is charged to broker if guarantee is specified with own account (HolderIdentification)", true, header.IsFeePaidByBroker(RefCusRateTypes.Dty, mop, logger));

			logger.Logs.Clear();
			guarantee.PW_BondNumber = otherEori;
			AssertEquals("Duty not charged to broker when ambiguous accounts are given", false, header.IsFeePaidByBroker(RefCusRateTypes.Dty, mop, logger));
			AssertEquals("Log item", $"Fee DTY with MoP={mop} excluded because multiple potential values for cash guarantee EORI were found", logger.Logs[0].Item2);

			guarantee.PW_HolderIdentification = ZString.Empty;
			var guarantee2 = dec.Guarantees.AddNew();
			guarantee2.PW_BondType = "G";
			guarantee2.PW_HolderIdentification = eori;
			guarantee2.PW_Password = "Y";
			logger.Logs.Clear();
			AssertEquals("Duty not charged to broker when ambiguous accounts are given", false, header.IsFeePaidByBroker(RefCusRateTypes.Dty, mop, logger));
			AssertEquals("Log item", $"Fee DTY with MoP={mop} excluded because multiple potential values for cash guarantee EORI were found", logger.Logs[0].Item2);
		}

		public void TestDutyBillingWithTwoEntryLines()
		{
			var logger = new DetailedLoggerForTest();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var gb = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom);
			const string grouping = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			helper.CreateNewOrGetExistingDataGrouping(grouping, Registry.Business.DeclarationApplicationCodeList.Descriptions.Customs_Declaration_Services, gb);
			var dut = helper.CreateNewOrGetExistingRateType(grouping, RefCusRateTypes.Dty, "Duty");
			dut.ZZR_IsPayable = true;
			helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dut.PK);
			const string mop = "P";
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, "Method of Payment");
			var mop0 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, mop, mop, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(mop0.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AutoRating, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Autorating_AlwaysBroker);
			var mop1 = helper.CreateNewOrGetExistingCusCodeList(grouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.MethodOfPayment, mop, mop, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(mop1.PK, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.AutoRating, Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Autorating_SeeDataElement83);

			const string eori = "GB1234";
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, GlbBranch.CurrentBranch.OrgProxy.PK));
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eori, ZString.Empty);
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = grouping;
			dec.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var header1 = dec.CustomsEntryHeaders.AddNew();
			var line1 = header1.MergedLines.AddNew();
			var inst1 = dec.CustomsEntryInstructions.AddNew();
			header1.CH_CEI_Instruction = inst1.PK;
			var fee1 = line1.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 99.99m);
			fee1.CF_MethodOfPayment = mop;
			var header2 = dec.CustomsEntryHeaders.AddNew();
			var line2 = header2.MergedLines.AddNew();
			var inst2 = dec.CustomsEntryInstructions.AddNew();
			header2.CH_CEI_Instruction = inst2.PK;
			var fee2 = line1.Fees.AddOrUpdate(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, 39.99m);
			fee2.CF_MethodOfPayment = mop;

			const string otherEori = "GB5678";
			AssertNotEquals("Pre-requisite: EORI != Other EORI", eori, otherEori);

			var guarantee1 = dec.Guarantees.AddNew();
			guarantee1.PW_BondType = "G";
			guarantee1.PW_BondNumber = eori;
			guarantee1.PW_Password = "Y";
			guarantee1.EntryInstructionID = inst1.PK;
			var guarantee2 = dec.Guarantees.AddNew();
			guarantee2.PW_BondType = "G";
			guarantee2.PW_BondNumber = otherEori;
			guarantee2.PW_Password = "Y";
			guarantee2.EntryInstructionID = inst2.PK;

			AssertEquals("Duty is charged to broker if guarantee is specified with own account", true, header1.IsFeePaidByBroker(RefCusRateTypes.Dty, mop, logger));
			AssertEquals("Duty not charged to broker if guarantee is specified with other account", false, header2.IsFeePaidByBroker(RefCusRateTypes.Dty, mop, logger));
		}
	}
}
