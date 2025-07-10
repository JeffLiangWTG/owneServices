using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class NFeEntryExportObject : NonPersistentBusinessObject
	{
		public NFeEntryExportObject(CusEntryHeader entryHeader)
			: base(entryHeader.Factory)
		{
			EntryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		}
		public readonly CusEntryHeader EntryHeader;

		public static NFeEntryExportObject New(CusEntryHeader entryHeader) => entryHeader == null ? null : new NFeEntryExportObject(entryHeader);

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeEntryExportObject|ReferenceNumber", Caption = "Ref No.")]
		public ZString ReferenceNumber => EntryHeader.CH_BGMReference;

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeEntryExportObject|EntryNumber", Caption = "Entry Number")]
		public ZString EntryNumber => EntryHeader.MovementReferenceNumber;

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeEntryExportObject|EntryNumberIssueDate", Caption = "Issue Date")]
		public ZDateTime EntryNumberIssueDate => EntryHeader.MovementReferenceNumberIssueDate;

		[ResourceStringData("NPBO:Enterprise.Customs.BR.Business.NFeEntryExportObject|ReleaseDate", Caption = "Release Date")]
		public ZDateTime ReleaseDate => EntryHeader.CH_EntryReleaseDate;

		#region NFeInvoiceLineExportObjectCollection

		[ChildEditable(true)]
		public NFeInvoiceLineExportObjectCollection Lines
		{
			get
			{
				if (fNFeInvoiceLines == null)
				{
					fNFeInvoiceLines = new NFeInvoiceLineExportObjectCollection(EntryHeader);
					RegisterEditableChildObject(fNFeInvoiceLines);
					fNFeInvoiceLines.Load();
				}
				return fNFeInvoiceLines;
			}
		}

		NFeInvoiceLineExportObjectCollection fNFeInvoiceLines;

		#endregion

		#region Total Value Properties

		public ZDecimal TotalFOBValue => Lines.Cast<NFeInvoiceLineExportObject>().Sum(x => x.FOBValue);

		public ZDecimal TotalFreightValue => Lines.Cast<NFeInvoiceLineExportObject>().Sum(x => x.FreightValue);

		public ZDecimal TotalInsuranceValue => Lines.Cast<NFeInvoiceLineExportObject>().Sum(x => x.InsuranceValue);

		public ZDecimal TotalCIFValue => Lines.Cast<NFeInvoiceLineExportObject>().Sum(x => x.CIFValue);

		public ZDecimal TotalGrossWeight => Lines.Cast<NFeInvoiceLineExportObject>().Sum(x => x.GrossWeight);

		public ZDecimal TotalNetWeight => Lines.Cast<NFeInvoiceLineExportObject>().Sum(x => x.NetWeight);

		#endregion
	}
}
