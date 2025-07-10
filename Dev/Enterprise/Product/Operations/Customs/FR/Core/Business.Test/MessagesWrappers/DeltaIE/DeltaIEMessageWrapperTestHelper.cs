using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	public static class DeltaIEMessageWrapperTestHelper
	{
		public static void AssertAdditionalSupplyChainActor(ICollection<IAdditionalSupplyChainActor> additionalSupplyChainActors, int count, string[] identificationNumbers, string[] roles, string[] sequenceNumbers)
		{
			if (identificationNumbers.Length != count)
			{
				Assertion.Fail("Expected identificationNumbers length not equal to expected count.");
			}
			else if (roles.Length != count)
			{
				Assertion.Fail("Expected roles length not equal to expected count.");
			}
			else if (sequenceNumbers.Length != count)
			{
				Assertion.Fail("Expected sequenceNumbers length not equal to expected count.");
			}
			Assertion.AssertType<Collection<IAdditionalSupplyChainActor>>(additionalSupplyChainActors);
			Assertion.AssertEquals("AdditionalSupplyChainActor: Count", count, additionalSupplyChainActors.Count);
			for (var i = 0; i < additionalSupplyChainActors.Count; i++)
			{
				AssertAdditionalSupplyChainActor_Single(additionalSupplyChainActors.ElementAt(i), identificationNumbers[i], roles[i], sequenceNumbers[i]);
			}
		}

		public static void AssertAdditionalSupplyChainActor_Single(IAdditionalSupplyChainActor additionalSupplyChainActor, string identificationNumber, string role, string sequenceNumber)
		{
			Assertion.AssertType<AdditionalSupplyChainActorWrapper>(additionalSupplyChainActor);
			Assertion.AssertEquals("AdditionalSupplyChainActor: IdentificationNumber", identificationNumber, additionalSupplyChainActor.IdentificationNumber);
			Assertion.AssertEquals("AdditionalSupplyChainActor: Role", role, additionalSupplyChainActor.Role);
		}

		public static void AssertSeller(ISeller seller, IAddress address, string name, string identificationNumber)
		{
			Assertion.AssertType<SellerWrapper>(seller);
			AssertAddress(seller.Address, address.City, address.Country, address.Postcode, address.StreetAndNumber);
			Assertion.AssertEquals("Seller: Name", name, seller.Name);
			Assertion.AssertEquals("Seller: IdentificationNumber", identificationNumber, seller.IdentificationNumber);
		}

		public static void AssertExporter(IExporter exporter, IAddress address, string name, string identificationNumber)
		{
			Assertion.AssertType<ExporterWrapper>(exporter);
			AssertAddress(exporter.Address, address.City, address.Country, address.Postcode, address.StreetAndNumber);
			Assertion.AssertEquals("Exporter: Name", name, exporter.Name);
			Assertion.AssertEquals("Exporter: IdentificationNumber", identificationNumber, exporter.IdentificationNumber);
		}

		public static void AssertConsignee(IConsignee consignee, IAddress address, string name, string identificationNumber)
		{
			Assertion.AssertType<ConsigneeWrapper>(consignee);
			AssertAddress(consignee.Address, address.City, address.Country, address.Postcode, address.StreetAndNumber);
			Assertion.AssertEquals("Consignee: Name", name, consignee.Name);
			Assertion.AssertEquals("Consignee: IdentificationNumber", identificationNumber, consignee.IdentificationNumber);
		}

		public static void AssertBuyer(IBuyer buyer, IAddress address, string name, string identificationNumber)
		{
			Assertion.AssertType<BuyerWrapper>(buyer);
			AssertAddress(buyer.Address, address.City, address.Country, address.Postcode, address.StreetAndNumber);
			Assertion.AssertEquals("Buyer: Name", name, buyer.Name);
			Assertion.AssertEquals("Buyer: IdentificationNumber", identificationNumber, buyer.IdentificationNumber);
		}
		public static IAddress GetAddressMock(string city, string country, string postcode, string streetAndNumber)
		{
			var address = new Mock<IAddress>();
			address.Setup(x => x.City).Returns(city);
			address.Setup(x => x.Country).Returns(country);
			address.Setup(x => x.Postcode).Returns(postcode);
			address.Setup(x => x.StreetAndNumber).Returns(streetAndNumber);
			return address.Object;
		}

		public static void AssertAddress(IAddress address, string city, string country, string postcode, string streetAndNumber)
		{
			Assertion.AssertType<OrganisationAddressWrapper>(address);
			Assertion.AssertEquals("Address: City", city, address.City);
			Assertion.AssertEquals("Address: Country", country, address.Country);
			Assertion.AssertEquals("Address: Postcode", postcode, address.Postcode);
			Assertion.AssertEquals("Address: StreetAndNumber", streetAndNumber, address.StreetAndNumber);
		}
	}
}
