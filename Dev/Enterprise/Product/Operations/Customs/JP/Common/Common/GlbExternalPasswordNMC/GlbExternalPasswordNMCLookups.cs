using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Common
{
	public class GlbExternalPasswordNMCLookups : GlbExternalPasswordWithPasswordTypeLookups
	{
		public GlbExternalPasswordNMCLookups(GlbExternalPasswordNMC parent) : base(parent)
		{
		}

		public override CodeDescriptionPairList PasswordStatusList => passwordStatusList ??= new ();
		CodeDescriptionPairList passwordStatusList;
	}
}
