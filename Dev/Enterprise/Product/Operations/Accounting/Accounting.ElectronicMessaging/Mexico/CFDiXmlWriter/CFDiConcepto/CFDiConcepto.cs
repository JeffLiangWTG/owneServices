
namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.sat.gob.mx/cfd/4")]
	public partial class ComprobanteConcepto
	{

		ComprobanteConceptoImpuestos impuestosField;

		ComprobanteConceptoInformacionAduanera[] informacionAduaneraField;

		ComprobanteConceptoCuentaPredial[] cuentaPredialField;

		ComprobanteConceptoComplementoConcepto complementoConceptoField;

		ComprobanteConceptoParte[] parteField;

		string claveProdServField;

		string noIdentificacionField;

		decimal cantidadField;

		string claveUnidadField;

		string unidadField;

		string descripcionField;

		decimal valorUnitarioField;

		decimal importeField;

		decimal descuentoField;

		bool descuentoFieldSpecified;

		c_ObjetoImp objetoImpField;

		/// <remarks/>
		public ComprobanteConceptoImpuestos Impuestos
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
		[System.Xml.Serialization.XmlElementAttribute("InformacionAduanera")]
		public ComprobanteConceptoInformacionAduanera[] InformacionAduanera
		{
			get
			{
				return this.informacionAduaneraField;
			}
			set
			{
				this.informacionAduaneraField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("CuentaPredial")]
		public ComprobanteConceptoCuentaPredial[] CuentaPredial
		{
			get
			{
				return this.cuentaPredialField;
			}
			set
			{
				this.cuentaPredialField = value;
			}
		}

		/// <remarks/>
		public ComprobanteConceptoComplementoConcepto ComplementoConcepto
		{
			get
			{
				return this.complementoConceptoField;
			}
			set
			{
				this.complementoConceptoField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("Parte")]
		public ComprobanteConceptoParte[] Parte
		{
			get
			{
				return this.parteField;
			}
			set
			{
				this.parteField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string ClaveProdServ
		{
			get
			{
				return this.claveProdServField;
			}
			set
			{
				this.claveProdServField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string NoIdentificacion
		{
			get
			{
				return this.noIdentificacionField;
			}
			set
			{
				this.noIdentificacionField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal Cantidad
		{
			get
			{
				return this.cantidadField;
			}
			set
			{
				this.cantidadField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string ClaveUnidad
		{
			get
			{
				return this.claveUnidadField;
			}
			set
			{
				this.claveUnidadField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Unidad
		{
			get
			{
				return this.unidadField;
			}
			set
			{
				this.unidadField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Descripcion
		{
			get
			{
				return this.descripcionField;
			}
			set
			{
				this.descripcionField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal ValorUnitario
		{
			get
			{
				return this.valorUnitarioField;
			}
			set
			{
				this.valorUnitarioField = value;
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
		public c_ObjetoImp ObjetoImp
		{
			get
			{
				return this.objetoImpField;
			}
			set
			{
				this.objetoImpField = value;
			}
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.sat.gob.mx/cfd/4")]
	public partial class ComprobanteConceptoComplementoConcepto
	{

		System.Xml.XmlElement[] anyField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAnyElementAttribute()]
		public System.Xml.XmlElement[] Any
		{
			get
			{
				return this.anyField;
			}
			set
			{
				this.anyField = value;
			}
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.sat.gob.mx/cfd/4")]
	public partial class ComprobanteConceptoParte
	{

		ComprobanteConceptoParteInformacionAduanera[] informacionAduaneraField;

		string claveProdServField;

		string noIdentificacionField;

		decimal cantidadField;

		string unidadField;

		string descripcionField;

		decimal valorUnitarioField;

		bool valorUnitarioFieldSpecified;

		decimal importeField;

		bool importeFieldSpecified;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("InformacionAduanera")]
		public ComprobanteConceptoParteInformacionAduanera[] InformacionAduanera
		{
			get
			{
				return this.informacionAduaneraField;
			}
			set
			{
				this.informacionAduaneraField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string ClaveProdServ
		{
			get
			{
				return this.claveProdServField;
			}
			set
			{
				this.claveProdServField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string NoIdentificacion
		{
			get
			{
				return this.noIdentificacionField;
			}
			set
			{
				this.noIdentificacionField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal Cantidad
		{
			get
			{
				return this.cantidadField;
			}
			set
			{
				this.cantidadField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Unidad
		{
			get
			{
				return this.unidadField;
			}
			set
			{
				this.unidadField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Descripcion
		{
			get
			{
				return this.descripcionField;
			}
			set
			{
				this.descripcionField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public decimal ValorUnitario
		{
			get
			{
				return this.valorUnitarioField;
			}
			set
			{
				this.valorUnitarioField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool ValorUnitarioSpecified
		{
			get
			{
				return this.valorUnitarioFieldSpecified;
			}
			set
			{
				this.valorUnitarioFieldSpecified = value;
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
