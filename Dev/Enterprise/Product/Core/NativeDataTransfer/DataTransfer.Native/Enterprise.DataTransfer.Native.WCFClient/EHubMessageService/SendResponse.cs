using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.ServiceModel;
using System.Xml.Schema;
using System.Xml.Serialization;
using Enterprise.DataTransfer.Native.Integration;

namespace Enterprise.DataTransfer.Native.WCFClient.EHubMessageService
{
	/// <remarks/>
	[GeneratedCodeAttribute("System.Xml", "2.0.50727.4927")]
	[SerializableAttribute()]
	[DebuggerStepThroughAttribute()]
	[DesignerCategoryAttribute("code")]
	[XmlTypeAttribute(AnonymousType = true, Namespace = "http://CargoWise.eHub.MessageRouting.Schema.MessageSendResponse")]
	public partial class MessageSendResponse : object, INotifyPropertyChanged, IResponseMessage
	{
		public static MessageSendResponse EmptyResponse
		{
			get
			{
				var response = new MessageSendResponse();
				return response;
			}
		}
		public static MessageSendResponse InvalidResponse(string errorMessage)
		{
			var response = new MessageSendResponse();
			response.HasError = true;
			response.ErrorMessage = errorMessage;
			return response;
		}

		string messageIDField;

		bool hasErrorField;

		string errorMessageField;

		string responseMessageField;

		/// <remarks/>
		[XmlElementAttribute(Form = XmlSchemaForm.Unqualified, Order = 0)]
		public string MessageID
		{
			get
			{
				return this.messageIDField;
			}
			set
			{
				this.messageIDField = value;
				this.RaisePropertyChanged("MessageID");
			}
		}

		/// <remarks/>
		[XmlElementAttribute(Form = XmlSchemaForm.Unqualified, Order = 1)]
		public bool HasError
		{
			get
			{
				return this.hasErrorField;
			}
			set
			{
				this.hasErrorField = value;
				this.RaisePropertyChanged("HasError");
			}
		}

		/// <remarks/>
		[XmlElementAttribute(Form = XmlSchemaForm.Unqualified, Order = 2)]
		public string ErrorMessage
		{
			get
			{
				return this.errorMessageField;
			}
			set
			{
				this.errorMessageField = value;
				this.RaisePropertyChanged("ErrorMessage");
			}
		}

		/// <remarks/>
		[System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Order = 3)]
		public string ResponseMessage
		{
			get
			{
				return this.responseMessageField;
			}
			set
			{
				this.responseMessageField = value;
				this.RaisePropertyChanged("ResponseMessage");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void RaisePropertyChanged(string propertyName)
		{
			PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
			if ((propertyChanged != null))
			{
				propertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}

	[DebuggerStepThroughAttribute()]
	[GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
	[MessageContractAttribute(IsWrapped = false)]
	public partial class SendResponse
	{
		[MessageBodyMemberAttribute(Namespace = "http://CargoWise.eHub.MessageRouting.Schema.MessageSendResponse", Order = 0)]
		public MessageSendResponse MessageSendResponse;

		public SendResponse()
		{
		}

		public SendResponse(MessageSendResponse MessageSendResponse)
		{
			this.MessageSendResponse = MessageSendResponse;
		}
	}
}
