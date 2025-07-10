using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ContactSalutationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ContactSalutationCollection()
			: base()
		{ }

		public ContactSalutationCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public ContactSalutationCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{ }

		public ContactSalutationCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{ }

		public new ContactSalutation this[int index]
		{
			get { return (ContactSalutation)Elements[index]; }
		}

		public new ContactSalutation AddNew()
		{
			return (ContactSalutation)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ContactSalutation(CurrentFallbackLevel, CurrentFactory);
		}

		#region Implementation

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ContactSalutationCollection(fallbackLevel, factory);
		}

		#endregion
	}

	public static class SalutationHelper
	{
		public static void LoadDefaultSalutations(ContactSalutationCollection parent)
		{
			AddDefaultAllGendersSalutations(parent);
			AddDefaultMaleSalutations(parent);
			AddDefaultFemaleSalutations(parent);
		}

		static void AddDefaultAllGendersSalutations(ContactSalutationCollection parent)
		{
			AddDefaultSalutation(Constants.DefaultSalutations.DefaultSalutation, parent, Core.Constants.SalutationGenders.All);
			AddDefaultSalutation(Constants.DefaultSalutations.HowAreYou, parent, Core.Constants.SalutationGenders.All);
			AddDefaultSalutation(Constants.DefaultSalutations.MyDearSirMadam, parent, Core.Constants.SalutationGenders.All);
			AddDefaultSalutation(Constants.DefaultSalutations.SirMadam, parent, Core.Constants.SalutationGenders.All);
			AddDefaultSalutation(Constants.DefaultSalutations.ToWhomItMayConcern, parent, Core.Constants.SalutationGenders.All);
			AddDefaultSalutation(Constants.DefaultSalutations.DearUser, parent, Core.Constants.SalutationGenders.All);
			AddDefaultSalutation(Constants.DefaultSalutations.ToWhomItMayConcernHowAreYou, parent, Core.Constants.SalutationGenders.All);
			AddDefaultSalutation(Constants.DefaultSalutations.SirMadamHowAreYou, parent, Core.Constants.SalutationGenders.All);
			AddDefaultSalutation(Constants.DefaultSalutations.DearJobTitle, parent, Core.Constants.SalutationGenders.All);
		}

		static void AddDefaultMaleSalutations(ContactSalutationCollection parent)
		{
			AddDefaultSalutation(Constants.DefaultSalutations.DearNameMale, parent, Core.Constants.SalutationGenders.Man);
			AddDefaultSalutation(Constants.DefaultSalutations.DearProfessionNameMale, parent, Core.Constants.SalutationGenders.Man);
			AddDefaultSalutation(Constants.DefaultSalutations.DearMale, parent, Core.Constants.SalutationGenders.Man);
			AddDefaultSalutation(Constants.DefaultSalutations.MyDearSir, parent, Core.Constants.SalutationGenders.Man);
			AddDefaultSalutation(Constants.DefaultSalutations.Sir, parent, Core.Constants.SalutationGenders.Man);
			AddDefaultSalutation(Constants.DefaultSalutations.DearUserMale, parent, Core.Constants.SalutationGenders.Man);
		}

		static void AddDefaultFemaleSalutations(ContactSalutationCollection parent)
		{
			AddDefaultSalutation(Constants.DefaultSalutations.DearNameFemale, parent, Core.Constants.SalutationGenders.Woman);
			AddDefaultSalutation(Constants.DefaultSalutations.DearProfessionNameFemale, parent, Core.Constants.SalutationGenders.Woman);
			AddDefaultSalutation(Constants.DefaultSalutations.DearFemale, parent, Core.Constants.SalutationGenders.Woman);
			AddDefaultSalutation(Constants.DefaultSalutations.MyDearMadam, parent, Core.Constants.SalutationGenders.Woman);
			AddDefaultSalutation(Constants.DefaultSalutations.Madam, parent, Core.Constants.SalutationGenders.Woman);
			AddDefaultSalutation(Constants.DefaultSalutations.DearUserFemale, parent, Core.Constants.SalutationGenders.Woman);
		}

		static void AddDefaultSalutation(MultilingualString multilingualSalutation, ContactSalutationCollection parent, string gender)
		{
			ContactSalutation salutation = parent.AddNew();
			salutation.Salutation = multilingualSalutation;
			salutation.Gender = gender;
		}

		public static IEnumerable<MultilingualString> GetSalutations(ZString contactGender)
		{
			var salutations = OrganisationsDataRegistry.Instance.ContactSalutation.Value.Cast<ContactSalutation>();
			MultilingualString[] result;

			if (contactGender == Constants.Genders.Man)
			{
				result = salutations.Where(s => s.Gender == Constants.SalutationGenders.Man || s.Gender == Constants.SalutationGenders.All).Select(s => s.RawSalutation).Distinct().ToArray();
			}
			else if (contactGender == Constants.Genders.Woman)
			{
				result = salutations.Where(s => s.Gender == Constants.SalutationGenders.Woman || s.Gender == Constants.SalutationGenders.All).Select(s => s.RawSalutation).Distinct().ToArray();
			}
			else
			{
				result = salutations.Select(s => s.RawSalutation).Distinct().ToArray();
			}

			if (result.Length == 0)
			{
				result = new MultilingualString[] { Constants.DefaultSalutations.DefaultSalutation };
			}
			return result;
		}
	}
}
