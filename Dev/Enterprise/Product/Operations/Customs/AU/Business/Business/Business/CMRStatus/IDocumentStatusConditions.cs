using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public interface IDocumentStatusConditions
	{
		CMRDocumentStatusConditions DocumentStatusConditions
		{
			get;
		}
	}
}
