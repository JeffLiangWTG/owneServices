using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Integration.DocumentEngine;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Constants = Enterprise.Core.Constants;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngineCore.DocumentSupport.Testing
{
	[TestsSubclassesOf(typeof(DocumentSupporter), typeof(ExcludeDocumentSupporterTestAttribute),
		new string[] { "Enterprise.MarketingManager.Business.GlbCompanyCampaign+GlbCompanyCampaignDocumentSupporter, Enterprise.MarketingManager.Business"
		, "Enterprise.eManifest.Business.SupplierBookingHeaderDocumentSupporter, Enterprise.eManifest.Business"
		, "Enterprise.eManifest.Business.SupplierBookingLineDocumentSupporter, Enterprise.eManifest.Business"
		, "Enterprise.Rating.Business.QuoteDocumentSupporter, Enterprise.Rating.Business"
		, "Enterprise.Rating.Business.RatingHeaderDocumentSupporter, Enterprise.Rating.Business"
		, "Enterprise.Freight.Business.CommonPickupDeliveryConfirmDocumentSupporter, Enterprise.Freight"
		, "Enterprise.Freight.Business.JobMawbDocumentSupporter, Enterprise.Freight"
		, "Enterprise.Freight.LocalCartage.Business.CartageDocumentSupporter, Enterprise.Freight.LocalCartage.Business"
		, "Enterprise.Freight.LocalCartage.Business.CartageLegDocumentSupporter, Enterprise.Freight.LocalCartage.Business"
		, "Enterprise.Freight.Forwarding.Business.ContainerDocumentSupporterShipment, Enterprise.Freight.Forwarding.Business"
		, "Enterprise.Freight.Forwarding.Business.ForwardingShipmentDocumentSupporter, Enterprise.Freight.Forwarding.Business"
		, "Enterprise.Freight.Forwarding.Business.ForwardingShipmentWrapperWithConsolAgentDocumentSupporter, Enterprise.Freight.Forwarding.Business"
		, "Enterprise.Freight.QuotedBookings.Business.PreAllocationDocumentSupporter, Enterprise.Freight.QuotedBookings.Business"
		, "Enterprise.Freight.QuotedBookings.Business.QuotedBookingDocumentSupporter, Enterprise.Freight.QuotedBookings.Business"
		, "Enterprise.Freight.Agency.Business.ReleaseInstanceDocumentSupporter, Enterprise.Freight.Agency.Business"
		, "Enterprise.Freight.Agency.Business.SundryChargesDocumentSupporter, Enterprise.Freight.Agency.Business"
		, "Enterprise.Freight.Agency.Business.AgencyShipmentDocumentSupporter, Enterprise.Freight.Agency.Business"
		, "Enterprise.Freight.Agency.Business.ContainerDetentionDocumentSupporter, Enterprise.Freight.Agency.Business"
		, "Enterprise.Freight.Agency.Business.VoyageAccountDocumentSupporter, Enterprise.Freight.Agency.Business"
		, "Enterprise.Customs.Business.CusEntryHeaderDocumentSupporter, Enterprise.Customs.Business"
		, "Enterprise.Customs.Business.BaseJobDeclarationDocumentSupporter, Enterprise.Customs.Business"
		, "Enterprise.Accounting.Business.DataInterface.ChinaJournalListing+ChinaJournalListingDocumentSupporter, Enterprise.Accounting.Business"
		, "Enterprise.Accounting.Business.DataInterface.ChinaDataInterface.VoucherFile.VoucherProvider+VoucherProviderDocumentSupporter, Enterprise.Accounting.Business"
		, "Enterprise.Accounting.Business.CashBook.DirectPayment.DirectPaymentDocumentSupporter, Enterprise.Accounting.Business"
		, "Enterprise.Accounting.Business.PayableOrder.AccPayableOrderHeaderDocumentSupporter, Enterprise.Accounting.Business"
		, "Enterprise.Accounting.Netting.NettingDocumentPrinterDocumentSupporter, Enterprise.Accounting.Netting"
		, "Enterprise.Accounting.Netting.NettingStatementDocumentSupporter, Enterprise.Accounting.Netting"
		, "Enterprise.Accounting.Netting.ParticipantStatementDocumentSupporter, Enterprise.Accounting.Netting"
		, "Enterprise.Accounting.Business.ARAP.HotCheque.AccHotCheque+AccHotChequeDocumentSupporter, Enterprise.Accounting.Business"
		, "Enterprise.Accounting.Business.ARAP.Invoicing.PrintSummaryDocumentSupporter, Enterprise.Accounting.Business"
		, "Enterprise.Accounting.Business.ARAP.Invoicing.CASSBillingDocumentSupporter, Enterprise.Accounting.Business"
		, "Enterprise.Accounting.Business.ARAP.Invoicing.PrintStatementDocumentSupporter, Enterprise.Accounting.Business"
		, "Enterprise.Accounting.Business.ARAP.Invoicing.StatementDocumentSupporter, Enterprise.Accounting.Business"
		, "Enterprise.Packing.Business.PkgPackageHeaderDocumentSupporter, Enterprise.Packing.Business"
		, "Enterprise.Packing.Business.PkgPackageJobPackageHeaderPivotDocumentSupporter, Enterprise.Packing.Business"
		, "Enterprise.Packing.Business.PkgPackageJobDocumentSupporter, Enterprise.Packing.Business"
		, "Enterprise.Packing.Business.PkgPackageDocumentSupporter, Enterprise.Packing.Business"
		, "Enterprise.Recruiter.Business.LatestCompletedSkillRatingDocumentSupporter, Enterprise.Recruiter.Business"
		, "Enterprise.Recruiter.Business.HRJobApplicantSkillRatingDocumentSupporter, Enterprise.Recruiter.Business"
		, "Enterprise.ProcessManagement.Business.WorkItemDocumentSupporter, Enterprise.ProcessManagement.Business"
		, "Enterprise.Tracking.Business.TrackingBooking+TrackingBookingDocumentSupporter, Enterprise.Tracking.Business" }, ExcludeClientDlls = true, ExcludePrivate = true)]
	public abstract class DocumentSupporterTest : TestCaseWithFactory
	{
		public void TestCustomisationSecurityCheckpointLooksRight()
		{
			var topLevelBO = GetDocumentSupportableBusinessObject();
			var topLevelBODocumentSupporter = topLevelBO.DocumentSupporter;
			var checkpoint = topLevelBODocumentSupporter.CustomisationSecurityCheckpoint;
			//None is also OK.
			if (checkpoint.DisplayText == "None")
			{
				Assert(true);
				return;
			}
			//Client checkpoints are also OK.
			if (ClientHookLoader.Instance.ClientHook != null && ClientHookLoader.Instance.ClientHook.IsInitialised)
			{
				Assert(true);
				return;
			}
			Assert(checkpoint.DisplayText + " should contain Customize", checkpoint.DisplayText.ToString().Contains("Customize"));
		}

		public void TestAllSystemDocumentsHaveADocumentTypeSoEDocsCanBeProperlyConfigured()
		{
			var result = new ZStringBuilder();
			var topLevelBO = GetDocumentSupportableBusinessObject();
			var documentCommands = Factory.Load<IDocumentCommand>(GetDocumentCommandQuery(topLevelBO));

			foreach (var documentCommand in documentCommands)
			{
				if (documentCommand.SU_IsSystemDefined && documentCommand.SU_IsPublished)
				{
					var templatePivots = Factory.Load<IStmMenuTemplatePivot>(new ZQuery(StmMenuTemplatePivotSchema.SI_SU, documentCommand.PK));

					foreach (var templatePivot in templatePivots)
					{
						if (templatePivot.SI_IsSystemDefined && templatePivot.SI_RT_DocType.IsEmpty)
						{
							result.Append("Menu Item: [" + documentCommand.SU_MenuName + "]  Template Pivot: [" + templatePivot.SI_DocumentTitle + "]");
						}
					}
				}
			}

			Assert("The following Menu Template Pivots do not have a DocType Assigned:\r\n\r\n" + result.ToStringWithNewLineBetweenAppends(), result.IsEmpty);
		}

		public void TestGetChildCollectionForAllSupportedChildBusinessContexts()
		{
			var topLevelBO = GetDocumentSupportableBusinessObject();
			var topLevelBODocumentSupporter = topLevelBO.DocumentSupporter;

			var childBusinessContexts = topLevelBODocumentSupporter.SupportedChildBusinessContexts;
			if (childBusinessContexts == null || childBusinessContexts.Length == 0)
			{
				Assert("No child business contexts to check", true);
			}
			else
			{
				var menuItem = Factory.LoadTop1<IDocumentCommand>(GetDocumentCommandQuery(topLevelBO));
				foreach (var childContext in childBusinessContexts)
				{
					AssertNotNull(string.Format("Should not return null for a SupportedChildBusinessContext: {0}.", childContext.ToString()), topLevelBODocumentSupporter.GetChildCollection((IStmMenuItem)menuItem, childContext, null));
				}
			}
		}

		[DeveloperOnlyTest]
		public void TestDocumentsShouldProvideMessageForNotAbleToPrint()
		{
			ZStringBuilder errorBuilder = new ZStringBuilder();
			var documentsProperty = Type.GetType("Enterprise.DocumentEngine.DocumentCommand, Enterprise.DocumentEngine").GetProperty("Documents");
			var stmMenuTemplatePivotType = Type.GetType("Enterprise.MasterFiles.Business.StmMenuTemplatePivot, Enterprise.MasterFiles.Business");
			var orgHeaderType = Type.GetType("Enterprise.MasterFiles.Business.OrgHeader, Enterprise.MasterFiles.Business");
			var templateGeneratorType = Type.GetType("Enterprise.DocumentEngine.DocBuilder.TemplateGenerator, Enterprise.DocumentEngine");
			var templateGeneratorTypeConstructor = templateGeneratorType.GetConstructor(new Type[] { stmMenuTemplatePivotType, orgHeaderType, typeof(ZString) });
			var dataContextValueProperty = templateGeneratorType.GetProperty("DataContext");

			foreach (IDocumentSupportable topLevelBO in TopLevelBOsForMessageNotPrintingTest)
			{
				var documentSupporter = topLevelBO.DocumentSupporter;
				IDocumentCommand[] documentCommands = Factory.Load<IDocumentCommand>(GetDocumentCommandQuery(topLevelBO));

				foreach (IDocumentCommand documentCommand in documentCommands)
				{
					if (documentCommand.SU_IsSystemDefined && documentCommand.SU_IsPublished && documentCommand.SU_MenuType == StmMenuItemTypes.Documents)
					{
						foreach (var pivot in documentsProperty.GetValue(documentCommand) as ICollection)
						{
							var templateGenerator = templateGeneratorTypeConstructor.Invoke(new[] { pivot, null, (ZString)Res.DefaultLanguage });
							DataContextValue dataContext = dataContextValueProperty.GetValue(templateGenerator) as DataContextValue;

							if (ShouldSkipWithContextAndMenu(dataContext.DataContext, (IStmMenuItem)documentCommand))
							{
								continue;
							}

							if (documentSupporter.ShowReasonForNotPrinting(dataContext.DataContext, (IStmMenuItem)documentCommand))
							{
								string reasonMessageForEmptyPack = documentSupporter.GetBODocDataProvidersNotFoundMessage(dataContext, (IStmMenuItem)documentCommand);
								if (string.IsNullOrEmpty(reasonMessageForEmptyPack))
								{
									if (errorBuilder.Length == 0)
									{
										errorBuilder.AppendLine(@"
<p><strong>There's no message for not able to print for these document command and data context cases.<br />
Please override DocumentSupporter.GetBODocDataProvidersNotFoundMessage to add some reasonable message. Or return false for them in DocumentSupporter.ShowReasonForNotPrinting to ignore them.</strong></p>
");
										errorBuilder.AppendLine(string.Format("Document command: {0}, DataContext: {1} <br />", documentCommand.SU_MenuName, dataContext.DataContext));
									}
									else
									{
										errorBuilder.AppendLine(string.Format("Document command: {0}, DataContext: {1} <br />", documentCommand.SU_MenuName, dataContext.DataContext));
									}
								}
							}
						}
					}
				}
			}

			if (errorBuilder.Length > 0)
			{
				HtmlFail(errorBuilder.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual bool ShouldSkipWithContextAndMenu(DataContext context, IStmMenuItem menu)
		{
			return false;
		}

		protected void AssertNotFoundMessage(IDocumentSupportable supportable, IStmMenuItem menu, DataContext[] contexts, bool needReturnMessage, string expectedMessage)
		{
			var supporter = supportable.DocumentSupporter;

			AssertNotNull(supporter);

			CombineAssertions(() =>
			{
				foreach (var context in contexts)
				{
					var wrappers = supporter.GetDocumentWrappers(context, menu);

					var dataContextValue = new DataContextValueForTesting(context);
					var notFoundMessage = supporter.GetBODocDataProvidersNotFoundMessage(dataContextValue, menu);

					if (needReturnMessage)
					{
						var message = $"Shoud not return any wrappers for {context}";
						Assert(message, wrappers == null || wrappers.Length == 0 || wrappers.All(c => c == null));

						message = $"Shoud return this message for {context}";
						AssertEquals(message, expectedMessage, notFoundMessage);
					}
					else
					{
						var message = $"Shoud return any wrappers for {context}";
						Assert(message, wrappers != null && wrappers.Length > 0 && wrappers.All(c => c != null));

						message = $"Shoud not return any messages for {context}";
						AssertEquals(message, ZString.Empty, notFoundMessage);
					}
				}
			});
		}

		[SnailTest]
		public virtual void TestRunningDocumentsShouldNotCauseException()
		{
			CombineAssertions(() =>
			{
				foreach (var topLevelBO in TopLevelBOsForRunningDocumentsTest)
				{
					AssertRunningDocumentsForBusinessObject(topLevelBO);
				}
			});
		}

		protected void AssertRunningDocumentsForBusinessObject(IDocumentSupportable topLevelBO)
		{
			var hasFetchStrategy = ((BusinessObject)topLevelBO).FetchStrategy is IDocumentSupporterFetchStrategy;
			if (hasFetchStrategy)
			{
				Factory.Save();
			}

			string tempPath = Path.Combine(Temp.TempPath, "DocTest_" + Guid.NewGuid().ToString());

			try
			{
				Directory.CreateDirectory(tempPath);

				var helper = new RunningDocumentsHelper();
				var documentCommands = Factory.Load<IDocumentCommand>(GetDocumentCommandQuery(topLevelBO));

				foreach (IDocumentCommand documentCommand in documentCommands)
				{
					void runDocument()
					{
						helper.SetDocumentCommandParent(documentCommand, topLevelBO);
						if (!ExcludeDocumentCommandTest(documentCommand) && documentCommand.SU_IsSystemDefined && documentCommand.SU_IsPublished && documentCommand.IsApplicable && documentCommand.SU_MenuType == StmMenuItemTypes.Documents)
						{
							var bizoForTest = hasFetchStrategy ? GetDocumentSupportableBizoInOtherFactory(topLevelBO, documentCommand, helper) : topLevelBO;

							using (bizoForTest.DocumentSupporter.InitialiseFetchStrategy())
							using (hasFetchStrategy ? AssertMaxDbHitsForAllFactories(documentCommand, bizoForTest) : null)
							{
								DoSetupForDocument(documentCommand, bizoForTest);

								using (var documentPrintSet = helper.CreateDocumentPrintSet(documentCommand, bizoForTest))
								{
									try
									{
										helper.RunDocumentPrintSet(documentPrintSet, tempPath);
									}
									catch (Exception e)
									{
										Fail("Exception encountered while running " + bizoForTest.DocumentSupporter.BusinessContext.ToString() + "'s " + documentCommand.SU_MenuName + System.Environment.NewLine + e.ToString());
									}
								}
							}

							int filesFound = helper.DeleteTempFilesReturningCount(tempPath);
							Assert("Xls files should have been generated for " + documentCommand.SU_MenuName + " in " + tempPath + ".  For document packs, make sure that you set up the child documents so that there will be something to generate a xls report for.", filesFound > 0);
						}
					}

					runDocument();

					if (Factory.Exists(ObjectFactory.GetType<IStmMenuDocumentConfig>(), GetTemplateConfigQuery(documentCommand.PK, true)))
					{
						var list = Factory.Load<IStmMenuDocumentConfig>(GetTemplateConfigQuery(documentCommand.PK));
						list.ForEach(e => e.S3_IsTemplate = true);

						foreach (var config in list)
						{
							config.S3_IsTemplate = false;
							runDocument();
							config.S3_IsTemplate = true;
						}

						list.ForEach(e => e.S3_IsTemplate = (ZBool)e.S3_IsTemplateInfo.OriginalValue);
					}
				}

				Assert(true);
			}
			finally
			{
				try
				{
					Directory.Delete(tempPath, true);
				}
				// If there's a previous failure and this blows an exception too,
				// unless we swallow this one we lose the first won't know what caused the problem.
				catch { }
			}
		}

		protected virtual IEnumerable<IDocumentSupportable> TopLevelBOsForMessageNotPrintingTest
		{
			get { yield return GetDocumentSupportableBusinessObject(); }
		}

		protected virtual IEnumerable<IDocumentSupportable> TopLevelBOsForRunningDocumentsTest
		{
			get { yield return GetDocumentSupportableBusinessObjectForRunningDocuments(); }
		}

		protected virtual bool ExcludeDocumentCommandTest(IDocumentCommand documentCommand)
		{
			return documentCommand.SU_MenuType == Constants.StmMenuItemTypes.OperationalActions && !documentCommand.HasChildMenus;
		}

		/// <summary>
		/// This is called before each document is run during the test.
		/// </summary>
		protected virtual void DoSetupForDocument(IDocumentCommand documentCommand, IDocumentSupportable documentSupportableBO)
		{
		}

		IDocumentSupportable GetDocumentSupportableBizoInOtherFactory(IDocumentSupportable documentSupportableBO, IDocumentCommand documentCommand, RunningDocumentsHelper helper)
		{
			var bizo = (BusinessObject)documentSupportableBO;
			var otherBizo = (IDocumentSupportable)new BusinessObjectFactory().Load(bizo.GetType(), bizo.PK);
			helper.SetDocumentCommandParent(documentCommand, otherBizo);
			return otherBizo;
		}

		IDisposable AssertMaxDbHitsForAllFactories(IDocumentCommand documentCommand, IDocumentSupportable documentSupportableBO)
		{
			var testFactory = ((BusinessObject)documentSupportableBO).Factory;
			var tablesToCollectQueriesFor = TablesToCollectQueriesFor;
			var tableQueryCollector = tablesToCollectQueriesFor != null && tablesToCollectQueriesFor.Any() ? testFactory.EnableTableHitQueryCollection(tablesToCollectQueriesFor) : null;
			var dbHitCollector = AssertMaxDbHitsForAllFactories(DbHitTestingFailureMessage(documentCommand, documentSupportableBO), MaxDBHitCounts, ignoreUnspecified: true, thresholdForUnspecified: 1, includeFactoryPredicate: (BusinessObjectFactory f) => f == testFactory);
			return new DisposableAction(() =>
			{
				dbHitCollector.Dispose();
				tableQueryCollector?.Dispose();
			});
		}

		protected virtual string[] TablesToCollectQueriesFor => null;

		protected virtual string DbHitTestingFailureMessage(IDocumentCommand documentCommand, IDocumentSupportable documentSupportableBO) => documentCommand.MenuItemUniqueCode;

		protected virtual Dictionary<string, int> MaxDBHitCounts => new Dictionary<string, int>();

		ZQuery GetDocumentCommandQuery(IDocumentSupportable topLevelBO)
		{
			var query = new ZQuery(StmMenuItemSchema.SU_BusinessContext, topLevelBO.DocumentSupporter.BusinessContext);
			query.AddToFilter(StmMenuItemSchema.SU_MenuType, new string[] { Constants.StmMenuItemTypes.Documents, Constants.StmMenuItemTypes.WebReports });
			return query;
		}

		ZQuery GetTemplateConfigQuery(ZGuid documentPK, bool templatesOnly = false)
		{
			var subQueryTemplatePivot = new ZDBOnlySubQuery(typeof(IStmMenuTemplatePivot), StmMenuTemplatePivotSchema.PK);
			subQueryTemplatePivot.AddToFilter(StmMenuTemplatePivotSchema.SI_SU, SQLComparisonOperator.Equal, documentPK);

			var query = new ZDBOnlyQuery(ObjectFactory.GetType<IStmMenuDocumentConfig>());
			if (templatesOnly)
			{
				query.AddToFilter(StmMenuDocumentConfigSchema.S3_IsTemplate, true);
			}
			query.AddSubQuery(StmMenuDocumentConfigSchema.S3_SI, subQueryTemplatePivot, JoinCondition.And);

			return query;
		}

		class RunningDocumentsHelper
		{
			public RunningDocumentsHelper()
			{
				DocumentNoteType = Type.GetType("Enterprise.DocumentEngine.DocumentNote, Enterprise.DocumentEngine");
				DocumentNoteLoadNoteMethod = DocumentNoteType.GetMethod("LoadNote", BindingFlags.Public | BindingFlags.Static);
				DocumentNoteUserDefinedFieldList = DocumentNoteType.GetProperty("UserDefinedFieldList");

				DeliveryInstructionsType = Type.GetType("Enterprise.DocumentEngine.DeliveryInstructions, Enterprise.DocumentEngine");
				DeliveryInstructionsDestination = DeliveryInstructionsType.GetProperty("Destination");
				DeliveryInstructionsOutputDirectory = DeliveryInstructionsType.GetProperty("OutputDirectory");

				DocumentPrintSetType = Type.GetType("Enterprise.DocumentEngine.DocumentPrintSet, Enterprise.DocumentEngine");
				DocumentPrintSetRunMethod = DocumentPrintSetType.GetMethod("Run", new Type[] { DeliveryInstructionsType, typeof(INotifications) });

				DocumentCommandParent = Type.GetType("Enterprise.DocumentEngine.DocumentCommand, Enterprise.DocumentEngine").GetProperty("Parent");
			}

			readonly Type DocumentNoteType;
			readonly MethodInfo DocumentNoteLoadNoteMethod;
			readonly PropertyInfo DocumentNoteUserDefinedFieldList;
			readonly Type DeliveryInstructionsType;
			readonly PropertyInfo DeliveryInstructionsDestination;
			readonly PropertyInfo DeliveryInstructionsOutputDirectory;
			readonly Type DocumentPrintSetType;
			readonly MethodInfo DocumentPrintSetRunMethod;
			readonly PropertyInfo DocumentCommandParent;

			Dictionary<IDocumentSupportable, object> CachedDocumentNotes => cachedDocumentNotes ?? (cachedDocumentNotes = new Dictionary<IDocumentSupportable, object>());
			Dictionary<IDocumentSupportable, object> cachedDocumentNotes;

			public void SetDocumentCommandParent(IDocumentCommand documentCommand, IDocumentSupportable documentSupportableBO)
			{
				if (DocumentCommandParent.GetValue(documentCommand, null) != documentSupportableBO)
				{
					DocumentCommandParent.SetValue(documentCommand, documentSupportableBO, null);
				}
			}

			public IDisposable CreateDocumentPrintSet(IDocumentCommand documentCommand, IDocumentSupportable documentSupportableBO)
			{
				var userDefinedFieldList = GetUserDefinedFieldList(documentSupportableBO);
				return (IDisposable)Activator.CreateInstance(DocumentPrintSetType, documentCommand, userDefinedFieldList);
			}

			public void RunDocumentPrintSet(IDisposable documentPrintSet, ZString tempPath)
			{
				var deliveryInstructions = GetDeliveryInstructions(tempPath);
				DocumentPrintSetRunMethod.Invoke(documentPrintSet, new object[] { deliveryInstructions, new NotificationCollection() });
			}

			object GetDeliveryInstructions(ZString tempPath)
			{
				var deliveryInstructions = Activator.CreateInstance(DeliveryInstructionsType);
				DeliveryInstructionsDestination.SetValue(deliveryInstructions, DeliveryInstructionDestination.Disk, null);
				DeliveryInstructionsOutputDirectory.SetValue(deliveryInstructions, tempPath, null);
				return deliveryInstructions;
			}

			object GetUserDefinedFieldList(IDocumentSupportable documentSupportableBO)
			{
				object documentNote = GetDocumentNote(documentSupportableBO);
				return documentNote != null ? DocumentNoteUserDefinedFieldList.GetValue(documentNote, null) : null;
			}

			object GetDocumentNote(IDocumentSupportable documentSupportableBO)
			{
				if (!CachedDocumentNotes.TryGetValue(documentSupportableBO, out object documentNote))
				{
					if (documentSupportableBO is IStmNoteParent)
					{
						documentNote = DocumentNoteLoadNoteMethod.Invoke(null, new object[] { documentSupportableBO });
						CachedDocumentNotes[documentSupportableBO] = documentNote;
					}
				}
				return documentNote;
			}

			public int DeleteTempFilesReturningCount(string tempPath)
			{
				string[] filesFound = Directory.GetFiles(tempPath);
				foreach (string fileName in filesFound)
				{
					try
					{
						File.Delete(fileName);
					}
					catch
					{
						System.Threading.Thread.Sleep(5000);
						try
						{
							File.Delete(fileName);
						}
						catch
						{
							System.Threading.Thread.Sleep(10000);
							try
							{
								File.Delete(fileName);
							}
							catch (Exception e)
							{
								Fail("Could not Delete File on Third Attempt after 15 seconds.\r\nFilename: " + fileName + "\r\nException Thrown: " + e.Message + "\r\n");
							}
						}
					}
				}
				return filesFound.Length;
			}
		}

		protected ZString StoredCountry;
		IGlbCompany currentCompany;

		protected override void SetUp()
		{
			IGlbCompany glbCompany = Factory.New<IGlbCompany>();
			currentCompany = (IGlbCompany)glbCompany.GetType().InvokeMember("CurrentCompany", BindingFlags.Public | BindingFlags.Static | BindingFlags.GetProperty, null, null, null);

			StoredCountry = currentCompany.GC_RN_NKCountryCode;

			if (!string.IsNullOrEmpty(TestCountryCode))
			{
				currentCompany.SetCountry(TestCountryCode);
			}

			string clientXmlFilePath = GetClientXmlFilePath();
			if (clientXmlFilePath != null)
			{
				var clientDocumentsUpgradeTaskType = Type.GetType("Enterprise.DbUpgrader.Data.ClientDocumentsUpgradeTask, Enterprise.DbUpgrader.Data.Documents");
				var task = Activator.CreateInstance(clientDocumentsUpgradeTaskType, clientXmlFilePath);
				clientDocumentsUpgradeTaskType.InvokeMember("Run", BindingFlags.InvokeMethod, null, task, null);
			}

			base.SetUp();
		}

		protected virtual string GetClientXmlFilePath()
		{
			return null;
		}

		protected virtual ZString TestCountryCode
		{
			get { return currentCompany.GC_RN_NKCountryCode; }
		}

		protected override void TearDown()
		{
			currentCompany.SetCountry(StoredCountry);
			base.TearDown();
		}

		protected virtual IDocumentSupportable GetDocumentSupportableBusinessObjectForRunningDocuments()
		{
			return GetDocumentSupportableBusinessObject();
		}

		protected abstract IDocumentSupportable GetDocumentSupportableBusinessObject();
	}
}
