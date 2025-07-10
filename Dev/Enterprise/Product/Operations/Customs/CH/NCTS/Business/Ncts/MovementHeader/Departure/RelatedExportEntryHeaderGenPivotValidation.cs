using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.NCTS.Business;

public class RelatedExportEntryHeaderGenPivotValidation : GenPivotValidation
{
	public RelatedExportEntryHeaderGenPivotValidation(RelatedExportEntryHeaderGenPivot parent) : base(parent)
	{
	}

	new RelatedExportEntryHeaderGenPivot Parent => (RelatedExportEntryHeaderGenPivot)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateRow();
		ValidateShipmentType();
	}

	void ValidateRow()
	{
		var query = new ZQuery(GenPivotSchema.XX_Relation2ID, Parent.XX_Relation2ID);
		query.AddToFilter(GenPivotSchema.XX_Relation1ID, SQLComparisonOperator.NotEqual, Parent.XX_Relation1ID);
		var existingRelation = Parent.Factory.LoadTop1<GenPivot>(query);

		if (existingRelation != null)
		{
			var jobReference = existingRelation.Relation1Object.Cast<NctsDepartureMovementHeader>().First().Header.BH_JobReference;
			var messageError = Res.GetString("F8F9FEDE-7FAB-421D-BF22-8592A337C0B7", "The Entry is attached to another NCTS Departure: {0}", jobReference);
			Parent.AddRowMessageError(messageError);
		}
	}

	public void ValidateShipmentType()
	{
		ValidateCalculatedProperty(Parent.ShipmentTypeInfo);
	}

	protected void CheckShipmentType()
	{
		var noActivationTypeMessage = Res.GetString("98FBDE3B-6C16-4200-A006-3E56BCF01993", "The Entry does not have an Activation Type.");
		var incorrectStatusMessage = Res.GetString("86AD6466-05AC-4744-874A-C07BDF2AE3FD", "The Entry is not in the correct Status.");
		var notActivatedMessage = Res.GetString("4349B339-0AFC-4509-BE4F-82F7628B1DB5", "The Entry is not activated and released yet.");

		Parent.RemoveRowMessageError(noActivationTypeMessage);
		Parent.RemoveRowMessageError(incorrectStatusMessage);
		Parent.RemoveRowWarning(notActivatedMessage);

		var relatedExport = Parent;

		switch (relatedExport.ShipmentType)
		{
			case CHJobMessageTypeList.Codes.Export:
				var entryStatus = relatedExport.EntryHeader.CH_EntryStatus;
				if (entryStatus != AdditionalCHEntryStatusList.Codes.ReleasedForExport && entryStatus != AdditionalCHEntryStatusList.Codes.CustomsAssessmentDecision)
				{
					Parent.AddRowWarning(notActivatedMessage);
				}
				break;
			case CHJobMessageTypeList.Codes.ExportDeclarationActivation:
				var entryHeader = relatedExport.EntryHeader;
				switch (entryHeader.Declaration.JE_MessageSubType)
				{
					case "":
						Parent.AddRowMessageError(noActivationTypeMessage);
						break;
					case ActivationTypeList.Codes.Edec:
						if (entryHeader.CH_EntryStatus != SwissCustomsConstants.CustomsStatusCodes.SubmittedToTaxud)
						{
							Parent.AddRowMessageError(incorrectStatusMessage);
						}
						break;
					case ActivationTypeList.Codes.Passar:
						if (entryHeader.CH_EntryStatus != AdditionalCHEntryStatusList.Codes.ReleasedForExport)
						{
							Parent.AddRowWarning(notActivatedMessage);
						}
						break;
				}
				break;
		}
	}
}
