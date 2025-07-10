using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Messaging.Interfaces.Common
{
	public interface ISupportingDocumentOnly : ISupportingDocument
	{
		///<summary>
		/// Xml Tag:indd48
		///</summary>
		ZBool IsD48AndNotClosed { get; }

		///<summary>
		/// Xml Tag:mntd48
		///</summary>
		ZDecimal D48Amount { get; }

		///<summary>
		/// Xml Tag:deld48
		///</summary>
		sbyte D48Deadline { get; }

		ZBool IsUnderInvoiceLine { get; }

		///<summary>
		/// Xml Tag:FichesImputations
		///</summary>
		IEnumerable<IImputationSheet> ImputationsSheets { get; }
	}
}
