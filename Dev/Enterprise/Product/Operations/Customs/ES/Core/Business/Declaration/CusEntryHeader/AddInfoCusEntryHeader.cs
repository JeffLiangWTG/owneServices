using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ES.Business.Declaration
{
	public class AddInfoCusEntryHeader : EU.Business.Declaration.AddInfoCusEntryHeader
	{
		public AddInfoCusEntryHeader(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
		{
		}

		#region Properties

		[List(nameof(Lookups) + "." + nameof(AddInfoCusEntryHeaderLookups.CircuitCodeList))]
		public override ZString ZG_Circuit { get => base.ZG_Circuit; set => base.ZG_Circuit = value; }

		[List(nameof(Lookups) + "." + nameof(AddInfoCusEntryHeaderLookups.CircuitCodeList))]
		public override ZString ZG_CircuitCan { get => base.ZG_CircuitCan; set => base.ZG_CircuitCan = value; }

		[List(nameof(Lookups) + "." + nameof(AddInfoCusEntryHeaderLookups.ClearanceResultCodeList))]
		public override ZString ZG_ClearanceResult { get => base.ZG_ClearanceResult; set => base.ZG_ClearanceResult = value; }

		#endregion

		protected override SchemaColumn[] ColumnsForFastSearch => new SchemaColumn[] { EUAddInfoSchema.ZG_Parallel };

		public new AddInfoCusEntryHeaderLookups Lookups => (AddInfoCusEntryHeaderLookups)base.Lookups;

		protected override EUAddInfoLookups GetNewLookups() => new AddInfoCusEntryHeaderLookups(this);
	}
}
