using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class SimplifiedLVSConsolidationOptionsWrapper : IConsolidationOptionsWrapper
	{
		public SimplifiedLVSConsolidationOptionsWrapper(SimplifiedLVS simplifiedLVS)
		{
			SimplifiedLVS = simplifiedLVS;
		}

		protected readonly SimplifiedLVS SimplifiedLVS;

		BusinessObjectFactory Factory
		{
			get
			{
				return SimplifiedLVS.Factory;
			}
		}

		#region IConsolidationStrategyBO Members

		public OrgHeader Importer
		{
			get { return SimplifiedLVS.Importer; }
		}

		public ZGuid ImporterPK
		{
			get { return SimplifiedLVS.JE_OH_Importer; }
		}

		public ZString LVSType
		{
			get { return ZString.Empty; }
		}

		public ZGuid BranchPK
		{
			get { return SimplifiedLVS.JE_GB; }
		}

		public ZGuid CompanyPK
		{
			get
			{
				var branch = Factory.Load<GlbBranch>(BranchPK);
				var company = branch != null ? branch.Company : Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
				return company == null ? ZGuid.Empty : company.PK;
			}
		}

		public ZString Broker
		{
			get { return SimplifiedLVS.JE_GS_NKCusAgent; }
		}

		public ZString PortOfClearanceCode
		{
			get { return SimplifiedLVS.CA_PortOfClearance; }
		}

		public ZZRefCusCodeListCombined PortOfClearance
		{
			get { return SimplifiedLVS.PortOfClearance; }
		}

		public ZDateTime EntryAuthorisationDate
		{
			get { return new ZDate(SimplifiedLVS.JE_EntryAuthorisationDate.Year, SimplifiedLVS.JE_EntryAuthorisationDate.Month, 1); }
		}

		public ZBool IsAllowOIC
		{
			get { return SimplifiedLVS.CA_AllowOIC; }
		}

		public ZString Province
		{
			get { return PortOfClearance?.GetAttribute(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.Province) ?? ZString.Empty; }
		}

		#endregion
	}
}
