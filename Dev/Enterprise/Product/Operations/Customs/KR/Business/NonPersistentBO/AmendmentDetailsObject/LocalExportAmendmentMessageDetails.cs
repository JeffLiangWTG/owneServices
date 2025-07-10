using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class LocalExportAmendmentMessageDetails : BaseAmendmentMessageDetails
	{
		public LocalExportAmendmentMessageDetails(object messageDeclaration)
		{
			if (messageDeclaration is CargoWise.Customs.KR.MessageDefinitions.GOVCBR5DR.Declaration declaration5DR)
			{
				Declaration5DR = declaration5DR;
			}
			else if (messageDeclaration is CargoWise.Customs.KR.MessageDefinitions.GOVCBR5DS.Declaration declaration5DS)
			{
				Declaration5DS = declaration5DS;
			}
		}

		CargoWise.Customs.KR.MessageDefinitions.GOVCBR5DR.Declaration Declaration5DR { get; }
		CargoWise.Customs.KR.MessageDefinitions.GOVCBR5DS.Declaration Declaration5DS { get; }

		public override ZString AmendmentType => Declaration5DR?.TransactionNatureCode.Value ?? Declaration5DS?.TransactionNatureCode.Value;
		public override ZString ReasonCode => Declaration5DR?.ReasonCode?.Value ?? Declaration5DS?.ReasonCode?.Value ?? ZString.Empty;
		public override ZString AmendReasonDescription => Declaration5DR?.Reason?.Value ?? Declaration5DS?.Reason?.Value ?? ZString.Empty;
		public ZDateTime SubmissionDate => ZDateTime.TryParseExact(Declaration5DR?.IssueDateTime ?? Declaration5DS?.IssueDateTime, out ZDateTime submissionDate, Constants.DateFormatType.Date) ? submissionDate : ZDateTime.Empty;

		public override ZInt AmendmentVersionNo => throw new System.NotImplementedException();
		public override ZString FaultParty => throw new System.NotImplementedException();
		public override ZString FaultPartyOtherDescription => throw new System.NotImplementedException();
		public override ZString PenaltyPaymentReasonCode => throw new System.NotImplementedException();
		public override ZDate DateOfFinalPrice => throw new System.NotImplementedException();
	}
}
