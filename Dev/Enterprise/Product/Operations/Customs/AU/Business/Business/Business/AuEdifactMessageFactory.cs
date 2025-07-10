using Enterprise.Edifact;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class AuEdifactMessageFactory
	{
		static MessageFactory fAUCMessageFactory;

		public static MessageFactory AUCMessageFactory  // Public so ZClientUPE can see it, yo
		{
			get
			{
				if (fAUCMessageFactory == null)
				{
					fAUCMessageFactory = new MessageFactory(
							new Edifact.D99B.EdifactD99BMessageFactory(),
							new Edifact.D00A.EdifactD00AMessageFactory(),
							new Edifact.D96B.EdifactD96BMessageFactory(),
							new Edifact.D98B.EdifactD98BMessageFactory(),
							new Edifact.D97BAU.EdifactD97BAUMessageFactory());
				}
				return fAUCMessageFactory;
			}
		}
	}
}
