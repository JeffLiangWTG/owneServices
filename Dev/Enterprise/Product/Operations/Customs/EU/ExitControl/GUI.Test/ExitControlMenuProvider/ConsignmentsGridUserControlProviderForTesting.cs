using System;
using Enterprise.Customs.EU.ExitControl.Business;

namespace Enterprise.Customs.EU.ExitControl.GUI.Testing;

public class ConsignmentsGridUserControlProviderForTesting : IConsignmentsGridUserControlProvider, IDisposable
{
	public ConsignmentsGridUserControlProviderForTesting(CusExitHeader exitHeader)
	{
		this.exitHeader = exitHeader;
	}
	readonly CusExitHeader exitHeader;

	public ConsignmentsGridUserControl UserControl => userControl ?? (userControl = new ConsignmentsGridUserControl());
	ConsignmentsGridUserControl userControl;

	void IDisposable.Dispose()
	{
		userControl?.Dispose();
	}

	IConsignmentsGridUserControl IConsignmentsGridUserControlProvider.UserControl => UserControl;
	CusExitHeader IConsignmentsGridUserControlProvider.ExitHeader => exitHeader;
}
