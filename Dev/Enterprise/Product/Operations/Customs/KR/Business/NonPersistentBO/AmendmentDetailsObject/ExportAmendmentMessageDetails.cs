using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class ExportAmendmentMessageDetails : BaseAmendmentMessageDetails
	{
		public ExportAmendmentMessageDetails(CargoWise.Customs.KR.MessageDefinitions.GOVCBR5AS.Declaration messageDeclaration)
		{
			MessageDeclaration5AS = messageDeclaration;
		}
		CargoWise.Customs.KR.MessageDefinitions.GOVCBR5AS.Declaration MessageDeclaration5AS { get; }

		public override ZInt AmendmentVersionNo => ZInt.ParseSafe(MessageDeclaration5AS?.VersionId.Value, 0);
		public override ZString AmendmentType => MessageDeclaration5AS?.TransactionNatureCode.Value ?? ZString.Empty;
		public override ZString AmendReasonDescription
		{
			get
			{
				if (AmendmentType == _5ASAmendmentType.Codes.Cancellation)
				{
					return exportDeclarationwithdrawReasonCodeList.GetDescriptionFromCode(ReasonCode);
				}
				return exportAmendmentReasonCodeList.GetDescriptionFromCode(ReasonCode);
			}
		}
		readonly ExportDeclarationwithdrawReasonCodeList exportDeclarationwithdrawReasonCodeList = new ExportDeclarationwithdrawReasonCodeList();
		readonly ExportAmendmentReasonCodeList exportAmendmentReasonCodeList = new ExportAmendmentReasonCodeList();
		public override ZString ReasonCode => MessageDeclaration5AS?.AdditionalInformation.StatementCode.Value ?? ZString.Empty;
		public override ZString FaultParty => MessageDeclaration5AS?.Reason.Value ?? ZString.Empty;
		public override ZString FaultPartyOtherDescription => exportImputationReasonCodeList.GetDescriptionFromCode(FaultParty);
		readonly ExportImputationReasonCodeList exportImputationReasonCodeList = new ExportImputationReasonCodeList();
		public ZString AmendmentTypeDescription => amendmentTypeList.GetDescriptionFromCode(AmendmentType);
		readonly _5ASAmendmentType amendmentTypeList = new _5ASAmendmentType();
		public ZDateTime SubmissionDate => ZDateTime.TryParseExact(MessageDeclaration5AS?.IssueDateTime ?? ZString.Empty, out ZDateTime submissionDate, Constants.DateFormatType.Date) ? submissionDate : ZDateTime.Empty;
		public ZString Details => MessageDeclaration5AS?.AdditionalInformation.StatementDescription.Value ?? ZString.Empty;

		public override ZString PenaltyPaymentReasonCode => throw new System.NotImplementedException();
		public override ZDate DateOfFinalPrice => throw new System.NotImplementedException();
	}
}
