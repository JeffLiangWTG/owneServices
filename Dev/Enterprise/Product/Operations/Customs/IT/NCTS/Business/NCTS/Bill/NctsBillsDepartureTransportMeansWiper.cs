using System;
using CargoWise.Common;

namespace Enterprise.Customs.IT.NCTS.Business;

sealed class NctsBillsDepartureTransportMeansWiper : IDisposable
{
	public NctsBillsDepartureTransportMeansWiper(NctsDepartureMovementHeader movementHeader)
	{
		this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
	}

	internal void Initialize() => HookValueChangedEvents();

	void HookValueChangedEvents()
	{
		foreach (var propertyInfo in movementHeader.GetDepartureTransportMeansProperties())
		{
			propertyInfo.ValueChanged += OnValueChanged;
		}
	}

	void UnHookValueChangedEvents()
	{
		foreach (var propertyInfo in movementHeader.GetDepartureTransportMeansProperties())
		{
			propertyInfo.ValueChanged -= OnValueChanged;
		}
	}

	void OnValueChanged(object sender, EventArgs e)
	{
		foreach (var nctsBill in movementHeader.Header?.Bills)
		{
			nctsBill.WipeDepartureTransportMeansIfNeeded();
			nctsBill.RefreshDepartureTransportMeansBindings();
		}
	}

	void IDisposable.Dispose()
	{
		if (!isDisposed)
		{
			UnHookValueChangedEvents();
			isDisposed = true;
		}
	}

	readonly NctsDepartureMovementHeader movementHeader;
	bool isDisposed;
}
