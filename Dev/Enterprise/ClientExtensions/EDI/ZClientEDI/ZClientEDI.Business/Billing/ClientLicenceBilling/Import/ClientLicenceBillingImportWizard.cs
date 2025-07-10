using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicenceBillingImportWizard : ImportWizard
	{
		public ClientLicenceBillingImportWizard(IImportCollectionInfo collectionInfo, ISettingsStorage settingsStorage, IFileMapper fileMapper) : base(collectionInfo, settingsStorage, fileMapper)
		{
		}

		protected override void ImportIntoBizObjCore(Action<BusinessObject, string, object> setValue, IEnumerable<ImportWizardMapping> mappedRecords, BusinessObject bizObj, string[] values)
		{
			base.ImportIntoBizObjCore(SetValueDirectly, mappedRecords, bizObj, values);
		}

		void SetValueDirectly(BusinessObject bizObj, string propertyName, object value)
		{
			bizObj[propertyName] = value;
		}
	}
}
