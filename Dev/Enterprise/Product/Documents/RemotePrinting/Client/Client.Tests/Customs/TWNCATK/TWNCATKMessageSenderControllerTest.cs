using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace Enterprise.RemotePrinting.Client.Tests
{
	sealed class TWNCATKMessageSenderControllerForTesting : TWNCATKMessageSenderController
	{
		public TWNCATKMessageSenderControllerForTesting(string localMachineName, CancellationToken cancellationToken) : base(localMachineName, cancellationToken)
		{
		}

		protected override void InitialiseWebServiceClient()
		{
			if (FakeConfigSetting)
			{
				OnShowInformation("TWNCATKMessageSenderControllerForTesting.InitialiseWebServiceClient()");
			}
			else
			{
				base.InitialiseWebServiceClient();
			}
		}
		internal bool FakeConfigSetting { get; set; } = true;

		protected override void Process()
		{
			ProcessCore(SettingManager.CurrentSetting);
		}

		protected override CustomseHubClientSettingManager CreateNewSettingManager(string machineName)
		{
			if (string.IsNullOrWhiteSpace(ConfigName))
			{
				#pragma warning disable CA2208
				throw new ArgumentNullException(nameof(ConfigName), "CreateNewSettingManager should not be called when not have a valid ConfigName.");
				#pragma warning restore CA2208
			}
			TWNCATKSettingManagerForTesting.TextForRetrieveStream = TextForRetrieveStreamForWebClient;
			var result = new TWNCATKSettingManagerForTesting(machineName, TWNCATKTestHelper.CreateFakeWebClient());
			return result;
		}

		protected override WebClientConfiguration GetNewConfigSetting(string configName) => FakeConfigSetting
			? new WebClientConfiguration { WebServiceUrl = $"http://web.client.config.setting/{configName}" }
			: base.GetNewConfigSetting(configName);

		internal WebClientConfiguration ConfigSettingForTesting => ConfigSetting;

		public static string TextForRetrieveStreamForWebClient { get => fTextForRetrieveStreamForWebClient; set => fTextForRetrieveStreamForWebClient = value; }

		[ThreadStatic]
		static string fTextForRetrieveStreamForWebClient;
	}

	public class TWNCATKMessageSenderControllerTest : TestCase
	{
		TWNCATKMessageSenderController TestController;
		StringBuilder Logger;

		const string inputXml = @"{0}
<Declaration xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
	xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
	xmlns=""urn:wco:datamodel:TW:N5203:R-00-05"">
	<AcceptanceDateTime>2020-04-09</AcceptanceDateTime>
	<FunctionCode>9</FunctionCode>
	<ID>AA  0911100004</ID>
	<InvoiceAmount>10000</InvoiceAmount>
	<TotalGrossMassMeasure>0</TotalGrossMassMeasure>
	<TotalPackageQuantity>0</TotalPackageQuantity>
	<TypeCode>G5</TypeCode>
	<Agent>
		<ID>111</ID>
		<RoleCode>CB</RoleCode>
		<tw_SubBoxID>1</tw_SubBoxID>
	</Agent>
	<BorderTransportMeans>
		<TypeCode>1</TypeCode>
	</BorderTransportMeans>
	<CurrencyExchange>
		<CurrencyTypeCode>TWD</CurrencyTypeCode>
		<RateNumeric>1</RateNumeric>
	</CurrencyExchange>
	<DutyTaxFee>
		<tw_DutyMethodCode>1</tw_DutyMethodCode>
	</DutyTaxFee>
	<GoodsShipment>
		<tw_ItemChargeAmount>10011</tw_ItemChargeAmount>
		<Buyer>
			<ID>C0CSPY</ID>
			<Name>C10 COMMUNICATIONS PTY LTD</Name>
			<Address>
				<CountryCode>ZA</CountryCode>
				<Line>109 ANDRE GREYVENSTEIN ROADDDDEEE PARK &amp; PRINCES RDS EAST JOHANNESBURG SOUTH AFRICA 2143</Line>
			</Address>
		</Buyer>
		<Consignment>
			<BorderTransportMeans>
				<JourneyID />
			</BorderTransportMeans>
			<DepartureTransportMeans>
				<TypeCode>12</TypeCode>
			</DepartureTransportMeans>
			<GoodsLocation>
				<ID>2</ID>
			</GoodsLocation>
			<GoodsLocation>
				<ID>017A1130</ID>
			</GoodsLocation>
			<LoadingLocation>
				<ID>AUSYD</ID>
			</LoadingLocation>
			<TransportContractDocument>
				<ID>NIL</ID>
				<TypeCode>704</TypeCode>
			</TransportContractDocument>
			<tw_BondedGoods>
				<tw_DocumentCode>N</tw_DocumentCode>
				<tw_Refundable>N</tw_Refundable>
			</tw_BondedGoods>
			<UnloadingLocation>
				<ID>ZAJNB</ID>
			</UnloadingLocation>
		</Consignment>
		<CustomsValuation>
			<ExitToEntryChargeAmount>100</ExitToEntryChargeAmount>
			<tw_OtherChargeAmount>11</tw_OtherChargeAmount>
		</CustomsValuation>
		<Exporter>
			<ID />
			<Name>A1 CHEMICALS PTY LTD</Name>
			<tw_TypeCode />
			<Address>
				<Line>19 HOMEDALE ROAD TEST BANKSTOWN NEW SOUTH WALES AUSTRALIA 2214</Line>
			</Address>
		</Exporter>
		<GovernmentAgencyGoodsItem>
			<SequenceNumeric>1</SequenceNumeric>
			<Commodity>
				<Description />
				<Name>NO BRAND</Name>
				<Classification>
					<ID>39269090908</ID>
					<IdentificationTypeCode>HS</IdentificationTypeCode>
				</Classification>
				<InvoiceLine>
					<ItemChargeAmount>10011</ItemChargeAmount>
					<tw_UnitPriceAmount>0</tw_UnitPriceAmount>
				</InvoiceLine>
			</Commodity>
			<GoodsMeasure>
				<NetWeightMeasure>0</NetWeightMeasure>
				<TariffQuantity>0</TariffQuantity>
				<tw_UnitCode />
			</GoodsMeasure>
			<GovernmentProcedure>
				<CurrentCode />
			</GovernmentProcedure>
		</GovernmentAgencyGoodsItem>
		<TradeTerms>
			<ConditionCode>EXW</ConditionCode>
		</TradeTerms>
	</GoodsShipment>
	<Packaging>
		<MarksNumbers>N/M</MarksNumbers>
		<TypeCode>CTN</TypeCode>
	</Packaging>
</Declaration>";

		const string defaultEncoding = @"<?xml version=""1.0"" encoding=""utf-8""?>";

		public void TestProcess()
		{
			AssertProcess(@"<?xml version=""1.0""?>");
			AssertProcess(@"<?xml version=""1.0"" encoding=""utf-8""?>");
			AssertProcess(@"<?xml version=""1.0"" encoding=""ISO-8859-1""?>");
			AssertProcess("");
		}

		void AssertProcess(string encoding)
		{
			Logger = new StringBuilder();
			TWNCATKMessageSenderControllerForTesting.TextForRetrieveStreamForWebClient = string.Format(CultureInfo.InvariantCulture, inputXml, encoding);
			TestController = new TWNCATKMessageSenderControllerForTesting(TWNCATKSettingManagerForTesting.CorrectMachineName, cts.Token) { ConfigName = "DEFAULT" };
			TestController.ShowInformation += (s, e) => Logger.AppendLine(e.Message);

			TestController.Run();
			TestController.Stop();
			var outputLog = Logger.ToString();
			Assert(outputLog.Contains("Message with tracking ID: 00000000-0000-0000-0000-000000000000 finalised."));
			var outputPath = Path.Combine(CustomseHubTestHelper.TestFolder, "TWNCATK", "SendToFolder", CustomseHubServiceClientProxyForTesting.Test_RetrievedFile);
			var outputText = File.ReadAllText(outputPath, Encoding.UTF8);
			CustomseHubTestHelper.AssertTextEqualsIgnoreXmlFormats("", string.Format(CultureInfo.InvariantCulture, inputXml, defaultEncoding), outputText);
			AssertContains(defaultEncoding, outputText);
		}

		public void TestInitialiseWebServiceClientErrorHandling()
		{
			var controller = new TWNCATKMessageSenderControllerForTesting(TWNCATKSettingManagerForTesting.CorrectMachineName, cts.Token) { FakeConfigSetting = false };
			var logger = new StringBuilder();
			controller.ShowInformation += (s, e) => logger.AppendLine(e.Message);

			try
			{
				controller.Run();
				controller.Stop();
			}
			catch (Exception) { }

			AssertContains("Should recorded logs when InitialiseWebServiceClient throws exception.", "Initializing Web Service Client failed: ", logger.ToString());
		}

		public void TestRefreshingConfigSettingWhenConfigNameChanged()
		{
			var controller = new TWNCATKMessageSenderControllerForTesting(TWNCATKSettingManagerForTesting.CorrectMachineName, cts.Token);
			controller.ConfigName = "config1";
			AssertEquals("To make sure ConfigSetting has value.", "http://web.client.config.setting/config1", controller.ConfigSettingForTesting.WebServiceUrl);

			controller.ConfigName = "config2";
			AssertEquals("Should refresh ConfigSetting when ConfigName changed.", "http://web.client.config.setting/config2", controller.ConfigSettingForTesting.WebServiceUrl);
		}

		CancellationTokenSource cts;

		protected override void SetUp()
		{
			base.SetUp();
			cts = new CancellationTokenSource();
			CustomseHubTestHelper.CreateTestFolders("TWNCATK", new List<string> { "SendToFolder" });
		}

		protected override void TearDown()
		{
			try
			{
				cts.Cancel();
			}
			finally
			{
				cts.Dispose();
			}

			CustomseHubTestHelper.DeleteTestFolders("TWNCATK");
			base.TearDown();
		}
	}
}
