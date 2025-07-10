using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.H7.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.H7.GUI
{
	public partial class UploadDocumentsForm : Customs.GUI.MessageSendingFormWithValidationDetails
	{
		public UploadDocumentsForm(BaseMessageSendingObjectParent parent)
			: base(parent)
		{
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormHeading => Res.GetString("df2f557a-524a-4dd7-968b-63933316ef08", "Upload Documents");

		protected override bool SendWithValidationErrorsCheckBoxVisible => false;

		protected override bool PreviewMessageCheckboxVisible => true;

		void SelectAllButton_Click(object sender, System.EventArgs e)
		{
			var shouldSelectAll = MessageSendingObjectParent.SendingObjectsCollection.Count != MessageSendingObjectParent.SelectedSendingObjects.Count();

			foreach (var messageSendingObject in MessageSendingObjectParent.SendingObjectsCollection.Cast<BaseMessageSendingObject>())
			{
				messageSendingObject.ShouldSend = shouldSelectAll;
			}
		}

		protected override ZUserControl GetBottomSectionUserControl()
		{
			return new UploadDocumentsAdditionalInfoUserControl();
		}

		protected override void AddUserControlToBottomSection()
		{
			var bottomSectionUserControl = GetBottomSectionUserControl();
			BindingSource.SetBindingMember(bottomSectionUserControl, "UploadDocumentsSendingActionFilteredCollection");
			bottomSectionUserControl.Dock = DockStyle.Fill;
			SplitContainer.Panel2.Controls.Add(bottomSectionUserControl);
		}

		protected override void SendButton_ClickCore()
		{
			base.SendButton_ClickCore();

			var messageSentCount = SendMessageToCustoms();
			if (messageSentCount > 0)
			{
				TrySaveAndShowMessage(BusinessEntity.Factory, messageSentCount);
			}
		}

		protected virtual int SendMessageToCustoms()
		{
			var messagesSent = 0;

			foreach (MessageSendingObject sendingObject in BusinessEntity.SelectedSendingObjects)
			{
				var sender = sendingObject.CreateSender();
				if (sender?.Send() != null)
				{
					messagesSent++;
				}
			}

			return messagesSent;
		}

		void TrySaveAndShowMessage(BusinessObjectFactory factory, int messagesCreated)
		{
			try
			{
				factory.Save();
				var message = Res.GetString("855a7820-b89f-473e-942d-9855f976d88b", "{0} message(s) queued for sending.", messagesCreated);
				Globals.Message.ShowInformation(message);
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
		}

		FilterStripBusinessObject GetUploadDocumentActionFilterBusinessObject()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				var filterBizo = GetUploadDocumentActionFilterBusinessObjectCore();
				ConfigureFilters(filterBizo);
				return filterBizo;
			}

			return null;
		}

		void ConfigureFilters(FilterStripBusinessObject filterBizo)
		{
			((IFilterStripBusinessObjectInternals)filterBizo).LayoutContext = "Messages to sent";
			filterBizo.ModuleFilters.First(f => f.Description == defaultFilterDescription).Visibility = FilterVisibility.AlwaysVisible;

			foreach (var filter in filterBizo.ModuleFilters)
			{
				if (!AvailableFiltersDescriptions.Contains(filter.Description))
				{
					filter.Visible = false;
				}
			}
		}

		protected virtual FilterStripBusinessObject GetUploadDocumentActionFilterBusinessObjectCore() =>
			ObjectFactory.Get<FilterStripBusinessObject>("EUH7BillFilterBusinessObject");

		void FilterStripControl_PerformSearch(object sender, PerformSearchEventArgs e)
		{
			if(MessageSendingObjectParent is IMessageSendingObjectFilteredCollectionProvider provider)
			{
				provider.MessageSendingObjectFilteredCollection.Load(filterStripControl?.FilterBusinessObject?.Filter);
			}
		}

		protected override IGridColumnLayoutProvider MessageSendingGridColumnLayoutProvider => messageSendingGridColumnLayoutProvider ??= GetNewColumnLayoutProvider();
		IGridColumnLayoutProvider messageSendingGridColumnLayoutProvider;

		protected virtual IGridColumnLayoutProvider GetNewColumnLayoutProvider() => new UploadDocumentsGridColumnLayout();

		protected virtual List<string> AvailableFiltersDescriptions => availableFiltersDescriptions ??= [(NoResString)"Bill Number", "MRN", "LRN", (NoResString)"Customs Status"];
		List<string> availableFiltersDescriptions;

		readonly ZString defaultFilterDescription = (NoResString)"Customs Status";
		ZFilterStripBaseControl filterStripControl;
	}
}
