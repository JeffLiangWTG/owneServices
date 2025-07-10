using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.JP.Business
{
	public class MSXMessageSendingObject : JobDeclarationMessageSendingObject
	{
		public MSXMessageSendingObject(CusEntryHeader header) : base(header)
		{
		}

		public new CusEntryHeader Header => (CusEntryHeader)base.Header;

		#region Attachments

		[ChildEditable(true)]
		public MSXMessageSendingObjectAttachmentCollection Attachments
		{
			get
			{
				if (attachments == null)
				{
					attachments = new MSXMessageSendingObjectAttachmentCollection(this);
					RegisterEditableChildObject(attachments);
				}
				return attachments;
			}
		}

		MSXMessageSendingObjectAttachmentCollection attachments;

		#endregion

		[ResourceStringData("433C7704-EBB8-4F77-8AE1-ADCD4E6D8EC0", Caption = "Registration Type")]
		public ZBool RegistrationType
		{
			get => registrationType;
			set
			{
				SetNonPersistentPropertyValue(RegistrationTypeInfo, ref registrationType, value);
			}
		}

		ZBool registrationType;

		public ZPropertyInfo RegistrationTypeInfo => GetZPropertyInfo(nameof(RegistrationType));

		[ResourceStringData("63A33E13-63AA-45D3-9C43-5103791787F9", Caption = "Communication")]
		public ZString Communication
		{
			get => communication;
			set
			{
				SetNonPersistentPropertyValue(CommunicationInfo, ref communication, value);
			}
		}

		ZString communication;

		public ZPropertyInfo CommunicationInfo => GetZPropertyInfo(nameof(Communication));

		public IEnumerable<IStorageDocsBaseCollection> EDocCollections
		{
			get
			{
				var declaration = Header.Declaration;

				if (declaration != null)
				{
					foreach (var eDocCollection in EDocsHelper.GetEDocCollections(declaration))
					{
						yield return eDocCollection;
					}
				}
			}
		}
	}
}
