using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Declaration
{
	public static class CdsDeclarationTypeCodeHelper
	{
		public static ZString GetCdsDeclarationTypeCode(this CusEntryHeader entry)
		{
			return entry.Declaration.JE_EntryStyle + entry.EntryInstruction.CEI_SubStyle;
		}
	}
}
