using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class GuaranteeReferenceProvider : IGuaranteeReference
	{
		protected readonly NctsGuarantee nctsGuarantee;

		public GuaranteeReferenceProvider(NctsGuarantee nctsGuarantee, ZInt sequence)
		{
			this.nctsGuarantee = Argument.NotNull(nctsGuarantee, nameof(nctsGuarantee));
			SequenceNumber = sequence;
		}

		public int SequenceNumber { get; }

		public virtual string GRN => nctsGuarantee.PW_BondNumber;

		public string AccessCode => nctsGuarantee.PW_Password;

		public decimal AmountToBeCovered => nctsGuarantee.PW_BondAmount;

		public string Currency
		{
			get
			{
				var currency = Core.Constants.CurrencyCodes.UnitedKingdom;

				if (nctsGuarantee.CusGuarantee != null && nctsGuarantee.CusGuarantee.CPH_UnitOfMeasure != string.Empty)
				{
					currency = nctsGuarantee.CusGuarantee.CPH_UnitOfMeasure;
				}
				return currency;
			}
		}
	}
}
