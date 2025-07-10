using System.Collections.Generic;

namespace Enterprise.Semaphores.Common.Testing
{
	public interface ISemaphoreTypeWithoutParameterLessContructorTestHelper
	{
		IEnumerable<ISemaphoreType> GetUniqueSemaphores();
	}
}
