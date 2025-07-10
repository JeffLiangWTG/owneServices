using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CustomNoteTypes : RegistryBusinessObjectTemplate
	{
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			CustomNoteTypes result = new CustomNoteTypes();
			result.noteModuleAndCountryList = (CustomNoteModuleAndCountryCollection)NoteModuleAndCountryList.Clone(fallbackLevel, factory);
			result.RegisterEditableChildObject(result.noteModuleAndCountryList);
			return result;
		}

		#region Get Note Types For Module

		public NoteTypeCollection GetNoteTypesForModuleAndThisCountry(string moduleIDName)
		{
			NoteTypeCollection result = new NoteTypeCollection();
			CustomNoteModuleAndCountry moduleForAll = NoteModuleAndCountryList.FindOrCreateEntryFromCode(moduleIDName, CustomNoteModuleAndCountry.ModuleAndCountryCodes.CountryALL, ZBool.False);
			CustomNoteModuleAndCountry moduleForCurrentCountry = NoteModuleAndCountryList.FindOrCreateEntryFromCode(moduleIDName, Env.CurrentCompany.Country.Code, ZBool.False);

			if (moduleForAll != null)
			{
				result.Add(moduleForAll.CustomNoteTypesList.ToNoteTypeCollection());
			}

			if (moduleForCurrentCountry != null)
			{
				result.Add(moduleForCurrentCountry.CustomNoteTypesList.ToNoteTypeCollection());
			}

			return result;
		}

		public NoteTypeCollection GetNoteTypesForModuleAndAllCountries(string moduleIDName)
		{
			NoteTypeCollection result = new NoteTypeCollection();
			List<CustomNoteModuleAndCountry> modulesForAllCountries = NoteModuleAndCountryList.FindModulesForAllCountries(moduleIDName);

			foreach (CustomNoteModuleAndCountry module in modulesForAllCountries)
			{
				result.Add(module.CustomNoteTypesList.ToNoteTypeCollection());
			}

			return result;
		}

		#endregion

		#region All Custom Note Types

		public NoteTypeCollection AllCustomNoteTypes
		{
			get
			{
				NoteTypeCollection result = new NoteTypeCollection();

				foreach (CustomNoteModuleAndCountry module in NoteModuleAndCountryList)
				{
					result.Add(module.CustomNoteTypesList.ToNoteTypeCollection());
				}

				return result;
			}
		}

		#endregion

		#region Note Type Exists

		public bool NoteTypeExistsByName(string noteName)
		{
			bool result = false;

			foreach (CustomNoteModuleAndCountry module in NoteModuleAndCountryList)
			{
				foreach (CustomNoteTypeItem item in module.CustomNoteTypesList)
				{
					if (noteName == item.NoteName)
					{
						result = true;
						break;
					}
				}
			}

			return result;
		}

		#endregion

		#region Selected Module ID

		public ZString CurrentSelectedModuleID
		{
			get { return currentSelectedModuleID; }
			set
			{
				currentSelectedModuleID = value;
				RegenerateSelectedModule();
			}
		}
		ZString currentSelectedModuleID;

		#endregion

		#region Selected Country Code

		public ZString CurrentSelectedCountryCode
		{
			get { return currentSelectedCountryCode; }
			set
			{
				currentSelectedCountryCode = value;
				RegenerateSelectedModule();
			}
		}
		ZString currentSelectedCountryCode;

		#endregion

		#region Selected Module

		public CustomNoteModuleAndCountry SelectedModule
		{
			get { return SelectedModuleCollectionForBinding.Count > 0 ? SelectedModuleCollectionForBinding[0] : SelectedModuleCollectionForBinding.AddNew(); }
			set
			{
				SelectedModuleCollectionForBinding.Remove(SelectedModuleCollectionForBinding[0]);
				SelectedModuleCollectionForBinding.Add(value);
			}
		}

		public CustomNoteModuleAndCountryCollection SelectedModuleCollectionForBinding
		{
			get
			{
				if (selectedModuleCollectionForBinding == null)
				{
					selectedModuleCollectionForBinding = new CustomNoteModuleAndCountryCollection(this);
				}

				return selectedModuleCollectionForBinding;
			}
		}
		CustomNoteModuleAndCountryCollection selectedModuleCollectionForBinding;

		#endregion

		#region Regenerating Modules

		void RegenerateSelectedModule()
		{
			if (CurrentSelectedModuleID != ZString.Empty && CurrentSelectedCountryCode != ZString.Empty)
			{
				SelectedModule = NoteModuleAndCountryList.FindOrCreateEntryFromCode(CurrentSelectedModuleID, CurrentSelectedCountryCode, ZBool.True);
			}
			else
			{
				SelectedModule = EmptyModule;
			}

			SelectedModuleCollectionForBinding.RefreshBinding();
		}

		CustomNoteModuleAndCountry EmptyModule
		{
			get
			{
				if (emptyModule == null)
				{
					emptyModule = new CustomNoteModuleAndCountry();
				}

				return emptyModule;
			}
		}
		CustomNoteModuleAndCountry emptyModule;

		#endregion

		#region Note Module And Country List

		public CustomNoteModuleAndCountryCollection NoteModuleAndCountryList
		{
			get
			{
				if (noteModuleAndCountryList == null)
				{
					noteModuleAndCountryList = new CustomNoteModuleAndCountryCollection(this);
					RegisterEditableChildObject(noteModuleAndCountryList);
				}

				return noteModuleAndCountryList;
			}
		}
		CustomNoteModuleAndCountryCollection noteModuleAndCountryList;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(CustomNoteModuleAndCountryCollection));
			serializer.Serialize(writer, NoteModuleAndCountryList);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ZXmlSerializer serializer = ZXmlSerializer.New(typeof(CustomNoteModuleAndCountryCollection));
			noteModuleAndCountryList = (CustomNoteModuleAndCountryCollection)serializer.Deserialize(reader);
			noteModuleAndCountryList.NoteTypeParent = this;
		}

		#endregion
	}
}
