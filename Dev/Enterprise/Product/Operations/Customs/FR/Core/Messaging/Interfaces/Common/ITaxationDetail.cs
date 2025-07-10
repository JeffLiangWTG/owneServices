namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	public interface ITaxationDetail
	{
		///<summary>
		/// Xml Tag: UniSpe
		///</summary>
		ISupplementaryUnit SuppUnit { get; }

		///<summary>
		/// Xml Tag: 
		///</summary>
		ITax Tax { get; }
	}
}
