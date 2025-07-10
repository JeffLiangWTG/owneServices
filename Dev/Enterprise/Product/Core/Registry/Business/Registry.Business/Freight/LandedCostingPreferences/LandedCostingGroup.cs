using System;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Registry.Business
{
	public interface ILandedCostPreference
	{
		ZByte LCGroupID { get; }
		ZString LCGroupName { get; }
		ZString DistributionBy { get; }
	}

	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class LandedCostingGroup : RegistryBusinessObjectTemplate, ILandedCostPreference
	{
		#region Schema

		public abstract class Schema
		{
			public const string GroupID = "GroupID";
			public const string GroupName = "GroupName";
			public const string CostDistributionCode = "CostDistributionCode";
			public const string CostDistributionDescription = "CostDistributionDescription";
			public const string Charges = "Charges";
		}

		#endregion

		public LandedCostingGroup()
		{
		}

		public LandedCostingGroup(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateGroupID();
			ValidateGroupName();
			ValidateCostDistributionCode();
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new LandedCostingGroup(fallbackLevel, factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);

			if (Charges != null)
			{
				LandedCostingGroup landedCostingGroupClone = (LandedCostingGroup)clone;
				landedCostingGroupClone.CloneChargesFrom(landedCostingGroupClone, Charges);
			}
		}

		#region Bound Properties

		#region Group ID

		public ZInt GroupID
		{
			get { return groupID; }
			set
			{
				SetNonPersistentPropertyValue<ZInt>(GroupIDInfo, ref groupID, value);
				if (!IsValidationSuspended)
				{
					ValidateGroupID();
				}
			}
		}

		public void ValidateGroupID()
		{
			GroupIDInfo.ClearAllNotifications();
			CompareValidation.CheckNumberGreaterThanZero(GroupIDInfo);

			if (ParentCollections.Count > 0)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(GroupIDInfo);
			}
		}

		public ZPropertyInfo GroupIDInfo
		{
			get { return GetZPropertyInfo(Schema.GroupID); }
		}

		ZInt groupID;

		#endregion

		#region Group Name

		[CargoWise.ComponentModel.MaxLength(25)]
		public ZString GroupName
		{
			get { return groupName; }
			set
			{
				CheckMaximumLength(GroupNameInfo, value);
				SetNonPersistentPropertyValue<ZString>(GroupNameInfo, ref groupName, value);
				if (!IsValidationSuspended)
				{
					ValidateGroupName();
				}
			}
		}

		public void ValidateGroupName()
		{
			GroupNameInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(GroupNameInfo);
		}

		public ZPropertyInfo GroupNameInfo
		{
			get { return GetZPropertyInfo(Schema.GroupName); }
		}

		ZString groupName;

		#endregion

		#region Cost Distribution Code

		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString CostDistributionCode
		{
			get { return costDistributionCode; }
			set
			{
				CheckMaximumLength(CostDistributionCodeInfo, value);
				SetNonPersistentPropertyValue<ZString>(CostDistributionCodeInfo, ref costDistributionCode, value);
				if (!IsValidationSuspended)
				{
					ValidateCostDistributionCode();
				}
			}
		}

		public void ValidateCostDistributionCode()
		{
			CostDistributionCodeInfo.ClearAllNotifications();

			MandatoryValidation.CheckEntered(CostDistributionCodeInfo);
			ListValidation.ErrorIfInvalidCode(CostDistributionCodeInfo, CostDistributionList);
		}

		public ZPropertyInfo CostDistributionCodeInfo
		{
			get { return GetZPropertyInfo(Schema.CostDistributionCode); }
		}

		ZString costDistributionCode;

		#endregion

		#region Cost Distribution Description

		public ZString CostDistributionDescription
		{
			get { return CostDistributionList.GetDescriptionFromCode(CostDistributionCode); }
		}

		public ZPropertyInfo CostDistributionDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.CostDistributionDescription); }
		}

		#endregion

		#endregion

		#region Charges

		public ChargeGroupAndChargeCodeCollection Charges
		{
			get
			{
				if (fCharges == null)
				{
					fCharges = new ChargeGroupAndChargeCodeCollection(this, CurrentFallbackLevel, CurrentFactory);
					RegisterEditableChildObject(fCharges);
				}

				return fCharges;
			}
		}

		void CloneChargesFrom(LandedCostingGroup parentLandedCostingGroup, ChargeGroupAndChargeCodeCollection existingCharges)
		{
			fCharges = existingCharges.Clone(parentLandedCostingGroup, CurrentFallbackLevel, CurrentFactory);
			RegisterEditableChildObject(fCharges);
		}

		ZXmlSerializer ChargeGroupAndChargeCodeCollectionSerialiser
		{
			get
			{
				if (fChargeGroupAndChargeCodeCollectionSerialiser == null)
				{
					fChargeGroupAndChargeCodeCollectionSerialiser = ZXmlSerializer.New(typeof(ChargeGroupAndChargeCodeCollection));
				}
				return fChargeGroupAndChargeCodeCollectionSerialiser;
			}
		}

		ChargeGroupAndChargeCodeCollection fCharges;
		ZXmlSerializer fChargeGroupAndChargeCodeCollectionSerialiser;

		#endregion

		#region Cost Distribution List

		public CodeDescriptionPairList CostDistributionList
		{
			get
			{
				if (fCostDistributionList == null)
				{
					Assembly assembly = Assembly.Load("Enterprise.MasterFiles.Business");
					Type orgLandedCostingPrefsLookupsType = assembly.GetType("Enterprise.MasterFiles.Business.OrgLandedCostingPrefsLookups");
					object orgLandedCostingPrefsLookups = Activator.CreateInstance(orgLandedCostingPrefsLookupsType, new object[] { null });

					PropertyInfo info = orgLandedCostingPrefsLookupsType.GetProperty("CostDistributionMechanisms", BindingFlags.Public | BindingFlags.Instance);
					fCostDistributionList = (CodeDescriptionPairList)info.GetValue(orgLandedCostingPrefsLookups, null);
				}

				return fCostDistributionList;
			}
		}

		CodeDescriptionPairList fCostDistributionList;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.GroupID, GroupID.ToString());
			writer.WriteElementString(Schema.GroupName, GroupName);
			writer.WriteElementString(Schema.CostDistributionCode, CostDistributionCode);

			ChargeGroupAndChargeCodeCollectionSerialiser.Serialize(writer, Charges);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			GroupID = ZInt.Parse(reader.ReadElementString(Schema.GroupID));
			GroupName = reader.ReadElementString(Schema.GroupName);
			CostDistributionCode = reader.ReadElementString(Schema.CostDistributionCode);

			ChargeGroupAndChargeCodeCollection charges = (ChargeGroupAndChargeCodeCollection)ChargeGroupAndChargeCodeCollectionSerialiser.Deserialize(reader);
			CloneChargesFrom(this, charges);
		}

		#endregion

		#region ILandedCostPreference Members

		ZByte ILandedCostPreference.LCGroupID
		{
			get { return Convert.ToByte(GroupID); }
		}

		ZString ILandedCostPreference.LCGroupName
		{
			get { return GroupName; }
		}

		ZString ILandedCostPreference.DistributionBy
		{
			get { return CostDistributionCode; }
		}

		#endregion
	}
}
