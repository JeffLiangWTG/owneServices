using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.IT.TemporaryStorage.Business;

public class ArrivalTransportMeans : EU.Business.CusTempStorage.ArrivalTransportMeans
{
	public ArrivalTransportMeans(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	[ResourceStringData("81EAD187-597E-4DD8-917F-65BE85EFC37E", Caption = "Transport ID")]
	public override ZString TPM_IdentificationNumber
	{
		get => base.TPM_IdentificationNumber;
		set => base.TPM_IdentificationNumber = value;
	}

	protected override CusTransportMeansValidation GetNewValidation() => new ArrivalTransportMeansValidation(this);
}

