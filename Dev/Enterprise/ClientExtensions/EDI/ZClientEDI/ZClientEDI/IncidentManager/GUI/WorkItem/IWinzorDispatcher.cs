using System;
using System.Threading.Tasks;

namespace Enterprise.Client.EDI.IncidentManager.GUI;

public interface IWinzorDispatcher
{
	public Task InvokeAsync(Action action);
}
