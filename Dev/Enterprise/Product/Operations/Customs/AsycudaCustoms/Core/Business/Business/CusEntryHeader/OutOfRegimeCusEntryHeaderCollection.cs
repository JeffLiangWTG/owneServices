using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	[ModuleID(ModuleId.EntryHeader)]
	public class OutOfRegimeCusEntryHeaderCollection : ActiveBusinessObjectCollection<CusEntryHeader>
	{
		readonly ZString messageType;

		public OutOfRegimeCusEntryHeaderCollection(BusinessObjectFactory factory, ZString messageType) : base(factory)
		{
			this.messageType = messageType;
		}

		protected override bool AllowNew => false;

		public override void Delete(CusEntryHeader businessObject) => throw new NotSupportedException();

		protected override ZQuery CreateRelationshipFilter()
		{
			var entryHeaderFilter = new ZDBOnlyQuery(typeof(CusEntryHeader));

			var declarationFilter = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
			declarationFilter.AddToFilter(JobDeclarationSchema.JE_GC, GlbCompany.CurrentCompany.PK);

			var entryNumberFilter = new ZDBOnlySubQuery(typeof(CusEntryNumber), CusEntryNumSchema.CE_ParentID);
			entryNumberFilter.AddToFilter(CusEntryNumSchema.CE_ParentTable, CusEntryHeaderSchema.Constants.TableName);
			entryNumberFilter.AddToFilter(CusEntryNumSchema.CE_EntryNum, SQLComparisonOperator.NotEqual, string.Empty);
			entryNumberFilter.AddToFilter(CusEntryNumSchema.CE_EntryIsSystemGenerated, ZBool.True);

			if (!messageType.IsEmpty)
			{
				entryHeaderFilter.AddToFilter(CusEntryHeaderSchema.CH_MessageType, messageType);
				declarationFilter.AddToFilter(JobDeclarationSchema.JE_MessageType, messageType);
				entryNumberFilter.AddToFilter(CusEntryNumSchema.CE_EntryType, messageType);
			}

			entryHeaderFilter.AddSubQuery(declarationFilter, JoinCondition.And);
			entryHeaderFilter.AddSubQuery(entryNumberFilter, JoinCondition.And);

			return base.CreateRelationshipFilter().AddToFilter(entryHeaderFilter);
		}

		protected override bool MatchesFilterCore(CusEntryHeader element, bool fetchOnlyFromLocalCache)
		{
			return base.MatchesFilterCore(element, fetchOnlyFromLocalCache) && element.IsOutOfRegime;
		}
	}
}
