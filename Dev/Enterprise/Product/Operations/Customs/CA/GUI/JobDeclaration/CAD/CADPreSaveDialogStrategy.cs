using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.CA.Business.MessageManagers;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	class CADPreSaveDialogStrategy : PreSaveDialogStrategy
	{
		public CADPreSaveDialogStrategy(JobDeclaration source)
		{
			this.entry = source.B3EntryHeader;
		}
		readonly CusEntryHeader entry;
		CADCorrectionMessageSendingActionWrapper wrapper;

		protected override bool ShouldRunPreSaveAction()
		{
			wrapper = new CADCorrectionMessageSendingActionWrapper(entry);
			wrapper.SaveWithoutSendMessage = true;
			using (var form = new CADCorrectionMessageSendingForm(wrapper))
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
				if (wrapper.OKClicked && wrapper.SaveWithoutSendMessage)
				{
					return false;
				}
				return true;
			}
		}

		protected override ContinueWithSave RunPreSaveAction()
		{
			if (wrapper.CancelClicked)
			{
				return ContinueWithSave.No;
			}
			else if (wrapper.OKClicked && wrapper.SendMessage)
			{
				wrapper.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(SendCADCorrectionMessage);
				return ContinueWithSave.Yes;
			}
			return ContinueWithSave.No;
		}

		void SendCADCorrectionMessage(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			factory.Saved -= new BusinessObjectFactory.SavedEventHandler(SendCADCorrectionMessage);
			if (savedSuccessfully)
			{
				var messageWrapper = new CADMessageWrapper(entry, wrapper.SendingActions);
				new CADMessageManager(messageWrapper, new MessageInstructionUserNotification()).SendMessage();
			}
		}
	}
}
