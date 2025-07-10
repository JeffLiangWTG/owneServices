using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public class WebPrintNudgeSuspendingRegistryDataType : RegistryDataTypeWithJsonSerializer<WebPrintNudgeSuspending>
	{
		public WebPrintNudgeSuspendingRegistryDataType(WebPrintNudgeSuspending webPrintNudgeSupending)
			: base(RegistryDataTypes.Codes.Binary, webPrintNudgeSupending)
		{
		}

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return new WebPrintNudgeSuspendingEditorInfo();
		}

		protected override WebPrintNudgeSuspending CloneValue(WebPrintNudgeSuspending value)
		{
			WebPrintNudgeSuspending cloneValue = null;
			if (value != null)
			{
				cloneValue = new WebPrintNudgeSuspending
				{
					MaxErrorsInMinutes = value.MaxErrorsInMinutes,
					IntervalMinutes = value.IntervalMinutes,
					SuspendMinutes = value.SuspendMinutes,
					MaxErrorsInHours = value.MaxErrorsInHours,
					IntervalHours = value.IntervalHours,
					SuspendHours = value.SuspendHours
				};
			}
			return cloneValue;
		}
	}
}
