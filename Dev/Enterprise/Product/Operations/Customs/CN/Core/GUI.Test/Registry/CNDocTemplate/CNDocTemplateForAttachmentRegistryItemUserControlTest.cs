using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CN.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI.Testing
{
	[TestedType(typeof(CNDocTemplateForAttachmentRegistryItemUserControl))]
	class CNDocTemplateForAttachmentRegistryItemUserControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new CNDocTemplateForAttachmentCollection(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => ((CNDocTemplateForAttachmentRegistryItemUserControl)control).MainGrid.ReadOnly;
	}
}
