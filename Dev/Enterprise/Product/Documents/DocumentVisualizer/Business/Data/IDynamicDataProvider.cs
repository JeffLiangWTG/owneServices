using System;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Business
{
	interface IDynamicDataProvider
	{
		Try<IDynamicData> TryCreate(string dataContext, Guid pivotID, IDocDataObjectParameters parameters);
	}
}