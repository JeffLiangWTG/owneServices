using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(SWControl))]
sealed class SWControlTest : CusSupportingInfoTest<SWControl>
{
	public void TestCSI_LineNo()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(SWControl.CSI_LineNoInfo);
		AssertEquals("Caption", "Serial Number", resData.Caption);
		AssertEquals("MediumCaption", "Serial No.", resData.MediumCaption);
		AssertEquals("ShortCaption", "Sr. No.", resData.ShortCaption);
	}

	public void TestISequenceNumberLine()
	{
		CombineAssertions(() =>
		{
			var sequenceLine = (IHugeSequenceNumberLine)SWControl;
			AssertEquals("FKToHeader", SWControl.Parent.PK, sequenceLine.FKToHeader);
			AssertEquals("SequenceNumber", (ZInt)1, sequenceLine.SequenceNumber);
		});
	}

	public void TestCSI_LineNoSequence()
	{
		var instruction = Factory.New<CusEntryInstruction>();
		var control1 = instruction.SWControls.AddNew();
		var control2 = instruction.SWControls.AddNew();
		var control3 = instruction.SWControls.AddNew();

		CombineAssertions(() =>
		{
			AssertEquals("Order 1", 1, control1.CSI_LineNo);
			AssertEquals("Order 2", 2, control2.CSI_LineNo);
			AssertEquals("Order 3", 3, control3.CSI_LineNo);

			instruction.SWControls.Remove(control2);
			AssertEquals("Order 1 stay same", 1, control1.CSI_LineNo);
			AssertEquals("Order 3 change to 2", 2, control3.CSI_LineNo);

			control1.Delete();
			AssertEquals("Order 2 change to 1", 1, control3.CSI_LineNo);
		});
	}

	public void TestControlResultDescription()
	{
		RefDataSetupTestHelper.SetupControlResultCode(Factory);
		var resData = DataBoundResourceStrings.GetDataForProperty(SWControl.ControlResultDescriptionInfo);
		SWControl.CSI_ReferenceNumber2 = "IN00121";
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "Control Result Description", resData.Caption);
			AssertEquals("MediumCaption", "Result Desc.", resData.MediumCaption);
			AssertEquals("ShortCaption", "Desc.", resData.ShortCaption);
			AssertEquals("Description", "Indicates no accessory is associated with the item", SWControl.ControlResultDescription);
		});
	}

	public void TestCSI_Description()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(SWControl.CSI_DescriptionInfo);
		CombineAssertions(() =>
		{
			AssertEquals(SWControl.Schema.CSI_DescriptionMaxLength, SWControl.CSI_DescriptionInfo.MaxLength);
			AssertEquals("Caption", "Control Result Remark", resData.Caption);
			AssertEquals("MediumCaption", "Ctrl. Remark", resData.MediumCaption);
			AssertEquals("ShortCaption", "Remark", resData.ShortCaption);
		});
	}

	public void TestCSI_ReferenceNumber2()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(SWControl.CSI_ReferenceNumber2Info);
		CombineAssertions(() =>
		{
			AssertEquals(SWControl.Schema.CSI_ReferenceNumber2MaxLength, SWControl.CSI_ReferenceNumber2Info.MaxLength);
			AssertEquals("Caption", "Control Result Code", resData.Caption);
			AssertEquals("MediumCaption", "Ctrl. Result", resData.MediumCaption);
			AssertEquals("ShortCaption", "Result", resData.ShortCaption);
		});
	}

	public void TestCSI_ControlLocation()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(SWControl.CSI_ControlLocationInfo);
		CombineAssertions(() =>
		{
			AssertEquals(SWControl.Schema.CSI_ControlLocationMaxLength, SWControl.CSI_ControlLocationInfo.MaxLength);
			AssertEquals("Caption", "Control Location", resData.Caption);
			AssertEquals("MediumCaption", "Ctrl. Location", resData.MediumCaption);
			AssertEquals("ShortCaption", "Location", resData.ShortCaption);
		});
	}

	public void TestCSI_DateOfIssue()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(SWControl.CSI_DateOfIssueInfo);
		AssertEquals("Caption", "Control Start Date", resData.Caption);
		AssertEquals("MediumCaption", "Ctrl. Start Dt.", resData.MediumCaption);
		AssertEquals("ShortCaption", "Start Dt.", resData.ShortCaption);
	}

	public void TestCSI_DateOfExpiry()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(SWControl.CSI_DateOfExpiryInfo);
		AssertEquals("Caption", "Control End Date", resData.Caption);
		AssertEquals("MediumCaption", "Ctrl. End Dt.", resData.MediumCaption);
		AssertEquals("ShortCaption", "End Dt.", resData.ShortCaption);
	}

	public void TestLookups()
	{
		AssertType<SWControlLookups>(SWControl.Lookups);
	}

	public void TestValidation()
	{
		AssertType<SWControlValidation>(SWControl.Validation);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBizObj(factory);

	protected override BusinessObject GetNewBusinessObject() => GetNewBizObj(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

	protected override IEnumerable<SWControl> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var control = GetNewBizObj(factory);
		yield return control;
	}

	protected override void LoadParentIfNeeded(BusinessObjectFactory factory, SWControl bizObj)
	{
		factory.Load<CusEntryInstruction>(bizObj.CSI_ParentID);
	}

	SWControl GetNewBizObj(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		return instruction.SWControls.AddNew();
	}

	SWControl SWControl => fSWControl ??= GetNewBizObj(Factory);
	SWControl fSWControl;
}
