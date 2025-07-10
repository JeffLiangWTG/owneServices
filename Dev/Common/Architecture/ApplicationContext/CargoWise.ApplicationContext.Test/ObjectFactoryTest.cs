using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Application.Exceptions;
using CargoWise.Application.InversionOfControl;
using CargoWise.Common;
using Moq;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Application.Testing
{
	class CountrySpecific
	{
		public virtual string Code
		{
			get
			{
				return "Base";
			}
		}
	}

	[CodeAlive("WI00575476 Baseline")]
	class CountrySpecificAu : CountrySpecific
	{
		public override string Code
		{
			get
			{
				return "AU";
			}
		}
	}

	class ObjectFactoryTest : ThreadSafeAccessTestCase
	{
		public void TestGetWithoutSecurityCheckWithParams()
		{
			var instance = ObjectFactory.GetWithoutSecurityCheck("ITestInterfaceWithConstructorTakingStringAndInt", "hello", 12);
			AssertNotNull(instance);
		}

		public void TestNewWorksOnParameterslessConstructor()
		{
			var instance = ObjectFactory.New<ITestInterface>();
			AssertNotNull("ObjectFactory.New<>()", instance);
			AssertEquals("ObjectFactory.New<>().GetType()", typeof(TestImplementation), instance.GetType());
		}

		public void TestNewWorksOnParameterslessConstructor_Generic()
		{
			var genericInstance = ObjectFactory.New<ITestGenericInterface<int>>();
			AssertNotNull(genericInstance);
			AssertEquals(typeof(TestGenericImplementation<int>), genericInstance.GetType());
		}

		public void TestNewWorks_WhenSubstituted()
		{
			ITestInterface testObject = new TestImplementation();
			using (ObjectFactory.Substitute(testObject))
			{
				var instance = ObjectFactory.New<ITestInterface>();
				AssertNotNull("ObjectFactory.New<>()", instance);
				AssertEquals("Should have returned substituted object.", testObject, instance);
			}
		}

		public void TestNewWorks_WhenSubstituted_Generic()
		{
			ITestGenericInterface<string> testObject = new TestGenericImplementation<string>();
			using (ObjectFactory.Substitute(testObject))
			{
				var instance = ObjectFactory.New<ITestGenericInterface<string>>();
				AssertNotNull("ObjectFactory.New<>()", instance);
				AssertEquals("Should have returned substituted object.", testObject, instance);

				// Generic is resolved by name so cannot be overridden for multiple types
				AssertExceptionThrown<InvalidCastException>(() => ObjectFactory.New<ITestGenericInterface<int>>());
			}
		}

		public void TestNewWorksOnConstructorWith2Parameters()
		{
			var instance = ObjectFactory.New<ITestInterfaceWithConstructorTakingStringAndInt>("hello", 12);
			AssertNotNull("ObjectFactory.New<>()", instance);
			CombineAssertions("ObjectFactory.New<>()", delegate
			{
				AssertEquals("GetType()", typeof(TestImplementationWithConstructorTakingStringAndInt), instance.GetType());
				AssertEquals("instance.FirstParameter", "hello", instance.FirstParameter);
				AssertEquals("instance.SecondParameter", 12, instance.SecondParameter);
			});
		}

		public void TestNewWorksOnConstructorWith2Parameters_Generic()
		{
			var instance = ObjectFactory.New<ITestGenericInterfaceWithConstructorTakingTwoParameters<string, int>>("hello", 12);
			AssertNotNull("ObjectFactory.New<>()", instance);
			CombineAssertions("ObjectFactory.New<>()", delegate
			{
				AssertEquals("GetType()", typeof(TestGenericImplementationWithConstructorTakingTwoParameters<string, int>), instance.GetType());
				AssertEquals("instance.FirstParameter", "hello", instance.FirstParameter);
				AssertEquals("instance.SecondParameter", 12, instance.SecondParameter);
			});
		}

		public void TestConstructorSelectionAmongMultipleOverloads()
		{
			// Act
			ITestInterfaceWithMultipleOverloads result1 = ObjectFactory.Get<ITestInterfaceWithMultipleOverloads>("ITestInterfaceWithMultipleOverloads", new DisposableAction(() => { }));
			ITestInterfaceWithMultipleOverloads result2 = ObjectFactory.Get<ITestInterfaceWithMultipleOverloads>("ITestInterfaceWithMultipleOverloads", new BufferedStream(new MemoryStream()));
			ITestInterfaceWithMultipleOverloads result1again = ObjectFactory.Get<ITestInterfaceWithMultipleOverloads>("ITestInterfaceWithMultipleOverloads", new DisposableAction(() => { }));
			ITestInterfaceWithMultipleOverloads result3 = ObjectFactory.Get<ITestInterfaceWithMultipleOverloads>("ITestInterfaceWithMultipleOverloads", new MemoryStream());
			ITestInterfaceWithMultipleOverloads result4 = ObjectFactory.Get<ITestInterfaceWithMultipleOverloads>("ITestInterfaceWithMultipleOverloads", new MemoryStream(), new MemoryStream());
			ITestInterfaceWithMultipleOverloads result5 = ObjectFactory.Get<ITestInterfaceWithMultipleOverloads>("ITestInterfaceWithMultipleOverloads", new MemoryStream(), new MemoryStream(), new MemoryStream());

			ITestInterfaceWithMultipleOverloads result1againagain = ObjectFactory.Get<ITestInterfaceWithMultipleOverloads>("ITestInterfaceWithMultipleOverloads", new DisposableAction(() => { }));
			ITestInterfaceWithMultipleOverloads result2again = ObjectFactory.Get<ITestInterfaceWithMultipleOverloads>("ITestInterfaceWithMultipleOverloads", new BufferedStream(new MemoryStream()));
			ITestInterfaceWithMultipleOverloads result3again = ObjectFactory.Get<ITestInterfaceWithMultipleOverloads>("ITestInterfaceWithMultipleOverloads", new MemoryStream());
			ITestInterfaceWithMultipleOverloads result4again = ObjectFactory.Get<ITestInterfaceWithMultipleOverloads>("ITestInterfaceWithMultipleOverloads", new MemoryStream(), new MemoryStream());
			ITestInterfaceWithMultipleOverloads result5again = ObjectFactory.Get<ITestInterfaceWithMultipleOverloads>("ITestInterfaceWithMultipleOverloads", new MemoryStream(), new MemoryStream(), new MemoryStream());

			// Assert
			AssertEquals("first", result1.Constructor);
			AssertEquals("first", result1again.Constructor);
			AssertEquals("second", result2.Constructor);
			AssertEquals("third", result3.Constructor);
			AssertEquals("fourth", result4.Constructor);
			AssertEquals("fifth", result5.Constructor);

			AssertEquals("first", result1againagain.Constructor);
			AssertEquals("second", result2again.Constructor);
			AssertEquals("third", result3again.Constructor);
			AssertEquals("fourth", result4again.Constructor);
			AssertEquals("fifth", result5again.Constructor);
		}

		public void TestConstructorSelectionAmongMultipleOverloads_Generic()
		{
			// Act
			var result1 = ObjectFactory.Get<ITestGenericWithMultipleOverloads<string>>("ITestGenericWithMultipleOverloads", "first");
			var result2 = ObjectFactory.Get<ITestGenericWithMultipleOverloads<string>>("ITestGenericWithMultipleOverloads", new BufferedStream(new MemoryStream()));
			var result1again = ObjectFactory.Get<ITestGenericWithMultipleOverloads<string>>("ITestGenericWithMultipleOverloads", "firstAgain");
			var result3 = ObjectFactory.Get<ITestGenericWithMultipleOverloads<string>>("ITestGenericWithMultipleOverloads", new MemoryStream());
			var result4 = ObjectFactory.Get<ITestGenericWithMultipleOverloads<string>>("ITestGenericWithMultipleOverloads", new MemoryStream(), new MemoryStream());
			var result5 = ObjectFactory.Get<ITestGenericWithMultipleOverloads<string>>("ITestGenericWithMultipleOverloads", new MemoryStream(), new MemoryStream(), new MemoryStream());

			var result1againagain = ObjectFactory.Get<ITestGenericWithMultipleOverloads<int>>("ITestGenericWithMultipleOverloads", 5);
			var result2again = ObjectFactory.Get<ITestGenericWithMultipleOverloads<int>>("ITestGenericWithMultipleOverloads", new BufferedStream(new MemoryStream()));
			var result3again = ObjectFactory.Get<ITestGenericWithMultipleOverloads<int>>("ITestGenericWithMultipleOverloads", new MemoryStream());
			var result4again = ObjectFactory.Get<ITestGenericWithMultipleOverloads<int>>("ITestGenericWithMultipleOverloads", new MemoryStream(), new MemoryStream());
			var result5again = ObjectFactory.Get<ITestGenericWithMultipleOverloads<int>>("ITestGenericWithMultipleOverloads", new MemoryStream(), new MemoryStream(), new MemoryStream());

			// Assert
			AssertEquals("first", result1.Constructor);
			AssertEquals("firstAgain", result1again.Constructor);
			AssertEquals("second", result2.Constructor);
			AssertEquals("third", result3.Constructor);
			AssertEquals("fourth", result4.Constructor);
			AssertEquals("fifth", result5.Constructor);

			AssertEquals("5", result1againagain.Constructor);
			AssertEquals("second", result2again.Constructor);
			AssertEquals("third", result3again.Constructor);
			AssertEquals("fourth", result4again.Constructor);
			AssertEquals("fifth", result5again.Constructor);
		}

		public void TestNullArgumentPassedToConstructors()
		{
			// Arrange
			// Act
			ITestInterfaceSupportingNullArguments result = ObjectFactory.Get<ITestInterfaceSupportingNullArguments>("ITestInterfaceSupportingNullArguments", new object[] { null });
			// Assert
			AssertEquals("first", result.Constructor);
			AssertExceptionThrown<CannotCreateObjectException>(() => ObjectFactory.Get<ITestInterfaceWithAmbigousConstructors>("ITestInterfaceWithAmbigousConstructors", new object[] { null }));
		}

		public void TestNullArgumentPassedToConstructors_Generic()
		{
			// Arrange
			// Act
			var result = ObjectFactory.Get<ITestGenericSupportingNullArguments<int>>("ITestGenericSupportingNullArguments", new object[] { null });
			// Assert
			AssertEquals("first", result.Constructor);
			AssertExceptionThrown<CannotCreateObjectException>(() => ObjectFactory.Get<ITestGenericWithAmbigousConstructors<int>>("ITestGenericWithAmbigousConstructors", new object[] { null }));
		}

		public void TestNewBlowsOnMultipleConstructors()
		{
			AssertExceptionThrown(typeof(CannotLoadObjectTypeException), "New<T> can only be used where there is only 1 public constructor. TypeName [CargoWise.Application.Testing.TestImplementationWith2Constructors] has 2 constructors.", () => ObjectFactory.New<ITestInterfaceWith2Constructors>("hello", 12));
		}

		public void TestNewBlowsOnMultipleConstructors_Generic()
		{
			AssertExceptionThrown(typeof(CannotLoadObjectTypeException), () => ObjectFactory.New<ITestGenericWith2Constructors<string>>("hello", 12));
		}

		public void TestConfigurationIsOk()
		{
			// just to trigger configuration loading
			Assert(!ObjectFactory.Contains("empty"));
		}

		public void TestTemporalRegistration()
		{
			var singleton = new TestImplementation();
			var nameSingleton = new TestImplementation();
			using (ObjectFactory.Substitute<ITestInterface>(singleton))
			{
				using (ObjectFactory.Substitute("TestSubstitutedInterfaceName", nameSingleton))
				{
					var singleton1 = ObjectFactory.Get<ITestInterface>();
					var singleton2 = ObjectFactory.Get<ITestInterface>();
					var singleton3 = ObjectFactory.Get<ITestInterface>("TestSubstitutedInterfaceName");
					Assert(singleton1 != null && singleton1.GetType() == typeof(TestImplementation));
					Assert(singleton2 != null && singleton2.GetType() == typeof(TestImplementation));
					Assert(singleton3 != null && singleton3.GetType() == typeof(TestImplementation));
					Assert(singleton == singleton1);
					Assert(singleton1 == singleton2);
					Assert(singleton3 == nameSingleton);
				}
			}
		}

		public void TestTemporalRegistration_Generic()
		{
			var singleton = new TestGenericImplementation<int>();
			var nameSingleton = new TestGenericImplementation<int>();
			using (ObjectFactory.Substitute<ITestGenericInterface<int>>(singleton))
			{
				using (ObjectFactory.Substitute("TestSubstitutedInterfaceName", nameSingleton))
				{
					var singleton1 = ObjectFactory.Get<ITestGenericInterface<int>>();
					var singleton2 = ObjectFactory.Get<ITestGenericInterface<int>>();
					var singleton3 = ObjectFactory.Get<ITestGenericInterface<int>>("TestSubstitutedInterfaceName");
					Assert(singleton1 != null && singleton1.GetType() == typeof(TestGenericImplementation<int>));
					Assert(singleton2 != null && singleton2.GetType() == typeof(TestGenericImplementation<int>));
					Assert(singleton3 != null && singleton3.GetType() == typeof(TestGenericImplementation<int>));
					Assert(singleton == singleton1);
					Assert(singleton1 == singleton2);
					Assert(singleton3 == nameSingleton);
				}
			}
		}

		public void TestGetByType()
		{
			var singleton = new TestImplementation();
			using (ObjectFactory.Substitute<ITestInterface>(singleton))
			{
				var singleton1 = ObjectFactory.Get(typeof(ITestInterface));
				var singleton2 = ObjectFactory.Get(typeof(ITestInterface));

				AssertType<TestImplementation>(singleton1);
				AssertType<TestImplementation>(singleton2);
				AssertSame(singleton1, singleton2);
			}
		}

		public void TestGetByType_Generic()
		{
			var singleton = new TestGenericImplementation<int>();
			using (ObjectFactory.Substitute<ITestGenericInterface<int>>(singleton))
			{
				var singleton1 = ObjectFactory.Get(typeof(ITestGenericInterface<int>));
				var singleton2 = ObjectFactory.Get(typeof(ITestGenericInterface<int>));

				AssertType<TestGenericImplementation<int>>(singleton1);
				AssertType<TestGenericImplementation<int>>(singleton2);
				AssertSame(singleton1, singleton2);
			}
		}

		public void TestHasBeenSubstituted()
		{
			var obj = new TestImplementation();
			Assert(!ObjectFactory.HasBeenSubstituted<ITestInterface>());
			Assert(!ObjectFactory.HasBeenSubstituted("ITestInterface"));
			using (ObjectFactory.Substitute<ITestInterface>(obj))
			{
				Assert(ObjectFactory.HasBeenSubstituted<ITestInterface>());
				Assert(ObjectFactory.HasBeenSubstituted("ITestInterface"));
			}
		}

		public void TestHasBeenSubstituted_Generic()
		{
			var obj = new TestGenericImplementation<int>();
			Assert(!ObjectFactory.HasBeenSubstituted<ITestGenericInterface<int>>());
			Assert(!ObjectFactory.HasBeenSubstituted("ITestGenericInterface"));
			using (ObjectFactory.Substitute<ITestGenericInterface<int>>(obj))
			{
				Assert(ObjectFactory.HasBeenSubstituted<ITestGenericInterface<int>>());
				Assert(ObjectFactory.HasBeenSubstituted("ITestGenericInterface"));
			}
		}

		public void TestSubstitute_Obj_MockEnteredGeneric_ShouldError()
		{
			AssertExceptionThrown(
				typeof(ArgumentException),
				@"Should not substitute with type 'Mock<T>' as this will not correctly configure the mock for ObjectFactory.Get<T>().
Please use the Mock instance's '.Object' attribute instead.",
				() => ObjectFactory.Substitute(new Mock<ITestGenericInterface<ITestInterface>>()));
		}

		public void TestSubstitute_Obj_MockEntered_ShouldError()
		{
			AssertExceptionThrown(
				typeof(ArgumentException),
				@"Should not substitute with type 'Mock<T>' as this will not correctly configure the mock for ObjectFactory.Get<T>().
Please use the Mock instance's '.Object' attribute instead.",
				() => ObjectFactory.Substitute(new Mock<ITestInterface>()));
		}

		public void TestSubstitute_Factory_MockEnteredGeneric_ShouldError()
		{
			AssertExceptionThrown(
				typeof(ArgumentException),
				@"Should not substitute with type 'Mock<T>' as this will not correctly configure the mock for ObjectFactory.Get<T>().
Please use the Mock instance's '.Object' attribute instead.",
				() => ObjectFactory.Substitute(() => new Mock<ITestGenericInterface<ITestInterface>>()));
		}

		public void TestSubstitute_Factory_MockEntered_ShouldError()
		{
			AssertExceptionThrown(
				typeof(ArgumentException),
				@"Should not substitute with type 'Mock<T>' as this will not correctly configure the mock for ObjectFactory.Get<T>().
Please use the Mock instance's '.Object' attribute instead.",
				() => ObjectFactory.Substitute(() => new Mock<ITestInterface>()));
		}

		public void TestSubstitute_FactoryArgs_MockEnteredGeneric_ShouldError()
		{
			AssertExceptionThrown(
				typeof(ArgumentException),
				@"Should not substitute with type 'Mock<T>' as this will not correctly configure the mock for ObjectFactory.Get<T>().
Please use the Mock instance's '.Object' attribute instead.",
				() => ObjectFactory.Substitute(arg => new Mock<ITestGenericInterface<ITestInterface>>()));
		}

		public void TestSubstitute_FactoryArgs_MockEntered_ShouldError()
		{
			AssertExceptionThrown(
				typeof(ArgumentException),
				@"Should not substitute with type 'Mock<T>' as this will not correctly configure the mock for ObjectFactory.Get<T>().
Please use the Mock instance's '.Object' attribute instead.",
				() => ObjectFactory.Substitute(arg => new Mock<ITestInterface>()));
		}

		public void TestSubstitute_NameObject_MockEnteredGeneric_ShouldError()
		{
			AssertExceptionThrown(
				typeof(ArgumentException),
				@"Should not substitute with type 'Mock<T>' as this will not correctly configure the mock for ObjectFactory.Get<T>().
Please use the Mock instance's '.Object' attribute instead.",
				() => ObjectFactory.Substitute("ITestGenericInterface<int>", new Mock<ITestGenericInterface<ITestInterface>>()));
		}

		public void TestSubstitute_NameObject_MockEntered_ShouldError()
		{
			AssertExceptionThrown(
				typeof(ArgumentException),
				@"Should not substitute with type 'Mock<T>' as this will not correctly configure the mock for ObjectFactory.Get<T>().
Please use the Mock instance's '.Object' attribute instead.",
				() => ObjectFactory.Substitute("ITestGenericInterface<int>", new Mock<ITestInterface>()));
		}

		public class ObjectFactoryTest_ClassCalledMock : ThreadSafeAccessTestCase
		{
			class Mock<T> { }

			public void TestSubstitute_Obj_ClassCalledMockGenericEntered_ShouldNotError()
			{
				AssertNoExceptionThrown(() => ObjectFactory.Substitute(new Mock<ITestGenericInterface<ITestInterface>>()));
			}

			public void TestSubstitute_Obj_ClassCalledMockEntered_ShouldNotError()
			{
				AssertNoExceptionThrown(() => ObjectFactory.Substitute(new Mock<ITestInterface>()));
			}

			public void TestSubstitute_Factory_ClassCalledMockGenericEntered_ShouldNotError()
			{
				AssertNoExceptionThrown(() => ObjectFactory.Substitute(() => new Mock<ITestGenericInterface<ITestInterface>>()));
			}

			public void TestSubstitute_Factory_ClassCalledMockEntered_ShouldNotError()
			{
				AssertNoExceptionThrown(() => ObjectFactory.Substitute(() => new Mock<ITestInterface>()));
			}

			public void TestSubstitute_FactoryArgs_ClassCalledMockGenericEntered_ShouldNotError()
			{
				AssertNoExceptionThrown(() => ObjectFactory.Substitute(arg => new Mock<ITestGenericInterface<ITestInterface>>()));
			}

			public void TestSubstitute_FactoryArgs_ClassCalledMockEntered_ShouldNotError()
			{
				AssertNoExceptionThrown(() => ObjectFactory.Substitute(arg => new Mock<ITestInterface>()));
			}

			public void TestSubstitute_NameObject_ClassCalledMockGenericEntered_ShouldNotError()
			{
				AssertNoExceptionThrown(() => ObjectFactory.Substitute("Mock", arg => new Mock<ITestGenericInterface<ITestInterface>>()));
			}

			public void TestSubstitute_NameObject_ClassCalledMockEntered_ShouldNotError()
			{
				AssertNoExceptionThrown(() => ObjectFactory.Substitute("Mock", arg => new Mock<ITestInterface>()));
			}
		}

		public void TestUnregisteredInterface()
		{
			AssertExceptionThrown(typeof(NoSuchObjectDefinitionException), () => ObjectFactory.Get<IUnregisteredInterface>());
		}

		public void TestGetList()
		{
			IList services = ObjectFactory.Get<IList>("TestDiscoverableServices");
			AssertEquals(2, services.Count);
			AssertEquals(true, services[0] is TestFreightDiscoverableService);
			AssertEquals(true, services[1] is TestCustomsDiscoverableService);
		}

		public void TestGetParallelList()
		{
			var parallelList = ObjectFactory.Get<IList>("TestParallelList").Cast<TestParallel>().ToList();
			AssertNotEquals(parallelList[0].ThreadId, parallelList[1].ThreadId);
		}

		public void TestThreadSafety()
		{
			var objectDefinitions = ObjectFactory.GetObjectDefinitions().Take(200);
			RunTestOnMultipleThreads(delegate
			{
				foreach (ObjectDefinition objectDefinition in objectDefinitions)
				{
#pragma warning disable 0612
					AssertNotNull(objectDefinition.Name, ObjectFactory.GetType(objectDefinition.Name, null));
#pragma warning restore 0612
				}
			}, 50, EndThreadTestAction.Join);
		}

		public void TestGetCountrySpecificOrDefault()
		{
			AssertEquals("Base class", "Base", ObjectFactory.GetCountrySpecificOrDefault<CountrySpecific>("NZ").Code);
			AssertEquals("AU class", "AU", ObjectFactory.GetCountrySpecificOrDefault<CountrySpecific>("AU").Code);
		}

		public void TestGetWithFactoryMethod0Args()
		{
			var instance = ObjectFactory.Get<ITestInterfaceWithFactoryMethod0Args>("ITestInterfaceWithFactoryMethod0Args");
			AssertEquals(typeof(TestImplementationWithFactoryMethod0Args), instance.GetType());
		}

		public void TestGetWithFactoryMethod0Args_Generic()
		{
			var instance = ObjectFactory.Get<ITestGenericWithFactoryMethod0Args<int>>("ITestGenericWithFactoryMethod0Args");
			AssertEquals(typeof(TestGenericWithFactoryMethod0Args<int>), instance.GetType());
		}

		public void TestGetWithFactoryMethod2Args()
		{
			var object1 = new TestImplementation();
			var object2 = new TestImplementation();
			var instance = ObjectFactory.Get<ITestInterfaceWithFactoryMethod2Args>("ITestInterfaceWithFactoryMethod2Args", object1, object2);
			AssertEquals(typeof(TestImplementationWithFactoryMethod2Args), instance.GetType());
			AssertEquals(object1, instance.Property1);
			AssertEquals(object2, instance.Property2);
		}

		public void TestGetWithFactoryMethod2Args_Generic()
		{
			var object1 = new TestGenericImplementation<int>();
			var object2 = new TestGenericImplementation<string>();
			var instance = ObjectFactory.Get<ITestGenericWithFactoryMethod2Args<int>>("ITestGenericWithFactoryMethod2Args", object1, object2);
			AssertEquals(typeof(TestGenericWithFactoryMethod2Args<int>), instance.GetType());
			AssertEquals(object1, instance.Property1);
			AssertEquals(object2, instance.Property2);
		}

		public void TestGetWithFactoryMethodAmbiguous()
		{
			AssertExceptionThrown<AmbiguousMatchException>(() => ObjectFactory.Get<ITestInterfaceWithFactoryMethodAmbiguous>("ITestInterfaceWithFactoryMethodAmbiguous"));
		}

		public void TestGetWithConstructorArguments()
		{
			var instance = ObjectFactory.Get<ITestInterfaceWithConstructorArguments>("ITestInterfaceWithConstructorArguments");
			AssertEquals(typeof(TestImplementationWithConstructorArguments), instance.GetType());
			AssertEquals(typeof(TestImplementation), instance.Property1.GetType());
			AssertEquals(typeof(TestImplementation), instance.Property2.GetType());
		}

		public void TestGetWithConstructorArguments_Generic()
		{
			// NOTE: For now we do not support generic <constructor-arg ... /> arguments
			var instance = ObjectFactory.Get<ITestGenericWithConstructorArguments<int>>("ITestGenericWithConstructorArguments");
			AssertEquals(typeof(TestGenericWithConstructorArguments<int>), instance.GetType());
			AssertEquals(typeof(TestImplementation), instance.Property1.GetType());
			AssertEquals(typeof(TestImplementation), instance.Property2.GetType());
		}

		public void TestGetWithObjectPropertyDefinition_WithLookupOfPropertyStringFromRefAttribute()
		{
			var instance = ObjectFactory.Get<ITestInterfaceWithObjectPropertyDefinitionWithLookupOfPropertyStringFromRefAttribute>("ITestInterfaceWithObjectPropertyDefinitionWithLookupOfPropertyStringFromRefAttribute");
			AssertEquals(typeof(TestImplementationWithObjectPropertyDefinitionWithLookupOfPropertyStringFromRefAttribute), instance.GetType());
			AssertEquals("ArbitraryString1", instance.Property1);
			AssertEquals("ArbitraryString2", instance.Property2);
		}

		public void TestGetWithObjectPropertyDefinition_WithLookupOfPropertyStringFromRefAttribute_Generic()
		{
			var instance = ObjectFactory.Get<ITestGenericWithObjectPropertyDefinitionWithLookupOfPropertyStringFromRefAttribute<string>>("ITestGenericWithObjectPropertyDefinitionWithLookupOfPropertyStringFromRefAttribute");
			AssertEquals(typeof(TestGenericWithObjectPropertyDefinitionWithLookupOfPropertyStringFromRefAttribute<string>), instance.GetType());
			AssertEquals("ArbitraryString1", instance.Property1);
			AssertEquals("ArbitraryString2", instance.Property2);
		}

		public void TestGetWithObjectPropertyDefinition_WithLookupOfPropertyTypeFromRefAttribute()
		{
			var instance = ObjectFactory.Get<ITestInterfaceWithObjectPropertyDefinitionWithLookupOfPropertyTypeFromRefAttribute>("ITestInterfaceWithObjectPropertyDefinitionWithLookupOfPropertyTypeFromRefAttribute");
			AssertEquals(typeof(TestImplementationWithObjectPropertyDefinitionWithLookupOfPropertyTypeFromRefAttribute), instance.GetType());
			AssertEquals(typeof(TestImplementation), instance.Property1);
			AssertEquals(typeof(TestImplementation), instance.Property2);
		}

		public void TestGetWithObjectPropertyDefinition_WithLookupOfPropertyTypeFromRefAttribute_Generic()
		{
			var instance = ObjectFactory.Get<ITestGenericWithObjectPropertyDefinitionWithLookupOfPropertyTypeFromRefAttribute<Type, string>>("ITestGenericWithObjectPropertyDefinitionWithLookupOfPropertyTypeFromRefAttribute");
			AssertEquals(typeof(TestGenericWithObjectPropertyDefinitionWithLookupOfPropertyTypeFromRefAttribute<Type, string>), instance.GetType());
			AssertEquals(typeof(TestImplementation), instance.Property1);
			AssertEquals("ArbitraryString", instance.Property2);
		}

		public void TestGetWithObjectPropertyDefinition_WithLookupOfPropertyObjectFromRefAttribute()
		{
			var instance = ObjectFactory.Get<ITestInterfaceWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttribute>("ITestInterfaceWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttribute");
			AssertEquals(typeof(TestImplementationWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttribute), instance.GetType());
			AssertEquals(typeof(TestImplementation), instance.Property1.GetType());
			AssertEquals(typeof(TestImplementation), instance.Property2.GetType());
		}

		public void TestGetWithObjectPropertyDefinition_WithLookupOfPropertyObjectFromRefAttribute_Generic()
		{
			var instance = ObjectFactory.Get<ITestGenericWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttribute<int, string>>("ITestGenericWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttribute");
			AssertEquals(typeof(TestGenericWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttribute<int, string>), instance.GetType());
			AssertEquals(typeof(TestGenericImplementation<int>), instance.Property1.GetType());
			AssertEquals(typeof(TestGenericImplementation<string>), instance.Property2.GetType());
		}

		public void TestGetWithObjectPropertyDefinition_WithLookupOfPropertyObjectFromRefAttribute_Field()
		{
			var instance = (TestImplementationWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttributeField)ObjectFactory.Get<ITestInterfaceWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttributeField>("ITestInterfaceWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttributeField");
			AssertEquals(typeof(TestImplementationWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttributeField), instance.GetType());
			AssertEquals(typeof(TestImplementation), instance.field1.GetType());
			AssertEquals(typeof(TestImplementation), instance.field2.GetType());
		}

		public void TestGetWithObjectPropertyDefinition_WithLookupOfPropertyObjectFromRefAttribute_Field_Generic()
		{
			var instance = (TestGenericWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttributeField<int, string>)ObjectFactory.Get<ITestGenericWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttributeField<int, string>>("ITestGenericWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttributeField");
			AssertEquals(typeof(TestGenericWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttributeField<int, string>), instance.GetType());
			AssertEquals(typeof(TestGenericImplementation<int>), instance.field1.GetType());
			AssertEquals(typeof(TestGenericImplementation<string>), instance.field2.GetType());
		}

		public void TestGetWithObjectPropertyDefinition_WithInlineObject()
		{
			var instance = ObjectFactory.Get<ITestInterfaceWithObjectPropertyDefinitionWithInlineObject>("ITestInterfaceWithObjectPropertyDefinitionWithInlineObject");
			AssertEquals(typeof(TestImplementationWithObjectPropertyDefinitionWithInlineObject), instance.GetType());
			AssertEquals(typeof(TestImplementation), instance.Property1.GetType());
			AssertEquals(typeof(TestImplementation), instance.Property2.GetType());
		}

		public void TestGetWithObjectPropertyDefinition_WithInlineObject_Generic()
		{
			var instance = ObjectFactory.Get<ITestGenericWithObjectPropertyDefinitionWithInlineObject<int, string>>("ITestGenericWithObjectPropertyDefinitionWithInlineObject");
			AssertEquals(typeof(TestGenericWithObjectPropertyDefinitionWithInlineObject<int, string>), instance.GetType());
			AssertEquals(typeof(TestGenericImplementation<int>), instance.Property1.GetType());
			AssertEquals(typeof(TestGenericImplementation<string>), instance.Property2.GetType());
		}

		public void TestGetWithGenericParams()
		{
			var instance = ObjectFactory.Get<ITestInterfaceWithGenericParams<int>>();
			AssertEquals(typeof(TestImplmentationWithGenericParams<int>), instance.GetType());
			AssertEquals(typeof(int), instance.Property1.GetType());
		}

		public void TestConfiguration_NoElements()
		{
			try
			{
				AssertNoExceptionThrown(() => ObjectFactory.Configure(IocConfigurationTests.TestConfigurationNoElements));
			}
			finally
			{
				ObjectFactory.Unconfigure(IocConfigurationTests.TestConfigurationNoElements);
			}
		}

		public void TestConfigurationSubSetSourceDictionary_TrueFirst_FollowFalse()
		{
			try
			{
				ObjectFactory.Configure(IocConfigurationTests.TestSourceDictionaryConfiguration_SubSet_TrueLocation);

				var dictionary = (Hashtable)ObjectFactory.Get("TestSourceDictionarySubSet");
				AssertEquals("TestSourceDictionarySubSet count", 1, dictionary.Count);
				AssertEquals("TestSourceDictionarySubSet[Unauthorized]", typeof(UnauthorizedInterfaceReference), dictionary["Unauthorized"]);

				AssertExceptionThrown<InvalidOperationException>("Add SourceDictionary with SubSet=false", "'TestSourceDictionarySubSet' in assembly://CargoWise.ApplicationContext.Test/CargoWise.Application.Testing/TestSourceDictionaryConfiguration_SubSet_False.xml has SubSet set to False however another configuration has it set True.", () => ObjectFactory.Configure(IocConfigurationTests.TestSourceDictionaryConfiguration_SubSet_FalseLocation));
				dictionary = (Hashtable)ObjectFactory.Get("TestSourceDictionarySubSet");
				AssertEquals("TestSourceDictionarySubSet count", 1, dictionary.Count);
				AssertEquals("TestSourceDictionarySubSet[Unauthorized]", typeof(UnauthorizedInterfaceReference), dictionary["Unauthorized"]);

				AssertNoExceptionThrown("Add SourceDictionary with SubSet=true", () => ObjectFactory.Configure(IocConfigurationTests.TestSourceDictionaryConfiguration2_SubSet_TrueLocation));
				dictionary = (Hashtable)ObjectFactory.Get("TestSourceDictionarySubSet");
				AssertEquals("TestSourceDictionarySubSet count", 2, dictionary.Count);
				AssertEquals("TestSourceDictionarySubSet[Unauthorized]", typeof(UnauthorizedInterfaceReference), dictionary["Unauthorized"]);
				AssertEquals("TestSourceDictionarySubSet[Key]", typeof(AuthorizedInterfaceReference), dictionary["Key"]);

				var testSourceDictionarySubSetObjectDefinition = ObjectFactory.GetObjectDefinitions().Single(x => x.Name == "TestSourceDictionarySubSet");
				AssertEquals("testSourceDictionarySubSetObjectDefinition.PropertyDefinitions.Single().EnableParallelInit", true, testSourceDictionarySubSetObjectDefinition.PropertyDefinitions.Single().EnableParallelInit);
				ObjectFactory.Unconfigure(IocConfigurationTests.TestSourceDictionaryConfiguration2_SubSet_TrueLocation);
			}
			finally
			{
				ObjectFactory.Unconfigure(IocConfigurationTests.TestSourceDictionaryConfiguration_SubSet_TrueLocation);
			}
		}

		public void TestConfigurationSubSetSourceDictionary_FalseFirst_FollowTrue()
		{
			try
			{
				ObjectFactory.Configure(IocConfigurationTests.TestSourceDictionaryConfiguration_SubSet_FalseLocation);

				var dictionary = (Hashtable)ObjectFactory.Get("TestSourceDictionarySubSet");
				AssertEquals("TestSourceDictionarySubSet count", 1, dictionary.Count);
				AssertEquals("TestSourceDictionarySubSet[Key]", typeof(AuthorizedInterfaceReference), dictionary["Key"]);

				AssertExceptionThrown<InvalidOperationException>("Add SourceDictionary with SubSet=true", "'TestSourceDictionarySubSet' in assembly://CargoWise.ApplicationContext.Test/CargoWise.Application.Testing/TestSourceDictionaryConfiguration_SubSet_True.xml has SubSet set to True however another configuration has it set False.", () => ObjectFactory.Configure(IocConfigurationTests.TestSourceDictionaryConfiguration_SubSet_TrueLocation));
				dictionary = (Hashtable)ObjectFactory.Get("TestSourceDictionarySubSet");
				AssertEquals("TestSourceDictionarySubSet count", 1, dictionary.Count);
				AssertEquals("TestSourceDictionarySubSet[Key]", typeof(AuthorizedInterfaceReference), dictionary["Key"]);
			}
			finally
			{
				ObjectFactory.Unconfigure(IocConfigurationTests.TestSourceDictionaryConfiguration_SubSet_FalseLocation);
			}
		}

		public void TestConfigurationSubSetSourceDictionary_AddDuplicate()
		{
			try
			{
				ObjectFactory.Configure(IocConfigurationTests.TestSourceDictionaryConfiguration_SubSet_TrueLocation);

				var dictionary = (Hashtable)ObjectFactory.Get("TestSourceDictionarySubSet");
				AssertEquals("TestSourceDictionarySubSet count", 1, dictionary.Count);
				AssertEquals("TestSourceDictionarySubSet[Unauthorized]", typeof(UnauthorizedInterfaceReference), dictionary["Unauthorized"]);

				AssertNoExceptionThrown("Add SourceDictionary with SubSet=false", () => ObjectFactory.Configure(IocConfigurationTests.TestSourceDictionaryConfiguration_SubSet_True_DuplicateLocation));
				dictionary = (Hashtable)ObjectFactory.Get("TestSourceDictionarySubSet");
				AssertEquals("TestSourceDictionarySubSet count", 1, dictionary.Count);
				AssertEquals("TestSourceDictionarySubSet[Unauthorized]", typeof(UnauthorizedInterfaceReference), dictionary["Unauthorized"]);
			}
			finally
			{
				ObjectFactory.Unconfigure(IocConfigurationTests.TestSourceDictionaryConfiguration_SubSet_TrueLocation);
			}
		}

		public void TestConfigurationSubSetSourceList_TrueFirst_FollowFalse()
		{
			try
			{
				ObjectFactory.Configure(IocConfigurationTests.TestSourceListConfiguration_SubSet_TrueLocation);

				var list = ObjectFactory.Get<IList>("TestSourceListSubSet");
				AssertEquals("TestSourceListSubSet count", 1, list.Count);
				Assert("TestSourceListSubSet[0]", list[0] is TestCustomsDiscoverableService);

				AssertExceptionThrown<InvalidOperationException>("Add SourceList with SubSet=false", "'TestSourceListSubSet' in assembly://CargoWise.ApplicationContext.Test/CargoWise.Application.Testing/TestSourceListConfiguration_SubSet_False.xml has SubSet set to False however another configuration has it set True.", () => ObjectFactory.Configure(IocConfigurationTests.TestSourceListConfiguration_SubSet_FalseLocation));
				list = ObjectFactory.Get<IList>("TestSourceListSubSet");
				AssertEquals("TestSourceListSubSet count", 1, list.Count);
				Assert("TestSourceListSubSet[0]", list[0] is TestCustomsDiscoverableService);

				AssertNoExceptionThrown("Add SourceList with SubSet=true", () => ObjectFactory.Configure(IocConfigurationTests.TestSourceListConfiguration2_SubSet_TrueLocation));
				list = ObjectFactory.Get<IList>("TestSourceListSubSet");
				AssertEquals("TestSourceListSubSet count", 2, list.Count);
				Assert("TestSourceListSubSet[0]", list[0] is TestCustomsDiscoverableService);
				Assert("TestSourceListSubSet[1]", list[1] is TestFreightDiscoverableService);

				var testSourceListSubSetObjectDefinition = ObjectFactory.GetObjectDefinitions().Single(x => x.Name == "TestSourceListSubSet");
				AssertEquals("testSourceListSubSetObjectDefinition.PropertyDefinitions.Single().EnableParallelInit", true, testSourceListSubSetObjectDefinition.PropertyDefinitions.Single().EnableParallelInit);
				ObjectFactory.Unconfigure(IocConfigurationTests.TestSourceListConfiguration2_SubSet_TrueLocation);
			}
			finally
			{
				ObjectFactory.Unconfigure(IocConfigurationTests.TestSourceListConfiguration_SubSet_TrueLocation);
			}
		}

		public void TestConfigurationSubSetSourceList_FalseFirst_FollowTrue()
		{
			try
			{
				ObjectFactory.Configure(IocConfigurationTests.TestSourceListConfiguration_SubSet_FalseLocation);

				var list = ObjectFactory.Get<IList>("TestSourceListSubSet");
				AssertEquals("TestSourceListSubSet count", 1, list.Count);
				Assert("TestSourceListSubSet[0]", list[0] is TestFreightDiscoverableService);

				AssertExceptionThrown<InvalidOperationException>("Add SourceList with SubSet=true", "'TestSourceListSubSet' in assembly://CargoWise.ApplicationContext.Test/CargoWise.Application.Testing/TestSourceListConfiguration_SubSet_True.xml has SubSet set to True however another configuration has it set False.", () => ObjectFactory.Configure(IocConfigurationTests.TestSourceListConfiguration_SubSet_TrueLocation));
				list = ObjectFactory.Get<IList>("TestSourceListSubSet");
				AssertEquals("TestSourceListSubSet count", 1, list.Count);
				Assert("TestSourceListSubSet[0]", list[0] is TestFreightDiscoverableService);
			}
			finally
			{
				ObjectFactory.Unconfigure(IocConfigurationTests.TestSourceListConfiguration_SubSet_FalseLocation);
			}
		}

		public void TestConfigurationSubSetSourceList_AddDuplicate()
		{
			try
			{
				ObjectFactory.Configure(IocConfigurationTests.TestSourceListConfiguration_SubSet_TrueLocation);

				var list = ObjectFactory.Get<IList>("TestSourceListSubSet");
				AssertEquals("TestSourceListSubSet count", 1, list.Count);
				Assert("TestSourceListSubSet[0]", list[0] is TestCustomsDiscoverableService);

				AssertNoExceptionThrown("Add SourceList with SubSet=false", () => ObjectFactory.Configure(IocConfigurationTests.TestSourceListConfiguration_SubSet_True_DuplicateLocation));
				list = ObjectFactory.Get<IList>("TestSourceListSubSet");
				AssertEquals("TestSourceListSubSet count", 1, list.Count);
				Assert("TestSourceListSubSet[0]", list[0] is TestCustomsDiscoverableService);
			}
			finally
			{
				ObjectFactory.Unconfigure(IocConfigurationTests.TestSourceListConfiguration_SubSet_TrueLocation);
			}
		}

		public void TestSubSetNotSourceDictionaryOrSourceList()
		{
			ObjectFactory.Configure(IocConfigurationTests.TestSubSetNotSourceDictionaryOrSourceListLocation);
			AssertExceptionThrown<InvalidOperationException>("SubSet=true but not SourceList or Dictionary", "'ITestDiscoverableService_Warehouse' in assembly://CargoWise.ApplicationContext.Test/CargoWise.Application.Testing/TestSubSetNotSourceDictionaryOrSourceList2.xml is configured to use SubSet however SubSet can only be used for either SourceList or SourceDictionary.", () => ObjectFactory.Configure(IocConfigurationTests.TestSubSetNotSourceDictionaryOrSourceList2Location));
		}

		public void TestConfigurationOverride()
		{
			ObjectFactory.Configure(IocConfigurationTests.TestOverrideConfigurationLocation);

			var nonGeneric = ObjectFactory.Get<ITestInterface>();
			AssertEquals(typeof(TestImplementationOverride), nonGeneric.GetType());

			var generic = ObjectFactory.Get<ITestGenericInterface<int>>();
			AssertEquals(typeof(TestGenericImplementationOverride<int>), generic.GetType());

			var dictionary = (Hashtable)ObjectFactory.Get("TestKeyTypeDictionary");
			AssertEquals("TestKeyTypeDictionary count", 2, dictionary.Count);
			AssertEquals("TestKeyTypeDictionary[Key]", typeof(AuthorizedInterfaceReference), dictionary["Key"]);
			AssertEquals("TestKeyTypeDictionary[Unauthorized]", typeof(UnauthorizedInterfaceReference), dictionary["Unauthorized"]);

			var services = ObjectFactory.Get<IList>("TestDiscoverableServices");
			AssertEquals(3, services.Count);
			AssertEquals(true, services[0] is TestFreightDiscoverableService);
			AssertEquals(true, services[1] is TestCustomsDiscoverableService);
			AssertEquals(true, services[2] is TestWarehouseDiscoverableService);

			ObjectFactory.Unconfigure(IocConfigurationTests.TestOverrideConfigurationLocation);

			AssertExceptionThrown(typeof(NoSuchObjectDefinitionException), () => ObjectFactory.Get<ITestInterface>());
			AssertExceptionThrown(typeof(NoSuchObjectDefinitionException), () => ObjectFactory.Get<ITestGenericInterface<int>>());

			dictionary = ObjectFactory.Get<Hashtable>("TestKeyTypeDictionary");
			AssertEquals("TestKeyTypeDictionary count", 1, dictionary.Count);
			AssertEquals("TestKeyTypeDictionary[Key]", typeof(AuthorizedInterfaceReference), dictionary["Key"]);

			services = ObjectFactory.Get<IList>("TestDiscoverableServices");
			AssertEquals(2, services.Count);
			AssertEquals(true, services[0] is TestFreightDiscoverableService);
			AssertEquals(true, services[1] is TestCustomsDiscoverableService);
		}

		protected override void SetUp()
		{
			base.SetUp();
			ObjectFactory.Configure(IocConfigurationTests.TestConfigurationLocation);
		}

		protected override void TearDown()
		{
			ObjectFactory.Unconfigure(IocConfigurationTests.TestConfigurationLocation);
		}
	}

	// CargoWise has already called ObjectFactory.Configure when running this test.
	// So GetObjectDefinitions returns the definitions used by CargoWise.
	class CW1ConfigTest : TestCase
	{
		[FrequentlyFailing]
		public void TestGetAllTypes()
		{
			CombineAssertions(() =>
			{
				var count = 0;
				foreach (var objectDefinition in ObjectFactory.GetObjectDefinitions())
				{
					Type type = null;
					AssertNoExceptionThrown(() => type = ObjectFactory.GetType(objectDefinition.Name));
					AssertNotNull($"Should have returned a type for the {objectDefinition.Name} mapping", type);

					count++;
				}

				AssertNotEquals("Ensure some object definitions have actually been loaded by CargoWise", 0, count);
			});
		}

		public void TestGetAllFactoryMethods()
		{
			var count = 0;
			foreach (var objectDefinition in ObjectFactory.GetObjectDefinitions())
			{
				try
				{
					if (objectDefinition.FactoryMethodName != null)
					{
						AssertNotNull(objectDefinition.GetFactoryMethod(typeof(ObjectFactory.EmptyType)));
						count++;
					}
				}
				catch (Exception ex)
				{
					Fail($"Attempting to get the object with definition of id='{objectDefinition.Name}', type='{objectDefinition.GetType(typeof(ObjectFactory.EmptyType)).Name}' and factory-method='{objectDefinition.FactoryMethodName}' threw an exception: {ex}");
				}
			}
			AssertNotEquals("Ensure some object definitions have actually been loaded by CargoWise", 0, count);
		}
	}
}
