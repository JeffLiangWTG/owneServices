using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.SDF
{
	public class StmSystemDefinedFieldCountryCollection : BusinessObjectCollection<StmSystemDefinedFieldCountry>
	{
		public StmSystemDefinedFieldCountryCollection(StmSystemDefinedField parentField, BusinessObjectFactory factory) : base(factory)
		{
			this.ParentField = parentField;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			StmSystemDefinedFieldCountry fieldCountry = (StmSystemDefinedFieldCountry)child;
			fieldCountry.S1_Name = ParentField.S1_Name;
			fieldCountry.S1_BusinessContext = ParentField.S1_BusinessContext;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "no need to be translated")]
		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery result = base.CreateRelationshipFilter();
			result.AddToFilter(StmSystemDefinedFieldSchema.S1_Name, ParentField.S1_Name);
			result.AddToFilter(StmSystemDefinedFieldSchema.S1_Name, SQLComparisonOperator.NotEqual, "");
			result.AddToFilter(StmSystemDefinedFieldSchema.S1_BusinessContext, ParentField.S1_BusinessContext);
			result.AddToFilter(StmSystemDefinedFieldSchema.S1_BusinessContext, SQLComparisonOperator.NotEqual, "");
			result.AddToFilter(StmSystemDefinedFieldSchema.PK, SQLComparisonOperator.NotEqual, ParentField.PK);
			return result;
		}

		readonly StmSystemDefinedField ParentField;
	}
}
