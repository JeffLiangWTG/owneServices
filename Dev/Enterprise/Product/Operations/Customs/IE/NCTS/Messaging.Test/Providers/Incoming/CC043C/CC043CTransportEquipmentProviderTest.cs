using System;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC043CTransportEquipmentProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("TransportEquipmentType missing", () => new CC043CTransportEquipmentProvider(null));
			});
		}

		public void TestSequenceNumber()
		{
			AssertEquals((ZShort)1, provider.SequenceNumber);
		}

		public void TestContainerIdentificationNumber()
		{
			AssertEquals("1234", provider.ContainerIdentificationNumber);
		}

		public void TestNumberOfSeals()
		{
			AssertEquals((ZShort)2, provider.NumberOfSeals);
		}

		public void TestSeals()
		{
			AssertType<CC043CSealProvider[]>(provider.Seals);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC043CTransportEquipmentProvider(new TransportEquipmentType05
			{
				SequenceNumber = "1",
				ContainerIdentificationNumber = "1234",
				NumberOfSeals = "2"
			});
		}
		CC043CTransportEquipmentProvider provider;
	}
}
