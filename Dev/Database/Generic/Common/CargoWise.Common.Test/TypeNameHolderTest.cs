using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;

namespace CargoWise.Common
{
	class TypeNameHolderTest : TestCase
	{
		public void TestFullName()
		{
			AssertEquals("FullName", "full.name", new TypeNameHolder("full.name", ServiceProvider).FullName);
			AssertEquals("Name", "name", new TypeNameHolder("full.name", ServiceProvider).Name);
			AssertEquals("Name", "name", new TypeNameHolder("name", ServiceProvider).Name);
			AssertEquals("Namespace", "full", new TypeNameHolder("full.name", ServiceProvider).Namespace);
			AssertEquals("Namespace", "", new TypeNameHolder("name", ServiceProvider).Namespace);
			AssertEquals("Equals", new TypeNameHolder("full.name", ServiceProvider), new TypeNameHolder("full.name", ServiceProvider));
		}

		public void TestFullName_ForGenericType()
		{
			TestFullName_ForGenericType("NTier.Business.FakeGenericType`1");
		}

		//Unit test removed as it violates the CodeContracts pre-requisites. The business logic of defining a type being the GenericTypeDefinition requires 
		//that the type name contains the '`' character in the name.
		//public void TestFullName_ForGenericType2()
		//{ TestFullName_ForGenericType("NTier.Business.FakeGenericType"); }
		public void TestFullName_ForGenericType3()
		{
			TestFullName_ForGenericType("NTier.Business.FakeGenericType`2");
		}

		void TestFullName_ForGenericType(string baseName)
		{
			Type type = TypeNameHolder.MakeGenericType(new TypeNameHolder(baseName), new Type[] { new TypeNameHolder("FakeGenericArgument1"), new TypeNameHolder("FakeGenericArgument2"), typeof(Exception) });
			AssertEquals("FullName", "NTier.Business.FakeGenericType`3[FakeGenericArgument1,FakeGenericArgument2,[" + typeof(Exception).AssemblyQualifiedName + "]]", type.FullName);
			AssertEquals("Name", "FakeGenericType`3", type.Name);
			AssertEquals("Namespace", "NTier.Business", type.Namespace);
		}

		public void TestExistsThisTypeOrAnyGenericArgumentTypesInSolution_ForGenericArgumentType()
		{
			TypeNameHolder type = (TypeNameHolder)TypeNameHolder.MakeGenericType(new TypeNameHolder("FakeGenericType`1", true), new Type[] { new TypeNameHolder("FakeGenericArgument1", false), new TypeNameHolder("FakeGenericArgument2", false) });
			AssertEquals(true, type.ExistsThisTypeOrAnyGenericArgumentTypesInSolution);
			TypeNameHolder type2 = (TypeNameHolder)TypeNameHolder.MakeGenericType(typeof(List<>), new Type[] { new TypeNameHolder("FakeGenericArgument1`1", false), new TypeNameHolder("FakeGenericArgument2", true) });
			AssertEquals(true, type2.ExistsThisTypeOrAnyGenericArgumentTypesInSolution);
			TypeNameHolder type3 = (TypeNameHolder)TypeNameHolder.MakeGenericType(typeof(List<>), new Type[] { new TypeNameHolder("FakeGenericArgument1", false), new TypeNameHolder("FakeGenericArgument2", false) });
			AssertEquals(false, type3.ExistsThisTypeOrAnyGenericArgumentTypesInSolution);
			TypeNameHolder type4 = (TypeNameHolder)TypeNameHolder.MakeGenericType(new TypeNameHolder("FakeGenericType`1", false), new Type[] { new TypeNameHolder("FakeGenericArgument1", false), new TypeNameHolder("FakeGenericArgument2", false) });
			AssertEquals(false, type4.ExistsThisTypeOrAnyGenericArgumentTypesInSolution);
		}

		public void TestUnderlyingSystemType()
		{
			AssertEquals("CodeDomSerializer uses UnderlyingSystemType on Deserialize", "FakeType", new TypeNameHolder("FakeType").UnderlyingSystemType.FullName);
		}

		#region IsGenericType / IsGenericTypeDefinition / GetGenericTypeDefinition
		public void TestIsGenericType()
		{
			AssertEquals(true, new TypeNameHolder("System.Collections.List`1[System.Int32]").IsGenericType);
			AssertEquals(false, new TypeNameHolder("System.Collections.List`1").IsGenericType);
			AssertEquals(false, new TypeNameHolder("System.Int32").IsGenericType);
		}

		public void TestIsGenericTypeDefinition()
		{
			AssertEquals(false, new TypeNameHolder("System.Collections.List`1[System.Int32]").IsGenericTypeDefinition);
			AssertEquals(true, new TypeNameHolder("System.Collections.List`1").IsGenericTypeDefinition);
			AssertEquals(false, new TypeNameHolder("System.Int32").IsGenericTypeDefinition);
		}

		public void TestGetGenericTypeDefinition()
		{
			AssertEquals("System.Collections.List`1", new TypeNameHolder("System.Collections.List`1[System.Int32]").GetGenericTypeDefinition().FullName);
		}

		[ExpectNoExceptions]
		public void TestGetGenericTypeDefinition_WithNonGenericType()
		{
			try
			{
				new TypeNameHolder("System.Collections.List`1").GetGenericTypeDefinition();
				Fail("Expected an exception as the type is not a generic type");
			}
			catch (Exception ex)
			{
				if (ex.GetType().FullName != "System.InvalidOperationException" || ex.Message != "Not a generic type")
				{
					throw;
				}
			}

			try
			{
				new TypeNameHolder("System.Int32").GetGenericTypeDefinition();
				Fail("Expected an exception as the type is not a generic type");
			}
			catch (Exception ex)
			{
				if (ex.GetType().FullName != "System.InvalidOperationException" || ex.Message != "Not a generic type")
				{
					throw;
				}
			}
		}

		#endregion
		#region MakeGenericType
		public void TestMakeGenericType_ForFakeType()
		{
			Type type = TypeNameHolder.MakeGenericType(new TypeNameHolder("FakeGenericType`1"), new Type[] { typeof(int) });
			AssertEquals(1, type.GetGenericArguments().Length);
			AssertEquals(typeof(int), type.GetGenericArguments()[0]);
		}

		public void TestMakeGenericType_ForFakeArgumentType()
		{
			Type type = TypeNameHolder.MakeGenericType(typeof(List<>), new Type[] { new TypeNameHolder("GenericArgumentType") });
			AssertEquals(1, type.GetGenericArguments().Length);
			AssertEquals("GenericArgumentType", type.GetGenericArguments()[0].FullName);
		}

		public void TestMakeGenericType_ForNonFakeType()
		{
			Type type = TypeNameHolder.MakeGenericType(typeof(List<>), new Type[] { typeof(int) });
			AssertEquals(typeof(List<int>), type);
		}

		public void TestMakeGenericType_Equals()
		{
			Type type = TypeNameHolder.MakeGenericType(new TypeNameHolder("FakeGenericType`1"), new Type[] { new TypeNameHolder("FakeGenericArgument1"), new TypeNameHolder("FakeGenericArgument2"), typeof(Exception) });
			Type type2 = TypeNameHolder.MakeGenericType(new TypeNameHolder("FakeGenericType`1"), new Type[] { new TypeNameHolder("FakeGenericArgument1"), new TypeNameHolder("FakeGenericArgument2"), typeof(Exception) });
			Type type3 = TypeNameHolder.MakeGenericType(new TypeNameHolder("FakeGenericType`1"), new Type[] { new TypeNameHolder("FakeGenericArgument1"), new TypeNameHolder("DifferentFakeGenericArgument"), typeof(Exception) });
			AssertEquals("Equal", true, object.Equals(type, type2));
			AssertEquals("Not equal", false, object.Equals(type, type3));
		}

		#endregion
		#region ISerializable
		public void TestIsSerializable()
		{
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(OnCurrentDomain_AssemblyResolve);
			try
			{
				TypeNameHolder type = new TypeNameHolder("NS.SomeFakeType", true);

				JsonSerializerOptions settings = new JsonSerializerOptions();
				settings.Converters.Add(new MyTypeDelegatorConverter());

				string json = JsonSerializer.Serialize(type, settings);
				TypeNameHolder deserialisedType = JsonSerializer.Deserialize<TypeNameHolder>(json, settings);

				AssertEquals("Should deserialise correctly", type.FullName, deserialisedType.FullName);
				AssertEquals("Should deserialise correctly", type.ExistsThisTypeOrAnyGenericArgumentTypesInSolution, deserialisedType.ExistsThisTypeOrAnyGenericArgumentTypesInSolution);
			}
			finally
			{
				AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(OnCurrentDomain_AssemblyResolve);
			}
		}

		#endregion
		#region Test Classes
		class MockServiceProvider : IServiceProvider
		{
			object IServiceProvider.GetService(Type serviceType)
			{
				return null;
			}
		}

		public class MyTypeDelegatorConverter : JsonConverter<object>
		{
			public override bool CanConvert(Type objectType)
			{
				return typeof(TypeNameHolder).IsAssignableFrom(objectType);
			}

			public override object Read(ref Utf8JsonReader reader, Type objectType, JsonSerializerOptions options)
			{
				string assemblyName = null;
				string typeName = null;
				bool existsThisType = false;

				while (reader.Read())
				{
					if (reader.TokenType == JsonTokenType.EndObject || reader.TokenType != JsonTokenType.PropertyName)
					{
						break;
					}

					string propertyName = reader.GetString();

					reader.Read();
					switch (propertyName)
					{
						case "$assemblyName":
							assemblyName = reader.GetString();
							break;
						case "$typeName":
							typeName = reader.GetString();
							break;
						case "ExistsThisTypeOrAnyGenericArgumentTypesInSolution":
							existsThisType = reader.GetBoolean();
							break;
					}
				}

				_ = Assembly.Load(assemblyName);
				TypeNameHolder typeNameHolder = new TypeNameHolder(typeName);
				PropertyInfo existsProperty = typeof(TypeNameHolder).GetProperty(nameof(TypeNameHolder.ExistsThisTypeOrAnyGenericArgumentTypesInSolution));
				existsProperty.SetValue(typeNameHolder, existsThisType);

				return typeNameHolder;
			}

			public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
			{
				TypeNameHolder typeNameHolder = (TypeNameHolder)value;

				writer.WriteStartObject();

				writer.WriteString("$assemblyName", typeNameHolder.Assembly.FullName);
				writer.WriteString("$typeName", typeNameHolder.UnderlyingSystemType.FullName);
				writer.WriteBoolean("ExistsThisTypeOrAnyGenericArgumentTypesInSolution", typeNameHolder.ExistsThisTypeOrAnyGenericArgumentTypesInSolution);

				writer.WriteEndObject();
			}
		}

		#endregion
		#region Implementation
		IServiceProvider ServiceProvider
		{
			get
			{
				if (serviceProvider == null)
				{
					serviceProvider = new MockServiceProvider();
				}

				return serviceProvider;
			}
		}

		IServiceProvider serviceProvider;
		Assembly OnCurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			return Assembly.Load(args.Name);
		}
		#endregion
	}
}
