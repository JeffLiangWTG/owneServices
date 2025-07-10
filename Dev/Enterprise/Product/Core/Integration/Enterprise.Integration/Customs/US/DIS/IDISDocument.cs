using CargoWise.Types;
namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class US
		{
			public static partial class DIS
			{
				public interface IDISDocument
				{
					ZString DocumentID { get; }
					ZInt IDSuffix { get; set; }
					ZString DocumentLabel { get; set; }
					ZString DocumentDescription { get; set; }
					ZString Comment { get; set; }
					ZGuid EDocsDocumentPK { get; set; }
					ZDateTime SubmitDateUTC { get; set; }
					ZString ShipmentNo { get; set; }
					ZString ITN { get; set; }
					ZString XTN { get; set; }
				}
			}
		}
	}
}