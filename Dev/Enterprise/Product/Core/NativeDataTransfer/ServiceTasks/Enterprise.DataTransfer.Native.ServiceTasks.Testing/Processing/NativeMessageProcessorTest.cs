using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.DataTransfer.Native.ServiceTasks.Testing
{
	public class NativeMessageProcessorTest : TestCaseWithFactory
	{
		public void TestShouldSuccessfullyProcessWhenEnableCodeMappingIsProvidedAs_EmptyTag()
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_MessageText =
				@"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0""> <Header> <OwnerCode>WORLOGNYC</OwnerCode> <EnableCodeMapping></EnableCodeMapping> </Header> <Body> <CurrencyExchangeRate version=""2.0""> <RefExchangeRate Action=""MERGE""> <ExRateType>BUY</ExRateType> <StartDate>2022-11-28T00:00:00</StartDate> <ExpiryDate>2022-11-28T23:59:59</ExpiryDate> <SellRate>3.673027125</SellRate> <IsSystem>true</IsSystem> <RefCurrency> <Code>AED</Code> </RefCurrency> <GlbCompany> <Code>EWR</Code> </GlbCompany> </RefExchangeRate> </CurrencyExchangeRate> </Body> </Native>";
			Factory.Save();

			new NativeMessageProcessor(new LoggingInformation()).ProcessMessage(message);

			var reloadedMessage = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
			AssertEquals(EDIMessageStatusList.Codes.Queued, reloadedMessage.EM_Status);
		}

		public void TestShouldSuccessfullyProcessWhenEnableCodeMappingIsProvidedAs_SelfClosingTag()
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_MessageText =
				@"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0""> <Header> <OwnerCode>WORLOGNYC</OwnerCode> <EnableCodeMapping/> </Header> <Body> <CurrencyExchangeRate version=""2.0""> <RefExchangeRate Action=""MERGE""> <ExRateType>BUY</ExRateType> <StartDate>2022-11-28T00:00:00</StartDate> <ExpiryDate>2022-11-28T23:59:59</ExpiryDate> <SellRate>3.673027125</SellRate> <IsSystem>true</IsSystem> <RefCurrency> <Code>AED</Code> </RefCurrency> <GlbCompany> <Code>EWR</Code> </GlbCompany> </RefExchangeRate> </CurrencyExchangeRate> </Body> </Native>";
			Factory.Save();

			new NativeMessageProcessor(new LoggingInformation()).ProcessMessage(message);

			var reloadedMessage = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
			AssertEquals(EDIMessageStatusList.Codes.Queued, reloadedMessage.EM_Status);
		}

		public void TestShouldSuccessfullyProcessWhenIsSystemIsProvidedAs_SelfClosingTag()
		{
			var message = Factory.NewWithValidTestData<EDIMessage>();
			message.EM_MessageText =
				@"<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0""> <Header> <OwnerCode>WORLOGNYC</OwnerCode> <EnableCodeMapping/> </Header> <Body> <CurrencyExchangeRate version=""2.0""> <RefExchangeRate Action=""MERGE""> <ExRateType>BUY</ExRateType> <StartDate>2022-11-28T00:00:00</StartDate> <ExpiryDate>2022-11-28T23:59:59</ExpiryDate> <SellRate>3.673027125</SellRate> <IsSystem/> <RefCurrency> <Code>AED</Code> </RefCurrency> <GlbCompany> <Code>EWR</Code> </GlbCompany> </RefExchangeRate> </CurrencyExchangeRate> </Body> </Native>";
			Factory.Save();

			new NativeMessageProcessor(new LoggingInformation()).ProcessMessage(message);

			var reloadedMessage = new BusinessObjectFactory().Load<EDIMessage>(message.PK);
			AssertEquals(EDIMessageStatusList.Codes.Queued, reloadedMessage.EM_Status);
		}
	}
}
