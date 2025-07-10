using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class DepartureGuaranteeNumberWrapper : IGuaranteeNumber
	{
		public DepartureGuaranteeNumberWrapper(ZString guaranteeType, ZString guaranteeAccessCode)
		{
			Type = guaranteeType;
			AccessCode = guaranteeAccessCode;
		}

		public ZString Type { get; }

		public ZString AccessCode { get; }
	}
}
