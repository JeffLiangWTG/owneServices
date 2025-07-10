using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Customs
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ManifestGroupNotification : GroupNotification
	{
		#region Schema

		public new class Schema  : GroupNotification.Schema
		{
			public const string SendErrorOnly = "SendErrorOnly";
		}

		#endregion

		public ManifestGroupNotification()
		{
		}

		public ManifestGroupNotification(ZString sendMode, ZGuid sendGroupPK, ZBool sendErrorOnly)
			: base(sendMode, sendGroupPK)
		{
			this.SendErrorOnly = sendErrorOnly;
		}

		public ManifestGroupNotification(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region SendErrorOnly

		public ZBool SendErrorOnly
		{
			get { return SendErrorOnly_ReadOnly ? ZBool.False : fSendErrorOnly; }
			set { SetNonPersistentPropertyValue(SendErrorOnlyInfo, ref fSendErrorOnly, value); }
		}
		ZBool fSendErrorOnly;

		public ZPropertyInfo SendErrorOnlyInfo
		{
			get { return GetZPropertyInfo(Schema.SendErrorOnly); }
		}

		bool SendErrorOnly_ReadOnly => SendMode == Core.Constants.EmailTo.NoEmails;

		#endregion

		public new static ManifestGroupNotification Default
		{
			get { return new ManifestGroupNotification(Core.Constants.EmailTo.StaffMemberAndNominatedGroup, Core.Constants.Groups.PostMastersGroupPK, false); }
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ManifestGroupNotification(fallbackLevel, factory);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			SendErrorOnly = reader.ReadElementStringAsZBool(Schema.SendErrorOnly);
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.SendErrorOnly, SendErrorOnly.ToString());
		}
	}
}
