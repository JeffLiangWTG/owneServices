using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.Customs.CA.Business
{
	public class InvoiceHeaderImportWizard : ImportWizard
	{
		public InvoiceHeaderImportWizard(IImportCollectionInfo collectionInfo, ISettingsStorage settingsStorage, IFileMapper fileMapper)
			: base(collectionInfo, settingsStorage, fileMapper)
		{
			Factory = collectionInfo.Collection.Factory;
		}

		protected override void ImportIntoBizObjCore(Action<BusinessObject, string, object> setValue, IEnumerable<ImportWizardMapping> mappedRecords, BusinessObject bizObj, string[] values)
		{
			base.ImportIntoBizObjCore(setValue, mappedRecords, bizObj, values);

			var invoiceHeader = bizObj as JobComInvoiceHeader;
			if (invoiceHeader != null)
			{
				var mappings = mappedRecords.Where(m => m.HasMappedFrom());
				var packQtyMapping = mappings.FirstOrDefault(m => m.MappingName == JobComInvoiceHeader.Schema.FirstPackageQty);
				if (packQtyMapping != null)
				{
					if (float.TryParse(packQtyMapping.GetMappedFieldValue(values).Trim(), out var value))
					{
						invoiceHeader.FirstPackageQty = (int)value;
					}
				}
			}
		}

		public new BusinessObjectFactory Factory { get; }
	}
}
