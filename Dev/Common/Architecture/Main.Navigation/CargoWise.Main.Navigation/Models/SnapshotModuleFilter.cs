using System;
using Enterprise.ZArchitecture.Modules;

namespace CargoWise.Main.Navigation;

#nullable disable
public record SnapshotModuleFilter
{
	public Guid ModuleFilterId { get; set; }
	public string ModuleFilterName { get; set; }

	public ModuleIdentifier ModuleId { get; set; }
	public string ModuleName { get; set; }
}

public record SnapshotModule
{
	public ModuleIdentifier ModuleId { get; set; }
	public string ModuleName { get; set; }
}
