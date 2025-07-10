using System;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class JobProfitLossRequiringReasonParameters : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string IsEnabled = "IsEnabled";
			public const string ProfitThreshold = "ProfitThreshold";
			public const string LossThreshold = "LossThreshold";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Schema constants")]
		public abstract class SchemaNames
		{
			public const string IsEnabled = "Is Enabled";
			public const string ProfitThreshold = "Profit Threshold";
			public const string LossThreshold = "Loss Threshold";
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new JobProfitLossRequiringReasonParameters();
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			JobProfitLossRequiringReasonParameters castedClone = (JobProfitLossRequiringReasonParameters)clone;
			if (fJobStatusCollection != null)
			{
				castedClone.fJobStatusCollection = (CodeSelectionCollection)JobStatusCollection.Clone(castedClone.CurrentFallbackLevel, castedClone.Factory);
				castedClone.RegisterEditableChildObject(JobStatusCollection);
			}
		}

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			ClearRowNotifications();

			base.RunPreSaveValidationCore();
			ValidateProfitThreshold();
			ValidateLossThreshold();

			if (ProfitThreshold > 0M && JobStatusCollection.Count == 0)
			{
				AddRowError(Res.GetString("371e7a87-ffbc-4abd-b92e-a26a986d7dd1", "At least one Status Code must be added to the Code Grid."));
			}
		}

		#endregion

		#region Properties

		#region Loss Threshold

		public ZDecimal LossThreshold
		{
			get { return fLossThreshold; }
			set
			{
				SetNonPersistentPropertyValue(LossThresholdInfo, ref fLossThreshold, value);
				if (!IsValidationSuspended)
				{
					ValidateLossThreshold();
				}
			}
		}

		public ZPropertyInfo LossThresholdInfo
		{
			get { return GetZPropertyInfo(Schema.LossThreshold, SchemaNames.LossThreshold); }
		}

		public void ValidateLossThreshold()
		{
			LossThresholdInfo.ClearAllNotifications();
			if (LossThreshold > ProfitThreshold)
			{
				LossThresholdInfo.AddError(Res.GetString("f70aabbc-afb5-4578-8e06-7cc514836c24", "This value must be less than or equal to the 'PROFIT / REVENUE percentage falls below' value."));
			}
		}

		ZDecimal fLossThreshold;

		#endregion

		#region Profit Threshold

		public ZDecimal ProfitThreshold
		{
			get { return fProfitThreshold; }
			set
			{
				SetNonPersistentPropertyValue(ProfitThresholdInfo, ref fProfitThreshold, value);
				if (!IsValidationSuspended)
				{
					ValidateProfitThreshold();
				}
			}
		}

		public ZPropertyInfo ProfitThresholdInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.ProfitThreshold, SchemaNames.ProfitThreshold);
			}
		}

		public void ValidateProfitThreshold()
		{
			ProfitThresholdInfo.ClearAllNotifications();
			if (ProfitThreshold < LossThreshold)
			{
				ProfitThresholdInfo.AddError(Res.GetString("6bd2e9ff-c01f-4cbb-9556-6df62b8b3ea2", "This value must be greater than or equal to the 'PROFIT / REVENUE percentage falls below' value."));
			}
		}

		ZDecimal fProfitThreshold;

		#endregion

		#region Decimal Places

		public ZInt DecimalPlaces
		{
			get { return 2; }
		}

		public ZPropertyInfo DecimalPlacesInfo
		{
			get { return GetZPropertyInfo(nameof(DecimalPlaces)); }
		}

		#endregion

		#region JobStatusCollection

		public CodeSelectionCollection JobStatusCollection
		{
			get
			{
				if (fJobStatusCollection == null)
				{
					fJobStatusCollection = new CodeSelectionCollection(JobHeaderStatusListProvider);
					RegisterEditableChildObject(fJobStatusCollection);
				}
				return fJobStatusCollection;
			}
		}
		CodeSelectionCollection fJobStatusCollection;

		ZXmlSerializer fJobStatusCollectionSerialiser;
		ZXmlSerializer JobStatusCollectionSerialiser
		{
			get
			{
				return fJobStatusCollectionSerialiser ?? (fJobStatusCollectionSerialiser = ZXmlSerializer.New(typeof(CodeSelectionCollection)));
			}
		}

		CodeDescriptionPairListProvider JobHeaderStatusListProvider
		{
			get
			{
				return fJobHeaderStatusListProvider ?? (fJobHeaderStatusListProvider =
					new CodeDescriptionPairListProvider(() => new JobHeaderStatusList()));
			}
		}
		CodeDescriptionPairListProvider fJobHeaderStatusListProvider;

		#endregion

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.ProfitThreshold, ProfitThreshold.ToString(null, CultureInfo.InvariantCulture));
			writer.WriteElementString(Schema.LossThreshold, LossThreshold.ToString(null, CultureInfo.InvariantCulture));
			JobStatusCollectionSerialiser.Serialize(writer, JobStatusCollection);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			// To provide compatibility with registries that may have been written with some culture
			try
			{
				ProfitThreshold = ZDecimal.Parse(reader.ReadElementString(Schema.ProfitThreshold), CultureInfo.InvariantCulture.NumberFormat);
			}
			catch (FormatException)
			{
				ProfitThreshold = ZDecimal.Parse(reader.ReadElementString(Schema.ProfitThreshold));
			}

			try
			{
				LossThreshold = ZDecimal.Parse(reader.ReadElementString(Schema.LossThreshold), CultureInfo.InvariantCulture.NumberFormat);
			}
			catch (FormatException)
			{
				LossThreshold = ZDecimal.Parse(reader.ReadElementString(Schema.LossThreshold));
			}
			fJobStatusCollection = (CodeSelectionCollection)JobStatusCollectionSerialiser.Deserialize(reader);
			fJobStatusCollection.SetCodesProvider(JobHeaderStatusListProvider);
			RegisterEditableChildObject(fJobStatusCollection);
		}

		#endregion
	}
}
