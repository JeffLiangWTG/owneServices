using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Modules
{
	public class RefContainerFilterBusinessObject : AutoRefContainerFilterBusinessObject
	{
		public RefContainerFilterBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Default Values

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			RC_DescriptionContains = true;
		}

		#endregion

		#region Filter

		protected override ZQuery SearchPanelFilterFilter
		{
			get
			{
				SQLComparisonOperator @operator = RC_DescriptionContains ? SQLComparisonOperator.Contains : SQLComparisonOperator.StartsWith;

				ZQuery query = new ZQuery();

				query.AddToFilter(RefContainerSchema.RC_Code, @operator, RC_Description);
				query.AddToFilter(JoinCondition.Or, RefContainerSchema.RC_Description, @operator, RC_Description);
				return query;
			}
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList ShippingModes
		{
			get { return new RefContainerLookups(null).ShippingModesList; }
		}

		#endregion
	}
}
