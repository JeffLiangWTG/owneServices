using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business
{
	[XmlSerializerAssembly("Enterprise.BufferManagement.Business.XmlSerializers")]
	public class WorkflowCategoryCollection : RegistryBusinessObjectCollection, ICodeDescriptionPairList
	{
		public new WorkflowCategory this[int i] => (WorkflowCategory)base[i];

		public new WorkflowCategory AddNew() => (WorkflowCategory)base.AddNew();

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			=> new WorkflowCategoryCollection();

		protected override BusinessObject CreateNonPersistentBusinessObject() => new WorkflowCategory();

		#region ICodeDescriptionPairList Members

		bool ICodeDescriptionPairList.ContainsCode(object code) => ContainsCode(code.ToString());

		string ICodeDescriptionPairList.GetDescriptionFromCode(string code)
		{
			var element = FindByCode(code);
			return (element != null) ? element.Description : ZString.Empty;
		}

		#endregion
	}
}


