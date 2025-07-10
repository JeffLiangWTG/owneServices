using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class SupportingDocumentCollection : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentCollection
	{
		public SupportingDocumentCollection(BusinessObject parent)
			: base(parent)
		{
			var header = parent as JobComInvoiceHeader;
			if (header != null)
			{
				MaxCountValidationEnable(SupportingDocumentsMaxCount, SupportingDocumentsMaxCountErrorMessage(SupportingDocumentsMaxCount));
			}

			var line = parent as JobComInvoiceLine;
			if (line != null)
			{
				MaxCountValidationEnable(99);
			}
		}

		public new SupportingDocument this[int i] => (SupportingDocument)base[i];

		public new SupportingDocument AddNew() => (SupportingDocument)base.AddNew();

		public new SupportingDocument AddNew(System.Type bizOType) => (SupportingDocument)base.AddNew(bizOType);

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			using (child.SuspendSettingHasChanges())
			{
				var lineFrom = (SupportingDocument)child;
				if (Master != null)
				{
					var line = Master as JobComInvoiceLine;
					if (line != null && line.IsImport)
					{
						lineFrom.CSI_Status = AvailabilityList.Codes.J;
					}
				}
			}
		}

		const int SupportingDocumentsMaxCount = 20;

		string SupportingDocumentsMaxCountErrorMessage(int supportingDocumentsMaxCount) => Res.GetString("2EC459BB-B884-4E86-944E-CD5AA7B8BB0F", "The maximum number ({0}) of allowed Supporting Documents per Invoice Header has been exceeded.", supportingDocumentsMaxCount);
	}
}
