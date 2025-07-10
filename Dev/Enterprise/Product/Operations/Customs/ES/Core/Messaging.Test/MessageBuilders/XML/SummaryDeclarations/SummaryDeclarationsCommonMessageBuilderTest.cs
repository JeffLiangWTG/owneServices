using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	public abstract class SummaryDeclarationsCommonMessageBuilderTest<TMessageBuilder, TProvider, T> : XMLMessageBuilderTest<TMessageBuilder, TProvider, T>
		where TProvider : class, ISummaryDeclarationsCommonMessageDataProvider
		where TMessageBuilder : SummaryDeclarationsCommonMessageBuilder<TProvider, T>
	{
		protected IENSAddressInformation SetUpENSAddressInformation(ZString id, ZString name, ZString address, ZString city, ZString postCode, ZString country, ZString language)
		{
			var addressInformation = new Mock<IENSAddressInformation>();
			addressInformation.Setup(m => m.Id).Returns(id);
			addressInformation.Setup(m => m.Name).Returns(name);
			addressInformation.Setup(m => m.Address).Returns(address);
			addressInformation.Setup(m => m.City).Returns(city);
			addressInformation.Setup(m => m.PostCode).Returns(postCode);
			addressInformation.Setup(m => m.Country).Returns(country);
			addressInformation.Setup(m => m.Language).Returns(language);
			return addressInformation.Object;
		}
	}
}
