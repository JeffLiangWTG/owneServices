using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class InlandTransport : EU.Business.Declaration.InlandTransport
{
	public InlandTransport(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{ }

	[MaxLength(27)]
	public override ZString CY_Data
	{
		get => base.CY_Data;
		set => base.CY_Data = value;
	}

	protected override TypeLoaderCollection parentLoaders => new(typeof(JobDeclaration));
}
