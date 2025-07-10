using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IDocumentStatus
	{
		CMRDocumentStatus DocumentStatus
		{
			get;
		}
	}
}
