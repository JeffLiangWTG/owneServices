using System.Linq;
using System.Xml;
using System.Xml.Serialization;

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ClientInTemplateSelectionCriteria : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string ProcessTaskCode = "ProcessTaskCode";
			public const string ProcessTaskDescription = "ProcessTaskDescription";
		}

		#endregion

		public ClientInTemplateSelectionCriteria()
		{
		}

		public ClientInTemplateSelectionCriteria(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ClientInTemplateSelectionCriteria(fallbackLevel, factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			ClientInTemplateSelectionCriteria clientInTemplateSelectionCriteriaClone = (ClientInTemplateSelectionCriteria)clone;
			clientInTemplateSelectionCriteriaClone.CloneValuesFrom(clientInTemplateSelectionCriteriaClone, SelectedItems);
		}

		void CloneValuesFrom(ClientInTemplateSelectionCriteria parent, ClientInTemplateSelectionCriteriaOrgTypesCollection existingSelectedItems)
		{
			if (existingSelectedItems != null)
			{
				selectedItems = (ClientInTemplateSelectionCriteriaOrgTypesCollection)existingSelectedItems.Clone(CurrentFallbackLevel, CurrentFactory);
				selectedItems.Parent = parent;
			}

			RegisterEditableChildObject(selectedItems);

			foreach (var orgType in ClientInTemplateSelectionCriteriaCollectionRegistryItem.GetDefaultOrgTypesByCode(parent.ProcessTaskCode).Cast<CodeDescriptionBool>().Where(x => !selectedItems.ContainsCode(x.Code)))
			{
				AvailableItems.Add(new ClientInTemplateSelectionCriteriaOrgType(orgType.Code));
			}
		}

		ZXmlSerializer ClientInTemplateSelectionCriteriaOrgTypesCollectionSerialiser
		{
			get
			{
				if (clientInTemplateSelectionCriteriaOrgTypesCollectionSerialiser == null)
				{
					clientInTemplateSelectionCriteriaOrgTypesCollectionSerialiser = ZXmlSerializer.New(typeof(ClientInTemplateSelectionCriteriaOrgTypesCollection));
				}

				return clientInTemplateSelectionCriteriaOrgTypesCollectionSerialiser;
			}
		}
		ZXmlSerializer clientInTemplateSelectionCriteriaOrgTypesCollectionSerialiser;

		#region Process Type Code

		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString ProcessTaskCode
		{
			get { return processTaskCode; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(ProcessTaskCodeInfo, ref processTaskCode, value);
			}
		}

		public ZPropertyInfo ProcessTaskCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ProcessTaskCode); }
		}

		ZString processTaskCode;

		#endregion

		#region Process Type Description

		public MultilingualString ProcessTaskDescription
		{
			get
			{
				if (processTaskDescription == null)
				{
					processTaskDescription = ClientInTemplateSelectionCriteriaCollectionRegistryItem.DefaultProcessTaskList.GetMultilingualDescriptionFromCode(ProcessTaskCode) ?? (NoResString)"";
				}

				return processTaskDescription;
			}
		}
		MultilingualString processTaskDescription;

		public ZPropertyInfo ProcessTaskDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ProcessTaskDescription); }
		}

		#endregion

		#region ClientInTemplateSelectionCriteriaOrgTypes Collections

		public ClientInTemplateSelectionCriteriaOrgTypesCollection SelectedItems
		{
			get
			{
				if (selectedItems == null)
				{
					selectedItems = new ClientInTemplateSelectionCriteriaOrgTypesCollection(this, CurrentFallbackLevel, CurrentFactory);
					RegisterEditableChildObject(selectedItems);
				}

				return selectedItems;
			}
		}
		ClientInTemplateSelectionCriteriaOrgTypesCollection selectedItems;

		public ClientInTemplateSelectionCriteriaOrgTypesCollection AvailableItems
		{
			get
			{
				if (availableItems == null)
				{
					availableItems = new ClientInTemplateSelectionCriteriaOrgTypesCollection(this, CurrentFallbackLevel, Factory);
					RegisterEditableChildObject(availableItems);
				}

				return availableItems;
			}
		}
		ClientInTemplateSelectionCriteriaOrgTypesCollection availableItems;

		#endregion

		#region Reset

		public void Reset()
		{
			var defaultList = GetDefaultValueByCode(ProcessTaskCode);

			if (defaultList != null)
			{
				SelectedItems.RemoveAll();
				foreach (ClientInTemplateSelectionCriteriaOrgType item in defaultList.SelectedItems)
				{
					ClientInTemplateSelectionCriteriaOrgType itemToAdd = (ClientInTemplateSelectionCriteriaOrgType)item.Clone(CurrentFallbackLevel, CurrentFactory);
					SelectedItems.Add(itemToAdd);
				}

				AvailableItems.RemoveAll();
				foreach (ClientInTemplateSelectionCriteriaOrgType item in defaultList.AvailableItems)
				{
					ClientInTemplateSelectionCriteriaOrgType itemToAdd = (ClientInTemplateSelectionCriteriaOrgType)item.Clone(CurrentFallbackLevel, CurrentFactory);
					AvailableItems.Add(itemToAdd);
				}
			}
		}

		public static ClientInTemplateSelectionCriteria GetDefaultValueByCode(string processTaskCode)
		{
			return ClientInTemplateSelectionCriteriaCollectionRegistryItem.DefaultCollectionValue.Cast<ClientInTemplateSelectionCriteria>().FirstOrDefault(x => x.ProcessTaskCode == processTaskCode);
		}

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ProcessTaskCode, ProcessTaskCode);

			ClientInTemplateSelectionCriteriaOrgTypesCollectionSerialiser.Serialize(writer, SelectedItems);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ProcessTaskCode = reader.ReadElementString(Schema.ProcessTaskCode);

			ClientInTemplateSelectionCriteriaOrgTypesCollection selectedOrgTypes = (ClientInTemplateSelectionCriteriaOrgTypesCollection)ClientInTemplateSelectionCriteriaOrgTypesCollectionSerialiser.Deserialize(reader);
			CloneValuesFrom(this, selectedOrgTypes);
		}

		#endregion
	}
}
