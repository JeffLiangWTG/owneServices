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

	public class RNSPlugInSupportLoadListWrapper : IRNSPlugInSupport
	{
		public RNSPlugInSupportLoadListWrapper(CFSLoadListConsol consol)
		{
			Argument.NotNull(consol, "consol");
			this.loadListConsol = consol;
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
				var number = loadListConsol.Numbers.GetFirstReferenceNumberByType(CanadaAdditionalReferenceNumberTypes.Codes.CCN);
				return number != null ? number.CE_EntryNum : ZString.Empty;
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
			get { return loadListConsol; }
		}

		Logs IRNSPlugInSupport.Logs
		{
			get { return loadListConsol.Logs; }
		}

		bool IRNSPlugInSupport.ReleaseStatusEventsSupported
		{
			get { return true; }
		}

		bool IRNSPlugInSupport.PlugInVisible
		{
			get { return loadListConsol.IsImport(); }
		}

		event EventHandler IRNSPlugInSupport.PlugInVisibilityDataChanged
		{
			add
			{
				loadListConsol.JK_RL_NKLoadPortInfo.ValueChanged += value;
				loadListConsol.JK_RL_NKDischargePortInfo.ValueChanged += value;
			}
			remove
			{
				loadListConsol.JK_RL_NKLoadPortInfo.ValueChanged -= value;
				loadListConsol.JK_RL_NKDischargePortInfo.ValueChanged -= value;
			}
		}

		RNSMultiMessageManager IRNSPlugInSupport.GetRNSMultiMessageManager()
		{
			return new RNSMultiMessageManager(RNSParentLoadListWrapper.Load(this.loadListConsol));
		}

		#endregion

		readonly CFSLoadListConsol loadListConsol;
	}
}
