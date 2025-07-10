using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageDefinitions.ProductCatalog;
using Enterprise.Customs.BR.Registry;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.Messaging.InterchangeProviders.Testing;

namespace Enterprise.Customs.BR.Business.Testing
{
	class BRBatchInterchangeProviderTest : InterchangeProviderTestCase
	{
		protected override InterchangeProviderBase GetInterchangeProvider(NonDependentEDIMessageCollection collection)
		{
			return new BRBatchInterchangeProvider(collection);
		}

		public override void TestMessagesPopulateNewInterchange()
		{
			var productJsonMessage = BRInterchangeProviderTest.GetProductJsonMessage("95041055050");

			var message1 = BRInterchangeProviderTest.CreateTransmitMessage(Factory, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Original, messageBody: productJsonMessage);
			var message2 = BRInterchangeProviderTest.CreateTransmitMessage(Factory, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Original, messageBody: productJsonMessage);
			var message3 = BRInterchangeProviderTest.CreateTransmitMessage(Factory, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.CatalogZipFile, messageBody: "");
			var message4 = BRInterchangeProviderTest.CreateTransmitMessage(Factory, MessageTypeList.Codes.OPE, EDIMessageSubTypeList.Codes.Original, messageBody: "");
			var message5 = BRInterchangeProviderTest.CreateTransmitMessage(Factory, MessageTypeList.Codes.OPE, EDIMessageSubTypeList.Codes.Original, messageBody: "");

			var messages = new NonDependentEDIMessageCollection(Factory);
			messages.AddRange(new BREDIMessage[] { message1, message2, message3, message4, message5 });

			var provider = new BRBatchInterchangeProvider(messages);
			var interchanges = provider.Interchanges;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Number Of Interchanges", 3, interchanges.Length);
				AssertEquals("CAT ORI", message1.Interchange.PK, message2.Interchange.PK);
				AssertNotEquals("CAT CZP", message1.Interchange.PK, message3.Interchange.PK);
				AssertNotEquals("CAT CZP", message1.Interchange.PK, message4.Interchange.PK);
				AssertEquals("OPE ORI", message4.Interchange.PK, message5.Interchange.PK);

				AssertEquals("message1.EM_MessageNum", "1", message1.EM_MessageNum);
				AssertEquals("message2.EM_MessageNum", "2", message2.EM_MessageNum);
				AssertEquals("message3.EM_MessageNum", "1", message3.EM_MessageNum);
				AssertEquals("message4.EM_MessageNum", "1", message4.EM_MessageNum);
				AssertEquals("message5.EM_MessageNum", "2", message5.EM_MessageNum);
			});
		}

		public void TestPopulateInterchangeMultipleMessages_CAT_ORI()
		{
			var productJsonMessage = BRInterchangeProviderTest.GetProductJsonMessage("95041055");

			using (BRCustomsDataRegistry.Instance.MaxNumberOfRowsInProductCatalogMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
			{
				AssertNumberOfInterchanges(0, 0);
				AssertNumberOfInterchanges(1, 1);
				AssertNumberOfInterchanges(2, 1);
				AssertNumberOfInterchanges(5, 2);

				void AssertNumberOfInterchanges(int numberOfMessagesToCreate, int expectedNumberOfInterchanges)
				{
					var ediMessageList = new List<BREDIMessage>();
					for (var i = 0; i < numberOfMessagesToCreate; i++)
					{
						ediMessageList.Add(BRInterchangeProviderTest.CreateTransmitMessage(Factory, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Original, "21BR0000022649", productJsonMessage));
					}
					var messagesCollection = new NonDependentEDIMessageCollection(Factory);
					messagesCollection.AddRange(ediMessageList.ToArray());
					var provider = new BRBatchInterchangeProvider(messagesCollection);
					var interchanges = provider.Interchanges;
					AssertEquals("Number Of Interchanges", expectedNumberOfInterchanges, interchanges.Length);
					foreach (var interchange in interchanges)
					{
						AssertLessThanOrEqualTo("Number Of Messages in one Interchange", interchange.ContainedMessages.Count, 3);
						AssertEquals("EI_BodyText", interchange.ContainedMessages.Count, JsonSerializer.Deserialize<ProdutoIntegracaoDTO[]>(interchange.EI_BodyText).Length);

						foreach (BREDIMessage message in interchange.ContainedMessages)
						{
							AssertEquals((interchange.ContainedMessages.IndexOf(x => x == message) + 1).ToString(), message.EM_MessageNum);
							AssertContains(message.EM_MessageText.Replace("\"seq\": 1,", $"\"seq\": {message.EM_MessageNum},").Trim(), interchange.EI_BodyText);
						}
					}
				}
			}
		}

		public void TestPopulateInterchangeMultipleMessages_CAT_LIN()
		{
			var productLinkJsonMessage = BRInterchangeProviderTest.GetProductLinkJsonMessage("95041055");

			BREDIMessage CreateProductLinksMessage(int countOfLink)
			{
				var messageBody = $"[{string.Join(",", new int[countOfLink].Select((x, i) => productLinkJsonMessage))}]";
				return BRInterchangeProviderTest.CreateTransmitMessage(Factory, MessageTypeList.Codes.CAT, EDIMessageSubTypeList.Codes.Link, "95041055", messageBody: messageBody);
			}

			using (BRCustomsDataRegistry.Instance.MaxNumberOfRowsInProductCatalogMessage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				var message1 = CreateProductLinksMessage(2);
				var message2 = CreateProductLinksMessage(1);
				var message3 = CreateProductLinksMessage(5);
				var message4 = CreateProductLinksMessage(2);
				var message5 = CreateProductLinksMessage(3);

				var messagesCollection = new NonDependentEDIMessageCollection(Factory);
				messagesCollection.AddRange(new [] { message1, message2, message3, message4, message5 });
				var provider = new BRBatchInterchangeProvider(messagesCollection);
				var interchanges = provider.Interchanges;
				AssertEquals("Number Of Interchanges", 3, interchanges.Length);
				AssertInterchangeCreated(interchanges[0], 3, message1, message2);
				AssertInterchangeCreated(interchanges[1], 5, message3);
				AssertInterchangeCreated(interchanges[2], 5, message4, message5);

				void AssertInterchangeCreated(EDIInterchange interchange, int countOfLink, params EDIMessage[] containsMessages)
				{
					AssertContainsExactElementsInAnyOrder(containsMessages, interchange.ContainedMessages);

					var links = JsonSerializer.Deserialize<FabricanteIntegracaoDTO[]>(interchange.EI_BodyText);
					AssertEquals("EI_BodyText", countOfLink, links.Length);

					var expectedSeqs = new int[countOfLink].Select((x, i) => i + 1);
					AssertContainsExactElementsInExactOrder(expectedSeqs, links.Select(x => x.seq));

					foreach (BREDIMessage message in interchange.ContainedMessages)
					{
						AssertEquals((interchange.ContainedMessages.IndexOf(x => x == message) + 1).ToString(), message.EM_MessageNum);
					}
				}
			}
		}
	}
}
