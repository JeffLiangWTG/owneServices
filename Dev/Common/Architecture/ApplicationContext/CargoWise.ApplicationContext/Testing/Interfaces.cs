using System;
using System.IO;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.Application.Testing
{
	public interface IUnregisteredInterface { }
	public interface ITestInterface { }
	public interface ITestInterfaceWithOverriddenImplementation { }
	public class TestImplementation : ITestInterface, ITestInterfaceWithOverriddenImplementation { }
	public class TestImplementationOverride : ITestInterface, ITestInterfaceWithOverriddenImplementation { }

	public interface ITestGenericInterface<T> { }
	public interface ITestMultiplesGenericInterface<T, U, F> { }
	public class TestGenericImplementation<T> : ITestGenericInterface<T> { }
	public class TestMultiplesGenericImplementation<T, U, F> : ITestMultiplesGenericInterface<T, U, F> { }
	public class TestGenericImplementationOverride<T> : ITestGenericInterface<T> { }

	public interface ITestInterfaceWithFactoryMethod0Args { }

	public class TestImplementationWithFactoryMethod0Args : ITestInterfaceWithFactoryMethod0Args
	{
		TestImplementationWithFactoryMethod0Args()
		{
		}

		public static TestImplementationWithFactoryMethod0Args Instance
		{
			get { return new TestImplementationWithFactoryMethod0Args(); }
		}
	}

	public interface ITestGenericWithFactoryMethod0Args<T> { }

	public class TestGenericWithFactoryMethod0Args<T> : ITestGenericWithFactoryMethod0Args<T>
	{
		TestGenericWithFactoryMethod0Args()
		{
		}

		public static TestGenericWithFactoryMethod0Args<T> Instance
		{
			get { return new TestGenericWithFactoryMethod0Args<T>(); }
		}
	}

	public interface ITestInterfaceWithFactoryMethod2Args
	{
		ITestInterface Property1 { get; set; }
		ITestInterface Property2 { get; set; }
	}

	public class TestImplementationWithFactoryMethod2Args : ITestInterfaceWithFactoryMethod2Args
	{
		TestImplementationWithFactoryMethod2Args()
		{
		}

		public static TestImplementationWithFactoryMethod2Args TheInstance(ITestInterface arg1, ITestInterface arg2)
		{
			var instance = new TestImplementationWithFactoryMethod2Args();
			instance.Property1 = arg1;
			instance.Property2 = arg2;
			return instance;
		}

		public ITestInterface Property1 { get; set; }
		public ITestInterface Property2 { get; set; }
	}

	public interface ITestGenericWithFactoryMethod2Args<T>
	{
		ITestGenericInterface<T> Property1 { get; set; }
		ITestGenericInterface<string> Property2 { get; set; }
	}

	public class TestGenericWithFactoryMethod2Args<T> : ITestGenericWithFactoryMethod2Args<T>
	{
		TestGenericWithFactoryMethod2Args()
		{
		}

		public static TestGenericWithFactoryMethod2Args<T> TheInstance(ITestGenericInterface<T> arg1, ITestGenericInterface<string> arg2)
		{
			var instance = new TestGenericWithFactoryMethod2Args<T>();
			instance.Property1 = arg1;
			instance.Property2 = arg2;
			return instance;
		}

		public ITestGenericInterface<T> Property1 { get; set; }
		public ITestGenericInterface<string> Property2 { get; set; }
	}

	public interface ITestInterfaceWithFactoryMethodAmbiguous { }
	[CodeAlive("WI00575476 Baseline")]
	public class TestImplementationWithFactoryMethodAmbiguous : ITestInterfaceWithFactoryMethodAmbiguous
	{
		public static TestImplementationWithFactoryMethodAmbiguous TheInstance(ITestInterface arg1, ITestInterface arg2)
		{
			return new TestImplementationWithFactoryMethodAmbiguous();
		}

		public static TestImplementationWithFactoryMethodAmbiguous TheInstance(ITestInterface arg1)
		{
			return new TestImplementationWithFactoryMethodAmbiguous();
		}
	}

	public interface IAuthorizedInterfaceReference { }
	public class AuthorizedInterfaceReference : IAuthorizedInterfaceReference { }
	public interface IUnauthorizedInterfaceReference { }
	[CodeAlive("WI00575476 Baseline")]
	public class UnauthorizedInterfaceReference : IUnauthorizedInterfaceReference { }

	public interface ITestDiscoverableService { }
	public class TestFreightDiscoverableService : ITestDiscoverableService { }
	public class TestCustomsDiscoverableService : ITestDiscoverableService { }
	public class TestWarehouseDiscoverableService : ITestDiscoverableService { }

	public class TestParallel
	{
		public readonly int ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
	}

	public interface ITestInterfaceWithConstructorTakingStringAndInt
	{
		string FirstParameter { get; }
		int SecondParameter { get; }
	}
	public class TestImplementationWithConstructorTakingStringAndInt : ITestInterfaceWithConstructorTakingStringAndInt
	{
		public TestImplementationWithConstructorTakingStringAndInt(string firstParameter, int secondParameter)
		{
			FirstParameter = firstParameter;
			SecondParameter = secondParameter;
		}

		public string FirstParameter
		{
			get;
			private set;
		}

		public int SecondParameter
		{
			get;
			private set;
		}
	}

	public interface ITestGenericInterfaceWithConstructorTakingTwoParameters<T, U>
	{
		T FirstParameter { get; }
		U SecondParameter { get; }
	}

	public class TestGenericImplementationWithConstructorTakingTwoParameters<T, U> : ITestGenericInterfaceWithConstructorTakingTwoParameters<T, U>
	{
		public TestGenericImplementationWithConstructorTakingTwoParameters(T firstParameter, U secondParameter)
		{
			FirstParameter = firstParameter;
			SecondParameter = secondParameter;
		}

		public T FirstParameter
		{
			get;
			private set;
		}

		public U SecondParameter
		{
			get;
			private set;
		}
	}

	public interface ITestInterfaceWith2Constructors { }
	[CodeAlive("WI00575476 Baseline")]
	public class TestImplementationWith2Constructors : ITestInterfaceWith2Constructors
	{
		public TestImplementationWith2Constructors(string first) { }
		public TestImplementationWith2Constructors(int second) { }
	}

	public interface ITestGenericWith2Constructors<T> { }
	public class TestGenericWith2Constructors<T> : ITestGenericWith2Constructors<T>
	{
		public TestGenericWith2Constructors(string first) { }
		public TestGenericWith2Constructors(int second) { }
	}

	public interface ITestInterfaceWithAmbigousConstructors { }
	[CodeAlive("Baseline")]
	public class TestImplementationWithAmbigousConstructors : ITestInterfaceWithAmbigousConstructors
	{
		public TestImplementationWithAmbigousConstructors(string first)
		{
		}
		public TestImplementationWithAmbigousConstructors(object second)
		{
		}
	}

	public interface ITestGenericWithAmbigousConstructors<T> { }
	public class TestGenericWithAmbigousConstructors<T> : ITestGenericWithAmbigousConstructors<T>
	{
		public TestGenericWithAmbigousConstructors(string first)
		{
		}
		public TestGenericWithAmbigousConstructors(object second)
		{
		}
	}

	public interface ITestInterfaceSupportingNullArguments
	{
		string Constructor { get; }
	}

	[CodeAlive("WI00575476 Baseline")]
	public class TestImplementationSupportingNullArguments : ITestInterfaceSupportingNullArguments
	{
		public TestImplementationSupportingNullArguments(Stream first)
		{
			Constructor = "first";
		}
		public TestImplementationSupportingNullArguments(int second)
		{
			Constructor = "second";
		}
		public string Constructor { get; private set; }
	}

	public interface ITestGenericSupportingNullArguments<T>
	{
		string Constructor { get; }
	}
	public class TestGenericSupportingNullArguments<T> : ITestGenericSupportingNullArguments<T>
	{
		public TestGenericSupportingNullArguments(Stream first)
		{
			Constructor = "first";
		}
		public TestGenericSupportingNullArguments(int second)
		{
			Constructor = "second";
		}
		public string Constructor { get; private set; }
	}

	public interface ITestInterfaceWithMultipleOverloads
	{
		string Constructor { get; }
	}
	[CodeAlive("WI00575476 Baseline")]
	public class TestImplementationWithMultipleOverloads : ITestInterfaceWithMultipleOverloads
	{
		public TestImplementationWithMultipleOverloads(IDisposable arg)
		{
			Constructor = "first";
		}
		public TestImplementationWithMultipleOverloads(Stream arg)
		{
			Constructor = "second";
		}
		public TestImplementationWithMultipleOverloads(MemoryStream arg)
		{
			Constructor = "third";
		}
		public TestImplementationWithMultipleOverloads(MemoryStream arg0, MemoryStream arg1)
		{
			Constructor = "fourth";
		}
		public TestImplementationWithMultipleOverloads(MemoryStream arg0, MemoryStream arg1, MemoryStream arg2)
		{
			Constructor = "fifth";
		}
		public string Constructor { get; private set; }
	}

	public interface ITestGenericWithMultipleOverloads<T>
	{
		string Constructor { get; }
	}
	public class TestGenericWithMultipleOverloads<T> : ITestGenericWithMultipleOverloads<T>
	{
		public TestGenericWithMultipleOverloads(T arg)
		{
			Constructor = arg.ToString();
		}
		public TestGenericWithMultipleOverloads(Stream arg)
		{
			Constructor = "second";
		}
		public TestGenericWithMultipleOverloads(MemoryStream arg)
		{
			Constructor = "third";
		}
		public TestGenericWithMultipleOverloads(MemoryStream arg0, MemoryStream arg1)
		{
			Constructor = "fourth";
		}
		public TestGenericWithMultipleOverloads(MemoryStream arg0, MemoryStream arg1, MemoryStream arg2)
		{
			Constructor = "fifth";
		}
		public string Constructor { get; private set; }
	}

	public interface ITestInterfaceWithConstructorArguments
	{
		ITestInterface Property1 { get; }
		ITestInterface Property2 { get; }
	}

	public class TestImplementationWithConstructorArguments : ITestInterfaceWithConstructorArguments
	{
		public TestImplementationWithConstructorArguments(ITestInterface arg1, ITestInterface arg2)
		{
			Property1 = arg1;
			Property2 = arg2;
		}

		public ITestInterface Property1
		{
			get;
			private set;
		}

		public ITestInterface Property2
		{
			get;
			private set;
		}
	}

	public interface ITestGenericWithConstructorArguments<T>
	{
		ITestInterface Property1 { get; }
		ITestInterface Property2 { get; }
	}

	public class TestGenericWithConstructorArguments<T> : ITestGenericWithConstructorArguments<T>
	{
		public TestGenericWithConstructorArguments(ITestInterface arg1, ITestInterface arg2)
		{
			Property1 = arg1;
			Property2 = arg2;
		}

		public ITestInterface Property1
		{
			get;
			private set;
		}

		public ITestInterface Property2
		{
			get;
			private set;
		}
	}

	public interface ITestInterfaceWithObjectPropertyDefinitionWithLookupOfPropertyStringFromRefAttribute
	{
		string Property1 { get; }
		string Property2 { get; }
	}

	public class TestImplementationWithObjectPropertyDefinitionWithLookupOfPropertyStringFromRefAttribute : ITestInterfaceWithObjectPropertyDefinitionWithLookupOfPropertyStringFromRefAttribute
	{
		public string Property1
		{
			get;
			private set;
		}

		public string Property2
		{
			get;
			private set;
		}
	}

	public interface ITestGenericWithObjectPropertyDefinitionWithLookupOfPropertyStringFromRefAttribute<T>
	{
		T Property1 { get; }
		string Property2 { get; }
	}

	public class TestGenericWithObjectPropertyDefinitionWithLookupOfPropertyStringFromRefAttribute<T> : ITestGenericWithObjectPropertyDefinitionWithLookupOfPropertyStringFromRefAttribute<T>
	{
		public T Property1
		{
			get;
			private set;
		}

		public string Property2
		{
			get;
			private set;
		}
	}

	public interface ITestInterfaceWithObjectPropertyDefinitionWithLookupOfPropertyTypeFromRefAttribute
	{
		Type Property1 { get; }
		Type Property2 { get; }
	}

	public class TestImplementationWithObjectPropertyDefinitionWithLookupOfPropertyTypeFromRefAttribute : ITestInterfaceWithObjectPropertyDefinitionWithLookupOfPropertyTypeFromRefAttribute
	{
		public Type Property1
		{
			get;
			private set;
		}

		public Type Property2
		{
			get;
			private set;
		}
	}

	public interface ITestGenericWithObjectPropertyDefinitionWithLookupOfPropertyTypeFromRefAttribute<T, U>
	{
		T Property1 { get; }
		U Property2 { get; }
	}

	public class TestGenericWithObjectPropertyDefinitionWithLookupOfPropertyTypeFromRefAttribute<T, U> : ITestGenericWithObjectPropertyDefinitionWithLookupOfPropertyTypeFromRefAttribute<T, U>
	{
		public T Property1
		{
			get;
			private set;
		}

		public U Property2
		{
			get;
			private set;
		}
	}

	public interface ITestInterfaceWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttribute
	{
		ITestInterface Property1 { get; }
		ITestInterface Property2 { get; }
	}

	public class TestImplementationWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttribute : ITestInterfaceWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttribute
	{
		public ITestInterface Property1
		{
			get;
			private set;
		}

		public ITestInterface Property2
		{
			get;
			private set;
		}
	}

	public interface ITestGenericWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttribute<T, U>
	{
		ITestGenericInterface<T> Property1 { get; }
		ITestGenericInterface<U> Property2 { get; }
	}

	public class TestGenericWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttribute<T, U> : ITestGenericWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttribute<T, U>
	{
		public ITestGenericInterface<T> Property1
		{
			get;
			private set;
		}

		public ITestGenericInterface<U> Property2
		{
			get;
			private set;
		}
	}

	public interface ITestInterfaceWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttributeField { }

	public class TestImplementationWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttributeField : ITestInterfaceWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttributeField
	{
		public ITestInterface field1;
		public ITestInterface field2;
	}

	public interface ITestGenericWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttributeField<T, U> { }

	public class TestGenericWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttributeField<T, U> : ITestGenericWithObjectPropertyDefinitionWithLookupOfPropertyObjectFromRefAttributeField<T, U>
	{
		public ITestGenericInterface<T> field1;
		public ITestGenericInterface<U> field2;
	}

	public interface ITestInterfaceWithObjectPropertyDefinitionWithInlineObject
	{
		ITestInterface Property1 { get; }
		ITestInterface Property2 { get; }
	}

	public class TestImplementationWithObjectPropertyDefinitionWithInlineObject : ITestInterfaceWithObjectPropertyDefinitionWithInlineObject
	{
		public ITestInterface Property1
		{
			get;
			private set;
		}

		public ITestInterface Property2
		{
			get;
			private set;
		}
	}

	public interface ITestGenericWithObjectPropertyDefinitionWithInlineObject<T, U>
	{
		ITestGenericInterface<T> Property1 { get; }
		ITestGenericInterface<U> Property2 { get; }
	}

	public class TestGenericWithObjectPropertyDefinitionWithInlineObject<T, U> : ITestGenericWithObjectPropertyDefinitionWithInlineObject<T, U>
	{
		public ITestGenericInterface<T> Property1
		{
			get;
			private set;
		}

		public ITestGenericInterface<U> Property2
		{
			get;
			private set;
		}
	}

	public interface ITestGenericInterfaceWithObjectPropertyDefinitionWithInlineObject<T>
	{
		ITestInterface Property1 { get; }
		ITestInterface Property2 { get; }
	}

	public class TestGenericImplementationWithObjectPropertyDefinitionWithInlineObject<T> : ITestGenericInterfaceWithObjectPropertyDefinitionWithInlineObject<T>
	{
		public ITestInterface Property1
		{
			get;
			private set;
		}

		public ITestInterface Property2
		{
			get;
			private set;
		}
	}

	public interface ITestInterfaceWithGenericParams<T>
	{
		T Property1 { get; }
	}

	public class TestImplmentationWithGenericParams<T> : ITestInterfaceWithGenericParams<T>
	{
		public T Property1 { get; private set; }
	}
}
