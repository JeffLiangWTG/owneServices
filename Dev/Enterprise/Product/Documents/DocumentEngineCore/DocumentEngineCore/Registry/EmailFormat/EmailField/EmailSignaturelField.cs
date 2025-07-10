using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class EmailSignatureField : EmailField
	{
		public EmailSignatureField()
		{
		}

		public EmailSignatureField(ZString index, ZString code)
			: base(index, code)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EmailSignatureField();
		}

		protected override EmailFieldCollection ParentCollection
		{
			get
			{
				return (EmailSignatureFieldCollection)GetParentCollection(this, typeof(EmailSignatureFieldCollection));
			}
		}

		#region Bound Properties

		#region LookUps

		public override CodeDescriptionPairList EmailFieldsPairList
		{
			get
			{
				return DocumentsDataRegistry.Instance.EmailSignatureFieldsPairList;
			}
		}

		#endregion

		#endregion
	}
}
