using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.Business.CusAuthorizationHeaderTypeList;
using CusAuthorisationHeader = Enterprise.Customs.Business.CusAuthorisationHeader;

namespace Enterprise.Customs.DE.Business
{
	class CusAuthorizationUsageImportUpdater : CusAuthorizationUsageUpdater
	{
		public CusAuthorizationUsageImportUpdater(JobDeclaration jobDeclaration) : base(jobDeclaration)
		{
			declaration = jobDeclaration;
			AddRuleConfigurationForAuthorizationTypeSDE();
			AddRuleConfigurationForAuthorizationTypeEIR();
			AddRuleConfigurationForAuthorizationTypeIPO();
			AddRuleConfigurationForAuthorizationTypeEUS();
			AddRuleConfigurationForAuthorizationTypeCWP();
			AddRuleConfigurationForAuthorizationTypeCW1();
		}

		void AddRuleConfigurationForAuthorizationTypeSDE()
		{
			ForAuthorizationType(Codes.SimplifiedDeclaration)
				.When(entryInstruction => entryInstruction.CEI_Style.In(new ZString[] { ImportDeclarationTypeList.Codes.VAV, ImportDeclarationTypeList.Codes.VZA, ImportDeclarationTypeList.Codes.VZL }))
				.ReturnHolderAndNumber(entryInstruction => GetSDEAuthorizationHolderAndNumber((CusEntryInstruction)entryInstruction));

			(ZGuid holder, ZString number) GetSDEAuthorizationHolderAndNumber(CusEntryInstruction entryInstruction)
			{
				var result = (ZGuid.Empty, ZString.Empty);
				switch (entryInstruction.CEI_Style)
				{
					case ImportDeclarationTypeList.Codes.VAV:
						result = GetLocalClearanceProcedureAuthorizationNumber(entryInstruction, Codes.SimplifiedDeclaration, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
						break;
					case ImportDeclarationTypeList.Codes.VZA:
						result = GetLocalClearanceProcedureAuthorizationNumber(entryInstruction, Codes.SimplifiedDeclaration, CusAuthorisationUsageRuleList.Codes.FreeCirculation);
						break;
					case ImportDeclarationTypeList.Codes.VZL:
						result = GetAuthorisationWithCustomsWarehousingRuleFromDeclarant(entryInstruction, Codes.SimplifiedDeclaration);
						break;
				}
				return result;
			}
		}

		void AddRuleConfigurationForAuthorizationTypeEIR()
		{
			ForAuthorizationType(Codes.EntryOfDataInTheDeclarantsRecords)
				.When(entryInstruction => entryInstruction.CEI_Style.In(new ZString[] { ImportDeclarationTypeList.Codes.AAV, ImportDeclarationTypeList.Codes.AZ, ImportDeclarationTypeList.Codes.AZL }))
				.ReturnHolderAndNumber(entryInstruction => GetEIRAuthorizationHolderAndNumber((CusEntryInstruction)entryInstruction));

			(ZGuid holder, ZString number) GetEIRAuthorizationHolderAndNumber(CusEntryInstruction entryInstruction)
			{
				var result = (ZGuid.Empty, ZString.Empty);
				switch (entryInstruction.CEI_Style)
				{
					case ImportDeclarationTypeList.Codes.AAV:
						result = GetLocalClearanceProcedureAuthorizationNumber(entryInstruction, Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationUsageRuleList.Codes.InwardProcessingProcedure);
						break;
					case ImportDeclarationTypeList.Codes.AZ:
						result = GetLocalClearanceProcedureAuthorizationNumber(entryInstruction, Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationUsageRuleList.Codes.FreeCirculation);
						break;
					case ImportDeclarationTypeList.Codes.AZL:
						result = GetAuthorisationWithCustomsWarehousingRuleFromDeclarant(entryInstruction, Codes.EntryOfDataInTheDeclarantsRecords);
						break;
				}
				return result;
			}
		}

		void AddRuleConfigurationForAuthorizationTypeIPO()
		{
			ForAuthorizationType(Codes.InwardProcessing)
				.When(entryInstruction => entryInstruction.CEI_Style.In(new ZString[] { ImportDeclarationTypeList.Codes.AAV, ImportDeclarationTypeList.Codes.VAV }))
				.ReturnHolderAndNumber(entryInstruction => GetIPOAuthorizationHolderAndNumber((CusEntryInstruction)entryInstruction));

			(ZGuid holder, ZString number) GetIPOAuthorizationHolderAndNumber(CusEntryInstruction entryInstruction)
			{
				var result = (ZGuid.Empty, ZString.Empty);
				var authorizationHolders = GetAuthorisationHolders();
				if (authorizationHolders.Length > 0)
				{
					var authorizations = CusAuthorisationHeader.Loader.GetAuthorisations(
						declaration.Factory,
						Constants.CountryCodes.Germany,
						new ZString[] { Codes.InwardProcessing },
						GetTransactionDate(entryInstruction),
						authorizationHolders);

					var auth = GetSingleAuthorizationSortedByHolders(authorizations, authorizationHolders);
					if (auth != null)
					{
						result = (auth.CPH_OH_PermitHolder, auth.CPH_Number);
					}
				}
				return result;
			}

			ZGuid[] GetAuthorisationHolders()
			{
				var collectedAuthorizationHolders = new List<ZGuid>();

				CollectHolder(collectedAuthorizationHolders, declaration.Declarant);
				if (declaration.JE_DeclarantType == RepresentationTypeList.Codes._3Indirect)
				{
					CollectHolder(collectedAuthorizationHolders, declaration.BuyingAgentAddress);
				}
				return collectedAuthorizationHolders.ToArray();
			}
		}

		void AddRuleConfigurationForAuthorizationTypeEUS()
		{
			ForAuthorizationType(Codes.EndUse)
				.When(entryInstruction => entryInstruction.InvoiceLines.Cast<JobComInvoiceLine>().Any(l => l.SupportingDocuments.Cast<SupportingDocument>().Any(d => CusAuthorizationHelper.SupportingDocumentTypesRequiringEndOfUseAuthorisation.Contains(d.CSI_Code))))
				.ReturnHolderAndNumber(entryInstruction => GetAuthorisationFromDeclarant(Codes.EndUse, ZDateTime.Today));
		}

		void AddRuleConfigurationForAuthorizationTypeCWP()
		{
			ForAuthorizationType(Codes.CustomsWarehousingCWP)
				.When(entryInstruction => entryInstruction.CEI_Style.In(new ZString[] { ImportDeclarationTypeList.Codes.AZL, ImportDeclarationTypeList.Codes.VZL, ImportDeclarationTypeList.Codes.EZL }))
				.ReturnHolderAndNumber(entryInstruction => GetAuthorisationFromDeclarant(Codes.CustomsWarehousingCWP, GetTransactionDate((CusEntryInstruction)entryInstruction)));
		}

		void AddRuleConfigurationForAuthorizationTypeCW1()
		{
			ForAuthorizationType(Codes.CustomsWarehousingCW1)
				.When(entryInstruction => entryInstruction.CEI_Style.In(new ZString[] { ImportDeclarationTypeList.Codes.AZL, ImportDeclarationTypeList.Codes.VZL, ImportDeclarationTypeList.Codes.EZL }))
				.ReturnHolderAndNumber(entryInstruction => GetAuthorisationFromDeclarant(Codes.CustomsWarehousingCW1, GetTransactionDate((CusEntryInstruction)entryInstruction)));
		}

		(ZGuid holder, ZString number) GetAuthorisationFromDeclarant(ZString cusAuthorizationType, ZDateTime transactionDate)
		{
			var result = (ZGuid.Empty, ZString.Empty);
			var authorizationHolder = declaration.DeclarantOrgAddress?.Header.PK ?? ZGuid.Empty;
			if (!authorizationHolder.IsEmpty)
			{
				var authorizations = CusAuthorisationHeader.Loader.GetAuthorisations(declaration.Factory,
					Constants.CountryCodes.Germany,
					new[] { cusAuthorizationType },
					transactionDate,
					authorizationHolder);

				if (authorizations.Length == 1)
				{
					var auth = authorizations[0];
					result = (auth.CPH_OH_PermitHolder, auth.CPH_Number);
				}
			}
			return result;
		}

		(ZGuid holder, ZString number) GetAuthorisationWithCustomsWarehousingRuleFromDeclarant(CusEntryInstruction entryInstruction, ZString cusAuthorizationType)
		{
			var result = (ZGuid.Empty, ZString.Empty);
			var authorizationHolder = declaration.DeclarantOrgAddress?.Header.PK ?? ZGuid.Empty;
			if (!authorizationHolder.IsEmpty)
			{
				var authorizations = CusAuthorisationHeader.Loader.GetAuthorisationsWithSpecificRule(
					declaration.Factory,
					new[] { cusAuthorizationType },
					new[] { authorizationHolder },
					Constants.CountryCodes.Germany,
					GetTransactionDate(entryInstruction),
					CusAuthorisationRuleTypeList.Codes.Usage,
					CusAuthorisationUsageRuleList.Codes.CustomsWarehousing,
					CusAuthorisationUsageRuleList.Codes.CustomsWarehousingType1);

				if (authorizations.Length == 1)
				{
					var auth = authorizations[0];
					result = (auth.CPH_OH_PermitHolder, auth.CPH_Number);
				}
			}
			return result;
		}

		internal (ZGuid holder, ZString number) GetLocalClearanceProcedureAuthorizationNumber(CusEntryInstruction entryInstruction, ZString cusAuthorizationType, ZString ruleValue)
		{
			var result = (ZGuid.Empty, ZString.Empty);
			var authorizationHolders = GetAuthorisationHolders();
			if (authorizationHolders.Length > 0)
			{
				var authorizations = CusAuthorisationHeader.Loader.GetAuthorisationsWithSpecificRule(declaration.Factory,
					new[] { cusAuthorizationType },
					authorizationHolders,
					Constants.CountryCodes.Germany,
					GetTransactionDate(entryInstruction),
					CusAuthorisationRuleTypeList.Codes.Usage,
					ruleValue);

				var auth = GetSingleAuthorizationSortedByHolders(authorizations, authorizationHolders);
				if (auth != null)
				{
					result = (auth.CPH_OH_PermitHolder, auth.CPH_Number);
				}
			}
			return result;

			ZGuid[] GetAuthorisationHolders()
			{
				var collectedAuthorizationHolders = new List<ZGuid>();
				var declarantType = declaration.JE_DeclarantType;

				CollectHolder(collectedAuthorizationHolders, declaration.Declarant);
				if (declarantType == RepresentationTypeList.Codes._2Direct)
				{
					CollectHolder(collectedAuthorizationHolders, declaration.Representative);
				}
				if (declarantType == RepresentationTypeList.Codes._3Indirect)
				{
					CollectHolder(collectedAuthorizationHolders, declaration.BuyingAgentAddress);
				}
				return collectedAuthorizationHolders.ToArray();
			}
		}

		CusAuthorisationHeader GetSingleAuthorizationSortedByHolders(CusAuthorisationHeader[] authorizations, ZGuid[] authorizationHolders)
		{
			CusAuthorisationHeader result = null;
			var length = authorizations.Length;
			if (length == 1)
			{
				result = authorizations[0];
			}
			else if (length > 1)
			{
				var firstAuthSortedByHolders = authorizationHolders.Join(authorizations, holder => holder, header => header.CPH_OH_PermitHolder, (holder, header) => header).First();
				var holderHasMultipleValidAuthorizations = authorizations.Any(x => x.CPH_OH_PermitHolder == firstAuthSortedByHolders.CPH_OH_PermitHolder && x.PK != firstAuthSortedByHolders.PK);
				if (!holderHasMultipleValidAuthorizations)
				{
					result = firstAuthSortedByHolders;
				}
			}
			return result;
		}

		ZDateTime GetTransactionDate(CusEntryInstruction entryInstruction)
		{
			var result = ZDateTime.Today;
			switch (entryInstruction.CEI_Style)
			{
				case ImportDeclarationTypeList.Codes.AAV:
				case ImportDeclarationTypeList.Codes.AZ:
				case ImportDeclarationTypeList.Codes.AZL:
					result = entryInstruction.CEI_LocalClearanceDate;
					break;
			}
			return result;
		}

		void CollectHolder(List<ZGuid> collectedAuthorizationHolders, OrgAddress address)
		{
			if (address != null)
			{
				collectedAuthorizationHolders.Add(address.Header.PK);
			}
		}

		readonly JobDeclaration declaration;
	}
}
