using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Netting
{
	public class NettingMovement : NonPersistentBusinessObject
	{
		public NettingMovement(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public ZGuid OrgPK => Organization != null ? Organization.PK : ZGuid.Empty;

		public OrgHeader Organization
		{
			get
			{
				if (organization == null && !CompanyCode.IsEmpty)
				{
					organization = OrgHeader.GetCompanyOrgProxyFromCompanyCode(Factory, CompanyCode);
				}

				return organization;
			}
		}

		OrgHeader organization;
		public ZString NettingType { get; set; }
		public ZString Currency { get; set; }
		public ZString Direction { get; set; }
		public ZDecimal MovementAmount { get; set; }
		public ZDecimal SignedMovementAmount { get; set; }
		public ZString NettingCurrency { get; set; }
		public ZDecimal ReportingRate { get; set; }
		public ZDecimal NettingCurrencyReportingAmount { get; set; }
		public ZDecimal Dealtrate { get; set; }
		public ZDecimal NettingCurrencyDealtAmount { get; set; }
		public MovementType MovementType { get; set; }
		public ZString CompanyCode { get; set; }
		public AccAPAccountDetails NettingAccountDetails
		{
			get
			{
				return Organization != null ? Organization.CompanyData.ARAccountDetailsCollection.Cast<AccARAccountDetails>().FirstOrDefault(x => x.A1_PaymentMethod == "NET" && x.A1_RX_NKAccountCurrency == Currency) : null;
			}
		}

		public override bool Equals(object obj)
		{
			var nettingMovement = obj as NettingMovement;
			if (nettingMovement.CompanyCode == CompanyCode && nettingMovement.Currency == Currency && nettingMovement.Direction == Direction && nettingMovement.MovementAmount == MovementAmount)
			{
				return true;
			}

			return false;
		}

		public override int GetHashCode()
		{
			return CompanyCode.GetHashCode() ^ Currency.GetHashCode() ^ Direction.GetHashCode() ^ MovementAmount.GetHashCode();
		}
	}

	public enum MovementType
	{
		Transaction,
		Offer,
		Request
	}
}
