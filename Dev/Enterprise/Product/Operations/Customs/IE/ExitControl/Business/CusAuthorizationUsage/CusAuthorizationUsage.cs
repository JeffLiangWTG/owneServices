using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class CusAuthorizationUsage : EU.Business.CusAuthorizationUsage,
		Integration.Customs.IEExitControl.ICusAuthorizationUsage,
		IClusterKeyWorker
	{
		public CusAuthorizationUsage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public CusExitConsignment CusExitConsignment => cusExitConsignment ?? (cusExitConsignment = (AGC_ParentTableCode.Equals(CusExitConsignmentSchema.Constants.Prefix) ? Factory.Load<CusExitConsignment>(AGC_ParentID) : null));
		CusExitConsignment cusExitConsignment;

		public CusExitConsignmentItem CusExitConsignmentItem => cusExitConsignmentItem ?? (cusExitConsignmentItem = (AGC_ParentTableCode.Equals(CusExitConsignmentItemSchema.Constants.Prefix) ? Factory.Load<CusExitConsignmentItem>(AGC_ParentID) : null));
		CusExitConsignmentItem cusExitConsignmentItem;

		public new CusAuthorizationUsageLookups Lookups => (CusAuthorizationUsageLookups)base.Lookups;
		protected override EU.Business.CusAuthorizationUsageLookups GetNewLookups() => new CusAuthorizationUsageLookups(this);

		ZPropertyInfoGuid IClusterKeyWorker.FkToParentPty => (ZPropertyInfoGuid)AGC_ParentIDInfo;

		IEnumerable<ClusterKeyChildInfo> IClusterKeyWorker.ClusterKeyChildList => null;
		Type IClusterKeyWorker.ParentBizObjType
		{
			get
			{
				switch (AGC_ParentTableCode)
				{
					case CusExitConsignmentSchema.Constants.Prefix:
						return typeof(CusExitConsignment);
					case CusExitConsignmentItemSchema.Constants.Prefix:
						return typeof(CusExitConsignmentItem);
					default:
						return null;
				}
			}
		}
	}
}
