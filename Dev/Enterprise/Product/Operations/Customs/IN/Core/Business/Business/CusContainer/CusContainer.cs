using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.IN.Business;

public partial class CusContainer
{
	public CusContainer(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	[List(nameof(Lookups) + "." + nameof(CusContainerLookups.SealTypeList))]
	[ResourceStringData("Enterprise.Customs.IN.Business.CusContainer|CO_SealType", Caption = "Seal Type", MediumCaption = "Seal Type", ShortCaption = "SL.Ty.")]
	public override ZString CO_SealType => base.CO_SealType;

	[ResourceStringData("Enterprise.Customs.IN.Business.CusContainer|CO_SealDeviceID", Caption = "Seal Device ID", MediumCaption = "Device ID", ShortCaption = "Device ID")]
	public override ZString CO_SealDeviceID { get => base.CO_SealDeviceID; set => base.CO_SealDeviceID = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.CusContainer|CO_MovementDocumentType", Caption = "Movement Document Type", MediumCaption = "Move. Doc. Type", ShortCaption = "Doc. Type")]
	public override ZString CO_MovementDocumentType { get => base.CO_MovementDocumentType; set => base.CO_MovementDocumentType = value; }

	[ResourceStringData("Enterprise.Customs.IN.Business.CusContainer|CO_MovementDocumentNum", Caption = "Movement Document Num", MediumCaption = "Move. Doc. No.", ShortCaption = "Doc. No.")]
	public override ZString CO_MovementDocumentNum { get => base.CO_MovementDocumentNum; set => base.CO_MovementDocumentNum = value; }
}
