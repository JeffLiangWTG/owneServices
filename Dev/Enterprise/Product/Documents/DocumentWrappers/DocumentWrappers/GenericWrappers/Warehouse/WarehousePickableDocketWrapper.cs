using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Barcode.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public abstract class WarehousePickableDocketWrapper : WarehouseDocketWrapper
	{
		#region Constructors

		// todo - refactor out and use the strategy constructor below instead.
		protected WarehousePickableDocketWrapper(WhsPickableDocket whsPickableDocket, BusinessObjectFactory factory)
			: base(whsPickableDocket, factory)
		{
		}

		protected WarehousePickableDocketWrapper(WhsPopulatePickableDocketStrategy strategy)
			: base(strategy)
		{
		}

		#endregion

		#region Properties 

		#region Consignee

		// todo - remove once all sub classed wrappers support the PopulateStrategy pattern.
		public override OrganisationWrapper Consignee
		{
			get
			{
				if (PopulateStrategy != null)
				{
					return base.Consignee;
				}
				else if (DocketBO != null && consignee == null)
				{
					consignee = new OrganisationWrapper(OrganisationUsageType.Consignee, ((WhsPickableDocket)DocketBO).ConsigneeDocAddress, Factory);
				}
				return consignee;
			}
		}
		OrganisationWrapper consignee;

		#endregion

		#region ConsigneeAddress

		// todo - remove once all sub classed wrappers support the PopulateStrategy pattern.
		public override AddressWrapper ConsigneeAddress
		{
			get
			{
				if (PopulateStrategy != null)
				{
					return base.ConsigneeAddress;
				}
				else if (DocketBO != null && consigneeAddress == null)
				{
					consigneeAddress = new AddressWrapper(DocketBO.GoodsBillToDocAddress, Factory);
				}
				return consigneeAddress;
			}
		}
		AddressWrapper consigneeAddress;

		#endregion

		#region HandlingInstructions

		// todo - remove once all sub classed wrappers support the PopulateStrategy pattern.
		protected override LabelValuePairWrapper HandlingInstructionsCore
		{
			get { return PackingDocketBO != null ? new LabelValuePairWrapper(Res.GetString("a22464f8-72de-4b0c-b338-f079fdba19fa", "Special Instructions"), PackingDocketBO.WD_HandlingInstructions, Factory) : LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region IncoTerm

		// todo - remove once all sub classed wrappers support the PopulateStrategy pattern.
		public override CodeAndDescriptionWrapper IncoTerm
		{
			get
			{
				if (PopulateStrategy != null)
				{
					return base.IncoTerm;
				}
				else
				{
					return DocketBO != null ? new CodeAndDescriptionWrapper(DocketBO.WD_INCO, DocketBO.Lookups.INCOTerms, Factory) : null;
				}
			}
		}

		#endregion

		#region JobNumberHeading

		protected override ZString JobNumberHeadingCore => PackingDocketBO != null ? Res.GetString("3b08e094-9ac1-4ef3-b477-5d4bbb73284d", "Order Number") : "";

		#endregion

		#region PickNo

		public override LabelValuePairWrapper PickNo
		{
			get
			{
				return PackingDocketBO != null && PackingDocketBO.Pick != null
						 ? new LabelValuePairWrapper(Res.GetString("9aba7053-2203-4dac-bd00-6aff2fb1c800", "Pick No"), PackingDocketBO.Pick.WP_PickNo, Factory)
						 : base.PickNo;
			}
		}

		#endregion

		#region StagingAreaName

		protected override LabelValuePairWrapper StagingAreaNameCore
		{
			get
			{
				var ddl = PackingDocketBO?.Pick?.DockDoorLocation;
				return ddl != null
					? new LabelValuePairWrapper(Res.GetString("fe66a69a-d83a-4b0c-9442-8463b83b6717", "Dock Door Location"), ddl.ToLocationString(), Factory)
					: LabelValuePairWrapper.Empty;
			}
		}

		#endregion

		#region TransportCoAddress

		protected override AddressWrapper TransportCoAddressCore => PackingDocketBO.GetTransportCoAddress(Factory);

		#endregion

		#region WarehouseCCPCode

		protected override ZString WarehouseCCPCodeCore
		{
			get
			{
				var result = ZString.Empty;
				if (DocketBO?.Warehouse?.WarehouseAddress != null)
				{
					var org = Factory.Load<OrgHeader>(DocketBO.Warehouse.WarehouseAddress.OA_OH);
					if (org != null)
					{
						result = org.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID, GlbCompany.CurrentCompany.Country);
					}
				}
				return result;
			}
		}

		#endregion

		#region ClientGCR

		public override ZString ClientGCR => DocketBO.GetClientCode(OrgCusCode.CodeTypes.CorporationCode);

		#endregion

		#region ClientCPC

		protected override ZString ClientCPCCore => DocketBO.GetClientCode(OrgCusCode.CodeTypes.CustomsCPPermitCode);

		#endregion

		#region CP_IssueNo

		public override ZString CP_IssueNo => ZString.Format("{0}{1}", DocketBO.GetClientCode(OrgCusCode.CodeTypes.CustomsCPPermitCode), DocketBO?.WD_DocketID.Right(8) ?? ZString.Empty);

		#endregion

		#region ATOEstCode

		public override ZString ATOEstCode => WarehouseCCPCode;

		#endregion

		#region ACSEstCode

		public override ZString ACSEstCode => PackingDocketBO.Consignee?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.ControlledPremisesID, GlbCompany.CurrentCompany.Country) ?? ZString.Empty;

		#endregion

		#region EmergencyContactMessageString

		protected override ZString EmergencyContactMessageStringCore
		{
			get
			{
				var result = ZString.Empty;
				if (PrintDGDetails == "Y")
				{
					result = Res.GetString("fd88e70e-d216-4260-a761-cd65dc9ba594", "Hazardous materials emergency contact number:\r\n{0} {1}", Client.DGContact.Name, Client.DGContact.PhoneFormatted);
				}

				return result;
			}
		}

		#endregion

		#region CurrencySymbol

		protected override ZString CurrencySymbolCore
		{
			get
			{
				var firstSymbol = ZBool.True;
				var result = ZString.Empty;
				foreach (WarehousePackingSlipLineWrapper packingLine in PackingLines)
				{
					if (packingLine.ExtendedLinePrice.Amount != 0m)
					{
						if (firstSymbol)
						{
							result = packingLine.CurrencySymbol;
							firstSymbol = ZBool.False;
						}
						else if (packingLine.CurrencySymbol != result)
						{
							result = ZString.Empty;
						}
					}
				}
				return result;
			}
		}

		#endregion

		#region TotalExtendedLinePriceWithSymbol

		protected override ZString TotalExtendedLinePriceWithSymbolCore
		{
			get
			{
				var result = ZString.Empty;
				if (TotalExtendedLinePrice.Amount != 0m)
				{
					result = CurrencySymbol + TotalExtendedLinePrice.Amount.ToString("0.00");
				}
				return result;
			}
		}

		#endregion

		#region WarehouseCartageCoordiatorName

		public override ZString WarehouseCartageCoordinatorName
		{
			get
			{
				var result = ZString.Empty;
				var header = DocketBO?.Warehouse?.WarehouseAddress?.Header;
				if (header != null)
				{
					var staffNK = header.StaffAssignments.GetStaffAssignment(StaffAssignmentRoles.Codes.CartageCoordinator, OrgStaffAssignmentsLookups.AllServices);
					var glbStaff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffNK);
					if (glbStaff != null)
					{
						result = glbStaff.GS_FullName;
					}
				}
				return result;
			}
		}

		#endregion

		#region WarehouseCartageCoordinatorPhone

		public override ZString WarehouseCartageCoordinatorPhone
		{
			get
			{
				var result = ZString.Empty;
				var header = DocketBO?.Warehouse?.WarehouseAddress?.Header;
				if (header != null)
				{
					var staffNK = header.StaffAssignments.GetStaffAssignment(StaffAssignmentRoles.Codes.CartageCoordinator, OrgStaffAssignmentsLookups.AllServices);
					var glbStaff = Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, staffNK);
					if (glbStaff != null)
					{
						result = glbStaff.GS_WorkPhone_Formatted;
					}
				}
				return result;
			}
		}

		#endregion

		#region TotalExtendedLinePrice

		public override MoneyWrapper TotalExtendedLinePrice => totalExtendedLinePrice ?? (totalExtendedLinePrice = PackingLines.TotalExtendedLinePrice);
		MoneyWrapper totalExtendedLinePrice;

		#endregion

		#region ContainerNumberAndTypeLine

		public override ZString ContainerNumberAndTypeLine => WhsOrderContainerSupport.ContainerNumberAndType(OrderContainers.ToIDocSimpleContainerCollection());

		#endregion

		#region Fields

		protected override ZBool PrintPageWithContainerNumberCore => (OrderContainers.Count > MaximumContainersWithTypeOnALine
			&& AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.Value);

		#endregion

		#region TotalTopLevelUnitsOrdered

		protected override ZDecimal TotalTopLevelUnitsOrderedCore
		{
			get
			{
				var result = 0m;
				foreach (WarehousePackingSlipLineWrapper packSlipLine in PackingLines)
				{
					result += packSlipLine.UnitsOrdered.ValueAsDecimal;
				}
				return result;
			}
		}

		#endregion

		#region TotalTopLevelUnitsMet

		protected override ZDecimal TotalTopLevelUnitsMetCore
		{
			get
			{
				var result = 0m;
				foreach (WarehousePackingSlipLineWrapper packSlipLine in PackingLines)
				{
					result += packSlipLine.UnitsMet.ValueAsDecimal;
				}
				return result;
			}
		}

		#endregion

		#region TotalGroupedLineUnitsMet

		protected override ZDecimal TotalGroupedLineUnitsMetCore
		{
			get
			{
				var result = 0m;
				foreach (WarehousePackingSlipLineWrapper packSlipLine in BillOfLadingPackingLines)
				{
					result += packSlipLine.GroupedLineUnitsMet;
				}

				return result;
			}
		}

		#endregion

		#endregion

		#region Collections

		#region GetJobLines

		// When WarehousePickableDocketWrapper is changed to be abstract as it should be, then it will be overriden with correct collections in child classes
		// for now it just handle the only collection that could be cast to WarehouseGenericWrapperCollection - WarehousePackingSlipLineWrapperCollection.
		protected override WarehouseGenericWrapperCollection GetJobLines()
		{
			return NewWarehousePackingSlipLineWrapperCollection();
		}

		#endregion

		#region Packing Lines Wrapper Collection

		protected override WarehousePackingSlipLineWrapperCollection NewWarehousePackingSlipLineWrapperCollection()
		{
			return new PackingLineWrapperCollectionHelperGeneric().GetPackingLines(PackingDocketBO);
		}

		#endregion

		#region Order Lines Wrapper Collection

		protected override DocWhsOrderLineCollection NewWarehouseOrderLineWrapperCollection()
		{
			return GetOrderLines();
		}

		#endregion

		#region RolledUpLinesForOrderCopyCollection
		protected override DocWhsOrderLineCollection NewWarehouseRolledUpLinesForOrderCopyCollection()
		{
			if (rolledUpLinesForOrderCopy == null)
			{
				var order = PackingDocketBO as WhsOrder;
				var docketLines = order != null ? order.ParentLines : PackingDocketBO.Lines;
				rolledUpLinesForOrderCopy = GetRolledUpLinesForOrderCopy(docketLines);
				var propertyToSortBy = PackingSlipLineWrapperHelper.GetPropertyToSortBy(PackingDocketBO);
				if (!propertyToSortBy.IsEmpty)
				{
					rolledUpLinesForOrderCopy.Sort(propertyToSortBy);
				}
			}

			return rolledUpLinesForOrderCopy;
		}

		DocWhsOrderLineCollection rolledUpLinesForOrderCopy;

		#region GetRolledUpLinesForOrderCopy

		DocWhsOrderLineCollection GetRolledUpLinesForOrderCopy(WhsPickableDocketLineCollection whsOrderLineCollection)
		{
			var result = new DocWhsOrderLineCollection(Factory);
			var docPickableDocketLinesDictionary = new Dictionary<string, DocWhsPickableDocketLine>();

			foreach (WhsPickableDocketLine line in whsOrderLineCollection)
			{
				string key = GetDictionaryKey(line);
				DocWhsPickableDocketLine docLine;
				if (docPickableDocketLinesDictionary.TryGetValue(key, out docLine))
				{
					docLine.AddLineForRollUp(line);
				}
				else
				{
					docLine = DocWhsPickableDocketLine.New(line, Factory);
					result.Add(docLine);
					docPickableDocketLinesDictionary.Add(key, docLine);
				}
			}
			return result;
		}

		string GetDictionaryKey(WhsPickableDocketLine line)
		{
			string result = "";
			string separator = "^";

			OrgSupplierPart part = line.SupplierPart;
			OrgHeader client = line.Docket.Client;
			if (client != null && client.PartAttributeManager.IsAttributeNeturalAndDocumentRollUp(part))
			{
				result += line.WE_OP;
				result += line.WE_PartAttrib1 + separator;
				result += line.WE_PartAttrib2 + separator;
				result += line.WE_PartAttrib3 + separator;
				result += line.WE_SerialNumber + separator;
				result += line.WE_ExpiryDate + separator;
				result += line.WE_PackingDate + separator;
				result += line.WE_LineComment;
			}
			else
			{
				result += line.PK; // do not roll up at all.
			}

			return result;
		}

		#endregion

		#endregion

		#region BillOfLadingPackingLines

		protected override WarehousePackingSlipLineWrapperCollection BillOfLadingPackingLinesCore
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
		WarehousePackingSlipLineWrapperCollection fBillOfLadingPackingLines;

		#endregion

		#region BillOfLadingPackingLinesUS

		protected override WarehousePackingSlipLineWrapperCollection BillOfLadingPackingLinesUSCore
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
		WarehousePackingSlipLineWrapperCollection billOfLadingPackingLinesUS;

		#endregion

		#region WhsOrderContainerSupport

		DocContainerCollectionHelper WhsOrderContainerSupport => whsOrderContainerSupport ?? (whsOrderContainerSupport = new DocContainerCollectionHelper(MaximumContainersWithTypeOnALine, false));
		DocContainerCollectionHelper whsOrderContainerSupport;

		#endregion

		#region OrderContainers

		DocWhsDocketContainerCollection OrderContainers
			=> DocketBO?.Containers != null
				? new DocWhsDocketContainerCollection(DocketBO.Containers, Factory)
				: new DocWhsDocketContainerCollection(Factory);

		#endregion

		#region PackageLabels

		protected override DocWhsPackageLabelCollection PackageLabelsCore
		{
			get
			{
				var result = new DocWhsPackageLabelCollection(Factory);

				// dodgy hack.
				var order = PackingDocketBO as WhsOrder;
				var workOrder = PackingDocketBO as WhsWorkOrder;

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

				foreach (var orderLine in PackingDocketBO.SelectedOrderLines)
				{
					for (var i = 0; i < Math.Ceiling(orderLine.WE_PackQuantity); i++)
					{
						var label = new WhsLabel();
						label.Number = FindPackageLabelNumber(orderLine, sortedOrderLineCollection, i);
						result.Add(DocWhsPackageLabel.New(label, DocWhsPickableDocketLine.New(orderLine, Factory), Factory));
					}
				}

				return result;
			}
		}

		#endregion

		#region PackageLabelsForBOM

		protected override DocWhsPackageLabelCollection PackageLabelsForBOMCore
		{
			get
			{
				var result = new DocWhsPackageLabelCollection(Factory);

				var order = PackingDocketBO as WhsOrder;
				var workOrder = PackingDocketBO as WhsWorkOrder;
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

				foreach (var orderLine in PackingDocketBO.SelectedOrderLines)
				{
					for (var i = 0; i < Math.Ceiling(orderLine.WE_PackQuantity); i++)
					{
						var label = new WhsLabel();
						label.Number = FindPackageLabelNumber(orderLine, sortedOrderLineCollection, i);
						result.Add(DocWhsPackageLabel.New(label, DocWhsPickableDocketLine.New(orderLine, Factory), Factory));

						if (orderLine.IsBOMProduct)
						{
							var workOrders = AutoCreateVirtualWorkOrder(orderLine, PackingDocketBO.WD_ExternalReferenceSplit);
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

		#endregion

		#endregion

		#region Page Header Info

		#region Generic and Common References

		public override LabelValuePairWrapper RequiredDate => DocketBO != null ? new LabelValuePairWrapper(Res.GetString("f35e27ae-f542-4e09-abad-1a12bca545f4", "Required Date"), DocketBO.WD_RequiredDate.ToZDateTime(), Factory) : LabelValuePairWrapper.Empty;

		#endregion

		#endregion

		#region Bizo Header Info

		#region Generic Wrapper Objects

		public override AddressWrapper GoodsBillToAddress
		{
			get
			{
				if (DocketBO != null && goodsBillToAddress == null)
				{
					goodsBillToAddress = new AddressWrapper(DocketBO.GoodsBillToDocAddress, Factory);
				}
				return goodsBillToAddress;
			}
		}
		AddressWrapper goodsBillToAddress;

		public override AddressWrapper TransportBillToAddress => DocketBO != null ? new AddressWrapper(DocketBO.TransportBillToDocAddress, Factory) : null;

		public override AddressWrapper PickUpAddress => pickUpAddress ?? (pickUpAddress = DocketBO != null ? new AddressWrapper(DocketBO.PickUpDocAddress, Factory) : null);
		AddressWrapper pickUpAddress;

		public override AddressWrapper DropOffAddress => dropOffAddress ?? (dropOffAddress = DocketBO != null ? new AddressWrapper(DocketBO.DropOffDocAddress, Factory) : null);
		AddressWrapper dropOffAddress;

		protected override AddressWrapper SupplierDocAddressCore => supplierDocAddressCore ?? (supplierDocAddressCore = DocketBO != null ? new AddressWrapper(DocketBO.SupplierDocAddress, Factory) : null);
		AddressWrapper supplierDocAddressCore;

		#endregion

		public override ZString BOLNumber => DocketBO != null ? DocketBO.WD_BOLNo : ZString.Empty;

		public override CodeAndDescriptionWrapper FulfillRule => DocketBO != null ? new CodeAndDescriptionWrapper(DocketBO.WD_WhsOrderFulfillmentRule, DocketBO.Lookups.WhsOrderFulfillmentRules, Factory) : null;

		public override CodeAndDescriptionWrapper PickOption => DocketBO != null ? new CodeAndDescriptionWrapper(DocketBO.WD_PickOption, DocketBO.Lookups.PickOptions, Factory) : null;

		public override CodeAndDescriptionWrapper CODType => DocketBO != null ? new CodeAndDescriptionWrapper(DocketBO.WD_CODPayMethod, DocketBO.Lookups.ShipperCODPaymentTypes, Factory) : null;

		public override MoneyWrapper CODAmount => DocketBO != null ? new MoneyWrapper(new Money(DocketBO.WD_ShipperCODAmount, null), Factory) : null;

		public override MoneyWrapper Insurance => DocketBO != null ? new MoneyWrapper(new Money(DocketBO.WD_LocalCartInsuranceCost, null), Factory) : null;

		public override ZBool IsWorkOrder => DocketBO is WhsComponentOrder;

		#endregion

		#region Page Footer Info

		public override VolumeWrapper CubicSent => DocketBO != null ? new VolumeWrapper(DocketBO.WD_CubicSent, DocketBO.WD_TotalCubicUnit, DocketBO.Lookups.CubicUnitTypes, Factory) : null;

		public override WeightWrapper WeightSent => DocketBO != null ? new WeightWrapper(DocketBO.WD_WeightSent, DocketBO.WD_TotalWeightUnit, WeightWrapper.StandardDecimalPlaces, DocketBO.Lookups.WeightUnitTypes, Factory) : null;

		public override ValueAndUnitWrapper PackagesSent => DocketBO != null ? new ValueAndUnitWrapper(DocketBO.WD_PackagesSent, DocketBO.WD_F3_NKTotalPackType, DocketBO.Lookups.ProductUQ, Factory) : null;

		public override ZShort PalletsSent => DocketBO != null ? DocketBO.WD_PalletsSent : ZShort.Zero;

		public override ZDecimal UnitsSent => DocketBO != null ? DocketBO.WD_UnitsSent : ZDecimal.Zero;

		public override ZString PrintDGDetails
		{
			get
			{
				var result = ZString.Empty;
				if (PackingLines != null)
				{
					foreach (WarehousePackingSlipLineWrapper warehousePackingSlipLineWrapperObject in PackingLines)
					{
						if (warehousePackingSlipLineWrapperObject.DangerousGoodsSubstance != null)
						{
							result = "Y";
							break;
						}
					}
				}
				return result;
			}
		}

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

		public override ZString BarcodeTextForFont => DocManagerBarcode.TextAs128sFontString;

		public override ZString BarcodeText => DocManagerBarcode.TextToEncode;

		#endregion

		#region Implementation

		DocWhsOrderLineCollection GetOrderLines()
		{
			var result = new DocWhsOrderLineCollection(DocketBO.Lines, Factory);
			var propertyToSortBy = PackingSlipLineWrapperHelper.GetPropertyToSortBy(PackingDocketBO);
			if (!propertyToSortBy.IsEmpty)
			{
				result.Sort(propertyToSortBy);
			}

			return result;
		}

		WarehousePackingSlipLineWrapperCollection MakeCollection()
		{
			var packingSlipWrappercollection = new WarehousePackingSlipLineWrapperCollection(Factory);

			foreach (WhsPickableDocketLine docketLine in DocketBO.Lines)
			{
				AccumulateGroupedLineAttributes(packingSlipWrappercollection, docketLine);
			}

			return packingSlipWrappercollection;
		}

		void AccumulateGroupedLineAttributes(WarehousePackingSlipLineWrapperCollection packingSlipLines, WhsPickableDocketLine docketLine)
		{
			WarehousePackingSlipLineWrapper slipLine = null;

			foreach (WhsReleaseLine releaseLine in docketLine.ReleaseLines)
			{
				if (releaseLine.Quantity > 0)
				{
					if (slipLine == null)
					{
						slipLine = new WarehousePackingSlipLineWrapper(releaseLine, docketLine, Factory);
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
			var result = 1;
			var temporaryCount = 0;
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

		protected ZString GetLoadNumber()
		{
			var loadNumbers = ZString.Empty;

			if (DocketBO != null)
			{
				var loadOrderSubQuery = new ZDBOnlySubQuery(typeof(WhsLoadOrder), WhsLoadOrderSchema.WOV_WLO_Load);
				loadOrderSubQuery.AddToFilter(WhsLoadOrderSchema.WOV_WD_Docket, DocketBO.PK);

				var loadQuery = new ZDBOnlyQuery(typeof(WhsLoad));
				loadQuery.AddSubQuery(loadOrderSubQuery, JoinCondition.And);

				var loads = Factory.Load<WhsLoad>(loadQuery);
				if (loads.Length > 0)
				{
					loadNumbers = string.Join(", ", loads.Select(l => l.WLO_JobID).OrderBy(l => l));
				}
			}

			return loadNumbers;
		}

		protected WhsPickableDocket PackingDocketBO => packingDocketBO ?? (packingDocketBO = (WhsPickableDocket)DocketBO);
		WhsPickableDocket packingDocketBO;

		const int MaximumContainersWithTypeOnALine = 5;

		#endregion
	}
}
