using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;

namespace Enterprise.Customs.Common.GUI.Testing
{
	public abstract class BorderWiseTariffFindBoxProviderTests : TestCaseWithFactory
	{
		public void TestGetTariffFindBoxWrapper_Should_ReturnFindBoxWrapperOfTypeFindBoxWrapperForBorderWise_When_ValidBusinessObjectAndBindingPropertyNameWithBorderWiseWebConfigured()
		{
			// Arrange
			var businessObject = (BusinessObject)Factory.New<DummyChildWithInterface>();
			var defaultFuncMock = new Mock<Func<IFindBoxPopup>>();
			defaultFuncMock.Setup(x => x.Invoke()).Returns(() => null);

			// Act
			var wrapper = BorderWiseTariffFindBoxProvider.GetTariffFindBoxWrapper(businessObject, "PropertyName", defaultFuncMock.Object);

			// Assert
			AssertEquals(typeof(FindBoxWrapperForBorderWise), wrapper.GetType());
			defaultFuncMock.Verify(x => x.Invoke(), Times.Never);
		}

		public void TestGetTariffFindBoxWrapper_Should_ReturnFindBoxWrapperOfTypeIFindBoxPopup_When_ExternalBorderComplianceToolIsConfiguredAsNone()
		{
			// Arrange
			ZArchitecture.Environment.DataRegistry.Instance.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
			var businessObject = (BusinessObject)Factory.New<DummyBusinessObject>();
			var defaultFuncMock = new Mock<Func<IFindBoxPopup>>();
			var bindingPropertyName = "PropertyName";
			var defaultFuncResult = new Mock<IFindBoxPopup>();
			defaultFuncMock.Setup(x => x.Invoke()).Returns(defaultFuncResult.Object);

			// Act
			var wrapper = BorderWiseTariffFindBoxProvider.GetTariffFindBoxWrapper(businessObject, bindingPropertyName, defaultFuncMock.Object);

			// Assert
			AssertNotNull("FindBox instance is not null", wrapper);
			AssertEquals(defaultFuncResult.Object, wrapper);
			defaultFuncMock.Verify(x => x.Invoke(), Times.Once);
		}

		public void TestGetTariffFindBoxWrapper_Should_ReturnsFindBoxWrapperOfTypeFindBoxWrapperForBorderWise_When_ValidTariffTypeWithBorderWiseWebConfigured()
		{
			// Arrange
			var tariffType = "TariffType";
			var defaultFuncMock = new Mock<Func<IFindBoxPopup>>();

			// Act
			var wrapper = BorderWiseTariffFindBoxProvider.GetTariffFindBoxWrapper(tariffType, defaultFuncMock.Object);

			// Assert
			AssertNotNull("FindBox instance is not null", wrapper);
			AssertEquals(typeof(FindBoxWrapperForBorderWise), wrapper.GetType());
		}

		public void TestGetTariffFindBoxWrapper_Should_ReturnsFindBoxWrapper_When_ValidTariffTypeAndCountryOverrideWithBorderWiseWebConfigured()
		{
			// Arrange
			var tariffType = "TariffType";
			var countryOverride = "AU";
			var defaultFuncMock = new Mock<Func<IFindBoxPopup>>();

			// Act
			var wrapper = BorderWiseTariffFindBoxProvider.GetTariffFindBoxWrapper(tariffType, countryOverride, defaultFuncMock.Object);

			// Assert
			AssertNotNull("FindBox instance is not null", wrapper);
			AssertEquals(typeof(FindBoxWrapperForBorderWise), wrapper.GetType());
		}

		public void TestGetTariffFindBoxWrapper_Should_ReturnsFindBoxWrapper_When_GivenAnApproriateAdditionalInfoObject()
		{
			// Arrange
			var defaultFuncMock = new Mock<Func<IFindBoxPopup>>();
			var addtionalData = new AdditionalDataForBorderWise("E", ZDateTime.Today, x => x.Replace(".", string.Empty));

			// Act
			var wrapper = BorderWiseTariffFindBoxProvider.GetTariffFindBoxWrapper(addtionalData, defaultFuncMock.Object);

			// Assert
			AssertNotNull("FindBox instance is not null", wrapper);
			AssertEquals(typeof(FindBoxWrapperForBorderWise), wrapper.GetType());
			AssertEquals("Wrapper.AdditionalData.ParameterForBorderWise", "E", ((FindBoxWrapperForBorderWise)wrapper).AdditionalData.ParameterForBorderWise);
		}

		public void TestGetTariffFindBoxWrapper_Should_ReturnsFindBoxWrapper_When_GivenBorderWiseFilters()
		{
			// Arrange
			var defaultFuncMock = new Mock<Func<IFindBoxPopup>>();
			var dateForDutyRate = ZDateTime.Today;
			var borderWiseFilters = new BorderWiseFilters("1234.56.78 03");
			borderWiseFilters.ImpExp = "I";
			borderWiseFilters.DateForDutyRate = ZDateTime.Today.ToString("yyyyMMdd", CultureInfo.CurrentCulture);
			borderWiseFilters.AdditionalData = new AdditionalDataForBorderWise(borderWiseFilters.ImpExp, dateForDutyRate);

			// Act
			var wrapper = BorderWiseTariffFindBoxProvider.GetTariffFindBoxWrapper(borderWiseFilters, defaultFuncMock.Object);

			// Assert
			AssertNotNull("FindBox instance is not null", wrapper);
			AssertEquals(typeof(FindBoxWrapperForBorderWise), wrapper.GetType());
			AssertEquals("Wrapper.AdditionalData.ParameterForBorderWise", "I", ((FindBoxWrapperForBorderWise)wrapper).AdditionalData.ParameterForBorderWise);
		}
	}
}
