using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.MasterFiles.Business;
using CusEntryHeader = Enterprise.Customs.FR.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class RepTaxOrganisationWrapper : IOrganisation
	{
		public static RepTaxOrganisationWrapper New(CusEntryHeader entryHeader, ErrorCollector errorCollector)
		{
			var fiscalReference = GetFiscalReference(entryHeader, errorCollector);
			return fiscalReference == null ? null : new RepTaxOrganisationWrapper(fiscalReference);
		}

		static CusFiscalReference GetFiscalReference(CusEntryHeader entryHeader, ErrorCollector errorCollector)
		{
			OrgHeader orgHeaderRepTax = null;
			CusFiscalReference fiscalReference = null;
			var declaration = entryHeader.Declaration;

			if (declaration.IsImport)
			{
				if (declaration.Importer != null)
				{
					orgHeaderRepTax = declaration.Importer;
				}
			}
			else
			{
				if (declaration.Supplier != null)
				{
					orgHeaderRepTax = declaration.Supplier;
				}
			}

			if (orgHeaderRepTax != null)
			{
				var euAddInfo = EUOrgImpAddInfo.Get(orgHeaderRepTax, declaration.CountryCode);
				if (euAddInfo != null)
				{
					euAddInfo.Deserialise();
					if (euAddInfo.ZO_UseFr3FiscalRepresentation && entryHeader.EntryInstruction != null)
					{
						fiscalReference = entryHeader.EntryInstruction.FiscalReferences.Cast<CusFiscalReference>().FirstOrDefault(x => x.CFR_Code == FiscalReferenceCodeList.Codes.FR3_TaxRepresentative);
					}
				}
			}

			return fiscalReference;
		}

		RepTaxOrganisationWrapper(CusFiscalReference fiscalReference)
		{
			this.fiscalReference = fiscalReference;
		}

		public ZString OrganisationNumber => fiscalReference.CFR_Reference;

		public ZString OrganisationNumberEoriOnly => fiscalReference.CFR_Reference;

		public ZString FullName => fiscalReference.Owner?.Header?.OH_FullName ?? ZString.Empty;

		public ZString Address => fiscalReference.Owner?.OA_Address1 ?? ZString.Empty;

		public ZString CountryCode => fiscalReference.Owner?.OA_RN_NKCountryCode ?? ZString.Empty;

		public ZString PostCode => fiscalReference.Owner?.OA_PostCode ?? ZString.Empty;

		public ZString City => fiscalReference.Owner?.OA_City ?? ZString.Empty;

		#region Export

		public ZString PartnerConsigneeID => fiscalReference.Owner?.Header?.CustomsClientID ?? ZString.Empty;

		public ZString PartnerDestIDInfo => fiscalReference.Owner?.UnrestrictedAdditionalAddressInformation ?? ZString.Empty;

		public ZString EoriCode => fiscalReference.Owner?.GetUnprefixedEORI() ?? ZString.Empty;

		#endregion

		readonly CusFiscalReference fiscalReference;
	}
}
