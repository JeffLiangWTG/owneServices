using System;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.DE.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestsSubclassesOf(typeof(ImportHeaderProvider))]
	public abstract class ImportHeaderProviderAbstractTest<T> : Customs.Business.Testing.DataProviderTestCase<T>
		where T : ImportHeaderProvider
	{
		protected OrgAddress GetOrgWithEORNumberAndEORIBranch(ZString eoriNumber, ZString ebsNumber)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, eoriNumber, Core.Constants.CountryCodes.Greece);
			var address = orgHeader.Addresses.AddNew();
			address.AddAddressType(OrgAddressType.CustomsAddressOfRecord);
			var ebsCode = orgHeader.CustomsCodes.AddNew(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, ebsNumber, Core.Constants.CountryCodes.Germany);
			ebsCode.OK_OA_PremisesAddress = address.PK;
			return address;
		}

		protected JobComInvoiceHeader AddInvoiceWithInvoiceLine()
		{
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			return invoice;
		}

		protected void AssertLocalClearanceProcedureFromPrimaryAuthorizationHolder(ZString cei_Style, ZString representationType, bool isSimplifiedDeclarationAuthorisationType, Func<ZString> localClearanceProcedure, string usageRule = CusAuthorisationUsageRuleList.Codes.FreeCirculation)
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.CreateAuthorisationWithRule(Customs.Business.CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DEC123_SDE", CusAuthorisationRuleTypeList.Codes.Usage, usageRule);
			var (auth2, _) = declarant.CreateAuthorisationWithRule(Customs.Business.CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DEC456_SDE", CusAuthorisationRuleTypeList.Codes.Usage, usageRule);
			auth2.CPH_StartDate = ZDate.Today.AddMonths(-3);
			auth2.CPH_EndDate = ZDate.Today.AddMonths(-1);
			declarant.CreateAuthorisationWithRule(Customs.Business.CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "DEC123_EIR", CusAuthorisationRuleTypeList.Codes.Usage, usageRule);
			var (auth4, _) = declarant.CreateAuthorisationWithRule(Customs.Business.CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "DEC456_EIR", CusAuthorisationRuleTypeList.Codes.Usage, usageRule);
			auth4.CPH_StartDate = ZDate.Today.AddMonths(-3);
			auth4.CPH_EndDate = ZDate.Today.AddMonths(-1);
			Factory.Save();

			entryInstruction.CEI_Style = cei_Style;
			entryInstruction.CEI_LocalClearanceDate = ZDate.Today.AddMonths(-2);
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_DeclarantType = representationType;

			var necessaryAuthorizationType = isSimplifiedDeclarationAuthorisationType ? Customs.Business.CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration : Customs.Business.CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			AssertEquals("Declarants authorization", isSimplifiedDeclarationAuthorisationType ? $"DEC123_{necessaryAuthorizationType}" : $"DEC456_{necessaryAuthorizationType}", localClearanceProcedure.Invoke());
		}

		protected void AssertLocalClearanceProcedureFromSecondaryAuthorizationHolder(string cei_Style, string representationType, bool isSimplifiedDeclarationAuthorisationType, Func<ZString> localClearanceProcedure, string usageRule = CusAuthorisationUsageRuleList.Codes.FreeCirculation)
		{
			var representative = Factory.New<OrgHeader>();
			representative.OH_Code = "REPRESENT";
			representative.CreateAuthorisationWithRule(Customs.Business.CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "REP123_SDE", CusAuthorisationRuleTypeList.Codes.Usage, usageRule);
			representative.CreateAuthorisationWithRule(Customs.Business.CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "REP123_EIR", CusAuthorisationRuleTypeList.Codes.Usage, usageRule);
			var (rep3, _) = representative.CreateAuthorisationWithRule(Customs.Business.CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "REP456_SDE", CusAuthorisationRuleTypeList.Codes.Usage, usageRule);
			rep3.CPH_StartDate = ZDate.Today.AddMonths(-3);
			rep3.CPH_EndDate = ZDate.Today.AddMonths(-1);
			var (rep4, _) = representative.CreateAuthorisationWithRule(Customs.Business.CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "REP456_EIR", CusAuthorisationRuleTypeList.Codes.Usage, usageRule);
			rep4.CPH_StartDate = ZDate.Today.AddMonths(-3);
			rep4.CPH_EndDate = ZDate.Today.AddMonths(-1);

			var buyingAgent = Factory.New<OrgHeader>();
			buyingAgent.OH_Code = "BUYINGAGENT";
			buyingAgent.CreateAuthorisationWithRule(Customs.Business.CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "BUY123_SDE", CusAuthorisationRuleTypeList.Codes.Usage, usageRule);
			buyingAgent.CreateAuthorisationWithRule(Customs.Business.CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "BUY123_EIR", CusAuthorisationRuleTypeList.Codes.Usage, usageRule);
			var (buy3, _) = buyingAgent.CreateAuthorisationWithRule(Customs.Business.CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "BUY456_SDE", CusAuthorisationRuleTypeList.Codes.Usage, usageRule);
			buy3.CPH_StartDate = ZDate.Today.AddMonths(-3);
			buy3.CPH_EndDate = ZDate.Today.AddMonths(-1);
			var (buy4, _) = buyingAgent.CreateAuthorisationWithRule(Customs.Business.CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "BUY456_EIR", CusAuthorisationRuleTypeList.Codes.Usage, usageRule);
			buy4.CPH_StartDate = ZDate.Today.AddMonths(-3);
			buy4.CPH_EndDate = ZDate.Today.AddMonths(-1);
			Factory.Save();

			entryInstruction.CEI_Style = cei_Style;
			entryInstruction.CEI_LocalClearanceDate = ZDate.Today.AddMonths(-2);
			declaration.JE_OA_Representative = representative.MainAddress.PK;
			declaration.JE_OA_BuyingAgentAddress = buyingAgent.MainAddress.PK;
			declaration.JE_DeclarantType = representationType;

			var necessaryAuthorizationType = isSimplifiedDeclarationAuthorisationType ? Customs.Business.CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration : Customs.Business.CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;

			var expectedFallbackLocalClearanceProcedure = ZString.Empty;
			if (representationType == EU.Business.RepresentationTypeList.Codes._2Direct)
			{
				expectedFallbackLocalClearanceProcedure = isSimplifiedDeclarationAuthorisationType ? $"REP123_{necessaryAuthorizationType}" : $"REP456_{necessaryAuthorizationType}";
			}
			else if (representationType == EU.Business.RepresentationTypeList.Codes._3Indirect)
			{
				expectedFallbackLocalClearanceProcedure = isSimplifiedDeclarationAuthorisationType ? $"BUY123_{necessaryAuthorizationType}" : $"BUY456_{necessaryAuthorizationType}";
			}
			AssertEquals("FallBackToRepresentative", expectedFallbackLocalClearanceProcedure, localClearanceProcedure.Invoke());
		}

		protected void AssertLocalClearanceProcedureWithInvalidParameters(ZString cei_Style, ZString representationType, ZDate localClearanceDate, Func<ZString> localClearanceProcedure, string usageRule = CusAuthorisationUsageRuleList.Codes.FreeCirculation)
		{
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "DECLARANT";
			declarant.CreateAuthorisationWithRule(Customs.Business.CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "DEC123", CusAuthorisationRuleTypeList.Codes.Usage, usageRule);
			declarant.CreateAuthorisationWithRule(Customs.Business.CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "DEC456", CusAuthorisationRuleTypeList.Codes.Usage, usageRule);
			var representative = Factory.New<OrgHeader>();
			representative.OH_Code = "REPRESENT";
			representative.CreateAuthorisationWithRule(Customs.Business.CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "REP123", CusAuthorisationRuleTypeList.Codes.Usage, usageRule);
			representative.CreateAuthorisationWithRule(Customs.Business.CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "REP456", CusAuthorisationRuleTypeList.Codes.Usage, usageRule);
			var buyingAgent = Factory.New<OrgHeader>();
			buyingAgent.OH_Code = "BUYINGAGENT";
			buyingAgent.CreateAuthorisationWithRule(Customs.Business.CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration, "BUY123", CusAuthorisationRuleTypeList.Codes.Usage, usageRule);
			buyingAgent.CreateAuthorisationWithRule(Customs.Business.CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "BUY456", CusAuthorisationRuleTypeList.Codes.Usage, usageRule);
			Factory.Save();

			entryInstruction.CEI_Style = cei_Style;
			entryInstruction.CEI_LocalClearanceDate = localClearanceDate;
			declaration.JE_OA_Representative = representative.MainAddress.PK;
			declaration.JE_OA_DeclarantAddress = declarant.MainAddress.PK;
			declaration.JE_OA_BuyingAgentAddress = representative.MainAddress.PK;
			declaration.JE_DeclarantType = representationType;

			AssertEquals(ZString.Empty, localClearanceProcedure.Invoke());
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
		}
		protected JobDeclaration declaration;
		protected CusEntryHeader entryHeader;
		protected CusEntryInstruction entryInstruction;
	}
}
