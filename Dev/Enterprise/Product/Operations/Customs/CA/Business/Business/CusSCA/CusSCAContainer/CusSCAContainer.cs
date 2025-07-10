using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.Business
{
	public class CusSCAContainer : BaseCusSCAContainer, ISCRContainer
	{
		public CusSCAContainer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public static new readonly TypeDecider TypeDecider = new CusSCAContainerTypeDecider();

		internal ForwardingContainer HookedContainer { get; set; }

		[ChildEditable()]
		public CusSCAPivotCollectionForContainer AssociatedPackLines
		{
			get
			{
				if (pivots == null)
				{
					pivots = new CusSCAPivotCollectionForContainer(this);
					RegisterEditableChildObject(pivots);
				}
				return pivots;
			}
		}
		CusSCAPivotCollectionForContainer pivots;

		[RelatedBusinessObject("OceanBill")]
		public override ZGuid CN_CB
		{
			get => base.CN_CB;
			set => base.CN_CB = value;
		}

		public CusSCAOceanBill OceanBill
		{
			get
			{
				if (fOceanBill == null)
				{
					fOceanBill = Factory.Load<CusSCAOceanBill>(CN_CB);
				}

				return fOceanBill;
			}
		}
		CusSCAOceanBill fOceanBill;

		public override void Delete()
		{
			foreach (CusSCAPivot pivot in AssociatedPackLines.ToArray())
			{
				pivot.Delete();
			}
			base.Delete();
		}

		[ReadOnlyMember(nameof(IsNonContainerised))]
		public override ZString CN_RC_NKContainerType
		{
			get { return base.CN_RC_NKContainerType; }
			set
			{
				base.CN_RC_NKContainerType = value;
				if (ContainerType != null && CN_ContainerSizeOrISOCode.IsEmpty)
				{
					CN_ContainerSizeOrISOCode = ContainerType.RC_ISOType;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusSCAContainerLookups.SCRContainerModes))]
		[ReadOnlyMember(nameof(IsNonContainerised))]
		public override ZString CN_ContainerMode
		{
			get { return base.CN_ContainerMode; }
			set
			{
				var oldValue = CN_ContainerMode;
				base.CN_ContainerMode = value;
				if (!IsCopying && oldValue != CN_ContainerMode)
				{
					AssociatedPackLines.MarkAsNeedingValidation();
				}
			}
		}

		[ReadOnlyMember(nameof(IsNonContainerised))]
		public override ZString CN_RN_NKCountryOfRegistration
		{
			get { return base.CN_RN_NKCountryOfRegistration; }
			set { base.CN_RN_NKCountryOfRegistration = value; }
		}

		[ReadOnlyMember(nameof(IsNonContainerised))]
		public override ZString CN_ContainerSizeOrISOCode
		{
			get { return base.CN_ContainerSizeOrISOCode; }
			set { base.CN_ContainerSizeOrISOCode = value; }
		}

		[ReadOnlyMember(nameof(IsNonContainerised))]
		public override ZString CN_ContainerNumber
		{
			get { return base.CN_ContainerNumber; }
			set
			{
				var oldValue = CN_ContainerNumber;
				base.CN_ContainerNumber = value;
				if (!IsCopying && oldValue != CN_ContainerNumber)
				{
					AssociatedPackLines.MarkAsNeedingValidation();
				}
			}
		}

		public bool IsNonContainerised
		{
			get { return this.CN_TypeOfContainer == CusSCAHouse.NonContaineriseID; }
		}

		public new CusSCAContainerLookups Lookups
		{
			get { return (CusSCAContainerLookups)base.Lookups; }
		}

		protected override Customs.Business.CusSCAContainerLookups GetNewLookups()
		{
			return new CusSCAContainerLookups(this);
		}

		protected override Customs.Business.CusSCAContainerValidation GetNewValidation()
		{
			return new CusSCAContainerValidation(this);
		}

		#region ISCRContainer members
		ZString ISCRContainer.ContainerNumber
		{
			get { return CN_ContainerNumber; }
		}

		ZString ISCRContainer.CountryOfRegistration
		{
			get { return CN_RN_NKCountryOfRegistration; }
		}

		ZString ISCRContainer.ContainerSizeCode
		{
			get { return CN_ContainerSizeOrISOCode; }
		}

		ZBool ISCRContainer.IsEmpty
		{
			get { return CN_ContainerMode == Core.Constants.ContainerModes.Empty; }
		}

		#endregion

	}
}
