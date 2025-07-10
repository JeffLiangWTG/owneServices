using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.ClusterKey.Testing
{
	class DummyClusterKeyChildBizo : EnterpriseBusinessObject, IClusterKeyWorker, IClusterKeyMaster
	{
		public DummyClusterKeyChildBizo(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		#region Schema

		public abstract class Schema
		{
			public const string TableName = "DummyDependentBizo";
			public const string PK = "ZD1_PK";
		}

		#endregion

		public override SchemaGuidColumn PKSchemaColumn => DummyDependentBizoSchema.PK;

		public override bool IsDeleted => MockIsDeleted ?? base.IsDeleted;
		public bool? MockIsDeleted { get; set; }

		public ZInt ZD1_Number
		{
			get => new ZInt(GetValueFromRowSafely(DummyDependentBizoSchema.ZD1_Number));
			set
			{
				this.CheckCanSetMasterClusterKey();
				SetPropertyValue(ZD1_NumberInfo, value);
			}
		}
		public ZPropertyInfo ZD1_NumberInfo => GetZPropertyInfo(DummyDependentBizoSchema.Constants.ZD1_Number);
		public ZPropertyInfoInt ClusterKeyPty => (ZPropertyInfoInt)ZD1_NumberInfo;

		public ZGuid ZD1_Z0
		{
			get => new ZGuid(GetValueFromRowSafely(DummyDependentBizoSchema.ZD1_Z0));
			set => SetPropertyValue(ZD1_Z0Info, value);
		}
		public virtual ZPropertyInfo ZD1_Z0Info => GetZPropertyInfo(DummyDependentBizoSchema.Constants.ZD1_Z0);
		public ZPropertyInfoGuid FkToParentPty => (ZPropertyInfoGuid)ZD1_Z0Info;

		public Type ParentBizObjType => typeof(DummyClusterKeyParentBizo);

		public IEnumerable<ClusterKeyChildInfo> ClusterKeyChildList => clusterKeyChildList;
		readonly ClusterKeyChildInfo[] clusterKeyChildList = new ClusterKeyChildInfo[] { new ClusterKeyChildInfo(typeof(DummyClusterKeyGrandChildBizo), DummyPivotSchema.ZDP_ZD1) };

		public DummyClusterKeyGrandChildBizo AddNewDependentBizo()
		{
			var newDependentBizo = Factory.New<DummyClusterKeyGrandChildBizo>();
			newDependentBizo.ZDP_ZD1 = PK;
			return newDependentBizo;
		}
	}
}
