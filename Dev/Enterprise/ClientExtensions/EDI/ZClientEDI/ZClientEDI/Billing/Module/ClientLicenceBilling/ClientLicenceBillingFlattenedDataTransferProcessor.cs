using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.Billing.Module
{
	public class ClientLicenceBillingFlattenedDataTransferProcessor : SimpleModuleDataTransferProcessor<ClientLicenceBilling, ClientLicenceBillingFlattened>
	{
		public ClientLicenceBillingFlattenedDataTransferProcessor(ClientLicenceBillingCollectionNonDependent collection, IImportCollectionInfo collectionInfo)
			: base(collection, collectionInfo)
		{
		}

		public override void Import()
		{
			if (flattenedCollection.Count == 0)
			{
				return;
			}

			OrgCodeMap = Factory.Load<EDIOrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, flattenedCollection.Cast<ClientLicenceBillingFlattened>().Select(x => x.OrgCode)))
				.ToDictionary(x => (string)x.OH_Code);
			Currencies = new HashSet<ZString>(new RefCurrencyCollection(Factory).Select(x => x.RX_Code));

			base.Import();
		}

		public override void Rollback()
		{
			foreach (var header in headerCollection.OfType<ClientLicenceBilling>())
			{
				header.CancelChanges();
			}
		}

		protected override ClientLicenceBilling CreateHeader(IBusinessObjectCollection headerCollection, ClientLicenceBillingFlattened flat)
		{
			EDIOrgHeader org;
			bool updated = false;

			if (!OrgCodeMap.TryGetValue(flat.OrgCode, out org))
			{
				AddMessage(flat, Res.GetString("d4481ddc-282b-4834-87a0-d2ef5316f765", "Org. Code not found"));
				return null;
			}

			var clientLicenceBilling = org.LicCompany?.ReadonlySelfBilling;

			if (clientLicenceBilling == null)
			{
				AddMessage(flat, Res.GetString("24132478-d57e-476a-8cdf-0ea87a35433f", "Org. {0} has no license", org.OH_Code));
				return null;
			}

			if (flat.IsCurrentPrepaymentBalanceProvided)
			{
				if (CheckCurrency(flat, flat.CurrentPrepaymentCurrency))
				{
					clientLicenceBilling.L4_RX_NKPredeterminedPrepaidBalanceCurrency = flat.CurrentPrepaymentCurrency;
					updated = true;
				}
				else
				{
					clientLicenceBilling.CancelChanges();
					return null;
				}
			}

			if (flat.IsCurrentPrepaymentBalanceProvided)
			{
				clientLicenceBilling.L4_PredeterminedPrepaidBalance = flat.CurrentPrepaymentBalance;
				updated = true;
			}

			if (flat.IsFuturePrepaymentBalanceProvided)
			{
				if (CheckCurrency(flat, flat.FuturePrepaymentCurrency))
				{
					clientLicenceBilling.L4_RX_NKFuturePredeterminedPrepaidBalanceCurrency = flat.FuturePrepaymentCurrency;
					updated = true;
				}
				else
				{
					clientLicenceBilling.CancelChanges();
					return null;
				}
			}

			if (flat.IsFuturePrepaymentBalanceProvided)
			{
				clientLicenceBilling.L4_FuturePredeterminedPrepaidBalance = flat.FuturePrepaymentBalance;
				updated = true;
			}

			if (updated)
			{
				headerCollection.Add(clientLicenceBilling);
				return clientLicenceBilling;
			}
			else
			{
				AddMessage(flat, Res.GetString("00351805-a524-4e61-9053-2cf7cf254145", "Org. {0} has no data", org.OH_Code));
				return null;
			}
		}

		void AddMessage(ClientLicenceBillingFlattened flat, string reason)
		{
			Log += Res.GetString("c31772bc-d851-4f0e-b55a-653f489f1dfb", "Record [Org. Code: {0}] excluded: {1}"
				, flat.OrgCode, reason) + System.Environment.NewLine;
		}

		bool CheckCurrency(ClientLicenceBillingFlattened flat, ZString currency)
		{
			if (currency.IsEmpty || Currencies.Contains(currency))
			{
				return true;
			}
			else
			{
				AddMessage(flat, Res.GetString("734f82d8-0c81-41b7-bcbf-4059acdd79be", "Invalid Currency: {0}", currency));
				return false;
			}
		}

		Dictionary<string, EDIOrgHeader> OrgCodeMap;
		HashSet<ZString> Currencies;
	}
}
