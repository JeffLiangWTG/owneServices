using CargoWise.Types;
namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	///<summary>
	/// Xml Tag: References
	///</summary>
	public interface IReferences
	{
		///<summary>
		/// Xml Tag: refdos
		///</summary>
		ZString OwnerDeclarationIdentification { get; }

		///<summary>
		/// Xml Tag: refdec
		///</summary>
		ZString CusDeclarationNumber { get; }
	}
}
