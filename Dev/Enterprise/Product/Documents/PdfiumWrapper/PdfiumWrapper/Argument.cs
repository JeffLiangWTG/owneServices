using System;

namespace CargoWise.PdfiumWrapper
{
	// Can't reference CargoWise.Common
	class Argument
	{
		public static T NotNull<T>(T o, string name) where T : class
			=> o ?? throw new ArgumentNullException(name);
	}
}
