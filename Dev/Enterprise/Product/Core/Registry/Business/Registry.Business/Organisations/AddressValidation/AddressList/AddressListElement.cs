using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class AddressListElement : RegistryBusinessObject
	{
		#region Schema

		public new static class Schema
		{
			public const string AddressType = nameof(AddressType);
			public const string ControllerName = nameof(ControllerName);
		}

		#endregion

		#region Lookups

		public AddressTypeList AddressTypes => new AddressTypeList();

		public ControllerIDsList Controllers => new ControllerIDsList();

		#endregion

		#region RegistryBusinessObject Overrides

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new AddressListElement();

		protected override bool IsCodeMandatory => false;

		#endregion

		#region Related Entities

		#region Parent

		[BusinessObjectTestExclude]
		public AddressListCollection Parent => (AddressListCollection)GetParentCollection(this, typeof(AddressListCollection));

		#endregion

		#endregion

		#region Properties

		#region AddressType

		[MaxLength(50)]
		[List(nameof(AddressTypes))]
		[ResourceStringData("ff1242d6-8470-4d53-a6e9-2ccc31dafdf5", Caption = "Address Type")]
		public ZString AddressType
		{
			get { return addressType; }
			set
			{
				SetNonPersistentPropertyValue(AddressTypeInfo, ref addressType, value);
				if (!IsValidationSuspended)
				{
					ValidateAddressType();
				}
			}
		}

		public ZPropertyInfo AddressTypeInfo => GetZPropertyInfo(Schema.AddressType);

		ZString addressType;

		#endregion

		#region Controller

		[MaxLength(50)]
		[List(nameof(Controllers))]
		[ResourceStringData("4e6bf867-2cca-48fd-87df-3518238c6b85", Caption = "Controller")]
		public ZString ControllerName
		{
			get { return controllerName; }
			set
			{
				SetNonPersistentPropertyValue(ControllerNameInfo, ref controllerName, value);
				if (!IsValidationSuspended)
				{
					ValidateControllerName();
				}
			}
		}

		public ZPropertyInfo ControllerNameInfo => GetZPropertyInfo(Schema.ControllerName);

		ZString controllerName;

		#endregion

		#endregion

		#region Validation

		public void ValidateAddressType()
		{
			AddressTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(AddressTypeInfo);
			ListValidation.ErrorIfInvalidCode(AddressTypeInfo);
		}

		void ValidateControllerName()
		{
			ControllerNameInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ControllerNameInfo);
			ListValidation.ErrorIfInvalidCode(ControllerNameInfo);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateAddressType();
			ValidateControllerName();
		}

		#endregion

		#region Xml Serialisation

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);

			AddressType = reader.ReadElementString(Schema.AddressType);
			ControllerName = reader.ReadElementString(Schema.ControllerName);
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);

			writer.WriteElementString(Schema.AddressType, AddressType);
			writer.WriteElementString(Schema.ControllerName, ControllerName);
		}

		#endregion
	}
}
