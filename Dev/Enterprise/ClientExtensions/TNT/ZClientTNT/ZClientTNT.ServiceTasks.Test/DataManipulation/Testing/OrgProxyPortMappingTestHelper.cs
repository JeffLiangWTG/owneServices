using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TNT.ServiceTasks.Testing
{
	public class OrgProxyPortMappingTestHelper
	{
		public OrgProxyPortMappingTestHelper(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		#region OrgProxy
		public OrgHeader OrgProxy
		{
			get
			{
				if (fOrgProxy == null)
				{
					fOrgProxy = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
				}

				return fOrgProxy;
			}
		}

		OrgHeader fOrgProxy;
		#endregion
		#region AddPortMappingToOrgProxy
		public OrgPatternMatchOverride AddPortMappingToOrgProxy(ZString foreignCode, RefUNLOCO unloco)
		{
			OrgPatternMatchOverride pattern = OrgProxy.CreatePatternMatchOverrideForTest();
			pattern.OO_Relationship = Constants.OrgPatternMatchOverrideRelationships.Port;
			pattern.OO_ForeignCode = foreignCode;
			pattern.OO_LocalCode = unloco.Code;
			pattern.OO_LocalGuid = unloco.PK;
			return pattern;
		}

		#endregion
		#region AUSYDUnLoco
		public RefUNLOCO AUSYDUnLoco
		{
			get
			{
				if (fAUSYDUnLoco == null)
				{
					fAUSYDUnLoco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");
				}

				if (fAUSYDUnLoco == null)
				{
					fAUSYDUnLoco = Factory.New<RefUNLOCO>();
					fAUSYDUnLoco.RL_Code = "AUSYD";
					fAUSYDUnLoco.RL_PortName = "SYDNEY";
					Factory.Save();
				}

				return fAUSYDUnLoco;
			}
		}

		RefUNLOCO fAUSYDUnLoco;
		#endregion
		#region SGSINUnLoco
		public RefUNLOCO SGSINUnLoco
		{
			get
			{
				if (fSGSINUnLoco == null)
				{
					fSGSINUnLoco = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "SGSIN");
				}

				if (fSGSINUnLoco == null)
				{
					fSGSINUnLoco = Factory.New<RefUNLOCO>();
					fSGSINUnLoco.RL_Code = "SGSIN";
					fSGSINUnLoco.RL_PortName = "SINGAPORE";
					Factory.Save();
				}

				return fSGSINUnLoco;
			}
		}

		RefUNLOCO fSGSINUnLoco;
		#endregion
		readonly BusinessObjectFactory Factory;
	}
}
