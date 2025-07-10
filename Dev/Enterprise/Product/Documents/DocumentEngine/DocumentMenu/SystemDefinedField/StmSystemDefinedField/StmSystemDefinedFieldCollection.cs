using System.Globalization;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.SDF
{
	public class StmSystemDefinedFieldCollection : BusinessObjectCollection<StmSystemDefinedField>
	{
		public StmSystemDefinedFieldCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public void Load(BusinessContext businessContext, RefCountry country)
		{
			Load(businessContext, country.RN_Code);
		}

		public void Load(BusinessContext businessContext, ZString countryCode)
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(StmSystemDefinedField));

			string filter = string.Format(
				CultureInfo.InvariantCulture,
				@"
					{0} = @S1_BusinessContext
					AND {1} = 0
					AND {2} = ''
					AND
					(
						{3} IN
						(
							SELECT {3}
							FROM {4}
							WHERE {0} = @S1_BusinessContext
							GROUP BY {3}
							HAVING COUNT(*) = 1
						)
						OR {3} IN
						(
							SELECT {3}
							FROM {4}
							WHERE {0} = @S1_BusinessContext
							AND {2} = @S1_RN_NKCntrySpecific
							AND {5} = 0
						)
						OR
						(
							{3} NOT IN
							(
								SELECT {3}
								FROM {4}
								WHERE {0} = @S1_BusinessContext
								AND {2} = @S1_RN_NKCntrySpecific
								AND {5} = 1
							)
							AND {3} NOT IN
							(
								SELECT {3}
								FROM {4}
								WHERE {0} = @S1_BusinessContext
								AND {2} != @S1_RN_NKCntrySpecific and {2} != ''
								AND {5} = 0
							)
						)
					)",
					StmSystemDefinedFieldSchema.Constants.S1_BusinessContext,    // 0
					StmSystemDefinedFieldSchema.Constants.S1_OrderColumn,        // 1
					StmSystemDefinedFieldSchema.Constants.S1_RN_NKCntrySpecific, // 2
					StmSystemDefinedFieldSchema.Constants.S1_Name,               // 3
					StmSystemDefinedFieldSchema.Constants.TableName,             // 4
					StmSystemDefinedFieldSchema.Constants.S1_IsSuppressed);      // 5

			ZSqlParameterCollection parameters = new ZSqlParameterCollection();
			parameters.Add("@S1_BusinessContext", businessContext.ToString(), StmSystemDefinedFieldSchema.S1_BusinessContext);
			parameters.Add("@S1_RN_NKCntrySpecific", countryCode, StmSystemDefinedFieldSchema.S1_RN_NKCntrySpecific);

			query.AddFilterAndZSQLParameterCollection(filter, parameters);

			Load(query);
		}
	}
}
