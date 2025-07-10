using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IT.Business.Declaration;

public class EntryAmendmentHandlerValidation : AutoEntryAmendmentHandlerValidation
{
	public EntryAmendmentHandlerValidation(AutoEntryAmendmentHandler parent) : base(parent)
	{
	}

	protected override void CheckMovementReferenceNumber()
	{
		base.CheckMovementReferenceNumber();

		var parent = Parent;
		var movementReferenceNumber = parent.MovementReferenceNumber;
		var targetInfo = parent.MovementReferenceNumberInfo;

		if (movementReferenceNumber.IsEmpty)
		{
			MandatoryValidation.CheckEntered(targetInfo);
			return;
		}

		if (movementReferenceNumber.Length != AutoEntryAmendmentHandler.Schema.MovementReferenceNumberMaxLength)
		{
			targetInfo.AddError(Res.GetString("2B02C7B9-75D8-4C56-A00F-75EC12C24B0A", "MRN length must be 18 characters"));
			return;
		}

		var factory = parent.Factory;
		var formatInvalidMsg = MRNFormatValidator.CheckMRNFormat(movementReferenceNumber, factory, ZString.Empty);
		if (!formatInvalidMsg.IsEmpty)
		{
			targetInfo.AddMessageError(formatInvalidMsg);
		}

		var zQuery = new ZQuery(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Standard.MovementReferenceNumber);
		zQuery.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Italy);
		zQuery.AddToFilter(CusEntryNumSchema.CE_EntryNum, movementReferenceNumber);
		zQuery.AddToFilter(CusEntryNumSchema.CE_ParentTable, new string[] { CusEntryHeaderSchema.Constants.TableName, CusInBondHeaderSchema.Constants.TableName });

		var cusEntryNumber = factory.LoadTop1<CusEntryNumber>(zQuery);
		if (cusEntryNumber == null)
		{
			return;
		}

		if (cusEntryNumber.CE_ParentTable == CusEntryHeaderSchema.Constants.TableName)
		{
			targetInfo.AddWarning(Res.GetString("1F5DFAFF-6D7C-4B50-8B0F-44FBE27EEAFB", "The MRN number entered is already present in an Entry of another Customs Declaration"));
		}
		else if (cusEntryNumber.CE_ParentTable == CusInBondHeaderSchema.Constants.TableName)
		{
			targetInfo.AddWarning(Res.GetString("CC7E87E4-F2E2-48EB-8F82-289A299D87EC", "The MRN number entered is already present in another NCTS Departure Declaration"));
		}
	}

	protected override void CheckTotalEntryLines()
	{
		var parent = Parent;
		if (!parent.ShowTotalEntryLines)
		{
			return;
		}

		base.CheckTotalEntryLines();

		var totalEntryLines = parent.TotalEntryLines;
		var targetInfo = parent.TotalEntryLinesInfo;

		if (totalEntryLines.IsEmpty || totalEntryLines < 1 || totalEntryLines > 99999)
		{
			targetInfo.AddError(Res.GetString("2FA573F8-2DC1-4649-A12B-E6ACE6A445AC", "Total Entry Lines: enter a numeric value between 1 and 99999"));
		}
	}
}
