using System;
using System.Collections;
using System.IO;
using System.Text;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using CustomsShared = Enterprise.Customs.Business;

namespace Enterprise.Client.Wow
{
	public class ContainerCustomsDeclGenerator
	{
		#region Execute

		public void Execute(INotifications notify, bool merge)
		{
			NotificationBuffer buffer = new NotificationBuffer(notify);
			ZDateTime dateBeforeGenerate = ZDateTime.Now;

			Hashtable containerKeyToContainerRecords = GetContainerKeyToContainerRecords();
			ExecuteWithFactory(containerKeyToContainerRecords, merge, buffer);
			try
			{
				notify.Notify(new InfoNotification("Saving to the database..."));
				BusinessObjectFactory.SaveTogether(FactoryProvider.Current, new ContainerCustomsDeclLastCreatedUpdater(dateBeforeGenerate));

				FactoryProvider.CreateNewWithoutSave();
			}
			catch (IOException ex1)
			{
				notify.Notify(new ErrorNotification(WowErrorType.PostToDatabaseError, ex1.Message));
			}
			catch (SqlException ex1)
			{
				notify.Notify(new ErrorNotification(WowErrorType.PostToDatabaseError, ex1.Message));
			}
			catch (ZSaveException ex1)
			{
				notify.Notify(new ErrorNotification(WowErrorType.PostToDatabaseError, ex1.Message));
			}
			catch (EmailSendFailedException ex1)
			{
				notify.Notify(new ErrorNotification(WowErrorType.PostToDatabaseError, ex1.Message));
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				Globals.Message.ShowDeveloperException(ex);
			}
		}

		#region class ContainerCustomsDeclLastCreatedUpdater

		internal class ContainerCustomsDeclLastCreatedUpdater : SaveInTransactionAction
		{
			public ContainerCustomsDeclLastCreatedUpdater(ZDateTime dateBeforeGenerate)
			{
				this.DateBeforeGenerate = dateBeforeGenerate;
			}

			public readonly ZDateTime DateBeforeGenerate;

			protected override IChangedTableNames SaveInTransaction()
			{
				WowDataRegistry.Instance.ContainerCustomsDeclLastCreated = DateBeforeGenerate;
				return ChangedTableNames.Empty;
			}

			protected override ITransactionManager BeginTransactionWithManager()
			{
				return new StubTransactionManager();
			}

			protected override bool IsInTransaction
			{
				get { return false; }
			}
		}

		#endregion

		#endregion

		#region Implementation

		void ExecuteWithFactory(IDictionary containerKeyToContainerRecords, bool merge, INotifications notify)
		{
			ArrayList declarations = new ArrayList();
			int totalCreated = 0;

			foreach (DictionaryEntry entry in containerKeyToContainerRecords)
			{
				DestinationAggregateKey key = (DestinationAggregateKey)entry.Key;
				if (!key.Destination.IsEmpty && !key.MasterBill.IsEmpty)
				{
					ArrayList containerRecords = (ArrayList)entry.Value;
					JobDeclaration declaration = CreateDeclarationFromContainerRecords(key, containerRecords, notify);
					declarations.Add(declaration);
					totalCreated++;
				}
			}

			if (merge)
			{
				MergeDeclarations(declarations, notify);
			}
			notify.Notify(new InfoNotification(totalCreated.ToString() + " declarations created"));
		}

		void MergeDeclarations(IList declarations, INotifications notify)
		{
			foreach (JobDeclaration decl in declarations)
			{
				notify.Notify(new InfoNotification(
					"Performing merge for declaration on container(s) " + GeContainerNumList(decl.CusContainers) + "."));
				decl.MessageInitiator = new LoggedSendsMessagesToAUCustoms(decl, notify);
				decl.DoMerge();
			}
		}

		ZString GeContainerNumList(CustomsShared.ICusContainerCollection<CustomsShared.BaseCusContainer> containers)
		{
			StringBuilder result = new StringBuilder();

			foreach (CusContainer current in containers)
			{
				result.Append(current.CO_ContainerNumber);
				result.Append(",");
			}

			return result.ToString().TrimEnd(',');
		}

		#region CreateDeclarationFromContainerRecords

		JobDeclaration CreateDeclarationFromContainerRecords(
			DestinationAggregateKey key, IList containerRecords, INotifications notify)
		{
			JobDeclaration decl = FactoryProvider.Current.New<JobDeclaration>();

			decl.JE_MasterBill = key.MasterBill;
			decl.JE_VesselName = key.ArrivalVessel;
			decl.JE_RL_NKFinalDestination = key.Destination;
			decl.JE_RL_NKPortOfArrival = key.Destination;

			Hashtable suppliersToContainerRecords = GetSupplierToContainerRecords(containerRecords);
			foreach (DictionaryEntry entry in suppliersToContainerRecords)
			{
				ZGuid supplierPK = (ZGuid)entry.Key;
				ArrayList containers = (ArrayList)entry.Value;

				JobComInvoiceHeader invoice = decl.Invoices.AddNew();
				foreach (OrderLineDeliverContainer container in containers)
				{
					JobComInvoiceLine newInvoiceLine = invoice.JobComInvoiceLines.AddNew();
					UpdateDeclarationAndRelatedFromContainer(container, decl, invoice, newInvoiceLine, notify);
				}
			}
			return decl;
		}

		#endregion

		#endregion

		#region GetSupplierToContainerRecords

		Hashtable GetSupplierToContainerRecords(IList containerRecords)
		{
			Hashtable result = new Hashtable();
			foreach (OrderLineDeliverContainer container in containerRecords)
			{
				ZGuid supplierPK = container.OrderLineDelivery.OrderLine.Order.SupplierPK;
				ArrayList recordsForThisSupplier = (ArrayList)result[supplierPK];
				if (recordsForThisSupplier == null)
				{
					recordsForThisSupplier = new ArrayList();
					result[supplierPK] = recordsForThisSupplier;
				}
				recordsForThisSupplier.Add(container);
			}
			return result;
		}

		#endregion

		#region GetContainerKeyToContainerRecords

		Hashtable GetContainerKeyToContainerRecords()
		{
			Hashtable result = new Hashtable();
			WoolworthsOrder[] orders = ReadOrdersWhoseContainersModifiedOrAddedAfter(WowDataRegistry.Instance.ContainerCustomsDeclLastCreated);
			foreach (WoolworthsOrder order in orders)
			{
				foreach (OrderLine line in order.OrderLines)
				{
					foreach (OrderLineDelivery delivery in line.Deliveries)
					{
						foreach (OrderLineDeliverContainer container in delivery.Containers)
						{
							if (!container.J5_ContainerNum.IsEmpty && ContainerNeedsDeclaration(container))
							{
								DestinationAggregateKey key = new DestinationAggregateKey(container);
								ArrayList recordsForThisDestination = (ArrayList)result[key];
								if (recordsForThisDestination == null)
								{
									recordsForThisDestination = new ArrayList();
									result[key] = recordsForThisDestination;
								}
								recordsForThisDestination.Add(container);
							}
						}
					}
				}
			}
			return result;
		}

		WoolworthsOrder[] ReadOrdersWhoseContainersModifiedOrAddedAfter(ZDateTime afterDate)
		{
			ZQuery orderQuery = GetFilterForOrdersWhoseContainersModifiedOrAddedAfter(afterDate);
			WoolworthsOrder[] orders = FactoryProvider.Current.Load<WoolworthsOrder>(orderQuery);
			return orders;
		}

		protected ZQuery GetFilterForOrdersWhoseContainersModifiedOrAddedAfter(ZDateTime afterDate)
		{
			var orderQuery = new ZDBOnlyQuery(typeof(WoolworthsOrder));

			var importerBranchQuery = new ZDBOnlySubQuery(typeof(OrgCompanyData), OrgCompanyDataSchema.OB_OH);
			importerBranchQuery.AddToFilter(OrgCompanyDataSchema.OB_GC, Env.CurrentCompany.PK);
			importerBranchQuery.AddToFilter(OrgCompanyDataSchema.OB_GB_ControllingBranch, Env.CurrentBranch.PK);

			var importerAddressQuery = new ZDBOnlySubQuery(typeof(OrgAddress), OrgAddressSchema.PK);
			importerAddressQuery.AddSubQuery(OrgAddressSchema.OA_OH, importerBranchQuery, JoinCondition.And);

			orderQuery.AddSubQuery(JobOrderHeaderSchema.JD_OA_BuyerAddress, importerAddressQuery, JoinCondition.And);

			if (afterDate.IsValid)
			{
				var orderLineQuery = new ZDBOnlySubQuery(typeof(OrderLine), JobOrderLineSchema.JO_JD);
				var deliveryQuery = new ZDBOnlySubQuery(typeof(OrderLineDelivery), JobOrderLineDeliverySchema.J4_JO);
				var containerQuery = new ZDBOnlySubQuery(typeof(OrderLineDeliverContainer), JobOrderLineDeliverContainerSchema.J5_J4);
				var containerLogQuery = new ZDBOnlySubQuery(typeof(StmALog), StmALogSchema.SL_Parent);

				var orderContainerModifiedOrAddedAfterFilter = GetOrderContainerModifiedOrAddedAfterFilter(afterDate);
				containerLogQuery.AddToFilter(orderContainerModifiedOrAddedAfterFilter);

				containerQuery.AddSubQuery(containerLogQuery, JoinCondition.And);
				deliveryQuery.AddSubQuery(containerQuery, JoinCondition.And);
				orderLineQuery.AddSubQuery(deliveryQuery, JoinCondition.And);
				orderQuery.AddSubQuery(orderLineQuery, JoinCondition.And);
			}

			return orderQuery;
		}

		ZQuery GetOrderContainerModifiedOrAddedAfterFilter(ZDateTime afterDate)
		{
			ZQuery eventTimeFilter = new ZQuery();
			eventTimeFilter.AddToFilter(JoinCondition.And, StmALogSchema.SL_EventTime, SQLComparisonOperator.GreaterThanOrEqualTo, afterDate);
			ZQuery editedOrAddedFilter = new ZQuery();
			editedOrAddedFilter.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.AddedARecordToTheSystem.Code);
			editedOrAddedFilter.AddToFilter(JoinCondition.Or, StmALogSchema.SL_SE_NKEvent, SQLComparisonOperator.Equal, Events.EditedARecord.Code);

			ZQuery result = new ZQuery();
			result.AddToFilter(eventTimeFilter);
			result.AddToFilter(editedOrAddedFilter, JoinCondition.And);

			return result;
		}

		bool ContainerNeedsDeclaration(OrderLineDeliverContainer orderContainer)
		{
			ZQuery filter = new ZQuery(CusContainerSchema.CO_ContainerNumber, orderContainer.J5_ContainerNum);
			CusContainer[] cusContainersWithSameContainerNumber = orderContainer.Factory.Load<CusContainer>(filter);

			ArrayList existingCusContainers = new ArrayList();
			foreach (CusContainer declContainer in cusContainersWithSameContainerNumber)
			{
				if (!declContainer.Declaration.JE_IsCancelled &&
					orderContainer.J5_Voyage.ToLower().Trim() == declContainer.JobDeclaration.JE_VoyageFlightNo.ToLower().Trim() &&
					orderContainer.J5_RV_NKArrivalVessel.ToLower().Trim() == declContainer.JobDeclaration.JE_VesselName.ToLower().Trim() &&
					orderContainer.J5_MasterBill.ToLower().Trim() == declContainer.JobDeclaration.JE_MasterBill.ToLower().Trim() &&
					orderContainer.OrderLineDelivery.J4_RL_NKDestinationPort.ToLower().Trim() == declContainer.JobDeclaration.JE_RL_NKFinalDestination.ToLower().Trim())
				{
					existingCusContainers.Add(declContainer);
				}
			}
			return (existingCusContainers.Count == 0);
		}

		#endregion

		#region Order to Declaration Column Mapping Functions

		// IF YOU CHANGE THE MAPPINGS, YOU MUST UPDATE THE WOOLWORTHS SPECIFIC DOCUMENTATION FOR THIS

		void UpdateDeclarationAndRelatedFromContainer(OrderLineDeliverContainer container,
			JobDeclaration decl, JobComInvoiceHeader invoice, JobComInvoiceLine newInvoiceLine, INotifications notify)
		{
			WoolworthsOrder order = (WoolworthsOrder)container.OrderLineDelivery.OrderLine.Order;
			WoolworthsOrderLine orderLine = (WoolworthsOrderLine)container.OrderLineDelivery.OrderLine;
			OrderLineDelivery delivery = container.OrderLineDelivery;
			CusContainer cusContainer = AddContainerToDeclaration(decl, container);

			((ISupportDataImporting)decl).IsImportingData = true;
			((ISupportDataImporting)cusContainer).IsImportingData = true;
			((ISupportDataImporting)invoice).IsImportingData = true;
			((ISupportDataImporting)newInvoiceLine).IsImportingData = true;
			try
			{
				UpdateDeclarationFromOrderContainer(order, orderLine, delivery, container, decl);
				UpdateCustomsContainerFromOrderContainer(order, orderLine, delivery, container, cusContainer);
				UpdateInvoiceFromOrderContainer(order, orderLine, delivery, container, invoice);
				CopyContainerToInvoiceLine(order, orderLine, delivery, container, newInvoiceLine, notify);
			}
			finally
			{
				((ISupportDataImporting)decl).IsImportingData = false;
				((ISupportDataImporting)cusContainer).IsImportingData = false;
				((ISupportDataImporting)invoice).IsImportingData = false;
				((ISupportDataImporting)newInvoiceLine).IsImportingData = false;
			}
		}

		CusContainer AddContainerToDeclaration(JobDeclaration decl, OrderLineDeliverContainer container)
		{
			CusContainer containerOnJobDec = decl.CusContainers.Find(container.J5_ContainerNum);

			if (containerOnJobDec == null)
			{
				decl.JE_ContainerCount++;
				containerOnJobDec = decl.CusContainers.AddNew();
				containerOnJobDec.CO_ContainerNumber = container.J5_ContainerNum;
			}

			return containerOnJobDec;
		}

		void UpdateDeclarationFromOrderContainer(
			WoolworthsOrder order, WoolworthsOrderLine orderLine, OrderLineDelivery delivery,
			OrderLineDeliverContainer container, JobDeclaration decl)
		{
			decl.JE_ContainerMode = Core.Constants.ContainerModes.FCL;
			decl.JE_DateOfArrival = container.J5_ETA;
			decl.JE_DateAtFinalDestination = container.J5_ETA;
			decl.JE_DateOfFirstArrival = container.J5_ETA;
			decl.JE_ExportDate = container.J5_ETD;
			decl.JE_DateAtOrigin = container.J5_ETD;
			decl.JE_GoodsDescription = orderLine.JO_Description;
			decl.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			decl.JE_MasterBill = container.J5_MasterBill;
			decl.ImporterDeliveryAddress.E2_OA_Address = (delivery.DeliveryPoint == null) ? ZGuid.Empty : delivery.DeliveryPoint.PK;
			decl.JE_OH_Forwarder = order.JD_OH_SendingAgent;
			decl.JE_OH_Importer = WowDataRegistry.Instance.DeclarationImporter;
			decl.JE_OH_Supplier = order.SupplierPK;
			decl.JE_RL_NKOrigin = order.JD_RL_NKPortOfLoading;
			decl.JE_RL_NKFinalDestination = delivery.J4_RL_NKDestinationPort;
			decl.JE_RL_NKPortOfArrival = delivery.J4_RL_NKDestinationPort;
			decl.JE_RL_NKPortOfLoading = order.JD_RL_NKPortOfLoading;
			decl.JE_VesselName = container.J5_RV_NKArrivalVessel;
			decl.JE_TransportMode = order.JD_TransportMode.IsEmpty && !container.J5_RV_NKArrivalVessel.IsEmpty ? Core.Constants.TransportModes.Sea : order.JD_TransportMode.ToString();
			decl.JE_VoyageFlightNo = container.J5_Voyage;
			decl.JE_TotalWeight += container.J5_Weight;
			decl.JE_TotalVolume += container.J5_Volume;
			decl.JE_TotalNoOfPacks += (int)(decimal)container.J5_PackCount;

			JobHeader.Loader loader = new JobHeader.Loader(decl);
			JobHeader job = loader.TryLoadOrCreate();
			job.SetDefaultsForJob();

			job.JH_OA_LocalChargesAddr = (order.Buyer != null) ? order.Buyer.MainAddress.PK : ZGuid.Empty;

			if (job.Department == null)
			{
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			}

			// populate the valuation basis from the buyer supplier link
			ZQuery linkFilter = new ZQuery();
			linkFilter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Buyer, order.BuyerPK);
			linkFilter.AddToFilter(OrgSupplierBuyerLinkSchema.OL_OH_Supplier, order.SupplierPK);
			OrgSupplierBuyerLink link = FactoryProvider.Current.LoadTop1<OrgSupplierBuyerLink>(linkFilter);
			if (link != null)
			{
				decl.AddInfo.ZA_ValuationBasis_Hidden = link.OL_ValuationBasis.Left(decl.AddInfo.ZA_ValuationBasis_HiddenInfo.MaxLength);
			}
		}

		void UpdateCustomsContainerFromOrderContainer(
			WoolworthsOrder order, WoolworthsOrderLine orderLine, OrderLineDelivery delivery,
			OrderLineDeliverContainer container, CusContainer cusContainer)
		{
			cusContainer.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.FCL;
			RefContainer containerType = order.Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, container.J5_RC_NKContainerType);
			if (containerType != null)
			{
				cusContainer.CO_RC = containerType.PK;
			}
			cusContainer.CO_Seal = container.J5_ContainerSeal;
			cusContainer.CO_Weight += container.J5_Weight;

			JobDeclaration declaration = (JobDeclaration)cusContainer.Declaration;

			if (declaration.Bills.Count > 0)
			{
				Package package = GetPackageByContainerNumber(container.J5_ContainerNum, declaration.Packages);

				if (package == null)
				{
					package = declaration.Packages.AddNew();
					package.CW_ContainerNoOrEquipmentNo = cusContainer.CO_ContainerNumber;
				}

				package.CW_HouseBill = declaration.Bills[0].CU_BillUniqueCode;
				package.CW_PackQty += container.J5_PackCount;
			}
		}

		Package GetPackageByContainerNumber(ZString containerNum, DeclarationLevelPackageCollection packages)
		{
			Package result = null;

			foreach (Package current in packages)
			{
				if (current.CW_ContainerNoOrEquipmentNo == containerNum)
				{
					result = current;
					break;
				}
			}

			return result;
		}

		void UpdateInvoiceFromOrderContainer(
			WoolworthsOrder order, WoolworthsOrderLine orderLine, OrderLineDelivery delivery,
			OrderLineDeliverContainer container, JobComInvoiceHeader invoice)
		{
			invoice.JZ_IncoTerm = MapIncoTermToCustomsTerm(order.JD_IncoTerm);
			invoice.JZ_InvoiceAmount += (orderLine.JO_ItemPrice * container.J5_PackCount * orderLine.JO_OuterPacks);
			invoice.JZ_InvoiceNumber = (order.Supplier != null) ? order.Supplier.OH_Code : ZString.Empty;
			invoice.JZ_OH_Buyer = order.BuyerPK;
			invoice.JZ_OH_Supplier = order.SupplierPK;
			RefCountry countryOfOrigin = (RefCountry)order.Factory.Load(typeof(RefCountry), order.JD_RN_CountryOfOrigin);
			if (countryOfOrigin != null)
			{
				invoice.AddInfo.ZA_ORG = countryOfOrigin.RN_Code;
			}

			invoice.JZ_RX_NKInvoice_Currency = (order.OrderCurrency != null) ? order.JD_RX_NKOrderCurrency : ZString.Empty;
			invoice.JZ_Volume += container.J5_Volume;
			invoice.JZ_VolumeUQ = container.J5_VolumeUQ;
			invoice.JZ_Weight += container.J5_Weight;
			invoice.JZ_WeightUQ = container.J5_WeightUQ;
			invoice.JZ_Nature10PackCount += container.J5_PackCount;
		}

		void CopyContainerToInvoiceLine(
			WoolworthsOrder order, WoolworthsOrderLine orderLine, OrderLineDelivery delivery,
			OrderLineDeliverContainer container, JobComInvoiceLine newInvoiceLine, INotifications notify)
		{
			newInvoiceLine.JI_LinePrice = (orderLine.JO_ItemPrice * container.J5_PackCount * orderLine.JO_OuterPacks);
			newInvoiceLine.JI_OrderNumber = order.JD_OrderNumber.Left(newInvoiceLine.JI_OrderNumberInfo.MaxLength);
			newInvoiceLine.JI_PartNo = orderLine.JO_Partno;
			newInvoiceLine.JI_InvoiceQuantity = (decimal)container.J5_PackCount * orderLine.JO_OuterPacks;
			newInvoiceLine.JI_CustomAttrib3 = (order.Buyer != null) ? order.Buyer.OH_Code : ZString.Empty;
			newInvoiceLine.JI_CustomAttrib4 = container.J5_ContainerNum;
			newInvoiceLine.JI_CustomDecimal1 = new ZDecimal(orderLine.JO_LineNo);

			var countryCode = GetCountryOfOriginFromContainer(container).Left(AUAddInfo.Schema.ZA_ORGMaxLength);
			if (newInvoiceLine.AggregatedZA_ORG != countryCode)
			{
				newInvoiceLine.AddInfo.ZA_ORG = countryCode;
			}

			newInvoiceLine.JI_Volume = container.J5_Volume;
			newInvoiceLine.JI_VolumeUQ = container.J5_VolumeUQ;
			newInvoiceLine.JI_Weight = container.J5_Weight;
			newInvoiceLine.JI_WeightUQ = container.J5_WeightUQ;

			// this MUST be set after the product number
			newInvoiceLine.JI_InvoiceUQ = Enterprise.Core.Constants.PkgUnit.Unit;
		}

		ZString MapIncoTermToCustomsTerm(ZString incoTerm)
		{
			switch (incoTerm.ToUpper())
			{
				case "EXW":
					return "PAF";
				case "CFR":
					return "C&F";
				case "CIP":
					return "C&I";
				case "CIF":
					return "CIF";
				case "DDU":
					return "LIS";
				default:
					return incoTerm;
			}
		}

		ZString GetCountryOfOriginFromContainer(OrderLineDeliverContainer container)
		{
			return GetCountryOfOriginFromContainer(container, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
		}

		protected ZString GetCountryOfOriginFromContainer(OrderLineDeliverContainer container, ZGuid mappingOrgPK)
		{
			ZString result = ZString.Empty;
			string uNLOCO = container.J5_CustomAttribute1.Trim();
			if (uNLOCO.Length >= 2)
			{
				result = MapCountry(container.Factory, uNLOCO.Substring(0, 2), mappingOrgPK);
			}
			return result;
		}

		ZString MapCountry(BusinessObjectFactory factory, ZString sourceCountryCode, ZGuid mappingOrgPK)
		{
			ZString result = sourceCountryCode;
			ZQuery filter = new ZQuery(OrgPatternMatchOverrideSchema.OO_OH, mappingOrgPK);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_Relationship, Core.Constants.OrgPatternMatchOverrideRelationships.Country);
			filter.AddToFilter(OrgPatternMatchOverrideSchema.OO_ForeignCode, sourceCountryCode);

			OrgPatternMatchOverride match = factory.LoadTop1<OrgPatternMatchOverride>(filter);
			if (match != null)
			{
				result = match.OO_LocalCode;
			}
			return result;
		}

		#endregion

		#region DestinationAggregateKey

		internal class DestinationAggregateKey
		{
			public DestinationAggregateKey(OrderLineDeliverContainer container)
			{
				this.Voyage = container.J5_Voyage;
				if (container.OrderLineDelivery.OrderLine.Order.JD_TransportMode == Enterprise.Core.Constants.TransportModes.Air)
				{
					this.ArrivalVessel = ZString.Empty;
				}
				else
				{
					this.ArrivalVessel = container.J5_RV_NKArrivalVessel;
				}
				this.MasterBill = container.J5_MasterBill;
				this.Destination = container.OrderLineDelivery.J4_RL_NKDestinationPort;
			}

			public DestinationAggregateKey(ZString voyage, ZString arrivalVessel, ZString mawb, ZString destination)
			{
				this.Voyage = voyage;
				this.ArrivalVessel = arrivalVessel;
				this.MasterBill = mawb;
				this.Destination = destination;
			}

			public readonly ZString Voyage;
			public readonly ZString ArrivalVessel;
			public readonly ZString MasterBill;
			public readonly ZString Destination;

			public override bool Equals(object rhs)
			{
				return
					this.MasterBill == ((DestinationAggregateKey)rhs).MasterBill &&
					this.Voyage == ((DestinationAggregateKey)rhs).Voyage &&
					this.ArrivalVessel == ((DestinationAggregateKey)rhs).ArrivalVessel &&
					this.Destination == ((DestinationAggregateKey)rhs).Destination;
			}

			public override int GetHashCode()
			{
				return this.Destination.GetHashCode();
			}
		}

		#endregion

		#region Factory

		BusinessObjectFactoryProvider FactoryProvider
		{
			get { return (fFactoryProvider) ?? (fFactoryProvider = new BusinessObjectFactoryProvider()); }
		}
		BusinessObjectFactoryProvider fFactoryProvider;

		#endregion

		internal const string DickSmithBranch = "DSE";
	}
}
