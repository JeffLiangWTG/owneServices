using System;
using System.Xml.Schema;

namespace Enterprise.DataTransfer.Xml
{
	public class WebServicesXmlSchemaDefinitions : XmlSchemaDefinitionsBase
	{
		#region Instance

		protected WebServicesXmlSchemaDefinitions()
		{
		}

		public static WebServicesXmlSchemaDefinitions Instance
		{
			get
			{
				WebServicesXmlSchemaDefinitions result = (WebServicesXmlSchemaDefinitions)WeakInstance.Target;
				if (result == null)
				{
					result = new WebServicesXmlSchemaDefinitions();
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

		#region Web Consol

		[ExpectXmlSchemaContainsRootElement("WebConsols")]
		public XmlSchema WebConsolsSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "WebConsol.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("WebConsol")]
		public XmlSchema WebConsolSchema
		{
			get { return GetCompiledSchemaNestedElement(WebConsolsSchema, "WebConsols"); }
		}

		#endregion

		#region Web Container

		[ExpectXmlSchemaContainsRootElement("WebContainers")]
		public XmlSchema WebContainersSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "WebContainer.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("WebContainer")]
		public XmlSchema WebContainerSchema
		{
			get { return GetCompiledSchemaNestedElement(WebContainersSchema, "WebContainers"); }
		}

		#endregion

		#region Web Order

		[ExpectXmlSchemaContainsRootElement("WebOrders")]
		public XmlSchema WebOrdersSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "WebOrder.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("WebOrder")]
		public XmlSchema WebOrderSchema
		{
			get { return GetCompiledSchemaNestedElement(WebOrdersSchema, "WebOrders"); }
		}

		#endregion

		#region Web Order Summary

		[ExpectXmlSchemaContainsRootElement("WebOrderSummaries")]
		public XmlSchema WebOrderSummarysSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "WebOrderSummary.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("WebOrderSummary")]
		public XmlSchema WebOrderSummarySchema
		{
			get { return GetCompiledSchemaNestedElement(WebOrderSummarysSchema, "WebOrderSummaries"); }
		}

		#endregion

		#region Web Packing

		[ExpectXmlSchemaContainsRootElement("WebPackings")]
		public XmlSchema WebPackingsSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "WebPacking.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("WebPacking")]
		public XmlSchema WebPackingSchema
		{
			get { return GetCompiledSchemaNestedElement(WebPackingsSchema, "WebPackings"); }
		}

		#endregion

		#region Web Shipment

		[ExpectXmlSchemaContainsRootElement("WebShipments")]
		public XmlSchema WebShipmentsSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "WebShipment.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("WebShipment")]
		public XmlSchema WebShipmentSchema
		{
			get { return GetCompiledSchemaNestedElement(WebShipmentsSchema, "WebShipments"); }
		}

		#endregion

		#region WebShipment Filter

		[ExpectXmlSchemaContainsRootElement("WebShipmentFilter")]
		public XmlSchema WebShipmentFilterSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "WebShipmentFilter.xsd"); }
		}

		#endregion

		#region WebOrder Filter

		[ExpectXmlSchemaContainsRootElement("WebOrderFilter")]
		public XmlSchema WebOrderFilterSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "WebOrderFilter.xsd"); }
		}

		#endregion

		const string DefaultBaseResourceName = "Enterprise.DataTransfer.DataFileDefinitions.Xml.Version1";
	}
}
