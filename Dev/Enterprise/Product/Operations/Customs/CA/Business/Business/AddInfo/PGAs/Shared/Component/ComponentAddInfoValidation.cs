//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoComponentAddInfoValidation
//
//    This class should be used for overriding validation in AutoComponentAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.CA.Business
{
	using System.Globalization;
	using CargoWise.EntityFramework;
	using Enterprise.Customs.Business;

	//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
	#pragma warning disable IDE0079
	#pragma warning disable IDE0005
	using CargoWiseOne.ResourceStrings;
	#pragma warning restore IDE0005, IDE0079

	public class ComponentAddInfoValidation : AutoComponentAddInfoValidation
	{
		public ComponentAddInfoValidation(AutoComponentAddInfo parent) : base(parent)
		{
		}

		IPGAHeader PGAHeader
		{
			get
			{
				if (fPGAHeader == null)
				{
					fPGAHeader = (((ComponentAddInfo)Parent).Parent as Component)?.PGAHeader;
				}
				return fPGAHeader;
			}
		}
		IPGAHeader fPGAHeader;

		protected override void CheckCA_Name()
		{
			base.CheckCA_Name();

			if (PGAHeader is CNSCPGAHeader)
			{
				var cnscHeader = PGAHeader as CNSCPGAHeader;
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_NameInfo);

				if (cnscHeader.IsControlledSubstance)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.CA_NameInfo, Parent.Lookups.SpecificationList);
				}
			}
			else if (PGAHeader is HCPGAHeader)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_NameInfo);
			}
			else if (PGAHeader is ECCCPGAHeader)
			{
				var ecccHeader = PGAHeader as ECCCPGAHeader;
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_NameInfo);

				if (ecccHeader.CA_ODSProgramInd == YesNoList.Codes.Yes && ecccHeader.Components.Count > 1)
				{
					Parent.CA_NameInfo.AddMessageError(ComponentsAllowedOnPGA(1));
				}
			}
		}

		protected override void CheckCA_Type()
		{
			base.CheckCA_Type();

			var componentTypes = Parent.Lookups.ComponentTypes;
			if (componentTypes.Count > 0)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_TypeInfo, componentTypes);
			}
		}

		internal static string ComponentsAllowedOnPGA(int count)
		{
			return Res.GetString("7F4C3E79-AB2C-4C97-8DC0-498003F53083", "Component collection must have only {0} element(s)", count);
		}

		protected override void CheckCA_Qty()
		{
			base.CheckCA_Qty();
			if (PGAHeader is HCPGAHeader || PGAHeader is CNSCPGAHeader)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_QtyInfo);
			}
			if (PGAHeader is HCPGAHeader && !Parent.CA_Name.IsEmpty && Parent.CA_Qty == 0)
			{
				Parent.CA_QtyInfo.AddMessageError(string.Format(CultureInfo.InvariantCulture, QuantityAndUQCannotBeBlank, Parent.CA_Name));
			}
		}

		protected override void CheckCA_UQ()
		{
			base.CheckCA_UQ();
			if (Parent.CA_Qty > 0 && (PGAHeader is HCPGAHeader || PGAHeader is CNSCPGAHeader))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_UQInfo);
			}
			if (PGAHeader is HCPGAHeader && !Parent.CA_Name.IsEmpty && Parent.CA_UQ.IsEmpty)
			{
				Parent.CA_UQInfo.AddMessageError(string.Format(CultureInfo.InvariantCulture, QuantityAndUQCannotBeBlank, Parent.CA_Name));
			}

			if (PGAHeader != null && PGAHeader.GovAgencyIDCode == PGACodes.Codes.CNSC)
			{
				ListValidation.WarnIfInvalidCode(Parent.CA_UQInfo, Parent.Lookups.UnitList);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CA_UQInfo, Parent.Lookups.UnitList);
			}
		}

		protected override void CheckCA_Origin()
		{
			base.CheckCA_Origin();

			var ecccHeader = PGAHeader as ECCCPGAHeader;
			if (ecccHeader != null && ecccHeader.CA_ODSProgramInd == YesNoList.Codes.Yes)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CA_OriginInfo);
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.CA_OriginInfo, Parent.Lookups.Countries);
		}

		protected override void CheckCA_Concentration()
		{
			base.CheckCA_Concentration();
			if (Parent.CA_Concentration > 100 || Parent.CA_Concentration < 0)
			{
				Parent.CA_ConcentrationInfo.AddMessageError(Res.GetString("7f2f6f86-8515-4696-afaa-66a3a0150beb", "Concentration should not be greater than 100 and the minimum value should not be less than 0"));
			}
		}

		public const string QuantityAndUQCannotBeBlank = "Quantity and UQ for Ingredient {0} can’t be blank";
	}
}
