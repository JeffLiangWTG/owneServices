using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.USSalesTax.Implementation.Testing
{
	public class OverridableUSSalesTaxCalculatorTest : TestCaseWithFactory
	{
		public void TestAllInterfaceExposedMethodsDelegateToInner()
		{
			var invoice = ObjectCreator.CreateARInvoice<ARInvoice>("INV1234", ObjectCreator.AUD, 1m, ObjectCreator.AALSHI);
			var mockInner = new Mock<IUSSalesTaxCalculator>();
			var calculator = new OverridableUSSalesTaxCalculator(mockInner.Object);
			AssertSame("Inner should be same reference as was passed to constructor", mockInner.Object, calculator.Inner);

			calculator.IsEnabled(GlbBranch.CurrentBranch);
			mockInner.Verify(x => x.IsEnabled(It.IsNotNull<GlbBranch>()), Times.Once(), "IsEnabled() should delegate to Inner");

			calculator.GetConfiguration(GlbBranch.CurrentBranch);
			mockInner.Verify(x => x.GetConfiguration(It.IsNotNull<GlbBranch>()), Times.Once(), "GetConfiguration() should delegate to Inner");

			calculator.GetChargeCode(GlbBranch.CurrentBranch);
			mockInner.Verify(x => x.GetConfiguration(It.IsNotNull<GlbBranch>()), Times.Once(), "GetChargeCode() should delegate to Inner");

			var name = calculator.Name;
			mockInner.Verify(x => x.Name, Times.Once(), "Name property should delegate to Inner");

			calculator.ShouldShowMenuItemsOnInvoiceForm(invoice);
			mockInner.Verify(x => x.ShouldShowMenuItemsOnInvoiceForm(It.IsNotNull<InvoicingBase>()), Times.Once(), "ShouldShowMenuItemsOnInvoiceForm() should delegate to Inner");

			var calculateMenuText = calculator.CalculateMenuItemText;
			mockInner.Verify(x => x.CalculateMenuItemText, Times.Once(), "CalculateMenuItemText property should delegate to Inner");

			var submitMenuText = calculator.SubmitMenuItemText;
			mockInner.Verify(x => x.SubmitMenuItemText, Times.Once(), "SubmitMenuItemText property should delegate to Inner");

			calculator.CheckpointForCalculationMenuItem(invoice);
			mockInner.Verify(x => x.CheckpointForCalculationMenuItem(It.IsNotNull<InvoicingBase>()), Times.Once(), "CheckpointForCalculationMenuItem() should delegate to Inner");

			calculator.CheckpointForSubmitMenuItem(invoice);
			mockInner.Verify(x => x.CheckpointForSubmitMenuItem(It.IsNotNull<InvoicingBase>()), Times.Once(), "CheckpointForSubmitMenuItem() should delegate to Inner");

			var hint = calculator.MenuItemTroubleshootingHint;
			mockInner.Verify(x => x.MenuItemTroubleshootingHint, Times.Once(), "MenuItemTroubleshootingHint property should delegate to Inner");

			calculator.CalculateSalesTax(invoice);
			mockInner.Verify(x => x.CalculateSalesTax(It.IsNotNull<InvoicingBase>()), Times.Once(), "CalculateSalesTax() should delegate to Inner");

			calculator.SubmitSalesTax(invoice);
			mockInner.Verify(x => x.SubmitSalesTax(It.IsNotNull<InvoicingBase>()), Times.Once(), "SubmitSalesTax() should delegate to Inner");

			calculator.GetCurrentSalesTaxAmount(invoice);
			mockInner.Verify(x => x.GetCurrentSalesTaxAmount(It.IsNotNull<InvoicingBase>()), Times.Once(), "GetCurrentSalesTaxAmount() should delegate to Inner");
		}

		public void TestAllInterfaceExposedMethodsReturnNullZeroOrEmpty_WhenClientSpecificIsEmpty()
		{
			var invoice = ObjectCreator.CreateARInvoice<ARInvoice>("INV1234", ObjectCreator.AUD, 1m, ObjectCreator.AALSHI);
			using (SubstituteCollection(null))
			{
				var calculator = new OverridableUSSalesTaxCalculator();
				AssertEquals("When no client specific calculator is available, it should use Null calculator", "Null", calculator.Name);
				AssertEquals("When no client specific calculator is available, it should be disabled", false, calculator.IsEnabled(GlbBranch.CurrentBranch));
				AssertEquals("When no client specific calculator is available, it should be Off", ConfigurationStatus.Off, calculator.GetConfiguration(GlbBranch.CurrentBranch));
				var (chargePK, chargeCode) = calculator.GetChargeCode(GlbBranch.CurrentBranch);
				AssertEquals("When no client specific calculator is available, charge PK should be empty", ZGuid.Empty, chargePK);
				AssertEquals("When no client specific calculator is available, charge code should be empty", ZString.Empty, chargeCode);
				AssertEquals("When no client specific calculator is available, menu items should not be shown", false, calculator.ShouldShowMenuItemsOnInvoiceForm(invoice));
				AssertNull("When no client specific calculator is available, generic menu item text is used for calculation", calculator.CalculateMenuItemText);
				AssertNull("When no client specific calculator is available, generic menu item text is used for submission", calculator.SubmitMenuItemText);
				AssertEquals("When no client specific calculator is available, the Access Denied checkpoint should be used for calculation", "GLOBALAccessDenied", calculator.CheckpointForCalculationMenuItem(invoice).Code);
				AssertEquals("When no client specific calculator is available, the Access Denied checkpoint should be used for submission", "GLOBALAccessDenied", calculator.CheckpointForSubmitMenuItem(invoice).Code);
				AssertEquals("When no client specific calculator is available, hint text should be empty", string.Empty, calculator.MenuItemTroubleshootingHint.ToString());
				var (calculateResult, calculateEx) = calculator.CalculateSalesTax(invoice);
				AssertEquals("When no client specific calculator is available, sales tax is always zero", CalculationResult.Zero, calculateResult);
				AssertNull("When no client specific calculator is available, sales tax has no error", calculateEx);
				var (submitResult, submitEx) = calculator.SubmitSalesTax(invoice);
				AssertEquals("When no client specific calculator is available, sales tax is always zero", CalculationResult.Zero, submitResult);
				AssertNull("When no client specific calculator is available, sales tax has no error", submitEx);
			}
		}

		public void TestIsEnabled_WhenClientSpecificIsSet()
		{
			var mockClientSpecificCalculator = new Mock<IUSSalesTaxCalculator>();
			mockClientSpecificCalculator.Setup(x => x.IsEnabled(GlbBranch.CurrentBranch)).Returns(true);
			var mockClientSpecificFactory = new Mock<IUSSalesTaxCalculatorFactory>();
			mockClientSpecificFactory.Setup(x => x.Get()).Returns(mockClientSpecificCalculator.Object);
			using (SubstituteCollection(mockClientSpecificFactory.Object))
			{
				var calculator = new OverridableUSSalesTaxCalculator();
				AssertEquals("When client specific factory is available, it should delegate to it and return true", true, calculator.IsEnabled(GlbBranch.CurrentBranch));
			}

			mockClientSpecificCalculator.Setup(x => x.IsEnabled(GlbBranch.CurrentBranch)).Returns(false);
			using (SubstituteCollection(mockClientSpecificFactory.Object))
			{
				var calculator = new OverridableUSSalesTaxCalculator();
				AssertEquals("When client specific factory is available, it should delegate to it and return false", false, calculator.IsEnabled(GlbBranch.CurrentBranch));
			}
		}

		public void TestGetConfiguration_WhenClientSpecificIsSet()
		{
			var mockClientSpecificCalculator = new Mock<IUSSalesTaxCalculator>();
			mockClientSpecificCalculator.Setup(x => x.GetConfiguration(GlbBranch.CurrentBranch)).Returns(ConfigurationStatus.Off);
			var mockClientSpecificFactory = new Mock<IUSSalesTaxCalculatorFactory>();
			mockClientSpecificFactory.Setup(x => x.Get()).Returns(mockClientSpecificCalculator.Object);
			using (SubstituteCollection(mockClientSpecificFactory.Object))
			{
				var calculator = new OverridableUSSalesTaxCalculator();
				AssertEquals("When client specific calculator is available, it should delegate to it and return Off", ConfigurationStatus.Off, calculator.GetConfiguration(GlbBranch.CurrentBranch));
			}

			mockClientSpecificCalculator.Setup(x => x.GetConfiguration(GlbBranch.CurrentBranch)).Returns(ConfigurationStatus.Sandbox);
			using (SubstituteCollection(mockClientSpecificFactory.Object))
			{
				var calculator = new OverridableUSSalesTaxCalculator();
				AssertEquals("When client specific calculator is available, it should delegate to it and return Sandbox", ConfigurationStatus.Sandbox, calculator.GetConfiguration(GlbBranch.CurrentBranch));
			}

			mockClientSpecificCalculator.Setup(x => x.GetConfiguration(GlbBranch.CurrentBranch)).Returns(ConfigurationStatus.Production);
			using (SubstituteCollection(mockClientSpecificFactory.Object))
			{
				var calculator = new OverridableUSSalesTaxCalculator();
				AssertEquals("When client specific calculator is available, it should delegate to it and return Production", ConfigurationStatus.Production, calculator.GetConfiguration(GlbBranch.CurrentBranch));
			}
		}

		public void TestInner_HasScopeDefinedByFactory()
		{
			var singletonCalculator = new Mock<IUSSalesTaxCalculator>().Object;
			var mockClientSingletonFactory = new Mock<IUSSalesTaxCalculatorFactory>();
			mockClientSingletonFactory.Setup(x => x.Get()).Returns(singletonCalculator);
			using (SubstituteCollection(mockClientSingletonFactory.Object))
			{
				var calculator1 = new OverridableUSSalesTaxCalculator();
				var calculator2 = new OverridableUSSalesTaxCalculator();

				AssertSame("Inner calculator object created using default constructor should have singleton scope when factory return same instance", calculator1.Inner, calculator2.Inner);
			}

			var mockClientTransientFactory = new Mock<IUSSalesTaxCalculatorFactory>();
			mockClientTransientFactory.Setup(x => x.Get()).Returns(() => new Mock<IUSSalesTaxCalculator>().Object);
			using (SubstituteCollection(mockClientTransientFactory.Object))
			{
				var calculator1 = new OverridableUSSalesTaxCalculator();
				var calculator2 = new OverridableUSSalesTaxCalculator();

				var areSame = object.ReferenceEquals(calculator1.Inner, calculator2.Inner);
				Assert("Inner calculator object created using default constructor should have transient scope when factory return new instance", !areSame);
			}
		}

		#region Implementation

		IDisposable SubstituteCollection(IUSSalesTaxCalculatorFactory mock)
		{
			var list = ClearCollection();
			if (mock != null)
			{
				list.Add(mock);
			}

			return new DisposableAction(() => ClearCollection());

			ArrayList ClearCollection()
			{
				var innerList = ObjectFactory.Get<ListObject>("IUSSalesTaxCalculator_ClientSpecific");
				innerList.Clear();
				return innerList;
			}
		}

		TestObjectCreator ObjectCreator
			=> objectCreatorValue ?? (objectCreatorValue = new TestObjectCreator(Factory));
		TestObjectCreator objectCreatorValue;

		#endregion
	}
}
