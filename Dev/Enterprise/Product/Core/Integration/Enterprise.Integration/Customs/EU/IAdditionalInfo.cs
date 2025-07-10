using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class EU
		{
			public interface IAdditionalInfo : ICusSupportingInfo
			{
				ZBool CSI_NctsExportFromEC { get; set; }
			}
		}
	}
}
