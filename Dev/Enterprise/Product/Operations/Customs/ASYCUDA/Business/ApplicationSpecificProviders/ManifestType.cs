using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class ManifestType : IManifestType
	{
		public ManifestType(string code, string description, IEnumerable<string> applicableTransportModes, IEnumerable<string> applicableManifestStyles, MessageLevel messageLevel)
		{
			Code = code;
			Description = description;
			ApplicableTransportModes = applicableTransportModes;
			ApplicableManifestStyles = applicableManifestStyles;
			MessageLevel = messageLevel;
		}

		public ManifestType(string code, string description, string applicableTransportMode, string applicableManifestStyle, MessageLevel messageLevel)
			: this(code, description, new[] { applicableTransportMode }, new[] { applicableManifestStyle }, messageLevel)
		{
		}

		public ManifestType(string code, string description, string applicableTransportMode, IEnumerable<string> applicableManifestStyles, MessageLevel messageLevel)
			: this(code, description, new[] { applicableTransportMode }, applicableManifestStyles, messageLevel)
		{
		}

		public ManifestType(string code, string description, IEnumerable<string> applicableTransportModes, string applicableManifestStyle, MessageLevel messageLevel)
			: this(code, description, applicableTransportModes, new[] { applicableManifestStyle }, messageLevel)
		{
		}

		public ManifestType(string code, string description, IEnumerable<string> applicableTransportMode, IEnumerable<string> applicableManifestStyles, MessageLevel messageLevel, CodeDescriptionPairList manifestNatures)
			: this(code, description, applicableTransportMode, applicableManifestStyles, messageLevel)
		{
			ManifestNatures = manifestNatures;
		}

		public ManifestType(string code, string description, string applicableTransportMode, IEnumerable<string> applicableManifestStyles, MessageLevel messageLevel, CodeDescriptionPairList manifestNatures)
		: this(code, description, new[] { applicableTransportMode }, applicableManifestStyles, messageLevel)
		{
			ManifestNatures = manifestNatures;
		}

		public CodeDescriptionPairList ManifestNatures { get; }
		object ICodeDescription.PK => null;
		public string Code { get; }
		public string Description { get; }
		public IEnumerable<string> ApplicableTransportModes { get; }
		public IEnumerable<string> ApplicableManifestStyles { get; }
		public MessageLevel MessageLevel { get; }
		public bool Enabled { get; set; } = true;

		public class EqualityComparer : EqualityComparer<IManifestType>
		{
			public override bool Equals(IManifestType x, IManifestType y)
			{
				return x != null && y != null &&
					x.Code == y.Code &&
					x.Description == y.Description &&
					x.ApplicableTransportModes.ContainsSameElementsInAnyOrder(y.ApplicableTransportModes) &&
					x.ApplicableManifestStyles.ContainsSameElementsInAnyOrder(y.ApplicableManifestStyles) &&
					x.MessageLevel == y.MessageLevel &&
					(x.ManifestNatures != null && y.ManifestNatures != null ? x.ManifestNatures.ToArray().ContainsSameElementsInAnyOrder(y.ManifestNatures.ToArray()) : (x.ManifestNatures == null && y.ManifestNatures == null));
			}

			public override int GetHashCode(IManifestType obj) => obj?.GetHashCode() ?? -1;
		}
	}
}
