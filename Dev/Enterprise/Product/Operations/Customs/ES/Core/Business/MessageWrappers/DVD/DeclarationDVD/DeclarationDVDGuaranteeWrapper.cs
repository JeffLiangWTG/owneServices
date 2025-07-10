using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DeclarationDVDGuaranteeWrapper : IDeclarationDVDGuarantee
	{
		public DeclarationDVDGuaranteeWrapper(ESGuarantee guarantee)
		{
			this.guarantee = Argument.NotNull(guarantee, nameof(guarantee));
		}
		readonly ESGuarantee guarantee;

		public ZString GRNReference => guarantee.PW_BondType.IsEmpty ? guarantee.PW_BondNumber : ZString.Empty;

		public ZString NoGRNReference => !guarantee.PW_BondType.IsEmpty ? guarantee.PW_BondNumber : ZString.Empty;

		public ZString AccessCode => ZString.Empty;

		public ZString Currency => ZString.Empty;

		public ZDecimal Amount => ZDecimal.Zero;

		public ZString Office => guarantee.PW_BondFiledPort;
	}
}
