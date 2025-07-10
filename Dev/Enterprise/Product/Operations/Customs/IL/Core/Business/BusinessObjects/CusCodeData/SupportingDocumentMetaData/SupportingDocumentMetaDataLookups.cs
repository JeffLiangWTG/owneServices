using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IL.Business
{
	public class SupportingDocumentMetaDataLookups : CusCodeDataLookups
	{
		public SupportingDocumentMetaDataLookups(SupportingDocumentMetaData parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList CY_CodeList => Parent.Parent.GetSupportingDocumentMetaDataCodeList();

		public new SupportingDocumentMetaData Parent => (SupportingDocumentMetaData)base.Parent;
	}
}
