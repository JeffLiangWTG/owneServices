using System;
using Enterprise.Metadata.Business.Extension;
using NUnit.Framework;

namespace Enterprise.Metadata.Business.Tests
{
	class TypeExtensionTest : TestCase
	{
		public void TestGetFirstAttributeInHierarchy()
		{
			var attribute = typeof(Child).GetFirstAttributeInHierarchy<Test2Attribute>();
			AssertNotNull(attribute);
			AssertEquals("Parent", ((Test2Attribute)attribute).Level);

			var stopBefore = typeof(Parent);
			attribute = typeof(Child).GetFirstAttributeInHierarchy<Test2Attribute>(stopBefore);
			AssertNull(attribute);
			attribute = typeof(Child).GetFirstAttributeInHierarchy<NotAppliedAttribute>();
			AssertNull(attribute);
		}

		public void TestGetFirstAttribute()
		{
			var attribute = typeof(Child).GetFirstAttribute<Test1Attribute>();
			AssertNotNull(attribute);
			AssertEquals("Child", ((Test1Attribute)attribute).Level);
			attribute = typeof(Child).GetFirstAttribute<NotAppliedAttribute>();
			AssertNull(attribute);
		}

		public void TestGetInterfacesInHierarchy()
		{
			var interfacePrefix = "I";
			var interfaceSuffix = "Interface";
			var foundInterfaces = typeof(Child).GetInterfacesInHierarchy(interfacePrefix, interfaceSuffix);
			AssertEquals("Number of found tnterfaces", 2, foundInterfaces.Length);
			AssertEquals("Type of found interface", typeof(IChildInterface), foundInterfaces[0]);
			AssertEquals("Type of found interface", typeof(IParentInterface), foundInterfaces[1]);

			var stopBefore = typeof(Parent);
			foundInterfaces = typeof(Child).GetInterfacesInHierarchy(interfacePrefix, interfaceSuffix, stopBefore);
			AssertEquals("Number of found tnterfaces", 1, foundInterfaces.Length);
			AssertEquals("Type of found interface", typeof(IChildInterface), foundInterfaces[0]);
		}

		#region Type and Attribute

		[Test2("Parent")]
		class Parent : IParentInterface { }
		[Test1("Child")]
		class Child : Parent, IChildInterface { }

		#region Attribute

		[AttributeUsage(AttributeTargets.Class)]
		class Test1Attribute : Attribute
		{
			public Test1Attribute(string level)
			{ Level = level; }
			public string Level;
		}

		[AttributeUsage(AttributeTargets.Class)]
		class Test2Attribute : Attribute
		{
			public Test2Attribute(string level)
			{ Level = level; }
			public string Level;
		}

		[AttributeUsage(AttributeTargets.Class)]
		class NotAppliedAttribute : Attribute { }

		#endregion

		#region interface

		interface IParentInterface { }
		interface IChildInterface { }

		#endregion

		#endregion
	}
}
