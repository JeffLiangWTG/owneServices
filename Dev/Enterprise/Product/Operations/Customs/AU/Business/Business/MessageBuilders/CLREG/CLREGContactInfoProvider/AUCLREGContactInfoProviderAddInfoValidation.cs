using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AUCLREGContactInfoProviderAddInfoValidation : AutoAUCLREGContactInfoProviderAddInfoValidation
	{
		public AUCLREGContactInfoProviderAddInfoValidation(AutoAUCLREGContactInfoProviderAddInfo parent) : base(parent)
		{
		}

		CLREGInfoProvider CLREGProvider
		{
			get { return ((CLREGContactInfoProviderAddInfo)Parent).CLREGContactInfoProvider.CLREGProvider; }
		}

		ZBool IsABNWithoutCAC
		{
			get { return CLREGProvider != null && !CLREGProvider.ABN.IsEmpty && CLREGProvider.CAC.IsEmpty; }
		}

		ZBool IsOrganisation
		{
			get { return CLREGProvider != null && CLREGProvider.IsOrganisation; }
		}

		ZBool IsIndividual
		{
			get { return CLREGProvider != null && CLREGProvider.IsIndividual; }
		}

		ZBool HasNoContactData
		{
			get { return (IsABNWithoutCAC || IsOrganisation || IsIndividual) && !HasContactData; }
		}

		ZBool HasContactData
		{
			get
			{
				return !Parent.ZA_ContAH.IsEmpty || !Parent.ZA_ContPh.IsEmpty || !Parent.ZA_ContFax.IsEmpty ||
					!Parent.ZA_ContMob.IsEmpty || !Parent.ZA_ContEmail.IsEmpty ||
					!Parent.ZA_Cont1.IsEmpty || !Parent.ZA_Cont2.IsEmpty || !Parent.ZA_ContCity.IsEmpty ||
					!Parent.ZA_ContPort.IsEmpty || !Parent.ZA_ContPostCode.IsEmpty || !Parent.ZA_ContState.IsEmpty;
			}
		}

		ZBool HasContactDataForIndividual
		{
			get
			{
				return !Parent.ZA_ContPost1.IsEmpty || !Parent.ZA_ContPostCity.IsEmpty || !Parent.ZA_ContPostPort.IsEmpty ||
					!Parent.ZA_ContPostPostCode.IsEmpty || !Parent.ZA_ContPostState.IsEmpty;
			}
		}

		protected override void CheckZA_ContPort()
		{
			base.CheckZA_ContPort();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZA_ContPortInfo, Parent.Lookups.RefUNLOCOs);

			if (HasNoContactData)
			{
				Parent.ZA_ContPortInfo.AddMessageError(ContactDataRequired);
			}

			ValidateZA_ContAH();
			ValidateZA_ContFax();
			ValidateZA_ContEmail();
			ValidateZA_ContMob();
			ValidateZA_ContPh();
			ValidateZA_Cont1();
			ValidateZA_Cont2();
			ValidateZA_ContState();
			ValidateZA_ContPostCode();
		}

		protected override void CheckZA_ContPostCode()
		{
			base.CheckZA_ContPostCode();

			if (Parent.ZA_ContPort.SubstringSafe(0, 2) == Core.Constants.CountryCodes.Australia &&
				Parent.ZA_ContPostCode.Length > 4)
			{
				Parent.ZA_ContPostCodeInfo.AddMessageError(AUCLREGInfoProviderAddInfoValidation.Constants.InvalidPostCode);
			}

			if (HasNoContactData)
			{
				Parent.ZA_ContPostCodeInfo.AddMessageError(ContactDataRequired);
			}

			ValidateZA_ContAH();
			ValidateZA_ContFax();
			ValidateZA_ContEmail();
			ValidateZA_ContMob();
			ValidateZA_ContPh();
			ValidateZA_Cont1();
			ValidateZA_Cont2();
			ValidateZA_ContPort();
			ValidateZA_ContState();
		}

		protected override void CheckZA_ContState()
		{
			base.CheckZA_ContState();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZA_ContStateInfo, Parent.Lookups.OA_StateListForContactAddress);

			if (HasNoContactData)
			{
				Parent.ZA_ContStateInfo.AddMessageError(ContactDataRequired);
			}

			ValidateZA_ContAH();
			ValidateZA_ContFax();
			ValidateZA_ContEmail();
			ValidateZA_ContMob();
			ValidateZA_ContPh();
			ValidateZA_Cont1();
			ValidateZA_Cont2();
			ValidateZA_ContPort();
			ValidateZA_ContPostCode();
		}

		protected override void CheckZA_ContName()
		{
			base.CheckZA_ContName();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ZA_ContNameInfo);
		}

		protected override void CheckZA_ContPurpose()
		{
			base.CheckZA_ContPurpose();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ZA_ContPurposeInfo);
		}

		protected override void CheckZA_ContAH()
		{
			base.CheckZA_ContAH();

			if (HasNoContactData)
			{
				Parent.ZA_ContAHInfo.AddMessageError(ContactDataRequired);
			}

			ValidateZA_ContAHPref();
			ValidateZA_ContEmail();
			ValidateZA_ContFax();
			ValidateZA_ContMob();
			ValidateZA_ContPh();
			ValidateZA_Cont1();
			ValidateZA_Cont2();
			ValidateZA_ContCity();
			ValidateZA_ContPort();
			ValidateZA_ContState();
			ValidateZA_ContPostCode();
		}
		internal const string ContactDataRequired = "At least one of the following must be supplied: Contact Business Address details, Contact Phone, Contact Fax, Contact AH, Contact Mobile or Contact Email.";
		internal const string ContactDataRequiredForIndiv = "At least one of the following must be supplied: Contact Business Address details, Contact Postal Address details, Contact Phone, Contact Fax, Contact AH, Contact Mobile or Contact Email.";

		protected override void CheckZA_ContAHPref()
		{
			base.CheckZA_ContAHPref();

			if (!Parent.ZA_ContAH.IsEmpty && Parent.ZA_ContAHPref.IsEmpty)
			{
				Parent.ZA_ContAHPrefInfo.AddMessageError(string.Format(PhPrefixRequired, "After Hours Phone"));
			}
		}

		protected override void CheckZA_ContEmail()
		{
			base.CheckZA_ContEmail();
			if (HasNoContactData)
			{
				Parent.ZA_ContEmailInfo.AddMessageError(ContactDataRequired);
			}
			ValidateZA_ContAH();
			ValidateZA_ContFax();
			ValidateZA_ContMob();
			ValidateZA_ContPh();
			ValidateZA_Cont1();
			ValidateZA_Cont2();
			ValidateZA_ContCity();
			ValidateZA_ContPort();
			ValidateZA_ContState();
			ValidateZA_ContPostCode();
		}

		protected override void CheckZA_ContFax()
		{
			base.CheckZA_ContFax();
			if (HasNoContactData)
			{
				Parent.ZA_ContFaxInfo.AddMessageError(ContactDataRequired);
			}

			ValidateZA_ContFaxPref();
			ValidateZA_ContAH();
			ValidateZA_ContEmail();
			ValidateZA_ContMob();
			ValidateZA_ContPh();
			ValidateZA_Cont1();
			ValidateZA_Cont2();
			ValidateZA_ContCity();
			ValidateZA_ContPort();
			ValidateZA_ContState();
			ValidateZA_ContPostCode();
		}

		protected override void CheckZA_ContFaxPref()
		{
			base.CheckZA_ContFaxPref();

			if (!Parent.ZA_ContFax.IsEmpty && Parent.ZA_ContFaxPref.IsEmpty)
			{
				Parent.ZA_ContFaxPrefInfo.AddMessageError(string.Format(PhPrefixRequired, "Fax"));
			}
		}

		protected override void CheckZA_ContMob()
		{
			base.CheckZA_ContMob();
			if (HasNoContactData)
			{
				Parent.ZA_ContMobInfo.AddMessageError(ContactDataRequired);
			}
			ValidateZA_ContAH();
			ValidateZA_ContFax();
			ValidateZA_ContEmail();
			ValidateZA_ContPh();
			ValidateZA_Cont1();
			ValidateZA_Cont2();
			ValidateZA_ContCity();
			ValidateZA_ContPort();
			ValidateZA_ContState();
			ValidateZA_ContPostCode();
		}

		protected override void CheckZA_ContPh()
		{
			base.CheckZA_ContPh();
			if (HasNoContactData)
			{
				Parent.ZA_ContPhInfo.AddMessageError(ContactDataRequired);
			}

			ValidateZA_ContPhPref();
			ValidateZA_ContAH();
			ValidateZA_ContFax();
			ValidateZA_ContEmail();
			ValidateZA_ContMob();
			ValidateZA_Cont1();
			ValidateZA_Cont2();
			ValidateZA_ContCity();
			ValidateZA_ContPort();
			ValidateZA_ContState();
			ValidateZA_ContPostCode();
		}

		protected override void CheckZA_ContPhPref()
		{
			base.CheckZA_ContPhPref();

			if (!Parent.ZA_ContPh.IsEmpty && Parent.ZA_ContPhPref.IsEmpty)
			{
				Parent.ZA_ContPhPrefInfo.AddMessageError(string.Format(PhPrefixRequired, "Phone"));
			}
		}
		internal const string PhPrefixRequired = "Area code is required if {0} is entered.";

		protected override void CheckZA_Cont1()
		{
			base.CheckZA_Cont1();

			if (HasNoContactData)
			{
				Parent.ZA_Cont1Info.AddMessageError(ContactDataRequired);
			}

			ValidateZA_ContAH();
			ValidateZA_ContFax();
			ValidateZA_ContEmail();
			ValidateZA_ContMob();
			ValidateZA_ContPh();
			ValidateZA_Cont2();
			ValidateZA_ContCity();
			ValidateZA_ContPort();
			ValidateZA_ContState();
			ValidateZA_ContPostCode();
		}

		protected override void CheckZA_Cont2()
		{
			base.CheckZA_Cont2();

			if (HasNoContactData)
			{
				Parent.ZA_Cont2Info.AddMessageError(ContactDataRequired);
			}

			ValidateZA_ContAH();
			ValidateZA_ContFax();
			ValidateZA_ContEmail();
			ValidateZA_ContMob();
			ValidateZA_ContPh();
			ValidateZA_Cont1();
			ValidateZA_ContCity();
			ValidateZA_ContPort();
			ValidateZA_ContState();
			ValidateZA_ContPostCode();
		}

		protected override void CheckZA_ContCity()
		{
			base.CheckZA_ContCity();
			if (HasNoContactData)
			{
				Parent.ZA_ContCityInfo.AddMessageError(ContactDataRequired);
			}

			ValidateZA_ContAH();
			ValidateZA_ContFax();
			ValidateZA_ContEmail();
			ValidateZA_ContMob();
			ValidateZA_ContPh();
			ValidateZA_Cont1();
			ValidateZA_Cont2();
			ValidateZA_ContPort();
			ValidateZA_ContState();
			ValidateZA_ContPostCode();
		}

		#region Contact Postal Address Validation

		protected override void CheckZA_ContPostPort()
		{
			base.CheckZA_ContPostPort();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZA_ContPostPortInfo, Parent.Lookups.RefUNLOCOs);

			if (IsIndividual && !HasContactData && !HasContactDataForIndividual)
			{
				Parent.ZA_ContPostPortInfo.AddMessageError(ContactDataRequiredForIndiv);
			}

			ValidateZA_ContAH();
			ValidateZA_ContEmail();
			ValidateZA_ContFax();
			ValidateZA_ContMob();
			ValidateZA_ContPh();
			ValidateZA_Cont1();
			ValidateZA_Cont2();
			ValidateZA_ContCity();
			ValidateZA_ContPort();
			ValidateZA_ContState();
			ValidateZA_ContPostCode();
			ValidateZA_ContPost1();
			ValidateZA_ContPostCity();
			ValidateZA_ContPostState();
			ValidateZA_ContPostPostCode();
			ValidateZA_ContPostState();
		}

		protected override void CheckZA_ContPostPostCode()
		{
			base.CheckZA_ContPostPostCode();

			if (Parent.ZA_ContPostPort.SubstringSafe(0, 2) == Core.Constants.CountryCodes.Australia &&
				Parent.ZA_ContPostPostCode.Length > 4)
			{
				Parent.ZA_ContPostPostCodeInfo.AddMessageError(AUCLREGInfoProviderAddInfoValidation.Constants.InvalidPostCode);
			}

			if (IsIndividual && !HasContactData && !HasContactDataForIndividual)
			{
				Parent.ZA_ContPostPostCodeInfo.AddMessageError(ContactDataRequiredForIndiv);
			}

			ValidateZA_ContAH();
			ValidateZA_ContEmail();
			ValidateZA_ContFax();
			ValidateZA_ContMob();
			ValidateZA_ContPh();
			ValidateZA_Cont1();
			ValidateZA_Cont2();
			ValidateZA_ContCity();
			ValidateZA_ContPort();
			ValidateZA_ContState();
			ValidateZA_ContPostCode();
			ValidateZA_ContPost1();
			ValidateZA_ContPostCity();
			ValidateZA_ContPostState();
			ValidateZA_ContPostState();
			ValidateZA_ContPostPort();
		}

		protected override void CheckZA_ContPostState()
		{
			base.CheckZA_ContPostState();
			ListValidation.MessageErrorIfInvalidCode(Parent.ZA_ContPostStateInfo, Parent.Lookups.OA_StateListForContactPostalAddress);

			if (IsIndividual && !HasContactData && !HasContactDataForIndividual)
			{
				Parent.ZA_ContPostStateInfo.AddMessageError(ContactDataRequiredForIndiv);
			}

			ValidateZA_ContAH();
			ValidateZA_ContEmail();
			ValidateZA_ContFax();
			ValidateZA_ContMob();
			ValidateZA_ContPh();
			ValidateZA_Cont1();
			ValidateZA_Cont2();
			ValidateZA_ContCity();
			ValidateZA_ContPort();
			ValidateZA_ContState();
			ValidateZA_ContPostCode();
			ValidateZA_ContPost1();
			ValidateZA_ContPostCity();
			ValidateZA_ContPostPort();
			ValidateZA_ContPostState();
			ValidateZA_ContPostPostCode();
		}

		protected override void CheckZA_ContPost1()
		{
			base.CheckZA_ContPost1();

			if (IsIndividual && !HasContactData && !HasContactDataForIndividual)
			{
				Parent.ZA_ContPost1Info.AddMessageError(ContactDataRequiredForIndiv);
			}

			ValidateZA_ContAH();
			ValidateZA_ContEmail();
			ValidateZA_ContFax();
			ValidateZA_ContMob();
			ValidateZA_ContPh();
			ValidateZA_Cont1();
			ValidateZA_Cont2();
			ValidateZA_ContCity();
			ValidateZA_ContPort();
			ValidateZA_ContState();
			ValidateZA_ContPostCode();
			ValidateZA_ContPostCity();
			ValidateZA_ContPostPort();
			ValidateZA_ContPostState();
			ValidateZA_ContPostPostCode();
		}

		protected override void CheckZA_ContPostCity()
		{
			base.CheckZA_ContPostCity();

			if (IsIndividual && !HasContactData && !HasContactDataForIndividual)
			{
				Parent.ZA_ContPostCityInfo.AddMessageError(ContactDataRequiredForIndiv);
			}

			ValidateZA_ContAH();
			ValidateZA_ContEmail();
			ValidateZA_ContFax();
			ValidateZA_ContMob();
			ValidateZA_ContPh();
			ValidateZA_Cont1();
			ValidateZA_Cont2();
			ValidateZA_ContCity();
			ValidateZA_ContPort();
			ValidateZA_ContState();
			ValidateZA_ContPostCode();
			ValidateZA_ContPost1();
			ValidateZA_ContPostPort();
			ValidateZA_ContPostState();
			ValidateZA_ContPostPostCode();
		}

		#endregion
	}
}
