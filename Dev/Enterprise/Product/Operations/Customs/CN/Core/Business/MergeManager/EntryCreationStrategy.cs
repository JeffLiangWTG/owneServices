using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CN.Business
{
	public abstract class EntryCreationStrategy : Customs.Business.EntryCreationStrategy
	{
		protected EntryCreationStrategy(JobDeclaration declaration, ZString entryHeaderMessageTypeToNewEntryHeader, bool useAdditionalEntryLineLinks)
			: base(declaration, entryHeaderMessageTypeToNewEntryHeader)
		{
			UseAdditionalEntryLineLinks = useAdditionalEntryLineLinks;
		}
		protected readonly bool UseAdditionalEntryLineLinks;

		protected new JobDeclaration Declaration => base.Declaration as JobDeclaration;

		#region Overrides

		protected override IEnumerable<Customs.Business.CusEntryHeader> GetExistingEntriesCreatedThroughThisStrategy()
		{
			return Declaration.ActiveEntryHeaders.Cast<Customs.Business.CusEntryHeader>().Where(entry => entry.CH_MessageType == CH_MessageTypeToNewEntryHeader);
		}

		protected override Customs.Business.CusEntryLine GetExistingEntryLine(BaseJobComInvoiceLine invoiceLine)
		{
			return UseAdditionalEntryLineLinks
				? invoiceLine.AdditionalEntryLineLinks
					.Select(link => link.EntryLine)
					.FirstOrDefault(line => line.Header != null && line.Header.CH_MessageType == CH_MessageTypeToNewEntryHeader)
				: base.GetExistingEntryLine(invoiceLine);
		}

		protected override bool IsEntryHeaderValidToBeReused(Customs.Business.CusEntryHeader entry, BaseJobComInvoiceLine invoiceLine)
		{
			return base.IsEntryHeaderValidToBeReused(entry, invoiceLine) && entry.CH_MessageType == CH_MessageTypeToNewEntryHeader && (!entry.HasBeenLodgedAtCustoms || entry.CH_CEI_Instruction == invoiceLine.JI_CEI);
		}

		protected override AdditionalInvoiceLineEntryLineLink LinkInvoiceLineEntryLineAndReturnPivotIfUsed(Customs.Business.CusEntryLine entryLine, BaseJobComInvoiceLine baseInvoiceLine)
		{
			return UseAdditionalEntryLineLinks
				? baseInvoiceLine.AdditionalEntryLineLinks.AddLinkIfNoneExists(entryLine)
				: base.LinkInvoiceLineEntryLineAndReturnPivotIfUsed(entryLine, baseInvoiceLine);
		}

		protected override void ClearReferenceToEntryLineWhenLineIsNotValidForMerge(BaseJobComInvoiceLine baseInvoiceLine)
		{
			if (UseAdditionalEntryLineLinks)
			{
				foreach (var entryLine in baseInvoiceLine.AdditionalEntryLineLinks.GetEntryLineFor(CH_MessageTypeToNewEntryHeader))
				{
					baseInvoiceLine.AdditionalEntryLineLinks.DeleteLinkIfExistsFor(entryLine);
				}
			}
			else
			{
				base.ClearReferenceToEntryLineWhenLineIsNotValidForMerge(baseInvoiceLine);
			}
		}

		public override bool LineIsValidForMerge(BaseJobComInvoiceLine invoiceLine)
		{
			var instruction = GetEntryInstruction(invoiceLine as JobComInvoiceLine);
			return instruction != null && !instruction.IsDeleted;
		}

		protected override void AfterCreateOrGetEntryHeader(Customs.Business.CusEntryHeader entryHeader, BaseJobComInvoiceLine baseInvoiceLine)
		{
			base.AfterCreateOrGetEntryHeader(entryHeader, baseInvoiceLine);
			entryHeader.CH_CEI_Instruction = GetEntryInstruction(baseInvoiceLine as JobComInvoiceLine)?.PK ?? ZGuid.Empty;
		}

		#endregion

		#region MergeKey

		protected override MergeKey GetKeyForHeaderCore(BaseJobComInvoiceLine invoiceLine)
		{
			var result = base.GetKeyForHeaderCore(invoiceLine);
			result.Add(GetEntryInstruction(invoiceLine as JobComInvoiceLine)?.PK ?? ZGuid.Empty);
			return result;
		}

		protected abstract CusEntryInstruction GetEntryInstruction(JobComInvoiceLine invoiceLine);

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var result = base.GetKeyForLine(baseInvoiceLine);

			if (Declaration.JE_MergeBy != OrgConstants.MergeInvoiceLines.NotMerge)
			{
				var invoiceLine = (JobComInvoiceLine)baseInvoiceLine;

				var instruction = invoiceLine.EntryInstruction;
				var willGenerateEnteringEntry = instruction != null && (instruction.WillGenerateEnteringEntry || (instruction.ChildInstruction?.WillGenerateEnteringEntry ?? false));
				var willGenerateExitingEntry = instruction != null && (instruction.WillGenerateExitingEntry || (instruction.ChildInstruction?.WillGenerateExitingEntry ?? false));
				var hasChildInstruction = invoiceLine.ChildInstruction != null;
				result.Add((instruction?.CEI_ManualNo ?? ZString.Empty).IsEmpty ? ZInt.Zero : invoiceLine.JI_ProductManualNo);
				result.Add((instruction?.ChildInstruction?.CEI_ManualNo ?? ZString.Empty).IsEmpty ? ZInt.Zero : invoiceLine.JI_ProductManualNo2);

				result.Add(invoiceLine.JI_TradeUnitQty);
				result.Add(invoiceLine.JI_CustomsUnitQty);
				result.Add(invoiceLine.JI_CustomsSecondUnitQty);
				result.Add(invoiceLine.JI_RX_NKLinePriceCurr);
				result.Add(invoiceLine.JI_DutyMode);
				var ciqRequires = invoiceLine.CIQRequires;
				result.Add(ReturnValueOrEmpty(ciqRequires, invoiceLine.CIQProductQualifications.MergeKey));

				result.Add(baseInvoiceLine.JI_CountryOfOrigin);
				result.Add(baseInvoiceLine.JI_RN_NKCountryOfExport);
				result.Add(invoiceLine.JI_NameOfGoods);
				result.Add(invoiceLine.FormulaPricingRecordNumber);
				result.Add(invoiceLine.AdditionalInformationHelper.GetMergeKeyFromGoodsSpecModel());
				result.Add(ReturnValueOrEmpty(hasChildInstruction, invoiceLine.JI_NameOfGoods2));
				result.Add(ReturnValueOrEmpty(hasChildInstruction, invoiceLine.AdditionalInformation2Helper.GetMergeKeyFromGoodsSpecModel()));

				result.Add(ReturnValueOrEmpty(willGenerateEnteringEntry, invoiceLine.JI_CIQOriginState));

				result.Add(ReturnValueOrEmpty(willGenerateExitingEntry, invoiceLine.JI_OriginDistrict));
				result.Add(ReturnValueOrEmpty(willGenerateExitingEntry, invoiceLine.JI_OriginRegion));

				result.Add(ReturnValueOrEmpty(willGenerateEnteringEntry, invoiceLine.JI_DestinationDistrict));
				result.Add(ReturnValueOrEmpty(willGenerateEnteringEntry, invoiceLine.JI_DestinationRegion));

				var declarationImport = Declaration.IsImport;
				result.Add(ReturnValueOrEmpty(declarationImport, invoiceLine.JI_PrimaryPreference));
				result.Add(ReturnValueOrEmpty(declarationImport, invoiceLine.JI_SecondaryPreference));

				var cetificateOfOriginApplicable = invoiceLine.IsCertificateOfOriginApplicable;
				result.Add(ReturnValueOrEmpty(cetificateOfOriginApplicable, invoiceLine.CertificateOfOrigin));
				result.Add(ReturnValueOrEmpty(cetificateOfOriginApplicable, invoiceLine.ItemNoOnCertOfOrigin));
				result.Add(ReturnValueOrEmpty(cetificateOfOriginApplicable, invoiceLine.CertificateOfOriginCountry));
				result.Add(ReturnValueOrEmpty(cetificateOfOriginApplicable, invoiceLine.CertificateOfOriginType));

				result.Add(ReturnValueOrEmpty(ciqRequires, invoiceLine.JI_CIQTariff));
				result.Add(ReturnValueOrEmpty(ciqRequires, invoiceLine.JI_CIQEndUse));
				result.Add(ReturnValueOrEmpty(ciqRequires, invoiceLine.CargoAttributesAsString));
				result.Add(ReturnValueOrEmpty(ciqRequires, invoiceLine.CIQIngredient));
				result.Add(ReturnValueOrEmpty(ciqRequires, invoiceLine.JI_CIQQualityGuaranteePeriod));
				result.Add(ReturnValueOrEmpty(ciqRequires, invoiceLine.ManufacturerOrgPK));

				var undgName = invoiceLine.DangerousGoods?.UNDGSubstance?.DG_Code ?? ZString.Empty;
				result.Add(ReturnValueOrEmpty(ciqRequires, undgName));
				result.Add(ReturnValueOrEmpty(ciqRequires && !undgName.IsEmpty, invoiceLine.JI_NonDangerousChemicalFlag));
				result.Add(ReturnValueOrEmpty(ciqRequires && !undgName.IsEmpty, invoiceLine.JI_PackageTypeOfUNDG));

				result.Add(ReturnValueOrEmpty(ciqRequires, invoiceLine.JI_NDescription));
				result.Add(ReturnValueOrEmpty(ciqRequires, invoiceLine.JI_BrandName));
				result.Add(ReturnValueOrEmpty(ciqRequires, invoiceLine.JI_Model));

				AddMergeKeyFromSelectedRules(invoiceLine, result);
			}

			return result;
		}

		IZType ReturnValueOrEmpty(bool shouldReturnValue, IZType value)
		{
			return shouldReturnValue ? value : value.Default;
		}

		void AddMergeKeyFromSelectedRules(BaseJobComInvoiceLine invoiceLine, MergeKey key)
		{
			foreach (MergingRule mergingRule in Declaration.MergingRules)
			{
				var mergeRule = MergingRulesProvider.GetMergeRule(mergingRule.CY_Code);
				if (mergeRule != null)
				{
					mergeRule.GetKeysForLine((JobComInvoiceLine)invoiceLine).ForEach(x => key.Add(x));
				}
			}
		}

		#endregion
	}
}
