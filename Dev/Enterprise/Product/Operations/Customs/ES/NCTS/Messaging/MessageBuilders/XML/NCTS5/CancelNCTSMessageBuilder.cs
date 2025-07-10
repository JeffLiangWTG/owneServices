using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC014C_v515.CC014CV1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
	public class CancelNCTSMessageBuilder : NCTSCommonMessageBuilder<ICancelNCTSMessageDataProvider, Cc014Cv1Ent>
	{
		public CancelNCTSMessageBuilder(ICancelNCTSMessageDataProvider provider, ZString messageType, ZString messageSubType)
			: base(provider, messageType, messageSubType)
		{
		}

		protected override ZString GetMessageType() => "CC014C";

		protected override Cc014Cv1Ent GenerateXMLMessage()
		{
			var declaration = GetPopulatedTransactionId<Cc014Cv1Ent>();
			if (declaration != null)
			{
				declaration.Cc014C = GetPopulatedCC014CType();
			}
			return declaration;
		}

		Cc014CType GetPopulatedCC014CType()
		{
			var cC014Cv515 = GetPopulatedMessage<Cc014CType>();
			if (cC014Cv515 != null)
			{
				cC014Cv515.TransitOperation = GetPopulatedCommonTransitOperationMRN<TransitOperationType05>(provider.TransitOperation);
				cC014Cv515.Invalidation = GetPopulatedInvalidation();
				cC014Cv515.CustomsOfficeOfDeparture = GetPopulatedCustomOffice<CustomsOfficeOfDepartureType03>(provider.CustomsOfficeOfDeparture);
				cC014Cv515.HolderOfTheTransitProcedure = GetPopulatedCommonHolderOfTheTransitProcedure<HolderOfTheTransitProcedureType02>(provider.HolderOfTheTransitProcedure);
			}
			return cC014Cv515;
		}

		InvalidationType02 GetPopulatedInvalidation()
		{
			var invalidation = provider.Invalidation;
			return invalidation == null ? null : new InvalidationType02
			{
				InitiatedByCustoms = invalidation.IsInitiatedByCustoms ? Flag.Item1 : Flag.Item0,
				Justification = invalidation.Justification,
			};
		}
	}
}
