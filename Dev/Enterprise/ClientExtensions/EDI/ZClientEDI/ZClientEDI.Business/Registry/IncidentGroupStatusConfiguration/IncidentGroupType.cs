using System.Xml;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class IncidentGroupType : AutoIncidentGroupType
	{
		public IncidentGroupType() : base()
		{
		}
		public IncidentGroupType(BusinessObjectFactory factory) : base(factory)
		{
		}
		public IncidentGroupType(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}
		public IncidentGroupType(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		public ZString OriginalGroupType { get; private set; } = string.Empty;

		public override bool ReadOnly { get => IsSystem; }

		public IncidentGroupStatusConfigurationCollection IncidentGroupStatusConfigurations
		{
			get
			{
				if (incidentGroupStatusConfigurations == null)
				{
					incidentGroupStatusConfigurations = new IncidentGroupStatusConfigurationCollection();
					RegisterEditableChildObject(incidentGroupStatusConfigurations);
				}
				return incidentGroupStatusConfigurations;
			}

			private set
			{
				UnRegisterEditableChildObject(incidentGroupStatusConfigurations);
				incidentGroupStatusConfigurations = value;
				RegisterEditableChildObject(incidentGroupStatusConfigurations);
			}
		}
		IncidentGroupStatusConfigurationCollection incidentGroupStatusConfigurations;

		public void ShowChildrenNotification()
		{
			foreach (var item in IncidentGroupStatusConfigurations)
			{
				item.ShowNotification();
			}
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			IsSystem = false;
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var cloneGroupType = (IncidentGroupType)clone;
			if (!string.IsNullOrEmpty(GroupType))
			{
				cloneGroupType.OriginalGroupType = GroupType;
			}
			cloneGroupType.IncidentGroupStatusConfigurations = (IncidentGroupStatusConfigurationCollection)IncidentGroupStatusConfigurations.Clone(cloneGroupType.CurrentFallbackLevel, cloneGroupType.Factory);
		}

		public override void ValidateGroupType()
		{
			base.ValidateGroupType();
			MandatoryValidation.CheckEntered(GroupTypeInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(GroupTypeInfo);
			if (GroupType.Length != Schema.GroupTypeMaxLength)
			{
				GroupTypeInfo.AddError(ResString.GetMultilingualString("94d34622-98e9-4fbd-9f40-fcac1aa6065b", "The group type should have {0} characters", Schema.GroupTypeMaxLength));
			}
			if (GroupType != OriginalGroupType && IsGroupTypeInUse)
			{
				GroupTypeInfo.AddError(ResString.GetMultilingualString("de17f6a5-cb96-4645-983e-debb81b785f6", "This code cannot be changed as it is in use by at least one record in the system."));
			}
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = new IncidentGroupType(fallbackLevel, factory);
			clone.GroupType = this.GroupType;
			clone.Description = this.Description;
			clone.IsSystem = this.IsSystem;
			return clone;
		}

		#region Delete
		public override bool CanDelete => !IsSystem && !IsGroupTypeInUse;

		public bool IsGroupTypeInUse
		{
			get
			{
				if (!string.IsNullOrEmpty(OriginalGroupType))
				{
					var factory = Factory ?? new BusinessObjectFactory();
					var query = new ZQuery(IncidentManagementGroupSchema.ING_Type, OriginalGroupType);
					return factory.Exists(typeof(IncidentManagementGroup), query);
				}
				return false;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (IsSystem)
				{
					return ResString.GetMultilingualString("d50ae001-132e-43fe-a708-fd927a453997", "System-defined group type(s) cannot be deleted.");
				}
				return ResString.GetMultilingualString("d73761fc-a429-4aea-b06b-f3bb3c5ae660", $"The type {this.GroupType} cannot be deleted as it is the active type for existing Incident Management Groups.");
			}
		}
		#endregion

		#region XML Serialisation
		protected override void WriteConfigurations(XmlWriter writer)
		{
			IncidentGroupStatusConfigurations.Sort(IncidentGroupStatusConfiguration.Schema.Sequence);
			IncidentGroupStatusConfigurationsSerializer.Serialize(writer, IncidentGroupStatusConfigurations);
			IncidentGroupStatusConfigurations.RemovedBusinessObjList.Clear();
			ResetOriginalValue();
		}

		void ResetOriginalValue()
		{
			if (this.OriginalGroupType != GroupType)
			{
				this.SetOriginalGroupType(GroupType);
			}
			foreach (var item in IncidentGroupStatusConfigurations)
			{
				if (item.OriginalCode != item.Code)
				{
					item.SetOriginalCode(item.Code);
				}
			}
		}

		protected override void ReadConfigurations(XmlReader reader)
		{
			reader.ReadStartElement();
			IncidentGroupStatusConfigurations = (IncidentGroupStatusConfigurationCollection)IncidentGroupStatusConfigurationsSerializer.Deserialize(reader);
			reader.ReadEndElement();
			IncidentGroupStatusConfigurations.Sort(IncidentGroupStatusConfiguration.Schema.Sequence);
		}

		ZXmlSerializer IncidentGroupStatusConfigurationsSerializer
		{
			get
			{
				if (incidentGroupStatusConfigurationsSerializer == null)
				{
					incidentGroupStatusConfigurationsSerializer = ZXmlSerializer.New(typeof(IncidentGroupStatusConfigurationCollection));
				}

				return incidentGroupStatusConfigurationsSerializer;
			}
		}
		ZXmlSerializer incidentGroupStatusConfigurationsSerializer;
		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			var removedConfigurations = IncidentGroupStatusConfigurations.RemovedBusinessObjList;
			if (!removedConfigurations.IsNullOrEmpty())
			{
				foreach (var item in removedConfigurations)
				{
					var result = item as IncidentGroupStatusConfiguration;
					if (result.CheckIsCodeInUse())
					{
						AddRowError(ResString.GetMultilingualString("a91cf86c-7922-4f3f-a853-049c76b840fc", $"The stage {result.Code} cannot be deleted as it is the active stage for existing Incident Management Groups."));
						return;
					}
				}
			}
		}

		public void SetOriginalGroupType(ZString groupType)
		{
			this.OriginalGroupType = groupType;
		}
	}
}
