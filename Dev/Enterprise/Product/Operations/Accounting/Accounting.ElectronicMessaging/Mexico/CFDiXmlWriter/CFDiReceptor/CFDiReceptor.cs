namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.sat.gob.mx/cfd/4")]
	public partial class ComprobanteReceptor
	{

		string rfcField;

		string nombreField;

		string domicilioFiscalReceptorField;

		c_Pais residenciaFiscalField;

		bool residenciaFiscalFieldSpecified;

		string numRegIdTribField;

		c_RegimenFiscal regimenFiscalReceptorField;

		c_UsoCFDI usoCFDIField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Rfc
		{
			get
			{
				return this.rfcField;
			}
			set
			{
				this.rfcField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string Nombre
		{
			get
			{
				return this.nombreField;
			}
			set
			{
				this.nombreField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string DomicilioFiscalReceptor
		{
			get
			{
				return this.domicilioFiscalReceptorField;
			}
			set
			{
				this.domicilioFiscalReceptorField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public c_Pais ResidenciaFiscal
		{
			get
			{
				return this.residenciaFiscalField;
			}
			set
			{
				this.residenciaFiscalField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool ResidenciaFiscalSpecified
		{
			get
			{
				return this.residenciaFiscalFieldSpecified;
			}
			set
			{
				this.residenciaFiscalFieldSpecified = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string NumRegIdTrib
		{
			get
			{
				return this.numRegIdTribField;
			}
			set
			{
				this.numRegIdTribField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public c_RegimenFiscal RegimenFiscalReceptor
		{
			get
			{
				return this.regimenFiscalReceptorField;
			}
			set
			{
				this.regimenFiscalReceptorField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public c_UsoCFDI UsoCFDI
		{
			get
			{
				return this.usoCFDIField;
			}
			set
			{
				this.usoCFDIField = value;
			}
		}
	}
}
