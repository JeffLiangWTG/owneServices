using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class PopulateGuaranteesHelper
	{
		public void PopulateGuaranteesIfApplicable(JobDeclaration declaration)
		{
			if (!entryStatusNotPopulateGuarantees.Contains(declaration.JE_EntryStatus))
			{
				foreach (CusEntryInstruction entryInstructions in declaration.CustomsEntryInstructions)
				{
					CheckInvoiceLineIfNotWaranteesHaveAndCreate(declaration, entryInstructions);
				}
			}
		}

		public void CheckInvoiceLineIfNotWaranteesHaveAndCreate(JobDeclaration declaration, CusEntryInstruction entryInstructions)
		{
			if (!declaration.Guarantees.Cast<ESGuarantee>().Any(x => x.EntryInstructionID == entryInstructions.PK))
			{
				foreach (JobComInvoiceLine invoiceLine in declaration.InvoiceLines)
				{
					if (invoiceLine.JI_CEI == entryInstructions.PK)
					{
						CheckInvoiceLineForGuarantees(declaration, invoiceLine);
					}
				}
			}
		}

		public void CheckInvoiceLineForGuarantees(JobDeclaration declaration, JobComInvoiceLine invoiceLine)
		{
			var invoiceLineJI_CEI = invoiceLine.JI_CEI;
			var formattedProcedureTwoFirstPosition = invoiceLine.JI_FormattedProcedure.Substring(0, 2);
			var shouldCalculateA = ShouldCalculateTypeA(declaration, invoiceLine);
			var shouldCalculateC = ShouldCalculateTypeC(declaration, invoiceLine);
			var characterseventhPosition = FreePractice;
			if (invoiceLine.EntryInstruction != null && !invoiceLine.EntryInstruction.IsT2C && !invoiceLine.EntryInstruction.IsT2L)
			{
				CheckRealDebtGuranteesAndAddIfRequired(declaration, formattedProcedureTwoFirstPosition, invoiceLineJI_CEI, shouldCalculateA, shouldCalculateC);

				characterseventhPosition = SetcharacterseventhPosition(characterseventhPosition, formattedProcedureTwoFirstPosition);
				if (preferencesPopulateGuaranteesPotentialDebtDDA.Contains(formattedProcedureTwoFirstPosition))
				{
					characterseventhPosition = PotentialDepositOtherThanCustomsDeposit;
				}
				CheckDiferentFreePracticeGuaranteesAndAddIfRequired(declaration, characterseventhPosition, invoiceLineJI_CEI, formattedProcedureTwoFirstPosition, invoiceLine.JI_PrimaryPreference, shouldCalculateA, shouldCalculateC);
			}
		}

		void CheckDiferentFreePracticeGuaranteesAndAddIfRequired(JobDeclaration declaration, string characterseventhPosition, ZGuid invoiceLineJI_CEI, ZString formattedProcedureTwoFirstPosition, ZString primariPreference, bool shouldCalculateA, bool shouldCalculateC)
		{
			if (characterseventhPosition != FreePractice)
			{
				if (shouldCalculateA)
				{
					AddImporterOrDeclarantGuaranteesIfNotExist(declaration, characterseventhPosition, invoiceLineJI_CEI, AEATGuarantees);
				}
				if (shouldCalculateC && CharacterFifthPositionOfGuarantee(declaration) == ATCGuarantees)
				{
					AddImporterOrDeclarantGuaranteesIfNotExist(declaration, characterseventhPosition, invoiceLineJI_CEI, ATCGuarantees);
				}
			}
			if (!primariPreference.IsEmpty)
			{
				if ((formattedProcedureTwoFirstPosition == ConsumerDispatchAndFreePractice) && (preferencesPopulateGuaranteesCPC40.Contains(primariPreference.Substring(1, 2))))
				{
					if (shouldCalculateA)
					{
						AddImporterOrDeclarantGuaranteesIfNotExist(declaration, PotentialRegime44, invoiceLineJI_CEI, AEATGuarantees);
					}
					if (shouldCalculateC && CharacterFifthPositionOfGuarantee(declaration) == ATCGuarantees)
					{
						AddImporterOrDeclarantGuaranteesIfNotExist(declaration, PotentialRegime44, invoiceLineJI_CEI, ATCGuarantees);
					}
				}
			}
		}

		void AddImporterOrDeclarantGuaranteesIfNotExist(JobDeclaration declaration, string regimeType, ZGuid entryInstructionPK, ZString guaranteeType)
		{
			var importerGuaranteesAdded = AddGuaranteesIfNotExist(declaration, declaration.JE_OH_Importer, regimeType, entryInstructionPK, guaranteeType);

			if (!importerGuaranteesAdded && declaration.Declarant != null && declaration.JE_OH_Importer != declaration.Declarant.OA_OH)
			{
				AddGuaranteesIfNotExist(declaration, declaration.Declarant.OA_OH, regimeType, entryInstructionPK, guaranteeType);
			}
		}

		void CheckRealDebtGuranteesAndAddIfRequired(JobDeclaration declaration, ZString formattedProcedureTwoFirstPosition, ZGuid invoiceLineJI_CEI, bool shouldCalculateA, bool shouldCalculateC)
		{
			if (preferencesPopulateGuaranteesRealDebt.Contains(formattedProcedureTwoFirstPosition))
			{
				if (shouldCalculateA)
				{
					AddImporterOrDeclarantGuaranteesIfNotExist(declaration, FreePractice, invoiceLineJI_CEI, AEATGuarantees);
				}
				if (shouldCalculateC && CharacterFifthPositionOfGuarantee(declaration) == ATCGuarantees)
				{
					AddImporterOrDeclarantGuaranteesIfNotExist(declaration, FreePractice, invoiceLineJI_CEI, ATCGuarantees);
				}
			}
		}

		ZString SetcharacterseventhPosition(string characterseventhPosition, ZString formattedProcedureTwoFirstPosition)
		{
			switch (formattedProcedureTwoFirstPosition)
			{
				case ReleaseToFreeCirculationWithDutyRelief:
					characterseventhPosition = PotentialRegime44;
					break;
				case ReimportationOfStandardExchanges:
					characterseventhPosition = PotentialRegime48;
					break;
				case InclusionInTheImprovementRegime:
					characterseventhPosition = PotentialRegime51IncludesRiscalRPA;
					break;
				case TemporaryImport:
					characterseventhPosition = PotentialRegime53IncludesTaxIT;
					break;
				case BondingToCustomsWarehouse:
					characterseventhPosition = PotentialRegime71;
					break;
			}
			return characterseventhPosition;
		}

		bool AddGuaranteesIfNotExist(JobDeclaration declaration, ZGuid entity, string characterSeventhPosition, ZGuid entryInstructionPK, ZString fifthPositionOfGuarantee)
		{
			var createdOrExist = false;
			var query = new ZQuery(CusPermitHeaderSchema.CPH_ApplicationCode, CusPermitHeaderApplicationCodeList.Codes.Guarantee);
			query.AddToFilter(CusPermitHeaderSchema.CPH_OH_PermitHolder, entity);
			query.AddToFilter(CusPermitHeaderSchema.CPH_Type, EUGuaranteeTypeList.Codes.IMP);
			var loadCusGuaranteeHeader = declaration.Factory.Load<CusGuaranteeHeader>(query);

			foreach (CusGuaranteeHeader cusGuaranteeHeader in loadCusGuaranteeHeader)
			{
				var exist = declaration.Guarantees.Cast<ESGuarantee>().Any(x => x.PW_BondNumber == cusGuaranteeHeader.CPH_Number && x.EntryInstructionID == entryInstructionPK);
				if ((cusGuaranteeHeader.CPH_EndDate > ZDateTime.Today || cusGuaranteeHeader.CPH_EndDate.IsEmpty) &&
					(cusGuaranteeHeader.CPH_Number.Substring(4, 1) == fifthPositionOfGuarantee) &&
					(cusGuaranteeHeader.CPH_Number.Substring(6, 1) == characterSeventhPosition))
				{
					if (!exist)
					{
						var guarantee = declaration.Guarantees.AddNew();
						guarantee.PW_BondNumber = cusGuaranteeHeader.CPH_Number;
						guarantee.EntryInstructionID = entryInstructionPK;
						guarantee.PW_BondFiledPort = cusGuaranteeHeader.DefaultPW_BondFiledPortForGuarantee;
					}
					createdOrExist = true;
				}
			}
			return createdOrExist;
		}

		ZString CharacterFifthPositionOfGuarantee(JobDeclaration declaration) => declaration.DestinationStateIsCanaryIsland ? ATCGuarantees : AEATGuarantees;
		bool ShouldCalculateTypeA(JobDeclaration declaration, JobComInvoiceLine invoiceLine) => invoiceLine.ZG_MethodOfPayment == MethodOfPaymentList.Codes.R;
		bool ShouldCalculateTypeC(JobDeclaration declaration, JobComInvoiceLine invoiceLine) => invoiceLine.ZG_MethodOfPayment2 == MethodOfPaymentList.Codes.R && declaration.DestinationStateIsCanaryIsland;

		readonly ZString[] entryStatusNotPopulateGuarantees = new ZString[] { EntryStatusCodes.PreDeclarationAccepted, EntryStatusCodes.CustomsDeclarationAccepted, EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, EntryStatusCodes.Cleared };
		readonly ZString[] preferencesPopulateGuaranteesCPC40 = new ZString[] { SuspendedGoodsWithSpecialDestination, GoodsUnderTariffQuotaWithSpecialDestination, GoodsSubjectToSpecialDestination, GoodsCoveredByCanarySpecificTariffMeasuresAndSpecialDestinations, GoodsUnderSpecialSupplyRegimeAndSpecialDestination, GoodsUnderSpecialSupplyRegimeREAInCanary, GoodssubjectSpecialDestinationSuspensionForCertainWeaponsAndMilitaryWeaponsAndEquipment };
		readonly ZString[] preferencesPopulateGuaranteesRealDebt = new ZString[] { FreePracticeDispatchOfCEEGoods, FreePracticeAndIntroductionInDeposit, ConsumerDispatchAndFreePractice, ConsumerDispatchAndLPExemptFromVATShipmentToEM, ReleaseToFreeCirculationWithDutyRelief, ReimportationOfStandardExchanges, ConsumerDispatchDirective2006112EC, InclusionInTheImprovementRegime, TemporaryImport, Return, ReturnAndShipmentToAnotherEM, ConsumesPreviousGoodsInWarehouseRefCorreos };
		readonly ZString[] preferencesPopulateGuaranteesPotentialDebtDDA = new ZString[] { FreePracticeAndIntroductionInDeposit, ConsumerDispatchDirective2006112EC, DDAWarehouses73, DDAWarehouses78 };

		const string AEATGuarantees = "A";
		const string ATCGuarantees = "C";

		const string FreePractice = "L";
		const string PotentialRegime44 = "P";
		const string PotentialRegime48 = "Q";
		const string PotentialRegime51IncludesRiscalRPA = "R";
		const string PotentialRegime53IncludesTaxIT = "S";
		const string PotentialRegime71 = "T";
		const string PotentialDepositOtherThanCustomsDeposit = "D";

		const string FreePracticeDispatchOfCEEGoods = "01";
		const string FreePracticeAndIntroductionInDeposit = "07";
		const string ConsumerDispatchAndFreePractice = "40";
		const string ConsumerDispatchAndLPExemptFromVATShipmentToEM = "42";
		const string ReleaseToFreeCirculationWithDutyRelief = "44";
		const string ReimportationOfStandardExchanges = "48";
		const string ConsumerDispatchDirective2006112EC = "49";
		const string InclusionInTheImprovementRegime = "51";
		const string TemporaryImport = "53";
		const string Return = "61";
		const string ReturnAndShipmentToAnotherEM = "63";
		const string BondingToCustomsWarehouse = "71";
		const string DDAWarehouses73 = "73";
		const string DDAWarehouses78 = "78";
		const string ConsumesPreviousGoodsInWarehouseRefCorreos = "80";

		const string SuspendedGoodsWithSpecialDestination = "15";
		const string GoodsUnderTariffQuotaWithSpecialDestination = "23";
		const string GoodsSubjectToSpecialDestination = "40";
		const string GoodsCoveredByCanarySpecificTariffMeasuresAndSpecialDestinations = "84";
		const string GoodsUnderSpecialSupplyRegimeAndSpecialDestination = "86";
		const string GoodsUnderSpecialSupplyRegimeREAInCanary = "85";
		const string GoodssubjectSpecialDestinationSuspensionForCertainWeaponsAndMilitaryWeaponsAndEquipment = "96";
	}
}
