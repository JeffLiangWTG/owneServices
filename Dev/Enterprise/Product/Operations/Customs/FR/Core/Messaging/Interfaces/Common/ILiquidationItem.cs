using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	///<summary>
	/// Xml Tag: LiquidationArticle
	///</summary>
	public interface ILiquidationItem
	{
		///<summary>
		/// Xml Tag: numart
		///</summary>
		ZShort ArticleNumber { get; }

		///<summary>
		/// Xml Tag: TaxationDetail
		///</summary>
		ITaxationDetail TaxDetail { get; }
	}
}
