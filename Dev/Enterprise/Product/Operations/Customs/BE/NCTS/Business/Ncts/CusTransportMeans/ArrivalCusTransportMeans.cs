using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.BE.NCTS.Business;

public sealed class ArrivalCusTransportMeans : EU.NCTS.Business.ArrivalCusTransportMeans
{
	public ArrivalCusTransportMeans(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	[ResourceStringData("0498FAF1-9BFA-4523-895F-B781B8633D0F", Caption = "Sequence Number", MediumCaption = "Sequence No.", ShortCaption = "Seq.No.")]
	public override ZShort TPM_SequenceNumber
	{
		get => base.TPM_SequenceNumber;
		set => base.TPM_SequenceNumber = value;
	}
}
