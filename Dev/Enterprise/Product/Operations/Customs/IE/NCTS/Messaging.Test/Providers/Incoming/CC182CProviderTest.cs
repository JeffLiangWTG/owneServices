using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC182C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.tcl;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC182CProviderTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			CombineAssertions("All properties should return correct value.", () =>
			{
				var incident = provider.Incidents.Single();
				var transportEquipment = incident.TransportEquipment.Single();

				AssertEquals("MorementReferenceNumber", "19AA12345678901230", provider.MovementReferenceNumber);
				AssertEquals("IncidentNotificationDateAndTime", new ZDateTime(2023, 01, 30, 15, 00, 01), provider.IncidentNotificationDateAndTime);
				AssertEquals("CustomsOfficeOfDeparture", "RNALPHN8", provider.CustomsOfficeOfDeparture);
				AssertEquals("CustomsOfficeOfIncidentRegistration", "RNALPHN9", provider.CustomsOfficeOfIncidentRegistration);
				AssertEquals("IncidentCode", "1", incident.IncidentCode);
				AssertEquals("IncidentText", "Incident", incident.IncidentText);
				AssertEquals("EndorsementDate", new ZDate(2023, 1, 20), incident.EndorsementDate);
				AssertEquals("EndorsementAuthority", "A", incident.EndorsementAuthority);
				AssertEquals("EndorsementPlace", "Endorsement Place", incident.EndorsementPlace);
				AssertEquals("EndorsementCountry", "IE", incident.EndorsementCountry);
				AssertEquals("ContainerNumber", "1", transportEquipment.ContainerNumber);
				AssertEquals("NumberofSeals", "1", transportEquipment.NumberofSeals);
				AssertEquals("SealsIdentifier", "1", transportEquipment.SealsIdentifier.Single());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC182CProvider(CreateStandardProvider());
		}
		CC182CProvider provider;

		public static Cc182CType CreateStandardProvider(string mrn = "19AA12345678901230")
		{
			return new Cc182CType
			{
				MessageType = MessageTypes.Cc182C,
				TransitOperation = new TransitOperationType47
				{
					Mrn = mrn,
					IncidentNotificationDateAndTime = new DateTime(2023, 1, 30, 15, 0, 1),
				},
				Consignment = new Collection<IncidentType03>
				{
					new IncidentType03
					{
						Code = "1",
						SequenceNumber = "1",
						Text = "Incident",
						Endorsement = new EndorsementType03
						{
							Date = new DateTime(2023, 1, 20, 15, 0, 1),
							Authority = "A",
							Country = "IE",
							Place = "Endorsement Place"
						},

						Location = new LocationType02
						{
							QualifierOfIdentification = "Q",
							UnLocode = "2",
							Country = "IE",
							Address = new AddressType18
							{
								City = "CITY",
								Postcode = "2020",
								StreetAndNumber = "123 WHERE ST",
							},
						},

						TransportEquipment = new Collection<TransportEquipmentType07>
						{
							new TransportEquipmentType07
							{
								SequenceNumber = "1",
								ContainerIdentificationNumber = "1",
								NumberOfSeals = "1",
								Seal = new Collection<SealType04>
								{
									new SealType04
									{
										SequenceNumber = "1",
										Identifier = "1",
									}
								}
							}
						},
					},
				},
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03 { ReferenceNumber = "RNALPHN8" },
				CustomsOfficeOfIncidentRegistration = new CustomsOfficeOfIncidentRegistrationType02 { ReferenceNumber = "RNALPHN9" },
			};
		}
	}
}
