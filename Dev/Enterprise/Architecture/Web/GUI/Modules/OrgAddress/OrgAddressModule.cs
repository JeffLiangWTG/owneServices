using System;
using System.Linq;
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
	public class OrgAddressModule : SimpleModule
	{
		public OrgAddressModule(BusinessObjectFactory factory, ZPage page)
			: base(factory, page)
		{
		}

		public override ModuleIdentifier ID => WebModuleIDs.OrgAddress;

		public override Type FilterBusinessObjectType => typeof(OrgAddressFilterBusinessObject);

		public override ColumnAndSortOrder[] GetSortInfos(FilterBusinessObject filter) => new[] { new ColumnAndSortOrder(OrgAddressSchema.OA_Address1.Name, DefaultSortOrder) };

		public override Type GridCollectionType => typeof(OrgAddressCollection);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "CommandName should not be translated")]
		protected override DataGridColumn[] GetNewGridColumnFields() => new DataGridColumn[]
		{
			new ZButtonColumn(Res.GetString("d10eb55c-c996-49e4-9fa8-6c86128408bf", "Address 1"), OrgAddressSchema.OA_Address1.Name) { CommandName = "Select" },
			new ZTextEditColumn(Res.GetString("611b3ecb-b8a1-47c8-a1e7-a864c9f8b7cc", "Address 2"), OrgAddressSchema.OA_Address2.Name),
			new ZTextEditColumn(Res.GetString("fb615094-77f1-454f-9c4b-1dd7371618c3", "City"), OrgAddressSchema.OA_City.Name),
			new ZTextEditColumn(Res.GetString("0cd67b73-170c-452e-8ac9-91595753056c", "Port"), OrgAddressSchema.OA_RL_NKRelatedPortCode.Name),
			new ZTextEditColumn(Res.GetString("a9b4a8a8-de00-4575-b2fd-5bd6fcfeb896", "Company Name"), OrgAddressSchema.OA_CompanyNameOverride.Name),
			new ZTextEditColumn(Res.GetString("8df81b04-2207-4e81-b69c-b1be0f50c97f", "State"), OrgAddressSchema.OA_State.Name),
			new ZTextEditColumn(Res.GetString("f068a446-514c-453d-a26c-cd884962ab8d", "Post Code"), OrgAddressSchema.OA_PostCode.Name),
			new ZTextEditColumn(Res.GetString("9c66e49f-166f-4223-9f4e-3fb3a285111a", "Email"), OrgAddressSchema.OA_Email.Name),
			new ZTextEditColumn(Res.GetString("296109de-7588-4c2d-ba1b-3cbe6fbcf7ea", "Phone"), OrgAddressSchema.OA_Phone.Name),
			new ZTextEditColumn(Res.GetString("996dc6a0-e858-4aec-8a37-8df364a10ac5", "Fax"), OrgAddressSchema.OA_Fax.Name),
		};

		protected override DataGridColumn[] GetDefaultGridColumnFields() => GridColumnFields.Take(4).ToArray();
	}
}
