using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers;

namespace Enterprise.Customs.GB.DocumentWrappers.Freight
{
	public class DeclarationWrapper : DocBaseWrapper
	{
		public static DeclarationWrapper New(IAdsParticipant iAdsParticpant, BusinessObjectFactory factory)
		{
			return new DeclarationWrapper(iAdsParticpant, factory);
		}

		internal DeclarationWrapper(IAdsParticipant iAdsParticpant, BusinessObjectFactory factory)
			: base(iAdsParticpant, factory)
		{
			this.AdsParticipant = iAdsParticpant;
		}

		public IAdsParticipant AdsParticipant { get; private set; }
	}
}
