using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class SalesRelationsWrapper : GenericWrapper
	{
		public SalesRelationsWrapper(OrgOpportunity opportunity, BusinessObjectFactory factoryToWrap)
			: base(opportunity, factoryToWrap)
		{
			quotations = new RatingWrapperCollection(opportunity, Factory);
			oneOffQuotes = new FreightWrapperCollection(opportunity, Factory);
		}

		#region Sales Relations Properties
		public RatingWrapperCollection Quotations
		{
			get
			{
				return quotations;
			}
		}
		readonly RatingWrapperCollection quotations;

		public FreightWrapperCollection OneOffQuotes
		{
			get
			{
				return oneOffQuotes;
			}
		}
		readonly FreightWrapperCollection oneOffQuotes;

		#endregion
	}
}
