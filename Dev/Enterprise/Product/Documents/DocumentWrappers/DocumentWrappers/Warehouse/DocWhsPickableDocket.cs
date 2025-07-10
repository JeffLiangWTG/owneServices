using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocWhsPickableDocket : DocWhsDocket, IDocTypeCode, ILoadingSupport
	{
		#region Constructors

		protected DocWhsPickableDocket(WhsPickableDocket order, BusinessObjectFactory factoryToWrap)
			: base(order, factoryToWrap)
		{
		}

		protected DocWhsPickableDocket(WhsDocketLabelControl docketLabel, BusinessObjectFactory factoryToWrap)
			: base(docketLabel, factoryToWrap)
		{
		}

		public static DocWhsPickableDocket New(WhsPickableDocket order, BusinessObjectFactory factoryToWrap)
		{
			DocWhsPickableDocket result = null;
			var overridden = OverridableNewDelegate.Value;
			if (overridden != null)
			{
				result = overridden(order, factoryToWrap);
			}
			else if (order != null)
			{
				result = new DocWhsPickableDocket(order, factoryToWrap);
			}
			return result;
		}

		public static DocWhsPickableDocket New(WhsDocketLabelControl docketLabel, BusinessObjectFactory factoryToWrap)
		{
			return (docketLabel == null) ? null : ((docketLabel.Docket == null) ? null : new DocWhsPickableDocket(docketLabel, factoryToWrap));
		}

		#endregion

		#region Customs Stuff

		public ZString WarehouseCCPCode
		{
			get
			{
				ZString result = "";
				if (WhsOrder.Warehouse != null && WhsOrder.Warehouse.WarehouseAddress != null)
				{
					OrgHeader org = Factory.Load<OrgHeader>(WhsOrder.Warehouse.WarehouseAddress.OA_OH);
					if (org != null)
					{
						result = org.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID, GlbCompany.CurrentCompany.Country);
					}
				}
				return result;
			}
		}

		public ZString ACSEstCode
		{
			get
			{
				ZString result = "";
				OrgHeader org = WhsOrder.Consignee;
				if (org != null)
				{
					result = org.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID, GlbCompany.CurrentCompany.Country);
				}
				return result;
			}
		}

		public ZString ATOEstCode
		{
			get { return WarehouseCCPCode; }
		}

		public ZString ClientCPC
		{
			get { return WhsOrder.GetClientCode(OrgCusCode.CodeTypes.CustomsCPPermitCode); }
		}

		public ZString ClientGCR
		{
			get { return WhsOrder.GetClientCode(OrgCusCode.CodeTypes.CorporationCode); }
		}

		public ZString CP_IssueNo
		{
			get { return ClientCPC + WhsOrder.WD_DocketID.Right(8); }
		}

		#endregion

		#region Related Business Objects

		#region Consignee

		protected override DocOrganisation ConsigneeCore => DocOrganisation.New(WhsOrder.Consignee, Factory);

		protected override DocDocAddress ConsigneeAddressCore => DocDocAddress.New(WhsOrder.ConsigneeDocAddress, Factory);

		protected override DocDocAddress ConsigneeNameAndAddressCore => ConsigneeAddress;

		protected override ZString ConsigneeNameCore
		{
			get
			{
				var consigneeDocAddress = WhsOrder.ConsigneeDocAddress;
				return consigneeDocAddress != null && consigneeDocAddress.E2_AddressOverride
					? consigneeDocAddress.E2_CompanyName
					: consigneeDocAddress?.Organisation?.OH_FullName ?? ZString.Empty;
			}
		}

		#endregion

		#region DestinationPort

		protected override ZString DestinationPortCore
		{
			get
			{
				var consigneeDocAddress = WhsOrder.ConsigneeDocAddress;
				return consigneeDocAddress != null && !consigneeDocAddress.E2_AddressOverride
					? consigneeDocAddress.Address?.OA_RL_NKRelatedPortCode ?? ZString.Empty
					: ZString.Empty;
			}
		}

		#endregion

		#region TransportCo

		protected override DocOrganisation TransportCoCore => WhsOrder.GetTransportCompanyLegacy(Factory);

		protected override DocDocAddress TransportCoAddressCore => WhsOrder.GetTransportCoAddressLegacy(Factory);

		#endregion

		#region Collections

		public DocWhsOrderLineCollection OrderLines
		{
			get { return fOrderLines ?? (fOrderLines = GetOrderLines()); }
		}

		public DocWhsPackingSlipLineCollection BillOfLadingPackingLines
		{
			get
			{
				if (fBillOfLadingPackingLines == null)
				{
					fBillOfLadingPackingLines = MakeCollection();
				}
				return fBillOfLadingPackingLines;
			}
		}

		public DocWhsPackingSlipLineCollection BillOfLadingPackingLinesUS
		{
			get
			{
				if (billOfLadingPackingLinesUS == null)
				{
					billOfLadingPackingLinesUS = MakeCollection();
				}
				return billOfLadingPackingLinesUS;
			}
		}

		#region PackingLines

		public DocWhsPackingSlipLineCollection PackingLines
		{
			get { return packingLines ?? (packingLines = new PackingLineWrapperCollectionHelperLegacy().GetPackingLines(WhsOrder)); }
		}

		DocWhsPackingSlipLineCollection packingLines;

		#endregion

		public DocWhsLabelCollection DeliveryLabels
		{
			get
			{
				DocWhsLabelCollection result = new DocWhsLabelCollection(Factory);
				for (int i = 0; i < TotalNumberOfLabels; i++)
				{
					WhsLabel label = new WhsLabel();
					label.Number = i + 1;
					result.Add(DocWhsLabel.New(label, Factory));
				}
				return result;
			}
		}

		public DocWhsPackageLabelCollection PackageLabels
		{
			get
			{
				var result = new DocWhsPackageLabelCollection(Factory);

				// dodgy hack.
				var order = this.WhsOrder as WhsOrder;
				var workOrder = this.WhsOrder as WhsWorkOrder;

				WhsPickableDocketLineCollection sortedOrderLineCollection;
				if (order != null)
				{
					sortedOrderLineCollection = new WhsOrderLineCollectionWithoutChildLines(order);
				}
				else if (workOrder != null)
				{
					sortedOrderLineCollection = new WhsWorkOrderLineCollection(workOrder);
				}
				else
				{
					throw new ArgumentException("PickableDocket was not a WhsWorkOrder or WhsOrder and is not supported by the DocWhsOrder wrapper.");
				}

				foreach (WhsPickableDocketLine orderLine in WhsOrder.SelectedOrderLines)
				{
					for (int i = 0; i < Math.Ceiling(orderLine.WE_PackQuantity); i++)
					{
						var label = new WhsLabel();
						label.Number = FindPackageLabelNumber(orderLine, sortedOrderLineCollection, i);
						result.Add(DocWhsPackageLabel.New(label, DocWhsPickableDocketLine.New(orderLine, Factory), Factory));
					}
				}

				return result;
			}
		}

		public DocWhsPackageLabelCollection PackageLabelsForBOM
		{
			get
			{
				DocWhsPackageLabelCollection result = new DocWhsPackageLabelCollection(Factory);

				WhsOrder order = this.WhsOrder as WhsOrder;
				WhsWorkOrder workOrder = this.WhsOrder as WhsWorkOrder;
				WhsPickableDocketLineCollection sortedOrderLineCollection;

				if (order != null)
				{
					sortedOrderLineCollection = new WhsOrderLineCollectionWithoutChildLines(order);
				}
				else if (workOrder != null)
				{
					sortedOrderLineCollection = new WhsWorkOrderLineCollection(workOrder);
				}
				else
				{
					throw new ArgumentException("PickableDocket was not a WhsWorkOrder or WhsOrder and is not supported by the DocWhsOrder wrapper.");
				}

				foreach (WhsPickableDocketLine orderLine in WhsOrder.SelectedOrderLines)
				{
					for (int i = 0; i < Math.Ceiling(orderLine.WE_PackQuantity); i++)
					{
						WhsLabel label = new WhsLabel();
						label.Number = FindPackageLabelNumber(orderLine, sortedOrderLineCollection, i);
						result.Add(DocWhsPackageLabel.New(label, DocWhsPickableDocketLine.New(orderLine, Factory), Factory));

						if (orderLine.IsBOMProduct)
						{
							WhsWorkOrderCollection workOrders = AutoCreateVirtualWorkOrder(orderLine, WhsOrder.WD_ExternalReferenceSplit);
							foreach (WhsWorkOrder newWorkOrder in workOrders)
							{
								foreach (WhsWorkOrderLine workOrderLine in newWorkOrder.AllLines)
								{
									if (workOrderLine.WE_Level != 0)
									{
										result.Add(DocWhsPackageLabel.New(label, DocWhsPickableDocketLine.New(workOrderLine, Factory), Factory));
									}
								}
							}
						}
					}
				}
				return result;
			}
		}

		protected WhsWorkOrderCollection AutoCreateVirtualWorkOrder(WhsPickableDocketLine parentDocketLine, byte currentSplitNo)
		{
			var tempFactory = new BusinessObjectFactory();
			var parentDocketLineInTempFactory = (WhsPickableDocketLine)tempFactory.ImportFromAnotherFactory(parentDocketLine);

			var result = new WhsWorkOrderCollection(tempFactory);

			var workOrderResult = tempFactory.New<WhsWorkOrder>();
			workOrderResult.WD_WD_ParentDocket = parentDocketLineInTempFactory.PK;
			workOrderResult.WD_OH_Client = parentDocketLineInTempFactory.PickableDocket.WD_OH_Client;
			workOrderResult.WD_WW_Whs = parentDocketLineInTempFactory.PickableDocket.WD_WW_Whs;
			workOrderResult.WD_ExternalReference = parentDocketLineInTempFactory.PickableDocket.WD_ExternalReference;
			workOrderResult.WD_ExternalReferenceSplit = (byte)(currentSplitNo + 1);
			workOrderResult.WD_RequiredDate = parentDocketLineInTempFactory.PickableDocket.WD_RequiredDate;

			WhsPickableDocketLine newLine = workOrderResult.Lines.AddNew();
			newLine.WE_TransactionQuantity = 1;
			newLine.WE_OP = parentDocketLineInTempFactory.SupplierPart.PK;

			return result;
		}

		#endregion

		#region Implementation

		protected delegate DocWhsPickableDocket NewDelegate(WhsPickableDocket whsOrder, BusinessObjectFactory factoryToWrap);

		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		DocWhsPackingSlipLineCollection MakeCollection()
		{
			var tempFactory = new BusinessObjectFactory();
			var billOfLadingPackingLines = new DocWhsPackingSlipLineCollection(tempFactory);

			foreach (WhsPickableDocketLine orderLine in WhsOrder.Lines)
			{
				AccumulateGroupedLineAttributes(billOfLadingPackingLines, orderLine);
			}

			return billOfLadingPackingLines;
		}

		void AccumulateGroupedLineAttributes(DocWhsPackingSlipLineCollection packingSlipLines, WhsPickableDocketLine orderLine)
		{
			DocWhsPackingSlipLine slipLine = null;

			foreach (WhsReleaseLine releaseLine in orderLine.ReleaseLines)
			{
				if (releaseLine.Quantity > 0)
				{
					if (slipLine == null)
					{
						slipLine = DocWhsPackingSlipLine.New(releaseLine, orderLine, packingSlipLines.Factory);
					}
					slipLine.GroupedLineUnitsMet += releaseLine.Quantity;
				}
			}

			if (slipLine != null)
			{
				packingSlipLines.Add(slipLine);
			}
		}

		ZInt FindPackageLabelNumber(WhsPickableDocketLine selectedOrderLine, WhsPickableDocketLineCollection sortedOrderLineCollection, ZInt position)
		{
			ZInt result = 1;
			ZInt temporaryCount = ZInt.Zero;
			foreach (WhsPickableDocketLine orderLine in sortedOrderLineCollection)
			{
				if (orderLine == selectedOrderLine)
				{
					result += temporaryCount + position;
					break;
				}
				else
				{
					temporaryCount += (int)Math.Ceiling(orderLine.WE_PackQuantity);
				}
			}

			return result;
		}

		DocWhsOrderLineCollection fOrderLines;
		DocWhsPackingSlipLineCollection fBillOfLadingPackingLines;
		DocWhsPackingSlipLineCollection billOfLadingPackingLinesUS;

		#endregion

		#endregion

		#region Properties

		#region ZInt

		public ZInt TotalNumberOfPackageLabels
		{
			get { return (DocketLabel != null) ? DocketLabel.NumberOfPackageLabels : ZInt.Zero; }
		}

		#endregion

		#region ZString

		protected override ZString CarrierNameCore => WhsOrder.GetCarrierName();

		public ZString DockDoorLocation
		{
			get
			{
				var ddl = WhsOrder?.Pick?.DockDoorLocation;
				return ddl != null ? ddl.ToLocationString() : ZString.Empty;
			}
		}

		public ZString SpecialInstructions
		{
			get { return WhsOrder.WD_HandlingInstructions; }
		}

		public ZString PackingSlipTitle
		{
			get
			{
				ZString result = Res.GetString("ba236f03-7f96-4f77-b7a3-1515a7c53a9d", "Packing Slip");
				if (WhsOrder.Warehouse != null)
				{
					result = WhsOrder.Warehouse.PackingSlipTitle;
				}
				return result;
			}
		}

		public ZString BarcodeTextForExternalReference
		{
			get
			{
				TextBarcode barcode = new TextBarcode(WhsOrder.WD_ExternalReference);
				return barcode.TextAs128sFontString;
			}
		}

		public override ZString PickNo
		{
			get { return (WhsOrder.Pick != null) ? WhsOrder.Pick.WP_PickNo : ZString.Empty; }
		}

		public ZString EmergencyContactMessageString
		{
			get
			{
				ZString result = ZString.Empty;
				if (PrintDGDetails == "Y")
				{
					result = Res.GetString("fd88e70e-d216-4260-a761-cd65dc9ba594", "Hazardous materials emergency contact number:\r\n{0} {1}", DGContact, EmergencyNumber);
				}

				return result;
			}
		}

		public ZString PrintDGDetails
		{
			get
			{
				ZString result = ZString.Empty;
				if (PackingLines != null)
				{
					foreach (DocWhsPackingSlipLine docWhsPackingSlipLineObject in PackingLines)
					{
						if (docWhsPackingSlipLineObject.DGSubstance != null)
						{
							result = "Y";
							break;
						}
					}
				}

				return result;
			}
		}

		public ZString CurrencySymbol
		{
			get
			{
				ZBool firstSymbol = ZBool.True;
				ZString result = ZString.Empty;
				foreach (DocWhsPackingSlipLine docPackSlipLine in PackingLines)
				{
					if (docPackSlipLine.ExtendedLinePrice != ZDecimal.Zero)
					{
						if (firstSymbol)
						{
							result = docPackSlipLine.CurrencySymbol;
							firstSymbol = ZBool.False;
						}
						else
						{
							if (docPackSlipLine.CurrencySymbol != result)
							{
								result = ZString.Empty;
							}
						}
					}
				}
				return result;
			}
		}

		public ZString TotalExtendedLinePriceWithSymbol
		{
			get
			{
				ZString result = ZString.Empty;
				if (TotalExtendedLinePrice != ZDecimal.Zero)
				{
					result = CurrencySymbol + TotalExtendedLinePrice.ToString("0.00");
				}
				return result;
			}
		}

		public ZString PickingInstructions
		{
			get
			{
				ZString result = ZString.Empty;
				if (!WhsOrder.PickingInstructions.IsEmpty)
				{
					result = Res.GetString("af809e0b-591a-494b-a6aa-56ac5245f66b", "Order {0} - {1}", WhsOrder.WD_ExternalReference, WhsOrder.PickingInstructions);
				}
				return result;
			}
		}

		public ZString WarehouseCartageCoordinatorName
		{
			get
			{
				ZString result = "";
				if (WhsOrder.Warehouse != null && WhsOrder.Warehouse.WarehouseAddress != null && WhsOrder.Warehouse.WarehouseAddress.Header != null)
				{
					ZString staffNK = WhsOrder.Warehouse.WarehouseAddress.Header.StaffAssignments.GetStaffAssignment(StaffAssignmentRoles.Codes.CartageCoordinator, OrgStaffAssignmentsLookups.AllServices);
					GlbStaff glbStaff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffNK);
					if (glbStaff != null)
					{
						result = glbStaff.GS_FullName;
					}
				}
				return result;
			}
		}

		public ZString WarehouseCartageCoordinatorPhone
		{
			get
			{
				ZString result = "";
				if (WhsOrder.Warehouse != null && WhsOrder.Warehouse.WarehouseAddress != null && WhsOrder.Warehouse.WarehouseAddress.Header != null)
				{
					ZString staffNK = WhsOrder.Warehouse.WarehouseAddress.Header.StaffAssignments.GetStaffAssignment(StaffAssignmentRoles.Codes.CartageCoordinator, OrgStaffAssignmentsLookups.AllServices);
					GlbStaff glbStaff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffNK);
					if (glbStaff != null)
					{
						result = glbStaff.GS_WorkPhone_Formatted;
					}
				}
				return result;
			}
		}

		public MultilingualString CartageAdviceOpeningText
		{
			get
			{
				MultilingualString result = (NoResString)ZString.Empty;
				if (DocumentName.StartsWith((NoResString)"Cartage Advice"))
				{
					result = DocumentsDataRegistry.Instance.WarehouseCartageAdviceOpeningText.Value;
				}
				return result;
			}
		}

		public MultilingualString CartageAdviceClosingText
		{
			get
			{
				MultilingualString result = (NoResString)ZString.Empty;
				if (DocumentName.StartsWith((NoResString)"Cartage Advice"))
				{
					result = DocumentsDataRegistry.Instance.WarehouseCartageAdviceClosingText.Value;
				}
				return result;
			}
		}

		public ZString CargateAdviceContainerNumberAndTypeLine
		{
			get { return WhsDocketContainerSupport.ContainerNumberAndType(DocketContainers.ToIDocSimpleContainerCollection(), false, false); }
		}

		#endregion

		#region ZDecimal

		public ZDecimal TotalExtendedLinePrice
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				foreach (DocWhsPackingSlipLine docPackSlipLine in PackingLines)
				{
					result += docPackSlipLine.ExtendedLinePrice;
				}
				return result;
			}
		}

		public ZDecimal TotalTopLevelUnitsOrdered
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				foreach (DocWhsPackingSlipLine docPackSlipLine in PackingLines)
				{
					result += docPackSlipLine.TopLevelUnitsOrdered;
				}
				return result;
			}
		}

		public ZDecimal TotalTopLevelUnitsMet
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				foreach (DocWhsPackingSlipLine docPackSlipLine in PackingLines)
				{
					result += docPackSlipLine.TopLevelUnitsMet;
				}
				return result;
			}
		}

		public ZDecimal TotalGroupedLineUnitsMet
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				foreach (DocWhsPackingSlipLine docPackSlipLine in BillOfLadingPackingLines)
				{
					result += docPackSlipLine.GroupedLineUnitsMet;
				}
				return result;
			}
		}

		public ZDecimal TotalGroupedLineUnitsWeight
		{
			get { return WhsOrder.WD_WeightSent; }
		}

		public ZDecimal CollectionFee
		{
			get { return ZDecimal.Zero; }
		}

		public ZDecimal TotalCODCharges
		{
			get { return ShipperCODAmount + CollectionFee; }
		}

		#endregion

		#endregion

		#region IDocManagerBarcode Members

		protected override TextBarcode DocManagerBarcode
		{
			get
			{
				if (fDocManagerBarcode == null)
				{
					BarcodeGenerator generator = new BarcodeGenerator();
					fDocManagerBarcode = generator.CreateDocumentBarcode(DocManagerCode, DocManagerUniqueID, ((IDocTypeCode)this).DocTypeCode + ";");
				}
				return fDocManagerBarcode;
			}
		}
		TextBarcode fDocManagerBarcode;

		public override ZString BarcodeTextForFont
		{
			get { return DocManagerBarcode.TextAs128sFontString; }
		}

		public override ZString BarcodeText
		{
			get { return DocManagerBarcode.TextToEncode; }
		}

		#endregion

		#region IDocTypeCode Members

		ZString DocTypeCode;

		ZString IDocTypeCode.DocTypeCode
		{
			get { return DocTypeCode; }
			set { DocTypeCode = value; }
		}

		#endregion

		#region ILoadingSupport

		ZGuid ILoadingSupport.LoadPK
		{
			get => LoadPK;
			set => LoadPK = value;
		}

		ZGuid LoadPK;

		ZString ILoadingSupport.CommonLoadVolumeUQ
		{
			get => CommonLoadVolumeUQ;
			set => CommonLoadVolumeUQ = value;
		}

		ZString CommonLoadVolumeUQ;

		ZString ILoadingSupport.CommonLoadWeightUQ
		{
			get => CommonLoadWeightUQ;
			set => CommonLoadWeightUQ = value;
		}

		ZString CommonLoadWeightUQ;

		#endregion

		#region TotalLoadedPackages

		public LabelValuePairWrapper TotalLoadedPackages
			=> new LabelValuePairWrapper(Res.GetString("7472b1a4-541e-46e3-9548-7f27d746f27f", "Loaded Packages:"), LoadedPackages.Count(), Factory);

		#endregion

		#region TotalLoadedUnits

		public LabelValuePairWrapper TotalLoadedUnits
			=> new LabelValuePairWrapper(
				Res.GetString("4eb79e22-8dd3-4839-8a77-f35d4b690abe", "Loaded Units:"),
				LoadedPackages.SelectMany(p => p.PackedItemDivots).Sum(d => d.KI_PackedQty),
				Factory);

		#endregion

		#region TotalLoadedWeight

		public WeightWrapper TotalLoadedWeight
		{
			get
			{
				var isValidUnit = Constants.Weight.ContainsCode(CommonLoadWeightUQ);

				var groupedPackages = LoadedPackages
					.GroupBy(p => p.KP_WeightUQ)
					.Select(gp => (WeightUQ: gp.Key, WeightTotal: gp.Sum(p => p.KP_Weight)));

				return isValidUnit && groupedPackages.Any()
					? new WeightWrapper(groupedPackages.Sum(p => Constants.Weight.Convert(p.WeightTotal, p.WeightUQ, CommonLoadWeightUQ)), CommonLoadWeightUQ.ToString(), 2, Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight), Factory)
					: WeightWrapper.Empty;
			}
		}

		#endregion

		#region TotalLoadedVolume

		public VolumeWrapper TotalLoadedVolume
		{
			get
			{
				var isValidUnit = Constants.Volume.ContainsCode(CommonLoadVolumeUQ);

				var groupedPackages = LoadedPackages
					.GroupBy(p => p.KP_VolumeUQ)
					.Select(gp => (VolumeUQ: gp.Key, VolumeTotal: gp.Sum(p => p.KP_Volume)));

				return isValidUnit && groupedPackages.Any()
					? new VolumeWrapper(groupedPackages.Sum(p => Constants.Volume.Convert(p.VolumeTotal, p.VolumeUQ, CommonLoadVolumeUQ)), CommonLoadVolumeUQ.ToString(), Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume), Factory)
					: VolumeWrapper.Empty;
			}
		}

		IEnumerable<PkgPackage> LoadedPackages
		{
			get
			{
				if (loadedPackages == null)
				{
					var order = WhsOrder as WhsOrder;

					var packageJob = order?.PackageJob;
					if (packageJob == null)
					{
						loadedPackages = Array.Empty<PkgPackage>();
					}
					else
					{
						loadedPackages = Factory.Load<PkgPackage>(LoadedPackagesQuery(packageJob));
					}
				}

				return loadedPackages;
			}
		}
		IEnumerable<PkgPackage> loadedPackages;

		ZQuery LoadedPackagesQuery(PkgPackageJob packageJob)
		{
			var pivotQuery = new ZDBOnlySubQuery(typeof(WhsLoadPkgPackagePivot), WhsLoadPkgPackagePivotSchema.WLP_KP_Package);
			pivotQuery.AddToFilter(WhsLoadPkgPackagePivotSchema.WLP_WLO_Load, LoadPK);
			pivotQuery.AddToFilter(WhsLoadPkgPackagePivotSchema.WLP_LoadedTime, SQLComparisonOperator.NotEqual, null);
			pivotQuery.AddToFilter(WhsLoadPkgPackagePivotSchema.WLP_UnloadedTime, SQLComparisonOperator.Equal, null);

			var query = new ZDBOnlyQuery(typeof(PkgPackage));
			query.AddToFilter(PkgPackageSchema.KP_KJ_ParentPackageJob, packageJob.PK);
			query.AddSubQuery(pivotQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region Business Objects Overrides

		protected override DocWhsDocketLineCollection GetDocketLines()
		{
			return GetOrderLines();
		}

		DocWhsOrderLineCollection GetOrderLines()
		{
			DocWhsOrderLineCollection result = new DocWhsOrderLineCollection(WhsOrder.Lines, Factory);
			ZString propertyToSortBy = GetPropertyToSortBy();
			if (!propertyToSortBy.IsEmpty)
			{
				result.Sort(propertyToSortBy);
			}
			return result;
		}

		#endregion

		#region Implementation

		protected ZString GetPropertyToSortBy()
		{
			return WhsOrder is WhsOrder ? PackingSlipLineWrapperHelper.GetPropertyToSortBy(WhsOrder) : ZString.Empty;
		}

		protected WhsPickableDocket WhsOrder
		{
			get { return (WhsPickableDocket)WrappedObject; }
		}

		#endregion
	}
}
