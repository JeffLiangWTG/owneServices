using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Chief.Messaging.DLU;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Chief.DLU.Testing
{
	[TestedType(typeof(DLUMessage))]
	class DLUMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDefaults()
		{
			var bo = (DLUMessage)GetNewBusinessObject();
			AssertEquals(ChiefConstants.CusDecTypeDLU, bo.EM_MessageType);
		}
	}

	[TestedType(typeof(DLUMessageCollection))]
	class DLUMessageCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(DLUMessageCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new DLUMessageCollection(Factory);
		}
	}
}
