using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class EDIOpportunityRegistryCaption : OpportunityRegistryCaption
	{
		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = () => new EDIOpportunityRegistryCaption();
		}

		protected override ResourceString PotentialLabel => ResString.GetMultilingualString("86424F89-E08D-4230-ACE5-3BF65C348305", "Local Reach");
		protected override ResourceString CurrentLabel => ResString.GetMultilingualString("843FC03E-BCEC-4717-9806-4F143E8A1644", "Contracted");
	}
}

