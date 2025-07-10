namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.sat.gob.mx/cfd/4")]
	public partial class ComprobanteEmisor
	{

		string rfcField;

		string nombreField;

		c_RegimenFiscal regimenFiscalField;

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
		public c_RegimenFiscal RegimenFiscal
		{
			get
			{
				return this.regimenFiscalField;
			}
			set
			{
				this.regimenFiscalField = value;
			}
		}
	}
}
