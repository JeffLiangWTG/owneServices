using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public interface ITemporaryStoragePreviousDocumentCollection<out T> : Customs.Business.ICusSupportingInfoCollection<T>
		where T : TemporaryStoragePreviousDocument
	{
	}

	public class TemporaryStoragePreviousDocumentCollection<T> : Customs.Business.CusSupportingInfoCollection<T>, ITemporaryStoragePreviousDocumentCollection<T>
		where T : TemporaryStoragePreviousDocument
	{
		public TemporaryStoragePreviousDocumentCollection(BusinessObject parent) : base(parent, Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument)
		{
			InitMaxCountValidation();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (Master is TemporaryStorageHeader header && (header.IsTransfer || header.IsDeconsolidation))
			{
				var prevDoc = child as TemporaryStoragePreviousDocument;
				prevDoc.CSI_Code = TemporaryStorageConstants.PreviousDocumentsCodeType.NMRN;
			}
		}

		protected override bool AllowNewCore => !EnableMaxCountValidation || Count < MaxCount;

		void InitMaxCountValidation()
		{
			if (EnableMaxCountValidation)
			{
				var maxAllowedPreviousDocuments = PreviousDocumentConfiguration.CollectionMaxCount;
				MaxCountValidationEnable(maxAllowedPreviousDocuments, Res.GetString("22789EC6-76E6-4ECB-B1AA-6EEAA08AA1AB", "You are only allowed a maximum of 1 Previous Document."));
			}
		}

		bool EnableMaxCountValidation => PreviousDocumentConfiguration?.IsCollectionMaxCountValidationEnabled() ?? false;

		TemporaryStoragePreviousDocumentConfiguration PreviousDocumentConfiguration => Header?.Configuration?.PreviousDocumentConfiguration;

		TemporaryStorageHeader Header => Master switch
		{
			TemporaryStorageHeader header => header,
			TemporaryStorageBill bill => bill?.Header,
			TemporaryStoragePackedItem packedItem => packedItem?.Bill?.Header,
			_ => null
		};
	}
}
