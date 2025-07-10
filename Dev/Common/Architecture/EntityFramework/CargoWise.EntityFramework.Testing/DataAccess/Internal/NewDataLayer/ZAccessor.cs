using System.Collections.Generic;
using System.Data;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZAccessorTest : TestCaseWithDummy
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestConstructor()
		{
			DataSet data = new DataSet();
			DummyAccessor dummy = new DummyAccessor(data);
			AssertEquals("Set dataset", data, dummy.Data);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		public void TestLoadBlobFields()
		{
			ZDataRowDictionary dictionary = new ZDataRowDictionary();
			DummyAccessor accessor = new DummyAccessor(new DataSet());
			AssertEquals("Precondition - Accessor.DatabaseLoadCount should be 0.", 0, accessor.DatabaseLoadCount);

			HashSet<SchemaColumn> blobsToLoad = new HashSet<SchemaColumn>();
			blobsToLoad.Add(DummyBizoSchema.Z0_VarCharMax);

			accessor.LoadBlobFieldsForTable("tablename", dictionary, blobsToLoad);
			accessor.MockLoader.Verify(x => x.LoadBlobFieldsForTable("tablename", dictionary, blobsToLoad), Times.Never());
			AssertEquals("Accessor.DatabaseLoadCount should still be 0.", 0, accessor.DatabaseLoadCount);

			accessor.MockLoader.Reset();

			dictionary.Add("some key", new DataTable().NewRow());

			accessor.LoadBlobFieldsForTable("tablename", dictionary, blobsToLoad);
			accessor.MockLoader.Verify(m => m.LoadBlobFieldsForTable("tablename", dictionary, blobsToLoad), Times.Once());
			AssertEquals("Accessor.DatabaseLoadCount should now be 1.", 1, accessor.DatabaseLoadCount);
		}

		#region Implementation

		class DummyAccessor : ZAccessor
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
			public DummyAccessor(DataSet data)
				: base(data)
			{
			}

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
			public new DataSet Data
			{
				get { return base.Data; }
			}

			protected override ZLoader Loader
			{
				get { return MockLoader.Object; }
			}

			public Mock<ZLoader> MockLoader
			{
				get
				{
					if (fMockLoader == null)
					{
						fMockLoader = new Mock<ZLoader>(MockBehavior.Loose, new object[] { Data });
					}

					return fMockLoader;
				}
			}

			Mock<ZLoader> fMockLoader;

			protected override ZSaver Saver
			{
				get { return null; }
			}
		}

		#endregion
	}
}
