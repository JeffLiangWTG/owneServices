using Enterprise.Customs.CA.Business;
using Enterprise.Messaging.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.GUI
{
	public class EDIMessageWithDocumentsForm : EDIMessageForm
	{
		public EDIMessageWithDocumentsForm(EDIMessage message)
			: base(message)
		{
			PlugIns.Add(ControllerIDs.DocDataPlugIn);
		}
	}
}
