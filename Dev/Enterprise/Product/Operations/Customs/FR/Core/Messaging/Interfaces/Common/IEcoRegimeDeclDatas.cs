using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	///<summary>
	/// Xml Tag: DecEco
	///</summary>
	public interface IEcoRegimeDeclDatas
	{
		///<summary>
		/// Xml Tag: refdec
		///</summary>
		ZString CusDeclarationID { get; }
		///<summary>
		/// Xml Tag: typedececo
		///</summary>
		ZString EcoRegimeDeclTypeCode { get; }
	}
}
