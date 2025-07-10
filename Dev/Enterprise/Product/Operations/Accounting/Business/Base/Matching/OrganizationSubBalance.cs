using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.Base.Matching
{
	/// <summary>
	/// Represents the sub-balance of an organization in matching session
	/// </summary>
	public class OrganizationSubBalance : NonPersistentBusinessObject, IObsoleteValidation
	{
		public OrganizationSubBalance()
		{
		}

		public OrganizationSubBalance(ZGuid organization, ZDecimal amount, ZString ledger)
		{
			this.Ledger = ledger;
			this.Organization = organization;
			this.Amount = amount;
		}

		#region Properties

		#region Organization

		public ZGuid Organization
		{
			get
			{
				return fOrganization;
			}
			set
			{
				fOrganization = value;
			}
		}
		ZGuid fOrganization;

		#endregion

		#region Amount

		public ZDecimal Amount
		{
			get
			{
				return fAmount;
			}
			set
			{
				fAmount = value;
			}
		}
		ZDecimal fAmount;

		#endregion

		#region Ledger

		public ZString Ledger
		{
			get
			{
				return fLedger;
			}
			set
			{
				fLedger = value;
			}
		}
		ZString fLedger;

		#endregion
		#endregion
	}
}
