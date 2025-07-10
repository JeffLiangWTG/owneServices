using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class GuaranteeWrapper : IGuarantee
	{
		public GuaranteeWrapper(NctsGuarantee guarantee)
		{
			this.guarantee = Argument.NotNull(guarantee, nameof(guarantee));
		}

		public ZString GuaranteeType => guarantee.PW_BondType;

		public ZString GuaranteeReferenceNumber => guarantee.PW_BondNumber;

		public ZString OtherGuaranteeReference => guarantee.PW_BondNumber2;

		public ZString AccessCode => guarantee.PW_Password;

		public ZDecimal TaxAndDutyLiabiltyAmount => guarantee.PW_BondAmount;

		public ZString NotValidForEC => "0";

		public IReadOnlyCollection<ZString> NotValidForOtherContractingParties => notValidForOtherContractingParties ?? (notValidForOtherContractingParties = guarantee.PW_ValidityLimitation.Split(' '));
		IReadOnlyCollection<ZString> notValidForOtherContractingParties;

		protected readonly NctsGuarantee guarantee;
	}
}
