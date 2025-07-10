using System;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.CC060C;
using CargoWise.Customs.IE.MessageDefinitions.NCTSVersionP5_0.ctypes;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.NCTS.Messaging.Testing
{
	class CC060CProviderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("TransitOperation missing", () => new CC060CProvider(new Cc060CType()));
				AssertExceptionThrown<ArgumentException>("CustomsOfficeOfDeparture missing", () => new CC060CProvider(new Cc060CType()));
				AssertExceptionThrown<ArgumentException>("HolderOfTheTransitProcedure missing", () => new CC060CProvider(new Cc060CType()));
			});
		}

		public void TestMRN()
		{
			AssertEquals("MRN060", provider.MRN);
		}

		public void TestLRN()
		{
			AssertEquals("LRN060", provider.LRN);
		}

		public void TestControlNotificationDateAndTime()
		{
			AssertEquals(new DateTime(2023, 02, 11, 2, 3, 6), provider.ControlNotificationDateAndTime);
		}

		public void TestNotificationType()
		{
			AssertEquals("AB123C", provider.NotificationType);
		}

		public void TestCustomsOfficeOfDeparture()
		{
			AssertEquals("DEPNUM01", provider.CustomsOfficeOfDeparture);
		}

		public void TestHolderOfTheTransitProcedure()
		{
			var holderOfTheTransitProcedure1 = provider.HolderOfTheTransitProcedure;
			AssertType<CC060CHolderOfTheTransitProcedureProvider>(holderOfTheTransitProcedure1);
			var holderOfTheTransitProcedure2 = provider.HolderOfTheTransitProcedure;
			AssertSame("Is cached", holderOfTheTransitProcedure1, holderOfTheTransitProcedure2);
		}

		public void TestRepresentative()
		{
			AssertEquals("Representative is null", null, new CC060CProvider(GetEmptyXmlObject()).Representative);

			var representative1 = provider.Representative;
			AssertType<CC060CRepresentativeProvider>(representative1);
			var representative2 = provider.Representative;
			AssertSame("Is cached", representative1, representative2);
		}

		public void TestTypeOfControls()
		{
			AssertEquals("TypeOfControls is empty", 0, new CC060CProvider(GetEmptyXmlObject()).TypeOfControls.Count);

			var typeOfControls1 = provider.TypeOfControls;
			AssertType<CC060CTypeOfControlProvider>(typeOfControls1.ElementAt(0));
			var typeOfControls2 = provider.TypeOfControls;
			AssertSame("Is cached", typeOfControls1, typeOfControls2);
		}

		public void TestRequestedDocument()
		{
			AssertEquals("RequestedDocuments is empty", 0, new CC060CProvider(GetEmptyXmlObject()).RequestedDocuments.Count);

			var requestedDocuments1 = provider.RequestedDocuments;
			AssertType<CC060RequestedDocumentProvider>(requestedDocuments1.ElementAt(0));
			var requestedDocuments2 = provider.RequestedDocuments;
			AssertSame("Is cached", requestedDocuments1, requestedDocuments2);
		}

		protected override void SetUp()
		{
			base.SetUp();
			provider = new CC060CProvider(new Cc060CType
			{
				TransitOperation = new TransitOperationType22
				{
					Mrn = "MRN060",
					Lrn = "LRN060",
					ControlNotificationDateAndTime = new DateTime(2023, 02, 11, 2, 3, 6),
					NotificationType = "AB123C"
				},
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03
				{
					ReferenceNumber = "DEPNUM01"
				},
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType13
				{
					IdentificationNumber = "IDNUMABC000012345",
					TirHolderIdentificationNumber = "IDNUMABC123450000",
					Name = "BOB THE BUILDER",
					Address = new AddressType07
					{
						City = "CITY",
						Country = "ES",
						Postcode = "2020",
						StreetAndNumber = "123 WHERE ST"
					},
					ContactPerson = new ContactPersonType04
					{
						Name = "WENDY",
						PhoneNumber = "951",
						EMailAddress = "test@test.com"
					}
				},
				Representative = new RepresentativeType04
				{
					IdentificationNumber = "IDREPRES123450000",
					Status = "2",
					ContactPerson = new ContactPersonType04
					{
						Name = "Name 1",
						PhoneNumber = "+001",
						EMailAddress = "name1@test.com"
					}
				},
				TypeOfControls = new Collection<TypeOfControlsType>
				{
					new TypeOfControlsType
					{
						SequenceNumber = "1",
						Type = "ABC",
						Text = "Some text about the type of control"
					},
					new TypeOfControlsType
					{
						SequenceNumber = "2",
						Type = "ZXC",
						Text = "Another text related to the type of control"
					},
				},
				RequestedDocument = new Collection<RequestedDocumentType>
				{
					new RequestedDocumentType
					{
						SequenceNumber = "1",
						DocumentType = "DT01",
						Description = "Some description"
					},
					new RequestedDocumentType
					{
						SequenceNumber = "2",
						DocumentType = "DT02",
						Description = "Other text"
					}
				}
			});
		}
		CC060CProvider provider;

		Cc060CType GetEmptyXmlObject()
		{
			return new Cc060CType
			{
				TransitOperation = new TransitOperationType22 { },
				CustomsOfficeOfDeparture = new CustomsOfficeOfDepartureType03 { },
				HolderOfTheTransitProcedure = new HolderOfTheTransitProcedureType13 { },
			};
		}
	}
}
