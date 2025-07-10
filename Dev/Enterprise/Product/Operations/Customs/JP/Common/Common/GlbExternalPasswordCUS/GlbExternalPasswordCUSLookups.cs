using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Common
{
	public class GlbExternalPasswordCUSLookups : GlbExternalPasswordWithPasswordTypeLookups
	{
		public GlbExternalPasswordCUSLookups(GlbExternalPasswordCUS parent) : base(parent)
		{
		}

		public CodeDescriptionPairList TransportModeList => new UserCodeSpecificTransportModeList();
	}
}
