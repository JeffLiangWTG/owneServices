#if DEBUG
using System;
using System.Collections.Generic;
using CargoWise.Common;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.ComponentModel.Testing
{
	public class TypeTypeConverterTests : TestCase
	{
		#region ConvertFrom

		public void TestCanConvertFrom()
		{
			AssertEquals(true, converter.CanConvertFrom(typeof(string)));
			AssertEquals(false, converter.CanConvertFrom(typeof(Type)));
		}

		public void TestConvertFrom()
		{
			Type voidType = (Type)converter.ConvertFrom("void");
			AssertEquals("void type returned", typeof(void), voidType);

			Type type = (Type)converter.ConvertFrom("System.Int32");
			AssertEquals("int type returned", typeof(int), type);

			Type fakeType = (Type)converter.ConvertFrom("fake_type");
			AssertEquals("fake_type", fakeType.FullName);

			Type emptyType = (Type)converter.ConvertFrom("  ");
			AssertEquals("Should return null if the type name is an empty string otherwise .net will make it typeof(void) which the designer serialiser will throw a NullReferenceException", null, emptyType);

			Type nestedType = (Type)converter.ConvertFrom(typeof(TypeTypeConverterTests).FullName + "." + nameof(NestedType));
			AssertEquals("Should find nested type, even tho we're not using a plus (+)", true, (nestedType is TypeNameHolder));
		}

		public void TestConvertFrom_PrimitiveType()
		{
			AssertEquals(typeof(int), converter.ConvertFrom("int"));
			AssertEquals(typeof(float), converter.ConvertFrom("float"));
			AssertEquals(typeof(string), converter.ConvertFrom("string"));
			AssertEquals(typeof(decimal), converter.ConvertFrom("decimal"));
		}

		public void TestConvertFrom_NullableType()
		{
			AssertEquals(typeof(int?), converter.ConvertFrom("int?"));
		}

		public void TestConvertFrom_GenericType()
		{
			Type expectedRuntimeType = typeof(Dictionary<List<int?>, Exception>);
			AssertEquals(true, expectedRuntimeType == (Type)converter.ConvertFrom("System.Collections.Generic.Dictionary<System.Collections.Generic.List<int?>,System.Exception>"));
		}

		public void TestConvertFrom_FakeGenericType()
		{
			TestConvertFrom_FakeGenericType("System.Collections.Generic.Dictionary<System.Collections.Generic.List<[FakeTypeName]>, System.Exception>");
			TestConvertFrom_FakeGenericType("System.Collections.Generic.Dictionary < System.Collections.Generic.List<[FakeTypeName]>, System.Exception>");
			TestConvertFrom_FakeGenericType("System.Collections.Generic.Dictionary`2 [ [ System.Collections.Generic.List`1 [ FakeTypeName ] ] , [ " + typeof(Exception).AssemblyQualifiedName + " ] ]");
		}

		void TestConvertFrom_FakeGenericType(string typeName)
		{
			Type type = (Type)converter.ConvertFrom(typeName);
			AssertEquals("FullName", "System.Collections.Generic.Dictionary`2[System.Collections.Generic.List`1[FakeTypeName],[" + typeof(Exception).AssemblyQualifiedName + "]]", type.FullName);
			AssertEquals("GetGenericArguments().Length", 2, type.GetGenericArguments().Length);
			AssertEquals("GetGenericArguments()[0]", new TypeNameHolder("System.Collections.Generic.List`1[FakeTypeName]"), type.GetGenericArguments()[0]);
			AssertEquals("GetGenericArguments()[1]", typeof(Exception), type.GetGenericArguments()[1]);
		}

		public void TestConvertFrom_ReturnFakeGenericType_WhenGenericConstraintViolated()
		{
			Type type = (Type)converter.ConvertFrom("System.Nullable`1[System.Object]");
#if NETFRAMEWORK
			AssertEquals("FullName", "System.Nullable`1[System.Object, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]", type.FullName);
#else
			AssertEquals("FullName", "System.Nullable`1[System.Object, System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]", type.FullName);
#endif

			AssertEquals("GetGenericArguments().Length", 1, type.GetGenericArguments().Length);
			AssertEquals("GetGenericArguments()[0]", typeof(object), type.GetGenericArguments()[0]);
		}

#endregion

		#region ConvertTo

		public void TestCanConvertTo()
		{
			AssertEquals(false, converter.CanConvertFrom(typeof(Type)));
			AssertEquals(true, converter.CanConvertFrom(typeof(string)));
		}

		public void TestConvertTo()
		{
			AssertEquals("System.Int32", converter.ConvertTo(typeof(int), typeof(string)));
			AssertEquals("This happens in the DataGridViewTextBoxColumn sometimes", "", converter.ConvertTo(DBNull.Value, typeof(string)));
		}

		public void TestConvertTo_NullableType()
		{
			AssertEquals("System.Nullable`1[System.Int32]", converter.ConvertTo(typeof(int?), typeof(string)));
		}

		public void TestConvertTo_GenericType()
		{
			AssertEquals("System.Collections.Generic.Dictionary`2", converter.ConvertTo(typeof(Dictionary<,>), typeof(string)));
			AssertEquals("System.Collections.Generic.Dictionary`2[System.Collections.Generic.List`1[System.Int32],System.Exception]", converter.ConvertTo(typeof(Dictionary<List<int>, Exception>), typeof(string)));
		}

		#endregion

		#region Implementation

		[CodeAlive("This type is used dynamically with TypeTypeConverter")]
		public class NestedType { }

		readonly TypeTypeConverter converter = new TypeTypeConverter();

		#endregion
	}
}
#endif
