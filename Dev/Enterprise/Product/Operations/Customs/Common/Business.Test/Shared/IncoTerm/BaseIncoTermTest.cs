using NUnit.Framework;
namespace Enterprise.Customs.Common.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.MasterFiles.Business;

	public abstract class IncoTermTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCanThisIncotermHaveThisCharge()
		{
			CombineAssertions(() =>
			{
				foreach (var chargeCode in IncoTermAndChargeFactory.GetAllCharges())
				{
					NUnit.Framework.Assert.That(IncoTermAndChargeFactory.CanThisIncoTermHaveThisCharge(IncotermToTest, chargeCode), Is.EqualTo(ExpectedValueForCanThisIncoTermHaveThisCharge(chargeCode)), "This Incoterm and this charge:" + chargeCode.Code);
				}
			});
		}

		[ExpectNoExceptions]
		public void TestIsThisChargeMandatory()
		{
			CombineAssertions(() =>
			{
				foreach (var chargeCode in IncoTermAndChargeFactory.GetAllCharges())
				{
					NUnit.Framework.Assert.That(IncoTermAndChargeFactory.IsThisChargeMandatory(IncotermToTest, chargeCode.Code), Is.EqualTo(ExpectedValueForThisChargeMandatory(chargeCode)), $"This Incoterm '{IncotermToTest}' and this charge for being mandatory :" + chargeCode.Code);
				}
			});
		}

		[ExpectNoExceptions]
		public void TestIsThisChargeRecommededForThisIncoterm()
		{
			CombineAssertions(() =>
			{
				foreach (var chargeCode in IncoTermAndChargeFactory.GetAllCharges())
				{
					NUnit.Framework.Assert.That(IncoTermAndChargeFactory.IsThisChargeRecommendedForThisIncoTerm(IncotermToTest, chargeCode.Code), Is.EqualTo(ExpectedValueForThisChargeRecommeded(chargeCode)), "This Incoterm and this charge for being recommended :" + chargeCode.Code);
				}
			});
		}

		public abstract void TestITOTIncoterm();

		public abstract void TestMissingMandatoryCharges();

		#region Implementation

		protected TestInvoice CreateInvoice()
		{
			var declaration = Factory.New<TestDeclaration>();
			declaration.IncoTermAndChargeFactory = IncoTermAndChargeFactory;
			var invoice = declaration.Invoices.AddNew();
			invoice.IncoTerm = IncotermToTest;
			return invoice;
		}

		protected abstract string IncotermToTest { get; }
		protected abstract bool ExpectedValueForCanThisIncoTermHaveThisCharge(ICustomsChargeCode charge);
		protected virtual bool ExpectedValueForThisChargeRecommeded(ICustomsChargeCode charge)
		{
			return charge.Code == CustomsChargeTypeList.Codes.OverseasFreight || charge.Code == CustomsChargeTypeList.Codes.OverseasInsurance;
		}

		protected virtual bool ExpectedValueForThisChargeMandatory(ICustomsChargeCode charge)
		{
			return false;
		}

		protected IncoTermAndCustomsChargeFactory IncoTermAndChargeFactory
		{
			get
			{
				if (fIncoTermAndCustomsChargeFactory == null)
				{
					fIncoTermAndCustomsChargeFactory = IncoTermAndCustomsChargeFactory.GetByCountryCode(CountryContext());
				}
				return fIncoTermAndCustomsChargeFactory;
			}
		}
		IncoTermAndCustomsChargeFactory fIncoTermAndCustomsChargeFactory;

		protected virtual string CountryContext()
		{
			return GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		}

		#endregion
	}
}
