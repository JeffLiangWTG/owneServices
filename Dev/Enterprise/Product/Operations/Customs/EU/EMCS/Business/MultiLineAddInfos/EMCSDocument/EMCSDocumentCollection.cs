using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public interface IEMCSDocumentCollection<out T> : ICusSupportingInfoCollection<T>, ISupportMaxCountValidation where T : EMCSDocument
	{
	}

	public class EMCSDocumentCollection<TEMCSDocument> : CusSupportingInfoCollection<TEMCSDocument>, IEMCSDocumentCollection<TEMCSDocument> where TEMCSDocument : EMCSDocument
	{
		public EMCSDocumentCollection(BusinessObject parent) : base(parent, CusSupportingInfoTypeList.Codes.Certificate)
		{
		}

		protected override bool AllowNewCore => base.AllowNewCore && Master is EMCSJobDeclaration declaration && !declaration.IsMessageStatusSentOrAcknowledged;
	}
}
