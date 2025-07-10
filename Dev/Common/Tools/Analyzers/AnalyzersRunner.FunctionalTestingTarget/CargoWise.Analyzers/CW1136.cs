//CW1136:Do Not Use System Runtime Serialization Formatters Binary

using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace AnalyzersRunner.FunctionalTestingTarget.CargoWise.Analyzers
{
	class CW1136
	{
		public void SomeMethod(Stream stream, Object obj)
		{
			var formatter = new BinaryFormatter();
			formatter.Serialize(stream, obj);
		}
	}
}
