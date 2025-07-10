using System;
using CargoWise.EntityFramework;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[Serializable]
	class HouseCargoAvailableEventRaiser : LogSubscriber
	{
		public override string[] EventTypes
		{
			get { return new[] { Events.CargoReceivedAtDepotCode, Events.CustomsEntryStatusCode }; }
		}

		public override string FriendlyName
		{
			get { return "CVD Event Raiser"; }
		}

		public override bool IsRequired
		{
			get
			{
				if (isRequired == null)
				{
					var companyLoader = new GlbCompany.Loader(new BusinessObjectFactory());
					var auCompanies = companyLoader.LoadCompanies(Core.Constants.CountryCodes.Australia);
					isRequired = auCompanies.Length > 0;
				}
				return isRequired.Value;
			}
		}
		bool? isRequired;

		public override bool HasDynamicProperties => true;

		public override string Name
		{
			get { return "CVDEventRaiser"; }
		}

		public override string[] TableNames
		{
			get { return new[] { CusHAWBSchema.Constants.TableName, CusSCAPivotSchema.Constants.TableName }; }
		}

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			foreach (var queuedLog in queuedLogs)
			{
				var parent = queuedLog.Factory.Load(queuedLog.SJ_ParentTableCode, queuedLog.SJ_ParentID) as ICargoDepotEventParent;
				if (parent != null)
				{
					var isCargoReceivedAtDepot =
						queuedLog.SJ_SE_NKEvent == Events.CargoReceivedAtDepotCode ||
						parent.CargoReceivedAtDepotLogs.Count > 0;
					if (isCargoReceivedAtDepot && parent.IsCargoStatusClear)
					{
						if (parent.CargoAvailableAtDepotLogs.Count == 0)
						{
							parent.CargoAvailableAtDepotLogs.AddNew();
						}
					}
					else
					{
						parent.CargoAvailableAtDepotLogs.CancelAll();
					}
				}
			}
		}
	}
}
