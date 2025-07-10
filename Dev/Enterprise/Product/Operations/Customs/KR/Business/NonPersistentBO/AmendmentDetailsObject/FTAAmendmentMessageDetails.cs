using CargoWise.Types;

namespace Enterprise.Customs.KR.Business
{
	public class FTAAmendmentMessageDetails : BaseAmendmentMessageDetails
	{
		public FTAAmendmentMessageDetails(CargoWise.Customs.KR.MessageDefinitions.GOVCBR105.Declaration messageDeclaration)
		{
			Populate(null, messageDeclaration);
		}
		public FTAAmendmentMessageDetails(CargoWise.Customs.KR.MessageDefinitions.GOVCBRDHS.Declaration messageDeclaration)
		{
			Populate(messageDeclaration, null);
		}

		public override ZInt AmendmentVersionNo => VersionNumber;

		public override ZString AmendmentType => Type;

		public override ZString AmendReasonDescription => Description;

		public override ZString ReasonCode => throw new System.NotImplementedException();

		public override ZString FaultParty => throw new System.NotImplementedException();

		public override ZString FaultPartyOtherDescription => throw new System.NotImplementedException();

		public override ZString PenaltyPaymentReasonCode => throw new System.NotImplementedException();

		public override ZDate DateOfFinalPrice => throw new System.NotImplementedException();

		void Populate(CargoWise.Customs.KR.MessageDefinitions.GOVCBRDHS.Declaration declarationDHS, CargoWise.Customs.KR.MessageDefinitions.GOVCBR105.Declaration declaration105)
		{
			if (declarationDHS != null)
			{
				VersionNumber = ZInt.Parse(declarationDHS.VersionId.Value);
				Type = declarationDHS.AdditionalInformation?.StatementCode?.Value ?? ZString.Empty;
				Description = declarationDHS.Reason.Value;
			}
			else
			{
				VersionNumber = ZInt.Parse(declaration105.VersionId.Value);
				Type = declaration105.AdditionalInformation.StatementCode.Value;
				Description = declaration105.Reason.Value;
			}
		}
		ZInt VersionNumber { get; set; }
		ZString Type { get; set; }
		ZString Description { get; set; }
	}
}
