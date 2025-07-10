using System;

namespace Enterprise.ZArchitecture.Core
{
	public interface IExtraExceptionHandler
	{
		bool HandleException(Exception exceptionToHandle);
	}
}
