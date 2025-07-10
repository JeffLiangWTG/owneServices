using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class ChargeWithCostForTest : ChargeWithCost
	{
		public ChargeWithCostForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override JobChargeValidation GetNewValidation()
		{
			return new ChargeWithCostValidationForTest(this);
		}

		public bool JR_APInvoiceDateInitialReadOnlyExposed
		{
			set { JR_APInvoiceDateInitialReadOnly = value; }
		}

		public bool JR_APDocumentReceivedDateInitialReadOnlyExposed
		{
			set { JR_APDocumentReceivedDateInitialReadOnly = value; }
		}

		protected internal override ZDecimal CalculateCFXAmt()
		{
			throw new NotImplementedException();
		}
	}
}
