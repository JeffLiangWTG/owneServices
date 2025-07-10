using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Security.Provider;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security.Testing
{
	sealed class DocumentsSecurityInfoProviderTest : StmMenuItemsSecurityInfoProviderTest<DocumentsSecurityInfoProvider>
	{
		protected override ZQuery GetNewMenuItemQuery()
		{
			return new DocumentZQuery();
		}

		protected override void AssertSecurityInfo(ISecurityInfo securityInfo, StmMenuItem item, string moduleID)
		{
			AssertEquals("Modify / visualisation override checkpoint.", "Modify", securityInfo.Nodes.ElementAt(0).Name);
			AssertEquals("Modify / visualisation override checkpoint.", "DocVis" + moduleID, securityInfo.Nodes.ElementAt(0).Checkpoint.Code);
			AssertEquals("Modify / visualisation override checkpoint.", item.PK, securityInfo.Nodes.ElementAt(0).Checkpoint.ItemGuid);
		}

		protected override SecurityCheckpoint GetMenuItemCheckpoint(SecurityCore security, Guid guid, string menuName, ModuleIdentifier moduleId, ISecurityCheckpoint parent)
		{
			return security.FindOrCreateDocumentCheckpoint(guid, (NoResString)menuName, moduleId, parent);
		}

		protected override StmMenuItemCheckpointHelper CheckpointHelper
		{
			get { return new DocumentsCheckpointHelper(); }
		}
	}
}
