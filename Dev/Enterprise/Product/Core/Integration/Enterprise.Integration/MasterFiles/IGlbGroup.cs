using System.Collections;
using CargoWise.Common.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration
{
	/// <summary>
	/// *** Please implement new MasterFiles interfaces out in Enterprise.MasterFiles.Integration. ***

	/// This has been defined here as it is used in ZArchitecture.Core, but all other MasterFiles 
	/// interfaces are defined in Enterprise.MasterFiles.Integration.

	/// *** Please implement new MasterFiles interfaces out in Enterprise.MasterFiles.Integration. ***
	/// </summary>
	public interface IGlbGroup : IBusiness
	{
		ZGuid PK { get; }
		ZString GG_Code { get; set; }
		ZString GG_Desc { get; set; }
		[SuppressWeaklyTypedCollectionMessage]
		IList Staff { get; }
	}
}
