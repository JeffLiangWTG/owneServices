using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class AddInfoCusExitDetail : AddInfo
	{
		public AddInfoCusExitDetail(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
		{
		}

		#region Properties

		[List(nameof(Lookups) + "." + nameof(AddInfoCusExitDetailLookups.CircuitCodeList))]
		public override ZString ZG_Circuit { get => base.ZG_Circuit; set => base.ZG_Circuit = value; }

		#endregion

		#region Lookups

		public new AddInfoCusExitDetailLookups Lookups => (AddInfoCusExitDetailLookups)base.Lookups;

		protected override EUAddInfoLookups GetNewLookups() => new AddInfoCusExitDetailLookups(this);

		#endregion
	}
}
