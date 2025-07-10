using System.Collections.Generic;
using System.Diagnostics;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	public static class RowFactoryTestUtils
	{
		public static Dictionary<long, StackTrace> GetActiveRowFactoryStacktraces()
		{
			Dictionary<long, StackTrace> result = new Dictionary<long, StackTrace>();
			RowFactory[] factories = RowFactory.GetActiveRowFactories();
			foreach (RowFactory factory in factories)
			{
				result.Add(factory._Instance, factory.ConstructionStackTrace);
			}

			return result;
		}

		public static bool CreateStackTraceOnConstruction
		{
			get { return RowFactory.CreateStackTraceOnConstruction; }
			set { RowFactory.CreateStackTraceOnConstruction = value; }
		}
	}
}