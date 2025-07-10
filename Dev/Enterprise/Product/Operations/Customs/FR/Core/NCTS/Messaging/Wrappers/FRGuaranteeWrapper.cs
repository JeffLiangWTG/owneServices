using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging
{
	public class FRGuaranteeWrapper : EU.NCTS.Business.GuaranteeWrapper, IGuarantee
	{
		public FRGuaranteeWrapper(FRNctsGuarantee guarantee) : base(guarantee)
		{
		}

		ZString IGuarantee.GuaranteeReferenceNumber => ((FRNctsGuarantee)guarantee).GuaranteeReferenceNumber;
	}
}
