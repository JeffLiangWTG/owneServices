using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	[TestedType(typeof(CLMessage))]
	class CLMessageTest : EDIMessageTest
	{
		public void TestMessageDefaults()
		{
			var message = Factory.New<CLMessage>();
			AssertEquals(EDIInterchange.ApplicationCodes.CLCustoms, message.EM_ApplicationCode);
		}

		public void TestLinkedObjectTypes()
		{
			var deEDIMessage = Factory.New<CLEDIMessageForTest>();
			AssertCollectionContains(typeof(AsycudaBill), deEDIMessage.AdditionalRegisteredLinkedObjectTypes);
		}
	}

	[TestedType(typeof(CLMessage.Loader))]
	class LoaderTestCase : CargoWise.EntityFramework.LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new CLMessage.Loader(Factory);
		}
	}

	class CLEDIMessageForTest : CLMessage
	{
		public CLEDIMessageForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public IEnumerable<Type> AdditionalRegisteredLinkedObjectTypes => GetAdditionalRegisteredLinkedObjectTypes();
	}
}
