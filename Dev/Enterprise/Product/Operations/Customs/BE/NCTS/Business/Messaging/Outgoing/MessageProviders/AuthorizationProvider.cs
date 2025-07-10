using CargoWise.Customs.BE.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Argument = CargoWise.Common.Argument;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class AuthorizationProvider : IAuthorization
	{
		readonly CusAuthorizationUsage cusAuthorizationUsage;

		public AuthorizationProvider(CusAuthorizationUsage cusAuthorizationUsage, ZInt sequenceNumber)
		{
			this.cusAuthorizationUsage = Argument.NotNull(cusAuthorizationUsage, nameof(cusAuthorizationUsage));
			SequenceNumber = sequenceNumber.ToString();
		}

		public string SequenceNumber { get; }

		public string Type => cusAuthorizationUsage.CustomsCode;

		public string ReferenceNumber => cusAuthorizationUsage.AGC_Number;

		public string HolderOfAuthorisation => null;
	}
}
