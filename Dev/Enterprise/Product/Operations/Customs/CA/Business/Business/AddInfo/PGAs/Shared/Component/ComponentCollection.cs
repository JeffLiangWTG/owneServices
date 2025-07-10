using CargoWise.EntityFramework;
using Enterprise.Customs.Business.MultiLineAddInfos;

namespace Enterprise.Customs.CA.Business
{
	public class ComponentCollection : DependentCusAddInfoCollection<Component, BusinessObject>
	{
		public ComponentCollection(BusinessObject master)
			: base(master, CusAddInfoTypeAttribute.Codes.CAComponent)
		{
		}

		public void CopyPersistentValuesFrom(ComponentCollection source)
		{
			RemoveAndDeleteAll();
			foreach (BusinessObject sourceElement in source)
			{
				BusinessObject targetElement = AddNew();
				targetElement.CopyPersistentValuesFrom(sourceElement);
			}
		}
	}
}
