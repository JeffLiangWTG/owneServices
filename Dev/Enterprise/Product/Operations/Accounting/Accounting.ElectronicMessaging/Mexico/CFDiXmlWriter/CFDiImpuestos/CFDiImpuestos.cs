namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.sat.gob.mx/cfd/4")]
	public partial class ComprobanteImpuestos
	{

		ComprobanteImpuestosRetencion[] retencionesField;

		ComprobanteImpuestosTraslado[] trasladosField;

		decimal totalImpuestosRetenidosField;

		bool totalImpuestosRetenidosFieldSpecified;

		decimal totalImpuestosTrasladadosField;

		bool totalImpuestosTrasladadosFieldSpecified;

		/// <remarks/>
		[System.Xml.Serialization.XmlArrayItemAttribute("Retencion", IsNullable = false)]
		public ComprobanteImpuestosRetencion[] Retenciones
		{
			get
			{
				return this.retencionesField;
			}
			set
			{
				this.retencionesField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlArrayItemAttribute("Traslado", IsNullable = false)]
		public ComprobanteImpuestosTraslado[] Traslados
		{
			get
			{
				return this.trasladosField;
			}
			set
			{
				this.trasladosField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal TotalImpuestosRetenidos
		{
			get
			{
				return this.totalImpuestosRetenidosField;
			}
			set
			{
				this.totalImpuestosRetenidosField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool TotalImpuestosRetenidosSpecified
		{
			get
			{
				return this.totalImpuestosRetenidosFieldSpecified;
			}
			set
			{
				this.totalImpuestosRetenidosFieldSpecified = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal TotalImpuestosTrasladados
		{
			get
			{
				return this.totalImpuestosTrasladadosField;
			}
			set
			{
				this.totalImpuestosTrasladadosField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool TotalImpuestosTrasladadosSpecified
		{
			get
			{
				return this.totalImpuestosTrasladadosFieldSpecified;
			}
			set
			{
				this.totalImpuestosTrasladadosFieldSpecified = value;
			}
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.sat.gob.mx/cfd/4")]
	public partial class ComprobanteImpuestosRetencion
	{

		c_Impuesto impuestoField;

		decimal importeField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public c_Impuesto Impuesto
		{
			get
			{
				return this.impuestoField;
			}
			set
			{
				this.impuestoField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal Importe
		{
			get
			{
				return this.importeField;
			}
			set
			{
				this.importeField = value;
			}
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.sat.gob.mx/cfd/4")]
	public partial class ComprobanteImpuestosTraslado
	{
		decimal baseField;

		c_Impuesto impuestoField;

		c_TipoFactor tipoFactorField;

		decimal tasaOCuotaField;

		bool tasaOCuotaFieldSpecified;

		decimal importeField;

		bool importeFieldSpecified;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal Base
		{
			get
			{
				return this.baseField;
			}
			set
			{
				this.baseField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public c_Impuesto Impuesto
		{
			get
			{
				return this.impuestoField;
			}
			set
			{
				this.impuestoField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public c_TipoFactor TipoFactor
		{
			get
			{
				return this.tipoFactorField;
			}
			set
			{
				this.tipoFactorField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal TasaOCuota
		{
			get
			{
				return this.tasaOCuotaField;
			}
			set
			{
				this.tasaOCuotaField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool TasaOCuotaSpecified
		{
			get
			{
				return this.tasaOCuotaFieldSpecified;
			}
			set
			{
				this.tasaOCuotaFieldSpecified = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal Importe
		{
			get
			{
				return this.importeField;
			}
			set
			{
				this.importeField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool ImporteSpecified
		{
			get
			{
				return this.importeFieldSpecified;
			}
			set
			{
				this.importeFieldSpecified = value;
			}
		}
	}
}
