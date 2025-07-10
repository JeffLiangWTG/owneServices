using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.DevTools;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(BizoProperty))]
	public class BizoPropertyTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new BizoProperty("Name", "Value");
		}

		public void TestIsKeyFieldChanged()
		{
			var eventRaised = false;
			var property = new BizoProperty("Name", "Value");
			property.IsKeyFieldChanged += (sender, args) => { eventRaised = true; };
			property.KeyField = true;

			AssertEquals(true, eventRaised);
			AssertEquals(false, property.IsSameValue);
			AssertEquals(false, property.NeedCompare);
		}

		public void TestValue_WhenCompareValueIsTheSame()
		{
			var property = new BizoProperty("Name", "Value", "Value");

			AssertEquals(false, property.ValueInfo.HasWarning("Different values found."));
			AssertEquals(true, property.IsSameValue);
			AssertEquals(true, property.NeedCompare);
		}

		public void TestValue_WhenCompareValueIsDifferent()
		{
			var property = new BizoProperty("Name", "Value", "DifferentValue");

			AssertEquals(true, property.ValueInfo.HasWarning("Different values found."));
			AssertEquals(false, property.IsSameValue);
			AssertEquals(true, property.NeedCompare);
		}

		public void TestValue_CompareZeros()
		{
			var property = new BizoProperty("Name", 0m, 0.000m);

			AssertEquals(false, property.ValueInfo.HasWarning("Different values found."));
			AssertEquals(true, property.IsSameValue);
			AssertEquals(true, property.NeedCompare);
		}

		public void TestIgnore_DefaultFalse()
		{
			var property = new BizoProperty("Name", "Value", "Value");

			AssertEquals(false, property.IgnoreField);
		}
	}
}
