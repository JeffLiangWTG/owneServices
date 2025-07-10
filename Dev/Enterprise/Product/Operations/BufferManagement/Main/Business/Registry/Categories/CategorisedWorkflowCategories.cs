using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.BufferManagement.Business
{
	[XmlSerializerAssembly("Enterprise.BufferManagement.Business.XmlSerializers")]
	[CodeProperty("Code"), DescriptionProperty("Description")]
	public class CategorisedWorkflowCategories : RegistryBusinessObject, ICategorisedRegistryBusinessObjectCollection
	{
		public void SetCategories(WorkflowCategoryCollection categoriesToSet)
		{
			UnRegisterEditableChildObject(Categories);
			categories = categoriesToSet;
			RegisterEditableChildObject(Categories);
		}

		[ChildEditable]
		public WorkflowCategoryCollection Categories
		{
			get
			{
				if (categories == null)
				{
					categories = new WorkflowCategoryCollection();
					RegisterEditableChildObject(categories);
				}
				return categories;
			}
		}

		WorkflowCategoryCollection categories;

		protected override int MaxDescriptionLength => 256;

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CategorisedWorkflowCategories();
		}

		protected override void CopyCollectionsToClone(RegistryBusinessObjectTemplate clone, FallbackLevel currentFallbackLevel,
			BusinessObjectFactory factory)
		{
			base.CopyCollectionsToClone(clone, currentFallbackLevel, factory);

			var category = (CategorisedWorkflowCategories)clone;

			category.Categories.RemoveAll();
			category.Categories.AddRange((BusinessObjectCollection)Categories.Clone(currentFallbackLevel, factory));
		}

		#endregion

		#region Xml Serialization

		protected override void ReadMoreElements(XmlReader reader)
		{
			base.ReadMoreElements(reader);
			SetCategories((WorkflowCategoryCollection)CollectionSerialiser.Deserialize(reader));
		}

		protected override void WriteMoreElements(XmlWriter writer)
		{
			base.WriteMoreElements(writer);
			CollectionSerialiser.Serialize(writer, Categories);
		}

		ZXmlSerializer CollectionSerialiser => collectionSerialiser ?? (collectionSerialiser = ZXmlSerializer.New(typeof(WorkflowCategoryCollection)));

		ZXmlSerializer collectionSerialiser;

		#endregion ICategorisedRegistryBusinessObjectCollection

		#region

		public RegistryBusinessObjectCollection InnerCollection => Categories;

		#endregion
	}
}
