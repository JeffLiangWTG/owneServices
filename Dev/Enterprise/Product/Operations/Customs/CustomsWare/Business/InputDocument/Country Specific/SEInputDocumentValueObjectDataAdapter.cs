using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.DataTransfer.Integration;
using Enterprise.Integration;

namespace Enterprise.Customs.CustomsWare.Business
{
	public class SEInputDocumentValueObjectDataAdapter : EUInputDocumentValueObjectDataAdapter, ICustomsWareSEInputDocumentValueObjectDataAdapter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override ZString GetDeclarationType(XSD.DeclarationHeader xsdDeclarationHeader)
		{
			ZString result = "";
			if (xsdDeclarationHeader.DeclarationType.StartsWith("SE", System.StringComparison.OrdinalIgnoreCase))
			{
				ZString type = xsdDeclarationHeader.DeclarationType.SubstringSafe(2);
				switch (type)
				{
					//DNU = import(standard)
					//DRT = import(correction)
					//DBK = import(clearance request)
					//DNK = import(standard with clearance request)
					//HNU = import(Simplified)
					//HNK = import(simplified with clearance request)
					//HRT = import(correction)
					//HBK = import(simplified clearance request
					//TNU = import(supplementary)
					//TRT = import(supplementary, correction)
					//ALI = import(local clearance)
					//TQN = import(local clearance supplementary)
					case "DNU":
					case "DRT":
					case "DBK":
					case "DNK":
					case "HNU":
					case "HNK":
					case "HRT":
					case "HBK":
					case "TNU":
					case "TRT":
					case "ALI":
					case "TQN":
						result = "IMP";
						break;
					//UNU      =             export (standard)
					//URT = export(correction)
					//UGE = export(local clearance)
					case "UNU":
					case "URT":
					case "UGE":
						result = "EXP";
						break;
					//NCTSDEP = transit (departure)
					//NCTSARR = transit(arrival)
					case "NCTSDEP":
					case "NCTSARR":
						result = "NCT";
						break;
				}
			}

			if (result.IsEmpty)
			{
				result = base.GetDeclarationType(xsdDeclarationHeader);
			}

			return result;
		}

		protected override void ImportEntry(Customs.Business.BaseJobDeclaration dec, Customs.Business.CusEntryHeader entry, XSD.Declaration xsddec, IValueObjectImportContext context)
		{
			base.ImportEntry(dec, entry, xsddec, context);

			var seEntry = entry as CusEntryHeader;

			if (seEntry != null)
			{
				foreach (XSD.Reference xsdreference in xsddec.DeclarationHeader.Reference)
				{
					if (xsdreference.RefCode == "CIR")
					{
						var entryNum = CusEntryNumber.LoadOrCreate(seEntry, CusEntryNumberTypes.Standard.LocalReferenceNumber, Core.Constants.CountryCodes.Sweden);
						entryNum.CE_EntryType = seEntry.CH_MessageType;
						entryNum.CE_EntryNum = xsdreference.RefText;
						break;
					}
				}
			}
		}
	}
}
