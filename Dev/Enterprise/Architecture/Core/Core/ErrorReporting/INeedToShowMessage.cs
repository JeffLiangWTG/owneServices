using System;

namespace Enterprise.ZArchitecture.Core
{
	public interface INeedToShowMessage
	{
		IDisposable SuppressNewFormInTransactionWarning();
		bool CanFormBeCreatedDuringDbTransaction(Type type);
	}
}
