using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Declaration;

public class AddInfoCusEntryHeader : EU.Business.Declaration.AddInfoCusEntryHeader
{
	public AddInfoCusEntryHeader(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
	{
	}

	#region Properties

	[List(nameof(Lookups) + "." + nameof(AddInfoCusEntryHeaderLookups.AmendmentStatusList))]
	public override ZString ZG_AmendmentStatus { get => base.ZG_AmendmentStatus; set => base.ZG_AmendmentStatus = value; }

	#endregion

	protected override SchemaColumn[] ColumnsForFastSearch => new SchemaColumn[] { EUAddInfoSchema.ZG_Parallel };

	public new AddInfoCusEntryHeaderLookups Lookups => (AddInfoCusEntryHeaderLookups)base.Lookups;

	protected override EUAddInfoLookups GetNewLookups() => new AddInfoCusEntryHeaderLookups(this);
}
