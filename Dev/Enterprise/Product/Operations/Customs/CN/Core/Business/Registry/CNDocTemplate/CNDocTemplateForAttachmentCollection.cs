using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CN.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.CN.Business.XmlSerializers")]
	public class CNDocTemplateForAttachmentCollection : RegistryBusinessObjectCollectionTemplate
	{
		public CNDocTemplateForAttachmentCollection()
			: base(new BusinessObjectFactory { NameForDebugging = "CNDocTemplateForAttachmentCollection_Ctor" })
		{
		}

		public CNDocTemplateForAttachmentCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		: base(fallbackLevel, factory)
		{
		}

		public new CNDocTemplateForAttachment this[int i] => (CNDocTemplateForAttachment)Elements[i];

		public new CNDocTemplateForAttachment AddNew() => (CNDocTemplateForAttachment)base.AddNew();

		protected override BusinessObject CreateNonPersistentBusinessObject()
				=> new CNDocTemplateForAttachment(CurrentFallbackLevel, CurrentFactory);

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
				=> new CNDocTemplateForAttachmentCollection(fallbackLevel, factory);

		#region Defaults

		public static CNDocTemplateForAttachmentCollection GetDefault()
		{
			var result = new CNDocTemplateForAttachmentCollection();

			SetupDefaultFields(result.AddNew(), ZGuid.Empty, ".CustomsDeclarationDocument", (NoResString)"CN Customs Invoice(System)", "MSC", (NoResString)"Miscellaneous Document", "00000001");
			SetupDefaultFields(result.AddNew(), ZGuid.Empty, ".CustomsDeclarationDocument", (NoResString)"CN Purchase Order(System)", "MSC", (NoResString)"Miscellaneous Document", "00000004");

			return result;
		}

		static void SetupDefaultFields(CNDocTemplateForAttachment defaultDocTemplate, ZGuid organizationPK, ZString dataContext, ZString documentTemplate, ZString documentType, ZString documentDescription, ZString attachmentType)
		{
			using (defaultDocTemplate.GetValidationSuspender())
			{
				defaultDocTemplate.OrganizationPK = organizationPK;
				defaultDocTemplate.DataContext = dataContext;
				defaultDocTemplate.DocumentTemplate = documentTemplate;
				defaultDocTemplate.DocumentType = documentType;
				defaultDocTemplate.DocumentDescription = documentDescription;
				defaultDocTemplate.AttachmentType = attachmentType;
			}
		}

		#endregion
	}
}
