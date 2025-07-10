using CargoWise.Common;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.RD409;
using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging.UCC5
{
	public sealed class RD409Provider
	{
		public RD409Provider(Rd409 xmlObject)
		{
			this.xmlObject = Argument.NotNull(xmlObject, nameof(xmlObject));
		}
		readonly Rd409 xmlObject;

		public ZString ApplicationReferenceId => Header?.ApplicationReferenceId;

		public ZString Date => Header?.Date;

		public ZString Applicant => Header?.Applicant;

		public ZBool DepositRefundApplicationApproved => Header?.DepositRefundApplicationApproved ?? false;

		public ZString ReasonNotApproved => Header?.ReasonNotApproved;

		public ZString StatementOfTheDecisionTakingCustomsAuthority => Header?.StatementOfTheDecisionTakingCustomsAuthority;

		public ZDecimal AmountOfDepositRefund => (DepositRefundDetails?.AmountOfDepositRefund).GetValueOrDefault();

		public ZString PayerEORIForRefund => DepositRefundDetails?.PayerEoriForRefund;

		HeaderType Header => xmlObject.Header;

		DepositRefundDetailsType DepositRefundDetails => xmlObject.DepositRefundDetails;
	}
}
