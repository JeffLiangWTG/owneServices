using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class InlandTransportCollection : EU.Business.Declaration.InlandTransportCollection
{
	public InlandTransportCollection(JobDeclaration master)
		: base(master)
	{
		SetMaxCountValidation();
	}

	public override Type GetTypeOfElementsFromPK(ZGuid pk) => typeof(InlandTransport);

	public new InlandTransport AddNew() => (InlandTransport)base.AddNew();

	public new InlandTransport AddNew(Type bizOType) => (InlandTransport)base.AddNew(bizOType);

	public new InlandTransport this[int index] => (InlandTransport)base[index];

	public new JobDeclaration Master => (JobDeclaration)base.Master;

	internal void SetMaxCountValidation()
	{
		var transportModeInland = Master.JE_TransportModeInland;
		var maxCountForValidation = 1;
		if (transportModeInland == TransportTypeList.Codes.Road)
		{
			maxCountForValidation = 3;
		}

		MaxCountValidationEnable(maxCountForValidation);
	}
}
