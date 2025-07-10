using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Module;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.AFR.Module
{
	public class WorkflowFilterStripsHelperJPAFR : WorkflowFilterStripsHelper
	{
		public WorkflowFilterStripsHelperJPAFR(Type businessObjectType, ZString templateCode, BusinessObjectFactory factory)
			: base(businessObjectType, templateCode, factory)
		{
			AlternativeTaskParentColumn = JPAFRHeaderSchema.JPH_ParentId;
		}

		public WorkflowFilterStripsHelperJPAFR(Type businessObjectType)
			: base(businessObjectType)
		{
			AlternativeTaskParentColumn = JPAFRHeaderSchema.JPH_ParentId;
		}
	}
}
