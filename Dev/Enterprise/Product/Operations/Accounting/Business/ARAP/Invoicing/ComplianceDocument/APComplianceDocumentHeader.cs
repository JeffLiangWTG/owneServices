using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing.ComplianceDocument;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business
{
	public partial class APComplianceDocumentHeader : AccComplianceDocumentHeader
	{
		public APComplianceDocumentHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{ }

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ADH_Ledger = LedgerTypes.AccountsPayable;
		}

		protected override bool ADH_ComplianceSubType_ReadOnly => !IsAdded;

		protected override bool ADH_DocumentDate_ReadOnly => !IsAdded;

		protected override bool ADH_ReportingPeriod_ReadOnly => !IsAdded;

		protected override bool ADH_DocumentNumber_ReadOnly => !IsAdded;

		protected override bool ADH_XD_ComplianceBook_ReadOnly => true;

		protected override AccComplianceDocumentHeaderValidation GetNewValidation()
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan)
			{
				return new APComplianceDocumentHeaderValidationTaiwan(this);
			}
			else
			{
				return new APComplianceDocumentHeaderValidation(this);
			}
		}

		protected override ZString InternalReference => Environment.Env.NumberFountains.ComplianceDocumentInternalReference(ADH_Ledger, ADH_TransactionType, ADH_GC_Company.ToGuid()).GetNextFormatted(Factory);
	}
}
