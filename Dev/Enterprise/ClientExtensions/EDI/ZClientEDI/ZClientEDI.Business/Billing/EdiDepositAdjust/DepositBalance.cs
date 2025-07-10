using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class DepositBalance : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DepositBalance(BusinessObjectFactory factory, ZGuid orgPk, string chargeCode)
			: base(factory)
		{
			OrgPk = orgPk;
			ChargeCode = chargeCode;
		}

		public DepositBalance(LicenceCompany company, string chargeCode)
			: base(company != null ? company.Factory : null)
		{
			ChargeCode = chargeCode;
			if (company != null)
			{
				OrgPk = company.LC_OH;
				Company = company;
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			IsValid = true;
		}

		readonly LicenceCompany Company;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CA1031")] // catching Exception
		public static bool UpdateSafe(bool rebuildAll)
		{
			try
			{
				Update(rebuildAll);
				return true;
			}
			catch (Exception ex)
			{
				if (ex.IsCriticalException())
				{
					throw;
				}

				return false;
			}
		}

		public static void Update(bool rebuildAll)
		{
			using (var cmd = Db.Connection.Command("exec EdiDepositBalanceUpdate " + (rebuildAll ? "1" : "0")))
			{
				cmd.ExecuteNonQuery();
			}
		}

		public static bool UpdateWithRetries(bool rebuildAll, int maxTries, int millisecondsBetweenTries)
		{
			int tries = 0;
			bool ok;
			do
			{
				if (tries > 0)
				{
					System.Threading.Thread.Sleep(millisecondsBetweenTries);
				}
				ok = UpdateSafe(rebuildAll);
				++tries;
			} while (!ok && (tries < maxTries));
			return ok;
		}

		#region Deposit Adjust

		public EdiDepositAdjustCollection Adjustments
		{
			get
			{
				if (adjustments == null)
				{
					Company.CreateDepositAdjustmentsIfNeeded(true);
					adjustments = new EdiDepositAdjustCollection(Org, ChargeCode);
					RegisterEditableChildObject(adjustments);
				}
				return adjustments;
			}
		}

		EdiDepositAdjustCollection adjustments;

		#endregion

		public EDIOrgHeader Org
		{
			get { return Factory.Load<EDIOrgHeader>(OrgPk); }
		}

		#region OrgPk

		public ZGuid OrgPk
		{
			get { return orgPk; }
			set
			{
				SetNonPersistentPropertyValue(OrgPkInfo, ref orgPk, value);
			}
		}
		ZGuid orgPk;

		public ZPropertyInfo OrgPkInfo
		{
			get { return GetZPropertyInfo(nameof(OrgPk)); }
		}

		#endregion

		#region ChargeCode

		public ZString ChargeCode
		{
			get { return chargeCode; }
			set
			{
				SetNonPersistentPropertyValue(ChargeCodeInfo, ref chargeCode, value);
			}
		}
		ZString chargeCode;

		public ZPropertyInfo ChargeCodeInfo
		{
			get { return GetZPropertyInfo(nameof(ChargeCode)); }
		}

		#endregion

		#region CurrencyCode

		public ZString CurrencyCode
		{
			get { return currencyCode; }
			set
			{
				SetNonPersistentPropertyValue(CurrencyCodeInfo, ref currencyCode, value);
			}
		}
		ZString currencyCode;

		public ZPropertyInfo CurrencyCodeInfo
		{
			get { return GetZPropertyInfo(nameof(CurrencyCode)); }
		}

		#endregion

		#region Amount

		public ZDecimal Amount
		{
			get { return amount; }
			set
			{
				SetNonPersistentPropertyValue(AmountInfo, ref amount, value);
			}
		}
		ZDecimal amount;

		public ZPropertyInfo AmountInfo
		{
			get { return GetZPropertyInfo(nameof(Amount)); }
		}

		#endregion

		#region Tax

		public ZDecimal Tax
		{
			get { return tax; }
			set
			{
				SetNonPersistentPropertyValue(TaxInfo, ref tax, value);
			}
		}
		ZDecimal tax;

		public ZPropertyInfo TaxInfo
		{
			get { return GetZPropertyInfo(nameof(Tax)); }
		}

		#endregion

		#region IsCurrent

		public ZBool IsCurrent
		{
			get { return isCurrent; }
			set
			{
				SetNonPersistentPropertyValue(IsCurrentInfo, ref isCurrent, value);
			}
		}
		ZBool isCurrent;

		public ZPropertyInfo IsCurrentInfo
		{
			get { return GetZPropertyInfo(nameof(IsCurrent)); }
		}

		#endregion

		#region IsValid

		public ZBool IsValid
		{
			get { return isValid; }
			set
			{
				SetNonPersistentPropertyValue(IsValidInfo, ref isValid, value);
			}
		}
		ZBool isValid;

		public ZPropertyInfo IsValidInfo
		{
			get { return GetZPropertyInfo(nameof(IsValid)); }
		}

		#endregion

		public ZDateTime LastTransactionUtc
		{
			get => lastTransactionUtc;
			set => SetNonPersistentPropertyValue(LastTransactionUtcInfo, ref lastTransactionUtc, value);
		}
		ZDateTime lastTransactionUtc;

		public ZPropertyInfo LastTransactionUtcInfo => GetZPropertyInfo(nameof(LastTransactionUtc));

		public ZDateTime LastTransactionTimeLocal => LastTransactionUtc.ToLocalBranchTime();
	}
}

