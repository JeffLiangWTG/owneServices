using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.IN.Manifest.Module;

sealed class MessageSendingOperationalActionMethod : OperationalActionMethod
{
	public MessageSendingOperationalActionMethod() : base(new ZGuid("119894D9-1EBA-4115-8F1D-95898F42B515"))
	{
	}

	public override OperationalActionMethodApplicator NewApplicator(BusinessObjectFactory factory, OperationalActionMethodSettings settings)
		=> new MessageSendingOperationalActionMethodApplicator();

	public override string Name => Res.GetString("23A5951E-4743-4879-913E-1C0E78F0A42D", "Send EDI message");

	public override string Description => Res.GetString("741DFE87-CFAD-41C4-A8E0-1292A40F8029", "Sign and Send EDI message");

	public override FilterRequirementList GetFilterRequirements()
	{
		var result = base.GetFilterRequirements();
		result.Add(new FilterIsConsolGeneralManifestEnabledRegistryConstraint().Name, new string[] { "Y" });
		return result;
	}

	public override IComponent NewGuiControl() => new MessageSendingOperationalActionControl();

	public override bool HasControl => true;
}
