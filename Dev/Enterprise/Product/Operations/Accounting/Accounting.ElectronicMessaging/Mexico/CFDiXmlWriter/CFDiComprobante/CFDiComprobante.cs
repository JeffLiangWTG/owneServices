using System.Xml.Serialization;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.sat.gob.mx/cfd/4")]
	[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://www.sat.gob.mx/cfd/4", IsNullable = false)]
	[XmlSerializerAssembly("Enterprise.Accounting.ElectronicMessaging.XmlSerializers")]
	public partial class Comprobante
	{
		[XmlAttribute(Namespace = System.Xml.Schema.XmlSchema.InstanceNamespace)]
		public string schemaLocation = "http://www.sat.gob.mx/cfd/4 http://www.sat.gob.mx/sitio_internet/cfd/4/cfdv40.xsd"; // SuppressCodeSmell Reason = Constant String

		ComprobanteCfdiRelacionados[] cfdiRelacionadosField;

		ComprobanteEmisor emisorField;

		ComprobanteReceptor receptorField;

		ComprobanteConcepto[] conceptosField;

		ComprobanteImpuestos impuestosField;

		ComprobanteComplemento complementoField;

		ComprobanteAddenda addendaField;

		string versionField;

		string serieField;

		string folioField;

		System.DateTime fechaField;

		string selloField;

		c_FormaPago formaPagoField;

		bool formaPagoFieldSpecified;

		string noCertificadoField;

		string certificadoField;

		string condicionesDePagoField;

		decimal subTotalField;

		decimal descuentoField;

		bool descuentoFieldSpecified;

		c_Moneda monedaField;

		decimal tipoCambioField;

		bool tipoCambioFieldSpecified;

		decimal totalField;

		c_TipoDeComprobante tipoDeComprobanteField;

		c_Exportacion exportacionField;

		c_MetodoPago metodoPagoField;

		bool metodoPagoFieldSpecified;

		string lugarExpedicionField;

		string confirmacionField;

		public Comprobante()
		{
			this.versionField = "4.0";
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("CfdiRelacionados")]
		public ComprobanteCfdiRelacionados[] CfdiRelacionados
		{
			get
			{
				return this.cfdiRelacionadosField;
			}
			set
			{
				this.cfdiRelacionadosField = value;
			}
		}

		/// <remarks/>
		public ComprobanteEmisor Emisor
		{
			get
			{
				return this.emisorField;
			}
			set
			{
				this.emisorField = value;
			}
		}

		/// <remarks/>
		public ComprobanteReceptor Receptor
		{
			get
			{
				return this.receptorField;
			}
			set
			{
				this.receptorField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlArrayItemAttribute("Concepto", IsNullable = false)]
		public ComprobanteConcepto[] Conceptos
		{
			get
			{
				return this.conceptosField;
			}
			set
			{
				this.conceptosField = value;
			}
		}

		/// <remarks/>
		public ComprobanteImpuestos Impuestos
		{
			get
			{
				return this.impuestosField;
			}
			set
			{
				this.impuestosField = value;
			}
		}

		/// <remarks/>
		public ComprobanteComplemento Complemento
		{
			get
			{
				return this.complementoField;
			}
			set
			{
				this.complementoField = value;
			}
		}

		/// <remarks/>
		public ComprobanteAddenda Addenda
		{
			get
			{
				return this.addendaField;
			}
			set
			{
				this.addendaField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Version
		{
			get
			{
				return this.versionField;
			}
			set
			{
				this.versionField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Serie
		{
			get
			{
				return this.serieField;
			}
			set
			{
				this.serieField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Folio
		{
			get
			{
				return this.folioField;
			}
			set
			{
				this.folioField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public System.DateTime Fecha
		{
			get
			{
				return this.fechaField;
			}
			set
			{
				this.fechaField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Sello
		{
			get
			{
				return this.selloField;
			}
			set
			{
				this.selloField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public c_FormaPago FormaPago
		{
			get
			{
				return this.formaPagoField;
			}
			set
			{
				this.formaPagoField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool FormaPagoSpecified
		{
			get
			{
				return this.formaPagoFieldSpecified;
			}
			set
			{
				this.formaPagoFieldSpecified = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string NoCertificado
		{
			get
			{
				return this.noCertificadoField;
			}
			set
			{
				this.noCertificadoField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Certificado
		{
			get
			{
				return this.certificadoField;
			}
			set
			{
				this.certificadoField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string CondicionesDePago
		{
			get
			{
				return this.condicionesDePagoField;
			}
			set
			{
				this.condicionesDePagoField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal SubTotal
		{
			get
			{
				return this.subTotalField;
			}
			set
			{
				this.subTotalField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal Descuento
		{
			get
			{
				return this.descuentoField;
			}
			set
			{
				this.descuentoField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool DescuentoSpecified
		{
			get
			{
				return this.descuentoFieldSpecified;
			}
			set
			{
				this.descuentoFieldSpecified = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public c_Moneda Moneda
		{
			get
			{
				return this.monedaField;
			}
			set
			{
				this.monedaField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal TipoCambio
		{
			get
			{
				return this.tipoCambioField;
			}
			set
			{
				this.tipoCambioField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool TipoCambioSpecified
		{
			get
			{
				return this.tipoCambioFieldSpecified;
			}
			set
			{
				this.tipoCambioFieldSpecified = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal Total
		{
			get
			{
				return this.totalField;
			}
			set
			{
				this.totalField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public c_TipoDeComprobante TipoDeComprobante
		{
			get
			{
				return this.tipoDeComprobanteField;
			}
			set
			{
				this.tipoDeComprobanteField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public c_Exportacion Exportacion
		{
			get
			{
				return this.exportacionField;
			}
			set
			{
				this.exportacionField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public c_MetodoPago MetodoPago
		{
			get
			{
				return this.metodoPagoField;
			}
			set
			{
				this.metodoPagoField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool MetodoPagoSpecified
		{
			get
			{
				return this.metodoPagoFieldSpecified;
			}
			set
			{
				this.metodoPagoFieldSpecified = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string LugarExpedicion
		{
			get
			{
				return this.lugarExpedicionField;
			}
			set
			{
				this.lugarExpedicionField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Confirmacion
		{
			get
			{
				return this.confirmacionField;
			}
			set
			{
				this.confirmacionField = value;
			}
		}
	}
}
