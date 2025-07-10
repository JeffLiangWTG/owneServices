using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business.Test
{
	public class SchematicTestConfig
	{
		protected SchematicTestConfig(BusinessObjectFactory factory, string[] workflowTypes, string systemName, bool shouldUseExistingSystem)
		{
			System = shouldUseExistingSystem ? BMSTestHelper.GetOrCreateSystem(factory, workflowTypes) : BMSTestHelper.CreateSystem(factory, workflowTypes);
			System.FS_Name = systemName + " - " + workflowTypes.First();

			Bucket = BMSTestHelper.CreateBucket(System);
			Buffer = BMSTestHelper.CreateBuffer(System);

			ComponentLink = BMSTestHelper.LinkComponents(Bucket, Buffer);
			ComponentLink.FL_Name = "Buffer Entry";

			ReleaseGroup = factory.NewWithValidTestData<GlbGroup>();
			System.ReleaseGroups.AddNew().FSG_GG_Group = ReleaseGroup.PK;
		}

		internal static SchematicTestConfig Create(BusinessObjectFactory factory, string[] workflowTypes, string systemName, bool shouldUseExistingSystem)
		{
			return new SchematicTestConfig(factory, workflowTypes, systemName, shouldUseExistingSystem);
		}

		public BMSystem System { get; private set; }
		public BMComponent Bucket { get; private set; }
		public BMComponent Buffer { get; private set; }
		public BMComponentLink ComponentLink { get; private set; }
		public GlbGroup ReleaseGroup { get; private set; }

		public void RemoveReleaseGroup()
		{
			System.ReleaseGroups.DeleteAll();
			ReleaseGroup.Delete();

			ReleaseGroup = null;
		}
	}
}
