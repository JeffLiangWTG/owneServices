using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocProfitShareAgreement : DocBaseWrapper
	{
		protected DocProfitShareAgreement(OrgProfitShareDetails agreement, BusinessObjectFactory factoryToWrap)
			: base(agreement, factoryToWrap)
		{
		}

		public static DocProfitShareAgreement New(OrgProfitShareDetails agreement, BusinessObjectFactory factoryToWrap)
		{
			return agreement != null ? new DocProfitShareAgreement(agreement, factoryToWrap) : null;
		}

		OrgProfitShareDetails Agreement
		{
			get { return (OrgProfitShareDetails)WrappedObject; }
		}

		#region Agreement Type

		public ZString AgreementType
		{
			get { return Agreement.Lookups.AgreementTypes.GetDescriptionFromCode(Agreement.O4_AgreementType); }
		}

		#endregion
	}
}
