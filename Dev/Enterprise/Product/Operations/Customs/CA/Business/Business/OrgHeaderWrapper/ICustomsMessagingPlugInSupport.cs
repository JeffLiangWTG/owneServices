namespace Enterprise.Customs.CA.Business
{
	using System;
	using CargoWise.EntityFramework;

	public interface ICustomsMessagingPlugInSupport
	{
		BusinessObject Master { get; }
		bool PlugInVisible { get; }
		event EventHandler PlugInVisibilityDataChanged;
	}
}
