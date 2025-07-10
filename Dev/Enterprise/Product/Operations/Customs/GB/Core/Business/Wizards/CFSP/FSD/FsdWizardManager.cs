using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Business.Wizards.CFSP
{
	public interface IFsdWizardManager
	{
		void FindDeclarations();
		void CreateFsd();
		FsdWizard FsdWizard { get; }
		JobDeclaration Declaration { get; }
		bool ExecutedSuccessfully { get; }
	}

	public class FsdWizardManager : IFsdWizardManager
	{
		readonly string finsd = "FINSD";

		public FsdWizardManager(JobDeclaration declaration)
		{
			this.Declaration = declaration;
			this.FsdWizard = new FsdWizard(declaration.Factory);

			using (FsdWizard.SuspendSettingHasChanges())
			{
				ZDate lastMonth = ZDate.Today.AddMonths(-1);
				ZDate thisMonth = ZDate.Today;
				FsdWizard.DateFrom = new ZDate(lastMonth.Year, lastMonth.Month, 1);
				FsdWizard.DateTo = new ZDate(thisMonth.Year, thisMonth.Month, 1);
			}
		}

		public JobDeclaration Declaration { get; private set; }
		public FsdWizard FsdWizard { get; private set; }

		public void FindDeclarations()
		{
			sdiCusHeadersFound = new List<CusEntryHeader>();
			sdwCusHeadersFound = new List<CusEntryHeader>();
			foreach (var entry in GetImportSupplementaryDeclarations())
			{
				if (!entry.IsFSD && entry.EntryInstruction != null)
				{
					switch (entry.EntryInstruction.CEI_Style)
					{
						case ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration:
							sdiCusHeadersFound.Add(entry);
							break;
						case ImportSADDeclarationTypeList.Codes.ImportSupplementaryWarehouse:
							sdwCusHeadersFound.Add(entry);
							break;
					}
				}
			}

			FsdWizard.NumberSDIActual = FsdWizard.NumberSDIExpected = sdiCusHeadersFound.Count;
			FsdWizard.NumberSDWActual = FsdWizard.NumberSDWExpected = sdwCusHeadersFound.Count;
		}

		public CusEntryHeader[] GetImportSupplementaryDeclarations()
		{
			var entryHeaderQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
			entryHeaderQuery.AddToFilter(CusEntryHeaderSchema.CH_EntryStatus, SQLComparisonOperator.NotEqual, "CAN");
			entryHeaderQuery.IsNoLock = true;

			var ceiQ = new ZDBOnlySubQuery(typeof(EU.Business.Declaration.CusEntryInstruction), CusEntryHeaderSchema.CH_CEI_Instruction);
			ceiQ.AddToFilter(CusEntryInstructionSchema.CEI_Style, new[] { ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration, ImportSADDeclarationTypeList.Codes.ImportSupplementaryWarehouse });
			entryHeaderQuery.AddSubQuery(ceiQ, JoinCondition.And);

			var declarationSubQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
			if (!FsdWizard.Consignor.IsEmpty)
			{
				declarationSubQuery.AddToFilter(JobDeclarationSchema.JE_OH_Supplier, FsdWizard.Consignor);
			}

			var authorizationHeader = FsdWizard.AuthorizationHeader;
			if (authorizationHeader != null)
			{
				var orgRules = authorizationHeader.CusAuthorisationRules.Where(x => x.CPR_RuleCode == GBCusAuthorisationRuleTypeList.Codes.ORG).Select(x => x.CPR_ValueFrom);
				var orgRulePKs = orgRules?.Select(x => OrgHeader.LoadFromCode(FsdWizard.Factory, x).PK);
				if (orgRulePKs != null)
				{
					declarationSubQuery.AddToFilter(JobDeclarationSchema.JE_OH_Importer, orgRulePKs);
				}
			}
			else if (!FsdWizard.Consignee.IsEmpty)
			{
				declarationSubQuery.AddToFilter(JobDeclarationSchema.JE_OH_Importer, FsdWizard.Consignee);
			}

			declarationSubQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, MessageTypeList.Codes.Import);

			var entryNumSubQuery = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryHeader.Schema.TableName);
			entryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_IssueDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, FsdWizard.DateFrom);
			entryNumSubQuery.AddToFilter(CusEntryNumSchema.CE_IssueDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, FsdWizard.DateTo);
			entryNumSubQuery.IsNoLock = true;

			entryHeaderQuery.AddSubQuery(declarationSubQuery, JoinCondition.And);
			entryHeaderQuery.AddSubQuery(entryNumSubQuery, JoinCondition.And);

			var headers = Declaration.Factory.Load<CusEntryHeader>(entryHeaderQuery);
			return headers.Where(x => x.Declaration.JE_EidrType.IsEmpty || x.Declaration.JE_EidrType == FsdWizard.Procedure).ToArray();
		}

		public void CreateFsd()
		{
			Declaration.Invoices.DeleteAll();
			Declaration.InvoiceLines.RemoveAndDeleteAll();
			Declaration.JE_EntryAuthorisationDate = FsdWizard.DateFrom;
			Declaration.JE_MessageType = MessageTypeList.Codes.Import;
			Declaration.JE_OH_Importer = FsdWizard.Consignee;
			Declaration.JE_OH_Supplier = FsdWizard.Consignor;
			Declaration.JE_DeclarationType = ImportSADDeclarationTypeList.Codes.ImportSupplementaryDeclaration;
			Declaration.JE_EntryStyle = EntryStyleListImport.Codes.ImportNormal;
			Declaration.JE_EntrySubStyle = GB.Business.CodeDescriptionPairLists.EntrySubStyleListImport.Codes.SdpSupplementaryDeclarationOrSdpAndOrLcpEidrFinalSupplementaryDeclaration;  // IMY - it's FSD, baby!
			Declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			Declaration.JE_TotalNoOfPacks = 1; // as per CFSP test kit example

			var invoice = Declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_Procedure = JobComInvoiceLine.CfspFsdCPCCode;  // See tariff, vol 3, part 13, appendix E4.
			invoiceLine.Taxes.RemoveAndDeleteAll();  // no taxes needed, so remove any that may be present from the taxLineCreator
			invoice.PreviousDocuments.RemoveAndDeleteAll();

			var addInfos = invoiceLine.AdditionalInfos.AddNew();
			addInfos.CSI_Code = finsd;
			addInfos.CSI_Description = FsdWizard.AiStatement;

			SaveDetailsOfTheOtherDeclarationsThatWeFoundToProduceTheCountAsNotesOnNewDec();

			IBusiness declaration = Declaration;
			declaration.RunPreSaveValidationFetch(true);
			declaration.RunPreSaveValidation();
			ExecutedSuccessfully = true;
		}

		public bool ExecutedSuccessfully { get; set; }

		void SaveDetailsOfTheOtherDeclarationsThatWeFoundToProduceTheCountAsNotesOnNewDec()
		{
			if (sdiCusHeadersFound != null)// user pressed the FIND button
			{
				var note = Declaration.Notes.AddNew();
				StringBuilder sb = new StringBuilder();
				AppendRef(sb, sdiCusHeadersFound);
				AppendRef(sb, sdwCusHeadersFound);

				note.ST_NoteData = ZBlob.FromAscii(sb.ToString());
				note.ST_Description = "FSD wizard find results";  // The note will help audit the wizard, showing which decs comprise the total
				note.ST_IsCustomDescription = true;
			}
		}

		void AppendRef(StringBuilder sb, List<CusEntryHeader> cusEntryHeaders)
		{
			foreach (var entryHeader in cusEntryHeaders)
			{
				sb.AppendFormat(CultureInfo.InvariantCulture, "Ref={0}; DUCR={1}; Type/Style/Substyle={2}/{3}/{4}; Status={5}\r\n", entryHeader.Declaration.JE_DeclarationReference, entryHeader.Declaration.JE_UCR, entryHeader.Declaration.JE_DeclarationType, entryHeader.Declaration.JE_EntryStyle, entryHeader.Declaration.JE_EntrySubStyle, entryHeader.Declaration.JE_EntryStatus);
			}
		}

		List<CusEntryHeader> sdiCusHeadersFound;
		List<CusEntryHeader> sdwCusHeadersFound;
	}
}
