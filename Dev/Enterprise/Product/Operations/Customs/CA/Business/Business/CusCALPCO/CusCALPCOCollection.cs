using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CusCALPCOCollection : DependentBusinessObjectCollection<CusCALPCO, BusinessObject>
	{
		public CusCALPCOCollection(BusinessObject master) : base(master)
		{
		}

		protected override void SetCollectionRelationships(BusinessObject dependent)
		{
			base.SetCollectionRelationships(dependent);
			var child = (CusCALPCO)dependent;
			child.Parent = Master;
		}

		public void AddIfTypeNotExist(string type)
		{
			if (this.Cast<CusCALPCO>().All(x => x.CLP_Type != type))
			{
				var lpco = AddNew();
				lpco.CLP_Type = type;
			}
		}

		protected override SchemaGuidColumn FKSchemaColumnInDependent => CusCALPCOSchema.CLP_ParentID;

		public void CopyPersistentValuesFrom(CusCALPCOCollection source)
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
