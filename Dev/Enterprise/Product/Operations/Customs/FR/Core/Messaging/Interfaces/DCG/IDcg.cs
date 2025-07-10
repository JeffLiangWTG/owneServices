using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Messaging.Interfaces.DCG
{
	public interface IDCG : IDeclarationBase
	{
		///<summary>
		/// Xml Tag: Entete
		///</summary>
		IMessageEnvelope MessageEnvelope { get; }

		/// <summary>
		/// Xml Tag: numagr
		/// </summary>
		ZString DeltaAgreementNumber { get; }

		/// <summary>
		/// Xml Tag: numcre
		/// </summary>
		ZString DefermentAccountNumber { get; }

		/// <summary>
		/// Xml Tag: operep
		/// </summary>
		ZString OperationalRepresentative { get; }

		/// <summary>
		/// Xml Tag: paiement
		/// </summary>
		ZString PaymentType { get; }

		/// <summary>
		/// Xml Tag: debutregul
		/// </summary>
		ZDate PeriodStartDate { get; }

		/// <summary>
		/// Xml Tag: perioderegul
		/// </summary>
		ZString Frequency { get; }

		/// <summary>
		/// Xml Tag: typflux
		/// </summary>
		ZString Direction { get; }
	}
}
