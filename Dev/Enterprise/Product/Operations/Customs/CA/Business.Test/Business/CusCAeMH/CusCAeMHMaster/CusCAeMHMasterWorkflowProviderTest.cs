using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusCAeMHMaster))]
	sealed class CusCAeMHMasterWorkflowProviderTest : WorkflowProviderTest<CusCAeMHMaster, ProcessTaskCollection<CusCAeMHMasterProcessTask, CusCAeMHMaster>>
	{
		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.CAeManifestWorkflowDescriptorCode; }
		}

		protected override CusCAeMHMaster GetNewBusinessObject(BusinessObjectFactory factory)
		{
			return CusCAeMHMaster;
		}

		CusCAeMHMaster CusCAeMHMaster
		{
			get { return cusCAeMHMaster ?? (cusCAeMHMaster = Factory.New<CusCAeMHMaster>()); }
		}
		CusCAeMHMaster cusCAeMHMaster;

		protected override IWorkflowProvider ReloadWorkflowProvider(BusinessObjectFactory factory, CusCAeMHMaster workFlowProvider)
		{
			var reloadedCusSCAOceanBill = factory.Load<CusCAeMHMaster>(workFlowProvider.PK);
			return reloadedCusSCAOceanBill;
		}

		protected override void SetUp()
		{
			base.SetUp();

			Enterprise.ZArchitecture.Core.Testing.FountainTestListener.AddFountainAccess("CAMasterBilleManifest", Guid.Empty);
		}
	}
}
