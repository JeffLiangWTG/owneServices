using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	[DebuggerDisplay("{ConsumerTypeCode}-{Direction}-{TransportMode}-{ConsolType}")]
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class JobInvoicingDefaultGatewayDepartments : RegistryBusinessObjectTemplate, IJobInvoicingDefaultDepartments
	{
		public JobInvoicingDefaultGatewayDepartments()
			: this(false)
		{
		}

		public JobInvoicingDefaultGatewayDepartments(bool isDefaulting)
		{
			if (isDefaulting)
			{
				SuspendValidation();
			}
		}

		public JobInvoicingDefaultGatewayDepartments(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new JobInvoicingDefaultGatewayDepartments(fallbackLevel, null);
		}

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateConsolType();
			ValidateDepartment();
			ValidateDirection();
			ValidateTransportMode();
		}

		readonly MultilingualString IdenticalConfigurationExists = ResString.GetMultilingualString("552166D5-813E-4744-8F84-20D81F7D3147", "Configuration with identical criteria already exists.");

		void CheckIdenticalConfigurationExists()
		{
			var parentCollection = ParentCollections.OfType<JobInvoicingDefaultGatewayDepartmentsCollection>().FirstOrDefault();

			if (!DirectionInfo.HasErrors() && parentCollection != null)
			{
				foreach (JobInvoicingDefaultGatewayDepartments configuration in parentCollection)
				{
					if (PK != configuration.PK
						&& Direction == configuration.Direction
						&& TransportMode == configuration.TransportMode
						&& ConsolType == configuration.ConsolType)
					{
						DirectionInfo.AddError(IdenticalConfigurationExists);
						break;
					}
				}
			}
		}

		#endregion

		#region Bound Properties

		#region Direction

		ZString direction;

		[MaxLength(3)]
		[List(nameof(Directions))]
		public ZString Direction
		{
			get { return direction; }
			set
			{
				SetNonPersistentPropertyValue(DirectionInfo, ref direction, value);

				if (!IsValidationSuspended)
				{
					RunPreSaveValidationCore();
				}
			}
		}

		public ZPropertyInfo DirectionInfo
		{
			get { return GetZPropertyInfo(nameof(Direction)); }
		}

		void ValidateDirection()
		{
			DirectionInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DirectionInfo);
			ListValidation.ErrorIfInvalidCode(DirectionInfo);
			CheckIdenticalConfigurationExists();
		}

		#endregion

		#region Transport Mode

		ZString transportMode;
		[List(nameof(TrnModes))]
		public ZString TransportMode
		{
			get { return transportMode; }
			set
			{
				SetNonPersistentPropertyValue(TransportModeInfo, ref transportMode, value);

				if (!IsValidationSuspended)
				{
					RunPreSaveValidationCore();
				}
			}
		}

		public ZPropertyInfo TransportModeInfo
		{
			get { return GetZPropertyInfo(nameof(TransportMode)); }
		}

		void ValidateTransportMode()
		{
			TransportModeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(TransportModeInfo);
			ListValidation.ErrorIfInvalidCode(TransportModeInfo);
			CheckIdenticalConfigurationExists();
		}

		#endregion

		#region Consol Type

		ZString fConsolType;

		[MaxLength(3)]
		[List(nameof(ConsolTypes))]
		public ZString ConsolType
		{
			get { return fConsolType; }
			set
			{
				SetNonPersistentPropertyValue(ConsolTypeInfo, ref fConsolType, value);

				if (!IsValidationSuspended)
				{
					RunPreSaveValidationCore();
				}
			}
		}

		public ZPropertyInfo ConsolTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ConsolType)); }
		}

		void ValidateConsolType()
		{
			ConsolTypeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ConsolTypeInfo);
			ListValidation.ErrorIfInvalidCode(ConsolTypeInfo);

			if (!ConsolTypeInfo.HasErrors())
			{
				CheckIdenticalConfigurationExists();
			}
		}

		#endregion

		#region Department

		ZGuid fDepartment;
		[List(nameof(Departments))]
		public ZGuid Department
		{
			get { return fDepartment; }
			set
			{
				SetNonPersistentPropertyValue(DepartmentInfo, ref fDepartment, value);

				if (!IsValidationSuspended)
				{
					RunPreSaveValidationCore();
				}
			}
		}

		public ZPropertyInfo DepartmentInfo
		{
			get { return GetZPropertyInfo(nameof(Department)); }
		}

		void ValidateDepartment()
		{
			DepartmentInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DepartmentInfo);
			ListValidation.ErrorIfInvalidPK(DepartmentInfo);
		}

		#endregion

		#endregion

		#region Lookups

		public CodeDescriptionPairList ConsolTypes => CurrentFactory.GetCachedValue("JobInvoicingDefaultDepartments.ConsolTypes", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(JobInvoicingDefaultDepartmentConsolType.All);
			result.AddPair(AgentType.Agent, AgentTypeDescriptions.Agent);
			result.AddPair(AgentType.CoLoad, AgentTypeDescriptions.CoLoad);

			return result;
		});

		public GlbDepartmentCollection Departments => CurrentFactory.GetCachedValue("JobInvoicingDefaultDepartments.Departments", () => new GlbDepartmentCollection(CurrentFactory, NonMiscDepartmentQuery));

		public CodeDescriptionPairList TrnModes => CurrentFactory.GetCachedValue("JobInvoicingDefaultDepartments.TransportModes", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(TransportModes.All);
			result.AddPair(TransportModes.Air);
			result.AddPair(TransportModes.Sea);
			result.AddPair(TransportModes.Rail);
			result.AddPair(TransportModes.Road);
			return result;
		});

		public CodeDescriptionPairList Directions => CurrentFactory.GetCachedValue("JobInvoicingDefaultDepartments.Directions", () =>
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(FreightShipmentDirection.Code.All, FreightShipmentDirection.Description.All);
			result.AddPair(FreightShipmentDirection.Code.Import, FreightShipmentDirection.Description.Import);
			result.AddPair(FreightShipmentDirection.Code.Export, FreightShipmentDirection.Description.Export);
			result.AddPair(FreightShipmentDirection.Code.Domestic, FreightShipmentDirection.Description.Domestic);
			result.AddPair(FreightShipmentDirection.Code.Other, FreightShipmentDirection.Description.Other);
			return result;
		});

		ZQuery fNonMiscDepartmentQuery;
		ZQuery NonMiscDepartmentQuery
		{
			get
			{
				if (fNonMiscDepartmentQuery == null)
				{
					fNonMiscDepartmentQuery = new ZDBOnlyQuery(typeof(GlbDepartment));

					string sqlText = string.Format(CultureInfo.InvariantCulture, @"
								{0} = 0
								AND
								(
									{1} IS NULL OR
									{1} NOT IN
									(
										SELECT {2}
										FROM {3}
										WHERE {0} = 1
									)
								)",
												   GlbDepartmentSchema.Constants.GE_Misc, // 0
												   GlbDepartmentSchema.Constants.GE_GE, // 1
												   GlbDepartmentSchema.Constants.PK, // 2
												   GlbDepartmentSchema.Constants.TableName); // 3

					fNonMiscDepartmentQuery.AddFilterAndZSQLParameterCollection(sqlText, new ZSqlParameterCollection());
				}

				return fNonMiscDepartmentQuery;
			}
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(nameof(Direction), Direction);
			writer.WriteElementString(nameof(TransportMode), TransportMode);
			writer.WriteElementString(nameof(ConsolType), ConsolType);
			writer.WriteElementString(nameof(Department), Department.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			Direction = reader.ReadElementString(nameof(Direction));
			TransportMode = reader.ReadElementString(nameof(TransportMode));
			ConsolType = reader.ReadElementString(nameof(ConsolType));
			Department = new ZGuid(reader.ReadElementString(nameof(Department)));
		}

		#endregion
	}
}
