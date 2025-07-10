using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class DocumentProviderHelper
	{
		public DocumentProviderHelper(EMCSDocument document)
		{
			this.document = document;
		}
		readonly EMCSDocument document;

		public string DocumentType => CachedValueHelper.GetValue(ref documentType, () => document.CSI_SubType);
		CachedValue<string> documentType;

		public string DocumentReference => CachedValueHelper.GetValue(ref documentReference, () => document.CSI_ReferenceNumber);
		CachedValue<string> documentReference;
	}
}
