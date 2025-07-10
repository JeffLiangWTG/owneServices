namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.sat.gob.mx/cfd/4")]
	public partial class ComprobanteCfdiRelacionados
	{

		ComprobanteCfdiRelacionadosCfdiRelacionado[] cfdiRelacionadoField;

		c_TipoRelacion tipoRelacionField;

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute("CfdiRelacionado")]
		public ComprobanteCfdiRelacionadosCfdiRelacionado[] CfdiRelacionado
		{
			get
			{
				return this.cfdiRelacionadoField;
			}
			set
			{
				this.cfdiRelacionadoField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public c_TipoRelacion TipoRelacion
		{
			get
			{
				return this.tipoRelacionField;
			}
			set
			{
				this.tipoRelacionField = value;
			}
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.sat.gob.mx/cfd/4")]
	public partial class ComprobanteCfdiRelacionadosCfdiRelacionado
	{

		string uUIDField;

		/// <remarks/>
		[System.Xml.Serialization.XmlAttributeAttribute()]
		public string UUID
		{
			get
			{
				return this.uUIDField;
			}
			set
			{
				this.uUIDField = value;
			}
		}
	}
}
