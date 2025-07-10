using System;
using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public class AccComplianceDocumentHeaderTypeDecider : TypeDecider
	{
		public override Type GetTypeForLoad(DataRow row, BusinessObjectFactory factory)
		{
			string ledger = row[AccComplianceDocumentHeader.Schema.ADH_Ledger].ToString();

			switch (ledger)
			{
				case LedgerTypes.AccountsPayable:
					return typeof(APComplianceDocumentHeader);
				case LedgerTypes.AccountsReceivable:
					return typeof(ARComplianceDocumentHeader);
				default:
					throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Ledger '{0}' is invalid, ledger must be AR, AP.", ledger));
			}
		}

		public override Type GetTypeForNew()
		{
			throw new NoConcreteTypeException("New Transaction type cannot be determined");
		}

		public override Type GetTypeForBinding()
		{
			return null;
		}
	}
}
