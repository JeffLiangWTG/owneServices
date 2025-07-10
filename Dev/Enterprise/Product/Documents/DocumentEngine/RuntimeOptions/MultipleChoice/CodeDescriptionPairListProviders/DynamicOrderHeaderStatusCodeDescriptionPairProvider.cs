using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class DynamicOrderHeaderStatusCodeDescriptionPairProvider : DynamicKeyedCodePairListProvider<ZGuid>
	{
		public DynamicOrderHeaderStatusCodeDescriptionPairProvider(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override ReadOnlyCodeDescriptionPairList GetCommonBaseListCore()
		{
			return ObjectFactory.Get<Enterprise.Freight.Integration.Forwarding.IOrderStatusListProvider>().GetOrderStatusList();
		}

		protected override CodeDescriptionPairList GetDynamicCodeDescriptionPairListCore(ZGuid key, BusinessObjectFactory factory)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			ZDBOnlyQuery sTMQuery = new ZDBOnlyQuery(typeof(StmNote));
			sTMQuery.AddToFilter(StmNoteSchema.ST_Description, "OrderStatusTypes");
			//STMQuery.AddToFilter(STmNoteSchema.ST_Table, ) //what do I even put?
			ZDBOnlySubQuery miscServSubQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.PK);
			miscServSubQuery.AddToFilter(OrgMiscServSchema.OM_OH, key);
			sTMQuery.AddSubQuery(StmNoteSchema.ST_ParentID, miscServSubQuery, JoinCondition.And);

			HiddenStmNote filteredData = (HiddenStmNote)factory.LoadTop1(typeof(HiddenStmNote), sTMQuery);
			if (filteredData != null)
			{
				result.AddRangeOverwriteIfExists(new ReadOnlyCodeDescriptionPairList(filteredData.ST_NoteData));
			}
			return result;
		}
	}
}
