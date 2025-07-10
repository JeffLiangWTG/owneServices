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
	public partial class CreateMissingProductsInfo : RegistryBusinessObjectTemplate
	{
		public static class Schema
		{
			public const string IsOverrideToYes = "IsOverrideToYes";
			public const string DefaultRelationship = "DefaultRelationship";
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			DefaultRelationship = DefaultRalationshipCodes.Owner;
		}

		#region Properties

		public ZBool IsOverrideToYes
		{
			get { return isOverrideToYes; }
			set
			{
				if (IsOverrideToYes != value)
				{
					if (!value)
					{
						DefaultRelationship = DefaultRalationshipCodes.Owner;
					}
					SetNonPersistentPropertyValue(IsOverrideToYesInfo, ref isOverrideToYes, value);
				}
			}
		}
		ZBool isOverrideToYes;

		public ZPropertyInfo IsOverrideToYesInfo
		{
			get { return GetZPropertyInfo(Schema.IsOverrideToYes); }
		}

		[List("DefaultRalationshipLookup")]
		[MaxLength(3)]
		public ZString DefaultRelationship
		{
			get { return defaultRelationship; }
			set
			{
				SetNonPersistentPropertyValue(DefaultRelationshipInfo, ref defaultRelationship, value);

				if (!IsValidationSuspended)
				{
					ValidateDefaultRalationship();
				}
			}
		}
		ZString defaultRelationship;

		public ZPropertyInfo DefaultRelationshipInfo
		{
			get { return GetZPropertyInfo(Schema.DefaultRelationship); }
		}

		public void ValidateDefaultRalationship()
		{
			DefaultRelationshipInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DefaultRelationshipInfo);
			if (DefaultRelationship != DefaultRalationshipCodes.Both)
			{
				ListValidation.ErrorIfInvalidCode(DefaultRelationshipInfo, DefaultRalationshipLookup);
			}
			if (IsOverrideToYes && DefaultRelationship == DefaultRalationshipCodes.Both)
			{
				DefaultRelationshipInfo.AddMessageError(Res.GetString("132DD013-6388-4D67-ACF5-3C76884F33BA", "BTH – Both Owner and Supplier is no longer valid, please select either 'OWN – Owner' or 'SUP – Supplier'."));
			}
		}

		public CodeDescriptionPairList DefaultRalationshipLookup
		{
			get
			{
				if (defaultRalationshipLookup == null)
				{
					defaultRalationshipLookup = new CodeDescriptionPairList();
					defaultRalationshipLookup.Add(new CodeDescriptionPair(DefaultRalationshipCodes.Owner, Res.GetString("B93C2468-A892-49E8-84C9-CD1FE9F53630", "Owner")));
					defaultRalationshipLookup.Add(new CodeDescriptionPair(DefaultRalationshipCodes.Supplier, Res.GetString("21B04A03-A123-471A-992D-19A90133A5D8", "Supplier")));
				}
				return defaultRalationshipLookup;
			}
		}
		CodeDescriptionPairList defaultRalationshipLookup;

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CreateMissingProductsInfo();
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.IsOverrideToYes, IsOverrideToYes.ToString());
			writer.WriteElementString(Schema.DefaultRelationship, DefaultRelationship);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			IsOverrideToYes = new ZBool(reader.ReadElementString(Schema.IsOverrideToYes));
			DefaultRelationship = reader.ReadElementString(Schema.DefaultRelationship);
		}
	}

	public static class DefaultRalationshipCodes
	{
		public const string Owner = "OWN";
		public const string Supplier = "SUP";
		public const string Both = "BTH";
	}
}
