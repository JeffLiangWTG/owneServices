using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business.ClusterKey
{
	/// <summary>
	/// Mid or leaf level Cluster Key entity.
	/// Gets Cluster Key value from parent
	/// and, on behalf of the parent, dictates the key to its sub-tree.
	///
	/// --------------------------------------------------------------------  
	/// If used in conjunction with IClusterKeyMaster it can behave as:
	///   - MASTER => if detached (FK to Cluster Key Parent is null)
	///   - WORKER  => if attached to a Cluster Key Parent (FK is not null)
	/// --------------------------------------------------------------------  
	/// </summary>
	public interface IClusterKeyWorker : IClusterKeyEntity
	{
		Type ParentBizObjType { get; }
		ZPropertyInfoGuid FkToParentPty { get; }
		IEnumerable<ClusterKeyChildInfo> ClusterKeyChildList { get; }
	}
}
