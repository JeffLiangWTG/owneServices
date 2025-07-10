using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Common
{
	[XmlSerializerAssembly("Enterprise.Customs.JP.Common.XmlSerializers")]
	public class DefaultBrokerAndCredential : RegistryBusinessObjectTemplate
	{
		public DefaultBrokerAndCredential() : base()
		{
		}

		public DefaultBrokerAndCredential(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		#region Schema

		public static class Schema
		{
			public const string DefaultBrokerCode = "DefaultBrokerCode";
			public const string DefaultCredentialAIR = "DefaultCredentialAIR";
			public const string DefaultCredentialSEA = "DefaultCredentialSEA";
			public const string ForwarderManifestSEA = "ForwarderManifestSEA";
			public const string ForwarderManifestAIR = "ForwarderManifestAIR";
		}

		#endregion

		public new BusinessObjectFactory Factory => CurrentFactory;

		#region Overrides

		public override bool Equals(object obj)
		{
			return obj is DefaultBrokerAndCredential credential &&
				   EqualityComparer<ZString>.Default.Equals(DefaultBrokerCode, credential.DefaultBrokerCode) &&
				   EqualityComparer<ZString>.Default.Equals(ForwarderManifestSEA, credential.ForwarderManifestSEA) &&
				   EqualityComparer<ZString>.Default.Equals(ForwarderManifestAIR, credential.ForwarderManifestAIR) &&
				   EqualityComparer<ZString>.Default.Equals(DefaultCredentialSEA, credential.DefaultCredentialSEA) &&
				   EqualityComparer<ZString>.Default.Equals(DefaultCredentialAIR, credential.DefaultCredentialAIR);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = -1718027163;
				hashCode = hashCode * -1521134295 + base.GetHashCode();
				hashCode = hashCode * -1521134295 + DefaultBrokerCode.GetHashCode();
				hashCode = hashCode * -1521134295 + ForwarderManifestSEA.GetHashCode();
				hashCode = hashCode * -1521134295 + ForwarderManifestAIR.GetHashCode();
				hashCode = hashCode * -1521134295 + DefaultCredentialSEA.GetHashCode();
				hashCode = hashCode * -1521134295 + DefaultCredentialAIR.GetHashCode();
				return hashCode;
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory) => new DefaultBrokerAndCredential(fallbackLevel, factory);

		protected sealed override void ReadElements(XmlReaderWrapper reader)
		{
			DefaultBrokerCode = reader.ReadElementString(Schema.DefaultBrokerCode);
			DefaultCredentialSEA = reader.ReadElementString(Schema.DefaultCredentialSEA);
			DefaultCredentialAIR = reader.ReadElementString(Schema.DefaultCredentialAIR);
			ForwarderManifestSEA = reader.ReadElementString(Schema.ForwarderManifestSEA);
			ForwarderManifestAIR = reader.ReadElementString(Schema.ForwarderManifestAIR);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.DefaultBrokerCode, DefaultBrokerCode);
			writer.WriteElementString(Schema.DefaultCredentialSEA, DefaultCredentialSEA);
			writer.WriteElementString(Schema.DefaultCredentialAIR, DefaultCredentialAIR);
			writer.WriteElementString(Schema.ForwarderManifestSEA, ForwarderManifestSEA);
			writer.WriteElementString(Schema.ForwarderManifestAIR, ForwarderManifestAIR);
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();

			DefaultCredentialAIR = string.Empty;
			DefaultCredentialSEA = string.Empty;
			ForwarderManifestSEA = string.Empty;
			ForwarderManifestAIR = string.Empty;
		}

		#endregion

		#region Properties

		#region BrokerStaff

		[List(nameof(Lookups) + "." + nameof(DefaultBrokerAndCredentialLookups.DefaultBrokerCodeList))]
		[RelatedBusinessObject(nameof(DefaultBroker))]
		[ResourceStringData("Enterprise.Customs.JP.Common.DefaultBrokerAndCredential|DefaultBroker", Caption = "Default Broker")]
		public ZString DefaultBrokerCode
		{
			get => defaultBrokerCode;
			set
			{
				if (DefaultBrokerCode != value)
				{
					SetNonPersistentPropertyValue(DefaultBrokerCodeInfo, ref defaultBrokerCode, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateDefaultBrokerCode();
					}
					DefaultBrokerCodeInfo.RefreshBinding();
				}
			}
		}

		ZString defaultBrokerCode;

		public ZPropertyInfo DefaultBrokerCodeInfo => GetZPropertyInfo(Schema.DefaultBrokerCode);

		public GlbStaff DefaultBroker => CurrentFactory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, DefaultBrokerCode);

		#endregion

		#region Forwarder Manifest(SEA)

		[ResourceStringData("Enterprise.Customs.JP.Common.DefaultBrokerAndCredential|ForwarderManifestSEA", Caption = "Forwarder Manifest (SEA)")]
		[List(nameof(Lookups) + "." + nameof(DefaultBrokerAndCredentialLookups.CredentialSEAList))]
		public ZString ForwarderManifestSEA
		{
			get => forwarderManifestSEA;
			set
			{
				if (forwarderManifestSEA != value)
				{
					SetNonPersistentPropertyValue(ForwarderManifestSEAInfo, ref forwarderManifestSEA, value);
					ForwarderManifestSEAInfo.RefreshBinding();
				}
			}
		}

		ZString forwarderManifestSEA;

		public ZPropertyInfo ForwarderManifestSEAInfo => GetZPropertyInfo(Schema.ForwarderManifestSEA);

		#endregion

		#region Forwarder Manifest(AIR)

		[ResourceStringData("Enterprise.Customs.JP.Common.DefaultBrokerAndCredential|ForwarderManifestAIR", Caption = "Forwarder Manifest (AIR)")]
		[List(nameof(Lookups) + "." + nameof(DefaultBrokerAndCredentialLookups.CredentialAIRList))]
		public ZString ForwarderManifestAIR
		{
			get => forwarderManifestAIR;
			set
			{
				if (forwarderManifestAIR != value)
				{
					SetNonPersistentPropertyValue(ForwarderManifestAIRInfo, ref forwarderManifestAIR, value);
					ForwarderManifestAIRInfo.RefreshBinding();
				}
			}
		}

		ZString forwarderManifestAIR;

		public ZPropertyInfo ForwarderManifestAIRInfo => GetZPropertyInfo(Schema.ForwarderManifestAIR);

		#endregion

		#region Default Credential (SEA)

		[List(nameof(Lookups) + "." + nameof(DefaultBrokerAndCredentialLookups.CredentialSEAList))]
		[ResourceStringData("Enterprise.Customs.JP.Common.DefaultBrokerAndCredential|DefaultCredentialSEA", Caption = "Customs Declaration (SEA)")]
		public ZString DefaultCredentialSEA
		{
			get => defaultCredentialSEA;
			set
			{
				if (DefaultCredentialSEA != value)
				{
					SetNonPersistentPropertyValue(DefaultCredentialSEAInfo, ref defaultCredentialSEA, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateDefaultCredentialSEA();
					}
					DefaultCredentialSEAInfo.RefreshBinding();
				}
			}
		}

		ZString defaultCredentialSEA;

		public ZPropertyInfo DefaultCredentialSEAInfo => GetZPropertyInfo(Schema.DefaultCredentialSEA);

		#endregion

		#region Default Credential (AIR)

		[List(nameof(Lookups) + "." + nameof(DefaultBrokerAndCredentialLookups.CredentialAIRList))]
		[ResourceStringData("Enterprise.Customs.JP.Common.DefaultBrokerAndCredential|DefaultCredentialAIR", Caption = "Customs Declaration (AIR)")]
		public ZString DefaultCredentialAIR
		{
			get => defaultCredentialAIR;
			set
			{
				if (DefaultCredentialAIR != value)
				{
					SetNonPersistentPropertyValue(DefaultCredentialAIRInfo, ref defaultCredentialAIR, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateDefaultCredentialAIR();
					}
					DefaultCredentialAIRInfo.RefreshBinding();
				}
			}
		}

		ZString defaultCredentialAIR;

		public ZPropertyInfo DefaultCredentialAIRInfo => GetZPropertyInfo(Schema.DefaultCredentialAIR);

		#endregion

		#endregion

		#region Validation

		DefaultBrokerAndCredentialValidation validation;

		public DefaultBrokerAndCredentialValidation Validation => validation ?? (validation = new DefaultBrokerAndCredentialValidation(this));

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion

		#region Lookup

		DefaultBrokerAndCredentialLookups lookups;

		public DefaultBrokerAndCredentialLookups Lookups => lookups ?? (lookups = new DefaultBrokerAndCredentialLookups(this));

		#endregion
	}
}
