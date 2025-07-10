using System;
using CargoWise.EntityFramework;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class DocumentDataSaver
	{
		public DocumentDataSaver(IServiceContainer services, IDocument document, IVisualizerDocumentData documentData)
		{
			this.services = services ?? throw new ArgumentNullException(nameof(services));
			this.document = document ?? throw new ArgumentNullException(nameof(document));
			this.documentData = documentData ?? throw new ArgumentNullException(nameof(documentData));
		}

		readonly IDocument document;
		readonly IServiceContainer services;
		readonly IVisualizerDocumentData documentData;

		public void Save()
		{
			if (!(document.Data is IDynamicData data))
			{
				return;
			}

			var xml = data.GetOverriddenValuesXml();

			documentData.WriteXml(xml);

			try
			{
				documentData.Save();

				data.MergeDataFromXml(xml);

				data.HasChanges = false;

				services.Resolve<IEventBroker>().Publish(new SavedEvent(document, documentData.Name));
			}
			catch (ZSaveConcurrencyException)
			{
				documentData.ReloadSafe();

				var savedXml = documentData.ReadXml();

				data.MergeDataFromXml(savedXml);

				var lastEditUserName = documentData.GetSystemLastEditUserName();

				services
					.Resolve<IUserNotificationService>()
					.ShowMessage(
						Res.GetString("2fcc0e2d-6b65-4ba3-b70c-a9c6bcd5b37e", "While you were working on this form {0} has made some changes and as a result of that you could have lost some of your changes.\r\n\r\nPlease review the form before saving.", lastEditUserName),
						Res.GetString("5e13d1ee-4eb8-497d-985d-e16c0e33634d", "Saving failed..."));
			}
			catch (ZSaveException)
			{
				services
					.Resolve<IUserNotificationService>()
					.ShowMessage(
						Res.GetString("bccef35d-6d98-47ec-b4c1-c7b09adf31f8", "Your changes have not been saved because an error has occurred. Please close and then reopen the form."),
						Res.GetString("fb3cbb39-9a7c-4f83-b488-3a0b2ef8d164", "Saving failed..."));
			}
		}
	}
}
