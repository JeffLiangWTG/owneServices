using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ResourceStrings.Business;
using Enterprise.ResourceStrings.Business.Analysis;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	sealed class DocBuilderTemplateTranslationHelperTest : TestCaseWithFactory
	{
		public void TestGetTranslatableLabels()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=GenericFreightJob]
{A}-[Name=System Document Element Template]
{A}-[DataContext=GenericFreightJob]
{A}-[EmailSubject=<ReportName> - <JobNumber>]

{A}-[#ConfigurableSection:GEN, My Section]
{B}-[<DocumentTitle>]
{B}-[Page <Current Page> of <TotalPages>]
{B}-[INVOICE DATE]    {C}-[<ARInvoice.InvoiceDate>]
{B}-[<ShipmentTransportMode.Description> <ReportNameShort>]

{A}-[#EndOfReport]");

			var docBuilderTemplateTranslationHelper = new DocBuilderTemplateTranslationHelper();
			var labels = docBuilderTemplateTranslationHelper.GetUniqueLabels(Factory, template.SO_ExcelTemplatePath, template.SO_Template);

			AssertNotNull(Array.Find(labels, label => label.Data.Caption == "Page {0} of {1}"));
			AssertNotNull(Array.Find(labels, label => label.Data.Caption == "INVOICE DATE"));
			AssertContains("My Section", Array.Find(labels, label => label.Data.Caption == "{0} {1}").Data.Key);
		}

		#region TestGetAllResStringsUsedByBizOFields

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetAllResStringsUsedByBizOFields()
		{
			var docBuilderTemplateTranslationHelper = new DocBuilderTemplateTranslationHelper();
			var results = docBuilderTemplateTranslationHelper.GetAllResStringsUsedByBizOFields(Factory);
			AssertAllStringsAreWellFormed(results);
			AssertContainsAllBizOFieldStringsFromOldLanguageTemplate(results);
		}

		public void TestGetAllResStringsUsedByBizOFields_ExcludedMacros()
		{
			ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=GenericFreightJob]
{A}-[Name=System Document Element Template]
{A}-[#ConfigurableSection:GEN, Excluded Macros]
{B}-[<TranslateDBField(StmMenuItem.SU_MenuName, English Text)>]
{A}-[#EndOfReport]");

			var docBuilderTemplateTranslationHelper = new DocBuilderTemplateTranslationHelper();
			var results = docBuilderTemplateTranslationHelper.GetAllResStringsUsedByBizOFields(Factory);
			AssertAllStringsAreWellFormed(results);
		}

		void AssertAllStringsAreWellFormed(Dictionary<string, DataWithDocumentMacroUsage<CodeStringFinder.ResourceStringReference[]>> results)
		{
			CombineAssertions(delegate
			{
				foreach (var array in results.Values)
				{
					foreach (var reference in array.Data)
					{
						Assert(string.Format("Cannot detect simple key for string defined in {0} : {1} line {2}", reference.Context, reference.FileName, reference.Line), !string.IsNullOrEmpty(reference.Key));
						Assert(string.Format("Cannot detect simple default value for string {0} defined in {1} : {2} line {3}", reference.Key, reference.Context, reference.FileName, reference.Line), !string.IsNullOrEmpty(reference.Value));
					}
				}
			});
		}

		void AssertContainsAllBizOFieldStringsFromOldLanguageTemplate(Dictionary<string, DataWithDocumentMacroUsage<CodeStringFinder.ResourceStringReference[]>> results)
		{
			var readonlyResourceStrings = ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Res.DefaultLanguage);
			CombineAssertions(delegate
			{
				string line;
				using (var reader = new StringReader(oldLanguageTemplateBizOFieldContents))
				{
					string macro = null;
					while ((line = reader.ReadLine()) != null)
					{
						if (!string.IsNullOrWhiteSpace(line))
						{
							string[] parsed = line.Split('\t');
							if (!string.IsNullOrEmpty(parsed[0]))
							{
								macro = parsed[0];
							}
							else if (string.IsNullOrEmpty(macro))
							{
								throw new Exception("Cannot parse oldLanguageTemplateBizOFieldContents");
							}
							string value = parsed[1];
							Assert(string.Format("Missing macro '{0}'", macro), results.ContainsKey(macro));
							if (results.ContainsKey(macro))
							{
								var match = Array.Find(results[macro].Data, reference => reference.Value == value);
								HelpDataString matchingResourceString = null;
								if (match != null)
								{
									matchingResourceString = ResourceStringsFactory.Lookup(Res.DefaultLanguage, match.Key);
								}
								AssertNotNull(string.Format("Missing string '{0}' for macro '{1}'", value, macro), matchingResourceString);
							}
						}
					}
				}
			});
		}

		const string oldLanguageTemplateBizOFieldContents = @"	
ARInvoice.Copy	Copy
	
Consignee.TypeDescription	Consignee
	Ultimate Consignee
	
ARInvoice.InvoiceTypeReferenceNumberHeadingNoColon	Consol
	Load List
	Shipment
	Declaration
	Transport
	Invoice
	Docket
	
ARInvoice.RecipientTaxIDHeading	Client VAT ID No:
	
MasterBillHeading	Bill Of Lading
	Master Bill
	MAWB
	Ocean Bill Of Lading
	
ConsolRoutes[MostInteresting].Transport.ReferenceLabel	Flight / Date
	Vessel / Voyage / IMO(Lloyds)
	Road Reference
	Rail Reference
	
ShipmentRoutes.Transport.ReferenceLabel	Flight / Date
	Vessel / Voyage / IMO(Lloyds)
	Road Reference
	Rail Reference
	
ShipmentRoutes[MostInteresting].Transport.ReferenceLabel	Flight / Date
	Vessel / Voyage / IMO(Lloyds)
	Road Reference
	Rail Reference
	
ExportAgent.TypeDescription	Export Agent
	Export Broker
	
ImportAgent.TypeDescription	Import Agent
	Import Broker
	
HouseBillHeading	House Bill
	HAWB
	House Bill Of Lading
	Parcel Post Numbers
	
CartageInfo.JourneyOnePickUpHeading	PICKUP
	PICKUP FROM
	PICKUP EMPTY
	PICKUP FULL
	DELIVER TO
	DELIVER TO EMPTY
	DELIVER TO FULL
	
CartageInfo.JourneyOneDeliverToHeading	PICKUP
	PICKUP FROM
	PICKUP EMPTY
	PICKUP FULL
	DELIVER TO
	DELIVER TO EMPTY
	DELIVER TO FULL
	
CartageInfo.JourneyTwoPickUpHeading	PICKUP
	PICKUP FROM
	PICKUP EMPTY
	PICKUP FULL
	DELIVER TO
	DELIVER TO EMPTY
	DELIVER TO FULL
	
CartageInfo.JourneyTwoDeliverToHeading	PICKUP
	PICKUP FROM
	PICKUP EMPTY
	PICKUP FULL
	DELIVER TO
	DELIVER TO EMPTY
	DELIVER TO FULL
	
JobNumberHeading	Brokerage
	Shipment
	Load List
	Consol
	Order
	Quote No

ShipmentTransportMode.Description	Air
	Air Freight
	Sea Freight
	Road Freight
	Rail Freight

Services[First].ServiceDocumentTitle	Fumigation
	Quarantine Inspection
	Customs Hold
	Quarantine Unpack
	Tailgate
	Extra Inspection
	Survey
	Cleaning
	Washing
	Steam Cleaning
	FCL Free Storage
	FCL Underbond Storage
	Truck Wait Time (Penalty Type - TWT)
	Detention (Penalty Type - DET)

SignOff	Email:
	Work:
	Work Extension:
	Fax:
	Home:
	Mobile:

Rating.InvoiceTermsText	Cash on Delivery
	Payment in Advance
	{0} days from Inv. Date
	{0} days from EOM
	{0} days from EOP
	{0} days from shipment
	{0} months from inv. cycle
	From Date of Invoice
	From End of Month
	From End of Period
	From Date of Shipment
	Months From Invoice Cycle Date

ARInvoice.ReversalReason	Incorrect Data Entry
	Wrong Organization Code Used
	Incorrect Amounts
	Free Text

TransportBookings.BookingInstructions.InstructionType	Pickup
	Delivery
	Multi

GenericTransactionHeader.PaymentRequestText	IMMEDIATE PAYMENT REQUIRED:
	IMMEDIATE PAYMENT REQUESTED:

Rating.PageSets[ForwardingStandard].OriginDestinationAndFreightRates.RateLine.Description	Less than
	More than
	Up to
	{0} {1}
	{0} {1} to {2}
	{0} to {1} {2}
	{0} and above
	Base Rate
	Per Unit
	Day(s)
	Ounces
	Cubic Feet

Rating.PageSets[ShippingCompact].OriginDestinationAndFreightTableRows.Page.CFX.NameAndValue	Import - Air
	Import - Sea
	Export - Air
	Export - Sea

Rating.PageSets[ForwardingCompact].OriginDestinationAndFreightTableRows.SubRow.Split	Containerized
	Non-Containerized
	See Below

Rating.PageSets[ForwardingLandscapeComplexLoose].OriginDestinationAndChargeableTableRows.Row.Entries[1].TransitTime	Overnight
	Same Day
	1 Day
	{0} Days

RecipientNameAndAddress	The Accounts Payable Manager
	The Import Manager
	The Export Sea Freight Manager
	Local Client
	*** NO ORGANIZATION DETAILS FOUND ***

RecipientNameAndIntendedRecipientAddress()	ATTENTION: {0}

RegistryItem(Env.Registry.ShipmentRequestForMissingDocumentsClause)	We have not yet received documents for the shipment referenced herein. Please send the documents requested below, by E-mail attachment or Fax. If you have any problem that might delay the matter further, please contact the writer urgently. Otherwise, we look forward to receiving these documents as soon as possible. Without them, the completion of Customs formalities may not proceed and delivery will be delayed.

RegistryItem(DocumentsDataRegistry.Instance.ShipmentDelayAlertAlertText)	PLEASE NOTE, YOUR SHIPMENT HAS BEEN DELAYED, PLEASE SEE UPDATED SHIPMENT DETAILS BELOW

GenericTransactionHeader.DocumentTitle	Job Revenue Journal

RecipientSalutation	Dear user
	Dear {0}
	[m] Dear {0} {1}
	[m] Sir
	[f] Madam
	Sir/Madam
	
Format	Yes
	No

NumberToWords(NoOriginalBills)	One
	Two
	Three
	Four
	Five
	Six
	Seven
	Eight
	Nine
	Ten

WarehouseJob.WhoFinalised.Label	Finalized By
	
Containers.Type	Twenty foot flatrack
	Twenty foot platform
	Twenty foot reefer
	Forty foot general purpose

Containers.Commodities.CodeAndDescription	General
	AIRCRAFT
	AMMUNITION
	CEMENT
	CLOTHING
	FISH
	LIVE ANIMALS
	MEDICAL INSTRUMENTS
	PAPER
	PLASTICS
	RUBBER
	STEEL
	TOBACCO
	WHEAT

ARInvoice.LinesForInvoice.LineDescription	Origin Charges
	Freight Charges
	Insurance Charges
	Destination Charges

GenericTransactionHeader.OrganisationARAgreedPaymentMethod	Business Check
	Credit Card
	Bank Transfer
	Cash and/or Bank Check
	Debit Card

Rating.PageSets[ForwardingLandscapeComplexNonLoose].OriginDestinationAndContainerTableRows.SubRow.Columns[1].Heading	LCL Min.
	LCL per M3

DocumentOpeningText(ReportName, ShipmentTransportMode.Code)	We have no record of the return of the containers noted below, please note that Detention Charges will be applied for all containers not returned by their due date.

WarehouseJob.WorkOrderLevels1st	Level {0}

JobHeaderLocalClient.AssignedStaff.Relationship	Overall Representative

ConsolRoutes.Transport.Mode	Sea Freight

WarehouseJob.ReferencesExtended	Vehicle No
	Booking Party Reference
	Customs Approval Number

ServiceLevel.Description	Standard
	Direct
	Deferred
	Door to Door
	Transhipment

RequiredDocuments.MissingRequiredDocuments	Commercial Invoice
	Certificate of Origin
	Packing List
";
		#endregion

		#region TestReportNames

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportNames()
		{
			var query = new ZQuery(HelpDataStringSchema.HD_Language, Res.DefaultLanguage);
			query.AddToFilter(HelpDataStringSchema.HD_Code, SQLComparisonOperator.StartsWith, DocBuilderResourceStrings.ReportNameKeyPrefix);
			var reportNames = Array.ConvertAll(ResourceStringsFactory.Load(query), item => item.ToResourceStringData());
			AssertReportNameStringsUsedByReportNameMacros(reportNames);
			AssertContainsAllReportNamesFromOldLanguageTemplate(reportNames);
		}

		void AssertReportNameStringsUsedByReportNameMacros(ResourceStringData[] reportNames)
		{
			ResourceStringDemandedEventArgs lastResourceDemandedEvent;
			var eventHandler = new EventHandler<ResourceStringDemandedEventArgs>(delegate(object sender, ResourceStringDemandedEventArgs e)
				{
					lastResourceDemandedEvent = e;
				});
			Res.ResourceDemanded += eventHandler;
			try
			{
				foreach (var item in reportNames)
				{
					var excelTemplate = new ExcelTemplateForUnitTesting("TemplateWithTranslateTab.xls", TestFilesSubFolder.DocumentTestFiles);
					var dataSource = Factory.New<DummyBusinessObject>();
					var dataProviders = new DataProviderList(BODocDataProvider.Get(dataSource));

					using (var documentPack = new DocumentPack())
					using (var document = new Report(documentPack, excelTemplate, dataProviders, "Test", null, DocumentDirection.ANY, false))
					{
						((IReportForUnitTesting)document).fName = item.Caption;
						lastResourceDemandedEvent = null;
						new ReportName().GetReplacement("<ReportName>", document);
						AssertNotNull("lastResourceDemandedEvent", lastResourceDemandedEvent);
						AssertEquals(item.Key, lastResourceDemandedEvent.Key);
						lastResourceDemandedEvent = null;
						new ReportNameShort().GetReplacement("<ReportNameShort>", document);
						AssertNotNull("lastResourceDemandedEvent", lastResourceDemandedEvent);
						AssertEquals(item.Key, lastResourceDemandedEvent.Key);
					}
				}
			}
			finally
			{
				Res.ResourceDemanded -= eventHandler;
			}
		}

		void AssertContainsAllReportNamesFromOldLanguageTemplate(ResourceStringData[] reportNames)
		{
			CombineAssertions(delegate
			{
				string line;
				using (var reader = new StringReader(oldLanguageTemplateReportTitles))
				{
					while ((line = reader.ReadLine()) != null)
					{
						if (!string.IsNullOrWhiteSpace(line))
						{
							AssertNotNull(string.Format("Missing report name {0}", line), Array.Find(reportNames, item => item.Caption == line));
						}
					}
				}
			});
		}

		const string oldLanguageTemplateReportTitles = @"
Agent Profit Share Advice
Agents Instruction
Arrival Notice
Authorization For Payment
Authorization for Service
Booking Confirmation
Cargo Load List
Cargo Load List (Packed)
Cargo Load List (Unpacked)
Cartage Advice
Cartage Advice With Receipt
Charge Sheet
Co-Load Interim Receipt
Consol Cover Sheet
Consol Notes
Container Detention Charge Sheet
Container Detention Reminder
Cover Sheet
Declaration Notes
Delay Alert
Delivery Confirmation
Delivery Confirmation With Charges
Delivery Information
Delivery Order
Departure Notice
Dimensions Attachment
Disbursement Note
Dock Receipt
Documents Available Notice
EFT Request
Export Authority
Export Delay Alert
Forwarding Instruction
Interim Receipt
Invoice
Letter of Guarantee
Letter To Agent
Manifest
Multi-Container Cartage Advice
Pre-Alert
Proof Of Delivery
Request for Collect Charges
Request for Missing Documents
Request for Profit Share
Request for Service
Shipment Notes
Shipping Advice
Shipping Order
Time Slot Request
Transhipment Confirmation
Ultimate Consignee Notification
Ultimate Consignor Notification
Worksheet";

		#endregion

		public void TestTransformFormatMacro()
		{
			var docBuilderTemplateTranslationHelper = new DocBuilderTemplateTranslationHelper();
			AssertEquals("Rating.PageSets[ForwardingStandard].OriginDestinationAndFreightRates.Page.CFX.NameAndValue",
				docBuilderTemplateTranslationHelper.TransformFormatMacro("Rating.PageSets[ForwardingStandard].OriginDestinationAndFreightRates.Page.CFX.Format(\"{NameAndValue}\", NewLine)"));
		}

		[DeveloperOnlyTest]
		public void TestAllStringFieldsOfDocumentWrapperAreTranslatable()
		{
			var factory = new BusinessObjectFactory();
			var docBuilderTemplateTranslationHelper = new DocBuilderTemplateTranslationHelper();
			var nonTranslatableStringFields = new Dictionary<string, FunctionToValidate>();
			var errorBuilder = new ZStringBuilder();

			foreach (var report in docBuilderTemplateTranslationHelper.ForEachReport(factory))
			{
				using (report)
				{
					var macroCalledDelegate = new MacroTranslator.MacroCalledDelegate(delegate(string macroWithoutAngleBrackets, ValueProvider valueProvider)
						{
							macroWithoutAngleBrackets = docBuilderTemplateTranslationHelper.TransformFormatMacro(macroWithoutAngleBrackets);
							if (!nonTranslatableStringFields.ContainsKey(macroWithoutAngleBrackets))
							{
								if (valueProvider is DBOrBOValueProvider)
								{
									var methodInfoChain = ((BusinessObjectDataProvider)report.DataProvider).FindColumn(macroWithoutAngleBrackets);
									if (methodInfoChain != null && methodInfoChain.Length > 0)
									{
										var methodInfo = methodInfoChain[methodInfoChain.Length - 1].MethodInfo;
										var parentType = methodInfoChain[methodInfoChain.Length - 1].MethodInfo.DeclaringType;
										if (methodInfoChain.Length > 1 && !typeof(IBODocDataProviderCollectionHelper).IsAssignableFrom(parentType))
										{
											parentType = methodInfoChain[0].MethodInfo.ReturnType;
											if (typeof(IBODocDataProviderCollection).IsAssignableFrom(parentType))
											{
												parentType = methodInfoChain[methodInfoChain.Length - 1].MethodInfo.DeclaringType;
											}
											else
											{
												for (int i = 1; i < methodInfoChain.Length - 1; i++)
												{
													parentType = methodInfoChain[i].MethodInfo.ReturnType;
													if (typeof(IBODocDataProviderCollection).IsAssignableFrom(parentType))
													{
														parentType = methodInfoChain[methodInfoChain.Length - 1].MethodInfo.DeclaringType;
														break;
													}
												}
											}
										}

										if (parentType.Name == "DocumentWrapper" && methodInfo.Name == "ToString" && methodInfoChain.Length > 1)
										{
											parentType = methodInfoChain[methodInfoChain.Length - 2].MethodInfo.ReturnType;
											var defaultPropertyName = DefaultFieldAttribute.GetDefaultFieldName(parentType);
											if (!string.IsNullOrEmpty(defaultPropertyName))
											{
												var defaultProperty = parentType.GetProperty(defaultPropertyName);
												if (defaultProperty != null)
												{
													methodInfo = defaultProperty.GetGetMethod();
												}
											}
										}

										if (methodInfoChain.Length > 1 && (
												(parentType.Name == "CodeAndDescriptionWrapper" && (methodInfo.Name == "get_Description" || methodInfo.Name == "get_CodeAndDescription")) ||
												(parentType.Name == "LabelValuePairWrapper" && methodInfo.Name == "get_Label"))
										)
										{
											methodInfo = methodInfoChain[methodInfoChain.Length - 2].MethodInfo;
											parentType = methodInfoChain[methodInfoChain.Length - 2].MethodInfo.DeclaringType;
										}

										var assemblyName = parentType.Assembly.GetName().Name;
										if (IsStringReturnType(methodInfo.ReturnType) && assemblyName != "mscorlib" && !(parentType.Name == "DocumentWrapper" && methodInfo.Name == "ToString"))
										{
											var typeName = parentType.FullName;
											if (typeName.IndexOf('`') > -1)
											{
												typeName = typeName.Substring(0, typeName.IndexOf('`'));
											}
											nonTranslatableStringFields.Add(macroWithoutAngleBrackets, new FunctionToValidate(assemblyName, typeName, methodInfo.Name));
										}
									}
								}
							}
						}
					);

					try
					{
						report.MacroTranslator.MacroCalled += macroCalledDelegate;
						docBuilderTemplateTranslationHelper.ReplaceAllMacros(factory, report);
					}
					finally
					{
						report.MacroTranslator.MacroCalled -= macroCalledDelegate;
					}
				}
			}

			if (nonTranslatableStringFields.Count > 0)
			{
				var uniqueFunctions = new HashSet<string>();
				errorBuilder.AppendLine(
					"The following doc wrapper fields are not translatable, please check and see if fix is needed.");
				foreach (var item in nonTranslatableStringFields)
				{
					uniqueFunctions.Add($"Type Name: {item.Value.typeName}, function name: {item.Value.memberName} \r\n");
				}

				foreach (var item in uniqueFunctions)
				{
					errorBuilder.Append(item);
				}
				Fail(errorBuilder.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		bool IsStringReturnType(Type type)
		{
			return Array.IndexOf(StringReturnTypes, type.FullName) > -1 ||
				   (type.BaseType != null && IsStringReturnType(type.BaseType));
		}

		readonly string[] StringReturnTypes = new string[]
		{
			typeof(string).FullName,
			typeof(ZString).FullName
		};

		sealed class FunctionToValidate
		{
			public FunctionToValidate(string assemblyName, string typeName, string memberName)
			{
				this.assemblyName = assemblyName;
				this.typeName = typeName;
				this.memberName = memberName;
			}

			public readonly string assemblyName;
			public readonly string typeName;
			public readonly string memberName;
		}
	}
}
