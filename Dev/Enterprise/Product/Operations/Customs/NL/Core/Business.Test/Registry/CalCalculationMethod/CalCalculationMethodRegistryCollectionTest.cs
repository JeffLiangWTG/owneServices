using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(CalCalculationMethodRegistryCollection))]
sealed class CalCalculationMethodRegistryMessageVersionRegistryNonPersistentCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CalCalculationMethodRegistryCollection>
{
	public void TestPreventNewAndRemove()
	{
		var collection = new CalCalculationMethodRegistryCollection();
		CombineAssertions(() =>
		{
			AssertEquals("AllowNew", false, collection.AllowNew);
			AssertEquals("AllowRemove", false, collection.AllowRemove);
		});
	}

	public void TestDefaultValues()
	{
		var defaults = CalCalculationMethodRegistryCollection.DefaultCollection;

		CombineAssertions(() =>
		{
			AssertEquals("Name: Based on Duties/VAT Calculation Method", CalculationMethodList.Codes.DUT, defaults.Cast<CalCalculationMethodRegistry>().First(x => x.CalculationMethodName == CalculationMethodList.Codes.DUT).CalculationMethodName);
			AssertEquals("Default: Based on Duties/VAT Calculation Method", true, defaults.Cast<CalCalculationMethodRegistry>().First(x => x.CalculationMethodName == CalculationMethodList.Codes.DUT).CalculationMethodDefault);
			AssertEquals("Name: Based on weight Calculation Method", CalculationMethodList.Codes.WGT, defaults.Cast<CalCalculationMethodRegistry>().First(x => x.CalculationMethodName == CalculationMethodList.Codes.WGT).CalculationMethodName);
			AssertEquals("Name: Default amount per entry Calculation Method", CalculationMethodList.Codes.DEF, defaults.Cast<CalCalculationMethodRegistry>().First(x => x.CalculationMethodName == CalculationMethodList.Codes.DEF).CalculationMethodName);
		});
	}

	public void TestDefaultValueChanged()
	{
		var methodRegistries = CalCalculationMethodRegistryCollection.DefaultCollection;

		CombineAssertions(() =>
		{
			var basedOnDutiesVat = methodRegistries.Cast<CalCalculationMethodRegistry>().First(x => x.CalculationMethodName == CalculationMethodList.Codes.DUT);
			var basedOnWeight = methodRegistries.Cast<CalCalculationMethodRegistry>().First(x => x.CalculationMethodName == CalculationMethodList.Codes.WGT);
			var defaultAmountPerEntry = methodRegistries.Cast<CalCalculationMethodRegistry>().First(x => x.CalculationMethodName == CalculationMethodList.Codes.DEF);

			AssertEquals("1 Default true: Based on Duties/VAT Calculation Method", true, basedOnDutiesVat.CalculationMethodDefault);
			AssertEquals("1 Default false: Based on weight Calculation Method", false, basedOnWeight.CalculationMethodDefault);
			AssertEquals("1 Default false: Default amount per entry Calculation Method", false, defaultAmountPerEntry.CalculationMethodDefault);

			basedOnWeight.CalculationMethodDefault = true;
			AssertEquals("2 Default false: Based on Duties/VAT Calculation Method", false, basedOnDutiesVat.CalculationMethodDefault);
			AssertEquals("2 Default true: Based on weight Calculation Method", true, basedOnWeight.CalculationMethodDefault);
			AssertEquals("2 Default false: Default amount per entry Calculation Method", false, defaultAmountPerEntry.CalculationMethodDefault);

			defaultAmountPerEntry.CalculationMethodDefault = true;
			AssertEquals("3 Default false: Based on Duties/VAT Calculation Method", false, basedOnDutiesVat.CalculationMethodDefault);
			AssertEquals("3 Default false: Based on weight Calculation Method", false, basedOnWeight.CalculationMethodDefault);
			AssertEquals("3 Default true: Default amount per entry Calculation Method", true, defaultAmountPerEntry.CalculationMethodDefault);
		});
	}

	public void TestCalculationMethodDefaultError()
	{
		var message = CalCalculationMethodRegistryCollection.CalculationMethodDefaultError;
		var methodRegistries = CalCalculationMethodRegistryCollection.DefaultCollection;
		var baseodOnDutiesVat = methodRegistries.Cast<CalCalculationMethodRegistry>().First(x => x.CalculationMethodName == CalculationMethodList.Codes.DUT);
		var baseodOnDutiesVatDefaultInfo = baseodOnDutiesVat.CalculationMethodDefaultInfo;
		var basedOnWeight = methodRegistries.Cast<CalCalculationMethodRegistry>().First(x => x.CalculationMethodName == CalculationMethodList.Codes.WGT);
		var basedOnWeightDefaultInfo = basedOnWeight.CalculationMethodDefaultInfo;
		var defaultAmountPerEntry = methodRegistries.Cast<CalCalculationMethodRegistry>().First(x => x.CalculationMethodName == CalculationMethodList.Codes.DEF);
		var defaultAmountPerEntryDefaultInfo = defaultAmountPerEntry.CalculationMethodDefaultInfo;

		CombineAssertions(() =>
		{
			methodRegistries.RunPreSaveValidation();
			AssertNoError("1 No error: Based on Duties/VAT Calculation Method", baseodOnDutiesVatDefaultInfo, message);
			AssertNoError("1 No error: Based on weight Calculation Method", basedOnWeightDefaultInfo, message);
			AssertNoError("1 No error: Default amount per entry Calculation Method", defaultAmountPerEntryDefaultInfo, message);

			baseodOnDutiesVat.CalculationMethodDefault = false;
			methodRegistries.RunPreSaveValidation();
			AssertHasError("1 Has error: Based on Duties/VAT Calculation Method", baseodOnDutiesVatDefaultInfo, message);
			AssertHasError("1 Has error: Based on weight Calculation Method", basedOnWeightDefaultInfo, message);
			AssertHasError("1 Has error: Default amount per entry Calculation Method", defaultAmountPerEntryDefaultInfo, message);

			basedOnWeight.CalculationMethodDefault = true;
			methodRegistries.RunPreSaveValidation();
			AssertNoError("2 No error: Based on Duties/VAT Calculation Method", baseodOnDutiesVatDefaultInfo, message);
			AssertNoError("2 No error: Based on weight Calculation Method", basedOnWeightDefaultInfo, message);
			AssertNoError("2 No error: Default amount per entry Calculation Method", defaultAmountPerEntryDefaultInfo, message);

			defaultAmountPerEntry.CalculationMethodDefault = true;
			methodRegistries.RunPreSaveValidation();
			AssertNoError("3 No error: Based on Duties/VAT Calculation Method", baseodOnDutiesVatDefaultInfo, message);
			AssertNoError("3 No error: Based on weight Calculation Method", basedOnWeightDefaultInfo, message);
			AssertNoError("3 No error: Default amount per entry Calculation Method", defaultAmountPerEntryDefaultInfo, message);

			defaultAmountPerEntry.CalculationMethodDefault = false;
			methodRegistries.RunPreSaveValidation();
			AssertHasError("2 Has error: Based on Duties/VAT Calculation Method", baseodOnDutiesVatDefaultInfo, message);
			AssertHasError("2 Has error: Based on weight Calculation Method", basedOnWeightDefaultInfo, message);
			AssertHasError("2 Has error: Default amount per entry Calculation Method", defaultAmountPerEntryDefaultInfo, message);
		});
	}

	protected override CalCalculationMethodRegistryCollection GetCollectionToTest() => new CalCalculationMethodRegistryCollection();

	protected override BusinessObject GetNewElementToAddToTheCollection() => new CalCalculationMethodRegistry();
}

[TestedType(typeof(CalCalculationMethodRegistryCollection))]
sealed class CalCalculationMethodRegistryCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<CalCalculationMethodRegistryCollection>
{
	protected override bool RequiresFactory => true;

	protected override bool RequiresFallbackLevel => false;

	protected override CalCalculationMethodRegistryCollection GetCollectionToTest() => new CalCalculationMethodRegistryCollection();

	protected override BusinessObject GetNewElementToAddToTheCollection() => new CalCalculationMethodRegistry();
}
