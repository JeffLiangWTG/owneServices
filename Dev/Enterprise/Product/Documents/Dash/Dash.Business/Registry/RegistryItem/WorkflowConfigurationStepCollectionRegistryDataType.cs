using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Dash.Business
{
	[RegistryEditor("Enterprise.Dash.GUI.WorkflowConfigurationStepCollectionRegistryItemEditor, Enterprise.Dash.GUI")]
	public class WorkflowConfigurationStepCollectionRegistryDataType : NonPersistentBusinessObjectRegistryDataType<WorkflowConfigurationStepCollection>
	{
		internal readonly CodeDescriptionPairListProvider codesProvider;

		public WorkflowConfigurationStepCollectionRegistryDataType(CodeDescriptionPairListProvider codesProvider)
			: base(new WorkflowConfigurationStepCollection(codesProvider))
		{
			this.codesProvider = codesProvider;
		}

		protected override WorkflowConfigurationStepCollection DeserialiseCore(byte[] value)
		{
			var result = base.DeserialiseCore(value);
			result.SetCodesProvider(codesProvider);
			return result;
		}
	}
}
