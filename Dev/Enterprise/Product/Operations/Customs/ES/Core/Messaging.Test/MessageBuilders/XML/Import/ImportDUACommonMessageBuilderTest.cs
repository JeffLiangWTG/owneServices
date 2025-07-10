using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	public abstract class ImportDUACommonMessageBuilderTest<TMessageBuilder, TProvider, T, THeaderProvider, TLineProvider> : ImportCommonMessageBuilderTest<TMessageBuilder, TProvider, T, THeaderProvider, TLineProvider>
		where TProvider : class, IDUAImportDataProvider
		where TMessageBuilder : ImportCommonMessageBuilder<TProvider, T>
		where THeaderProvider : IImportCommonHeader
		where TLineProvider : IImportCommonLine
	{
		#region CommonTests

		public abstract void TestCAaduanaNullWhenMRNDeclared();

		public abstract void TestServDatadoEnCeutaMelillaFalse();

		public abstract void TestPopulateGarantiaGRN();

		public abstract void TestPopulateGarantiaGRNATC();

		#endregion

		#region Structures Common SetUp

		protected override void SetUp()
		{
			base.SetUp();

			mockProvider.Setup(m => m.MRN).Returns(ZString.Empty);
		}

		protected IDUAImportDeclaredTax SetUpDeclaredTax(ZString taxClass, ZDecimal taxableIncome, ZDecimal taxRate, ZString maxMinIndicator, ZString fiscalUnit, ZDecimal fee)
		{
			var mockDeclaredTax = new Mock<IDUAImportDeclaredTax>();
			mockDeclaredTax.Setup(m => m.TaxClass).Returns(taxClass);
			mockDeclaredTax.Setup(m => m.TaxableIncome).Returns(taxableIncome);
			mockDeclaredTax.Setup(m => m.TaxRate).Returns(taxRate);
			mockDeclaredTax.Setup(m => m.MaxMinIndicator).Returns(maxMinIndicator);
			mockDeclaredTax.Setup(m => m.FiscalUnit).Returns(fiscalUnit);
			mockDeclaredTax.Setup(m => m.Fee).Returns(fee);
			return mockDeclaredTax.Object;
		}

		#endregion
	}
}
