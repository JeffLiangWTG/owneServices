using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public abstract class NctsSupportingDocumentLookups : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentLookups
	{
		public NctsSupportingDocumentLookups(NctsSupportingDocument parent) : base(parent)
		{
		}

		public new NctsSupportingDocument Parent => (NctsSupportingDocument)base.Parent;

		public abstract ZZRefCusCodeListCombinedCollection TypeCodeList { get; }

		protected ZString DataGroupingCode => Parent.Header?.DefaultDataGroupingCode ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
	}
}
