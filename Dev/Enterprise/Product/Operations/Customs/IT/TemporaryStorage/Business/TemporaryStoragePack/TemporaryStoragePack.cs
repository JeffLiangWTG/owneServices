using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

sealed class TemporaryStoragePack : EU.Business.CusTempStorage.TemporaryStoragePack
{
	public TemporaryStoragePack(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new TemporaryStorageBill Bill => (TemporaryStorageBill)base.Bill;

	protected override AsycudaPackValidation GetNewValidation() => new TemporaryStoragePackValidation(this);

	[ReadOnlyMember(nameof(IsCustomsStatusAMG))]
	public override ZGuid ContainerPK { get => base.ContainerPK; set => base.ContainerPK = value; }

	bool IsCustomsStatusAMG => Bill?.Header?.IsCustomsStatusAMG ?? ZBool.False;
}
