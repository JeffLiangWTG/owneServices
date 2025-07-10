using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.CH;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(InAndOutwardProcessing))]
sealed class InAndOutwardProcessingTest : SingleCusSupportingInfoTest<InAndOutwardProcessing>
{
	public override void TestIsSavedByFactory() => CombineAssertions(() =>
	{
		base.TestIsSavedByFactory();
		InAndOutwardProcessing.Repair = true;
		Assert("Repair is not empty.", InAndOutwardProcessing.IsSavedByFactory);
	});

	public void TestSetDefaultValues() => CombineAssertions(() =>
	{
		AssertEquals(Common.CH.CusSupportingInfoTypeList.Codes.InAndOutwardProcessing, InAndOutwardProcessing.CSI_Type);
		AssertEquals(JobComInvoiceLineSchema.Constants.Prefix, InAndOutwardProcessing.CSI_ParentTableCode);
		AssertEquals(false, InAndOutwardProcessing.Repair);
	});

	public void TestCSI_SubTypeMaxLength() => AssertEquals(1, InAndOutwardProcessing.CSI_SubTypeInfo.MaxLength);

	public void TestCSI_CodeMaxLength() => AssertEquals(1, InAndOutwardProcessing.CSI_CodeInfo.MaxLength);

	public void TestCSI_ProcedureMaxLength() => AssertEquals(1, InAndOutwardProcessing.CSI_ProcedureInfo.MaxLength);

	public void TestCSI_IssuerTypeMaxLength() => AssertEquals(1, InAndOutwardProcessing.CSI_IssuerTypeInfo.MaxLength);

	public void TestCSI_StatusMaxLength() => AssertEquals(1, InAndOutwardProcessing.CSI_StatusInfo.MaxLength);

	public void TestCaptions() => CombineAssertions(() =>
	{
		CaptionTestHelper.AssertCaptions(InAndOutwardProcessing.CSI_SubTypeInfo, caption: "Direction");
		CaptionTestHelper.AssertCaptions(InAndOutwardProcessing.CSI_CodeInfo, caption: "Refinement Type");
		CaptionTestHelper.AssertCaptions(InAndOutwardProcessing.CSI_ProcedureInfo, caption: "Process Type");
		CaptionTestHelper.AssertCaptions(InAndOutwardProcessing.CSI_IssuerTypeInfo, caption: "Billing Type");
		CaptionTestHelper.AssertCaptions(InAndOutwardProcessing.RepairInfo, caption: "Repair");

		InAndOutwardProcessing.Parent.Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		CaptionTestHelper.AssertCaptions(InAndOutwardProcessing.CSI_DescriptionInfo, caption: "Repair Reason");

		InAndOutwardProcessing.Parent.Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		CaptionTestHelper.AssertCaptions(InAndOutwardProcessing.CSI_DescriptionInfo, caption: "Reason");

		InAndOutwardProcessing.Parent.Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		CaptionTestHelper.AssertCaptions(InAndOutwardProcessing.CSI_DescriptionInfo, caption: "Reason");
	});

	public void TestCSI_DescriptionMaxLength() => AssertEquals(280, InAndOutwardProcessing.CSI_DescriptionInfo.MaxLength);

	public void TestRepairCSI_StatusMapping() => CombineAssertions(() =>
	{
		AssertEquals($"Repair = {InAndOutwardProcessing.Repair}", InAndOutwardProcessingStatusCodes.RepairFalse, InAndOutwardProcessing.CSI_Status);
		InAndOutwardProcessing.Repair = true;
		AssertEquals($"Repair = {InAndOutwardProcessing.Repair}", InAndOutwardProcessingStatusCodes.RepairTrue, InAndOutwardProcessing.CSI_Status);
		InAndOutwardProcessing.CSI_Status = InAndOutwardProcessingStatusCodes.RepairFalse;
		AssertEquals($"CSI_Status = {InAndOutwardProcessing.CSI_Status}", false, InAndOutwardProcessing.Repair);
		InAndOutwardProcessing.CSI_Status = InAndOutwardProcessingStatusCodes.RepairTrue;
		AssertEquals($"CSI_Status = {InAndOutwardProcessing.CSI_Status}", true, InAndOutwardProcessing.Repair);
		InAndOutwardProcessing.CSI_Status = InAndOutwardProcessingStatusCodes.RepairFalse;
		AssertEquals($"CSI_Status = {InAndOutwardProcessing.CSI_Status}", false, InAndOutwardProcessing.Repair);
	});

	public void TestOnFactorySaving() => CombineAssertions(() =>
	{
		InAndOutwardProcessing.CSI_SubType = "X";
		InAndOutwardProcessing.CSI_CustomsOffice = "X";
		InAndOutwardProcessing.Parent.Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		Factory.Save();
		AssertEquals($"EXP: CSI_SubType removed", ZString.Empty, InAndOutwardProcessing.CSI_SubType);
		AssertEquals($"EXP: CSI_CustomsOffice not removed", "X", InAndOutwardProcessing.CSI_CustomsOffice);

		InAndOutwardProcessing.CSI_SubType = "X";
		InAndOutwardProcessing.CSI_CustomsOffice = "X";
		InAndOutwardProcessing.Parent.Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		Factory.Save();
		AssertEquals($"EDA: CSI_SubType removed", ZString.Empty, InAndOutwardProcessing.CSI_SubType);
		AssertEquals($"EDA: CSI_CustomsOffice not removed", "X", InAndOutwardProcessing.CSI_CustomsOffice);

		InAndOutwardProcessing.CSI_SubType = "X";
		InAndOutwardProcessing.CSI_CustomsOffice = "X";
		InAndOutwardProcessing.Parent.Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		Factory.Save();
		AssertEquals($"IMP: CSI_SubType not removed", "X", InAndOutwardProcessing.CSI_SubType);
		AssertEquals($"IMP: CSI_CustomsOffice removed", ZString.Empty, InAndOutwardProcessing.CSI_CustomsOffice);
	});

	public override void TestOnSaving()
	{
		MessageType = CHJobMessageTypeList.Codes.Import;
		base.TestOnSaving();
		MessageType = CHJobMessageTypeList.Codes.Export;
		base.TestOnSaving();
		MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		base.TestOnSaving();
	}

	public void TestIsExportOrExportDeclarationActivation() => CombineAssertions(() =>
	{
		InAndOutwardProcessing.Parent.Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		AssertEquals(InAndOutwardProcessing.Parent.Declaration.JE_MessageType, true, InAndOutwardProcessing.IsExportOrExportDeclarationActivation);
		InAndOutwardProcessing.Parent.Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;
		AssertEquals(InAndOutwardProcessing.Parent.Declaration.JE_MessageType, true, InAndOutwardProcessing.IsExportOrExportDeclarationActivation);
		InAndOutwardProcessing.Parent.Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		AssertEquals(InAndOutwardProcessing.Parent.Declaration.JE_MessageType, false, InAndOutwardProcessing.IsExportOrExportDeclarationActivation);
	});

	protected override IEnumerable<string> GetUsedFieldsNames()
	{
		if (MessageType != CHJobMessageTypeList.Codes.Export && MessageType != CHJobMessageTypeList.Codes.ExportDeclarationActivation)
		{
			yield return nameof(InAndOutwardProcessing.CSI_SubType);
		}
		yield return nameof(InAndOutwardProcessing.CSI_Code);
		yield return nameof(InAndOutwardProcessing.CSI_Procedure);
		yield return nameof(InAndOutwardProcessing.CSI_IssuerType);
		yield return nameof(InAndOutwardProcessing.CSI_Description);
		if (MessageType == CHJobMessageTypeList.Codes.Export || MessageType == CHJobMessageTypeList.Codes.ExportDeclarationActivation)
		{
			yield return nameof(InAndOutwardProcessing.CSI_CustomsOffice);
		}
	}

	protected override BusinessObject GetNewBusinessObject() => GetNewCusSupportingInfo(Factory);

	protected override IEnumerable<InAndOutwardProcessing> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
	{
		var inAndOutwardProcessing = GetNewCusSupportingInfo(factory);
		inAndOutwardProcessing.CSI_Description = "X";
		yield return inAndOutwardProcessing;
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetBizObjsForCorrectlyTypeDecideTest(factory).FirstOrDefault();

	protected override InAndOutwardProcessing GetNewCusSupportingInfo(BusinessObjectFactory factory)
	{
		var declaration = factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageType;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		return invoiceLine.InAndOutwardProcessings.AddNew();
	}

	InAndOutwardProcessing InAndOutwardProcessing => fInAndOutwardProcessing ?? (fInAndOutwardProcessing = GetNewCusSupportingInfo(Factory));
	InAndOutwardProcessing fInAndOutwardProcessing;

	ZString MessageType { get; set; }
}
