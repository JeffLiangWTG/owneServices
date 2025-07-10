using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.FR.Business.Declaration
{
	[ModuleID(ModuleId.EntryHeader)]
	public class CusEntryHeaderCollectionForCreditingCOD : ActiveBusinessObjectCollection<CusEntryHeader>
	{
		public CusEntryHeaderCollectionForCreditingCOD(BusinessObjectFactory factory) : base(factory, new ZQuery())
		{
		}

		public CusEntryHeaderCollectionForCreditingCOD(BusinessObjectFactory factory, ZBool isReleasing) : base(factory, GetFilter(isReleasing))
		{
		}

		static ZQuery GetFilter(ZBool isReleasing)
		{
			var entryHeaderQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));
			var additionalSql = FormattableString.Invariant($@"{CusEntryHeaderSchema.Constants.PK} IN (
									SELECT {CusEntryHeaderSchema.Constants.PK} FROM {CusEntryHeaderSchema.Constants.SqlSchemaName}.{CusEntryHeaderSchema.Constants.TableName}
									INNER JOIN {CusEntryNumSchema.Constants.SqlSchemaName}.{CusEntryNumSchema.Constants.TableName} ON {CusEntryNumSchema.Constants.CE_ParentID} = {CusEntryHeaderSchema.Constants.PK}
									INNER JOIN {JobDeclarationSchema.Constants.SqlSchemaName}.{JobDeclarationSchema.Constants.TableName} ON {CusEntryHeaderSchema.Constants.CH_ClusterKey} = {JobDeclarationSchema.Constants.JE_ClusterKey} AND {CusEntryNumSchema.Constants.CE_EntryType} = {JobDeclarationSchema.Constants.JE_MessageType}
									 ");

			additionalSql += isReleasing ? FormattableString.Invariant($@"AND (({JobDeclarationSchema.Constants.JE_MessageType} = 'IMP' AND {CusEntryHeaderSchema.Constants.CH_EntryStatus} >= '100')
											OR ({JobDeclarationSchema.Constants.JE_MessageType} = 'EXP' AND {CusEntryHeaderSchema.Constants.CH_EntryStatus} >= '100' AND ({CusEntryHeaderSchema.Constants.CH_ExitedStatus} = 'ESO' OR {CusEntryHeaderSchema.Constants.CH_ExitedStatus} = 'SOR'))))")
								: FormattableString.Invariant($@"AND {JobDeclarationSchema.Constants.JE_MessageType} = 'IMP' AND {CusEntryHeaderSchema.Constants.CH_EntryStatus} >= '100')");

			entryHeaderQuery.AddFilterAndZSQLParameterCollection(additionalSql, null);

			return entryHeaderQuery;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();

			var entryHeaderQuery = new ZDBOnlyQuery(typeof(CusEntryHeader));

			var declarationQuery = new ZDBOnlySubQuery(typeof(JobDeclaration), CusEntryHeaderSchema.CH_JE);
			var companyQuery = new ZDBOnlySubQuery(typeof(GlbCompany), JobDeclarationSchema.JE_GC);
			companyQuery.AddToFilter(GlbCompanySchema.PK, Environment.Env.CurrentCompany.PK);
			declarationQuery.AddSubQuery(companyQuery, JoinCondition.And);
			entryHeaderQuery.AddSubQuery(declarationQuery, JoinCondition.And);

			result.AddToFilter(entryHeaderQuery);

			return result;
		}

		protected override bool AllowNew
		{
			get { return false; }
		}

		public override void Delete(CusEntryHeader businessObject)
		{
			throw new NotSupportedException();
		}
	}
}
