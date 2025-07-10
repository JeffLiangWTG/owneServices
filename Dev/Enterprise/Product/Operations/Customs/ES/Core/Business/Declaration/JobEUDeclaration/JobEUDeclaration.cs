using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business.Declaration;

public class JobEUDeclaration(BusinessObjectFactory factory, DataRow row) : EU.Business.Declaration.JobEUDeclaration(factory, row)
{
	[List(nameof(Lookups) + "." + nameof(JobEUDeclarationLookups.RegionOrTerritoryOfDestinationList))]
	public override ZString EUD_RegionOrTerritoryOfDestination { get => base.EUD_RegionOrTerritoryOfDestination; set => base.EUD_RegionOrTerritoryOfDestination = value; }

	protected override Type JobDeclarationType => typeof(JobDeclaration);

	public new JobEUDeclarationLookups Lookups => (JobEUDeclarationLookups)base.Lookups;

	protected override EU.Business.Declaration.JobEUDeclarationLookups GetNewLookups() => new JobEUDeclarationLookups(this);
}
