using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	sealed class LookupFieldUserControlTestCase : TestCaseWithFactory
	{
		public void TestExpectedFilterType()
		{
			using (LookupFieldUserControl userControl = new LookupFieldUserControl())
			{
				AssertEquals("Expected filter type should be a LookupField", typeof(LookupField), userControl.ExpectedFilterType());
			}
		}

		public void TestSetFilter()
		{
			using (LookupFieldUserControl userControl = new LookupFieldUserControl())
			{
				LookupField lookupField = new LookupField(Factory);
				lookupField.DisplayName = "&ABC";

				CollectionProvider provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, "organisation");
				lookupField.SetCollectionProvider(provider);
				userControl.SetFilter(lookupField);

				AssertEquals("Label text should now set to '&&ABC' - ampersands escaped", "&&ABC", userControl.FieldFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("BindTo should be set to 'ZValue'", "ZValue", ((IDataBoundControl)userControl.FieldFindBox).DataMember);
				AssertEquals("BindToList should be set to 'BindToFindBoxList'", "BindToFindBoxList", userControl.FieldFindBox.BindToList);
				AssertEquals("ModuleID should be set to Orgs", ModuleIDs.Organisation, userControl.FieldFindBox.ModuleID);
			}
		}
	}
}
