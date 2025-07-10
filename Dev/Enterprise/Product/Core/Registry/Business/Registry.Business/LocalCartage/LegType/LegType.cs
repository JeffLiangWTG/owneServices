using System.ComponentModel;
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
	[CodeProperty(RegistryBusinessObject.Schema.Code), DescriptionProperty(RegistryBusinessObject.Schema.Description)]
	public class LegType : RegistryBusinessObject
	{
		#region Schema

		protected new abstract class Schema : RegistryBusinessObject.Schema
		{
			public const string PickupFromOrg = "PickupFromOrg";
			public const string WaitPointOrg = "WaitPointOrg";
			public const string DeliverToOrg = "DeliverToOrg";
			public const string MovementType = "MovementType";
			public const string Containerised = "Containerised";
			public const string EquipmentGroup = "EquipmentGroup";
			public const string IsSystemDefined = "IsSystemDefined";
		}

		#endregion

		public LegType()
		{
		}

		public LegType(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override string TableName
		{
			get { return "LegType"; }
		}

		#region Method Overrides

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidatePickupFromOrg();
			ValidateWaitPointOrg();
			ValidateDeliverToOrg();
			ValidateMovementType();
			ValidateContainerised();
			ValidateEquipmentGroup();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new LegType(factory);
		}

		#endregion

		#region Property Overrides

		[ReadOnlyMember(nameof(IsSystemDefined))]
		public override ZString Code
		{
			get { return base.Code; }
			set { base.Code = value; }
		}

		[ReadOnlyMember(nameof(IsSystemDefined))]
		public override MultilingualString Description
		{
			get { return base.Description; }
			set { base.Description = value; }
		}

		#endregion

		#region Validation Overrides

		protected override void ValidateDescriptionCore()
		{
			MandatoryValidation.CheckEntered(DescriptionInfo, Res.GetString("a4567f27-e1ab-4415-83e7-3302af2574cf", "Description"));
		}

		#endregion

		#region Bound Properties

		#region Pickup / Wait Point / Deliver Orgs

		#region PickupFromOrg

		[ReadOnlyMember(nameof(IsSystemDefined))]
		public ZString PickupFromOrg
		{
			get { return pickupFromOrg; }
			set
			{
				CheckMaximumLength(PickupFromOrgInfo, value);
				SetNonPersistentPropertyValue<ZString>(PickupFromOrgInfo, ref pickupFromOrg, value);
				if (!IsValidationSuspended)
				{
					ValidatePickupFromOrg();
				}
			}
		}

		public ZPropertyInfo PickupFromOrgInfo
		{
			get { return GetZPropertyInfo(Schema.PickupFromOrg); }
		}

		public int PickupFromOrg_MaxLength
		{
			get { return OrgType_List.MaxCodeLength; }
		}

		public void ValidatePickupFromOrg()
		{
			PickupFromOrgInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(PickupFromOrgInfo, OrgType_List);
			MandatoryValidation.CheckEntered(PickupFromOrgInfo);
		}

		ZString pickupFromOrg;

		#endregion

		#region WaitPointOrg

		[ReadOnlyMember(nameof(IsSystemDefined))]
		public ZString WaitPointOrg
		{
			get { return waitPointOrg; }
			set
			{
				CheckMaximumLength(WaitPointOrgInfo, value);
				SetNonPersistentPropertyValue<ZString>(WaitPointOrgInfo, ref waitPointOrg, value);
				if (!IsValidationSuspended)
				{
					ValidateWaitPointOrg();
				}
			}
		}

		public ZPropertyInfo WaitPointOrgInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.WaitPointOrg);
			}
		}

		public int WaitPointOrg_MaxLength
		{
			get { return OrgType_List.MaxCodeLength; }
		}

		public void ValidateWaitPointOrg()
		{
			WaitPointOrgInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(WaitPointOrgInfo, OrgType_List);
			if (EquipmentGroup == Constants.FCLEquipmentNeeded.WaitForUnpack)
			{
				MandatoryValidation.CheckEntered(WaitPointOrgInfo);
			}
			else if (EquipmentGroup != Constants.FCLEquipmentNeeded.WaitForUnpack && !WaitPointOrg.IsEmpty)
			{
				WaitPointOrgInfo.AddError(Res.GetString("ef62167c-1580-404f-9168-c9b575ba935d", "A Wait Type Equipment Group is needed for this Wait Point Organization."));
			}
		}

		ZString waitPointOrg;

		#endregion

		#region DeliverToOrg

		[ReadOnlyMember(nameof(IsSystemDefined))]
		public ZString DeliverToOrg
		{
			get { return deliverToOrg; }
			set
			{
				CheckMaximumLength(DeliverToOrgInfo, value);
				SetNonPersistentPropertyValue<ZString>(DeliverToOrgInfo, ref deliverToOrg, value);
				if (!IsValidationSuspended)
				{
					ValidateDeliverToOrg();
				}
			}
		}

		public ZPropertyInfo DeliverToOrgInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.DeliverToOrg);
			}
		}

		public int DeliverToOrg_MaxLength
		{
			get { return OrgType_List.MaxCodeLength; }
		}

		public void ValidateDeliverToOrg()
		{
			DeliverToOrgInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(DeliverToOrgInfo, OrgType_List);
			MandatoryValidation.CheckEntered(DeliverToOrgInfo);
		}

		ZString deliverToOrg;

		#endregion

		#endregion

		#region MovementType

		[ReadOnlyMember(nameof(IsSystemDefined))]
		public ZString MovementType
		{
			get { return movementType; }
			set
			{
				CheckMaximumLength(MovementTypeInfo, value);
				SetNonPersistentPropertyValue<ZString>(MovementTypeInfo, ref movementType, value);
				if (!IsValidationSuspended)
				{
					ValidateMovementType();
				}
			}
		}

		public ZPropertyInfo MovementTypeInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.MovementType);
			}
		}

		public int MovementType_MaxLength
		{
			get { return MovementType_List.MaxCodeLength; }
		}

		public void ValidateMovementType()
		{
			MovementTypeInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(MovementTypeInfo, MovementType_List);
			MandatoryValidation.CheckEntered(MovementTypeInfo);
		}

		ZString movementType;

		#endregion

		#region Containerised

		[ReadOnlyMember(nameof(IsSystemDefined))]
		public ZString Containerised
		{
			get { return containerised; }
			set
			{
				CheckMaximumLength(ContainerisedInfo, value);
				SetNonPersistentPropertyValue<ZString>(ContainerisedInfo, ref containerised, value);
				if (!IsValidationSuspended)
				{
					ValidateContainerised();
				}
			}
		}

		public ZPropertyInfo ContainerisedInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.Containerised);
			}
		}

		public int Containerised_MaxLength
		{
			get { return Containerised_List.MaxCodeLength; }
		}

		public void ValidateContainerised()
		{
			ContainerisedInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(ContainerisedInfo, Containerised_List);
			MandatoryValidation.CheckEntered(ContainerisedInfo);
		}

		ZString containerised;

		#endregion

		#region EquipmentGroup

		[ReadOnly(false)]
		public ZString EquipmentGroup
		{
			get { return equipmentGroup; }
			set
			{
				CheckMaximumLength(EquipmentGroupInfo, value);
				SetNonPersistentPropertyValue<ZString>(EquipmentGroupInfo, ref equipmentGroup, value);
				if (!IsValidationSuspended)
				{
					ValidateEquipmentGroup();
				}
			}
		}

		public ZPropertyInfo EquipmentGroupInfo
		{
			get
			{
				return GetZPropertyInfo(Schema.EquipmentGroup);
			}
		}

		public int EquipmentGroup_MaxLength
		{
			get { return EquipmentGroup_List.MaxCodeLength; }
		}

		public void ValidateEquipmentGroup()
		{
			EquipmentGroupInfo.ClearAllNotifications();
			ListValidation.ErrorIfInvalidCode(EquipmentGroupInfo, EquipmentGroup_List);
			if (!EquipmentGroupInfo.HasErrors())
			{
				if (EquipmentGroup == Constants.FCLEquipmentNeeded.WaitForUnpack && waitPointOrg.IsEmpty)
				{
					EquipmentGroupInfo.AddError(Res.GetString("e1d127fa-4d25-4fc8-b9e7-a1375b32559c", "A Wait Point Organization is needed for this Equipment Group."));
				}
			}
		}

		ZString equipmentGroup;

		#endregion

		#region IsSystemDefined

		[ReadOnly(true)]
		public ZBool IsSystemDefined
		{
			get { return isSystemDefined; }
			set
			{
				if (SetNonPersistentPropertyValue<ZBool>(IsSystemDefinedInfo, ref isSystemDefined, value))
				{
					CodeInfo.RefreshBinding();
					DescriptionInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo IsSystemDefinedInfo
		{
			get { return GetZPropertyInfo(Schema.IsSystemDefined); }
		}

		ZBool isSystemDefined;

		#endregion

		#endregion

		#region List Properties

		#region OrgType_List

		public CodeDescriptionPairList OrgType_List
		{
			get
			{
				if (fOrgType_List == null)
				{
					fOrgType_List = LocalCartageJobOrgTypeList.Instance;
				}

				return fOrgType_List;
			}
		}

		CodeDescriptionPairList fOrgType_List;

		#endregion

		#region MovementType_List

		public CodeDescriptionPairList MovementType_List
		{
			get
			{
				if (fMovementType_List == null)
				{
					fMovementType_List = new CodeDescriptionPairList();
					fMovementType_List.Add(new CodeDescriptionPair(Constants.CartageDirection.Origin, Constants.CartageDirectionDescription.Origin));
					fMovementType_List.Add(new CodeDescriptionPair(Constants.CartageDirection.Destination, Constants.CartageDirectionDescription.Destination));
					fMovementType_List.Add(new CodeDescriptionPair(Constants.CartageDirection.Local, Constants.CartageDirectionDescription.Local));
					fMovementType_List.Add(new CodeDescriptionPair(Constants.CartageDirection.LineHaul, Constants.CartageDirectionDescription.LineHaul));
				}

				return fMovementType_List;
			}
		}

		CodeDescriptionPairList fMovementType_List;

		#endregion

		#region Containerised_List

		public CodeDescriptionPairList Containerised_List
		{
			get
			{
				if (containerised_List == null)
				{
					containerised_List = new CodeDescriptionPairList();
					containerised_List.Add(new CodeDescriptionPair(Constants.ContainerModes.Containerised, Constants.ContainerModeDescriptions.Containerised));
					containerised_List.Add(new CodeDescriptionPair(Constants.ContainerModes.Loose, Constants.ContainerModeDescriptions.Loose));
				}

				return containerised_List;
			}
		}

		CodeDescriptionPairList containerised_List;

		#endregion

		#region EquipmentGroup_List	// WILL CHANGE - Dependent on Containerised

		public CodeDescriptionPairList EquipmentGroup_List
		{
			get
			{
				if (equipmentGroup_List == null)
				{
					equipmentGroup_List = new CodeDescriptionPairList();
					equipmentGroup_List.Add(new CodeDescriptionPair(Constants.FCLEquipmentNeeded.LiftOffOn, FCLEquipmentNeededList.Descriptions.LiftOffOn));
					equipmentGroup_List.Add(new CodeDescriptionPair(Constants.FCLEquipmentNeeded.SideLoader, FCLEquipmentNeededList.Descriptions.SideLoader));
					equipmentGroup_List.Add(new CodeDescriptionPair(Constants.FCLEquipmentNeeded.Trailer, FCLEquipmentNeededList.Descriptions.Trailer));
					equipmentGroup_List.Add(new CodeDescriptionPair(Constants.FCLEquipmentNeeded.WaitForUnpack, FCLEquipmentNeededList.Descriptions.WaitForUnpack));
				}

				return equipmentGroup_List;
			}
		}

		CodeDescriptionPairList equipmentGroup_List;

		#endregion

		#endregion

		#region New Properties

		public bool HasConsigneeOrConsignor
		{
			get
			{
				return (PickupFromOrg == LocalCartageJobOrgTypeList.Codes.CNE ||
						PickupFromOrg == LocalCartageJobOrgTypeList.Codes.CNR ||
						WaitPointOrg == LocalCartageJobOrgTypeList.Codes.CNE ||
						WaitPointOrg == LocalCartageJobOrgTypeList.Codes.CNR ||
						DeliverToOrg == LocalCartageJobOrgTypeList.Codes.CNE ||
						DeliverToOrg == LocalCartageJobOrgTypeList.Codes.CNR);
			}
		}

		public bool IsContainerised
		{
			get { return Containerised == Constants.ContainerModes.Containerised; }
		}

		public bool IsLoose
		{
			get { return Containerised == Constants.ContainerModes.Loose; }
		}

		#endregion

		#region Write/Read Xml

		protected override void WriteMoreElements(XmlWriter writer)
		{
			writer.WriteElementString(Schema.PickupFromOrg, PickupFromOrg);
			writer.WriteElementString(Schema.WaitPointOrg, WaitPointOrg);
			writer.WriteElementString(Schema.DeliverToOrg, DeliverToOrg);
			writer.WriteElementString(Schema.MovementType, MovementType);
			writer.WriteElementString(Schema.Containerised, Containerised);
			writer.WriteElementString(Schema.EquipmentGroup, EquipmentGroup);
			writer.WriteElementString(Schema.IsSystemDefined, IsSystemDefined.ToString());
		}

		protected override void ReadMoreElements(XmlReader reader)
		{
			PickupFromOrg = reader.ReadElementString(Schema.PickupFromOrg);
			WaitPointOrg = reader.ReadElementString(Schema.WaitPointOrg);
			DeliverToOrg = reader.ReadElementString(Schema.DeliverToOrg);
			MovementType = reader.ReadElementString(Schema.MovementType);
			Containerised = reader.ReadElementString(Schema.Containerised);
			EquipmentGroup = reader.ReadElementString(Schema.EquipmentGroup);
			IsSystemDefined = new ZBool(reader.ReadElementString(Schema.IsSystemDefined));
		}

		#endregion

		#region ICanDelete Members

		public override bool CanDelete
		{
			get { return !IsSystemDefined; }
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("2f6d7227-5dfb-4d8a-a814-bd70552954d9", "Cannot delete System Defined Leg Types."); }
		}

		#endregion
	}
}
