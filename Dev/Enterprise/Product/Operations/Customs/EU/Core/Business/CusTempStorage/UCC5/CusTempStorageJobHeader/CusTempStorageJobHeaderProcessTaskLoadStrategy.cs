using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class CusTempStorageJobHeaderProcessTaskLoadStrategy : IProcessTaskLoadStrategy
	{
		public Type GetTypeForLoad(string parentTablePrefix, ZGuid parentID, BusinessObjectFactory factory)
		{
			object job = parentID.IsValid && parentTablePrefix == CusTempStorageJobHeaderSchema.Constants.Prefix ? factory.Load<CusTempStorageJobHeader>(parentID) : null;
			return job != null
				? typeof(CusTempStorageJobHeaderProcessTask)
				: null;
		}

		public void AddAdditionalParentFilters(WorkflowDescriptor workflowDescriptor, ZDBOnlySubQuery subQuery)
		{
		}
	}
}

