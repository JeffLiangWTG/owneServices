using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.JP.Business
{
	public class MoveInDestinationValidation : CusSupportingInfoValidation
	{
		public MoveInDestinationValidation(MoveInDestination parent) : base(parent)
		{
		}

		new MoveInDestination Parent => (MoveInDestination)base.Parent;

		CusEntryInstruction EntryInstruction => Parent.Parent;

		JobDeclaration Declaration => EntryInstruction.JobDeclaration;

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();

			var parent = Parent;
			var parentCollection = parent.ParentCollection;
			var info = parent.CSI_CodeInfo;

			var code = parent.CSI_Code;
			if (parentCollection.Cast<MoveInDestination>().Any(x => x != parent && !string.IsNullOrWhiteSpace(x.CSI_Code) && x.CSI_Code == code))
			{
				info.AddMessageError(Res.GetString("EA8C39B5-BAFB-40CE-A153-EAB267274377", "Destination must be unique."));
			}

			var codeFisrtChar = code.SubstringSafe(0, 1);
			if (parentCollection.Cast<MoveInDestination>().Any(x => x != parent && !x.CSI_Code.StartsWith(codeFisrtChar)))
			{
				info.AddMessageError(Res.GetString("E04F3388-419C-4858-B769-FC1047E08A0C", "Destinations in the grid must be from the same customs."));
			}

			if (EntryInstruction.IsECR)
			{
				MandatoryValidation.MessageErrorIfNotEntered(info);
			}

			ListValidation.MessageErrorIfInvalidCode(info);
		}

		protected override void CheckCSI_DateOfIssue()
		{
			base.CheckCSI_DateOfIssue();
			var parent = Parent;
			MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_DateOfIssueInfo);
			if ((EntryInstruction.IsECR || Declaration.IsECRSendingInProgress) && (parent.CSI_DateOfIssue > ZDateTime.Today.AddDays(13) || parent.CSI_DateOfIssue.IsInThePastDatePartOnly))
			{
				parent.CSI_DateOfIssueInfo.AddMessageError(Res.GetString("4B85A09E-7DA5-4D42-821D-90505D48ECF4", "Move-In Date must be within 13 days from today."));
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			ListValidation.MessageErrorIfInvalidCode(Parent.CSI_ReferenceNumberInfo);
		}

		protected override void CheckCSI_Quantity()
		{
			base.CheckCSI_Quantity();

			var parent = Parent;
			var info = parent.CSI_QuantityInfo;
			CheckTotalExceeded(x => x.TotalCargoQuantity, x => x.CEI_CargoQuantity, info, Res.GetString("1B4AC9FA-3DB8-4EA9-B392-8F017F24C549", "The total quantity (Move in) must not exceed the cargo quantity."));
			MandatoryValidation.MessageErrorIfIsNegative(info);

			if (EntryInstruction.IsECR)
			{
				MandatoryValidation.MessageErrorIfNotEntered(info);
			}
		}

		protected override void CheckCSI_Quantity2()
		{
			base.CheckCSI_Quantity2();

			var parent = Parent;
			var info = parent.CSI_Quantity2Info;
			CheckTotalExceeded(x => x.TotalCustomsWeight, x => x.CEI_CustomsWeight, info, Res.GetString("BA6963D7-96AB-4B15-BADB-874C5FA5BE61", "The total weight (Move in) must not exceed the cargo weight."));

			TypeValidation.CheckValidDecimal(info, 9, 3);
			MandatoryValidation.MessageErrorIfIsNegative(info);

			if (EntryInstruction.IsECR)
			{
				MandatoryValidation.MessageErrorIfNotEntered(info);
			}
		}

		protected override void CheckCSI_Quantity3()
		{
			base.CheckCSI_Quantity3();

			var parent = Parent;
			var info = parent.CSI_Quantity3Info;
			CheckTotalExceeded(x => x.TotalCustomsVolume, x => x.CEI_CustomsVolume, info, Res.GetString("94332F87-0014-48F9-A68E-922CEE9ABE35", "The total volume (Move in) must not exceed the cargo volume."));
			TypeValidation.CheckValidDecimal(info, 9, 3);
			MandatoryValidation.MessageErrorIfIsNegative(info);
		}

		void CheckTotalExceeded(Func<MoveInDestinationCollection, decimal> sumSelector, Func<CusEntryInstruction, decimal> totalOnParentSelector, ZPropertyInfo targetInfo, string message)
		{
			if (sumSelector.Invoke(Parent.ParentCollection) > totalOnParentSelector.Invoke(EntryInstruction))
			{
				targetInfo.AddMessageError(message);
			}
		}

		protected override void CheckCSI_Description()
		{
			base.CheckCSI_Description();

			if (EntryInstruction.IsECR)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_DescriptionInfo);
			}
		}
	}
}
