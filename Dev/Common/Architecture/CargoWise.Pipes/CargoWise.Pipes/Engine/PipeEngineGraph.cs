using System;
using System.Collections.Generic;
using QuikGraph;

namespace CargoWise.Pipes
{
	[Serializable]
	class PipeEngineGraph<TPipe> : BidirectionalGraph<Guid, SEdge<Guid>>
		where TPipe : IPipe
	{
		public Dictionary<Guid, TPipe> PipesByKey { get; } = new Dictionary<Guid, TPipe>();

		public bool AddVertex(TPipe v)
		{
			var key = v.Key;
			PipesByKey[key] = v;

			return AddVertex(key);
		}
	}
}
