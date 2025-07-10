using System.Collections;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageSupportingDocumentLookups : SupportingDocumentLookups
	{
		public TemporaryStorageSupportingDocumentLookups(TemporaryStorageSupportingDocument parent) : base(parent)
		{
		}

		public new TemporaryStorageSupportingDocument Parent => (TemporaryStorageSupportingDocument)base.Parent;

		public override ICollection CodeList
		{
			get
			{
				ICollection result;
				var temporaryStorageHeader = Parent.TemporaryStorageHeader;
				if (temporaryStorageHeader == null)
				{
					result = new ZZRefCusCodeListCombinedCollection(Factory);
				}
				else
				{
					result = temporaryStorageHeader.Configuration.SupportingDocumentConfiguration.GetCodeList(Parent);
				}
				return result;
			}
		}
	}
}
