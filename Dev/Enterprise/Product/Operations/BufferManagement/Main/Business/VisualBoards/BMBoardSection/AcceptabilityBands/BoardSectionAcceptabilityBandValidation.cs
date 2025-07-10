using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.BufferManagement.Business
{
	public class BoardSectionAcceptabilityBandValidation : ZValidation
	{
		public BoardSectionAcceptabilityBandValidation(BoardSectionAcceptabilityBand parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly BoardSectionAcceptabilityBand parent;

		BoardSectionAcceptabilityBandCollection AcceptabilityBandCollection
		{
			get { return parent.BoardSection.SectionConfiguration.AcceptabilityBands; }
		}

		public void ValidateAcceptabilityBandPK()
		{
			ValidateCalculatedProperty(parent.AcceptabilityBandPKInfo);
		}

		protected void CheckAcceptabilityBandPK()
		{
			MandatoryValidation.CheckEntered(parent.AcceptabilityBandPKInfo);
			ListValidation.ErrorIfInvalidPK(parent.AcceptabilityBandPKInfo);

			if (AcceptabilityBandCollection.Cast<BoardSectionAcceptabilityBand>().Any(child => child.PK != parent.PK &&
				!child.IsDeleted &&
				child.AcceptabilityBandPK.Equals(parent.AcceptabilityBandPK) &&
				child.FiltersByReleaseGroupOverride.Equals(parent.FiltersByReleaseGroupOverride) &&
				child.FiltersBySectionOverride.Equals(parent.FiltersBySectionOverride)))
			{
				parent.AcceptabilityBandPKInfo.AddError(Res.GetString("EB612BA1-D15F-477B-AA5A-224A84A654B8",
					"The acceptability band has been duplicated. Same acceptability bands can be put on the same board if they have different values for the Filter By Release Group or Filter By Board Section fields only."));
			}
		}

		public void ValidateDisplaySequence()
		{
			ValidateCalculatedProperty(parent.DisplaySequenceInfo);
		}

		protected void CheckDisplaySequence()
		{
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(parent.DisplaySequenceInfo, AcceptabilityBandCollection);
		}

		public void ValidateAllBoundaryValues()
		{
			ValidateCautionMinOverride();
			ValidateGoodMinOverride();
			ValidateExcellentMinOverride();
			ValidateExcellentMaxOverride();
			ValidateGoodMaxOverride();
			ValidateCautionMaxOverride();
		}

		public void ValidateCautionMinOverride()
		{
			ValidateCalculatedProperty(parent.CautionMinOverrideInfo);
		}

		protected void CheckCautionMinOverride()
		{
			CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(parent.CautionMinOverrideInfo, parent.GoodMinOverrideInfo);
		}

		public void ValidateGoodMinOverride()
		{
			ValidateCalculatedProperty(parent.GoodMinOverrideInfo);
		}

		protected void CheckGoodMinOverride()
		{
			CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(parent.GoodMinOverrideInfo, parent.ExcellentMinOverrideInfo);
			CompareValidation.CheckNumberGreaterThanOrEqualToOtherNumber(parent.GoodMinOverrideInfo, parent.CautionMinOverrideInfo);
		}

		public void ValidateExcellentMinOverride()
		{
			ValidateCalculatedProperty(parent.ExcellentMinOverrideInfo);
		}

		protected void CheckExcellentMinOverride()
		{
			CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(parent.ExcellentMinOverrideInfo, parent.ExcellentMaxOverrideInfo);
			CompareValidation.CheckNumberGreaterThanOrEqualToOtherNumber(parent.ExcellentMinOverrideInfo, parent.GoodMinOverrideInfo);
		}

		public void ValidateExcellentMaxOverride()
		{
			ValidateCalculatedProperty(parent.ExcellentMaxOverrideInfo);
		}

		protected void CheckExcellentMaxOverride()
		{
			CompareValidation.CheckNumberGreaterThanOrEqualToOtherNumber(parent.ExcellentMaxOverrideInfo, parent.ExcellentMinOverrideInfo);
			CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(parent.ExcellentMaxOverrideInfo, parent.GoodMaxOverrideInfo);
		}

		public void ValidateGoodMaxOverride()
		{
			ValidateCalculatedProperty(parent.GoodMaxOverrideInfo);
		}

		protected void CheckGoodMaxOverride()
		{
			CompareValidation.CheckNumberGreaterThanOrEqualToOtherNumber(parent.GoodMaxOverrideInfo, parent.ExcellentMaxOverrideInfo);
			CompareValidation.CheckNumberLessThanOrEqualToOtherNumber(parent.GoodMaxOverrideInfo, parent.CautionMaxOverrideInfo);
		}

		public void ValidateCautionMaxOverride()
		{
			ValidateCalculatedProperty(parent.CautionMaxOverrideInfo);
		}

		protected void CheckCautionMaxOverride()
		{
			CompareValidation.CheckNumberGreaterThanOrEqualToOtherNumber(parent.CautionMaxOverrideInfo, parent.GoodMaxOverrideInfo);
		}

		public void ValidateShowOn()
		{
			ValidateCalculatedProperty(parent.ShowOnInfo);
		}

		protected void CheckShowOn()
		{
			MandatoryValidation.CheckEntered(parent.ShowOnInfo, Res.GetString("25fd4628-4454-4b05-b51b-d94c47f7f238", "option for how the acceptability band should be displayed"));
			ListValidation.ErrorIfInvalidCode(ResString.GetMultilingualString("4e2e7dda-0b4a-4cf1-b35c-3f49e14c21e0", "Please select a valid option for how the acceptability band should be displayed."), parent.ShowOnInfo);
		}

		public void ValidateFiltersBySectionOverride()
		{
			ValidateCalculatedProperty(parent.FiltersBySectionOverrideInfo);
		}

		protected void CheckFiltersBySectionOverride()
		{
			if (parent.IsFilteringBySection && parent.BoardSection.SectionConfiguration.CellsPerSubsection == 0)
			{
				parent.FiltersBySectionOverrideInfo.AddError(Res.GetString("f3d3657c-0b40-4f33-a9b4-136ab67173de", "Acceptability bands cannot filter by board section when the section has 0 cells per subsection."));
			}
		}

		public void ValidateMaximumItems()
		{
			ValidateCalculatedProperty(parent.MaximumItemsInfo);
		}

		protected void CheckMaximumItems()
		{
			CompareValidation.CheckNumberGreaterThanZero(parent.MaximumItemsInfo);
			CompareValidation.CheckLessThanOrEqualTo(parent.MaximumItemsInfo, 999);
		}

		public override Type AutoValidationType
		{
			get { return typeof(BoardSectionAcceptabilityBand); }
		}

		public override void ValidateAll()
		{
			ValidateAcceptabilityBandPK();
			ValidateDisplaySequence();
			ValidateAllBoundaryValues();
			ValidateShowOn();
			ValidateFiltersBySectionOverride();
			ValidateMaximumItems();
		}
	}
}
