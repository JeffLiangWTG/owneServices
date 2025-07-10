using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	class DependenceCollectionProvider : CollectionProviderWithCodeSupport
	{
		public DependenceCollectionProvider(BusinessObjectFactory businessObjectFactory)
			: base(businessObjectFactory)
		{
		}

		protected override IBusinessObjectCollection CreateCollection()
		{
			if (List.ContainsKey(dependentValue))
			{
				return List[dependentValue].Collection;
			}

			return null;
		}

		protected override IBusinessObjectCollection GetCollectionForFindbox()
		{
			if (List.ContainsKey(dependentValue))
			{
				return List[dependentValue].CollectionForFindbox;
			}

			return base.GetCollectionForFindbox();
		}

		public override ZArchitecture.Modules.ModuleIdentifier ModuleID
		{
			get
			{
				if (List.ContainsKey(dependentValue))
				{
					return List[dependentValue].ModuleID;
				}

				return ModuleIDs.NotAssigned;
			}
		}

		public override int MaxLength
		{
			get
			{
				if (List.ContainsKey(dependentValue)
					&& List[dependentValue] is CollectionProviderWithCodeSupport providerWithCodeSupport)
				{
					return providerWithCodeSupport.MaxLength;
				}

				return -1;
			}
		}

		public override void SetDependencyValue(string value)
		{
			base.SetDependencyValue(value);
			dependentValue = value;
			NeedRebindCollections = NeedRebindCollectionsForFindBox = true;
		}
		ZString dependentValue = ZString.Empty;

		public Dictionary<string, CollectionProvider> List
		{
			get { return fList; }
		}
		readonly Dictionary<string, CollectionProvider> fList = new Dictionary<string, CollectionProvider>();
	}
}
