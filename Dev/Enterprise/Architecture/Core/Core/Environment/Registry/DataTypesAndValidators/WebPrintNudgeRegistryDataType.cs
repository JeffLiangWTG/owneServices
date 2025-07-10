using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Environment
{
	public class WebPrintNudgeRegistryDataType : RegistryDataTypeWithJsonSerializer<WebPrintNudge>
	{
		public WebPrintNudgeRegistryDataType(WebPrintNudge webPrintNudge)
			: base(RegistryDataTypes.Codes.Binary, webPrintNudge)
		{
		}

		protected override IRegistryEditorInfo GetNewDefaultEditorInfo()
		{
			return new WebPrintNudgeEditorInfo();
		}

		protected override WebPrintNudge CloneValue(WebPrintNudge value)
		{
			WebPrintNudge cloneValue = null;
			if (value != null)
			{
				cloneValue = new WebPrintNudge
				{
					ChangingToUrlAddressDateTimeUtc = value.ChangingToUrlAddressDateTimeUtc,
					SwtichBackToIPAddressIntervalInHours = value.SwtichBackToIPAddressIntervalInHours,
					EnableIPAddress = value.EnableIPAddress
				};
			}
			return cloneValue;
		}
	}
}
