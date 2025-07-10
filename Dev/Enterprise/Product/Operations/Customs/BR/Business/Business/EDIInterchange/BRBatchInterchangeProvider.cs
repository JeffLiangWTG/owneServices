using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageDefinitions.ProductCatalog;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.BR.Business
{
	public class BRBatchInterchangeProvider : BRInterchangeProvider
	{
		public BRBatchInterchangeProvider(NonDependentEDIMessageCollection messageCollection)
			: base(messageCollection)
		{
		}

		protected override string GetCollationKey(EDIMessage message) => message.EM_MessageType + message.EM_MessageSubType;

		protected override void BuildInterchangeBatchesCore(NonDependentEDIMessageCollection messages)
		{
			foreach (var collatedMessage in Collate(messages))
			{
				if (collatedMessage.All(x => x is BREDIMessage message && message.IsProductMessage))
				{
					AddMessageCollection(CollateProductMessages(collatedMessage).ToArray());
				}
				else if (collatedMessage.All(x => x is BREDIMessage message && message.IsProductLinkMessage))
				{
					AddMessageCollection(CollateProductLinkMessages(collatedMessage).ToArray());
				}
				else
				{
					AddMessageCollection(collatedMessage);
				}
			}
		}

		IEnumerable<NonDependentEDIMessageCollection> CollateProductMessages(NonDependentEDIMessageCollection messages)
		{
			foreach (var chunkedMessages in messages.Batch(BRCustomsDataRegistry.Instance.MaxNumberOfRowsInProductCatalogMessage.Value))
			{
				var collection = new NonDependentEDIMessageCollection(messages.Factory);
				collection.AddRange(chunkedMessages);
				yield return collection;
			}
		}

		IEnumerable<NonDependentEDIMessageCollection> CollateProductLinkMessages(NonDependentEDIMessageCollection messages)
		{
			var maxNumberOfRows = BRCustomsDataRegistry.Instance.MaxNumberOfRowsInProductCatalogMessage.Value;
			var collection = new NonDependentEDIMessageCollection(messages.Factory);
			var totalCountInCollection = 0;

			foreach (EDIMessage message in messages)
			{
				var countInMessage = BRMessageHelper.DeserializeObject<FabricanteIntegracaoDTO[]>(message.EM_MessageText).Length;
				totalCountInCollection += countInMessage;
				if (totalCountInCollection > maxNumberOfRows)
				{
					yield return collection;

					collection = new NonDependentEDIMessageCollection(messages.Factory);
					totalCountInCollection = countInMessage;
				}

				collection.Add(message);
			}

			yield return collection;
		}

		protected override void PopulateInterchange(NonDependentEDIMessageCollection messages, EDIInterchange interchange)
		{
			base.PopulateInterchange(messages, interchange);

			var seq = 1;
			messages.Cast<EDIMessage>().ForEach(x => x.EM_MessageNum = seq++.ToString());
		}
	}
}
