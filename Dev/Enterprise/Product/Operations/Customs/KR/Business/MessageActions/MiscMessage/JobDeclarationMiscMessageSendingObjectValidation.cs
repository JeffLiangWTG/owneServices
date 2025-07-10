using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationMiscMessageSendingObjectValidation : JobDeclarationMiscMessageSendingObjectCoreValidation
	{
		public JobDeclarationMiscMessageSendingObjectValidation(JobDeclarationMiscMessageSendingObject parent) : base(parent)
		{
		}

		protected new JobDeclarationMiscMessageSendingObject Parent => (JobDeclarationMiscMessageSendingObject)base.Parent;

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateNewDate();
		}
		public void ValidateNewDate()
		{
			ValidateCalculatedProperty(Parent.NewDateInfo);
		}
		protected virtual void CheckNewDate()
		{
			if (Parent.ShouldSend)
			{
				if (Parent.AmendmentType == _5ASAmendmentType.Codes.Extension)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.NewDateInfo);

					if (!Parent.NewDate.IsEmpty && !Parent.CurrentDate.IsEmpty)
					{
						CompareValidation.CheckDateIsAfterAnotherDate(Parent.NewDateInfo, Parent.CurrentDateInfo);
					}
				}
			}
		}

		protected override void CheckShouldSend()
		{
			base.CheckShouldSend();
			if (Parent.ShouldSend && Parent.MessageType == ElectronicDocumentTypeList.Codes._5TM)
			{
				if (!Parent.MessageSendingEntryLines.Find(x => x.IsGoldOrItsProduct).Any())
				{
					Parent.ShouldSendInfo.AddError(Res.GetString("AF7098F6-70B6-46B9-BBF8-86980B2A28F5", "There are no lines which are gold or its product. Please indicate so for at least one entry line."));
				}
			}
		}
	}
}
