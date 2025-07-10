using System;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ValidationCacheTest : TestCaseWithFactory
	{
		public void TestHasValidationBeenRunWorksWithNullObject()
		{
			ValidationCache cache = new ValidationCache();
			DummyBusinessObject bizO = Factory.GetNull<DummyBusinessObject>();
			cache.BeginValidation(bizO);

			AssertEquals(false, cache.HasValidationBeenRun(bizO.Z0_CodeInfo));
			cache.SetHasValidationBeenRun(bizO.Z0_CodeInfo);
			AssertEquals(true, cache.HasValidationBeenRun(bizO.Z0_CodeInfo));
		}

		public void TestHasValidationBeenRun()
		{
			ValidationCache cache = new ValidationCache();
			DummyBusinessObject bizO = Factory.New<DummyBusinessObject>();
			cache.BeginValidation(bizO);

			AssertEquals(false, cache.HasValidationBeenRun(bizO.Z0_CodeInfo));
			cache.SetHasValidationBeenRun(bizO.Z0_CodeInfo);
			AssertEquals(true, cache.HasValidationBeenRun(bizO.Z0_CodeInfo));
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestBeginPreSaveValidationArgument()
		{
			ValidationCache cache = new ValidationCache();
			cache.BeginValidation((BusinessObject)null);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestBeginPreSaveValidationArgument2()
		{
			ValidationCache cache = new ValidationCache();
			cache.BeginValidation((ZPropertyInfo)null);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestEndPreSaveValidationArgument()
		{
			ValidationCache cache = new ValidationCache();
			cache.EndValidation((BusinessObject)null);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestEndPreSaveValidationArgument2()
		{
			ValidationCache cache = new ValidationCache();
			cache.EndValidation((ZPropertyInfo)null);
		}
	}
}
