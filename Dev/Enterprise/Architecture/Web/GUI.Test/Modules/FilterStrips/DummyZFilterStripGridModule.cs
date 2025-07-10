using System;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.WebControls;

namespace Enterprise.ZArchitecture.Web.Modules.Testing
{
	public class DummyZFilterStripGridModule : ZFilterStripGridModule
	{
		public DummyZFilterStripGridModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override Type GridCollectionType
		{
			get { return null; }
		}

		protected override DataGridColumn[] GetNewGridColumnFields()
		{
			return null;
		}

		protected override ZQuery GetCurrentLoggedInUserFilter(FilterBusinessObject filterBizO)
		{
			return new ZQuery();
		}

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter)
		{
			return null;
		}

		public override ModuleIdentifier ID
		{
			get { return DummyModuleIDs.Dummy; }
		}

		protected override SchemaPKColumn RelevantPersistantPKColumn
		{
			get
			{
				return null;
			}
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return null;
		}

		public void AddToDictionaryForTest(DataGridColumn value)
		{
			ColumnProvider.AddToDictionary(value as IUniqueKeyColumn);
		}

		public void AddToDictionaryForTest(ZTemplateColumn column)
		{
			ColumnProvider.AddToDictionary(column);
		}

		protected override GridColumnProvider GetColumnProvider()
		{
			return new GridColumnProvider();
		}
	}
}
