using Enterprise.ZArchitecture.Web.Shared.Test;

namespace Enterprise.DataTransfer.Native.Service
{
	class WithNativeDataTransferServiceAttribute : WithEnterpriseTestWebApplicationAttribute
	{
		public static WithNativeDataTransferServiceAttribute NativeDataTransferService => (WithNativeDataTransferServiceAttribute)Properties[ApplicationNameValue];

		protected override string ApplicationName => ApplicationNameValue;

		const string ApplicationNameValue = "NativeDataTransfer";
	}
}
