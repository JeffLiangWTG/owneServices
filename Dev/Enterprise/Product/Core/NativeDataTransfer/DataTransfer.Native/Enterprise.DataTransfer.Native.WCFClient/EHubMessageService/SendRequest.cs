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
	[XmlTypeAttribute(AnonymousType = true, Namespace = "http://CargoWise.eHub.MessageRouting.Schema.MessageSendRequest")]
	public partial class MessageSendRequest : object, INotifyPropertyChanged, IRequestMessage
	{
		string messageIDField;

		string senderTypeField;

		string senderIDField;

		string senderUsernameField;

		string recipientTypeField;

		string recipientIDField;

		string messageActionField;

		string messageEntityField;

		string messageField;

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
		public string SenderType
		{
			get
			{
				return this.senderTypeField;
			}
			set
			{
				this.senderTypeField = value;
				this.RaisePropertyChanged("SenderType");
			}
		}

		/// <remarks/>
		[XmlElementAttribute(Form = XmlSchemaForm.Unqualified, Order = 2)]
		public string SenderID
		{
			get
			{
				return this.senderIDField;
			}
			set
			{
				this.senderIDField = value;
				this.RaisePropertyChanged("SenderID");
			}
		}

		/// <remarks/>
		[XmlElementAttribute(Form = XmlSchemaForm.Unqualified, Order = 3)]
		public string SenderUsername
		{
			get
			{
				return this.senderUsernameField;
			}
			set
			{
				this.senderUsernameField = value;
				this.RaisePropertyChanged("SenderUsername");
			}
		}

		/// <remarks/>
		[XmlElementAttribute(Form = XmlSchemaForm.Unqualified, Order = 4)]
		public string RecipientType
		{
			get
			{
				return this.recipientTypeField;
			}
			set
			{
				this.recipientTypeField = value;
				this.RaisePropertyChanged("RecipientType");
			}
		}

		/// <remarks/>
		[XmlElementAttribute(Form = XmlSchemaForm.Unqualified, Order = 5)]
		public string RecipientID
		{
			get
			{
				return this.recipientIDField;
			}
			set
			{
				this.recipientIDField = value;
				this.RaisePropertyChanged("RecipientID");
			}
		}

		/// <remarks/>
		[XmlElementAttribute(Form = XmlSchemaForm.Unqualified, Order = 6)]
		public string MessageAction
		{
			get
			{
				return this.messageActionField;
			}
			set
			{
				this.messageActionField = value;
				this.RaisePropertyChanged("MessageAction");
			}
		}

		/// <remarks/>
		[XmlElementAttribute(Form = XmlSchemaForm.Unqualified, Order = 7)]
		public string MessageEntity
		{
			get
			{
				return this.messageEntityField;
			}
			set
			{
				this.messageEntityField = value;
				this.RaisePropertyChanged("MessageEntity");
			}
		}

		/// <remarks/>
		[XmlElementAttribute(Form = XmlSchemaForm.Unqualified, Order = 8)]
		public string Message
		{
			get
			{
				return this.messageField;
			}
			set
			{
				this.messageField = value;
				this.RaisePropertyChanged("Message");
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

		public virtual bool IsValid()
		{
			return true;
		}
	}

	[DebuggerStepThroughAttribute()]
	[GeneratedCodeAttribute("System.ServiceModel", "3.0.0.0")]
	[MessageContractAttribute(IsWrapped = false)]
	public partial class SendRequest
	{
		[MessageBodyMemberAttribute(Namespace = "http://CargoWise.eHub.MessageRouting.Schema.MessageSendRequest", Order = 0)]
		public MessageSendRequest MessageSendRequest;

		public SendRequest()
		{
		}

		public SendRequest(MessageSendRequest MessageSendRequest)
		{
			this.MessageSendRequest = MessageSendRequest;
		}
	}
}
