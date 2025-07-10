using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.MX.Business
{
	public class DeclarationMessageSendingObject : JobDeclarationMessageSendingObject
	{
		public DeclarationMessageSendingObject(CusEntryHeader header) : base(header)
		{
		}

		#region MessageType

		[ResourceStringData("Enterprise.Customs.MX.Business.DeclarationMessageSendingObject|MessageType", Caption = "Message Type", ShortCaption = "Msg. Type")]
		[List(nameof(MessageTypesList))]
		public override ZString MessageType
		{
			get => base.MessageType;
			set => base.MessageType = value;
		}

		public CodeDescriptionPairList MessageTypesList => Factory.GetCachedValue<MessageTypesList>();

		protected override bool MessageType_ReadOnly => false;

		#endregion
	}
}
