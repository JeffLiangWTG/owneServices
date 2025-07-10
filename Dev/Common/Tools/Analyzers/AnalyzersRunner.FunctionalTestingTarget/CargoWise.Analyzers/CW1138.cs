//CW1138:Do Not Use System Runtime Remoting

using System;
using System.Runtime.Remoting;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1138
	{
		public void RetrieveObject(Type type, string url)
		{
			_ = RemotingServices.Connect(type, url);
		}
	}
}
