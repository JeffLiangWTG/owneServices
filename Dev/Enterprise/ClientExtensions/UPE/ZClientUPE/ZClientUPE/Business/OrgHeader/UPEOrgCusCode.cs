using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPEOrgCusCode : OrgCusCode
	{
		public new class CodeTypes : OrgCusCode.CodeTypes
		{
			public const string UPSCustomerAccountNumber = "UAN";
			public const string UPSCustomerAccountNumberDescription = "Unique Account Number";
		}

		public UPEOrgCusCode(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override OrgCusCodeLookups GetNewLookups()
		{
			return new UPEOrgCusCodeLookups(this);
		}

		protected override OrgCusCodeValidation GetNewValidation()
		{
			return new UPEOrgCusCodeValidation(this);
		}
	}
}
