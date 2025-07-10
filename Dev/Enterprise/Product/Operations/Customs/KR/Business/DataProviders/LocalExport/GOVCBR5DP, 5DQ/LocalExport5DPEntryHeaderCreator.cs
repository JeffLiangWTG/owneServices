using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	public class LocalExport5DPEntryHeaderCreator : LocalExportCommonCreator
	{
		protected override void PopulateHeader(CusEntryHeader entry, LocalExportEntryHeader localExportData)
		{
			base.PopulateHeader(entry, localExportData);
			var declaration = entry.Declaration;
			localExportData.BondedAreaCode = string.Join("-", new ZString[] { declaration.JE_LocationOtherInformation, declaration.JE_SubLocationOfGoods }.Where(x => !x.IsEmpty));
			if (declaration.JE_EntryDate.IsValid)
			{
				localExportData.DeclarationDate = declaration.JE_EntryDate.ToDateTime();
			}
		}
		protected override void PopulateEntryLine(CusEntryLine entryLine, JobComInvoiceLine invoiceLine, LocalExportEntryLine entryLineData)
		{
			base.PopulateEntryLine(entryLine, invoiceLine, entryLineData);
			entryLineData.MaterialCode = invoiceLine.JI_Ingredient;
		}
	}
}
