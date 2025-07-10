using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Enterprise.DocumentVisualizer.Core;

namespace Enterprise.DocumentVisualizer.DataTransformation
{
	#region SuppressResourceStringsCheckRegion

	sealed class SeaBookingRequestV1ToV2Transformation : XsltDataTransformation
	{
		public override string Description => "Transform the SeaBookingRequest data from V1 to V2.";

		public override string DataStoreName => "SeaBookingRequest";

		protected override string GetXslt()
		{
			var provider = new XsltProvider();
			return provider.GetXsltFromFile("SeaBookingRequestV1ToV2");
		}

		protected override void OnAfterRun(XDocument input, XDocument xslt, XDocument result, INotificationsHandler handler)
		{
			var untransformableElements = new[]
			{
				new
				{
					Name = "Container - TareWeight",
					XPath = "//Property[@Name='TareWeightConverted']"
				},
				new
				{
					Name = "Package - Weight",
					XPath = "//Property[@Name='WeightConverted']"
				},
				new
				{
					Name = "Package - Volume",
					XPath = "//Property[@Name='VolumeConverted']"
				},
				new
				{
					Name = "Package - Packs",
					XPath = "//Property[@Name='PackQty']"
				}
			};

			var elementsThatHaveNotBeenTransformed = untransformableElements
				.Where(elem => input.XPathSelectElement(elem.XPath) != null)
				.Select(elem => elem.Name)
				.ToArray();

			if (elementsThatHaveNotBeenTransformed.Any())
			{
				var source = new NotificationSource(DataStoreName);
				var message = string.Format(CultureInfo.InvariantCulture,
					"Some fields were overridden by you, but are no longer able to remain overridden for compliance purposes. Please check the following fields: {0}.",
					string.Join(", ", elementsThatHaveNotBeenTransformed));

				handler.Add(new Notification(source, NotificationType.Warning, message));
			}
		}
	}

	#endregion
}
