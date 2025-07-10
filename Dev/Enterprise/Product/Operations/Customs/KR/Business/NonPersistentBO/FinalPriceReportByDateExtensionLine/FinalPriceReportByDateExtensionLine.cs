using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.KR.Business
{
	public class FinalPriceReportByDateExtensionLine : AutoFinalPriceReportByDateExtensionLine
	{
		public FinalPriceReportByDateExtensionLine(FinalPriceReportByDateExtensionHeader parent)
			: base(parent.Factory)
		{
			Parent = parent;
		}
		public FinalPriceReportByDateExtensionHeader Parent { get; }

		[ResourceStringData("61B083BE-EBD6-4D9A-BED0-A127F27FBD42", Caption = "Import Entry Number")]
		public override ZString ImportDeclarationNumber { get => base.ImportDeclarationNumber; set => base.ImportDeclarationNumber = value; }

		[ResourceStringData("F9F8826A-2041-4DF3-BDEC-91C0F319428F", Caption = "Application Reason")]
		public override ZString ApplicationReason { get => base.ApplicationReason; set => base.ApplicationReason = value; }

		[ResourceStringData("D3139D07-91EF-463C-A7D5-6988F6D3B8D8", Caption = "Extension Date")]
		public override ZDateTime ExtensionDate { get => base.ExtensionDate; set => base.ExtensionDate = value; }

		public KREntryHeaderDetailsView EntryDetailsView { get; set; }

		public ZDateTime EntryReleaseDateFrom5SH { get; set; }

		protected override FinalPriceReportByDateExtensionLineValidation GetNewValidation() => new FinalPriceReportByDateExtensionLineValidation(this);
	}
}
