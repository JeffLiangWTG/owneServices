using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.DataMapping.Testing
{
	sealed class ImportPropertyInfoImplTest : TestCase
	{
		public void TestIsMandatory()
		{
			var stuff = new ImportPropertyInfoImpl<Object>("TEST");
			Assert(!stuff.IsMandatory);
			stuff = new ImportPropertyInfoImpl<Object>("TEST", true);
			Assert(stuff.IsMandatory);
		}

		public void TestHeaderText()
		{
			AssertEquals("Decimal", new ImportPropertyInfoImpl<NonPersistentBizObjForTesting>("SomeDecimal").HeaderText);
			AssertEquals("SomeInt", new ImportPropertyInfoImpl<NonPersistentBizObjForTesting>("SomeInt").HeaderText);
		}

		class NonPersistentBizObjForTesting : NonPersistentBusinessObject
		{
			public NonPersistentBizObjForTesting() { }

			[ResourceStringData("DummyBizo|Z0_Decimal", Caption = "Decimal")]
			public ZDecimal SomeDecimal { get; set; }

			public ZInt SomeInt { get; set; }
		}
	}
}
