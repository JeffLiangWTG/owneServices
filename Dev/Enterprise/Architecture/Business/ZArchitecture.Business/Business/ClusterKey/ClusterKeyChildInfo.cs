using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.ZArchitecture.Business.ClusterKey
{
	public class ClusterKeyChildInfo
	{
		public ClusterKeyChildInfo(Type bizObjType, SchemaGuidColumn fkColumn)
		{
			BizObjType = bizObjType;
			FkColumn = fkColumn;
		}

		public Type BizObjType { get; }
		public SchemaGuidColumn FkColumn { get; }

		internal IEnumerable<IClusterKeyWorker> LoadChildObjects(EnterpriseBusinessObject parent)
		{
			var query = new ZQuery(FkColumn, parent.PK);
			return (IClusterKeyWorker[])parent.Factory.Load(BizObjType, query);
		}
	}
}
