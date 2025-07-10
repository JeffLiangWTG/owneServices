using System;
using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.ComponentModel.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class AddInfoSyncPropertyAttributeTest : TestCase
	{
		public void TestAttributeData()
		{
			var component = new TestComponent();
			var property = (KPropertyDescriptor)TypeDescriptor.GetProperties(component)["PropertyWithAddInfoSync"];
			var attribute = (AddInfoSyncPropertyAttribute)property.GetAttributesAllowMultiple(typeof(AddInfoSyncPropertyAttribute)).FirstOrDefault();
			AssertEquals("AddInfoName", "Greeting", attribute.AddInfoName);
			AssertEquals("AddInfoValueType", typeof(ZString), attribute.AddInfoValueType);
			AssertEquals("FastSearchName", "KD_Name", attribute.FastSearchName);
		}

		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
#if NET

				AssertExceptionThrown<ArgumentException>("addInfoName is empty", @"Value cannot be empty string (""""). (Parameter 'addInfoName')",
					() => new AddInfoSyncPropertyAttribute("", typeof(ZString)));

				AssertExceptionThrown<ArgumentException>("addInfoName is null", @"Value cannot be null. (Parameter 'addInfoName')",
					() => new AddInfoSyncPropertyAttribute(null, typeof(ZString)));

				AssertExceptionThrown<ArgumentException>("addInfoValueType is null", @"Value cannot be null. (Parameter 'addInfoValueType')",
					() => new AddInfoSyncPropertyAttribute("Hello", null));

				AssertExceptionThrown<ArgumentException>("fastSearchName is empty", "fastSearchName parameter should be either null or not empty",
					() => new AddInfoSyncPropertyAttribute("Hello", typeof(ZString), ""));
#else
				AssertExceptionThrown<ArgumentException>("addInfoName is empty", @"Value cannot be empty string ("""").
Parameter name: addInfoName", () => new AddInfoSyncPropertyAttribute("", typeof(ZString)));

				AssertExceptionThrown<ArgumentException>("addInfoName is null", @"Value cannot be null.
Parameter name: addInfoName", () => new AddInfoSyncPropertyAttribute(null, typeof(ZString)));

				AssertExceptionThrown<ArgumentException>("addInfoValueType is null", @"Value cannot be null.
Parameter name: addInfoValueType", () => new AddInfoSyncPropertyAttribute("Hello", null));

				AssertExceptionThrown<ArgumentException>("fastSearchName is empty", "fastSearchName parameter should be either null or not empty", () => new AddInfoSyncPropertyAttribute("Hello", typeof(ZString), ""));
#endif
			});
		}

		class TestComponent : KComponent
		{
			[AddInfoSyncProperty("Greeting", typeof(ZString), "KD_Name")]
			public string PropertyWithAddInfoSync
			{ get { return ""; } }
		}
	}
}
