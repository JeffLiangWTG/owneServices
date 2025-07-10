using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class TemporaryStorageSupportingDocument : EU.Business.CusTempStorage.TemporaryStorageSupportingDocument
{
	public TemporaryStorageSupportingDocument(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : EU.Business.CusTempStorage.TemporaryStorageSupportingDocument.Schema
	{
		public new const int CSI_ReferenceNumberMaxLength = 35;
	}

	protected override int CSI_ReferenceNumberMaxLength => Schema.CSI_ReferenceNumberMaxLength;

	protected override CusSupportingInfoValidation GetNewValidation() => new TemporaryStorageSupportingDocumentValidation(this);
}
