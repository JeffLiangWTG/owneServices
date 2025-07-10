using System.Collections;
using System.Xml;

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.IssueManager.Module
{
	class ErrorLogStatusFilter : ModuleTextFilter
	{
		public ErrorLogStatusFilter(ZString description, GetTextQuery queryDelegate, IList list)
			: base(description, queryDelegate, list)
		{
		}

		#region Status Property

		public override ZString Property
		{
			get
			{
				return base.Property;
			}
			set
			{
				base.Property = value;
				SetPrefixAndSuffixLabelText();
				RefreshBinding();
			}
		}

		void SetPrefixAndSuffixLabelText()
		{
			switch (base.Property)
			{
				case CodeConstants.Fixed:
					PrefixText = "In the last";
					SuffixText = "days";
					break;
			}
		}

		#endregion

		#region Labels Text

		ZString prefixText = "For Over";

		public ZString PrefixText
		{
			get
			{
				return prefixText;
			}
			set
			{
				prefixText = value;
				PrefixTextInfo.RefreshBinding();
				InvalidateCachedQuery();
			}
		}

		public ZPropertyInfo PrefixTextInfo
		{
			get { return GetZPropertyInfo(nameof(PrefixText)); }
		}

		ZString suffixText = "hours";

		public ZString SuffixText
		{
			get
			{
				return suffixText;
			}
			set
			{
				suffixText = value;
				SuffixTextInfo.RefreshBinding();
				InvalidateCachedQuery();
			}
		}

		public ZPropertyInfo SuffixTextInfo
		{
			get { return GetZPropertyInfo(nameof(SuffixText)); }
		}

		#endregion

		#region Related Time Frame

		ZDecimal relatedTimeFrame;

		public ZDecimal RelatedTimeFrame
		{
			get
			{
				return relatedTimeFrame;
			}
			set
			{
				if (value <= 10000)
				{
					relatedTimeFrame = value;
					RelatedTimeFrameInfo.RefreshBinding();
					InvalidateCachedQuery();
				}
			}
		}

		public ZPropertyInfo RelatedTimeFrameInfo
		{
			get { return GetZPropertyInfo(nameof(RelatedTimeFrame)); }
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList StatusList
		{
			get { return base.List as CodeDescriptionPairList; }
		}

		public static class DescriptionConstants
		{
			public const string FirstReported = "First Reported";
			public const string LastReported = "Last Reported";
			public const string FirstProcessed = "First Processed";
			public const string Fixed = "Fixed";
			public const string Ignored = "Ignored";
			public const string Occurrences = "Occurrences";
			public const string NotFixed = "Not Fixed";
			public const string All = "All";
		}

		public static class CodeConstants
		{
			public const string Fixed = "FIX";
			public const string Ignored = "IGN";
			public const string NotFixed = "NOF";
		}

		#endregion

		#region XML Serialization

		protected override void SerializePropertiesToXml(XmlWriter writer)
		{
			base.SerializePropertiesToXml(writer);
			writer.WriteElementString("RelatedTimeFrame", RelatedTimeFrame.ToString());
		}

		protected override void DeserializePropertiesFromXml(XmlReader reader)
		{
			base.DeserializePropertiesFromXml(reader);
			RelatedTimeFrame = ZDecimal.ParseSafe(reader.ReadElementString("RelatedTimeFrame"), 0);
		}

		#endregion
	}
}
