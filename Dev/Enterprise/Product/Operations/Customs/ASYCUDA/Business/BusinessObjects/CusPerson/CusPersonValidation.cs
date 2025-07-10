using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class CusPersonValidation : Customs.Business.CusPersonValidation
	{
		public CusPersonValidation(CusPerson parent)
			: base(parent)
		{
		}

		public new CusPerson Parent => (CusPerson)base.Parent;

		protected override void CheckCPN_PER_Person()
		{
			base.CheckCPN_PER_Person();
			CheckPersonIsUnique();
			if (Parent.Person != null)
			{
				CheckBirthDate();
				CheckPersonCountry();
				CheckIdentificationNumber();
				CheckPersonGender();
				CheckPersonNationality();
				CheckPersonPassport();
				CheckPersonPassportExpiry();
			}
		}

		protected override void CheckCPN_IsPassenger()
		{
			base.CheckCPN_IsPassenger();
			var header = Parent.Header;
			if (header != null && !header.IsDeleted)
			{
				var driverCount = header.Persons.Cast<CusPerson>().Where(p => p.IsDriver).Take(2);
				if (driverCount.Count() != 1)
				{
					Parent.CPN_IsPassengerInfo.AddMessageError("Exactly one person should be marked as the driver, i.e. not a passenger.");
				}
			}
		}

		void CheckPersonIsUnique()
		{
			var header = Parent.Header;
			if (header != null && !header.IsDeleted)
			{
				var duplicatePerson = header.Persons.Cast<CusPerson>().FirstOrDefault(p => !p.IsDeleted && p.PK != Parent.PK && p.CPN_PER_Person == Parent.CPN_PER_Person);
				if (duplicatePerson != null)
				{
					Parent.CPN_PER_PersonInfo.AddError("Person must be unique");
				}
			}
		}

		void CheckBirthDate()
		{
			if (Parent.PersonBirthDate.IsEmpty)
			{
				Parent.CPN_PER_PersonInfo.AddMessageError("Please enter a Birth Date against the person");
			}
		}

		void CheckPersonCountry()
		{
			if (Parent.PersonCountry.IsEmpty)
			{
				Parent.CPN_PER_PersonInfo.AddMessageError("Please enter a Country against the person");
			}
		}

		protected virtual void CheckIdentificationNumber()
		{
			if (IdentificationNumberMandatoryCondition && Parent.PersonIdentificationNumber.IsEmpty)
			{
				Parent.CPN_PER_PersonInfo.AddMessageError("Please enter an Identification Number against the person");
			}
		}

		protected virtual bool IdentificationNumberMandatoryCondition => true;

		void CheckPersonGender()
		{
			var personGender = Parent.PersonGender;
			if (personGender != Core.Constants.Genders.Man && personGender != Core.Constants.Genders.Woman)
			{
				Parent.CPN_PER_PersonInfo.AddMessageError("Please update the Gender against the person. Only Male or Female is valid");
			}
		}

		void CheckPersonNationality()
		{
			if (Parent.PersonNationality.IsEmpty)
			{
				Parent.CPN_PER_PersonInfo.AddMessageError("Please enter a Nationality against the person");
			}
		}

		void CheckPersonPassport()
		{
			if (PersonPassportMandatoryCondition && Parent.PersonPassport.IsEmpty)
			{
				Parent.CPN_PER_PersonInfo.AddMessageError("Please enter Passport Details against the person");
			}
		}

		protected virtual bool PersonPassportMandatoryCondition => true;

		void CheckPersonPassportExpiry()
		{
			var personpassportExpiryDate = Parent.PersonPassportExpiry;
			if (PersonPassportExpiryMandatoryCondition && personpassportExpiryDate.IsEmpty)
			{
				Parent.CPN_PER_PersonInfo.AddMessageError("Please enter a Passport Expiry Date against the person");
			}
			if (personpassportExpiryDate < ZDate.Today)
			{
				Parent.CPN_PER_PersonInfo.AddMessageError("Please update the Passport Expiry Date against the person as it has expired");
			}
		}

		protected virtual bool PersonPassportExpiryMandatoryCondition => true;
	}
}
