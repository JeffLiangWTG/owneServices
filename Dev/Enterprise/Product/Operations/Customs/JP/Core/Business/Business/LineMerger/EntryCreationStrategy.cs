using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.Business
{
	public class EntryCreationStrategy : Customs.Business.EntryCreationStrategy
	{
		public EntryCreationStrategy(JobDeclaration declaration) : base(declaration)
		{
		}

		public EntryCreationStrategy(JobDeclaration declaration, ZString entryHeaderMessageTypeToNewEntryHeader) : base(declaration, entryHeaderMessageTypeToNewEntryHeader)
		{
		}

		public override MergeKey GetKeyForLine(BaseJobComInvoiceLine baseInvoiceLine)
		{
			var result = base.GetKeyForLine(baseInvoiceLine);
			if (baseInvoiceLine is JobComInvoiceLine invoiceLine)
			{
				var valuesForPreventMerging = GetValuesForPreventMerging(invoiceLine);

				if (valuesForPreventMerging.Any(c => !c.IsDefault))
				{
					result.Add(invoiceLine.PK);
				}

				var valuesForMerging = GetValuesForMerging(invoiceLine);
				valuesForMerging.ForEach(result.Add);
			}
			return result;
		}

		IEnumerable<IZType> GetValuesForMerging(JobComInvoiceLine invoiceLine)
		{
			if (invoiceLine.IsExport)
			{
				if (invoiceLine.JI_NACCSCode != ExportNACCSCodeList.Codes.X)
				{
					yield return invoiceLine.JI_NACCSCode;
				}
				if (ValueTypeList.Codes.L.Equals(invoiceLine.EntryInstruction?.CEI_ValueType))
				{
					yield return invoiceLine.JI_CustomsUnitQty;
					yield return invoiceLine.JI_CustomsSecondUnitQty;
				}
			}
			else
			{
				yield return invoiceLine.JI_DomesticConsumptionTaxExemptionIsPartial;
				yield return invoiceLine.JI_Description;
				yield return invoiceLine.JI_CountryOfOrigin;
				yield return invoiceLine.JI_PrimaryPreference;
				yield return invoiceLine.JI_SecondaryPreference;
				yield return invoiceLine.JI_FEFTAArticle48;
			}
		}

		IEnumerable<IZType> GetValuesForPreventMerging(JobComInvoiceLine invoiceLine)
		{
			if (invoiceLine.IsExport)
			{
				yield return new ZBool(invoiceLine.JI_NACCSCode == ExportNACCSCodeList.Codes.X);
				yield return invoiceLine.JI_FEFTAArticle48;
				yield return invoiceLine.JI_TradeControlOrderAppendix;
				yield return invoiceLine.JI_DutyReductionExemptionRefundCode;
				yield return invoiceLine.JI_DomesticConsumptionTaxExemptionCode;
				yield return new ZBool(invoiceLine.OtherLaws.Count != 0);
			}
			else
			{
				yield return invoiceLine.JI_AdvanceRulingOnClassification;
				yield return invoiceLine.JI_AdvanceRulingOnOrigin;
				yield return invoiceLine.JI_DomesticConsumptionTaxExemptionCode;
				yield return invoiceLine.JI_DutyReductionExemptionRefundCode;
				yield return invoiceLine.JI_NACCSCode;
				yield return invoiceLine.JI_StorageType;
				yield return invoiceLine.JI_TradeControlOrderAppendix;
				yield return invoiceLine.JI_CustomsUnitQty;
				yield return invoiceLine.JI_CustomsSecondUnitQty;
			}
		}

		protected override Customs.Business.CusEntryHeader GetExistingEntryHeader(BaseJobComInvoiceLine invoiceLine)
		{
			var existingEntryLine = GetExistingEntryLine(invoiceLine);

			if (existingEntryLine != null)
			{
				return existingEntryLine.Header;
			}
			else
			{
				return invoiceLine.Declaration.ActiveEntryHeaders.Cast<CusEntryHeader>().FirstOrDefault(x => x.CH_CEI_Instruction == invoiceLine.JI_CEI);
			}
		}
	}
}
