using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class DepositBalanceCollection : NonPersistentBusinessObjectCollection<DepositBalance>
	{
		public DepositBalanceCollection(LicenceCompany parent)
			: base(parent != null ? parent.Factory : null)
		{
			Company = parent;
		}

		readonly LicenceCompany Company;

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DepositBalance(Company, "");
		}

		public static IList<DepositBalance> LoadFromDb(IEnumerable<Guid> orgPks = null, bool updateIfNeeded = false)
		{
			var result = new List<DepositBalance>();

			if (orgPks == null || orgPks.Any())
			{
				string sql = "select DEB_OH, DEB_ChargeCode, DEB_RX_NKCurrency, DEB_Amount, DEB_Tax, DEB_IsCurrent, DEB_IsValid, DEB_LastTransactionUtc from dbo.EdiViewDepositBalance"
					+ (orgPks != null && orgPks.Any() ? " where DEB_OH in ('" + string.Join("', '", orgPks) + "')" : "");

				int tryCount = updateIfNeeded ? 2 : 1;
				do
				{
					using (var cmd = Db.Connection.Command(sql))
					{
						using (var reader = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
						{
							while (reader.Read())
							{
								var pk = reader.GetGuid(0);
								var chargeCode = reader.GetString(1);
								var item = new DepositBalance(null, pk, chargeCode);
								item.CurrencyCode = reader.GetString(2);
								item.Amount = reader.GetDecimal(3);
								item.Tax = reader.GetDecimal(4);
								item.IsCurrent = reader.GetBoolean(5);
								item.IsValid = reader.GetBoolean(6);
								var dateOrDbNull = reader.GetValue(7);
								item.LastTransactionUtc = (dateOrDbNull is DBNull) ? ZDateTime.Empty : (DateTime)dateOrDbNull;
								result.Add(item);
							}
						}
					}

					if (tryCount > 1)
					{
						if (!result.Any(x => !x.IsCurrent))
						{
							tryCount = 1;
						}
						else
						{
							result.Clear();
							DepositBalance.UpdateSafe(false);
						}
					}
				}
				while (--tryCount > 0);
			}

			return result;
		}

		public static IList<DepositBalance> LoadFromDb(ZGuid orgPk, bool updateIfNeeded = false)
		{
			return LoadFromDb(!orgPk.IsEmpty ? new Guid[] { orgPk.ToGuid() } : null, updateIfNeeded);
		}

		public override void Load()
		{
			var list = LoadFromDb(Company.LC_OH, true);

			var chargeCodes = EDIDataRegistry.Instance.DepositChargeCodes.Value;
			var mainCodes = chargeCodes.Cast<ICodeDescription>().Select(x => string.IsNullOrEmpty(x.Description) ? x.Code : x.Description).Distinct().OrderBy(x => x).ToArray();

			if (Company != null)
			{
				if (mainCodes.Length > 0 && Count == 0)
				{
					Company.Factory.Saved += Factory_Saved;
				}

				foreach (var chargeCode in mainCodes)
				{
					var item = this.Cast<DepositBalance>().FirstOrDefault(x => x.ChargeCode == chargeCode);
					if (item == null)
					{
						item = new DepositBalance(Company, chargeCode);
						this.Add(item);
					}

					var loaded = list.FirstOrDefault(x => x.ChargeCode == chargeCode);
					if (loaded != null)
					{
						item.CurrencyCode = loaded.CurrencyCode;
						item.Amount = loaded.Amount;
						item.Tax = loaded.Tax;
						item.IsCurrent = loaded.IsCurrent;
						item.IsValid = loaded.IsValid;
						item.LastTransactionUtc = loaded.LastTransactionUtc;
					}
				}
			}
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			if (savedSuccessfully)
			{
				Load();
				HasChanges = false;
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		#endregion
	}
}


