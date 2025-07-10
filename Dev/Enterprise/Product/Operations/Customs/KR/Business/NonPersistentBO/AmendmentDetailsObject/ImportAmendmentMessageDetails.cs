using System;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ImportAmendmentMessageDetails : BaseAmendmentMessageDetails
	{
		public ImportAmendmentMessageDetails(CargoWise.Customs.KR.MessageDefinitions.GOVCBR5FE.Declaration messageDeclaration)
		{
			if (Object.ReferenceEquals(messageDeclaration, null))
			{
				throw new ArgumentNullException(nameof(messageDeclaration));
			}
			MessageDeclaration5FE = messageDeclaration;
		}
		CargoWise.Customs.KR.MessageDefinitions.GOVCBR5FE.Declaration MessageDeclaration5FE { get; }

		public override ZInt AmendmentVersionNo => ZInt.ParseSafe(MessageDeclaration5FE.VersionId.Value, 0);
		public override ZString AmendmentType => MessageDeclaration5FE.TransactionNatureCode.Value;
		public override ZString AmendReasonDescription => MessageDeclaration5FE.Amendment.Content?.Value ?? ZString.Empty;
		public override ZString ReasonCode => MessageDeclaration5FE.Amendment.ChangeReasonCode.Value;
		public override ZString FaultParty => MessageDeclaration5FE.ReasonCode.Value;
		public override ZString FaultPartyOtherDescription => MessageDeclaration5FE?.Reason?.Value ?? ZString.Empty;
		public ZString AmendmentTypeDescription => amendmentTypeList.GetDescriptionFromCode(AmendmentType);
		readonly _5ASAmendmentType amendmentTypeList = new _5ASAmendmentType();
		public ZDateTime SubmissionDate => ZDateTime.TryParseExact(MessageDeclaration5FE.IssueDateTime, out ZDateTime submissionDate, Constants.DateFormatType.Date) ? submissionDate : ZDateTime.Empty;
		public ZString Details => MessageDeclaration5FE.AdditionalInformation.StatementDescription?.Value ?? ZString.Empty;

		public override ZString PenaltyPaymentReasonCode => MessageDeclaration5FE.AdditionalInformation.AdditionalPaymentCode?.Value ?? ZString.Empty;
		public override ZDate DateOfFinalPrice => throw new NotImplementedException();
	}
}
