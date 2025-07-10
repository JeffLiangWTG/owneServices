using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.Manifest.H7.GUI
{
	public sealed partial class UploadDocumentsForm : EU.H7.GUI.UploadDocumentsForm
	{
		public UploadDocumentsForm(BaseMessageSendingObjectParent parent) : base(parent)
		{
		}

		protected override int SendMessageToCustoms()
		{
			var messageSent = 0;
			var messageSendingObjects = BusinessEntity.SelectedSendingObjects.Cast<UploadDocumentsSendingAction>();

			var messageSender = new H7MessageSender(messageSendingObjects);
			var messageBuildersToSend = messageSender.GetMessageBuildersData();

			try
			{
				var messagesInfo = messageSender.Send(messageBuildersToSend);
				messageSent = messagesInfo.MessagesSent;
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.Show(CouldNotCreateMessageError);
			}

			return messageSent;
		}

		static ZString CouldNotCreateMessageError => Res.GetString("e26ee280-a869-414a-8239-65f0e3e6fd11", "Could not create declaration message for bill(s).");

		protected override IGridColumnLayoutProvider MessageSendingGridColumnLayoutProvider => messageSendingGridColumnLayoutProvider ??= GetNewColumnLayoutProvider();
		IGridColumnLayoutProvider messageSendingGridColumnLayoutProvider;

		protected override IGridColumnLayoutProvider GetNewColumnLayoutProvider() => new UploadDocumentsGridColumnLayout();

		protected override FilterStripBusinessObject GetUploadDocumentActionFilterBusinessObjectCore() => ObjectFactory.Get<FilterStripBusinessObject>("ESH7BillFilterBusinessObject");

		protected override List<string> AvailableFiltersDescriptions =>
		[
			(NoResString)"Bill Number", (NoResString)"MRN (H7)", (NoResString)"LRN (G3)", (NoResString)"MRN (G3)", (NoResString)"Customs Status"
		];
	}
}
