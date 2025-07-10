using System;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class ChargeGroupAndChargeCode : RegistryBusinessObjectTemplate
	{
		#region Schema

		public abstract class Schema
		{
			public const string ChargeGroupCode = "ChargeGroupCode";
			public const string ChargeGroupDescription = "ChargeGroupDescription";
			public const string ChargeCodePK = "ChargeCodePK";
			public const string ChargeCodeDescription = "ChargeCodeDescription";
			public const string IsExcluded = "IsExcluded";
		}

		#endregion

		public ChargeGroupAndChargeCode()
		{
		}

		public ChargeGroupAndChargeCode(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ChargeGroupAndChargeCode(fallbackLevel, factory);
		}

		protected override void CopyValuesToClone(RegistryBusinessObjectTemplate clone)
		{
			base.CopyValuesToClone(clone);
			((ChargeGroupAndChargeCode)clone).ChargeCodeCodeForDefaultValue = ChargeCodeCodeForDefaultValue;
		}

		#region Bound Properties

		#region ChargeGroup Code

		[MaxLength(3)]
		public ZString ChargeGroupCode
		{
			get { return chargeGroupCode; }
			set
			{
				CheckMaximumLength(ChargeGroupCodeInfo, value);
				SetNonPersistentPropertyValue<ZString>(ChargeGroupCodeInfo, ref chargeGroupCode, value);

				if (!IsValidationSuspended)
				{
					ValidateChargeGroupCode();
				}
			}
		}

		public ZPropertyInfo ChargeGroupCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeGroupCode); }
		}

		public void ValidateChargeGroupCode()
		{
			ChargeGroupCodeInfo.ClearAllNotifications();

			ListValidation.ErrorIfInvalidCode(ChargeGroupCodeInfo, ChargeGroupList);

			ValidatedIsExcluded();
			ValidateChargeExistsInOneLandedCostingGroupOnly();
		}

		ZString chargeGroupCode;

		#endregion

		#region ChargeGroup Description

		public ZString ChargeGroupDescription
		{
			get { return ChargeGroupList.GetDescriptionFromCode(ChargeGroupCode); }
		}

		public ZPropertyInfo ChargeGroupDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeGroupDescription); }
		}

		#endregion

		#region ChargeCode PK

		public ZGuid ChargeCodePK
		{
			get
			{
				if (!IsChargeCodePkSet && CurrentFallbackLevel != null)
				{
					ZQuery filter = new ZQuery(AccChargeCodeSchema.AC_GC, CurrentFallbackLevel.CompanyPK(false));
					filter.AddToFilter(AccChargeCodeSchema.AC_Code, ChargeCodeCodeForDefaultValue);
					BusinessObject accChargeCode = (BusinessObject)CurrentFactory.LoadTop1<Enterprise.MasterFiles.Integration.IAccChargeCode>(filter);

					chargeCodePK = (accChargeCode != null) ? accChargeCode.PK : ZGuid.Empty;
				}
				return chargeCodePK;
			}
			set
			{
				SetNonPersistentPropertyValue<ZGuid>(ChargeCodePKInfo, ref chargeCodePK, value);
				if (CurrentFallbackLevel != null)
				{
					IsChargeCodePkSet = true;
				}
				if (!IsValidationSuspended)
				{
					ValidateChargeCodePK();
				}
			}
		}

		public ZPropertyInfo ChargeCodePKInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeCodePK); }
		}

		public void ValidateChargeCodePK()
		{
			if (!IsValidatingChargeCodePK)
			{
				IsValidatingChargeCodePK = true;

				ChargeCodePKInfo.ClearAllNotifications();

				if (CurrentFallbackLevel != null)
				{
					ListValidation.ErrorIfInvalidPK(ChargeCodePKInfo, ChargeCodeList);
				}

				if (!ChargeGroupCode.IsEmpty && ChargeCode != null)
				{
					ZString aC_ChargeGroup = (ZString)ChargeCode[AccChargeCodeSchema.Constants.AC_ChargeGroup];

					if (IsExcluded && aC_ChargeGroup != ChargeGroupCode)
					{
						ChargeCodePKInfo.AddError(Res.GetString("b3243116-5c5b-4db9-b1f2-6267093924dc", "You cannot exclude this Charge Code as it is not part of the Charge Group specified."));
					}
					else if (!IsExcluded && aC_ChargeGroup == ChargeGroupCode)
					{
						ChargeCodePKInfo.AddError(Res.GetString("6c171a98-f27d-48c1-931d-6a3f64c52a7c", "You cannot include this Charge Code as it is already part of the Charge Group specified, and therefore already included."));
					}
				}

				ValidatedIsExcluded();
				ValidateChargeExistsInOneLandedCostingGroupOnly();

				IsValidatingChargeCodePK = false;
			}
		}

		ZGuid chargeCodePK;
		bool IsValidatingChargeCodePK;

		bool IsChargeCodePkSet
		{
			get { return ChargeCodeCodeForDefaultValue.IsEmpty; }
			set { ChargeCodeCodeForDefaultValue = ""; }
		}

		#endregion

		#region ChargeCode Description

		public ZString ChargeCodeDescription
		{
			get
			{
				ZString result = "";

				if (ChargeCode != null)
				{
					result = (ZString)ChargeCode[AccChargeCodeSchema.Constants.AC_Desc];
				}

				return result;
			}
		}

		public ZPropertyInfo ChargeCodeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.ChargeCodeDescription); }
		}

		#endregion

		#region Is Excluded

		public ZBool IsExcluded
		{
			get { return isExcluded; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(IsExcludedInfo, ref isExcluded, value);
				if (!IsValidationSuspended)
				{
					ValidatedIsExcluded();
				}
			}
		}

		public ZPropertyInfo IsExcludedInfo
		{
			get { return GetZPropertyInfo(Schema.IsExcluded); }
		}

		public void ValidatedIsExcluded()
		{
			IsExcludedInfo.ClearAllNotifications();

			if (IsExcluded && (ChargeGroupCode.IsEmpty || ChargeCodePK.IsEmpty))
			{
				IsExcludedInfo.AddError(Res.GetString("53e35ebf-cfc6-4d8e-b1cd-b7a43fe33d2c", "You cannot exclude a Charge unless you specify a Charge Group and a Charge Code."));
			}
			ValidateChargeCodePK();
		}

		ZBool isExcluded;

		#endregion

		#endregion

		#region List Properties

		#region ChargeGroup List

		public CodeDescriptionPairList ChargeGroupList
		{
			get
			{
				if (fChargeGroupList == null)
				{
					object orgLandedCostingPrefChargesLookups = Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IOrgLandedCostingPrefChargesLookups>(), new object[] { null });

					PropertyInfo info = ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IOrgLandedCostingPrefChargesLookups>().GetProperty("IncoTermChargeGroups", BindingFlags.Public | BindingFlags.Instance);
					fChargeGroupList = (CodeDescriptionPairList)info.GetValue(orgLandedCostingPrefChargesLookups, null);
				}

				return fChargeGroupList;
			}
		}

		CodeDescriptionPairList fChargeGroupList;

		#endregion

		#region ChargeCode List

		public BusinessObjectCollection ChargeCodeList
		{
			get
			{
				if (fChargeCodeList == null || HasFallbackLevelChanged)
				{
					if (CurrentFallbackLevel != null)
					{
						fChargeCodeList = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IAccChargeCodeCollection>(), new object[] { CurrentFactory, new ZQuery(), CurrentFallbackLevel.CompanyPK(false) });
					}
					else
					{
						fChargeCodeList = (BusinessObjectCollection)Activator.CreateInstance(ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IAccChargeCodeCollection>(), new object[] { CurrentFactory });
					}
				}

				return fChargeCodeList;
			}
		}

		bool HasFallbackLevelChanged
		{
			get
			{
				bool result = false;

				if (CurrentChargeCodeListFallbackLevel != CurrentFallbackLevel)
				{
					result = true;
					CurrentChargeCodeListFallbackLevel = CurrentFallbackLevel;
				}

				return result;
			}
		}

		FallbackLevel CurrentChargeCodeListFallbackLevel;
		BusinessObjectCollection fChargeCodeList;

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();

			ValidateChargeGroupCode();
			ValidateChargeCodePK();
			ValidatedIsExcluded();
		}

		void ValidateChargeExistsInOneLandedCostingGroupOnly()
		{
			ClearRowNotifications();

			if (ParentLandedCostingGroupCollection != null)
			{
				LandedCostingGroup landedCostingGroup = null;

				if (!ChargeGroupCode.IsEmpty)
				{
					landedCostingGroup = ParentLandedCostingGroupCollection.GetLandedCostingGroupFromChargeGroup(ChargeGroupCode, ParentLandedCostingGroup);
				}

				if (landedCostingGroup == null && ChargeCode != null && !IsExcluded)
				{
					landedCostingGroup = ParentLandedCostingGroupCollection.GetLandedCostingGroupFromChargeCode(ChargeCode, ParentLandedCostingGroup);
				}

				if (landedCostingGroup != null)
				{
					AddRowError(Res.GetString("417a4730-e22a-4219-8416-9c97eda9642a", "This Charge Group/Code is already included in Landed Costing Group {0} ({1}).", landedCostingGroup.GroupID, landedCostingGroup.GroupName));
				}
			}
		}

		LandedCostingGroup ParentLandedCostingGroup
		{
			get
			{
				LandedCostingGroup result = null;

				if (GetParentCollection(this, typeof(ChargeGroupAndChargeCodeCollection)) != null)
				{
					result = ((ChargeGroupAndChargeCodeCollection)ParentCollections.First()).ParentLandedCostingGroup;
				}

				return result;
			}
		}

		LandedCostingGroupCollection ParentLandedCostingGroupCollection
		{
			get
			{
				LandedCostingGroupCollection result = null;

				if (ParentLandedCostingGroup != null && GetParentCollection(ParentLandedCostingGroup, typeof(LandedCostingGroupCollection)) != null)
				{
					result = (LandedCostingGroupCollection)ParentLandedCostingGroup.ParentCollections.First();
				}

				return result;
			}
		}

		#endregion

		#region ChargeCode BusinessObject

		BusinessObject ChargeCode
		{
			get
			{
				if (fChargeCode == null || fChargeCode.PK != ChargeCodePK)
				{
					fChargeCode = (BusinessObject)CurrentFactory.Load<Enterprise.MasterFiles.Integration.IAccChargeCode>(ChargeCodePK);
				}

				return fChargeCode;
			}
		}

		BusinessObject fChargeCode;

		#endregion

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(Schema.ChargeGroupCode, ChargeGroupCode);
			writer.WriteElementString(Schema.ChargeCodePK, ChargeCodePK.ToString());
			writer.WriteElementString(Schema.IsExcluded, IsExcluded.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			ChargeGroupCode = reader.ReadElementString(Schema.ChargeGroupCode);
			ChargeCodePK = new ZGuid(reader.ReadElementString(Schema.ChargeCodePK));
			IsExcluded = new ZBool(reader.ReadElementString(Schema.IsExcluded));
		}

		#endregion

		internal ZString ChargeCodeCodeForDefaultValue;
	}
}
