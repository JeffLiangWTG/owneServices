using Enterprise.Customs.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class AUImportDiscardedMessageUserControl : CustomsEntryAndDiscardedMessagesUserControl
	{
		public AUImportDiscardedMessageUserControl()
		{
			InitializeComponent();
		}

		protected override BaseCustomsEntryUserControl GetMessageUserControl()
		{
			ImportMessageUserControl result = null;

			if (JobDeclaration is Business.JobDeclaration auDeclaration && (auDeclaration.IsImportCMR || (auDeclaration.IsEXPDeclaration && auDeclaration.DeclarationExportCusEntryNumber == null)))
			{
				result = new AUCMRMessageUserControl();
			}
			else
			{
				result = new AUImportMessageUserControl();
			}

			return result;
		}
	}
}
