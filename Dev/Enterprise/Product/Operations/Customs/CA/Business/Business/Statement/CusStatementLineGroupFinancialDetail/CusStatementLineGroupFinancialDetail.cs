using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CusStatementLineGroupFinancialDetail : AutoCusStatementLineGroupFinancialDetail
	{
		public CusStatementLineGroupFinancialDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject(nameof(LineGroup))]
		public override ZGuid B11_B10
		{
			get => base.B11_B10;
			set => base.B11_B10 = value;
		}

		public CusStatementLineGroup LineGroup => Factory.Load<CusStatementLineGroup>(B11_B10);

		public ZString ChargeTypeDescription
		{
			get
			{
				ZString result = B11_Type.IsEmpty
					? string.Empty
					: Factory.GetCachedValue<PostingJournalTypeList>().GetDescriptionFromCode(B11_Type);

				return result.IsEmpty ? B11_Type : result;
			}
		}

		public ZPropertyInfo ChargeTypeDescriptionInfo => GetZPropertyInfo(nameof(ChargeTypeDescription));

		#endregion
	}
}
