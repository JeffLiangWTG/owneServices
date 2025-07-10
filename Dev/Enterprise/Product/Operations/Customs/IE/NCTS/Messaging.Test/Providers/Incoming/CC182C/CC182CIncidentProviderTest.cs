using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC182CIncidentProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("Incident missing", () => new CC182CIncidentProvider(null));
			});
		}

		public void TestIncidentCode()
		{
			AssertEquals("Incident Code", "Code", provider.IncidentCode);
		}

		public void TestIncidentText()
		{
			AssertEquals("Incident text", "Incident", provider.IncidentText);
		}

		public void TestEndorsementDate()
		{
			AssertEquals("Endorsement Date", new ZDate(2023, 1, 20), provider.EndorsementDate);
		}

		public void TestEndorsementAuthority()
		{
			AssertEquals("Endorsement Authority", "A", provider.EndorsementAuthority);
		}

		public void TestEndorsementPlace()
		{
			AssertEquals("Endorsement Place", "Endorsement Place", provider.EndorsementPlace);
		}

		public void TestEndorsementCountry()
		{
			AssertEquals("Endorsement Country", "IE", provider.EndorsementCountry);
		}

		public void TestTransportEquipment()
		{
			var transportEquipment = provider.TransportEquipment.Single();
			AssertEquals("12", transportEquipment.ContainerNumber);
			AssertEquals("1", transportEquipment.NumberofSeals);
			AssertEquals("123", transportEquipment.SealsIdentifier.Single());
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC182CIncidentProvider(new IncidentType03
			{
				SequenceNumber = "1",
				Code = "Code",
				Text = "Incident",
				Endorsement = new EndorsementType03
				{
					Date = new DateTime(2023, 1, 20, 15, 0, 1),
					Authority = "A",
					Country = "IE",
					Place = "Endorsement Place"
				},
				TransportEquipment = new Collection<TransportEquipmentType07>
						{
							new TransportEquipmentType07
							{
								SequenceNumber = "1",
								ContainerIdentificationNumber = "12",
								NumberOfSeals = "1",
								Seal = new Collection<SealType04>
								{
									new SealType04
									{
										SequenceNumber = "1",
										Identifier = "123",
									}
								}
							}
						},
			});
		}
		CC182CIncidentProvider provider;
	}
}
