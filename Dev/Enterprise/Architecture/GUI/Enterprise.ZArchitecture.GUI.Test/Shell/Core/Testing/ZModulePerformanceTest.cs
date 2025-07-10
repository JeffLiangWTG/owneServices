using System.Collections;
using System.Text;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	public abstract class ZModulePerformanceTest : TransactionedTestCase
	{
		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestModulePerformance()
		{
			var iD = GetModuleID();
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(iD))
			{
				try
				{
					if (module == null)
					{
						Fail("Unable to get module for ID : " + iD.ToString());
					}
					this.module = module;
					PrepareModuleForPerformanceTest(module);
					var embeddedControl = module.EmbeddedControl;
					using (var form = new ZForm())
					{
						form.Controls.Add(embeddedControl);
						form.Show();
						form.Height = 700;
						form.Width = 1366;
						this.module.Grid.ResetColumns();
						((IFilterGridModuleInternalsForTesting)module).PerformSearch();
						Application.DoEvents();

						Mark("After PerformSearch", ((IFilterGridModuleInternalsForTesting)module).GridCollection.Factory.DatabaseLoadCount, MaximumDBHitsForPerformSearch);
						if (this.module.Grid.VisibleRowCount < 2)
						{
							Fail("At least two rows must be visible on the grid");
						}
						var mimumObjects = this.module.Grid.VisibleRowCount + 1;
						if (this.module.GridCollection.Count < mimumObjects)
						{
							Fail("Insufficient business objects were returned to perform this test - you need at least " + mimumObjects.ToString() + " objects returned in the collection.");
						}
						ShowResults();
					}
				}
				catch (ModuleGuiNotSupportedException)
				{
					// Acceptable failure
				}
			}
		}

		protected IFilterGridModuleInternalsForTesting module;

		int GetDistinctTableCount(ZGrid grid)
		{
			var hashTable = new Hashtable();
			foreach (var column in grid.Columns)
			{
				var propertyName = column.ColumnStyle.MappingName;
				var ptyNameParts = propertyName.Split('_');

				if (ptyNameParts.Length >= 2 && ptyNameParts[0].Length <= 3 && ptyNameParts[1].Length <= 3)
				{
					hashTable[propertyName.Substring(3, 2)] = true;
				}
			}
			return hashTable.Count;
		}

		protected abstract void PrepareModuleForPerformanceTest(ZFilterGridModule module);
		protected virtual int MaximumDBHitsForPerformSearch
		{
			get
			{
				var distinctTables = GetDistinctTableCount(module.Grid);
				var hitsForGridPlusCount = 2;
				return distinctTables + hitsForGridPlusCount;
			}
		}

		//
		//			#region Setup + "After CreateFullyPopulatedObject"
		//			BusinessObject BizO = SetupObject();
		//			// Load and JIT
		//			using (ZWinForm form = GetForm(BizO))
		//			{
		//				form.Show();
		//				Application.DoEvents();
		//				form.BusinessEntity.RunPreSaveValidation();
		//			}
		//			BizO.Factory.Save();
		//
		//			#endregion
		//
		//			BusinessObjectFactory Factory2 = new BusinessObjectFactory();
		//			HitCalculator hitCalculator = new HitCalculator(Factory2);
		//			Mark("Before top level object create", hitCalculator.DatabaseLoadCount, 0);
		//			BusinessObject BizOInSecondFactory = Factory2.Load(BizO.GetType(), BizO.PK);
		//			using (ZWinForm form = GetForm(BizOInSecondFactory))
		//			{
		//				form.Show();
		//				Application.DoEvents();
		//				Mark("After Form Show", hitCalculator.DatabaseLoadCount, MaximumDBHitsForFormOpen);
		//				form.BusinessEntity.RunPreSaveValidationFetch();
		//				Mark("After PreSaveValidationFetch", hitCalculator.DatabaseLoadCount, 0);
		//				form.BusinessEntity.RunPreSaveValidation();
		//				Mark("After PreSave Validation", hitCalculator.DatabaseLoadCount, MaximumDBHitsForPreSaveValidation);
		//			}

		protected abstract ModuleIdentifier GetModuleID();

		public ModuleIdentifier ModuleID
		{
			get { return GetModuleID(); }
		}

		void Mark(string location, int actualDBHits, int maximumAllowableDBHits)
		{
			Failure |= actualDBHits > maximumAllowableDBHits;
			stringBuilder.Append(location + " - Maximum : " + maximumAllowableDBHits.ToString() + ", Actual : " + actualDBHits.ToString() + System.Environment.NewLine);
			if (ShowDebugWindow && !NUnit.Framework.TestingState.IsRunningOnDAT)
			{
				System.Windows.Forms.MessageBox.Show(location); // debug flag to help in profiling
			}
		}

		void ShowResults()
		{
			if (Failure)
			{
				var results = stringBuilder.ToString();
				Fail(results);
			}
		}

		bool Failure;
		readonly StringBuilder stringBuilder = new StringBuilder();

		protected virtual bool ShowDebugWindow
		{
			get { return false; }
		}
	}
}
