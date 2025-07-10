using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal;

[XsdSchema(Placement.Outer)]
public class PotentialCarrier : IDataObject
{
	public PotentialCarrier() { }

	public PotentialCarrier(IOrganisationData carrier, IOrganisationData creditor)
	{
		Code = carrier.Code;
		Name = carrier.FullName;
		Creditor = OrganisationData.New(creditor);
	}

	[MaxLength(12), Mandatory]
	public ZString? Code { get; set; }

	[MaxLength(100)]
	public ZString? Name { get; set; }

	public OrganisationData Creditor { get; set; }
}
