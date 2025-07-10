using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.Billing.Module
{
	public class ClientLicenceFeeFlattenedDataTransferProcessor : SimpleModuleDataTransferProcessor<ClientLicenceFee, ClientLicenceFeeFlattened>
	{
		public ClientLicenceFeeFlattenedDataTransferProcessor(ClientLicenceFeeCollectionNonDependent collection, IImportCollectionInfo collectionInfo)
			: base(collection, collectionInfo)
		{
			alltaxDateCodes = ClientLicenceFeeLookups.AllTaxDateCodes;
		}

		readonly CodeDescriptionPairList alltaxDateCodes;

		public override void Import()
		{
			if (flattenedCollection.Count == 0)
			{
				return;
			}

			var fees = flattenedCollection.Cast<ClientLicenceFeeFlattened>();

			OrgCodeMap = Factory.Load<EDIOrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, fees.Where(x => !x.OrgCode.IsEmpty).Select(x => x.OrgCode)))
				.ToDictionary(x => (string)x.OH_Code);

			base.Import();
		}

		public override void Rollback()
		{
			foreach (var fee in headerCollection.OfType<ClientLicenceFee>().ToArray())
			{
				if (fee.IsInDatabase)
				{
					fee.CancelChanges();
				}
				else
				{
					fee.Delete();
				}
			}
		}

		protected override ClientLicenceFee CreateHeader(IBusinessObjectCollection headerCollection, ClientLicenceFeeFlattened flat)
		{
			var org = GetOrg(flat);
			if (org == null)
			{
				return null;
			}

			LicenceDatabase licenceDatabase = null;
			if (!flat.ServerCode.IsEmpty)
			{
				licenceDatabase = GetLicenceDatabase(org, flat);
				if (licenceDatabase == null)
				{
					return null;
				}
			}

			if (!Validate(flat))
			{
				return null;
			}

			if (org.LicCompany == null)
			{
				AddMessage(flat, Res.GetString("9ccd1e89-dfb4-470f-a05b-a1e0a8b15e17", "Organization does not have a license.", org));
				return null;
			}

			var fee = Import(org.LicCompany.Fees, flat);
			if (licenceDatabase != null && fee != null)
			{
				fee.L8_LD = licenceDatabase.PK;
			}

			if (fee != null)
			{
				headerCollection.Add(fee);
			}

			return fee;
		}

		EDIOrgHeader GetOrg(ClientLicenceFeeFlattened flat)
		{
			EDIOrgHeader org = null;

			if (!flat.OrgCode.IsEmpty)
			{
				if (!OrgCodeMap.TryGetValue(flat.OrgCode, out org))
				{
					AddMessage(flat, Res.GetString("5b9bca19-49d6-4050-8f9d-4e81c1f12232", "Org. Code not found"));
					return null;
				}
			}

			return org;
		}

		LicenceDatabase GetLicenceDatabase(EDIOrgHeader org, ClientLicenceFeeFlattened flat)
		{
			var licenceDatabase = org.LicCompany?.LicHeadersForAllDatabases
				.Select(x => x.Database)
				.FirstOrDefault(x => x.LD_ServerCode == flat.ServerCode);

			if (licenceDatabase == null)
			{
				AddMessage(flat, Res.GetString("4be8ca63-5609-4643-94dd-c539590b5e49", "Database not found"));
				return null;
			}
			return licenceDatabase;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		bool Validate(ClientLicenceFeeFlattened flat)
		{
			if (flat.HasErrors)
			{
				var errMsg = string.Join("\r\n", flat.Notifications.Where(x => x.Type.IsFatal).Select(x => x.Message));
				AddMessage(flat, errMsg);
				return false;
			}

			if (!IsValidDateRange(flat.StartDate, flat.EndDate))
			{
				AddMessage(flat, Res.GetString("0EA7D02F-88A0-4E27-92E8-A99F66A852C4", "Please enter valid Start Date / End Date"));
				return false;
			}

			if (!alltaxDateCodes.ContainsCode(flat.TaxDate))
			{
				AddMessage(flat, Res.GetString("2FF54246-0F9E-419E-8BD8-9ED0841219DA", "Tax Date code is invalid: {0}", flat.TaxDate));
				return false;
			}

			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		ClientLicenceFee Import(ClientLicenceFeeCollection fees, ClientLicenceFeeFlattened flat)
		{
			ClientLicenceFee newFee = fees.Factory.New<ClientLicenceFee>();
			newFee.L8_StartDate = flat.StartDate;
			newFee.L8_EndDate = flat.EndDate;
			newFee.L8_RX_NKCurrency = flat.Currency;
			newFee.L8_Amount = flat.Amount;
			newFee.L8_ChargeCode = flat.ChargeCode;
			newFee.L8_Comment = flat.Comment;
			newFee.L8_Description = flat.Description;
			newFee.L8_Order = flat.Order;
			newFee.L8_RenewalMonths = flat.RenewalMonths;
			newFee.L8_SystemCode = flat.SystemCode;
			newFee.L8_Type = flat.FeeType;
			newFee.L8_IsDiscountable = flat.IsDiscountable;
			newFee.L8_TaxDateCode = flat.TaxDate;

			fees.Add(newFee);

			newFee.RunPreSaveValidation();
			if (newFee.HasErrors)
			{
				var errMsg = string.Join("\r\n", newFee.Notifications.Where(x => x.Type.IsFatal).Select(x => x.Message));
				AddMessage(flat, errMsg);
				newFee.Delete();
				newFee = null;
			}

			return newFee;
		}

		static bool IsValidDateRange(ZDateTime fromDate, ZDateTime toDate)
		{
			return !fromDate.IsEmpty && (toDate.IsEmpty || toDate.Date >= fromDate.Date);
		}

		void AddMessage(ClientLicenceFeeFlattened flat, string reason)
		{
			Log += Res.GetString("2F6B8332-98A8-4990-AF53-8F6D9E32C8B8", "Record [Org. Code: {0}, Server Code: {1}] excluded: {2}"
				, flat.OrgCode, flat.ServerCode, reason) + System.Environment.NewLine;
		}

		Dictionary<string, EDIOrgHeader> OrgCodeMap;
	}
}
