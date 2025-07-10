using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.CusTempStorage;

public class TemporaryStorageHeaderGuarantee : EU.Business.CusTempStorage.TemporaryStorageHeaderGuarantee
{
	public TemporaryStorageHeaderGuarantee(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new TemporaryStorageHeader TemporaryStorageHeader => (TemporaryStorageHeader)base.TemporaryStorageHeader;

	protected override CusBondDetailValidation GetNewValidation() => new TemporaryStorageHeaderGuaranteeValidation(this);
}
