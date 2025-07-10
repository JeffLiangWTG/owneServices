using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.Organisations.DeniedPartyScreening.XmlSerializers")]
	public class DpsConfidenceThresholdsBusinessObject : RegistryBusinessObjectTemplate<DpsConfidenceThresholdsValidation>
	{
		public DpsConfidenceThresholdsBusinessObject()
			: this(DeniedPartyConstants.MatchScores.Medium, DeniedPartyConstants.MatchScores.High)
		{
		}

		[SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors", Justification = "Virtual method is HasChanges_set. This is safe to call here.")]
		public DpsConfidenceThresholdsBusinessObject(int mediumThreshold, int highThreshold)
			: base()
		{
			MediumThreshold = mediumThreshold;
			HighThreshold = highThreshold;
			HasChanges = false;
		}

		#region Schema

		public static class Schema
		{
			public static readonly string MediumThreshold = nameof(MediumThreshold);
			public static readonly string HighThreshold = nameof(HighThreshold);
		}

		#endregion

		#region Constants

		public static class Constants
		{
			public const int MediumThresholdMinimum = 65;
			public const int MediumThresholdMaximum = 85;

			public const int HighThresholdMinimum = 75;
			public const int HighThresholdMaximum = 95;

			public const int ThresholdDifferenceMinimum = 10;
		}

		#endregion

		#region Properties

		public ZInt MediumThreshold
		{
			get { return mediumThreshold; }
			set
			{
				SetNonPersistentPropertyValue(MediumThresholdInfo, ref mediumThreshold, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateAll();
				}
			}
		}
		ZInt mediumThreshold;

		public ZPropertyInfo MediumThresholdInfo => GetZPropertyInfo(Schema.MediumThreshold);

		public ZInt HighThreshold
		{
			get { return highThreshold; }
			set
			{
				SetNonPersistentPropertyValue(HighThresholdInfo, ref highThreshold, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateAll();
				}
			}
		}
		ZInt highThreshold;

		public ZPropertyInfo HighThresholdInfo => GetZPropertyInfo(Schema.HighThreshold);

		#endregion

		#region Overrides

		protected override DpsConfidenceThresholdsValidation GetNewValidation() => new DpsConfidenceThresholdsValidation(this);

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new DpsConfidenceThresholdsBusinessObject(MediumThreshold, HighThreshold);

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteElementString(Schema.MediumThreshold, MediumThreshold.ToString());
			writer.WriteElementString(Schema.HighThreshold, HighThreshold.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			using (GetValidationSuspender())
			{
				MediumThreshold = reader.ReadElementStringAsZInt(Schema.MediumThreshold);
				HighThreshold = reader.ReadElementStringAsZInt(Schema.HighThreshold);
			}
		}

		public override bool Equals(object obj)
		{
			var isEqual = false;
			if (obj != null && GetType() == obj.GetType())
			{
				var other = (DpsConfidenceThresholdsBusinessObject)obj;
				isEqual = (HighThreshold == other.HighThreshold) && (MediumThreshold == other.MediumThreshold);
			}

			return isEqual;
		}

		public override int GetHashCode() => MediumThreshold.GetHashCode() ^ HighThreshold.GetHashCode();

		#endregion

	}
}
