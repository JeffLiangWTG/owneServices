using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	///<summary>
	/// Xml Tag:MetaData
	///</summary>
	public interface IMetaData
	{
		///<summary>
		/// Xml Tag:Application
		///</summary>
		ZString Application { get; }

		///<summary>
		/// Xml Tag:DeclarationReference
		///</summary>
		ZString DeclarationReference { get; }

		///<summary>
		/// Xml Tag:EntryNumberType
		///</summary>
		ZString EntryNumberType { get; }
	}
}
