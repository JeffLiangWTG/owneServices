using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public interface ITriageAssistParent : IBusiness
	{
		ZGuid PK { get; }
		ZString Priority { get; set; }
		ZString Product { get; set; }
		ZString Module { get; set; }
		ZString ProductArea { get; set; }
		ZString Category { get; set; }
		ZString SourceModuleId { get; set; }
		ZString SourceModuleWithPath { get; }
		ZBool IsSourceModuleOverriden { get; }
		IncidentTriage IncidentTriage { get; }
		ZPropertyInfo TriagePKInfo { get; }
		ZGuid TriagePK { get; set; }
		Logs Logs { get; }
		HashSet<ZString> nonTriageOverridableCriticalities { get; }
		string TablePrefix { get; }
	}
}
