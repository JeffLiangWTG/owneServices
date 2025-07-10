using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

[TestedType(typeof(DocumentAvailability))]
sealed class DocumentAvailabilityTest : CusSupportingInfoTest<DocumentAvailability>
{
	public void TestDocumentType()
	{
		var info = DataBoundResourceStrings.GetDataForProperty(DocumentAvailability.CSI_CodeInfo);
		AssertEquals("Document Type", info.Caption);
	}

	public void TestAvailabilityStatus()
	{
		var info = DataBoundResourceStrings.GetDataForProperty(DocumentAvailability.CSI_StatusInfo);
		AssertEquals("Availability Status", info.Caption);
	}

	public void TestReasonCode()
	{
		var info = DataBoundResourceStrings.GetDataForProperty(DocumentAvailability.CSI_SubTypeInfo);
		AssertEquals("Reason Code", info.Caption);
	}

	public void TestLookups() => AssertType<DocumentAvailabilityLookups>(DocumentAvailability.Lookups);

	public void TestValidation() => AssertType<DocumentAvailabilityValidation>(DocumentAvailability.Validation);

	#region Implementation
	protected override IEnumerable<DocumentAvailability> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		yield return instruction.DocumentAvailability.AddNew();
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		return entryInstruction.DocumentAvailability.AddNew();
	}

	protected override BusinessObject GetBusinessObjectForFetchForLoad()
	{
		return GetNewBusinessObject();
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		var instruction = declaration.CustomsEntryInstructions.AddNew();
		return instruction.DocumentAvailability.AddNew();
	}

	DocumentAvailability DocumentAvailability => documentAvailability ??= (DocumentAvailability)GetNewBusinessObject();
	DocumentAvailability documentAvailability;
	#endregion
}
