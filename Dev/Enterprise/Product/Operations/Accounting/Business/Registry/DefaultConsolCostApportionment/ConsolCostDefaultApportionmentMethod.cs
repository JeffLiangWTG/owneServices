using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesConstants;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ConsolCostDefaultApportionmentMethod : RegistryBusinessObjectTemplate, IApportionmentMethodOverride
	{
		#region Schema

		public abstract class Schema
		{
			public const string Module = "Module";
			public const string ConsolType = "ConsolType";
			public const string TransportMode = "TransportMode";
			public const string Direction = "Direction";
			public const string ContainerMode = "ContainerMode";
			public const string Apportionment = "Apportionment";
		}

		#endregion

		public ConsolCostDefaultApportionmentMethod()
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ConsolCostDefaultApportionmentMethod();
		}

		public ConsolCostDefaultApportionmentMethodCollection ParentCollection;

		#region Transport Mode

		[MaxLength(3)]
		[List("Lookups.TransportModeList")]
		public ZString TransportMode
		{
			get { return TransportModeInfo.ReadOnly ? ZString.Empty : fTransportMode; }
			set
			{
				CheckMaximumLength(TransportModeInfo, value);
				SetNonPersistentPropertyValue(TransportModeInfo, ref fTransportMode, value);
				if (!IsValidationSuspended)
				{
					ValidateTransportMode();
				}
			}
		}

		public ZPropertyInfo TransportModeInfo
		{
			get { return GetZPropertyInfo(Schema.TransportMode); }
		}

		public void ValidateTransportMode()
		{
			TransportModeInfo.ClearAllNotifications();

			Validation.ValidateTransportMode();
		}

		ZString fTransportMode = ApportionmentMethod.AllCode;

		#endregion

		#region Container Mode

		[MaxLength(3)]
		[List("Lookups.ContainerModeList")]
		public ZString ContainerMode
		{
			get { return ContainerModeInfo.ReadOnly ? ZString.Empty : fContainerMode; }
			set
			{
				CheckMaximumLength(ContainerModeInfo, value);
				SetNonPersistentPropertyValue(ContainerModeInfo, ref fContainerMode, value);
				if (!IsValidationSuspended)
				{
					ValidateContainerMode();
				}
			}
		}

		public ZPropertyInfo ContainerModeInfo
		{
			get { return GetZPropertyInfo(Schema.ContainerMode); }
		}

		public void ValidateContainerMode()
		{
			ContainerModeInfo.ClearAllNotifications();

			Validation.ValidateContainerMode();
		}

		ZString fContainerMode = ApportionmentMethod.AllCode;

		#endregion

		#region Module

		[MaxLength(3)]
		[List("Lookups.ModuleList")]
		public ZString Module
		{
			get { return ModuleInfo.ReadOnly ? ZString.Empty : fModule; }
			set
			{
				CheckMaximumLength(ModuleInfo, value);
				SetNonPersistentPropertyValue(ModuleInfo, ref fModule, value);
				if (!IsValidationSuspended)
				{
					ValidateModule();
				}
				if (value == ApportionmentMethodModules.TransportBooking)
				{
					ConsolType = ApportionmentMethod.AllCode;
					Direction = ApportionmentMethod.AllCode;
				}
				else if (value == ApportionmentMethodModules.TransitWarehouse)
				{
					ConsolType = ApportionmentMethod.AllCode;
					Direction = ApportionmentMethod.AllCode;
					TransportMode = ApportionmentMethod.AllCode;
					ContainerMode = ApportionmentMethod.AllCode;
				}
			}
		}

		public ZPropertyInfo ModuleInfo
		{
			get { return GetZPropertyInfo(Schema.Module); }
		}

		public void ValidateModule()
		{
			ModuleInfo.ClearAllNotifications();

			Validation.ValidateModule();
		}

		ZString fModule = ApportionmentMethod.AllCode;

		#endregion

		#region Direction

		[MaxLength(3)]
		[List("Lookups.DirectionList")]
		public ZString Direction
		{
			get { return DirectionInfo.ReadOnly ? ZString.Empty : fDirection; }
			set
			{
				CheckMaximumLength(DirectionInfo, value);
				SetNonPersistentPropertyValue(DirectionInfo, ref fDirection, value);
				if (!IsValidationSuspended)
				{
					ValidateDirection();
				}
			}
		}

		public ZPropertyInfo DirectionInfo
		{
			get { return GetZPropertyInfo(Schema.Direction); }
		}

		public void ValidateDirection()
		{
			DirectionInfo.ClearAllNotifications();

			Validation.ValidateDirection();
		}

		ZString fDirection = ApportionmentMethod.AllCode;

		#endregion

		#region Consol Type

		[MaxLength(3)]
		[List("Lookups.ConsolTypeList")]
		public ZString ConsolType
		{
			get { return ConsolTypeInfo.ReadOnly ? ZString.Empty : fConsolType; }
			set
			{
				CheckMaximumLength(ConsolTypeInfo, value);
				SetNonPersistentPropertyValue(ConsolTypeInfo, ref fConsolType, value);
				if (!IsValidationSuspended)
				{
					ValidateConsolType();
				}
			}
		}

		public ZPropertyInfo ConsolTypeInfo
		{
			get { return GetZPropertyInfo(Schema.ConsolType); }
		}

		public void ValidateConsolType()
		{
			ConsolTypeInfo.ClearAllNotifications();

			Validation.ValidateConsolType();
		}

		ZString fConsolType = ApportionmentMethod.AllCode;

		#endregion

		#region Apportionment

		[MaxLength(3)]
		[List("Lookups.ApportionmentList")]
		public ZString Apportionment
		{
			get { return ApportionmentInfo.ReadOnly ? ZString.Empty : fApportionment; }
			set
			{
				CheckMaximumLength(ApportionmentInfo, value);
				SetNonPersistentPropertyValue(ApportionmentInfo, ref fApportionment, value);
				if (!IsValidationSuspended)
				{
					ValidateApportionment();
				}
			}
		}

		public ZPropertyInfo ApportionmentInfo
		{
			get { return GetZPropertyInfo(Schema.Apportionment); }
		}

		public void ValidateApportionment()
		{
			ApportionmentInfo.ClearAllNotifications();
			Validation.ValidateApportionment();
		}

		public void ValidateApportionmentList()
		{
			ApportionmentInfo.ClearAllNotifications();
			Validation.ValidateApportionmentList();
		}

		ZString fApportionment;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateModule();
			ValidateConsolType();
			ValidateTransportMode();
			ValidateContainerMode();
			ValidateDirection();
			ValidateApportionment();
		}

		public ConsolCostDefaultApportionmentMethodValidation Validation
		{
			get { return new ConsolCostDefaultApportionmentMethodValidation(this); }
		}

		#endregion

		#region Lookups

		public ConsolCostDefaultApportionmentMethodLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = GetNewLookups();
				}
				return fLookups;
			}
		}

		protected virtual ConsolCostDefaultApportionmentMethodLookups GetNewLookups()
		{
			return new ConsolCostDefaultApportionmentMethodLookups(this);
		}

		ConsolCostDefaultApportionmentMethodLookups fLookups;

		#endregion

		ZString IApportionmentMethodOverride.Module => Module;

		ZString IApportionmentMethodOverride.ConsolType => ConsolType;

		ZString IApportionmentMethodOverride.ApportionmentMethod => Apportionment;

		ZString IApportionmentMethodOverride.ContainerMode => ContainerMode;

		ZString IApportionmentMethodOverride.TransportMode => TransportMode;

		ZString IApportionmentMethodOverride.Direction => Direction;

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.Module, Module);
			writer.WriteElementString(Schema.ConsolType, ConsolType);
			writer.WriteElementString(Schema.TransportMode, TransportMode);
			writer.WriteElementString(Schema.ContainerMode, ContainerMode);
			writer.WriteElementString(Schema.Apportionment, Apportionment);
			writer.WriteElementString(Schema.Direction, Direction);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Module = reader.ReadElementString(Schema.Module);
			ConsolType = reader.ReadElementString(Schema.ConsolType);
			TransportMode = reader.ReadElementString(Schema.TransportMode);
			ContainerMode = reader.ReadElementString(Schema.ContainerMode);
			Apportionment = reader.ReadElementString(Schema.Apportionment);
			Direction = reader.ReadElementString(Schema.Direction);
		}
	}
}
