using System.Data;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSeaManOBLDetail : BaseCusSeaManOBLDetail, ICusUnderbondDependentCollectionParent, IUnderbondMovementRequestHeaderProvider, IAUCusUnderbondUnionCollectionParent, Integration.Customs.AU.ICusSeaManOBLDetail
	{
		public CusSeaManOBLDetail(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Implementation

		protected override Customs.Business.CusSeaManOBLDetailValidation GetNewValidation()
		{
			return new CusSeaManOBLDetailValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BD_CargoVolumeUM = CMRQuantityUnits.Codes.CubicMetre;
		}

		CMRContainerUtilities ContainerUtilities
		{
			get
			{
				if (fContainerUtilities == null)
				{
					fContainerUtilities = new CMRContainerUtilities();
				}
				return fContainerUtilities;
			}
		}
		CMRContainerUtilities fContainerUtilities;

		bool HasContainerType
		{
			get
			{
				return (ContainerType != null);
			}
		}

		#endregion

		#region Overrides

		#region ContainerNumber

		public override ZString BD_ContainerNumber
		{
			get { return base.BD_ContainerNumber; }
			set
			{
				ZString upperCaseValue = value.ToUpper();
				if (upperCaseValue == CMRImportCargoTypes.Descriptions.Bulk.ToUpper())
				{
					base.BD_LineCargoType = CMRImportCargoTypes.Codes.Bulk;
				}
				else if (upperCaseValue == CMRImportCargoTypes.Descriptions.BreakBulk.ToUpper())
				{
					base.BD_LineCargoType = CMRImportCargoTypes.Codes.BreakBulk;
				}
				base.BD_ContainerNumber = upperCaseValue;
			}
		}

		protected bool BD_ContainerNumber_ReadOnly
		{
			get { return IsBulk || IsBreakBulk; }
		}

		#endregion

		#region LineCargoType

		[List(nameof(Lookups) + "." + nameof(CusSeaManOBLDetailLookups.CargoTypes))]
		public override ZString BD_LineCargoType
		{
			get { return base.BD_LineCargoType; }
			set
			{
				bool wasBulkOrBreakBulk = IsBulk || IsBreakBulk;
				base.BD_LineCargoType = value;
				if (IsBulk || IsBreakBulk)
				{
					base.BD_ContainerNumber = Lookups.CargoTypes.GetDescriptionFromCode(value).ToUpper();
				}
				else if (wasBulkOrBreakBulk)
				{
					base.BD_ContainerNumber = ZString.Empty;
				}
			}
		}

		#endregion

		#region ContainerSize

		[List(nameof(Lookups) + "." + nameof(CusSeaManOBLDetailLookups.ContainerSizes))]
		[ReadOnlyMember(nameof(IsContainerTypeSpecified))]
		public override ZString BD_ContainerSizeOrISOCode
		{
			get
			{
				if (HasContainerType)
				{
					return ContainerUtilities.GetContainerSizeCode(ContainerType);
				}
				return base.BD_ContainerSizeOrISOCode;
			}
		}

		protected bool IsContainerTypeSpecified
		{
			get { return HasContainerType; }
		}

		#endregion

		#region Container Type

		[ReadOnlyMember(nameof(IsContainerTypeSpecified))]
		public override ZString BD_TypeOfContainer
		{
			get
			{
				if (HasContainerType)
				{
					return ContainerUtilities.GetContainerTypeCode(ContainerType);
				}
				return ContainerUtilities.GetContainerTypeMappingOrOriginal(base.BD_TypeOfContainer);
			}
			set
			{
				if (value.Length > CusSeaManOBLDetailSchema.BD_TypeOfContainer.MaxLength)
				{
					ZString mapped = ContainerUtilities.GetContainerTypeMappingCodeOrEmpty(value);
					if (!mapped.IsEmpty)
					{
						base.BD_TypeOfContainer = mapped;
					}
				}
				else
				{
					base.BD_TypeOfContainer = value;
				}
			}
		}

		#endregion

		#region RC_ContainerType

		[List(nameof(Lookups) + "." + nameof(CusSeaManOBLDetailLookups.ContainerTypes))]
		public override ZGuid BD_RC_ContainerType
		{
			get { return base.BD_RC_ContainerType; }
			set
			{
				base.BD_RC_ContainerType = value;

				if (HasContainerType)
				{
					BD_ContainerSizeOrISOCode = ZString.Empty;
					BD_TypeOfContainer = ZString.Empty;
				}
			}
		}

		#endregion

		#region BD_CargoVolume

		[DecimalPlaces(2)]
		public override ZDecimal BD_CargoVolume
		{
			get { return base.BD_CargoVolume; }
			set { base.BD_CargoVolume = value; }
		}

		#endregion

		#region BD_CargoVolumeUM

		[List(nameof(Lookups) + "." + nameof(CusSeaManOBLDetailLookups.QuantityUnits))]
		public override ZString BD_CargoVolumeUM { get => base.BD_CargoVolumeUM; set => base.BD_CargoVolumeUM = value; }

		#endregion

		#region BD_GrossWeight

		[DecimalPlaces(2)]
		public override ZDecimal BD_GrossWeight
		{
			get { return base.BD_GrossWeight; }
			set { base.BD_GrossWeight = value; }
		}

		#endregion

		#region BD_GrossWeightUM

		[List(nameof(Lookups) + "." + nameof(CusSeaManOBLDetailLookups.GrossWeightCodes))]
		public override ZString BD_GrossWeightUM { get => base.BD_GrossWeightUM; set => base.BD_GrossWeightUM = value; }

		#endregion

		#region BD_PackType

		[List(nameof(Lookups) + "." + nameof(CusSeaManOBLDetailLookups.PackageTypes))]
		public override ZString BD_PackType { get => base.BD_PackType; set => base.BD_PackType = value; }

		#endregion

		#endregion

		#region IUnderbondMovementRequestHeaderProvider Members

		IUnderbondMovementRequestHeader IUnderbondMovementRequestHeaderProvider.GetHeader(CusUnderbond underbond)
		{
			return new CusSeaManOBLDetailUnderbondMovementRequestHeader(underbond, this);
		}

		public bool IsBureau
		{
			get { return false; }
		}

		#endregion

		#region Properties

		#region IsCargoListLine

		public ZBool IsCargoListLine
		{
			get { return Header != null && Header.BO_HeaderCargoType == CMRImportCargoCodes.Codes.Import; }
		}

		#endregion

		#region IsFCL

		public ZBool IsFCL
		{
			get { return BD_LineCargoType == CMRImportCargoTypes.Codes.FullContainerLoadWithMultipleHouseBills; }
		}

		#endregion

		#region IsLCL

		public ZBool IsLCL
		{
			get { return BD_LineCargoType == CMRImportCargoTypes.Codes.LessThanContainerLoad; }
		}

		#endregion

		#region IsBreakBulk

		public ZBool IsBreakBulk
		{
			get { return BD_LineCargoType == CMRImportCargoTypes.Codes.BreakBulk; }
		}

		#endregion

		#region IsBulk

		public ZBool IsBulk
		{
			get { return BD_LineCargoType == CMRImportCargoTypes.Codes.Bulk; }
		}

		#endregion

		#endregion

		#region ICusUnderbondDependentCollectionParent

		Customs.Business.CusUnderbondCollection ICusUnderbondDependentCollectionParent.Underbonds
		{
			get
			{
				if (fUnderbonds == null)
				{
					fUnderbonds = new CusUnderbondCollection(this);
					fUnderbonds.Load();
					RegisterEditableChildObject(fUnderbonds);
				}
				return fUnderbonds;
			}
		}
		CusUnderbondCollection fUnderbonds;

		ZString IOutturnableLine.UnderbondHumanReadableName
		{
			get
			{
				StringBuilder builder = new StringBuilder();
				if (Header != null && !Header.BO_OceanBill.IsEmpty)
				{
					builder.Append(Header.BO_OceanBill);
					builder.Append(" - ");
				}

				if (IsBreakBulk)
				{
					builder.Append("Break Bulk");
				}
				else if (IsBulk)
				{
					builder.Append("Bulk");
				}
				else
				{
					builder.Append("Container");

					if (!BD_ContainerNumber.IsEmpty)
					{
						builder.Append(": ");
						builder.Append(BD_ContainerNumber);
					}
				}

				return builder.ToString();
			}
		}

		bool ICusUnderbondDependentCollectionParent.CanSendWithoutDelay
		{
			get
			{
				CMREdiMessageFunctions messageFunctions = new CMREdiMessageFunctions();
				return messageFunctions.DoMessagesContainAnyCARSTs(Header.Messages);
			}
		}

		ZString ICusUnderbondDependentCollectionParent.Details
		{
			get { return ZString.Empty; }
		}

		IOutturnableLine[] ICusUnderbondDependentCollectionParent.OutturnableLines
		{
			get { return System.Array.Empty<IOutturnableLine>(); }
		}

		bool ICusUnderbondDependentCollectionParent.UsesTranshipmentPortOnUnderbond
		{
			get { return false; }
		}

		ZString ICusUnderbondDependentCollectionParent.DefaultTranshipmentPort
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ICusUnderbondUnionCollectionParent Members

		Customs.Business.CusUnderbondUnionCollection ICusUnderbondUnionCollectionParent.AllUnderbonds => AllUnderbonds;

		public CusUnderbondUnionCollection AllUnderbonds
		{
			get
			{
				if (allUnderbonds == null)
				{
					allUnderbonds = new CusUnderbondUnionCollection(this);
					allUnderbonds.Load();
				}
				return allUnderbonds;
			}
		}
		CusUnderbondUnionCollection allUnderbonds;

		ZString IOutturnableLine.CargoStatus
		{
			get { return ZString.Empty; }
		}

		ZInt IOutturnableLine.PackagesManifested
		{
			get { return 0; }
		}

		public ICusUnderbondDependentCollectionParent[] GetAllPossibleCollectionProviders()
		{
			return new ICusUnderbondDependentCollectionParent[] { this };
		}

		bool ICusUnderbondUnionCollectionParent.IsForAirCargo
		{
			get { return false; }
		}

		#endregion
	}
}
