using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngine.MacroValueProviders.Utilities;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Customs.US;
using static Enterprise.Integration.DocumentWrappers;
using Res = DocumentWrappers.Res;
using ResString = DocumentWrappers.ResString;

namespace Enterprise.DocumentWrappers.Customs.Base
{
	public abstract class DocBaseJobDeclaration : DocBaseWrapperWithJobHeader, Integration.DocumentWrappers.IDocBaseJobDeclaration, IPreAlert, IDocJobDetail, IShipperDepartureNotice, ITimeSlotRequest, IDocCartageAdvice, IRequestForMissingDocuments, IContainsSuppressedFields, IDocServicesParent
	{
		internal static Type GetDocDeclarationType(BaseJobDeclaration baseJobDeclaration)
		{
#if DEBUG
			if (baseJobDeclaration is Integration.Customs._CustomsTemplate_.IJobDeclaration)
			{
				return ObjectFactory.GetType<Integration.Customs._CustomsTemplate_.IDocDeclaration>();
			}
#endif
			var result = typeof(General.DocDeclaration);
			var docWrappersProvider = GetDocWrappersProvider(baseJobDeclaration);
			if (docWrappersProvider != null)
			{
				result = docWrappersProvider.DocJobDeclarationType;
			}

			return result;
		}

		public static DocBaseJobDeclaration New(BaseJobDeclaration baseJobDeclaration, BusinessObjectFactory factoryToWrap)
		{
#if DEBUG
			if (baseJobDeclaration is Integration.Customs._CustomsTemplate_.IJobDeclaration templateDeclaration)
			{
				return (DocBaseJobDeclaration)ObjectFactory.Get<Integration.Customs._CustomsTemplate_.IDocumentWrapperProvider>().NewDocDeclaration(templateDeclaration, factoryToWrap);
			}
#endif
			DocBaseJobDeclaration result;
			var docWrappersProvider = GetDocWrappersProvider(baseJobDeclaration);
			if (docWrappersProvider != null)
			{
				result = (DocBaseJobDeclaration)docWrappersProvider.NewDocDeclarationWrapper(baseJobDeclaration, factoryToWrap);
			}
			else
			{
				result = General.DocDeclaration.New(baseJobDeclaration, factoryToWrap);
			}

			return result;
		}

		static IDocWrappersProvider GetDocWrappersProvider(BaseJobDeclaration baseJobDeclaration)
		{
			IDocWrappersProvider result = null;
			var docWrappersProviders = ObjectFactory.Get("DocWrappersProviders") as Hashtable;
			var countryCode = baseJobDeclaration.CountryCode.ToString();
			if (docWrappersProviders != null && docWrappersProviders.ContainsKey(countryCode))
			{
				var handle = (ObjectHandle)docWrappersProviders[countryCode];
				result = (IDocWrappersProvider)handle.GetObject();
			}
			return result;
		}

		public DocBaseJobDeclaration(BaseJobDeclaration baseJobDeclaration, BusinessObjectFactory factoryToWrap)
			: base(baseJobDeclaration, factoryToWrap)
		{
		}

		public override string ToString()
		{
			return DeclarationNumber;
		}

		#region Abstract

		protected abstract DocBaseCusContainerCollection CreateCusContainerCollection(ICusContainerCollection<BaseCusContainer> collectionToWrap);
		protected abstract DocBaseJobComInvoiceLineCollection CreateJobComInvoiceLineCollection(InvoiceLineCompleteCollection collectionToWrap);
		protected abstract DocBaseJobComInvoiceGroupHeaderCollection CreateJobComInvoiceGroupHeaderCollection(IJobComInvoiceGroupHeaderCollection<BaseJobComInvoiceGroupHeader> collectionToWrap);
		protected abstract DocBaseJobComInvoiceHeader CreateJobComInvoiceHeader(BaseJobComInvoiceHeader invoiceToWrap);
		protected abstract DocBaseJobComInvoiceGroupHeader CreateJobComInvoiceGroupHeader(BaseJobComInvoiceGroupHeader invoiceGroupToWrap);
		protected abstract DocBaseJobComInvoiceHeaderCollection CreateAllInvoiceHeaderCollection(InvoiceHeaderActiveCollection collectionToWrap);
		protected virtual DocBaseJobComInvoiceLineCollection CreateFSAJobComInvoiceLineCollection(InvoiceLineCompleteCollection collectionToWrap)
		{
			return null;
		}
		protected virtual DocBaseJobComInvoiceLineCollection CreateFSAJobComInvoiceLineLineOneCollection(InvoiceLineCompleteCollection collectionToWrap)
		{
			return null;
		}

		#endregion

		#region Overrides

		protected override void OnDocWrappersContextSet()
		{
			base.OnDocWrappersContextSet();

			if (IsExportMessage)
			{
				OverrideDocumentDirectionAfterItsSetByTheReport_HACK_DoNotUse_ToBeRemoved(DocumentEngineCore.DocumentSupport.DocumentDirection.DEP);
			}
			else if (IsImportMessage)
			{
				OverrideDocumentDirectionAfterItsSetByTheReport_HACK_DoNotUse_ToBeRemoved(DocumentEngineCore.DocumentSupport.DocumentDirection.ARV);
			}
		}

		#endregion

		public ZString EmailSubjectNumber
		{
			get { return DeclarationReference; }
		}

		public ZString Bills
		{
			get
			{
				ZString result = "";
				ZString separator = "";
				foreach (Bill bill in BaseJobDeclaration.Bills)
				{
					result += separator + bill.CU_HouseBill;
					separator = ",";
				}
				return result;
			}
		}

		#region Virtual

		public virtual ZString AUCusEntryNumberType
		{
			get { return ZString.Empty; }
		}

		public virtual ZString SupplierInvoiceNumbers
		{
			get
			{
				ZString result = ZString.Empty;

				foreach (DocBaseJobComInvoiceGroupHeader groupHeader in InvoiceGroupHeadersInternal)
				{
					result += groupHeader.InvoiceHeaderNumbers;
				}
				return result.TrimEndIncludingWhiteSpace(',');
			}
		}

		public DocOrganisation ImporterOfRecord
		{
			get { return ImporterOfRecordAddress?.Organisation; }
		}

		public virtual ZString EntryPortName
		{
			get { return ZString.Empty; }
		}

		public virtual ZString EntryPortCode
		{
			get { return ZString.Empty; }
		}

		public virtual ZString PaymentType
		{
			get { return ZString.Empty; }
		}

		public virtual ZString PaymentTypeDescription
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ZString Array Fields

		public ZString[] DetailedGoodsDescriptionArray
		{
			get { return GetNotesInStringArray(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, BaseJobDeclaration.NotesOfDeclarationOrShipment); }
		}

		public ZString[] MarksAndNumberArray
		{
			get { return GetNotesInStringArray(PredefinedNoteTypes.Instance.MarksAndNumbers.Description, BaseJobDeclaration.NotesOfDeclarationOrShipment); }
		}

		public ZString[] AllNotes
		{
			get { return GetAllNotesInStringArray(BaseJobDeclaration.NotesOfDeclarationOrShipment); }
		}

		public ZString[] CertificateOfOriginNoteArray
		{
			get { return GetNotesInStringArray(PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description, BaseJobDeclaration.NotesOfDeclarationOrShipment); }
		}

		#endregion

		#region ZString Fields

		public ZString MessageSubTypeDescription
		{
			get { return BaseJobDeclaration.MessageSubTypeDescription; }
		}

		public ZString MessageTypeDescription
		{
			get { return BaseJobDeclaration.MessageTypeDescription; }
		}

		public ZString Context
		{
			get { return "DECLARATION"; }
		}

		public ZString ExporterContact
		{
			get
			{
				ZString result = ZString.Empty;
				DocContacts contacts = ExporterDefaultContact;
				if (contacts != null)
				{
					result = contacts.Code;
				}
				return result;
			}
		}

		public DocContacts ExporterDefaultContact
		{
			get
			{
				return DocContacts.New(new DefaultContactFinder(BaseJobDeclaration.Supplier).DefaultContact(ContactType.Consignor), Factory);
			}
		}

		public ZString ImporterContact
		{
			get
			{
				ZString result = ZString.Empty;
				DocContacts contacts = ImporterDefaultContact;
				if (contacts != null)
				{
					result = contacts.Code;
				}
				return result;
			}
		}

		public DocContacts ImporterDefaultContact
		{
			get
			{
				return DocContacts.New(new DefaultContactFinder(BaseJobDeclaration.Importer).DefaultContact(ContactType.Consignee), Factory);
			}
		}

		public ZString RequestForMissingDocAddressDefault
		{
			get
			{
				if (BaseJobDeclaration.IsImport)
				{
					return (Importer != null) ? Importer.PostalAddress : ZString.Empty;
				}
				else if (BaseJobDeclaration.IsExport)
				{
					return (Supplier != null) ? Supplier.PostalAddress : ZString.Empty;
				}
				return "";
			}
		}

		public ZString MissingRequiredDocuments
		{
			get { return DocsAndCartage.JobDocumentsRequired.MissingRequiredDocuments; }
		}

		public ZString PackTypeDescription
		{
			get { return BaseJobDeclaration.Lookups != null ? BaseJobDeclaration.Lookups.JE_TotalNoOfPacksPackType_List.GetDescriptionFromCode(PackType) : ""; }
		}

		public ZString LineOfContainerNumbers
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();
				foreach (DocBaseCusContainer currentContainer in ContainersInternal)
				{
					builder.Append(currentContainer.ContainerNumber);
				}
				return builder.ToStringWithDelimiterBetweenAppends(", ");
			}
		}

		public ZString LineOfSealNumbers
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();
				foreach (DocBaseCusContainer currentSeal in ContainersInternal)
				{
					builder.Append(currentSeal.SealNumber);
				}
				return builder.ToStringWithDelimiterBetweenAppends(", ");
			}
		}

		public ZString LineOfContainerNumbersWithContainerMode
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (DocBaseCusContainer currentContainer in ContainersInternal)
				{
					ZString containerType = currentContainer.Type.Trim().Left(1);
					if (containerType.IsEmpty)
					{
						result += currentContainer.ContainerNumber + ", ";
					}
					else
					{
						result += containerType + "/" + currentContainer.ContainerNumber + ", ";
					}
				}

				result = result.TrimEndIncludingWhiteSpace(',');

				return result;
			}
		}

		DocContainerCollectionHelper FreightContainerSupport
		{
			get { return freightContainerSupport ?? (freightContainerSupport = new DocContainerCollectionHelper(MaximumContainersWithTypeOnALine, false)); }
		}
		DocContainerCollectionHelper freightContainerSupport;

		public ZString ContainerNumberAndTypeLine
		{
			get
			{
				return FreightContainerSupport.ContainerNumberAndType(ContainersInternal.ToIDocSimpleContainerCollection());
			}
		}

		public ZString ContainerNumberTypeAndClientRefLine
		{
			get
			{
				return FreightContainerSupport.ContainerNumberAndType(ContainersInternal.ToIDocSimpleContainerCollection(), true);
			}
		}

		public ZString ContainerNumberOnNewLine
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (DocBaseCusContainer currentContainer in ContainersInternal)
				{
					if (currentContainer != null)
					{
						result += currentContainer.ContainerNumber + System.Environment.NewLine;
					}
				}
				return result;
			}
		}

		public ZString ContainerTypeOnNewLine
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (DocBaseCusContainer currentContainer in ContainersInternal)
				{
					if (currentContainer != null)
					{
						result += currentContainer.Container != null ?
												currentContainer.Container.Code + System.Environment.NewLine :
												System.Environment.NewLine;
					}
				}
				return result;
			}
		}

		public ZBool PrintPageWithContainerNumber
		{
			get
			{
				return (ContainersInternal.Count > MaximumContainersWithTypeOnALine && AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.Value);
			}
		}

		const int MaximumContainersWithTypeOnALine = 5;

		public ZString DisbursementNoteTitle
		{
			get { return Env.Registry.DisbursementNoteTitle.Trim().ToUpper(); }
		}

		public ZString ForwardingAgentHeader
		{
			get
			{
				if (Forwarder != null)
				{
					return Res.GetString("e150ad84-e224-481b-ba8f-034ace974726", "FORWARDER");
				}
				else if (ShippingLine != null)
				{
					return TransportModeIsSea ? Res.GetString("0602f5bc-00d7-4f4f-8aaa-c7a3fc944565", "SHIPPING LINE") : Res.GetString("587c2399-cecf-4cd0-ac6a-8d2d7f86d942", "AIRLINE");
				}
				else
				{
					return Res.GetString("e150ad84-e224-481b-ba8f-034ace974726", "FORWARDER");
				}
			}
		}

		public ZString StatementFooterAddress
		{
			get { return base.GetStatementFooterAddress(Factory); }
		}

		public ZString VoyageFlightDetails
		{
			get
			{
				ZString result = ZString.Empty;

				if (TransportModeIsSea)
				{
					if (Vessel != null)
					{
						result = Vessel.ToString();
					}

					result += " / " + VoyageFlightNo;
				}
				else if (TransportModeIsAir)
				{
					result = VoyageFlightNo;
				}

				return result;
			}
		}

		public ZString Transport
		{
			get
			{
				ZString result = ZString.Empty;

				if (TransportModeIsSea)
				{
					if (Vessel != null)
					{
						result = Vessel.ToString() + " / " + VoyageFlightNo + " / " + Vessel.LloydsNumber;
					}
					else
					{
						result = VoyageFlightNo;
					}
				}
				else if (TransportModeIsAir)
				{
					result = VoyageFlightNo;
					if (ExportDate.IsValid)
					{
						result += " " + ExportDate.Date.ToShortDateString();
					}
				}

				return result;
			}
		}

		public ZString Packages
		{
			get { return PackagesCore; }
		}

		protected virtual ZString PackagesCore
		{
			get
			{
				ZString innerPack = ZString.Empty;
				ZString packageDetails = ZString.Empty;

				if (!TotalNoOfPacks.IsEmpty)
				{
					packageDetails = Res.GetString("7c8eca2d-bac4-4dc1-b049-65dad834d558", "{0} {1} (OUTER)", TotalNoOfPacks, PackType);
				}

				if (!TotalNoOfPieces.IsEmpty)
				{
					innerPack = Res.GetString("c135ceeb-56cc-4ff3-8fd5-406396ef8587", "{0} (INNER)", TotalNoOfPieces);
				}

				if (innerPack != ZString.Empty)
				{
					if (packageDetails != ZString.Empty)
					{
						packageDetails += ", " + innerPack;
					}
					else
					{
						packageDetails = innerPack;
					}
				}

				return packageDetails;
			}
		}

		public ZString FirstOrderNumber
		{
			get
			{
				ZString result = ZString.Empty;

				if (Orders.Count > 0)
				{
					result = Orders[0].OrderNumber;
				}

				return result;
			}
		}

		public ZString OrderReference
		{
			get { return DocsAndCartage.OrderItemsAsString; }
		}

		public ZString OwnerRefAndOrderRef
		{
			get
			{
				ZString result = "";
				if (IsImportMessage)
				{
					if (!OwnerRef.IsEmpty && !OrderRef.Contains(OwnerRef))
					{
						result += OwnerRef + " ";
					}
				}

				result += OrderRef;

				return result;
			}
		}

		public ZString OrderRef
		{
			get { return OrderRefCore; }
		}

		protected virtual ZString OrderRefCore
		{
			get
			{
				ZString result = ZString.Empty;

				if (BaseJobDeclaration.AttachedOrders.Count > 0)
				{
					foreach (Order order in BaseJobDeclaration.AttachedOrders)
					{
						result += order.JD_OrderNumber + ",";
					}
				}
				else
				{
					result += DocsAndCartage.OrderItemsAsString;
				}

				return result.TrimEndIncludingWhiteSpace(',');
			}
		}

		public DocOrderCollection Orders
		{
			get
			{
				DocOrderCollection orderCollection = new DocOrderCollection(Factory);
				foreach (Order order in BaseJobDeclaration.AttachedOrders)
				{
					DocOrder orderToAdd = DocOrder.New(order, Factory);
					orderCollection.Add(orderToAdd);
				}
				return orderCollection;
			}
		}

		public ZString DeclarationNumber
		{
			get { return BaseJobDeclaration.DeclarationNumber; }
		}

		public virtual ZString CustomsEntryNumber
		{
			get { return DeclarationNumber; }
		}

		public ZString EntryStatusDescription
		{
			get { return BaseJobDeclaration.JE_EntryStatusDescription; }
		}

		public ZString FCLDeliveryOrPickupEquipmentNeeded
		{
			get { return BaseJobDeclaration.JE_FCLDeliveryOrPickupEquipmentNeeded; }
		}

		public ZString AddInfo
		{
			get { return BaseJobDeclaration.JE_AddInfo; }
		}

		public ZString AgentsReference
		{
			get { return BaseJobDeclaration.JE_AgentsReference; }
		}

		public ZString ContainerMode
		{
			get
			{
				ZString result = BaseJobDeclaration.JE_ContainerMode;

				if (!result.IsEmpty && result != Core.Constants.ContainerModes.NonContainerised)
				{
					var freightContainerMode = BaseJobDeclaration.FreightContainerMode;
					if (!freightContainerMode.IsEmpty)
					{
						result = BaseJobDeclaration.FreightContainerMode;
					}
				}

				return result;
			}
		}

		public ZString DeclarationReference
		{
			get { return BaseJobDeclaration.JE_DeclarationReference; }
		}

		public ZString CartageAdviceDeclarationReference
		{
			get { return CartageAdviceDeclarationReferenceCore; }
		}

		protected virtual ZString CartageAdviceDeclarationReferenceCore
		{
			get { return DeclarationReference; }
		}

		public ZString EFTMode
		{
			get { return BaseJobDeclaration.JE_EFTMode; }
		}

		public ZString EntryStatus
		{
			get { return BaseJobDeclaration.JE_EntryStatus; }
		}

		public ZString ExportGoodsType
		{
			get { return BaseJobDeclaration.JE_ExportGoodsType; }
		}

		public ZString Folio
		{
			get { return BaseJobDeclaration.JE_Folio; }
		}

		public ZString GoodsDescription
		{
			get { return GoodsDescriptionCore; }
		}

		protected virtual ZString GoodsDescriptionCore
		{
			get { return BaseJobDeclaration.JE_GoodsDescription; }
		}

		public ZString LloydsIMO
		{
			get { return BaseJobDeclaration.JE_LloydsIMO; }
		}

		public ZString MasterBill
		{
			get { return BaseJobDeclaration.JE_MasterBill; }
		}

		public ZString MergeBy
		{
			get { return BaseJobDeclaration.JE_MergeBy; }
		}

		public ZString MessageSubType
		{
			get { return BaseJobDeclaration.JE_MessageSubType; }
		}

		public ZString MessageType
		{
			get { return BaseJobDeclaration.JE_MessageType; }
		}

		public virtual bool IsReconciliation
		{
			get { return false; }
		}

		public ZString HouseBill
		{
			get { return BaseJobDeclaration.JE_HouseBill; }
		}

		public ZString HouseBillAndIssueDate
		{
			get { return FreightHelperClass.FormatBillAndIssueDate(HouseBill, HouseBillIssueDate); }
		}

		public ZString OperationalStatus
		{
			get { return BaseJobDeclaration.JE_OperationalStatus; }
		}

		public ZString OwnerRef
		{
			get { return OwnerRefCore; }
		}

		protected virtual ZString OwnerRefCore
		{
			get { return BaseJobDeclaration.JE_OwnerRef; }
		}

		public virtual ZString PaymentMethod
		{
			get { return BaseJobDeclaration.JE_PaymentMethod; }
		}

		public ZString ShipmentIncoTerm
		{
			get { return BaseJobDeclaration.JE_ShipmentIncoTerm; }
		}

		public ZString ShipmentIncoTermDescription
		{
			get { return BaseJobDeclaration.Lookups.IncoTermList.GetDescriptionFromCode(ShipmentIncoTerm); }
		}

		public ZString PackType
		{
			get { return BaseJobDeclaration.JE_TotalNoOfPacksPackType; }
		}

		public ZString WeightUQ
		{
			get { return BaseJobDeclaration.JE_TotalWeightUnit; }
		}

		public ZString TransportMode
		{
			get { return BaseJobDeclaration.JE_TransportMode; }
		}

		public ZString VoyageFlightNo
		{
			get { return BaseJobDeclaration.JE_VoyageFlightNo; }
		}

		public ZString VolumeUQ
		{
			get { return BaseJobDeclaration.JE_TotalVolumeUnit; }
		}

		public ZString VoyageFlightHeading
		{
			get
			{
				ZString result = ZString.Empty;

				if (TransportModeIsSea)
				{
					result = Res.GetString("a336e8e8-60e5-4586-95dd-c88426f38aaf", "VESSEL / VOYAGE NO.");
				}
				else if (TransportModeIsAir)
				{
					result = Res.GetString("dcf29014-bc62-457c-a0a6-1079c4afd6d9", "FLIGHT NO.");
				}

				return result;
			}
		}

		public ZString TransportHeading
		{
			get
			{
				ZString result = ZString.Empty;

				if (TransportModeIsSea)
				{
					result = Res.GetString("c958ea7b-c5c2-4ced-bb98-ba8a2ba63a6c", "VESSEL / VOYAGE NO. / IMO(Lloyds)");
				}
				else if (TransportModeIsAir)
				{
					result = Res.GetString("3cb7a9af-0c35-4109-8211-f793b199d3e8", "FLIGHT NO. & DATE");
				}

				return result;
			}
		}

		public ZString HouseBillHeading
		{
			get
			{
				ZString result = ZString.Empty;

				if (TransportModeIsSea)
				{
					result = Res.GetString("0df73cee-5d9d-485d-b45d-6ffc8d1bf905", "HOUSE BILL OF LADING");
				}
				else if (TransportModeIsAir)
				{
					result = Res.GetString("5bd3c284-dcf7-4017-bff5-03ac893c4f1f", "HAWB");
				}
				else if (TransportModeIsRail)
				{
					result = Res.GetString("c6efe938-21ae-409b-a0d8-25c2314cb427", "HOUSE BILL");
				}
				else if (TransportModeIsRoad)
				{
					result = Res.GetString("c6efe938-21ae-409b-a0d8-25c2314cb427", "HOUSE BILL");
				}
				else if (TransportModeIsPost)
				{
					result = Res.GetString("e40f48d4-7681-43ce-9455-8644590c723c", "PARCEL POST NUMBERS");
				}
				else
				{
					result = Res.GetString("c6efe938-21ae-409b-a0d8-25c2314cb427", "HOUSE BILL");
				}

				return result;
			}
		}

		public ZString HouseBillAndIssueHeading
		{
			get { return FreightHelperClass.FormatBillAndIssueHeading(HouseBillHeading, HouseBillIssueDate); }
		}

		public ZString MasterBillHeading
		{
			get
			{
				ZString result = ZString.Empty;

				if (TransportModeIsSea)
				{
					result = Res.GetString("e6b63042-5196-4f5f-a641-ebc49dda86f6", "OCEAN BILL OF LADING");
				}
				else if (TransportModeIsAir)
				{
					result = Res.GetString("8a0814d2-6716-4474-910a-6dd25790b337", "MAWB");
				}
				else if (TransportModeIsRail)
				{
					result = Res.GetString("61328c37-5b8a-4391-9907-6163139eafeb", "MASTER BILL");
				}
				else if (TransportModeIsRoad)
				{
					result = Res.GetString("61328c37-5b8a-4391-9907-6163139eafeb", "MASTER BILL");
				}
				else
				{
					result = Res.GetString("61328c37-5b8a-4391-9907-6163139eafeb", "MASTER BILL");
				}

				return result;
			}
		}

		public ZString MasterBillAndIssueHeading
		{
			get { return FreightHelperClass.FormatBillAndIssueHeading(MasterBillHeading, MasterBillIssueDate); }
		}

		public ZString EquipmentType
		{
			get
			{
				ZString result = ZString.Empty;
				result = FCLDeliveryOrPickupEquipmentNeeded;
				if (!result.IsEmpty)
				{
					if (IsExportMessage)
					{
						result += " - " + BaseJobDeclaration.DocsAndCartage.Lookups.PickupEquipmentNeededList.GetDescriptionFromCode(result);
					}
					else
					{
						result += " - " + BaseJobDeclaration.DocsAndCartage.Lookups.DeliveryEquipmentNeededList.GetDescriptionFromCode(result);
					}
				}
				return result;
			}
		}

		public ZString ImportExportText
		{
			get
			{
				ZString result = ZString.Empty;

				if (IsImportMessage)
				{
					result = Res.GetString("819e6b8d-d6ea-4ea6-b021-e6d2b71e4f06", "Import");
				}
				else if (IsExportMessage)
				{
					result = Res.GetString("310d749c-059a-4c0d-b7f3-ce9705f67195", "Export");
				}

				return result;
			}
		}

		public ZString PhysicalPostalAddressForImporter
		{
			get
			{
				ZString result = ZString.Empty;

				if (PhysicalAddressForImporter != null && CurrentCompany != null)
				{
					WrapperPostalAddressFormatter addressFormatter = new WrapperPostalAddressFormatter(PhysicalAddressForImporter, CurrentCompany);
					if (addressFormatter != null)
					{
						result = addressFormatter.PostalAddress();
					}
				}

				return result;
			}
		}

		public ZString PhysicalPostalAddressForExporter
		{
			get
			{
				ZString result = ZString.Empty;

				if (PhysicalAddressForExporter != null && CurrentCompany != null)
				{
					WrapperPostalAddressFormatter addressFormatter = new WrapperPostalAddressFormatter(PhysicalAddressForExporter, CurrentCompany);
					if (addressFormatter != null)
					{
						result = addressFormatter.PostalAddress();
					}
				}

				return result;
			}
		}

		public ZString TotalInvoiceLineCustomAttrib2
		{
			get
			{
				ZDecimal total = 0M;
				ZDecimal parsedValue = 0M;
				foreach (DocBaseJobComInvoiceLine line in InvoiceLinesInternal)
				{
					if (ZDecimal.TryParse(line.CustomAttrib2, out parsedValue))
					{
						total += parsedValue;
					}
				}
				return FormatNumber(total);
			}
		}

		public ZString TodaysDateYYYYMMDD
		{
			get { return ZDateTime.Now.ToString("yyyy MM dd"); }
		}

		public ZString ContainerWeightHeading
		{
			get
			{
				foreach (DocBaseCusContainer container in ContainersInternal)
				{
					if (!container.Weight.IsEmpty)
					{
						return Res.GetString("ee864740-f7aa-43b0-9f84-94be6aab29e7", "Weight");
					}
				}
				return ZString.Empty;
			}
		}
		public ZString CertificateOfOriginClause
		{
			get { return DocumentsDataRegistry.Instance.CertificateOfOriginStandardClause.Value; }
		}

		public ZString RequestForMissingDocumentsInstruction
		{
			get { return Env.Registry.ShipmentRequestForMissingDocumentsClause; }
		}

		public ZString ShipmentOrBrokerageNumber
		{
			get { return DeclarationReference; }
		}

		public ZString DeclarationOrConsolNumber
		{
			get { return DeclarationNumber; }
		}

		public ZString ImporterABNOrCID
		{
			get { return GetOrganisationABNOrCID(Importer); }
		}

		public ZString BrokerABNOrCID
		{
			get { return GetOrganisationABNOrCID(Broker); }
		}

		public ZString ImportersName
		{
			get { return BaseJobDeclaration.Importer == null ? ZString.Empty : BaseJobDeclaration.Importer.OH_FullName; }
		}

		public ZString SchDEntryWithDesc
		{
			get { return SchDEntryWithDescCore; }
		}

		protected virtual ZString SchDEntryWithDescCore
		{
			get { return BaseJobDeclaration.JE_CustomsOffice; }
		}

		public ZString ReleaseStatus
		{
			get { return ReleaseStatusCore; }
		}

		protected virtual ZString ReleaseStatusCore
		{
			get { return ZString.Empty; }
		}

		public ZString ReleaseStatusDescription
		{
			get { return ReleaseStatusDescriptionCore; }
		}

		protected virtual ZString ReleaseStatusDescriptionCore
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region ZDateTime Fields

		public ZDateTime ETD
		{
			get { return DateAtOrigin; }
		}

		public ZDateTime ETA
		{
			get { return DateAtFinalDestination; }
		}

		public ZDateTime ConversionDate
		{
			get { return ((ICurrencyConverterProvider)BaseJobDeclaration).CurrencyConverter.DateForRate; }
		}

		public ZDateTime EstimatedDeliveryOrPickup
		{
			get { return BaseJobDeclaration.JE_EstimatedDeliveryOrPickup; }
		}

		public ZDateTime DeliveryOrPickupRequiredBy
		{
			get { return BaseJobDeclaration.JE_DeliveryOrPickupRequiredBy; }
		}

		public ZDateTime DateAtFinalDestination
		{
			get { return BaseJobDeclaration.JE_DateAtFinalDestination; }
		}

		public ZDateTime DateAtOrigin
		{
			get { return BaseJobDeclaration.JE_DateAtOrigin; }
		}

		public ZDateTime DateOfArrival
		{
			get { return BaseJobDeclaration.JE_DateOfArrival; }
		}

		public ZDateTime DateOfFirstArrival
		{
			get { return BaseJobDeclaration.JE_DateOfFirstArrival; }
		}

		public ZDateTime EntryAuthorisationDate
		{
			get { return BaseJobDeclaration.JE_EntryAuthorisationDate; }
		}

		public ZDateTime EntrySubmittedDate
		{
			get { return BaseJobDeclaration.JE_EntrySubmittedDate; }
		}

		public virtual ZDateTime ExportDate
		{
			get { return BaseJobDeclaration.JE_ExportDate; }
		}

		public ZDateTime SystemCreateTime
		{
			get { return BaseJobDeclaration.JE_SystemCreateTimeUtc; }
		}

		public ZDateTime SystemLastEditTime
		{
			get { return BaseJobDeclaration.JE_SystemLastEditTimeUtc; }
		}

		public ZDateTime CartageAdvised
		{
			get { return BaseJobDeclaration.JP_Calc_CartageAdvised; }
		}

		public ZDateTime CartageCompleted
		{
			get { return BaseJobDeclaration.JE_CartageCompleted; }
		}

		public ZDateTime StorageCommenceDate
		{
			get { return TransportModeIsAir || (Core.Constants.ContainerModes.IsLCLType(ContainerMode)) ? LCLStorageCommenceDate : FCLStorageCommenceDate; }
		}

		public ZDateTime LCLStorageCommenceDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (DocsAndCartage != null)
				{
					result = DocsAndCartage.LCLStorageCommences;
				}

				if (result.IsEmpty && BaseJobDeclaration.CusContainers.Count > 0)
				{
					result = BaseJobDeclaration.CusContainers[0].LCLStorageCommences;
				}
				return result;
			}
		}

		public ZDateTime FCLStorageCommenceDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (DocsAndCartage != null)
				{
					result = DocsAndCartage.FCLStorageCommences;
				}

				if (result.IsEmpty && BaseJobDeclaration.CusContainers.Count > 0)
				{
					result = BaseJobDeclaration.CusContainers[0].ArrivalCTOStorageStartDate;
				}
				return result;
			}
		}

		public ZDateTime HouseBillIssueDate
		{
			get { return Shipment != null ? Shipment.HouseBillIssueDate : ZDateTime.Empty; }
		}

		public ZDateTime MasterBillIssueDate
		{
			get { return Shipment != null ? Shipment.MasterBillIssueDate : ZDateTime.Empty; }
		}

		public ZDateTime ReleaseDate
		{
			get { return BaseJobDeclaration.JE_EntryAuthorisationDate; }
		}

		#endregion

		#region ZDecimal Fields

		public ZDecimal TotalFOBInLocalCurrency
		{
			get { return BaseJobDeclaration.TotalFOBInLocalCurrency.Amount; }
		}

		public ZDateTime DeliveryOrPickupLabourTime
		{
			get { return BaseJobDeclaration.JE_DeliveryOrPickupLabourTime; }
		}

		public ZDecimal DeliveryOrPickupLabourCharge
		{
			get { return BaseJobDeclaration.JE_DeliveryOrPickupLabourCharge; }
		}

		public ZDecimal DemurrageOnDeliveryOrPickupCharge
		{
			get { return BaseJobDeclaration.JE_PickupOrDeliveryTruckWaitCharge; }
		}

		public ZDecimal TotalVolume
		{
			get { return BaseJobDeclaration.JE_TotalVolume; }
		}

		public ZDecimal TotalWeight
		{
			get { return BaseJobDeclaration.GrossWeight.Amount; }
		}

		public ZDecimal TotalWeightInKG
		{
			get { return BaseJobDeclaration.GrossWeight.InKilogramsSafe; }
		}

		public ZDecimal TotalExportFOBAmount
		{
			get
			{
				ZDecimal result = 0;
				if (BaseJobDeclaration.Invoices != null && BaseJobDeclaration.Invoices.Count > 0)
				{
					foreach (BaseJobComInvoiceHeader invoiceHeader in BaseJobDeclaration.Invoices)
					{
						result += invoiceHeader.JZ_Calc_FOBAmount;
					}
				}
				return result;
			}
		}

		#endregion

		#region ZByte Fields

		public ZDateTime DemurrageOnDeliveryOrPickupTime
		{
			get { return BaseJobDeclaration.JE_PickupOrDeliveryTruckWaitTime; }
		}

		public ZDecimal LandedCostByCost
		{
			get { return (decimal)BaseJobDeclaration.JE_LandedCostByCost / 100; }
		}

		public ZDecimal LandedCostByUnits
		{
			get { return (decimal)BaseJobDeclaration.JE_LandedCostByUnits / 100; }
		}

		public ZDecimal LandedCostByVolume
		{
			get { return (decimal)BaseJobDeclaration.JE_LandedCostByVolume / 100; }
		}

		public ZDecimal LandedCostByWeight
		{
			get { return (decimal)BaseJobDeclaration.JE_LandedCostByWeight / 100; }
		}

		#endregion

		#region ZShort Fields

		public ZShort ContainerCount
		{
			get { return BaseJobDeclaration.JE_ContainerCount; }
		}

		#endregion

		#region ZBool Fields

		public ZBool IsPersonalEffects
		{
			get { return BaseJobDeclaration.JE_IsPersonalEffects; }
		}

		public ZBool IsImportMessage
		{
			get { return BaseJobDeclaration.IsImport; }
		}

		public ZBool IsExportMessage
		{
			get { return BaseJobDeclaration.IsExport; }
		}

		public ZBool IsUnattachedOrder
		{
			get { return ZBool.False; }
		}

		public ZBool TransportModeIsPost
		{
			get { return BaseJobDeclaration.IsPost; }
		}

		public ZBool TransportModeIsRail
		{
			get { return BaseJobDeclaration.IsRail; }
		}

		public ZBool TransportModeIsRoad
		{
			get { return BaseJobDeclaration.IsRoad; }
		}

		public ZBool TransportModeIsOther
		{
			get { return !TransportModeIsAir && !TransportModeIsSea && !TransportModeIsPost; }
		}

		public ZBool IsExWarehouse
		{
			get { return BaseJobDeclaration.IsExWarehouse; }
		}

		public ZBool DisplayLogo
		{
			get { return (ZBool)DocumentsDataRegistry.Instance.DisplayLogo.Value; }
		}

		public ZBool IsBulkLike
		{
			get { return BaseJobDeclaration.IsBulk || BaseJobDeclaration.IsLiquid || BaseJobDeclaration.HasBreakBulk; }
		}

		public ZBool IsFCLLike
		{
			get { return PrintAsContainers; }
		}

		#endregion

		#region ZInt Fields

		public ZInt TotalNoOfPacks
		{
			get { return BaseJobDeclaration.JE_TotalNoOfPacks; }
		}

		public ZInt TotalNoOfPieces
		{
			get { return BaseJobDeclaration.JE_TotalNoOfPieces; }
		}

		#endregion

		#region Wrapper Fields

		#region DocAddress

		public DocDocAddress PickupAddress
		{
			get
			{
				if (IsExportMessage)
				{
					if (BaseJobDeclaration.SupplierPickupAddress.IsValidAddress)
					{
						return DocDocAddress.New(BaseJobDeclaration.SupplierPickupAddress, Factory);
					}
					else if (Supplier != null)
					{
						return Supplier.PickUpDocAddress;
					}
				}
				else
				{
					if (IsFCLLike || IsBulkLike)
					{
						return CTOAddress;
					}

					return DepotAddress;
				}

				return null;
			}
		}

		public DocDocAddress DeliverToAddress
		{
			get
			{
				if (IsExportMessage)
				{
					if (IsFCLLike || IsBulkLike)
					{
						return CTOAddress;
					}

					return DepotAddress;
				}
				else
				{
					if (BaseJobDeclaration.ImporterDeliveryAddress.IsValidAddress)
					{
						return DocDocAddress.New(BaseJobDeclaration.ImporterDeliveryAddress, Factory);
					}
					else if (Importer != null)
					{
						return Importer.DeliverDocAddress;
					}
				}

				return null;
			}
		}

		public DocDocAddress GoodsAvailableAt
		{
			get
			{
				if (ContainerMode == Core.Constants.ContainerModes.FCL)
				{
					return CTOAddress;
				}
				else if (ContainerMode == Core.Constants.ContainerModes.Containerised)
				{
					return GetDepotFromContainerType();
				}
				else
				{
					return DepotAddress;
				}
			}
		}

		public DocDocAddress ContainerParkAddress
		{
			get
			{
				if (fContainerParkAddress == null)
				{
					fContainerParkAddress = DocDocAddress.New(BaseJobDeclaration.ContainerYardDocAddress.Address, Factory);
				}
				return fContainerParkAddress;
			}
		}
		DocDocAddress fContainerParkAddress;

		public DocDocAddress CTOAddress
		{
			get
			{
				if (fCTOAddress == null)
				{
					fCTOAddress = DocDocAddress.New(BaseJobDeclaration.ContainerTerminalOperatorDocAddress.Address, Factory);
				}
				return fCTOAddress;
			}
		}
		DocDocAddress fCTOAddress;

		public DocDocAddress DepotAddress
		{
			get
			{
				if (fDepotAddress == null)
				{
					fDepotAddress = DocDocAddress.New(BaseJobDeclaration.DepotDocAddress.Address, Factory);
				}
				return fDepotAddress;
			}
		}
		DocDocAddress fDepotAddress;

		public DocDocAddress WarehouseAddress
		{
			get { return DocDocAddress.New(BaseJobDeclaration.WarehouseAddress, Factory); }
		}

		public DocDocAddress ClientPickupDeliveryAddress
		{
			get { return DocDocAddress.New(BaseJobDeclaration.ClientPickupDeliveryAddress, Factory); }
		}

		public DocDocAddress ImporterOfRecordAddress
		{
			get { return DocDocAddress.New(ImporterOfRecordAddressCore, Factory); }
		}

		protected virtual OrgAddress ImporterOfRecordAddressCore
		{
			get
			{
				return BaseJobDeclaration?.Importer?.MainAddress;
			}
		}

		public DocDocAddress UltimateConsigneeAddress
		{
			get { return DocDocAddress.New(UltimateConsigneeAddressCore, Factory); }
		}

		protected virtual OrgAddress UltimateConsigneeAddressCore
		{
			get { return BaseJobDeclaration.ConsigneeAddress; }
		}

		#endregion

		#region DocOrganisation

		public DocOrganisation LocalParty
		{
			get { return DocOrganisation.New(BaseJobDeclaration.LocalParty, Factory); }
		}

		public DocOrganisation ForwardingAgentMerchant
		{
			get
			{
				if (Forwarder != null)
				{
					return Forwarder;
				}
				else if (ShippingLine != null)
				{
					return ShippingLine;
				}

				return null;
			}
		}

		public DocOrganisation Cartage
		{
			get
			{
				DocOrganisation result = null;

				if (DocsAndCartage != null)
				{
					result = IsExportMessage ? DocsAndCartage.PickupCartageCo : DocsAndCartage.DeliveryCartageCo;
				}

				return result;
			}
		}

		public DocOrganisation CartageOrganisation
		{
			get
			{
				if (Cartage == null)
				{
					ZGuid headerPK = ZGuid.Empty;

					if (TransportModeIsSea)
					{
						if (ContainerMode == Core.Constants.ContainerModes.FCL)
						{
							headerPK = FreightDataRegistry.Instance.FCLCartageCompany.Value;
						}
						else if (ContainerMode == Core.Constants.ContainerModes.Containerised)
						{
							ZString containerType = "";
							if (ContainersInternal.Count > 0)
							{
								containerType = ContainersInternal[0].Type;
							}

							if (containerType == Core.Constants.ContainerModes.FCL || containerType == Core.Constants.ContainerModes.FCLMixedShipper)
							{
								headerPK = FreightDataRegistry.Instance.FCLCartageCompany.Value;
							}
							else
							{
								headerPK = FreightDataRegistry.Instance.LCLCartageCompany.Value;
							}
						}
						else
						{
							headerPK = FreightDataRegistry.Instance.LCLCartageCompany.Value;
						}
					}
					else if (TransportModeIsAir)
					{
						headerPK = FreightDataRegistry.Instance.AIRCartageCompany.Value;
					}

					OrgHeader header = (OrgHeader)BaseJobDeclaration.Factory.Load(typeof(OrgHeader), headerPK);
					return DocOrganisation.New(header, Factory);
				}
				else
				{
					return Cartage;
				}
			}
		}

		public DocOrganisation DeliveryOrPickupCartageCo
		{
			get { return DocOrganisation.New(BaseJobDeclaration.DeliveryOrPickupCartageCo, Factory); }
		}

		public DocOrganisation Forwarder
		{
			get { return DocOrganisation.New(BaseJobDeclaration.Forwarder, Factory); }
		}

		public DocOrganisation Importer
		{
			get { return GetImporter(); }
		}

		protected virtual DocOrganisation GetImporter()
		{
			return DocOrganisation.New(BaseJobDeclaration.Importer, Factory);
		}

		public DocOrganisation Supplier
		{
			get { return GetSupplier(); }
		}

		protected virtual DocOrganisation GetSupplier()
		{
			return DocOrganisation.New(BaseJobDeclaration.Supplier, Factory);
		}

		public DocOrganisation ShippingLine
		{
			get { return DocOrganisation.New(BaseJobDeclaration.ShippingLine, Factory); }
		}

		public DocOrganisation ExportForwarder
		{
			get
			{
				if (Forwarder != null)
				{
					return Forwarder;
				}
				else if (Shipment != null && Shipment.Consol != null)
				{
					return Shipment.Consol.SendingForwarder;
				}
				else
				{
					return null;
				}
			}
		}

		public DocOrganisation PickupAddressOrganisation
		{
			get { return PickupAddress != null ? PickupAddress.Organisation : null; }
		}

		public DocOrganisation DeliverToAddressOrganisation
		{
			get { return DeliverToAddress != null ? DeliverToAddress.Organisation : null; }
		}

		#endregion

		#region DocContacts

		public ZInt NotifyPartyCount
		{
			get { return 1; }
		}

		public DocContacts NotifyParty
		{
			get
			{
				OrgContact result = new DefaultContactFinder(BaseJobDeclaration.Importer).DefaultContact(ContactType.NotifyParty);
				return DocContacts.New(result, Factory);
			}
		}

		public DocContacts NotifyParty2
		{
			get { return null; }
		}

		public DocContacts NotifyParty3
		{
			get { return null; }
		}

		#endregion

		#region DocUNLOCO

		public DocUNLOCO FinalDestination
		{
			get { return DocUNLOCO.New(FinalDestinationCore, Factory); }
		}

		protected virtual RefUNLOCO FinalDestinationCore
		{
			get { return BaseJobDeclaration.FinalDestination; }
		}

		public DocUNLOCO Origin
		{
			get { return DocUNLOCO.New(OriginCore, Factory); }
		}

		protected virtual RefUNLOCO OriginCore
		{
			get { return BaseJobDeclaration.Origin; }
		}

		public DocUNLOCO PortOfArrival
		{
			get { return DocUNLOCO.New(BaseJobDeclaration.PortOfArrival, Factory); }
		}

		public DocUNLOCO PortOfFirstArrival
		{
			get { return DocUNLOCO.New(BaseJobDeclaration.PortOfFirstArrival, Factory); }
		}

		public DocUNLOCO PortOfLoading
		{
			get { return DocUNLOCO.New(BaseJobDeclaration.PortOfLoading, Factory); }
		}

		#endregion

		#region DocJobInvoicingJob

		public DocJobInvoicingJob JobInvoicingJob
		{
			get { return BaseJobDeclaration.Job != null ? DocJobInvoicingJob.New(BaseJobDeclaration.Factory, BaseJobDeclaration.Job.PK) : null; }
		}

		#endregion

		protected DocBankAccount ReceiptBankAccount;
		public DocBankAccount BankAccount
		{
			get
			{
				if (ReceiptBankAccount == null)
				{
					AccBankAccount fReceiptBankAccount = AccBankAccount.GetDefaultReceiptBankAccount(BaseJobDeclaration.LocalCurrencyCode, GlbBranch.CurrentBranch, Factory);
					if (fReceiptBankAccount != null)
					{
						ReceiptBankAccount = DocBankAccount.New(fReceiptBankAccount, Factory);
					}
				}
				return ReceiptBankAccount;
			}
		}

		public DocBranch Branch
		{
			get { return DocBranch.New(BaseJobDeclaration.Branch, Factory); }
		}

		public override DocJobHeader JobHeader
		{
			get { return DocJobHeader.New(BaseJobDeclaration.Job, Factory); }
		}

		public DocForwardingShipment Shipment
		{
			get { return DocForwardingShipment.New(BaseJobDeclaration.Shipment, Factory); }
		}

		public DocServiceLevel ServiceLevel
		{
			get { return DocServiceLevel.New(BaseJobDeclaration.ServiceLevel, Factory); }
		}

		public DocVessel Vessel
		{
			get { return DocVessel.New(BaseJobDeclaration.Vessel, Factory); }
		}

		public DocJobDocsAndCartage DocsAndCartage
		{
			get { return DocJobDocsAndCartage.New(BaseJobDeclaration.DocsAndCartage, Factory); }
		}

		public DocAddress PhysicalAddressForExporter
		{
			get { return DocAddress.New(BaseJobDeclaration.PhysicalAddressForExporter, Factory); }
		}

		public DocAddress PhysicalAddressForImporter
		{
			get { return DocAddress.New(BaseJobDeclaration.PhysicalAddressForImporter, Factory); }
		}

		public DocStaff BrokerStaff
		{
			get { return DocStaff.New(Declarant, Factory); }
		}

		protected GlbStaff Declarant
		{
			get
			{
				if (fDeclarant == null)
				{
					fDeclarant = new CachedProperty<GlbStaff>(Factory, delegate
					{ return DeclarantDelegate; });
				}
				return fDeclarant.Value;
			}
		}
		CachedProperty<GlbStaff> fDeclarant;

		protected virtual GlbStaff DeclarantDelegate
		{
			get { return ((BaseJobDeclaration)WrappedObject).CusAgent; }
		}

		#endregion

		#region Notes

		public enum InstructionType
		{
			Cartage,
			Handling,
			Special,
		}

		ZString OrganizationInstructions(InstructionType instType)
		{
			ZString result = ZString.Empty;
			if (IsExportMessage)
			{
				if (PickupAddressOrganisation != null)
				{
					result = OrgInstructionsForDirection(PickupAddressOrganisation, instType, nameof(StmNoteContextDirection.E));
				}
			}
			else
			{
				if (DeliverToAddressOrganisation != null)
				{
					result = OrgInstructionsForDirection(DeliverToAddressOrganisation, instType, nameof(StmNoteContextDirection.I));
				}
			}
			return result;
		}

		ZString OrgInstructionsForDirection(DocOrganisation pickupOrDeliverOrg, InstructionType instType, ZString contextDirection)
		{
			ZString result = ZString.Empty;
			switch (instType)
			{
				case InstructionType.Cartage:
					result = pickupOrDeliverOrg.GetCartageInstructionsByDirectionAndTransportOrContainerMode(contextDirection, TransportMode, ContainerMode);
					break;
				case InstructionType.Handling:
					result = pickupOrDeliverOrg.GetHandlingInstructionsByDirectionAndTransportOrContainerMode(contextDirection, TransportMode, ContainerMode);
					break;
				case InstructionType.Special:
					result = pickupOrDeliverOrg.GetSpecialInstructionsByDirectionAndTransportOrContainerMode(contextDirection, TransportMode, ContainerMode);
					break;
			}

			return result;
		}

		public virtual ZString CartageInstructions
		{
			get { return PickupOrDeliveryCartageInstructions(BaseJobDeclaration.NotesOfDeclarationOrShipment); }
		}

		/// <summary>
		/// Return cartage instructions entered on the job.
		/// If no instructions on the job, only then fall back to cartage instructions on the respective organization.
		/// </summary>
		public ZString DeclarationOrOrgCartageInstructions
		{
			get
			{
				ZString result = ZString.Empty;
				if (!CartageInstructions.IsEmpty)
				{
					result = (CartageInstructions + "\n");
				}
				else
				{
					result = OrganizationInstructions(InstructionType.Cartage);
				}
				return result.TrimEnd('\n');
			}
		}

		public ZString DetailedGoodsDescription
		{
			get { return GetNotes(PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, BaseJobDeclaration.NotesOfDeclarationOrShipment); }
		}

		public ZString FirstLineOfMarksAndNumbers
		{
			get { return (MarksAndNumberArray.Length >= 1) ? MarksAndNumberArray[0] : ZString.Empty; }
		}

		public ZString MarksAndNumbers
		{
			get { return GetNotes(PredefinedNoteTypes.Instance.MarksAndNumbers.Description, BaseJobDeclaration.NotesOfDeclarationOrShipment); }
		}

		public ZString CertificateOfOriginNote
		{
			get { return GetNotes(PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description, BaseJobDeclaration.NotesOfDeclarationOrShipment); }
		}

		public ZString HandlingInstruction
		{
			get { return PickupOrDeliveryHandlingInstructions(BaseJobDeclaration.NotesOfDeclarationOrShipment); }
		}

		/// <summary>
		/// Return handling instructions entered on the job.
		/// If no instructions on the job, only then fall back to handling instructions on the respective organization.
		/// </summary>
		public ZString DeclarationOrOrgHandlingInstruction
		{
			get
			{
				ZString result = ZString.Empty;
				if (!HandlingInstruction.IsEmpty)
				{
					result = (HandlingInstruction + "\n");
				}
				else
				{
					result = OrganizationInstructions(InstructionType.Handling);
				}
				return result.TrimEnd('\n');
			}
		}

		public ZString SpecialInstructions
		{
			get { return PickupOrDeliverySpecialInstructions(BaseJobDeclaration.NotesOfDeclarationOrShipment); }
		}

		/// <summary>
		/// Return Special instructions entered on the job.
		/// If no instructions on the job, only then fall back to Special instructions on the respective organization.
		/// </summary>
		public ZString DeclarationOrOrgSpecialInstruction
		{
			get
			{
				ZString result = ZString.Empty;
				if (!SpecialInstructions.IsEmpty)
				{
					result = (SpecialInstructions + "\n");
				}
				else
				{
					result = OrganizationInstructions(InstructionType.Special);
				}
				return result.TrimEnd('\n');
			}
		}

		#endregion

		#region Summary Commercial Invoice
		public ZString InvoiceNumber
		{
			get
			{
				ZString result = ZString.Empty;

				if (InvoiceHeadersInternal != null && InvoiceHeadersInternal.Count == 1)
				{
					result = InvoiceHeadersInternal[0].InvoiceNumber;
				}
				else if (InvoiceHeadersInternal != null && InvoiceHeadersInternal.Count > 1)
				{
					result = Res.GetString("7a68621a-d66a-4b84-9a22-8e81bf0315a0", "AS BELOW");
				}

				return result;
			}
		}

		public virtual ZDateTime EarliestInvoiceDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				DocBaseJobComInvoiceHeaderCollection headerCollection = InvoiceHeadersInternal;
				headerCollection.Sort("InvoiceDate", ListSortDirection.Ascending);

				foreach (DocBaseJobComInvoiceHeader header in headerCollection)
				{
					if (!header.InvoiceDate.IsEmpty)
					{
						result = header.InvoiceDate;
						break;
					}
				}
				return result;
			}
		}

		public virtual ZString CountryOfOrigin
		{
			get
			{
				ZString result = ZString.Empty;
				foreach (DocBaseJobComInvoiceHeader header in InvoiceHeadersInternal)
				{
					if (result.IsEmpty)
					{
						result = header.CountryOfOrigin;
					}
					else if (result != header.CountryOfOrigin)
					{
						result = Res.GetString("a4f60c6d-08c0-45c1-b9cf-9b39dc3cfaba", "VARIOUS");
						break;
					}
				}
				return result;
			}
		}

		public virtual ZDecimal TotalInvoiceAmount
		{
			get
			{
				ZDecimal result = 0M;

				foreach (DocBaseJobComInvoiceHeader header in InvoiceHeadersInternal)
				{
					if (ConvertInvoiceValuesToLocalCurrency && CurrentCompany.Country != null && CurrentCompany.Country.Currency != null && header.InvoiceCurr != null)
					{
						result += ConvertAmounts(CurrentCompany.Country.Currency, header.InvoiceCurr, header.InvoiceAmount);
					}
					else
					{
						result += header.InvoiceAmount;
					}
				}

				return result;
			}
		}

		public virtual DocCurrency TotalInvoiceCurrency
		{
			get
			{
				if (fTotalInvoiceCurrency == null)
				{
					if (ConvertInvoiceValuesToLocalCurrency)
					{
						fTotalInvoiceCurrency = DocCurrency.New(GlbCompany.CurrentCompany.Country.LocalCurrency, Factory);
					}
					else
					{
						if (InvoiceHeadersInternal != null && InvoiceHeadersInternal.Count > 0)
						{
							fTotalInvoiceCurrency = InvoiceHeadersInternal[0].InvoiceCurr;
						}
					}
				}
				return fTotalInvoiceCurrency;
			}
		}

		public ZDecimal TotalIncludedCosts
		{
			get
			{
				ZDecimal result = 0M;
				foreach (DocBaseJobComInvoiceHeader header in InvoiceHeadersInternal)
				{
					if (ConvertInvoiceValuesToLocalCurrency && CurrentCompany.Country != null && CurrentCompany.Country.Currency != null && header.InvoiceCurr != null)
					{
						result += ConvertAmounts(CurrentCompany.Country.Currency, header.InvoiceCurr, header.TotalIncludedCosts);
					}
					else
					{
						result += header.TotalIncludedCosts;
					}
				}

				return result;
			}
		}

		public virtual ZDecimal TotalIncludedOverseasFreight
		{
			get
			{
				CalculateTotalInvoiceCharges();
				return fTotalIncludedOverseasFreight;
			}
		}

		public virtual ZDecimal TotalExcludedOverseasFreight
		{
			get
			{
				CalculateTotalInvoiceCharges();
				return fTotalExcludedOverseasFreight;
			}
		}

		public virtual ZDecimal TotalIncludedOverseasInsurance
		{
			get
			{
				CalculateTotalInvoiceCharges();
				return fTotalIncludedOverseasInsurance;
			}
		}

		public virtual ZDecimal TotalExcludedOverseasInsurance
		{
			get
			{
				CalculateTotalInvoiceCharges();
				return fTotalExcludedOverseasInsurance;
			}
		}

		public virtual ZDecimal TotalIncludedExWorks
		{
			get
			{
				CalculateTotalInvoiceCharges();
				return fTotalIncludedExWorks;
			}
		}

		public virtual ZDecimal TotalExcludedExWorks
		{
			get
			{
				CalculateTotalInvoiceCharges();
				return fTotalExcludedExWorks;
			}
		}

		public virtual ZDecimal TotalIncludedForeignInlandFreight
		{
			get
			{
				CalculateTotalInvoiceCharges();
				return fTotalIncludedForeignInlandFreight;
			}
		}

		public virtual ZDecimal TotalExcludedForeignInlandFreight
		{
			get
			{
				CalculateTotalInvoiceCharges();
				return fTotalExcludedForeignInlandFreight;
			}
		}

		public virtual ZDecimal TotalIncludedPackingCosts
		{
			get
			{
				CalculateTotalInvoiceCharges();
				return fTotalIncludedPackingCosts;
			}
		}

		public virtual ZDecimal TotalExcludedPackingCosts
		{
			get
			{
				CalculateTotalInvoiceCharges();
				return fTotalExcludedPackingCosts;
			}
		}

		public virtual ZDecimal TotalIncludedLandingCharges
		{
			get
			{
				CalculateTotalInvoiceCharges();
				return fTotalIncludedLandingCharges;
			}
		}

		public virtual ZDecimal TotalExcludedLandingCharges
		{
			get
			{
				CalculateTotalInvoiceCharges();
				return fTotalExcludedLandingCharges;
			}
		}

		public virtual ZDecimal TotalIncludedOtherCharges1
		{
			get
			{
				CalculateTotalInvoiceCharges();
				return fTotalIncludedOtherCharges1;
			}
		}

		public virtual ZDecimal TotalExcludedOtherCharges1
		{
			get
			{
				CalculateTotalInvoiceCharges();
				return fTotalExcludedOtherCharges1;
			}
		}

		public virtual ZDecimal TotalIncludedOtherCharges2
		{
			get
			{
				CalculateTotalInvoiceCharges();
				return fTotalExcludedOtherCharges1;
			}
		}

		public virtual ZDecimal TotalExcludedOtherCharges2
		{
			get
			{
				CalculateTotalInvoiceCharges();
				return fTotalExcludedOtherCharges2;
			}
		}

		public virtual ZDecimal TotalIncludedDiscount
		{
			get
			{
				CalculateTotalInvoiceCharges();
				return fTotalIncludedDiscount;
			}
		}

		public virtual ZDecimal TotalExcludedDiscount
		{
			get
			{
				CalculateTotalInvoiceCharges();
				return fTotalExcludedDiscount;
			}
		}

		public virtual ZDecimal TotalIncludedCommission
		{
			get
			{
				CalculateTotalInvoiceCharges();
				return fTotalIncludedCommission;
			}
		}

		public virtual ZDecimal TotalExcludedCommission
		{
			get
			{
				CalculateTotalInvoiceCharges();
				return fTotalExcludedCommission;
			}
		}

		protected ZBool ConvertInvoiceValuesToLocalCurrency
		{
			get
			{
				if (!fInvoiceCurrenciesChecked)
				{
					ZString currencyCode = "";
					foreach (DocBaseJobComInvoiceHeader header in InvoiceHeadersInternal)
					{
						if (header.InvoiceCurr != null)
						{
							if (currencyCode.IsEmpty)
							{
								currencyCode = header.InvoiceCurr.Code;
							}
							else if (header.InvoiceCurr.Code != currencyCode)
							{
								fConvertInvoiceValues = ZBool.True;
								break;
							}
						}
					}
					fInvoiceCurrenciesChecked = ZBool.True;
				}
				return fConvertInvoiceValues;
			}
		}

		protected void CalculateTotalInvoiceCharges()
		{
			if (!HasCalculatedTotalInvoiceCharges)
			{
				foreach (DocBaseJobComInvoiceHeader header in InvoiceHeadersInternal)
				{
					DocCurrency headerCurrency = header.InvoiceCurr;

					fTotalIncludedOverseasFreight += AddAmounts(header.IncludedOverseasFreight, headerCurrency, header.IncludedOverseasFreightCurrency);
					fTotalExcludedOverseasFreight += AddAmounts(header.ExcludedOverseasFreight, headerCurrency, header.ExcludedOverseasFreightCurrency);

					fTotalIncludedOverseasInsurance += AddAmounts(header.IncludedOverseasInsurance, headerCurrency, header.IncludedOverseasInsuranceCurrency);
					fTotalExcludedOverseasInsurance += AddAmounts(header.ExcludedOverseasInsurance, headerCurrency, header.ExcludedOverseasInsuranceCurrency);

					fTotalIncludedExWorks += AddAmounts(header.IncludedExWorks, headerCurrency, header.IncludedExWorksCurrency);
					fTotalExcludedExWorks += AddAmounts(header.ExcludedExWorks, headerCurrency, header.ExcludedExWorksCurrency);

					fTotalIncludedForeignInlandFreight += AddAmounts(header.IncludedForeignInlandFreight, headerCurrency, header.IncludedForeignInlandFreightCurrency);
					fTotalExcludedForeignInlandFreight += AddAmounts(header.ExcludedForeignInlandFreight, headerCurrency, header.ExcludedForeignInlandFreightCurrency);

					fTotalIncludedPackingCosts += AddAmounts(header.IncludedPackingCosts, headerCurrency, header.IncludedPackingCostsCurrency);
					fTotalExcludedPackingCosts += AddAmounts(header.ExcludedPackingCosts, headerCurrency, header.ExcludedPackingCostsCurrency);

					fTotalIncludedLandingCharges += AddAmounts(header.IncludedLandingCharges, headerCurrency, header.IncludedLandingChargesCurrency);
					fTotalExcludedLandingCharges += AddAmounts(header.ExcludedLandingCharges, headerCurrency, header.ExcludedLandingChargesCurrency);

					fTotalIncludedOtherCharges1 += AddAmounts(header.IncludedOtherCharges1, headerCurrency, header.IncludedOtherCharges1Currency);
					fTotalExcludedOtherCharges1 += AddAmounts(header.ExcludedOtherCharges1, headerCurrency, header.ExcludedOtherCharges1Currency);

					fTotalIncludedOtherCharges2 += AddAmounts(header.IncludedOtherCharges2, headerCurrency, header.IncludedOtherCharges2Currency);
					fTotalExcludedOtherCharges2 += AddAmounts(header.ExcludedOtherCharges2, headerCurrency, header.ExcludedOtherCharges2Currency);

					fTotalIncludedDiscount += AddAmounts(header.IncludedDiscount, headerCurrency, header.IncludedDiscountCurrency);
					fTotalExcludedDiscount += AddAmounts(header.ExcludedDiscount, headerCurrency, header.ExcludedDiscountCurrency);

					fTotalIncludedCommission += AddAmounts(header.IncludedCommission, headerCurrency, header.IncludedCommissionCurrency);
					fTotalExcludedCommission += AddAmounts(header.ExcludedCommission, headerCurrency, header.ExcludedCommissionCurrency);
				}
				HasCalculatedTotalInvoiceCharges = true;
			}
		}

		protected ZDecimal AddAmounts(ZDecimal amountToAdd, DocCurrency headerCurrency, DocCurrency costCurrency)
		{
			ZDecimal result = 0M;
			if (headerCurrency != null && costCurrency != null)
			{
				if (CurrentCompany.Country != null && CurrentCompany.Country.Currency != null && ConvertInvoiceValuesToLocalCurrency)
				{
					result = ConvertAmounts(CurrentCompany.Country.Currency, costCurrency, amountToAdd);
				}
				else
				{
					if (headerCurrency.Code == costCurrency.Code)
					{
						result = amountToAdd;
					}
					else
					{
						result = ConvertAmounts(headerCurrency, costCurrency, amountToAdd);
					}
				}
			}
			else
			{
				result = amountToAdd;
			}

			return result;
		}

		protected ZDecimal ConvertAmounts(DocCurrency destinationCurrencyWrapper, DocCurrency originalCurrencyWrapper, ZDecimal amount)
		{
			ZDecimal result = amount;
			if (destinationCurrencyWrapper != null && originalCurrencyWrapper != null)
			{
				if (destinationCurrencyWrapper.Code != originalCurrencyWrapper.Code)
				{
					RefCurrency destinationRefCurrency = (RefCurrency)destinationCurrencyWrapper.WrappedObject;

					result = ((ICurrencyConverterProvider)BaseJobDeclaration).CurrencyConverter.ConvertExact(new Money(amount, originalCurrencyWrapper), destinationRefCurrency).Amount;
				}
			}
			return result;
		}

		#region Variables

		protected bool HasCalculatedTotalInvoiceCharges;
		protected DocCurrency fTotalInvoiceCurrency;
		protected ZDecimal fTotalIncludedOverseasFreight;
		protected ZDecimal fTotalExcludedOverseasFreight;
		protected ZDecimal fTotalIncludedOverseasInsurance;
		protected ZDecimal fTotalExcludedOverseasInsurance;
		protected ZDecimal fTotalIncludedExWorks;
		protected ZDecimal fTotalExcludedExWorks;
		protected ZDecimal fTotalIncludedForeignInlandFreight;
		protected ZDecimal fTotalExcludedForeignInlandFreight;
		protected ZDecimal fTotalIncludedPackingCosts;
		protected ZDecimal fTotalExcludedPackingCosts;
		protected ZDecimal fTotalIncludedLandingCharges;
		protected ZDecimal fTotalExcludedLandingCharges;
		protected ZDecimal fTotalIncludedOtherCharges1;
		protected ZDecimal fTotalExcludedOtherCharges1;
		protected ZDecimal fTotalIncludedOtherCharges2;
		protected ZDecimal fTotalExcludedOtherCharges2;
		protected ZDecimal fTotalIncludedDiscount;
		protected ZDecimal fTotalExcludedDiscount;
		protected ZDecimal fTotalIncludedCommission;
		protected ZDecimal fTotalExcludedCommission;
		protected ZBool fInvoiceCurrenciesChecked;
		protected ZBool fConvertInvoiceValues;

		#endregion

		#endregion

		#region Landed Costing Exchange Rates

		public DocLandedCostingExchangeRateCollection LandedCostingExchangeRatesInCurrencyCodeOrder
		{
			get
			{
				Hashtable exchangeRates = new Hashtable();
				foreach (BaseJobComInvoiceHeader invoice in this.BaseJobDeclaration.Invoices)
				{
					if (invoice.Invoice_Currency != null)
					{
						exchangeRates[invoice.Invoice_Currency.RX_Code.ToString()] = invoice.JZ_InvoiceCurrExRate;
					}
				}
				if (Env.Registry.LandedCostingFallbackExRatesToJobInvoicing &&
						JobInvoicingJob != null)
				{
					foreach (DocJobExchangeRate rate in JobInvoicingJob.ExchangeRates)
					{
						if (!exchangeRates.Contains(rate.CurrencyCode) ||
								((ZDecimal)exchangeRates[rate.CurrencyCode]) == 0m)
						{
							exchangeRates[rate.CurrencyCode.ToString()] = rate.SellRate;
						}
					}
				}

				DocLandedCostingExchangeRateCollection result = new DocLandedCostingExchangeRateCollection(Factory);
				foreach (DictionaryEntry entry in exchangeRates)
				{
					ZString currencyCode = (string)entry.Key;
					ZDecimal rate = (ZDecimal)entry.Value;
					if (rate == 0m)
					{
						rate = 1m;
					}
					result.Add(new DocLandedCostingExchangeRate(Factory, currencyCode, rate));
				}
				result.Sort("CurrencyCode");
				return result;
			}
		}

		#endregion

		#region Collections

		public DocBaseCusEntryHeaderCollection RateEntryHeaders
		{
			get
			{
				if (fRateEntryHeaders == null)
				{
					fRateEntryHeaders = CreateNewEntryHeadersCollection(BaseJobDeclaration.CustomsEntryHeaders);
				}
				return fRateEntryHeaders;
			}
		}
		DocBaseCusEntryHeaderCollection fRateEntryHeaders;

		protected abstract DocBaseCusEntryHeaderCollection CreateNewEntryHeadersCollection(
			ICusEntryHeaderCollection<CusEntryHeader> collectionToWrap);

		public DocBaseBillOfLadingCollection BillOfLadings
		{
			get
			{
				if (fBillOfLadings == null)
				{
					fBillOfLadings = CreateNewBillOfLadingsCollection();
					fBillOfLadings.LoadBillOfLadings();
				}

				return fBillOfLadings;
			}
		}
		DocBaseBillOfLadingCollection fBillOfLadings;

		protected virtual DocBaseBillOfLadingCollection CreateNewBillOfLadingsCollection()
		{
			return new DocBaseBillOfLadingCollection(BaseJobDeclaration);
		}

		#endregion

		#region Implementation

		BaseJobDeclaration BaseJobDeclaration
		{
			get { return (BaseJobDeclaration)WrappedObject; }
		}

		protected DocBaseCusContainerCollection ContainersInternal
		{
			get
			{
				ICusContainerCollection<BaseCusContainer> containers = BaseJobDeclaration.CusContainers;
				containers.Sort(BaseCusContainer.Schema.CO_ContainerNumber, System.ComponentModel.ListSortDirection.Ascending);
				return CreateCusContainerCollection(containers);
			}
		}

		public IDocSimpleContainerCollection SimpleContainers
		{
			get
			{
				IDocSimpleContainerCollection result = new IDocSimpleContainerCollection(Factory);
				if (ContainersInternal != null)
				{
					foreach (DocBaseCusContainer container in ContainersInternal)
					{
						result.Add(container);
					}
				}
				return result;
			}
		}

		protected DocBaseJobComInvoiceGroupHeaderCollection InvoiceGroupHeadersInternal
		{
			get { return CreateJobComInvoiceGroupHeaderCollection(BaseJobDeclaration.JobComInvoiceGroupHeaders); }
		}

		protected DocBaseJobComInvoiceHeaderCollection InvoiceHeadersInternal
		{
			get { return CreateAllInvoiceHeaderCollection(BaseJobDeclaration.Invoices); }
		}

		protected DocBaseJobComInvoiceLineCollection InvoiceLinesInternal
		{
			get
			{
				DocBaseJobComInvoiceLineCollection coll = CreateJobComInvoiceLineCollection(BaseJobDeclaration.InvoiceLines);
				coll.SortOnInvoiceHeaderThenProductCode();
				return coll;
			}
		}

		protected DocBaseJobComInvoiceLineCollection FSAInvoiceLinesLineOne
		{
			get
			{
				DocBaseJobComInvoiceLineCollection coll = CreateFSAJobComInvoiceLineLineOneCollection(BaseJobDeclaration.InvoiceLines);
				//Coll.SortOnInvoiceHeaderThenProductCode();
				return coll;
			}
		}

		protected DocBaseJobComInvoiceLineCollection FSAInvoiceLinesLinesInternal
		{
			get
			{
				DocBaseJobComInvoiceLineCollection coll = CreateFSAJobComInvoiceLineCollection(BaseJobDeclaration.InvoiceLines);
				coll.SortOnInvoiceHeaderThenProductCode();
				return coll;
			}
		}

		protected DocBaseJobComInvoiceLineCollection InvoiceLinesSortedByLineNoInternal
		{
			get
			{
				DocBaseJobComInvoiceLineCollection coll = CreateJobComInvoiceLineCollection(BaseJobDeclaration.InvoiceLines);
				coll.SortOnInvoiceLineNumber();
				return coll;
			}
		}

		protected DocBaseJobComInvoiceLineCollection InvoiceLinesSortedByMergedLineNoInternal
		{
			get
			{
				DocBaseJobComInvoiceLineCollection coll = CreateJobComInvoiceLineCollection(BaseJobDeclaration.InvoiceLines);
				coll.SortOnMergedLineNumber();
				return coll;
			}
		}

		protected DocBaseJobComInvoiceLineCollection InvoiceLinesSortedByMergedNumericLineNoInternal
		{
			get
			{
				DocBaseJobComInvoiceLineCollection coll = CreateJobComInvoiceLineCollection(BaseJobDeclaration.InvoiceLines);
				coll.SortOnMergedNumericLineNumber();
				return coll;
			}
		}

		protected DocBaseJobComInvoiceHeader InvoiceHeaderInternal
		{
			get
			{
				DocBaseJobComInvoiceHeader header = null;
				if (BaseJobDeclaration.Invoices != null && BaseJobDeclaration.Invoices.Count > 0)
				{
					BaseJobDeclaration.Invoices.ApplySort(BaseJobComInvoiceHeader.Schema.JZ_InvoiceNumber, ListSortDirection.Ascending);
					foreach (BaseJobComInvoiceHeader invoiceHeader in BaseJobDeclaration.Invoices)
					{
						if (invoiceHeader.JZ_GroupInvoice == ZBool.False)
						{
							header = CreateJobComInvoiceHeader(invoiceHeader);
							break;
						}
					}
				}
				return header;
			}
		}

		protected DocBaseJobComInvoiceGroupHeader ActiveInvoiceHeaderGroupInternal
		{
			get
			{
				DocBaseJobComInvoiceGroupHeader header = null;

				if (BaseJobDeclaration.ActiveGroupHeader.Count > 0)
				{
					header = CreateJobComInvoiceGroupHeader(BaseJobDeclaration.ActiveGroupHeader[0]);
				}

				return header;
			}
		}

		protected ZString GetOrganisationABNOrCID(DocOrganisation org)
		{
			if (org != null)
			{
				return org.ABN.IsEmpty ? org.CID : org.ABN;
			}
			return ZString.Empty;
		}

		#region Cartage Advice

		protected DocDocAddress GetDepotFromContainerType()
		{
			DocDocAddress result;

			var freightContainerMode = BaseJobDeclaration.FreightContainerMode;
			if (freightContainerMode == Core.Constants.ContainerModes.FCL || freightContainerMode == Core.Constants.ContainerModes.FCLMixedShipper)
			{
				result = CTOAddress;
			}
			else
			{
				result = DepotAddress;
			}

			return result;
		}

		#endregion

		#endregion

		#region IPreAlert Members

		public DocTransportCollection CompleteRouting
		{
			get
			{
				DocTransportCollection result;

				if (Shipment != null)
				{
					result = Shipment.CompleteRouting;
				}
				else
				{
					result = new DocTransportCollection(Factory);
					result.Add(DocTransport.New(BaseJobDeclaration, Factory));
				}

				return result;
			}
		}

		public ZString PortDisplayMode
		{
			get { return "LoadDischargeCollectDeliver"; }
		}

		public ZBool ShowChargesOnArrivalNotice
		{
			get { return false; }
		}

		public ZBool ShowExchangeRatesOnArrivalNotice
		{
			get { return false; }
		}

		public ZString PreAlertDocumentHeader
		{
			get { return TransportMode + " " + ReportName; }
		}

		public ZString JobNumberHeading
		{
			get { return Res.GetString("92c92c2b-c382-48bf-b078-b0f7851be0cf", "BROKERAGE"); }
		}

		public ZString JobNumber
		{
			get { return DeclarationReference; }
		}

		public ZString SecondJobNumberHeading
		{
			get { return DeclarationNumber.IsEmpty ? "" : Res.GetString("cd9cd083-bd40-4a38-a7ec-8a73334bfec5", "DECLARATION:"); }
		}

		public ZString SecondJobNumber
		{
			get { return DeclarationNumber; }
		}

		public ZString AvailableDateHeading
		{
			get { return ZString.Empty; }
		}

		public ZDateTime AvailableDate
		{
			get { return TransportModeIsAir || (Core.Constants.ContainerModes.IsLCLType(ContainerMode)) ? LCLAvailableDate : FCLAvailableDate; }
		}

		public ZDateTime LCLAvailableDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (DocsAndCartage != null)
				{
					result = DocsAndCartage.LCLAvailable;
				}
				if (result.IsEmpty && BaseJobDeclaration.CusContainers.Count > 0)
				{
					result = BaseJobDeclaration.CusContainers[0].LCLAvailable;
				}
				return result;
			}
		}

		public ZDateTime FCLAvailableDate
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;
				if (DocsAndCartage != null)
				{
					result = DocsAndCartage.FCLAvailable;
				}
				if (result.IsEmpty && BaseJobDeclaration.CusContainers.Count > 0)
				{
					result = BaseJobDeclaration.CusContainers[0].FCLAvailable;
				}
				return result;
			}
		}

		public ZString StorageStartsHeading
		{
			get { return ZString.Empty; }
		}

		public ZString StorageStartsDate
		{
			get { return ZString.Empty; }
		}

		public ZString UltimateNotification
		{
			get { return ZString.Empty; }
		}

		public ZString PreAlertReferenceHeading
		{
			get { return Res.GetString("479fb9d3-402a-48b9-bd6f-0ed2764b538f", "ORDER NUMBERS / REFERENCE"); }
		}

		public ZString PreAlertReference
		{
			get
			{
				ZString order = OrderRef;
				ZString owner = OwnerRef;
				return order + (!order.IsEmpty && !owner.IsEmpty ? " " : "") + owner;
			}
		}

		public ZString OrderNumbers
		{
			get { return OwnerRefAndOrderRef; }
		}

		public ZString CargoStatus
		{
			get
			{
				ZString result = "";

				if (BaseJobDeclaration.AttachedOrders.Count > 0)
				{
					foreach (Order order in BaseJobDeclaration.AttachedOrders)
					{
						result += order.JD_OrderNumber + ",";
					}
				}
				else
				{
					result += DocsAndCartage.OrderItemsAsString;
				}

				return result.TrimEndIncludingWhiteSpace(',');
			}
		}

		public ZString BrokerName
		{
			get { return Broker.Name; }
		}

		public DocOrganisation Broker
		{
			get { return CurrentBranch.Organisation ?? CurrentCompany.Organisation; }
		}

		public DocOrganisation ImportBroker
		{
			get { return Broker; }
		}

		public ZString TransportInfo
		{
			get { return Transport; }
		}

		public ZString MasterBillNum
		{
			get { return MasterBill; }
		}

		public ZString MasterBillAndIssueDate
		{
			get { return FreightHelperClass.FormatBillAndIssueDate(MasterBillNum, MasterBillIssueDate); }
		}

		public ZString Weight
		{
			get { return WeightCore; }
		}

		protected virtual ZString WeightCore
		{
			get { return TotalWeight.ToString(2); }
		}

		public ZString WeightUnit
		{
			get { return WeightUnitCore; }
		}

		protected virtual ZString WeightUnitCore
		{
			get { return WeightUQ; }
		}

		public ZString UnitOfWeight
		{
			get { return WeightUQ; }
		}

		public ZString Volume
		{
			get { return VolumeCore; }
		}

		protected virtual ZString VolumeCore
		{
			get { return TotalVolume.ToString(2); }
		}

		public ZString VolumeUnit
		{
			get { return VolumeUnitCore; }
		}

		protected virtual ZString VolumeUnitCore
		{
			get { return VolumeUQ; }
		}

		public ZString UnitOfVolume
		{
			get { return VolumeUQ; }
		}

		public ZString Chargeable
		{
			get { return Shipment != null ? Shipment.Chargeable : ZString.Empty; }
		}

		public ZString ChargeableUnit
		{
			get { return Shipment != null ? Shipment.ChargeableUnit : ZString.Empty; }
		}

		public ZString CollectedFromETDString
		{
			get { return DateAtOrigin.ToShortDateString(); }
		}

		public ZString DeliveredToETAString
		{
			get { return DateAtFinalDestination.ToShortDateString(); }
		}

		public ZString LoadingETDString
		{
			get { return ExportDate.ToShortDateString(); }
		}

		public ZString DischargeETAString
		{
			get { return DateOfArrival.ToShortDateString(); }
		}

		public ZString ArrivalReference
		{
			get { return ""; }
		}

		public ZString KANumber
		{
			get { return ""; }
		}

		public ZString CTOArrivalBerth
		{
			get { return ""; }
		}

		public ZString HazCat
		{
			get { return ""; }
		}

		public ZString CommodityAndHazCat
		{
			get { return ""; }
		}

		public DocDocAddress UnpackAt
		{
			get { return DepotAddress; }
		}
		public DocOrganisation Consignor
		{
			get { return Supplier; }
		}

		public ZString ConsignorAddress
		{
			get { return Consignor != null ? Consignor.PostalAddress : ZString.Empty; }
		}

		public DocOrganisation Consignee
		{
			get { return Importer; }
		}

		public ZString ConsigneeAddress
		{
			get { return Consignee != null ? Consignee.PostalAddress : ZString.Empty; }
		}

		public DocUNLOCO OriginLoco
		{
			get { return Origin; }
		}

		public DocUNLOCO DestinationLoco
		{
			get { return FinalDestination; }
		}

		public DocUNLOCO PortOfDischarge
		{
			get { return PortOfArrival; }
		}

		public DocCommodityCollection Commodity
		{
			get { return new DocCommodityCollection(Factory); }
		}

		public ZString ReleaseType
		{
			get { return ZString.Empty; }
		}

		public ZString AlertText
		{
			get { return DocumentsDataRegistry.Instance.CustomsDelayAlertAlertText.Value; }
		}

		public TrackingConstants.BusinessContext TrackingBusinessContext
		{
			get { return TrackingConstants.BusinessContext.Declaration; }
		}

		public ZGuid TrackingBusinessObjectPK
		{
			get { return BaseJobDeclaration.PK; }
		}

		#endregion

		#region DocManager Barcode Properties

		protected override ZString DocManagerUniqueID
		{
			get { return DeclarationReference; }
		}

		#endregion

		#region IDocJobDetail Members

		ZString IDocJobDetail.OrderNumbersForInvoice
		{
			get { return OrderNumbers; }
		}

		ZString IDocJobDetail.OurReference
		{
			get { return DeclarationReference; }
		}

		ZString IDocJobDetail.SupplierAsString
		{
			get { return Supplier != null ? Supplier.Name : ZString.Empty; }
		}

		ZString IDocJobDetail.VesselAndVoyage
		{
			get { return Transport; }
		}

		ZString IDocJobDetail.MasterBillNumber
		{
			get { return MasterBillNum; }
		}

		ZString IDocJobDetail.ETAPortName
		{
			get { return (FinalDestination != null) ? FinalDestination.PortName : ZString.Empty; }
		}

		ZString IDocJobDetail.ETDPortName
		{
			get { return (Origin != null) ? Origin.PortName : ZString.Empty; }
		}

		ZString IDocJobDetail.Service
		{
			get { return ContainerMode; }
		}

		public ZString PackageQuantity
		{
			get { return TotalNoOfPacks.ToString(); }
		}

		public ZString PackageType
		{
			get { return PackType; }
		}

		ZString IDocJobDetail.ConsolDepot
		{
			get { return ZString.Empty; }
		}

		ZString IDocJobDetail.WeightAsString
		{
			get { return Weight + " " + WeightUnit; }
		}

		ZString IDocJobDetail.VolumeAsString
		{
			get { return Volume + " " + VolumeUnit; }
		}

		ZString IDocJobDetail.Note
		{
			get { return !OrderNumbers.IsEmpty ? new ZString(Res.GetString("5895dd62-d140-44cf-9eac-1633487207f8", "Order Numbers: {0}", OrderNumbers)) : ZString.Empty; }
		}

		ZString IDocJobDetail.ConsignorAsString
		{
			get { return Consignor != null ? Consignor.Name : ZString.Empty; }
		}

		ZString IDocJobDetail.ConsigneeAsString
		{
			get { return Consignee != null ? Consignee.Name : ZString.Empty; }
		}

		ZString IDocJobDetail.ShortContainerAndSealNumbersForInvoice
		{
			get
			{
				ZString containerAndSealNumbers = ZString.Empty;
				if (ContainersInternal.Count > 0)
				{
					var container = ContainersInternal[0];
					containerAndSealNumbers += container.ContainerNumber + " / " + container.SealNumber;
					if (container.Container != null)
					{
						containerAndSealNumbers += " / " + container.Container.Code;
					}
				}
				return containerAndSealNumbers;
			}
		}

		ZString IDocJobDetail.LongContainerAndSealNumbersForInvoice
		{
			get
			{
				ZString containerString = ZString.Empty;
				foreach (DocBaseCusContainer container in ContainersInternal)
				{
					containerString += "- " + container.ContainerNumber.PadRight(15, ' ');
					containerString += " - " + container.SealNumber.PadRight(20, ' ') + " - ";

					ZString containerCode = (container.Container != null) ? container.Container.Code : ZString.Empty;
					containerString += containerCode.PadRight(15, ' ') + "\n";
				}

				return containerString;
			}
		}

		ZInt IDocJobDetail.NumberOfContainers
		{
			get { return (ContainersInternal != null) ? ContainersInternal.Count : 0; }
		}

		ZString IDocJobDetail.MarksAndNumbersForInvoice
		{
			get { return MarksAndNumbers; }
		}

		ZString IDocJobDetail.ShortGoodsDescriptionForInvoice
		{
			get { return GoodsDescription; }
		}

		ZString IDocJobDetail.LongGoodsDescriptionForInvoice
		{
			get { return GoodsDescription; }
		}

		ZDateTime IDocJobDetail.ETADate
		{
			get { return DateOfArrival; }
		}

		ZDateTime IDocJobDetail.ETDDate
		{
			get { return ExportDate; }
		}

		public ZBool TransportModeIsAir
		{
			get { return BaseJobDeclaration.IsAir; }
		}

		public ZBool TransportModeIsSea
		{
			get { return BaseJobDeclaration.IsSea; }
		}

		#endregion

		#region IShipperDepartureNotice Members

		public ZString ShipperDepartureNoticeDocumentHeader
		{
			get
			{
				ZString heading = ZString.Empty;

				if (TransportModeIsSea && (PackingMode == Core.Constants.ContainerModes.FCL || PackingMode == Core.Constants.ContainerModes.LCL))
				{
					heading += PackingMode + " ";
				}

				heading += TransportModeDescription.Trim();

				if (!heading.EndsWith(Res.GetString("1bd5f61b-43e3-4b2d-b347-341a9ba13776", "Freight")))
				{
					heading += " " + Res.GetString("1bd5f61b-43e3-4b2d-b347-341a9ba13776", "Freight");
				}

				heading += " " + ReportName;

				return heading.Trim();
			}
		}

		public ZString TransportModeAndPackingMode
		{
			get
			{
				ZString result = ZString.Empty;
				if (!TransportMode.IsEmpty && !PackingMode.IsEmpty)
				{
					result = Res.GetString("32a1c144-eca8-46c2-825f-73b7f9511126", "Transport Mode {0} Container Mode {1}", TransportMode, PackingMode);
				}
				return result;
			}
		}

		public virtual ZString TransportModeDescription
		{
			get { return BaseJobDeclaration.Lookups.TransportTypeList.GetDescriptionFromCode(TransportMode); }
		}

		public ZString HeadingTransportMode
		{
			get
			{
				ZString heading = ZString.Empty;

				if (TransportModeIsSea && (PackingMode == Core.Constants.ContainerModes.FCL || PackingMode == Core.Constants.ContainerModes.LCL))
				{
					heading += PackingMode + " ";
				}

				heading += TransportModeDescription.Trim();

				string wordToRemove = Res.GetString("1bd5f61b-43e3-4b2d-b347-341a9ba13776", "Freight");
				if (heading.EndsWith(wordToRemove))
				{
					heading = heading.ToString().Remove(heading.LastIndexOf(wordToRemove), wordToRemove.Length);
				}

				return heading.Trim();
			}
		}

		public ZString PackingMode
		{
			get { return ContainerMode; }
		}

		public DocOrganisation ReceivingForwarder
		{
			get
			{
				DocOrganisation result = null;

				if (NotifyParty != null)
				{
					result = NotifyParty.Organisation;
				}

				return result;
			}
		}

		public DocOrganisation ShippersDeliveryAgent
		{
			get
			{
				DocOrganisation result = null;

				if (DepotAddress != null && DepotAddress.Organisation != null)
				{
					result = DepotAddress.Organisation;
				}

				return result;
			}
		}

		public ZString AgentsBookingReference
		{
			get { return ZString.Empty; }
		}

		public ZString DepartureReference
		{
			get { return ZString.Empty; }
		}

		public ZInt NoOfOriginalBills
		{
			get { return 0; }
		}

		public ZInt NoOfCopyBills
		{
			get { return 0; }
		}

		#endregion

		#region IDocCartageAdvice Members

		public CartageAdviceHelper CartageAdvice
		{
			get
			{
				if (fCartageAdvice == null)
				{
					fCartageAdvice = new CartageAdviceHelper(this, Factory);
				}
				return fCartageAdvice;
			}
		}
		CartageAdviceHelper fCartageAdvice;

		#region Headings

		public MultilingualString JourneyOnePickUpHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("99f96459-c855-438a-a2d8-f368c73fc581", "PICKUP");

				if (IsEmptyLeg(true))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("4034bc6c-8716-4139-9c19-1f708486278b", "EMPTY"));
				}
				else if (IsFullLeg(true))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("b07abbc8-d956-4fdc-b99f-331d1e77ea61", "FULL"));
				}

				if (!PrintTwoJourneys)
				{
					if (IsExportMessage && DocsAndCartage.PickupRequiredBy.IsValid)
					{
						result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("bdfe601a-5e34-4451-a205-f5f51dc78d0e", "DATE {0}", DocsAndCartage.PickupRequiredBy.ToLongTimeString()));
					}
					else if (!IsExportMessage && DocsAndCartage.EstimatedDelivery.IsValid)
					{
						result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("bdfe601a-5e34-4451-a205-f5f51dc78d0e", "DATE {0}", DocsAndCartage.EstimatedDelivery.ToLongTimeString()));
					}
				}
				return result;
			}
		}

		public MultilingualString JourneyOneDeliverToHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("e8e9c70f-ba3e-46f6-ad31-c1e47ba035ec", "DELIVER TO");

				if (IsEmptyLeg(true))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("4034bc6c-8716-4139-9c19-1f708486278b", "EMPTY"));
				}
				else if (IsFullLeg(true))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("b07abbc8-d956-4fdc-b99f-331d1e77ea61", "FULL"));
				}

				if (!PrintTwoJourneys)
				{
					if (!IsExportMessage && DocsAndCartage.DeliveryRequiredBy.IsValid)
					{
						result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("bdfe601a-5e34-4451-a205-f5f51dc78d0e", "DATE {0}", DocsAndCartage.DeliveryRequiredBy.ToLongTimeString()));
					}
				}

				return result;
			}
		}

		public MultilingualString JourneyTwoPickUpHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("99f96459-c855-438a-a2d8-f368c73fc581", "PICKUP");

				if (IsEmptyLeg(false))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("4034bc6c-8716-4139-9c19-1f708486278b", "EMPTY"));
				}
				else if (IsFullLeg(false))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("b07abbc8-d956-4fdc-b99f-331d1e77ea61", "FULL"));
				}

				return result;
			}
		}

		public MultilingualString JourneyTwoDeliverToHeading
		{
			get
			{
				MultilingualString result = ResString.GetMultilingualString("e8e9c70f-ba3e-46f6-ad31-c1e47ba035ec", "DELIVER TO");

				if (IsEmptyLeg(false))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("4034bc6c-8716-4139-9c19-1f708486278b", "EMPTY"));
				}
				else if (IsFullLeg(false))
				{
					result = MultilingualString.Join(" ", result, ResString.GetMultilingualString("b07abbc8-d956-4fdc-b99f-331d1e77ea61", "FULL"));
				}

				return result;
			}
		}

		#endregion

		#region Addresses

		public DocDocAddress JourneyOnePickUpAddress
		{
			get
			{
				if (fJourneyOnePickUpAddress == null)
				{
					if (!PrintTwoJourneys)
					{
						fJourneyOnePickUpAddress = PickupAddress;
					}
					else if (IsExportMessage)
					{
						fJourneyOnePickUpAddress = ContainerParkAddress;
					}
					else
					{
						fJourneyOnePickUpAddress = CTOAddress;
					}
				}
				return fJourneyOnePickUpAddress;
			}
		}
		DocDocAddress fJourneyOnePickUpAddress;

		public DocDocAddress JourneyOneDeliverToAddress
		{
			get
			{
				if (fJourneyOneDeliverToAddress == null)
				{
					if (!PrintTwoJourneys)
					{
						fJourneyOneDeliverToAddress = DeliverToAddress;
					}
					else if (IsExportMessage)
					{
						fJourneyOneDeliverToAddress = JourneyOneDeliverToAddressForExport;
					}
					else
					{
						fJourneyOneDeliverToAddress = JourneyOneDeliverToAddressForImport;
					}
				}
				return fJourneyOneDeliverToAddress;
			}
		}
		DocDocAddress fJourneyOneDeliverToAddress;

		public DocDocAddress JourneyOneDeliverToAddressForExport
		{
			get
			{
				DocDocAddress result = null;

				if (DocsAndCartage != null && DocsAndCartage.PickupAddress != null)
				{
					result = DocsAndCartage.PickupAddress;
				}
				else if (Supplier != null)
				{
					result = Supplier.PickUpDocAddress;
				}

				return result;
			}
		}

		public DocDocAddress JourneyOneDeliverToAddressForImport
		{
			get
			{
				DocDocAddress result = null;

				if (DocsAndCartage != null && DocsAndCartage.DeliveryAddress != null)
				{
					result = DocsAndCartage.DeliveryAddress;
				}
				else if (Importer != null)
				{
					result = Importer.DeliverDocAddress;
				}

				return result;
			}
		}
		public DocDocAddress JourneyTwoPickUpAddress
		{
			get
			{
				if (fJourneyTwoPickUpAddress == null)
				{
					if (IsExportMessage)
					{
						fJourneyTwoPickUpAddress = JourneyTwoPickUpAddressForExport;
					}
					else
					{
						fJourneyTwoPickUpAddress = JourneyTwoPickUpAddressForImport;
					}
				}
				return fJourneyTwoPickUpAddress;
			}
		}
		DocDocAddress fJourneyTwoPickUpAddress;

		public DocDocAddress JourneyTwoPickUpAddressForExport
		{
			get
			{
				DocDocAddress result = null;

				if (DocsAndCartage != null && DocsAndCartage.PickupAddress != null)
				{
					result = DocsAndCartage.PickupAddress;
				}
				else if (Supplier != null)
				{
					result = Supplier.PickUpDocAddress;
				}

				return result;
			}
		}

		public DocDocAddress JourneyTwoPickUpAddressForImport
		{
			get
			{
				DocDocAddress result = null;

				if (DocsAndCartage != null && DocsAndCartage.DeliveryAddress != null)
				{
					result = DocsAndCartage.DeliveryAddress;
				}
				else if (Importer != null)
				{
					result = Importer.DeliverDocAddress;
				}

				return result;
			}
		}

		public DocDocAddress JourneyTwoDeliverToAddress
		{
			get
			{
				DocDocAddress result = null;

				if (IsExportMessage)
				{
					result = CTOAddress;
				}
				else
				{
					result = ContainerParkAddress;
				}
				return result;
			}
		}

		#endregion

		#region Contacts

		#region JourneyOnePickUpContact Details

		public ZString JourneyOnePickUpContactName
		{
			get { return JourneyOnePickUpAddress != null ? JourneyOnePickUpAddress.GetContactName(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyOnePickUpContactPhone
		{
			get { return JourneyOnePickUpAddress != null ? JourneyOnePickUpAddress.GetContactPhone(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyOnePickUpContactFax
		{
			get { return JourneyOnePickUpAddress != null ? JourneyOnePickUpAddress.GetContactFax(ContactType.LocalTransport) : ZString.Empty; }
		}

		#endregion

		#region JourneyOneDeliverToContact Details

		public ZString JourneyOneDeliverToContactName
		{
			get { return JourneyOneDeliverToAddress != null ? JourneyOneDeliverToAddress.GetContactName(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyOneDeliverToContactPhone
		{
			get { return JourneyOneDeliverToAddress != null ? JourneyOneDeliverToAddress.GetContactPhone(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyOneDeliverToContactFax
		{
			get { return JourneyOneDeliverToAddress != null ? JourneyOneDeliverToAddress.GetContactFax(ContactType.LocalTransport) : ZString.Empty; }
		}

		#endregion

		#region JourneyTwoPickUpContact Details

		public ZString JourneyTwoPickUpContactName
		{
			get { return JourneyTwoPickUpAddress != null ? JourneyTwoPickUpAddress.GetContactName(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyTwoPickUpContactPhone
		{
			get { return JourneyTwoPickUpAddress != null ? JourneyTwoPickUpAddress.GetContactPhone(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyTwoPickUpContactFax
		{
			get { return JourneyTwoPickUpAddress != null ? JourneyTwoPickUpAddress.GetContactFax(ContactType.LocalTransport) : ZString.Empty; }
		}

		#endregion

		#region JourneyTwoDeliverToContact Details

		public ZString DeliveryAddress
		{
			get { return BaseJobDeclaration.DeliveryAddress; }
		}

		public ZString JourneyTwoDeliverToContactName
		{
			get { return JourneyTwoDeliverToAddress != null ? JourneyTwoDeliverToAddress.GetContactName(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyTwoDeliverToContactPhone
		{
			get { return JourneyTwoDeliverToAddress != null ? JourneyTwoDeliverToAddress.GetContactPhone(ContactType.LocalTransport) : ZString.Empty; }
		}

		public ZString JourneyTwoDeliverToContactFax
		{
			get { return JourneyTwoDeliverToAddress != null ? JourneyTwoDeliverToAddress.GetContactFax(ContactType.LocalTransport) : ZString.Empty; }
		}

		#endregion

		#endregion

		public DocDocAddressCollection AddressesWithWareHousing
		{
			get
			{
				DocDocAddressCollection docAddressesOnCartageAdvice = new DocDocAddressCollection(Factory);

				if (JourneyOnePickUpAddress != null)
				{
					docAddressesOnCartageAdvice.Add(JourneyOnePickUpAddress);
				}

				if (JourneyOneDeliverToAddress != null)
				{
					docAddressesOnCartageAdvice.Add(JourneyOneDeliverToAddress);
				}

				if (PrintTwoJourneys)
				{
					if (JourneyTwoPickUpAddress != null)
					{
						docAddressesOnCartageAdvice.Add(JourneyTwoPickUpAddress);
					}

					if (JourneyTwoDeliverToAddress != null)
					{
						docAddressesOnCartageAdvice.Add(JourneyTwoDeliverToAddress);
					}
				}
				return docAddressesOnCartageAdvice.GetDocAddressesWithWareHousing();
			}
		}

		public ZBool PrintAsContainers
		{
			get
			{
				if (ContainersInternal != null)
				{
					foreach (DocBaseCusContainer cusContainer in ContainersInternal)
					{
						if ((cusContainer.Type == Core.Constants.ContainerModes.FCL) ||
								(!IsExportMessage && cusContainer.Type == Core.Constants.ContainerModes.FCLMixedShipper))
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public ZBool PrintTwoJourneys
		{
			get { return PrintAsContainers; }
		}

		public ZString FullHandlingInstructions
		{
			get { return DeclarationOrOrgHandlingInstruction; }
		}

		public ZString FullCartageInstructions
		{
			get { return DeclarationOrOrgCartageInstructions; }
		}

		public ZBool IsAir
		{
			get { return TransportModeIsAir; }
		}

		#region Implementation

		public ZBool IsEmptyLeg(ZBool isJourneyOne)
		{
			return PrintTwoJourneys && ((isJourneyOne && IsExportMessage) || (!isJourneyOne && !IsExportMessage));
		}

		public ZBool IsFullLeg(ZBool isJourneyOne)
		{
			return PrintTwoJourneys && ((isJourneyOne && !IsExportMessage) || (!isJourneyOne && IsExportMessage));
		}

		#endregion

		#region ITimeSlotRequest Members

		public ZString ConsolNumber
		{
			get { return null; }
		}

		public ZString BookingReference
		{
			get { return null; }
		}

		public ZDateTime CartageCutOffDate
		{
			get
			{
				var result = ZDateTime.Empty;

				var firstLeg = BaseJobDeclaration.TransportsIncludingRelated.FirstLeg;
				if (firstLeg != null)
				{
					result = PackingMode == Core.Constants.ContainerModes.FCL ?
						firstLeg.JW_TerminalCutOff :
						firstLeg.JW_DepotCutOff;
				}

				return result;
			}
		}

		public ZDateTime CartageAvailableDate
		{
			get { return AvailableDate; }
		}

		public ZDateTime CutOffOrAvailableDate
		{
			get { return IsImportMessage ? AvailableDate : DeliveryOrPickupRequiredBy; }
		}

		public ZDateTime CartageReceivalDate
		{
			get
			{
				var result = ZDateTime.Empty;

				var firstLeg = BaseJobDeclaration.TransportsIncludingRelated.FirstLeg;
				if (firstLeg != null)
				{
					result = PackingMode == Core.Constants.ContainerModes.FCL ?
						firstLeg.JW_TerminalReceivalCommences :
						firstLeg.JW_DepotReceivalCommences;
				}

				return result;
			}
		}

		public ZDateTime CartageStorageCommenceDate
		{
			get { return StorageCommenceDate; }
		}

		public ZDateTime PickupOrStorageCommenceDate
		{
			get { return IsImportMessage ? StorageCommenceDate : EstimatedDeliveryOrPickup; }
		}

		public ZString PickupOrStorageCommenceDateHeading
		{
			get { return IsImportMessage ? CartageAdvice.StorageCommencesHeading : CartageAdvice.PickupDateHeading; }
		}

		public ZString ETAString
		{
			get { return FreightHelperClass.FormatETAETDDate(TransportModeIsSea, ETA); }
		}

		public ZString ETDString
		{
			get { return FreightHelperClass.FormatETAETDDate(TransportModeIsSea, ETD); }
		}

		public ZString BookingETA
		{
			get { return null; }
		}

		public ZString BookingETD
		{
			get { return null; }
		}

		#endregion

		#endregion

		#region IContainsSuppressedFields

		public ZBool SuppressFlightDetails
		{
			get { return false; }
		}

		public ZString MasterBill_OrSuppressed
		{
			get { return Suppression.GetValue(MasterBillNum, BaseJobDeclaration, SuppressFields.MasterBill, "*", DocumentContactType); }
		}

		public ZString MasterBillAndIssueDate_OrSuppressed
		{
			get { return Suppression.GetValue(FreightHelperClass.FormatBillAndIssueDate(MasterBill_OrSuppressed, MasterBillIssueDate), BaseJobDeclaration, SuppressFields.MasterBill, "*", DocumentContactType); }
		}

		public ZString TransportInfo_OrSuppressed
		{
			get { return Suppression.GetValue(TransportInfo, BaseJobDeclaration, SuppressFields.TransportInfo, "*", DocumentContactType); }
		}

		public ZString ETD_OrSuppressed
		{
			get { return Suppression.GetValue(CollectedFromETDString, BaseJobDeclaration, SuppressFields.ETD, "*", DocumentContactType); }
		}

		public ZString ATD_OrSuppressed
		{
			get { return Suppression.GetValue(CollectedFromETDString, BaseJobDeclaration, SuppressFields.ATD, "*", DocumentContactType); }
		}

		public ZString LoadingETD_OrSuppressed
		{
			get { return Suppression.GetValue(LoadingETDString, BaseJobDeclaration, SuppressFields.ETD, "*", DocumentContactType); }
		}

		public ZString LoadingATD_OrSuppressed
		{
			get { return Suppression.GetValue(LoadingETDString, BaseJobDeclaration, SuppressFields.ATD, "*", DocumentContactType); }
		}

		public ZString CarrierName_OrSuppressed
		{
			get { return ""; }
		}

		public ZString CarrierCCC_OrSuppressed
		{
			get { return ""; }
		}

		public ZString SuppressFlightDetailsFooter
		{
			get { return ""; }
		}

		#endregion

		#region IRequestForMissingDocuments Members

		public ZString ContainerNumbers
		{
			get { return LineOfContainerNumbers; }
		}

		public ZString SealNumbers
		{
			get { return LineOfSealNumbers; }
		}

		public ZString OwnerRefAndOrderRefHeading
		{
			get { return Res.GetString("479fb9d3-402a-48b9-bd6f-0ed2764b538f", "ORDER NUMBERS / REFERENCE"); }
		}

		public ZString ConsigneeOrgHeading
		{
			get { return Res.GetString("6362b398-0b66-4b46-98fe-26201776f5e1", "IMPORTER"); }
		}

		public ZString ConsignorOrgHeading
		{
			get { return Res.GetString("854f9bca-9479-4bdf-b964-8985cde3c38c", "SUPPLIER"); }
		}

		public DocOrganisation ConsigneeOrg
		{
			get { return Consignee; }
		}

		public DocOrganisation ConsignorOrg
		{
			get { return Consignor; }
		}

		#endregion

		#region Exporter Docs Fields
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant String")]
		public static class SDFields
		{
			public const string NetMassInKG = "Net Mass (kg)";
			public const string PreCarriageFrom = "Pre-Carriage From";
			public const string PreCarriageBy = "Pre-Carriage By";
			public const string DocumentaryCreditNumber = "Documentary Credit Number";
			public const string PercentageContentFromOrigin = "Percentage of Content From Origin";
			public const string AdditionalPaymentTerms = "Payment Terms (Additional)";
			public const string LetterOfCreditNumber = "Letter of Credit Number";
			public const string LetterOfCreditDate = "Letter of Credit Date";
			public const string InsurancePolicyNumber = "Insurance Policy Number";
			public const string InsuredValue = "Insured Value (Include Currency)";
			public const string PackingListNo = "Packing List Number";
			public const string PackDate = "Pack Date";
			public const string ContainerListNo = "Container List Number";
		}

		public ZString NetMass
		{
			get { return BaseJobDeclaration.DocNote.GetSystemDefinedFieldValue(SDFields.NetMassInKG); }
		}

		public ZString NetMassInWords
		{
			get
			{
				ZDecimal mass = ZDecimal.ParseSafe(NetMass, ZDecimal.Zero);
				return NumberToString_EN.ConvertNumberToWords((long)mass);
			}
		}

		public ZString PreCarriageFrom
		{
			get { return BaseJobDeclaration.DocNote.GetSystemDefinedFieldValue(SDFields.PreCarriageFrom); }
		}

		public ZString PreCarriageBy
		{
			get { return BaseJobDeclaration.DocNote.GetSystemDefinedFieldValue(SDFields.PreCarriageBy); }
		}

		public ZString DocumentaryCreditNumber
		{
			get { return BaseJobDeclaration.DocNote.GetSystemDefinedFieldValue(SDFields.DocumentaryCreditNumber); }
		}

		public ZString PercentageContentFromOrigin
		{
			get
			{
				ZString result = BaseJobDeclaration.DocNote.GetSystemDefinedFieldValue(SDFields.PercentageContentFromOrigin);
				return result.IsEmpty ? new ZString("0") : result;
			}
		}

		public ZString AdditionalPaymentTerms
		{
			get { return BaseJobDeclaration.DocNote.GetSystemDefinedFieldValue(SDFields.AdditionalPaymentTerms); }
		}

		public ZString LetterOfCreditNumber
		{
			get { return BaseJobDeclaration.DocNote.GetSystemDefinedFieldValue(SDFields.LetterOfCreditNumber); }
		}

		public ZString LetterOfCreditDate
		{
			get { return BaseJobDeclaration.DocNote.GetSystemDefinedFieldValue(SDFields.LetterOfCreditDate); }
		}

		public ZString InsurancePolicyNumber
		{
			get { return BaseJobDeclaration.DocNote.GetSystemDefinedFieldValue(SDFields.InsurancePolicyNumber); }
		}

		public ZString InsuredValue
		{
			get { return BaseJobDeclaration.DocNote.GetSystemDefinedFieldValue(SDFields.InsuredValue); }
		}

		public ZString PackingListNo
		{
			get
			{
				ZString result = BaseJobDeclaration.DocNote.GetSystemDefinedFieldValue(SDFields.PackingListNo);
				if (result.IsEmpty)
				{
					result = BaseJobDeclaration.JE_DeclarationReference;
				}
				return result;
			}
		}

		public ZString PackDate
		{
			get
			{
				ZString result = BaseJobDeclaration.DocNote.GetSystemDefinedFieldValue(SDFields.PackDate);
				if (result.IsEmpty)
				{
					result = ZDateTime.Today.ToShortDateString();
				}
				return result;
			}
		}

		public ZString ContainerListNo
		{
			get
			{
				ZString result = BaseJobDeclaration.DocNote.GetSystemDefinedFieldValue(SDFields.ContainerListNo);
				if (result.IsEmpty)
				{
					result = BaseJobDeclaration.JE_DeclarationReference;
				}
				return result;
			}
		}

		public ZString AdditionalInformation => BaseJobDeclaration.Invoices.FirstOrDefault()?.JZ_Remarks ?? ZString.Empty;

		public ZString ExportersBankName => BaseJobDeclaration.Invoices.FirstOrDefault()?.JZ_ExporterBankName ?? ZString.Empty;

		public ZString ExportersBankAccountNo => BaseJobDeclaration.Invoices.FirstOrDefault()?.JZ_ExporterBankAccountNumber ?? ZString.Empty;

		public ZString ExportersBankSWIFTCode => BaseJobDeclaration.Invoices.FirstOrDefault()?.JZ_ExporterBankSWIFTCode ?? ZString.Empty;
		#endregion

		protected override Image JobHeaderBranchLogo
		{
			get
			{
				Image result = base.JobHeaderBranchLogo;
				if (result == null)
				{
					DocBranch branch = Branch;
					return branch != null ? branch.Logo : null;
				}
				return result;
			}
		}

		public IBusinessObjectCollection<IErrorsRecord> LatestDispositions
		{
			get { return fLatestDispositions ?? (fLatestDispositions = CreateLatestDispositionsCore()); }
		}
		IBusinessObjectCollection<IErrorsRecord> fLatestDispositions;

		protected virtual IBusinessObjectCollection<IErrorsRecord> CreateLatestDispositionsCore()
		{
			return (IBusinessObjectCollection<IErrorsRecord>)Activator.CreateInstance(ObjectFactory.GetType<IStatusErrorsDataViewCollection>(), new object[] { Factory });
		}
	}
}
