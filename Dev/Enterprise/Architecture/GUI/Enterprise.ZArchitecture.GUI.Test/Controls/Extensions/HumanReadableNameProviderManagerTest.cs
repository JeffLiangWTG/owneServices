using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class HumanReadableNameProviderManagerTest : TestCaseWithDummy
	{
		public void TestRegisterAndUnregisterCaptionProvider()
		{
			HumanReadableNameProviderManager.CountOfRegisteredProviders = 0;
			HumanReadableNameProviderManager.CountOfUnRegisteredProviders = 0;
			var dummy = Factory.New<DummyBusinessObject>();
			using (var form = new ZForm(dummy))
			using (var grid = new ZGrid())
			{
				grid.ColumnStyles.Add(
					new ZTextBoxColumnStyleInfo
					{
						ColumnName = AutoDummyBizo.Schema.Z0_VarCharMax,
						Caption = "New VarCharMax"
					}
					);
				grid.ColumnStyles.Add
					(
					new ZTextBoxColumnStyleInfo
					{
						ColumnName = AutoDummyBizo.Schema.Z0_Description,
						Caption = "New Description"
					}
					);
				form.Controls.Add(grid);
				grid.CopyCaptionsToPropertyHumanReadableNameForTest = true;
				grid.SetDataBinding(dummy, "");
				AssertEquals(1, (HumanReadableNameProviderManager.CountOfRegisteredProviders));
				AssertEquals(0, (HumanReadableNameProviderManager.CountOfUnRegisteredProviders));
				HumanReadableNameProviderManager.CountOfRegisteredProviders = 0;
				HumanReadableNameProviderManager.CountOfUnRegisteredProviders = 0;
				grid.SetDataBinding(null, "");
			}
			AssertEquals(0, (HumanReadableNameProviderManager.CountOfRegisteredProviders));
			AssertEquals(1, (HumanReadableNameProviderManager.CountOfUnRegisteredProviders));
		}

		public void TestGetHumanReadableNameForHumanReadableNameProviderManager()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var child1 = dummy.Collection.AddNew();
			var child2 = dummy.Collection.AddNew();
			var child3 = dummy.Collection.AddNew();

			var humanReadableNameProviderManager = HumanReadableNameProviderManager.HumanReadableNameProviderManagerForTest();
			humanReadableNameProviderManager.ProvidersExposedForTests.Add(new DummyCaptionProvider("Z0_VarCharMax", "New Caption"));
			humanReadableNameProviderManager.ProvidersExposedForTests.Add(new DummyCaptionProvider("Z0_Description", "New Description"));

			AssertEquals("New Caption", humanReadableNameProviderManager.GetHumanReadableName(child1.Z0_VarCharMaxInfo));
			AssertEquals("New Caption", humanReadableNameProviderManager.GetHumanReadableName(child2.Z0_VarCharMaxInfo));
			AssertEquals("New Caption", humanReadableNameProviderManager.GetHumanReadableName(child3.Z0_VarCharMaxInfo));

			AssertEquals("New Description", humanReadableNameProviderManager.GetHumanReadableName(child1.Z0_DescriptionInfo));
			AssertEquals("New Description", humanReadableNameProviderManager.GetHumanReadableName(child2.Z0_DescriptionInfo));
			AssertEquals("New Description", humanReadableNameProviderManager.GetHumanReadableName(child3.Z0_DescriptionInfo));
		}

		class DummyCaptionProvider : IHumanReadableNameProvider
		{
			public DummyCaptionProvider(string name, ZString caption)
			{
				this.name = name;
				this.caption = caption;
			}

			readonly string name;
			readonly ZString caption;

			public ZString GetHumanReadableName(ZPropertyInfo propertyInfo)
			{
				if (propertyInfo.Name == name)
				{
					return caption;
				}
				return ZString.Empty;
			}
		}
	}
}
