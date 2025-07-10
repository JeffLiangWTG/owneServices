using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	/// <summary>
	/// Xml Tag:Transport
	/// </summary>
	public interface ITransport
	{
		/// <summary>
		/// Xml Tag:modfrotra
		/// </summary>
		ZString ModeOfTRansport { get; }

		/// <summary>
		/// Xml Tag:inttra
		/// </summary>
		ZString ModeOfTRansportInland { get; }

		/// <summary>
		/// Xml Tag:conteneurtra
		/// </summary>
		ZString ContainerMode { get; }

		/// <summary>
		/// Xml Tag:natfrotra
		/// </summary>
		ZString NationalityOfTransport { get; }

		/// <summary>
		/// Xml Tag:burfro
		/// </summary>
		ZString CusOffice { get; }

		/// <summary>
		/// Xml Tag:aeroportemb
		/// </summary>
		ZString IATAAirportOfLoading { get; }

		/// <summary>
		/// Xml Tag:aertra
		/// </summary>
		ZString AirRoadType { get; }

		#region Export
		/// <summary>
		/// Xml Tag:idtransport
		/// </summary>
		ZString TransportID { get; }

		/// <summary>
		/// Xml Tag:transportMethodPayment
		/// </summary>
		ZString TransportMethodPayment { get; }
		#endregion
	}
}
