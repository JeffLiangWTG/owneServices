using System.Collections.Generic;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	sealed class DataEditing : CommandProvider
	{
		readonly Command resetOverriddenData = new Command(CommandIds.ResetOverriddenData, Res.GetString("4086E265-D011-4883-8FDE-AA0311850A3F", "Reset"));
		readonly Command saveOverriddenData = new Command(CommandIds.SaveOverriddenData, Res.GetString("322c150f-6aad-4956-94d6-914487e7da15", "Save"));

		protected override IEnumerable<ICommand> CreateCommandsCore()
		{
			yield return resetOverriddenData;
			yield return saveOverriddenData;
		}

		protected override void OnDocumentInfoCreatedCore(IDocumentInfo documentInfo)
		{
			resetOverriddenData.Invoker   = _ => ResetOverriddenData(documentInfo.Services, documentInfo.Document, documentInfo.Descriptor);
			resetOverriddenData.IsEnabled = () => IsResetOverrideEnabled(documentInfo.Document);

			saveOverriddenData.Invoker   = _ => SaveOverridenData(documentInfo.Services, documentInfo.Document, documentInfo.Descriptor);
			saveOverriddenData.IsEnabled = () => IsSaveOverriddenDataEnabled(documentInfo.Document, documentInfo.Descriptor);
		}

		void ResetOverriddenData(IServiceContainer services, IDocument document, IDocumentDescriptor descriptor)
		{
			var security = services.Resolve<IDocumentSecurityService>();

			if (!security.CanModify)
			{
				security.ShowModifyError();
				return;
			}

			if (document?.Data != null
				&& document.Data.IsOverriddenIncludingChildren)
			{
				document.Data.CancelChanges();
				services.Resolve<IEventBroker>().Publish(new ResetEvent(document, descriptor.DocumentData.Name));
			}
		}

		void SaveOverridenData(IServiceContainer services, IDocument document, IDocumentDescriptor descriptor)
		{
			if (!descriptor.DocumentData.HasChanges
				&& (document.Data == null || !document.Data.HasChanges))
			{
				services.Resolve<IUserNotificationService>().ShowMessage(
					Res.GetString("ecb45819-aff1-4c49-b468-00f722bbdc75", "This document does not have any changes."),
					Res.GetString("b4d3b7ba-b584-4d37-8ded-76c016a46199", "Message"));
			}
			else
			{
				var saver = new DocumentDataSaver(
					services,
					document,
					descriptor.DocumentData);

				saver.Save();
			}
		}

		bool IsSaveOverriddenDataEnabled(IDocument document, IDocumentDescriptor descriptor)
		{
			return document?.Data != null && document.Data.HasChanges
				|| descriptor?.DocumentData != null && descriptor.DocumentData.HasChanges;
		}

		bool IsResetOverrideEnabled(IDocument document)
		{
			return document?.Data != null
				&& document.Data.IsOverriddenIncludingChildren;
		}
	}
}
