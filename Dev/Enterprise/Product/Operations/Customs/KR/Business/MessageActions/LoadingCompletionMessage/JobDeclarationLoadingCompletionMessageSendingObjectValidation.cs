using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class JobDeclarationLoadingCompletionMessageSendingObjectValidation : JobDeclarationMessageSendingObjectValidation
	{
		public JobDeclarationLoadingCompletionMessageSendingObjectValidation(JobDeclarationLoadingCompletionMessageSendingObject parent) : base(parent)
		{
		}
		protected new JobDeclarationLoadingCompletionMessageSendingObject Parent => base.Parent as JobDeclarationLoadingCompletionMessageSendingObject;

		public void ValidateLoadingDate()
		{
			ValidateCalculatedProperty(Parent.LoadingDateInfo);
		}

		protected void CheckLoadingDate()
		{
			if (Parent.ShouldSend)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.LoadingDateInfo);
			}
		}

		protected override void CheckShouldSend()
		{
			if (Parent.Header.Declaration.JE_MessageSubType == LocalExportTransactionNatureCodeList.Codes._08)
			{
				Parent.ShouldSendInfo.AddError(Res.GetString("980A0242-661E-42D4-848A-90648DA73A8C", "If the 'Declaration Type is '08', 'DF3 - Local Export Completion Declaration' is not relevant and you should not send this message."));
			}
			base.CheckShouldSend();
		}
	}
}
