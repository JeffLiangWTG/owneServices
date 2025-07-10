using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class HVLVDetailsPreScreeningConfiguration : RegistryBusinessObjectTemplate
	{
		#region Schema

		protected abstract class Schema
		{
			public const string IsEnabled = "IsEnabled";
		}

		#endregion

		public HVLVDetailsPreScreeningConfiguration()
		: base()
		{
		}

		public HVLVDetailsPreScreeningConfiguration(FallbackLevel fallbackLevel)
		: base(fallbackLevel)
		{
		}

		#region IsNeedHVLVPreScreening

		public ZBool IsEnabled
		{
			get { return isEnabled; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(IsEnabledInfo, ref isEnabled, value);
			}
		}

		public ZPropertyInfo IsEnabledInfo
		{
			get { return GetZPropertyInfo(Schema.IsEnabled); }
		}

		public static ResourceStringData IsEnabledUICaption => Res.GetData("1c735d73-7051-40b0-adca-f133a8d96dc2", "Enable HVLV Pre-Screening");

		ZBool isEnabled;

		#endregion

		#region Cloning

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new HVLVDetailsPreScreeningConfiguration(fallbackLevel);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var castedClone = (HVLVDetailsPreScreeningConfiguration)clone;

			if (rules != null)
			{
				castedClone.rules = (HVLVPreScreeningRuleCollection)Rules.Clone(castedClone.CurrentFallbackLevel, castedClone.Factory);
				castedClone.RegisterEditableChildObject(castedClone.Rules);
			}
		}

		#endregion

		#region XML Serialisation

		ZXmlSerializer RulesSerialiser
		{
			get { return rulesSerialiser ?? (rulesSerialiser = ZXmlSerializer.New(typeof(HVLVPreScreeningRuleCollection))); }
		}
		ZXmlSerializer rulesSerialiser;

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.IsEnabled, IsEnabled.ToString());
			RulesSerialiser.Serialize(writer, Rules);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			IsEnabled = new ZBool(reader.ReadElementString(Schema.IsEnabled));
			rules = (HVLVPreScreeningRuleCollection)RulesSerialiser.Deserialize(reader);
			RegisterEditableChildObject(rules);
		}

		#endregion

		[BusinessObjectTestExclude]
		public HVLVPreScreeningRuleCollection Rules
		{
			get
			{
				if (rules == null)
				{
					rules = new HVLVPreScreeningRuleCollection();
					RegisterEditableChildObject(rules);
				}
				return rules;
			}
		}
		HVLVPreScreeningRuleCollection rules;

		public HVLVPreScreeningRule[] GetMostMatchedPreScreeningRules(string tableCode, ZString transportMode, ZString eTailer, ZString originCountry, ZString destinationCountry)
		{
			var dic = new Dictionary<string, string>();
			var screeningRules = Enumerable.Empty<HVLVPreScreeningRule>();

			dic.Add("ETailer", eTailer);
			dic.Add("OriginCountryCode", originCountry);
			dic.Add("DestinationCountryCode", destinationCountry);

			if (tableCode == JobShipmentSchema.Constants.Prefix)
			{
				dic.Add("TransportMode", transportMode);
				screeningRules = Rules.Cast<HVLVPreScreeningRule>().Where(n => n.isApplyShipment);
			}
			else if (tableCode == HVLVBookingHeaderSchema.Constants.Prefix)
			{
				screeningRules = Rules.Cast<HVLVPreScreeningRule>().Where(n => n.isApplyBookingHeader);
			}

			return GetMostMatchedPreScreeningRules(dic, screeningRules);
		}

		HVLVPreScreeningRule[] GetMostMatchedPreScreeningRules(Dictionary<string, string> preScreeningFields, IEnumerable<HVLVPreScreeningRule> screeningRules)
		{
			var matchedRules = new Dictionary<int, List<HVLVPreScreeningRule>>();
			var maxmatchedWeight = 0;
			foreach (HVLVPreScreeningRule preScreeningRule in screeningRules)
			{
				var matchedWeight = 0;
				var count = preScreeningFields.Count + 1;
				var matchingFound = true;
				foreach (var field in preScreeningFields)
				{
					var property = preScreeningRule.FindPropertyInfo(field.Key);
					if (!ShouldSkipForField(field))
					{
						if (property.Value.IsEmpty)
						{
							matchedWeight++;
						}
						else if (!property.Value.IsEmpty && property.Value.ToString() == field.Value)
						{
							matchedWeight += count;
						}
						else
						{
							matchingFound = false;
							break;
						}
					}
				}

				if (matchingFound)
				{
					if (matchedRules.TryGetValue(matchedWeight, out var matchedRuleList))
					{
						matchedRuleList.Add(preScreeningRule);
					}
					else
					{
						matchedRules[matchedWeight] = new List<HVLVPreScreeningRule>() { preScreeningRule };
					}
					maxmatchedWeight = Math.Max(matchedWeight, maxmatchedWeight);
				}
			}
			return maxmatchedWeight > 0 ? matchedRules[maxmatchedWeight].ToArray() : null;
		}

		bool ShouldSkipForField(KeyValuePair<string, string> field)
		{
			var shouldSkipForField = false;
			if (field.Key == "TransportMode" && string.IsNullOrEmpty(field.Value))
			{
				shouldSkipForField = true;
			}

			return shouldSkipForField;
		}
	}
}
