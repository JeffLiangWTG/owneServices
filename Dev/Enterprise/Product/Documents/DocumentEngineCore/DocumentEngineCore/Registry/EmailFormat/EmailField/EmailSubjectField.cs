using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class EmailSubjectField : EmailField
	{
		public EmailSubjectField()
		{
		}

		public EmailSubjectField(ZString index, ZString code)
			: base(index, code)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EmailSubjectField();
		}

		protected override EmailFieldCollection ParentCollection
		{
			get
			{
				return (EmailSubjectFieldCollection)GetParentCollection(this, typeof(EmailSubjectFieldCollection));
			}
		}

		#region Bound Properties

		#region LookUps
		public override CodeDescriptionPairList EmailFieldsPairList
		{
			get
			{
				return DocumentsDataRegistry.Instance.EmailSubjectFieldsPairList;
			}
		}
		#endregion

		#endregion
	}
}
