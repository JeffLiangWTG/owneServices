using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestsSubclassesOf(typeof(ITemporaryStorageBillValidationDecider))]
	public abstract class TemporaryStorageBillValidationDeciderAbstractTest<T> : TestCaseWithFactory
		where T : class, ITemporaryStorageBillValidationDecider, new()
	{
		public void TestIsGrossWeightCheckSupported()
		{
			var validationDecider = new T();
			AssertEquals(ExpectedIsGrossWeightCheckSupported, validationDecider.IsGrossWeightCheckSupported);
		}

		protected abstract bool ExpectedIsGrossWeightCheckSupported { get; }

		public void TestIsTypeOfBillDocumentCheckSupported()
		{
			var validationDecider = new T();
			AssertEquals(ExpectedIsTypeOfBillDocumentCheckSupported, validationDecider.IsTypeOfBillDocumentCheckSupported);
		}

		protected abstract bool ExpectedIsTypeOfBillDocumentCheckSupported { get; }

		public abstract void TestIsTypeOfBillDocumentMandatory();
		public abstract void TestIsABL_BillNumberMandatory();
		public abstract void TestAllowDuplicateTypeAndNumber();
		public abstract void TestIsConsignorOrgPKMandatory();
		public abstract void TestIsShipperNameMandatory();
		public abstract void TestIsShipperCountryMandatory();
		public abstract void TestIsShipperPostcodeMandatory();
		public abstract void TestIsShipperRegNoTypeMandatory();
		public abstract void TestIsConsigneeOrgPKMandatory();
		public abstract void TestIsConsigneeNameMandatory();
		public abstract void TestIsConsigneeCountryMandatory();
		public abstract void TestIsConsigneePostcodeMandatory();
		public abstract void TestIsConsigneeRegNoTypeMandatory();

		public void TestShouldValidateConsignorOrgPK()
		{
			var decider = new T();
			AssertEquals(ExpectedIsConsignorOrgPKCheckSupported, decider.IsConsignorOrgPKCheckSupported);
		}

		public void TestShouldValidateConsigneeOrgPK()
		{
			var decider = new T();
			AssertEquals(ExpectedIsConsigneeOrgPKCheckSupported, decider.IsConsigneeOrgPKCheckSupported);
		}

		public void TestShouldValidateShipperName()
		{
			var decider = new T();
			AssertEquals(ExpectedIsShipperNameCheckSupported, decider.IsShipperNameCheckSupported);
		}

		public void TestShouldValidateShipperCountry()
		{
			var decider = new T();
			AssertEquals(ExpectedIsShipperCountryCheckSupported, decider.IsShipperCountryCheckSupported);
		}

		public void TestShouldValidateShipperPostcode()
		{
			var decider = new T();
			AssertEquals(ExpectedIsShipperPostcodeCheckSupported, decider.IsShipperPostcodeCheckSupported);
		}

		public void TestShouldValidateShipperRegNoType()
		{
			var decider = new T();
			AssertEquals(ExpectedIsShipperRegNoTypeCheckSupported, decider.IsShipperRegNoTypeCheckSupported);
		}

		public void TestShouldValidateConsigneeName()
		{
			var decider = new T();
			AssertEquals(ExpectedIsConsigneeNameCheckSupported, decider.IsConsigneeNameCheckSupported);
		}

		public void TestShouldValidateConsigneeCountry()
		{
			var decider = new T();
			AssertEquals(ExpectedIsConsigneeCountryCheckSupported, decider.IsConsigneeCountryCheckSupported);
		}

		public void TestShouldValidateConsigneePostcode()
		{
			var decider = new T();
			AssertEquals(ExpectedIsConsigneePostcodeCheckSupported, decider.IsConsigneePostcodeCheckSupported);
		}

		public void TestShouldValidateConsigneeRegNoType()
		{
			var decider = new T();
			AssertEquals(ExpectedIsConsigneeRegNoTypeCheckSupported, decider.IsConsigneeRegNoTypeCheckSupported);
		}

		protected abstract bool ExpectedIsConsignorOrgPKCheckSupported { get; }
		protected abstract bool ExpectedIsConsigneeOrgPKCheckSupported { get; }

		protected abstract bool ExpectedIsShipperNameCheckSupported { get; }
		protected abstract bool ExpectedIsShipperCountryCheckSupported { get; }
		protected abstract bool ExpectedIsShipperPostcodeCheckSupported { get; }
		protected abstract bool ExpectedIsShipperRegNoTypeCheckSupported { get; }

		protected abstract bool ExpectedIsConsigneeNameCheckSupported { get; }
		protected abstract bool ExpectedIsConsigneeCountryCheckSupported { get; }
		protected abstract bool ExpectedIsConsigneePostcodeCheckSupported { get; }
		protected abstract bool ExpectedIsConsigneeRegNoTypeCheckSupported { get; }
	}
}
