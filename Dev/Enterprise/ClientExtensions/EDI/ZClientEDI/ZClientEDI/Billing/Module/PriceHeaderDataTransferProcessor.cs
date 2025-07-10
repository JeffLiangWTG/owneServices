using CargoWise.EntityFramework;
using CargoWise.Types;
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
	public class PriceHeaderDataTransferProcessor : SimpleModuleDataTransferProcessor<ClientLicencePriceHeader, PriceHeaderImport>
	{
		public PriceHeaderDataTransferProcessor(ClientLicencePriceHeaderCollection collection, IImportCollectionInfo collectionInfo)
			: base(collection, collectionInfo)
		{
		}

		public override void Import()
		{
			if (flattenedCollection.Count != 0)
			{
				base.Import();
			}
		}

		void AddErrorMessage(PriceHeaderImport flat, string reason)
		{
			Log += Res.GetString("59D1682D-36F0-4EC2-BD02-55F54D10F39D", "Record [Org. Code: {0}, Price List version: {1}, Currency: {2}] excluded: {3}"
				, flat.OrgCode, flat.PricelistVersion, flat.Currency, reason) + System.Environment.NewLine;
		}

		protected override ClientLicencePriceHeader CreateHeader(IBusinessObjectCollection headerCollection, PriceHeaderImport flat)
		{
			var org = Factory.LoadFromNaturalKey<EDIOrgHeader>(OrgHeaderSchema.OH_Code, flat.OrgCode);
			if (org == null)
			{
				AddErrorMessage(flat, Res.GetString("797FC8FA-F0E0-4C0C-BEFB-7C1D18B40733", "Org. Code not found"));
				return null;
			}

			if (org.LicCompany == null)
			{
				AddErrorMessage(flat, Res.GetString("4DF64683-CBA0-4D46-BD20-DC9E619DFDBA", "Server Code not found"));
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

			var stdPricesCompany = LicenceCompany.GetStandardPricesCompany(Factory);
			if (stdPricesCompany != null)
			{
				var query = new ZQuery(ClientLicencePriceHeaderSchema.L6_PricelistVersion, flat.PricelistVersion);
				query.AddToFilter(ClientLicencePriceHeaderSchema.L6_SystemCode, BillingConstants.PriceHeaderType.ODM);
				query.AddToFilter(ClientLicencePriceHeaderSchema.L6_RX_NKCurrency, flat.Currency);
				query.AddToFilter(ClientLicencePriceHeaderSchema.L6_LC, stdPricesCompany.PK);

				var stdPriceHeader = Factory.LoadTop1<ClientLicencePriceHeader>(query);
				if (stdPriceHeader == null)
				{
					AddErrorMessage(flat, Res.GetString("B2884933-43F7-4477-A3CF-C609742340F1", "Matching version and currency not found"));
					return null;
				}
			}

			var duplicateQuery = new ZQuery(ClientLicencePriceHeaderSchema.L6_SystemCode, BillingConstants.PriceHeaderType.ODM);
			duplicateQuery.AddToFilter(ClientLicencePriceHeaderSchema.L6_LC, org.LicCompany.PK);
			var query1 = new ZQuery(ClientLicencePriceHeaderSchema.L6_RX_NKCurrency, flat.Currency);
			query1.AddToFilter(ClientLicencePriceHeaderSchema.L6_PricelistVersion, flat.PricelistVersion);
			var query2 = new ZQuery(new ZQuery(ClientLicencePriceHeaderSchema.L6_ValidFrom, flat.ValidFrom));
			var orQuery = new ZQuery();
			orQuery.AddToFilter(query1);
			orQuery.AddToFilter(query2, JoinCondition.Or);
			duplicateQuery.AddToFilter(orQuery);
			if (null != Factory.LoadTop1<ClientLicencePriceHeader>(duplicateQuery))
			{
				AddErrorMessage(flat, Res.GetString("E3CE9D8F-EAC7-496D-B564-022089B04E5E", "Price List with same settings or Valid From already exists"));
				return null;
			}

			var priceHeader = Factory.New<ClientLicencePriceHeader>();
			priceHeader.L6_LC = org.LicCompany.PK;
			priceHeader.L6_ValidFrom = flat.ValidFrom;
			priceHeader.L6_PricelistVersion = flat.PricelistVersion;
			priceHeader.L6_RX_NKCurrency = flat.Currency;
			priceHeader.L6_SystemCode = BillingConstants.PriceHeaderType.ODM;
			priceHeader.L6_IsStandard = true;
			priceHeader.L6_UseStandardDiscount = flat.UseStdDiscountIsDefined ? flat.UseStdDiscount : ZBool.True;
			priceHeader.L6_LicenceEdition = BillingConstants.LicenceEdition.Country;
			headerCollection.Add(priceHeader);
			return priceHeader;
		}
	}
}
