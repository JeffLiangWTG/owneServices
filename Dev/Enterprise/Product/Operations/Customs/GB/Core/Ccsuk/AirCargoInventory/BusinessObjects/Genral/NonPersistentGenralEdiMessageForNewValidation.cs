using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral
{
	public class NonPersistentGenralEdiMessageForNewValidation : AutoNonPersistentGenralEdiMessageForNewValidation
	{
		public NonPersistentGenralEdiMessageForNewValidation(AutoNonPersistentGenralEdiMessageForNew parent)
			: base(parent) { }

		protected override void CheckSendingProfile()
		{
			base.CheckSendingProfile();
			if (Parent.SendingProfile.IsEmpty)
			{
				MandatoryValidation.CheckEntered(Parent.SendingProfileInfo);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(Parent.SendingProfileInfo);
				if (Parent.SendingProfile.StartsWith(LicenceAndPimaHelper.ShedProfilePrefix))
				{
					CusHAWBValidation.EnsureSecurityRightForShedPima(Parent.SendingProfileInfo);
				}
			}
		}

		protected override void CheckShedOrBadge()
		{
			base.CheckShedOrBadge();
			if (Parent.IsRecipientAgent || Parent.IsRecipientShed)
			{
				MandatoryValidation.CheckEntered(Parent.ShedOrBadgeInfo);
			}
		}

		protected override void CheckAirport()
		{
			base.CheckAirport();
			if (Parent.IsRecipientShed)
			{
				if (Parent.Airport.Length != 3)
				{
					Parent.AirportInfo.AddError("Please select a valid 3-char airport code so that the recipient PIMA can be generated. The airport should be that at which the shed is located");
				}
			}
		}

		protected override void CheckPreformattedLinesOf70()
		{
			base.CheckPreformattedLinesOf70();
			ValidateBodyAndFormattingCheckbox(Parent.PreformattedLinesOf70Info);
			ValidatePayload();
		}

		protected override void CheckPayload()
		{
			base.CheckPayload();
			ValidateBodyAndFormattingCheckbox(Parent.PayloadInfo);
			ValidatePreformattedLinesOf70();
		}

		void ValidateBodyAndFormattingCheckbox(ZPropertyInfo zPropertyInfo)
		{
			if (Parent.PreformattedLinesOf70)
			{
				var lines = Regex.Split(Parent.Payload, System.Environment.NewLine);
				if (lines.Length > 20)
				{
					zPropertyInfo.AddError("When preformatted, the body cannot contain more than 20 lines. Truncate it or send two messages.");
				}
				if ((from string l in lines where l.Length > 70 select l).Any())
				{
					zPropertyInfo.AddError("When preformatted, the body cannot contain lines longer than 70 characters. Shorten the long lines.");
				}
			}
		}
	}
}
