using CargoWise.Types;
namespace Enterprise.Integration
{
	public static partial class Customs
	{
		public static partial class CA
		{
			public static partial class DIF
			{
				public interface IDIFDocument
				{
					ZString BusinessNumber { get; set; }
					ZString PGA { get; set; }
					ZString DocumentType { get; set; }
					ZString DocumentDescription { get; set; }
					ZString DocumentNumber { get; set; }
					ZString Comment { get; set; }
					ZGuid EDocsDocumentPK { get; set; }
					ZString URN { get; }
					ZDateTime EffectiveDate { get; set; }
					ZDateTime ExpiryDate { get; set; }
					ZString Status { get; set; }
					ZGuid StakeHolderOrg { get; }
				}
			}
		}
	}
}