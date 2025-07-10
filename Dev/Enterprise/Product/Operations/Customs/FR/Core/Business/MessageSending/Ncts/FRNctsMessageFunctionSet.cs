namespace Enterprise.Customs.FR.Business.MessageSending
{
	public abstract class FRNctsMessageFunctionSet : EU.NCTS.Business.MessageGeneration.NctsMessageFunctionSet
	{
		protected override string CodeCore { get; }

		public class PrelodgeValidationMessage : DeclarationDataMessage
		{
			public PrelodgeValidationMessage()
			{
				SentMessageStatusCode = FrNctsMessageStatusList.Codes.PrelodgeValidationSent;
			}

			protected override string CodeCore => "IEF15";
		}
	}
}
