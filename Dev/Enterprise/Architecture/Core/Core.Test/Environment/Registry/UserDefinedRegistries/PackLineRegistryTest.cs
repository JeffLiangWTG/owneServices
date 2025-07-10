using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class PackLineRegistryTest : TransactionedTestCase
	{
		public void TestPackLineCustomAttribute1Caption()
		{
			AssertEquals("PackLineCustomAttribute1Caption", "", Registry.Freight.PackLine.PackLineCustomAttribute1Caption);
			Registry.Freight.PackLine.PackLineCustomAttribute1Caption = "test";
			AssertEquals("PackLineCustomAttribute1Caption", "test", Registry.Freight.PackLine.PackLineCustomAttribute1Caption);
		}

		public void TestPackLineCustomAttribute2Caption()
		{
			AssertEquals("PackLineCustomAttribute2Caption", "", Registry.Freight.PackLine.PackLineCustomAttribute2Caption);
			Registry.Freight.PackLine.PackLineCustomAttribute2Caption = "test";
			AssertEquals("PackLineCustomAttribute2Caption", "test", Registry.Freight.PackLine.PackLineCustomAttribute2Caption);
		}

		public void TestPackLineCustomAttribute3Caption()
		{
			AssertEquals("PackLineCustomAttribute3Caption", "", Registry.Freight.PackLine.PackLineCustomAttribute3Caption);
			Registry.Freight.PackLine.PackLineCustomAttribute3Caption = "test";
			AssertEquals("PackLineCustomAttribute3Caption", "test", Registry.Freight.PackLine.PackLineCustomAttribute3Caption);
		}

		public void TestPackLineCustomAttribute1Hint()
		{
			AssertEquals("PackLineCustomAttribute1Hint", "Custom defined column caption can be changed in the Registry, under System/Freight/Pack Line.", Registry.Freight.PackLine.PackLineCustomAttribute1Hint);
			Registry.RawRegistry.PackLineCustomAttribute1Hint.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test");
			AssertEquals("PackLineCustomAttribute1Hint", "test", Registry.Freight.PackLine.PackLineCustomAttribute1Hint);
		}

		public void TestPackLineCustomAttribute2Hint()
		{
			AssertEquals("PackLineCustomAttribute2Hint", "Custom defined column caption can be changed in the Registry, under System/Freight/Pack Line.", Registry.Freight.PackLine.PackLineCustomAttribute2Hint);
			Registry.RawRegistry.PackLineCustomAttribute2Hint.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test");
			AssertEquals("PackLineCustomAttribute2Hint", "test", Registry.Freight.PackLine.PackLineCustomAttribute2Hint);
		}

		public void TestPackLineCustomAttribute3Hint()
		{
			AssertEquals("PackLineCustomAttribute3Hint", "Custom defined column caption can be changed in the Registry, under System/Freight/Pack Line.", Registry.Freight.PackLine.PackLineCustomAttribute3Hint);
			Registry.RawRegistry.PackLineCustomAttribute3Hint.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test");
			AssertEquals("PackLineCustomAttribute3Hint", "test", Registry.Freight.PackLine.PackLineCustomAttribute3Hint);
		}

		public void TestPackLineCustomAttribute4Hint()
		{
			AssertEquals("PackLineCustomAttribute4Hint", "Custom defined column caption can be changed in the Registry, under System/Freight/Pack Line.", Registry.Freight.PackLine.PackLineCustomAttribute4Hint);
			Registry.RawRegistry.PackLineCustomAttribute4Hint.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test");
			AssertEquals("PackLineCustomAttribute4Hint", "test", Registry.Freight.PackLine.PackLineCustomAttribute4Hint);
		}

		public void TestPackLineCustomDecimal1Hint()
		{
			AssertEquals("PackLineCustomDecimal1Hint", "Custom defined column caption can be changed in the Registry, under System/Freight/Pack Line.", Registry.Freight.PackLine.PackLineCustomDecimal1Hint);
			Registry.RawRegistry.PackLineCustomDecimal1Hint.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test");
			AssertEquals("PackLineCustomDecimal1Hint", "test", Registry.Freight.PackLine.PackLineCustomDecimal1Hint);
		}

		public void TestPackLineCustomDecimal2Hint()
		{
			AssertEquals("PackLineCustomDecimal2Hint", "Custom defined column caption can be changed in the Registry, under System/Freight/Pack Line.", Registry.Freight.PackLine.PackLineCustomDecimal2Hint);
			Registry.RawRegistry.PackLineCustomDecimal2Hint.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test");
			AssertEquals("PackLineCustomDecimal2Hint", "test", Registry.Freight.PackLine.PackLineCustomDecimal2Hint);
		}

		public void TestPackLineCustomDate1Hint()
		{
			AssertEquals("PackLineCustomDate1Hint", "Custom defined column caption can be changed in the Registry, under System/Freight/Pack Line.", Registry.Freight.PackLine.PackLineCustomDate1Hint);
			Registry.RawRegistry.PackLineCustomDate1Hint.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test");
			AssertEquals("PackLineCustomDate1Hint", "test", Registry.Freight.PackLine.PackLineCustomDate1Hint);
		}

		public void TestPackLineCustomFlag1Hint()
		{
			AssertEquals("PackLineCustomFlag1Hint", "Custom defined column caption can be changed in the Registry, under System/Freight/Pack Line.", Registry.Freight.PackLine.PackLineCustomFlag1Hint);
			Registry.RawRegistry.PackLineCustomFlag1Hint.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "test");
			AssertEquals("PackLineCustomFlag1Hint", "test", Registry.Freight.PackLine.PackLineCustomFlag1Hint);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Registry = new DataRegistry();
		}

		DataRegistry Registry;
		#endregion

	}
}
