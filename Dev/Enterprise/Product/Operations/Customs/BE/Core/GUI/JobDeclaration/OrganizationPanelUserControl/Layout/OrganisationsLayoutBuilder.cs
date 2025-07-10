using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.BE.Business.Declaration;

namespace Enterprise.Customs.BE.GUI;

public class OrganisationsLayoutBuilder : EU.GUI.OrganisationsLayoutBuilder
{
	EU.GUI.OrganisationsControlBag EUBag { get; } = EU.GUI.OrganisationsControlBag.Instance;

	protected override void SetDefaultVisibilities()
	{
		base.SetDefaultVisibilities();

		SetVisibility(CommonBag.ConsigneeAddressControl, jobDec => jobDec.IsImport, jobDec => jobDec.JE_MessageTypeInfo);

		SetVisibility(EUBag.ExporterDocAddressControl, jobDec => jobDec.IsExport, jobDec => jobDec.JE_MessageTypeInfo);
		SetVisibility(EUBag.ContractualPartnerDocAddressControl, jobDec => jobDec.IsExport, jobDec => jobDec.JE_MessageTypeInfo);
		SetVisibility(EUBag.CarrierEUBorderDocAddressControl, jobDec => jobDec.IsExport, jobDec => jobDec.JE_MessageTypeInfo);

		SetCaption(CommonBag.SellerAddressControl, j => ((JobDeclaration)j).JE_OA_SellerAddressLabel, j => j.JE_MessageTypeInfo);
		SetCaption(CommonBag.RepresentativeAddressControl, j => RepresentativeCaption);
		SetCaption(CommonBag.DeclarantOfficeAddressControl, jobDeclaration => DeclarantOfficeCaption, jobDeclaration => jobDeclaration.JE_MessageTypeInfo);
		SetCaption(EUBag.CarrierEUBorderDocAddressControl, jobDeclaration => CarrierEUBorderCaption, jobDeclaration => jobDeclaration.JE_MessageTypeInfo);
		SetCaption(EUBag.ExporterDocAddressControl, jobDeclaration => ExporterCaption, jobDeclaration => jobDeclaration.JE_MessageTypeInfo);
	}

	internal static ResourceStringData RepresentativeCaption => Res.GetData("CB815A6A-08F2-41AC-8E81-13160E0669BA", "[UCC 3/19] Representative");
	internal static ResourceStringData CarrierEUBorderCaption => Res.GetData("D2CF7AC6-1BF7-4C68-8464-EC44F0C87BF3", "[UCC 3/31] Carrier EU border");
	internal static ResourceStringData DeclarantOfficeCaption => Res.GetData("839FB24D-EA8B-47C0-BA4B-3BCBE6389C90", "[UCC 3/17] Declarant");
	internal static ResourceStringData ExporterCaption => Res.GetData("796F8498-C4D1-40B7-AB83-FE14EF1A3B3D", "[UCC 3/1] Exporter");
}
