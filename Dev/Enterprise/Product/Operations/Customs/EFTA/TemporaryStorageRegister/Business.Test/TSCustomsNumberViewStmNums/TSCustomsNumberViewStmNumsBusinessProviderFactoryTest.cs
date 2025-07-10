using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(TSCustomsNumberViewStmNumsBusinessProviderFactory))]
sealed class TSCustomsNumberViewStmNumsBusinessProviderFactoryTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>(() => new TSCustomsNumberViewStmNumsBusinessProviderFactory(null));
	}

	public void TestGetProvider_WhenProviderExistsForProviderKey()
	{
		var countySpecificMock = Mock.Of<TSCustomsNumberViewStmNumsBusinessProviderForTest>();
		var countySpecificMockObjectHandle = Mock.Of<ObjectHandle>(m => m.GetObject(It.IsAny<object[]>()) == countySpecificMock);

		var defaultKeyMock = Mock.Of<TSCustomsNumberViewStmNumsBusinessProviderForTest>();
		var defaultKeyMockObjectHandle = Mock.Of<ObjectHandle>(m => m.GetObject(It.IsAny<object[]>()) == defaultKeyMock);

		var hashTable = new Hashtable
		{
			{ "DE", countySpecificMockObjectHandle },
			{ "Default", defaultKeyMockObjectHandle }
		};

		using (ObjectFactory.Substitute(CustomsNumberBusinessProvidersRegistrationKey, hashTable))
		{
			var provider = NumberProviderFactory.GetProvider("DE", ZGuid.NewZGuid());
			AssertSame(countySpecificMock, provider);
		}
	}

	public void TestGetProvider_WhenNoProviderForProviderKey()
	{
		var defaultKeyMock = Mock.Of<TSCustomsNumberViewStmNumsBusinessProviderForTest>();
		var defaultKeyMockObjectHandle = Mock.Of<ObjectHandle>(m => m.GetObject(It.IsAny<object[]>()) == defaultKeyMock);

		var hashTable = new Hashtable
		{
			{ "Default", defaultKeyMockObjectHandle }
		};

		using (ObjectFactory.Substitute(CustomsNumberBusinessProvidersRegistrationKey, hashTable))
		{
			var provider = NumberProviderFactory.GetProvider("DE", ZGuid.NewZGuid());
			AssertSame(defaultKeyMock, provider);
		}
	}

	public void TestGetProvider_WithCustomKeyIsSpecifiedAsDefault()
	{
		var customKeyMock = Mock.Of<TSCustomsNumberViewStmNumsBusinessProviderForTest>();
		var customKeyMockObjectHandle = Mock.Of<ObjectHandle>(m => m.GetObject(It.IsAny<object[]>()) == customKeyMock);

		var defaultKeyMock = Mock.Of<TSCustomsNumberViewStmNumsBusinessProviderForTest>();
		var defaultKeyMockObjectHandle = Mock.Of<ObjectHandle>(m => m.GetObject(It.IsAny<object[]>()) == defaultKeyMock);

		var hashTable = new Hashtable
		{
			{ "CUSTOM_KEY", customKeyMockObjectHandle },
			{ "Default", defaultKeyMockObjectHandle }
		};

		using (ObjectFactory.Substitute(CustomsNumberBusinessProvidersRegistrationKey, hashTable))
		{
			var provider = new TSCustomsNumberViewStmNumsBusinessProviderFactoryForTest(Factory).GetProvider("DE", ZGuid.NewZGuid());
			AssertSame(customKeyMock, provider);
		}
	}

	public void TestGetProvider_WhenNoProviderIsFound()
	{
		var defaultKeyMock = Mock.Of<TSCustomsNumberViewStmNumsBusinessProviderForTest>();
		var defaultKeyMockObjectHandle = Mock.Of<ObjectHandle>(m => m.GetObject(It.IsAny<object[]>()) == defaultKeyMock);

		var hashTable = new Hashtable
		{
			{ "XZY", defaultKeyMockObjectHandle }
		};

		using (ObjectFactory.Substitute(CustomsNumberBusinessProvidersRegistrationKey, hashTable))
		{
			var provider = NumberProviderFactory.GetProvider("DE", ZGuid.NewZGuid());
			AssertNull(provider);
		}
	}

	TSCustomsNumberViewStmNumsBusinessProviderFactory NumberProviderFactory => numberProviderFactory ??= new TSCustomsNumberViewStmNumsBusinessProviderFactory(Factory);
	TSCustomsNumberViewStmNumsBusinessProviderFactory numberProviderFactory;

	const string CustomsNumberBusinessProvidersRegistrationKey = "TSCustomsNumberViewStmNumsBusinessProviders";

	class TSCustomsNumberViewStmNumsBusinessProviderFactoryForTest : TSCustomsNumberViewStmNumsBusinessProviderFactory
	{
		public TSCustomsNumberViewStmNumsBusinessProviderFactoryForTest(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override string DefaultProviderValueResolverKey => "CUSTOM_KEY";
	}
}

public class TSCustomsNumberViewStmNumsBusinessProviderForTest : TSCustomsNumberViewStmNumsBusinessProvider
{
	public TSCustomsNumberViewStmNumsBusinessProviderForTest()
		: base(new BusinessObjectFactory(), "DE", ZGuid.NewZGuid())
	{
	}
}
