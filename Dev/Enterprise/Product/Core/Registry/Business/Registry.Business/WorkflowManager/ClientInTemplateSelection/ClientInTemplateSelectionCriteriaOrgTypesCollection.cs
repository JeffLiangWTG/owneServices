using System.Collections;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ClientInTemplateSelectionCriteriaOrgTypesCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ClientInTemplateSelectionCriteriaOrgTypesCollection()
		{
		}

		public ClientInTemplateSelectionCriteriaOrgTypesCollection(ClientInTemplateSelectionCriteria parent, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
			Parent = parent;
		}

		public new ClientInTemplateSelectionCriteriaOrgType this[int i]
		{
			get { return (ClientInTemplateSelectionCriteriaOrgType)Elements[i]; }
		}

		public new ClientInTemplateSelectionCriteriaOrgType AddNew()
		{
			return (ClientInTemplateSelectionCriteriaOrgType)base.AddNew();
		}

		public override bool ReadOnly
		{
			get { return true; }
		}

		public ClientInTemplateSelectionCriteriaOrgType GetValueByCode(string code)
		{
			return this.Cast<ClientInTemplateSelectionCriteriaOrgType>().FirstOrDefault(x => x.OrgTypeCode == code);
		}

		#region ContainsCode

		public bool ContainsCode(ZString orgTypeCode)
		{
			return this.Any(x => ((ClientInTemplateSelectionCriteriaOrgType)x).OrgTypeCode == orgTypeCode);
		}

		#endregion

		#region MoveItem

		public void MoveItem(int fromIndex, int toIndex)
		{
			if (Count < fromIndex || Count < toIndex)
			{
				return;
			}

			var list = (IList)this;
			var itemToMove = list[fromIndex];
			list.RemoveAt(fromIndex);
			list.Insert(toIndex, itemToMove);
		}

		#endregion

		#region Implementation

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ClientInTemplateSelectionCriteriaOrgType(CurrentFallbackLevel, CurrentFactory);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ClientInTemplateSelectionCriteriaOrgTypesCollection(Parent, fallbackLevel, factory);
		}

		public ClientInTemplateSelectionCriteria Parent { get; set; }

		#endregion
	}
}
