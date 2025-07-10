using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(CustomFieldWrapper))]
	sealed class CustomFieldWrapperTest : GenericWrapperTest
	{
		#region TestWrapperMappingsEmpty

		public override void TestWrapperMappingsEmpty()
		{
			var emptyWrapper = new CustomFieldWrapper(null, null, Factory);
			AssertEquals("emptyWrapper.Caption", "", emptyWrapper.Caption);
			AssertEquals("emptyWrapper.FieldName", "", emptyWrapper.CustomFieldName);
			AssertEquals("emptyWrapper.Value", "", emptyWrapper.Value);
		}

		#endregion

		#region TestWrapperMappingsFull

		public void TestWrapperMappingsFull()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var customLabel = orgHeader.CustomLabels.AddNew();
			customLabel.OT_FieldName = Constants.CustomLabels.Organisation.CustomAttribute1;
			customLabel.OT_Caption = "Hidden Name";
			customLabel.OT_Type = OrgConstants.CustomLabelType.Form;
			orgHeader.MiscServ.OM_CustomAttrib1 = "Custom Value";
			Factory.Save();

			var customField = new CustomLabelInfo(Constants.CustomLabels.Organisation.CustomAttribute1, OrgMiscServSchema.Constants.OM_CustomAttrib1, typeof(ZString), (NoResString)"", (NoResString)"", CustomLabelStyles.UpperCase, orgHeader, Factory);
			var wrapper = new CustomFieldWrapper(orgHeader.MiscServ, customField, Factory);
			AssertEquals("wrapper.Caption", "Hidden Name", wrapper.Caption);
			AssertEquals("wrapper.CustomFieldName", Constants.CustomLabels.Organisation.CustomAttribute1, wrapper.CustomFieldName);
			AssertEquals("wrapper.Value", "Custom Value", wrapper.Value);
		}

		#endregion

		#region Implementation

		protected override ZString ExpectedDefaultFormatting
		{
			get { return "Registry : (No Default Field Value Available on Registry)"; }
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new CustomFieldWrapper(null, null, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return @"CustomField
======================================================================
Name                                    Type
----------------------------------------------------------------------
Caption                                 MultilingualString
Value                                   String";
			}
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			var data = new TestDataSimpleEnvironment(Factory, saveFactory_doNotUseForNewTests: false);
			var helper = new WhsTestHelperFunctions(Factory);
			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			return new CustomFieldWrapper(order, (CustomLabelInfo)new WhsDocket.CustomLabelsProvider(order).GetCustomFields(data.Org1, Factory)[0], Factory);
		}

		#endregion
	}
}
