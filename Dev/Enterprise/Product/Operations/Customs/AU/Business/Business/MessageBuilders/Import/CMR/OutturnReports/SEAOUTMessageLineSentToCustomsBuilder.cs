using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SEAOUTMessageLineSentToCustomsBuilder
	{
		public SEAOUTMessageLineSentToCustomsBuilder(IEDIMessageCollectionProvider provider)
		{
			this.provider = provider;
		}
		readonly IEDIMessageCollectionProvider provider;

		BusinessObjectFactory Factory
		{
			get { return provider.Factory; }
		}

		public SEAOUTMessageLineSentToCustoms[] LineCollection
		{
			get
			{
				if (fLineCollection == null)
				{
					fLineCollection = new Dictionary<string, SEAOUTMessageLineSentToCustoms>();
					var query = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, ((BusinessObject)provider).PK);
					query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
					query.AddToFilter(EDIMessageSchema.EM_MessageType, CMRMessage.CMRMessageTypes.SEAOUT);
					query.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc;
					var sutResponseMessages = Factory.Load<CMRSEAOUTRMessage>(query);
					provider.Messages.Load();
					foreach (var sutResponseMessage in sutResponseMessages)
					{
						if (!sutResponseMessage.IsFullyRejected)
						{
							var outgoingMsg = sutResponseMessage.GetOutgoingMessageByBGMRefAndVersion(provider.Messages, CMRMessage.CMRMessageTypes.SEAOUT);
							var sutOutgoingMsg = outgoingMsg != null ? Factory.Load<CMRSEAOUTMessage>(outgoingMsg.PK) : null;
							if (sutOutgoingMsg != null)
							{
								if (sutOutgoingMsg.EM_MessageSubType == CMRMessage.ManifestResponseSubTypes.Withdrawn)
								{
									fLineCollection = new Dictionary<string, SEAOUTMessageLineSentToCustoms>();
								}
								else
								{
									foreach (var line in sutOutgoingMsg.LineCollection)
									{
										if (sutResponseMessage.IsFullyAccepted || sutResponseMessage.MatchingLine(line.ContainerNo, line.OceanBill, line.HouseBill) == null)
										{
											var lineActionCode = line.LineAction;
											if (lineActionCode == LineAction.Insert && !fLineCollection.ContainsKey(line.UniqueIdentifier))
											{
												fLineCollection.Add(line.UniqueIdentifier, line);
											}
											else if (lineActionCode == LineAction.Delete && fLineCollection.ContainsKey(line.UniqueIdentifier))
											{
												fLineCollection.Remove(line.UniqueIdentifier);
											}
											else if (lineActionCode == LineAction.Amend && fLineCollection.ContainsKey(line.UniqueIdentifier))
											{
												fLineCollection.Remove(line.UniqueIdentifier);
												fLineCollection.Add(line.UniqueIdentifier, line);
											}
										}
									}
								}
							}
						}
					}
				}

				return fLineCollection.Values.ToArray();
			}
		}

		Dictionary<string, SEAOUTMessageLineSentToCustoms> fLineCollection;
	}
}
