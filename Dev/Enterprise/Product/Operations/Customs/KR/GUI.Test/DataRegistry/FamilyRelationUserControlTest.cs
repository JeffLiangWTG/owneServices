using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(FamilyRelationUserControl))]
	sealed class FamilyRelationUserControlTest : RegistryZUserControlTestCase
	{
		protected override Form GetFormToBashCore()
		{
			return base.GetFormToBashCore();
		}

		protected override IBusiness GetNewBusinessEntity() => new FamilyRelationCollection(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => ((FamilyRelationUserControl)control).FamilyRelationGrid.ReadOnly;
	}
}
