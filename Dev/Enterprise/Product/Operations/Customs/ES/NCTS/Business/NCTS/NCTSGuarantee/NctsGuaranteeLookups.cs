using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsGuaranteeLookups : EU.NCTS.Business.NctsGuaranteeLookups
	{
		public NctsGuaranteeLookups(NctsGuarantee parent) : base(parent)
		{
		}
		new NctsGuarantee Parent => (NctsGuarantee)base.Parent;
		protected override ZGuid? SecondaryGuaranteeHolderAddress => Parent.NctsHeader?.DeclarantOrgPK;

		protected override CodeDescriptionPairList BondTypeListCore
		{
			get
			{
				if (Parent.NctsHeader?.IsPhase5Departure ?? false)
				{
					return Factory.GetCachedValue<ESNCTS5GuaranteeTypeList>();
				}

				return base.BondTypeListCore;
			}
		}
	}
}
