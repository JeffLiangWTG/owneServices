using CargoWise.Types;
namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	/// <summary>
	/// Xml Tag:Bureau
	/// </summary>
	public interface ICusOffice
	{
		/// <summary>
		/// Xml Tag:burdom
		/// </summary>
		ZString OfficeOfDeclaration { get; }

		/// <summary>
		/// Xml Tag:burrat
		/// </summary>
		ZString OfficeOfLodgement { get; }

		/// <summary>
		/// Xml Tag:burunivis
		/// </summary>
		ZString VisitingOffice { get; }

		#region Export
		/// <summary>
		/// Xml Tag:bureausortie
		/// </summary>
		ZString ExitOffice { get; }
		/// <summary>
		/// Xml Tag:typesortie
		/// </summary>
		ZString ECSExitType { get; }
		/// <summary>
		/// Xml Tag:motiv
		/// </summary>
		ZString ECSMotivation { get; }
		#endregion
	}
}
