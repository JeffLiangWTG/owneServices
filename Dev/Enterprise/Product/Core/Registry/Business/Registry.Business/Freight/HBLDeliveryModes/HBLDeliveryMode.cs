using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class HBLDeliveryMode : RegistryBusinessObject, ICanDelete
	{
		protected new abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string ShowInList = "ShowInList";
		}

		public HBLDeliveryMode()
		{
		}

		public HBLDeliveryMode(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		#region Property

		HBLDeliveryModeCollection ParentCollection
		{
			get { return (HBLDeliveryModeCollection)GetParentCollection(this, typeof(HBLDeliveryModeCollection)); }
		}

		public ZBool ShowInList
		{
			get { return showInList; }
			set
			{
				SetNonPersistentPropertyValue(ShowInListInfo, ref showInList, value);

				if (!IsValidationSuspended)
				{
					ValidateShowInList();
				}
			}
		}

		public ZPropertyInfo ShowInListInfo
		{
			get { return GetZPropertyInfo(Schema.ShowInList); }
		}
		ZBool showInList;

		#endregion

		#region Read Only Members

		public bool Code_ReadOnly
		{
			get { return true; }
		}

		protected override int CodeMaxLengthDefaultValue
		{
			get { return 9; }
		}

		public bool Description_ReadOnly
		{
			get { return true; }
		}

		public bool EnglishDescription_ReadOnly
		{
			get { return true; }
		}

		protected override int MaxDescriptionLength
		{
			get { return 256; }
		}

		#endregion

		#region XML Serialisation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new HBLDeliveryMode();
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ShowInList, ShowInList.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			ShowInList = reader.ReadElementStringAsZBool(Schema.ShowInList);
		}

		#endregion

		#region validation

		public void ValidateShowInList()
		{
			ShowInListInfo.ClearAllNotifications();

			if (ParentCollection != null)
			{
				var itemShowInList = ParentCollection.Cast<HBLDeliveryMode>().FirstOrDefault(x => x.ShowInList);
				if (itemShowInList == null)
				{
					ShowInListInfo.AddError(Res.GetString("6bdd8709-8e94-44c4-8632-1200355c065d", "At least one item must be 'Show In List'."));
				}
			}
		}

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get { return false; }
		}

		#endregion
	}
}
