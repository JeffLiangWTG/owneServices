using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class GB
		{
			public interface ISupportingDocument : EU.ISupportingDocument
			{
				ZString CSI_Actions { get; set; }
				ZString CSI_Availability { get; set; }
			}
		}
	}
}
