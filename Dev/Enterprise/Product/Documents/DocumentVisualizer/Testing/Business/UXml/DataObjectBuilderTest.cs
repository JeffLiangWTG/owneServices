using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using CargoWise.IO;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;
using DataContext = Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11.DataContext;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class DataObjectBuilderTest : TestCase
	{
		#region TestBuildDataObject

		public void TestBuildDataObject()
		{
			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PortOfLoading = new UNLOCO
				{
					Code = "AUSYD",
					Name = "Sydney"
				},
				PortOfDischarge = new UNLOCO
				{
					Code = "PLGDN",
					Name = "Gdansk"
				},
			};
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
					{
						AddressType = "Consignee",
						Address1 = "Unit 3a",
						Address2 = "69 Lost Lane",
						State = "NSW",
						Postcode = "2015",
						City = "Alexandria"
					}
				});

			const string expected =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <PortOfDischarge Name=""Gdansk"">PLGDN</PortOfDischarge>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
    <OrganizationAddressCollection>
      <OrganizationAddress>
        <AddressType>Consignee</AddressType>
        <Address1>Unit 3a</Address1>
        <Address2>69 Lost Lane</Address2>
        <City>Alexandria</City>
        <Postcode>2015</Postcode>
        <State>NSW</State>
      </OrganizationAddress>
    </OrganizationAddressCollection>
  </Shipment>
</UniversalShipment>";

			AssertBuildDataObject(dataObject, expected);
		}

		public void TestBuildDataObjectDoesNotThrowExceptionWhenCollectionElementValueIsNull()
		{
			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				PortOfLoading = new UNLOCO
				{
					Code = "AUSYD",
					Name = "Sydney"
				},
			};
			dataObject.SetAdditionalReferenceCollection(() => new DataObjectList<AdditionalReference>());

			const string expected = @"<?xml version=""1.0"" encoding=""utf-16""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <PortOfLoading Name=""Sydney"">AUSYD</PortOfLoading>
  </Shipment>
</UniversalShipment>";

			var dynamicData = dataObject.MakeDynamic();

			var refNumbersCollection = (IDynamicDataCollection)dynamicData.GetDynamicProperty(nameof(dataObject.AdditionalReferenceCollection));
			var refNumber = refNumbersCollection.Create();

			AssertNotNull("prerequisite: collection element has been created", refNumber);
			AssertContainsExactElementsInAnyOrder("prerequisite: collection contains created element",
					new[] { refNumber }, refNumbersCollection);

			AssertBuildDataObject(dynamicData, expected);
		}

		#endregion

		#region TestBuildDataObjectDataContext

		public void TestBuildDataObjectDataContext()
		{
			var dataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = DataContextFactory.New(UniversalXmlInfo.Namespace_2012_11)
			};

			dataObject.DataContext.AddDataTarget(DataContextType.ForwardingConsol, "Consol Id");

			var asmShipment = new Shipment(DefaultDataObjectWriterStrategy.Instance)
			{
				DataContext = new DataContext
				{
					DataSource = new DataSource
					{
						Key = "ASM Shipment Id",
						Type = "ForwardingShipment"
					}
				},
				ShipmentType = new CodeDescriptionPair
				{
					Code = "ASM",
					Description = "Assembly Master"
				}
			};

			asmShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>
				{
					new Shipment(DefaultDataObjectWriterStrategy.Instance)
					{
						DataContext = new DataContext
						{
							DataSource = new DataSource
							{
								Key = "STD Shipment Id",
								Type = "ForwardingShipment"
							}
						},
						ShipmentType = new CodeDescriptionPair
						{
							Code = "STD",
							Description = "Standard"
						}
					}
				});

			dataObject.SetSubShipmentCollection(() => new DataObjectList<Shipment>
			{
				asmShipment
			});

			const string expected =
@"<?xml version=""1.0"" encoding=""utf-16""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2012/11"" version=""2.0"">
  <Shipment>
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Key>Consol Id</Key>
          <Type>ForwardingConsol</Type>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <SubShipmentCollection>
      <SubShipment>
        <DataContext>
          <DataSource>
            <Key>ASM Shipment Id</Key>
            <Type>ForwardingShipment</Type>
          </DataSource>
        </DataContext>
        <ShipmentType Description=""Assembly Master"">ASM</ShipmentType>
        <SubShipmentCollection>
          <SubShipment>
            <DataContext>
              <DataSource>
                <Key>STD Shipment Id</Key>
                <Type>ForwardingShipment</Type>
              </DataSource>
            </DataContext>
            <ShipmentType Description=""Standard"">STD</ShipmentType>
          </SubShipment>
        </SubShipmentCollection>
      </SubShipment>
    </SubShipmentCollection>
  </Shipment>
</UniversalShipment>";

			AssertBuildDataObject(dataObject, expected);
		}

		#endregion

		#region Implementation

		void AssertBuildDataObject(Shipment dataObject, string expected)
		{
			AssertBuildDataObject(dataObject.MakeDynamic(), expected);
		}

		void AssertBuildDataObject(IDynamicData dynamicData, string expected)
		{
			var ns = UniversalXmlInfo.Namespace_2012_11;

			var builder = new DataObjectBuilder(ns, dynamicData);
			var result = builder.Build();

			XDocument xml;

			using (var stream = (SubStreamableStream)new MemoryStream())
			{
				var writer = new UniversalDataBuss.XmlIO.XmlWriting.XmlWriter();
				writer.WriteXML(result, stream, ns);
				xml = XDocument.Load(stream);
			}

			AssertMultilineASCIIEquals("xml", expected, xml.ToXmlString());
		}

		#endregion
	}
}
