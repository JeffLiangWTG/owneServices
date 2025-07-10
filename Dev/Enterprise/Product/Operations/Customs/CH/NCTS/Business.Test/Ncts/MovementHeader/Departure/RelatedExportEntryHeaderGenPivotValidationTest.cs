using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;
using static Enterprise.Customs.CH.Business.SwissCustomsConstants;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(RelatedExportEntryHeaderGenPivotValidation))]
sealed class RelatedExportEntryHeaderGenPivotValidationTest : BusinessObjectValidationTestCase
{
	public void TestValidateRow()
	{
		var messageError = $"The Entry is attached to another NCTS Departure: NCT00000001";

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_JobReference = "NCT00000001";
		RelatedExportEntryHeaderGenPivot.Relation1Object = nctsHeader.MovementHeader;

		var nctsHeader2 = Factory.New<NctsHeader>();
		nctsHeader2.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader2.BH_JobReference = "NCT00000002";

		var declaration2 = Factory.New<JobDeclaration>();
		var entryHeader2 = declaration2.CustomsEntryHeaders.AddNew();
		var relatedExportEntryHeaderGenPivot2 = Factory.New<RelatedExportEntryHeaderGenPivot>();
		relatedExportEntryHeaderGenPivot2.Relation1Object = nctsHeader2.MovementHeader;
		relatedExportEntryHeaderGenPivot2.Relation2Object = entryHeader2;

		CombineAssertions(() =>
		{
			relatedExportEntryHeaderGenPivot.Validation.ValidateAll();
			Assert("Attachements unique", !relatedExportEntryHeaderGenPivot2.RowMessageErrors.ContainsNotificationContaining(messageError));

			relatedExportEntryHeaderGenPivot2.Relation2Object = EntryHeader;
			relatedExportEntryHeaderGenPivot2.Validation.ValidateAll();
			Assert("Entry already attached to another NCTS Departure", relatedExportEntryHeaderGenPivot2.RowMessageErrors.ContainsNotificationContaining(messageError));
		});
	}

	public void TestCheckShipmentType() => CombineAssertions(() =>
	{
		var messageNoActivationType = "The Entry does not have an Activation Type.";
		var messageIncorrectStatus = "The Entry is not in the correct Status.";
		var messageNotActivated = "The Entry is not activated and released yet.";

		EntryHeader.Declaration.JE_MessageType = CHJobMessageTypeList.Codes.ExportDeclarationActivation;

		RelatedExportEntryHeaderGenPivot.Validation.ValidateShipmentType();
		AssertHasRowMessageError(AssertionMessage("Activation type is missing"), RelatedExportEntryHeaderGenPivot, messageNoActivationType);

		EntryHeader.Declaration.JE_MessageSubType = ActivationTypeList.Codes.Edec;

		foreach (var entryStatus in new[] { string.Empty, CustomsStatusCodes.ShipmentRelease, CustomsStatusCodes.CustomsDeclarationReceived })
		{
			EntryHeader.CH_EntryStatus = entryStatus;
			RelatedExportEntryHeaderGenPivot.Validation.ValidateShipmentType();
			AssertHasRowMessageError(AssertionMessage("Not activated"), RelatedExportEntryHeaderGenPivot, messageIncorrectStatus);
		}

		EntryHeader.CH_EntryStatus = CustomsStatusCodes.SubmittedToTaxud;
		RelatedExportEntryHeaderGenPivot.Validation.ValidateShipmentType();
		AssertNoRowMessageError(AssertionMessage("Activated"), RelatedExportEntryHeaderGenPivot, messageIncorrectStatus);

		EntryHeader.Declaration.JE_MessageSubType = ActivationTypeList.Codes.Passar;

		foreach (var entryStatus in new[] { string.Empty, AdditionalCHEntryStatusList.Codes.Active, AdditionalCHEntryStatusList.Codes.DecisionToControl })
		{
			EntryHeader.CH_EntryStatus = entryStatus;
			RelatedExportEntryHeaderGenPivot.Validation.ValidateShipmentType();
			AssertHasRowWarning(AssertionMessage("Not activated"), RelatedExportEntryHeaderGenPivot, messageNotActivated);
		}

		EntryHeader.CH_EntryStatus = AdditionalCHEntryStatusList.Codes.ReleasedForExport;
		RelatedExportEntryHeaderGenPivot.Validation.ValidateShipmentType();
		AssertNoRowWarningContaining(AssertionMessage("Activated"), RelatedExportEntryHeaderGenPivot, messageNotActivated);

		EntryHeader.Declaration.JE_MessageType = CHJobMessageTypeList.Codes.Export;
		EntryHeader.Declaration.JE_MessageSubType = ZString.Empty;

		foreach (var entryStatus in new[] { string.Empty, AdditionalCHEntryStatusList.Codes.Active, AdditionalCHEntryStatusList.Codes.DecisionToControl })
		{
			EntryHeader.CH_EntryStatus = entryStatus;
			RelatedExportEntryHeaderGenPivot.Validation.ValidateShipmentType();
			AssertHasRowWarning(AssertionMessage("Not activated"), RelatedExportEntryHeaderGenPivot, messageNotActivated);
		}

		EntryHeader.CH_EntryStatus = AdditionalCHEntryStatusList.Codes.ReleasedForExport;
		RelatedExportEntryHeaderGenPivot.Validation.ValidateShipmentType();
		AssertNoRowWarningContaining(AssertionMessage("Passar Export has been activated"), RelatedExportEntryHeaderGenPivot, messageNotActivated);

		EntryHeader.CH_EntryStatus = AdditionalCHEntryStatusList.Codes.CustomsAssessmentDecision;
		RelatedExportEntryHeaderGenPivot.Validation.ValidateShipmentType();
		AssertNoRowWarningContaining(AssertionMessage("Passar Export has eVV received"), RelatedExportEntryHeaderGenPivot, messageNotActivated);

		string AssertionMessage(string info) => $"MessageType={EntryHeader.Declaration.JE_MessageType} MessageSubType={EntryHeader.Declaration.JE_MessageSubType} EntryStatus={EntryHeader.CH_EntryStatus} - {info}";
	});

	RelatedExportEntryHeaderGenPivot RelatedExportEntryHeaderGenPivot => relatedExportEntryHeaderGenPivot ?? (relatedExportEntryHeaderGenPivot = Factory.New<RelatedExportEntryHeaderGenPivot>());
	RelatedExportEntryHeaderGenPivot relatedExportEntryHeaderGenPivot;

	CusEntryHeader EntryHeader => entryHeader ??= CreateEntryHeader();
	CusEntryHeader entryHeader;

	CusEntryHeader CreateEntryHeader()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		RelatedExportEntryHeaderGenPivot.Relation2Object = entryHeader;
		return entryHeader;
	}
}
