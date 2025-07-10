using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	///<summary>
	/// Xml Tag: Preference
	///</summary>
	public interface IPreference
	{
		///<summary>
		/// Xml Tag: preftar2
		///</summary>
		ZString CodePart1 { get; }
		///<summary>
		/// Xml Tag: preftar2
		///</summary>
		ZString CodePart2 { get; }
	}
}
