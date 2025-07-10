using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.NL.Business.Declaration;

public class CusLinkPackage : Customs.Business.BaseCusLinkPackage
{
	public CusLinkPackage(JobComInvoiceLine invoiceLine) : base(invoiceLine)
	{
	}

	[ResourceStringData("F3901851-C068-4166-811B-3A9D235EB6D9", ShortCaption = "[UCC 6/11] Pack.#", Caption = "[UCC 6/11] Pack. No.", FullDescription = "[UCC 6/11] Package Number", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
	[ResourceStringData("CE6295B6-0A1D-4DAA-92D2-8509F6CA711F", ShortCaption = "[UCC 6/11] Pack.#", Caption = "[UCC 6/11] Pack. No.", FullDescription = "[UCC 6/11] Package Number", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
	public new ZString PackageNumber => GetPackageNumberDesc();

	[ResourceStringData("5BE2B411-75C4-4780-AF03-FA40AB672A15", ShortCaption = "[UCC 6/10] Pack.Qty.", Caption = "[UCC 6/10] Pack. Qty.", FullDescription = "[UCC 6/10] Package Quantity", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
	[ResourceStringData("A10D04AA-4228-4227-A4C9-3272913D3E5A", ShortCaption = "[UCC 6/10] Pack.Qty.", Caption = "[UCC 6/10] Pack. Qty.", FullDescription = "[UCC 6/10] Package Quantity", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
	public override ZInt PackQty
	{
		get => base.PackQty;
		set => base.PackQty = value;
	}
}
