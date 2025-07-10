using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	public abstract class DVDCommonMessageBuilderTest<TMessageBuilder, TProvider, T> : XMLMessageBuilderTest<TMessageBuilder, TProvider, T>
		where TProvider : class, IDVDCommonDataProvider
		where TMessageBuilder : XMLMessageBuilder<TProvider, T>
	{
		#region Tests

		public abstract void TestPopulateSegmentosDeServicioIsTestFalse();

		#endregion

		protected override sealed ZString GetSignedMessageTestFileContent() => GetTestFile();
		protected override sealed ZString GetUnsignedMessageTestFileContent() => GetTestFile();
		protected abstract ZString GetTestFile();

		protected override void SetUp()
		{
			base.SetUp();
			mockProvider.Setup(m => m.MRN).Returns("20ES00999830001277");
		}

		protected Mock<IDVDCommonPackage> SetUpPackage(ZString type, ZString marks, ZInt quantity)
		{
			var mockPackages = new Mock<IDVDCommonPackage>();
			mockPackages.Setup(m => m.PackageType).Returns(type);
			mockPackages.Setup(m => m.Marks).Returns(marks);
			mockPackages.Setup(m => m.NumberOfPackages).Returns(quantity);
			return mockPackages;
		}
	}
}
