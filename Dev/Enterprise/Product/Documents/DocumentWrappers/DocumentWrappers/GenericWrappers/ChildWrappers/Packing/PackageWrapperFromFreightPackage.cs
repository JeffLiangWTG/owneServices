using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PackageWrapperFromFreightPackage : PackageWrapper
	{
		public PackageWrapperFromFreightPackage(PackLine packageBO, BusinessObjectFactory factory)
			: base(packageBO, factory)
		{
			PackageBO = packageBO ?? factory.GetNull<PackLine>();

			if (PackageBO.CurrentConsol != null)
			{
				SetParentConsol(PackageBO.CurrentConsol);
			}
		}
		readonly PackLine PackageBO;

		protected override ZString GetDescription()
		{
			var result = PackageBO.JL_DetailedDescription != ZString.Empty ? PackageBO.JL_DetailedDescription : PackageBO.JL_Description;

			if (result.IsEmpty && ParentShipment != null)
			{
				result = ParentShipment.JS_GoodsDescription;
			}

			return result;
		}

		protected override PackageWrapperCollection GetNewInnerPackages()
		{
			var result = new PackageWrapperCollection(Factory);

			if (PackageBO.JL_FreightMode == FreightConstants.OuterPackType && PackageBO.Shipment?.InnerPackLines != null)
			{
				foreach (var innerPackLine in PackageBO.Shipment.InnerPackLines.OfType<ForwardingPackLine>().Where(line => line.JL_JL_OuterPackLine == PackageBO.PK))
				{
					result.Add(new PackageWrapperFromFreightPackage(innerPackLine, Factory));
				}
			}

			return result;
		}

		protected override ZString GetPackLineId() => PackageBO.JL_PackLineId;

		protected override UNDGSubstanceWrapperCollection GetUNDGSubstances()
		{
			UNDGSubstanceWrapperCollection result = new UNDGSubstanceWrapperCollection(Factory);
			foreach (UNDGDataItem dGDataItem in PackageBO.UNDGs)
			{
				if (dGDataItem.Substance != null || !dGDataItem.DI_IMOClass.IsEmpty)
				{
					UNDGSubstanceWrapper wrapper = new UNDGSubstanceWrapper(dGDataItem, Factory);
					wrapper.ContainingPackage = this;
					result.Add(wrapper);
				}
			}

			return result;
		}

		protected override PackQTYWrapper GetPackages()
		{
			return new PackQTYWrapper(PackageBO.JL_PackageCount, PackageBO.JL_F3_NKPackType, PackageBO.JL_F3_NKPackType_List.GetAsCodeDescriptionPair(), Factory);
		}

		protected override PackQTYWrapper GetOutturnedPackages()
		{
			return new PackQTYWrapper(PackageBO.JL_Outturn, PackageBO.JL_F3_NKPackType, PackageBO.JL_F3_NKPackType_List.GetAsCodeDescriptionPair(), Factory);
		}

		protected override PackQTYWrapper GetPillagedPackages()
		{
			return new PackQTYWrapper(PackageBO.JL_Pillaged, PackageBO.JL_F3_NKPackType, PackageBO.JL_F3_NKPackType_List.GetAsCodeDescriptionPair(), Factory);
		}

		protected override PackQTYWrapper GetDamagedPackages()
		{
			return new PackQTYWrapper(PackageBO.JL_Damaged, PackageBO.JL_F3_NKPackType, PackageBO.JL_F3_NKPackType_List.GetAsCodeDescriptionPair(), Factory);
		}

		protected override WeightWrapper GetWeight()
		{
			int decimals = (int)MetaData.GetMetaData(PackageBO, PackageBO.JL_ActualWeightInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
			return new WeightWrapper(PackageBO.JL_ActualWeight, PackageBO.PackLineWeightUnit, decimals, PackageBO.JL_ActualWeightUQ_List, Factory);
		}

		protected override WeightWrapper GetOutturnedWeight()
		{
			int decimals = (int)MetaData.GetMetaData(PackageBO, PackageBO.JL_OutturnedWeightInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
			return new WeightWrapper(PackageBO.JL_OutturnedWeight, PackageBO.PackLineWeightUnit, decimals, PackageBO.JL_ActualWeightUQ_List, Factory);
		}

		protected override VolumeWrapper GetVolume()
		{
			int decimals = (int)MetaData.GetMetaData(PackageBO, PackageBO.JL_ActualVolumeInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
			return new VolumeWrapper(PackageBO.JL_ActualVolume, PackageBO.PackLineVolumeUnit, decimals, PackageBO.JL_ActualVolumeUQ_List, Factory);
		}

		protected override VolumeWrapper GetOutturnedVolume()
		{
			int decimals = (int)MetaData.GetMetaData(PackageBO, PackageBO.JL_OutturnedVolumeInfo.PropertyDescriptor, MetaDataTypes.DecimalPlaces, false);
			return new VolumeWrapper(PackageBO.JL_OutturnedVolume, PackageBO.PackLineVolumeUnit, decimals, PackageBO.JL_ActualVolumeUQ_List, Factory);
		}

		protected override DimensionsWrapper GetDimensions()
		{
			return new DimensionsWrapper(PackageBO.JL_Length, PackageBO.JL_Width, PackageBO.JL_Height, PackageBO.JL_UnitOfDimension, PackageBO.JL_UnitOfDimension_List, Factory);
		}

		protected override ContainerWrapper GetContainer()
		{
			CommonContainer container = PackageBO.GetContainer(ParentConsol);
			return new ContainerWrapperFromFreight(container, Factory);
		}

		protected override PackLine GetFreightPackLine()
		{
			return PackageBO;
		}

		protected override ZInt GetPackingOrder()
		{
			return PackageBO.JL_ContainerPackingOrder;
		}

		protected override ZString GetMarksAndNumbers()
		{
			return PackageBO.JL_MarksAndNumbers.IsEmpty ? ParentShipment == null ? ZString.Empty : ParentShipment.JS_MarksAndNumbers : PackageBO.JL_MarksAndNumbers;
		}

		protected override ZString GetContainerNo()
		{
			var container = GetPackLineContainer();
			return container != null ? container.JC_ContainerNum : PackageBO.JL_Calc_ContainerNum;
		}

		protected override ZString GetContainerJobID()
		{
			var container = GetPackLineContainer();
			return (container != null && !container.JC_ContainerJobID.IsEmpty) ? container.JC_ContainerJobID : PackageBO.JL_Calc_ContainerNum;
		}

		CommonContainer GetPackLineContainer()
		{
			return PackageBO.GetContainer(ParentConsol);
		}

		protected override ZString GetHouseBill()
		{
			return ParentShipment != null ? ParentShipment.JS_HouseBill : ZString.Empty;
		}

		#region Parent Consol

		internal void SetParentConsol(CommonConsol consol)
		{
			parentConsol = consol;
		}

		CommonConsol ParentConsol
		{
			get { return parentConsol ?? (parentConsol = GetParentConsolFromShipment()); }
		}
		CommonConsol parentConsol;

		CommonConsol GetParentConsolFromShipment()
		{
			CommonConsol consol = null;

			if (PackageBO != null && PackageBO.Shipment != null)
			{
				var shipment = PackageBO.Shipment as ForwardingShipment;
				if (shipment != null)
				{
					consol = shipment.CurrentConsolForDocuments;
				}

				if (consol == null)
				{
					consol = DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV)
						? PackageBO.Shipment.ArrivalConsolForDocuments
						: PackageBO.Shipment.DepartureConsolForDocuments;
				}
			}

			return consol;
		}

		#endregion

		protected override ZString GetMasterBill()
		{
			if (ParentConsol != null)
			{
				return ParentConsol.JK_MasterBillNum;
			}

			ZString result = ZString.Empty;
			ForwardingShipment shipment = (ForwardingShipment)ParentShipment;
			if (shipment != null)
			{
				ForwardingConsol consol = shipment.Consols.GetEarliestConsol();
				if (consol != null)
				{
					result = consol.JK_MasterBillNum;
				}
			}
			return result;
		}

		protected override ZString GetOutturnComment()
		{
			return PackageBO.JL_OutturnComment;
		}

		protected override CodeAndDescriptionWrapper GetCommodity()
		{
			return new CodeAndDescriptionWrapper(PackageBO.JL_RH_NKCommodityCode, PackageBO.Lookups.CommodityCodes, Factory);
		}

		protected override CodeAndDescriptionWrapper GetDamagedReason()
		{
			return CodeAndDescriptionWrapper.Empty;
		}

		protected override ZString GetRefNumber()
		{
			return PackageBO.JL_RefNumber;
		}

		protected override ZString GetExportRefNumber()
		{
			return PackageBO.JL_ExportRefNumber;
		}

		protected override ZString GetImportRefNumber()
		{
			return PackageBO.JL_ImportRefNumber;
		}

		protected override ZString GetPackageReferenceHeaderText()
		{
			var result = Res.GetString("05e9c7d3-9f30-436d-8519-8baea8036757", "EXPORT REF NUMBER");

			if (ParentShipment != null)
			{
				if (ParentShipment.IsImport())
				{
					result = Res.GetString("c7ce2d2d-daa7-4041-9ee8-7705c059eed5", "IMPORT REF NUMBER");
				}
				else
				{
					var countryCodes = new ZString[] { Core.Constants.CountryCodes.China, Core.Constants.CountryCodes.Taiwan, Core.Constants.CountryCodes.HongKong };
					if (countryCodes.Contains(GlbCompany.CurrentCompany.Country.Code) && countryCodes.Contains(ParentShipment.JS_RL_NKOrigin.SubstringSafe(0, 2)))
					{
						result = Res.GetString("d8404672-06fe-4566-99b8-90fed8865082", "SHIPPING ORDER/SHI LIAN DAN");
					}
				}
			}
			return result;
		}

		protected override FreightWrapper GetParent()
		{
			FreightWrapper[] parents = FreightWrapper.New(ParentShipment, Factory);
			var parent = (parents.Length > 0) ? parents[0] : null;

			var iPackLineOverride = parent as IPackLineOverrider;
			if (iPackLineOverride != null)
			{
				iPackLineOverride.SetPackageOverride(PackageBO);
			}

			return parent;
		}

		internal void SetParentShipment(CommonShipment shipment)
		{
			this.shipment = shipment;
		}
		CommonShipment shipment;

		CommonShipment ParentShipment
		{
			get { return shipment ?? (shipment = PackageBO.Shipment); }
		}

		protected override ZDecimal GetLinePrice()
		{
			return PackageBO.JL_LinePrice;
		}

		protected override ZShort GetItemNumber()
		{
			return PackageBO.JL_ItemNo;
		}

		protected override ZString GetHarmonizedCode()
		{
			var codes = new List<ZString>();

			if (ParentShipment != null)
			{
				if (DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.DEP))
				{
					codes.AddRange(PackageBO.HarmonisedCodes.Where(x => x.JLH_RN_NKCountry == ParentShipment.JS_RL_NKOrigin.SubstringSafe(0, 2)).Select(x => x.JLH_Code));
				}
				else if (DocumentDirection == nameof(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV))
				{
					codes.AddRange(PackageBO.HarmonisedCodes.Where(x => x.JLH_RN_NKCountry == ParentShipment.JS_RL_NKDestination.SubstringSafe(0, 2)).Select(x => x.JLH_Code));
				}
				else
				{
					if (!PackageBO.JL_HarmonisedCode.IsEmpty)
					{
						codes.Add(PackageBO.JL_HarmonisedCode);
					}
					else if (PackageBO.HarmonisedCodes.Any(x => x.JLH_RN_NKCountry == ParentShipment.JS_RL_NKOrigin))
					{
						codes.AddRange(PackageBO.HarmonisedCodes.Where(x => x.JLH_RN_NKCountry == ParentShipment.JS_RL_NKOrigin.SubstringSafe(0, 2)).Select(x => x.JLH_Code));
					}
					else
					{
						codes.AddRange(PackageBO.HarmonisedCodes.Where(x => x.JLH_RN_NKCountry == ParentShipment.JS_RL_NKDestination.SubstringSafe(0, 2)).Select(x => x.JLH_Code));
					}
				}
			}

			if (!codes.Any() && !PackageBO.JL_HarmonisedCode.IsEmpty)
			{
				codes.Add(PackageBO.JL_HarmonisedCode);
			}

			return new ZStringBuilder(codes).ToStringWithDelimiterBetweenAppends(", ");
		}

		protected override HarmonisedCodeWrapperCollection GetHarmonizedCodes()
		{
			return new HarmonisedCodeWrapperCollection(PackageBO.HarmonisedCodes, Factory);
		}

		protected override LocationWrapper GetOrigin()
		{
			return new LocationWrapper(PackageBO.JL_RN_NKOrigin, Factory);
		}

		protected override PackProductWrapperCollection GetProducts()
		{
			var forwardingPackage = PackageBO as ForwardingPackLine;

			return (forwardingPackage != null) ? new PackProductWrapperCollection(forwardingPackage, Factory) : PackProductWrapperCollection.Empty;
		}

		protected override ZString GetCustomAttribute1()
		{
			return PackageBO.JL_CustomAttrib1;
		}

		protected override ZString GetCustomAttribute2()
		{
			return PackageBO.JL_CustomAttrib2;
		}

		protected override ZString GetCustomAttribute3()
		{
			return PackageBO.JL_CustomAttrib3;
		}

		protected override ZString GetCustomAttribute4()
		{
			return PackageBO.JL_CustomAttrib4;
		}

		protected override ZDateTime GetCustomDate1()
		{
			return PackageBO.JL_CustomDate1;
		}

		protected override ZDateTime GetCustomDate2()
		{
			return PackageBO.JL_CustomDate2;
		}

		protected override ZDecimal GetCustomDecimal1()
		{
			return PackageBO.JL_CustomDecimal1;
		}

		protected override ZDecimal GetCustomDecimal2()
		{
			return PackageBO.JL_CustomDecimal2;
		}

		protected override ZBool GetCustomFlag1()
		{
			return PackageBO.JL_CustomFlag1;
		}

		protected override ZBool GetCustomFlag2()
		{
			return PackageBO.JL_CustomFlag2;
		}

		protected override ZString GetCartonGroupAndSize()
		{
			return ZString.Empty;
		}

		protected override ZString GetIndent()
		{
			return "";
		}

		protected override ZString GetDisplayOrder()
		{
			return "";
		}

		protected override ZBool GetIsTopLevelPackage()
		{
			return true;
		}

		protected override ZBool GetIsTopLevelNonContainerisedPackage()
		{
			return true;
		}

		protected override ZBool GetIsOuterPackage()
		{
			return false;
		}

		protected override ZBool GetIsExclusive()
		{
			return false;
		}

		protected override ZBool GetHasSingleProduct()
		{
			return ZBool.False;
		}

		protected override ZBool GetHasPackedItem()
		{
			return PackedItem != null;
		}

		protected override PackedItemWrapper GetPackedItem()
		{
			return null;
		}

		protected override PackedItemWrapperCollection GetPackedItems()
		{
			return new PackedItemWrapperCollection(Factory);
		}

		protected override ZShort GetOutterPackageSequence()
		{
			return ZShort.Zero;
		}

		protected override ZShort GetOutterPackagesCount()
		{
			return ZShort.Zero;
		}

		protected override ZShort GetInnerPackagesCount()
		{
			return ZShort.Zero;
		}

		protected override ZString GetPostcodeBarcodeNumber()
		{
			return ZString.Empty;
		}

		protected override ZString GetStarTrack_QRCodeText()
		{
			return ZString.Empty;
		}

		protected override ZInt GetInners()
		{
			return ZInt.Zero;
		}

		protected override ZString GetInnersDetail()
		{
			return ZString.Empty;
		}

		protected override ZInt GetPackedItemCount()
		{
			return ZInt.Zero;
		}

		protected override ZBool GetIsExpiryUsed()
		{
			return ZBool.False;
		}

		protected override ZBool GetIsPackingDateUsed()
		{
			return ZBool.False;
		}

		protected override ZBool GetIsPartAttrib1Used()
		{
			return ZBool.False;
		}

		protected override ZBool GetIsPartAttrib2Used()
		{
			return ZBool.False;
		}

		protected override ZBool GetIsPartAttrib3Used()
		{
			return ZBool.False;
		}

		protected override ZBool GetIsTrackedSerialUsed()
		{
			return ZBool.False;
		}

		protected override PackageAuditWrapper GetMostRecentAudit()
		{
			return null;
		}

		#region PackageState

		protected override PackageStateWrapper GetPackageState()
		{
			return null;
		}

		#endregion

		protected override TextBarcode DocManagerBarCodeWithUniqueID
		{
			get
			{
				const bool USE_OPTIMISED_ENCODING = true; // shortens barcode
				return GlbCompany.CurrentCompany != null && ParentShipment != null
					? new TextBarcode(string.Concat(GlbCompany.CurrentCompany.LicenceEnterpriseCode,
						GlbCompany.CurrentCompany.LicenceServerID,
						ParentShipment.JS_UniqueConsignRef,
						"-",
						PackageNumber.ToString().PadLeft(5, '0')), USE_OPTIMISED_ENCODING)
					: new TextBarcode(ZString.Empty);
			}
		}
	}
}
