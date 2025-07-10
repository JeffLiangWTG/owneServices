using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationMessageSendingObjectValidationCore : Customs.Business.JobDeclarationMessageSendingObjectValidation
	{
		public JobDeclarationMessageSendingObjectValidationCore(JobDeclarationMessageSendingObject sendingObject) : base(sendingObject)
		{
		}

		ZString MessageType => Parent.MessageType;
		CusEntryHeader Header => Parent.Header;
		new JobDeclarationMessageSendingObject Parent => (JobDeclarationMessageSendingObject)base.Parent;

		protected override void CheckShouldSend()
		{
			base.CheckShouldSend();
			if (Parent.ShouldSend)
			{
				Parent.ShouldSendInfo.CheckShouldSendCore(Header, MessageType, additionalEntryNumFilter());

				if (MessageType == ElectronicDocumentTypeList.Codes._5SI && !Header.InvoiceHeaders().Any(x => x.Parcels.Count > 0))
				{
					Parent.ShouldSendInfo.AddError(Res.GetString("7E29EC3F-00F0-4178-8E51-5F2820B9E84C", "The invoice for this entry requires at least one piece of parcel information."));
				}

				if (Parent.MessageSendingValidation.MessageErrors.Count() > 0)
				{
					Parent.ShouldSendInfo.CheckShouldSendWithMessageErrors(Res.GetString("181ced21-9ee4-45e1-9533-c9f54864e32e", "entry"));
				}
			}
		}

		protected virtual Func<CusEntryNumber, bool> additionalEntryNumFilter() => null;
	}
}
