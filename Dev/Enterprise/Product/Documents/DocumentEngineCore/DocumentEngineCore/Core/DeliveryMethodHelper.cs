using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.DocumentEngineCore
{
	public static class DeliveryMethodHelper
	{
		public static bool IsEmailOrEPrint(string deliveryMethod)
		{
			return deliveryMethod == Core.Constants.ContactNotifyModes.Email || deliveryMethod == Core.Constants.ContactNotifyModes.EPrint;
		}

		public static bool IsEDoc(string deliveryMethod)
		{
			return deliveryMethod == ContactNotifyModes.EDoc;
		}

		public static bool IsEmail(string deliveryMethod)
		{
			return deliveryMethod == ContactNotifyModes.Email;
		}

		public static string GetEmailAttachmentOutOfSizeLimitError(IEnumerable<IDeliveryEmailAttachment> attachments)
		{
			var result = string.Empty;

			if (attachments != null)
			{
				long registrySizeLimitInBytes = (long)SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.Value * 1024 * 1024;

				var outOfLimitAttachmentNames =
					attachments.Where(a => a?.FileSizeInBytes > registrySizeLimitInBytes && a.ShouldBeAttached).Select(a => a.FileName);

				var attachmentNames = outOfLimitAttachmentNames as IList<string> ?? outOfLimitAttachmentNames.ToList();
				if (attachmentNames.Any())
				{
					result = Res.GetString("68D14D42-A8BA-4F10-A3A2-ACF8D6247204",
						"One or more eDoc files exceeds the {0}MB attachment limit and cannot be sent: {1}. The limit is defined in the Registry at {2}.",
						SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB.Value,
						attachmentNames.Aggregate((x, y) => x + ", " + y),
						((IRegistryItemInternals)SystemDataRegistry.Instance.EmailAttachmentSizeLimitInMB).Location
					);
				}
			}

			return result;
		}

		public static IEnumerable<string> GetSupportedDelvieryMethodsFor(PrintCopyType mode)
		{
			switch (mode)
			{
				case PrintCopyType.ALL: return ContactNotifyModes.All;
				case PrintCopyType.PRN: return new[] { ContactNotifyModes.Print, ContactNotifyModes.EPrint };
				case PrintCopyType.EML: return new[] { ContactNotifyModes.Email };
				case PrintCopyType.FAX: return new[] { ContactNotifyModes.Fax };
				default: return Enumerable.Empty<string>();
			}
		}
	}
}
