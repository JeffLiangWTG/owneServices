using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	public interface ITariffAdditionalCode
	{
		///<summary>
		/// Xml Tag:code
		///</summary>
		ZString Code { get; }

		///<summary>
		/// Xml Tag:
		///</summary>
		ZString Description { get; }

		ZBool IsCompleted { get; }
	}
}
