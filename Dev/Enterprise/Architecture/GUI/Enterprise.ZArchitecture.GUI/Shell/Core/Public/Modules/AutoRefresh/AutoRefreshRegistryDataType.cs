using System;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ZArchitecture.Modules.AutoRefresh
{
	public class AutoRefreshRegistryDataType : RegistryDataType<AutoRefreshTimeOut>
	{
		public AutoRefreshRegistryDataType()
			: base("", new AutoRefreshTimeOut())
		{
		}

		protected override AutoRefreshTimeOut DeserialiseCore(byte[] value)
		{
			var result = new AutoRefreshTimeOut();
			result.IsEnabled = Convert.ToBoolean(value[0]);
			result.RefreshTimeInMinutes = value[1];

			return result;
		}

		protected override byte[] SerialiseCore(AutoRefreshTimeOut value)
		{
			return new byte[] { Convert.ToByte(value.IsEnabled), value.RefreshTimeInMinutes };
		}

		protected override AutoRefreshTimeOut CloneValue(AutoRefreshTimeOut value)
		{
			return new AutoRefreshTimeOut(value.IsEnabled, value.RefreshTimeInMinutes);
		}

		protected override bool HasDefaultEditorInfoCore
		{
			get { return false; }
		}
	}
}
