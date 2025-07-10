using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using static Enterprise.Customs.Business.CusAuthorizationHeaderTypeList;
using CusAuthorisationHeader = Enterprise.Customs.Business.CusAuthorisationHeader;

namespace Enterprise.Customs.DE.Business
{
	class CusAuthorizationUsageExportUpdater : CusAuthorizationUsageUpdater
	{
		public CusAuthorizationUsageExportUpdater(JobDeclaration jobDeclaration) : base(jobDeclaration)
		{
			declaration = jobDeclaration;
			AddRuleConfigurationForAuthorizationTypeSDE();
			AddRuleConfigurationForAuthorizationTypeCCL();
			AddRuleConfigurationForAuthorizationTypeOPO();
			AddRuleConfigurationForAuthorizationTypeEIR();
		}
		readonly JobDeclaration declaration;

		void AddRuleConfigurationForAuthorizationTypeSDE()
		{
			ForAuthorizationType(Codes.SimplifiedDeclaration)
				.When(euEntryInstruction => ((CusEntryInstruction)euEntryInstruction).IsSDEExportOrSDEOutwardProcessing())
				.ReturnHolderAndNumber(euEntryInstruction =>
				{
					var entryInstruction = (CusEntryInstruction)euEntryInstruction;
					var onlyFromDeclarant = entryInstruction.Constellation2ndDigitIs0And3rdIs0() || entryInstruction.Constellation2ndDigitIs1And3rdIs0() || entryInstruction.Constellation3rdDigitIs0And4thIs0();
					var fromDeclarantThenRepresentative = entryInstruction.Constellation2ndDigitIs0And3rdIs1() || entryInstruction.Constellation2ndDigitIs1And3rdIs1() || entryInstruction.Constellation3rdDigitIs1And4thIs0();
					var list = Array.Empty<CusAuthorisationHeader>();
					if (onlyFromDeclarant || fromDeclarantThenRepresentative)
					{
						var ruleUsageCode = entryInstruction.IsSDEExport() ? CusAuthorisationUsageRuleList.Codes.AccreditedExporter : CusAuthorisationUsageRuleList.Codes.OutwardProcessingProcedure;
						if (!DeclarantOrgHeaderPK.IsEmpty)
						{
							list = GetAuthorisationsWithUsageRule(Codes.SimplifiedDeclaration, DeclarantOrgHeaderPK, ruleUsageCode);
						}
						if (NeedGetListFromRepresentative(fromDeclarantThenRepresentative, list))
						{
							list = GetAuthorisationsWithUsageRule(Codes.SimplifiedDeclaration, RepresentativeOrgHeaderPK, ruleUsageCode);
						}
					}
					return GetSingleAuthorization(list);
				});
		}

		void AddRuleConfigurationForAuthorizationTypeCCL()
		{
			ForAuthorizationType(Codes.CentralizedClearance)
				.When(euEntryInstruction => ((CusEntryInstruction)euEntryInstruction).IsCCLExport())
				.ReturnHolderAndNumber(euEntryInstruction =>
				{
					var entryInstruction = (CusEntryInstruction)euEntryInstruction;
					var onlyFromDeclarant = entryInstruction.Constellation3rdDigitIs0();
					var fromDeclarantThenRepresentative = entryInstruction.Constellation3rdDigitIs1();
					var list = Array.Empty<CusAuthorisationHeader>();
					if (onlyFromDeclarant || fromDeclarantThenRepresentative)
					{
						if (!DeclarantOrgHeaderPK.IsEmpty)
						{
							list = GetAuthorisations(Codes.CentralizedClearance, DeclarantOrgHeaderPK);
						}
						if (NeedGetListFromRepresentative(fromDeclarantThenRepresentative, list))
						{
							list = GetAuthorisations(Codes.CentralizedClearance, RepresentativeOrgHeaderPK);
						}
					}
					return GetSingleAuthorization(list);
				});
		}

		void AddRuleConfigurationForAuthorizationTypeOPO()
		{
			ForAuthorizationType(Codes.OutwardProcessing)
				.When(euEntryInstruction => ((CusEntryInstruction)euEntryInstruction).IsOPOOutwardProcessing())
				.ReturnHolderAndNumber(euEntryInstruction =>
				{
					var list = Array.Empty<CusAuthorisationHeader>();
					if (!DeclarantOrgHeaderPK.IsEmpty)
					{
						list = GetAuthorisations(Codes.OutwardProcessing, DeclarantOrgHeaderPK);
					}
					return GetSingleAuthorization(list);
				});
		}

		void AddRuleConfigurationForAuthorizationTypeEIR()
		{
			ForAuthorizationType(Codes.EntryOfDataInTheDeclarantsRecords)
				.When(euEntryInstruction => ExportDeclarationTypeTimeList.IsMultipleDeclarationForExport(euEntryInstruction.CEI_SubStyle))
				.ReturnHolderAndNumber(euEntryInstruction =>
				{
					var list = Array.Empty<CusAuthorisationHeader>();
					if (!DeclarantOrgHeaderPK.IsEmpty)
					{
						list = GetAuthorisationsWithUsageRule(Codes.EntryOfDataInTheDeclarantsRecords, DeclarantOrgHeaderPK, CusAuthorisationUsageRuleList.Codes.AccreditedExporter);
					}
					return GetSingleAuthorization(list);
				});
		}

		ZBool NeedGetListFromRepresentative(ZBool fromDeclarantThenRepresentative, IEnumerable<CusAuthorisationHeader> listFromDeclarant) => fromDeclarantThenRepresentative && !RepresentativeOrgHeaderPK.IsEmpty && !listFromDeclarant.Any();

		(ZGuid, ZString) GetSingleAuthorization(CusAuthorisationHeader[] authorizations)
		{
			var result = (ZGuid.Empty, ZString.Empty);
			if (authorizations.Length == 1)
			{
				var auth = authorizations[0];
				result = (auth.CPH_OH_PermitHolder, auth.CPH_Number);
			}
			return result;
		}

		CusAuthorisationHeader[] GetAuthorisationsWithUsageRule(ZString type, ZGuid permitHolder, ZString ruleValue) =>
			CusAuthorisationHeader.Loader.GetAuthorisationsWithSpecificRule(declaration.Factory, new ZString[] { type }, new[] { permitHolder }, Core.Constants.CountryCodes.Germany, ZDateTime.Today, CusAuthorisationRuleTypeList.Codes.Usage, ruleValue);

		CusAuthorisationHeader[] GetAuthorisations(ZString type, ZGuid permitHolder) =>
			CusAuthorisationHeader.Loader.GetAuthorisations(declaration.Factory, Core.Constants.CountryCodes.Germany, new ZString[] { type }, ZDateTime.Today, new[] { permitHolder });

		ZGuid DeclarantOrgHeaderPK => declaration.Declarant?.Header?.PK ?? ZGuid.Empty;

		ZGuid RepresentativeOrgHeaderPK => declaration.Representative?.Header?.PK ?? ZGuid.Empty;
	}
}
