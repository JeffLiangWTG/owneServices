using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class CusTempStorageRegHeader : EU.TemporaryStorage.Business.CusTempStorageRegHeader, Integration.Customs.IT.ICusTempStorageRegHeader
{
	public CusTempStorageRegHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new CusTempStorageRegLineCollection CusTempStorageRegLines => (CusTempStorageRegLineCollection)base.CusTempStorageRegLines;

	[ResourceStringData("Enterprise.Customs.IT.TemporaryStorage.Business.CusTempStorageRegHeader|SRH_Reference", Caption = "Register Reference")]
	public override ZString SRH_Reference { get => base.SRH_Reference; set => base.SRH_Reference = value; }

	[ResourceStringData("Enterprise.Customs.IT.TemporaryStorage.Business.CusTempStorageRegHeader|SRH_PreviousReference", Caption = "Bill MRN")]
	public override ZString SRH_PreviousReference { get => base.SRH_PreviousReference; set => base.SRH_PreviousReference = value; }

	[ResourceStringData("Enterprise.Customs.IT.TemporaryStorage.Business.CusTempStorageRegHeader|SRH_TransportID", Caption = "Transport ID")]
	public override ZString SRH_TransportID { get => base.SRH_TransportID; set => base.SRH_TransportID = value; }

	protected override EFTA.TemporaryStorageRegister.Business.CusTempStorageRegLineCollection CreateNewCusTempStorageRegLines() => new CusTempStorageRegLineCollection(this);

	protected override Type GetStorageRegLineTypeCore() => typeof(CusTempStorageRegLine);

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		SRH_AppCode = ITConstants.TemporaryStorage.AppCodeTSR;
	}
}
