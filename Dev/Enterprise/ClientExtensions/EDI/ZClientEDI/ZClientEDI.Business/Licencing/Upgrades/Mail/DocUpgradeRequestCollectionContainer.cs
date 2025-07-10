using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business
{
	public class DocUpgradeRequestCollectionContainer : DocumentWrapper
	{
		DocUpgradeRequestCollectionContainer(UpgradeRequestCollectionContainer objectToWrap, BusinessObjectFactory factory)
			: base(objectToWrap, factory)
		{ }

		public static DocUpgradeRequestCollectionContainer New(UpgradeRequestCollectionContainer objectToWrap, BusinessObjectFactory factory)
		{
			return new DocUpgradeRequestCollectionContainer(objectToWrap, factory);
		}

		public new UpgradeRequestCollectionContainer WrappedObject
		{
			get { return (UpgradeRequestCollectionContainer)base.WrappedObject; }
		}

		#region Current Processing Organisation

		[DocumentField("The client's full name")]
		public ZString ClientName
		{
			get { return WrappedObject.CurrentOrganisation != null ? WrappedObject.CurrentOrganisation.OH_FullNameTruncated : ZString.Empty; }
		}

		[DocumentField("The client's code")]
		public ZString ClientCode
		{
			get { return WrappedObject.CurrentOrganisation != null ? WrappedObject.CurrentOrganisation.OH_Code : ZString.Empty; }
		}

		#endregion

		#region Release Build

		[DocumentField("Release build description")]
		public ZString ReleaseDescription
		{
			get { return WrappedObject.SelectedReleaseBuild != null ? WrappedObject.SelectedReleaseBuild.ReleaseDisplayText : ZString.Empty; }
		}

		[DocumentField("Release build date")]
		public ZString ReleaseExeDate
		{
			get { return WrappedObject.SelectedReleaseBuild != null ? WrappedObject.SelectedReleaseBuild.HL_ExeVersionDate.ToLongTimeString() : string.Empty; }
		}

		[DocumentField("Release build version number")]
		public ZString ReleaseVersionNumber
		{
			get { return WrappedObject.SelectedReleaseBuild != null ? WrappedObject.SelectedReleaseBuild.ExeVersion : ZString.Empty; }
		}

		#endregion

		#region Upgrade Notification Message

		[DocumentField("Upgrade notification message")]
		public ZString UpgradeNotificationMessage
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();
				if (WrappedObject.UpgradesForCurrentOrganisation != null)
				{
					foreach (UpgradeRequest upgrade in WrappedObject.UpgradesForCurrentOrganisation)
					{
						builder.Append(upgrade.NotificationMessage);
					}
				}
				return builder.ToString();
			}
		}

		#endregion

		#region Additional Text

		[DocumentField("Prefix to email subject")]
		public ZString NotificationSubjectPrefix
		{
			get { return WrappedObject.NotificationSubjectPrefix; }
		}

		[DocumentField("Additional notification")]
		public ZString AdditionalNotification
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();
				if (!WrappedObject.AdditionalNotification.IsEmpty)
				{
					builder.AppendLine(WrappedObject.AdditionalNotification);
				}
				return builder.ToString();
			}
		}

		#endregion
	}
}

