using System.Linq;
using System.Xml.Serialization;

using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ClientInTemplateSelectionCriteriaCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ClientInTemplateSelectionCriteriaCollection()
		{
		}

		public ClientInTemplateSelectionCriteriaCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new ClientInTemplateSelectionCriteria this[int i]
		{
			get { return (ClientInTemplateSelectionCriteria)Elements[i]; }
		}

		public new ClientInTemplateSelectionCriteria AddNew()
		{
			return (ClientInTemplateSelectionCriteria)base.AddNew();
		}

		public override bool ReadOnly
		{
			get { return true; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ClientInTemplateSelectionCriteria(CurrentFallbackLevel, Factory);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ClientInTemplateSelectionCriteriaCollection();
		}

		internal void AddMissingProcessTypesFromDefaultsIfMissing()
		{
			var defaults = ClientInTemplateSelectionCriteriaCollectionRegistryItem.DefaultCollectionValue
				.Cast<ClientInTemplateSelectionCriteria>();

			foreach (var criteriaForWorkflowCode in defaults)
			{
				if (GetValueByCode(criteriaForWorkflowCode.ProcessTaskCode) == null)
				{
					Add(criteriaForWorkflowCode);
				}
			}
		}

		#region GetValueByCode

		public ClientInTemplateSelectionCriteria GetValueByCode(string code)
		{
			return this.Cast<ClientInTemplateSelectionCriteria>().FirstOrDefault(x => x.ProcessTaskCode == code);
		}

		#endregion
	}
}
