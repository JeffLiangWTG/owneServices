using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.SWT
{
	class ReportDataProvider
	{
		public void LoadData(BusinessObjectFactory factory)
		{
			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(factory);
			collection.Load(SqlScript, GetParameters());

			for (int i = 0; i < collection.Count; i++)
			{
				DynamicBusinessObject bizO = collection[i];
				ZGuid consignorPk = (ZGuid)bizO["ConsignorPK"];
				ZGuid consigneePk = (ZGuid)bizO["ConsigneePK"];

				if (!consigneePks.Contains(consigneePk))
				{
					consigneePks.Add(consigneePk);
				}
				if (!consignorPks.Contains(consignorPk) && IsNominatedConsignor(consignorPk))
				{
					consignorPks.Add(consignorPk);
				}
			}
		}

		public IEnumerable<ZGuid> ConsigneePKs
		{
			get { return consigneePks; }
		}

		public IEnumerable<ZGuid> ConsignorPKs
		{
			get { return consignorPks; }
		}

		public bool HasData
		{
			get { return consigneePks.Count > 0; }
		}

		#region Implementation

		string SqlScript
		{
			get { return "select * from ClientOrgsForNotYetArrived(@CountryCode, @Today)"; }
		}

		ZSqlParameterCollection GetParameters()
		{
			ZSqlParameterCollection result = new ZSqlParameterCollection();
			ZSqlParameter datePar = ZSqlParameter.New("@Today", ZDateTime.Now, JobShipmentSchema.JS_E_DEP);
			result.Add(datePar);
			ZSqlParameter countryCodePar = ZSqlParameter.New("@CountryCode", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, JobConsolSchema.JK_RL_NKDischargePort);
			result.Add(countryCodePar);
			return result;
		}

		bool IsNominatedConsignor(ZGuid consignorPK)
		{
			bool result = false;
			OrgHeaderCodeListCollection consignorList = new OrgHeaderCodeListCollection(SWTDataRegistry.Instance.ConsignorsList.Value);
			foreach (OrgHeaderCodeListElement element in consignorList)
			{
				if (element.ClientGuid == consignorPK)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		readonly List<ZGuid> consigneePks = new List<ZGuid>();
		readonly List<ZGuid> consignorPks = new List<ZGuid>();

		#endregion
	}
}
