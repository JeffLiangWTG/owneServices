using Enterprise.Customs.KR.Messaging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.KR.Business
{
	public class SupportingDocumentLookups : Customs.Business.CusSupportingInfoLookups
	{
		public SupportingDocumentLookups(SupportingDocument parent)
			: base(parent)
		{
		}

		public SupportingDocument LocalExportSupportingDocument
		{
			get { return Parent; }
		}

		protected new SupportingDocument Parent
		{
			get { return (SupportingDocument)base.Parent; }
		}

		public CodeDescriptionPairList LocalExportDocumentTypeList => Factory.GetCachedValue<LocalExportDocumentTypeList>();
	}
}
