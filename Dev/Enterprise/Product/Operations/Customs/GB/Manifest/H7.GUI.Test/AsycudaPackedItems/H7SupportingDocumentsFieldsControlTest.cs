using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.GUI.PlugIn.Testing;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.H7.GUI.Testing
{
	public class H7SupportingDocumentsFieldsControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceType()
		{
			SupportingDocumentsControlTestHelper.AssertBindingSource(SupportingDocumentsFieldsControlType, typeof(AsycudaPackedItem));
		}

		public void TestGroupBox()
		{
			SupportingDocumentsControlTestHelper.AssertGroupBox(SupportingDocumentsFieldsControlType, testForCommonCaptionAsResourceString: true, "Supporting Documents");
		}

		public void TestFields()
		{
			SupportingDocumentsControlTestHelper.AssertFields(SupportingDocumentsFieldsControlType, FieldsDetails);
		}

		protected IEnumerable<(string ControlName, int TabIndex, Type ControlType)> FieldsDetails => new (string, int, Type)[]
		{
			("CSI_CodeCodeFindBox", 0, typeof(ZCodeFindBox)),
			("CSI_ReferenceNumberTextBox", 1, typeof(ZTextBox)),
			("CSI_ActionsDropEdit", 2, typeof(ZDropEdit)),
			("CSI_AvailabilityDropEdit", 3, typeof(ZDropEdit)),
			("CSI_SubTypeTextBox", 4, typeof(ZTextBox)),
			("CSI_DateOfIssueDateEdit", 5, typeof(ZDateEdit)),
			("CSI_DateOfExpiryDateEdit", 6, typeof(ZDateEdit)),
			("CSI_DescriptionTextBox", 7, typeof(ZTextBox)),
			("CSI_ReferenceNumber2TextBox", 8, typeof(ZTextBox))
		};

		protected Type SupportingDocumentsFieldsControlType => typeof(H7SupportingDocumentsFieldsControl);
	}
}
