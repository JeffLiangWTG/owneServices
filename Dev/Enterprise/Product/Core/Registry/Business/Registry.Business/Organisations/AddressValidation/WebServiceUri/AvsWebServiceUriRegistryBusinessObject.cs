using System.ComponentModel;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class AvsWebServiceUriRegistryBusinessObject : CodeDescriptionBool
	{
		public string ServiceUri
		{
			get => EnglishDescription;
			set => EnglishDescription = value;
		}

		public bool EnableSystemToSystemTrustAuthentication
		{
			get => Bool;
			set => Bool = value;
		}

		public string Type
		{
			get => Code;
			set => Code = value;
		}

		[ReadOnly(true)]
		public override ZString Code {
			get => base.Code;
			set => base.Code = value;
		}

		[ReadOnlyMember(nameof(CodeAndDescriptionReadOnly))]
		public override ZString EnglishDescription {
			get => FormatServiceUri(base.EnglishDescription);
			set => base.EnglishDescription = FormatServiceUri(value);
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AvsWebServiceUriRegistryBusinessObject
			{
				ServiceUri = ServiceUri,
				EnableSystemToSystemTrustAuthentication = EnableSystemToSystemTrustAuthentication,
				Type = Type
			};
		}

		protected override int CodeMaxLengthDefaultValue => 10;

		protected virtual string FormatServiceUri(string uri)
		{
			var formatedUri = uri?.Trim();
			if (!string.IsNullOrEmpty(formatedUri) && !formatedUri.EndsWith("/"))
			{
				formatedUri += "/";
			}

			return formatedUri;
		}
	}
}
