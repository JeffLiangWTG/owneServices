using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Business.MonthlyClosing;

namespace Enterprise.Customs.DE.GUI.Testing
{
	public static class FSimplifiedDeclarationHelper
	{
		public static CusEntryHeader CreateEntryHeader(JobDeclaration jobDeclaration)
		{
			var entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();

			var entryHeaderLine = entryHeader.AllEntryLines.AddNew();
			entryHeaderLine.CL_LineNumber = 1;
			return entryHeader;
		}

		public static CusReconEntry CreateCusReconEntry(CusReconDeclaration declaration, CusEntryHeader entryHeader)
		{
			var entry = declaration.CusReconEntries.AddNew();
			entry.CRE_CH_OriginalEntry = entryHeader.PK;
			entry.CRE_EntryType = CusReconConstants.Lodged;
			entry.CRE_OA_DeclarantAddress = declaration.DeclarantAddress.PK;
			return entry;
		}

		public static CusReconEntryLine CreateCusReconEntryLine(CusReconEntry entry, ZShort lineNumber, ZShort originalLineNumber)
		{
			var line = entry.CusReconEntryLines.AddNew();
			line.CRL_LineNumber = lineNumber;
			line.CRL_OriginalEntryLineNumber = originalLineNumber;
			return line;
		}
	}
}

