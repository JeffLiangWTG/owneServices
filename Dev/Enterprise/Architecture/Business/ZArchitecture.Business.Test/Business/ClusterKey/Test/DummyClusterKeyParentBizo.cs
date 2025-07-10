using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.ClusterKey.Testing
{
	sealed class DummyClusterKeyParentBizo : EnterpriseBusinessObject, IClusterKeyMaster
	{
		public DummyClusterKeyParentBizo(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		#region Schema

		public abstract class Schema
		{
			public const string TableName = "DummyBizo";
			public const string PK = "Z0_PK";
		}

		#endregion

		public override SchemaGuidColumn PKSchemaColumn => DummyBizoSchema.PK;

		public override bool IsDeleted => MockIsDeleted ?? base.IsDeleted;
		public bool? MockIsDeleted { get; set; }

		public ZInt Z0_Number
		{
			get => new ZInt(GetValueFromRowSafely(DummyBizoSchema.Z0_Number));
			set
			{
				this.CheckCanSetMasterClusterKey();
				SetPropertyValue(Z0_NumberInfo, value);
			}
		}
		public ZPropertyInfo Z0_NumberInfo => GetZPropertyInfo(DummyBizoSchema.Constants.Z0_Number);
		public ZPropertyInfoInt ClusterKeyPty => (ZPropertyInfoInt)Z0_NumberInfo;

		public DummyClusterKeyChildBizo AddNewDependentBizo()
		{
			var newDependentBizo = Factory.New<DummyClusterKeyChildBizo>();
			newDependentBizo.ZD1_Z0 = PK;
			return newDependentBizo;
		}
	}
}
