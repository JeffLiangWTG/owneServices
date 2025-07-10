using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocAuthorityToDealLineConditionDetails : DocBaseWrapper
	{
		DocAuthorityToDealLineConditionDetails(AuthorityToDealLineConditionDetails lineConditionDetails, BusinessObjectFactory factory)
			: base(lineConditionDetails, factory)
		{
		}

		public static DocAuthorityToDealLineConditionDetails New(AuthorityToDealLineConditionDetails authorityToDealLineConditionDetails, BusinessObjectFactory factory)
		{
			return (authorityToDealLineConditionDetails == null) ? null : new DocAuthorityToDealLineConditionDetails(authorityToDealLineConditionDetails, factory);
		}

		AuthorityToDealLineConditionDetails LineConditionDetails
		{
			get { return (AuthorityToDealLineConditionDetails)WrappedObject; }
		}

		public override string ToString()
		{
			return ID;
		}

		public ZString ID
		{
			get { return LineConditionDetails.ID; }
		}

		public ZString Locations
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (ZString location in LocationsArray)
				{
					result += location + ", ";
				}
				return result.Trim().TrimEnd(',');
			}
		}

		public ZString Description
		{
			get { return LineConditionDetails.Description; }
		}

		protected ZString[] LocationsArray
		{
			get { return LineConditionDetails.Locations; }
		}
	}
}
