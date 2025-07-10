using System;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Customs
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public abstract class PackageTypePair : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string CustomsPackageType = "CustomsPackageType";
			public const int CustomsPackageTypeMaxLength = 3;
			public const string CustomsPackageTypeDescription = "CustomsPackageTypeDescription";
			public const string FreightPackageType = "FreightPackageType";
			public const int FreightPackageTypeMaxLength = 3;
			public const string FreightPackageTypeDescription = "FreightPackageTypeDescription";
		}

		#endregion

		#region Properties

		#region FreightPackageTypeDescription

		public ZString FreightPackageTypeDescription
		{
			get
			{
				var type = ObjectFactory.GetType<IRefPackTypeCollection>();
				var collection = (IRefPackTypeCollection)Activator.CreateInstance(type, RegistryFactory.Instance);
				var description = collection.GetDescriptionFromCode(FreightPackageType);
				return description;
			}
		}

		public ZPropertyInfo FreightPackageTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.FreightPackageTypeDescription); }
		}

		#endregion

		#region FreightPackageType

		[List("FreightPackageTypesList")]
		[MaxLength(Schema.FreightPackageTypeMaxLength)]
		public ZString FreightPackageType
		{
			get { return freightPackageType; }
			set
			{
				SetNonPersistentPropertyValue(FreightPackageTypeInfo, ref freightPackageType, value.ToUpper());
				if (!IsValidationSuspended)
				{
					ValidateFreightPackageType();
				}
			}
		}
		ZString freightPackageType;

		public ZPropertyInfo FreightPackageTypeInfo
		{
			get { return GetZPropertyInfo(Schema.FreightPackageType); }
		}

		public void ValidateFreightPackageType()
		{
			FreightPackageTypeInfo.ClearAllNotifications();
			CheckFreightPackageType();
			CheckUniqueness(FreightPackageTypeInfo);
		}

		protected virtual void CheckFreightPackageType()
		{
			MandatoryValidation.CheckEntered(FreightPackageTypeInfo);
			ListValidation.ErrorIfInvalidCode(FreightPackageTypeInfo, FreightPackageTypesList, FreightPackageTypeShouldBeInList);
		}
		internal static IMultilingualString FreightPackageTypeShouldBeInList
		{
			get { return ResString.GetMultilingualString("4737ebdd-7ec4-4d8d-a2e6-8907eafa1eb6", "Freight Package Type. Freight Package Type should be in Freight Package Type list"); }
		}

		public CodeDescriptionPairList FreightPackageTypesList
		{
			get
			{
				if (freightPackageTypesList == null)
				{
					var type = ObjectFactory.GetType<IRefPackTypeCollection>();
					var collection = (IRefPackTypeCollection)type.GetConstructor(new Type[] { typeof(BusinessObjectFactory) }).Invoke(new object[] { CurrentFactory });
					freightPackageTypesList = (CodeDescriptionPairList)collection.GetAsCodeDescriptionPairFull();
				}
				return freightPackageTypesList;
			}
		}
		CodeDescriptionPairList freightPackageTypesList;

		void CheckUniqueness(ZPropertyInfo propertyInfo)
		{
			if (!FreightPackageType.IsEmpty)
			{
				foreach (BusinessObjectCollection collection in ParentCollections)
				{
					foreach (PackageTypePair packageType in collection)
					{
						if ((packageType.FreightPackageType == FreightPackageType) && (this != packageType))
						{
							propertyInfo.AddError(string.Format(PackageTypeShouldBeUnique, FreightPackageType));
							break;
						}
					}
				}
			}
		}
		internal static string PackageTypeShouldBeUnique
		{
			get { return Res.GetString("1991f944-ad27-47ce-8127-791f1772f5a2", "Please select a different Freight package type. '{0:G}' is already mapped"); }
		}

		#endregion

		#region CustomsPackageTypeDescription

		public ZString CustomsPackageTypeDescription
		{
			get { return CustomsPackageTypesList.GetDescriptionFromCode(CustomsPackageType); }
		}

		public ZPropertyInfo CustomsPackageTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CustomsPackageTypeDescription); }
		}

		#endregion

		#region CustomsPackageType

		public virtual ZString CustomsPackageTypeFieldType => nameof(FieldType.TextDropEdit);

		[List("CustomsPackageTypesList")]
		[MaxLength(Schema.CustomsPackageTypeMaxLength)]
		public ZString CustomsPackageType
		{
			get { return customsPackageType; }
			set
			{
				SetNonPersistentPropertyValue(CustomsPackageTypeInfo, ref customsPackageType, value.ToUpper());
				if (!IsValidationSuspended)
				{
					ValidateCustomsPackageType();
				}
			}
		}
		ZString customsPackageType;

		public ZPropertyInfo CustomsPackageTypeInfo
		{
			get { return GetZPropertyInfo(Schema.CustomsPackageType); }
		}

		public void ValidateCustomsPackageType()
		{
			CustomsPackageTypeInfo.ClearAllNotifications();
			CheckCustomsPackageType();
		}

		protected virtual void CheckCustomsPackageType()
		{
			MandatoryValidation.CheckEntered(CustomsPackageTypeInfo);
			if (InvalidCustomsPackageTypeNotificationType == CargoWise.EntityFramework.NotificationType.Error)
			{
				ListValidation.ErrorIfInvalidCode(CustomsPackageTypeInfo, CustomsPackageTypesList, CustomsPackageTypeShouldBeInList);
			}
			else if (InvalidCustomsPackageTypeNotificationType == CargoWise.EntityFramework.NotificationType.Warning)
			{
				ListValidation.WarnIfInvalidCode(CustomsPackageTypeInfo, CustomsPackageTypesList, CustomsPackageTypeShouldBeInList);
			}
		}

		protected virtual INotificationType InvalidCustomsPackageTypeNotificationType => CargoWise.EntityFramework.NotificationType.Error;

		public virtual IMultilingualString CustomsPackageTypeShouldBeInList
		{
			get { return ResString.GetMultilingualString("ed9275b2-dc19-46f4-9f35-0612391064b9", "Customs Package Type. Customs Package Type should be in Customs Package Type list"); }
		}

		public abstract CodeDescriptionPairList CustomsPackageTypesList { get; }

		#endregion

		#endregion

		#region Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCustomsPackageType();
			ValidateFreightPackageType();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			PackageTypePair result = GetInstanceForClone();
			result.FreightPackageType = FreightPackageType;
			result.CustomsPackageType = CustomsPackageType;
			return result;
		}

		protected abstract PackageTypePair GetInstanceForClone();

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.CustomsPackageType, CustomsPackageType);
			writer.WriteElementString(Schema.FreightPackageType, FreightPackageType);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			CustomsPackageType = reader.ReadElementString(Schema.CustomsPackageType);
			FreightPackageType = reader.ReadElementString(Schema.FreightPackageType);
		}

		#endregion
	}
}
