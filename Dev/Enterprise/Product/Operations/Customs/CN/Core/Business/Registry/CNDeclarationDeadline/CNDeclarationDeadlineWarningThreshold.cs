using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CN.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.CN.Business.XmlSerializers")]
	public class CNDeclarationDeadlineWarningThreshold : RegistryBusinessObjectTemplate
	{
		#region Schema

		abstract class Schema
		{
			public const string TransportMode = "TransportMode";
			public const string FirstLevelThreshold = "FirstLevelThreshold";
			public const string FirstLevelWarningColor = "FirstLevelWarningColor";
			public const string SecondLevelThreshold = "SecondLevelThreshold";
			public const string SecondLevelWarningColor = "SecondLevelWarningColor";
			public const string ThirdLevelThreshold = "ThirdLevelThreshold";
			public const string ThirdLevelWarningColor = "ThirdLevelWarningColor";
			public const string DelayedWarningColor = "DelayedWarningColor";
		}

		#endregion

		#region Constructors
		public CNDeclarationDeadlineWarningThreshold()
			: base()
		{
		}

		public CNDeclarationDeadlineWarningThreshold(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CNDeclarationDeadlineWarningThreshold(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}
		#endregion

		#region Properties

		protected override ZString HumanReadableNameCore => Res.GetString("A388788D-B148-4484-8D4B-BACFA1AFB872", "Declaration Deadline Warning Threshold");

		#region TransportMode
		[List(nameof(TransportTypeListWithAll))]
		public ZString TransportMode
		{
			get => transportMode;
			set
			{
				SetNonPersistentPropertyValue(TransportModeInfo, ref transportMode, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTransportMode();
				}
			}
		}

		public ZPropertyInfo TransportModeInfo => GetZPropertyInfo(Schema.TransportMode);

		ZString transportMode;
		#endregion

		#region FirstLevelThreshold
		public ZInt FirstLevelThreshold
		{
			get => firstLevelThreshold;
			set
			{
				if (firstLevelThreshold != value)
				{
					SetNonPersistentPropertyValue(FirstLevelThresholdInfo, ref firstLevelThreshold, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateFirstLevelThresholdInfo();
					}
					FirstLevelThresholdInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo FirstLevelThresholdInfo => GetZPropertyInfo(Schema.FirstLevelThreshold);

		ZInt firstLevelThreshold;
		#endregion

		#region FirstLevelWarningColor

		[List(nameof(List))]
		[MaxLength(11)]
		public ZString FirstLevelWarningColor
		{
			get => firstLevelWarningColor;
			set
			{
				value = ColorHelper.FormatColorValue(value);
				SetNonPersistentPropertyValue(FirstLevelWarningColorInfo, ref firstLevelWarningColor, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateFirstLevelWarningColor();
				}
				FirstLevelWarningColorInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo FirstLevelWarningColorInfo => GetZPropertyInfo(Schema.FirstLevelWarningColor);

		ZString firstLevelWarningColor;

		#endregion

		#region SecondLevelThreshold
		public ZInt SecondLevelThreshold
		{
			get => secondLevelThreshold;
			set
			{
				if (secondLevelThreshold != value)
				{
					SetNonPersistentPropertyValue(SecondLevelThresholdInfo, ref secondLevelThreshold, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateSecondLevelThresholdInfo();
					}
					SecondLevelThresholdInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo SecondLevelThresholdInfo => GetZPropertyInfo(Schema.SecondLevelThreshold);

		ZInt secondLevelThreshold;
		#endregion

		#region SecondLevelWarningColor

		[List(nameof(List))]
		[MaxLength(11)]
		public ZString SecondLevelWarningColor
		{
			get => secondLevelWarningColor;
			set
			{
				value = ColorHelper.FormatColorValue(value);
				SetNonPersistentPropertyValue(SecondLevelWarningColorInfo, ref secondLevelWarningColor, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateSecondLevelWarningColor();
				}
				SecondLevelWarningColorInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo SecondLevelWarningColorInfo => GetZPropertyInfo(Schema.SecondLevelWarningColor);

		ZString secondLevelWarningColor;

		#endregion

		#region ThirdLevelThreshold
		public ZInt ThirdLevelThreshold
		{
			get => thirdLevelThreshold;
			set
			{
				if (thirdLevelThreshold != value)
				{
					SetNonPersistentPropertyValue(ThirdLevelThresholdInfo, ref thirdLevelThreshold, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateThirdLevelThresholdInfo();
					}
					ThirdLevelThresholdInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo ThirdLevelThresholdInfo => GetZPropertyInfo(Schema.ThirdLevelThreshold);

		ZInt thirdLevelThreshold;
		#endregion

		#region ThirdLevelWarningColor

		[List(nameof(List))]
		[MaxLength(11)]
		public ZString ThirdLevelWarningColor
		{
			get => thirdLevelWarningColor;
			set
			{
				value = ColorHelper.FormatColorValue(value);
				SetNonPersistentPropertyValue(ThirdLevelWarningColorInfo, ref thirdLevelWarningColor, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateThirdLevelWarningColor();
				}
				ThirdLevelWarningColorInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ThirdLevelWarningColorInfo => GetZPropertyInfo(Schema.ThirdLevelWarningColor);

		ZString thirdLevelWarningColor;

		#endregion

		#region DelayedWarningColor

		[List(nameof(List))]
		[MaxLength(11)]
		public ZString DelayedWarningColor
		{
			get => delayedWarningColor;
			set
			{
				value = ColorHelper.FormatColorValue(value);
				SetNonPersistentPropertyValue(DelayedWarningColorInfo, ref delayedWarningColor, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDelayedWarningColor();
				}
				DelayedWarningColorInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DelayedWarningColorInfo => GetZPropertyInfo(Schema.DelayedWarningColor);

		ZString delayedWarningColor;

		#endregion

		#endregion

		#region Lookups

		List<ZString> List => new List<ZString>();

		public CodeDescriptionPairList TransportTypeListWithAll
		{
			get
			{
				if (transportTypeListWithAll == null)
				{
					transportTypeListWithAll = new CodeDescriptionPairList();
					transportTypeListWithAll.AddPair(ALL, Res.GetString("3BCE10CD-DDE1-4FBD-9BA4-968CF5608271", "All"));
					transportTypeListWithAll.AddRange(new TransportTypeList());
				}
				return transportTypeListWithAll;
			}
		}
		public const string ALL = "ALL";
		CodeDescriptionPairList transportTypeListWithAll;
		#endregion

		#region Validation

		CNDeclarationDeadlineWarningThresholdValidation fValidation;
		public CNDeclarationDeadlineWarningThresholdValidation Validation => fValidation ?? (fValidation = new CNDeclarationDeadlineWarningThresholdValidation(this));

		#endregion

		#region Implementation
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			=> new CNDeclarationDeadlineWarningThreshold(fallbackLevel, factory);

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion

		#region XML Serialization

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteElementString(Schema.TransportMode, TransportMode);
			writer.WriteElementString(Schema.FirstLevelThreshold, FirstLevelThreshold.ToString());
			writer.WriteElementString(Schema.FirstLevelWarningColor, FirstLevelWarningColor);
			writer.WriteElementString(Schema.SecondLevelThreshold, SecondLevelThreshold.ToString());
			writer.WriteElementString(Schema.SecondLevelWarningColor, SecondLevelWarningColor);
			writer.WriteElementString(Schema.ThirdLevelThreshold, ThirdLevelThreshold.ToString());
			writer.WriteElementString(Schema.ThirdLevelWarningColor, ThirdLevelWarningColor);
			writer.WriteElementString(Schema.DelayedWarningColor, DelayedWarningColor);
			base.WriteElements(writer);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			TransportMode = reader.ReadElementString(Schema.TransportMode);
			FirstLevelThreshold = ZInt.TryParse(reader.ReadElementString(Schema.FirstLevelThreshold), out var firstLevel) ? firstLevel : ZInt.Zero;
			FirstLevelWarningColor = reader.ReadElementString(Schema.FirstLevelWarningColor);
			SecondLevelThreshold = ZInt.TryParse(reader.ReadElementString(Schema.SecondLevelThreshold), out var secondLevel) ? secondLevel : ZInt.Zero;
			SecondLevelWarningColor = reader.ReadElementString(Schema.SecondLevelWarningColor);
			ThirdLevelThreshold = ZInt.TryParse(reader.ReadElementString(Schema.ThirdLevelThreshold), out var thirdLevel) ? thirdLevel : ZInt.Zero;
			ThirdLevelWarningColor = reader.ReadElementString(Schema.ThirdLevelWarningColor);
			DelayedWarningColor = reader.ReadElementString(Schema.DelayedWarningColor);
		}
		#endregion
	}
}
