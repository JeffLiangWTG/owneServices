using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ContactSalutation : RegistryBusinessObjectTemplate
	{
		public ContactSalutation() { }

		public ContactSalutation(BusinessObjectFactory factory)
			: base(factory) { }

		public ContactSalutation(FallbackLevel fallbackLevel)
			: base(fallbackLevel) { }

		public ContactSalutation(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory) { }

		#region schema

		public static class Schema
		{
			public const string Salutation = "Salutation";
			public const string Gender = "Gender";
			public const int GenderMaxLength = 6;
			public const int SalutationMaxLength = 50;
		}

		#endregion

		#region Method Overrides

		protected override void RunPreSaveValidationCore()
		{
			ValidateGender();
			base.RunPreSaveValidationCore();
		}

		#endregion

		#region Bound Properties

		[MaxLength(Schema.SalutationMaxLength)]
		public MultilingualString Salutation
		{
			get { return salutation == null ? (NoResString)"" : salutation.Replace("[m] ", "").Replace("[f] ", ""); }
			set
			{
				salutation = value;
				SalutationInfo.RefreshBinding();
			}
		}
		MultilingualString salutation;

		public ZPropertyInfo SalutationInfo
		{
			get { return this.GetZPropertyInfo(nameof(Salutation)); }
		}

		[MaxLength(Schema.SalutationMaxLength)]
		public ZString EnglishSalutation
		{
			get { return Salutation.GetUnresolvedString(); }
			set
			{
				CheckMaximumLength(EnglishSalutationInfo, value);
				Salutation = (NoResString)value;
				EnglishSalutationInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo EnglishSalutationInfo
		{
			get { return this.GetZPropertyInfo(nameof(EnglishSalutation)); }
		}

		internal ZString AnnotatedEnglishSalutation
		{
			get
			{
				string prefix = Gender == Constants.SalutationGenders.Man ? (NoResString)"[m] " : (Gender == Constants.SalutationGenders.Woman ? (NoResString)"[f] " : "");
				return prefix + EnglishSalutation;
			}
		}

		internal MultilingualString RawSalutation
		{
			get { return salutation; }
		}

		[List("Genders")]
		[MaxLength(Schema.GenderMaxLength)]
		public ZString Gender
		{
			get { return gender; }
			set
			{
				if (gender != value)
				{
					CheckMaximumLength(GenderInfo, value);
					gender = value;
					ValidateGender();
					GenderInfo.RefreshBinding();
				}
			}
		}
		ZString gender;

		void ValidateGender()
		{
			GenderInfo.ClearAllNotifications();
			if (gender.IsEmpty)
			{
				GenderInfo.AddError(Res.GetString("8a64452b-dc36-455e-a661-ad599e205fb6", "Please specify gender"));
			}
		}

		public ZPropertyInfo GenderInfo
		{
			get { return this.GetZPropertyInfo(nameof(Gender)); }
		}

		public CodeDescriptionPairList Genders
		{
			get
			{
				if (genders == null)
				{
					genders = new CodeDescriptionPairList();
					genders.Add(new CodeDescriptionPair(Core.Constants.SalutationGenders.All, Core.Constants.SalutationGendersDescription.All));
					genders.Add(new CodeDescriptionPair(Core.Constants.SalutationGenders.Man, Core.Constants.SalutationGendersDescription.Man));
					genders.Add(new CodeDescriptionPair(Core.Constants.SalutationGenders.Woman, Core.Constants.SalutationGendersDescription.Woman));
				}
				return genders;
			}
		}
		CodeDescriptionPairList genders;

		#endregion

		#region RegistryBusinessObjectTemplate Members

		#region XML Serialisation

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			EnglishSalutation = reader.ReadElementString(Schema.Salutation);
			Gender = new ZString(reader.ReadElementString(Schema.Gender));
		}

		protected override void WriteElements(XmlWriter writer)
		{
			writer.WriteElementString(Schema.Salutation, EnglishSalutation);
			writer.WriteElementString(Schema.Gender, Gender);
		}

		#endregion

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ContactSalutation(fallbackLevel, factory) { salutation = Salutation, gender = Gender };
		}

		#endregion
	}
}
