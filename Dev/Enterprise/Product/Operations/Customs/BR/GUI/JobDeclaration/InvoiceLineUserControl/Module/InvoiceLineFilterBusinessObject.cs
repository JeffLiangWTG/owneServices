using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.BR.GUI
{
	public class InvoiceLineFilterBusinessObject : Customs.GUI.InvoiceLineFilterBusinessObject
	{
		public InvoiceLineFilterBusinessObject(Func<IInvoicesProvider> getInvoicesProvider, Func<ZString, ZBool> isColumnAvailable)
			: base(getInvoicesProvider, isColumnAvailable)
		{
			this.getInvoicesProvider = getInvoicesProvider;
		}

		JobDeclaration Declaration => InvoicesProvider as JobDeclaration;

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
			=> new InvoiceLineFilterBusinessObject(getInvoicesProvider, IsColumnAvailable);

		readonly Func<IInvoicesProvider> getInvoicesProvider;

		protected new IInvoicesProvider InvoicesProvider => base.InvoicesProvider;

		protected override void AddOrUpdateFunc(ModuleFilterCollection filters)
		{
			base.AddOrUpdateFunc(filters);

			if (Declaration != null)
			{
				if (Declaration.IsExport)
				{
					AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.Cpc, invoiceLine => invoiceLine.JI_Procedure, ResString.GetMultilingualString("eb412ad3-b5bf-4a3c-9a86-66b2b9e30c15", InvoiceLineFilterConstants.Cpc));
					AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.NfeKey, invoiceLine => ((JobComInvoiceLine)invoiceLine).JI_NFeNumber, ResString.GetMultilingualString("d2e1f7f4-a708-4fa1-939f-13336007a668", InvoiceLineFilterConstants.NfeKey));
					AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.NfeItemNum, invoiceLine => ((JobComInvoiceLine)invoiceLine).JI_NFeItemNumber, ResString.GetMultilingualString("45a3d1e3-ed93-4cf6-986f-db049509c1d5", InvoiceLineFilterConstants.NfeItemNum));
				}

				if (Declaration.IsImport)
				{
					AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.ManufacturerIndicator, invoiceLine => ((JobComInvoiceLine)invoiceLine).JI_ManufacturerIndicator, ResString.GetMultilingualString("5C30B2B1-C3D4-417A-9D53-118EABDCFCE7", InvoiceLineFilterConstants.ManufacturerIndicator));
				}

				if (Declaration.IsImportOnly)
				{
					AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.PermitNumber, invoiceLine => ((JobComInvoiceLine)invoiceLine).Permits.Select(x => x.CSI_ReferenceNumber), ResString.GetMultilingualString("FD61D737-6C4D-4F0E-AA65-38091C952EA8", "Permit"));
					AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.AuthorityIdentifier, invoiceLine => ((JobComInvoiceLine)invoiceLine).JI_CatalogAuthorityIdentifier, ResString.GetMultilingualString("8DFE9EC0-77B2-4440-AFB7-DA00558183B5", InvoiceLineFilterConstants.AuthorityIdentifier));
					AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.AuthorityVersion, invoiceLine => ((JobComInvoiceLine)invoiceLine).JI_CatalogAuthorityVersion, ResString.GetMultilingualString("895F247C-E2B9-4A12-98DE-69DD73E01A6A", InvoiceLineFilterConstants.AuthorityVersion));
					AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.ManufacturerVersion, invoiceLine => ((JobComInvoiceLine)invoiceLine).JI_ManufacturerAuthorityVersion.ToString(), ResString.GetMultilingualString("6FE14558-A57D-4499-8279-4B539EF8E1FD", InvoiceLineFilterConstants.ManufacturerVersion));
					AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.GoodsCatalog, invoiceLine => ((JobComInvoiceLine)invoiceLine).GoodsCatalog?.CGC_CatalogCode ?? ZString.Empty, ResString.GetMultilingualString("442BCCE5-F60E-43B6-9D9D-E5C019AA6BB5", InvoiceLineFilterConstants.GoodsCatalog));
					AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.ComplementaryDescription, invoiceLine => ((JobComInvoiceLine)invoiceLine).ComplementaryDescription.ToString(), ResString.GetMultilingualString("86B82421-BA4B-43C9-9A19-98B422CCDD2D", InvoiceLineFilterConstants.ComplementaryDescription));
					AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.GoodsApplication, invoiceLine => ((JobComInvoiceLine)invoiceLine).JI_GoodsApplication.ToString(), ResString.GetMultilingualString("302D5CBB-12D0-4C93-AB69-AD45B150F6CF", InvoiceLineFilterConstants.GoodsApplication));
					AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.GoodsCondition, invoiceLine => ((JobComInvoiceLine)invoiceLine).JI_GoodsCondition.ToString(), ResString.GetMultilingualString("AF7FDB1B-15BD-484E-B6F6-EF1747859F35", InvoiceLineFilterConstants.GoodsCondition));
				}

				if (Declaration.IsImportSiscomex)
				{
					AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.ImportLicenseNo, invoiceLine => ((JobComInvoiceLine)invoiceLine).ImportLicenseNumber, ResString.GetMultilingualString("ED4ACDD2-BD4D-429E-AEAC-88A7F1693870", InvoiceLineFilterConstants.ImportLicenseNo));
				}

				if (Declaration.IsImportLicense)
				{
					AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.DrawbackModality, invoiceLine => ((JobComInvoiceLine)invoiceLine).DrawbackModality, ResString.GetMultilingualString("53841788-1E7F-446A-AAAD-39B080FE2813", InvoiceLineFilterConstants.DrawbackModality));
					AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.DrawbackCANumber, invoiceLine => ((JobComInvoiceLine)invoiceLine).DrawbackCANumber, ResString.GetMultilingualString("64D82CAB-B5B9-4B7D-B6E9-DBE7143398C8", InvoiceLineFilterConstants.DrawbackCANumber));
					AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.TariffDetach, invoiceLine => ((JobComInvoiceLine)invoiceLine).TariffDetachs.Select(x => x.CY_Code), ResString.GetMultilingualString("A38D7858-4325-4F68-AB98-4D3FC222B2EB", InvoiceLineFilterConstants.TariffDetach));
					AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.Agreement, invoiceLine => ((JobComInvoiceLine)invoiceLine).JI_SecondaryPreference, ResString.GetMultilingualString("46438493-87F3-45C5-AF22-5F8FDB68E435", InvoiceLineFilterConstants.Agreement));
					filters.RemoveFilter(filters[Customs.GUI.InvoiceLineFilterConstants.ContainerNumber]);
				}

				if ((Declaration.IsImportSiscomex || Declaration.IsImportLicense))
				{
					AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.ManufacturerCode, invoiceLine => ((JobComInvoiceLine)invoiceLine).ManufacturerOrgCode, ResString.GetMultilingualString("94147810-3D5E-4F56-BFED-A5569BA961AB", InvoiceLineFilterConstants.ManufacturerCode));
					AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.NaladiHs, invoiceLine => ((JobComInvoiceLine)invoiceLine).NaladiHs, ResString.GetMultilingualString("7A0146A6-549A-48EF-8B59-C746FE2074D5", InvoiceLineFilterConstants.NaladiHs));
					AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.DutyTaxRegime, invoiceLine => ((JobComInvoiceLine)invoiceLine).DutyTaxRegime, ResString.GetMultilingualString("BD4C8B0A-679E-4291-99BE-0A612C7A3E8C", InvoiceLineFilterConstants.DutyTaxRegime));
					AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.DutyLegalBase, invoiceLine => ((JobComInvoiceLine)invoiceLine).DutyLegalBase, ResString.GetMultilingualString("D5E73DB4-0A0E-4735-AE7D-EFE0EC4474CC", InvoiceLineFilterConstants.DutyLegalBase));
					AddTranslatableTextFunc(filters, InvoiceLineFilterConstants.InvoiceUQ, invoiceLine => ((JobComInvoiceLine)invoiceLine).JI_InvoiceUQ, ResString.GetMultilingualString("DAAB7D57-525E-4C83-AB08-C0DE817BB493", InvoiceLineFilterConstants.InvoiceUQ));
				}
			}

			AddTranslatableNumberRangeFunc(filters, InvoiceLineFilterConstants.NetWeight, invoiceLine => ((JobComInvoiceLine)invoiceLine).JI_NetWeight, ResString.GetMultilingualString("AD1CC6AE-B15F-4272-AAB0-DBE7BA21A21E", InvoiceLineFilterConstants.NetWeight));
		}
	}
}
