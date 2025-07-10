using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.ClusterKey.Testing
{
	sealed class DummyClusterKeyGrandChildBizo : EnterpriseBusinessObject, IClusterKeyWorker
	{
		public DummyClusterKeyGrandChildBizo(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		#region Schema

		public abstract class Schema
		{
			public const string TableName = "DummyPivot";
			public const string PK = "ZDP_PK";
		}

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ZDP_Number = 0;
		}

		public override SchemaGuidColumn PKSchemaColumn => DummyPivotSchema.PK;

		public ZPropertyInfoInt ClusterKeyPty => clusterKeyPty ?? (clusterKeyPty = new ZPropertyInfoInt(this, "ZDP_Number"));
		ZPropertyInfoInt clusterKeyPty;
		public ZInt ZDP_Number
		{
			get => Convert.ToInt32(ZDP_AddInfo);
			set => ZDP_AddInfo = value.ToString();
		}

		public ZString ZDP_AddInfo
		{
			get => new ZString(GetValueFromRowSafely(DummyPivotSchema.ZDP_AddInfo));
			set => SetPropertyValue(ZDP_AddInfoInfo, value);
		}
		public ZPropertyInfo ZDP_AddInfoInfo => GetZPropertyInfo(DummyPivotSchema.Constants.ZDP_AddInfo);

		public ZGuid ZDP_ZD1
		{
			get => new ZGuid(GetValueFromRowSafely(DummyPivotSchema.ZDP_ZD1));
			set => SetPropertyValue(ZDP_ZD1Info, value);
		}
		public ZPropertyInfo ZDP_ZD1Info => GetZPropertyInfo(DummyPivotSchema.Constants.ZDP_ZD1);
		public ZPropertyInfoGuid FkToParentPty => (ZPropertyInfoGuid)ZDP_ZD1Info;

		public Type ParentBizObjType => typeof(DummyClusterKeyChildBizo);

		public IEnumerable<ClusterKeyChildInfo> ClusterKeyChildList => null;
	}
}
