using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.UPE.Registry.Business
{
	[XmlSerializerAssembly("ZClientUPE.XmlSerializers")]
	public class UPEGlbGroupsRegistryObject : RegistryBusinessObjectTemplate
	{
		public UPEGlbGroupsRegistryObject() : base() { }

		abstract class UPEGlbGroupsSchema
		{
			public const string Group = "Group";
			public const string GroupDescription = "GroupDescription";
		}

		#region Override

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new UPEGlbGroupsRegistryObject();
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(UPEGlbGroupsSchema.Group, Group.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Group = new ZGuid(reader.ReadElementString(UPEGlbGroupsSchema.Group));
		}

		#endregion

		#region Properties

		#region Group

		ZGuid group;

		public ZGuid Group
		{
			get { return group; }
			set
			{
				SetNonPersistentPropertyValue(GroupInfo, ref group, value);
				if (!IsValidationSuspended)
				{
					ValidateGroup();
				}
			}
		}

		public ZPropertyInfo GroupInfo
		{
			get { return GetZPropertyInfo(UPEGlbGroupsSchema.Group); }
		}

		void ValidateGroup()
		{
			GroupInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(GroupInfo);
			if (!Group.IsValid)
			{
				GroupInfo.AddError(ListValidation.GetNotificationMessage((string)null).ToString());
			}
		}

		public GlbGroupCollection GroupCollection
		{
			get { return groupCollection ?? (groupCollection = new GlbGroupCollection(LocalFactory)); }
		}

		GlbGroupCollection groupCollection;

		#endregion

		#region GroupDescription

		[CargoWise.ComponentModel.MaxLength(35)]
		public ZString GroupDescription
		{
			get
			{
				GlbGroup grp = LocalFactory.Load<GlbGroup>(Group);
				return grp != null ? grp.GG_Desc : ZString.Empty;
			}
		}

		public ZPropertyInfo GroupDescriptionInfo
		{
			get { return GetZPropertyInfo(UPEGlbGroupsSchema.GroupDescription); }
		}

		BusinessObjectFactory localFactory;
		BusinessObjectFactory LocalFactory
		{
			get { return localFactory ?? (localFactory = new BusinessObjectFactory()); }
		}

		#endregion

		#endregion
	}
}
