using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Business.Organisation
{
	public class NonPersistentOrgHeaderChecker : AutoNonPersistentOrgHeaderChecker
	{
		public NonPersistentOrgHeaderChecker(BusinessObjectFactory factory, OrgHeader orgHeader)
			: base(factory)
		{
			this.orgHeader = Argument.NotNull(orgHeader, nameof(orgHeader));
			vatOrgCusCode = orgHeader.CustomsCodes.GetOrgCusCodeObjectMatching(OrgCusCode.CodeTypes.VATCode);
			eoriOrgCusCode = orgHeader.CustomsCodes.GetOrgCusCodeObjectMatching(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);

			VerifyVAT = !VAT.IsEmpty;
			VerifyEORI = !EORI.IsEmpty;
		}

		public override ZString VAT => vatOrgCusCode != null ? vatOrgCusCode.OK_CustomsRegNo : ZString.Empty;

		public override ZString EORI => eoriOrgCusCode != null ? orgHeader.GetEuIdentificationNumber() : ZString.Empty;

		public OrgHeader OrgHeader => orgHeader;

		public OrgCusCode VatOrgCusCode => vatOrgCusCode;

		public OrgCusCode EoriOrgCusCode => eoriOrgCusCode;

		public OrgCusCode[] CodesForQuery
		{
			get
			{
				var results = new List<OrgCusCode>();
				if (VerifyVAT && VatOrgCusCode != null)
				{
					results.Add(VatOrgCusCode);
				}
				if (VerifyEORI && EoriOrgCusCode != null)
				{
					results.Add(EoriOrgCusCode);
				}
				return results.ToArray();
			}
		}

		public ZBool IsValid => CodesForQuery.Length > 0;

		readonly OrgHeader orgHeader;
		readonly OrgCusCode vatOrgCusCode;
		readonly OrgCusCode eoriOrgCusCode;
	}
}
