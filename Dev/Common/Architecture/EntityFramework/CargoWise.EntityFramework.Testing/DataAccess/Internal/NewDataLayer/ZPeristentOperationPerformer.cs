using System.Data;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	class DummyPersistentOperationPerformer : ZPersistentOperationPerformer
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public DummyPersistentOperationPerformer(DataSet data) : base(data)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public new DataSet Data
		{
			get
			{
				return base.Data;
			}
		}
	}

	sealed class ZPersistentOperationPerformerTest : TestCase
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestConstructor()
		{
			DataSet data = new DataSet();
			DummyPersistentOperationPerformer dummy = new DummyPersistentOperationPerformer(data);
			AssertEquals("Set correctly", data, dummy.Data);
		}
	}
}
