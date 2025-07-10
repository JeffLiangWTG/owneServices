using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	public class StmModuleFilterCollection : ActiveBusinessObjectCollection<StmModuleFilter>
	{
		public StmModuleFilterCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public StmModuleFilterCollection(BusinessObjectFactory factory, ZString identifier, FilterStripLayoutsHelper layoutHelper, ZQuery additionalFilter = null)
			: this(factory, identifier, layoutHelper, ZGuid.Empty, additionalFilter)
		{ }

		public StmModuleFilterCollection(BusinessObjectFactory factory, ZString identifier, FilterStripLayoutsHelper layoutHelper, ZGuid additionalItemPK, ZQuery additionalFilter = null)
			: base(factory, additionalFilter)
		{
			if (!Globals.IsTest && identifier.IsEmpty)
			{
				throw new ArgumentException("LayoutContext (ModuleID) cannot be empty when constructing an StmModuleFilterCollection.");
			}

			Identifier = identifier;
			LayoutHelper = layoutHelper;
			AdditionalItemPK = additionalItemPK;
		}

		readonly ZString Identifier;
		readonly FilterStripLayoutsHelper LayoutHelper;
		readonly ZGuid AdditionalItemPK;

		#region SetDefaultsForNewElementCore

		protected override void SetDefaultsForNewElementCore(StmModuleFilter newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);

			newElement.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			newElement.S9_ModuleID = Identifier;

			// for web, it is OrgContact
			newElement.S9_RelatedEntityID = LayoutHelper.CurrentUserPk;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(new StmModuleFilter.Loader(Factory).GetQuery(Identifier, LayoutHelper));

			if (!AdditionalItemPK.IsEmpty && AdditionalItemPK.IsValid)
			{
				result.AddToFilter(JoinCondition.Or, StmModuleFilterSchema.PK, AdditionalItemPK);
			}

			return result;
		}

		#endregion
	}
}
