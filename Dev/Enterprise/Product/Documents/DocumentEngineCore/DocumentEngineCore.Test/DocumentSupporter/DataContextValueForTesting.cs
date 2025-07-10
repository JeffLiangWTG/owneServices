using System;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngineCore.DocumentSupport.Testing
{
	[Serializable]
	public sealed class DataContextValueForTesting : DataContextValue
	{
		public DataContextValueForTesting(DataContext dataContext) : base(dataContext) { }
	}
}
