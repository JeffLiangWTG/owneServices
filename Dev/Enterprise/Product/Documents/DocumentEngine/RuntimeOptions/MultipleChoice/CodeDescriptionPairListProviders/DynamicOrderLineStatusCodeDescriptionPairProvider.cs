using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public class DynamicOrderLineStatusCodeDescriptionPairProvider : DynamicKeyedCodePairListProvider<ZGuid>
	{
		public DynamicOrderLineStatusCodeDescriptionPairProvider(BusinessObjectFactory factory)
			: base(factory)
		{ }

		protected override ReadOnlyCodeDescriptionPairList GetCommonBaseListCore()
		{
			return ObjectFactory.Get<Enterprise.Freight.Integration.Forwarding.IOrderStatusListProvider>().GetOrderLineStatusList();
		}

		protected override CodeDescriptionPairList GetDynamicCodeDescriptionPairListCore(ZGuid key, BusinessObjectFactory factory)
		{
			CodeDescriptionPairList result = new CodeDescriptionPairList(OLookUpEditType.CustomType);
			result.AddRange(ObjectFactory.Get<Enterprise.Freight.Integration.Forwarding.IOrderStatusListProvider>().GetOrderLineStatusList());

			ZDBOnlyQuery sTMQuery = new ZDBOnlyQuery(typeof(StmNote));
			sTMQuery.AddToFilter(StmNoteSchema.ST_Description, "OrderLineStatusTypes");
			ZDBOnlySubQuery miscServSubQuery = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.PK);
			miscServSubQuery.AddToFilter(OrgMiscServSchema.OM_OH, new ZGuid(key));
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
