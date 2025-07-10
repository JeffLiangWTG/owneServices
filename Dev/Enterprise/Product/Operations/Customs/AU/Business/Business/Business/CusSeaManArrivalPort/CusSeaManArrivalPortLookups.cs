using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManArrivalPortLookups : Customs.Business.CusSeaManArrivalPortLookups
	{
		public CusSeaManArrivalPortLookups(Customs.Business.AutoCusSeaManArrivalPort parent)
			: base(parent)
		{
		}

		new CusSeaManArrivalPort Parent
		{
			get { return (CusSeaManArrivalPort)base.Parent; }
		}

		public SeaCTOCollection CTOAddressOrgs
		{
			get
			{
				SeaCTOCollection result = new SeaCTOCollection(Factory);
				result.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(OrgConstants.FilterControl.UNLOCOType.OrgPort, "Property", Parent.BA_RL_NKArrivalPort));
				return result;
			}
		}

		public CodeDescriptionPairList ActualArrivalStatusList
		{
			get
			{
				if (fActualArrivalStatusList == null)
				{
					fActualArrivalStatusList = GetNewActualArrivalStatusList();
				}

				return fActualArrivalStatusList;
			}
		}
		CodeDescriptionPairList fActualArrivalStatusList;

		public CodeDescriptionPairList BerthCodeList
		{
			get
			{
				ZDateTime referenceDate = Parent.BA_ArrivalPortATA.Date;
				if (referenceDate.IsEmpty)
				{
					referenceDate = Parent.BA_ArrivalPortETA;
				}
				if (referenceDate.IsEmpty)
				{
					referenceDate = ZDateTime.Today.Date;
				}
				var arrivalPort = Parent.BA_RL_NKArrivalPort;

				return Factory.GetCachedValue($"CusSeaManArrivalPortLookups|BerthCodeList|{referenceDate}|{arrivalPort}", () =>
				{
					return CMRReferenceDataHelper.SetupBerthCodeList(Factory, referenceDate, arrivalPort);
				});
			}
		}

		#region Implementation

		protected CodeDescriptionPairList GetNewActualArrivalStatusList()
		{
			return new CMRBaseStatuses();
		}

		#endregion
	}
}
