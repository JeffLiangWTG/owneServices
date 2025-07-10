using Moq;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	public abstract class ENSCommonMessageBuilderTest<TMessageBuilder, TProvider, T> : SummaryDeclarationsCommonMessageBuilderTest<TMessageBuilder, TProvider, T>
		where TProvider : class, IENSCommonMessageDataProvider
		where TMessageBuilder : ENSCommonMessageBuilder<TProvider, T>
	{
		#region CommonTests

		public abstract void TestPopulateConsignor();
		public abstract void TestPopulateConsignee();
		public abstract void TestPopulateNotifyParty();
		public abstract void TestPopulateCommonGoodsItems();
		public abstract void TestPopulateCommonLine();
		public abstract void TestPopulateCertificates();
		public abstract void TestPopulateCertificate();
		public abstract void TestPopulateSpecialMentions();
		public abstract void TestPopulateSpecialMention();
		public abstract void TestPopulateLineConsignor();
		public abstract void TestPopulateCommodityCode();
		public abstract void TestPopulateLineConsignee();
		public abstract void TestPopulateContainers();
		public abstract void TestPopulateContainer();
		public abstract void TestPopulateBorderTransportMeans();
		public abstract void TestPopulateBorderTransport();
		public abstract void TestPopulatePackages();
		public abstract void TestPopulatePackage();
		public abstract void TestPopulateLineNotifyParty();
		public abstract void TestPopulateItineraryCountries();
		public abstract void TestPopulateRepresentativeTrader();
		public abstract void TestPopulateLodgingPerson();
		public abstract void TestPopulateSeals();
		public abstract void TestPopulateSeal();
		public abstract void TestPopulateCommonFirstEntryCustomsOffice();
		public abstract void TestPopulateSubsequentEntriesCustomsOffices();
		public abstract void TestPopulateSubsequentEntriesCustomsOffice();
		public abstract void TestPopulateEntryCarrierTrader();
		#endregion

		#region Structures Common SetUp

		protected IENSBorderTransport SetUpBorderTransport()
		{
			var mockBorderTransport = new Mock<IENSBorderTransport>();
			mockBorderTransport.Setup(m => m.Id).Returns("1111111");
			mockBorderTransport.Setup(m => m.Language).Returns("ES");
			mockBorderTransport.Setup(m => m.Nationality).Returns("ES");
			return mockBorderTransport.Object;
		}
		#endregion
	}
}
