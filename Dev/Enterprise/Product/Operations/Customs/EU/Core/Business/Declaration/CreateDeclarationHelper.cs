using System.Collections.Generic;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.EU.Business
{
	public class CreateDeclarationHelper : Customs.Business.CreateDeclarationHelper, Integration.Customs.EU.ICreateDeclarationHelper
	{
		protected override ICollection<CreateBrokerageQuestion> GetQuestionsToCreateDeclarationWarningCore()
		{
			return new List<CreateBrokerageQuestion>()
			{
				new CreateBrokerageQuestion()
				{
					Question = NotNominatedCustomsBrokerQuestionText,
					Message = NotNominatedCustomsBrokerMessage,
					ConditionToAsk = delegate(ForwardingShipment shipment) { return shipment.JS_CommunityTransitStatus != ExportCommunityTransitStatusList.Codes.C; }
				},
				new CreateBrokerageQuestion()
				{
					Question = Res.GetString("1a303d2c-634a-4a0b-929d-63c6ba02c7bc", "If the shipment is C-Status, a declaration is not needed. Do you still want to create the declaration?"),
					Message = Res.GetString("c9b6ca04-436d-4ce5-a784-d34cb88620c6", "This shipment is C-Status. A declaration is not needed."),
					ConditionToAsk = delegate(ForwardingShipment shipment) { return shipment.JS_CommunityTransitStatus == ExportCommunityTransitStatusList.Codes.C; }
				}
			};
		}

		public override ImportJobDeclaration GetNewImportJobDeclaration(ForwardingShipment shipment)
		{
			return new ImportDeclaration(shipment.Factory);
		}
	}
}
