using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Messaging.Interfaces
{
	public interface IDeclarationImportExport
	{
		#region MetaData
		///<summary>
		/// Xml Tag: MetaData
		///</summary>
		IMetaData MetaData { get; }

		#endregion

		#region Declaration

		///<summary>
		/// Xml Tag: Entete
		///</summary>
		IMessageEnvelope MessageEnvelope { get; }

		///<summary>
		/// Xml Tag: Entete
		///</summary>
		IHeader Header { get; }

		///<summary>
		/// Xml Tag: Gen
		///</summary>
		ICusProcedure CusProcedure { get; }

		///<summary>
		/// Xml Tag: Article
		///</summary>
		IEnumerable<IArticle> Articles { get; }

		#endregion

		#region Liquidation

		///<summary>
		/// Xml Tag: Entete
		///</summary>
		IHeader LiquidationHeader { get; }
		///<summary>
		/// Xml Tag: LiquidationArticles
		///</summary>
		IEnumerable<ILiquidationItem> Liquidation { get; }

		#endregion

		ZDateTime MessageDate { get; }
		ZString MessageType { get; }
		ZBool HasIntoWarehouseProcedure { get; }
		ZBool IsOfficeOfLodgementDifferentFromOfficeOfExit { get; }
	}
}
