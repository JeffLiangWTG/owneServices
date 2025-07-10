using System;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CountryDefaultLanguageBusinessObject : RegistryBusinessObjectTemplate
	{
		#region schema and constructors

		class Schema
		{
			public const string CountryPk = "CountryPk";
			public const string DefaultLanguage = "DefaultLanguage";
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CountryDefaultLanguageBusinessObject(factory);
		}

		public CountryDefaultLanguageBusinessObject()
		{
		}

		public CountryDefaultLanguageBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#endregion

		#region Properties

		#region CountryPK

		[List("CountryCollection")]
		public ZGuid CountryPk
		{
			get => countryPk;
			set
			{
				SetNonPersistentPropertyValue<ZGuid>(CountryPkInfo, ref countryPk, value);
				if (!IsValidationSuspended)
				{
					ValidateCountryPk();
				}
			}
		}

		public ZPropertyInfo CountryPkInfo => GetZPropertyInfo(Schema.CountryPk);

		public void ValidateCountryPk()
		{
			CountryPkInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(CountryPkInfo);
			TypeValidation.CheckValidGuid(CountryPkInfo);
			ListValidation.ErrorIfInvalidPK(CountryPkInfo, CountryCollection);
			if (ParentCollection != null && ParentCollection.OfType<CountryDefaultLanguageBusinessObject>().Count(x => x.countryPk == CountryPk && !CountryPk.IsEmpty) > 1)
			{
				CountryPkInfo.AddError(ResString.GetMultilingualString("B38EE212-F2B0-4077-B095-34D265536A5E", "Default language of '{0}' has been specified.", CountryName));
			}
		}

		CountryDefaultLanguageBusinessObjectCollection ParentCollection
		{
			get { return (CountryDefaultLanguageBusinessObjectCollection)GetParentCollection(this, typeof(CountryDefaultLanguageBusinessObjectCollection)); }
		}

		public IBusinessObjectCollection CountryCollection => countryCollection ?? (countryCollection = (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<IRefCountryCollection>(), CurrentFactory));
		IBusinessObjectCollection countryCollection;

		ZGuid countryPk;

		#endregion

		#region CountryName

		public ZString CountryName
		{
			get
			{
				var countryName = string.Empty;
				if (countryPk.IsValid)
				{
					var country = CurrentFactory.Load<IRefCountry>(countryPk);
					countryName = country?.RN_Desc ?? ZString.Empty;
				}

				return countryName;
			}
		}

		#endregion

		#region DefaultLanguage

		[List("Languages")]
		public ZString DefaultLanguage
		{
			get => defaultLanguage;
			set
			{
				SetNonPersistentPropertyValue<ZString>(DefaultLanguageInfo, ref defaultLanguage, value);
				if (!IsValidationSuspended)
				{
					ValidateDefaultLanguage();
				}
			}
		}

		public ZPropertyInfo DefaultLanguageInfo => GetZPropertyInfo(Schema.DefaultLanguage);

		public void ValidateDefaultLanguage()
		{
			DefaultLanguageInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(DefaultLanguageInfo);
			ListValidation.ErrorIfInvalidCode(DefaultLanguageInfo, Languages);
		}

		ZString defaultLanguage;

		public CodeDescriptionPairList Languages => languages ?? (languages = new CodeDescriptionPairList(OLookUpEditType.Language));
		CodeDescriptionPairList languages;

		#endregion

		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateCountryPk();
			ValidateDefaultLanguage();
		}

		#region XML Reading and Writing

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			CountryPk = new Guid(reader.ReadElementString(Schema.CountryPk));
			DefaultLanguage = reader.ReadElementString(Schema.DefaultLanguage);
		}

		protected sealed override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.CountryPk, CountryPk.ToString());
			writer.WriteElementString(Schema.DefaultLanguage, DefaultLanguage);
		}

		#endregion
	}
}
