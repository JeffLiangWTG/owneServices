using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSAddInfoCusContainer : AddInfo
	{
		public EMCSAddInfoCusContainer(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
		{
		}

		[List(nameof(Lookups) + "." + nameof(EMCSAddInfoCusContainerLookups.EMCSDestinationTypeList))]
		public override ZString ZG_UnitCode
		{
			get => base.ZG_UnitCode;
			set => base.ZG_UnitCode = value;
		}

		public new EMCSAddInfoCusContainerValidation Validation => (EMCSAddInfoCusContainerValidation)base.Validation;
		protected override EUEMCSAddInfoValidation GetNewValidation() => new EMCSAddInfoCusContainerValidation(this);

		public new EMCSAddInfoCusContainerLookups Lookups => (EMCSAddInfoCusContainerLookups)base.Lookups;
		protected override EUEMCSAddInfoLookups GetNewLookups() => new EMCSAddInfoCusContainerLookups(this);
	}
}
