using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	///<summary>
	/// Xml Tag:Document
	///</summary>
	public interface ISupportingDocument
	{
		///<summary>
		/// Xml Tag:typdocpec
		///</summary>
		ZString Code { get; }

		///<summary>
		/// Xml Tag:ComOfInfDc25
		///</summary>
		ZString Description { get; }

		///<summary>
		/// Xml Tag:natdocpec
		///</summary>
		ZString Type { get; }

		///<summary>
		/// Xml Tag:refdoc/refdocpec
		///</summary>
		ZString RefNumber { get; }

		///<summary>
		/// Xml Tag:datdoc
		///</summary>
		ZDateTime DateIssue { get; }

		///<summary>
		/// Xml Tag:refPFAid
		///</summary>
		ZString PFAIdentification { get; }

		///<summary>
		/// Xml Tag:refPFAdoc
		///</summary>
		ZString PFADocument { get; }
	}
}
