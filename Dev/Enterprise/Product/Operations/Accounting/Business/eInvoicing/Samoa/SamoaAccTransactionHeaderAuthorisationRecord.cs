using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.EInvoicing.TaxCore;
using Enterprise.MasterFiles.Business.CountryCompliance;

namespace Enterprise.Accounting.Business.EInvoicing.Samoa
{
	public class SamoaAccTransactionHeaderAuthorisationRecord : TaxCoreAccTransactionHeaderAuthorisationRecord
	{
		public SamoaAccTransactionHeaderAuthorisationRecord(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AHF_RecordType = AccTransactionHeaderAuthorisationRecordTypes.WesternSamoa;
		}

		protected override AdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper GetAdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper()
		{
			return new TaxCoreAdditionalAccTransactionHeaderAuthorisationRecordInfoForDocWrapper(this);
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			AHF_Counter = "WSI";
			AHF_Number = "TSTWSI";
		}
#endif
	}
}
