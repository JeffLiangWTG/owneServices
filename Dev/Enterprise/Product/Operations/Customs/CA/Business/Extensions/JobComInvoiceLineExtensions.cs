using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public static class JobComInvoiceLineExtensions
	{
		public static OrgHeader GetLPCOHolderParty(this JobComInvoiceLine invoiceLine, ZString type)
		{
			switch (type)
			{
				case LPCOHolderPartyTypeCodes.Codes.Importer:
				case LPCOHolderPartyTypeCodes.Codes.ImporterOfRecord:
					{
						return invoiceLine.Declaration?.GetLPCOHolderParty(type);
					}

				case LPCOHolderPartyTypeCodes.Codes.Supplier:
					{
						return invoiceLine.InvoiceHeader?.Supplier ?? invoiceLine.Declaration?.GetLPCOHolderParty(type);
					}

				case LPCOHolderPartyTypeCodes.Codes.Exporter:
					{
						return invoiceLine.InvoiceHeader?.ExporterDocumentaryAddress.HasRealOrganisation ?? false ? invoiceLine.InvoiceHeader?.ExporterDocumentaryAddress.Organisation : null;
					}

				case LPCOHolderPartyTypeCodes.Codes.Manufacturer:
					{
						return invoiceLine.ManufacturerAddress?.Header ?? invoiceLine.InvoiceHeader?.ManufacturerAddress?.Header;
					}

				default:
					{
						return null;
					}
			}
		}

		public static ZString GetLPCOHolderType(this JobComInvoiceLine invoiceLine, ZGuid stakeHolderOrg)
		{
			var result = ZString.Empty;
			if (!stakeHolderOrg.IsEmpty)
			{
				var invoiceHeader = invoiceLine.InvoiceHeader;
				if (stakeHolderOrg == invoiceLine.ManufacturerOrgPK)
				{
					result = LPCOHolderPartyTypeCodes.Codes.Manufacturer;
				}
				else if (invoiceHeader != null)
				{
					if (invoiceHeader.JZ_OH_Supplier_Effective == stakeHolderOrg)
					{
						result = LPCOHolderPartyTypeCodes.Codes.Supplier;
					}
					else if (GetEffectiveOrganisationPK(invoiceHeader.ExporterDocumentaryAddress) == stakeHolderOrg)
					{
						result = LPCOHolderPartyTypeCodes.Codes.Exporter;
					}
				}

				if (result.IsEmpty)
				{
					var dec = invoiceLine.Declaration;
					if (dec != null)
					{
						if (dec.ImporterOfRecord != null && stakeHolderOrg == dec.ImporterOfRecord.PK)
						{
							result = LPCOHolderPartyTypeCodes.Codes.ImporterOfRecord;
						}
						else if (stakeHolderOrg == dec.JE_OH_Importer)
						{
							result = LPCOHolderPartyTypeCodes.Codes.Importer;
						}
					}
				}
				if (result.IsEmpty && GlbCompany.CurrentCompany.GC_OH_OrgProxy == stakeHolderOrg)
				{
					result = LPCOHolderPartyTypeCodes.Codes.Broker;
				}
			}
			return result;
		}

		public static IEnumerable<ZGuid> GetHolderOrgHeadersPks(this JobComInvoiceLine invoiceLine)
		{
			var holderOrgHeadersPks = new List<ZGuid>();
			if (invoiceLine != null)
			{
				holderOrgHeadersPks.Add(invoiceLine.ManufacturerOrgPK);
				var invoiceHeader = invoiceLine.InvoiceHeader;
				if (invoiceHeader != null)
				{
					holderOrgHeadersPks.Add(invoiceHeader.JZ_OH_Supplier_Effective);
					holderOrgHeadersPks.Add(GetEffectiveOrganisationPK(invoiceHeader.ExporterDocumentaryAddress));
				}
			}
			var dec = invoiceLine.Declaration;
			if (dec != null)
			{
				holderOrgHeadersPks.Add(dec.JE_OH_Importer);
				if (dec.ImporterOfRecord != null)
				{
					holderOrgHeadersPks.Add(dec.ImporterOfRecord.PK);
				}
			}
			return holderOrgHeadersPks;
		}

		static ZGuid GetEffectiveOrganisationPK(JobDocAddress docaAddress) => docaAddress.E2_AddressOverride ? ZGuid.Empty : docaAddress.OrganisationPK;
	}
}
