using CargoWise.EntityFramework;
using CargoWise.Types;
using ESCusEntryLine = Enterprise.Customs.ES.Business.Declaration.CusEntryLine;

namespace Enterprise.Customs.ES.DocumentWrappers.SADH
{
	public class ESDocSADHLineExport : ESDocSADHLine
	{
		public static ESDocSADHLineExport New(ESCusEntryLine entryLine, BusinessObjectFactory factory)
			=> entryLine == null ? null : new ESDocSADHLineExport(entryLine, factory);

		protected ESDocSADHLineExport(ESCusEntryLine entryLine, BusinessObjectFactory factory)
			: base(entryLine, factory)
		{
		}

		public new ZDecimal Box41SupplementaryQty => EntryLine.SupplementaryQuantity;

		public new ZDecimal Box42ItemPrice => EntryLine.TotalLinePrice.Amount;

		protected override int Box44MaxLength => EntryLine.CL_LineNumber == 1 ? EntryLine.ExportBox44MaxLength : EntryLine.ExportBox44BISPageMaxLength;

		protected override ZString SupplementaryUnitsDecimalFormatSpain => "N6";
	}
}
