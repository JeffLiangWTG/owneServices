using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	///<summary>
	/// Xml Tag: FichesImputation
	///</summary>
	public interface IImputationSheet
	{
		///<summary>
		/// Xml Tag: numLigne
		///</summary>
		ZString LineNumber { get; }
		///<summary>
		/// Xml Tag: refProduit
		///</summary>
		ZString ProductReference { get; }
		///<summary>
		/// Xml Tag: nomProduit
		///</summary>
		ZString ProductName { get; }
		///<summary>
		/// Xml Tag: unitImput
		///</summary>
		ZString ImputationUnit { get; }
		///<summary>
		/// Xml Tag: nbrImput
		///</summary>
		ZDecimal ImputationQuantity { get; }
		ZBool ShouldWriteImputationQuantity { get; }
		///<summary>
		/// Xml Tag: poidsProv
		///</summary>
		IWeight ProvisionalWeight { get; }
		ZBool ShouldWriteProvisionalWeight { get; }
		///<summary>
		/// Xml Tag: devImput
		///</summary>
		ZString ImputationCurrency { get; }
		///<summary>
		/// Xml Tag: montImput
		///</summary>
		ZDecimal ImputationAmount { get; }
		ZBool ShouldWriteImputationAmount { get; }
		///<summary>
		/// Xml Tag: msn
		///</summary>
		ZDecimal NetWeight { get; }
		ZBool ShouldWriteNetWeight { get; }
	}
}
