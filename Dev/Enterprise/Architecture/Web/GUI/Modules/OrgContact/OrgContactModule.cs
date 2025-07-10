using System;
using System.Web.UI.WebControls;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Res = Enterprise.ZArchitecture.Web.GUI.Res;

namespace Enterprise.ZArchitecture.Web.Modules
{
	public class OrgContactModule : SimpleModule
	{
		public OrgContactModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID => WebModuleIDs.OrgContact;

		public override Type FilterBusinessObjectType => typeof(OrgContactFilterBusinessObject);

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter) => new[] { new ColumnAndSortOrder(OrgContactSchema.OC_ContactName.Name, DefaultSortOrder) };

		public override Type GridCollectionType => typeof(OrgContactCollection);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "command name")]
		protected override DataGridColumn[] GetNewGridColumnFields() => new DataGridColumn[]
		{
			new ZButtonColumn(Res.GetString("f6b930e3-634c-487a-9b77-dc0102063ed4", "Name"), OrgContactSchema.OC_ContactName.Name) { CommandName = "Select" },
			new ZTextEditColumn(Res.GetString("0193bd1f-6b80-4544-9ee4-2244c6b2a116", "Title"), OrgContactSchema.OC_Title.Name),
		};
	}
}
