namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	using System;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.CA.Business.MessageManagers;
	using Enterprise.Customs.Common;
	using Enterprise.Freight.CFS.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Business;

	public class RNSPlugInSupportTallyWrapper : IRNSPlugInSupport
	{
		public RNSPlugInSupportTallyWrapper(TallyContainer container)
		{
			Argument.NotNull(container, "container");
			this.container = container;
		}

		#region Implementation of IRNSRequest

		ZDateTime IRNSRequestData.DateOfArrival
		{
			get { return ZDateTime.Now; }
		}

		ZString IRNSRequestData.CargoControlNumber
		{
			get
			{
				if (container.Consol != null)
				{
					var number = container.Consol.Numbers.GetFirstReferenceNumberByType(CanadaAdditionalReferenceNumberTypes.Codes.CCN);
					return number != null ? number.CE_EntryNum : ZString.Empty;
				}
				else
				{
					return ZString.Empty;
				}
			}
		}

		ZString IRNSRequestData.HouseBillNumber
		{
			get { return ZString.Empty; }
		}

		ZString IRNSRequestData.TransactionNumber
		{
			get { return ZString.Empty; }
		}

		ZString IRNSRequestData.OfficeCode
		{
			get { return ZString.Empty; }
		}

		ZString IRNSRequestData.SubLocationCode
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region Implementation of IRNSPlugInSupport

		BusinessObject IRNSPlugInSupport.Master
		{
			get { return container; }
		}

		Logs IRNSPlugInSupport.Logs
		{
			get { return container.Logs; }
		}

		bool IRNSPlugInSupport.ReleaseStatusEventsSupported
		{
			get { return true; }
		}

		bool IRNSPlugInSupport.PlugInVisible
		{
			get { return container.IsImport(); }
		}

		event EventHandler IRNSPlugInSupport.PlugInVisibilityDataChanged
		{
			add
			{
				if (container.Consol != null)
				{
					container.Consol.JK_RL_NKLoadPortInfo.ValueChanged += value;
					container.Consol.JK_RL_NKDischargePortInfo.ValueChanged += value;
				}
			}
			remove
			{
				if (container.Consol != null)
				{
					container.Consol.JK_RL_NKLoadPortInfo.ValueChanged -= value;
					container.Consol.JK_RL_NKDischargePortInfo.ValueChanged -= value;
				}
			}
		}

		RNSMultiMessageManager IRNSPlugInSupport.GetRNSMultiMessageManager()
		{
			return new RNSMultiMessageManager(RNSParentTallyWrapper.Load(this.container));
		}

		#endregion

		readonly TallyContainer container;
	}
}
