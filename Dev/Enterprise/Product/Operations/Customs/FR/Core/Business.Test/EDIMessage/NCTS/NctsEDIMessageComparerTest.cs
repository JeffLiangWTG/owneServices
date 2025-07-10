using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.CusTempStorage.Testing;
using Enterprise.Customs.FR.Business.EdiMessages;
using Enterprise.Customs.FR.Business.MessageProcessors;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.EDIMessages.Testing
{
	sealed class NctsEDIMessageComparerTest : TestCaseWithFactory
	{
		public void TestProcessOrder()
		{
			AssertProcessOrder("1", "CC028C", string.Empty, "CCF02C", "_GARANTIE_SOUS_ENRG", "CC029C", string.Empty);
			AssertProcessOrder("2", "CC028C", string.Empty, "CCF02C", "_GARANTIE_SOUS_ENRG");
			AssertProcessOrder("3", "CCF02C", "_GARANTIE_SOUS_ENRG", "CC029C", string.Empty);
			AssertProcessOrder("4", "CC028C", string.Empty, "CC029C", string.Empty);
			AssertProcessOrder("5", "CC019C", string.Empty, "CCF02C", "_NOTIF_ARRIVEE_DEST", "CC045C", string.Empty);
			AssertProcessOrder("6", "CC019C", string.Empty, "CCF02C", "_NOTIF_ARRIVEE_DEST");
			AssertProcessOrder("7", "CCF02C", "_NOTIF_ARRIVEE_DEST", "CC045C", string.Empty);
			AssertProcessOrder("8", "CC019C", string.Empty, "CC045C", string.Empty);
			AssertProcessOrder("9", "CCF02C", "_DEMANDE_INVALID", "CC009C", string.Empty);
			AssertProcessOrder("10", "CCF02C", "_DEMANDE_RECTIF", "CC029C", string.Empty);
			AssertProcessOrder("11", "CCF02C", "_DEMANDE_RECTIF", "CC004C", string.Empty);
			AssertProcessOrder("12", "CCF02C", "_DEMANDE_RECTIF", "CC022C", string.Empty);
			AssertProcessOrder("13", "CC028C", string.Empty, "CC928C", string.Empty);
			AssertProcessOrder("14", "CCF02C", "_AVIS_ANT_ARRIV_DEM", "CCF03C", string.Empty, "CC025C", string.Empty);
			AssertProcessOrder("15", "CCF02C", "_AVIS_ANT_ARRIV_DEM", "CCF03C", string.Empty);
			AssertProcessOrder("16", "CCF03C", string.Empty, "CC025C", string.Empty);
			AssertProcessOrder("17", "CCF03C", string.Empty, "CC043C", string.Empty);
			AssertProcessOrder("18", "CC182C", string.Empty, "CCF03C", string.Empty, "CC025C", string.Empty);
			AssertProcessOrder("19", "CC182C", string.Empty, "CCF03C", string.Empty);
			AssertProcessOrder("20", "CC182C", string.Empty, "CC025C", string.Empty);
			AssertProcessOrder("21", "CCF02C", "_RECHERCHE_ENGAGEE", "CC035C", string.Empty, "CCF02C", "_INFO_RECOUVREMENT", "CC025C", string.Empty);
			AssertProcessOrder("22", "CCF02C", "_RECHERCHE_ENGAGEE", "CC035C", string.Empty);
			AssertProcessOrder("23", "CC035C", string.Empty, "CCF02C", "_INFO_RECOUVREMENT");
			AssertProcessOrder("24", "CCF02C", "_INFO_RECOUVREMENT", "CC025C", string.Empty);
			AssertProcessOrder("25", "CC035C", string.Empty, "CC025C", string.Empty);
		}

		public void TestEntryStatusAreWellDefined()
		{
			CombineAssertions(() =>
			{
				NctsEDIMessageComparer.EntryStatusOrderList.SelectMany(x => new List<string>
				{
					x.Item1,
					x.Item2
				}).ForEach(x =>
				{
					if (x.StartsWith("F02"))
					{
						AssertStartsWith("If you use F02 to define the order, you must include tag <TransitOperation>.<Statut>.", "F02|", x);
					}

					Assert($"Entry status {x} is not a valid one.", new TP5ResponseMessageSubTypeList().ContainsCode(x.Split('|')[0]));
				});
			});
		}

		void AssertProcessOrder(string interchangeNumberPrefix, string responseType1, string responseFunction1, string responseType2, string responseFunction2, string responseType3 = "", string responseFunction3 = "", string responseType4 = "", string responseFunction4 = "")
		{
			var nctsHeader = GetNCTSHeader();
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN1";
			var entryNumber = CusEntryNumber.New<CusEntryNumber>(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, nctsHeader.CountryCode);
			entryNumber.CE_EntryNum = "MRN1";

			var fileSubPath1 = responseType1.Contains("CC025") ? "CC025C_RI1_" : responseType1;
			var messageText1 = !string.IsNullOrEmpty(responseType1) ? resourceRetriever.Value.GetString($"Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_{fileSubPath1}ResponseMessage{responseFunction1}.xml") : string.Empty;
			var interchange1 = SetupInterchange(interchangeNumberPrefix, responseType1, "120001", messageText1);

			var fileSubPath2 = responseType2.Contains("CC025") ? "CC025C_RI1_" : responseType2;
			var messageText2 = !string.IsNullOrEmpty(responseType2) ? resourceRetriever.Value.GetString($"Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_{fileSubPath2}ResponseMessage{responseFunction2}.xml") : string.Empty;
			var interchange2 = SetupInterchange(interchangeNumberPrefix, responseType2, "120002", messageText2);

			var fileSubPath3 = responseType3.Contains("CC025") ? "CC025C_RI1_" : responseType3;
			var messageText3 = !string.IsNullOrEmpty(responseType3) ? resourceRetriever.Value.GetString($"Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_{fileSubPath3}ResponseMessage{responseFunction3}.xml") : string.Empty;
			var interchange3 = !string.IsNullOrEmpty(responseType3) ? SetupInterchange(interchangeNumberPrefix, responseType3, "120003", messageText3) : SetupInterchange(interchangeNumberPrefix, "Y", "120003", string.Empty);

			var fileSubPath4 = responseType4.Contains("CC025") ? "CC025C_RI1_" : responseType4;
			var messageText4 = !string.IsNullOrEmpty(responseType4) ? resourceRetriever.Value.GetString($"Enterprise.Customs.FR.Business.Testing.EDIMessage.TestFiles.NCTS_{fileSubPath4}ResponseMessage{responseFunction4}.xml") : string.Empty;
			var interchange4 = !string.IsNullOrEmpty(responseType4) ? SetupInterchange(interchangeNumberPrefix, responseType4, "120004", messageText4) : SetupInterchange(interchangeNumberPrefix, "Z", "120004", string.Empty);

			Factory.Save();

			var processor = new TP5IncomingMessageProcessor(new LoggingInformation());
			processor.ProcessInterchangesAndExecuteBatch(new System.Threading.CancellationToken());

			var msg1 = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange1.PK));
			var msg2 = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange2.PK));
			var msg3 = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange3.PK));
			var msg4 = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_EI, interchange4.PK));

			var messages = new List<EDIMessage>() { msg2, msg1 };
			var expectedSortedMessagesInOrder = new string[] { $"{interchangeNumberPrefix}.{responseType1}.120001", $"{interchangeNumberPrefix}.{responseType2}.120002" };

			if (!string.IsNullOrEmpty(responseType3))
			{
				messages = new List<EDIMessage>() { msg3, msg2, msg1 };
				expectedSortedMessagesInOrder = new string[] { $"{interchangeNumberPrefix}.{responseType1}.120001", $"{interchangeNumberPrefix}.{responseType2}.120002", $"{interchangeNumberPrefix}.{responseType3}.120003" };
			}

			if (!string.IsNullOrEmpty(responseType4))
			{
				messages = new List<EDIMessage>() { msg4, msg3, msg2, msg1 };
				expectedSortedMessagesInOrder = new string[] { $"{interchangeNumberPrefix}.{responseType1}.120001", $"{interchangeNumberPrefix}.{responseType2}.120002", $"{interchangeNumberPrefix}.{responseType3}.120003", $"{interchangeNumberPrefix}.{responseType4}.120004" };
			}

			var sortedMessages = NctsEDIMessageComparer.GetSortedMessages(messages, ListSortDirection.Ascending);
			AssertContainsExactElementsInExactOrder(expectedSortedMessagesInOrder, sortedMessages.Select(x => x.EM_InterchangeNumber));
		}

		EDIInterchange SetupInterchange(string interchangeNumberPrefix, string responseType, string responseTime, string messageText)
		{
			var intchg = Factory.NewWithValidTestData<EDIInterchange>();
			intchg.EI_From = "EASYLOG2TEST_EAD";
			intchg.EI_To = "WTLDFRFRM";
			intchg.EI_ApplicationCode = ApplicationCodeList.Codes.GenericMessageDelivery;
			intchg.EI_InterchangeType = GenericMessageDeliveryInterchangeTypeList.Codes.FRCustomsTP5;
			intchg.EI_InterchangeNum = $"{interchangeNumberPrefix}.{responseType}.{responseTime}";
			intchg.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			intchg.EI_Status = EDIInterchange.Status.Queued;
			intchg.EI_IsActive = true;
			intchg.EI_GB = GlbBranch.CurrentBranch.PK;
			intchg.EI_BodyText = messageText;
			return intchg;
		}

		NctsHeader GetNCTSHeader()
		{
			var nctsHeader = Factory.New<DummyNctsHeader_TestNewFromDeltaT>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			return nctsHeader;
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
	}
}
