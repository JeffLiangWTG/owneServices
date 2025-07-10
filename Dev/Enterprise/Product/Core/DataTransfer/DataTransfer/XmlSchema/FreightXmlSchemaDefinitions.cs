using System;
using System.Xml.Schema;

namespace Enterprise.DataTransfer.Xml
{
	public class FreightXmlSchemaDefinitions : XmlSchemaDefinitionsBase
	{
		#region Instance

		protected FreightXmlSchemaDefinitions()
		{
		}

		public static FreightXmlSchemaDefinitions Instance
		{
			get
			{
				FreightXmlSchemaDefinitions result = (FreightXmlSchemaDefinitions)WeakInstance.Target;
				if (result == null)
				{
					result = new FreightXmlSchemaDefinitions();
					WeakInstance.Target = result;
				}
				return result;
			}
		}

		static WeakReference WeakInstance
		{
			get { return weakInstance ?? (weakInstance = new WeakReference(null)); }
		}
		[ThreadStatic] static WeakReference weakInstance;

		#endregion

		[ExpectXmlSchemaContainsRootElement("Shipments")]
		public XmlSchema ShipmentsSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "Shipment.xsd"); }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		[ExpectXmlSchemaContainsRootElement("Shipment")]
		public XmlSchema SingleShipmentSchema
		{
			get { return GetCompiledSchemaNestedElement(ShipmentsSchema, "Shipments"); }
		}

		[ExpectXmlSchemaContainsRootElement("Consol")]
		public XmlSchema ConsolAndShipmentSchema
		{
			get
			{
				if (fConsolAndShipmentSchema == null)
				{
					XmlSchema readSchema = GetCompiledSchema(DefaultBaseResourceName, "ConsolAndShipmentForInternalUse.xsd");
					fConsolAndShipmentSchema = new XsdSchemaBuilder().CloneSchema(readSchema);
					XmlSchemaElement rootElement = (XmlSchemaElement)fConsolAndShipmentSchema.Items[0];
					rootElement.Name = "Consol";
					new XsdSchemaBuilder().CompileSchema(fConsolAndShipmentSchema);
				}
				return fConsolAndShipmentSchema;
			}
		}
		XmlSchema fConsolAndShipmentSchema;

		[ExpectXmlSchemaContainsRootElement("ShipmentBookings")]
		public XmlSchema ShipmentBookingsSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "ShipmentBooking.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("ShipmentBooking")]
		public XmlSchema SingleShipmentBookingSchema
		{
			get { return GetCompiledSchemaNestedElement(ShipmentBookingsSchema, "ShipmentBookings"); }
		}

		[ExpectXmlSchemaContainsRootElement("AWBHeaders")]
		public XmlSchema AWBHeadersSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "ExportAWBHeader.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("AWBHeader")]
		public XmlSchema SingleAWBHeaderSchema
		{
			get { return GetCompiledSchemaNestedElement(AWBHeadersSchema, "AWBHeaders"); }
		}

		[ExpectXmlSchemaContainsRootElement("Consols")]
		public XmlSchema ConsolsSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "Consol.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("Consol")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public XmlSchema SingleConsolSchema
		{
			get { return GetCompiledSchemaNestedElement(ConsolsSchema, "Consols"); }
		}

		[ExpectXmlSchemaContainsRootElement("Schedules")]
		public XmlSchema SchedulesSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "Schedule.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("Schedule")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public XmlSchema SingleScheduleSchema
		{
			get { return GetCompiledSchemaNestedElement(SchedulesSchema, "Schedules"); }
		}

		[ExpectXmlSchemaContainsRootElement("Sailing")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public XmlSchema SailingSchema
		{
			get { return GetCompiledSchemaWithElementOfType(ConsolsSchema, "Sailing", "Sailing"); }
		}

		[ExpectXmlSchemaContainsRootElement("RoadRailFlight")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public XmlSchema RoadRailFlightSchema
		{
			get { return GetCompiledSchemaWithElementOfType(ConsolsSchema, "Flight", "RoadRailFlight"); }
		}

		[ExpectXmlSchemaContainsRootElement("SailingBase")]
		public XmlSchema SailingBaseSchema
		{
			get { return GetCompiledSchemaWithElementOfType(ConsolsSchema, "SailingBase", "SailingBase"); }
		}

		[ExpectXmlSchemaContainsRootElement("CartageJobs")]
		public XmlSchema CartageJobsSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "CartageJob.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("CartageJob")]
		public XmlSchema SingleCartageJobSchema
		{
			get { return GetCompiledSchemaNestedElement(CartageJobsSchema, "CartageJobs"); }
		}

		[ExpectXmlSchemaContainsRootElement("Orders")]
		public XmlSchema OrdersSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "Order.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("Order")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public XmlSchema SingleOrderSchema
		{
			get { return GetCompiledSchemaNestedElement(OrdersSchema, "Orders"); }
		}

		[ExpectXmlSchemaContainsRootElement("ContainerType")]
		public XmlSchema SingleContainerType
		{
			get { return GetCompiledSchemaWithElementOfType(XmlSchemaDefinitions.Instance.ElementsSchema, "ContainerType", "ContainerType"); }
		}

		[ExpectXmlSchemaContainsRootElement("Container")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public XmlSchema SingleJobContainer
		{
			get { return GetCompiledSchemaWithElementOfType(XmlSchemaDefinitions.Instance.ElementsSchema, "Container", "Container"); }
		}

		[ExpectXmlSchemaContainsRootElement("Package")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public XmlSchema SinglePackage
		{
			get { return GetCompiledSchemaWithElementOfType(XmlSchemaDefinitions.Instance.ElementsSchema, "Package", "Package"); }
		}

		[ExpectXmlSchemaContainsRootElement("InnerPackage")]
		public XmlSchema SingleInnerPackage
		{
			get { return GetCompiledSchemaWithElementOfType(XmlSchemaDefinitions.Instance.ElementsSchema, "PackageBase", "InnerPackage"); }
		}

		[ExpectXmlSchemaContainsRootElement("AgencyBillsOfLading")]
		public XmlSchema AgencyBillsOfLadingSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "AgencyBillOfLading.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("AgencyBillOfLading")]
		public XmlSchema SingleAgencyBillOfLadingSchema
		{
			get { return GetCompiledSchemaNestedElement(AgencyBillsOfLadingSchema, "AgencyBillsOfLading"); }
		}

		[ExpectXmlSchemaContainsRootElement("ContainerMovements")]
		public XmlSchema ContainerMovementSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "ContainerMovement.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("ContainerMovement")]
		public XmlSchema SingleContainerMovementSchema
		{
			get { return GetCompiledSchemaNestedElement(ContainerMovementSchema, "ContainerMovements"); }
		}

		[ExpectXmlSchemaContainsRootElement("Billing")]
		public XmlSchema BillingSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "Billing.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("ContainerLeg")]
		public XmlSchema SingleContainerLeg
		{
			get { return GetCompiledSchemaWithElementOfType(XmlSchemaDefinitions.Instance.ElementsSchema, "ContainerLeg", "ContainerLeg"); }
		}

		const string DefaultBaseResourceName = "Enterprise.DataTransfer.DataFileDefinitions.Xml.Version1";
	}
}
