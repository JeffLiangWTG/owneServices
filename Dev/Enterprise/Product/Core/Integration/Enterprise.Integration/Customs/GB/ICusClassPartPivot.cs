using CargoWise.Types;

namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class GB
		{
			public interface ICusClassPartPivot
			{
				ZString CI_CPC { get; set; }

				ITaxOnlyForPivotCollection Taxes { get; }

				ISupportingDocumentCollection SupportingDocuments { get; }

				IPreviousDocumentCollection PreviousDocuments { get; }

				IAdditionalInfoCollection AdditionalInfos { get; }
			}
		}
	}
}
