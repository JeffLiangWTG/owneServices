using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using ResString = ZClientEDI.Business.ResString;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class IncidentGroupStatusConfiguration : AutoIncidentGroupStatusConfiguration, ICanDelete
	{
		public IncidentGroupStatusConfiguration() : base()
		{
		}

		public IncidentGroupStatusConfiguration(BusinessObjectFactory factory) : base(factory)
		{
		}
		public IncidentGroupStatusConfiguration(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}
		public IncidentGroupStatusConfiguration(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
		}

		#region Properties Read-Only
		public bool Sequence_ReadOnly { get => IsSystem; }
		public bool Code_ReadOnly { get => IsSystem; }
		public bool TriggerOn_ReadOnly { get => true; }
		public bool Enabled_ReadOnly { get => IncidentGroupStatusConfigurationConstants.EnabledOnlyConfigurations.Contains(Code); }
		#endregion

		public string ParentCode { set; get; }

		public ZString OriginalCode { get; private set; } = string.Empty;

		public bool CheckIsCodeInUse()
		{
			if (!string.IsNullOrEmpty(ParentCode) && !string.IsNullOrEmpty(OriginalCode))
			{
				var factory = Factory ?? new BusinessObjectFactory();
				var query = new ZQuery(IncidentManagementGroupSchema.ING_Type, ParentCode);
				query.AddToFilter(IncidentManagementGroupSchema.ING_Status, OriginalCode);
				var exist = factory.Exists(typeof(IncidentManagementGroup), query);
				if (exist)
				{
					this.AddRowWarning(ResString.GetMultilingualString("bd7ff045-ed07-4560-a73a-f508f013710c", "This stage is the active stage for one or more Incident Management Groups. It cannot be deleted."));
				}
				return exist;
			}
			return false;
		}

		protected override void SetCustomDefaultValuesCore()
		{
			base.SetCustomDefaultValuesCore();
			TriggerOn = IncidentGroupStatusConfigurationConstants.TriggerOnDefaultValue;
			IsSystem = false;
			Enabled = true;
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new IncidentGroupStatusConfiguration(fallbackLevel, factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			var cloneGroupType = (IncidentGroupStatusConfiguration)clone;
			if (!string.IsNullOrEmpty(Code))
			{
				cloneGroupType.OriginalCode = Code;
			}
		}

		#region Delete
		public override bool CanDelete => !IsSystem && !CheckIsCodeInUse();

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (IsSystem)
				{
					return ResString.GetMultilingualString("fa9c85f5-90a8-4477-9166-2c20856884a2", "System-defined setup(s) cannot be deleted.");
				}
				return ResString.GetMultilingualString("30af5510-a46f-4d38-aab7-af8ec25a805c", $"The stage {this.Code} cannot be deleted as it is the active stage for existing Incident Management Groups. You may disable the stage, which will allow any user to manually set the active stage when opening an affected group.");
			}
		}
		#endregion

		#region Validation
		public override void ValidateSequence()
		{
			base.ValidateSequence();
			MandatoryValidation.CheckEntered(SequenceInfo);
			if (Sequence < 0)
			{
				SequenceInfo.AddError(ResString.GetMultilingualString("5d9af5d4-fa6c-4231-8195-aba520da4730", "Sequence should be greater than 0."));
			}
		}

		public override void ValidateCode()
		{
			base.ValidateCode();
			MandatoryValidation.CheckEntered(CodeInfo);
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(CodeInfo);
			if (Code.Length != Schema.CodeMaxLength)
			{
				CodeInfo.AddError(ResString.GetMultilingualString("58364d7d-490a-46ab-ae9f-8e2c6e1d3393", "Code should have 3 characters."));
			}
			if (Code != OriginalCode && CheckIsCodeInUse())
			{
				CodeInfo.AddError(ResString.GetMultilingualString("38362156-6a95-4a97-876f-7c0bc761b9d2", "This code cannot be changed as it is in use by at least one record in the system."));
			}
		}

		public override void ValidateTriggerOn()
		{
			base.ValidateTriggerOn();
			if (((Code == IncidentGroupStatusConfigurationConstants.ActiveIncident || Code == IncidentGroupStatusConfigurationConstants.PostIncident)) && !ControlIncidents)
			{
				TriggerOnInfo.AddWarning(ResString.GetMultilingualString("2e2d733b-6012-4402-860e-a26f4e28b091", "Broadcast will not be sent"));
			}
		}

		public override void ValidateControlIncidents()
		{
			base.ValidateControlIncidents();
			ValidateTriggerOn();
		}

		#endregion

		#region Notification

		public void ShowNotification()
		{
			CheckIsCodeInUse();
			ValidateTriggerOn();
		}

		#endregion

		public void SetOriginalCode(ZString code)
		{
			this.OriginalCode = code;
		}
	}
}
