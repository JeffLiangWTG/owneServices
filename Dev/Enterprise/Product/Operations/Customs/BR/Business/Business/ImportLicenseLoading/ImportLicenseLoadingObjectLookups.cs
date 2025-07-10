using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseLoadingObjectLookups : ZLookups
	{
		public ImportLicenseLoadingObjectLookups(ImportLicenseLoadingObject parent)
			: base(parent)
		{
		}

		ImportLicenseLoadingObject ImportLicenseLoadingObject
		{
			get { return (ImportLicenseLoadingObject)Parent; }
		}

		public IBusinessObjectCollection InvoiceHeaderList => ImportLicenseLoadingObject?.Declaration?.Invoices
															  ?? (IBusinessObjectCollection)new ActiveBusinessObjectCollection<JobComInvoiceHeader>(Factory, ZQuery.NoResultQuery);

		public CodeDescriptionPairList ImportLicenseTypeList => Factory.GetCachedValue<ImportLicenseType>();

		public CodeDescriptionPairList ImportLicenseFeeTypeList => BRRefCusTaxOrFee.GetFeeTypeList(Factory, ZDateTime.Now);
	}
}
