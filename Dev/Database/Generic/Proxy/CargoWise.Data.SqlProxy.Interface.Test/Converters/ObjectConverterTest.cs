using CargoWise.Data.SqlProxy.Interface.Converters;

namespace CargoWise.Data.SqlProxy.Interface.Test.Converters;

class ObjectConverterTest
{
	[TestCaseSource(nameof(IntegerCastTestCases))]
	public void PrimitiveTypeCast(object value, Type targetType)
	{
		Assert.Throws<InvalidCastException>(() => _ = (int)value);

		var castMethod = typeof(ObjectConverter)
			.GetMethod(nameof(ObjectConverter.PrimitiveTypeCast), System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
		var genericCastMethod = castMethod!.MakeGenericMethod(targetType);
		var castedValue = genericCastMethod.Invoke(null, [value]);

		Assert.That(castedValue, Is.InstanceOf(targetType));
		Assert.That(castedValue, Is.EqualTo(value));
	}

	static IEnumerable<TestCaseData> IntegerCastTestCases()
	{
		yield return new TestCaseData(1L, typeof(byte));
		yield return new TestCaseData(1L, typeof(short));
		yield return new TestCaseData(1L, typeof(int));
		yield return new TestCaseData(1D, typeof(long));
		yield return new TestCaseData(1L, typeof(float));
		yield return new TestCaseData(1M, typeof(double));
		yield return new TestCaseData(1L, typeof(decimal));
		yield return new TestCaseData(new byte[] { 0x31, 0x44 }, typeof(byte[]));
	}
}
