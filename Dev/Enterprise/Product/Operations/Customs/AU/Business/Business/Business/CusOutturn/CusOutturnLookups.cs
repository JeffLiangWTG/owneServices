using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusOutturnLookups : Customs.Business.CusOutturnLookups
	{
		public CusOutturnLookups(CusOutturn outturn)
			: base(outturn)
		{
		}

		public override CodeDescriptionPairList OutturnResultTypeList
		{
			get { return Factory.GetCachedValue<CMROutturnResultType>(); }
		}

		protected override CodeDescriptionPairList GetNewCargoTypes()
		{
			return Factory.GetCachedValue<CMRImportCargoTypes>();
		}

		protected override CodeDescriptionPairList GetNewPackageTypes()
		{
			if (Parent.C5_CargoType == CMRImportCargoTypes.Codes.Bulk)
			{
				return Factory.GetCachedValue<CMRQuantityUnits>();
			}

			return Factory.GetCachedValue<CMRPackageTypes>();
		}

		#region CommercialStatusList

		public override ReadOnlyCodeDescriptionPairList CommercialStatusList
		{
			get { return Env.Registry.AUCustoms.SeaCargoCommercialStatus; }
		}

		#endregion

		#region Implementation

		new CusOutturn Parent
		{
			get { return (CusOutturn)base.Parent; }
		}

		#endregion
	}
}
