#if DEBUG
using System;
using System.CodeDom;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using CargoWise.Common;
using CargoWise.ComponentModel.Design;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public partial class TypeFixCodeDomSerialiserTests : TestCase
	{
		[RequiresSoftware(RequiredSoftware.VisualStudio)]
		public void TestSerialise()
		{
			TestTypeFixCodeDomSerialiser serialiser = new TestTypeFixCodeDomSerialiser();
			object codeObject = serialiser.Serialize(new MockSerialisationManager(), new TestObject(new TypeNameHolder("fake_type")));
			AssertEquals("Should serialise the type properly", "fake_type", ((CodeTypeOfExpression)codeObject).Type.BaseType);
		}

		[RequiresSoftware(RequiredSoftware.VisualStudio)]
		public void TestDeserialise()
		{
			TestTypeFixCodeDomSerialiser serialiser = new TestTypeFixCodeDomSerialiser();
			Type returnedType = (Type)serialiser.Deserialize(new MockSerialisationManager(), new CodeTypeOfExpression("fake_type"));
			AssertEquals("Should deserialise the type properly to some fake type", "fake_type", returnedType.FullName);
		}

		#region TestCheckAppliedCorrectly

		public void TestCheckAppliedCorrectly()
		{
			TypeFixCodeDomSerializer.CheckAppliedCorrectly(typeof(TestInstance1));
			TypeFixCodeDomSerializer.CheckAppliedCorrectly(typeof(TestInstance2));
			try
			{
				TypeFixCodeDomSerializer.CheckAppliedCorrectly(null);
				Fail("Expected exception due to type being null");
			}
			catch (ArgumentNullException)
			{
				Assert(true);
			}
			try
			{
				TypeFixCodeDomSerializer.CheckAppliedCorrectly(typeof(TestInstanceNoSerialiser));
				Fail("Expected exception due to " + nameof(TypeFixCodeDomSerializer) + " not being applied");
			}
			catch (ArgumentException)
			{
				Assert(true);
			}
			try
			{
				TypeFixCodeDomSerializer.CheckAppliedCorrectly(typeof(TestInstanceSerializerAppliedIncorrectly));
				Fail("Expected exception due to " + nameof(TypeFixCodeDomSerializer) + " not being applied properly");
			}
			catch (ArgumentException)
			{
				Assert(true);
			}
			try
			{
				TypeFixCodeDomSerializer.CheckAppliedCorrectly(typeof(TestInstanceSerializerAppliedIncorrectly2));
				Fail("Expected exception due to " + nameof(TypeFixCodeDomSerializer) + " not being applied properly");
			}
			catch (ArgumentException)
			{
				Assert(true);
			}
		}

		[DesignerSerializer(typeof(MyCodeDomSerialiser), typeof(CodeDomSerializer))]
#pragma warning disable CA1052
		public class TestInstance1
#pragma warning restore CA1052
		{
			internal class MyCodeDomSerialiser : TypeFixCodeDomSerializer { public MyCodeDomSerialiser() : base(typeof(TestInstance1)) { } }
		}

		public class TestInstance2 : TestInstance1
		{
		}

		public class TestInstanceNoSerialiser
		{
		}

		[DesignerSerializer(typeof(TypeFixCodeDomSerializer), typeof(CodeDomSerializer))]
		public class TestInstanceSerializerAppliedIncorrectly
		{
		}

		[DesignerSerializer(typeof(MyCodeDomSerialiser), typeof(CodeDomSerializer))]
		public static class TestInstanceSerializerAppliedIncorrectly2
		{
			internal class MyCodeDomSerialiser : TypeFixCodeDomSerializer { public MyCodeDomSerialiser() : base(typeof(TestInstance1)) { } }
		}

		#endregion

		class TestObject
		{
			public TestObject(Type type)
			{ this.type = type; }

			Type type;
			public Type Type
			{
				get { return type; }
				set { type = value; }
			}
		}

		class TestTypeFixCodeDomSerialiser : TypeFixCodeDomSerializer
		{
			public TestTypeFixCodeDomSerialiser()
				: base(typeof(TestObject))
			{ }
		}

		class MockSerialisationManager : IDesignerSerializationManager
		{
			#region IDesignerSerializationManager Members

			public void ReportError(object errorInformation)
			{ throw new NotSupportedException(); }

			public void RemoveSerializationProvider(IDesignerSerializationProvider provider)
			{ throw new NotSupportedException(); }

			public event ResolveNameEventHandler ResolveName
			{
				add { throw new NotSupportedException(); }
				remove { throw new NotSupportedException(); }
			}

			public event EventHandler SerializationComplete
			{
				add { throw new NotSupportedException(); }
				remove { throw new NotSupportedException(); }
			}

			public void AddSerializationProvider(IDesignerSerializationProvider provider)
			{ throw new NotSupportedException(); }

			public string GetName(object value)
			{ throw new NotSupportedException(); }

			public ContextStack Context
			{ get { throw new NotSupportedException(); } }

			public void SetName(object instance, string name)
			{ throw new NotSupportedException(); }

			public object GetSerializer(Type objectType, Type serializerType)
			{ return new MockBaseCodeDomSerializer(); }

			public object CreateInstance(Type type, ICollection arguments, string name, bool addToContainer)
			{ throw new NotSupportedException(); }

			public PropertyDescriptorCollection Properties
			{ get { throw new NotSupportedException(); } }

			public object GetInstance(string name)
			{ throw new NotSupportedException(); }

			public Type GetType(string typeName)
			{ return Type.GetType(typeName); }

			#endregion

			#region IServiceProvider Members

			public object GetService(Type serviceType)
			{ return null; }

			#endregion
		}

		// note that this isn't a realistic way of serialising an entire object, but is good enough for test
		class MockBaseCodeDomSerializer : KCodeDomSerializer
		{
			public override object Serialize(IDesignerSerializationManager manager, object value)
			{ return new CodeTypeOfExpression(((TestObject)value).Type.FullName); }

			public override object Deserialize(IDesignerSerializationManager manager, object codeObject)
			{
				Type result = null;
				if (codeObject is CodeTypeOfExpression)
				{
					result = manager.GetType(((CodeTypeOfExpression)codeObject).Type.BaseType);
				}
				return result;
			}
		}
	}
}
#endif
