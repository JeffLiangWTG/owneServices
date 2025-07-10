using System;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class TemporaryStorageContainer : EU.Business.CusTempStorage.TemporaryStorageContainer, Integration.Customs.IT.ITemporaryStorageContainer, ICusSealTypeSupporter
{
	public TemporaryStorageContainer(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
	{
		return base.GetShouldPropertiesBeReadOnly(property) || IsCustomsStatusAMG;
	}

	public new TemporaryStorageHeader Header => (TemporaryStorageHeader)base.Header;

	public new CusSealCollection AdditionalSeals => (CusSealCollection)base.AdditionalSeals;

	protected override EU.Business.Declaration.CusSealCollection GetCusSealCollection()
	{
		var sealCollection = new CusSealCollection(this);
		if (IsCustomsStatusAMG)
		{
			sealCollection.SetCountedReadOnlyIncludingChildren(true);
		}
		return sealCollection;
	}

	Type ICusSealTypeSupporter.CusSealType => typeof(CusSeal);

	public new TemporaryStorageContainerValidation Validation => (TemporaryStorageContainerValidation)base.Validation;

	protected override AsycudaContainerValidation GetNewValidation() => new TemporaryStorageContainerValidation(this);

	bool IsCustomsStatusAMG => Header?.IsCustomsStatusAMG ?? ZBool.False;
}
