using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class RequireReasonForCLRWrapper : RegistryBusinessObjectTemplate
	{
		public RequireReasonForCLRWrapper()
		{
		}

		public RequireReasonForCLRWrapper(RequireReasonForCLRItemCollection collection)
		{
			itemCollection = collection;
		}

		public static class Schema
		{
			public const string RequireReasonForCLR = "RequireReasonForCLR";
		}

		#region Bound Properties

		public ZBool YesRadioButtonSelection
		{
			get => RequireReasonForCLR;
			set => RequireReasonForCLR = value;
		}

		public ZBool NoRadioButtonSelection
		{
			get => !RequireReasonForCLR;
			set => RequireReasonForCLR = !value;
		}

		ZBool requireReasonForCLR;
		public ZBool RequireReasonForCLR
		{
			get => requireReasonForCLR;
			set => SetNonPersistentPropertyValue(RequireReasonForCLRInfo, ref requireReasonForCLR, value);
		}

		public ZPropertyInfo RequireReasonForCLRInfo => GetZPropertyInfo(Schema.RequireReasonForCLR);

		RequireReasonForCLRItemCollection itemCollection;
		public RequireReasonForCLRItemCollection ItemCollection
		{
			get
			{
				if (itemCollection == null)
				{
					itemCollection = new RequireReasonForCLRItemCollection();
					RegisterEditableChildObject(itemCollection);
				}

				return itemCollection;
			}
		}

		#endregion

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			if (ItemCollection != null)
			{
				((RequireReasonForCLRWrapper)clone).SetItemCollection((RequireReasonForCLRItemCollection)ItemCollection.Clone(null, null));
			}
		}

		public void SetItemCollection(RequireReasonForCLRItemCollection collection)
		{
			UnRegisterEditableChildObject(ItemCollection);
			itemCollection = collection;
			RegisterEditableChildObject(ItemCollection);
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new RequireReasonForCLRWrapper();
		}

		ZXmlSerializer RequireReasonForCLRItemCollectionSerializer => ZXmlSerializer.New(typeof(RequireReasonForCLRItemCollection));

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			RequireReasonForCLR = new ZBool(reader.ReadElementString(Schema.RequireReasonForCLR));
			var value = (RequireReasonForCLRItemCollection)RequireReasonForCLRItemCollectionSerializer.Deserialize(reader);
			ItemCollection.CopyElementValuesFrom(value);
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.RequireReasonForCLR, RequireReasonForCLR.ToString());
			RequireReasonForCLRItemCollectionSerializer.Serialize(writer, ItemCollection);
		}
	}
}
