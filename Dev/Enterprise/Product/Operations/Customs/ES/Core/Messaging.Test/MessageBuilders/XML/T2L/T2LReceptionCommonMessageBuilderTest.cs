
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	public abstract class T2LReceptionCommonMessageBuilderTest<TMessageBuilder, TProvider, T> : T2LCommonMessageBuilderTest<TMessageBuilder, TProvider, T, IReceptionHeader, IT2LLineCommon>
		where TProvider : class, IReceptionMessageDataProvider
		where TMessageBuilder : T2LCommonMessageBuilder<TProvider, T>
	{
		#region CommonTests

		public abstract void TestPopulateDeclarante();

		public abstract void TestPopulatePartidas();

		public abstract void TestPopulateLine();

		public abstract void TestPopulateBultos();

		public abstract void TestPopulatePackage();

		public abstract void TestPopulateVehiculos();

		public abstract void TestPopulateVehicle();

		#endregion
		#region Structures Common SetUp
		protected Mock<IReceptionHeader> SetUpCommonHeader()
		{
			var mockHeader = new Mock<IReceptionHeader>();

			mockHeader.Setup(m => m.ExpeditionCountry).Returns("ES");
			mockHeader.Setup(m => m.TotalLinesNum).Returns(2);
			mockHeader.Setup(m => m.TotalPackagesQty).Returns(1);
			mockHeader.Setup(m => m.ContainersIndicator).Returns(true);

			var mockDeclarant = BuilderHelperTest.SetUpPartyName("89890001K", "Pelinganos");
			mockHeader.Setup(m => m.Declarant).Returns(mockDeclarant);

			mockHeader.Setup(m => m.ReceptionCustomsOffice).Returns("001900");
			mockHeader.Setup(m => m.ReceptionT2LReference).Returns("12ES001900L0000012");
			mockHeader.Setup(m => m.ExpeditionDate).Returns(ZDateTime.BrettsBirthday);

			return mockHeader;
		}
		#endregion
	}
}
