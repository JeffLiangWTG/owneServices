using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonAuthorisationWrapper : INCTSCommonAuthorisation
	{
		public NCTS5CommonAuthorisationWrapper(EU.NCTS.Business.CusAuthorizationUsage authorization, ZShort seqNum)
		{
			this.authorization = Argument.NotNull(authorization, nameof(authorization));

			SequenceNumber = seqNum.ToString();
		}
		readonly EU.NCTS.Business.CusAuthorizationUsage authorization;

		public ZString SequenceNumber { get; }

		public ZString Type => authorization.CustomsCode.IsEmpty ? authorization.AGC_Code : authorization.CustomsCode;

		public ZString ReferenceNumber => authorization.AGC_Number;
	}
}
