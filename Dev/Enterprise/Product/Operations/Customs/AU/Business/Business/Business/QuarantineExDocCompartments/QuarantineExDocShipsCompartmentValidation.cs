using CargoWise.EntityFramework;
//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoQuarantineExDocShipsCompartmentValidation
//
//    This class should be used for overriding validation in AutoQuarantineExDocShipsCompartmentValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineExDocShipsCompartmentValidation : AutoQuarantineExDocShipsCompartmentValidation
	{
		public QuarantineExDocShipsCompartmentValidation(AutoQuarantineExDocShipsCompartment parent)
			: base(parent)
		{
		}

		protected new QuarantineExDocShipsCompartment Parent => (QuarantineExDocShipsCompartment)base.Parent;

		protected override void CheckQC_Compartments()
		{
			base.CheckQC_Compartments();
			if (Parent.QuarantineExDocHeader.QH_ProduceType == EXDOCCommodityCodes.Codes.GrainsAndPlants)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.QC_CompartmentsInfo, "Compartments must be non blank");
			}
			else if (!Parent.QC_Compartments.IsEmpty)
			{
				Parent.QC_CompartmentsInfo.AddMessageError("May only be supplied if produce type is grains and plants.");
			}
		}

		protected override void CheckQC_RL_NKInspectionPort()
		{
			base.CheckQC_RL_NKInspectionPort();
			ListValidation.MessageErrorIfInvalidCode(Parent.QC_RL_NKInspectionPortInfo, Parent.Lookups.InspectionPorts);
			if (Parent.QuarantineExDocHeader.QH_ProduceType == EXDOCCommodityCodes.Codes.GrainsAndPlants)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.QC_RL_NKInspectionPortInfo, "Inspection port must be entered.");
			}
			else if (!Parent.QC_RL_NKInspectionPort.IsEmpty)
			{
				Parent.QC_RL_NKInspectionPortInfo.AddMessageError("May only be supplied if produce type is grains and plants.");
			}
		}

		protected override void CheckQC_InspectionDate()
		{
			base.CheckQC_InspectionDate();
			if (Parent.QuarantineExDocHeader.QH_ProduceType == EXDOCCommodityCodes.Codes.GrainsAndPlants)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.QC_InspectionDateInfo, "Inspection date must be entered.");
			}
			else if (!Parent.QC_InspectionDate.IsEmpty)
			{
				Parent.QC_InspectionDateInfo.AddMessageError("May only be supplied if produce type is grains and plants.");
			}
		}
	}
}
