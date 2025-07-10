using System;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EMMMessageBuilderTest : ManifestMessageBuilderAbstractTest
	{
		public void TestSimpleEMMMessage()
		{
			var testConsol = CreateSimpleEMMConsol();
			var builder = new EMMMessageBuilder(testConsol);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertEquals("NumberOfMessagesGenerated", 1, builder.GeneratedMessageStrings.Length);
			var expectedString = new EmbeddedResourceRetriever().GetString(GetEmbeddedResourcePath("SimpleEMM.txt"));
			AssertMultilineASCIIEquals("MessageText", expectedString, builder.GeneratedMessageStrings[0].Replace("'", "'\r\n"));
		}

		public void TestIncludeEMMInReplacementMessage()
		{
			var testConsol = CreateSimpleEMMConsol();
			var wrapper = new FreightConsolWrapper(testConsol);
			var entryNum = wrapper.CreateMainManifestNumber();
			entryNum.CE_EntryNum = "123456";
			var builder = new EMMMessageBuilder(testConsol);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Replace;
			AssertEquals("NumberOfMessagesGenerated", 1, builder.GeneratedMessageStrings.Length);
			Assert("MMN Included", builder.GeneratedMessageStrings[0].IndexOf(entryNum.CE_EntryNum) != -1);
		}

		public void TestSimpleEMMMessageStandalone()
		{
			var header = CreateSimpleEMMHeader();
			var builder = new EMMMessageBuilder(header);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertEquals("NumberOfMessagesGenerated", 1, builder.GeneratedMessageStrings.Length);
			var expectedString = new EmbeddedResourceRetriever().GetString(GetEmbeddedResourcePath("SimpleEMM.txt"));
			AssertMultilineASCIIEquals("MessageText", expectedString, builder.GeneratedMessageStrings[0].Replace("'", "'\r\n"));
		}

		public void TestCanSaveStandaloneMessage()
		{
			TestSimpleEMMMessageStandalone();
			Factory.Save();
		}

		protected override IManifestMessageBuilder NewMessageBuilder(Common.MessageBuilders.MessageSubTypes messageSubType, object data)
		{
			var builder = new EMMMessageBuilder((ForwardingConsol)data);
			builder.MessageSubType = messageSubType;
			return builder;
		}

		string GetEmbeddedResourcePath(string fileName) => "Enterprise.Customs.AU.Declaration.Business.Testing.MessageBuilders.Export.Manifest.TestFiles." + fileName;

		ForwardingConsol CreateSimpleEMMConsol()
		{
			var result = Factory.New<ForwardingConsol>();
			result.JK_UniqueConsignRef = "C00001277";
			result.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			result.JK_RL_NKDischargePort = "NZAKL";
			result.JK_RL_NKLoadPort = "AUSYD";

			var transport = result.Transports[0];
			EnsureQantasAirLineCodeSet();
			transport.JW_VoyageFlight = "QF303";
			result.JK_MasterBillNum = "081-00000011";

			transport.JW_ATD = new ZDateTime(new DateTime(2004, 1, 28));
			return result;
		}

		ExportCustomsManifestHeader CreateSimpleEMMHeader()
		{
			var result = Factory.New<ExportCustomsManifestHeader>();
			result.ED_BGMReference = "C00001277";
			result.ED_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			result.ED_RN_NKCountryOfDestination = "NZ";
			result.ED_RL_NKPortOfDeparture = "AUSYD";
			result.ED_FlightNumber = "QF303";
			result.ED_AirWayBill = "08100000011";

			result.ED_DepartureDate = new ZDateTime(new DateTime(2004, 1, 28));
			return result;
		}
	}
}
