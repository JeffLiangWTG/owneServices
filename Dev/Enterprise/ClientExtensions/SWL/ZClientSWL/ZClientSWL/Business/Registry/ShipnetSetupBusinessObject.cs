using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Client.SWL.Business
{
	[XmlSerializerAssembly("ZClientSWL.XmlSerializers")]
	public class ShipnetSetupBusinessObject : RegistryBusinessObjectTemplate, IDisposable
	{
		public ShipnetSetupBusinessObject()
		{
		}

		public ShipnetSetupBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Schema

		public static class Schema
		{
			public const string IsShipnetCarrier = "IsShipnetCarrier";
			public const string CommunicationPK = "CommunicationPK";
			public const string CommunicationMode = "CommunicationMode";
			public const string ChargeGroups = "ChargeGroups";
			public const string DebtorControlCode = "DebtorControlCode";
			public const string CreditorControlCode = "CreditorControlCode";
		}

		#endregion

		#region Bound Properties

		public OrgCompanyData CompanyData
		{
			get { return CurrentFactory.Load<OrgCompanyData>(companyDataPK); }
		}

		#region IShipnetSetupBusinessObject Members

		#region IsShipnetCarrier

		public ZBool IsShipnetCarrier
		{
			get
			{
				return fIsShipnetCarrier;
			}
			set
			{
				if (fIsShipnetCarrier != value)
				{
					SetNonPersistentPropertyValue<ZBool>(IsShipnetCarrierInfo, ref fIsShipnetCarrier, value);
					if (fIsShipnetCarrier)
					{
						RegisterCommunicationMode();
					}
					else
					{
						UnRegisterCommunicationModeAndDeleteOldValue();
					}
					if (!IsValidationSuspended && !((IBusinessObjectInternals)this).IsCopying)
					{
						ValidateIsShipnetCarrier();
					}
				}
			}
		}

		void RegisterCommunicationMode()
		{
			if (fCommunicationMode != null)
			{
				RegisterEditableChildObject(fCommunicationMode);
			}
		}

		void UnRegisterCommunicationModeAndDeleteOldValue()
		{
			if (fCommunicationMode != null)
			{
				UnRegisterEditableChildObject(fCommunicationMode);
				fCommunicationMode.Delete();
				fCommunicationMode = null;
			}
			if (fChargeGroups != null)
			{
				UnRegisterEditableChildObject(fChargeGroups);
				fChargeGroups.RemoveAndDeleteAll();
				fChargeGroups = null;
			}
			fCommunicationPK = ZGuid.Empty;
			DebtorControlCode = ZString.Empty;
			CreditorControlCode = ZString.Empty;
		}

		ZBool fIsShipnetCarrier;

		public ZPropertyInfo IsShipnetCarrierInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.IsShipnetCarrier);
			}
		}

		void ValidateIsShipnetCarrier()
		{
			IsShipnetCarrierInfo.ClearAllNotifications();
		}

		#endregion

		internal ZGuid CompanyDataPK
		{
			get { return companyDataPK; }
			set { companyDataPK = value; }
		}
		ZGuid companyDataPK;

		#endregion

		#region CommunicationMode

		public EDICommunicationsMode CommunicationMode
		{
			get
			{
				if (fCommunicationMode == null || fCommunicationMode.IsDeleted || fCommunicationMode.PK != fCommunicationPK)
				{
					if (fCommunicationMode != null && !fCommunicationMode.IsDeleted)
					{
						UnRegisterEditableChildObject(fCommunicationMode);
						fCommunicationMode.Delete();
						fCommunicationMode = null;
					}

					if (fCommunicationPK.IsValid)
					{
						fCommunicationMode = CurrentFactory.Load<EDICommunicationsMode>(fCommunicationPK);
					}

					if (fCommunicationMode == null)
					{
						fCommunicationMode = LoadFromParentDetails();
					}

					if (fCommunicationMode == null)
					{
						fCommunicationMode = CurrentFactory.New<EDICommunicationsMode>();
						fCommunicationPK = fCommunicationMode.PK;
						if (CompanyData != null && fCommunicationMode.EK_ParentID != CompanyData.PK)
						{
							fCommunicationMode.EK_ParentID = CompanyData.PK;
							fCommunicationMode.EK_ParentTableCode = OrgCompanyDataSchema.Constants.Prefix;
						}
					}
					if (IsShipnetCarrier)
					{
						RegisterEditableChildObject(fCommunicationMode);
					}
				}
				if (CompanyData != null && fCommunicationMode.EK_ParentID != CompanyData.PK)
				{
					fCommunicationMode.EK_ParentID = CompanyData.PK;
					fCommunicationMode.EK_ParentTableCode = OrgCompanyDataSchema.Constants.Prefix;
				}
				fCommunicationMode.EK_Module = EDICommunicationsMode.Modules.Shipnet;
				return fCommunicationMode;
			}
		}
		EDICommunicationsMode fCommunicationMode;

		EDICommunicationsMode LoadFromParentDetails()
		{
			EDICommunicationsMode result = null;
			OrgCompanyData companyData = CompanyData;
			if (companyData != null)
			{
				ZQuery query = new ZQuery(EDICommunicationsModeSchema.EK_Module, EDICommunicationsMode.Modules.Shipnet);
				query.AddToFilter(EDICommunicationsModeSchema.EK_ParentID, companyData.PK);
				query.AddToFilter(EDICommunicationsModeSchema.EK_ParentTableCode, companyData.TablePrefix);
				result = CurrentFactory.LoadTop1<EDICommunicationsMode>(query);
			}

			return result;
		}

		internal ZGuid fCommunicationPK;

		#endregion

		#region ChargeGroups

		public ShipnetChargeGroupCollection ChargeGroups
		{
			get
			{
				if (fChargeGroups == null)
				{
					fChargeGroups = new ShipnetChargeGroupCollection(this, CurrentFactory);
					RegisterEditableChildObject(fChargeGroups);
				}
				return fChargeGroups;
			}
		}

		ShipnetChargeGroupCollection fChargeGroups;

		#endregion

		#region DebtorControlCode

		[MaxLength(15)]
		public ZString DebtorControlCode
		{
			get
			{
				return fDebtorControlCode;
			}
			set
			{
				value = value.TrimEnd(' ');
				if (fDebtorControlCode != value)
				{
					CheckMaximumLength(DebtorControlCodeInfo, value);
					SetNonPersistentPropertyValue<ZString>(DebtorControlCodeInfo, ref fDebtorControlCode, value);
					if (!IsValidationSuspended && !((IBusinessObjectInternals)this).IsCopying)
					{
						ValidateDebtorControlCode();
					}
				}
			}
		}

		ZString fDebtorControlCode;

		public ZPropertyInfo DebtorControlCodeInfo
		{
			get { return GetZPropertyInfo(Schema.DebtorControlCode); }
		}

		void ValidateDebtorControlCode()
		{
			DebtorControlCodeInfo.ClearAllNotifications();
			if (IsShipnetCarrier)
			{
				MandatoryValidation.CheckEntered(DebtorControlCodeInfo);
			}
		}

		#endregion

		#region CreditorControlCode

		[MaxLength(15)]
		public ZString CreditorControlCode
		{
			get
			{
				return fCreditorControlCode;
			}
			set
			{
				value = value.TrimEnd(' ');
				if (fCreditorControlCode != value)
				{
					CheckMaximumLength(CreditorControlCodeInfo, value);
					SetNonPersistentPropertyValue<ZString>(CreditorControlCodeInfo, ref fCreditorControlCode, value);
					if (!IsValidationSuspended && !((IBusinessObjectInternals)this).IsCopying)
					{
						ValidateCreditorControlCode();
					}
				}
			}
		}

		ZString fCreditorControlCode;

		public ZPropertyInfo CreditorControlCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CreditorControlCode); }
		}

		void ValidateCreditorControlCode()
		{
			CreditorControlCodeInfo.ClearAllNotifications();
			if (IsShipnetCarrier)
			{
				MandatoryValidation.CheckEntered(CreditorControlCodeInfo);
			}
		}

		#endregion

		#region EDI Export Mapping Address Type List

		CodeDescriptionPairList fEDIExportMappingConfigurationList;
		public CodeDescriptionPairList EDIExportMappingConfigurationList
		{
			get
			{
				if (fEDIExportMappingConfigurationList == null)
				{
					fEDIExportMappingConfigurationList = new ShipnetExportCommunicationsTransportMappingList();
				}
				return fEDIExportMappingConfigurationList;
			}
		}

		#endregion

		#endregion

		public override bool HasChanges
		{
			get { return base.HasChanges; }
			set
			{
				bool oldHasChange = HasChanges;
				base.HasChanges = value;
				if (!IsCopying)
				{
					if (oldHasChange != value && oldHasChange)
					{
						SetAllChildHasChange(value);
					}
				}
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			if (fCommunicationMode != null)
			{
				UnRegisterEditableChildObject(fCommunicationMode);
				fCommunicationMode = null;
			}
			if (fChargeGroups != null)
			{
				UnRegisterEditableChildObject(fChargeGroups);
				fChargeGroups = null;
			}
		}

		#endregion

		#region Implementation

		#region Overrides

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			ShipnetSetupBusinessObject cloned = (ShipnetSetupBusinessObject)clone;
			using (cloned.GetValidationSuspender())
			{
				cloned.companyDataPK = this.companyDataPK;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get { return Res.GetString("229b4510-f6b7-4e3e-a4cd-9cb3beb3d665", "Shipnet Settings"); }
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			ShipnetSetupBusinessObject cloneObj = new ShipnetSetupBusinessObject(factory);
			try
			{
				((IBusinessObjectInternals)cloneObj).IsCopying = true;
				cloneObj.IsShipnetCarrier = IsShipnetCarrier;
				cloneObj.fCommunicationPK = fCommunicationPK;
				cloneObj.DebtorControlCode = DebtorControlCode;
				cloneObj.CreditorControlCode = CreditorControlCode;
				cloneObj.ChargeGroups.AddRange((BusinessObjectCollection)ChargeGroups.Clone(fallbackLevel, factory));
			}
			finally
			{
				cloneObj.HasChanges = false;
				((IBusinessObjectInternals)cloneObj).IsCopying = false;
			}
			return cloneObj;
		}

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.CommunicationPK, fCommunicationPK.ToString());
			writer.WriteElementString(Schema.DebtorControlCode, DebtorControlCode.ToString());
			writer.WriteElementString(Schema.CreditorControlCode, CreditorControlCode.ToString());
			ChargeGroupCollectionSerializer.Serialize(writer, ChargeGroups);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			try
			{
				((IBusinessObjectInternals)this).IsCopying = true;
				fCommunicationPK = new ZGuid(reader.ReadElementString(Schema.CommunicationPK));
				DebtorControlCode = reader.ReadElementString(Schema.DebtorControlCode);
				CreditorControlCode = reader.ReadElementString(Schema.CreditorControlCode);
				ChargeGroups.RemoveAndDeleteAll();
				ChargeGroups.AddRange((ShipnetChargeGroupCollection)ChargeGroupCollectionSerializer.Deserialize(reader));
			}
			finally
			{
				HasChanges = false;
				((IBusinessObjectInternals)this).IsCopying = false;
			}
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateChargeGroups();
			ValidateDebtorControlCode();
			ValidateCreditorControlCode();
		}

		protected void SetAllChildHasChange(bool changed)
		{
			if (fChargeGroups != null)
			{
				foreach (ShipnetChargeGroup group in fChargeGroups)
				{
					group.HasChanges = changed;
				}
			}
		}

		public override void Delete()
		{
			CommunicationMode.Delete();
			base.Delete();
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			fCommunicationPK = new ZGuid("11111111-1111-1111-1111-111111111111");
			DebtorControlCode = "DebtorCode";
			CreditorControlCode = "CreditorCode";
			ShipnetChargeGroup chargeGroup = ChargeGroups.AddNew();
			chargeGroup.FillWithValidTestData(kind, propertyPath);
		}
#endif

#endregion

		void ValidateChargeGroups()
		{
			ClearRowNotifications();
			if (IsShipnetCarrier)
			{
				if (ChargeGroups.Count == 0)
				{
					AddRowError(Res.GetString("728d1a6b-367a-4eea-a4d8-a6af80493381", "A Charge Group must be assigned."));
				}
				else
				{
					CheckDuplicateCharges();
				}
			}
		}

		#region CheckDuplicateCharges

		void CheckDuplicateCharges()
		{
			for (int i = 0; i < ChargeGroups.Count; i++)
			{
				for (int j = i + 1; j < ChargeGroups.Count; j++)
				{
					foreach (ShipnetCharge charge in ChargeGroups[i].Charges)
					{
						if (charge.ChargePK.IsValid && ChargeGroups[j].Charges.ContainsChargePK(charge.ChargePK))
						{
							if (charge.AccChargeCode == null)
							{
								AddRowError(Res.GetString("947992d3-1ac0-47d9-8581-8e8622d87f9d", "A charge code is assigned to more than one Charge Group."));
							}
							else
							{
								AddRowError(Res.GetString("5ea6155e-3f91-4884-b5c6-ec468777afdf", "Charge code '{0}' is assigned to more than one Charge Group.", charge.AccChargeCode.AC_Code));
							}
							break;
						}
					}
				}
			}
		}

		#endregion

		#region ChargeGroupCollectionSerializer

		ZXmlSerializer ChargeGroupCollectionSerializer
		{
			get
			{
				if (fChargeGroupCollectionSerializer == null)
				{
					fChargeGroupCollectionSerializer = ZXmlSerializer.New(typeof(ShipnetChargeGroupCollection));
				}

				return fChargeGroupCollectionSerializer;
			}
		}

		ZXmlSerializer fChargeGroupCollectionSerializer;

		#endregion
	}
	#endregion
}

