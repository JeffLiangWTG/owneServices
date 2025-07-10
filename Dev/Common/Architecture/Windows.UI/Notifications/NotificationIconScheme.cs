using System;
using System.Collections.Concurrent;
using System.Drawing;
using CargoWise.ComponentModel;

namespace CargoWise.Windows.UI
{
	/// <summary>
	/// A central repository for rendering information for notification types.
	/// </summary>
	public class NotificationIconScheme
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		protected NotificationIconScheme()
		{
			SetIcon(NotificationType.Error, InternalIcons.GetIcon(InternalIconTypes.Error));
			SetIcon(NotificationType.Warning, InternalIcons.GetIcon(InternalIconTypes.Warning));
		}

		public static NotificationIconScheme Instance
		{
			get { return instance.Value; }
		}
		[Common.Testing.SuppressThreadStaticFieldMessage]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021", Justification = "Safely handled, required for forms on other threads")]
		static readonly Lazy<NotificationIconScheme> instance = new Lazy<NotificationIconScheme>(() => new NotificationIconScheme());

		#region GetIcon / GetImage / SetIcon

		/// <summary>
		/// Get the notification icon for an INotificationType.
		/// </summary>
		public Icon GetIcon(INotificationType notificationType)
		{
			Icon result;
			if (!Icons.TryGetValue(notificationType, out result))
			{
				result = SystemIcons.Information;
			}
			return result;
		}

		/// <summary>
		/// Get the notification image for an INotificationType.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess", Justification = "It's fine if the state changes in the mean time, just means we do the same work twice.")]
		public Image GetImage(INotificationType notificationType)
		{
			Image image = null;
			if (!Images.TryGetValue(notificationType, out image))
			{
				Icon icon = GetIcon(notificationType);
				image = icon.ToBitmap();
				Images[notificationType] = image;
			}
			return image;
		}

		/// <summary>
		/// Set the notification icon for an INotificationType.
		/// </summary>
		public void SetIcon(INotificationType notificationType, Icon icon)
		{ Icons[notificationType] = icon; }

		#endregion

		#region GetMiniImage / GetMiniImage / SetMiniIcon

		/// <summary>
		/// Get the small notification icon for an INotificationType.
		/// </summary>
		public Image GetMiniImage(INotificationType notificationType)
		{
			Image result;
			if (!MiniImages.TryGetValue(notificationType, out result))
			{
				result = SystemIcons.Information.ToBitmap();
			}
			return result;
		}

		/// <summary>
		/// Set the small notification icon for an INotificationType.
		/// </summary>
		public void SetMiniImage(INotificationType notificationType, Image image)
		{ MiniImages[notificationType] = image; }

		#endregion

		#region Implementation

		ConcurrentDictionary<INotificationType, Icon> Icons
		{
			get { return icons ?? (icons = new ConcurrentDictionary<INotificationType, Icon>()); }
		}
		ConcurrentDictionary<INotificationType, Icon> icons;

		ConcurrentDictionary<INotificationType, Image> Images
		{
			get { return images ?? (images = new ConcurrentDictionary<INotificationType, Image>()); }
		}
		ConcurrentDictionary<INotificationType, Image> images;

		ConcurrentDictionary<INotificationType, Image> MiniImages
		{
			get { return miniImages ?? (miniImages = new ConcurrentDictionary<INotificationType, Image>()); }
		}
		ConcurrentDictionary<INotificationType, Image> miniImages;

		#endregion
	}
}
