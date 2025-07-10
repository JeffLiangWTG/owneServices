namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.Customs.CA.Business.MessageManagers;
	using Enterprise.ZArchitecture.Business;

	public interface IRNSPlugInSupport : IRNSRequestData
	{
		BusinessObject Master { get; }

		Logs Logs { get; }
		bool ReleaseStatusEventsSupported { get; }

		bool PlugInVisible { get; }
		event EventHandler PlugInVisibilityDataChanged;

		RNSMultiMessageManager GetRNSMultiMessageManager();
	}
}
