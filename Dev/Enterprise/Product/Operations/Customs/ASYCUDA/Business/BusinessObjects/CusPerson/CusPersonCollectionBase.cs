using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ASYCUDA.Business;

public class CusPersonCollection : DependentBusinessObjectCollection<CusPerson, AsycudaManifestHeader>
{
	public CusPersonCollection(AsycudaManifestHeader master) : base(master)
	{
	}

	protected override SchemaGuidColumn FKSchemaColumnInDependent
	{
		get { return CusPersonSchema.CPN_ParentID; }
	}

	protected override void SetDefaultsForNewChild(BusinessObject child)
	{
		base.SetDefaultsForNewChild(child);
		var cpn = (CusPerson)child;
		SetToPassengerIfDriverAlreadyPresent(cpn);
	}

	void SetToPassengerIfDriverAlreadyPresent(CusPerson cpn)
	{
		foreach (var brother in this.OfType<CusPerson>().Except(new[] { cpn }))
		{
			if (brother.IsDriver)
			{
				cpn.CPN_IsPassenger = true;
				break;
			}
		}
	}
}
