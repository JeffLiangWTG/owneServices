using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.MasterFiles.Business;
using ZString = CargoWise.Types.ZString;

namespace Enterprise.DataTransfer.Native.Business.Xml.Deserializers.Tests
{
	internal class AddInfoDeserialiserTest : TestCaseWithDummy
	{
		public void TestOversizedAddInfoIsTruncatedToItsMaxLength()
		{
			const int maxLength = 57;
			var exepectedValue = "Description=" + new ZString('x', maxLength);
			var addInfoCollectionElement = new XElement("AddInfoCollection");
			var addInfoElement = new XElement("AddInfo");
			addInfoElement.Add(new XElement("Key", "Description"));
			addInfoElement.Add(new XElement("Value", new ZString('x', maxLength * 2)));
			addInfoCollectionElement.Add(addInfoElement);

			var definitionFinder = new DefinitionFinder { Cache = EntitySetDefinitionCache.GetInstance() };
			var definition = definitionFinder.FindByEntitySetName("Product").Entities.FindDefinition("OrgSupplierPart.CusClassPartPivot");

			var properties = new List<Property>();
			var serializer = new AddInfoDeserialiser(addInfoCollectionElement, properties, definition, "FDA");
			serializer.Deserialise();

			Assert("Description property is deserialized", properties.Any());
			AssertEquals("Over-sized description is truncated to its maximum length", exepectedValue, properties.First().Value.ToString());
		}

		public void TestAddInfoDataWithMID()
		{
			var addInfoCollectionElement =
				new XElement("AddInfoCollection",
					new XElement("AddInfo",
						new XElement("Key", "ManufacturerAddress"),
						new XElement("OrganizationAddress",
							new XElement("RegistrationNumberCollection",
								new XElement("RegistrationNumber",
									new XElement("Type",
										new XElement("Code", "MID")),
									new XElement("CountryOfIssue",
										new XElement("Code", "US")),
									new XElement("Value", "ABCDEFGHIJ01"))))),
					new XElement("AddInfo",
						new XElement("Key", "OA_ShipperAddress"),
						new XElement("OrganizationAddress",
							new XElement("RegistrationNumberCollection",
								new XElement("RegistrationNumber",
									new XElement("Type",
										new XElement("Code", "MID")),
									new XElement("CountryOfIssue",
										new XElement("Code", "US")),
									new XElement("Value", "ABCDEFGHIJ02"))))),
					new XElement("AddInfo",
						new XElement("Key", "DummyAddress1"),
						new XElement("OrganizationAddress",
							new XElement("RegistrationNumberCollection",
								new XElement("RegistrationNumber",
									new XElement("Type",
										new XElement("Code", "MID")),
									new XElement("CountryOfIssue",
										new XElement("Code", "US")),
									new XElement("Value", "ABCDEFGHIJ03"))))),
					new XElement("AddInfo",
						new XElement("Key", "DummyAddress2"),
						new XElement("OrganizationAddress",
							new XElement("RegistrationNumberCollection",
								new XElement("RegistrationNumber",
									new XElement("Type",
										new XElement("Code", "MID")),
									new XElement("CountryOfIssue",
										new XElement("Code", "TW")),
									new XElement("Value", "ABCDEFGHIJ04"))))));

			var manufacturer = Factory.New<OrgHeader>();
			manufacturer.OH_Code = "ORG";
			manufacturer.MainAddress.OA_Code = "101 Main Street";
			manufacturer.MainAddress.Address1 = "101 Main Street";
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "ABCDEFGHIJ03", Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();

			var definitionFinder = new DefinitionFinder { Cache = EntitySetDefinitionCache.GetInstance() };
			var definition = definitionFinder.FindByEntitySetName("Product").Entities.FindDefinition("OrgSupplierPart.CusClassPartPivot.AdditionalInformationChild");

			var properties = new List<Property>();
			var serializer = new AddInfoDeserialiser(addInfoCollectionElement, properties, definition, "FDA");
			serializer.Deserialise();

			Assert("Property is deserialized", properties.Any());
			AssertEquals("Property name", "AddInfoData", properties.First().Name);
			AssertEquals("Addresses now support US MID", string.Concat("DummyAddress1=", manufacturer.MainAddress.PK.ToString(), "*ManufacturerAddress=ABCDEFGHIJ01*OA_ShipperAddress=ABCDEFGHIJ02"), properties.First().Value.ToString());
		}
	}
}
