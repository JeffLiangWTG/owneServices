using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.Testing.Business.Reflection
{
	internal interface CustomReadonlyCollection : IReadOnlyCollection<ReadonlyBO>
	{
		public ZString ZStringPropertyInCustomReadonlyCollection1 { get; set; }
		public ZString ZStringPropertyInCustomReadonlyCollection2 { get; set; }
	}
}
