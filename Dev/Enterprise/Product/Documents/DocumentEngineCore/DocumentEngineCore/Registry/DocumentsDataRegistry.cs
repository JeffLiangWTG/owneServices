using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.BrandManager;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.DocumentEngineCore.Registry.LogDocumentRenderer;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	public sealed class DocumentsDataRegistry : RegistryItemSet
	{
		DocumentsDataRegistry()
		{
		}

		public static DocumentsDataRegistry Instance
		{
			get
			{
				if (fInstance == null)
				{
					fInstance = new DocumentsDataRegistry();
				}
				return fInstance;
			}
		}
		[ThreadStatic]
		static DocumentsDataRegistry fInstance;

		public override bool IsForProductivityWise => true;

		#region Categories

		public abstract class Categories : RawDataRegistry.Categories
		{
			public static MultilingualString Documents_QuotationsandRates_IncludeTransportProvideronQuotationDocument { get { return CombineCategories(Documents_QuotationsandRates, ResString.GetMultilingualString("51ee7ac8-e2bd-4aa3-bd61-ead3ab53eccd", "Include Transport Provider on Quotation Document")); } }
			public static MultilingualString Documents_DocBuilder { get { return CombineCategories(Documents, ResString.GetMultilingualString("dc3d2f20-9a76-4f4a-a8b5-ae20578d4e63", "DocBuilder")); } }
			public static MultilingualString Documents_DocBuilder_Accounting { get { return CombineCategories(Documents_DocBuilder, ResString.GetMultilingualString("65ddca59-f7d0-4bc0-bd6d-9f80df62af7d", "Accounting")); } }
			public static MultilingualString Documents_LinerAgency_DeliveryOrder { get { return CombineCategories(Documents_LinerAgency, ResString.GetMultilingualString("d95846f0-e98d-4210-8fc7-be06fb5dcce5", "Delivery Order")); } }
			public static MultilingualString Documents_LinerAgency_DetentionAdvice { get { return CombineCategories(Documents_LinerAgency, ResString.GetMultilingualString("48587746-d914-4baa-9c2b-523b430ee864", "Detention Advice")); } }
			public static MultilingualString Documents_Forwarding_Consol_CarrierBookingRequest { get { return CombineCategories(Documents_Forwarding_Consol, ResString.GetMultilingualString("e11e7a2c-b99d-46b3-a848-0af75b69ec01", "Carrier Booking Request")); } }
			public static MultilingualString Documents_Forwarding_Consol_AirlineSecurityStatements { get { return CombineCategories(Documents_Forwarding_Consol, ResString.GetMultilingualString("72c48ac3-7d88-4c89-ba50-2480b64af4dd", "Airline Security Statements")); } }
			public static MultilingualString Documents_Forwarding_Consol_RateConfirmation { get { return CombineCategories(Documents_Forwarding_Consol, ResString.GetMultilingualString("797d9970-1b0e-4e0c-8e7f-9ed2a830e600", "Rate Confirmation")); } }
			public static MultilingualString Documents_Forwarding_Consol_RequestForProfitShare { get { return CombineCategories(Documents_Forwarding_Consol, ResString.GetMultilingualString("7a3c4022-8e7d-470d-a0cb-e39e797cdd47", "Request For Profit Share")); } }
			public static MultilingualString Documents_Forwarding_Customs_DelayAlert { get { return CombineCategories(Documents_Forwarding_Customs, ResString.GetMultilingualString("a06bb0aa-b5f6-4d14-ad7e-4faff567e109", "Delay Alert")); } }
			public static MultilingualString Documents_Forwarding_Customs_EFTRequest { get { return CombineCategories(Documents_Forwarding_Customs, ResString.GetMultilingualString("479c3d05-50d6-47d1-a3fe-17e1cd6f709a", "EFT Request")); } }
			public static MultilingualString Documents_Forwarding_Shipment_ContainerDetentionReminder { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("41ab8bb9-7f68-4829-8fc1-c2eb6294092c", "Container Detention Reminder")); } }
			public static MultilingualString Documents_Forwarding_Shipment_ForwardersCertificateofReceipt { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("108e8575-1d65-4cda-aed1-c32870211f09", "Forwarders Certificate of Receipt")); } }
			public static MultilingualString Documents_Forwarding_Shipment_RequestforCollectCharges { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("1d2084ad-4643-42af-a2f6-a79c8d415af2", "Request for Collect Charges")); } }
			public static MultilingualString Documents_Forwarding_Shipment_RequestForProfitShare { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("9eb79529-8a85-42e6-ac99-b438485307b9", "Request For Profit Share")); } }
			public static MultilingualString Documents_Forwarding_Shipment_LetterofGuarantee { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("87f6e5bb-b929-4f77-b48b-b4ca26688cc0", "Letter of Guarantee")); } }
			public static MultilingualString Documents_Forwarding_Shipment_InterimReceipt { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("fce03bc7-7f54-4e3d-a912-68b240bb0a2b", "Interim Receipt")); } }
			public static MultilingualString Documents_Forwarding_Shipment_ShippingOrder { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("457bd0c2-47e4-43f8-9524-c272ed3777f5", "Shipping Order")); } }
			public static MultilingualString Documents_Forwarding_Shipment_DelayAlert { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("0ae7d0d2-6ad1-4f87-b234-1d2170d2d1d9", "Delay Alert")); } }
			public static MultilingualString Documents_Forwarding_Shipment_ProofofDelivery { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("f3dab348-d577-4817-962c-98091ab65529", "Proof of Delivery")); } }
			public static MultilingualString Documents_Forwarding_Shipment_ContainerLiabilityStatement { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("ec694b09-0e0e-490c-a979-64e18fb47ab0", "Container Liability Statement")); } }
			public static MultilingualString Documents_Forwarding_Shipment_WorkSheet { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("564fa834-8681-485d-9b74-42ca8c715b20", "Work Sheet")); } }
			public static MultilingualString Documents_Forwarding_Shipment_FOBAndienungVersicherungsanmeldung { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("8ed91b4f-4328-4808-baf0-31ad4cf0bb1f", "FOB - Andienung & Versicherungsanmeldung")); } }
			public static MultilingualString Documents_Forwarding_Shipment_Verpflichtungsschein { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("72a06d64-c575-409b-88d8-e6772b0e7b67", "Verpflichtungsschein")); } }
			public static MultilingualString Documents_Forwarding_Shipment_IcelandicArrivalNoticeFooter { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("33fe23cf-59b9-46ee-a598-ac41a6ddf1cc", "Icelandic Arrival Notice Footer")); } }
			public static MultilingualString Documents_Forwarding_Shipment_LetterofIndemnity { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("2626942c-77c0-4fe9-88d4-5ea0d9735a09", "Letter of Indemnity")); } }
			public static MultilingualString Documents_Forwarding_Shipment_DeliveryInformation { get { return CombineCategories(Documents_Forwarding_Shipment, ResString.GetMultilingualString("a53b62c6-d73a-4ac7-b9dc-6737cf2adf2e", "Delivery Information")); } }
			public static MultilingualString Documents_Forwarding_ExporterDocs { get { return CombineCategories(Documents_Forwarding, ResString.GetMultilingualString("b7dccee0-e1ed-4a87-b194-7a6790039520", "Exporter Docs")); } }
			public static MultilingualString Documents_Forwarding_ExporterDocs_CertificateofOrigin { get { return CombineCategories(Documents_Forwarding_ExporterDocs, ResString.GetMultilingualString("eb687d9f-2b05-4a99-8587-77e8927d4895", "Certificate of Origin")); } }
			public static MultilingualString Documents_Service { get { return CombineCategories(Documents, ResString.GetMultilingualString("5485754e-16ad-452b-8b85-77aee5d2548c", "Service")); } }
			public static MultilingualString Documents_Service_RequestForService { get { return CombineCategories(Documents_Service, ResString.GetMultilingualString("4402352b-49fc-4bf4-9160-d8a74619fa13", "Request For Service")); } }
			public static MultilingualString Documents_Service_AuthorisationForService { get { return CombineCategories(Documents_Service, ResString.GetMultilingualString("cf74807e-b74e-4597-a51f-1731776859a4", "Authorization For Service")); } }
			public static MultilingualString Documents_Branding { get { return CombineCategories(Documents, ResString.GetMultilingualString("3fc9aeaf-a14b-42b9-b545-2f4c81c72003", "Branding")); } }
			public static MultilingualString Documents_CFS_GatePass { get { return CombineCategories(Documents_CFS, ResString.GetMultilingualString("9c80fbf5-ee15-4adf-8bef-8ac116454601", "Gate Pass")); } }
			public static MultilingualString Documents_Orders_OrderDelayAlert { get { return CombineCategories(Documents_Orders, ResString.GetMultilingualString("c3da473d-e8f3-4b23-b391-aa8e086735f1", "Order Delay Alert")); } }
			public static MultilingualString Documents_LandedCosting { get { return CombineCategories(Documents, ResString.GetMultilingualString("5170e0b5-f49e-4fb9-817f-51fc5370694a", "Landed Costing")); } }
			public static MultilingualString Documents_Labels { get { return CombineCategories(Documents, ResString.GetMultilingualString("1db71480-be68-449b-b184-fec7826e54c4", "Labels")); } }
			public static MultilingualString Documents_RecipientDetails { get { return CombineCategories(Documents, ResString.GetMultilingualString("f547bb67-3dc1-41c3-9b3d-557f8363309a", "Recipient Details")); } }
			public static MultilingualString Documents_Warehouse { get { return CombineCategories(Documents, ResString.GetMultilingualString("bcd362c3-7b7e-491d-b76a-620c5085e5f0", "Warehouse")); } }
			public static MultilingualString Documents_Warehouse_CartageAdvice { get { return CombineCategories(Documents_Warehouse, ResString.GetMultilingualString("dd8d6d2e-0358-4329-aefb-cc430641b1e3", "Cartage Advice")); } }
			public static MultilingualString Documents_TransportBooking { get { return CombineCategories(Documents, ResString.GetMultilingualString("61a24b41-3b9e-4744-82c1-f18cb643217e", "Transport Booking")); } }
			public static MultilingualString Documents_TransportBooking_CartageAdvice { get { return CombineCategories(Documents_TransportBooking, ResString.GetMultilingualString("0e199c2d-5d78-4fb1-aa8c-7d16bae98514", "Cartage Advice")); } }
			public static MultilingualString Documents_AddressFormatting { get { return CombineCategories(Documents, ResString.GetMultilingualString("e32a6698-9cbf-45ae-985a-d8300e07c86f", "Address Formatting")); } }
			public static MultilingualString Documents_DocumentSigningService { get { return CombineCategories(Documents, ResString.GetMultilingualString("BECE6E33-6F89-48D4-AEAF-79CBFA497325", "Document Signing Service")); } }
			public static MultilingualString Documents_DocumentSigningService_SignatureImagePositioning { get { return CombineCategories(Documents_DocumentSigningService, ResString.GetMultilingualString("e0a7fc25-09d1-4eef-bc6b-4c93e30f62d9", "Signature Image Positioning")); } }
			public static MultilingualString Documents_DocumentSigningService_Portugal => CombineCategories(Documents_DocumentSigningService, ResString.GetMultilingualString("A6EF78EA-983B-4681-B4E3-581FEC004D5B", "Portugal (PT)"));
		}

		#endregion

		public CustomsIncoTermOverrideCollectionRegistryItem CustomsIncoTermsOverride
		{
			get
			{
				return GetItem("CustomsIncoTermsOverride", delegate
				{
					return new CustomsIncoTermOverrideCollectionRegistryItem(
						"CustomsIncoTermsOverride",
						Categories.Documents_Customs,
						ResString.GetMultilingualString("68d0b369-1d90-05a4-4a29-b9e351ed04f6", "Customs Incoterms Override"),
						ResString.GetMultilingualString("c782e822-4a2c-1d8a-4db2-3207c843a46e", "Documents like 'Commercial Invoice' will normally show the Incoterm as entered on the Invoice which is applicable only to that Customs Country/Region. The document can show the equivalent International Incoterm code by mapping it here."),
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default
						);
				});
			}
		}

		public IntRegistryItem ReportDBCommandTimeOut
		{
			get
			{
				return GetItem("ReportDBCommandTimeOut", delegate
				{
					IntRegistryItem result = new IntRegistryItem(
						"ReportDBCommandTimeOut",
						Categories.Documents,
						ResString.GetMultilingualString("a7ed7fb3-0a50-4ccb-95eb-903759e17f1b", "Report Command Timeout"),
						ResString.GetMultilingualString("a2506c76-271c-4f40-a102-52ca03190672", @"This is the time in seconds after which a database query on a report will timeout.
This timeout does not apply to the entire report generation process, but only the portion where the data necessary to generate the report is retrieved from the database. "),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						900);
					result.DataType = new IntRegistryDataType(300, 1800);
					return result;
				});
			}
		}

		public IntRegistryItem ReportPreviewTimeout
		{
			get
			{
				return GetItem("ReportPreviewTimeout", delegate
				{
					IntRegistryItem result = new IntRegistryItem(
						"ReportPreviewTimeout",
						Categories.Documents,
						ResString.GetMultilingualString("c477381c-ee1c-42b9-b1e5-a7068c1e156e", "Report Preview Timeout"),
						ResString.GetMultilingualString("8091e680-c350-4a06-8fc3-19133e1fe840", @"This is the time in seconds after which a database query on a report preview will timeout.
This timeout does not apply to the entire report generation process, but only the portion where the data necessary to generate the report is retrieved from the database."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						300);
					result.DataType = new IntRegistryDataType(30, 1800);
					return result;
				});
			}
		}

		public IntRegistryItem ReportDBCommandMaximumDegreeOfParallelism
		{
			get
			{
				return GetItem("ReportDBCommandMaximumDegreeOfParallelism", () =>
					new IntRegistryItem(
						"ReportDBCommandMaximumDegreeOfParallelism",
						Categories.Documents,
						ResString.GetMultilingualString("2fcbdc8d-a49e-49c5-90c0-cb25350a3840", "Report Command Maximum Degree Of Parallelism"),
						ResString.GetMultilingualString("30df0fa4-2655-4fea-9a44-53ecb88f597a",
							@"The MAXDOP(Maximum Degree of Parallelism) value specifies how many processors are used to execute a single SQL query.
Note: A high value could accelerate query execution, but significantly raises CPU usage and potentially affects system performance. The value of zero indicates that it is unset, and the decision to parallelize the query is made by the database engine."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						0));
			}
		}

		public IntRegistryItem WebPrintDocumentPackMaxSize =>
			GetItem("WebPrintDocumentPackMaxSize", () =>
				new IntRegistryItem(
					"WebPrintDocumentPackMaxSize",
					Categories.Documents,
					ResString.GetMultilingualString("9b88bfe5-8c44-48f6-a88a-fa35e036189e", "Remote Printing Doc Pack Maximum Batch Size"),
					ResString.GetMultilingualString("4a381976-9a41-48ee-a956-87c530e87871", @"Maximum batch size, in Megabytes, for Doc Pack print jobs to be sent to the Remote Printing print server.

By default, Doc Packs are sent to the print server in one batch.  When there are many documents in the Doc Pack, the size of the batch can be too large for the network to handle and this can disrupt the delivery of the print jobs.  This setting will split larger Doc Packs into several batches when sending to the print server.

The default value is 0 for no limit on the batch size (i.e. doing it in a single batch) and the maximum batch size is 1000 Megabytes."),
					RegistryStorageFlags.System,
					RegistryOptions.PreserveTestValue,
					0)
				{
					DataType = new IntRegistryDataType(0, 1000)
				});

		public BooleanRegistryItem WebPrintAllowDirectPrintPrintPushNotification =>
			GetItem("WebPrintAllowDirectPrintPrintPushNotification", () =>
				new BooleanRegistryItem(
					"WebPrintAllowDirectPrintPrintPushNotification",
					Categories.Documents,
					ResString.GetMultilingualString("4043be4b-8754-4bab-99b8-30c49728803e", "Allow Pushing Print Jobs Directly to Print Server"),
					ResString.GetMultilingualString("dc58b9e2-87bc-49d4-bb7a-6c2ba6de3cbe", @"When enabled, print jobs can be pushed immediately to the Print Server via the WebPrint Client. Additionally, the Enable Print Nudging option in the WebPrint Client Configuration needs to be enabled.

Print jobs that are under 30 Kilobytes will be pushed directly to the Print Server. Larger print jobs will trigger a nudge notification to the Print Server to download the print job.

Note: Pushing print jobs directly to the Print Server may cause print jobs to be printed out of order. If you have documents that need to be printed in sequential order, do NOT enable this option."),
					RegistryStorageFlags.System,
					RegistryOptions.PreserveTestValue,
					false));

		public IntRegistryItem WebPrintSignalRIncomingMaxSize =>
			GetItem("WebPrintSignalRIncomingMaxSize", () =>
				new IntRegistryItem(
					"WebPrintSignalRIncomingMaxSize",
					Categories.Documents,
					ResString.GetMultilingualString("221ebd9b-128d-4c70-9dff-a44a926af17b", "Remote Printing Incoming Message Maximum Size"),
					ResString.GetMultilingualString("191f8952-0f13-4dc2-8ea9-1bd82dc977c6", @"Maximum size, in Kilobytes, for incoming messages from Remote Printing Client to Remote Printing Web Application.

When Printing Nudging is enabled, Remote Printing Clients will register on Remote Printing Web Application Server to be able to receive nudging about new print jobs. Clients will send some system messages to Server (e.g. list of available printers). Sometimes system such messages can exceed maximum length of allowed incoming messages. Default maximum length can be overridden with this registry.

Accepted values: 0 - 100,000 Kilobyte (100 Megabyte), where 0 means no maximum size limit.
It is not recommended to make maximum size too large, as it may increase memory usage and reduce Remote Printing Server performance."),
					RegistryStorageFlags.System,
					RegistryOptions.PreserveTestValue,
					100)
				{
					DataType = new IntRegistryDataType(0, 100_000)
				});

		public WebPrintNudgeRegistryItem WebPrintNudge =>
			GetItem("WebPrintNudge", () =>
				new WebPrintNudgeRegistryItem(
					"WebPrintNudge",
					Categories.Documents,
					ResString.GetMultilingualString("7E83554C-9148-4FA9-97B2-B0798AFE1063", "Remote Printing Nudge"),
					ResString.GetMultilingualString("BE65F924-F0D3-4B32-8344-86F1DB51BC85", @"This setting controls how Document Engine sends nudge messages to Remote Printing web service.

When option 'IP address' (default) is selected, nudge messages will be sent as HTTP request directly to the web service IP address.

When option 'Web service URL address' is selected, nudge messages will be sent as HTTP request to the web service URL address.

NOTE:
If direct connection to the web service IP address is not possible, this setting will be automatically changed to 'Web service URL address'.
It will switch back to 'IP address' after specified number of hours."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController,
					new WebPrintNudge { EnableIPAddress = true, SwtichBackToIPAddressIntervalInHours = WebPrintNudgeDefaultValue.SwtichBackToIPAddressIntervalInHours }));

		public WebPrintNudgeSuspendingRegistryItem WebPrintNudgeSuspending =>
			GetItem("WebPrintNudgeSuspending", () =>
				new WebPrintNudgeSuspendingRegistryItem(
					"WebPrintNudgeSuspending",
					Categories.Documents,
					ResString.GetMultilingualString("95CF01DB-0BCF-4642-80DB-A62884AE1625", "Remote Printing Nudge Suspending"),
					ResString.GetMultilingualString("50A91C9E-FF84-45F0-A4A2-7E9B2BB0D41D", @"This setting controls how to suspend remote printing nudge request.

Minutes E.g:
Maximum error count: 3, Interval minutes: 15, Suspend minutes: 10
If there are more than 3 errors within 15 minutes, Remote Printing Nudge will be suspended for 10 minutes.

Hours E.g:
Maximum error count: 10, Interval hours: 1, Suspend hours: 1
If there are more than 10 errors within 1 hour, Remote Printing Nudge will be suspended for 1 hour."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController,
					new WebPrintNudgeSuspending
					{
						MaxErrorsInMinutes = 3,
						IntervalMinutes = 15,
						SuspendMinutes = 10,

						MaxErrorsInHours = 10,
						IntervalHours = 1,
						SuspendHours = 1,
					}));

		public BooleanRegistryItem WebPrintForceToUseHTTPSForWebPrintRequests =>
			GetItem("WebPrintForceToUseHTTPSForWebPrintRequests", () =>
				new BooleanRegistryItem(
					"WebPrintForceToUseHTTPSForWebPrintRequests",
					Categories.Documents,
					ResString.GetMultilingualString("2606CBAC-35DF-43E9-A1AC-84F327777F67", "Force to use HTTPS for WebPrint requests"),
					ResString.GetMultilingualString("75521B19-9375-460A-9567-F3C585F5CD1E", "When enabled, Remote Printing Web Application Server will receive nudging HTTPS requests, please disable it if something goes wrong when enabled."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForController,
					EnvProxy.IsHostedWithCargowise));

		#region SuppressResourceStringsCheckRegion

		public BooleanRegistryItem DocumentsSourceControlIsEnabled
		{
			get
			{
				return GetItem("DocumentsSourceControlIsEnabled", delegate
				{
					return new BooleanRegistryItem(
						"DocumentsSourceControlIsEnabled",
						Categories.Documents,
						(NoResString)"Enable documents source control.",
						(NoResString)"Specifies whether to go to Source Control when building the 'documents' menu. This is a Development only registry item and by default is enabled. If you disable it, the documents menu will be much faster to open, however you will not be able to perform any source control operations.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForDevelopers,
						true);
				});
			}
		}

		#endregion

		public DocumentPreviewFormLayoutRegistryItem DocumentPreviewFormLayout
		{
			get
			{
				return GetItem("DocumentPreviewFormLayout", delegate
				{
					return new DocumentPreviewFormLayoutRegistryItem("DocumentPreviewFormLayout");
				});
			}
		}

		public BooleanRegistryItem AirFreightIncludeTransportProviderOnQuotation
		{
			get
			{
				return GetItem("AirFreightIncludeTransportProviderOnQuotation", delegate
				{
					return new BooleanRegistryItem(
						"AirFreightIncludeTransportProviderOnQuotation",
						Categories.Documents_QuotationsandRates_IncludeTransportProvideronQuotationDocument,
						ResString.GetMultilingualString("2b319cb9-1503-4e3f-a2ab-9631f5623922", "Air Freight"),
						ResString.GetMultilingualString("31cc3f23-b266-449d-b3cc-a56ac612c07e", "This will determine whether the Transport Provider (Shipping or Air line) will be mentioned on the Quotation document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						true);
				});
			}
		}

		public BooleanRegistryItem SeaFreightIncludeTransportProviderOnQuotation
		{
			get
			{
				return GetItem("SeaFreightIncludeTransportProviderOnQuotation", delegate
				{
					return new BooleanRegistryItem(
						"SeaFreightIncludeTransportProviderOnQuotation",
						Categories.Documents_QuotationsandRates_IncludeTransportProvideronQuotationDocument,
						ResString.GetMultilingualString("83dae6d4-c939-4284-9072-c3c5f6072445", "Sea Freight"),
						ResString.GetMultilingualString("31cc3f23-b266-449d-b3cc-a56ac612c07e", "This will determine whether the Transport Provider (Shipping or Air line) will be mentioned on the Quotation document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						true);
				});
			}
		}

		public BooleanRegistryItem AllowDocumentsToBeModified
		{
			get
			{
				return GetItem("AllowDocumentsToBeModified", delegate
				{
					return new BooleanRegistryItem(
						"AllowDocumentsToBeModified",
						Categories.Documents,
						ResString.GetMultilingualString("133fe7fb-342e-4b13-822d-5cf6bbb8f03b", "Allow Documents To Be Modified"),
						ResString.GetMultilingualString("f4428438-bd3b-407d-8f7d-d4ae0f2d23bc", "When printing / previewing documents, there is a button that allows user to view the document in an editing mode and user would be able to override most of the fields on that document before printing. Turning on this registry setting will enable users to modify documents. Individual staff / document can then be granted or denied 'modify' access in the Security tab of Staff or Group form. If this registry is turned off, then this feature is disabled throughout the system."),
						RegistryStorageFlags.System,
						RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public BooleanRegistryItem AddEmailFaxCoverNote
		{
			get
			{
				return GetItem("AddEmailFaxCoverNote", delegate
				{
					return new BooleanRegistryItem(
						"AddEmailFaxCoverNote",
						Categories.Documents,
						ResString.GetMultilingualString("170e7809-6250-4f1e-98B7-13324fca5c4f", "Add Email/Fax Cover Note"),
						ResString.GetMultilingualString("911e3275-33dc-4398-a6c4-70ea27f382f3", "Specifies whether the email fax cover note will be added to the email body when delivering documents through email."),
						RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public BooleanRegistryItem AddEmailSignature
		{
			get
			{
				return GetItem("AddEmailSignature", delegate
				{
					return new BooleanRegistryItem(
						"AddEmailSignature",
						Categories.Documents,
						ResString.GetMultilingualString("b06e7f0f-eeb5-4956-bf4d-009d5ae849ca", "Add Email Signature"),
						ResString.GetMultilingualString("17539cd2-502b-4700-9fbe-05d4bb9b0683", "Specifies whether the email signature will be added to the email footer when delivering documents through email."),
						RegistryStorageFlags.Company,
						true);
				});
			}
		}

		public BooleanRegistryItem EmbedFontsInPDF
		{
			get
			{
				return GetItem("EmbedFontsInPDF", delegate
				{
					return new BooleanRegistryItem(
						"EmbedFontsInPDF",
						Categories.Documents,
						ResString.GetMultilingualString("d284a76b-ffa8-40ec-a3b0-bbbc651ecd2f", "Embed Fonts In PDF"),
						ResString.GetMultilingualString("fcccef6f-3129-46ef-b40e-9e9e4ae60a77", "Ensures that fonts are embedded in PDF document so that when these documents are rendered on non-{0} workstations, the fonts display correctly.", BrandingFactory.Instance.ProductName),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public StringRegistryItem EPrintEmailAddress
		{
			get
			{
				return GetItem("EPrintEmailAddress",
					() => new StringRegistryItem(
						"EPrintEmailAddress",
						Categories.Documents,
						ResString.GetMultilingualString("178c294c-a9bd-414c-b73b-86ddf4357071", "ePrint Email Address"),
						ResString.GetMultilingualString("672070d6-2eac-4034-8515-5aff0dd9d7aa", "Specify the printer email address that will be used to deliver documents and reports using the ePrint delivery method."),
						new EmailStringRegistryDataType(),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.Default)
					);
			}
		}

		public BooleanRegistryItem UseRecompileQueryHint
		{
			get
			{
				return GetItem("UseRecompileQueryHint", delegate
				{
					return new BooleanRegistryItem(
						"UseRecompileQueryHint",
						Categories.Documents,
						ResString.GetMultilingualString("99d102d0-e91e-4401-bd0f-d333dd83c376", "Use recompile SQL Query Hint"),
						ResString.GetMultilingualString("2b26a7e6-deef-4fe6-b0c4-f8b85951183c", "This will determinate whether 'recompile' SQL Query Hint will be used by default for all documents."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public DocBuilderDataSourceRegistryItem DocBuilderDataSource
		{
			get
			{
				return GetItem("DocBuilderDataSource", delegate
				{
					var defaultValue = new DocBuilderDataSource();
					defaultValue.Brokerage = true;

					return new DocBuilderDataSourceRegistryItem(
						"DocBuilderDataSource",
						Categories.Documents_DocBuilder,
						ResString.GetMultilingualString("5df674c9-2f80-4e19-85df-2a04er037fg4", "DocBuilder Data Source (Freight/Brokerage)"),
						ResString.GetMultilingualString("985ww031-2431-4s12-f775-ed14td22ge34", "When printing documents using DocBuilder, this determines where the system retrieves data from, Freight or Brokerage. If Brokerage is selected, system falls back to Freight data if Brokerage data is absent."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						defaultValue);
				});
			}
		}

		public BooleanRegistryItem UseNewDocBuilderForwardingDocuments
		{
			get
			{
				return GetItem("UseNewDocStripForwardingDocuments", delegate
				{
					return new BooleanRegistryItem(
						"UseNewDocStripForwardingDocuments",
						Categories.Documents_DocBuilder,
						ResString.GetMultilingualString("5cd674b9-2e80-4f19-85ae-2a04be037fe3", "Use DocBuilder Freight Documents"),
						ResString.GetMultilingualString("985dd031-2431-4b12-b775-dc14db66df33", "When printing Freight Documents, use the new style DocBuilder based document that allow the user to include or exclude whatever sections they like."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false);
				});
			}
		}

		public BooleanRegistryItem UseNewDocBuilderOrderManagerDocuments
		{
			get
			{
				return GetItem("UseNewDocBuilderOrderManagerDocuments", delegate
				{
					return new BooleanRegistryItem(
						"UseNewDocBuilderOrderManagerDocuments",
						Categories.Documents_DocBuilder,
						ResString.GetMultilingualString("50e87f2b-6853-4cc0-a8ed-09ea84849658", "Use DocBuilder Order Manager Documents"),
						ResString.GetMultilingualString("6b79459b-c1c1-4461-8825-c0c7627a4e8d", "When printing Order Documents, use the new style DocBuilder based document that allow the user to include or exclude whatever sections they like."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false);
				});
			}
		}

		public BooleanRegistryItem UseNewDocBuilderWarehouseDocumentsOnly
		{
			get
			{
				return GetItem("UseNewDocBuilderWarehouseDocumentsOnly", delegate
				{
					return new BooleanRegistryItem(
						"UseNewDocBuilderWarehouseDocumentsOnly",
						Categories.Documents_DocBuilder,
						ResString.GetMultilingualString("3f900ac5-98c5-47bc-a6d6-1e096fe08364", "Use DocBuilder Warehouse Documents Only"),
						ResString.GetMultilingualString("d3e5e823-8bc5-4522-b154-a18193655cf4", "When printing Warehouse Documents, only use the new style DocBuilder based document that allow the user to include or exclude whatever sections they like."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						true);
				});
			}
		}

		public BooleanRegistryItem UseNewDocBuilderOrganizationDocumentsOnly => GetItem(
			"UseNewDocBuilderOrganizationDocumentsOnly", () => new BooleanRegistryItem(
				"UseNewDocBuilderOrganizationDocumentsOnly",
				Categories.Documents_DocBuilder,
				ResString.GetMultilingualString("BD32E538-98A9-4D66-AE80-CCF7B6516086", "Use DocBuilder Organization Documents Only"),
				ResString.GetMultilingualString("0E2726D5-6783-4502-A632-65CF9CCFAA34",
					"When printing Organization Documents, only use the new style DocBuilder based document that allow the user to include or exclude whatever sections they like."),
				RegistryStorageFlags.System | RegistryStorageFlags.Company,
				DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
				false));

		public BooleanRegistryItem UseNewDocBuilderLinerAndAgencyDocumentsOnly
		{
			get
			{
				return GetItem("UseNewDocBuilderLinerAndAgencyDocumentsOnly", delegate
				{
					return new BooleanRegistryItem(
						"UseNewDocBuilderLinerAndAgencyDocumentsOnly",
						Categories.Documents_DocBuilder,
						ResString.GetMultilingualString("fecfde50-95d9-4ae3-a5f1-7354e28c1580", "Use DocBuilder Liner And Agency Documents Only"),
						ResString.GetMultilingualString("11ba4ff1-0621-4bc8-8cef-594910f1cdc2", "When printing Liner And Agency Documents, only use the new style DocBuilder based document that allow the user to include or exclude whatever sections they like."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false);
				});
			}
		}

		public BooleanRegistryItem UseNewDocBuilderAccountingVoucher
		{
			get
			{
				return GetItem("UseNewDocBuilderAccountingVoucher", delegate
				{
					return new BooleanRegistryItem(
						"UseNewDocBuilderAccountingVoucher",
						Categories.Documents_DocBuilder_Accounting,
						ResString.GetMultilingualString("6BC1A3B2-C8D0-41F5-8051-9FC7400D17D3", "Use DocBuilder Accounting Voucher"),
						ResString.GetMultilingualString("BFDC6586-A29B-454A-9C10-7EFAD600960A", @"This registry setting controls the document template that is used when printing the Accounting Voucher document.

Select 'Yes' - The system will use the DocBuilder Accounting Voucher document.

Select 'No' - The system will use the Legacy Accounting Voucher document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false);
				});
			}
		}

		public BooleanRegistryItem UseNewDocBuilderARInvoice
		{
			get
			{
				return GetItem("UseNewDocBuilderARInvoice", delegate
				{
					return new BooleanRegistryItem(
						"UseNewDocBuilderARInvoice",
						Categories.Documents_DocBuilder_Accounting,
						ResString.GetMultilingualString("80d008df-8e95-442c-a70d-f471d9c1ad3a", "Use DocBuilder AR Invoice"),
						ResString.GetMultilingualString("b497997a-80d4-456d-8051-32b82a3cd758", @"This registry setting controls the document template that is used when printing the AR Invoice document.

Select 'Yes' - The system will use the DocBuilder AR Invoice document.

Select 'No' - The system will use the Legacy AR Invoice document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						true);
				});
			}
		}

		public BooleanRegistryItem UseNewDocBuilderRemittanceAdvice
		{
			get
			{
				return GetItem("UseNewDocBuilderRemittanceAdvice", delegate
				{
					return new BooleanRegistryItem(
						"UseNewDocBuilderRemittanceAdvice",
						Categories.Documents_DocBuilder_Accounting,
						ResString.GetMultilingualString("373663ce-29be-49e3-aab5-0f69a5d04ed8", "Use DocBuilder Remittance Advice"),
						ResString.GetMultilingualString("ac87e549-c58c-419c-a6ad-861c15303fa8", @"This registry setting controls the document template that is used when printing the Remittance Advice.

Select 'Yes' - The system will use the DocBuilder Remittance Advice.

Select 'No' - The system will use the Legacy Remittance Advice."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						true);
				});
			}
		}

		public BooleanRegistryItem UseNewDocBuilderPaymentVoucher
		{
			get
			{
				return GetItem("UseNewDocBuilderPaymentVoucher", delegate
				{
					return new BooleanRegistryItem(
						"UseNewDocBuilderPaymentVoucher",
						Categories.Documents_DocBuilder_Accounting,
						ResString.GetMultilingualString("cf8d5350-195e-4166-b45d-9656ea594197", "Use DocBuilder Payment Voucher"),
						ResString.GetMultilingualString("c1426b2e-1a97-4d11-84fd-5c9c2e6bc0a8", @"This registry setting controls the document template that is used when printing the Payment Voucher.

Select 'Yes' - The system will use the DocBuilder Payment Voucher.

Select 'No' - The system will use the Legacy Payment Voucher."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						true);
				});
			}
		}

		public BooleanRegistryItem UseNewDocBuilderReceiptDocument
		{
			get
			{
				return GetItem("UseNewDocBuilderReceiptDocument", delegate
				{
					return new BooleanRegistryItem(
						"UseNewDocBuilderReceiptDocument",
						Categories.Documents_DocBuilder_Accounting,
						ResString.GetMultilingualString("cf25a061-864e-488a-bdc6-657eee9afa17", "Use DocBuilder Receipt Document"),
						ResString.GetMultilingualString("72ee110b-a515-4eb4-80a4-48b946022c53", @"This registry setting controls the document template that is used when printing the Receipt document.

Select 'Yes' - The system will use the DocBuilder Receipt document.

Select 'No' - The system will use the Legacy Receipt document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						true);
				});
			}
		}

		public BooleanRegistryItem UseNewDocBuilderMatchingDocument
		{
			get
			{
				return GetItem("UseNewDocBuilderMatchingDocument", delegate
				{
					return new BooleanRegistryItem(
						"UseNewDocBuilderMatchingDocument",
						Categories.Documents_DocBuilder_Accounting,
						ResString.GetMultilingualString("beb79885-cac4-4120-af60-e92b176563a7", "Use DocBuilder Matching Document"),
						ResString.GetMultilingualString("ca46823a-ca32-441b-982c-70a45c27bd6a", @"This registry setting controls the document template that is used when printing the Matching document.

Select 'Yes' - The system will use the DocBuilder Matching document.

Select 'No' - The system will use the Legacy Matching document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						true);
				});
			}
		}

		public BooleanRegistryItem UseNewDocBuilderStatementDocument
		{
			get
			{
				return GetItem("UseNewDocBuilderStatementDocument", delegate
				{
					return new BooleanRegistryItem(
						"UseNewDocBuilderStatementDocument",
						Categories.Documents_DocBuilder_Accounting,
						ResString.GetMultilingualString("bfd2bdcb-270d-4c50-bbb2-2e85dee12922", "Use DocBuilder Statement Document"),
						ResString.GetMultilingualString("cfc2aa2e-992f-4ef2-86dc-a30180f3c8c4", @"This registry setting controls the document template that is used when printing the Statement document.

Select 'Yes' - The system will use the DocBuilder Statement document.

Select 'No' - The system will use the Legacy Statement document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						true);
				});
			}
		}

		public BooleanRegistryItem UseNewDocBuilderDepositBatch
		{
			get
			{
				return GetItem("UseNewDocBuilderDepositBatch", delegate
				{
					return new BooleanRegistryItem(
						"UseNewDocBuilderDepositBatch",
						Categories.Documents_DocBuilder_Accounting,
						ResString.GetMultilingualString("ae6eef98-ba46-4387-9b39-698cb9a14c96", "Use DocBuilder Deposit Batch Document"),
						ResString.GetMultilingualString("1ecdc8c6-7a0f-4945-a84c-b65ccb3db2e0", @"This registry setting controls the document template that is used when printing the Deposit Batch document.

Select 'Yes' - The system will use the DocBuilder Deposit Batch document.

Select 'No' - The system will use the Legacy Deposit Batch document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						true);
				});
			}
		}

		public BooleanRegistryItem UseNewDocBuilderCostConfirmationDocument
		{
			get
			{
				return GetItem("UseNewDocBuilderCostConfirmationDocument", delegate
				{
					return new BooleanRegistryItem(
						"UseNewDocBuilderCostConfirmationDocument",
						Categories.Documents_DocBuilder_Accounting,
						ResString.GetMultilingualString("9600cd33-ef0c-4e09-b139-828556ded53f", "Use DocBuilder Cost Confirmation Document"),
						ResString.GetMultilingualString("97d021af-566a-47fe-a14e-9144052163f1", @"This registry setting controls the document template that is used when printing the cost confirmation document.

Select 'Yes' - The system will use the DocBuilder cost confirmation document.

Select 'No' - The system will use the Legacy cost confirmation document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						true);
				});
			}
		}

		public BooleanRegistryItem UseNewDocBuilderSupplementaryDetail
		{
			get
			{
				return GetItem("UseNewDocBuilderSupplementaryDetail", delegate
				{
					return new BooleanRegistryItem(
						"UseNewDocBuilderSupplementaryDetail",
						Categories.Documents_DocBuilder_Accounting,
						ResString.GetMultilingualString("c8c26082-9da8-4e66-8861-884c171f8945", "Use DocBuilder Periodic Invoice Supplementary Detail"),
						ResString.GetMultilingualString("5edad09e-11ff-4957-897e-8a4c954e4aa2", @"This registry setting controls the document template that is used when printing the supplementary detail document attached to periodic invoices.

Select 'Yes' - The system will use the DocBuilder supplementary detail document.

Select 'No' - The system will use the Legacy supplementary detail document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						true);
				});
			}
		}

		public BooleanRegistryItem UseNewDocBuilderRatingAndQuotationDocuments
		{
			get
			{
				return GetItem("UseNewDocBuilderRatingAndQuotationDocuments", delegate
				{
					return new BooleanRegistryItem(
						"UseNewDocBuilderRatingAndQuotationDocuments",
						Categories.Documents_DocBuilder,
						ResString.GetMultilingualString("14b9a127-44d0-48d6-829a-c3f934d973fd", "Use DocBuilder Rating and Quotation Documents"),
						ResString.GetMultilingualString("aeead382-1987-42c0-b6f0-cbd3629df4c4", "When printing Rating and Quotation Documents, use the new style DocBuilder based document that allow the user to include or exclude whatever sections they like."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false);
				});
			}
		}

		public DocBuilderThemeRegistryItem DocBuilderTheme
		{
			get
			{
				return GetItem("DocBuilderTheme", delegate
				{
					var themeRegistry = new DocBuilderThemeRegistry();
					return new DocBuilderThemeRegistryItem(
						"DocBuilderTheme",
						Categories.Documents_DocBuilder,
						ResString.GetMultilingualString("c0fea8a4-c921-4383-9018-b0c82be88f1b", "DocBuilder Theme"),
						ResString.GetMultilingualString("75ca9c1e-9678-4723-b4fe-3aa8931f551d", "Allows you to change the colors used for DocBuilder documents. You can either chose from the pre-defined themes, or create your own."),
						themeRegistry);
				});
			}
		}

		public CodeDescriptionBoolRegistryItem SuppressPrintingOfFlightDate
		{
			get
			{
				RegistryItemImplWithDynamicDefaultValue.DefaultValueGetter valueGetter = (companyPK, branchPK, departmentPK) =>
																						 RegistrySuppressionHelper.GetDefaultFields(branchPK, new List<string> { Core.Constants.CountryCodes.UnitedStates, Core.Constants.CountryCodes.Australia });

				return GetItem("SuppressPrintingOfFlightDate",
								 () => new CodeDescriptionBoolWithSelectionValidationRegistryItem("SuppressPrintingOfFlightDate",
																		 Categories.Documents,
																		 ResString.GetMultilingualString("3c6a58bd-38e1-4cf4-8cac-e43ecd886f2d", "Suppress Export Flight Details"),
																		 ResString.GetMultilingualString("505cdc0c-2961-4805-9203-d58fc5b2e992",
"Use this registry to configure the details to be suppressed on export documents (those issued to the Consignor) until at least one of the Consol ETD or Shipment Actual Pickup dates are in the past.\r\n" +
"For the U.S., flight details configured in this Registry will be suppressed for Consolidations with passenger flights whose ATD (Actual Time of Departure) of the final routing leg is not in the past."),
																		 RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
																		 DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
																		 new CodeDescriptionBoolRegistryEditorInfo(ResString.GetMultilingualString("b4780b98-4b35-4dc5-b5c9-4dd489b26050", "Suppress"), true, true),
																		 valueGetter));
			}
		}

		#region Delivery Order Sub-Category

		public DeliveryOrderCollectionRegistryItem DeliveryOrderTermsAndConditions
		{
			get
			{
				return GetItem("DeliveryOrderTermsAndConditions", delegate
				{
					return new DeliveryOrderCollectionRegistryItem(
						"DeliveryOrderTermsAndConditions",
						Categories.Documents_LinerAgency_DeliveryOrder,
						ResString.GetMultilingualString("2524a53a-66ab-4045-ab60-7768152cb67b", "Delivery Order Terms & Conditions by Principal"),
						ResString.GetMultilingualString("e44bc92f-7d19-4935-93e4-93959490aa5f", "The following Terms & Conditions settings will be used for Delivery Order documents related to the specified principal."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		#endregion

		#region Carrier Booking Request Sub-Category

		public DocumentOpenCloseTextRegistryItem CarrierBookingRequestOpeningText
		{
			get
			{
				return GetItem("CarrierBookingRequestOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"CarrierBookingRequestOpeningText",
						Categories.Documents_Forwarding_Consol_CarrierBookingRequest,
						ResString.GetMultilingualString("aabd4372-137b-4872-beaa-ecd619198535", "Opening Text"),
						ResString.GetMultilingualString("ca0e6606-66a9-48e0-8f51-7fea92ac35fa", "The opening text for the Carrier Booking Request document."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem CarrierBookingRequestClosingText
		{
			get
			{
				return GetItem("CarrierBookingRequestClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"CarrierBookingRequestClosingText",
						Categories.Documents_Forwarding_Consol_CarrierBookingRequest,
						ResString.GetMultilingualString("9bb7341c-4d22-4b0f-acb1-8b271810b1e2", "Closing Text"),
						ResString.GetMultilingualString("ab3ed619-b8dd-41c3-8f8d-75a57779384c", "The closing text for the Carrier Booking Request document."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		#endregion

		#region Delivery Information

		public DocumentOpenCloseTextRegistryItem DeliveryInformationOpeningText
		{
			get
			{
				return GetItem("DeliveryInformationOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"DeliveryInformationOpeningText",
						Categories.Documents_Forwarding_Shipment_DeliveryInformation,
						ResString.GetMultilingualString("aabd4372-137b-4872-beaa-ecd619198535", "Opening Text"),
						ResString.GetMultilingualString("60acd5a2-1ab4-4f3b-8ef7-94a6b140c0ba", "The opening text for the Delivery Information document."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem DeliveryInformationClosingText
		{
			get
			{
				return GetItem("DeliveryInformationClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"DeliveryInformationClosingText",
						Categories.Documents_Forwarding_Shipment_DeliveryInformation,
						ResString.GetMultilingualString("9bb7341c-4d22-4b0f-acb1-8b271810b1e2", "Closing Text"),
						ResString.GetMultilingualString("c60556b5-580c-4eaf-bb60-ea2e3fd07427", "The closing text for the Delivery Information document."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		#endregion

		#region Service Sub-Category

		public DocumentOpenCloseTextRegistryItem RequestForServiceOpeningText
		{
			get
			{
				return GetItem("RequestForServiceOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"RequestForServiceOpeningText",
						Categories.Documents_Service_RequestForService,
						ResString.GetMultilingualString("aabd4372-137b-4872-beaa-ecd619198535", "Opening Text"),
						ResString.GetMultilingualString("9f8ef2ae-39b0-4820-91b5-d2a075bc4084", "The opening text for the Request for Service document."),
						RegistryStorageFlags.All,
						RegistryOptions.PreserveTestValue);
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem RequestForServiceClosingText
		{
			get
			{
				return GetItem("RequestForServiceClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"RequestForServiceClosingText",
						Categories.Documents_Service_RequestForService,
						ResString.GetMultilingualString("9bb7341c-4d22-4b0f-acb1-8b271810b1e2", "Closing Text"),
						ResString.GetMultilingualString("f8c9f896-d7c4-4826-b7b6-fd557d0895b7", "The closing text for the Request for Service document."),
						RegistryStorageFlags.All,
						RegistryOptions.PreserveTestValue);
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem AuthorisationForServiceOpeningText
		{
			get
			{
				return GetItem("AuthorisationForServiceOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"AuthorisationForServiceOpeningText",
						Categories.Documents_Service_AuthorisationForService,
						ResString.GetMultilingualString("aabd4372-137b-4872-beaa-ecd619198535", "Opening Text"),
						ResString.GetMultilingualString("5bd277f9-0c3d-4876-9cfe-ce868a649449", "The opening text for the Authorization for Service document."),
						RegistryStorageFlags.All,
						RegistryOptions.PreserveTestValue);
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem AuthorisationForServiceClosingText
		{
			get
			{
				return GetItem("AuthorisationForServiceClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"AuthorisationForServiceClosingText",
						Categories.Documents_Service_AuthorisationForService,
						ResString.GetMultilingualString("9bb7341c-4d22-4b0f-acb1-8b271810b1e2", "Closing Text"),
						ResString.GetMultilingualString("1ea2415f-359a-4f39-816c-17c4dadc8014", "The closing text for the Authorization for Service document."),
						RegistryStorageFlags.All,
						RegistryOptions.PreserveTestValue);
				});
			}
		}
		#endregion

		#region Branding Sub-Category

		public BooleanRegistryItem EnableClientBranding
		{
			get
			{
				return GetItem("EnableClientBranding", delegate
				{
					return new BooleanRegistryItem(
						"EnableClientBranding",
						Categories.Documents_Branding,
						ResString.GetMultilingualString("fcd451cd-6c22-4c52-9404-001ad8bf9f63", "Enable Client Branding"),
						ResString.GetMultilingualString("c87a1b1e-110f-4f74-a8fc-06e4794c326b", "Select \"Yes\" to enable Client Branding."),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public BooleanRegistryItem EnableAgentBranding
		{
			get
			{
				return GetItem("EnableAgentBranding", delegate
				{
					return new BooleanRegistryItem(
						"EnableAgentBranding",
						Categories.Documents_Branding,
						ResString.GetMultilingualString("ce568b4b-e458-410e-ab7d-0ebff1dbb208", "Enable Agent Branding"),
						ResString.GetMultilingualString("d10256cb-54bb-4ea6-9a52-1a0b80d2edfd", "Select \"Yes\" to enable Agent Branding."),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public ClientTariffAndLevelCollectionRegistryItem ClientTariffAndLevels
		{
			get
			{
				return GetItem("ClientTariffAndLevels", delegate
				{
					return new ClientTariffAndLevelCollectionRegistryItem(
						"ClientTariffAndLevels",
						Categories.Documents_Branding,
						ResString.GetMultilingualString("f2b8f75f-98f6-49ff-bdc6-731dd743b04a", "Client Branding"),
						ResString.GetMultilingualString("57502a99-deb3-4315-b0ac-f11fc964ed00", "The following branding settings will be used for all clients who are using the specified company tariff level."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue);
				});
			}
		}

		public AgentDocumentBrandCollectionRegistryItem AgentDocumentBrand
		{
			get
			{
				return GetItem("AgentDocumentBrand", delegate
				{
					return new AgentDocumentBrandCollectionRegistryItem(
						"AgentDocumentBrand",
						Categories.Documents_Branding,
						ResString.GetMultilingualString("582b8d20-074d-4170-8618-43fc9a417b7e", "Agent Branding"),
						ResString.GetMultilingualString("194dc3bf-f956-4451-ba2f-cca69b30c5f6", "The following branding settings will be used for all clients who are using the specified agent category."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue);
				});
			}
		}

		public PrincipalBrandingCollectionRegistryItem PrincipalDocumentBrand
		{
			get
			{
				return GetItem("PrincipalDocumentBrand", delegate
				{
					return new PrincipalBrandingCollectionRegistryItem(
						"PrincipalDocumentBrand",
						Categories.Documents_Branding,
						ResString.GetMultilingualString("03773fa7-fd2c-4819-a341-10c15ecdccac", "Principal Branding"),
						ResString.GetMultilingualString("fd3a61e2-e7ee-4396-8214-683586fcced8", "The following branding settings will be used for documents relating to the specified principal."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue);
				});
			}
		}

		public LocalTransportCompanyBrandingCollectionRegistryItem LocalTransportCompanyBrand
		{
			get
			{
				return GetItem("LocalTransportCompanyBrand", delegate
				{
					return new LocalTransportCompanyBrandingCollectionRegistryItem(
						"LocalTransportCompanyBrand",
						Categories.Documents_Branding,
						ResString.GetMultilingualString("2e739615-11e5-497e-a498-a9d52b6cb774", "Local Transport Company Label Branding"),
						ResString.GetMultilingualString("626481c1-b974-4b61-b791-cf32a9f6d4b3", "The following branding settings will be used for transport company labels."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		public HybridDocumentBrandCollectionRegistryItem HBLAgentBrandingImage
		{
			get
			{
				return GetItem("HBLAgentBrandingImage", delegate
				{
					return new HybridDocumentBrandCollectionRegistryItem(
						"HBLAgentBrandingImage",
						Categories.Documents_Branding,
						ResString.GetMultilingualString("7ca48a55-44db-4032-96e8-636c9133a136", "HBL Agent Branding Image"),
						ResString.GetMultilingualString("9e278e58-4af0-46b9-a57e-757fe29b313b", "Specify an image to be used for HBL branding for all clients who are using the specified agent category."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
				});
			}
		}

		public HybridDocumentBrandCollectionRegistryItem HAWBAgentBrandingImage
		{
			get
			{
				return GetItem("HAWBAgentBrandingImage", delegate
				{
					return new HybridDocumentBrandCollectionRegistryItem(
						"HAWBAgentBrandingImage",
						Categories.Documents_Branding,
						ResString.GetMultilingualString("7b929ada-ea21-4d38-8a82-e69548663bbe", "HAWB Agent Branding Image"),
						ResString.GetMultilingualString("31dfce74-70a3-4b66-952c-f3e8ee15372c", "Specify an image to be used for HAWB branding for all clients who are using the specified agent category."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
				});
			}
		}

		public StringRegistryItem HBLAndHAWBBrandingOption
		{
			get
			{
				return GetItem("HBLAndHAWBBrandingOption", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"HBLAndHAWBBrandingOption",
						Categories.Documents_Branding,
						ResString.GetMultilingualString("2a048adc-e1ec-4ca9-b55d-407ffdffc78d", "HBL and HAWB Branding Option"),
						ResString.GetMultilingualString("f3da78b1-c88c-47ca-a98b-7cbfb7e66205", "Select Client-branded to brand HBL and HAWBs to client Tariff level. Or select Agent-branded to brand HBL and HAWBs to Forwarder's Agent category."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						HBLAndHAWBBrandingOptionEditorInfo.AgentBranded);

					result.EditorInfo = new HBLAndHAWBBrandingOptionEditorInfo();
					return result;
				});
			}
		}

		#endregion

		#region Forwarding Sub-Category

		#region Customs Sub-Category

		public MultilingualStringRegistryItem CustomsDelayAlertAlertText
		{
			get
			{
				return GetItem("CustomsDelayAlertAlertText", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem(
						"CustomsDelayAlertAlertText",
						Categories.Documents_Forwarding_Customs_DelayAlert,
						ResString.GetMultilingualString("6306a1fe-d057-4bbf-9f4f-e204126c97a6", "Alert Text"),
						ResString.GetMultilingualString("dfba1bb6-8bf9-4188-945a-0586d97b65c0", "The text that will be displayed in the header of the document as an alert."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						ResString.GetMultilingualString("e969aaa7-da2b-477d-8788-b24403da9fb7", "PLEASE NOTE, YOUR SHIPMENT HAS BEEN DELAYED, PLEASE SEE UPDATED SHIPMENT DETAILS BELOW"));

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public DocumentOpenCloseText EFTRequest
		{
			get { return new DocumentOpenCloseText(EFTRequestOpeningTextRaw.Value, EFTRequestClosingTextRaw.Value); }
		}

		internal DocumentOpenCloseTextRegistryItem EFTRequestOpeningTextRaw
		{
			get
			{
				return GetItem("EFTRequestOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"EFTRequestOpeningText",
						Categories.Documents_Forwarding_Customs_EFTRequest,
						ResString.GetMultilingualString("aabd4372-137b-4872-beaa-ecd619198535", "Opening Text"),
						ResString.GetMultilingualString("ca2fad2a-d00f-400a-953e-61e0b32537d2", "This is the opening text for the EFT Request document."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
				});
			}
		}

		internal DocumentOpenCloseTextRegistryItem EFTRequestClosingTextRaw
		{
			get
			{
				return GetItem("EFTRequestClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"EFTRequestClosingText",
						Categories.Documents_Forwarding_Customs_EFTRequest,
						ResString.GetMultilingualString("9bb7341c-4d22-4b0f-acb1-8b271810b1e2", "Closing Text"),
						ResString.GetMultilingualString("ab021499-eec2-4c45-a7ea-f64c2df3f66c", "This is the closing text for the EFT Request document."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
				});
			}
		}

		#endregion

		#region Shipment Sub-Category

		public CodePairRegistryItem HBLChargesDefaultDisplay
		{
			get
			{
				return GetItem("HBLChargesDisplay", delegate
				{
					return new CodePairRegistryItem(
						"HBLChargesDisplay",
						Categories.Documents_Forwarding_Shipment_BillofLading,
						ResString.GetMultilingualString("9b3ce309-52a8-4e9d-ab33-1e9bd5574132", "HBL Charges Default Display"),
						ResString.GetMultilingualString("a24a51b0-3e30-b1a5-45ee-97fd1db16aa0", "The default type of charges to display on House Bills of Lading"),
						HBLChargesDefaultDisplayTypesPairListProvider,
						RegistryStorageFlags.Branch | RegistryStorageFlags.Company | RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						HBLChargesDisplayTypes.CollectCharges);
				});
			}
		}

		public static class HBLChargesDisplayTypes
		{
			public const string NoCharges = "NON";
			public const string CollectCharges = "SHW";
			public const string PrepaidCharges = "PPD";
			public const string AsAgreed = "AGR";
			public const string PrepaidAndCollectCharges = "ALL";
			public const string OriginalAsAgreedCopyWithCollectCharges = "CCL";
			public const string OriginalAsAgreedCopyWithPrepaidCharges = "CPP";
			public const string OriginalAsAgreedCopyWithPrepaidAndCollectCharges = "CAL";
		}

		ICodeDescriptionPairListProvider HBLChargesDefaultDisplayTypesPairListProvider
		{
			get
			{
				if (fHBLChargesDefaultDisplayTypesPairListProvider == null)
				{
					fHBLChargesDefaultDisplayTypesPairListProvider = new CodeDescriptionPairListProvider(() =>
					{
						var list = new CodeDescriptionPairList();
						list.AddPair(HBLChargesDisplayTypes.NoCharges, ResString.GetMultilingualString("1b680a9b-bc58-492d-ae81-96ef73cba1a3", "No Charges showing"));
						list.AddPair(HBLChargesDisplayTypes.CollectCharges, ResString.GetMultilingualString("54c417b6-786a-4b09-9b6b-e4cf9753220c", "Show Collect Charges"));
						list.AddPair(HBLChargesDisplayTypes.PrepaidCharges, ResString.GetMultilingualString("02faf6f9-0a12-404c-9aeb-2f3c9f1f24a5", "Show Prepaid Charges"));
						list.AddPair(HBLChargesDisplayTypes.AsAgreed, ResString.GetMultilingualString("7e63345a-3225-4d23-8455-f8a3c4768e76", "Show \"As Agreed\" in the charges section"));
						list.AddPair(HBLChargesDisplayTypes.PrepaidAndCollectCharges, ResString.GetMultilingualString("220f8295-f23f-4079-a6c8-860f1f4ab67c", "Show Prepaid & Collect Charges"));
						list.AddPair(HBLChargesDisplayTypes.OriginalAsAgreedCopyWithCollectCharges, ResString.GetMultilingualString("5226e674-9893-490f-8d2e-cea13c6b33d9", "Show Original \"As Agreed\" & Copy with Collect Charges"));
						list.AddPair(HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidCharges, ResString.GetMultilingualString("09a35034-b1d4-4dc6-8cbc-7e414595cadc", "Show Original \"As Agreed\" & Copy with Prepaid Charges"));
						list.AddPair(HBLChargesDisplayTypes.OriginalAsAgreedCopyWithPrepaidAndCollectCharges, ResString.GetMultilingualString("8f33514c-c04a-46cd-9e6b-92b46aaab23a", "Show Original \"As Agreed\" & Copy with Prepaid & Collect Charges"));
						return list;
					});
				}

				return fHBLChargesDefaultDisplayTypesPairListProvider;
			}
		}
		ICodeDescriptionPairListProvider fHBLChargesDefaultDisplayTypesPairListProvider;

		public CodeDescriptionPairList HBLChargesDefaultDisplayTypesPairList
		{
			get
			{
				if (fHBLChargesDefaultDisplayTypesPairList == null)
				{
					fHBLChargesDefaultDisplayTypesPairList = HBLChargesDefaultDisplayTypesPairListProvider.CodeDescriptionPairList;
				}
				return fHBLChargesDefaultDisplayTypesPairList;
			}
		}
		CodeDescriptionPairList fHBLChargesDefaultDisplayTypesPairList;

		public GuidArrayRegistryItem BOLLumpSumDisplayCountries
		{
			get
			{
				return GetItem("BOLLumpSumDisplayCountries", delegate
				{
					GuidArrayRegistryItem result = new GuidArrayRegistryItem(
						"BOLLumpSumDisplayCountries",
						Categories.Documents_Forwarding_Shipment_BillofLading,
						ResString.GetMultilingualString("32cd98d7-309f-427e-a9d1-5eae4dbfeaf3", "Print Charges as Lump Sum"),
						ResString.GetMultilingualString("f2dde26d-45c5-46a4-aad8-b33577d70d36", "Charges will be printed as a lump sum for any shipments where the destination is in one of the below specified countries/regions."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						Array.Empty<Guid>());

					result.EditorInfo = new CountryListRegistryEditorInfo();
					return result;
				});
			}
		}

		public BooleanRegistryItem BOLPrintTotalCharges
		{
			get
			{
				return GetItem("BOLPrintTotalCharges", delegate
				{
					return new BooleanRegistryItem(
						"BOLPrintTotalCharges",
						Categories.Documents_Forwarding_Shipment_BillofLading,
						ResString.GetMultilingualString("39b781da-4d40-40da-967f-f1f0ae3954c5", "Print Total Charges"),
						ResString.GetMultilingualString("c74b2780-6b64-47bc-8cd9-b3e5024c9627", "Override this registry to include the total of charges displayed on a house bill.\r\n\r\nThe total will not be displayed if charges are \"As Agreed\" or \"Print Charges as Lump Sum\" is applicable for the HBL destination country/region."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false);
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem DangerousGoodsStatement
		{
			get
			{
				return GetItem("DangerousGoodsStatement", delegate
				{
					DocumentOpenCloseTextRegistryItem result = new DocumentOpenCloseTextRegistryItem(
						"DangerousGoodsStatement",
						Categories.Documents_Forwarding_Shipment,
						ResString.GetMultilingualString("0cc33436-5a76-4329-913a-2bf412c6becc", "Dangerous Goods Statement"),
						ResString.GetMultilingualString("ddba17e1-5dc1-452f-8ebb-878aa4078185", "This Statement will be printed on Dangerous Goods related documents."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						ResString.GetMultilingualString("e9592f91-961e-41df-afa0-5244da0e3daf", "This is to certify that the above named materials are properly classified, described, packaged, marked and labeled, and are in proper condition for transport according to applicable domestic and international regulations."));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem ContainerDetentionReminderOpeningText
		{
			get
			{
				return GetItem("ContainerDetentionReminderOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"ContainerDetentionReminderOpeningText",
						Categories.Documents_Forwarding_Shipment_ContainerDetentionReminder,
						ResString.GetMultilingualString("aabd4372-137b-4872-beaa-ecd619198535", "Opening Text"),
						ResString.GetMultilingualString("90077fbe-729a-4d04-a7fa-a4ce3f42c263", "This is the opening text for the Container Detention Reminder document."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						ResString.GetMultilingualString("13540bf3-3ecc-4dc1-8335-aa41949c7755", "We have no record of the return of the containers noted below, please note that Detention Charges will be applied for all containers not returned by their due date."));
				});
			}
		}

		public ImageRegistryItem ForwardersCertificateOfReceiptLogo
		{
			get
			{
				return GetItem("ForwardersCertificateOfReceiptLogo", delegate
				{
					return new ImageRegistryItem("ForwardersCertificateOfReceiptLogo", Categories.Documents_Forwarding_Shipment_ForwardersCertificateofReceipt,
						ResString.GetMultilingualString("1f8fdbd3-eb58-4d12-91c0-ba8702ba32b8", "Logo"), ResString.GetMultilingualString("1d68f46b-e350-4d28-a774-851bad6f98b3", "Logo for the Forwarders Certificate of Receipt."), RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
				});
			}
		}

		public MultilingualStringRegistryItem ForwardersCertificateOfReceiptFCRClause
		{
			get
			{
				return GetItem("ForwardersCertificateOfReceiptFCRClause", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem("ForwardersCertificateOfReceiptFCRClause", Categories.Documents_Forwarding_Shipment_ForwardersCertificateofReceipt,
						ResString.GetMultilingualString("41285970-205c-4226-8eec-a3ffeef30ae6", "FCR Clause"), ResString.GetMultilingualString("20cae777-a450-4197-8011-9a7c23dc7fe8", "FCR Clause for the Forwarders Certificate of Receipt."), RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, ResString.GetMultilingualString("6496fbf1-0805-4110-93f1-2e0b4e56b544", "The Forwarder certifies having received and assumed control of the above mentioned goods in external apparent good order and condition at the disposal of the Consignee.\r\nThis Forwarder Certificate of Receipt is not a document of title as far as the goods are concerned.\r\nThe production or surrendering of this Forwarder Certificate of Receipt will not entitle its holder to take delivery of the goods.\r\nOnce the goods are received by the Forwarder from the shipper, the right of disposing the goods rests with the Consignee.\r\nThe goods and instructions are accepted and dealt with subject to the standard terms and conditions, available on request, of the forwarder issuing this Forwarder Certificate of Receipt."));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem RequestForCollectChargesOpeningText
		{
			get
			{
				return GetItem("RequestForCollectChargesOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"RequestForCollectChargesOpeningText",
						Categories.Documents_Forwarding_Shipment_RequestforCollectCharges,
						ResString.GetMultilingualString("aabd4372-137b-4872-beaa-ecd619198535", "Opening Text"),
						ResString.GetMultilingualString("c980f58a-0174-4e52-8ac8-43fb6ad75248", "Request for Collect Charges Opening Text."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						ResString.GetMultilingualString("b67908a5-ca1f-4682-b998-98a69080ded4", "Please provide details of all charges to collect for the shipment noted below."));
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem RequestForCollectChargesClosingText
		{
			get
			{
				return GetItem("RequestForCollectChargesClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"RequestForCollectChargesClosingText",
						Categories.Documents_Forwarding_Shipment_RequestforCollectCharges,
						ResString.GetMultilingualString("9bb7341c-4d22-4b0f-acb1-8b271810b1e2", "Closing Text"),
						ResString.GetMultilingualString("a55d4ce0-057b-4e4b-9eb1-8e62c0954303", "Request for Collect Charges Closing Text."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem RequestForProfitShareOpeningText_Shipment
		{
			get
			{
				return GetItem("RequestForProfitShareOpeningText_Shipment", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"RequestForProfitShareOpeningText_Shipment",
						Categories.Documents_Forwarding_Shipment_RequestForProfitShare,
						ResString.GetMultilingualString("aabd4372-137b-4872-beaa-ecd619198535", "Opening Text"),
						ResString.GetMultilingualString("05aef198-80c5-457d-a78b-5d2d914aea56", "The opening text for the Request for Profit Share."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
				});
			}
		}

		public MultilingualStringRegistryItem GuaranteeDeclarationText
		{
			get
			{
				return GetItem("GuaranteeDeclarationText", delegate
				{
					ResourceString defaultValue = ResString.GetMultilingualString("1d3f335a-b710-4909-8c1a-307c02dfa7c7", @"In consideration of your complying with our above request we hereby agree as follows: -
1. To indemnify you and hold you harmless in respect of any liability, loss or damage of whatsoever nature which you may sustain by reason of delivering the goods to us (the Consignee) in accordance with our request.
2. To pay you on demand the amount of any loss or damage which the master and/or agents of the vessel or any other of your servants or agents whatsoever may incur as a result of delivering the goods as aforesaid.
3. In the event of any proceedings being commenced against you or any of your servant or agents in connection with the delivery of the goods as aforesaid, to provide you or them from time to time on demand with sufficient to defend the same.
4. If called upon to do so at any time while the goods are in our custody, possession or control to redeliver the same to you.
5. To produce and deliver to you the Bills of Lading for the above goods duly endorsed, as soon as these documents shall have arrived.
6. Liability of each and every person under this indemnity shall be joint and several and shall not be conditional upon your proceeding first against any person, whether or not such person is party to or liable under this indemnity.
7. This indemnity shall be construed in accordance with Singapore law and each and every person liable under this indemnity shall at your request summit to the jurisdiction of the High Court of Singapore and the liability there under shall be determined accordingly.");

					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem(
						"GuaranteeDeclarationText",
						Categories.Documents_Forwarding_Shipment_LetterofGuarantee,
						ResString.GetMultilingualString("1cb2d089-00f0-49ba-a551-6fafca6350a9", "Guarantee Declaration Text"),
						ResString.GetMultilingualString("a5bf1d9f-5e0b-4291-9ebd-7b01119822c4", "This Guarantee Declaration will be used in the Singapore Document Letter of Guarantee as an agreement that the Consignee will cover for the Forwarder and pay any related charges."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						defaultValue);

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					result.CountryFilterPKs = CountryFilterPKs.Singapore;
					return result;
				});
			}
		}

		public BooleanRegistryItem DisplayContainerDetailsOnShipment
		{
			get
			{
				return GetItem("DisplayContainerDetailsOnShipment", delegate
				{
					return new BooleanRegistryItem(
						"DisplayContainerDetailsOnShipment",
						Categories.Documents_Forwarding_Shipment_InterimReceipt,
						ResString.GetMultilingualString("97357386-9ec0-4bc7-9c78-bffd0db40c09", "Display Container details as part of Marks & Numbers"),
						ResString.GetMultilingualString("6d1580be-2c13-4e17-9ac0-b3f2c54e8ae0", "This registry item controls whether Container details are displayed as part of Marks & Numbers on Shipments."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem ShippingOrderClosingText
		{
			get
			{
				return GetItem("ShippingOrderClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"ShippingOrderClosingText",
						Categories.Documents_Forwarding_Shipment_ShippingOrder,
						ResString.GetMultilingualString("9bb7341c-4d22-4b0f-acb1-8b271810b1e2", "Closing Text"),
						ResString.GetMultilingualString("8f536240-e3c3-4edc-8a54-3a6ddef38985", "The Closing text that will be displayed at the bottom of every Shipping Order Document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem DelayAlertOpeningText
		{
			get
			{
				return GetItem("DelayAlertOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"DelayAlertOpeningText",
						Categories.Documents_Forwarding_Shipment_DelayAlert,
						ResString.GetMultilingualString("aabd4372-137b-4872-beaa-ecd619198535", "Opening Text"),
						ResString.GetMultilingualString("90c2f6a3-f39c-4e61-a98c-cd2deb9586cc", "This is the Opening Text for Delay Alert documents."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem DelayAlertClosingText
		{
			get
			{
				return GetItem("DelayAlertClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"DelayAlertClosingText",
						Categories.Documents_Forwarding_Shipment_DelayAlert,
						ResString.GetMultilingualString("9bb7341c-4d22-4b0f-acb1-8b271810b1e2", "Closing Text"),
						ResString.GetMultilingualString("645b8764-c937-4ac1-a405-8c23e5b000b1", "This is the Closing Text for Delay Alert documents."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
				});
			}
		}

		public MultilingualStringRegistryItem ShipmentDelayAlertAlertText
		{
			get
			{
				return GetItem("ShipmentDelayAlertAlertText", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem(
						"ShipmentDelayAlertAlertText",
						Categories.Documents_Forwarding_Shipment_DelayAlert,
						ResString.GetMultilingualString("6306a1fe-d057-4bbf-9f4f-e204126c97a6", "Alert Text"),
						ResString.GetMultilingualString("dfba1bb6-8bf9-4188-945a-0586d97b65c0", "The text that will be displayed in the header of the document as an alert."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						ResString.GetMultilingualString("e969aaa7-da2b-477d-8788-b24403da9fb7", "PLEASE NOTE, YOUR SHIPMENT HAS BEEN DELAYED, PLEASE SEE UPDATED SHIPMENT DETAILS BELOW"));

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		ResourceString GetHAWBTermsDefault()
		{
			return ResString.GetMultilingualString("7770afd3-96f0-4329-b559-cdfc89651802", "It is agreed that the goods described herein are accepted in apparent good order and condition (except as noted) for carriage SUBJECT TO CONDITIONS OF CONTRACT ON THE REVERSE SIDE HEREOF. ALL GOODS MAY BE CARRIED BY ANY OTHER MEANS INCLUDING ROAD OR ANY OTHER CARRIER UNLESS SPECIFIC CONTRARY INSTRUCTIONS ARE GIVEN HEREON BY THE SHIPPER, AND THE SHIPPER AGREES THAT THE SHIPMENT MAY BE CARRIED VIA INTERMEDIATE STOPPING PLACES WHICH THE CARRIER DEEMS APPROPRIATE. THE SHIPPERS ATTENTION IS DRAWN TO THE NOTICE CONCERNING CARRIER'S LIMITATIONS OF LIABILITY.\r\nShipper may increase such limitation of liability by declaring a higher value for carriage and paying supplemental charge if required.\r\n* The Terms and Conditions as noted on the reverse side of this Transport Document are not applicable for OCEAN shipments. These shipments will be subject to the Terms and Conditions of the appointed carrier, including Limitation of Liability.");
		}

		public MultilingualStringRegistryItem DomesticHAWBTerms
		{
			get
			{
				return GetItem("DomesticHAWBTerms", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem(
						"DomesticHAWBTerms",
						Categories.Documents_Forwarding_Shipment,
						ResString.GetMultilingualString("8018e573-77bf-4d2c-8bc8-bde969462bad", "Domestic House Bill Terms (Quick Booking)"),
						ResString.GetMultilingualString("1120e87f-0470-46b3-97ce-5e2e1575bc06", "This Statement will be printed on domestic laser HAWB document generated from the Quick Booking system.."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached | RegistryOptions.PreserveTestValue,
						GetHAWBTermsDefault());
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public MultilingualStringRegistryItem BookingInternationalHAWBTerms
		{
			get
			{
				return GetItem("BookingInternationalHAWBTerms", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem(
						"BookingInternationalHAWBTerms",
						Categories.Documents_Forwarding_Shipment,
						ResString.GetMultilingualString("791ccc1d-cd63-48af-aed1-b89e0ab4a92c", "International House Bill Terms (Quick Booking)"),
						ResString.GetMultilingualString("c4923141-192d-4c18-bdcd-226a97ef9002", "This Statement will be printed on international laser HAWB document generated from the Quick Booking system."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached,
						GetHAWBTermsDefault());
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		#region POD

		public BooleanRegistryItem ShowMilestonesOnPODDocument
		{
			get
			{
				return GetItem("ShowMilestonesOnPODDocument", delegate
				{
					return new BooleanRegistryItem(
						"ShowMilestonesOnPODDocument",
						Categories.Documents_Forwarding_Shipment_ProofofDelivery,
						ResString.GetMultilingualString("1ba3eb11-9179-4c29-b339-d1f48bc334a3", "Show Milestones On POD Document"),
						ResString.GetMultilingualString("2c2d959b-e2f3-468b-8a6b-a3b22dbba670", "Display Milestones on POD Document"),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false);
				});
			}
		}

		public BooleanRegistryItem IncludeEstimatedMilesonesOnPODDocument
		{
			get
			{
				return GetItem("IncludeEstimatedMilesonesOnPODDocument", delegate
				{
					return new BooleanRegistryItem(
						"IncludeEstimatedMilesonesOnPODDocument",
						Categories.Documents_Forwarding_Shipment_ProofofDelivery,
						ResString.GetMultilingualString("5f385d74-5e1d-4a2d-a680-a4db3f577059", "Include Estimated Milestones on POD Document"),
						ResString.GetMultilingualString("55f1139f-3696-4def-b8af-6f41695c4e72", "Show Estimated Milestones on POD Document if there is no Actual Date"),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false);
				});
			}
		}

		#endregion

		public DocumentOpenCloseText LetterOfIndemnity
		{
			get { return new DocumentOpenCloseText(LetterOfIndemnityOpeningText.Value, LetterOfIndemnityClosingText.Value); }
		}

		internal DocumentOpenCloseTextRegistryItem LetterOfIndemnityOpeningText
		{
			get
			{
				return GetItem("LetterOfIndemnityOpeningText", delegate
				{
					DocumentOpenCloseTextRegistryItem result = new DocumentOpenCloseTextRegistryItem(
						"LetterOfIndemnityOpeningText",
						Categories.Documents_Forwarding_Shipment_LetterofIndemnity,
						ResString.GetMultilingualString("aabd4372-137b-4872-beaa-ecd619198535", "Opening Text"),
						ResString.GetMultilingualString("2d4919bb-924c-4c8e-8778-90e91646d075", "The opening text for the Letter of Indemnity document."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						ResString.GetMultilingualString("e2ab2b53-2579-455d-b647-1e59c9855d26", "Please alter the markings on the above Delivery Order in respect of the under mentioned cargo."));
					result.CountryFilterPKs = CountryFilterPKs.Singapore;
					return result;
				});
			}
		}

		internal DocumentOpenCloseTextRegistryItem LetterOfIndemnityClosingText
		{
			get
			{
				return GetItem("LetterOfIndemnityClosingText", delegate
				{
					DocumentOpenCloseTextRegistryItem result = new DocumentOpenCloseTextRegistryItem(
						"LetterOfIndemnityClosingText",
						Categories.Documents_Forwarding_Shipment_LetterofIndemnity,
						ResString.GetMultilingualString("9bb7341c-4d22-4b0f-acb1-8b271810b1e2", "Closing Text"),
						ResString.GetMultilingualString("45d96d1f-ced5-4db9-85dc-6d775d8239c9", "The closing text for the Letter of Indemnity document."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						ResString.GetMultilingualString("be2d5953-8dc4-4707-a9fe-f22fd9fea7f8", "We hereby undertake and agree to indemnify you against all consequences and/or liabilities of any kind whatsoever directly or indirectly arising from or relating to the said delivery, and immediately on demand on all payment made by you in respect of such consequences and/or liabilities including costs as between solicitor and client and all any sues demanded by you for the defense of any proceeding brought against you by reason of the delivery aforesaid."));
					result.CountryFilterPKs = CountryFilterPKs.Singapore;
					return result;
				});
			}
		}

		public MultilingualStringRegistryItem ContainerLiabilityStatementLiabilityWarningText
		{
			get
			{
				return GetItem("ContainerLiabilityStatementAdministrationFeeText", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem(
						"ContainerLiabilityStatementAdministrationFeeText",
						Categories.Documents_Forwarding_Shipment_ContainerLiabilityStatement,
						ResString.GetMultilingualString("9bf732cc-43d5-48af-bde7-1394843aee66", "Liability Warning Text"),
						ResString.GetMultilingualString("fbc0f79f-55e4-4162-a9b1-8b34697e4b99", "Text describing the Container Liability warning."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						ResString.GetMultilingualString("0cc10fd2-1161-477c-b7e8-b80a12b4d438", "By issuing the Delivery Orders to you for the above mentioned period of time, and by accepting them, you (as the owner or agent of the goods) therefore accept the Terms and Conditions of the Bill of Lading which require the return of the containers in a clean and undamaged condition within the stated free time to the container depot.\r\n\r\nFailure to comply with this agreement will result in all detention charges, cost of repairs or internal cleaning of the containers and an administration fee per container being debited to your company (see above charges)."));
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public MultilingualStringRegistryItem ContainerLiabilityAcceptanceText
		{
			get
			{
				return GetItem("ContainerLiabilityAcceptanceText", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem(
						"ContainerLiabilityAcceptanceText",
						Categories.Documents_Forwarding_Shipment_ContainerLiabilityStatement,
						ResString.GetMultilingualString("c99c98a2-98e0-47e0-aace-b7888d657976", "Liability Acceptance Text"),
						ResString.GetMultilingualString("8b7a0385-c218-47ac-96ef-a2760fa4f378", "Text describing the Container Liability Acceptance."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						ResString.GetMultilingualString("b8c64946-84d3-4e6a-bd88-356baab59ea6", "The signing of this agreement is your acceptance of the above terms."));

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public BooleanRegistryItem DisplayLogo
		{
			get
			{
				return GetItem("DisplayLogo", delegate
				{
					return new BooleanRegistryItem(
						"DisplayLogo",
						Categories.Documents_Forwarding_Shipment_WorkSheet,
						ResString.GetMultilingualString("0e3484be-22c4-48e2-b888-ab10a61c34a2", "Display Logo"),
						ResString.GetMultilingualString("be3b78c5-0a9b-42e3-b997-175a9f535ec4", "Display the logo on the Work sheet document that prints from Shipments and Customs Declarations."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem FOBAndienungClosingText
		{
			get
			{
				return GetItem("FOBAndienungClosingText", delegate
				{
					DocumentOpenCloseTextRegistryItem result = new DocumentOpenCloseTextRegistryItem(
						"FOBAndienungClosingText",
						Categories.Documents_Forwarding_Shipment_FOBAndienungVersicherungsanmeldung,
						ResString.GetMultilingualString("9bb7341c-4d22-4b0f-acb1-8b271810b1e2", "Closing Text"),
						ResString.GetMultilingualString("9d2de7cd-b53e-4fab-85df-e92e6dccec7a", "The Closing text that will be displayed at the bottom of every FOB - Andienung & Versicherungsanmeldung Documents."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default
					);
					result.CountryFilterPKs = CountryFilterPKs.Germany;
					return result;
				});
			}
		}

		public StringRegistryItem FOBAndienungInsuranceCoveredText
		{
			get
			{
				return GetItem("FOBAndienungInsuranceCoveredText", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"FOBAndienungInsuranceCoveredText",
						Categories.Documents_Forwarding_Shipment_FOBAndienungVersicherungsanmeldung,
						ResString.GetMultilingualString("542f53e9-ab6f-41a4-82ae-a8adbe3f6617", "Insurance Covered Text"),
						null,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"");
					result.CountryFilterPKs = CountryFilterPKs.Germany;
					return result;
				});
			}
		}

		public StringRegistryItem FOBAndienungInsuranceNotCoveredText
		{
			get
			{
				return GetItem("FOBAndienungInsuranceNotCoveredText", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"FOBAndienungInsuranceNotCoveredText",
						Categories.Documents_Forwarding_Shipment_FOBAndienungVersicherungsanmeldung,
						ResString.GetMultilingualString("220cf061-7934-4422-93b4-95ab02087c53", "Insurance Not Covered Text"),
						null,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"");
					result.CountryFilterPKs = CountryFilterPKs.Germany;
					return result;
				});
			}
		}

		public StringRegistryItem VerpflichtungsscheinAccountNumber
		{
			get
			{
				return GetItem("VerpflichtungsscheinAccountNumber", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"VerpflichtungsscheinAccountNumber",
						Categories.Documents_Forwarding_Shipment_Verpflichtungsschein,
						ResString.GetMultilingualString("8c959202-c289-4333-baba-560396138847", "Account Number for Verpflichtungsschein"),
						null,
						RegistryStorageFlags.System | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"");
					result.CountryFilterPKs = CountryFilterPKs.Germany;
					return result;
				});
			}
		}

		public StringRegistryItem IcelandAirArrivalNoticeFooter
		{
			get
			{
				return GetItem("IcelandAirArrivalNoticeFooter", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"IcelandAirArrivalNoticeFooter",
						Categories.Documents_Forwarding_Shipment_IcelandicArrivalNoticeFooter,
						ResString.GetMultilingualString("dece4960-90a2-4b7f-8503-840ea21be046", "Air Arrival Notice Document Footer"),
						null,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"");

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					result.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.IceLand;
					return result;
				});
			}
		}

		public StringRegistryItem IcelandSeaArrivalNoticeFooter
		{
			get
			{
				return GetItem("IcelandSeaArrivalNoticeFooter", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"IcelandSeaArrivalNoticeFooter",
						Categories.Documents_Forwarding_Shipment_IcelandicArrivalNoticeFooter,
						ResString.GetMultilingualString("8ebd940b-6dbc-445a-89fd-3cf60ba261bc", "Sea Arrival Notice Document Footer"),
						null,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"");

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					result.CountryFilterPKs = RegistryItemSet.CountryFilterPKs.IceLand;
					return result;
				});
			}
		}

		#endregion

		#region Consol Sub-Category

		ResourceString GetAirlineSecurityKnownStatementDefault()
		{
			return ResString.GetMultilingualString("de230c36-97c3-4c90-929e-15faa1557ef5", "Company ________________________________ is in compliance with its TSA approved security program and all applicable security directives. Our number assigned by TSA is ________________________________. All cargo tendered in conjunction with this certification was either 1) accepted from a known shipper or an unknown shipper in accordance with TSA requirements specified in the Indirect Air Carrier Standard Security Program or 2) accepted under transfer from another aircraft operator, foreign air carrier, or IAC operating under a TSA-approved or accepted security program. The individual whose name appears below certifies that he or she is an employee or authorized representative of _______________________________________ and understands that any fraudulent or false statement made in connection with this certification may subject this individual and _______________________ to both (1) civil penalties under 49 CFR 1540.103(b) and (2) fines and/or imprisonment of not more than 5 years under 18 U.S.C. 1001.");
		}

		ResourceString GetAirlineSecurityUnknownStatementDefault()
		{
			return ResString.GetMultilingualString("52760189-c760-4191-a8b0-22aed4c933fa", "Company ________________________________ is in compliance with its TSA approved security program and all applicable security directives. Our number assigned by TSA is ________________________________. This shipment contains cargo originating from an unknown shipper not exempted by TSA. This shipment must be transported ONLY on ALL-CARGO AIRCRAFT. The individual whose name appears below certifies that he or she is an employee or Authorized Representative of _______________________________________________ and understands that any fraudulent or false statement made in connection with this certification may subject this individual and _______________________________________________ to both civil penalties under 49 CFR Part 1540.103(b) and fines and/or imprisonment of not more than 5 years under 18 U.S.C. 1001.");
		}

		ResourceString GetAWBSecurityDeclarationStatementDefault()
		{
			return ResString.GetMultilingualString("c9ba91fa-4878-4285-a6eb-7f9fee5dcad2", "I, <LoginFullName>, an authorized representative of <CompanyName>, declare that the cargo on AWB <MasterBill> has been secured for carriage by air as a result of the procedure indicated above. I understand that a false declaration may lead to legal action being taken.");
		}

		public MultilingualStringRegistryItem AirlineSecurityStatementKnownShipper
		{
			get
			{
				return GetItem("AirlineSecurityStatementKnownShipper", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem(
						"AirlineSecurityStatementKnownShipper",
						Categories.Documents_Forwarding_Consol_AirlineSecurityStatements,
						ResString.GetMultilingualString("5b142bc4-5c50-461c-8503-e239be916ebe", "IAC Known Shipper Statement"),
						ResString.GetMultilingualString("a15bb992-ace9-460a-af74-0600d8665389", "This Statement will be printed on IAC Certification - Known Shipper document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached | RegistryOptions.PreserveTestValue,
						GetAirlineSecurityKnownStatementDefault());
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public MultilingualStringRegistryItem AirlineSecurityStatementUnknownShipper
		{
			get
			{
				return GetItem("AirlineSecurityStatementUnknownShipper", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem(
						"AirlineSecurityStatementUnknownShipper",
						Categories.Documents_Forwarding_Consol_AirlineSecurityStatements,
						ResString.GetMultilingualString("bdc88b52-ca13-4f25-b498-2f7bb42a7589", "IAC Unknown Shipper Statement"),
						ResString.GetMultilingualString("22f008a8-c456-482b-812d-cfe097b1cfa3", "This Statement will be printed on IAC Certification - Unknown Shipper document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached | RegistryOptions.PreserveTestValue,
						GetAirlineSecurityUnknownStatementDefault());
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public MultilingualStringRegistryItem AWBSecurityDeclarationStatement
		{
			get
			{
				return GetItem("AWBSecurityDeclarationStatement", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem(
						 "AWBSecurityDeclarationStatement",
						 Categories.Documents_Forwarding_Consol_AirlineSecurityStatements,
						 ResString.GetMultilingualString("c1728be4-7939-4f21-b366-6fe646d646fb", "AWB Security Declaration Statement"),
						 ResString.GetMultilingualString("e83e7789-5b27-4835-ade0-4b48bbbae4cd", "This Statement will be shown on the Air Export Security Declaration issued from the Consol."),
						 RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						 DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.NotCached,
						 GetAWBSecurityDeclarationStatementDefault());
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem ConsoleRateConfirmationClosingText
		{
			get
			{
				return GetItem("ConsoleRateConfirmationClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"ConsoleRateConfirmationClosingText",
						Categories.Documents_Forwarding_Consol_RateConfirmation,
						ResString.GetMultilingualString("9bb7341c-4d22-4b0f-acb1-8b271810b1e2", "Closing Text"),
						ResString.GetMultilingualString("a27592fa-7aa8-47b5-821b-a06bedb4586f", "The Closing text that will be displayed at the bottom of every Rate Confirmation Document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem ConsoleRateConfirmationOpeningText
		{
			get
			{
				return GetItem("ConsoleRateConfirmationOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"ConsoleRateConfirmationOpeningText",
						Categories.Documents_Forwarding_Consol_RateConfirmation,
						ResString.GetMultilingualString("aabd4372-137b-4872-beaa-ecd619198535", "Opening Text"),
						ResString.GetMultilingualString("a7d83be0-605c-46d1-8eae-fbc18a1f01f9", "The Opening text that will be displayed at the bottom of every Rate Confirmation Document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
				});
			}
		}

		public BooleanRegistryItem DisplayContainerDetailsOnConsol
		{
			get
			{
				return GetItem("DisplayContainerDetailsOnConsol", delegate
				{
					return new BooleanRegistryItem(
						"DisplayContainerDetailsOnConsol",
						Categories.Documents_Forwarding_Consol_ForwardingInstruction,
						ResString.GetMultilingualString("97357386-9ec0-4bc7-9c78-bffd0db40c09", "Display Container details as part of Marks & Numbers"),
						ResString.GetMultilingualString("afb4717c-1923-4d64-b358-23a2389be68c", "This registry item controls whether Container details are displayed as part of Marks & Numbers on Consols."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public BooleanRegistryItem DisplayPackLinesByContainerOnConsol
		{
			get
			{
				return GetItem("DisplayPackLinesByContainerOnConsol", delegate
				{
					return new BooleanRegistryItem(
						"DisplayPackLinesByContainerOnConsol",
						Categories.Documents_Forwarding_Consol_ForwardingInstruction,
						ResString.GetMultilingualString("79249ba7-d7b1-404e-93f9-2163b9f85aa1", "Display Pack Lines as part of Containers"),
						ResString.GetMultilingualString("f9c645f8-b325-41be-9bbb-f690c95bdfde", "This registry item controls whether packing lines details are displayed as part of Containers on Consols."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						false);
				});
			}
		}

		public CodePairRegistryItem ReleaseType
		{
			get
			{
				return GetItem("ConsolReleaseType", delegate
				{
					return new CodePairRegistryItem(
						"ConsolReleaseType",
						Categories.Documents_Forwarding_Consol_ForwardingInstruction,
						ResString.GetMultilingualString("37f35305-00c3-4d6a-aca9-f610e4ddffee", "Release Type"),
						ResString.GetMultilingualString("22be2e94-b904-41aa-a89a-e90d700aa9e2", "Default Release Type for Consol"),
						new CodeDescriptionPairListProvider(() => FreightDataRegistry.Instance.ReleaseTypes.Value.GetCodeDescriptionPairList()),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						"");
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem RequestForProfitShareOpeningText_Consol
		{
			get
			{
				return GetItem("RequestForProfitShareOpeningText_Consol", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"RequestForProfitShareOpeningText_Consol",
						Categories.Documents_Forwarding_Consol_RequestForProfitShare,
						ResString.GetMultilingualString("aabd4372-137b-4872-beaa-ecd619198535", "Opening Text"),
						ResString.GetMultilingualString("05aef198-80c5-457d-a78b-5d2d914aea56", "The opening text for the Request for Profit Share."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
				});
			}
		}

		public CodePairRegistryItem BOLChargesDefaultDisplay
		{
			get
			{
				return GetItem("BOLChargesDisplay", delegate
				{
					return new CodePairRegistryItem(
						"BOLChargesDisplay",
						Categories.Documents_Forwarding_Consol,
						(NoResString)"Bill Of Lading Charges Default Display",
						(NoResString)"The requested default type of charges display to print on Bill of Lading",
						HBLChargesDefaultDisplayTypesPairListProvider,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.IsOnlyForSupport,
						HBLChargesDisplayTypes.CollectCharges);
				});
			}
		}

		#endregion

		#region Exporter Docs Sub-Category

		public MultilingualStringRegistryItem CertificateOfOriginStandardClause
		{
			get
			{
				return GetItem("CertificateOfOriginStandardClause", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem(
						"CertificateOfOriginStandardClause",
						Categories.Documents_Forwarding_ExporterDocs_CertificateofOrigin,
						ResString.GetMultilingualString("39989e42-1d74-44b8-9bff-6bcda27a15d0", "Standard Clause"),
						ResString.GetMultilingualString("143a34de-97e6-4d15-9e30-bb89081c14e6", "This clause will be automatically followed by the country/region of origin of the products in the Certificate of Origin document"),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						ResString.GetMultilingualString("7c00f16f-8a1f-4515-bc1b-abdc3b1fb28b", "I the undersigned, duly authorized by the above exporter and having made the necessary enquiries HEREBY CERTIFY THAT the goods listed above are of origin, production and manufacture in:"));

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public StringRegistryItem LocalChamberOfCommerceInformation
		{
			get
			{
				return GetItem("LocalChamberOfCommerceInformation", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"LocalChamberOfCommerceInformation",
						Categories.Documents_Forwarding_ExporterDocs_CertificateofOrigin,
						ResString.GetMultilingualString("7023bb70-5622-4771-ad7d-7fc250416704", "Local Chamber Of Commerce Information"),
						ResString.GetMultilingualString("6bf6aab2-4fd3-46a6-b10a-095a027ed719", "This information will be printed in the lower portion in the Certificate of Origin document"),
						RegistryStorageFlags.System | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"");

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					result.CountryFilterPKs = Enterprise.Core.CountryGuids.CountriesUnderUSCustomsJurisdiction;

					return result;
				});
			}
		}

		public StringRegistryItem NotaryPublicInformation
		{
			get
			{
				return GetItem("NotaryPublicInformation", delegate
				{
					StringRegistryItem result = new StringRegistryItem(
						"NotaryPublicInformation",
						Categories.Documents_Forwarding_ExporterDocs_CertificateofOrigin,
						ResString.GetMultilingualString("aaeb9303-3f8b-49c9-bee5-43bbf1da6853", "Notary Public Information"),
						ResString.GetMultilingualString("6bf6aab2-4fd3-46a6-b10a-095a027ed719", "This information will be printed in the lower portion in the Certificate of Origin document"),
						RegistryStorageFlags.System | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"");

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					result.CountryFilterPKs = Enterprise.Core.CountryGuids.CountriesUnderUSCustomsJurisdiction;

					return result;
				});
			}
		}

		#endregion
		#endregion

		#region Liner & Agency Sub-Category

		public BooleanRegistryItem ShowChargesOnAgencyBookingConfirmation
		{
			get
			{
				return GetItem("ShowChargesOnAgencyBookingConfirmation", () =>
					new BooleanRegistryItem("ShowChargesOnAgencyBookingConfirmation", Categories.Documents_LinerAgency_BookingConfirmation, ResString.GetMultilingualString("F7F1D6E8-5627-41FB-8FBF-4D3CC3EF2DEF", "Show Charges (Legacy Documents)"), ResString.GetMultilingualString("72e02372-e15b-4f76-85a6-ce9600bcdcdf", "Show charges on booking confirmations printed from Liner & Agency."), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, false));
			}
		}

		public BooleanRegistryItem ShowChargesOnAgencyArrivalNotice
		{
			get
			{
				return GetItem("ShowChargesOnAgencyArrivalNotice", () =>
					new BooleanRegistryItem("ShowChargesOnAgencyArrivalNotice", Categories.Documents_LinerAgency_ArrivalNotice, ResString.GetMultilingualString("264E4496-A7D1-4466-94C4-C1E443A53492", "Show Charges (Legacy Documents)"), ResString.GetMultilingualString("e705fea6-7bc9-48bc-be55-5eebf9e5bf59", "Show charges on arrival notices printed from Liner & Agency."), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default, false));
			}
		}

		public DocumentOpenCloseTextRegistryItem AgencyArrivalNoticeOpeningText
		{
			get
			{
				return GetItem("AgencyArrivalNoticeOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("AgencyArrivalNoticeOpeningText", Categories.Documents_LinerAgency_ArrivalNotice, ResString.GetMultilingualString("86f88e8c-863c-40a9-ad7b-ef11e17ee8d2", "Opening Text"), ResString.GetMultilingualString("72476232-dbea-468b-a6d7-c78b9f54766b", "Arrival Notice Opening Text"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem AgencyArrivalNoticeClosingText
		{
			get
			{
				return GetItem("AgencyArrivalNoticeClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("AgencyArrivalNoticeClosingText", Categories.Documents_LinerAgency_ArrivalNotice, ResString.GetMultilingualString("9e6b688d-f3d5-44ba-83a7-d91721a9548b", "Closing Text"), ResString.GetMultilingualString("7940cdca-523b-4f4a-9a7c-3d182254abf3", "Arrival Notice Closing Text"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		public DocumentOpenCloseText AgencyShipmentArrivalNotice
		{
			get
			{
				return new DocumentOpenCloseText(
					AgencyArrivalNoticeOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					AgencyArrivalNoticeClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public DocumentOpenCloseTextRegistryItem ContainerReleaseOpeningText
		{
			get
			{
				return GetItem("AgencyContainerReleaseOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("AgencyContainerReleaseOpeningText", Categories.Documents_LinerAgency_ContainerRelease, ResString.GetMultilingualString("5e64da6a-ba47-43c4-8cde-58c6a9d22470", "Opening Text"), ResString.GetMultilingualString("ca22dbcd-d1f2-417a-84e7-e8b9e14e6c57", "Container Release Opening Text"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem ContainerReleaseClosingText
		{
			get
			{
				return GetItem("AgencyContainerReleaseClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem("AgencyContainerReleaseClosingText", Categories.Documents_LinerAgency_ContainerRelease, ResString.GetMultilingualString("b2ae69b4-4314-49f5-b6bf-c2d04d40a3f6", "Closing Text"), ResString.GetMultilingualString("bf5e57f3-b278-4fdd-884a-3c10a2b6632e", "Container Release Closing Text"), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		public DocumentOpenCloseText ContainerRelease
		{
			get
			{
				return new DocumentOpenCloseText(
					ContainerReleaseOpeningText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK),
					ContainerReleaseClosingText.GetFallBackValueAtAllLevels(EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK));
			}
		}

		public MultilingualStringRegistryItem ImportDeliveryOrderDetentionChargesText
		{
			get
			{
				return GetItem("ImportDeliveryOrderDentionChargesText", delegate
				{
					return new MultilingualStringRegistryItem(
						"ImportDeliveryOrderDentionChargesText",
						Categories.Documents_LinerAgency_DeliveryOrder, ResString.GetMultilingualString("188744eb-9686-4e7f-bbd8-d74886f79cb0", "Detention Charges"),
						ResString.GetMultilingualString("2c5b8925-6970-4193-864c-93a3a7e689ee", "The text to display in the 'Detention Charges' section of the Delivery Order"),
						new StringRegistryDataType(0, 500),
						new TextRegistryEditorInfo(TextEditorType.Memo),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, null
						);
				});
			}
		}

		public MultilingualStringRegistryItem ImportDeliveryOrderDeliveryClerkNote
		{
			get
			{
				return GetItem("ImportDeliveryOrderDeliveryClerkNote", delegate
				{
					return new MultilingualStringRegistryItem(
						"ImportDeliveryOrderDeliveryClerkNote",
						Categories.Documents_LinerAgency_DeliveryOrder, ResString.GetMultilingualString("4a58545f-498b-4924-a602-acfcf6a06683", "Delivery Clerk Note"),
						ResString.GetMultilingualString("b9c997c2-8045-4444-afc6-55f157c77a8b", "The text to display next to the 'Detention Charges' section of the Delivery Order"),
						new StringRegistryDataType(0, 500),
						new TextRegistryEditorInfo(TextEditorType.Memo),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue, null
						);
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem DetentionAdviceOpeningText
		{
			get
			{
				return GetItem("DetentionAdviceOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"DetentionAdviceOpeningText",
						Categories.Documents_LinerAgency_DetentionAdvice,
						ResString.GetMultilingualString("aabd4372-137b-4872-beaa-ecd619198535", "Opening Text"),
						ResString.GetMultilingualString("7feecb56-1eb9-4c44-9cde-795563a88812", "The opening text for the Detention Advice document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem DetentionAdviceClosingText
		{
			get
			{
				return GetItem("DetentionAdviceClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"DetentionAdviceClosingText",
						Categories.Documents_LinerAgency_DetentionAdvice,
						ResString.GetMultilingualString("9bb7341c-4d22-4b0f-acb1-8b271810b1e2", "Closing Text"),
						ResString.GetMultilingualString("fd15bc75-e77e-4ee8-a155-255cf1ae46e3", "The closing text for the Detention Advice document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem AgencyBookingConfirmationOpeningText
		{
			get
			{
				return GetItem("AgencyBookingConfirmationOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"AgencyBookingConfirmationOpeningText",
						Categories.Documents_LinerAgency_BookingConfirmation,
						ResString.GetMultilingualString("aabd4372-137b-4872-beaa-ecd619198535", "Opening Text"),
						ResString.GetMultilingualString("0412afd0-ba39-4543-88eb-d43d510545ca", "The opening text for the Booking Confirmation document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem AgencyBookingConfirmationClosingText
		{
			get
			{
				return GetItem("AgencyBookingConfirmationClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"AgencyBookingConfirmationClosingText",
						Categories.Documents_LinerAgency_BookingConfirmation,
						ResString.GetMultilingualString("9bb7341c-4d22-4b0f-acb1-8b271810b1e2", "Closing Text"),
						ResString.GetMultilingualString("3edd6c55-3dd9-4961-a88c-50f625510284", "The closing text for the Booking Confirmation document."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		#endregion

		#region Digital Docs / Certification Sub-Category
		public BooleanRegistryItem EnableNZCFTASubmissionToCAB
		{
			get
			{
				return GetItem("EnableNZCFTASubmissionToCAB", () =>
					new BooleanRegistryItem("EnableNZCFTASubmissionToCAB",
					Categories.Documents_DigitalDocs_Certification,
					ResString.GetMultilingualString("d29f2e88-ac83-4b4c-a0ee-24869a104615", "NZCFTA"),
					ResString.GetMultilingualString("cef2dd07-a1cd-4394-9419-dd3e2997f68d", "Enable submission of NZCFTA Certificate to CAB."),
					RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					false)
				);
			}
		}

		public BooleanRegistryItem EnableCPTPPSubmissionToCAB
		{
			get
			{
				return GetItem("EnableCPTPPSubmissionToCAB", () =>
					new BooleanRegistryItem("EnableCPTPPSubmissionToCAB",
					Categories.Documents_DigitalDocs_Certification,
					ResString.GetMultilingualString("423a763f-b9ae-4913-b79d-40724f844768", "CPTPP"),
					ResString.GetMultilingualString("f481c108-0edf-479a-9dc3-2b8954c20cc2", "Enable submission of CPTPP Certificate to CAB."),
					RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					false)
				);
			}
		}

		public BooleanRegistryItem EnableRCEPSubmissionToCAB
		{
			get
			{
				return GetItem("EnableRCEPSubmissionToCAB", () =>
					new BooleanRegistryItem("EnableRCEPSubmissionToCAB",
					Categories.Documents_DigitalDocs_Certification,
					ResString.GetMultilingualString("9db54623-d804-4e87-88fb-cdb06da637fb", "RCEP"),
					ResString.GetMultilingualString("49323fda-b919-4d83-9aae-d7d0302ef7fa", "Enable submission of RCEP Certificate to CAB."),
					RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					false)
				);
			}
		}

		public BooleanRegistryItem EnableCertOfOriginIndemnity
		{
			get
			{
				return GetItem("EnableCertOfOriginIndemnity", () =>
					new BooleanRegistryItem("EnableCertOfOriginIndemnity",
					Categories.Documents_DigitalDocs_Certification,
					ResString.GetMultilingualString("9C8F26A5-8FD4-41E0-BE5B-9B39B43984AD", "Enable Cert of Origin Indemnity"),
					ResString.GetMultilingualString("a329ef93-12d5-472b-aa70-47f41679d27d", "Indemnity Terms"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false)
				);
			}
		}
		public MultilingualStringRegistryItem CertOfOriginIndemnity
		{
			get
			{
				return GetItem("CertOfOriginIndemnity", delegate
				{
					return new MultilingualStringRegistryItem
					(
						"CertOfOriginIndemnity",
						Categories.Documents_DigitalDocs_Certification,
						ResString.GetMultilingualString("6c136222-8e4e-4d9c-8967-0d9822f71f0f", "Cert of Origin Indemnity"),
						ResString.GetMultilingualString("a329ef93-12d5-472b-aa70-47f41679d27d", "Indemnity Terms"),
						new StringRegistryDataType(),
						new TextRegistryEditorInfo(TextEditorType.Memo),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						ResString.GetMultilingualString("bd21e1d9-99a7-4bbc-a6fa-31fd1ce0c3ae", @"The Applicant (or the Applicant on behalf of the Consignor), by utilizing WiseTech Certification, certifies that:
	a)	the goods mentioned in the certificates and/or other documents originate in the country (countries) specified in the documents(s) and comply with the rules of origin applicable in the country (countries) to those goods
	b)	the information in the certificates and/or other documents provided to WiseTech Certification is accurate, true and complete
	c)	they (the applicant) will advise WiseTech Certification and any other person(s) to whom the applicant provides the Certificates and/or other documents promptly in writing of any inaccuracy, omission or change in such information, or in the origin of the goods
	d)	they (the applicant) will maintain, and present upon request, such documentation as is necessary to verify the truth, accuracy and completeness of all Certificates, and/or other documents, issued by WiseTech Certification
	e)	in consideration for WiseTech Certification’s issuance of Certificates or Origin and/or other documents, the applicant agrees to release, discharge and hold harmless WiseTech Certification from any liability in connection with the issuance of the Certificates and/or other documents and to indemnify WiseTech Certification in respect of any costs herewith
	f)	in the event of requests which stem from a legitimate enquiry from someone in possession of statutory authority e.g. Police, Department of Foreign Affairs & Trade or officials acting with authority of a Court Order, I/we hereby permit WiseTech Certification to allow direct access, under the power of statutory authority, to such commercial information as may be required as part of the enquiry
	g)	the applicant is authorized to give the undertakings set out herein")
					);
				});
			}
		}

		public BooleanRegistryItem EnableAANZFTASubmissionToCAB
		{
			get
			{
				return GetItem("EnableAANZFTASubmissionToCAB", () =>
					new BooleanRegistryItem("EnableAANZFTASubmissionToCAB",
					Categories.Documents_DigitalDocs_Certification,
					ResString.GetMultilingualString("79C4CF4D-5490-4C66-836B-5D92C1A6850B", "AANZFTA"),
					ResString.GetMultilingualString("EDD5403F-45CC-4A03-BF32-6BF7BCC60999", "Enable submission of AANZFTA Certificate to CAB."),
					RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					false)
				);
			}
		}

		public BooleanRegistryItem EnableCOONZSubmissionToCAB
		{
			get
			{
				return GetItem("EnableCOONZSubmissionToCAB", () =>
					new BooleanRegistryItem("EnableCOONZSubmissionToCAB",
					Categories.Documents_DigitalDocs_Certification,
					ResString.GetMultilingualString("8FCFAC90-516B-476C-A0CD-0591B187ACBA", "Cert of Origin NZ"),
					ResString.GetMultilingualString("AC28065B-530B-4E24-A3BC-920875B6D152", "Enable submission of Cert of Origin NZ to CAB."),
					RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					false)
				);
			}
		}

		public BooleanRegistryItem EnableChAFTASubmissionToCAB
		{
			get
			{
				return GetItem("EnableChAFTASubmissionToCAB", () =>
					new BooleanRegistryItem("EnableChAFTASubmissionToCAB",
					Categories.Documents_DigitalDocs_Certification,
					ResString.GetMultilingualString("630AFC71-F50A-4AA3-8424-349A3A9D5355", "ChAFTA"),
					ResString.GetMultilingualString("122766F9-A0AE-4F6C-86DC-68C83E3CC47B", "Enable submission of ChAFTA Certificate to CAB."),
					RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					false)
				);
			}
		}

		public BooleanRegistryItem EnableCOOUSSubmissionToCAB
		{
			get
			{
				return GetItem("EnableCOOUSSubmissionToCAB", () =>
					new BooleanRegistryItem("EnableCOOUSSubmissionToCAB",
					Categories.Documents_DigitalDocs_Certification,
					ResString.GetMultilingualString("f8ad4619-cfd5-48cb-9985-6be8c290fc7a", "COOUS"),
					ResString.GetMultilingualString("5b26f261-5cf7-4b9a-ad20-5457b82363f7", "Enable submission of US Cert of Origin to CAB."),
					RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					false)
				);
			}
		}

		public BooleanRegistryItem EnableVerboseLoggingForCertification
		{
			get
			{
				return GetItem("EnableVerboseLoggingForCertification", () =>
					new BooleanRegistryItem("EnableVerboseLoggingForCertification",
					Categories.Documents_DigitalDocs_Certification,
					ResString.GetMultilingualString("A0CC0E29-60CD-4FDF-BFE1-FB7030CF072F", "Verbose Logging"),
					ResString.GetMultilingualString("89E20601-21B5-4237-8C33-67A91166EF24", "Enable Verbose Logging"),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false)
				);
			}
		}

		public BooleanRegistryItem EnableSubmissionToCustomsAuthority
		{
			get
			{
				return GetItem("EnableSubmissionToCustomsAuthority", () =>
					new BooleanRegistryItem("EnableSubmissionToCustomsAuthority",
					Categories.Documents_DigitalDocs_Certification,
					ResString.GetMultilingualString("6BD57EEC-0C66-4F4F-9D6E-91A3CD3907E1", "Enable Submission to Customs Authority"),
					ResString.GetMultilingualString("E00E1201-BE86-44C9-9AEE-D3FE0C09F9C1", "When turned off, Customs Connector will not forward the submission to the customs authority and will simply return a rejection."),
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					false)
				);
			}
		}

		public BooleanRegistryItem EnableJAEPASubmissionToCAB
		{
			get
			{
				return GetItem("EnableJAEPASubmissionToCAB", () =>
					new BooleanRegistryItem("EnableJAEPASubmissionToCAB",
					Categories.Documents_DigitalDocs_Certification,
					ResString.GetMultilingualString("7228baef-dbfd-43b3-bc41-21440fe27092", "JAEPA"),
					ResString.GetMultilingualString("f3a2b282-3abe-4c01-9919-49649a95e29a", "Enable submission of JAEPA Certificate to CAB."),
					RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					false)
				);
			}
		}

		public BooleanRegistryItem EnableAUKFTASubmissionToCAB
		{
			get
			{
				return GetItem("EnableAUKFTASubmissionToCAB", () =>
					new BooleanRegistryItem("EnableAUKFTASubmissionToCAB",
					RawDataRegistry.Categories.Documents_DigitalDocs_Certification,
					ResString.GetMultilingualString("59BF8F6E-5B22-43D0-883E-01B21D8D86F3", "A-UKFTA"),
					ResString.GetMultilingualString("CC1F3BBC-417B-4384-9E5E-E47CC232BB2E", "Enable submission of A-UKFTA Certificate to CAB."),
					RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					false)
				);
			}
		}

		public BooleanRegistryItem EnableTAFTASubmissionToCAB
		{
			get
			{
				return GetItem("EnableTAFTASubmissionToCAB", () =>
					new BooleanRegistryItem("EnableTAFTASubmissionToCAB",
					RawDataRegistry.Categories.Documents_DigitalDocs_Certification,
					ResString.GetMultilingualString("1AFBA77B-4BF5-40C8-986A-6BE17B045166", "TAFTA"),
					ResString.GetMultilingualString("58C40B2D-0857-4227-BF47-92028F3B706D", "Enable submission of TAFTA Certificate to CAB."),
					RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					false)
				);
			}
		}

		public BooleanRegistryItem EnableIAECTASubmissionToCAB
		{
			get
			{
				return GetItem("EnableIAECTASubmissionToCAB", () =>
					new BooleanRegistryItem("EnableIAECTASubmissionToCAB",
					RawDataRegistry.Categories.Documents_DigitalDocs_Certification,
					ResString.GetMultilingualString("D43EDF2A-F80C-4E82-946D-534C338BCD70", "IA-ECTA"),
					ResString.GetMultilingualString("6F3E6762-736E-4631-8085-C0FCB4B308B9", "Enable submission of IA-ECTA Certificate to CAB."),
					RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					false)
				);
			}
		}

		public BooleanRegistryItem EnableKAFTASubmissionToCAB
		{
			get
			{
				return GetItem("EnableKAFTASubmissionToCAB", () =>
					new BooleanRegistryItem("EnableKAFTASubmissionToCAB",
					RawDataRegistry.Categories.Documents_DigitalDocs_Certification,
					ResString.GetMultilingualString("EDD1E4B5-ECE5-4BC8-B079-F063284AAEAD", "KAFTA"),
					ResString.GetMultilingualString("F5F262E4-A8B1-4565-86B1-685E510E2B01", "Enable submission of KAFTA Certificate to CAB."),
					RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					false)
				);
			}
		}

		public BooleanRegistryItem EnablePAFTASubmissionToCAB
		{
			get
			{
				return GetItem("EnablePAFTASubmissionToCAB", () =>
					new BooleanRegistryItem("EnablePAFTASubmissionToCAB",
					Categories.Documents_DigitalDocs_Certification,
					ResString.GetMultilingualString("C0CB7166-901F-4ACD-8D4A-4DF63EC9CFC7", "PAFTA"),
					ResString.GetMultilingualString("B6847FE7-B5B3-46B1-8ACC-AC0E0B4F5E66", "Enable submission of PAFTA Certificate to CAB."),
					RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					false)
				);
			}
		}

		public BooleanRegistryItem EnableIACEPASubmissionToCAB
		{
			get
			{
				return GetItem("EnableIACEPASubmissionToCAB", () =>
					new BooleanRegistryItem("EnableIACEPASubmissionToCAB",
					RawDataRegistry.Categories.Documents_DigitalDocs_Certification,
					ResString.GetMultilingualString("88868952-1125-4DE7-8A1F-61D4888A42C1", "IA-CEPA"),
					ResString.GetMultilingualString("2412FFB9-312A-4F66-8B9A-A44FBA28D747", "Enable submission of IA-CEPA Certificate to CAB."),
					RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					false)
				);
			}
		}

		public BooleanRegistryItem EnableCertOfOriginAUSubmissionToCAB
		{
			get
			{
				return GetItem("EnableCertOfOriginAUSubmissionToCAB", () =>
					new BooleanRegistryItem("EnableCertOfOriginAUSubmissionToCAB",
					RawDataRegistry.Categories.Documents_DigitalDocs_Certification,
					ResString.GetMultilingualString("D0138496-D532-446B-8ED9-516C93EB9364", "Cert of Origin AU"),
					ResString.GetMultilingualString("A70B736C-2ED2-427A-9296-C935764574AE", "Enable submission of Cert of Origin AU Certificate to CAB."),
					RegistryStorageFlags.Company,
					RegistryOptions.IsOnlyForSupport,
					false)
				);
			}
		}

		#endregion

		#region CFS / Gate Pass Sub-Category

		public MultilingualStringRegistryItem GatePassClauseText
		{
			get
			{
				return GetItem("GatePassClauseText", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem(
						"GatePassClauseText",
						Categories.Documents_CFS_GatePass,
						ResString.GetMultilingualString("5394d0ed-5377-4dc6-9301-c546a5b12278", "Clause"),
						ResString.GetMultilingualString("3bfcd9dc-fa69-425e-b104-8c33fc2a855d", "The Clause text that will appear at the bottom of the Gate Pass document, to indicate the condition of the goods received."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						ResString.GetMultilingualString("6d58bdce-caf5-4dff-a8c3-028fe6caff36", "Received in apparent good order and condition subject to the terms and conditions contained in the relevant bill of lading, and the exceptions noted hereon."));

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public BooleanRegistryItem ShowGatePassStatementText
		{
			get
			{
				return GetItem("ShowGatePassStatementText", delegate
				{
					return new BooleanRegistryItem(
						"ShowGatePassStatementText",
						Categories.Documents_CFS_GatePass,
						ResString.GetMultilingualString("c98e3466-bf4b-493f-9622-64f857040ce5", "Show Underbond Movement Clause"),
						ResString.GetMultilingualString("be0a6634-205c-45de-b1a7-da0648c836ca", "Show the Underbond Movement Clause at the bottom of the Gate Pass document?"),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}
		#endregion

		#region Orders Sub-Category

		public MultilingualStringRegistryItem OrderDelayAlertText
		{
			get
			{
				return GetItem("OrderDelayAlertText", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem(
						"OrderDelayAlertText",
						Categories.Documents_Orders_OrderDelayAlert,
						ResString.GetMultilingualString("6306a1fe-d057-4bbf-9f4f-e204126c97a6", "Alert Text"),
						ResString.GetMultilingualString("183b2b2f-4d29-4617-a356-d0b7d398e88a", "The Text that will be displayed in the header of the document as an alert."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						ResString.GetMultilingualString("97215384-9812-4372-8f32-dba2cd3cdf62", "PLEASE NOTE, YOUR ORDER HAS BEEN DELAYED, PLEASE SEE UPDATED ORDER DETAILS BELOW"));

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}
		#endregion

		#region User Sign Off Sub-Category

		public BooleanRegistryItem ShowPublishedStaffDetailsOnDocuments
		{
			get
			{
				return GetItem("ShowPublishedStaffDetailsOnDocuments", delegate
				{
					return new BooleanRegistryItem(
						"ShowPublishedStaffDetailsOnDocuments",
						Categories.Documents_UserSignOff,
						ResString.GetMultilingualString("3814e4c6-189d-47ca-a2ee-ee3085667cc1", "Show Published Staff Details On Documents"),
						ResString.GetMultilingualString("2919607d-38d5-4f95-b3d3-1ae7703dc255", "If the user's contact details are published, show these in the document sign off."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public BooleanRegistryItem ShowUserTitleOnDocuments
		{
			get
			{
				return GetItem("ShowUserTitleOnDocuments", delegate
				{
					return new BooleanRegistryItem(
						"ShowUserTitleOnDocuments",
						Categories.Documents_UserSignOff,
						ResString.GetMultilingualString("4ed43634-15cc-4d33-ab90-2c9a184970ca", "Show User Title on Documents"),
						ResString.GetMultilingualString("0ad5543c-88b3-492f-a428-89504bb5c8af", "When this registry item is set to 'Yes', the operator's title will be shown on documents."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}
		#endregion

		#region Quotation and Rates Sub-Category

		public DocumentOpenCloseTextRegistryItem QuoteClosingText
		{
			get
			{
				return GetItem("QuoteClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"QuoteClosingText",
						Categories.Documents_QuotationsandRates,
						ResString.GetMultilingualString("71a9b816-36f6-4ea7-bf59-1bcb2987e8f3", "Quote Entry Closing Text"),
						ResString.GetMultilingualString("6131d665-80e3-4682-bf42-f3e1dbb63876", "The default quote page closing text."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default
					);
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem QuoteOpeningText
		{
			get
			{
				return GetItem("QuoteOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"QuoteOpeningText",
						Categories.Documents_QuotationsandRates,
						ResString.GetMultilingualString("4d20ba8b-de37-436a-aae6-436a752dc768", "Quote Entry Opening Text"),
						ResString.GetMultilingualString("c060f8e2-cfd1-4827-9c07-9aa8c269bd87", "The default quote page opening text."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default
					);
				});
			}
		}

		public BooleanRegistryItem IncludeCFXinExchangeRateOnQuotationPrinting
		{
			get
			{
				return GetItem("IncludeCFXinExchangeRateOnQuotationPrinting", delegate
				{
					return new BooleanRegistryItem(
						"IncludeCFXinExchangeRateOnQuotationPrinting",
						Categories.Documents_QuotationsandRates,
						ResString.GetMultilingualString("9d8976f8-3c5f-48ac-8f4d-e87cc397d361", "Include Currency Uplift (CFX) in Exchange Rates on Quotation Document"),
						ResString.GetMultilingualString("4fd68266-b639-41e8-8fd3-43acf92296cc", "If this option is on, currency uplift (CFX) is included in printed exchange rates."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public BooleanRegistryItem PrintInheritedOriginChargesDefault
		{
			get
			{
				return GetItem("PrintInheritedOriginChargesDefault", delegate
				{
					return new BooleanRegistryItem(
						"PrintInheritedOriginChargesDefault",
						Categories.Documents_QuotationsandRates,
						ResString.GetMultilingualString("bd3328d0-dea8-4474-be75-d22e47a1b33d", "Print Client Rate / Company Tariff Origin Charges on Quote"),
						ResString.GetMultilingualString("049474f7-c1e0-46ac-a776-f1c01295fafe", "This is the default setting for new quotations. It specifies whether applicable client rate / company tariff origin charges will be printed on this quotation."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public IRegistryItem PrintInheritedDestinationChargesDefault
		{
			get
			{
				return GetItem("PrintInheritedDestinationChargesDefault", delegate
				{
					return new BooleanRegistryItem(
						"PrintInheritedDestinationChargesDefault",
						Categories.Documents_QuotationsandRates,
						ResString.GetMultilingualString("e1b2ce0a-66a6-47ee-92fc-432fbe5caf9e", "Print Client Rate / Company Tariff Destination Charges on Quote"),
						ResString.GetMultilingualString("7c25207a-19fe-47e5-8c89-a9ffb36b8c29", "This is the default setting for new quotations. It specifies whether applicable client rate / company tariff destination charges will be printed on this quotation."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public CodePairRegistryItem ShowCalculationDescriptionOnOneOffQuotes
		{
			get
			{
				var lookUpListProvider = new CodeDescriptionPairListProvider(() =>
				{
					var lookUpList = new CodeDescriptionPairList();
					lookUpList.AddPair(ShowCalculationDescriptionOnOneOffQuotesCode.Yes, ResString.GetMultilingualString("5462AECD-3876-444C-981F-D2431C69E2F0", "Under Charge Description generated per Client Organization Invoicing Setup, Calculation details of the Charge is Printed"));
					lookUpList.AddPair(ShowCalculationDescriptionOnOneOffQuotesCode.No, ResString.GetMultilingualString("BD7DB287-FD2D-4AEB-A133-878BC33A987F", "Under Charge Description generated per Client Organization Invoicing Setup, Calculation details of the Charge is NOT Printed"));
					lookUpList.AddPair(ShowCalculationDescriptionOnOneOffQuotesCode.Charge, ResString.GetMultilingualString("6FDC1FDA-72D1-4A0B-95CC-CC7E06DD0113", "Print Charge Description defaulted in One Off Quote ignoring Client Organization Invoicing Setup"));
					return lookUpList;
				});

				return GetItem("ShowCalculationDescriptionOnOneOffQuotes", delegate
				{
					return new CodePairRegistryItem(
						"ShowCalculationDescriptionOnOneOffQuotes",
						Categories.Documents_QuotationsandRates,
						ResString.GetMultilingualString("DE3BE84E-55DC-48F1-99EC-A9FECBD28AB6", "One Off Quotes - Include Calculation Description"),
						ResString.GetMultilingualString("65BD0243-E2FB-4B19-A076-4D99E4A8F9C7", "This registry controls whether the Invoicing Setup of Client Organization and/or mathematical calculation included for displaying Charges in One Off Quotes."),
						lookUpListProvider,
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						ShowCalculationDescriptionOnOneOffQuotesCode.No);
				});
			}
		}

		public BooleanRegistryItem AlternativeRateFormat
		{
			get
			{
				return GetItem("AlternativeRateFormat", delegate
				{
					return new BooleanRegistryItem(
					"AlternativeRateFormat",
					Categories.Documents_QuotationsandRates,
					ResString.GetMultilingualString("0f3f3559-4b37-4bd1-914c-a05d075860a2", "Use Alternative Per Unit Rate Format"),
					ResString.GetMultilingualString("0cfa785d-ed15-42ae-8bf8-97aae9f87878", "If this option is turned on, per unit rates for containers will be printed on individual lines."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
					true);
				});
			}
		}

		public MultilingualStringRegistryItem AcceptancePageOpeningText
		{
			get
			{
				return GetItem("AcceptancePageOpeningText", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem("AcceptancePageOpeningText", Categories.Documents_QuotationsandRates, ResString.GetMultilingualString("bc749fd5-91fa-4fd2-923c-66f8de825e74", "Acceptance Page Opening Text"), ResString.GetMultilingualString("8aaf394b-2032-4461-9e12-2842ad93cb61", "Allows you to enter additional text to print on the Quotation Acceptance."), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public MultilingualStringRegistryItem AcceptancePageClosingText
		{
			get
			{
				return GetItem("AcceptancePageClosingText", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem("AcceptancePageClosingText", Categories.Documents_QuotationsandRates, ResString.GetMultilingualString("f7f7ff24-c940-4f44-bb00-c323dfa92f2b", "Acceptance Page Closing Text"), ResString.GetMultilingualString("d05cd9af-592f-4bc4-98e1-8ed40eb6c0f8", "Allows you to enter additional text to print on the Quotation Acceptance."), RegistryStorageFlags.All, DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		#endregion

		#region ShowContractNumbersOnShippingQuotationDocuments.

		public BooleanRegistryItem ShowContractNumbersOnShippingQuotationDocuments
		{
			get
			{
				return GetItem("ShowContractNumbersOnShippingQuotationDocuments", delegate
				{
					return new BooleanRegistryItem(
						"ShowContractNumbersOnShippingQuotationDocuments",
						Categories.Documents_QuotationsandRates,
						ResString.GetMultilingualString("756C908D-BF5B-4D9D-9A10-593F2C0981B1", "Show contract numbers on shipping quotation documents."),
						ResString.GetMultilingualString("40D3DDDC-887D-4536-86AD-F2229BA94041", "Controls whether to show or hide Contract Number on the quotation documents."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						true);
				});
			}
		}

		#endregion

		#region ShowLocalCurrencyonSpotQuotePricingPage

		public BooleanRegistryItem ShowLocalCurrencyonSpotQuotePricingPage
		{
			get
			{
				return GetItem("ShowLocalCurrencyonSpotQuotePricingPage", delegate
				{
					return new BooleanRegistryItem(
						"ShowLocalCurrencyonSpotQuotePricingPage",
						Categories.Documents_QuotationsandRates,
						ResString.GetMultilingualString("4e6e9475-cc8e-42f1-b2d1-4ded536d02e8", "Show local currency equivalents and exchange rates on the Spot Quote Pricing Page."),
						ResString.GetMultilingualString("895a74b5-03e6-4a92-a3b3-4fed60ecf9b3", "Controls whether local currency equivalents and exchange rates appear on the Spot Quote document.\r\n\r\nOnly applies to legacy versions of the document."),
						RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						true);
				});
			}
		}

		#endregion

		#region Landed Costing Sub-Category

		public MultilingualStringRegistryItem LandedCostingClosingText
		{
			get
			{
				return GetItem("LandedCostingClosingText", delegate
				{
					MultilingualStringRegistryItem result = new MultilingualStringRegistryItem(
						"LandedCostingClosingText",
						Categories.Documents_LandedCosting,
						ResString.GetMultilingualString("4665c197-13ee-4bff-840b-4efc5eab1bf5", "Landed Costing Closing Text"),
						ResString.GetMultilingualString("0da48a4b-3596-4b73-926b-b58221e37157", "This is the closing text that will appear in the Landed Costing document."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);

					result.EditorInfo = new TextRegistryEditorInfo(TextEditorType.Memo);
					return result;
				});
			}
		}

		public BooleanRegistryItem PullLandedCostingDataFromBillingTabOnly
		{
			get
			{
				return GetItem("PullLandedCostingDataFromBillingTabOnly", delegate
				{
					return new BooleanRegistryItem(
						"PullLandedCostingDataFromBillingTabOnly",
						Categories.Documents_LandedCosting,
						ResString.GetMultilingualString("b03418ce-ce59-4653-b1ad-01ac31b0e53f", "Pull Landed Costing details from Billing Tab Only"),
						ResString.GetMultilingualString("055e972a-a2c8-47f4-a68c-94f5e255ef37", "If this registry item is set then Landed Costing details will only be copied from the Billing Tab and not from the Customs Entry. Customs Duty and Fees will always be obtained from the Entry, regardless of the setting of this Registry Item."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}

		public BooleanRegistryItem DoNotPullZeroAmountsFromBillingTab
		{
			get
			{
				return GetItem("DoNotPullZeroAmountsFromBillingTab", delegate
				{
					return new BooleanRegistryItem(
						"DoNotPullZeroAmountsFromBillingTab",
						Categories.Documents_LandedCosting,
						ResString.GetMultilingualString("fab2a408-6496-40e2-97fd-1d8f31fdac78", "Do not pull zero Amounts from Billing Tab."),
						ResString.GetMultilingualString("ed6435e4-afb7-4ae2-9476-ac50c16599f1", "This registry item controls whether zero value lines, on the billing tab, will be copied to the landed costing grid."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue,
						false);
				});
			}
		}
		#endregion

		public BooleanRegistryItem IncludeCancelledInvoicesInDocumentPacks
		{
			get
			{
				return GetItem("IncludeCancelledInvoicesInDocumentPacks", delegate
				{
					return new BooleanRegistryItem(
						"IncludeCancelledInvoicesInDocumentPacks",
						Categories.Documents,
						ResString.GetMultilingualString("3fe4355a-9ccd-4568-afa7-77f0d78699ea", "Include Canceled Invoices In Document Packs"),
						ResString.GetMultilingualString("b80d068e-123c-4eb5-9755-1436bc696439", "This setting controls whether Canceled Invoices are included within Document Packs."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						false);
				});
			}
		}

		public CodePairRegistryItem FreightChargesConversionFactorDisplayOption
		{
			get
			{
				return GetItem("FreightChargesConversionFactorDisplayOption", delegate
				{
					return new CodePairRegistryItem("FreightChargesConversionFactorDisplayOption",
						Categories.Documents_QuotationsandRates,
						ResString.GetMultilingualString("d8ca3a4f-0b23-481b-be5e-e853effd8427",
						"Freight Charges Conversion Factor Display Option"),
						ResString.GetMultilingualString("22a5c072-c5ff-49fd-bf81-97e81e5b6f75",
						"This setting allows you to specify whether the conversion factor itself, or simply 'per W/M' is shown on quotations."),
						OLookUpEditType.ConversionFactor,
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
						"CON");
				});
			}
		}

		public WatermarkRegistryItem Watermark
		{
			get
			{
				return GetItem("DocumentWatermark", delegate
				{
					return new WatermarkRegistryItem(
						"DocumentWatermark",
						Categories.Documents,
						ResString.GetMultilingualString("0385fda9-c826-437d-814a-c334add88512", "Watermark"),
						ResString.GetMultilingualString("e01ef0c1-5819-4502-aad8-a6cb9b2b5c29", "You can choose to use a text or an image watermark to put on all documents delivered in 'Draft' mode. The image must be a PNG file with a transparent background and transparent content, otherwise it will cover portions of the document. The red markings in the diagram show the system defaults."),
						RegistryStorageFlags.System,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.PreserveTestValue);
				});
			}
		}

		public IntRegistryItem TestWatermarkOpacity
		{
			get
			{
				return GetItem("TestWatermarkOpacity", delegate
				{
					IntRegistryItem result = new IntRegistryItem(
						"TestWatermarkOpacity",
						Categories.Documents,
						ResString.GetMultilingualString("3a3ca586-e49f-4dbc-ad0e-8ff0943cb8a0", "Test Watermark Opacity"),
						ResString.GetMultilingualString("0bc487b1-ef7d-481a-903d-2db6d87ea5e1", "How opaque the 'Training / Test Non Commercial Use only' watermark should be. 255 for fully opaque (black), 10 for nearly transparent (white)."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						60
						);
					result.DataType = new IntRegistryDataType(10, 255);
					return result;
				});
			}
		}

		public IntRegistryItem StoredNumberOfSupersededTemplates
		{
			get
			{
				return GetItem<IntRegistryItem>("StoredNumberOfSupersededTemplates", delegate
				{
					IntRegistryItem result = new IntRegistryItem(
						"StoredNumberOfSupersededTemplates",
						Categories.Documents,
						ResString.GetMultilingualString("921e6133-1eaa-42e1-a91f-8015167a93a9", "Stored number of superseded templates"),
						ResString.GetMultilingualString("f209c5c7-42be-46ac-9f24-a767dbd70903", "This setting manages how many superseded templates are retained."),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForController,
						10
					);
					result.DataType = new IntRegistryDataType(1, Int32.MaxValue);
					return result;
				});
			}
		}

		public EmailFormatRegistryItem EmailFormat
		{
			get
			{
				return GetItem("EmailFormat", delegate
				{
					return new EmailFormatRegistryItem(
						"EmailFormat",
						Categories.Documents,
						ResString.GetMultilingualString("b8fec518-bf9a-4202-91b4-7940b9534792", "Email Format"),
						ResString.GetMultilingualString("20980c0e-1ebc-4e05-b94e-f6bf1ce0d4a4", "You can set the fields to be included in email subject and signature, and choose how they will be displayed."),
						RegistryStorageFlags.Company,
						RegistryOptions.PreserveTestValue);
				});
			}
		}

		public CodeDescriptionPairList EmailSubjectFieldsPairList
		{
			get
			{
				if (fEmailSubjectFieldsPairList == null)
				{
					fEmailSubjectFieldsPairList = new CodeDescriptionPairList();
					fEmailSubjectFieldsPairList.AddPair(Core.Constants.EmailFormat.EmailFieldCodes.CompanyBrandName, Core.Constants.EmailFormat.EmailFieldDescriptions.CompanyBrandName);
					fEmailSubjectFieldsPairList.AddPair(Core.Constants.EmailFormat.EmailFieldCodes.BranchName, Core.Constants.EmailFormat.EmailFieldDescriptions.BranchName);
					fEmailSubjectFieldsPairList.AddPair(Core.Constants.EmailFormat.EmailFieldCodes.BranchCode, Core.Constants.EmailFormat.EmailFieldDescriptions.BranchCode);
					fEmailSubjectFieldsPairList.AddPair(Core.Constants.EmailFormat.EmailFieldCodes.DocumentName, Core.Constants.EmailFormat.EmailFieldDescriptions.DocumentName);
				}
				return fEmailSubjectFieldsPairList;
			}
		}
		CodeDescriptionPairList fEmailSubjectFieldsPairList;

		public CodeDescriptionPairList EmailSignatureFieldsPairList
		{
			get
			{
				if (fEmailSignatureFieldsPairList == null)
				{
					fEmailSignatureFieldsPairList = new CodeDescriptionPairList();
					fEmailSignatureFieldsPairList.AddPair(Core.Constants.EmailFormat.EmailFieldCodes.CompanyBrandName, Core.Constants.EmailFormat.EmailFieldDescriptions.CompanyBrandName);
					fEmailSignatureFieldsPairList.AddPair(Core.Constants.EmailFormat.EmailFieldCodes.CompanyAddress, Core.Constants.EmailFormat.EmailFieldDescriptions.CompanyAddress);
					fEmailSignatureFieldsPairList.AddPair(Core.Constants.EmailFormat.EmailFieldCodes.CompanyPhone, Core.Constants.EmailFormat.EmailFieldDescriptions.CompanyPhone);
					fEmailSignatureFieldsPairList.AddPair(Core.Constants.EmailFormat.EmailFieldCodes.CompanyFax, Core.Constants.EmailFormat.EmailFieldDescriptions.CompanyFax);
					fEmailSignatureFieldsPairList.AddPair(Core.Constants.EmailFormat.EmailFieldCodes.CompanyEmail, Core.Constants.EmailFormat.EmailFieldDescriptions.CompanyEmail);
					fEmailSignatureFieldsPairList.AddPair(Core.Constants.EmailFormat.EmailFieldCodes.CompanyWeb, Core.Constants.EmailFormat.EmailFieldDescriptions.CompanyWeb);

					fEmailSignatureFieldsPairList.AddPair(Core.Constants.EmailFormat.EmailFieldCodes.BranchName, Core.Constants.EmailFormat.EmailFieldDescriptions.BranchName);
					fEmailSignatureFieldsPairList.AddPair(Core.Constants.EmailFormat.EmailFieldCodes.BranchCode, Core.Constants.EmailFormat.EmailFieldDescriptions.BranchCode);
					fEmailSignatureFieldsPairList.AddPair(Core.Constants.EmailFormat.EmailFieldCodes.BranchAddress, Core.Constants.EmailFormat.EmailFieldDescriptions.BranchAddress);
					fEmailSignatureFieldsPairList.AddPair(Core.Constants.EmailFormat.EmailFieldCodes.BranchPhone, Core.Constants.EmailFormat.EmailFieldDescriptions.BranchPhone);
					fEmailSignatureFieldsPairList.AddPair(Core.Constants.EmailFormat.EmailFieldCodes.BranchFax, Core.Constants.EmailFormat.EmailFieldDescriptions.BranchFax);
					fEmailSignatureFieldsPairList.AddPair(Core.Constants.EmailFormat.EmailFieldCodes.BranchEmail, Core.Constants.EmailFormat.EmailFieldDescriptions.BranchEmail);
					fEmailSignatureFieldsPairList.AddPair(Core.Constants.EmailFormat.EmailFieldCodes.BranchWeb, Core.Constants.EmailFormat.EmailFieldDescriptions.BranchWeb);

					fEmailSignatureFieldsPairList.AddPair(Core.Constants.EmailFormat.EmailFieldCodes.UserName, Core.Constants.EmailFormat.EmailFieldDescriptions.UserName);
					fEmailSignatureFieldsPairList.AddPair(Core.Constants.EmailFormat.EmailFieldCodes.UserTitle, Core.Constants.EmailFormat.EmailFieldDescriptions.UserTitle);
					fEmailSignatureFieldsPairList.AddPair(Core.Constants.EmailFormat.EmailFieldCodes.UserWorkPhone, Core.Constants.EmailFormat.EmailFieldDescriptions.UserWorkPhone);
					fEmailSignatureFieldsPairList.AddPair(Core.Constants.EmailFormat.EmailFieldCodes.UserFax, Core.Constants.EmailFormat.EmailFieldDescriptions.UserFax);
					fEmailSignatureFieldsPairList.AddPair(Core.Constants.EmailFormat.EmailFieldCodes.UserMobile, Core.Constants.EmailFormat.EmailFieldDescriptions.UserMobile);
					fEmailSignatureFieldsPairList.AddPair(Core.Constants.EmailFormat.EmailFieldCodes.UserEmail, Core.Constants.EmailFormat.EmailFieldDescriptions.UserEmail);
				}
				return fEmailSignatureFieldsPairList;
			}
		}
		CodeDescriptionPairList fEmailSignatureFieldsPairList;

		#region Labels Sub-Category

		public StringRegistryItem CompanyDisplayName
		{
			get
			{
				return GetItem("CompanyDisplayName", delegate
				{
					return new StringRegistryItem(
						"CompanyDisplayName",
						Categories.Documents_Labels,
						ResString.GetMultilingualString("991815e0-f964-467f-897c-276d46f19a56", "Company Display Name"),
						ResString.GetMultilingualString("20c5705b-d4a1-4b37-b646-15373b61d1a4", "A shortened company name for printing on labels."),
						new StringRegistryDataType(0, 35),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.PreserveTestValue);
				});
			}
		}
		#endregion

		public BooleanRegistryItem EnableSpecificPageRangesPrintingOption => GetItem("EnableSpecificPageRangesPrintingOption", delegate
		{
			return new BooleanRegistryItem(
				"EnableSpecificPageRangesPrintingOption",
				Categories.Documents,
				ResString.GetMultilingualString("C479BFC3-F195-4782-91DD-C7EE679277A4", "Enable Specific Page Ranges Printing Option"),
				ResString.GetMultilingualString("086E74DD-DCD5-4329-B25A-1E4A3D05A3D6", "Enables the Specific Page Ranges Printing Option."),
				RegistryStorageFlags.System | RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
				false);
		});

		public bool ShowOnlyPrintersUserCanPrintTo
		{
			get { return ShowOnlyPrintersUserCanPrintToRaw.GetValueWithoutFallback(Env.CurrentUser.PK, Guid.Empty, Guid.Empty); }
			set { ShowOnlyPrintersUserCanPrintToRaw.SetValue(Env.CurrentUser.PK, Guid.Empty, Guid.Empty, value); }
		}

		internal BooleanRegistryItem ShowOnlyPrintersUserCanPrintToRaw
		{
			get
			{
				return GetItem("ShowOnlyPrintersUserCanPrintTo", delegate
				{
					return new BooleanRegistryItem(
						"ShowOnlyPrintersUserCanPrintTo",
						null, null, null,
						RegistryStorageFlags.Company,
						RegistryOptions.IsHidden | RegistryOptions.PreserveTestValue,
						true);
				});
			}
		}

		public ReportColumnSettingsRetriever ReportColumnSettings
		{
			get
			{
				if (reportColumnSettings == null)
				{
					reportColumnSettings = new ReportColumnSettingsRetriever();
				}
				return reportColumnSettings;
			}
		}

		ReportColumnSettingsRetriever reportColumnSettings;

		#region Recipient Details

		public BooleanRegistryItem PrintSystemCreatedContactWhenNoRealContactFound
		{
			get
			{
				return GetItem("PrintSystemCreatedContactWhenNoRealContactFound", delegate
				{
					return new BooleanRegistryItem(
						"PrintSystemCreatedContactWhenNoRealContactFound",
						Categories.Documents_RecipientDetails,
						ResString.GetMultilingualString("0d8e1f92-79db-4a5a-b6e4-bf12093f6240", "Use Generic Contact when None Found"),
						ResString.GetMultilingualString("2860dbaf-e7b5-4e5e-b523-16669affa500", "By default, when no real contact is found for document delivery, a generic contact is used such as 'The Accounts Payable Manager'. This setting allows you to turn off this behavior."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		public BooleanRegistryItem ContactNameUpperCase
		{
			get
			{
				return GetItem("ContactNameUpperCase", delegate
				{
					return new BooleanRegistryItem(
						"ContactNameUpperCase",
						Categories.Documents_RecipientDetails,
						ResString.GetMultilingualString("b8d4f0cf-f2ab-44b2-bb73-89a2c2f6a2e0", "Print Contact Name in Upper Case"),
						ResString.GetMultilingualString("c90b8574-8765-4e2a-9731-a341c77e947a", "By default, contact names are printed in upper case. You can change this by changing this setting."),
						RegistryStorageFlags.System,
						true);
				});
			}
		}

		#endregion

		#region Warehouse Sub-Category

		#region Cartage Advice

		public DocumentOpenCloseTextRegistryItem WarehouseCartageAdviceClosingText
		{
			get
			{
				return GetItem("WarehouseCartageAdviceClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"WarehouseCartageAdviceClosingText",
						Categories.Documents_Warehouse_CartageAdvice,
						ResString.GetMultilingualString("9bb7341c-4d22-4b0f-acb1-8b271810b1e2", "Closing Text"),
						ResString.GetMultilingualString("e095bb3a-8eb3-4c13-8d2e-2ad44efe3839", "The Closing text that will be displayed at the bottom of every Warehouse Cartage Advice document."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem WarehouseCartageAdviceOpeningText
		{
			get
			{
				return GetItem("WarehouseCartageAdviceOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"WarehouseCartageAdviceOpeningText",
						Categories.Documents_Warehouse_CartageAdvice,
						ResString.GetMultilingualString("aabd4372-137b-4872-beaa-ecd619198535", "Opening Text"),
						ResString.GetMultilingualString("889ca723-82c6-4de1-b045-57cf3b914f10", "The Opening text that will be displayed at the header of every Warehouse Cartage Advice document."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		#endregion

		#endregion

		#region Transport Booking Sub-Category

		#region Cartage Advice

		public DocumentOpenCloseTextRegistryItem TransportBookingCartageAdviceClosingText
		{
			get
			{
				return GetItem("TransportBookingCartageAdviceClosingText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"TransportBookingCartageAdviceClosingText",
						Categories.Documents_TransportBooking_CartageAdvice,
						ResString.GetMultilingualString("9bb7341c-4d22-4b0f-acb1-8b271810b1e2", "Closing Text"),
						ResString.GetMultilingualString("1add6261-9db3-4334-aaa2-d26c4c61ac72", "The Closing text that will be displayed at the bottom of every Transport Booking Cartage Advice document."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		public DocumentOpenCloseTextRegistryItem TransportBookingCartageAdviceOpeningText
		{
			get
			{
				return GetItem("TransportBookingCartageAdviceOpeningText", delegate
				{
					return new DocumentOpenCloseTextRegistryItem(
						"TransportBookingCartageAdviceOpeningText",
						Categories.Documents_TransportBooking_CartageAdvice,
						ResString.GetMultilingualString("aabd4372-137b-4872-beaa-ecd619198535", "Opening Text"),
						ResString.GetMultilingualString("627e3530-cc6f-4564-b6d9-0624a22c62ab", "The Opening text that will be displayed at the header of every Transport Booking Cartage Advice document."),
						RegistryStorageFlags.All,
						DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default);
				});
			}
		}

		#endregion

		#endregion

		ICodeDescriptionPairListProvider RedirectedDocumentAddressFormattingOptionsListProvider
		{
			get
			{
				if (fRedirectedDocumentAddressFormattingOptionsListProvider == null)
				{
					fRedirectedDocumentAddressFormattingOptionsListProvider = new CodeDescriptionPairListProvider(() => new RedirectedDocumentAddressFormattingOptionList());
				}

				return fRedirectedDocumentAddressFormattingOptionsListProvider;
			}
		}
		ICodeDescriptionPairListProvider fRedirectedDocumentAddressFormattingOptionsListProvider;

		internal CodeDescriptionPairList RedirectedDocumentAddressFormattingOptions
		{
			get { return fRedirectedDocumentAddressFormattingOptions ?? (fRedirectedDocumentAddressFormattingOptions = RedirectedDocumentAddressFormattingOptionsListProvider.CodeDescriptionPairList); }
		}
		CodeDescriptionPairList fRedirectedDocumentAddressFormattingOptions;

		public CodePairRegistryItem RedirectedDocumentAddressFormatting
		{
			get
			{
				return GetItem("RedirectedDocumentAddressFormatting", delegate
				{
					return new CodePairRegistryItem(
						"RedirectedDocumentAddressFormatting",
						Categories.Documents_AddressFormatting,
						ResString.GetMultilingualString("41d15bec-9b2e-4d12-b25a-5d85763c7580", "'Redirect To' Address Handling"),
						ResString.GetMultilingualString("0c760187-cb56-4db8-855e-428ac45c0712", "When sending a Document that has an 'Official' recipient (e.g.: AR Invoice), the default behavior is to print the 'Official' contact and no other at the top of the Document. This setting allows you to override this behavior, and will affect the generation of the recipient name/address on all documents that have an 'Official' recipient."),
						RedirectedDocumentAddressFormattingOptionsListProvider,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RedirectedDocumentAddressFormattingOptionList.Codes.OfficialContactOnly);
				});
			}
		}

		public CodePairRegistryItem AddressPosition
		{
			get
			{
				return GetItem(
					"AddressPosition",
					delegate
					{
						return new CodePairRegistryItem(
							"AddressPosition",
							Categories.Documents_DocBuilder,
							ResString.GetMultilingualString("4aff5373-b813-4b87-bc00-b60f443bd07f", "Address Position"),
							ResString.GetMultilingualString("1c337ebf-cb95-4819-ade7-ab69cdf5103d", "Set the position of the address on DocBuilder documents to the left or right."),
							new CodeDescriptionPairListProvider(() => new AddressPositionList()),
							RegistryStorageFlags.System | RegistryStorageFlags.Company);
					});
			}
		}

		public CodePairRegistryItem DeliveryMethodWhenNoContactSpecified
		{
			get
			{
				return GetItem("DeliveryMethodWhenNoContactSpecified", delegate
				{
					return new CodePairRegistryItem(
						"DeliveryMethodWhenNoContactSpecified",
						Categories.Documents,
						ResString.GetMultilingualString("c9d8b072-d36f-4304-8f2a-e71191be12e8", "Delivery Method When No Contact Specified"),
						ResString.GetMultilingualString("c9d8b072-d36f-4304-8f2a-e71191be12e9", "When no default contact can be found for a document delivery, the system will try to send to the main office address using the delivery method specified in this option."),
						DeliveryMethodOptionsListProvider,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						DeliveryMethodOptionList.Codes.EmailFaxThenPrint);
				});
			}
		}

		ICodeDescriptionPairListProvider DeliveryMethodOptionsListProvider
		{
			get
			{
				if (fDeliveryMethodOptionsListProvider == null)
				{
					fDeliveryMethodOptionsListProvider = new CodeDescriptionPairListProvider(() => new DeliveryMethodOptionList());
				}

				return fDeliveryMethodOptionsListProvider;
			}
		}
		ICodeDescriptionPairListProvider fDeliveryMethodOptionsListProvider;

		internal CodeDescriptionPairList DeliveryMethodOptions
		{
			get { return deliveryMethodOptions ?? (deliveryMethodOptions = DeliveryMethodOptionsListProvider.CodeDescriptionPairList); }
		}
		CodeDescriptionPairList deliveryMethodOptions;

		public CodePairRegistryItem EDocsToBeAutoAttachedForDelivery
		{
			get
			{
				return GetItem("EDocsToBeAutoAttachedForDelivery", delegate
				{
					return new CodePairRegistryItem(
						"EDocsToBeAutoAttachedForDelivery",
						Categories.Documents,
						ResString.GetMultilingualString("75d2a08d-9175-4a20-a291-5418c157ebe9", "eDocs Documents Auto-Attached by Type"),
						ResString.GetMultilingualString("b3565f10-9bde-4b23-ae42-62905f9a1c1a", "Select which eDocs Documents of a given Document Type to automatically attach when a Document Type is configured to be attached on delivery of a given Document. NB: This only has an effect when there are multiple Documents with the same Document Type selected in the one eDocs module."),
						DeliverEDocsOptionsListProvider,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						DeliverEDocsOptionList.Codes.SendMostRecentSystemGeneratedAndAllManuallyAdded);
				});
			}
		}

		ICodeDescriptionPairListProvider DeliverEDocsOptionsListProvider
		{
			get
			{
				if (fDeliverEDocsOptionsListProvider == null)
				{
					fDeliverEDocsOptionsListProvider = new CodeDescriptionPairListProvider(() => new DeliverEDocsOptionList());
				}

				return fDeliverEDocsOptionsListProvider;
			}
		}
		ICodeDescriptionPairListProvider fDeliverEDocsOptionsListProvider;

		internal CodeDescriptionPairList DeliverEDocsOptions
		{
			get { return deliverEDocsOptions ?? (deliverEDocsOptions = DeliverEDocsOptionsListProvider.CodeDescriptionPairList); }
		}
		CodeDescriptionPairList deliverEDocsOptions;

		public CodePairRegistryItem ExcelDefaultRenderingFormat
		{
			get
			{
				return GetItem("ExcelDefaultRenderingFormat", delegate
				{
					return new CodePairRegistryItem(
						"ExcelDefaultRenderingFormat",
						Categories.Documents,
						ResString.GetMultilingualString("4454f6e3-8392-460d-b893-ef22801067a9", "Excel default rendering format"),
						ResString.GetMultilingualString("96e0f5d0-1232-4854-879b-b17600230bbf", @"Select the Excel rendering format that the Document Engine will use by default to generate reports/documents. This is the format you will get when you preview any document TIF/PDF documents and decided to open them in Excel.

Note: If you choose Excel 97-2003 rendering by default and you deliver a document/report as PDF/TIF that hits the 65,535 rows limit, Document Engine will automatically use Excel 2007 rendering instead."),
						ExcelFileFormatOptionsListProvider,
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						ExcelFileFormatOptionList.Codes.XLSX);
				});
			}
		}

		ICodeDescriptionPairListProvider ExcelFileFormatOptionsListProvider
		{
			get
			{
				if (fExcelFileFormatOptionsListProvider == null)
				{
					fExcelFileFormatOptionsListProvider = new CodeDescriptionPairListProvider(() => new ExcelFileFormatOptionList());
				}

				return fExcelFileFormatOptionsListProvider;
			}
		}
		ICodeDescriptionPairListProvider fExcelFileFormatOptionsListProvider;

		internal CodeDescriptionPairList ExcelFileFormatOptions
		{
			get { return excelFileFormatOptions ?? (excelFileFormatOptions = ExcelFileFormatOptionsListProvider.CodeDescriptionPairList); }
		}
		CodeDescriptionPairList excelFileFormatOptions;

		public BooleanRegistryItem DeliverDocumentsToPrintersInPdfFormat
		{
			get
			{
				return GetItem("DeliverDocumentsToPrintersInPdfFormat", () => new BooleanRegistryItem(
					"DeliverDocumentsToPrintersInPdfFormat",
					RawDataRegistry.Categories.Documents,
					ResString.GetMultilingualString("{23792CC2-E43B-4BBA-BF12-07CD01905819}",
						"Deliver Documents to printers in PDF format"),
					ResString.GetMultilingualString("{E6D03DC8-FD11-449B-B26F-3A701D8FB16D}",
						@"When this registry item is turned on, the system will use PDF as the default format for print jobs rather than Excel format.
Combined with the ""Embed Fonts In PDF"" registry setting, this will avoid having to install fonts on the WebPrint Client servers."),
					RegistryStorageFlags.Company | RegistryStorageFlags.System,
					false));
			}
		}

		public BooleanRegistryItem AutomaticallySwitchToXLSXIfRequired
		{
			get
			{
				return GetItem("AutomaticallySwitchToXLSXIfRequired", delegate
				{
					return new BooleanRegistryItem(
						"AutomaticallySwitchToXLSXIfRequired",
						Categories.Documents,
						ResString.GetMultilingualString("d96b7991-1146-4602-9ebb-22d949f52698", "Automatically switch to XLSX file format if required"),
						ResString.GetMultilingualString("b5737263-5992-40bd-ad46-e9ff87a7b928", @"This is to let Document Engine automatically switch to Excel 2007 XLSX file format if a document/report generated as XLS contains too many rows for Excel 2003.
This will overcome rows limitation (65,535) from Excel 2003.
If this setting is disabled and a document delivered as XLS exceeds 65,535 rows, users will be prompted if they want to use XLSX format instead."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						true);
				});
			}
		}

		public ParameterizedStringRegistryItem AttentionPrefixForAddressContactNameLineText
		{
			get
			{
				return GetItem("AttentionPrefixForAddressContactNameLineText", delegate
				{
					return new ParameterizedStringRegistryItem("AttentionPrefixForAddressContactNameLineText",
						Categories.Documents_AddressFormatting,
						ResString.GetMultilingualString("e05b8520-0c4e-49b6-bd65-5bdc1884fbef", "'Attention' Contact Prefix"),
						ResString.GetMultilingualString("e8fb1209-4f89-4f4a-acca-9ec232de5d1a", @"Allows you to change the 'ATTENTION:' prefix shown on the Contact line of the formatted address at the top of a Document."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryOptions.Default,
						BuildAttentionPrefixLiteral(ExampleName),
						ExampleName
						);
				});
			}
		}

		public ParameterizedStringRegistryItem RedirectToPrefixForAddressContactNameLineText
		{
			get
			{
				return GetItem("RedirectToPrefixForAddressContactNameLineText", delegate
				{
					return new ParameterizedStringRegistryItem("RedirectToPrefixForAddressContactNameLineText",
						Categories.Documents_AddressFormatting,
						ResString.GetMultilingualString("edf85591-cf2d-49b4-9ff4-f1234b7ec351", "'Redirect To' Contact Prefix"),
						ResString.GetMultilingualString("79b60d55-2943-4105-b73b-d0f7c55116f7", @"Allows you to change the 'REDIRECT TO:' prefix shown on the Contact line of the formatted address at the top of a Document. This setting will only have an effect when the 'Redirect To' Address Handling is enabled."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						RegistryOptions.Default,
						BuildRedirectToPrefixLiteral(ExampleName),
						ExampleName
						);
				});
			}
		}

		ResourceString BuildAttentionPrefixLiteral(ResourceString exampleName)
		{
			return ResString.GetMultilingualString("27d8188d-3f6a-47a0-8b8b-e1b3050b9a95", "ATTENTION: {0}", exampleName);
		}

		ResourceString BuildRedirectToPrefixLiteral(ResourceString exampleName)
		{
			return ResString.GetMultilingualString("d70ea7db-06e1-4749-8879-44e63402096d", "REDIRECT TO: {0}", exampleName);
		}

		public ResourceString ExampleName
		{
			get
			{
				return ResString.GetMultilingualString("9d2091f5-58e2-4578-9e97-21bb780df196", "Example Name");
			}
		}

		public BooleanRegistryItem UseScheduledTaskDescriptionInEmailSubjectForScheduledReports
		{
			get
			{
				return GetItem("UseScheduledTaskDescriptionInEmailSubjectForScheduledReports", () =>
				{
					return new BooleanRegistryItem(
						"UseScheduledTaskDescriptionInEmailSubjectForScheduledReports",
						Categories.Documents,
						ResString.GetMultilingualString("F6BC1FBB-B7E9-4E59-A406-2373B10B8F39", "Use Scheduled Task Description In Email Subject For Scheduled Reports"),
						ResString.GetMultilingualString("BC762302-4028-4D92-B02C-66019BA1031C", @"This setting specifies what will be used in the email subject when delivering a Scheduled Report via email.
If enabled, the Scheduled Task Description will be used, otherwise the Report Name will be used in the email subject."),
						RegistryStorageFlags.Company,
						defaultValue: false);
				});
			}
		}

		#region Document Delivery Default Language

		public DocumentDeliveryDefaultLanguagesRegistryItem DocumentDeliveryDefaultLanguage
		{
			get
			{
				return GetItem("DocumentDeliveryDefaultLanguage",
					() => new DocumentDeliveryDefaultLanguagesRegistryItem(
						"DocumentDeliveryDefaultLanguage",
						Categories.Documents,
						ResString.GetMultilingualString("256170FD-8FA0-4D66-88E3-049EE24CF1BA", "Document Delivery Default Language"),
						ResString.GetMultilingualString("7EBA2DB1-AEA4-4941-A1B8-9CC38C510362", "Specify the sequence to be used when determining the language of documents being delivered."),
						RegistryStorageFlags.Company | RegistryStorageFlags.System,
						new DocumentDeliveryDefaultLanguagesCollection()
					)
				);
			}
		}

		#endregion

		#region SuppressResourceStringsCheckRegion

		public LogDocumentRendererRegistryItem LogDocumentRenderer
		{
			get
			{
				return GetItem("LogDocumentRenderer", delegate
				{
					return new LogDocumentRendererRegistryItem(
						"LogDocumentRenderer",
						Categories.Documents,
						(NoResString)"Log Document Renderer",
						(NoResString)"Enables the logging of the rendering of documents.",
						new LogDocumentRendererRegistry());
				});
			}
		}

		#endregion

		#region DocumentImages

		public SystemDefinableRegistryImageCollectionRegistryItem DocumentImages
		{
			get
			{
				return GetItem("DocumentImages", delegate
				{
					return new SystemDefinableRegistryImageCollectionRegistryItem(
						"DocumentImages",
						Categories.Documents,
						ResString.GetMultilingualString("c3578040-1a66-4b12-a404-3faffbfe1341", "Document Images"),
						ResString.GetMultilingualString("f2c7997f-3988-46ad-b715-973026ec0003", @"Image files that are used with document and report macros."),
						RegistryStorageFlags.All);
				});
			}
		}

		#endregion

		public DigitalSignatureRegistryItem DigitalSignature => GetItem("DigitalSignature", delegate
		{
			return new DigitalSignatureRegistryItem(
				"DigitalSignature",
				Categories.Documents,
				ResString.GetMultilingualString("ce0c8f79-1513-4e94-a2ca-c25c3d3d068b", "Add Digital Signature"),
				ResString.GetMultilingualString("711dc20c-1113-482e-a013-54bb89821e6f", "Enables Signing of PDF Documents."),
				new DigitalSignatureRegistry());
		});

		#region EnableFormSupportforDocumentDeliveryCompletionActions

		public BooleanRegistryItem EnableFormSupportforDocumentDeliveryCompletionActions
		{
			get
			{
				return GetItem("EnableFormSupportforDocumentDeliveryCompletionActions", delegate
				{
					return new BooleanRegistryItem(
						"EnableFormSupportforDocumentDeliveryCompletionActions",
						Categories.Documents,
						ResString.GetMultilingualString("FD4C99C6-EEDB-411E-9E7C-9C8B3C814418", "Enable Form Support for Document Delivery Completion Actions"),
						ResString.GetMultilingualString("FC433817-83CA-479B-A5F0-12B6B4DF5FE6", "Set this to true to allow forms to be selected for the EDC and DOC document delivery types"),
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport,
						false);
				});
			}
		}

		#endregion

		#region Use Staff Login To Generates Reports

		public BooleanRegistryItem EnforceDataAccessOnReport
		{
			get
			{
				return GetItem("EnforceDataAccessOnReport", delegate
				{
					return new BooleanRegistryItem("EnforceDataAccessOnReport",
						Categories.Documents,
						ResString.GetMultilingualString("c2ad5a05-6a5c-41d7-8051-b3165d2c4dda", "Enforce Data Access on Report"),
						ResString.GetMultilingualString("25729a44-f12f-4c0e-a7f2-8b5842c2ad9b", "When this flag is enabled, reports will run under the staff database user of their respective print user. When disabled, all reports run with a standard application login."),
						RegistryStorageFlags.System,
						false);
				});
			}
		}

		#endregion

		#region Document Signing Service

		public BooleanRegistryItem EnableDocumentSigningService
		{
			get
			{
				return GetItem("EnableDocumentSigningService", delegate
				{
					return new BooleanRegistryItem(
						new RegistryItemImplWithDynamicDefaultValue(
						"EnableDocumentSigningService",
						new[] { Categories.Documents_DocumentSigningService },
						(NoResString)"Enable Document Signing Service",
						(NoResString)"When this registry is enabled, the document signing service tabs, controls, and fields will be made available to the users of the company that has been enabled. Otherwise, the functionality is only available to CW1 Support users.",
						new BooleanRegistryDataType(),
						new BooleanRegistryEditorInfo(),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						EnableDocumentSigningServiceDefaultValueGetter));
				});
			}
		}

		object EnableDocumentSigningServiceDefaultValueGetter(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var countryCode = GetCountryCode(companyPK, branchPK);
			var enableDocumentSigningServiceCountryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(countryCode) as IEnableDocumentSigningServiceProvider;
			if (enableDocumentSigningServiceCountryFactory != null)
			{
				return enableDocumentSigningServiceCountryFactory.IsEnableDocumentSigningService();
			}
			return false;
		}

		public StringRegistryItem CloudSigningServiceProviderAPIEndpoint
		{
			get
			{
				return GetItem("CloudSigningServiceProviderAPIEndpoint", delegate
				{
					return new StringRegistryItem(
						new RegistryItemImplWithDynamicDefaultValue(
						"CloudSigningServiceProviderAPIEndpoint",
						new[] { Categories.Documents_DocumentSigningService },
						(NoResString)"Cloud Signing Service Provider API Endpoint",
						(NoResString)"Determines the URL used for the external document signing service provider API.",
						new UriRegistryDataType(Uri.UriSchemeHttps),
						new TextRegistryEditorInfo(TextEditorType.Url),
						RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						CloudSigningServiceProviderAPIEndpointDefaultValueGetter));
				});
			}
		}

		object CloudSigningServiceProviderAPIEndpointDefaultValueGetter(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var countryCode = GetCountryCode(companyPK, branchPK);
			var cloudSigningServiceProviderAPIEndpointCountryFactory = ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(countryCode) as ICloudSigningServiceProviderAPIEndpointProvider;
			if (cloudSigningServiceProviderAPIEndpointCountryFactory != null)
			{
				return cloudSigningServiceProviderAPIEndpointCountryFactory.GetCloudSigningServiceProviderAPIEndpoint();
			}
			return string.Empty;
		}

		static string GetCountryCode(Guid companyPK, Guid branchPK)
		{
			var company = companyPK == EnvProxy.Instance.CurrentCompany.PK ?
							EnvProxy.Instance.CurrentCompany : (ICompany)new BusinessObjectFactory().Load<IGlbCompany>(companyPK);

			if (company == null)
			{
				var branch = new BusinessObjectFactory().Load<IGlbBranch>(branchPK);
				company = (ICompany)branch?.Company;
			}

			return company?.Country?.Code ?? string.Empty;
		}

		public DocumentSigningServiceCredentialsWithProviderRegistryItem DocumentSigningServiceCredentials
			=> GetItem("SigningServiceProviderAPICredentials", () =>
			{
				var defaultValue = new DocumentSigningServiceCredentialsWithProviderConfiguration();

				return new DocumentSigningServiceCredentialsWithProviderRegistryItem(
						"SigningServiceProviderAPICredentials",
						Categories.Documents_DocumentSigningService,
						ResString.GetMultilingualString("8C76D2EF-A504-4F9B-93B3-85B5E84C8764", "Signing Service Provider API Credentials"),
						ResString.GetMultilingualString("0F5DCEBE-3DDB-4C3A-81A6-427E14D41BB0", "This registry defines the credentials required for accessing the document signing service provider resources."),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						defaultValue);
			});

		public DocumentSigningServicePartnerCredentialsRegistryItem DocumentSigningServicePartnerCredentials
			=> GetItem("DocumentSigningServicePartnerCredentials", () =>
			{
				var defaultValue = new DocumentSigningServicePartnerCredentials();
				if (Env.Instance.IsProductionSystem)
				{
					defaultValue.PartnerID = "9815455884";
					defaultValue.PartnerAccessKey = "0b9ec0fa756dcf02d6512f08459a0be5612e820d8847af25f24f8b4711c771ab";
				}
				else
				{
					defaultValue.PartnerID = "6340408123";
					defaultValue.PartnerAccessKey = "96ea58b42b7ae1f2796f0e1344ee159d8eb793a21c17f9203b496d77a1fe3129";
				}

				return new DocumentSigningServicePartnerCredentialsRegistryItem(
								"DocumentSigningServicePartnerCredentials",
								Categories.Documents_DocumentSigningService,
								(NoResString)"Signing Service Provider Partner Credentials",
								(NoResString)"Used to store the Cloud Signing Service Provider's Partner ID.",
								RegistryStorageFlags.System,
								RegistryOptions.IsOnlyForSupport, defaultValue);
			});

		public DocumentSigningServiceCredentialsRegistryItem DocumentSigningServicePartnerCredentialsAccessToken
			=> GetItem("DocumentSigningServicePartnerCredentialsAccessToken", () =>
			{
				var defaultValue = new DocumentSigningServiceCredentialsConfiguration();

				if (Env.Instance.IsProductionSystem)
				{
					//TODO: insert *real* Production credentials
					defaultValue.ClientID = (NoResString)"wisetechglobal";
					defaultValue.AccessKey = "42lpvdn6gzzcg8vquvtvcwckya";
				}
				else
				{
					defaultValue.ClientID = (NoResString)"wisetechglobal";
					defaultValue.AccessKey = "42lpvdn6gzzcg8vquvtvcwckya";
				}

				return new DocumentSigningServiceCredentialsRegistryItem(
					"DocumentSigningServicePartnerCredentialsAccessToken",
					Categories.Documents_DocumentSigningService_Portugal,
					(NoResString)"Partner Credentials and Access Token",
					(NoResString)"Used to store the Cloud Signing Service Provider's credentials.",
					RegistryStorageFlags.System,
					RegistryOptions.IsOnlyForSupport,
					defaultValue,
					accessKeyCaption: (NoResString)"Client Secret",
					keyIDCaption: (NoResString)"Access Token");
			});

		public DateTimeRegistryItem DocumentSigningServiceCertificateExpiryDate
		{
			get
			{
				return GetItem("DocumentSigningServiceCertificateExpiryDate", delegate
				{
					return new DateTimeRegistryItem(
						"DocumentSigningServiceCertificateExpiryDate",
						Categories.Documents_DocumentSigningService,
						ResString.GetMultilingualString("D0693E8F-DAAB-4BAE-84E9-3CC427FE639E", "Document Signing Service Expiry Date"),
						ResString.GetMultilingualString("FD62E11A-EB70-4786-9251-763E379C6F82", "This registry manually stores the date the current document signing digital signature certificate expires."),
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long),
						RegistryStorageFlags.Company | RegistryStorageFlags.Branch,
						RegistryOptions.IsOnlyForController,
						DateTime.MinValue,
						false
					);
				});
			}
		}

		public DateTimeRegistryItem DocumentSigningServiceLastCertificateExpiryNotificationDate
		{
			get
			{
				return GetItem("DocumentSigningServiceLastCertificateExpiryNotificationDate", delegate
				{
					return new DateTimeRegistryItem(
						"DocumentSigningServiceLastCertificateExpiryNotificationDate",
						Categories.Documents_DocumentSigningService,
						(NoResString)"Last Document Signing Service Expiry Notification Date",
						(NoResString)"This registry stores the last time a notification email was sent regarding the expiring of the document signing digital signature certificate.",
						new DateTimeRegistryEditorInfo(ZDateTimePickerFormat.Long),
						RegistryStorageFlags.Branch,
						RegistryOptions.IsHidden,
						DateTime.MinValue,
						false
					);
				});
			}
		}

		public IntRegistryItem SigningServiceProviderBatching
		{
			get
			{
				return GetItem("SigningServiceProviderBatching", delegate
				{
					return new IntRegistryItem(
						"SigningServiceProviderBatching",
						Categories.Documents_DocumentSigningService,
						ResString.GetMultilingualString("806464E6-63E5-4D7F-B451-E055E9F1FE16", "Signing Service Provider Batching"),
						ResString.GetMultilingualString("5E5B7C60-DE0F-4BAD-9715-E0C96E51226D", "This registry defines the batch size of document hashes to be included in a single signing transaction for countries and remote signing service providers that support batching. Set this registry to a number greater than zero to override the default batch number."),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						(EnvProxy.IsHostedWithCargowise ? RegistryOptions.IsOnlyForSupport : RegistryOptions.IsOnlyForController),
						0, 0, 10);
				});
			}
		}

		public IntRegistryItem SigningServiceRetriesInterval
		{
			get
			{
				return GetItem("SigningServiceRetriesInterval", delegate
				{
					return new IntRegistryItem(
						"SigningServiceRetriesInterval",
						Categories.Documents_DocumentSigningService,
						(NoResString)"Signing Service Retries Interval",
						(NoResString)"This setting is used to determine the minimum wait time (in minutes) between retries. The DOS service task will retry up to a maximum of three attempts before failing.",
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						RegistryOptions.IsOnlyForSupport,
						10, 0, 60);
				});
			}
		}

		public StringRegistryItem OverrideSignedByUsername
		{
			get
			{
				return GetItem("OverrideSignedByUsername", delegate
				{
					return new StringRegistryItem(
						"OverrideSignedByUsername",
						Categories.Documents_DocumentSigningService,
						ResString.GetMultilingualString("F33F5B2E-1477-449F-AC2B-2D4A228F3614", "Override Signed By Username"),
						ResString.GetMultilingualString("16236937-0254-44C9-9404-9B593A263601", "When set, this name will be used instead of the currently logged in user name displayed on the visible signature."),
						new StringRegistryDataType(2, 57),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						null);
				});
			}
		}

		public StringRegistryItem SigningServiceSignedByLabel
		{
			get
			{
				return GetItem("SigningServiceSignedByLabel", delegate
				{
					return new StringRegistryItem(
						"SigningServiceSignedByLabel",
						Categories.Documents_DocumentSigningService,
						ResString.GetMultilingualString("92C20422-285A-4CB4-AC42-A1F7109DE5C2", "Signing Service Signed By Label"),
						ResString.GetMultilingualString("EC036A49-042F-4DB4-9C57-A82C9EE132AB", "The name of the signing entity displayed on the visible signature and incorporated into the digital signature."),
						new StringRegistryDataTypeWithExcludedCharactersDataType(3, 80, new[] { '.' }),
						RegistryStorageFlags.System | RegistryStorageFlags.Company,
						BrandingFactory.Instance.ProductName);
				});
			}
		}

		public GuidRegistryItem DocumentSigningNotificationGroup
		{
			get
			{
				return GetItem("DocumentSigningNotificationGroup", delegate
				{
					GuidRegistryItem result = new GuidRegistryItem(
						"DocumentSigningNotificationGroup",
						Categories.Documents_DocumentSigningService,
						ResString.GetMultilingualString("5259707D-B3BD-4784-B6ED-C8B73C22793B", "Document Signing Notification Group"),
						ResString.GetMultilingualString("17995DD8-B516-41D7-BBF7-77A5D724725D", "The staff group that will be notified about errors in the Document Signing Service."),
						RegistryStorageFlags.Company);
					result.EditorInfo = new GuidFindBoxRegistryEditorInfo(RegistryFindBoxCollection.GlbGroup);
					return result;
				});
			}
		}

		public DocumentsAllowedForSigningRegistryItem DocumentsAllowedForSigning
		{
			get
			{
				return GetItem("DocumentsAllowedForSigning", delegate
				{
					return new DocumentsAllowedForSigningRegistryItem(
						"DocumentsAllowedForSigning",
						Categories.Documents_DocumentSigningService,
						(NoResString)"Documents Allowed For Signing",
						(NoResString)@"This list contains the document types which have been enabled for remote document signing. If document types are not in this list, they will be blocked from remote signing.

Note: If the logged-in user is a CWSupport user, ignore this rule and allow any document type to be saved and sent for remote document signing using DOS service task.",
						RegistryStorageFlags.System,
						RegistryOptions.IsOnlyForSupport);
				});
			}
		}

		public ColorPairRegistryItem VisibleSignatureFontAndBackColor
		{
			get
			{
				return GetItem("VisibleSignatureFontAndBackColor", () => new ColorPairRegistryItem(
					"VisibleSignatureFontAndBackColor",
					Categories.Documents_DocumentSigningService,
					ResString.GetMultilingualString("fb2d7808-0da9-4254-9109-4b2b5fd1d7ff",
						"Visible Signature Font And Background Colors"),
					ResString.GetMultilingualString("e4df0c04-7e56-424b-89ac-104a889123e3",
						"Overriding this setting will change the default font color and background color on the visible digital signature on signed documents. The background color is used if there is no background image set in the 'Signature Background Image' registry setting."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					new ColorPairSelector()));
			}
		}

		public ImageRegistryItem SignatureBackgroundImage
		{
			get
			{
				const int maxHeight = 100;
				const int minHeight = 100;
				const int maxWidth = 700;
				const int minWidth = 150;

				return GetItem("SignatureBackgroundImage", () => new ImageRegistryItem(
					"SignatureBackgroundImage",
					Categories.Documents_DocumentSigningService,
					ResString.GetMultilingualString("f91cdf2e-6512-4d47-8c62-c060ac19f303",
						"Signature Background Image"),
					ResString.GetMultilingualString("74a772a7-2a09-473f-a4d6-b55d66d61ea1",
						"The image used as background of the visible signature. Minimum size {0}x{1} (Width x Height), maximum size {2}x{3} (Width x Height).",
						minWidth, minHeight, maxWidth, maxHeight),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.Default,
					null) { DataType = new ImageRegistryDataType(maxWidth, maxHeight, minWidth, minHeight) } );
			}
		}

		public CodePairRegistryItem SignatureImagePositioningAnchor
		{
			get
			{
				return GetItem("SignatureImagePositioningAnchor", () => new CodePairRegistryItem(
					"SignatureImagePositioningAnchor",
					Categories.Documents_DocumentSigningService_SignatureImagePositioning,
					ResString.GetMultilingualString("43688f31-0530-4df6-9731-7467b3004576",
						"Anchor"),
					ResString.GetMultilingualString("b5241c90-e4a0-4c71-85dd-1c07f3f483c1",
						"The corner to use as a reference for positioning the signature image."),
					SignatureImagePositioningAnchorsPairListProvider,
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					DocumentSigningRegistryConstants.SignatureImagePositioningAnchors.BottomLeft));
			}
		}

		ICodeDescriptionPairListProvider SignatureImagePositioningAnchorsPairListProvider
		{
			get
			{
				return fSignatureImagePositioningAnchorsPairListProvider ??
					   (fSignatureImagePositioningAnchorsPairListProvider = new CodeDescriptionPairListProvider(() =>
					   {
						   var list = new CodeDescriptionPairList();
						   list.AddPair(DocumentSigningRegistryConstants.SignatureImagePositioningAnchors.BottomLeft,
							   ResString.GetMultilingualString("227f70aa-4949-4f2e-a689-9f1c2891f20d",
								   "Bottom Left Corner"));
						   list.AddPair(DocumentSigningRegistryConstants.SignatureImagePositioningAnchors.TopLeft,
							   ResString.GetMultilingualString("dda851c6-9604-43e3-98b8-5c12be3dd45b",
								   "Top Left Corner"));
						   list.AddPair(DocumentSigningRegistryConstants.SignatureImagePositioningAnchors.BottomRight,
							   ResString.GetMultilingualString("c068c670-121b-42d3-8a7b-2a12c9ec2a8d",
								   "Bottom Right Corner"));
						   list.AddPair(DocumentSigningRegistryConstants.SignatureImagePositioningAnchors.TopRight,
							   ResString.GetMultilingualString("aa88cc58-717f-4858-a42f-216d17b27b6f",
								   "Top Right Corner"));
						   return list;
					   }));
			}
		}
		ICodeDescriptionPairListProvider fSignatureImagePositioningAnchorsPairListProvider;

		public CodeDescriptionPairList SignatureImagePositioningAnchorsPairList
			=> SignatureImagePositioningAnchorsPairListProvider.CodeDescriptionPairList;

		public IntRegistryItem SignatureImagePositioningHorizontalMargin
		{
			get
			{
				return GetItem("SignatureImagePositioningHorizontalMargin", () => new IntRegistryItem(
					"SignatureImagePositioningHorizontalMargin",
					Categories.Documents_DocumentSigningService_SignatureImagePositioning,
					ResString.GetMultilingualString("c61f3f44-38e5-4567-9917-8d748359648a",
						"Horizontal Margin"),
					ResString.GetMultilingualString("d6199764-a421-4fc1-badc-6881217f9fac",
						"Specifies the horizontal positioning (in millimeters) of the signature image relative to the anchor point. If you need to set this to a value over 200, consider changing the anchor point."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.Default,
					3, 0, 200));
			}
		}

		public IntRegistryItem SignatureImagePositioningVerticalMargin
		{
			get
			{
				return GetItem("SignatureImagePositioningVerticalMargin", () => new IntRegistryItem(
					"SignatureImagePositioningVerticalMargin",
					Categories.Documents_DocumentSigningService_SignatureImagePositioning,
					ResString.GetMultilingualString("195a1989-7194-448c-81d7-4e7ae83f00f6",
						"Vertical Margin"),
					ResString.GetMultilingualString("80011fff-3d18-4cf6-a483-909bf722a5ac",
						"Specifies the vertical positioning (in millimeters) of the signature image relative to the anchor point. If you need to set this to a value over 200, consider changing the anchor point."),
					RegistryStorageFlags.System | RegistryStorageFlags.Company,
					RegistryOptions.Default,
					3, 0, 200));
			}
		}

		public IntRegistryItem DocumentSignaturePlaceholderSize
		{
			get
			{
				return GetItem("DocumentSignaturePlaceholderSize", () => new IntRegistryItem(
					"DocumentSignaturePlaceholderSize",
					Categories.Documents_DocumentSigningService,
					ResString.GetMultilingualString("EAA826DF-E14C-4078-A1A7-C33AEE6BF5D2",
						"Document Signature Placeholder Size"),
					ResString.GetMultilingualString("667519D9-7337-4C62-A507-BAA6107FB08A",
						@"This setting specifies the initial memory allocation for digital signatures in your documents, measured in kilobytes (KB).

Default Size: We start with a pre-set size of 16 KB to accommodate typical signatures. This size is a balance between ensuring enough space for most signatures and maintaining a compact document size.
Dynamic Adjustment: If a signature exceeds this initial allocation, the system will automatically expand the memory space to fit it. This ensures that all documents, even those with unexpectedly large signatures, are processed successfully without any manual intervention.
Impact on Document Size: Please note that increasing this value might result in larger overall document sizes. We recommend leaving it at the default unless you frequently encounter issues with signatures not fitting the allocated space.
Tips for Optimal Use: If you're unsure about adjusting this setting, keep it at the default. It's optimized for general use. Adjust only if you have specific needs for larger signatures."),
					RegistryStorageFlags.System,
					RegistryOptions.Default,
					16, 16, 128));
			}
		}

		#endregion
	}
}
