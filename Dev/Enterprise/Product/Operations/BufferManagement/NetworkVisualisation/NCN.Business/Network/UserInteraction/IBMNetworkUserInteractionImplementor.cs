using System;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business
{
	public interface IBMNetworkUserInteractionImplementor : INetworkUserInteractionImplementor
	{
		IProgressReporterProvider ProgressReporterProvider { get; }
		bool HasUserConfirmed(string message, string caption, params ConfirmationNotification[] notifications);
		bool HasUserAnsweredYes(string message, string caption);
		void ShowMessage(string message);
		T ShowMultiOptionDialog<T>(string message, string caption, T defaultValue, ButtonStripAction<T>[] buttonStrips) where T : struct, IConvertible;
	}
}
