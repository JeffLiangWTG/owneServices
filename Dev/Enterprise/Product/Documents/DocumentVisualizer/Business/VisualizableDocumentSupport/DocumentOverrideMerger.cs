using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DataTransformation;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.Business
{
	public sealed class DocumentOverrideMerger
	{
		public DocumentOverrideMerger(IServiceContainer services, IVisualizerDocumentData documentData)
		{
			Argument.NotNull(services, nameof(services));
			Argument.NotNull(documentData, nameof(documentData));

			this.services = services;
			this.documentData = documentData;
		}

		readonly IServiceContainer services;
		readonly IVisualizerDocumentData documentData;

		public Either<string, IDocument> MergeOverride(IDocument document)
		{
			Argument.NotNull(document, nameof(document));

			var res = ReadUserOverride()
				.Then(TransformUserOverride);

			if (res.IsLeft)
			{
				return res.Left;
			}

			try
			{
				document.Data.MergeDataFromXml(res.Right);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return ex.Message;
			}

			return new Either<string, IDocument>(document);
		}

		public Either<string, IDocument> ApplyOverride(IDocument document)
		{
			Argument.NotNull(document, nameof(document));

			var res = ReadUserOverride()
				.Then(TransformUserOverride);

			if (res.IsLeft)
			{
				return res.Left;
			}

			try
			{
				document.Data.ApplyDataFromXml(res.Right);
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				return ex.Message;
			}

			return new Either<string, IDocument>(document);
		}

		Either<string, XDocument> ReadUserOverride()
		{
			XDocument xml = null;

			try
			{
				xml = documentData.ReadXml();
			}
			catch (XmlException ex)
			{
				Task.Run(() => ErrorReporter.ReportOnce("FB-FailedToReadXmlOverride", ex));

				var message = Res.GetString("e9cc5339-5b66-4e0d-bba0-c76187a12148", "There has been a problem creating document data. WTG support has been informed about this problem. In the meantime, you can proceed however all overrides will be lost. Proceed?");

				if (!services.Resolve<IUserNotificationService>().ShowConfirmation(message, Res.GetString("1b7c1fc9-109b-4192-825d-210e9a90950e", "Confirmation")))
				{
					return Res.GetString("b912f1b5-4a2a-4876-97be-57d794b5a5e7", "There was an error reading data override.");
				}
			}

			return xml;
		}

		Either<string, XDocument> TransformUserOverride(XDocument xml)
		{
			if (xml == null
				|| !xml.RequiresTransformation())
			{
				return xml;
			}

			var xmlTransformNotificationHandler = new XmlTransformNotificationsHandler();

			var tryTransformOverride = xml.RunTransformation(
				documentData.Name,
				xmlTransformNotificationHandler);

			if (tryTransformOverride.IsFaulted)
			{
				var message = Res.GetString("8e2f2347-1b3d-4860-99da-5813ea997f2d", "There has been a problem transforming document data to the latest version. You can proceed however once you save all overrides will be lost. Proceed?");

				if (!services.Resolve<IUserNotificationService>().ShowConfirmation(message, Res.GetString("89b04e30-4e70-42e0-832c-cefee58fcb7c", "Confirmation")))
				{
					return Res.GetString("dd4b7899-4559-4d5b-878e-a817cdad0237", "There was an error transforming data override.");
				}

				AddTransformationNote(xmlTransformNotificationHandler.Notifications, xml, null);

				return (XDocument)null;
			}

			var transformedXml = tryTransformOverride.Value;
			var notifications = xmlTransformNotificationHandler
				.Notifications
				.ToArray();

			if (notifications.Length > 0)
			{
				var warningsAndErrors = notifications
					.Where(n => n.Type == NotificationType.Warning
						|| n.Type == NotificationType.Error)
					.ToArray();

				ShowNotificationsToUser(warningsAndErrors);
				AddTransformationNote(notifications, xml, transformedXml);
			}

			return transformedXml;
		}

		void ShowNotificationsToUser(IReadOnlyCollection<INotification> notifications)
		{
			if (notifications.Count == 0)
			{
				return;
			}

			var builder = services.Resolve<INotificationViewBuildService>();

			var notificationView = builder.CreateNotificationView(
				(NoResString)"transformation",
				Res.GetString("01ad2998-3950-45e0-9df5-78e658a1d361", "Overrides have been transformed to the latest version. Click here to view details."),
				NotificationType.Warning,
				() => services.Resolve<IUserNotificationService>().ShowNotifications(notifications));

			services.Resolve<IDocumentView>().NotificationViews.Add(notificationView);
		}

		#region SuppressResourceStringsCheckRegion

		void AddTransformationNote(IEnumerable<INotification> transformNotifications, XDocument original, XDocument transformed)
		{
			if (!(documentData is IStmNoteParent noteParent))
			{
				return;
			}

			var noteTextLines = new List<string>();
			noteTextLines.AddRange(transformNotifications.Select(n => string.Format(CultureInfo.InvariantCulture, "[{0}] {1}", n.Type, n.Message)));
			noteTextLines.Add("Original xml:");
			noteTextLines.Add(original.ToXmlString());
			noteTextLines.Add("Transformed xml:");
			noteTextLines.Add(transformed?.ToXmlString() ?? "---none---");

			var note = noteParent.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.DocumentDataTransformationNote.Description;
			note.ST_NoteDataAsText = string.Join(System.Environment.NewLine, noteTextLines);
		}

		#endregion

		#region Nested Types

		sealed class XmlTransformNotificationsHandler : INotificationsHandler, INotificationProvider
		{
			readonly IList<INotification> notifications = new List<INotification>();

			public void Add(INotification notification)
			{
				if (notification == null)
				{
					return;
				}

				notifications.Add(notification);
			}
			public IEnumerable<INotification> Notifications => notifications;
		}

		#endregion
	}
}
