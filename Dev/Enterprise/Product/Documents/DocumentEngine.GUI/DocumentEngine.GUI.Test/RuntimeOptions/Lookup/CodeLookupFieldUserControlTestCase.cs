using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	sealed class CodeLookupFieldUserControlTestCase : TestCaseWithFactory
	{
		public void TestExpectedFilterType()
		{
			using (CodeLookupFieldUserControl userControl = new CodeLookupFieldUserControl())
			{
				AssertEquals("Expected filter type should be a CodeLookupField", typeof(CodeLookupField), userControl.ExpectedFilterType());
			}
		}

		public void TestSetFilter()
		{
			using (CodeLookupFieldUserControl userControl = new CodeLookupFieldUserControl())
			{
				CodeLookupField codeLookupField = new CodeLookupField(Factory);
				codeLookupField.DisplayName = "&ABC";
				CollectionProvider provider = CollectionAndModuleIDBuilder.GetCollectionAndModuleID(Factory, "vessel");
				codeLookupField.SetCollectionProvider(provider);
				userControl.SetFilter(codeLookupField);

				AssertEquals("Label text should now set to '&&ABC' - ampersands escaped", "&&ABC", userControl.FieldFindBox.GetExtension<ILabelCaptionRenderer>().Caption);
				AssertEquals("BindTo should be set to 'ZValue'", "ZValue", ((IDataBoundControl)userControl.FieldFindBox).DataMember);
				AssertEquals("BindToList should be set to 'BindToFindBoxList'", "BindToFindBoxList", userControl.FieldFindBox.BindToList);
				AssertEquals("ModuleID should be set to vessel", ModuleIDs.RefVessel, userControl.FieldFindBox.ModuleID);
			}
		}
	}
}
