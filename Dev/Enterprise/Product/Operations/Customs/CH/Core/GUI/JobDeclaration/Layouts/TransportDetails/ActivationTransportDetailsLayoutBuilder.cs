using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public class ActivationTransportDetailsLayoutBuilder : ColumnLayoutBuilder<MessageSendingDeclaration, Customs.GUI.TransportDetailsControlBag>
{
	protected override int MaxColumns => 1;

	public override Customs.GUI.TransportDetailsControlBag CommonBag { get; } = Customs.GUI.TransportDetailsControlBag.Instance;
}
