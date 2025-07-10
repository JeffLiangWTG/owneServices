using System;
using System.Text;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Web.Business
{
	[Serializable]
	public class AccountingTransactionExportRequest : AccountingTransactionCreateBatchRequest
	{
		public AccountingTransactionExportRequest()
		{
			Namespace = UniversalXmlInfo.Namespace_2011_11;
		}

		public int BatchNumber { get; set; }

		public string Namespace { get; set; }

		public override string Validate()
		{
			string baseErrors = base.Validate();
			StringBuilder errors = new StringBuilder(baseErrors != null ? baseErrors + "\r\n" : string.Empty);

			if (BatchNumber == 0)
			{
				errors.AppendLine((NoResString)"BatchNumber cannot be zero.");
			}

			if (!Namespace.IsValidUniversalXmlNamespace())
			{
				errors.AppendLine(string.Format((NoResString)"Invalid namespace [{0}] - Please use a valid Universal Namespace.", Namespace ?? (NoResString)"(null)"));
			}

			return errors.Length > 0 ? errors.ToString().Trim() : null;
		}
	}
}
