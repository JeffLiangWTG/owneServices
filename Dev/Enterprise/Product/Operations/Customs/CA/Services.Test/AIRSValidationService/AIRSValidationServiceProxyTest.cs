using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CA.Services.AIRSValidationService;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.CA.Services.Testing
{
	sealed class AIRSValidationServiceProxyTest : TestCaseWithFactory
	{
		class AIRSValidationServiceProxyForHandleUnobservedExceptionTest : AIRSValidationServiceProxy
		{
			public AIRSValidationServiceProxyForHandleUnobservedExceptionTest(IAIRSValidationServiceSettings settings) : base(settings)
			{
			}

			public ValidateTransactionResult ValidateTransactionResult => validateTransactionResult ?? (validateTransactionResult = new ValidateTransactionResult());
			ValidateTransactionResult validateTransactionResult;

			protected override Task<byte[]> GetValidationTask(BrokerValidationServiceClient bvsClient, byte[] xmlContent)
			{
				return Task.Run(() =>
				{
					if (ValidateTransactionResult != null)
					{
						ValidateTransactionResult.IsSpecified = true;
						throw new Exception("Unobserved exception.");
					}
					return ValidateTransactionResult.Serialize();
				});
			}

			protected override void HandleExceptions(Exception e, IValidateTransaction requestObject, IEnumerable<IAIRSValidationQueriedLine> lines)
			{
				ValidateTransactionResult.IsSpecified = false;
			}
		}

		[ExpectNoExceptions]
		public void TestHandleUnobservedTaskException()
		{
			var service = new AIRSValidationServiceProxyForHandleUnobservedExceptionTest(new AIRSIIDValidationServiceSettingsForTesting());
			var validationLines = new List<AIRSIIDValidationQueriedLineForTesting>();
			validationLines.Add(new AIRSIIDValidationQueriedLineForTesting { Commodity = "1" });
			service.ValidateRequirements(validationLines, new CancellationTokenSource());
			NUnit.Framework.Assert.That(service.ValidateTransactionResult.IsSpecified, NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestGetNewBVSClient()
		{
			var setting1 = new AIRSOGDValidationServiceSettingsForTesting();
			var service1 = new AIRSValidationServiceProxyForTesting(setting1);
			service1.GetNewBVSClient();
			NUnit.Framework.Assert.That(service1.BindingForTest, NUnit.Framework.Is.TypeOf<BasicHttpBinding>());
			NUnit.Framework.Assert.That(((BasicHttpBinding)service1.BindingForTest).UseDefaultWebProxy, NUnit.Framework.Is.EqualTo(true));

			var setting2 = new AIRSValidationServiceSettingsWithHttpsUrlForTesting();
			var service2 = new AIRSValidationServiceProxyForTesting(setting2);
			service2.GetNewBVSClient();
			NUnit.Framework.Assert.That(service2.BindingForTest, NUnit.Framework.Is.TypeOf<BasicHttpsBinding>());
			NUnit.Framework.Assert.That(((BasicHttpsBinding)service2.BindingForTest).UseDefaultWebProxy, NUnit.Framework.Is.EqualTo(false));
			NUnit.Framework.Assert.That(((BasicHttpsBinding)service2.BindingForTest).ProxyAddress.ToString(), NUnit.Framework.Is.EqualTo("http://www.testuri.com/"));
		}

		public void TestValidateWebException()
		{
			var exceptionMessage = "There's network issue to connect to the AIRS Validation Service. Please try later.";
			AssertValidateExceptionCatched(exceptionMessage, x => new WebException(x));
		}

		public void TestValidateTimeoutException()
		{
			var exceptionMessage = "The time allotted to this operation may have been a portion of a longer timeout.";
			AssertValidateExceptionCatched(exceptionMessage, x => new TimeoutException(x));
		}

		public void TestValidateIIDRequirements()
		{
			var service = new AIRSValidationServiceProxyForTesting(new AIRSIIDValidationServiceSettingsForTesting());
			var validationLines = new List<AIRSIIDValidationQueriedLineForTesting>();
			validationLines.Add(CreateAIRSIIDValidationQueriedLine("A1", "1", "020500", "512700", "BC", "US", "TN", "69", "34"));
			validationLines.Add(CreateAIRSIIDValidationQueriedLine("A1", "2", "020500", "512700", "BC", "US", "TN", "69", "34"));
			validationLines.Add(CreateAIRSIIDValidationQueriedLine("A1", "3", "020500", "512700", "BC", "US", "TN", "69", "34"));
			validationLines.Add(CreateAIRSIIDValidationQueriedLine("A2", "4", "020500", "512700", "BC", "US", "TN", "69", "34"));
			validationLines.Add(CreateAIRSIIDValidationQueriedLine("A2", "5", "020500", "512700", "BC", "US", "TN", "69", "34"));
			AddValidateTransactionResult(service.ValidateTransactionResult, "A1", "1", "MISSING OR INVALID REGISTRATION NUMBER");
			AddValidateTransactionResult(service.ValidateTransactionResult, "A1", "2", "MISSING OR INVALID DESTINATION PROVINCE");
			AddValidateTransactionResult(service.ValidateTransactionResult, "A2", "4", "MISSING OR INVALID REGISTRATION NUMBER");
			service.ValidateRequirements(validationLines, new CancellationTokenSource());

			NUnit.Framework.Assert.That(validationLines[0].ValidationResponse, NUnit.Framework.Is.EqualTo("MISSING OR INVALID REGISTRATION NUMBER"), "Line1: Response");
			NUnit.Framework.Assert.That(validationLines[0].ValidationFaultMessageType, NUnit.Framework.Is.EqualTo(ValidationFaultMessageType.None), "ValidationFaultMessageType");
			Assert("Line1: AIRS Validation Query Completed", validationLines[0].ValidationCompleted);
			NUnit.Framework.Assert.That(validationLines[0].ValidationFaultMessage, NUnit.Framework.Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "Line1: Fault Message");

			NUnit.Framework.Assert.That(validationLines[1].ValidationResponse, NUnit.Framework.Is.EqualTo("MISSING OR INVALID DESTINATION PROVINCE"), "Line2: Response");
			Assert("Line2: AIRS Validation Query Completed", validationLines[1].ValidationCompleted);
			NUnit.Framework.Assert.That(validationLines[1].ValidationFaultMessage, NUnit.Framework.Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "Line2: Fault Message");
			NUnit.Framework.Assert.That(validationLines[1].ValidationFaultMessageType, NUnit.Framework.Is.EqualTo(ValidationFaultMessageType.None), "ValidationFaultMessageType");

			NUnit.Framework.Assert.That(validationLines[2].ValidationResponse, NUnit.Framework.Is.EqualTo(default(string)), "Line3: Response - should be [null]");
			Assert("Line3: AIRS Validation Query Completed", validationLines[2].ValidationCompleted);
			NUnit.Framework.Assert.That(validationLines[2].ValidationFaultMessage, NUnit.Framework.Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "Line1: Fault Message");
			NUnit.Framework.Assert.That(validationLines[2].ValidationFaultMessageType, NUnit.Framework.Is.EqualTo(ValidationFaultMessageType.None), "ValidationFaultMessageType");

			NUnit.Framework.Assert.That(validationLines[3].ValidationResponse, NUnit.Framework.Is.EqualTo("MISSING OR INVALID REGISTRATION NUMBER"), "Line4: Response");
			Assert("Line4: AIRS Validation Query Completed", validationLines[3].ValidationCompleted);
			NUnit.Framework.Assert.That(validationLines[3].ValidationFaultMessage, NUnit.Framework.Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "Line4: Fault Message");
			NUnit.Framework.Assert.That(validationLines[3].ValidationFaultMessageType, NUnit.Framework.Is.EqualTo(ValidationFaultMessageType.None), "ValidationFaultMessageType");

			NUnit.Framework.Assert.That(validationLines[4].ValidationResponse, NUnit.Framework.Is.EqualTo(default(string)), "Line5: Response - should be [null]");
			Assert("Line5: AIRS Validation Query Completed", validationLines[4].ValidationCompleted);
			NUnit.Framework.Assert.That(validationLines[4].ValidationFaultMessage, NUnit.Framework.Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "Line5: Fault Message");
			NUnit.Framework.Assert.That(validationLines[4].ValidationFaultMessageType, NUnit.Framework.Is.EqualTo(ValidationFaultMessageType.None), "ValidationFaultMessageType");
		}

		public void TestValidateIIDRequirements_CancelByUser()
		{
			var service = new AIRSValidationServiceProxyForTesting(new AIRSIIDValidationServiceSettingsForTesting());
			service.TaskDelay = 10000;
			var validationLines = new List<AIRSIIDValidationQueriedLineForTesting>();
			validationLines.Add(new AIRSIIDValidationQueriedLineForTesting());

			var cts = new CancellationTokenSource();
			new Thread((object data) =>
			{
				Thread.Sleep(100);
				((CancellationTokenSource)data).Cancel();
			}).Start(cts);
			service.ValidateRequirements(validationLines, cts);
			NUnit.Framework.Assert.That(validationLines[0].ValidationFaultMessageType, NUnit.Framework.Is.EqualTo(ValidationFaultMessageType.QueryAborted), "ValidationFaultMessageType");
			NUnit.Framework.Assert.That(validationLines[0].ValidationFaultMessage, NUnit.Framework.Is.EqualTo("AIRS Validation Query Aborted").Using(CustomComparers.TypeComparison));
			Assert("AIRS Validation Query Aborted", !validationLines[0].ValidationCompleted);
		}

		public void TestValidateIIDRequirements_CancelByTimeout()
		{
			var setting = new AIRSIIDValidationServiceSettingsForTesting();
			var service = new AIRSValidationServiceProxyForTesting(setting);
			service.TaskDelay = 10000;
			var validationLines = new List<AIRSIIDValidationQueriedLineForTesting>();
			validationLines.Add(new AIRSIIDValidationQueriedLineForTesting());
			service.ValidateRequirements(validationLines, new CancellationTokenSource(100));
			NUnit.Framework.Assert.That(validationLines[0].ValidationFaultMessageType, NUnit.Framework.Is.EqualTo(ValidationFaultMessageType.QueryAborted), "ValidationFaultMessageType");
			NUnit.Framework.Assert.That(validationLines[0].ValidationFaultMessage, NUnit.Framework.Is.EqualTo("AIRS Validation Query Aborted").Using(CustomComparers.TypeComparison));
			Assert("AIRS Validation Query Aborted", !validationLines[0].ValidationCompleted);
		}

		public void TestValidateIIDRequirements_Exception()
		{
			var service = new AIRSValidationServiceProxyForTesting(new AIRSIIDValidationServiceSettingsForTesting());
			var validationLines = new List<AIRSIIDValidationQueriedLineForTesting>();
			validationLines.Add(new AIRSIIDValidationQueriedLineForTesting() { Commodity = "1" });
			service.ValidateTransactionException = new FaultException<ServiceFaultContract>(
				new ServiceFaultContract()
				{
					ErrorCode = "ERR002",
					ErrorDetail = "Data at the root level is invalid. Line 1, position 1.",
					ErrorMessage = "The XML is invalid.",
					RequestId = "R#000005"
				});

			service.ValidateRequirements(validationLines, new CancellationTokenSource());
			NUnit.Framework.Assert.That(validationLines[0].ValidationFaultMessageType, NUnit.Framework.Is.EqualTo(ValidationFaultMessageType.SOAPError), "ValidationFaultMessageType");
			NUnit.Framework.Assert.That(validationLines[0].ValidationFaultMessage, NUnit.Framework.Is.EqualTo("ErrorCode:ERR002 ErrorMessage:The XML is invalid.\r\nErrorDetail:Data at the root level is invalid. Line 1, position 1.\r\nRequestId:R#000005").Using(CustomComparers.TypeComparison));
			Assert("AIRS Validation Query Failed", validationLines[0].ValidationCompleted);

			validationLines = new List<AIRSIIDValidationQueriedLineForTesting>();
			validationLines.Add(new AIRSIIDValidationQueriedLineForTesting() { Commodity = "1" });
			service.ValidateTransactionException = new Exception("Unknown Error");
			service.ValidateRequirements(validationLines, new CancellationTokenSource());
			NUnit.Framework.Assert.That(validationLines[0].ValidationFaultMessageType, NUnit.Framework.Is.EqualTo(ValidationFaultMessageType.HttpError), "ValidationFaultMessageType");
			NUnit.Framework.Assert.That(validationLines[0].ValidationFaultMessage, NUnit.Framework.Is.EqualTo("Unknown Error").Using(CustomComparers.TypeComparison));
			Assert("AIRS Validation Query Failed", validationLines[0].ValidationCompleted);
		}

		public void TestGenerateValidateIIDTransaction_CommoditiesNumberLimitExceed()
		{
			var validationLines = new List<AIRSIIDValidationQueriedLineForTesting>();
			for (int i = 0; i < 500; i++)
			{
				validationLines.Add(CreateAIRSIIDValidationQueriedLine("A1", i.ToString(), "020500", "512700", "BC", "US", "TN", "69", "34"));
			}

			var service = new AIRSValidationServiceProxyForTesting(new AIRSIIDValidationServiceSettingsForTesting());
			var validateTransactions = service.GenerateValidateTransaction(validationLines);
			NUnit.Framework.Assert.That(validateTransactions.Count(), NUnit.Framework.Is.EqualTo(2), "The number of commodities cannot exceed the limit.");
			NUnit.Framework.Assert.That(((ValidateIIDTransaction)validateTransactions.First().Transaction).CommodityGroup.Cast<ValidateIIDTransactionCommodityGroup>().Sum(g => g.Commodity.Count), NUnit.Framework.Is.EqualTo(393), "The fisrt request should contains 400 commodity.");
			NUnit.Framework.Assert.That(((ValidateIIDTransaction)validateTransactions.Last().Transaction).CommodityGroup.Cast<ValidateIIDTransactionCommodityGroup>().Sum(g => g.Commodity.Count), NUnit.Framework.Is.EqualTo(107), "The second request should contains 100 commodity.");
			Assert("The size of the entire message cannot exceed the limit.", validateTransactions.First().Transaction.Serialize().Length < service.MessageSizeLimit);
			Assert("The size of the entire message cannot exceed the limit.", validateTransactions.Last().Transaction.Serialize().Length < service.MessageSizeLimit);
		}

		public void TestGenerateValidateIIDTransaction_MaxSizeLimitExceed()
		{
			var validationLines = new List<AIRSIIDValidationQueriedLineForTesting>();
			for (int i = 0; i < 500; i++)
			{
				validationLines.Add(CreateAIRSIIDValidationQueriedLine("A1", i.ToString(), "020500", "512700", "BC", "US", "TN", "69", "34"));
			}

			var service = new AIRSValidationServiceProxyForTesting(new AIRSIIDValidationServiceSettingsForTesting());
			var validateTransactions = service.GenerateValidateTransaction(validationLines);
			NUnit.Framework.Assert.That(validateTransactions.Count(), NUnit.Framework.Is.EqualTo(2), "The size of the entire message cannot exceed the limit.");
			Assert("The number of commodities cannot exceed the limit.", ((ValidateIIDTransaction)validateTransactions.First().Transaction).CommodityGroup.Cast<ValidateIIDTransactionCommodityGroup>().Sum(g => g.Commodity.Count) <= 400);
			Assert("The number of commodities cannot exceed the limit.", ((ValidateIIDTransaction)validateTransactions.Last().Transaction).CommodityGroup.Cast<ValidateIIDTransactionCommodityGroup>().Sum(g => g.Commodity.Count) <= 400);
			Assert("The size of the entire message cannot exceed the limit.", validateTransactions.First().Transaction.Serialize().Length < service.MessageSizeLimit);
			Assert("The size of the entire message cannot exceed the limit.", validateTransactions.Last().Transaction.Serialize().Length < service.MessageSizeLimit);
		}

		[ExpectNoExceptions]
		public void TestGenerateValidateIIDTransaction()
		{
			#region Expect XML

			string expectXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ValidateIIDTransaction>
  <CommodityGroup commodityGroupId=""A1"">
    <Commodity commodityId=""1"">
      <HSNumber>020500</HSNumber>
      <AirsCode>512700</AirsCode>
      <DeliveryPartyProvince>BC</DeliveryPartyProvince>
      <OriginCountry>US</OriginCountry>
      <OriginState>TN</OriginState>
      <Enduse>69</Enduse>
      <Miscellaneous>34</Miscellaneous>
      <Registrations>
        <Registration registrationId=""7"" />
        <MaterializedLpco registrationId=""19"" />
        <DematerializedLpco registrationId=""65"" />
      </Registrations>
    </Commodity>
    <Commodity commodityId=""2"">
      <HSNumber>020500</HSNumber>
      <AirsCode>512700</AirsCode>
      <DeliveryPartyProvince>BC</DeliveryPartyProvince>
      <OriginCountry>US</OriginCountry>
      <OriginState>TN</OriginState>
      <Enduse>69</Enduse>
      <Miscellaneous>34</Miscellaneous>
      <Registrations>
        <Registration registrationId=""7"" />
        <MaterializedLpco registrationId=""19"" />
        <DematerializedLpco registrationId=""65"" />
      </Registrations>
    </Commodity>
    <Commodity commodityId=""3"">
      <HSNumber>020500</HSNumber>
      <AirsCode>512700</AirsCode>
      <DeliveryPartyProvince>BC</DeliveryPartyProvince>
      <OriginCountry>US</OriginCountry>
      <OriginState>TN</OriginState>
      <Enduse>69</Enduse>
      <Miscellaneous>34</Miscellaneous>
      <Registrations>
        <Registration registrationId=""7"" />
        <MaterializedLpco registrationId=""19"" />
        <DematerializedLpco registrationId=""65"" />
      </Registrations>
    </Commodity>
  </CommodityGroup>
  <CommodityGroup commodityGroupId=""A2"">
    <Commodity commodityId=""4"">
      <HSNumber>020500</HSNumber>
      <AirsCode>512700</AirsCode>
      <DeliveryPartyProvince>BC</DeliveryPartyProvince>
      <OriginCountry>US</OriginCountry>
      <OriginState>TN</OriginState>
      <Enduse>69</Enduse>
      <Miscellaneous>34</Miscellaneous>
      <Registrations>
        <Registration registrationId=""7"" />
        <MaterializedLpco registrationId=""19"" />
        <DematerializedLpco registrationId=""65"" />
      </Registrations>
    </Commodity>
    <Commodity commodityId=""5"">
      <HSNumber>020500</HSNumber>
      <Registrations />
    </Commodity>
  </CommodityGroup>
</ValidateIIDTransaction>";

			#endregion

			var validationLines = new List<AIRSIIDValidationQueriedLineForTesting>();
			validationLines.Add(CreateAIRSIIDValidationQueriedLine("A1", "1", "020500", "512700", "BC", "US", "TN", "69", "34"));
			validationLines.Add(CreateAIRSIIDValidationQueriedLine("A1", "2", "020500", "512700", "BC", "US", "TN", "69", "34"));
			validationLines.Add(CreateAIRSIIDValidationQueriedLine("A1", "3", "020500", "512700", "BC", "US", "TN", "69", "34"));
			validationLines.Add(CreateAIRSIIDValidationQueriedLine("A2", "4", "020500", "512700", "BC", "US", "TN", "69", "34"));
			var line = CreateAIRSIIDValidationQueriedLine("A2", "5", "020500", "", "", "", "", "", "");
			line.RegistrationsList.Clear();
			validationLines.Add(line);

			var service = new AIRSValidationServiceProxyForTesting(new AIRSIIDValidationServiceSettingsForTesting());
			var validateTransaction = service.GenerateValidateTransaction(validationLines).First();
			var data = validateTransaction.Transaction.Serialize();
			var xml = new string(System.Text.Encoding.UTF8.GetChars(data));

			NUnit.Framework.Assert.That(xml, NUnit.Framework.Is.EqualTo(expectXml));
		}

		public void TestValidateRequirements()
		{
			var service = new AIRSValidationServiceProxyForTesting(new AIRSOGDValidationServiceSettingsForTesting());
			var validationLines = new List<AIRSOGDValidationQueriedLineForTesting>();
			validationLines.Add(CreateAIRSValidationQueriedLine("A1", "1", "020500", "21247", "14", "512700", "BC", "US", "TN", "69", "34", "14", "28"));
			validationLines.Add(CreateAIRSValidationQueriedLine("A1", "2", "020500", "21247", "14", "512700", "BC", "US", "TN", "69", "34", "14", "28"));
			validationLines.Add(CreateAIRSValidationQueriedLine("A1", "3", "020500", "21247", "14", "512700", "BC", "US", "TN", "69", "34", "14", "28"));
			validationLines.Add(CreateAIRSValidationQueriedLine("A2", "4", "020500", "21247", "14", "512700", "BC", "US", "TN", "69", "34", "14", "28"));
			validationLines.Add(CreateAIRSValidationQueriedLine("A2", "5", "020500", "21247", "14", "512700", "BC", "US", "TN", "69", "34", "14", "28"));
			AddValidateTransactionResult(service.ValidateTransactionResult, "A1", "1", "MISSING OR INVALID REGISTRATION NUMBER");
			AddValidateTransactionResult(service.ValidateTransactionResult, "A1", "2", "MISSING OR INVALID DESTINATION PROVINCE");
			AddValidateTransactionResult(service.ValidateTransactionResult, "A2", "4", "MISSING OR INVALID REGISTRATION NUMBER");
			service.ValidateRequirements(validationLines, new CancellationTokenSource());

			NUnit.Framework.Assert.That(validationLines[0].ValidationResponse, NUnit.Framework.Is.EqualTo("MISSING OR INVALID REGISTRATION NUMBER"), "Line1: Response");
			NUnit.Framework.Assert.That(validationLines[0].ValidationFaultMessageType, NUnit.Framework.Is.EqualTo(ValidationFaultMessageType.None), "ValidationFaultMessageType");
			Assert("Line1: AIRS Validation Query Completed", validationLines[0].ValidationCompleted);
			NUnit.Framework.Assert.That(validationLines[0].ValidationFaultMessage, NUnit.Framework.Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "Line1: Fault Message");

			NUnit.Framework.Assert.That(validationLines[1].ValidationResponse, NUnit.Framework.Is.EqualTo("MISSING OR INVALID DESTINATION PROVINCE"), "Line2: Response");
			Assert("Line2: AIRS Validation Query Completed", validationLines[1].ValidationCompleted);
			NUnit.Framework.Assert.That(validationLines[1].ValidationFaultMessage, NUnit.Framework.Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "Line2: Fault Message");
			NUnit.Framework.Assert.That(validationLines[1].ValidationFaultMessageType, NUnit.Framework.Is.EqualTo(ValidationFaultMessageType.None), "ValidationFaultMessageType");

			NUnit.Framework.Assert.That(validationLines[2].ValidationResponse, NUnit.Framework.Is.EqualTo(default(string)), "Line3: Response - should be [null]");
			Assert("Line3: AIRS Validation Query Completed", validationLines[2].ValidationCompleted);
			NUnit.Framework.Assert.That(validationLines[2].ValidationFaultMessage, NUnit.Framework.Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "Line1: Fault Message");
			NUnit.Framework.Assert.That(validationLines[2].ValidationFaultMessageType, NUnit.Framework.Is.EqualTo(ValidationFaultMessageType.None), "ValidationFaultMessageType");

			NUnit.Framework.Assert.That(validationLines[3].ValidationResponse, NUnit.Framework.Is.EqualTo("MISSING OR INVALID REGISTRATION NUMBER"), "Line4: Response");
			Assert("Line4: AIRS Validation Query Completed", validationLines[3].ValidationCompleted);
			NUnit.Framework.Assert.That(validationLines[3].ValidationFaultMessage, NUnit.Framework.Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "Line4: Fault Message");
			NUnit.Framework.Assert.That(validationLines[3].ValidationFaultMessageType, NUnit.Framework.Is.EqualTo(ValidationFaultMessageType.None), "ValidationFaultMessageType");

			NUnit.Framework.Assert.That(validationLines[4].ValidationResponse, NUnit.Framework.Is.EqualTo(default(string)), "Line5: Response - should be [null]");
			Assert("Line5: AIRS Validation Query Completed", validationLines[4].ValidationCompleted);
			NUnit.Framework.Assert.That(validationLines[4].ValidationFaultMessage, NUnit.Framework.Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison), "Line5: Fault Message");
			NUnit.Framework.Assert.That(validationLines[4].ValidationFaultMessageType, NUnit.Framework.Is.EqualTo(ValidationFaultMessageType.None), "ValidationFaultMessageType");
		}

		public void TestValidateRequirements_CancelByUser()
		{
			var service = new AIRSValidationServiceProxyForTesting(new AIRSOGDValidationServiceSettingsForTesting());
			service.TaskDelay = 10000;
			var validationLines = new List<AIRSOGDValidationQueriedLineForTesting>();
			validationLines.Add(new AIRSOGDValidationQueriedLineForTesting());

			var cts = new CancellationTokenSource();
			new Thread((object data) =>
			{
				Thread.Sleep(100);
				((CancellationTokenSource)data).Cancel();
			}).Start(cts);
			service.ValidateRequirements(validationLines, cts);
			NUnit.Framework.Assert.That(validationLines[0].ValidationFaultMessageType, NUnit.Framework.Is.EqualTo(ValidationFaultMessageType.QueryAborted), "ValidationFaultMessageType");
			NUnit.Framework.Assert.That(validationLines[0].ValidationFaultMessage, NUnit.Framework.Is.EqualTo("AIRS Validation Query Aborted").Using(CustomComparers.TypeComparison));
			Assert("AIRS Validation Query Aborted", !validationLines[0].ValidationCompleted);
		}

		public void TestValidateRequirements_CancelByTimeout()
		{
			var setting = new AIRSOGDValidationServiceSettingsForTesting();
			var service = new AIRSValidationServiceProxyForTesting(setting);
			service.TaskDelay = 10000;
			var validationLines = new List<AIRSOGDValidationQueriedLineForTesting>();
			validationLines.Add(new AIRSOGDValidationQueriedLineForTesting());
			service.ValidateRequirements(validationLines, new CancellationTokenSource(100));
			NUnit.Framework.Assert.That(validationLines[0].ValidationFaultMessageType, NUnit.Framework.Is.EqualTo(ValidationFaultMessageType.QueryAborted), "ValidationFaultMessageType");
			NUnit.Framework.Assert.That(validationLines[0].ValidationFaultMessage, NUnit.Framework.Is.EqualTo("AIRS Validation Query Aborted").Using(CustomComparers.TypeComparison));
			Assert("AIRS Validation Query Aborted", !validationLines[0].ValidationCompleted);
		}

		public void TestValidateRequirements_Exception()
		{
			var service = new AIRSValidationServiceProxyForTesting(new AIRSOGDValidationServiceSettingsForTesting());
			var validationLines = new List<AIRSOGDValidationQueriedLineForTesting>();
			validationLines.Add(new AIRSOGDValidationQueriedLineForTesting() { Commodity = "1" });
			service.ValidateTransactionException = new FaultException<ServiceFaultContract>(
				new ServiceFaultContract()
				{
					ErrorCode = "ERR002",
					ErrorDetail = "Data at the root level is invalid. Line 1, position 1.",
					ErrorMessage = "The XML is invalid.",
					RequestId = "R#000005"
				});

			service.ValidateRequirements(validationLines, new CancellationTokenSource());
			NUnit.Framework.Assert.That(validationLines[0].ValidationFaultMessageType, NUnit.Framework.Is.EqualTo(ValidationFaultMessageType.SOAPError), "ValidationFaultMessageType");
			NUnit.Framework.Assert.That(validationLines[0].ValidationFaultMessage, NUnit.Framework.Is.EqualTo("ErrorCode:ERR002 ErrorMessage:The XML is invalid.\r\nErrorDetail:Data at the root level is invalid. Line 1, position 1.\r\nRequestId:R#000005").Using(CustomComparers.TypeComparison));
			Assert("AIRS Validation Query Failed", validationLines[0].ValidationCompleted);

			validationLines = new List<AIRSOGDValidationQueriedLineForTesting>();
			validationLines.Add(new AIRSOGDValidationQueriedLineForTesting() { Commodity = "1" });
			service.ValidateTransactionException = new Exception("Unknown Error");
			service.ValidateRequirements(validationLines, new CancellationTokenSource());
			NUnit.Framework.Assert.That(validationLines[0].ValidationFaultMessageType, NUnit.Framework.Is.EqualTo(ValidationFaultMessageType.HttpError), "ValidationFaultMessageType");
			NUnit.Framework.Assert.That(validationLines[0].ValidationFaultMessage, NUnit.Framework.Is.EqualTo("Unknown Error").Using(CustomComparers.TypeComparison));
			Assert("AIRS Validation Query Failed", validationLines[0].ValidationCompleted);
		}

		public void TestGenerateValidateTransaction_CommoditiesNumberLimitExceed()
		{
			var validationLines = new List<AIRSOGDValidationQueriedLineForTesting>();
			for (int i = 0; i < 500; i++)
			{
				validationLines.Add(CreateAIRSValidationQueriedLine("A1", i.ToString(), "020500", "21247", "14", "512700", "BC", "US", "TN", "69", "34", "14"));
			}

			var service = new AIRSValidationServiceProxyForTesting(new AIRSOGDValidationServiceSettingsForTesting());
			var validateTransactions = service.GenerateValidateTransaction(validationLines);
			NUnit.Framework.Assert.That(validateTransactions.Count(), NUnit.Framework.Is.EqualTo(2), "The number of commodities cannot exceed the limit.");
			NUnit.Framework.Assert.That(((ValidateTransaction)validateTransactions.First().Transaction).CommodityGroup.Cast<ValidateTransactionCommodityGroup>().Sum(g => g.Commodity.Count), NUnit.Framework.Is.EqualTo(400), "The fisrt request should contains 400 commodity.");
			NUnit.Framework.Assert.That(((ValidateTransaction)validateTransactions.Last().Transaction).CommodityGroup.Cast<ValidateTransactionCommodityGroup>().Sum(g => g.Commodity.Count), NUnit.Framework.Is.EqualTo(100), "The second request should contains 100 commodity.");
			Assert("The size of the entire message cannot exceed the limit.", validateTransactions.First().Transaction.Serialize().Length < service.MessageSizeLimit);
			Assert("The size of the entire message cannot exceed the limit.", validateTransactions.Last().Transaction.Serialize().Length < service.MessageSizeLimit);
		}

		public void TestGenerateValidateTransaction_MaxSizeLimitExceed()
		{
			var validationLines = new List<AIRSOGDValidationQueriedLineForTesting>();
			for (int i = 0; i < 500; i++)
			{
				validationLines.Add(CreateAIRSValidationQueriedLine("A1", i.ToString(), "020500", "21247", "14", "512700", "BC", "US", "TN", "69", "34", "14", "28", "40", "98", "99"));
			}

			var service = new AIRSValidationServiceProxyForTesting(new AIRSOGDValidationServiceSettingsForTesting());
			var validateTransactions = service.GenerateValidateTransaction(validationLines);
			NUnit.Framework.Assert.That(validateTransactions.Count(), NUnit.Framework.Is.EqualTo(2), "The size of the entire message cannot exceed the limit.");
			Assert("The number of commodities cannot exceed the limit.", ((ValidateTransaction)validateTransactions.First().Transaction).CommodityGroup.Cast<ValidateTransactionCommodityGroup>().Sum(g => g.Commodity.Count) <= 400);
			Assert("The number of commodities cannot exceed the limit.", ((ValidateTransaction)validateTransactions.Last().Transaction).CommodityGroup.Cast<ValidateTransactionCommodityGroup>().Sum(g => g.Commodity.Count) <= 400);
			Assert("The size of the entire message cannot exceed the limit.", validateTransactions.First().Transaction.Serialize().Length < service.MessageSizeLimit);
			Assert("The size of the entire message cannot exceed the limit.", validateTransactions.Last().Transaction.Serialize().Length < service.MessageSizeLimit);
		}

		[ExpectNoExceptions]
		public void TestGenerateValidateTransaction()
		{
			#region Expect XML

			string expectXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ValidateTransaction>
  <CommodityGroup commodityGroupId=""A1"">
    <Commodity commodityId=""1"">
      <HSNumber>020500</HSNumber>
      <RequirementId>21247</RequirementId>
      <RequirementVersion>14</RequirementVersion>
      <AirsCode>512700</AirsCode>
      <DestinationProvince>BC</DestinationProvince>
      <OriginCountry>US</OriginCountry>
      <OriginState>TN</OriginState>
      <Enduse>69</Enduse>
      <Miscellaneous>34</Miscellaneous>
      <Registration registrationId=""14"" />
      <Registration registrationId=""28"" />
    </Commodity>
    <Commodity commodityId=""2"">
      <HSNumber>020500</HSNumber>
      <RequirementId>21247</RequirementId>
      <RequirementVersion>14</RequirementVersion>
      <AirsCode>512700</AirsCode>
      <DestinationProvince>BC</DestinationProvince>
      <OriginCountry>US</OriginCountry>
      <OriginState>TN</OriginState>
      <Enduse>69</Enduse>
      <Miscellaneous>34</Miscellaneous>
      <Registration registrationId=""14"" />
      <Registration registrationId=""28"" />
    </Commodity>
    <Commodity commodityId=""3"">
      <HSNumber>020500</HSNumber>
      <RequirementId>21247</RequirementId>
      <RequirementVersion>14</RequirementVersion>
      <AirsCode>512700</AirsCode>
      <DestinationProvince>BC</DestinationProvince>
      <OriginCountry>US</OriginCountry>
      <OriginState>TN</OriginState>
      <Enduse>69</Enduse>
      <Miscellaneous>34</Miscellaneous>
      <Registration registrationId=""14"" />
      <Registration registrationId=""28"" />
    </Commodity>
  </CommodityGroup>
  <CommodityGroup commodityGroupId=""A2"">
    <Commodity commodityId=""4"">
      <HSNumber>020500</HSNumber>
      <RequirementId>21247</RequirementId>
      <RequirementVersion>14</RequirementVersion>
      <AirsCode>512700</AirsCode>
      <DestinationProvince>BC</DestinationProvince>
      <OriginCountry>US</OriginCountry>
      <OriginState>TN</OriginState>
      <Enduse>69</Enduse>
      <Miscellaneous>34</Miscellaneous>
      <Registration registrationId=""14"" />
      <Registration registrationId=""28"" />
    </Commodity>
    <Commodity commodityId=""5"">
      <HSNumber>020500</HSNumber>
    </Commodity>
  </CommodityGroup>
</ValidateTransaction>";

			#endregion

			var validationLines = new List<AIRSOGDValidationQueriedLineForTesting>();
			validationLines.Add(CreateAIRSValidationQueriedLine("A1", "1", "020500", "21247", "14", "512700", "BC", "US", "TN", "69", "34", "14", "28"));
			validationLines.Add(CreateAIRSValidationQueriedLine("A1", "2", "020500", "21247", "14", "512700", "BC", "US", "TN", "69", "34", "14", "28"));
			validationLines.Add(CreateAIRSValidationQueriedLine("A1", "3", "020500", "21247", "14", "512700", "BC", "US", "TN", "69", "34", "14", "28"));
			validationLines.Add(CreateAIRSValidationQueriedLine("A2", "4", "020500", "21247", "14", "512700", "BC", "US", "TN", "69", "34", "14", "28"));
			validationLines.Add(CreateAIRSValidationQueriedLine("A2", "5", "020500", "", "", "", "", "", "", "", ""));

			var service = new AIRSValidationServiceProxyForTesting(new AIRSOGDValidationServiceSettingsForTesting());
			var validateTransaction = service.GenerateValidateTransaction(validationLines).First();
			var data = validateTransaction.Transaction.Serialize();
			var xml = new string(System.Text.Encoding.UTF8.GetChars(data));

			NUnit.Framework.Assert.That(xml, NUnit.Framework.Is.EqualTo(expectXml));
		}

		static AIRSIIDValidationQueriedLineForTesting CreateAIRSIIDValidationQueriedLine(string commodityGroup, string commodity, string hsNumber, string airsCode, string deliveryPartyProvince,
			string originCountry, string originState, string endUse, string miscellaneous)
		{
			var line = new AIRSIIDValidationQueriedLineForTesting();
			line.CommodityGroup = commodityGroup;
			line.Commodity = commodity;
			line.HSNumber = hsNumber;
			line.AirsCode = airsCode;
			line.DeliveryPartyProvince = deliveryPartyProvince;
			line.OriginCountry = originCountry;
			line.OriginState = originState;
			line.EndUse = endUse;
			line.Miscellaneous = miscellaneous;
			line.RegistrationsList.Add(new AIRSValidationQueriedLineRegistration("19", AIRSValidationQueriedLineRegistrationTypes.MaterializedLpco));
			line.RegistrationsList.Add(new AIRSValidationQueriedLineRegistration("65", AIRSValidationQueriedLineRegistrationTypes.DematerializedLpco));
			line.RegistrationsList.Add(new AIRSValidationQueriedLineRegistration("7", AIRSValidationQueriedLineRegistrationTypes.Normal));
			return line;
		}

		static AIRSOGDValidationQueriedLineForTesting CreateAIRSValidationQueriedLine(string commodityGroup, string commodity, string hsNumber, string requirementId, string requirementVersion,
			string airsCode, string destinationProvince, string originCountry, string originState, string endUse, string miscellaneous, params string[] registrationIds)
		{
			var line = new AIRSOGDValidationQueriedLineForTesting();
			line.CommodityGroup = commodityGroup;
			line.Commodity = commodity;
			line.HSNumber = hsNumber;
			line.RequirementId = requirementId;
			line.RequirementVersion = requirementVersion;
			line.AirsCode = airsCode;
			line.DestinationProvince = destinationProvince;
			line.OriginCountry = originCountry;
			line.OriginState = originState;
			line.EndUse = endUse;
			line.Miscellaneous = miscellaneous;
			foreach (var registrationId in registrationIds)
			{
				line.RegistrationsList.Add(new AIRSValidationQueriedLineRegistration(registrationId));
			}
			return line;
		}

		static void AddValidateTransactionResult(ValidateTransactionResult result, string commodityGroupId, string commodityId, string error)
		{
			var commodityGroup = result.CommodityGroup.Cast<ValidateTransactionResultCommodityGroup>().FirstOrDefault(g => g.commodityGroupId == commodityGroupId);
			if (commodityGroup == null)
			{
				commodityGroup = result.CommodityGroup.AddNew();
				commodityGroup.commodityGroupId = commodityGroupId;
			}
			var commodity = commodityGroup.Commodity.AddNew();
			commodity.commodityId = commodityId;
			commodity.Error = error;
		}

		void AssertValidateExceptionCatched(ZString exceptionMessage, Func<ZString, Exception> validateException)
		{
			var service = new AIRSValidationServiceProxyForTesting(new AIRSIIDValidationServiceSettingsForTesting());
			var validationLines = new List<AIRSIIDValidationQueriedLineForTesting>();
			validationLines.Add(new AIRSIIDValidationQueriedLineForTesting { Commodity = "1" });
			service.ValidateException = validateException.Invoke(exceptionMessage);

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => service.ValidateRequirements(validationLines, new CancellationTokenSource()));
				NUnit.Framework.Assert.That(validationLines[0].ValidationFaultMessageType, NUnit.Framework.Is.EqualTo(ValidationFaultMessageType.HttpError), "ValidationFaultMessageType");
				NUnit.Framework.Assert.That(validationLines[0].ValidationFaultMessage, NUnit.Framework.Is.EqualTo(exceptionMessage), "Exception Message");
				NUnit.Framework.Assert.That(validationLines[0].ValidationCompleted, NUnit.Framework.Is.EqualTo(true), "AIRS Validation Query Failed");
			});
		}

		sealed class AIRSValidationServiceSettingsWithHttpsUrlForTesting : IAIRSValidationServiceSettings
		{
			public ZString Uri => "https://www.testuri.com";

			public string SchemaVersion => "1.0";

			public string Key => "Test";

			public bool FrenchPreferred => false;

			public string UserName => string.Empty;

			public string Password => string.Empty;

			public ZString WebProxyUri => "http://www.testuri.com";
		}

		sealed class AIRSIIDValidationServiceSettingsForTesting : IAIRSValidationServiceSettings
		{
			public ZString Uri => "http://www.testuri.com";

			public string SchemaVersion => "2.0";

			public string Key => "Test";

			public bool FrenchPreferred => false;

			public string UserName => string.Empty;

			public string Password => string.Empty;

			public ZString WebProxyUri => ZString.Empty;
		}

		sealed class AIRSOGDValidationQueriedLineForTesting : IAIRSOGDValidationQueriedLine
		{
			public string CommodityGroup { get; set; }

			public string Commodity { get; set; }

			public string HSNumber { get; set; }

			public string RequirementId { get; set; }

			public string RequirementVersion { get; set; }

			public string AirsCode { get; set; }

			public string DestinationProvince { get; set; }

			public string OriginCountry { get; set; }

			public string OriginState { get; set; }

			public string EndUse { get; set; }

			public string Miscellaneous { get; set; }

			public string ValidationResponse { get; set; }

			public ZString ValidationFaultMessage { get; set; }

			public bool ValidationCompleted { get; set; }

			public ValidationFaultMessageType ValidationFaultMessageType { get; set; }

			public List<ZGuid> InvoiceLinePKs { get; }

			public IEnumerable<IAIRSValidationQueriedLineRegistration> Registrations => RegistrationsList;

			public List<AIRSValidationQueriedLineRegistration> RegistrationsList = new List<AIRSValidationQueriedLineRegistration>();
		}

		sealed class AIRSIIDValidationQueriedLineForTesting : IAIRSIIDValidationQueriedLine
		{
			public string CommodityGroup { get; set; }

			public string Commodity { get; set; }

			public string HSNumber { get; set; }

			public string AirsCode { get; set; }

			public string DeliveryPartyProvince { get; set; }

			public string OriginCountry { get; set; }

			public string OriginState { get; set; }

			public string EndUse { get; set; }

			public string Miscellaneous { get; set; }

			public string ValidationResponse { get; set; }

			public ZString ValidationFaultMessage { get; set; }

			public bool ValidationCompleted { get; set; }

			public ValidationFaultMessageType ValidationFaultMessageType { get; set; }

			public List<ZGuid> InvoiceLinePKs { get; }

			public IEnumerable<IAIRSValidationQueriedLineRegistration> Registrations => RegistrationsList;

			public List<AIRSValidationQueriedLineRegistration> RegistrationsList = new List<AIRSValidationQueriedLineRegistration>();
		}

		sealed class AIRSValidationQueriedLineRegistration : IAIRSValidationQueriedLineRegistration
		{
			public AIRSValidationQueriedLineRegistration(string registrationId, AIRSValidationQueriedLineRegistrationTypes registrationType)
			{
				RegistrationId = registrationId;
				RegistrationType = registrationType;
			}

			public AIRSValidationQueriedLineRegistration(string registrationId)
			{
				RegistrationId = registrationId;
			}

			public string RegistrationId { get; set; }

			public AIRSValidationQueriedLineRegistrationTypes RegistrationType { get; set; }
		}
	}
}
