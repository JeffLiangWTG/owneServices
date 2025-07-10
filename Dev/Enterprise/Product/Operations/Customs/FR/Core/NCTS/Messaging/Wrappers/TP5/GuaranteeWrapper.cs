using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class GuaranteeWrapper : IGuarantee
	{
		GuaranteeWrapper(EU.NCTS.Business.NctsGuarantee guarantee)
		{
			this.guarantee = Argument.NotNull(guarantee, nameof(guarantee));
		}

		readonly EU.NCTS.Business.NctsGuarantee guarantee;

		public static GuaranteeWrapper New(EU.NCTS.Business.NctsGuarantee guarantee) => guarantee == null ? null : new GuaranteeWrapper(guarantee);

		public string GuaranteeType => guaranteeType ?? (guaranteeType = guarantee.PW_BondType);
		string guaranteeType;

		public string OtherGuaranteeReference => otherGuaranteeReference ?? (otherGuaranteeReference = guarantee.PW_BondNumber2);
		string otherGuaranteeReference;

		public IGuaranteeReference GuaranteeReference => guaranteeReference ?? (guaranteeReference = GuaranteeReferenceWrapper.New(guarantee));
		IGuaranteeReference guaranteeReference;
	}
}
