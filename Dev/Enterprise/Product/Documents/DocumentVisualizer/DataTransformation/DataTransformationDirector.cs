using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Common;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.DataTransformation
{
	sealed class DataTransformationDirector
	{
		public DataTransformationDirector(VersionLabel version, string dataStoreName, INotificationsHandler notificationsHandler)
		{
			Argument.NotNull(version, nameof(version));
			Argument.NotNull(notificationsHandler, nameof(notificationsHandler));

			this.version = version;
			this.dataStoreName = dataStoreName;
			this.notificationsHandler = notificationsHandler;
		}

		readonly VersionLabel version;
		readonly string dataStoreName;
		readonly INotificationsHandler notificationsHandler;

		#region SuppressResourceStringsCheckRegion

		public Try<XDocument> Transform(XDocument xml)
		{
			if (xml == null)
			{
				return Try<XDocument>.Success(xml);
			}

			var mappings = Mappings
				.Where(mapping => string.IsNullOrEmpty(mapping.Transformation.DataStoreName)
					|| string.Compare(mapping.Transformation.DataStoreName, dataStoreName, StringComparison.OrdinalIgnoreCase) == 0)
				.OrderByDescending(mapping => mapping.Version)
				.TakeWhile(mapping => mapping.Version.CompareTo(version) > 0 && mapping.Version.CompareTo(VisualizerDocumentDataVersion.DocumentData) <= 0);

			var initialInfoAdded = false;

			void AddInitialInfo()
			{
				var message = string.Format(CultureInfo.InvariantCulture,
				"Transforming '{0}' from ver. {1}.{2} to ver. {3}.{4}",
				dataStoreName,
				version.Major, version.Minor,
				VisualizerDocumentDataVersion.DocumentData.Major, VisualizerDocumentDataVersion.DocumentData.Minor);

				var notification = new Notification(new NotificationSource("Data Transformation Director"),
					NotificationType.Information,
					message);

				notificationsHandler.Add(notification);
			}

			var stopWatch = new Stopwatch();

			foreach (var mapping in mappings)
			{
				if (!initialInfoAdded)
				{
					AddInitialInfo();
					initialInfoAdded = true;
				}

				var source = new NotificationSource(mapping.Transformation.Description);

				var message = string.Format(CultureInfo.InvariantCulture, "Running '{0}'.", mapping.Transformation.Description);

				notificationsHandler.Add(new Notification(source, NotificationType.Information, message));

				stopWatch.Reset();

				try
				{
					var transformation = mapping.Transformation;

					xml = transformation.Run(xml, notificationsHandler);
				}
				catch (Exception exc) when (!exc.IsCriticalException())
				{
					return Try<XDocument>.Failure(exc);
				}

				var timeTaken = stopWatch.Elapsed;

				message = string.Format(CultureInfo.InvariantCulture, "Finished '{0}' in {1}.", mapping.Transformation.Description, timeTaken);

				notificationsHandler.Add(new Notification(source, NotificationType.Information, message));
			}

			return Try<XDocument>.Success(xml);
		}

		IEnumerable<Mapping> Mappings => Mapper.GetMappings();

		#endregion
	}
}
