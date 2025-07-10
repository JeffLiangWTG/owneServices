using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.JAS.Registry.Business
{
	public enum CognosModes
	{
		Empty,
		AI,
		AE,
		MI,
		ME,
		CHB,
		NV,
		WPT,
		PR,
		OTH
	}

	[XmlSerializerAssembly("ZClientJAS.XmlSerializers")]
	public class CognosModeMapping : RegistryBusinessObjectTemplate
	{
		#region Schema

		public static class Schema
		{
			public const string SelectedMode = "SelectedMode";
			public const string IsValidModeSelected = "IsValidModeSelected";
		}

		#endregion

		#region Cognos Mode

		[CargoWise.ComponentModel.MaxLength(4)]
		public ZString SelectedMode
		{
			get { return fSelectedMode; }
			set
			{
				if (fSelectedMode != value)
				{
					CheckMaximumLength(SelectedModeInfo, value);
					SetNonPersistentPropertyValue(SelectedModeInfo, ref fSelectedMode, value);
					if (!IsValidationSuspended)
					{
						Validation.ValidateSelectedMode();
					}
					if (mappedDepartments != null)
					{
						mappedDepartments.SelectedModeAsEnum = SelectedModeAsEnum;
					}
					RefreshBinding();
				}
			}
		}

		public ZPropertyInfo SelectedModeInfo
		{
			get { return GetZPropertyInfo(nameof(SelectedMode)); }
		}

		public CodeDescriptionPairList Modes
		{
			get
			{
				if (fModes == null)
				{
					fModes = new CodeDescriptionPairList();
					fModes.AddPair(nameof(CognosModes.AI), "Air Import");
					fModes.AddPair(nameof(CognosModes.AE), "Air Export");
					fModes.AddPair(nameof(CognosModes.MI), "Maritime Import");
					fModes.AddPair(nameof(CognosModes.ME), "Maritime Export");
					fModes.AddPair(nameof(CognosModes.CHB), "Customs House Brokerage");
					fModes.AddPair(nameof(CognosModes.NV), "Maritime Export NVOCC");
					fModes.AddPair(nameof(CognosModes.WPT), "Warehouse/Packing/Trucking");
					fModes.AddPair(nameof(CognosModes.PR), "Projects");
					fModes.AddPair(nameof(CognosModes.OTH), "Other");
				}
				return fModes;
			}
		}

		public ZBool IsValidModeSelected
		{
			get { return SelectedModeAsEnum != CognosModes.Empty; }
		}

		public ZPropertyInfo IsValidModeSelectedInfo
		{
			get { return GetZPropertyInfo(nameof(IsValidModeSelected)); }
		}

		CognosModes SelectedModeAsEnum
		{
			get { return ConvertToModeEnum(SelectedMode); }
		}

		internal CognosModes ConvertToModeEnum(ZString mode)
		{
			CognosModes result = CognosModes.Empty;

			if (!mode.IsEmpty)
			{
				try
				{
					result = (CognosModes)Enum.Parse(typeof(CognosModes), mode);
				}
				catch (ArgumentException)
				{
				}
			}

			return result;
		}

		ZString fSelectedMode;
		CodeDescriptionPairList fModes;

		#endregion

		#region Departments

		public GlbDepartmentCollection MappedDepartments
		{
			get
			{
				return
					mappedDepartments ??
						(mappedDepartments = new MappedCognosGlbDepartmentCollection(DepartmentFactory, MappedDepartmentDictionary) { SelectedModeAsEnum = this.SelectedModeAsEnum });
			}
		}
		MappedCognosGlbDepartmentCollection mappedDepartments;

		public GlbDepartmentCollection AvailableDepartments
		{
			get { return availableDepartments ?? (availableDepartments = new AvailableCognosGlbDepartmentCollection(DepartmentFactory, MappedDepartmentDictionary)); }
		}
		GlbDepartmentCollection availableDepartments;

		BusinessObjectFactory DepartmentFactory
		{
			get
			{
				return departmentFactory != null && departmentFactory.IsOwnedByCurrentThread
					? departmentFactory
					: (departmentFactory = new BusinessObjectFactory());
			}
		}
		BusinessObjectFactory departmentFactory;

		public class MappedCognosGlbDepartmentCollection : GlbDepartmentCollection
		{
			[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
			public MappedCognosGlbDepartmentCollection(BusinessObjectFactory factory, Dictionary<CognosModes, List<ZGuid>> mappedDepartmentDictionary)
				: base(factory)
			{
				this.mappedDepartmentDictionary = mappedDepartmentDictionary;
			}

			protected override bool MatchesFilterCore(GlbDepartment element, bool fetchOnlyFromLocalCache)
			{
				List<ZGuid> mappedDepartmentPKs;
				if (mappedDepartmentDictionary.TryGetValue(SelectedModeAsEnum, out mappedDepartmentPKs))
				{
					return mappedDepartmentPKs.Contains(element.PK);
				}
				return false;
			}

			public CognosModes SelectedModeAsEnum
			{
				get { return selectedModeAsEnum; }
				set
				{
					selectedModeAsEnum = value;
					(this as IActiveBusinessObjectCollection).Refresh();
				}
			}
			CognosModes selectedModeAsEnum;

			protected override object[] GetCollectionState()
			{
				return new object[] { mappedDepartmentDictionary, SelectedModeAsEnum };
			}

			readonly Dictionary<CognosModes, List<ZGuid>> mappedDepartmentDictionary;

			protected override bool AllowNew => false;
		}

				public
				class AvailableCognosGlbDepartmentCollection : GlbDepartmentCollection
		{
			[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
			public AvailableCognosGlbDepartmentCollection(BusinessObjectFactory factory, Dictionary<CognosModes, List<ZGuid>> mappedDepartmentDictionary)
				: base(factory)
			{
				this.mappedDepartmentDictionary = mappedDepartmentDictionary;
			}

			protected override bool MatchesFilterCore(GlbDepartment element, bool fetchOnlyFromLocalCache)
			{
				foreach (List<ZGuid> mappedDeptPKs in mappedDepartmentDictionary.Values)
				{
					if (mappedDeptPKs.Contains(element.PK))
					{
						return false;
					}
				}
				return true;
			}

			protected override object[] GetCollectionState()
			{
				return new object[] { mappedDepartmentDictionary };
			}

			readonly Dictionary<CognosModes, List<ZGuid>> mappedDepartmentDictionary;
		}

		public virtual void MapDepartments(params GlbDepartment[] departments)
		{
			if (SelectedModeAsEnum != CognosModes.Empty)
			{
				foreach (GlbDepartment department in departments)
				{
					if (!((IBusinessObjectCollection)AvailableDepartments).Contains(department.PK))
					{
						throw new InvalidOperationException("Should not add a department which have already been mapped");
					}
				}

				List<ZGuid> mappedDepartmentPKs = GetOrCreateNewDepartmentPKList(SelectedModeAsEnum);
				AddUniquePKs(mappedDepartmentPKs, departments);
				((IActiveBusinessObjectCollection)AvailableDepartments).Refresh();
				((IActiveBusinessObjectCollection)MappedDepartments).Refresh();
				RefreshBinding();
			}
		}

		public virtual void UnmapDepartments(params GlbDepartment[] departments)
		{
			if (SelectedModeAsEnum != CognosModes.Empty)
			{
				List<ZGuid> mappedDepartmentPKs;
				if (MappedDepartmentDictionary.TryGetValue(SelectedModeAsEnum, out mappedDepartmentPKs))
				{
					RemovePKs(mappedDepartmentPKs, departments);
					if (mappedDepartmentPKs.Count == 0)
					{
						MappedDepartmentDictionary.Remove(SelectedModeAsEnum);
					}
				}
				((IActiveBusinessObjectCollection)AvailableDepartments).Refresh();
				((IActiveBusinessObjectCollection)MappedDepartments).Refresh();
			}
		}

		internal GlbDepartmentCollection AllDepartments
		{
			get
			{
				if (fAllDepartments == null)
				{
					fAllDepartments = new GlbDepartmentCollection(FactoryForDepartments);
				}
				return fAllDepartments;
			}
		}

		internal BusinessObjectFactory FactoryForDepartments
		{
			get
			{
				if (fFactoryForDepartments == null)
				{
					fFactoryForDepartments = new BusinessObjectFactory();
				}
				return fFactoryForDepartments;
			}
		}

		internal List<ZGuid> GetOrCreateNewDepartmentPKList(CognosModes mode)
		{
			List<ZGuid> result;
			if (!MappedDepartmentDictionary.TryGetValue(mode, out result))
			{
				result = new List<ZGuid>();
				MappedDepartmentDictionary.Add(mode, result);
			}
			return result;
		}

		internal void AddUniquePKs(List<ZGuid> pKList, GlbDepartment[] departments)
		{
			foreach (GlbDepartment department in departments)
			{
				if (!pKList.Contains(department.PK))
				{
					pKList.Add(department.PK);
				}
			}
		}

		internal void RemovePKs(List<ZGuid> pKList, GlbDepartment[] departments)
		{
			foreach (GlbDepartment department in departments)
			{
				if (pKList.Contains(department.PK))
				{
					pKList.Remove(department.PK);
				}
			}
		}

		internal Dictionary<CognosModes, List<ZGuid>> MappedDepartmentDictionary
		{
			get
			{
				if (fMappedDepartmentDictionary == null)
				{
					fMappedDepartmentDictionary = new Dictionary<CognosModes, List<ZGuid>>();
				}
				return fMappedDepartmentDictionary;
			}
		}

		internal BusinessObjectFactory fFactoryForDepartments;
		internal GlbDepartmentCollection fAllDepartments;
		internal Dictionary<CognosModes, List<ZGuid>> fMappedDepartmentDictionary;

		#endregion

		#region Xml Serialisation

		static class CognosModeXmlConstants
		{
			public const string Modes = "MODES";
			public const string Mappings = "MAPPINGS";
			public const string Mode = "MODE";
			public const string Mapping = "MAPPING";
			public const string Name = "NAME";
			public const string SelectedMode = "SELECTEDMODE";
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(CognosModeXmlConstants.SelectedMode, SelectedMode);
			foreach (CognosModes mode in Enum.GetValues(typeof(CognosModes)))
			{
				if (mode != CognosModes.Empty)
				{
					WriteModeElement(writer, mode);
				}
			}
		}

		protected override void ReadElements(XmlReaderWrapper wrapper)
		{
			SelectedMode = wrapper.ReadElementString(CognosModeXmlConstants.SelectedMode);
			XmlReader reader = wrapper.Reader;

			if (reader.Name == CognosModeXmlConstants.Mode)
			{
				do
				{
					ReadModeElement(reader);
				} while (reader.ReadToNextSibling(CognosModeXmlConstants.Mode));
			}
		}

		void WriteModeElement(XmlWriter writer, CognosModes mode)
		{
			List<ZGuid> mappedDepartmentPKs;
			if (MappedDepartmentDictionary.TryGetValue(mode, out mappedDepartmentPKs) && mappedDepartmentPKs.Count > 0)
			{
				writer.WriteStartElement(CognosModeXmlConstants.Mode);                                  // <MODE>
				writer.WriteAttributeString(CognosModeXmlConstants.Name, mode.ToString());
				WriteDepartmentPKsElements(writer, mappedDepartmentPKs);
				writer.WriteEndElement();                                                               // </MODE>
			}
		}

		void WriteDepartmentPKsElements(XmlWriter writer, List<ZGuid> mappedDepartmentPKs)
		{
			foreach (ZGuid deptPK in mappedDepartmentPKs)
			{
				writer.WriteElementString(CognosModeXmlConstants.Mapping, deptPK.ToString());       // <MAPPING/>
			}
		}

		void ReadModeElement(XmlReader reader)
		{
			string mode = reader.GetAttribute(CognosModeXmlConstants.Name);
			CognosModes cognosMode = ConvertToModeEnum(mode);
			if (cognosMode != CognosModes.Empty)
			{
				ReadDepartmentPKsElements(reader, cognosMode);
			}
		}

		void ReadDepartmentPKsElements(XmlReader reader, CognosModes cognosMode)
		{
			List<ZGuid> mappedDepartmentPKs = new List<ZGuid>();
			if (reader.ReadToDescendant(CognosModeXmlConstants.Mapping))
			{
				do
				{
					mappedDepartmentPKs.Add(new ZGuid(reader.ReadString()));
				} while (reader.ReadToNextSibling(CognosModeXmlConstants.Mapping));
			}
			MappedDepartmentDictionary.Add(cognosMode, mappedDepartmentPKs);
		}

		#endregion

		#region Overrides

		public override bool Equals(object obj)
		{
			bool result = false;

			CognosModeMapping otherMapping = obj as CognosModeMapping;
			if (otherMapping != null)
			{
				result = CompareMappedDepartmentDictionaries(MappedDepartmentDictionary, otherMapping.MappedDepartmentDictionary);
			}

			return result;
		}

		public override int GetHashCode()
		{
			return MappedDepartmentDictionary.Count;
		}

		bool CompareMappedDepartmentDictionaries(Dictionary<CognosModes, List<ZGuid>> lhs, Dictionary<CognosModes, List<ZGuid>> rhs)
		{
			if (lhs.Count != rhs.Count)
			{
				return false;
			}

			foreach (KeyValuePair<CognosModes, List<ZGuid>> keyValuePair in lhs)
			{
				List<ZGuid> rhsList;
				if (!rhs.TryGetValue(keyValuePair.Key, out rhsList))
				{
					return false;
				}

				List<ZGuid> lhsList = keyValuePair.Value;
				if (lhsList.Count != rhsList.Count)
				{
					return false;
				}

				lhsList.Sort();
				rhsList.Sort();
				for (int i = 0; i < lhsList.Count; i++)
				{
					if (lhsList[i] != rhsList[i])
					{
						return false;
					}
				}
			}

			return true;
		}

		#endregion

		public ZGuid[] GetDepartmentPKs(ZString mode)
		{
			return GetDepartmentPKs(ConvertToModeEnum(mode));
		}

		public ZGuid[] GetDepartmentPKs(CognosModes mode)
		{
			List<ZGuid> list;
			MappedDepartmentDictionary.TryGetValue(mode, out list);
			return (list != null) ? list.ToArray() : Array.Empty<ZGuid>();
		}

		public string GetCognosMode(GlbDepartment department)
		{
			if (department == null)
			{
				throw new ArgumentNullException(nameof(department));
			}

			return GetCognosMode(department.PK);
		}

		public string GetCognosMode(ZGuid departmentPK)
		{
			foreach (KeyValuePair<CognosModes, List<ZGuid>> keyValuePair in MappedDepartmentDictionary)
			{
				if (keyValuePair.Value.Contains(departmentPK))
				{
					return keyValuePair.Key.ToString();
				}
			}

			return "";
		}

		public CognosModeMappingValidation Validation
		{
			get { return new CognosModeMappingValidation(this); }
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			CognosModeMapping result = new CognosModeMapping();

			foreach (KeyValuePair<CognosModes, List<ZGuid>> keyValuePair in MappedDepartmentDictionary)
			{
				List<ZGuid> pKList = new List<ZGuid>();
				pKList.InsertRange(0, keyValuePair.Value);
				result.MappedDepartmentDictionary.Add(keyValuePair.Key, pKList);
			}

			return result;
		}

#region Mode
#endregion
#region Departments
#endregion
#region Equals
#endregion
#region Implementation
#endregion
			}
}
