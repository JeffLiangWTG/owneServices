#pragma warning disable 1591

namespace Enterprise.Customs.GB.CNS.WebServices.CnsPrints
{
	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCode("System.Web.Services", "2.0.50727.3053")]
	[System.Diagnostics.DebuggerStepThrough()]
	[System.ComponentModel.DesignerCategory("code")]
	[System.Web.Services.WebServiceBinding(Name = "MailBoxPort", Namespace = "http://www.cnsonline.net/MailBox")]
	public partial class MailBox : System.Web.Services.Protocols.SoapHttpClientProtocol
	{
		/// <remarks/>
		[System.Web.Services.Protocols.SoapDocumentMethod("", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
		[return: System.Xml.Serialization.XmlElement("getAvailableEdifactPrintsResponse", Namespace = "http://www.cnsonline.net/MailBox/schema")]
		public GetAvailableEdifactPrintsResponse GetAvailableEdifactPrints([System.Xml.Serialization.XmlElement("getAvailableEdifactPrints", Namespace="http://www.cnsonline.net/MailBox/schema")]
				GetAvailableEdifactPrints getAvailableEdifactPrints1)
		{
			object[] results = this.Invoke("GetAvailableEdifactPrints", new object[] {
						getAvailableEdifactPrints1 });
			return ((GetAvailableEdifactPrintsResponse)(results[0]));
		}

		/// <remarks/>
		[System.Web.Services.Protocols.SoapDocumentMethod("", Use = System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle = System.Web.Services.Protocols.SoapParameterStyle.Bare)]
		[return: System.Xml.Serialization.XmlElement("acknowledgeEdifactPrintsResponse", Namespace = "http://www.cnsonline.net/MailBox/schema")]
		public AcknowledgeEdifactPrintsResponse AcknowledgeEdifactPrints([System.Xml.Serialization.XmlElement("acknowledgeEdifactPrints", Namespace="http://www.cnsonline.net/MailBox/schema")]
			AcknowledgeEdifactPrints acknowledgeEdifactPrints1)
		{
			object[] results = this.Invoke("AcknowledgeEdifactPrints", new object[] { acknowledgeEdifactPrints1 });
			return ((AcknowledgeEdifactPrintsResponse)(results[0]));
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.cnsonline.net/MailBox/schema")]
	public partial class GetAvailableEdifactPrints
	{
		string deviceField;

		public string device
		{
			get
			{
				return this.deviceField;
			}
			set
			{
				this.deviceField = value;
			}
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCode("System.Xml", "2.0.50727.3053")]
	[System.Serializable()]
	[System.Diagnostics.DebuggerStepThrough()]
	[System.ComponentModel.DesignerCategory("code")]
	[System.Xml.Serialization.XmlType(Namespace = "http://www.cnsonline.net/MailBox/schema")]
	public partial class AcknowledgeEdifactPrintsResponse
	{
		string messageCodeField;

		string messageTextField;

		/// <remarks/>
		public string messageCode
		{
			get
			{
				return this.messageCodeField;
			}
			set
			{
				this.messageCodeField = value;
			}
		}

		/// <remarks/>
		public string messageText
		{
			get
			{
				return this.messageTextField;
			}
			set
			{
				this.messageTextField = value;
			}
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "2.0.50727.3053")]
	[System.SerializableAttribute()]
	[System.Diagnostics.DebuggerStepThroughAttribute()]
	[System.ComponentModel.DesignerCategoryAttribute("code")]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.cnsonline.net/MailBox/schema")]
	public partial class AcknowledgeEdifactPrints
	{
		string deviceField;
		decimal batchIdField;

		/// <remarks/>
		public string device
		{
			get
			{
				return this.deviceField;
			}
			set
			{
				this.deviceField = value;
			}
		}

		/// <remarks/>
		public decimal batchId
		{
			get
			{
				return this.batchIdField;
			}
			set
			{
				this.batchIdField = value;
			}
		}
	}

	/// <remarks/>
	[System.CodeDom.Compiler.GeneratedCode("System.Xml", "2.0.50727.3053")]
	[System.Serializable()]
	[System.Diagnostics.DebuggerStepThrough()]
	[System.ComponentModel.DesignerCategory("code")]
	[System.Xml.Serialization.XmlType(Namespace = "http://www.cnsonline.net/MailBox/schema")]
	public partial class GetAvailableEdifactPrintsResponse
	{
		decimal batchIdField;
		decimal messageCountField;
		string[] messagesField;

		/// <remarks/>
		public decimal batchId
		{
			get
			{
				return this.batchIdField;
			}
			set
			{
				this.batchIdField = value;
			}
		}

		/// <remarks/>
		public decimal messageCount
		{
			get
			{
				return this.messageCountField;
			}
			set
			{
				this.messageCountField = value;
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlArrayItem("printMessage", IsNullable = false)]
		public string[] messages
		{
			get
			{
				return this.messagesField;
			}
			set
			{
				this.messagesField = value;
			}
		}
	}
}

#pragma warning restore 1591
