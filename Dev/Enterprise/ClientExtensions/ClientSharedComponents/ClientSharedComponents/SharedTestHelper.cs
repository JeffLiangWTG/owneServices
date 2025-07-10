#if DEBUG
using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.ClientSharedComponents
{
	public class SharedTestHelper : IDisposable
	{
		public SharedTestHelper(BusinessObjectFactory factory)
		{
			this.Factory = factory;
		}

		public SharedTestHelper() { }

		#region Set Factory Items
		public virtual OrgHeader FindOrCreateOrgHeader(string orgHeaderCode)
		{
			OrgHeader result = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, orgHeaderCode));
			if (result == null)
			{
				result = Factory.NewWithValidTestData<OrgHeader>();
				result.OH_Code = orgHeaderCode;
				SetSharedOrganisationCore(result, orgHeaderCode);
				Factory.Save();
			}
			return result;
		}

		protected virtual void SetSharedOrganisationCore(OrgHeader org, string orgHeaderCode) { }

		public virtual GlbBranch FindOrCreateBranch(string branchCode)
		{
			GlbBranch result = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, branchCode);
			if (result == null)
			{
				result = Factory.NewWithValidTestData<GlbBranch>();
				result.GB_Code = branchCode;
				Factory.Save();
			}
			return result;
		}

		public virtual ForwardingConsol FindOrCreateForwardingConsol(ZString uniqueConsignRef, ZString transportMode, ZString agentType, ZString consolMode)
		{
			ForwardingConsol result = Factory.LoadFromNaturalKey<ForwardingConsol>(JobConsolSchema.JK_UniqueConsignRef, uniqueConsignRef);
			if (result == null)
			{
				result = Factory.NewWithValidTestData<ForwardingConsol>();
				result.JK_UniqueConsignRef = uniqueConsignRef;
				result.JK_TransportMode = transportMode;
				result.JK_ConsolMode = consolMode;
				result.JK_AgentType = agentType;
				SetSharedConsolCore(result);
				Factory.Save();
			}
			return result;
		}

		protected virtual void SetSharedConsolCore(ForwardingConsol consol) { }

		public virtual ForwardingShipment FindOrCreateForwardingShipment(ZString uniqueConsignRef, ZString transportMode)
		{
			ForwardingShipment result = Factory.LoadFromNaturalKey<ForwardingShipment>(JobShipmentSchema.JS_UniqueConsignRef, uniqueConsignRef);
			if (result == null)
			{
				result = Factory.NewWithValidTestData<ForwardingShipment>();
				result.JS_UniqueConsignRef = uniqueConsignRef;
				result.JS_TransportMode = transportMode;
				SetSharedShipmentCore(result);
				Factory.Save();
			}
			return result;
		}

		protected virtual void SetSharedShipmentCore(ForwardingShipment shipment) { }

		public virtual GlbDepartment FindOrCreateDepartment(string departmentCode)
		{
			GlbDepartment result = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, departmentCode);
			if (result == null)
			{
				result = Factory.NewWithValidTestData<GlbDepartment>();
				result.GE_Code = departmentCode;
				Factory.Save();
			}
			return result;
		}

		public AccChargeCode FindOrCreateCharge(string chargeCode, string chargeGroup)
		{
			AccChargeCode result = FindOrCreateCharge(chargeCode);
			result.AC_ChargeGroup = chargeGroup;
			Factory.Save();
			return result;
		}

		public virtual AccChargeCode FindOrCreateCharge(string chargeCode)
		{
			ZQuery filter = new ZQuery(AccChargeCodeSchema.AC_Code, chargeCode);
			filter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbBranch.CurrentBranch.Company.PK);
			AccChargeCode result = Factory.LoadTop1<AccChargeCode>(filter);

			if (result == null)
			{
				AccChargeCode chargeNew = new BusinessObjectFactory().New<AccChargeCode>();
				chargeNew.AC_Code = chargeCode;
				chargeNew.AC_Desc = "DUMMY CHARGECODE";
				chargeNew.AC_GC = GlbCompany.CurrentCompany.PK;
				chargeNew.FillWithValidTestData();
				chargeNew.Factory.Save();
				result = Factory.LoadTop1<AccChargeCode>(filter);
			}

			return result;
		}

		public Order CreateOrder(ZString orderNo, ZByte split, ZString orderStatus, ZString product, ZShort lineNo, ZShort subLineNo, ZShort lineNo2, ZShort subLineNo2)
		{
			var result = Factory.New<Order>();
			result.BuyerPK = Buyer.PK;
			result.SupplierPK = Supplier.PK;
			result.JD_OrderNumber = orderNo;
			result.JD_OrderNumberSplit = split;
			result.JD_OrderStatus = orderStatus;

			if (lineNo > 0)
			{
				result.OrderLines.Add(CreateOrderLine(product, lineNo, subLineNo));
			}

			if (lineNo2 > 0)
			{
				result.OrderLines.Add(CreateOrderLine(product, lineNo2, subLineNo2));
			}
			return result;
		}

		public OrderLine CreateOrderLine(ZString product, ZShort lineNo, ZShort subLineNo)
		{
			OrderLine result = (OrderLine)Factory.New(typeof(OrderLine));
			result.JO_LineNo = lineNo;
			result.JO_SubLineNo = subLineNo;
			result.JO_Partno = product;

			return result;
		}

		public Xsd.Order CreateXsdOrder(ZString orderNo, ZByte split, ZString product, ZShort lineNo, ZShort subLineNo, ZShort lineNo2, ZShort subLineNo2)
		{
			Xsd.Order result = new Xsd.Order();

			result.Events.IsSpecified = false;
			result.OrderIdentifier.IsSpecified = true;
			result.OrderIdentifier.OrderNumber = orderNo;
			result.OrderIdentifier.OrderNumberSplit = split;

			result.OrderDetail.IsSpecified = true;
			result.OrderDetail.Buyer.IsSpecified = true;
			result.OrderDetail.Buyer.EDICode = Buyer.OH_Code;

			result.OrderDetail.Supplier.IsSpecified = true;
			result.OrderDetail.Supplier.EDICode = Supplier.OH_Code;

			if (lineNo > 0)
			{
				result.OrderLines.Add(CreateXsdOrderLine(product, lineNo, subLineNo));
			}

			if (lineNo2 > 0)
			{
				result.OrderLines.Add(CreateXsdOrderLine(product, lineNo2, subLineNo2));
			}

			result.OrderLines.IsSpecified = result.OrderLines.Count > 0;

			return result;
		}

		public Xsd.OrderOrderLine CreateXsdOrderLine(ZString product, ZShort lineNo, ZShort subLineNo)
		{
			Xsd.OrderOrderLine result = new Xsd.OrderOrderLine();
			result.OrderLineNo = lineNo;
			result.OrderSubLineNoSpecified = subLineNo > 0;
			result.OrderSubLineNo = subLineNo;
			result.OrderLineDetail.IsSpecified = true;
			result.OrderLineDetail.Product = product;
			return result;
		}

		public Xsd.TxnHeader CreateARInvoiceXSD()
		{
			Xsd.TxnHeader invoice = new Xsd.TxnHeader();
			invoice.InvoiceDate = ZDateTime.Today;
			invoice.InvTerm = Core.Constants.InvoiceTerms.CashOnDelivery;
			invoice.Ledger = Xsd.TxnLedgerType.AR;
			invoice.JobInvoiceNo = "InvoiceNum";
			invoice.TxnType = Xsd.TxnType.INV;

			invoice.OsInvoiceAmtExclTax.Value = 1000m;
			invoice.OsInvoiceAmtExclTax.CurrencyCode = Core.Constants.CurrencyCodes.Australia;
			invoice.OsInvoiceAmtInclTax.Value = 1100m;

			invoice.OsTaxAmount.Value = 100m;
			invoice.OsTaxAmount.CurrencyCode = Core.Constants.CurrencyCodes.Australia;

			Xsd.TxnLine line1 = invoice.TxnLines.AddNew();
			line1.ChargeCode = "Code1";
			line1.Description = "ChargeDesc1";
			line1.OsInvoiceAmtExclTax.Value = 600m;
			line1.OsInvoiceAmtExclTax.CurrencyCode = Core.Constants.CurrencyCodes.Australia;

			Xsd.TxnLine line2 = invoice.TxnLines.AddNew();
			line2.ChargeCode = "Code2";
			line2.Description = "ChargeDsesc \r\n ABC 12.00 \r\n DEF 20.00 \r\n IJK ";
			line2.OsInvoiceAmtExclTax.Value = 400m;
			line2.OsInvoiceAmtExclTax.CurrencyCode = Core.Constants.CurrencyCodes.Australia;

			return invoice;
		}

		public Xsd.ConsolAndShipment CreateJobDecXSD()
		{
			Xsd.ConsolAndShipment jobDec = new Xsd.ConsolAndShipment();

			Xsd.ConsolIdentifier consolIdentifier = jobDec.Consol.ConsolIdentifier.AddNew();
			consolIdentifier.ConsolIdentifierType = Xsd.ConsolIdentifierType.MasterWaybill;
			consolIdentifier.Value = "MAWB";

			jobDec.Shipment.ShipmentDetails.AgentReference = "JobNum";
			jobDec.Consol.ConsolDetail.ReceivingAgent.EDICode = "Forwarder";

			jobDec.Shipment.ShipmentDetails.TransportMode = Xsd.TransportMode.SEA;

			jobDec.Shipment.ShipmentDetails.TotalOuterPacksQty.Value = 10;
			jobDec.Shipment.ShipmentDetails.TotalOuterPacksQty.DimensionType = Core.Constants.PkgUnit.Package;

			jobDec.Shipment.ShipmentDetails.Weight.Value = 100;
			jobDec.Shipment.ShipmentDetails.Weight.DimensionType = Core.Constants.Weight.Kilograms;

			Xsd.Container container = jobDec.Consol.ConsolDetail.Containers.AddNew();
			container.ContainerNumber = "ContainerNum";

			jobDec.Shipment.ShipmentDetails.PortOfOrigin.Port.Value = "NZAKL";
			jobDec.Shipment.ShipmentDetails.PortofDestination.EstimatedDateTime = ZDateTime.Today;
			jobDec.Shipment.ShipmentDetails.PortofDestination.Port.Value = "AUSYD";
			jobDec.Shipment.ShipmentDetails.GoodsDescription = "Goods Description";

			Xsd.AdditionalCustomsInformation buyerCode = jobDec.Shipment.Declaration.AddCustomsDetails.AddNew();
			Xsd.AdditionalCustomsInformation supplierCode = jobDec.Shipment.Declaration.AddCustomsDetails.AddNew();
			Xsd.AdditionalCustomsInformation cusEntryCPDecPermit = jobDec.Shipment.Declaration.AddCustomsDetails.AddNew();
			Xsd.AdditionalCustomsInformation cusEntryCPDecPermitID = jobDec.Shipment.Declaration.AddCustomsDetails.AddNew();

			buyerCode.CustomsDetailValue = "CustomAttrib1";
			supplierCode.CustomsDetailValue = "CustomsRegNo";
			cusEntryCPDecPermit.CustomsDetailValue = "ON_Permit";
			cusEntryCPDecPermitID.CustomsDetailValue = "ON_CPDecNum";

			Xsd.SailingWithVesselVoyage voyage = new Xsd.SailingWithVesselVoyage();
			voyage.VoyageNo = "VoyageNo";
			voyage.VesselName = "Vessel";

			jobDec.Consol.ConsolDetail.Item = voyage;

			Xsd.ShipmentIdentifier shipmentIdentifier = jobDec.Shipment.ShipmentIdentifier.AddNew();
			shipmentIdentifier.ShipmentIdentifierType = Xsd.ShipmentIdentifierType.Housebill;
			shipmentIdentifier.Value = "HAWB";

			Xsd.EventCollection cESEvents = new Xsd.EventCollection();
			Xsd.Event @event = cESEvents.AddNew();
			@event.Information = "CLR";
			@event.PostedDateTime = ZDateTime.Today;
			@event.Code = Events.CustomsEntryStatus.Code;

			jobDec.Consol.Events.Event = cESEvents;

			jobDec.Shipment.ShipmentDetails.OrderReferences = new string[] { "Order1", "Order2", "Order3" };

			jobDec.Shipment.ShipmentDetails.Consignee.EDICode = Importer.OH_Code;

			Xsd.OrgAddress address = jobDec.Shipment.ShipmentDetails.Consignee.OrganisationDetails.Addresses.AddNew();
			address.AddressLine1 = "Addr 1";
			address.AddressLine2 = "Addr 2";
			address.CityOrSuburb = "Sydney";
			address.CompanyName = "Importer Co.";
			address.PostCode = "PostCode";

			jobDec.Shipment.ShipmentDetails.Consignor.EDICode = "Supplier";

			Xsd.ShipmentShipmentDetailsDeliver deliver = new Xsd.ShipmentShipmentDetailsDeliver();
			deliver.Address = jobDec.Shipment.ShipmentDetails.Consignee.OrganisationDetails.Addresses[0];
			deliver.DeliveryFrom = new ZDateTime(2006, 12, 12, 12, 12, 12);
			deliver.GoodsDelivered = new ZDateTime(2006, 12, 13, 12, 12, 12);

			jobDec.Shipment.ShipmentDetails.Deliver = deliver;

			Xsd.InvoiceHeader invoice = jobDec.Shipment.Invoices.AddNew();
			invoice.InvoiceNumber = "InvoiceNum";
			invoice.InvoiceDate = new ZDate(2006, 12, 12);
			invoice.InvoiceAmount.CurrencyCode = Core.Constants.CurrencyCodes.HongKong;
			invoice.InvoiceAmount.Value = 100;

			Xsd.InvoiceLine line = invoice.InvoiceLines.AddNew();
			line.InvoiceQty.Value = 10;
			line.InvoiceQty.DimensionType = Core.Constants.PkgUnit.Bag;

			Xsd.InvoiceLineLineClassification classification = new Xsd.InvoiceLineLineClassification();
			classification.OriginOfGoods = Core.Constants.CountryCodes.HongKong;
			classification.TariffLookup = "LookUp";
			classification.TariffCode.Value = "123.123.123.22";

			line.LineClassification = classification;

			line.LinePrice.Value = 100m;
			line.LinePrice.CurrencyCode = Core.Constants.CurrencyCodes.HongKong;

			line.ProductNumber = "PartNo";
			line.ProductDescription = "PartDesc";

			Xsd.InvoiceLineSummary summary = new Xsd.InvoiceLineSummary();
			summary.FOB.Value = 100m;
			summary.FOB.CurrencyCode = Core.Constants.CurrencyCodes.HongKong;
			summary.DutyPercentSpecified = true;
			summary.DutyPercent = 10m;
			summary.Duty.Value = 10m;
			summary.Duty.CurrencyCode = Core.Constants.CurrencyCodes.HongKong;

			line.Summary = summary;

			Xsd.CustomsEntry entry = jobDec.Shipment.ShipmentDetails.CustomsEntries.AddNew();
			entry.CustomsEntryNumber.Number = "CusEntryNum";
			entry.CustomsEntryNumber.Country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;

			Xsd.ChargesCharge charge = entry.Charges.AddNew();
			charge.ChargeAmount.Value = 10m;
			charge.ChargeType = CusEntryChargeTypeList.Codes.GSTAmount;

			return jobDec;
		}

		/// <summary>
		/// Remember the current company and branch and switch to a Kiwi one.
		/// </summary>
		/// <remarks>Call after base.SetUp() in your protected override void SetUp() method of your test class.
		/// Nothing changes if an invalid country code is passed.</remarks>
		/// <param name="countyCode">Use Core.Constants.CountryCodes.</param>
		/// <seealso cref="RevertToInitialCompanyAndBranch()"/>
		public void SwitchToCompanyAndBranch(string countyCode)
		{
			switch (countyCode)
			{
				case Core.Constants.CountryCodes.NewZealand:
					SwitchToCompanyAndBranchNz();
					break;

				case Core.Constants.CountryCodes.UnitedStates:
					SwitchToCompanyAndBranchUs();
					break;
			}
		}

		void RememberCurrentBranchAndDepartment()
		{
			initialUserContext = Env.CurrentUserContext;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		void SetNewCompanyBranchAndDepartment(ZGuid branchPk)
		{
			GlbDepartment bRNDepartment = FindOrCreateDepartment("BRN");
			Factory.Save();
			Env.SetUserContext(new UserContext(GlbStaff.CurrentUser.GS_LoginName, branchPk.ToGuid(), bRNDepartment.PK.ToGuid()));
		}

		void SwitchToCompanyAndBranchNz()
		{
			RememberCurrentBranchAndDepartment();

			GlbCompany newZealandCo = Factory.NewWithValidTestData<GlbCompany>(TestBusinessObjectKind.MinimumRequiredToSave);
			newZealandCo.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.NewZealand;
			newZealandCo.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
			newZealandCo.GC_Code = "NZC";
			newZealandCo.GC_Name = "New Zealand Company";
			newZealandCo.GC_Address1 = "1 somewhere st Auckland";
			newZealandCo.GC_City = "Auckland";
			newZealandCo.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "NZAKL")).PK;

			GlbBranch nZBranch = Factory.NewWithValidTestData<GlbBranch>(TestBusinessObjectKind.MinimumRequiredToSave);
			nZBranch.GB_Code = "AKL";
			nZBranch.GB_City = "Auckland";
			nZBranch.GB_RL_NKHomePort = "NZAKL";
			nZBranch.GB_GC = newZealandCo.PK;
			nZBranch.GB_BranchName = "NZ Branch";
			nZBranch.GB_Address1 = "1 somewhere st Auckland";

			SetNewCompanyBranchAndDepartment(nZBranch.PK);
		}

		void SwitchToCompanyAndBranchUs()
		{
			RememberCurrentBranchAndDepartment();

			GlbCompany unitedStatesCo = Factory.NewWithValidTestData<GlbCompany>(TestBusinessObjectKind.MinimumRequiredToSave);
			unitedStatesCo.GC_RX_NKLocalCurrency = Core.Constants.CurrencyCodes.UnitedStates;
			unitedStatesCo.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			unitedStatesCo.GC_Code = "USC";
			unitedStatesCo.GC_Name = "United States Company";
			unitedStatesCo.GC_Address1 = "1 somewhere Avenue";
			unitedStatesCo.GC_City = "Chicago";
			unitedStatesCo.GC_OH_OrgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "USCHI")).PK;

			GlbBranch usBranch = Factory.NewWithValidTestData<GlbBranch>(TestBusinessObjectKind.MinimumRequiredToSave);
			usBranch.GB_Code = "CHI";
			usBranch.GB_City = "Chicago";
			usBranch.GB_RL_NKHomePort = "USCHI";
			usBranch.GB_GC = unitedStatesCo.PK;
			usBranch.GB_BranchName = "US Branch";
			usBranch.GB_Address1 = "1 somewhere Avenue";

			SetNewCompanyBranchAndDepartment(usBranch.PK);
		}

		/// <summary>
		/// Revert the current company and branch to the previously remembered one.
		/// </summary>
		/// <remarks>Call before base.TearDown() in your protected override void TearDown() method of your test class.</remarks>
		/// <seealso cref="SwitchToNzCompanyAndBranch()"/>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void RevertToInitialCompanyAndBranch()
		{
			Env.SetUserContext(initialUserContext);
		}

		protected OrgHeader Buyer
		{
			get
			{
				if (buyer == null)
				{
					buyer = Factory.NewWithValidTestData<OrgHeader>();
					buyer.OH_FullName = "Buyer";
					buyer.OH_RL_NKClosestPort = "AUSYD";
					buyer.MainAddress.OA_Address1 = "Buyer";
					buyer.MainAddress.OA_Code = "ABCD";
					buyer.MainAddress.OA_City = "City";
					buyer.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
					buyer.OH_IsConsignee = true;
				}
				return buyer;
			}
		}
		OrgHeader buyer;

		protected OrgHeader Supplier
		{
			get
			{
				if (supplier == null)
				{
					supplier = Factory.NewWithValidTestData<OrgHeader>();
					supplier.OH_FullName = "Supplier";
					supplier.OH_RL_NKClosestPort = "NZAKL";
					supplier.MainAddress.OA_Address1 = "Supplier";
					supplier.MainAddress.OA_City = "City";
					supplier.OH_IsConsignor = true;
				}
				return supplier;
			}
		}
		OrgHeader supplier;

		public OrgHeader Forwarder
		{
			get
			{
				if (fForwarder == null)
				{
					fForwarder = Factory.New<OrgHeader>();
					fForwarder.OH_FullName = "ForwardingAgent";
					fForwarder.OH_Code = "Forwarder";
					fForwarder.OH_IsForwarder = true;
					fForwarder.MainAddress.OA_Address1 = "Addr 1";
					fForwarder.MainAddress.OA_Address2 = "Addr 2";
					fForwarder.MainAddress.OA_City = "Sydney";
					fForwarder.MainAddress.OA_State = "NSW";
					fForwarder.MainAddress.OA_PostCode = "PostCode";
				}
				return fForwarder;
			}
		}
		OrgHeader fForwarder;

		protected OrgHeader SharedShippingLine
		{
			get
			{
				if (shippingLine == null)
				{
					shippingLine = Factory.NewWithValidTestData<OrgHeader>();
					shippingLine.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierPrincipalCode, "SHL");
					Factory.Save();
				}
				return shippingLine;
			}
		}
		OrgHeader shippingLine;

		public OrgHeader Importer
		{
			get
			{
				if (fImporter == null)
				{
					fImporter = SetupImporter();
				}
				return fImporter;
			}
		}
		OrgHeader fImporter;

		public OrgHeader Sender
		{
			get
			{
				if (fSender == null)
				{
					fSender = SetupSender();
				}

				return fSender;
			}
		}
		OrgHeader fSender;

		OrgHeader SetupImporter()
		{
			OrgHeader importer = Factory.New<OrgHeader>();
			importer.OH_Code = "Importer";
			importer.OH_FullName = "Importer Co";
			importer.OH_IsConsignee = true;
			importer.OH_RL_NKClosestPort = "AUSYD";

			importer.MainAddress.OA_Address1 = "Addr 1";
			importer.MainAddress.OA_Address2 = "Addr 2";
			importer.MainAddress.OA_City = "Sydney";
			importer.MainAddress.OA_State = "NSW";
			importer.MainAddress.OA_PostCode = "PostCode";

			Factory.Save();

			OrgCusCode cusCode = importer.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.CodeTypes.GlobalTrackingName;
			cusCode.OK_CustomsRegNo = ReceiverCode;

			Factory.Save();

			importer.EDICommunicationsModes.ClientSpecificCommunicationTransport = "EMA";
			importer.EDICommunicationsModes.ClientSpecificDestination = SenderEmail;
			importer.MiscServ.OM_OH = importer.PK;

			Factory.Save();

			return importer;
		}

		public ZString SenderEmail
		{
			get { return "abc.com.au"; }
		}

		public ZString ReceiverCode
		{
			get { return "Receiver"; }
		}

		OrgHeader SetupSender()
		{
			OrgHeader sender = Factory.New<OrgHeader>();
			sender.OH_Code = "Sender";
			sender.OH_IsConsignor = true;
			sender.OH_RL_NKClosestPort = "NZAKL";
			Factory.Save();

			return sender;
		}

		public JobDeclaration CreateSimpleImportDeclaration()
		{
			JobDeclaration jobDec = GetNewDeclaration(JobMessageTypeList.Codes.Import, Core.Constants.TransportModes.Air);
			jobDec.JE_DeclarationReference = "B00001001";
			jobDec.JE_MasterBill = "MAWB";
			jobDec.JE_HouseBill = "HAWB";
			jobDec.JE_GoodsDescription = "Goods Description";
			jobDec.JE_DateOfArrival = new ZDateTime(2006, 6, 6, 15, 23, 53);
			jobDec.JE_VoyageFlightNo = "CX100";
			jobDec.JE_TotalNoOfPacks = 13;
			jobDec.JE_TotalWeight = 137m;

			Factory.Save();

			return jobDec;
		}

		public JobDeclaration GetNewDeclaration(ZString messageType, ZString transportMode)
		{
			JobDeclaration result = JobDeclaration.New(Factory);
			if (messageType == JobMessageTypeList.Codes.Import)
			{
				result.JE_RL_NKPortOfArrival = "HKHKG";
				result.JE_RL_NKOrigin = "NZCHC";
			}
			else if (messageType == JobMessageTypeList.Codes.Export)
			{
				result.JE_RL_NKPortOfArrival = "HKHKG";
				result.JE_RL_NKOrigin = "NZAKL";
			}

			result.JE_TransportMode = transportMode;
			result.JE_MessageType = messageType;
			return result;
		}

		public JobDeclaration CreateImpDeclaration()
		{
			ZTestHelper helper = new ZTestHelper(Factory);
			helper.PopulateSimpleImportDeclaration();

			JobDeclaration jobDec = helper.Declaration;

			jobDec.JE_TransportMode = Core.Constants.TransportModes.Air;
			jobDec.JE_DeclarationReference = "JobNum";
			jobDec.JE_RL_NKOrigin = "NZAKL";
			jobDec.JE_RL_NKFinalDestination = "AUSYD";
			jobDec.JE_HouseBill = "HAWB";
			jobDec.JE_MasterBill = "MAWB";
			jobDec.JE_MessageStatus = CMRImportEntryAdvice.ATDReceived.Code;
			jobDec.JE_TotalNoOfPacks = 10;
			jobDec.JE_TotalWeight = 100m;
			jobDec.JE_VoyageFlightNo = "CX100";

			jobDec.Logs.AddNew(Events.CustomsEntryStatus, CMRImportEntryAdvice.ATDReceived.Code);

			jobDec.JE_OH_Importer = Importer.PK;
			jobDec.JE_OH_Supplier = Sender.PK;
			jobDec.JE_DateAtFinalDestination = ZDateTime.Today;
			jobDec.JE_OH_Forwarder = Forwarder.PK;

			jobDec.JE_EstimatedDeliveryOrPickup = new ZDateTime(2006, 12, 20);
			jobDec.JE_CartageCompleted = new ZDateTime(2006, 12, 21);
			jobDec.JE_EntryStatus = "ATD";

			CusEntryHeader header = jobDec.CustomsEntryHeaders.AddNew();
			header.EntryNumber = "CusEntryNum";

			return jobDec;
		}

		#endregion

		#region Set Registry Items
		#region Set Valid Registry Items
		public virtual void SetValidRegistryAll()
		{
		}
		#endregion

		#region Set Invalid Registry Items
		#endregion
		#endregion

		protected OrgHeader SharedConsignee
		{
			get { return (consignee) ?? (consignee = Factory.NewWithValidTestData<OrgHeader>()); }
		}
		OrgHeader consignee;

		protected OrgHeader SharedConsignor
		{
			get { return (consignor) ?? (consignor = Factory.NewWithValidTestData<OrgHeader>()); }
		}
		OrgHeader consignor;

		public BusinessObjectFactory SharedFactory
		{
			get { return Factory; }
			set { Factory = value; }
		}

		protected BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = new BusinessObjectFactory()); }
			private set { factory = value; }
		}
		BusinessObjectFactory factory;

		IUserContext initialUserContext;

		protected RefUNLOCO GetUNLOCOAndSetIATACode(ZString portCode, ZString iataCode)
		{
			RefUNLOCO port = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, portCode));
			port.RL_IATA = iataCode;
			Factory.Save();
			return port;
		}

		protected RefUNLOCO SharedOrigin
		{
			get { return sharedOrigin ?? (sharedOrigin = GetUNLOCOAndSetIATACode("AUSYD", "ABC")); }
		}
		RefUNLOCO sharedOrigin;

		protected RefUNLOCO SharedDestination
		{
			get { return sharedDestination ?? (sharedDestination = GetUNLOCOAndSetIATACode("NZAKL", "DEF")); }
		}
		RefUNLOCO sharedDestination;

		protected OrgHeader SharedDeliveryAgent
		{
			get
			{
				if (deliveryAgent == null)
				{
					deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
					deliveryAgent.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CarrierPrincipalCode, "DLV");
					Factory.Save();
				}
				return deliveryAgent;
			}
		}
		OrgHeader deliveryAgent;

		protected RefVessel SharedVessel
		{
			get { return (vessel) ?? (vessel = Factory.NewWithValidTestData<RefVessel>()); }
		}
		RefVessel vessel;

		#region IDisposable Members

		public void Dispose()
		{
			if (initialUserContext != null)
			{
				RevertToInitialCompanyAndBranch();
			}
		}

		#endregion
	}
}
#endif
