using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.JP.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.JP.GUI.Testing
{
	[TestedType(typeof(DefaultBrokerAndCredentialRegistryItemUserControl))]
	sealed class DefaultBrokerAndCredentialRegistryItemUserControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity() => new DefaultBrokerAndCredential(new FallbackLevel(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()), Factory);

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity) => ((DefaultBrokerAndCredentialRegistryItemUserControl)control).FindSingleOrDefault<ZCodeFindBox>(x => x.Name == "DefaultBrokerCodeFindBox").ReadOnly;

		public void TestCredentialsReadOnly()
		{
			using (var registryZUserControl = GetNewControl() as DefaultBrokerAndCredentialRegistryItemUserControl)
			{
				var newBusinessEntity = GetNewBusinessEntity();
				registryZUserControl.SetDataBinding(newBusinessEntity, null);

				var defaultCredentialSEADropEdit = registryZUserControl.FindSingleOrDefault<ZDropEdit>(x => x.Name == "DefaultCredentialSEADropEdit");
				var defaultCredentialAIRDropEdit = registryZUserControl.FindSingleOrDefault<ZDropEdit>(x => x.Name == "DefaultCredentialAIRDropEdit");
				var forwarderManifestSEADropEdit = registryZUserControl.FindSingleOrDefault<ZDropEdit>(x => x.Name == "ForwarderManifestSEADropEdit");
				var forwarderManifestAIRDropEdit = registryZUserControl.FindSingleOrDefault<ZDropEdit>(x => x.Name == "ForwarderManifestAIRDropEdit");
				var defaultBrokerCodeFindBox = registryZUserControl.FindSingleOrDefault<ZCodeFindBox>(x => x.Name == "DefaultBrokerCodeFindBox");

				defaultBrokerCodeFindBox.CodeBox.Text = "";
				registryZUserControl.ReadOnly = true;
				AssertCredentialDropEditsReadOnly(true);

				defaultBrokerCodeFindBox.CodeBox.Text = "";
				registryZUserControl.ReadOnly = false;
				AssertCredentialDropEditsReadOnly(true);

				defaultBrokerCodeFindBox.CodeBox.Text = "L";
				registryZUserControl.ReadOnly = false;
				AssertCredentialDropEditsReadOnly(false);

				defaultBrokerCodeFindBox.CodeBox.Text = "L";
				registryZUserControl.ReadOnly = true;
				AssertCredentialDropEditsReadOnly(true);

				void AssertCredentialDropEditsReadOnly(bool readOnly)
				{
					AssertEquals($"DefaultCredentialSEADropEdit should be read only {readOnly} when user control is read only {registryZUserControl.ReadOnly} and current code is {defaultBrokerCodeFindBox.Text}", readOnly, defaultCredentialSEADropEdit.ReadOnly);
					AssertEquals($"DefaultCredentialSEADropEdit should be read only {readOnly} when user control is read only {registryZUserControl.ReadOnly} and current code is {defaultBrokerCodeFindBox.Text}", readOnly, defaultCredentialAIRDropEdit.ReadOnly);
					AssertEquals($"DefaultCredentialSEADropEdit should be read only {readOnly} when user control is read only {registryZUserControl.ReadOnly} and current code is {defaultBrokerCodeFindBox.Text}", readOnly, forwarderManifestSEADropEdit.ReadOnly);
					AssertEquals($"DefaultCredentialSEADropEdit should be read only {readOnly} when user control is read only {registryZUserControl.ReadOnly} and current code is {defaultBrokerCodeFindBox.Text}", readOnly, forwarderManifestAIRDropEdit.ReadOnly);
				}
			}
		}
	}
}
