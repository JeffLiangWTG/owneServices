using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.DocumentEngineCore.Registry.DocumentSigningRegistryConstants;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class DocumentSigningServiceCredentialsWithProviderConfiguration : DocumentSigningServiceCredentials
	{
		#region Schema

		public new abstract class Schema : DocumentSigningServiceCredentials.Schema
		{
			public const string ProviderCode = "ProviderCode";
		}

		#endregion

		#region Properties

		string providerCode;

		[MaxLength(3)]
		[List("ProviderCodesList")]
		public override ZString ProviderCode
		{
			get { return providerCode; }
			set
			{
				if (providerCode != value)
				{
					CheckMaximumLength(ProviderCodeInfo, value);
					providerCode = value;
					if (!IsValidationSuspended)
					{
						ValidateProviderCode();
					}
					ProviderCodeInfo.RefreshBinding();
					HasChanges = true;
				}
			}
		}

		public ZPropertyInfo ProviderCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ProviderCode); }
		}

		public CodeDescriptionPairList ProviderCodesList => new PdfSigningOptionList();

		#endregion

		#region Validation
		public void ValidateProviderCode()
		{
			ProviderCodeInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(ProviderCodeInfo);
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateProviderCode();
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DocumentSigningServiceCredentialsWithProviderConfiguration();
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteElementString(Schema.ProviderCode, ProviderCode);
			base.WriteElements(writer);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ProviderCode = reader.ReadElementString(Schema.ProviderCode);
			base.ReadElements(reader);
		}

		#endregion
	}
}
