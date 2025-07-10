using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class JobComInvoiceLineValidation : Customs.Business.BaseJobComInvoiceLineValidation
	{
		public JobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new JobComInvoiceLine Parent
		{
			get { return (JobComInvoiceLine)base.Parent; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidVehicleVIN();
		}

		public void ValidVehicleVIN()
		{
			ValidateCalculatedProperty(Parent.VehicleVINInfo);
		}

		protected void CheckVehicleVIN()
		{
			var vehicle = Parent.FirstVehicle;
			if (vehicle != null)
			{
				vehicle.Validation.ValidateCVH_VehicleIdentificationNumber();
				Parent.VehicleVINInfo.AddAllNotificationsFrom(vehicle.CVH_VehicleIdentificationNumberInfo);
				Parent.MarkAsNeedingValidation();
			}
		}

		protected override void CheckJI_PreviousEntryNumber()
		{
			if (Parent.JI_PreviousEntryNumber.IsEmpty
				&& (Parent.Declaration?.IsBondedWarehouseAutomationOn ?? false)
				&& Parent.HasOutOfWarehouseProcedure)
			{
				Parent.JI_PreviousEntryNumberInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Parent.JI_PreviousEntryNumberInfo.HumanReadableName));
			}
		}

		protected override void CheckJI_PreviousEntryLineNumber()
		{
			var number = Parent.JI_PreviousEntryLineNumber;
			if (number <= ZShort.Zero
				&& (Parent.Declaration?.IsBondedWarehouseAutomationOn ?? false)
				&& Parent.HasOutOfWarehouseProcedure)
			{
				var info = Parent.JI_PreviousEntryLineNumberInfo;
				if (number < ZShort.Zero)
				{
					info.AddMessageError(MandatoryValidation.ValueCannotBeNegativeMessage(info.HumanReadableName));
				}
				else
				{
					info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(info.HumanReadableName));
				}
			}
		}

		protected override void CheckJI_PartNo()
		{
			base.CheckJI_PartNo();
			if (Parent.Part == null && (Parent.Declaration?.IsBondedWarehouseAutomationOn ?? false))
			{
				Parent.JI_PartNoInfo.AddMessageError(Res.GetString("5D8046F3-D909-42BB-B452-B190435DCBB1", "Please enter a valid Product."));
			}
		}

		protected override void CheckJI_BondedWhsQuantity()
		{
			base.CheckJI_BondedWhsQuantity();
			if (Parent.JI_BondedWhsQuantity.IsEmpty && (Parent.Declaration?.IsBondedWarehouseAutomationOn ?? false))
			{
				Parent.JI_BondedWhsQuantityInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Parent.JI_BondedWhsQuantityInfo.HumanReadableName));
			}
		}

		protected override void CheckJI_BondedWhsUnitQty()
		{
			base.CheckJI_BondedWhsUnitQty();
			if (Parent.JI_BondedWhsUnitQty.IsEmpty && (Parent.Declaration?.IsBondedWarehouseAutomationOn ?? false))
			{
				Parent.JI_BondedWhsUnitQtyInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Parent.JI_BondedWhsUnitQtyInfo.HumanReadableName));
			}
		}

		protected override void CheckJI_Procedure()
		{
			base.CheckJI_Procedure();
			var procedure = Parent.CusProcedure;
			if (procedure != null)
			{
				var declaration = Parent.Declaration;
				var declarationMessageType = declaration?.JE_MessageType ?? ZString.Empty;
				var targetInfo = Parent.JI_ProcedureInfo;
				if (!declarationMessageType.IsEmpty && !procedure.ShipmentTypes.Any(x => x.ToUpper() == declarationMessageType))
				{
					var shipmentTypeDesc = declaration.Lookups.MessageTypeList.GetDescriptionFromCode(declarationMessageType) ?? declarationMessageType;
					targetInfo.AddMessageError(Res.GetString("7a35fb0e-9d13-48ec-aadf-31056466a144", "{0} Procedure Code should be selected when Shipment Type is {0}", shipmentTypeDesc));
				}

				var instruction = Parent.EntryInstruction;
				var declarationType = instruction?.CEI_Style ?? ZString.Empty;
				if (!declarationType.IsEmpty && !procedure.Groups.Any(x => x.ToUpper() == declarationType))
				{
					var declarationTypeDesc = instruction?.Lookups.StyleList.GetDescriptionFromCode(declarationType) ?? declarationType;
					targetInfo.AddMessageError(Res.GetString("70c9622f-98bb-4c9d-bb43-105793e49b1f", "{0} Procedure Code should be selected when Declaration Type is {0}", declarationTypeDesc));
				}
			}
		}
	}
}
