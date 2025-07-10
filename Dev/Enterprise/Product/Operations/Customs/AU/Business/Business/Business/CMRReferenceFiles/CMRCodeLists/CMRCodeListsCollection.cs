
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRCodeListsCollection : BusinessObjectCollection<CMRCodeLists>
	{
		public CMRCodeListsCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CMRCodeListsCollection(BusinessObjectFactory factory, ZString codeType)
			: base(factory, GetCodeTypeFilter(codeType))
		{
			this.codeType = codeType;
			DefaultFilterValues();
		}

		protected override void AddNotificationWhenAdditionalFilterNotMet(StringCollectionX errors, BusinessObject selectedBusinessObject)
		{
			base.AddNotificationWhenAdditionalFilterNotMet(errors, selectedBusinessObject);
			errors.Add("This record is not type of " + codeType);
		}

		protected static ZQuery GetCodeTypeFilter(ZString codeType)
		{
			ZQuery result = new ZQuery();
			if (!codeType.IsEmpty)
			{
				result.AddToFilter(JoinCondition.And, CMRCodeListsSchema.CI_CodeType, SQLComparisonOperator.Equal, codeType);
			}
			return result;
		}

		protected void DefaultFilterValues()
		{
			FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Code Type", "Property", codeType));
		}

		readonly ZString codeType;
	}
}
