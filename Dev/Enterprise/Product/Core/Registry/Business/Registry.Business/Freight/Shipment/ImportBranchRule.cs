using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[DebuggerDisplay("UseBranchFromXml: {UseBranchFromXml} Origin: {DefaultToBranchRelatedToOriginLoadPort} Destination: {DefaultToBranchRelatedToDestinationDischargePort} Any: {DefaultToAny} DoNotCreate: {DoNotCreate}")]
	public class ImportBranchRule : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string UseBranchFromXml = "UseBranchFromXml";
			public const string DefaultToBranchRelatedToOriginLoadPort = "DefaultToBranchRelatedToOriginLoadPort";
			public const string DefaultToBranchRelatedToDestinationDischargePort = "DefaultToBranchRelatedToDestinationDischargePort";
			public const string FallbackRule = "FallbackRule";
		}

		#endregion

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			UseBranchFromXml = true;

			DefaultToBranchRelatedToOriginLoadPort = 1;
			DefaultToBranchRelatedToDestinationDischargePort = 2;
			FallbackRule = FallbackCodes.DefaultToAny;
		}

		public static class FallbackCodes
		{
			public const string DefaultToAny = "ANY";
			public const string DoNotCreate = "NOT";
		}

		public enum BranchSearchRules
		{
			FromInterchange,
			FromOriginLoadPort,
			FromOriginLoadPortCountry,
			FromDestinationDischargePort,
			FromDestinationDischargePortCountry,
			Any,
			Cancel
		}

		#region Properties

		public ZBool UseBranchFromXml
		{
			get { return useBranchFromXml; }
			set { SetNonPersistentPropertyValue(UseBranchFromXmlInfo, ref useBranchFromXml, value); }
		}
		ZBool useBranchFromXml;

		public ZPropertyInfo UseBranchFromXmlInfo
		{
			get { return GetZPropertyInfo(Schema.UseBranchFromXml); }
		}

		public ZShort DefaultToBranchRelatedToOriginLoadPort
		{
			get { return defaultToBranchRelatedToOriginLoadPort; }
			set
			{
				SetNonPersistentPropertyValue(DefaultToBranchRelatedToOriginLoadPortInfo, ref defaultToBranchRelatedToOriginLoadPort, value);

				if (!IsValidationSuspended)
				{
					ValidateDefaultToBranchRelatedToOriginLoadPort();
				}
			}
		}
		ZShort defaultToBranchRelatedToOriginLoadPort;

		public ZPropertyInfo DefaultToBranchRelatedToOriginLoadPortInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.DefaultToBranchRelatedToOriginLoadPort, "Default to Branch Related to Origin/Load Port");
			}
		}

		public void ValidateDefaultToBranchRelatedToOriginLoadPort()
		{
			DefaultToBranchRelatedToOriginLoadPortInfo.ClearAllNotifications();
			CompareValidation.CheckWithinRange(DefaultToBranchRelatedToOriginLoadPortInfo, 0, 2);
		}

		public ZShort DefaultToBranchRelatedToDestinationDischargePort
		{
			get { return defaultToBranchRelatedToDestinationDischargePort; }
			set
			{
				SetNonPersistentPropertyValue(DefaultToBranchRelatedToDestinationDischargePortInfo, ref defaultToBranchRelatedToDestinationDischargePort, value);

				if (!IsValidationSuspended)
				{
					ValidateDefaultToRelatedToDestinationDischargePort();
				}
			}
		}
		ZShort defaultToBranchRelatedToDestinationDischargePort;

		public ZPropertyInfo DefaultToBranchRelatedToDestinationDischargePortInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.DefaultToBranchRelatedToDestinationDischargePort, "Default to Branch Related to Destination/Discharge Port");
			}
		}

		public void ValidateDefaultToRelatedToDestinationDischargePort()
		{
			DefaultToBranchRelatedToDestinationDischargePortInfo.ClearAllNotifications();
			CompareValidation.CheckWithinRange(DefaultToBranchRelatedToDestinationDischargePortInfo, 0, 2);
		}

		[List("FallbackRuleLookup")]
		[MaxLength(3)]
		public ZString FallbackRule
		{
			get { return fallbackRule; }
			set
			{
				SetNonPersistentPropertyValue(FallbackRuleInfo, ref fallbackRule, value);

				if (!IsValidationSuspended)
				{
					ValidateFallbackRule();
				}
			}
		}
		ZString fallbackRule;

		public ZPropertyInfo FallbackRuleInfo
		{
			get { return GetZPropertyInfo(Schema.FallbackRule); }
		}

		public void ValidateFallbackRule()
		{
			FallbackRuleInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(FallbackRuleInfo);
			ListValidation.ErrorIfInvalidCode(FallbackRuleInfo, FallbackRuleLookup);
		}

		public CodeDescriptionPairList FallbackRuleLookup
		{
			get
			{
				if (fallbackRuleLookup == null)
				{
					fallbackRuleLookup = new CodeDescriptionPairList();
					fallbackRuleLookup.Add(new CodeDescriptionPair(FallbackCodes.DefaultToAny, Res.GetString("6676ec60-64dc-48ec-a407-750922e451f5", "Default to any")));
					fallbackRuleLookup.Add(new CodeDescriptionPair(FallbackCodes.DoNotCreate, Res.GetString("81f0e800-fe30-4e5c-a788-3eddcbd065bb", "Do not create")));
				}
				return fallbackRuleLookup;
			}
		}
		CodeDescriptionPairList fallbackRuleLookup;

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ClearRowNotifications();

			ValidateDefaultToBranchRelatedToOriginLoadPort();
			ValidateDefaultToRelatedToDestinationDischargePort();
			ValidateFallbackRule();
			ValidateCorrectValuesEntered();

			if (!UseBranchFromXml && DefaultToBranchRelatedToOriginLoadPort == 0 && DefaultToBranchRelatedToDestinationDischargePort == 0 && FallbackRule == FallbackCodes.DoNotCreate)
			{
				AddRowError(Res.GetString("ae302d7a-713e-4723-8556-cffdb6851951", "According to current setting shipments will never be created."));
			}
		}

		void ValidateCorrectValuesEntered()
		{
			ZPropertyInfo[] propertyInfos = new[] { DefaultToBranchRelatedToOriginLoadPortInfo, DefaultToBranchRelatedToDestinationDischargePortInfo };

			var values = propertyInfos
				.Select(propertyInfo => new
				{
					Name = propertyInfo.HumanReadableName,
					Value = (ZShort)propertyInfo.Value
				})
				.Where(info => info.Value > 0)
				.OrderBy(info => info.Value)
				.ToArray();

			for (int i = 0; i < values.Length; i++)
			{
				if (values[i].Value != i + 1)
				{
					AddRowError(Res.GetString("9809e217-6018-4870-a224-f750ba527b14", "{0} has an invalid value.", values[i].Name));
					break;
				}
			}
		}

		#endregion

		public IEnumerable<BranchSearchRules> GetBranchSearchRules()
		{
			if (UseBranchFromXml)
			{
				yield return BranchSearchRules.FromInterchange;
			}

			if (DefaultToBranchRelatedToOriginLoadPort > 0 && DefaultToBranchRelatedToDestinationDischargePort > 0 &&
				DefaultToBranchRelatedToOriginLoadPort < DefaultToBranchRelatedToDestinationDischargePort)
			{
				yield return BranchSearchRules.FromOriginLoadPort;
				yield return BranchSearchRules.FromDestinationDischargePort;
				yield return BranchSearchRules.FromOriginLoadPortCountry;
				yield return BranchSearchRules.FromDestinationDischargePortCountry;
			}
			else if (DefaultToBranchRelatedToOriginLoadPort > 0 && DefaultToBranchRelatedToDestinationDischargePort > 0)
			{
				yield return BranchSearchRules.FromDestinationDischargePort;
				yield return BranchSearchRules.FromOriginLoadPort;
				yield return BranchSearchRules.FromDestinationDischargePortCountry;
				yield return BranchSearchRules.FromOriginLoadPortCountry;
			}
			else if (DefaultToBranchRelatedToOriginLoadPort > 0)
			{
				yield return BranchSearchRules.FromOriginLoadPort;
				yield return BranchSearchRules.FromOriginLoadPortCountry;
			}
			else if (DefaultToBranchRelatedToDestinationDischargePort > 0)
			{
				yield return BranchSearchRules.FromDestinationDischargePort;
				yield return BranchSearchRules.FromDestinationDischargePortCountry;
			}

			if (FallbackRule == FallbackCodes.DefaultToAny)
			{
				yield return BranchSearchRules.Any;
			}
			else
			{
				yield return BranchSearchRules.Cancel;
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ImportBranchRule();
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.UseBranchFromXml, UseBranchFromXml.ToString());
			writer.WriteElementString(Schema.DefaultToBranchRelatedToOriginLoadPort, DefaultToBranchRelatedToOriginLoadPort.ToString());
			writer.WriteElementString(Schema.DefaultToBranchRelatedToDestinationDischargePort, DefaultToBranchRelatedToDestinationDischargePort.ToString());
			writer.WriteElementString(Schema.FallbackRule, FallbackRule);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			UseBranchFromXml = new ZBool(reader.ReadElementString(Schema.UseBranchFromXml));
			DefaultToBranchRelatedToOriginLoadPort = new ZShort(reader.ReadElementString(Schema.DefaultToBranchRelatedToOriginLoadPort));
			DefaultToBranchRelatedToDestinationDischargePort = new ZShort(reader.ReadElementString(Schema.DefaultToBranchRelatedToDestinationDischargePort));
			FallbackRule = reader.ReadElementString(Schema.FallbackRule);
		}
	}
}
