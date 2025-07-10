using System.Linq;
using CargoWise.Application.InversionOfControl;
using NUnit.Framework;

namespace CargoWise.Application.Testing
{
	class IocConfigurationTests : TestCase
	{
		public const string TestConfigurationLocation = "assembly://CargoWise.ApplicationContext.Test/CargoWise.Application.Testing/TestConfiguration.xml";
		public const string TestConfigurationNoElements = "assembly://CargoWise.ApplicationContext.Test/CargoWise.Application.Testing/TestConfiguration_NoElements.xml";
		public const string TestOverrideConfigurationLocation = "assembly://CargoWise.ApplicationContext.Test/CargoWise.Application.Testing/TestOverrideConfiguration.xml";
		public const string TestSourceDictionaryConfiguration_SubSet_FalseLocation = "assembly://CargoWise.ApplicationContext.Test/CargoWise.Application.Testing/TestSourceDictionaryConfiguration_SubSet_False.xml";
		public const string TestSourceDictionaryConfiguration_SubSet_TrueLocation = "assembly://CargoWise.ApplicationContext.Test/CargoWise.Application.Testing/TestSourceDictionaryConfiguration_SubSet_True.xml";
		public const string TestSourceDictionaryConfiguration2_SubSet_TrueLocation = "assembly://CargoWise.ApplicationContext.Test/CargoWise.Application.Testing/TestSourceDictionaryConfiguration2_SubSet_True.xml";
		public const string TestSourceDictionaryConfiguration_SubSet_True_DuplicateLocation = "assembly://CargoWise.ApplicationContext.Test/CargoWise.Application.Testing/TestSourceDictionaryConfiguration_SubSet_True_Duplicate.xml";
		public const string TestSourceListConfiguration_SubSet_FalseLocation = "assembly://CargoWise.ApplicationContext.Test/CargoWise.Application.Testing/TestSourceListConfiguration_SubSet_False.xml";
		public const string TestSourceListConfiguration_SubSet_TrueLocation = "assembly://CargoWise.ApplicationContext.Test/CargoWise.Application.Testing/TestSourceListConfiguration_SubSet_True.xml";
		public const string TestSourceListConfiguration2_SubSet_TrueLocation = "assembly://CargoWise.ApplicationContext.Test/CargoWise.Application.Testing/TestSourceListConfiguration2_SubSet_True.xml";
		public const string TestSourceListConfiguration_SubSet_True_DuplicateLocation = "assembly://CargoWise.ApplicationContext.Test/CargoWise.Application.Testing/TestSourceListConfiguration_SubSet_True_Duplicate.xml";
		public const string TestSubSetNotSourceDictionaryOrSourceListLocation = "assembly://CargoWise.ApplicationContext.Test/CargoWise.Application.Testing/TestSubSetNotSourceDictionaryOrSourceList.xml";
		public const string TestSubSetNotSourceDictionaryOrSourceList2Location = "assembly://CargoWise.ApplicationContext.Test/CargoWise.Application.Testing/TestSubSetNotSourceDictionaryOrSourceList2.xml";

		public void TestLoadTestConfiguration()
		{
			ObjectDefinitions objectDefinitions = ObjectDefinitions.Create(TestConfigurationLocation);
			AssertNotNull(objectDefinitions);
			AssertNotNull(objectDefinitions.Definitions);
			AssertGreaterThan(objectDefinitions.Definitions.Length, 1);
		}
		public void TestConfigureAndUnconfigureObjectFactory()
		{
			ObjectFactory.Configure(TestConfigurationLocation);
			Assert(ObjectFactory.GetObjectDefinitions().Any(od => string.Equals(od.Name, "IAuthorizedInterfaceReference")));
			ObjectFactory.Unconfigure(TestConfigurationLocation);
			Assert(!ObjectFactory.GetObjectDefinitions().Any(od => string.Equals(od.Name, "IAuthorizedInterfaceReference")));
		}
	}
}
