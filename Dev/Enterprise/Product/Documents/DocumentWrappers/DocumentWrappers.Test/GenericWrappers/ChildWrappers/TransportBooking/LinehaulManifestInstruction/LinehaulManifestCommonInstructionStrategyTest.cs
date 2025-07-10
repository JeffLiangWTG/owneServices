using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportConsignment.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	public abstract class LinehaulManifestCommonInstructionStrategyTest : TestCaseWithFactory
	{
		#region Implementation

		public virtual LinehaulManifestCommonInstructionStrategy Instruction { get { return null; } }

		public DtbLinehaulManifest GetManifest()
		{
			depot1 = Factory.NewWithValidTestData<OrgAddress>();
			depot1.OA_Address1 = "5 SESEME STREET";
			depot2 = Factory.NewWithValidTestData<OrgAddress>();
			depot2.OA_Address1 = "9 BLAH STREET";

			var manifest = Factory.New<DtbLinehaulManifest>();
			manifest.LHM_OA_OriginDepot = depot1.PK;
			manifest.LHM_OA_DestinationDepot = depot2.PK;

			Factory.Save();
			return manifest;
		}

		protected OrgAddress depot1;
		protected OrgAddress depot2;

		#endregion
	}
}
