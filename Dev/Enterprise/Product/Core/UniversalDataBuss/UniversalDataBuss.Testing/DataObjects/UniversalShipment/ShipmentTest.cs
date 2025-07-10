using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Application;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(Shipment))]
	class ShipmentTest : TopLevelDataObjectTestCase<Shipment>
	{
		#region TestGetSourceDataObject

		public void TestGetSourceDataObject()
		{
			var consol = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			consol.DataContext = DataContextFactory.New();
			consol.DataContext.AddDataSource(DataContextType.ForwardingConsol, "DUM123");
			consol.DataContext.AddDataSource(DataContextType.ForwardingShipment, "DUM456");
			consol.DataContext.AddDataSource(DataContextType.CustomsDeclaration, "DUM789");

			var masterShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			masterShipment.DataContext = DataContextFactory.New();
			masterShipment.DataContext.AddDataSource(DataContextType.DummyBusinessObject, "DUM789");

			var shipmentAndCustoms = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipmentAndCustoms.DataContext = DataContextFactory.New();
			shipmentAndCustoms.DataContext.AddDataSource(DataContextType.ForwardingShipment, "DUM456");
			shipmentAndCustoms.DataContext.AddDataSource(DataContextType.CustomsDeclaration, "DUM789");

			consol.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			consol.SubShipmentCollection.Add(masterShipment);

			masterShipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>());
			masterShipment.SubShipmentCollection.Add(shipmentAndCustoms);

			AssertEquals("Source should be shipmentAndCustoms.", shipmentAndCustoms, Shipment.GetSourceDataObject(consol));
			AssertEquals("Not Top Level, so returns itself.", masterShipment, Shipment.GetSourceDataObject(masterShipment));
			AssertEquals("Not Top Level, so returns itself.", shipmentAndCustoms, Shipment.GetSourceDataObject(shipmentAndCustoms));
		}

		#endregion

		#region TestEmptyObjectWritesFineThroughXmlWriter

		public void TestEmptyObjectWritesFineThroughXmlWriter()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

				using (var stream = (SubStreamableStream)new MemoryStream())
				{
					var xmlWriter = ObjectFactory.Get<IXmlWriter>();
					xmlWriter.WriteXML(shipment, stream);

					using (var reader = new StreamReader(stream))
					{
						string result = reader.ReadToEnd();
						AssertMultilineASCIIEquals("Serialized UberShipment", EmptyShipmentXML.Trim(), result);
					}
				}
			}
		}

		#endregion

		#region EmptyShipmentXML

		const string EmptyShipmentXML = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment>
  </Shipment>
</UniversalShipment>
";
		#endregion

		#region DocData

		public void TestIsDocDataRestricted()
		{
			eAdaptorRegistry.Instance.SupportDocDataInUniversalShipmentXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals(true, Shipment.IsDocDataRestricted);

			eAdaptorRegistry.Instance.SupportDocDataInUniversalShipmentXML.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals(false, Shipment.IsDocDataRestricted);
		}

		#endregion

		protected override List<string> ExpectedAllowLineControlWhiteSpaceAttributePropertiesCore() => new List<string>()
		{
			nameof(Shipment.GoodsDescription)
		};

		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>
			{
				{ nameof(Shipment.AdditionalTerms), JobOrderHeaderSchema.JD_AdditionalTerms.MaxLength },
				{ nameof(Shipment.AgentsReference), Math.Max(Math.Max(JobShipmentSchema.JS_BookingReference.MaxLength, JobDeclarationSchema.JE_AgentsReference.MaxLength), JobShipmentSchema.JS_ConsolReference.MaxLength) },
				{ nameof(Shipment.AgreedPlaceCode), JobEUDeclarationSchema.EUD_AgreedPlaceCode.MaxLength },
				{ nameof(Shipment.CartageWaybillNumber), JobShipmentSchema.JS_CartageWaybill.MaxLength },
				{ nameof(Shipment.FirstBuyerContact), JobOrderHeaderSchema.JD_FirstBuyerContact.MaxLength },
				{ nameof(Shipment.Folio), Math.Max(Math.Max(JobDeclarationSchema.JE_Folio.MaxLength, CusMAWBSchema.CM_Folio.MaxLength), CusHAWBSchema.CS_FolioReference.MaxLength) },
				{ nameof(Shipment.HBLContainerPackModeOverride), JobShipmentSchema.JS_HBLContainerPackModeOverride.MaxLength },
				{ nameof(Shipment.InterimReceiptNumber), JobShipmentSchema.JS_InterimReceipt.MaxLength },
				{ nameof(Shipment.LloydsIMO), Math.Max(Math.Max(JobDeclarationSchema.JE_LloydsIMO.MaxLength, CusOutturnHeaderSchema.C6_LloydsIMO.MaxLength), CusInBondHeaderSchema.BH_LloydsNumber.MaxLength) },
				{ nameof(Shipment.OwnerRef), JobDeclarationSchema.JE_OwnerRef.MaxLength },
				{ nameof(Shipment.DefermentAccountNumber), JobDeclarationSchema.JE_DefermentAccountNumber.MaxLength },
				{ nameof(Shipment.SecondBuyerContact), JobOrderHeaderSchema.JD_SecondBuyerContact.MaxLength },
				{ nameof(Shipment.VesselName), Math.Max(RefVesselSchema.RV_Code.MaxLength, WhsItemReceiveTransportationUnitSchema.WRH_VehicleReference.MaxLength) },
				{ nameof(Shipment.VoyageFlightNo), Math.Max(Math.Max(JobVoyageSchema.JV_VoyageFlight.MaxLength, CusMAWBSchema.CM_FlightNo.MaxLength), Math.Max(CusSCAOceanBillSchema.CB_Voyage.MaxLength, JobDeclarationSchema.JE_VoyageFlightNo.MaxLength)) },
				{ nameof(Shipment.WarehouseLocation), Math.Max(WhsLocationViewSchema.WLV_LocationString.MaxLength, Math.Max(CusHAWBSchema.CS_WarehouseLocation.MaxLength, JobShipmentSchema.JS_WarehouseLocation.MaxLength)) },
				{ nameof(Shipment.WayBillNumber), Math.Max(CusMAWBSchema.CM_MAWB.MaxLength, Math.Max(HVLVConsignmentSchema.HVC_WaybillNumber.MaxLength, JobDeclarationSchema.JE_HouseBill.MaxLength)) },
				{ nameof(Shipment.CoLoadMasterBillNumber), JobConsolSchema.JK_CoLoadMasterBill.MaxLength },
				{ nameof(Shipment.ConsigneeBussinessNumber), CusSCAHouseSchema.CA_ConsigneeBusinessNumber.MaxLength },
				{ nameof(Shipment.SlotReference), Math.Max(JobContainerSchema.JC_ArrivalSlotReference.MaxLength, Math.Max(DtbBookingConfirmationSchema.KK_SlotReference.MaxLength, GateBookingDetailSchema.GTD_SlotReference.MaxLength)) },
				{ nameof(Shipment.ManifestNumber), JobDeclarationSchema.JE_ManifestNumber.MaxLength },
				{ nameof(Shipment.BookingConfirmationReference), JobConsolSchema.JK_BookingReference.MaxLength },
				{ nameof(Shipment.CFSReference), JobConsolSchema.JK_CustomsReference.MaxLength },
				{ nameof(Shipment.CoLoadBookingConfirmationReference), JobConsolSchema.JK_CoLoadBookingReference.MaxLength },
				{ nameof(Shipment.ConsigneeIdentifier), CusSCAHouseSchema.CA_ConsigneeIdentifier.MaxLength },
				{ nameof(Shipment.ConsignorIdentifier), CusSCAHouseSchema.CA_ConsignorIdentifier.MaxLength },
				{ nameof(Shipment.QuoteNumber), Math.Max(JobCartageSchema.JJ_QuoteNumber.MaxLength, AsycudaBillSchema.ABL_CarrierReference.MaxLength) },
				{ nameof(Shipment.GoodsDescription), GoodsDescriptionMaxLength },
				{ nameof(Shipment.CarrierContractNumber), JobConsolSchema.JK_CarrierContractNumber.MaxLength },
				{ nameof(Shipment.MarksAndNumbers),JobSupplierBookingSchema.JSB_MarksAndNumbers.MaxLength },
				{ nameof(Shipment.CarrierAccountBillingType),OrgWhsClientAccountAssociationSchema.OWC_BillingType.MaxLength },
				{ nameof(Shipment.GoodsDestination), JobDeclarationSchema.JE_GoodsDestination.MaxLength },
				{ nameof(Shipment.FMCTariffID), JobShipmentSchema.JS_FMCTariffID.MaxLength },
				{ nameof(Shipment.RateCommodity), JobShipmentSchema.JS_RH_NKRateCommodity.MaxLength },
				{ nameof(Shipment.ElectronicBillOfLadingReference), JobConsolSchema.JK_ElectronicBillOfLadingReference.MaxLength },

				// The below constant Max Length need to be filled in with schema max lengths over time to make sure both the max lengths must match
				{ nameof(Shipment.VendorIdentifier), Math.Max(Math.Max(SupplierBookingLineSchema.DL_VendorIdentifier.MaxLength, CusSCAHouseSchema.CA_VendorIdentifier.MaxLength), CusHAWBSchema.CS_VendorIdentifier.MaxLength) },// HVLVConsignmentSchema.HVC_VendorIdentifier.MaxLength=35
				{ nameof(Shipment.UniqueConsignmentReference), JobDeclarationSchema.JE_UCR.MaxLength },
				{ nameof(Shipment.ConsignmentNote), DtbConsignmentSchema.LTC_ConnoteNumber.MaxLength },
				{ nameof(Shipment.Direction), DtbConsignmentSchema.LTC_Direction.MaxLength  },
			};
		}

		static int GoodsDescriptionMaxLength => new[]
		{
			CusHAWBItemsSchema.CHI_GoodsDescription, CusHAWBSchema.CS_GoodsDescription,
			CusOutturnSchema.C5_GoodsDescription, CusSCAPivotSchema.CV_GoodsDescription,
			CusUSLVItemSchema.ULI_GoodsDescription, DtbBookingConsolidationSchema.KB_GoodsDescription,
			JobCartageSchema.JJ_GoodsDescription,
			JobDeclarationSchema.JE_GoodsDescription, JobShipmentSchema.JS_GoodsDescription,
			AsycudaBillSchema.ABL_GoodsDescription
		}.Max(x => x.MaxLength);

		public void TestShipmentAttributesAttribute()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var shipmentAttributesAttribute = shipment.GetType().GetAttribute<DataObjectAttributesAttribute>();
			AssertNotNull(shipmentAttributesAttribute);
			AssertEquals("AllowUpdateOfCustomsDeclarationAfterCommencement", true, shipmentAttributesAttribute.HasAttributeDefined(nameof(shipment.AllowUpdateOfCustomsDeclarationAfterCommencement)));

			var xsdGenerator = ObjectFactory.Get<IUniversalXsdGenerator>();
			var outputXML = xsdGenerator.GetCommonSchemaXsdOutput();
			AssertContains(@"<xs:attribute name=""AllowUpdateOfCustomsDeclarationAfterCommencement"" type=""xs:boolean"" />", outputXML);

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(string.Format(EmptyShipmentXMLForRead, false))))
			{
				var xmlReader = ObjectFactory.Get<IXmlReader>();
				var logger = new TestErrorLogger();
				xmlReader.ReadXML(shipment, stream, logger);
				AssertEquals(false, shipment.AllowUpdateOfCustomsDeclarationAfterCommencement.Value);
			}

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(string.Format(EmptyShipmentXMLForRead, true))))
			{
				shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				var xmlReader = ObjectFactory.Get<IXmlReader>();
				var logger = new TestErrorLogger();
				xmlReader.ReadXML(shipment, stream, logger);
				AssertEquals(true, shipment.AllowUpdateOfCustomsDeclarationAfterCommencement.Value);
			}

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(string.Format(NonEmptyShipmentXMLForRead, true))))
			{
				shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				var xmlReader = ObjectFactory.Get<IXmlReader>();
				var logger = new TestErrorLogger();
				xmlReader.ReadXML(shipment, stream, logger);
				AssertEquals(true, shipment.AllowUpdateOfCustomsDeclarationAfterCommencement.Value);
			}

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(EmptyShipmentXML)))
			{
				shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				var xmlReader = ObjectFactory.Get<IXmlReader>();
				var logger = new TestErrorLogger();
				AssertNoExceptionThrown(() =>
				{
					xmlReader.ReadXML(shipment, stream, logger);
				});
				AssertEquals(false, shipment.AllowUpdateOfCustomsDeclarationAfterCommencement.HasValue);
			}
		}

		#region Empty Shipment XML for Read

		const string EmptyShipmentXMLForRead = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment AllowUpdateOfCustomsDeclarationAfterCommencement=""{0}"">
  </Shipment>
</UniversalShipment>
";

		const string NonEmptyShipmentXMLForRead = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment AllowUpdateOfCustomsDeclarationAfterCommencement=""{0}"">
    <AdditionalTerms>FOB</AdditionalTerms>
  </Shipment>
</UniversalShipment>
";

		const string ShipmentXMLWithDataContextForRead = @"
<?xml version=""1.0"" encoding=""utf-8""?>
<UniversalShipment xmlns=""http://www.cargowise.com/Schemas/Universal/2011/11"" version=""1.1"">
  <Shipment AllowUpdateOfCustomsDeclarationAfterCommencement=""{0}"">
    <DataContext>
      <DataTargetCollection>
        <DataTarget>
          <Type>{1}</Type>
          <Key>AMS0008008</Key>
        </DataTarget>
      </DataTargetCollection>
    </DataContext>
    <AdditionalTerms>FOB</AdditionalTerms>
  </Shipment>
</UniversalShipment>
";
		#endregion

		public void TestIDataObjectParseSupporter()
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var supporter = shipment as IDataObjectParseSupporter;

			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(string.Format(EmptyShipmentXMLForRead, true))))
			{
				var xmlReader = ObjectFactory.Get<IXmlReader>();
				var logger = new TestErrorLogger();
				xmlReader.ReadXML(shipment, stream, logger);
				AssertEquals(true, shipment.AllowUpdateOfCustomsDeclarationAfterCommencement.Value);
				AssertDataObjectParseSupporter(shipment, supporter, false);
				shipment.AllowUpdateOfCustomsDeclarationAfterCommencement = false;
				AssertDataObjectParseSupporter(shipment, supporter, false);
			}
			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(string.Format(ShipmentXMLWithDataContextForRead, true, nameof(DataContextType.CustomsDeclaration)))))
			{
				var xmlReader = ObjectFactory.Get<IXmlReader>();
				var logger = new TestErrorLogger();
				xmlReader.ReadXML(shipment, stream, logger);
				AssertEquals(true, shipment.AllowUpdateOfCustomsDeclarationAfterCommencement.Value);
				AssertDataObjectParseSupporter(shipment, supporter, true);
				shipment.AllowUpdateOfCustomsDeclarationAfterCommencement = false;
				AssertDataObjectParseSupporter(shipment, supporter, true);
			}
			using (var stream = (SubStreamableStream)new MemoryStream(UTF8Encoding.UTF8.GetBytes(string.Format(ShipmentXMLWithDataContextForRead, true, "TEST"))))
			{
				var xmlReader = ObjectFactory.Get<IXmlReader>();
				var logger = new TestErrorLogger();
				xmlReader.ReadXML(shipment, stream, logger);
				AssertEquals(true, shipment.AllowUpdateOfCustomsDeclarationAfterCommencement.Value);
				AssertDataObjectParseSupporter(shipment, supporter, false);
				shipment.AllowUpdateOfCustomsDeclarationAfterCommencement = false;
				AssertDataObjectParseSupporter(shipment, supporter, false);
			}
			AssertContains(@"Only the following elements are supported when 'AllowUpdateOfCustomsDeclarationAfterCommencement' is flagged as true.", supporter.GetErrorTextWhenNonSupportedElementsFound());
		}

		void AssertDataObjectParseSupporter(Shipment shipment, IDataObjectParseSupporter supporter, bool isDeclaration)
		{
			var properties = shipment.GetType().GetProperties();
			var supportedElementsAfterCustomsDeclarationCommenced = new ZString[] { nameof(shipment.DataContext), nameof(shipment.AdditionalReferenceCollection), nameof(shipment.NoteCollection) };

			AssertNotNull("Shipment implements IDataObjectParseSupporter", supporter);
			var isSupportLimited = shipment.AllowUpdateOfCustomsDeclarationAfterCommencement.GetValueOrDefault() && isDeclaration;
			foreach (var property in properties)
			{
				var propertyName = property.Name;
				if (isSupportLimited)
				{
					AssertEquals("AllowUpdate and declaration only support a few elements", supportedElementsAfterCustomsDeclarationCommenced.Contains(propertyName), supporter.IsElementSupported(propertyName));
				}
				else
				{
					AssertEquals("Not allowUpdate or not declaration support all elements", true, supporter.IsElementSupported(propertyName));
				}
			}
		}
	}
}
