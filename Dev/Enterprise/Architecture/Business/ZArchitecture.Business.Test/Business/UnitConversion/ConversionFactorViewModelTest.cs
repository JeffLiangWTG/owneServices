using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(ConversionFactorViewModel))]
	sealed class ConversionFactorViewModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor_ArgumentsAreNull_ThrowException()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new ConversionFactorViewModel(null, t => ValidationMock));
			AssertExceptionThrown<ArgumentNullException>(() => new ConversionFactorViewModel(t => LookupsMock, null));
		}

		public void TestLookupsGetter_ReturnLookupFromGivenProvider()
		{
			var viewModel = new ConversionFactorViewModel(t => LookupsMock, t => ValidationMock);
			AssertEquals("Lookups", LookupsMock, viewModel.Lookups);
		}

		public void TestValidationGetter_ReturnValidationFromGivenProvider()
		{
			var viewModel = new ConversionFactorViewModel(t => LookupsMock, t => ValidationMock);
			AssertEquals("Validation", ValidationMock, viewModel.Validation);
		}

		public void TestConversionFactorStringGetter()
		{
			var viewModel = new ConversionFactorViewModel(t => LookupsMock, t => ValidationMock);

			viewModel.ConversionFactor = new ConversionFactor(100, "", "");
			AssertEquals("100 /", viewModel.ConversionFactorString);

			viewModel.ConversionFactor = ConversionFactor.Empty;
			AssertEquals("", viewModel.ConversionFactorString);

			viewModel.ConversionFactorString = "McLaren";
			AssertEquals("McLaren", viewModel.ConversionFactorString);

			viewModel.ConversionFactor = new ConversionFactor(100, "KG", "M3");
			AssertEquals("100 KG/M3", viewModel.ConversionFactorString);

			viewModel.ConversionFactor = new ConversionFactor(100.01m, "KG", "M3");
			AssertEquals("100.01 KG/M3", viewModel.ConversionFactorString);

			viewModel.ConversionFactor = new ConversionFactor(100.001m, "KG", "M3");
			AssertEquals("100.001 KG/M3", viewModel.ConversionFactorString);

			viewModel.ConversionFactor = new ConversionFactor(100.000m, "KG", "M3");
			AssertEquals("100 KG/M3", viewModel.ConversionFactorString);
		}

		public void TestConversionFactorStringSetter()
		{
			var viewModel = new ConversionFactorViewModel(t => LookupsMock, t => ValidationMock);

			viewModel.ConversionFactorString = "McLaren";
			Assert("Conversion Factor is empty", viewModel.ConversionFactor.IsEmpty);

			viewModel.ConversionFactorString = "100";
			Assert("Conversion Factor is empty", viewModel.ConversionFactor.IsEmpty);

			viewModel.ConversionFactorString = "100 KG";
			Assert("Conversion Factor is empty", viewModel.ConversionFactor.IsEmpty);

			viewModel.ConversionFactorString = "100 /KG";
			Assert("Conversion Factor is empty", viewModel.ConversionFactor.IsEmpty);

			viewModel.ConversionFactorString = "100 KG/M3";
			AssertEquals("Conversion Factor", new ConversionFactor(100, "KG", "M3"), viewModel.ConversionFactor);

			viewModel.ConversionFactorString = "100.01 KG/M3";
			AssertEquals("Conversion Factor", new ConversionFactor(100.01m, "KG", "M3"), viewModel.ConversionFactor);

			viewModel.ConversionFactorString = "100.000 KG/M3";
			AssertEquals("Conversion Factor", new ConversionFactor(100m, "KG", "M3"), viewModel.ConversionFactor);

			viewModel.ConversionFactorString = "100.000123 KG/M3";
			AssertEquals("Conversion Factor", new ConversionFactor(100.000123m, "KG", "M3"), viewModel.ConversionFactor);
		}

		IConversionFactorValidation ValidationMock
		{
			get
			{
				if (validationMock == null)
				{
					validationMock = new Mock<IConversionFactorValidation>().Object;
				}

				return validationMock;
			}
		}

		IConversionFactorLookups LookupsMock
		{
			get
			{
				if (lookupsMock == null)
				{
					lookupsMock = new Mock<IConversionFactorLookups>().Object;
				}

				return lookupsMock;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ConversionFactorViewModel(t => new ConversionFactorLookups(t), t => new ConversionFactorValidation(t));
		}

		IConversionFactorValidation validationMock;
		IConversionFactorLookups lookupsMock;
	}
}
