using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.Billing.Module
{
	public class PriceHeaderLinkDataTransferProcessor : SimpleModuleDataTransferProcessor<EdiPriceHeaderLink, PriceHeaderLinkImport>
	{
		public PriceHeaderLinkDataTransferProcessor(EdiPriceHeaderLinkCollection linkCollection, IImportCollectionInfo collectionInfo)
			: base(linkCollection, collectionInfo)
		{
		}

		public override void Import()
		{
			if (flattenedCollection.Count != 0)
			{
				base.Import();
			}
		}

		void AddErrorMessage(PriceHeaderLinkImport flat, string reason)
		{
			Log += Res.GetString("0D284656-EF0A-4C7A-B870-13DF45B6EA84", "Record [Org. Code: {0}, Server Code: {1}, Price List version: {2}, Currency: {3}] excluded: {4}"
				, flat.OrgCode, flat.ServerCode, flat.PricelistVersion, flat.Currency, reason) + System.Environment.NewLine;
		}

		protected override EdiPriceHeaderLink CreateHeader(IBusinessObjectCollection headerCollection, PriceHeaderLinkImport flat)
		{
			var org = Factory.LoadFromNaturalKey<EDIOrgHeader>(OrgHeaderSchema.OH_Code, flat.OrgCode);
			if (org == null)
			{
				AddErrorMessage(flat, Res.GetString("797FC8FA-F0E0-4C0C-BEFB-7C1D18B40733", "Org. Code not found"));
				return null;
			}

			var db = org.LicCompany?.LicDatabases.Cast<LicenceDatabase>().FirstOrDefault(x => x.LD_ServerCode == flat.ServerCode);
			if (db == null)
			{
				AddErrorMessage(flat, Res.GetString("DCB7B7AA-9067-42EA-80ED-F398F36165C8", "Server Code not found"));
				return null;
			}

			var query = new ZQuery(ClientLicencePriceHeaderSchema.L6_PricelistVersion, flat.PricelistVersion);
			query.AddToFilter(ClientLicencePriceHeaderSchema.L6_SystemCode, BillingConstants.PriceHeaderType.STL);
			var stdPricesCompany = LicenceCompany.GetStandardPricesCompany(Factory);
			if (stdPricesCompany != null)
			{
				query.AddToFilter(ClientLicencePriceHeaderSchema.L6_LC, stdPricesCompany.PK);
			}

			var priceHeader = Factory.LoadTop1<ClientLicencePriceHeader>(query);
			if (priceHeader == null)
			{
				AddErrorMessage(flat, Res.GetString("8f24fd26-5f15-4e60-8958-4d8f2e7232bb", "Price List version not found"));
				return null;
			}

			if (null == Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, flat.Currency))
			{
				AddErrorMessage(flat, Res.GetString("8FE36300-D9F9-4FF4-B93F-9EA6F5D8F03D", "Currency not found"));
				return null;
			}

			if (flat.ValidFrom.IsEmpty || !flat.ValidFrom.IsValidSmallDateTime)
			{
				AddErrorMessage(flat, Res.GetString("9D769A28-E869-4C5D-BE1A-0322DE6DEF96", "Valid From is missing or out of range"));
				return null;
			}

			if (db.PriceHeaderLinks.Any(x => x.PHL_ValidFrom == flat.ValidFrom))
			{
				AddErrorMessage(flat, Res.GetString("23FF52C4-2ABD-4CAE-8DEE-BFE66488ED0A", "Price List with same Valid From already exists"));
				return null;
			}

			var link = db.PriceHeaderLinks.AddNew();
			link.PHL_L6 = priceHeader.PK;
			link.PHL_ValidFrom = flat.ValidFrom;
			link.PHL_ValidTo = flat.ValidTo;
			link.PHL_RX_NKCurrency = flat.Currency;
			link.PHL_LD = db.PK;
			link.PHL_CorePackCode = flat.CorePack;
			link.PHL_CoreUpliftPercent = flat.CoreUpliftPercent;
			link.PHL_VolumeCode = flat.Volume;
			link.PHL_VolumePercent = flat.VolumePercent;
			link.Validation.ValidateAll();
			if (!link.HasErrors)
			{
				headerCollection.Add(link);
				return link;
			}
			else
			{
				var err = link.Notifications.FirstOrDefault(x => x.Type == NotificationType.Error);
				AddErrorMessage(flat, err != null ? err.Message : "Invalid");
				link.Delete();
				return null;
			}
		}
	}
}
