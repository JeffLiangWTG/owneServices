using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.CDS
{
	public enum CdsFsdCreatedResult { Unknown, Warning, Success }

	public interface ICDSFinalSupplementaryDeclarationHelperManager
	{
		void CreateCDSFsd();
		CDSFinalSupplementaryDeclarationHelper CDSFsdWizard { get; }
		JobDeclaration Declaration { get; }
		CdsFsdCreatedResult ExecutionResult { get; }
		void CountTimelyEntries();
	}

	public class CDSFinalSupplementaryDeclarationHelperManager : ICDSFinalSupplementaryDeclarationHelperManager
	{
		public CDSFinalSupplementaryDeclarationHelperManager(JobDeclaration declaration)
		{
			Argument.NotNull(declaration, "declaration");

			Declaration = declaration;
			CDSFsdWizard = new CDSFinalSupplementaryDeclarationHelper(declaration);
			ExecutionResult = CdsFsdCreatedResult.Unknown;
		}

		public void CreateCDSFsd()
		{
			Declaration.Invoices.DeleteAll();
			Declaration.InvoiceLines.RemoveAndDeleteAll();
			Declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			Declaration.PreviousDocuments.RemoveAndDeleteAll();

			Declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
			Declaration.JE_CustomsProfile = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			Declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			Declaration.JE_OA_DeclarantAddress = CDSFsdWizard.DeclarantAddressPK;

			var cusEntryInstruction = Declaration.CusEntryInstruction;
			cusEntryInstruction.CEI_SubStyle = EntrySubStyleCodeList.Codes.Q;
			cusEntryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.FinalSupplementaryDeclaration;

			var authorisationType = CDSFsdWizard.AuthorisationType;
			ZAddress authorisationAddress = new ZAddress(CDSFsdWizard.AuthorisationHolderAddressPKInfo);
			ZString authorisationNumber = DefaultAuthorisationNumber;

			var authorisationList = CusAuthorisationHeader.Loader.GetAuthorisations(Declaration.Factory, Declaration.CountryCode, new[] { authorisationType }, ZDateTime.Today, new[] { authorisationAddress.OrgHeader.PK });
			if (authorisationList.Length == 1)
			{
				authorisationNumber = authorisationList[0].CPH_Number;
			}
			var authorisation1 = cusEntryInstruction.CusAuthorizationUsages.AddNew();
			authorisation1.AGC_Code = authorisationType;
			authorisation1.AGC_Number = authorisationNumber;
			authorisation1.AGC_OH_Owner = authorisationAddress.OrgHeader.PK;

			var invoiceHeader = Declaration.Invoices.AddNew();
			ZDateTime dueDate = CDSFsdWizard.DueDate;
			invoiceHeader.JZ_InvoiceNumber = "FSD FOR " + dueDate.ToString("yyyy-MM-dd");

			var invoiceLine = Declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Procedure = "009097F";
			var additionalInfoY = invoiceLine.AdditionalInfos.AddNew();
			additionalInfoY.CSI_Code = "FINSY";
			additionalInfoY.CSI_Description = "SDY=" + CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted.ToString() + "/" + CDSFsdWizard.NumberOfTypeYDeclarationsDue.ToString();

			var additionalInfoZ = invoiceLine.AdditionalInfos.AddNew();
			additionalInfoZ.CSI_Code = "FINSZ";
			additionalInfoZ.CSI_Description = "SDZ=" + CDSFsdWizard.NumberOfTypeZDeclarationsSubmitted.ToString() + "/" + CDSFsdWizard.NumberOfTypeZDeclarationsDue.ToString();

			var additionalInfoL = invoiceLine.AdditionalInfos.AddNew();
			additionalInfoL.CSI_Code = "FINSL";
			var finslDescription = "";
			var allChildren = new List<CDSFinalSupplementaryDeclarationHelperLateChild>();
			foreach (CDSFinalSupplementaryDeclarationHelperLateChild lateChild in CDSFsdWizard.CDSFinalSupplementaryDeclarationHelperLateChildCollection)
			{
				allChildren.Add(lateChild);
			}

			if (allChildren.Count > 0)
			{
				var queryLateChildGroup = from lateChild in allChildren
										  group lateChild by new
										  {
											  Year = lateChild.DueDate.ToString("yy"),
											  Month = lateChild.DueDate.ToString("MM"),
										  } into lateChildGroup
										  select lateChildGroup;

				foreach (var lc in queryLateChildGroup)
				{
					var year = lc.Key.Year;
					var month = lc.Key.Month;
					ZInt y = 0;
					ZInt z = 0;
					foreach (CDSFinalSupplementaryDeclarationHelperLateChild item in lc)
					{
						y += item.NumberOfTypeYDeclarations;
						z += item.NumberOfTypeZDeclarations;
					}
					finslDescription += month + "/" + year + "=Y " + y + " " + month + "/" + year + "=Z " + z + " ";
				}
				finslDescription.TrimEnd();
			}
			else
			{
				finslDescription = "0/0";
			}
			additionalInfoL.CSI_Description = finslDescription;

			var previousDocument = Declaration.PreviousDocuments.AddNew();
			previousDocument.CSI_ReferenceNumber = "FSD";
			previousDocument.CSI_Code = "ZZZ";
			previousDocument.CSI_SubType = "Z";
			Declaration.JE_EntryAuthorisationDate = CDSFsdWizard.DueDate;
			Declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			ExecutionResult = authorisationNumber == DefaultAuthorisationNumber ? CdsFsdCreatedResult.Warning : CdsFsdCreatedResult.Success;
		}

		public CDSFinalSupplementaryDeclarationHelper CDSFsdWizard { get; private set; }
		public JobDeclaration Declaration { get; private set; }

		public CdsFsdCreatedResult ExecutionResult { get; set; }

		public const string DefaultAuthorisationNumber = "Insert FSD authorisation no. here";

		public void CountTimelyEntries()
		{
			if (CDSFsdWizard.StartOfPeriod.IsValid && CDSFsdWizard.DueDate.IsValid)
			{
				var entryHeaderQueryY = GetBaseEntryHeaderQuery();
				var entryHeaderQueryZ = entryHeaderQueryY.DeepClone() as ZDBOnlyQuery;

				var ceiYSubQuery = new ZDBOnlySubQuery(typeof(Customs.Business.CusEntryInstruction), CusEntryInstructionSchema.PK);
				ceiYSubQuery.AddToFilter(CusEntryInstructionSchema.CEI_SubStyle, GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSupplementaryDeclarationOrSdpAndOrLcpEidrFinalSupplementaryDeclaration);
				entryHeaderQueryY.AddSubQuery(CusEntryHeaderSchema.CH_CEI_Instruction, CusEntryInstructionSchema.PK, ceiYSubQuery, JoinCondition.And);

				var ceiZSubQuery = new ZDBOnlySubQuery(typeof(Customs.Business.CusEntryInstruction), CusEntryInstructionSchema.PK);
				ceiZSubQuery.AddToFilter(CusEntryInstructionSchema.CEI_SubStyle, GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.LcpEidrSupplementaryDeclarationImportOrWarehouseRemovalSdiSdw);
				entryHeaderQueryZ.AddSubQuery(CusEntryHeaderSchema.CH_CEI_Instruction, CusEntryInstructionSchema.PK, ceiZSubQuery, JoinCondition.And);

				CDSFsdWizard.NumberOfTypeYDeclarationsSubmitted = Declaration.Factory.GetDatabaseCount(typeof(EU.Business.Declaration.CusEntryHeader), entryHeaderQueryY);
				CDSFsdWizard.NumberOfTypeZDeclarationsSubmitted = Declaration.Factory.GetDatabaseCount(typeof(EU.Business.Declaration.CusEntryHeader), entryHeaderQueryZ);
			}
		}

		ZDBOnlyQuery GetBaseEntryHeaderQuery()
		{
			var entryHeaderQuery = new ZDBOnlyQuery(typeof(EU.Business.Declaration.CusEntryHeader));
			var declarationSubQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);

			entryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_EntryStatus, SQLComparisonOperator.NotEqual, "CAN");
			entryHeaderQuery.IsNoLock = true;

			declarationSubQuery.AddToFilter(JobDeclarationSchema.JE_ApplicationCode, Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services);

			var companySubQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbCompanySchema.PK);
			companySubQuery.AddToFilter(GlbCompanySchema.GC_RN_NKCountryCode, SQLComparisonOperator.Equal, Core.Constants.CountryCodes.UnitedKingdom);
			companySubQuery.AddToFilter(GlbCompanySchema.PK, GlbCompany.CurrentCompany.PK);
			declarationSubQuery.AddSubQuery(JobDeclarationSchema.JE_GC, companySubQuery, JoinCondition.And);

			var combinedGenAddOnQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			var helper = new ModelViewColumnQueryHelper<JobDeclaration>();

			var genAddOnSubQuery = helper.GetModuleFilterQuery(JobDeclaration.Schema.PK, JobDeclaration.Schema.PK, GBJobDeclarationSchema.Constants.TableName, GBJobDeclarationSchema.Constants.JE_EidrType, SQLComparisonOperator.Equal, EidrTypeList.Codes.CFS);
			var genAddOnSubQueryWhereNoGenAddOnMeaningBlankEidr = helper.GetModuleFilterQuery(JobDeclaration.Schema.PK, JobDeclaration.Schema.PK, GBJobDeclarationSchema.Constants.TableName, GBJobDeclarationSchema.Constants.JE_EidrType, SQLComparisonOperator.Equal, ZString.Empty);
		
			combinedGenAddOnQuery.AddToFilter(genAddOnSubQuery, JoinCondition.And);
			combinedGenAddOnQuery.AddToFilter(genAddOnSubQueryWhereNoGenAddOnMeaningBlankEidr, JoinCondition.Or);
			declarationSubQuery.AddToFilter(combinedGenAddOnQuery, JoinCondition.And);

			if (CDSFsdWizard.FindOnlyForDueDate)
			{
				var dueDateGenAddQuery = helper.GetDateFilterQuery(JobDeclaration.Schema.PK, JobDeclaration.Schema.PK, GBJobDeclarationSchema.Constants.TableName, GBJobDeclarationSchema.Constants.JE_SuppDecDueDate, DateComparisonOperator.HasDateInRange, CDSFsdWizard.DueDate.Date, CDSFsdWizard.DueDate.Date);
				declarationSubQuery.AddToFilter(dueDateGenAddQuery, JoinCondition.And);
			}

			var combinedImporterQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			var importerHeader = CDSFsdWizard.ImporterHeader;
			if (importerHeader != null)
			{
				combinedImporterQuery.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_OH_Importer, importerHeader.PK);
			}

			var authorizationHeader = CDSFsdWizard.AuthorisationHolderAddress?.Header;
			if (CDSFsdWizard.FindForImporterLinkedOrg && authorizationHeader != null && !CDSFsdWizard.AuthorisationType.IsEmpty)
			{
				var permitHeader = Customs.Business.CusAuthorisationHeader.Loader.GetAuthorisation(
						Declaration.Factory,
						authorizationHeader.CountryCode,
						CDSFsdWizard.AuthorisationType,
						CDSFsdWizard.DueDate,
						authorizationHeader.PK);
				var orgRules = permitHeader?.CusAuthorisationRules.Where(x => x.CPR_RuleCode == GBCusAuthorisationRuleTypeList.Codes.ORG).Select(x => x.CPR_ValueFrom);
				var orgRulePKs = orgRules?.Select(x => MasterFiles.Business.OrgHeader.LoadFromCode(CDSFsdWizard.Factory, x).PK);
				if (orgRulePKs != null)
				{
					combinedImporterQuery.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_OH_Importer, orgRulePKs);
				}
				else
				{
					combinedImporterQuery.AddToFilter(JoinCondition.Or, JobDeclarationSchema.JE_OH_Importer, authorizationHeader.PK);
				}
			}
			declarationSubQuery.AddToFilter(combinedImporterQuery, JoinCondition.And);

			if (!CDSFsdWizard.DeclarantAddressPK.IsEmpty)
			{
				declarationSubQuery.AddToFilter(JoinCondition.And, JobDeclarationSchema.JE_OA_DeclarantAddress, CDSFsdWizard.DeclarantAddressPK);
			}

			if (CDSFsdWizard.FindOnlyForAuthType || CDSFsdWizard.FindOnlyForAuthHolder)
			{
				var authUsageSubQuery = new ZDBOnlySubQuery(typeof(CusAuthorizationUsage), CusAuthorizationUsageSchema.AGC_ParentID);
				authUsageSubQuery.AddToFilter(CusAuthorizationUsageSchema.AGC_ParentTableCode, CusEntryInstructionSchema.Constants.Prefix);

				var filter = new ZQuery();
				if (CDSFsdWizard.FindOnlyForAuthHolder && !CDSFsdWizard.AuthorisationHolderAddressPK.IsEmpty && authorizationHeader != null)
				{
					filter.AddToFilter(CusAuthorizationUsageSchema.AGC_OH_Owner, authorizationHeader.PK);
				}
				if (CDSFsdWizard.FindOnlyForAuthType && !CDSFsdWizard.AuthorisationType.IsEmpty)
				{
					filter.AddToFilter(JoinCondition.Or, CusAuthorizationUsageSchema.AGC_Code, CDSFsdWizard.AuthorisationType);
				}

				if (!filter.IsEmpty)
				{
					authUsageSubQuery.AddToFilter(filter, JoinCondition.And);
					entryHeaderQuery.AddSubQuery(CusEntryHeaderSchema.CH_CEI_Instruction, authUsageSubQuery, JoinCondition.And);
				}
			}

			var entryNumSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, EU.Business.Declaration.CusEntryHeader.Schema.TableName);
			entryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_IssueDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, CDSFsdWizard.StartOfPeriod);
			entryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_IssueDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, CDSFsdWizard.DueDate);
			entryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryNumberTypes.Standard.MovementReferenceNumber);
			entryNumSubQuery.IsNoLock = true;

			entryHeaderQuery.AddSubQuery(declarationSubQuery, JoinCondition.And);
			entryHeaderQuery.AddSubQuery(entryNumSubQuery, JoinCondition.And);
			return entryHeaderQuery;
		}
	}
}
