using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Accounting.Business.AccountingUtils;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class PeriodClosureConfiguration : RegistryBusinessObjectTemplate
	{
		#region Schema

		abstract class Schema
		{
			public const string IntervalType = "IntervalType";
			public const string SubLedgerInterval = "SubLedgerInterval";
			public const string GeneralLedgerInterval = "GeneralLedgerInterval";
			public const string AdjustmentLedgerInterval = "AdjustmentLedgerInterval";
		}

		#endregion

		public PeriodClosureConfiguration()
		{
		}

		public PeriodClosureConfiguration(string intervalType, int subLedgerInterval = 0, int generalLedgerInterval = 0, int adjustmentLedgerInterval = 0)
		{
			IntervalType = intervalType;
			SubLedgerInterval = subLedgerInterval;
			GeneralLedgerInterval = generalLedgerInterval;
			AdjustmentLedgerInterval = adjustmentLedgerInterval;
		}

		public PeriodClosureConfiguration(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		#region Read / Write Elements

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			IntervalType = reader.ReadElementString(Schema.IntervalType);
			SubLedgerInterval = reader.ReadElementStringAsZInt(Schema.SubLedgerInterval);
			GeneralLedgerInterval = reader.ReadElementStringAsZInt(Schema.GeneralLedgerInterval);
			AdjustmentLedgerInterval = reader.ReadElementStringAsZInt(Schema.AdjustmentLedgerInterval);
		}

		protected override void WriteElements(System.Xml.XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.IntervalType, IntervalType);
			writer.WriteElementString(Schema.SubLedgerInterval, SubLedgerInterval.ToString());
			writer.WriteElementString(Schema.GeneralLedgerInterval, GeneralLedgerInterval.ToString());
			writer.WriteElementString(Schema.AdjustmentLedgerInterval, AdjustmentLedgerInterval.ToString());
		}

		#endregion

		#region GetClone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PeriodClosureConfiguration(fallbackLevel);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			var target = (PeriodClosureConfiguration)clone;
			base.CopyValuesToClone(target);
			target.IntervalType = IntervalType;
			target.SubLedgerInterval = SubLedgerInterval;
			target.GeneralLedgerInterval = GeneralLedgerInterval;
			target.AdjustmentLedgerInterval = AdjustmentLedgerInterval;
		}

		#endregion

		#region IntervalTypes

		public PeriodClosureConfigurationValidation Validation => validation ?? (validation = new PeriodClosureConfigurationValidation(this));
		PeriodClosureConfigurationValidation validation;

		#endregion

		#region IntervalTypes

		public CodeDescriptionPairList IntervalTypes
		{
			get
			{
				if (intervalTypes == null)
				{
					intervalTypes = new CodeDescriptionPairList();
					intervalTypes.AddPair(PeriodClosureConfigurationIntervalType.Minutes, ResString.GetMultilingualString("2e6c8b14-e092-4f9d-84f3-4c5d451f3ac3", "Minutes"));
					intervalTypes.AddPair(PeriodClosureConfigurationIntervalType.Days, ResString.GetMultilingualString("311c68b0-eb33-471b-a6fb-86c9b5a20787", "Days"));
					intervalTypes.AddPair(PeriodClosureConfigurationIntervalType.Hours, ResString.GetMultilingualString("3d2dd886-2bcf-41ce-a3a9-8b42aba2a800", "Hours"));
				}

				return intervalTypes;
			}
		}
		CodeDescriptionPairList intervalTypes;

		#endregion

		#region IntervalType

		[List("IntervalTypes")]
		[MaxLength(5)]
		public ZString IntervalType
		{
			get { return intervalType; }
			set
			{
				SetNonPersistentPropertyValue(IntervalTypeInfo, ref intervalType, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateIntervalType();
				}
			}
		}
		ZString intervalType;

		public ZPropertyInfo IntervalTypeInfo
		{
			get { return GetZPropertyInfo(Schema.IntervalType); }
		}

		#endregion

		#region SubLedger

		public ZInt SubLedgerInterval
		{
			get { return subLedgerInterval; }
			set
			{
				SetNonPersistentPropertyValue(SubLedgerIntervalInfo, ref subLedgerInterval, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateSubLedgerInterval();
				}
			}
		}
		ZInt subLedgerInterval;

		public ZPropertyInfo SubLedgerIntervalInfo
		{
			get { return GetZPropertyInfo(Schema.SubLedgerInterval); }
		}

		#endregion

		#region GeneralLedger

		public ZInt GeneralLedgerInterval
		{
			get { return generalLedgerInterval; }
			set
			{
				SetNonPersistentPropertyValue(GeneralLedgerIntervalInfo, ref generalLedgerInterval, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateGeneralLedgerInterval();
				}
			}
		}
		ZInt generalLedgerInterval;

		public ZPropertyInfo GeneralLedgerIntervalInfo
		{
			get { return GetZPropertyInfo(Schema.GeneralLedgerInterval); }
		}

		#endregion

		#region AdjustmentLedger

		public ZInt AdjustmentLedgerInterval
		{
			get { return adjustmentLedgerInterval; }
			set
			{
				SetNonPersistentPropertyValue(AdjustmentLedgerIntervalInfo, ref adjustmentLedgerInterval, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateAdjustmentLedgerInterval();
				}
			}
		}
		ZInt adjustmentLedgerInterval;

		public ZPropertyInfo AdjustmentLedgerIntervalInfo
		{
			get { return GetZPropertyInfo(Schema.AdjustmentLedgerInterval); }
		}

		#endregion
	}
}
