using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC182CTransportEquipmentProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("TransportEquipmentType missing", () => new CC182CTransportEquipmentProvider(null));
			});
		}

		public void TestContainerNumber()
		{
			AssertEquals("Container Number", "12", provider.ContainerNumber);
		}

		public void TestNumberofSeals()
		{
			AssertEquals("Number of Seals", "1", provider.NumberofSeals);
		}

		public void TestSealsIdentifier()
		{
			var identifier = provider.SealsIdentifier;
			AssertEquals("123", identifier.Single());
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC182CTransportEquipmentProvider(new TransportEquipmentType07
			{
				SequenceNumber = "1",
				ContainerIdentificationNumber = "12",
				NumberOfSeals = "1",
				Seal = new Collection<SealType04>
				{
					new SealType04
					{
						SequenceNumber = "1",
						Identifier = "123"
					}
				}
			});
		}
		CC182CTransportEquipmentProvider provider;
	}
}
