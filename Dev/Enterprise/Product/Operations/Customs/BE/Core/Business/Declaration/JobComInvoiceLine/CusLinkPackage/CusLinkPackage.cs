using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.BE.Business.Declaration;

public class CusLinkPackage : Customs.Business.BaseCusLinkPackage
{
	public CusLinkPackage(JobComInvoiceLine invoiceLine) : base(invoiceLine)
	{
	}

	[ResourceStringData("501AB505-AFDC-4167-B0D0-8D1088F031BA", ShortCaption = "[UCC 6/11] Pack.#", Caption = "[UCC 6/11] Pack. No.", FullDescription = "[UCC 6/11] Package Number", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
	[ResourceStringData("EE8C7DDD-D8D8-46DA-9516-E0408F9F00AF", ShortCaption = "[UCC 6/11] Pack.#", Caption = "[UCC 6/11] Pack. No.", FullDescription = "[UCC 6/11] Package Number", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
	public new ZString PackageNumber => GetPackageNumberDesc();

	[ResourceStringData("F556303B-142A-4EAA-AE07-5BDFCC54CC17", ShortCaption = "[UCC 6/10] Pack.Qty.", Caption = "[UCC 6/10] Pack. Qty.", FullDescription = "[UCC 6/10] Package Quantity", MultipleKey = JobDeclaration.CaptionKeyExportUCC6)]
	[ResourceStringData("38C13CD7-F3AA-4056-A4F0-08FE6373431A", ShortCaption = "[UCC 6/10] Pack.Qty.", Caption = "[UCC 6/10] Pack. Qty.", FullDescription = "[UCC 6/10] Package Quantity", MultipleKey = JobDeclaration.CaptionKeyImportUCC6)]
	public override ZInt PackQty
	{
		get => base.PackQty;
		set => base.PackQty = value;
	}
}
