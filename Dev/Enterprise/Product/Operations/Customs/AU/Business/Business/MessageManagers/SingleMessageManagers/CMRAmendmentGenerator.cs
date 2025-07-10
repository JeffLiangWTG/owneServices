using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CMRAmendmentGenerator : AmendmentMessagesGenerator
	{
		public CMRAmendmentGenerator(BusinessObject businessObject)
			: base(businessObject)
		{
		}

		#region Overridden Methods

		protected override EDIMessage GenerateAmendmentMessageCore()
		{
			return GetAmendmentBuilder().PopulateMessagesReturningResult();
		}

		protected override EDIMessage GenerateOriginalMessageCore()
		{
			return GetOriginalBuilder().PopulateMessagesReturningResult();
		}

		protected override EDIMessage GenerateWithdrawalMessageCore(BusinessObject bizo, BusinessObject bizoInNewFactory)
		{
			return GetWithdrawalBuilder(bizo, bizoInNewFactory).PopulateMessagesReturningResult();
		}

		#endregion

		#region Abstract Methods

		protected internal abstract CMRMessageBuilder GetBuilder(BusinessObject bizo);
		protected internal abstract EDIMessageCollection GetMesssageCollection(BusinessObject bizo);

		#endregion

		#region Implementation

		CMRMessageBuilder GetAmendmentBuilder()
		{
			CMRMessageBuilder builder = GetBuilder(businessObject);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Change;
			builder.Messages = GetMesssageCollection(businessObject);
			return builder;
		}

		CMRMessageBuilder GetOriginalBuilder()
		{
			CMRMessageBuilder builder = GetBuilder(businessObject);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			builder.Messages = GetMesssageCollection(businessObject);
			return builder;
		}

		CMRMessageBuilder GetWithdrawalBuilder(BusinessObject bizo, BusinessObject bizoInNewFactory)
		{
			CMRMessageBuilder builder = GetBuilder(bizoInNewFactory);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Withdraw;
			builder.Messages = GetMesssageCollection(bizo);
			return builder;
		}

		#endregion
	}
}
