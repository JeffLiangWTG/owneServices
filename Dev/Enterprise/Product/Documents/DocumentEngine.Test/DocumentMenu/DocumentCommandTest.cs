using System.Collections.Generic;
using System.Data;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	public class DocumentCommandTest : TestCaseWithFactory
	{
		public void TestDeliveryRestrictions()
		{
			var menuItem = Factory.NewWithValidTestData<DocumentCommand>();
			AssertEquals(0, menuItem.DeliveryRestrictions.Count);

			var restriction = Factory.NewWithValidTestData<StmMenuDeliveryRestriction>();
			restriction.SDR_SU = menuItem.PK;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var menuItemLoaded = newFactory.Load<DocumentCommand>(menuItem.PK);
			AssertEquals(1, menuItemLoaded.DeliveryRestrictions.Count);
		}

		public virtual void TestIsUserDefinedConditionMet()
		{
			var bizObject = Factory.New<DummyEnterpriseBusinessObject>();
			var intField = bizObject.Z0_Calculated;
			var stringField = bizObject.Z0_Code;
			var processor = ObjectFactory.Get<ITextMacroProcessor>();

			var documentCommand = Factory.NewWithValidTestData<DocumentCommand>();
			var type = typeof(DocumentCommand);
			var method = type.GetMethod("IsUserDefinedConditionMet", BindingFlags.Instance | BindingFlags.NonPublic);

			var macro1 = new ZString($"'<Z0_Calculated>' == '{intField}'");
			var macro2 = new ZString($"'<Z0_Code>' == '{stringField}'");

			Assert((bool)method.Invoke(documentCommand, new object[] { processor, bizObject, macro1 }));
			Assert((bool)method.Invoke(documentCommand, new object[] { processor, bizObject, macro2 }));
		}

		public virtual void TestGetDataStateBeforeRunPassMenuPK_UDF()
		{
			var parentBusinessObject = Factory.New<DummyDocumentDeliveryRestrictionBusinessObject>();
			parentBusinessObject.Z0_NVarChar = "JerryTest";
			parentBusinessObject.OriginCountryCodeForTest = "AU";
			parentBusinessObject.DestinationCountryCodeForTest = "MX";

			var supporter = new DummyBusinessObjectDocumentSupporter(parentBusinessObject);

			var documentCommand = Factory.NewWithValidTestData<DocumentCommand>();
			documentCommand.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.UDF);
			documentCommand.SU_DeliveryRestrictionMacro = "\"<Z0_NVarChar>\"==\"Test\"";
			documentCommand.SU_DeliveryRestrictionDescription = "ParentBusinessObject Z0_NVarChar should be <Z0_NVarChar>";

			NewDocumentCommandDeliveryRestriction(documentCommand, true, "MX", "", DeliveryRestrictionType.NON, "", "restriction1 OriginCountryCode should be AU");
			NewDocumentCommandDeliveryRestriction(documentCommand, true, "", "AU", DeliveryRestrictionType.NON, "", "restriction2 DestinationCountryCode should be MX");
			NewDocumentCommandDeliveryRestriction(documentCommand, true, "", "", DeliveryRestrictionType.UDF, "\"<Z0_NVarChar>\"==\"Test\"", "restriction3 Z0_NVarChar should be <Z0_NVarChar>");
			NewDocumentCommandDeliveryRestriction(documentCommand, true, "AU", "MX", DeliveryRestrictionType.UDF, "\"<Z0_NVarChar>\"==\"Test\"", "restriction4 Z0_NVarChar should be <Z0_NVarChar>");

			var dataState = supporter.GetDataStateBeforeRun(documentCommand);
			var expectedMessage = @"User defined delivery restriction condition is not met. 
ParentBusinessObject Z0_NVarChar should be JerryTest
restriction1 OriginCountryCode should be AU
restriction2 DestinationCountryCode should be MX
restriction3 Z0_NVarChar should be JerryTest
restriction4 Z0_NVarChar should be JerryTest
";
			AssertEquals(false, dataState.IsValid);
			AssertEquals(expectedMessage, dataState.ErrorMessage);

			documentCommand.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.NON);

			dataState = supporter.GetDataStateBeforeRun(documentCommand);
			expectedMessage = @"User defined delivery restriction condition is not met. 
restriction1 OriginCountryCode should be AU
restriction2 DestinationCountryCode should be MX
restriction3 Z0_NVarChar should be JerryTest
restriction4 Z0_NVarChar should be JerryTest
";
			AssertEquals(false, dataState.IsValid);
			AssertEquals(expectedMessage, dataState.ErrorMessage);
		}

		public void TestDeliveryRestrictionsForStmMenu()
		{
			var parentBusinessObject = Factory.New<DummyDocumentDeliveryRestrictionBusinessObject>();
			parentBusinessObject.Z0_NVarChar = "HankTest";

			var supporter = new DummyBusinessObjectDocumentSupporter(parentBusinessObject);

			var stmMenuItem = Factory.NewWithValidTestData<StmMenuItem>();
			stmMenuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.UDF);
			stmMenuItem.SU_DeliveryRestrictionMacro = "\"<Z0_NVarChar>\"==\"Test\"";
			stmMenuItem.SU_DeliveryRestrictionDescription = "ParentBusinessObject Z0_NVarChar should be <Z0_NVarChar>";

			var dataState = supporter.GetDataStateBeforeRun(stmMenuItem);

			var expectedMessage = @"User defined delivery restriction condition is not met. 
ParentBusinessObject Z0_NVarChar should be HankTest
";

			AssertEquals(false, dataState.IsValid);
			AssertEquals(expectedMessage, dataState.ErrorMessage);
		}

		DocumentCommandDeliveryRestriction NewDocumentCommandDeliveryRestriction(DocumentCommand documentCommand, bool isActive, string originCountryCode, string destinationCountryCode, DeliveryRestrictionType restrictionType, string restrictionMacro, string restrictionDescription)
		{
			var documentCommandDeliveryRestriction = Factory.NewWithValidTestData<DocumentCommandDeliveryRestriction>();
			documentCommandDeliveryRestriction.SDR_IsActive = isActive;
			documentCommandDeliveryRestriction.SDR_RN_NKOriginCountryCode = originCountryCode;
			documentCommandDeliveryRestriction.SDR_RN_NKDestinationCountryCode = destinationCountryCode;
			documentCommandDeliveryRestriction.SDR_DeliveryRestrictionType = restrictionType.ToString();
			documentCommandDeliveryRestriction.SDR_DeliveryRestrictionMacro = restrictionMacro;
			documentCommandDeliveryRestriction.SDR_DeliveryRestrictionDescription = restrictionDescription;
			documentCommandDeliveryRestriction.SDR_SU = documentCommand.PK;

			return documentCommandDeliveryRestriction;
		}

		public void TestDeleteRelatedDeliveryRestrictions()
		{
			var documentCommand = Factory.NewWithValidTestData<DocumentCommand>();
			AssertEquals(0, documentCommand.DeliveryRestrictions.Count);

			var documentCommandDeliveryRestriction1 = Factory.NewWithValidTestData<DocumentCommandDeliveryRestriction>();
			var documentCommandDeliveryRestriction2 = Factory.NewWithValidTestData<DocumentCommandDeliveryRestriction>();
			var documentCommandDeliveryRestriction3 = Factory.NewWithValidTestData<DocumentCommandDeliveryRestriction>();

			documentCommandDeliveryRestriction1.SDR_SU = documentCommand.PK;
			documentCommandDeliveryRestriction2.SDR_SU = documentCommand.PK;
			documentCommandDeliveryRestriction3.SDR_SU = documentCommand.PK;

			AssertEquals(3, documentCommand.DeliveryRestrictions.Count);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var documentCommandLoaded = newFactory.Load<DocumentCommand>(documentCommand.PK);

			AssertEquals(3, documentCommandLoaded.DeliveryRestrictions.Count);

			documentCommandLoaded.Delete();

			AssertEquals(0, documentCommandLoaded.DeliveryRestrictions.Count);
		}

		public void TestGetDeliveryRestrictionErrorMessage()
		{
			var dummyManager = new DummyDocumentDeliveryCreditControlManager();
			using (var manager = ObjectFactory.Substitute<IDocumentDeliveryCreditControlManager>(dummyManager))
			{
				var parentBusinessObject = Factory.New<DummyDocumentDeliveryRestrictionBusinessObject>();
				parentBusinessObject.OriginCountryCodeForTest = "AU";
				var documentCommand = Factory.NewWithValidTestData<DocumentCommand>();
				var deliveryCancelledMessage = "Document Delivery canceled.";
				var deliveryRestrictionNotMeet = "User defined delivery restriction condition is not met.";
				documentCommand.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.UDF);
				documentCommand.SU_DeliveryRestrictionMacro = "1!=1";
				AssertContains(deliveryRestrictionNotMeet, ((IDocumentCommand)documentCommand).GetDeliveryRestrictionErrorMessage(parentBusinessObject, parentBusinessObject));

				documentCommand.SU_DeliveryRestrictionMacro = "1==1";
				AssertEquals(string.Empty, ((IDocumentCommand)documentCommand).GetDeliveryRestrictionErrorMessage(parentBusinessObject, parentBusinessObject));

				documentCommand.SU_DeliveryRestrictionMacro = "\"<ComplianceRiskStatus.CommodityRisk>\"==\"PRS\"";
				AssertEquals(deliveryCancelledMessage, ((IDocumentCommand)documentCommand).GetDeliveryRestrictionErrorMessage(parentBusinessObject, parentBusinessObject));

				// Test for case insensitivity
				documentCommand.SU_DeliveryRestrictionMacro = "\"<complianceriSkstatus.jobrisk>\"==\"CLR\"";
				AssertEquals(deliveryCancelledMessage, ((IDocumentCommand)documentCommand).GetDeliveryRestrictionErrorMessage(parentBusinessObject, parentBusinessObject));

				documentCommand.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.NON);
				AssertEquals(string.Empty, ((IDocumentCommand)documentCommand).GetDeliveryRestrictionErrorMessage(parentBusinessObject, parentBusinessObject));

				var documentCommandDeliveryRestriction1 = documentCommand.DeliveryRestrictions.AddNew();
				documentCommandDeliveryRestriction1.SDR_DeliveryRestrictionType = "NON";
				documentCommandDeliveryRestriction1.SDR_RN_NKOriginCountryCode = "AU";
				AssertEquals(string.Empty, ((IDocumentCommand)documentCommand).GetDeliveryRestrictionErrorMessage(parentBusinessObject, parentBusinessObject));

				documentCommandDeliveryRestriction1.SDR_DeliveryRestrictionType = "UDF";
				documentCommandDeliveryRestriction1.SDR_DeliveryRestrictionMacro = "1!=1";
				AssertContains(deliveryRestrictionNotMeet, ((IDocumentCommand)documentCommand).GetDeliveryRestrictionErrorMessage(parentBusinessObject, parentBusinessObject));

				documentCommandDeliveryRestriction1.SDR_DeliveryRestrictionMacro = "1==1";
				AssertEquals(string.Empty, ((IDocumentCommand)documentCommand).GetDeliveryRestrictionErrorMessage(parentBusinessObject, parentBusinessObject));

				documentCommandDeliveryRestriction1.SDR_DeliveryRestrictionMacro = "\"<ComplianceRiskStatus.CommodityRisk>\"==\"PRS\"";
				AssertEquals(deliveryCancelledMessage, ((IDocumentCommand)documentCommand).GetDeliveryRestrictionErrorMessage(parentBusinessObject, parentBusinessObject));

				var dummyBizo = Factory.New<DocDummyBusinessObject>();
				documentCommandDeliveryRestriction1.SDR_RN_NKOriginCountryCode = string.Empty;
				AssertEquals(deliveryCancelledMessage, ((IDocumentCommand)documentCommand).GetDeliveryRestrictionErrorMessage(dummyBizo, dummyBizo));

				dummyManager.ShouldStopDeliveryForTest = false;
				AssertNotEquals(deliveryCancelledMessage, ((IDocumentCommand)documentCommand).GetDeliveryRestrictionErrorMessage(dummyBizo, dummyBizo));
			}
		}

		class DummyDocumentDeliveryCreditControlManager : IDocumentDeliveryCreditControlManager
		{
			public ZString GetDocumentDeliveryStatusForCreditManagement(BusinessObject businessObject, string documentDescription, ZGuid menuPK, bool creditCheckEnabled = true, string documentDirection = "")
			{
				return "Test";
			}

			public bool ShouldStopDeliveryForTest { get; set; } = true;

			public bool ShouldStopDelivery(BusinessObject businessObject)
			{
				return ShouldStopDeliveryForTest;
			}
		}

		public void TestAttachmentTypes()
		{
			var documentCommand = Factory.New<DocumentCommand>();
			var attachmentTypes = documentCommand.AttachmentTypes;

			AssertEquals(8, attachmentTypes.Count);
			Assert("Attachment types should contain XLS.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xls));
			Assert("Attachment types should contain XLSX.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Xlsx));
			Assert("Attachment types should contain PDF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdf));
			Assert("Attachment types should contain PDF/A.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdfa));
			Assert("Attachment types should contain PDFC.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Pdfc));
			Assert("Attachment types should contain TIF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Tif));
			Assert("Attachment types should contain HTML.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Html));
			Assert("Attachment types should contain HTMF.", attachmentTypes.ContainsCode(AttachmentTypeList.Codes.Htmf));
		}

		public class DocDummyBusinessObject : DummyBusinessObject, IDocumentSupportable, IDocManagerSupport
		{
			public DocDummyBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public DocumentCommandCollection DocumentCommands
			{
				get
				{
					if (fDocumentCommands == null)
					{
						fDocumentCommands = new DocumentCommandCollection(this);
						fDocumentCommands.Load();
					}
					return fDocumentCommands;
				}
			}

			public DocumentCommandCollection DocumentCommandsWithForms
			{
				get
				{
					if (fDocumentCommandsWithForms == null)
					{
						fDocumentCommandsWithForms = new DocumentCommandCollection(this, true);
						fDocumentCommandsWithForms.Load();
					}
					return fDocumentCommandsWithForms;
				}
			}

			public override string TableName
			{
				get
				{
					if (string.IsNullOrEmpty(tableNameOverride))
					{
						return base.TableName;
					}
					else
					{
						return tableNameOverride;
					}
				}
			}

			public void SetTableNameOverride(string value)
			{
				tableNameOverride = value;
			}

			string tableNameOverride;
			DocumentCommandCollection fDocumentCommands;
			DocumentCommandCollection fDocumentCommandsWithForms;

			public bool ReturnNullForSupportedChildBusinessContext;

			#region IDocumentSupportable Members

			public virtual DocumentSupporter DocumentSupporter
			{
				get => documentSupporter ?? (documentSupporter = new DummyBusinessObjectDocumentSupporter(this));
			}

			DummyBusinessObjectDocumentSupporter documentSupporter;

			#endregion

			public class DummyWrapper : DocumentWrapper
			{
				public DummyWrapper()
					: base(null, new BusinessObjectFactory())
				{
				}

				public override string ToString()
				{
					return "";
				}
			}

			#region IDocManagerSupport Members

			DocManagerInfo IDocManagerSupport.DocManagerInfo
			{
				get
				{
					if (docManagerInfo == null)
					{
						docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.Shipment);
					}
					return docManagerInfo;
				}
			}
			DocManagerInfo docManagerInfo;

			#endregion
		}

		public class DummyBusinessObjectDocumentSupporter : DocumentSupporter
		{
			public DummyBusinessObjectDocumentSupporter(DocDummyBusinessObject docDummyBusinessObject)
				: base(docDummyBusinessObject)
			{
			}
			DocDummyBusinessObject DocDummyBusinessObject
			{
				get { return (DocDummyBusinessObject)BusinessObject; }
			}
			public override BusinessContext[] SupportedChildBusinessContexts
			{
				get
				{
					if (DocDummyBusinessObject.ReturnNullForSupportedChildBusinessContext)
					{
						return null;
					}
					else
					{
						return new BusinessContext[] { BusinessContext.Consol };
					}
				}
			}

			public override BusinessContext BusinessContext
			{
				get
				{
					return BusinessContext.Shipment;
				}
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				if (dataContext == Core.Constants.DataContext.Shipment)
				{
					return new DocumentWrapper[] { new DocDummyBusinessObject.DummyWrapper() };
				}
				else
				{
					return null;
				}
			}

			protected override Core.Constants.DataContext[] GetSupportedDataContexts()
			{
				return new Core.Constants.DataContext[] { Core.Constants.DataContext.Shipment, Core.Constants.DataContext.Consol };
			}

			protected override List<DataContextValue> GetSupportedBODataSources()
			{
				return supportedBODataSources;
			}

			public void AddSupportedBODataSource(string dataContextValue)
			{
				supportedBODataSources.Add(new DataContextValue(dataContextValue));
			}

			public void RemoveSupportedBODataSource(string dataContextValue)
			{
				supportedBODataSources.Remove(new DataContextValue(dataContextValue));
			}

			readonly List<DataContextValue> supportedBODataSources = new List<DataContextValue>();

			public override string GetFilterValue(DocumentFilters filterName)
			{
				var result = string.Empty;

				switch (filterName)
				{
					case DocumentFilters.CO:
						result = GlbCompany.CurrentCompany.GC_Code;
						break;

					default:
						result = base.GetFilterValue(filterName);
						break;
				}

				return result;
			}

			public override ISecurityCheckpoint CustomisationSecurityCheckpoint { get { return Env.Security.None; } }

			protected override ZString GetPDFPasswordCore(DeliverableInfo deliverableInfo) => "password";
		}

		public class DocDummyBusinessObjectWithSupporterThatSupportDocBuilderInvoiceAsChildCommand : DocDummyBusinessObject
		{
			public DocDummyBusinessObjectWithSupporterThatSupportDocBuilderInvoiceAsChildCommand(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region IDocumentSupportable Members

			public override DocumentSupporter DocumentSupporter
			{
				get { return new DummyBusinessObjectDocumentSupporterThatSupportDocBuilderInvoiceAsChildCommand(this); }
			}

			#endregion
		}

		public class DummyBusinessObjectDocumentSupporterThatSupportDocBuilderInvoiceAsChildCommand : DummyBusinessObjectDocumentSupporter
		{
			public DummyBusinessObjectDocumentSupporterThatSupportDocBuilderInvoiceAsChildCommand(DocDummyBusinessObject docDummyBusinessObject)
				: base(docDummyBusinessObject)
			{
			}

			public override bool SupportDocBuilderInvoiceAsChildCommand
			{
				get { return true; }
			}
		}

		class DummyDocumentDeliveryRestrictionBusinessObject : DocDummyBusinessObject, IOriginDestinationForDocumentDeliveryRestriction
		{
			public DummyDocumentDeliveryRestrictionBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public string OriginCountryCodeForTest { get; set; }
			public string DestinationCountryCodeForTest { get; set; }

			public string OriginCountryCode => OriginCountryCodeForTest;

			public string DestinationCountryCode => DestinationCountryCodeForTest;
		}

		public void TestIsApplicableWithMenuItemBeDataSource()
		{
			var command = Factory.NewWithValidTestData<DocumentCommand>();
			command.SU_MenuName = "Test Menu";

			command.SU_FilterList = "\"<SU_MenuName>\" != \"\"";
			Assert(command.IsApplicable);

			command.SU_FilterList = "\"<SU_MenuName>\" == \"WhatEver\"";
			Assert(!command.IsApplicable);
		}

		public void TestIsApplicableWithInvalidFilter()
		{
			var cachedValue = Globals.GetIsUnitTestingProductionFunctionality();
			try
			{
				Globals.SetIsUnitTestingProductionFunctionality(true);
				DocumentCommand command = Factory.New<DocumentCommand>();
				command.SU_BusinessContext = nameof(BusinessContext.Test);
				command.SU_IsPublished = true;
				command.SU_MenuName = "Test Menu Name";
				command.SU_MenuIndex = 0;
				command.SU_FilterList = "\"<CountryCode>\" = \"<CountryCode>\"";

				command.Parent = Factory.New<DocDummyBusinessObject>();

				command.SU_IsSystemDefined = false;
				AssertEquals("command.IsApplicable", false, command.IsApplicable);
				AssertEquals("ExceptionReporterTestListener.Instance.Count", 0, ExceptionReporterTestListener.Instance.Count);

				command.SU_IsSystemDefined = true;
				AssertEquals("command.IsApplicable", false, command.IsApplicable);
				AssertEquals("ExceptionReporterTestListener.Instance.Count", 1, ExceptionReporterTestListener.Instance.Count);
				string exceptionMessage = ExceptionReporterTestListener.Instance[0].Message;
				ExceptionReporterTestListener.Instance.Clear();
				AssertContains("BusinessContext=[Test], MenuName=[Test Menu Name], FilterList=[\"<CountryCode>\" = \"<CountryCode>\"]", exceptionMessage);

				command.SU_MenuPath = "Test Menu Path";
				AssertEquals("command.IsApplicable", false, command.IsApplicable);
				AssertEquals("ExceptionReporterTestListener.Instance.Count", 1, ExceptionReporterTestListener.Instance.Count);
				exceptionMessage = ExceptionReporterTestListener.Instance[0].Message;
				ExceptionReporterTestListener.Instance.Clear();
				AssertContains("BusinessContext=[Test], MenuName=[Test Menu Path/Test Menu Name], FilterList=[\"<CountryCode>\" = \"<CountryCode>\"]", exceptionMessage);

				command.SU_MenuPath = "Test Menu Path 2/";
				AssertEquals("command.IsApplicable", false, command.IsApplicable);
				AssertEquals("ExceptionReporterTestListener.Instance.Count", 1, ExceptionReporterTestListener.Instance.Count);
				exceptionMessage = ExceptionReporterTestListener.Instance[0].Message;
				ExceptionReporterTestListener.Instance.Clear();
				AssertContains("BusinessContext=[Test], MenuName=[Test Menu Path 2/Test Menu Name], FilterList=[\"<CountryCode>\" = \"<CountryCode>\"]", exceptionMessage);

				command.SU_FilterList = "\"<CountryCode>\" == \"<CountryCode>\"";
				AssertEquals("command.IsApplicable", true, command.IsApplicable);
				AssertEquals("ExceptionReporterTestListener.Instance.Count", 0, ExceptionReporterTestListener.Instance.Count);
			}
			finally
			{
				ErrorReporter.Clear();
				Globals.SetIsUnitTestingProductionFunctionality(cachedValue);
			}
		}

		public void TestIsApplicable_Document()
		{
			var parent = Factory.New<DocDummyBusinessObject>();
			parent.Z0_Code = "AAA";

			var command = Factory.New<DocumentCommand>();
			command.Parent = parent;
			command.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;
			command.SU_BusinessContext = nameof(BusinessContext.Test);
			command.SU_IsPublished = true;
			command.SU_MenuName = "Test Menu Name";
			command.SU_MenuIndex = 0;
			command.SU_FilterList = "\"<Z0_Code>\" == \"AAA\"";

			AssertEquals("Command is applicable as matches the filter", true, command.IsApplicable);

			parent.Z0_Code = "BBB";

			AssertEquals("Command is not applicable as doesn't match the filter", false, command.IsApplicable);
		}

		public void TestIsApplicable_Form_ForCommandParentWhichSupportsDocumentVisualizer()
		{
			var parent = (DummyBusinessObject)ObjectFactory.Get<IDocumentVisualizerTestHelper>().CreateBusinessObjectWithDocumentVisualiserSupport(Factory);
			parent.Z0_Code = "AAA";

			var command = Factory.New<DocumentCommand>();
			command.Parent = (IDocumentSupportable)parent;
			command.SU_MenuType = Core.Constants.StmMenuItemTypes.Forms;
			command.SU_BusinessContext = nameof(BusinessContext.Test);
			command.SU_IsPublished = true;
			command.SU_MenuName = "Test Menu Name";
			command.SU_MenuIndex = 0;
			command.SU_FilterList = "Z0_Code == \"AAA\"";

			AssertEquals("Command is applicable as matches the filter", true, command.IsApplicable);

			parent.Z0_Code = "BBB";

			AssertEquals("Command is not applicable as doesn't match the filter", false, command.IsApplicable);
		}

		public void TestIsApplicable_Form_ForCommandParentWhichDoesNotSupportDocumentVisualizer()
		{
			var parent = Factory.New<DocDummyBusinessObject>();
			parent.Z0_Code = "AAA";

			var command = Factory.New<DocumentCommand>();
			command.Parent = parent;
			command.SU_MenuType = Core.Constants.StmMenuItemTypes.Forms;
			command.SU_BusinessContext = nameof(BusinessContext.Test);
			command.SU_IsPublished = true;
			command.SU_MenuName = "Test Menu Name";
			command.SU_MenuIndex = 0;

			Assert("Command is non applicable as the command parent does not support Document Visualizer when filter is empty", !command.IsApplicable);

			command.SU_FilterList = "Z0_Code == \"AAA\"";
			Assert("Command is non applicable as the command parent does not support Document Visualizer even though it matches the filter", !command.IsApplicable);
		}

		public void TestCodeAndDescription()
		{
			DocumentCommand document = Factory.New<DocumentCommand>();
			document.SU_MenuName = "x";
			document.SU_BusinessContext = "y";
			document.SU_MenuPath = "z";
			document.SU_ContactType = "NCT";

			AssertEquals("Code", "y : x : z : NCT", CodePropertyAttribute.CodeFromBusinessObject(document));
			AssertEquals("Description", "y : x : z : NCT", DescriptionPropertyAttribute.DescriptionFromBusinessObject(document));
		}

		public void TestLoadDocumentCommandCorrectly_MoreThanOneDocumentWithSameName()
		{
			var documentCommand1 = Factory.NewWithValidTestData<DocumentCommand>();
			documentCommand1.SU_BusinessContext = nameof(BusinessContext.Shipment);
			documentCommand1.SU_IsSystemDefined = true;
			documentCommand1.SU_MenuName = "Shipment Document";

			var documentCommand2 = Factory.NewWithValidTestData<DocumentCommand>();
			documentCommand2.SU_BusinessContext = nameof(BusinessContext.Shipment);
			documentCommand2.SU_IsSystemDefined = true;
			documentCommand2.SU_MenuName = "Shipment Document";

			var documentCommand3 = Factory.NewWithValidTestData<DocumentCommand>();
			documentCommand3.SU_BusinessContext = nameof(BusinessContext.Shipment);
			documentCommand3.SU_IsSystemDefined = false;
			documentCommand3.SU_MenuName = "Shipment Document";

			var docBuilderTemplate = Factory.NewWithValidTestData<StmTemplateBase>();
			docBuilderTemplate.SO_Name = Core.Constants.SectionRepositoryTemplateNames.System;
			docBuilderTemplate.SO_IsSystemDefined = true;

			var legacyTemplate = Factory.NewWithValidTestData<StmTemplateBase>();
			legacyTemplate.SO_Name = "Shipment Template";
			legacyTemplate.SO_IsSystemDefined = true;

			var document1 = documentCommand1.Documents.AddNew();
			document1.SI_SU = documentCommand1.PK;
			document1.SI_SO = docBuilderTemplate.PK;

			var document2 = documentCommand2.Documents.AddNew();
			document2.SI_SU = documentCommand2.PK;
			document2.SI_SO = legacyTemplate.PK;

			var document3 = documentCommand1.Documents.AddNew();
			document3.SI_SU = documentCommand1.PK;
			document3.SI_SO = docBuilderTemplate.PK;

			Factory.Save();

			var docDummy = Factory.NewWithValidTestData<DocDummyBusinessObject>();
			var returnedCommand = DocumentCommand.GetDocumentCommand(Factory, docDummy, "Shipment Document");
			AssertEquals("GetDocumentCommand", documentCommand2, returnedCommand);

			returnedCommand = DocumentCommand.GetDocumentCommand(Factory, docDummy, "Shipment Document", isDocBuilder: true);
			AssertEquals("GetDocumentCommand", documentCommand1, returnedCommand);
		}

		DocumentCommand CreateDocumentCommand(string menuName, bool isSystemDefined)
		{
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			documentCommand.SU_IsPublished = true;
			documentCommand.SU_IsSystemDefined = isSystemDefined;
			documentCommand.SU_MenuName = menuName;
			documentCommand.SU_MenuIndex = 1;
			documentCommand.SU_MenuPath = "";
			documentCommand.SU_MenuShortcut = "CtrlF1";
			return documentCommand;
		}

		public void TestGetDocumentCommand()
		{
			var pubSysShipmentMenu = CreateDocumentCommand("Pub System Shipment Document", true);
			var pubNonSystemShipmentMenu = CreateDocumentCommand("Pub Non System Shipment Document", false);

			Factory.Save();

			DocDummyBusinessObject docDummy = Factory.New<DocDummyBusinessObject>();
			docDummy.Z0_Code = "Tst";
			docDummy.Z0_Description = "Desc";

			DocumentCommand returnedCommand = null;

			returnedCommand = DocumentCommand.GetDocumentCommand(null, null, ZString.Empty);
			AssertNull("GetDocumentCommand should return null if arguments are invalid", returnedCommand);

			returnedCommand = DocumentCommand.GetDocumentCommand(Factory, null, ZString.Empty);
			AssertNull("GetDocumentCommand should return null if arguments are invalid", returnedCommand);

			returnedCommand = DocumentCommand.GetDocumentCommand(Factory, docDummy, ZString.Empty);
			AssertNull("GetDocumentCommand should return null if arguments are invalid", returnedCommand);

			returnedCommand = DocumentCommand.GetDocumentCommand(Factory, docDummy, "Pub System Shipment Document");
			AssertNotNull("GetDocumentCommand should not return null for valid arguments", returnedCommand);
			AssertEquals("GetDocumentCommand", pubSysShipmentMenu, returnedCommand);

			returnedCommand = DocumentCommand.GetDocumentCommand(Factory, docDummy, "Pub Non System Shipment Document", true);
			AssertNull(returnedCommand);
		}

		public void TestGetDocumentCommandWithFilterOverloads()
		{
			StmMenuItem menuItem = StmMenuItem.New(Factory);
			menuItem.SU_MenuName = "I WILL EAT MY OWN LEFT NUT IF THIS RECORD ALREADY EXISTS";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Shipment);
			menuItem.SU_MenuPath = "HONEST I WILL";
			menuItem.SU_GS_NKStaffCode = "XXX";
			menuItem.SU_IsSystemDefined = true;
			menuItem.SU_IsClientSpecific = false;
			menuItem.SU_FilterList = "I REALLY MEAN IT";

			DocDummyBusinessObject docDummy = Factory.New<DocDummyBusinessObject>();
			docDummy.Z0_Code = "Tst";
			docDummy.Z0_Description = "Desc";

			DocumentCommand menuItem1 = DocumentCommand.GetDocumentCommand(Factory, docDummy, menuItem.SU_MenuName, menuItem.SU_MenuPath, menuItem.SU_FilterList);
			AssertEquals("Loaded PK matches PK of Expected Matching Record", menuItem.PK, menuItem1.PK);
		}

		public void TestFilterList()
		{
			DocumentCommand command = Factory.New<DocumentCommand>();
			AssertEquals("HasErrors", false, command.SU_FilterListInfo.HasErrors());

			command.SU_FilterList = "MOD-AIR";
			AssertEquals("HasErrors", true, command.SU_FilterListInfo.HasErrors());
			AssertEquals("Filter format is incorrect, you must enter a Code, followed by the '=' sign, and followed by the expected value. e.g. MOD=SEA",
						 command.SU_FilterListInfo.GetErrors().GetFirstMessage());

			command.SU_FilterList = "BAD=FFF";
			AssertEquals("HasErrors", true, command.SU_FilterListInfo.HasErrors());
			Assert("Starts with ''BAD' code is incorrect'", command.SU_FilterListInfo.GetErrors().GetFirstMessage().StartsWith("'BAD' code is incorrect"));
			Assert("contains 'MOD'", command.SU_FilterListInfo.GetErrors().GetFirstMessage().IndexOf(nameof(DocumentFilters.MOD)) > 0);

			command.SU_FilterList = "MOD=AIR";
			AssertEquals("HasErrors", false, command.SU_FilterListInfo.HasErrors());
		}

		public void TestWhenGetChildBusinessContextsReturnedNull()
		{
			DocumentCommand pubSysShipmentMenu = Factory.New<DocumentCommand>();
			pubSysShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			pubSysShipmentMenu.SU_IsPublished = true;
			pubSysShipmentMenu.SU_IsSystemDefined = true;
			pubSysShipmentMenu.SU_MenuName = "Pub System Shipment Document";

			DocumentCommand pubSysConsolMenu = Factory.New<DocumentCommand>();
			pubSysConsolMenu.SU_BusinessContext = nameof(BusinessContext.Consol);
			pubSysConsolMenu.SU_IsPublished = true;
			pubSysConsolMenu.SU_IsSystemDefined = true;
			pubSysConsolMenu.SU_MenuName = "Pub System Consol Document";

			DocumentCommand pubSysContainerMenu = Factory.New<DocumentCommand>();
			pubSysContainerMenu.SU_BusinessContext = nameof(BusinessContext.CFSContainerRego);
			pubSysContainerMenu.SU_IsPublished = true;
			pubSysContainerMenu.SU_IsSystemDefined = true;
			pubSysContainerMenu.SU_MenuName = "Pub System Container Document";

			DocDummyBusinessObject docDummy = Factory.New<DocDummyBusinessObject>();
			docDummy.ReturnNullForSupportedChildBusinessContext = true;

			AssertEquals("DocumentCommands.Count", 1, docDummy.DocumentCommands.Count);
			AssertEquals("MenuName", "Pub System Shipment Document", docDummy.DocumentCommands[0].SU_MenuName);

			AssertEquals("SupportedChildBusinessContexts", null, docDummy.DocumentSupporter.SupportedChildBusinessContexts);
		}

		public void TestParent()
		{
			DocDummyBusinessObject docDummy = Factory.New<DocDummyBusinessObject>();
			DocumentCommand testCommand = Factory.New<DocumentCommand>();

			Assert("No DocManagerFilter on the Documents collection of TestCommand", testCommand.Documents.DocManagerFilter.IsEmpty);
			testCommand.Parent = docDummy;
			AssertEquals("After parent set, the DocManagerFilter on the TestCommand is SCL", "SCL", testCommand.DocManagerCode);
			AssertEquals("After parent set, the DocManagerFilter on the Documents collection of TestCommand has Parent RefType value of SCL", "SCL", testCommand.Documents.DocManagerFilter);
		}

		public void TestParentDocumentSupporter()
		{
			var docDummy = Factory.New<DocDummyBusinessObject>();
			var docDummy2 = Factory.New<DocDummyBusinessObject>();
			var testCommand = Factory.New<DocumentCommand>();

			testCommand.Parent = docDummy;
			var docSupporter = testCommand.ParentDocumentSupporter;
			AssertEquals("Should cache Document Supporter", docSupporter, testCommand.ParentDocumentSupporter);

			testCommand.Parent = docDummy2;
			AssertNotEquals("Should get a new Document Supporter", docSupporter, testCommand.ParentDocumentSupporter);
		}

		public void TestControllerCanEditIsPublished()
		{
			ZBool oldIsControllerValue = GlbStaff.CurrentUser.GS_IsController;

			try
			{
				GlbStaff.CurrentUser.GS_IsController = true;

				DocumentCommand pubSysShipmentMenu = Factory.New<DocumentCommand>();
				pubSysShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
				pubSysShipmentMenu.SU_IsPublished = true;
				pubSysShipmentMenu.SU_IsSystemDefined = true;
				pubSysShipmentMenu.SU_MenuName = "Pub System Shipment Document";
				pubSysShipmentMenu.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;

				AssertEquals("IsPublishedInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_IsPublishedInfo.ReadOnly);
				AssertEquals("SU_IsModifiableInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_IsModifiableInfo.ReadOnly);

				pubSysShipmentMenu.EditingMode = MenuEditingMode.AllowAll;
				AssertEquals("IsPublishedInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_IsPublishedInfo.ReadOnly);
				AssertEquals("SU_IsModifiableInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_IsModifiableInfo.ReadOnly);

				pubSysShipmentMenu.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
				AssertEquals("IsPublishedInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_IsPublishedInfo.ReadOnly);
				AssertEquals("SU_IsModifiableInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_IsModifiableInfo.ReadOnly);

				pubSysShipmentMenu.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
				AssertEquals("SU_IsModifiableInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_IsModifiableInfo.ReadOnly);
				AssertEquals("IsPublishedInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_IsPublishedInfo.ReadOnly);

				GlbStaff.CurrentUser.GS_IsController = false;

				DocumentCommand pubSysShipmentMenu2 = Factory.New<DocumentCommand>();
				pubSysShipmentMenu2.SU_BusinessContext = nameof(BusinessContext.Shipment);
				pubSysShipmentMenu2.SU_IsPublished = true;
				pubSysShipmentMenu2.SU_IsSystemDefined = true;
				pubSysShipmentMenu2.SU_MenuName = "Pub System Shipment Document2";
				pubSysShipmentMenu2.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;

				AssertEquals("IsPublishedInfo.ReadOnly", ZBool.True, pubSysShipmentMenu2.SU_IsPublishedInfo.ReadOnly);
				AssertEquals("SU_IsModifiableInfo.ReadOnly", ZBool.True, pubSysShipmentMenu2.SU_IsModifiableInfo.ReadOnly);

				pubSysShipmentMenu2.EditingMode = MenuEditingMode.AllowAll;
				AssertEquals("IsPublishedInfo.ReadOnly", ZBool.False, pubSysShipmentMenu2.SU_IsPublishedInfo.ReadOnly);
				AssertEquals("SU_IsModifiableInfo.ReadOnly", ZBool.False, pubSysShipmentMenu2.SU_IsModifiableInfo.ReadOnly);

				pubSysShipmentMenu2.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
				AssertEquals("IsPublishedInfo.ReadOnly", ZBool.True, pubSysShipmentMenu2.SU_IsPublishedInfo.ReadOnly);
				AssertEquals("SU_IsModifiableInfo.ReadOnly", ZBool.True, pubSysShipmentMenu2.SU_IsModifiableInfo.ReadOnly);

				pubSysShipmentMenu2.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
				AssertEquals("IsPublishedInfo.ReadOnly", ZBool.False, pubSysShipmentMenu2.SU_IsPublishedInfo.ReadOnly);
				AssertEquals("SU_IsModifiableInfo.ReadOnly", ZBool.False, pubSysShipmentMenu2.SU_IsModifiableInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = oldIsControllerValue;
			}
		}

		public void TestControllerCanEditIsCreditControlled()
		{
			ZBool oldIsControllerValue = GlbStaff.CurrentUser.GS_IsController;
			ZBool oldIsCreditHolderValue = Env.Security.ReceivablesOnCreditHoldController.IsAllowed;

			try
			{
				GlbStaff.CurrentUser.GS_IsController = true;
				Env.Security.ReceivablesOnCreditHoldController.IsAllowed = false;

				DocumentCommand pubSysShipmentMenu = Factory.New<DocumentCommand>();
				pubSysShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
				pubSysShipmentMenu.SU_IsSystemDefined = true;
				pubSysShipmentMenu.SU_IsClientSpecific = false;
				pubSysShipmentMenu.SU_MenuName = "Pub System Shipment Document";
				pubSysShipmentMenu.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
				pubSysShipmentMenu.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.UDF);
				pubSysShipmentMenu.SU_DeliveryRestrictionMacro = "<Macro>";

				AssertEquals("SU_DeliveryRestrictionTypeInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_DeliveryRestrictionTypeInfo.ReadOnly);
				AssertEquals("SU_DeliveryRestrictionMacroInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_DeliveryRestrictionMacroInfo.ReadOnly);
				AssertEquals("SU_DeliveryRestrictionDescriptionInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_DeliveryRestrictionDescriptionInfo.ReadOnly);

				pubSysShipmentMenu.EditingMode = MenuEditingMode.AllowAll;
				AssertEquals("SU_DeliveryRestrictionTypeInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_DeliveryRestrictionTypeInfo.ReadOnly);
				AssertEquals("SU_DeliveryRestrictionMacroInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_DeliveryRestrictionMacroInfo.ReadOnly);
				AssertEquals("SU_DeliveryRestrictionDescriptionInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_DeliveryRestrictionDescriptionInfo.ReadOnly);

				pubSysShipmentMenu.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
				AssertEquals("SU_DeliveryRestrictionTypeInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_DeliveryRestrictionTypeInfo.ReadOnly);
				AssertEquals("SU_DeliveryRestrictionMacroInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_DeliveryRestrictionMacroInfo.ReadOnly);
				AssertEquals("SU_DeliveryRestrictionDescriptionInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_DeliveryRestrictionDescriptionInfo.ReadOnly);

				pubSysShipmentMenu.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
				AssertEquals("SU_DeliveryRestrictionTypeInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_DeliveryRestrictionTypeInfo.ReadOnly);
				AssertEquals("SU_DeliveryRestrictionMacroInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_DeliveryRestrictionMacroInfo.ReadOnly);
				AssertEquals("SU_DeliveryRestrictionDescriptionInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_DeliveryRestrictionDescriptionInfo.ReadOnly);

				pubSysShipmentMenu.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.NON);
				AssertEquals("SU_DeliveryRestrictionTypeInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_DeliveryRestrictionTypeInfo.ReadOnly);
				AssertEquals("SU_DeliveryRestrictionMacroInfo.ReadOnly", ZBool.True, pubSysShipmentMenu.SU_DeliveryRestrictionMacroInfo.ReadOnly);
				AssertEquals("SU_DeliveryRestrictionDescriptionInfo.ReadOnly", ZBool.True, pubSysShipmentMenu.SU_DeliveryRestrictionDescriptionInfo.ReadOnly);

				GlbStaff.CurrentUser.GS_IsController = false;

				DocumentCommand pubSysShipmentMenu2 = Factory.New<DocumentCommand>();
				pubSysShipmentMenu2.SU_BusinessContext = nameof(BusinessContext.Shipment);
				pubSysShipmentMenu2.SU_IsSystemDefined = false;
				pubSysShipmentMenu2.SU_IsClientSpecific = true;
				pubSysShipmentMenu2.SU_MenuName = "Pub System Shipment Document";
				pubSysShipmentMenu2.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
				pubSysShipmentMenu2.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.NON);

				AssertEquals("SU_DeliveryRestrictionTypeInfo.ReadOnly", ZBool.True, pubSysShipmentMenu2.SU_DeliveryRestrictionTypeInfo.ReadOnly);
				AssertEquals("SU_DeliveryRestrictionMacroInfo.ReadOnly", ZBool.True, pubSysShipmentMenu2.SU_DeliveryRestrictionMacroInfo.ReadOnly);
				AssertEquals("SU_DeliveryRestrictionDescriptionInfo.ReadOnly", ZBool.True, pubSysShipmentMenu2.SU_DeliveryRestrictionDescriptionInfo.ReadOnly);

				pubSysShipmentMenu2.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.UDF);
				AssertEquals("SU_DeliveryRestrictionTypeInfo.ReadOnly", ZBool.True, pubSysShipmentMenu2.SU_DeliveryRestrictionTypeInfo.ReadOnly);
				AssertEquals("SU_DeliveryRestrictionMacroInfo.ReadOnly", ZBool.True, pubSysShipmentMenu2.SU_DeliveryRestrictionMacroInfo.ReadOnly);
				AssertEquals("SU_DeliveryRestrictionDescriptionInfo.ReadOnly", ZBool.True, pubSysShipmentMenu2.SU_DeliveryRestrictionDescriptionInfo.ReadOnly);

				pubSysShipmentMenu2.EditingMode = MenuEditingMode.AllowAll;
				pubSysShipmentMenu2.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.UDF);
				pubSysShipmentMenu2.SU_DeliveryRestrictionMacro = "<Macro>";
				AssertEquals("SU_DeliveryRestrictionTypeInfo.ReadOnly", ZBool.False, pubSysShipmentMenu2.SU_DeliveryRestrictionTypeInfo.ReadOnly);
				AssertEquals("SU_DeliveryRestrictionMacroInfo.ReadOnly", ZBool.False, pubSysShipmentMenu2.SU_DeliveryRestrictionMacroInfo.ReadOnly);
				AssertEquals("SU_DeliveryRestrictionDescriptionInfo.ReadOnly", ZBool.False, pubSysShipmentMenu2.SU_DeliveryRestrictionDescriptionInfo.ReadOnly);

				pubSysShipmentMenu2.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
				AssertEquals("SU_DeliveryRestrictionTypeInfo.ReadOnly", ZBool.False, pubSysShipmentMenu2.SU_DeliveryRestrictionTypeInfo.ReadOnly);
				AssertEquals("SU_DeliveryRestrictionMacroInfo.ReadOnly", ZBool.False, pubSysShipmentMenu2.SU_DeliveryRestrictionMacroInfo.ReadOnly);
				AssertEquals("SU_DeliveryRestrictionDescriptionInfo.ReadOnly", ZBool.False, pubSysShipmentMenu2.SU_DeliveryRestrictionDescriptionInfo.ReadOnly);

				pubSysShipmentMenu2.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
				pubSysShipmentMenu2.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.NON);
				AssertEquals("SU_DeliveryRestrictionTypeInfo.ReadOnly", ZBool.True, pubSysShipmentMenu2.SU_DeliveryRestrictionTypeInfo.ReadOnly);
				AssertEquals("SU_DeliveryRestrictionMacroInfo.ReadOnly", ZBool.True, pubSysShipmentMenu2.SU_DeliveryRestrictionMacroInfo.ReadOnly);
				AssertEquals("SU_DeliveryRestrictionDescriptionInfo.ReadOnly", ZBool.True, pubSysShipmentMenu2.SU_DeliveryRestrictionDescriptionInfo.ReadOnly);

				Env.Security.ReceivablesOnCreditHoldController.IsAllowed = true;

				pubSysShipmentMenu2.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
				pubSysShipmentMenu2.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.UDF);
				pubSysShipmentMenu2.SU_DeliveryRestrictionMacro = "<Macro>";

				AssertEquals("SU_DeliveryRestrictionTypeInfo.ReadOnly", ZBool.False, pubSysShipmentMenu2.SU_DeliveryRestrictionTypeInfo.ReadOnly);
				AssertEquals("SU_DeliveryRestrictionMacroInfo.ReadOnly", ZBool.False, pubSysShipmentMenu2.SU_DeliveryRestrictionMacroInfo.ReadOnly);
				AssertEquals("SU_DeliveryRestrictionDescriptionInfo.ReadOnly", ZBool.False, pubSysShipmentMenu2.SU_DeliveryRestrictionDescriptionInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = oldIsControllerValue;
				Env.Security.ReceivablesOnCreditHoldController.IsAllowed = oldIsCreditHolderValue;
			}
		}

		#region TestSU_EmailSubjectLine_ReadOnlyness

		#region TestSU_EmailSubjectLine_ReadOnlyness Controller

		public void TestSU_EmailSubjectLine_ReadOnlyness_IsController()
		{
			ZBool oldIsControllerValue = GlbStaff.CurrentUser.GS_IsController;

			try
			{
				var documentCommand = Factory.New<DocumentCommand>();
				documentCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
				documentCommand.SU_MenuName = "System Document";
				documentCommand.SU_IsSystemDefined = true;

				//MenuEditingMode has no impact on the readonlyness for a Controller user
				documentCommand.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
				AssertTestSU_EmailSubjectLine_ReadOnlyness_IsController(documentCommand);

				documentCommand.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
				AssertTestSU_EmailSubjectLine_ReadOnlyness_IsController(documentCommand);

				documentCommand.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
				AssertTestSU_EmailSubjectLine_ReadOnlyness_IsController(documentCommand);

				documentCommand.EditingMode = MenuEditingMode.AllowAll;
				AssertTestSU_EmailSubjectLine_ReadOnlyness_IsController(documentCommand);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = oldIsControllerValue;
			}
		}

		void AssertTestSU_EmailSubjectLine_ReadOnlyness_IsController(DocumentCommand documentCommand)
		{
			GlbStaff.CurrentUser.GS_IsController = true;
			ResetDocumentCommand(documentCommand);

			Assert(!documentCommand.SU_EmailSubjectLineInfo.ReadOnly);

			documentCommand.ChildMenus.AddNew();
			Assert("No Documents, Is NOT a Dock Pack, Has ChildMenus => READONLY", documentCommand.SU_EmailSubjectLineInfo.ReadOnly);

			documentCommand.Documents.AddNew();
			Assert("Has some documents => NOT READONLY", !documentCommand.SU_EmailSubjectLineInfo.ReadOnly);

			documentCommand.Documents.RemoveAll();
			documentCommand.SU_IsDocPack = true;
			Assert("Is DocPack => NOT READONLY", !documentCommand.SU_EmailSubjectLineInfo.ReadOnly);
		}

		#endregion

		#region TestHasChildMenu

		public void TestHasChildMenu()
		{
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_BusinessContext = nameof(BusinessContext.DtbConsignment);
			documentCommand.SU_MenuName = "System Document";

			AssertEquals("Should have no ChildMenus and return false.", false, documentCommand.HasChildMenus);

			documentCommand.ChildMenus.AddNew();
			AssertEquals("Should have ChildMenus and return true.", true, documentCommand.HasChildMenus);
		}

		#endregion

		#region TestSU_EmailSubjectLine_ReadOnlyness NotController

		public void TestSU_EmailSubjectLine_ReadOnlyness_IsNotController()
		{
			ZBool oldIsControllerValue = GlbStaff.CurrentUser.GS_IsController;

			try
			{
				GlbStaff.CurrentUser.GS_IsController = false;

				var documentCommand = Factory.New<DocumentCommand>();
				documentCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
				documentCommand.SU_MenuName = "System Document";
				documentCommand.SU_IsSystemDefined = true;
				documentCommand.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;

				//System Document
				Assert("Not a controller and system Doc => Always READONLY", documentCommand.SU_EmailSubjectLineInfo.ReadOnly);

				documentCommand.ChildMenus.AddNew();
				Assert("Not a controller and system Doc => Always READONLY", documentCommand.SU_EmailSubjectLineInfo.ReadOnly);

				documentCommand.Documents.AddNew();
				Assert("Not a controller and system Doc => Always READONLY", documentCommand.SU_EmailSubjectLineInfo.ReadOnly);

				documentCommand.Documents.RemoveAll();
				documentCommand.SU_IsDocPack = true;
				Assert("Not a controller and system Doc => Always READONLY", documentCommand.SU_EmailSubjectLineInfo.ReadOnly);

				//Non System Document
				ResetDocumentCommand(documentCommand);
				documentCommand.SU_IsSystemDefined = false;

				Assert("No ChildMenus => NOT READONLY", !documentCommand.SU_EmailSubjectLineInfo.ReadOnly);

				documentCommand.ChildMenus.AddNew();
				Assert("No Documents, Is NOT a Dock Pack, Has ChildMenus => READONLY", documentCommand.SU_EmailSubjectLineInfo.ReadOnly);

				documentCommand.Documents.AddNew();
				Assert("Has Documents => NOT READONLY", !documentCommand.SU_EmailSubjectLineInfo.ReadOnly);

				documentCommand.Documents.RemoveAll();
				documentCommand.SU_IsDocPack = true;
				Assert("Is DocPack => NOT READONLY", !documentCommand.SU_EmailSubjectLineInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = oldIsControllerValue;
			}
		}

		void ResetDocumentCommand(DocumentCommand documentCommand)
		{
			documentCommand.SU_IsDocPack = false;
			documentCommand.SU_IsSystemDefined = true;
			documentCommand.Documents.RemoveAll();
			documentCommand.ChildMenus.RemoveAll();
		}

		#endregion

		#endregion

		public void TestSU_Calc_IsWebSupportable_ReadOnly()
		{
			var systemDocument = Factory.New<DocumentCommand>();
			systemDocument.SU_BusinessContext = nameof(BusinessContext.Shipment);
			systemDocument.SU_IsPublished = true;
			systemDocument.SU_IsSystemDefined = true;
			systemDocument.SU_MenuName = "System Shipment Document";

			systemDocument.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("SU_Calc_IsWebSupportable.ReadOnly", true, systemDocument.SU_Calc_IsWebSupportableInfo.ReadOnly);

			systemDocument.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("SU_Calc_IsWebSupportable.ReadOnly", false, systemDocument.SU_Calc_IsWebSupportableInfo.ReadOnly);

			systemDocument.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("SU_Calc_IsWebSupportable.ReadOnly", true, systemDocument.SU_Calc_IsWebSupportableInfo.ReadOnly);

			systemDocument.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("SU_Calc_IsWebSupportable.ReadOnly", false, systemDocument.SU_Calc_IsWebSupportableInfo.ReadOnly);

			systemDocument.SU_IsSystemDefined = false;
			systemDocument.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("SU_Calc_IsWebSupportable.ReadOnly", false, systemDocument.SU_Calc_IsWebSupportableInfo.ReadOnly);

			systemDocument.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("SU_Calc_IsWebSupportable.ReadOnly", false, systemDocument.SU_Calc_IsWebSupportableInfo.ReadOnly);

			systemDocument.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("SU_Calc_IsWebSupportable.ReadOnly", false, systemDocument.SU_Calc_IsWebSupportableInfo.ReadOnly);

			systemDocument.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("SU_Calc_IsWebSupportable.ReadOnly", false, systemDocument.SU_Calc_IsWebSupportableInfo.ReadOnly);
		}

		public void TestEDocs()
		{
			DocDummyBusinessObject docDummy = Factory.New<DocDummyBusinessObject>();
			DocumentCommand testCommand = Factory.New<DocumentCommand>();
			testCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			testCommand.SU_MenuName = "Test Document";
			testCommand.Parent = docDummy;

			AssertEquals("After parent set, the DocManagerFilter on the TestCommand is SCL", "SCL", testCommand.DocManagerCode);

			RefDocType docType = Factory.New<RefDocType>();
			docType.RT_ReferenceType = "SHP";
			docType.RT_DocType = "ABC";
			docType.RT_Desc = "This is a document type";
			StmMenuEDocs eDoc = Factory.New<DocumentStmMenuEDocs>();
			eDoc.SX_SU = testCommand.PK;
			eDoc.SX_RT_DocType = docType.PK;

			AssertEquals("MenuItem eDocs count should be 1", 1, testCommand.EDocs.Count);
			AssertEquals("MenuItem matches new item created", eDoc, testCommand.EDocs[0]);

			testCommand.EDocs.Remove(eDoc);
			AssertEquals("MenuItem eDocs count should be 0", 0, testCommand.EDocs.Count);
		}

		public void TestAddEDocNonSystemGenerated()
		{
			DocDummyBusinessObject docDummy = Factory.New<DocDummyBusinessObject>();
			DocumentCommand testCommand = Factory.New<DocumentCommand>();
			testCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			testCommand.SU_MenuName = "Test Document";
			testCommand.Parent = docDummy;

			AssertEquals("Precondition: EDocs count should be 0", 0, testCommand.EDocs.Count);

			RefDocType docType = Factory.New<RefDocType>();
			docType.RT_ReferenceType = "SHP";
			docType.RT_DocType = "ABC";
			docType.RT_Desc = "This is a document type";
			testCommand.AddEDoc(docType);
			AssertEquals("EDocs count should be 1", 1, testCommand.EDocs.Count);
			AssertEquals("EDocsView count should also be 1", 1, testCommand.EDocsView.Count);

			testCommand.AddEDoc(docType);
			AssertEquals("Trying to add the same doctype again should have absolutely no effect - collection count should stay at 1", 1, testCommand.EDocs.Count);
			AssertEquals("Trying to add the same doctype again should have absolutely no effect - collection view count should stay at 1", 1, testCommand.EDocsView.Count);
		}

		public void TestAddEDocSystemGenerated()
		{
			DocDummyBusinessObject docDummy = Factory.New<DocDummyBusinessObject>();
			DocumentCommand testCommand = Factory.New<DocumentCommand>();
			testCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			testCommand.SU_MenuName = "Test Document";
			testCommand.Parent = docDummy;

			RefDocType docType = Factory.New<RefDocType>();
			docType.RT_ReferenceType = "SHP";
			docType.RT_DocType = "ABC";
			docType.RT_Desc = "This is a document type";

			StmMenuEDocs eDoc = Factory.New<DocumentStmMenuEDocs>();
			eDoc.SX_SU = testCommand.PK;
			eDoc.SX_RT_DocType = docType.PK;
			eDoc.SX_IsSystemDefined = true;
			eDoc.SX_IsClientSupressed = true;

			AssertEquals("Precondition: starting with a system-generated edoc that is supressed, EDocs count should be 1", 1, testCommand.EDocs.Count);
			AssertEquals("Precondition: starting with a system-generated edoc that is supressed, the view count should be 0", 0, testCommand.EDocsView.Count);

			testCommand.AddEDoc(docType);
			AssertEquals("When calling add with a previously suppressed eDoc record, it should just update the flag on that record. The EDocs collection count should stay the same.", 1, testCommand.EDocs.Count);
			AssertEquals("EDocs view count should be updated to 1 from 0, now that the record is visible", 1, testCommand.EDocs.Count);

			testCommand.AddEDoc(docType);
			AssertEquals("Trying to add the same doctype again should have absolutely no effect - collection count should stay at 1", 1, testCommand.EDocs.Count);
			AssertEquals("Trying to add the same doctype again should have absolutely no effect - collection view count should stay at 1", 1, testCommand.EDocsView.Count);
		}

		public void TestRemoveEDocNonSystemGenerated()
		{
			DocDummyBusinessObject docDummy = Factory.New<DocDummyBusinessObject>();
			DocumentCommand testCommand = Factory.New<DocumentCommand>();
			testCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			testCommand.SU_MenuName = "Test Document";
			testCommand.Parent = docDummy;

			RefDocType docType = Factory.New<RefDocType>();
			docType.RT_ReferenceType = "SHP";
			docType.RT_DocType = "ABC";
			docType.RT_Desc = "This is a document type";

			StmMenuEDocs eDoc = Factory.New<DocumentStmMenuEDocs>();
			eDoc.SX_SU = testCommand.PK;
			eDoc.SX_RT_DocType = docType.PK;

			AssertEquals("Precondition: eDocs collection count is 1", 1, testCommand.EDocs.Count);
			AssertEquals("Precondition: eDocs collection view count is 1", 1, testCommand.EDocsView.Count);

			testCommand.RemoveEDoc(eDoc);

			AssertEquals("eDocs collection count should be 0 after remove", 0, testCommand.EDocs.Count);
			AssertEquals("eDocs collection view count should be 0 after remove", 0, testCommand.EDocsView.Count);
			Assert("eDoc object should be deleted", eDoc.IsDeleted);
		}

		public void TestRemoveEDocSystemGenerated()
		{
			DocDummyBusinessObject docDummy = Factory.New<DocDummyBusinessObject>();
			DocumentCommand testCommand = Factory.New<DocumentCommand>();
			testCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			testCommand.SU_MenuName = "Test Document";
			testCommand.Parent = docDummy;

			RefDocType docType = Factory.New<RefDocType>();
			docType.RT_ReferenceType = "SHP";
			docType.RT_DocType = "ABC";
			docType.RT_Desc = "This is a document type";

			StmMenuEDocs eDoc = Factory.New<DocumentStmMenuEDocs>();
			eDoc.SX_SU = testCommand.PK;
			eDoc.SX_RT_DocType = docType.PK;
			eDoc.SX_IsSystemDefined = true;

			AssertEquals("Precondition: eDocs collection count is 1", 1, testCommand.EDocs.Count);
			AssertEquals("Precondition: eDocs collection view count is 1", 1, testCommand.EDocsView.Count);

			testCommand.RemoveEDoc(eDoc);

			AssertEquals("eDocs collection count should be 1 after remove, because we didn't actually remove it just set the SX_IsClientSupressed flag", 1, testCommand.EDocs.Count);
			AssertEquals("eDocs collection view count should be 0 after remove", 0, testCommand.EDocsView.Count);
			Assert("eDoc object should NOT be deleted", !eDoc.IsDeleted);
		}

		public void TestGetDocWrapperForMenuDataContext()
		{
			DocumentCommand testCommand = Factory.New<DocumentCommand>();
			testCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			testCommand.SU_IsPublished = true;
			testCommand.SU_IsSystemDefined = true;
			testCommand.SU_MenuName = "Test Command";
			testCommand.SU_MenuDataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			Factory.Save();

			DocDummyBusinessObject dummy = Factory.New<DocDummyBusinessObject>();
			testCommand.Parent = dummy;

			AssertEquals("correct command should be returned", testCommand, dummy.DocumentCommands[0]);
			AssertNotNull("Dummy docwrapper returned - should not be null", testCommand.GetDocWrapperForMenuDataContext());
			DocumentWrapper docWrapper = dummy.DocumentSupporter.GetDocumentWrappers(Core.Constants.DataContext.Shipment, null)[0];
			IBODocDataProvider docWrapperReturned = testCommand.GetDocWrapperForMenuDataContext();
			AssertEquals("Dummy docwrapper type", docWrapper.GetType(), docWrapperReturned.GetType());
		}

		public void TestGetReportForMenuDataContextReplacements()
		{
			DocumentCommand testCommand = Factory.New<DocumentCommand>();
			testCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			testCommand.SU_IsPublished = true;
			testCommand.SU_IsSystemDefined = true;
			testCommand.SU_MenuName = "Test Command";
			testCommand.SU_MenuDataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			Factory.Save();

			DocDummyBusinessObject dummy = Factory.New<DocDummyBusinessObject>();
			AssertEquals("correct command should be returned", testCommand, dummy.DocumentCommands[0]);
		}

		public void TestIsModifiableValidator()
		{
			DocumentCommand testCommand = Factory.New<DocumentCommand>();
			testCommand.SU_SupportsVisualisation = false;
			testCommand.SU_IsModifiable = false;
			testCommand.Validation.ValidateSU_IsModifiable();
			AssertEquals("HasErrors", false, testCommand.SU_IsModifiableInfo.HasErrors());

			testCommand.SU_SupportsVisualisation = false;
			testCommand.SU_IsModifiable = true;
			testCommand.Validation.ValidateSU_IsModifiable();
			AssertEquals("HasErrors", true, testCommand.SU_IsModifiableInfo.HasErrors());

			testCommand.SU_SupportsVisualisation = true;
			testCommand.SU_IsModifiable = false;
			testCommand.Validation.ValidateSU_IsModifiable();
			AssertEquals("HasErrors", false, testCommand.SU_IsModifiableInfo.HasErrors());

			testCommand.SU_SupportsVisualisation = true;
			testCommand.SU_IsModifiable = true;
			testCommand.Validation.ValidateSU_IsModifiable();
			AssertEquals("HasErrors", false, testCommand.SU_IsModifiableInfo.HasErrors());
		}

		public void TestIsEDocsProviderPlaceholder()
		{
			DocumentCommand command = Factory.New<DocumentCommand>();
			AssertEquals("IsEDocsProviderPlaceholder", false, command.IsEDocsProviderPlaceholder);

			command.SU_FilterList = Core.Constants.MenuItemFilters.EDocsProviderPlaceholderTag;
			AssertEquals("IsEDocsProviderPlaceholder", false, command.IsEDocsProviderPlaceholder);

			command.SU_IsSystemDefined = true;
			AssertEquals("IsEDocsProviderPlaceholder", true, command.IsEDocsProviderPlaceholder);
		}

		public void TestReadOnly()
		{
			DocumentCommand command = Factory.New<DocumentCommand>();
			AssertEquals("ReadOnly", false, command.ReadOnly);

			command.ReadOnly = true;
			AssertEquals("ReadOnly", true, command.ReadOnly);

			command.ReadOnly = false;
			AssertEquals("ReadOnly", false, command.ReadOnly);

			command.SU_FilterList = Core.Constants.MenuItemFilters.EDocsProviderPlaceholderTag;
			AssertEquals("ReadOnly", false, command.ReadOnly);

			command.SU_IsSystemDefined = true;
			AssertEquals("ReadOnly", true, command.ReadOnly);

			command.Delete();
			AssertEquals("ReadOnly", false, command.ReadOnly);
		}

		public void TestIRootTypeProvider()
		{
			DocDummyBusinessObject dummy = Factory.New<DocDummyBusinessObject>();
			dummy.SetTableNameOverride("JobShipment");
			DocumentCommand command = Factory.New<DocumentCommand>();
			command.Parent = dummy;

			var provider = (IRootTypeProvider)command;
			AssertEquals(2, provider.RootTypes.Length);
			AssertEquals(typeof(DocumentCommand), provider.RootTypes[0]);
			AssertEquals(typeof(DocDummyBusinessObject), provider.RootTypes[1]);
			AssertEquals(2, provider.Roots.Length);
			AssertEquals(command, provider.Roots[0]);
			AssertEquals(dummy, provider.Roots[1]);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Enterprise.DbUpgrader.Data.Testing.DocumentTablesCleaner.Clean();
		}

		#endregion
	}
}
