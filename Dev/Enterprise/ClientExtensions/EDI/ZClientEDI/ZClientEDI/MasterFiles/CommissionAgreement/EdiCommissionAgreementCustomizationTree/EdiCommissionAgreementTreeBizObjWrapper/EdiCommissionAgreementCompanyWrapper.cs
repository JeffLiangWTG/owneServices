using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiCommissionAgreementCompanyWrapper : EdiCommissionAgreementTreeBizObjWrapper
	{
		public EdiCommissionAgreementCompanyWrapper(EdiCommissionAgreementCustomization customization, ClientCompany clientCompany)
			: base(customization, null)
		{
			Argument.NotNull(clientCompany, "clientCompany");
			this.ClientCompany = clientCompany;
		}

		internal readonly ClientCompany ClientCompany;

		#region Selected

		public override ZBool Selected
		{
			get { return GetMatchingCompanyPivots().Any(); }
			set
			{
				if (value)
				{
					if (!GetMatchingCompanyPivots().Any())
					{
						customization.CompanyPivots.AddNew(ClientCompany);
					}
				}
				else
				{
					foreach (var companyPivot in GetMatchingCompanyPivots().ToArray())
					{
						companyPivot.Delete();
					}
				}
			}
		}

		public override ZBool Selected_Enabled
		{
			get { return true; }
		}

		#endregion

		#region Code

		public override ZString Code
		{
			get { return ClientCompany.LCC_Code; }
		}

		#endregion

		#region Description

		public override ZString Description
		{
			get { return ClientCompany.LCC_Name; }
		}

		#endregion

		IEnumerable<EdiCommissionAgreementCompanyPivot> GetMatchingCompanyPivots()
		{
			return customization.CompanyPivots.Where(x => x.EPY_LCC == ClientCompany.PK);
		}
	}
}
