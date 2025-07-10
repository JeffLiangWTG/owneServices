using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class QueryOnGuaranteeSendingObject : BaseMessageSendingObject
	{
		// DO NOT DELETE - needed by ZFormBasherTest due to password field
		public QueryOnGuaranteeSendingObject() { }

		public QueryOnGuaranteeSendingObject(NctsGuarantee guarantee)
		{
			this.guarantee = Argument.NotNull(guarantee, nameof(guarantee));
		}

		public ZString GuaranteeType => guarantee.PW_BondType;

		public ZString GuaranteeReferenceNumber => guarantee.PW_BondNumber;

		public ZString OtherGuaranteeReference => guarantee.PW_BondNumber2;

		[Password]
		public ZString AccessCode => guarantee.PW_Password;

		protected readonly NctsGuarantee guarantee;
	}
}
