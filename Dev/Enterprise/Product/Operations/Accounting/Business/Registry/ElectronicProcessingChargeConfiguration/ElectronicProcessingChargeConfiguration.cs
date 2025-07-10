using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ElectronicProcessingChargeConfiguration : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string JobType = "JobType";
			public const string StartDate = "StartDate";
			public const string EndDate = "EndDate";
		}

		#endregion Schema

		public ElectronicProcessingChargeConfiguration()
		{
		}

		ElectronicProcessingChargeConfiguration(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ElectronicProcessingChargeConfiguration(fallbackLevel);
		}

		public ElectronicProcessingChargeConfigurationCollection ParentCollection
		{
			get
			{
				if (((IBusinessObjectInternals)this).ParentCollections.Length > 0)
				{
					return (ElectronicProcessingChargeConfigurationCollection)((IBusinessObjectInternals)this).ParentCollections[0];
				}
				else
				{
					return null;
				}
			}
		}

		[List("Lookups.JobTypeList")]
		public ZString JobType
		{
			get => jobType;
			set
			{
				SetNonPersistentPropertyValue(JobTypeInfo, ref jobType, value);
				if (!IsValidationSuspended)
				{
					ValidateJobType();
				}
			}
		}
		ZString jobType = ZString.Empty;

		public ZPropertyInfo JobTypeInfo
		{
			get { return GetZPropertyInfo(Schema.JobType); }
		}

		public ZDate StartDate
		{
			get => startDate;
			set
			{
				SetNonPersistentPropertyValue(StartDateInfo, ref startDate, value);
				if (!IsValidationSuspended)
				{
					ValidateStartDate();
				}
			}
		}
		ZDate startDate = ZDate.Empty;

		public ZPropertyInfo StartDateInfo
		{
			get { return GetZPropertyInfo(Schema.StartDate); }
		}

		public ZDate EndDate
		{
			get => endDate;
			set
			{
				SetNonPersistentPropertyValue(EndDateInfo, ref endDate, value);
				EndDateInfo.ClearAllNotifications();
				TypeValidation.CheckValidSmallDateTime(EndDateInfo);
			}
		}
		ZDate endDate = ZDate.Empty;

		public ZPropertyInfo EndDateInfo
		{
			get { return GetZPropertyInfo(Schema.EndDate); }
		}

		public ElectronicProcessingChargeConfigurationLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new ElectronicProcessingChargeConfigurationLookups();
				}

				return lookups;
			}
		}
		ElectronicProcessingChargeConfigurationLookups lookups;

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.JobType, JobType);
			writer.WriteElementString(Schema.StartDate, StartDate.ToString());
			writer.WriteElementString(Schema.EndDate, EndDate.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			JobType = reader.ReadElementString(Schema.JobType);
			StartDate = new ZDate(reader.ReadElementString(Schema.StartDate));
			EndDate = new ZDate(reader.ReadElementString(Schema.EndDate));
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateJobType();
			ValidateStartDate();
			ValidateEndDate();
		}

		void ValidateJobType()
		{
			JobTypeInfo.ClearAllNotifications();
			IMultilingualString humanReadableName = ResString.GetMultilingualString("A2467E11-AC7A-4CA1-B251-6E74C37B41BD", "Job Type");
			MandatoryValidation.CheckEntered(JobTypeInfo, humanReadableName);
			ListValidation.ErrorIfInvalidCode(JobTypeInfo, Lookups.JobTypeList, humanReadableName);

			ValidateDuplicated();
		}

		void ValidateStartDate()
		{
			IMultilingualString humanReadableName = ResString.GetMultilingualString("97B7AB75-83EF-45D2-BEB9-2EB5F603678A", "Start Date");
			ValidateDate(humanReadableName, StartDateInfo);
		}

		void ValidateEndDate()
		{
			if (!EndDate.IsEmpty && !EndDate.Equals(StartDate))
			{
				CompareValidation.CheckDateIsAfterAnotherDate(EndDateInfo, StartDateInfo);
			}
		}

		void ValidateDate(IMultilingualString humanReadableName, ZPropertyInfo info)
		{
			info.ClearAllNotifications();
			MandatoryValidation.CheckEntered(info, humanReadableName);
			TypeValidation.CheckValidSmallDateTime(info);
		}

		void ValidateDuplicated()
		{
			var errorMessage = Res.GetString("20ED8985-7411-412A-BDD0-1AAB155DB9F7", "This row has been duplicated and must be unique.");
			ClearRowNotificationsContaining(errorMessage);

			if (!HasErrors && CheckDuplicated())
			{
				AddRowError(errorMessage);
			}

			bool CheckDuplicated()
			{
				if (ParentCollection == null)
				{
					return false;
				}

				return ParentCollection.OfType<ElectronicProcessingChargeConfiguration>()
					.Except(new[] { this })
					.Any(item => item.JobType == JobType);
			}
		}
	}
}
