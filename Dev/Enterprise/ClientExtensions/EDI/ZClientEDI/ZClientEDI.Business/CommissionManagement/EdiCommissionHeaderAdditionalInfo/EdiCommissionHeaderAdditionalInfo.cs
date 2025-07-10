using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.CommissionManagement.Business;

namespace Enterprise.Client.EDI.CommissionManagement.Business
{
	public class EdiCommissionHeaderAdditionalInfo : AutoEdiCommissionHeaderAdditionalInfo
	{
		public EdiCommissionHeaderAdditionalInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region ECH_CH0

		[RelatedBusinessObject("CommissionHeader")]
		public override ZGuid ECH_CH0
		{
			get { return base.ECH_CH0; }
			set { base.ECH_CH0 = value; }
		}

		public AccCommissionHeader CommissionHeader
		{
			get { return Factory.Load<AccCommissionHeader>(ECH_CH0); }
		}

		#endregion

		#region ECH_LC

		public ClientCompany ClientCompany
		{
			get { return Factory.Load<ClientCompany>(ECH_LCC); }
		}

		#endregion

		#region ECH_LD

		public LicenceDatabase LicenceDatabase
		{
			get { return Factory.Load<LicenceDatabase>(ECH_LD); }
		}

		#endregion

		#region LicenceCode

		[ResourceStringData("EdiCommissionHeaderAdditionalInfo|EnterpriseDatabaseClientCompanyString", Caption = "Enterprise / Database / Company")]
		public ZString EnterpriseDatabaseClientCompanyString
		{
			get
			{
				var result = new ZStringBuilder();

				if (LicenceDatabase != null)
				{
					result.Append(LicenceDatabase.EnterpriseCode + "-" + LicenceDatabase.LD_ServerCode);
				}

				if (ClientCompany != null)
				{
					result.Append((!result.IsEmpty ? "-" : "") + ClientCompany.LCC_Code);
					if (!string.IsNullOrEmpty(ClientCompany.LCC_RN_NKCountryCode))
					{
						result.Append(" (" + ClientCompany.LCC_RN_NKCountryCode + ")");
					}
				}

				return result.ToString();
			}
		}

		#endregion
	}
}

