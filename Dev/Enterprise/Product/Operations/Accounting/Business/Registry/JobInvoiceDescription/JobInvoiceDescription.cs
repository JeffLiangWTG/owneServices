using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class JobInvoiceDescription : RegistryBusinessObjectTemplate, IJobConfigurationSelector
	{
		#region Schema

		public abstract class Schema
		{
			public const string JobType = "JobType";
			public const string DirectionCode = "DirectionCode";
			public const string Mode = "Mode";
			public const string InvoiceDescription = "InvoiceDescription";
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new JobInvoiceDescription();
		}

		IJobConfigurationSelector[] IJobConfigurationSelector.ParentCollectionForValidation
		{
			get { return ParentCollectionForValidation; }
		}

		public JobInvoiceDescription[] ParentCollectionForValidation
		{
			get
			{
				JobInvoiceDescription[] result = System.Array.Empty<JobInvoiceDescription>();
				var parentCollection = GetParentCollection(this, typeof(JobInvoiceDescriptionCollection));
				if (parentCollection != null)
				{
					result = parentCollection.Select(element => (JobInvoiceDescription)element).ToArray();
				}
				return result;
			}
		}

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateJobType();
			ValidateDirectionCode();
			ValidateMode();
			ValidateInvoiceDescription();
		}

		public JobInvoiceDescriptionValidation Validation
		{
			get { return new JobInvoiceDescriptionValidation(this); }
		}

		#endregion

		#region Default Values

		#endregion

		#region Properties

		#region JobType

		[MaxLength(3)]
		[List("JobTypeList")]
		public ZString JobType
		{
			get { return fJobType; }
			set
			{
				CheckMaximumLength(JobTypeInfo, value);
				if (fJobType != value)
				{
					SetNonPersistentPropertyValue(JobTypeInfo, ref fJobType, value);
					InvoiceDescription = ZString.Empty;
					if (!IsValidationSuspended)
					{
						ValidateJobType();
					}
				}
			}
		}
		ZString fJobType;

		public virtual ZPropertyInfo JobTypeInfo
		{
			get { return GetZPropertyInfo(Schema.JobType); }
		}

		public ZArchitecture.Core.CodeDescriptionPairList JobTypeList
		{
			get { return JobInvoiceDescriptionLookups.JobTypeList; }
		}

		public void ValidateJobType()
		{
			JobTypeInfo.ClearAllNotifications();

			Validation.ValidateJobType();
		}

		#endregion

		#region Direction

		[MaxLength(3)]
		[List("DirectionList")]
		public ZString DirectionCode
		{
			get { return DirectionCodeInfo.ReadOnly ? ZString.Empty : fDirectionCode; }
			set
			{
				CheckMaximumLength(DirectionCodeInfo, value);
				SetNonPersistentPropertyValue(DirectionCodeInfo, ref fDirectionCode, value);
				if (!IsValidationSuspended)
				{
					ValidateDirectionCode();
				}
			}
		}
		ZString fDirectionCode;

		public virtual ZPropertyInfo DirectionCodeInfo
		{
			get { return GetZPropertyInfo(Schema.DirectionCode); }
		}

		public bool DirectionCode_ReadOnly
		{
			get
			{
				return ReadOnlyHelper.DirectionCode_ReadOnly;
			}
		}

		public ZArchitecture.Core.CodeDescriptionPairList DirectionList
		{
			get { return JobInvoiceDescriptionLookups.DirectionList; }
		}

		public void ValidateDirectionCode()
		{
			DirectionCodeInfo.ClearAllNotifications();

			Validation.ValidateDirectionCode();
		}

		#endregion

		#region Mode

		[MaxLength(3)]
		[List("ModeList")]
		public ZString Mode
		{
			get { return ModeInfo.ReadOnly ? ZString.Empty : fMode; }
			set
			{
				CheckMaximumLength(ModeInfo, value);
				SetNonPersistentPropertyValue(ModeInfo, ref fMode, value);
				if (!IsValidationSuspended)
				{
					ValidateMode();
				}
			}
		}

		public virtual ZPropertyInfo ModeInfo
		{
			get { return GetZPropertyInfo(Schema.Mode); }
		}

		public bool Mode_ReadOnly
		{
			get
			{
				return ReadOnlyHelper.Mode_ReadOnly;
			}
		}

		public ZArchitecture.Core.CodeDescriptionPairList ModeList
		{
			get { return JobInvoiceDescriptionLookups.ModeList; }
		}

		public void ValidateMode()
		{
			ModeInfo.ClearAllNotifications();

			Validation.ValidateMode();
		}
		ZString fMode;

		#endregion

		#region InvoiceDescription

		[MaxLength(1024)]
		[BusinessObjectMaxLengthTestExclude]
		public ZString InvoiceDescription
		{
			get { return fInvoiceDescription; }
			set
			{
				CheckMaximumLength(InvoiceDescriptionInfo, value);
				SetNonPersistentPropertyValue(InvoiceDescriptionInfo, ref fInvoiceDescription, value);
				if (!IsValidationSuspended)
				{
					ValidateInvoiceDescription();
				}
			}
		}

		public virtual ZPropertyInfo InvoiceDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.InvoiceDescription); }
		}

		public bool InvoiceDescription_ReadOnly
		{
			get { return ReadOnlyHelper.InvoiceDescription_ReadOnly; }
		}

		public void ValidateInvoiceDescription()
		{
			InvoiceDescriptionInfo.ClearAllNotifications();

			Validation.ValidateInvoiceDescription();
		}

		ZString fInvoiceDescription;

		#endregion

		#endregion

		#region JobInvoiceDescriptionLookups

		JobConfigurationSelectorLookups IJobConfigurationSelector.Lookups
		{
			get { return JobInvoiceDescriptionLookups; }
		}

		public JobConfigurationSelectorLookups JobInvoiceDescriptionLookups
		{
			get
			{
				return fLookups ?? (fLookups = GetNewLookups());
			}
		}
		JobConfigurationSelectorLookups fLookups;

		protected virtual JobConfigurationSelectorLookups GetNewLookups()
		{
			return new JobConfigurationSelectorLookups(this);
		}

		#endregion

		#region Read Only Helper

		public JobInvoiceDescriptionReadOnly ReadOnlyHelper
		{
			get
			{
				return fReadOnlyHelper ?? (fReadOnlyHelper = new JobInvoiceDescriptionReadOnly(this));
			}
		}

		JobInvoiceDescriptionReadOnly fReadOnlyHelper;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.JobType, JobType);
			writer.WriteElementString(Schema.DirectionCode, DirectionCode);
			writer.WriteElementString(Schema.Mode, Mode);
			writer.WriteElementString(Schema.InvoiceDescription, InvoiceDescription);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected override void ReadElements(XmlReaderWrapper reader)
		{
			JobType = reader.ReadElementString(Schema.JobType);

			string oldDirectionName = "Direction";
			if (reader.Reader.Name == oldDirectionName)
			{
				string oldFormatValue = reader.ReadElementString(oldDirectionName);
				switch (oldFormatValue)
				{
					case "All":
						DirectionCode = Constants.FreightShipmentDirection.Code.All;
						break;
					case "Import":
						DirectionCode = Constants.FreightShipmentDirection.Code.Import;
						break;
					case "Export":
						DirectionCode = Constants.FreightShipmentDirection.Code.Export;
						break;
					case "Domestic":
						DirectionCode = Constants.FreightShipmentDirection.Code.Domestic;
						break;
					case "Other":
						DirectionCode = Constants.FreightShipmentDirection.Code.Other;
						break;
				}
			}
			else
			{
				DirectionCode = reader.ReadElementString(Schema.DirectionCode);
			}

			Mode = reader.ReadElementString(Schema.Mode);
			InvoiceDescription = reader.ReadElementString(Schema.InvoiceDescription);
		}

		#endregion
	}
}