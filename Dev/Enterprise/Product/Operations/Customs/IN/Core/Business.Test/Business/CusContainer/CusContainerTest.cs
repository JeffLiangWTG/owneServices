using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IN.Business.Testing;

[TestedType(typeof(CusContainer))]
sealed class CusContainerTest : Customs.Business.Testing.BaseCusContainerTest<CusContainer, JobDeclaration>
{
	protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bo) => new BaseCusContainer.CustomLabelsProvider(((CusContainer)bo).Declaration);

	public void TestCO_SealType()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(CusContainer.CO_SealTypeInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Seal Type", resData.Caption);
			AssertEquals("MediumCaption", "Seal Type", resData.MediumCaption);
			AssertEquals("ShortCaption", "SL.Ty.", resData.ShortCaption);
		});
	}
	public void TestCO_SealDeviceID()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(CusContainer.CO_SealDeviceIDInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Seal Device ID", resData.Caption);
			AssertEquals("MediumCaption", "Device ID", resData.MediumCaption);
			AssertEquals("ShortCaption", "Device ID", resData.ShortCaption);
		});
	}

	public void TestCO_MovementDocumentType()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(CusContainer.CO_MovementDocumentTypeInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Movement Document Type", resData.Caption);
			AssertEquals("MediumCaption", "Move. Doc. Type", resData.MediumCaption);
			AssertEquals("ShortCaption", "Doc. Type", resData.ShortCaption);
		});
	}

	public void TestCO_MovementDocumentNum()
	{
		var resData = DataBoundResourceStrings.GetDataForProperty(CusContainer.CO_MovementDocumentNumInfo);
		CombineAssertions(() =>
		{
			AssertNotNull("Res string data", resData);
			AssertEquals("Caption", "Movement Document Num", resData.Caption);
			AssertEquals("MediumCaption", "Move. Doc. No.", resData.MediumCaption);
			AssertEquals("ShortCaption", "Doc. No.", resData.ShortCaption);
		});
	}

	CusContainer CusContainer => cusContainer ??= Factory.New<CusContainer>();
	CusContainer cusContainer;
}
