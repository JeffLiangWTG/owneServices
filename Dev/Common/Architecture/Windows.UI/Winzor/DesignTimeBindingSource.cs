using System;

namespace CargoWise.Windows.UI
{
	internal class DesignTimeBindingSource : FixedDotNetBindingSource
	{
		public static Type GetTypeToUseAsDataSource(Type type) => type;
	}
}
