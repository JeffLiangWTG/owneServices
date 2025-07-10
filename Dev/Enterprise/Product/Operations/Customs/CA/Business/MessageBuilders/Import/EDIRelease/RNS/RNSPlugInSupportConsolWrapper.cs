namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	using System;
	using CargoWise.Common;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Customs.CA.Business.MessageManagers;
	using Enterprise.Customs.Common;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Business;

	public class RNSPlugInSupportConsolWrapper : IRNSPlugInSupport
	{
		public RNSPlugInSupportConsolWrapper(ForwardingConsol consol)
		{
			Argument.NotNull(consol, "consol");
			this.consol = consol;
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
				var number = consol.Numbers.GetFirstReferenceNumberByType(CanadaAdditionalReferenceNumberTypes.Codes.CCN);
				return number != null ? number.CE_EntryNum : ZString.Empty;
			}
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

		ZString IRNSRequestData.HouseBillNumber
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region Implementation of IRNSPlugInSupport

		BusinessObject IRNSPlugInSupport.Master
		{
			get { return consol; }
		}

		Logs IRNSPlugInSupport.Logs
		{
			get { return consol.Logs; }
		}

		bool IRNSPlugInSupport.ReleaseStatusEventsSupported
		{
			get { return true; }
		}

		bool IRNSPlugInSupport.PlugInVisible
		{
			get { return consol.IsImport(); }
		}

		event EventHandler IRNSPlugInSupport.PlugInVisibilityDataChanged
		{
			add
			{
				consol.JK_RL_NKLoadPortInfo.ValueChanged += value;
				consol.JK_RL_NKDischargePortInfo.ValueChanged += value;
			}
			remove
			{
				consol.JK_RL_NKLoadPortInfo.ValueChanged -= value;
				consol.JK_RL_NKDischargePortInfo.ValueChanged -= value;
			}
		}

		RNSMultiMessageManager IRNSPlugInSupport.GetRNSMultiMessageManager()
		{
			return new RNSMultiMessageManager(RNSParentConsolWrapper.Load(this.consol));
		}

		#endregion

		readonly ForwardingConsol consol;
	}
}
