using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	///<summary>
	/// Xml Tag: RegimeEco
	///</summary>
	public interface IEcoRegimeDatas
	{
		///<summary>
		/// Xml Tag: DecEcos
		///</summary>
		IEnumerable<IEcoRegimeDeclDatas> DecEcos { get; }

		///<summary>
		/// Xml Tag: montantgar
		///</summary>
		ZDecimal GuaranteeAmount { get; }

		///<summary>
		/// Xml Tag: delapur
		///</summary>
		sbyte? NumberDaysOfDischarge { get; }
	}
}
