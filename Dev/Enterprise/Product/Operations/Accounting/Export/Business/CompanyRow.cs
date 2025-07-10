using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Accounting.Export.Business
{
	public class CompanyRow : IGlbCompany
	{
		public ZGuid PK { get; set; }
		public ZString GC_Code { get; set; }
		public ZString GC_Name { get; set; }
		public ZGuid GC_OH_OrgProxy { get; set; }
		public ZString GC_RN_NKCountryCode { get; set; }
		public ZBool GC_IsActive { get; set; }

		public ZString LicenceKeyIdentifier { get; set; }

		public ZString LicenceEnterpriseCode { get; }
		public ZString LicenceServerID { get; }

		public IRefCountry Country { get; set; }
		public IRefCurrency Currency { get; set; }

#if DEBUG
		void IGlbCompany.SetCountry(string countryCode)
		{
			throw new NotImplementedException("This SetCountry method is not implemented on CompanyRow class");
		}

		IDisposable IGlbCompany.TemporarilySetCountry(string countryCode)
		{
			throw new NotImplementedException("This TemporarilySetCountry method is not implemented on CompanyRow class");
		}
#endif
		public string FirstActiveBranchCode
		{
			get
			{
				return string.Empty;
			}
		}

		public IEnumerable<IGlbBranch> GetActiveBranches()
		{
			throw new NotImplementedException();
		}

		public bool HasOnlyOneActiveBranch()
		{
			throw new NotImplementedException();
		}

		public bool IsBranchActive(string branchCode)
		{
			throw new NotImplementedException();
		}
	}

	public class CountryRow : IRefCountry
	{
		public ZGuid PK { get; set; }
		public ZString RN_Code { get; set; }
		public ZString RN_Desc { get; set; }
	}

	public class CurrencyRow : IRefCurrency
	{
		public int Decimals { get; set; }
		public ZGuid PK { get; set; }
		public ZString RX_Code { get; set; }
		public ZString RX_Desc { get; set; }
	}
}
