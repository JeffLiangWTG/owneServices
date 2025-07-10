using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	///<summary>
	/// Xml Tag: Motivation
	///</summary>
	public interface IMotivation
	{
		///<summary>
		/// Xml Tag: motiv
		///</summary>
		ZString Motivation { get; }

		///<summary>
		/// Xml Tag: justifreg
		///</summary>
		ZString RegularJustification { get; }

		///<summary>
		/// Xml Tag: commentaire
		///</summary>
		ZString Comment { get; }

		///<summary>
		/// Xml Tag: nouvelledest
		///</summary>
		ZString NewDestination { get; }
	}
}
