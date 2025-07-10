using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.GUI.MessageSending;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI
{
	class CDSSendsMessagesToCustomsGUI : SendsMessagesToCustomsGUI, IEnhancedValidation
	{
		public bool ShowEnhancedValidation(IEnhancedValidationEntryWrapper headerWrapper)
		{
			using var form = new EnhancedValidationForm(headerWrapper);
			return ZFormModaliser.ShowDialogWithoutDispose(form) == System.Windows.Forms.DialogResult.OK;
		}
	}
}
