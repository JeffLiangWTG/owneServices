using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DEPARTMessageBuilderTest : TestCaseWithFactory
	{
		public void TestBuildOriginalMessage()
		{
			consol = GetTestConsol();
			var expectedMessage = new EmbeddedResourceRetriever().GetString(GetEmbeddedResourcePath("OriginalDepartureReport.txt")).Replace("\r\n", "");
			var builder = new DEPARTMessageBuilder(consol);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertEquals("MessageText", expectedMessage, builder.MessageText);
		}

		public void TestBuildOriginalMessageFromExportCustomsManifestHeader()
		{
			header = GetTestExportCustomsManifestHeader();
			var expectedMessage = new EmbeddedResourceRetriever().GetString(GetEmbeddedResourcePath("OriginalDepartureReport.txt")).Replace("\r\n", "");
			var builder = new DEPARTMessageBuilder(header);
			builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
			AssertEquals("MessageText", expectedMessage, builder.MessageText);
		}

		public void TestStripSpacesOutOfABN()
		{
			var oldRegNo = GlbCompany.CurrentCompany.OrgProxy.LocalBusinessRegNo;
			try
			{
				GlbCompany.CurrentCompany.OrgProxy.LocalBusinessRegNo = "123 456 789 01";
				header = GetTestExportCustomsManifestHeader();
				header.ED_TransportMode = Core.Constants.TransportModes.Sea;
				var builder = new DEPARTMessageBuilder(header);
				builder.MessageSubType = Common.MessageBuilders.MessageSubTypes.Create;
				Assert("MessageTextStippedOfSpaces", builder.MessageText.Contains("12345678901"));
			}
			finally
			{
				GlbCompany.CurrentCompany.OrgProxy.LocalBusinessRegNo = oldRegNo;
			}
		}

		ExportCustomsManifestHeader GetTestExportCustomsManifestHeader()
		{
			var result = Factory.New<ExportCustomsManifestHeader>();
			result.ED_TransportMode = Core.Constants.TransportModes.Air;
			result.ED_FlightNumber = "QF608";
			result.ED_RL_NKPortOfDestination = "NZAKL";
			result.ED_DepartureDate = new ZDateTime(2004, 5, 12, 16, 50, 0);
			result.ED_BGMReference = "C00001509";
			SetCTOCode(result, "8553P");
			return result;
		}

		ForwardingConsol GetTestConsol()
		{
			var result = Factory.New<ForwardingConsol>();
			result.JK_TransportMode = Core.Constants.TransportModes.Air;

			var transport = result.Transports[0];
			transport.JW_VoyageFlight = "QF608";
			transport.JW_RL_NKDiscPort = "NZAKL";
			transport.JW_ATD = new ZDateTime(2004, 5, 12, 16, 50, 0);
			result.JK_UniqueConsignRef = "C00001509";
			SetCTOCode(result, "8553P");
			return result;
		}

		void SetCTOCode(ExportCustomsManifestHeader header, ZString cTOCode)
		{
			var cTOHeader = header.Factory.New<OrgHeader>();
			header.ED_OA_CTOAddress = cTOHeader.MainAddress.PK;
			cTOHeader.MainAddress.LocalControlledPremisesID = cTOCode;
		}

		void SetCTOCode(ForwardingConsol consol, ZString cTOCode)
		{
			var cTOAddress = consol.Factory.New<OrgAddress>();
			consol.JK_OA_DepartureCTOAddress = cTOAddress.PK;
			var cTOHeader = consol.Factory.New<OrgHeader>();
			cTOAddress.OA_OH = cTOHeader.PK;
			cTOHeader.SetLocalCustomsCode(OrgCusCode.CodeTypes.ControlledPremisesID, cTOCode);
		}

		string GetEmbeddedResourcePath(string fileName) => "Enterprise.Customs.AU.Declaration.Business.Testing.MessageBuilders.Export.DEPART.TestFiles." + fileName;

		ExportCustomsManifestHeader header;
		ForwardingConsol consol;
	}
}
