using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CustomNoteModuleAndCountry : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string ModuleIDName = "ModuleIDName";
			public const string CountryCode = "CountryCode";
		}

		#endregion

		public static class ModuleAndCountryCodes
		{
			public const string CountryALL = "ALL";
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			CustomNoteModuleAndCountry result = new CustomNoteModuleAndCountry();
			result.customNoteTypesList = (CustomNoteTypeItemCollection)CustomNoteTypesList.Clone(fallbackLevel, factory);
			result.RegisterEditableChildObject(result.customNoteTypesList);
			return result;
		}

		#region Note Type List

		public CustomNoteTypeItemCollection CustomNoteTypesList
		{
			get
			{
				if (customNoteTypesList == null)
				{
					customNoteTypesList = new CustomNoteTypeItemCollection(this);
					RegisterEditableChildObject(customNoteTypesList);
				}

				return customNoteTypesList;
			}
		}
		CustomNoteTypeItemCollection customNoteTypesList;

		#endregion

		#region NoteTypeParent

		internal CustomNoteTypes NoteTypeParent
		{
			get { return ParentCollections.Count > 0 ? ((CustomNoteModuleAndCountryCollection)ParentCollections.First()).NoteTypeParent : null; }
		}

		#endregion

		#region Bound Properties

		#region ModuleIDName

		public ZString ModuleIDName
		{
			get { return moduleIDName; }
			set
			{
				CheckMaximumLength(ModuleIDNameInfo, value);
				SetNonPersistentPropertyValue<ZString>(ModuleIDNameInfo, ref moduleIDName, value);
			}
		}
		ZString moduleIDName;

		public ZPropertyInfo ModuleIDNameInfo
		{
			get { return GetZPropertyInfo(Schema.ModuleIDName); }
		}

		#endregion

		#region CountryCode

		public ZString CountryCode
		{
			get { return countryCode; }
			set
			{
				CheckMaximumLength(CountryCodeInfo, value);
				SetNonPersistentPropertyValue<ZString>(CountryCodeInfo, ref countryCode, value);
			}
		}
		ZString countryCode;

		public ZPropertyInfo CountryCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CountryCode); }
		}

		#endregion

		#endregion

		#region Lookups

		public IBusinessObjectCollection CountriesOfAllCompanies
		{
			get
			{
				if (countriesOfAllCompanies == null)
				{
					ZDBOnlyQuery countryQuery = new ZDBOnlyQuery(typeof(IRefCountry));
					ZDBOnlySubQuery companyQuery = new ZDBOnlySubQuery(typeof(IGlbCompany), GlbCompanySchema.GC_RN_NKCountryCode);
					countryQuery.AddSubQuery(RefCountrySchema.RN_Code, companyQuery, JoinCondition.And);
					countriesOfAllCompanies = (IBusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IRefCountryCollection>(), new BusinessObjectFactory(), countryQuery);
					countriesOfAllCompanies.ApplySort(new SortInfo(RefCountrySchema.RN_Desc.Name, System.ComponentModel.ListSortDirection.Ascending));
				}
				return countriesOfAllCompanies;
			}
		}
		IBusinessObjectCollection countriesOfAllCompanies;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ModuleIDName, ModuleIDName);
			writer.WriteElementString(Schema.CountryCode, CountryCode);
			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(CustomNoteTypeItemCollection));
			serializer.Serialize(writer, CustomNoteTypesList);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ModuleIDName = reader.ReadElementString(Schema.ModuleIDName);
			CountryCode = reader.ReadElementString(Schema.CountryCode);
			customNoteTypesList = new CustomNoteTypeItemCollection(this);
			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(CustomNoteTypeItemCollection));
			customNoteTypesList = (CustomNoteTypeItemCollection)serializer.Deserialize(reader);
			customNoteTypesList.ModuleAndCountry = this;
		}

		#endregion
	}

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[XmlRoot("NoteModuleAndCountryList")]
	public class CustomNoteModuleAndCountryCollection : RegistryBusinessObjectCollectionTemplate
	{
		public CustomNoteModuleAndCountryCollection()
		{
		}

		public CustomNoteModuleAndCountryCollection(CustomNoteTypes noteTypeParent)
		{
			this.NoteTypeParent = noteTypeParent;
		}

		public CustomNoteTypes NoteTypeParent
		{
			get { return noteTypeParent; }
			set { noteTypeParent = value; }
		}
		CustomNoteTypes noteTypeParent;

		public new CustomNoteModuleAndCountry this[int i]
		{
			get { return (CustomNoteModuleAndCountry)Elements[i]; }
		}

		public new CustomNoteModuleAndCountry AddNew()
		{
			return (CustomNoteModuleAndCountry)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CustomNoteModuleAndCountryCollection(NoteTypeParent);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CustomNoteModuleAndCountry();
		}

		public CustomNoteModuleAndCountry FindOrCreateEntryFromCode(ZString moduleIDName, ZString countryCode, ZBool createIfNotExists)
		{
			CustomNoteModuleAndCountry result = null;
			if (moduleIDName != ZString.Empty && countryCode != ZString.Empty)
			{
				foreach (CustomNoteModuleAndCountry item in this)
				{
					if (item.ModuleIDName == moduleIDName && item.CountryCode == countryCode)
					{
						result = item;
						break;
					}
				}
			}

			if (result == null && createIfNotExists)
			{
				result = AddNew();
				result.ModuleIDName = moduleIDName;
				result.CountryCode = countryCode;
			}

			return result;
		}

		public List<CustomNoteModuleAndCountry> FindModulesForAllCountries(ZString moduleIDName)
		{
			List<CustomNoteModuleAndCountry> result = new List<CustomNoteModuleAndCountry>();
			if (moduleIDName != ZString.Empty)
			{
				foreach (CustomNoteModuleAndCountry item in this)
				{
					if (item.ModuleIDName == moduleIDName)
					{
						result.Add(item);
					}
				}
			}

			return result;
		}
	}
}
