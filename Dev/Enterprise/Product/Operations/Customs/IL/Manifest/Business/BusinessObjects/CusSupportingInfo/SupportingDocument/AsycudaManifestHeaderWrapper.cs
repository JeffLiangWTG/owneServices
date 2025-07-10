using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.IL.Business;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaManifestHeaderWrapper : NonPersistentBusinessObject
	{
		public AsycudaManifestHeaderWrapper(AsycudaManifestHeader header)
		{
			this.header = header;
		}
		readonly AsycudaManifestHeader header;

		public SupportingDocumentWrappersCollection SupportingDocuments
		{
			get
			{
				if (supportingDocuments == null)
				{
					supportingDocuments = new SupportingDocumentWrappersCollection(Factory);
					supportingDocuments.AddRange(GetMessageSendingObjects());
				}

				RegisterEditableChildObject(supportingDocuments);
				return supportingDocuments;
			}
		}
		SupportingDocumentWrappersCollection supportingDocuments;

		IEnumerable<SupportingDocumentWrapper> GetMessageSendingObjects()
		{
			var supportingDocuments = header.Bills.Cast<AsycudaBill>().SelectMany(s => s.SupportingDocuments.Cast<SupportingDocument>());

			if (supportingDocuments != null)
			{
				var filteredSupportingDocuments = supportingDocuments
					.Where(x => x.CSI_Status.IsEmpty
					|| x.CSI_Status == RequestedSupportingStatusList.Codes.REQ
					|| x.CSI_Status == RequestedSupportingStatusList.Codes.FAL)
					.Select(x => new SupportingDocumentWrapper(x));

				return filteredSupportingDocuments;
			}
			return Enumerable.Empty<SupportingDocumentWrapper>();
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			if (!SupportingDocuments.Any(c => ((SupportingDocumentWrapper)c).IsSelected))
			{
				AddRowError(Res.GetString("5BD05A2E-3299-445A-8BCB-CED2E87DB9CE", "Please select at least one valid supporting document to send the message."));
			}
		}
	}
}
