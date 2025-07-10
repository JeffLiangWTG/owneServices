using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.AFR.Business
{
	[SingleObjectAroundARow]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public class JPAFRContainer : AutoJPAFRContainer, ICanDelete, ISynchroniserReadOnlyMembersProvider, ISailingSynchronisationTarget<BillOfLadingContainer>
	{
		internal const string AutomaticSearchForDischargedContainerExclusionIdentifier = "A";

		public JPAFRContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : AutoJPAFRContainer.Schema
		{
			public const string JPC_SearchExclusionIdForCheckBox = "JPC_SearchExclusionIdForCheckBox";
		}

		#endregion

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			JPC_OwnershipCode = ContainerOwnershipCodeList.Codes.CarrierSupplied;
		}

		[RelatedBusinessObject("Bill")]
		public override ZGuid JPC_JPB_Bill
		{
			get { return base.JPC_JPB_Bill; }
			set { base.JPC_JPB_Bill = value; }
		}

		public JPAFRBills Bill
		{
			get { return Factory.Load<JPAFRBills>(JPC_JPB_Bill); }
		}

		#region ReadOnly
		public List<string> SynchroniserReadOnlyMembers { get { return synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string>()); } }
		List<string> synchroniserReadOnlyMembers;

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			return MetaData.GetReadOnlyExcludingMethodProvider(this, property) || SynchroniserReadOnlyMembers.Contains(property.Name);
		}
		#endregion

		#region Override Properties

		public override ZString JPC_ContainerNum
		{
			get { return base.JPC_ContainerNum; }
			set
			{
				var oldValue = JPC_ContainerNum;
				base.JPC_ContainerNum = value;
				if (!IsCopying && oldValue != JPC_ContainerNum)
				{
					DefaultDataFromMatchingContainer();
					if (ShouldChangeMatchedContainers(oldValue, JPC_ContainerNum))
					{
						UpdateMatchingContainers(oldValue, JPC_ContainerNumInfo);
					}
				}
			}
		}

		void DefaultDataFromMatchingContainer()
		{
			var bill = Bill;
			var header = bill == null ? null : bill.Header;
			if (header != null)
			{
				var matchedContainer = header.GetMatchingContainers(PK, JPC_ContainerNum).FirstOrDefault();
				if (matchedContainer != null)
				{
					using (header.SuspendUpdateMatchingContainers())
					{
						JPC_CCCApplicationId = matchedContainer.JPC_CCCApplicationId;
						JPC_IsEmpty = matchedContainer.JPC_IsEmpty;
						JPC_OperatorCode = matchedContainer.JPC_OperatorCode;
						JPC_OwnershipCode = matchedContainer.JPC_OwnershipCode;
						JPC_RC_ContainerType = matchedContainer.JPC_RC_ContainerType;
						JPC_Seal1 = matchedContainer.JPC_Seal1;
						JPC_Seal2 = matchedContainer.JPC_Seal2;
						JPC_SearchExclusionId = matchedContainer.JPC_SearchExclusionId;
						JPC_TypeOfService = matchedContainer.JPC_TypeOfService;
						JPC_VanningType = matchedContainer.JPC_VanningType;
					}
				}
			}
		}

		bool ShouldChangeMatchedContainers(ZString oldValue, ZString newValue)
		{
			return ShouldChangeMatchedContainersOverride != null && ShouldChangeMatchedContainersOverride(this, oldValue, newValue);
		}

		public Func<JPAFRContainer, ZString, ZString, bool> ShouldChangeMatchedContainersOverride;

		public override ZBool JPC_IsEmpty
		{
			get { return base.JPC_IsEmpty; }
			set
			{
				var oldValue = JPC_IsEmpty;
				base.JPC_IsEmpty = value;
				if (!IsCopying && oldValue != JPC_IsEmpty)
				{
					UpdateMatchingContainers(JPC_ContainerNum, JPC_IsEmptyInfo);
				}
			}
		}

		public override ZGuid JPC_RC_ContainerType
		{
			get { return base.JPC_RC_ContainerType; }
			set
			{
				var oldValue = JPC_RC_ContainerType;
				base.JPC_RC_ContainerType = value;
				if (!IsCopying && oldValue != JPC_RC_ContainerType)
				{
					UpdateMatchingContainers(JPC_ContainerNum, JPC_RC_ContainerTypeInfo);
				}
			}
		}

		public override ZString JPC_Seal1
		{
			get { return base.JPC_Seal1; }
			set
			{
				var oldValue = JPC_Seal1;
				base.JPC_Seal1 = value;
				if (!IsCopying && oldValue != JPC_Seal1)
				{
					UpdateMatchingContainers(JPC_ContainerNum, JPC_Seal1Info);
				}
			}
		}

		public override ZString JPC_Seal2
		{
			get { return base.JPC_Seal2; }
			set
			{
				var oldValue = JPC_Seal2;
				base.JPC_Seal2 = value;
				if (!IsCopying && oldValue != JPC_Seal2)
				{
					UpdateMatchingContainers(JPC_ContainerNum, JPC_Seal2Info);
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JPAFRContainerLookups.ContainerOwnershipCodeList))]
		public override ZString JPC_OwnershipCode
		{
			get { return base.JPC_OwnershipCode; }
			set
			{
				var oldValue = JPC_OwnershipCode;
				base.JPC_OwnershipCode = value;
				if (!IsCopying && oldValue != JPC_OwnershipCode)
				{
					UpdateMatchingContainers(JPC_ContainerNum, JPC_OwnershipCodeInfo);
				}
			}
		}

		#region VOCC Section

		[List(nameof(Lookups) + "." + nameof(JPAFRContainerLookups.CustomsConventionContainerApplicationList))]
		public override ZString JPC_CCCApplicationId
		{
			get { return base.JPC_CCCApplicationId; }
			set
			{
				var oldValue = JPC_CCCApplicationId;
				base.JPC_CCCApplicationId = value;
				if (!IsCopying && oldValue != JPC_CCCApplicationId)
				{
					UpdateMatchingContainers(JPC_ContainerNum, JPC_CCCApplicationIdInfo);
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JPAFRContainerLookups.ServiceTypeOnDeliveryCodeList))]
		public override ZString JPC_TypeOfService
		{
			get { return base.JPC_TypeOfService; }
			set
			{
				var oldValue = JPC_TypeOfService;
				base.JPC_TypeOfService = value;
				if (!IsCopying && oldValue != JPC_TypeOfService)
				{
					UpdateMatchingContainers(JPC_ContainerNum, JPC_TypeOfServiceInfo);
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JPAFRContainerLookups.VanningTypeCodeList))]
		public override ZString JPC_VanningType
		{
			get { return base.JPC_VanningType; }
			set
			{
				var oldValue = JPC_VanningType;
				base.JPC_VanningType = value;
				if (!IsCopying && oldValue != JPC_VanningType)
				{
					UpdateMatchingContainers(JPC_ContainerNum, JPC_VanningTypeInfo);
				}
			}
		}

		public override ZString JPC_SearchExclusionId
		{
			get { return base.JPC_SearchExclusionId; }
			set
			{
				var oldValue = JPC_SearchExclusionId;
				base.JPC_SearchExclusionId = value;
				if (!IsCopying && oldValue != JPC_SearchExclusionId)
				{
					UpdateMatchingContainers(JPC_ContainerNum, JPC_SearchExclusionIdInfo);
				}
			}
		}

		[ResourceStringData("JPAFRContainer|JPC_SearchExclusionIdForCheckBox", Caption = "Search for Discharge Container Exclusion", FullDescription = "Automatic Search for Discharged Container Exclusion Identifier")]
		public ZBool JPC_SearchExclusionIdForCheckBox
		{
			get { return this.JPC_SearchExclusionId == AutomaticSearchForDischargedContainerExclusionIdentifier; }
			set
			{
				var oldValue = JPC_SearchExclusionIdForCheckBox;
				this.JPC_SearchExclusionId = value ? AutomaticSearchForDischargedContainerExclusionIdentifier : string.Empty;
				if (!IsCopying && oldValue != JPC_SearchExclusionIdForCheckBox)
				{
					UpdateMatchingContainers(JPC_ContainerNum, JPC_SearchExclusionIdForCheckBoxInfo);
				}
			}
		}

		public ZWrappedPropertyInfo JPC_SearchExclusionIdForCheckBoxInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JPC_SearchExclusionIdForCheckBox, x => JPC_SearchExclusionIdInfo); }
		}

		public override ZString JPC_OperatorCode
		{
			get { return base.JPC_OperatorCode; }
			set
			{
				var oldValue = JPC_OperatorCode;
				base.JPC_OperatorCode = value;
				if (!IsCopying && oldValue != JPC_OperatorCode)
				{
					UpdateMatchingContainers(JPC_ContainerNum, JPC_OperatorCodeInfo);
				}
			}
		}

		#endregion

		#endregion

		#region ICanDelete Members

		bool ICanDelete.CanDelete
		{
			get { return !Bill.ShouldSynchronise; }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
			=> ResString.GetMultilingualString("JPAFRContainer|5D629F0D-4CBB-4352-994A-B2222DF655CC", "Container values are copied from the Consol. If you want to delete this record, please do it in the Consol, or you may tick 'Override Freight Defaults'.");

		#endregion

		void UpdateMatchingContainers(ZString containerNumberToMatch, ZPropertyInfo info)
		{
			var bill = Bill;
			var header = bill == null ? null : bill.Header;
			if (header != null)
			{
				header.UpdateMatchingContainers(PK, containerNumberToMatch, info);
			}
		}

		public bool IsBillShippingLineEntry
		{
			get
			{
				return (bool)(isBillShippingLineEntry ?? (isBillShippingLineEntry = Bill != null && Bill.IsShippingLineEntry));
			}
		}
		bool? isBillShippingLineEntry;

		#region ISailingSynchronisationTarget<BillOfLadingContainer> Members

		bool ISailingSynchronisationTarget<BillOfLadingContainer>.IsMatched(BillOfLadingContainer sailingTarget)
		{
			return IsMatched(sailingTarget);
		}

		bool IsMatched(BillOfLadingContainer sailingContainer)
		{
			var sailingBill = ((ISailingSynchronisationTarget<BillOfLading>)Bill).Source;
			return IsMatched(sailingContainer, sailingBill);
		}

		bool IsMatched(BillOfLadingContainer sailingContainer, BillOfLading sailingBill)
		{
			return sailingBill != null && sailingBill.IsContainerised && sailingBill.RealContainers.Contains(sailingContainer) && this.JPC_ContainerNum == sailingContainer.JC_ContainerNum;
		}

		void ISailingSynchronisationTarget<BillOfLadingContainer>.Set(BillOfLadingContainer sailingTarget)
		{
			using (this.GetValidationSuspender())
			{
				this.JPC_ContainerNum = sailingTarget.JC_ContainerNum;
			}
		}

		BillOfLadingContainer ISailingSynchronisationTarget<BillOfLadingContainer>.Source
		{
			get { return SailingSynchronisationSource; }
		}

		BillOfLadingContainer SailingSynchronisationSource
		{
			get
			{
				if (fSailingSynchronisationSource != null && IsMatched(fSailingSynchronisationSource))
				{
					return fSailingSynchronisationSource;
				}
				fSailingSynchronisationSource = null;
				var sailingBill = ((ISailingSynchronisationTarget<BillOfLading>)Bill).Source;
				if (sailingBill != null)
				{
					fSailingSynchronisationSource = sailingBill.RealContainers.Cast<BillOfLadingContainer>().FirstOrDefault(x => IsMatched(x, sailingBill));
				}
				return fSailingSynchronisationSource;
			}
		}
		BillOfLadingContainer fSailingSynchronisationSource;

		void ISailingSynchronisationTarget<BillOfLadingContainer>.Synchronise()
		{
			var sailingContainer = SailingSynchronisationSource;
			if (sailingContainer != null)
			{
				using (this.GetValidationSuspender())
				{
					this.JPC_Seal1 = sailingContainer.JC_SealNum.Substring(0, JPC_Seal1Info.MaxLength);
					this.JPC_Seal2 = sailingContainer.JC_AdditionalSealNum.Substring(0, JPC_Seal2Info.MaxLength);
					if (string.IsNullOrWhiteSpace(this.JPC_Seal1) && string.IsNullOrWhiteSpace(this.JPC_Seal2))
					{
						this.JPC_Seal1 = Constants.ContainerNoSeal;
					}
					this.JPC_RC_ContainerType = sailingContainer.JC_RC;
					this.JPC_IsEmpty = sailingContainer.JC_IsEmptyContainer;
					this.JPC_OwnershipCode = sailingContainer.JC_IsShipperOwned ? new ZString(ContainerOwnershipCodeList.Codes.ShipperSupplied) : this.JPC_OwnershipCode;
				}
			}
		}

		#endregion
	}
}
