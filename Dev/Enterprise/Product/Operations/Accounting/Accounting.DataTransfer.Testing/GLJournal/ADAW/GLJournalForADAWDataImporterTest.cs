using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Billing.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLJournals.Testing
{
	sealed class GLJournalForADAWDataImporterTest : MultiCompaniesGLJournalFlatFileDataImporterTest
	{
		public new void TestExtractToDataAdapter()
		{
			var collection = SetUpADAWCollection();
			var importer = new GLJournalForADAWDataImporterTestClass(collection);
			var xsd = new Xsd.GLJournalCollection();
			importer.CreateConverter(new NotificationBuffer()).ImportFlatFile(xsd, null, null);
			importer.ExtractToDataAdapter(xsd, new NotificationBuffer());

			AssertEquals("Should import 2 GL journals.", 2, importer.ImportedJournals_ForTestOnly.Count);
			AssertEquals("EDI", importer.ImportedJournals_ForTestOnly[0].Company.GC_Code);
			AssertEquals("EDI", importer.ImportedJournals_ForTestOnly[1].Company.GC_Code);
		}

		public void TestImportADAWWithAttribute()
		{
			AccountingMasterFilesRegistry.Instance.EnableReportingBooksFeature.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			ObjectCreater.CreateTestPeriodsForEntireYear(2022);
			var collection = new GLJournalHeaderForADAWCollection(Factory, "H");
			SetupGLJournalHeaderForADAW(collection.AddNew());
			var chart = ObjectCreater.CreateAlternateChart("MGT");
			ObjectCreater.CreateAccAlternateChartFormat(chart, 1, "9");
			ObjectCreater.CreateAccAlternateGLAccountDissection(GlHeader, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, false);
			ObjectCreater.CreateAccAlternateGLAccountDissection(GlHeader, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, false);
			ObjectCreater.CreateAccAlternateGLAccountDissection(GlHeader, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, false);
			ObjectCreater.CreateAccAlternateGLAccountDissection(GlHeader, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, false);
			ObjectCreater.CreateAccAlternateGLAccountDissection(GlHeader, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, false);
			ObjectCreater.CreateAccAlternateGLAccountDissection(GlHeader, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR, false);
			ObjectCreater.CreateAccAlternateGLAccountDissection(GlHeader2, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, false);
			ObjectCreater.CreateAccAlternateGLAccountDissection(GlHeader2, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, false);
			ObjectCreater.CreateAccAlternateGLAccountDissection(GlHeader2, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, false);
			ObjectCreater.CreateAccAlternateGLAccountDissection(GlHeader2, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, false);
			ObjectCreater.CreateAccAlternateGLAccountDissection(GlHeader2, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, false);
			ObjectCreater.CreateAccAlternateGLAccountDissection(GlHeader2, chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR, false);

			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GlHeader.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GlHeader2.PK.ToGuid());
			var org = ObjectCreater.CreateOrgHeader("org", true, true);
			Factory.Save();

			collection[0].GLJournalLines[0].AttributeORG = "Zorg";
			collection[0].GLJournalLines[0].AttributeOCG = "TPY";
			collection[0].GLJournalLines[0].AttributeLFO = "LOC";
			collection[0].GLJournalLines[0].AttributeLFE = "WEU";
			collection[0].GLJournalLines[0].AttributeTIC = "STI";
			collection[0].GLJournalLines[0].AttributeSPR = "SPR";
			collection[0].GLJournalLines[1].AttributeORG = "Zorg";
			collection[0].GLJournalLines[1].AttributeOCG = "INT";
			collection[0].GLJournalLines[1].AttributeLFO = "FOR";
			collection[0].GLJournalLines[1].AttributeLFE = "OEU";
			collection[0].GLJournalLines[1].AttributeTIC = "ETI";
			collection[0].GLJournalLines[1].AttributeSPR = "SPS";

			var importer = new GLJournalForADAWDataImporterTestClass(collection);
			var xsd = new Xsd.GLJournalCollection();
			var notification = new NotificationBuffer();
			importer.CreateConverter(notification).ImportFlatFile(xsd, null, null);
			Assert(!notification.HasErrors);
			importer.ExtractToDataAdapter(xsd, new NotificationBuffer());
			var result = importer.LastImportedJournal.Factory.Load<AccTransactionLineDissectionAttribute>(new ZQuery());

			CombineAssertions(() =>
			{
				AssertEquals(12, result.Length);
				AssertEquals(2, result.Where(r => r.ALD_Attribute == "ORG" && r.ALD_AttributeValueID == org.PK && r.ALD_AttributeValue == "").Count());
				AssertEquals(2, result.Where(r => r.ALD_Attribute == "OCG" && (r.ALD_AttributeValue == "TPY" || r.ALD_AttributeValue == "INT")).Count());
				AssertEquals(2, result.Where(r => r.ALD_Attribute == "LFO" && (r.ALD_AttributeValue == "LOC" || r.ALD_AttributeValue == "FOR")).Count());
				AssertEquals(2, result.Where(r => r.ALD_Attribute == "LFE" && (r.ALD_AttributeValue == "WEU" || r.ALD_AttributeValue == "OEU")).Count());
				AssertEquals(2, result.Where(r => r.ALD_Attribute == "TIC" && (r.ALD_AttributeValue == "STI" || r.ALD_AttributeValue == "ETI")).Count());
				AssertEquals(2, result.Where(r => r.ALD_Attribute == "SPR" && (r.ALD_AttributeValue == "SPR" || r.ALD_AttributeValue == "SPS")).Count());
			});
		}

		public void TestImportFlatFileLackDateAndInvalidPostPeriod()
		{
			AssertImportFlatFileLackDate(TransactionTypes.GLStandardJournal, "180001", "202212");
			AssertImportFlatFileLackDate(TransactionTypes.GLStandardJournal, "180001A", "202212");
		}

		public void TestImportFlatFileLackDateAndInvalidReversePeriod_AJL()
		{
			AssertImportFlatFileLackDate(TransactionTypes.GLAutoJournal, "202212", "180001");
			AssertImportFlatFileLackDate(TransactionTypes.GLAutoJournal, "202212", "180001A");
		}

		public void TestImportFlatFileLackDateAndInvalidReversePeriod_RJL()
		{
			AssertImportFlatFileLackDate(TransactionTypes.GLReversingJournal, "202212", "180001");
			AssertImportFlatFileLackDate(TransactionTypes.GLReversingJournal, "202212", "180001A");
		}

		public void TestImportFlatFileLackDateAndPostPeriod()
		{
			AssertImportFlatFileLackDate(TransactionTypes.GLStandardJournal, string.Empty, "202212", isLackBothDateAndPeriod: true);
		}

		public void TestImportFlatFileLackDateAndReversePeriod_AJL()
		{
			AssertImportFlatFileLackDate(TransactionTypes.GLAutoJournal, "202212", string.Empty, isLackBothDateAndPeriod: true);
		}

		public void TestImportFlatFileLackDateAndReversePeriod_RJL()
		{
			AssertImportFlatFileLackDate(TransactionTypes.GLReversingJournal, "202212", string.Empty, isLackBothDateAndPeriod: true);
		}

		void AssertImportFlatFileLackDate(string journalType, string postPeriod, string reverseOrEndPeriod, bool isLackBothDateAndPeriod = false)
		{
			var collection = SetUpADAWCollection();
			collection[0].PostPeriod = postPeriod;
			collection[0].ReverseOrEndPeriod = reverseOrEndPeriod;
			collection[0].JournalType = journalType;
			var importer = new GLJournalForADAWDataImporterTestClass(collection);
			var xsd = new Xsd.GLJournalCollection();
			var notificationBuffer = new NotificationBuffer();
			importer.CreateConverter(notificationBuffer).ImportFlatFile(xsd, null, null);

			string periodType;
			string exceptPeriod;

			if (journalType == TransactionTypes.GLReversingJournal || journalType == TransactionTypes.GLAutoJournal)
			{
				periodType = "Reverse/Ending";
				exceptPeriod = reverseOrEndPeriod;
			}
			else
			{
				periodType = "Post";
				exceptPeriod = postPeriod;
			}

			if (isLackBothDateAndPeriod)
			{
				if (periodType == "Post")
				{
					AssertEquals("Error: Header[1].EDI - No Post Date or Post Period specified. Please specify at least one of the values.", notificationBuffer.Events[0].Message);
				}
				else
				{
					AssertEquals("Error: Header[1].EDI - No Reverse/Ending Period specified. Please specify a value.", notificationBuffer.Events[0].Message);
				}
			}
			else
			{
				AssertEquals("Error: Header[1].EDI - " + periodType + " period " + exceptPeriod + " is invalid. Please check specified value against your Period Management setup.", notificationBuffer.Events[0].Message);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestImportData()
		{
			var collection = SetUpADAWCollection();
			var importer = new GLJournalForADAWDataImporterTestClass(collection);
			var filePath = BaseSourcePath + @"Enterprise\Product\Operations\Accounting\Accounting.DataTransfer\GLJournal\Testing\MultiCompaniesGLJournal\ValidGJLJournal.csv";
			var notificationBuffer = new NotificationBuffer();

			importer.ImportData(filePath, notificationBuffer, null);

			var newFactory = new BusinessObjectFactory();
			var journals = newFactory.Load<GLJournal>(new ZQuery());
			AssertEquals(2, journals.Length);
			AssertEquals(GenApprovalRequestApprovalStatus.Posted, journals[0].ApprovalRequestStatus);
			AssertEquals(GenApprovalRequestApprovalStatus.Posted, journals[1].ApprovalRequestStatus);

			var fileName = "ValidGJLJournal.csv";
			AssertFileOnEDocs(journals[0], fileName);
			AssertFileOnEDocs(journals[1], fileName);
		}

		void AssertFileOnEDocs(IDocManagerSupport docParent, string fileName)
		{
			var docManagerInfo = docParent.DocManagerInfo;
			AssertEquals("One eDoc is attached.", 1, docManagerInfo.AllEDocs.Count);
			var eDoc = docManagerInfo.AllEDocs[0];
			AssertEquals(DocManagerCodes.GLJournal, eDoc.DocType);
			AssertEquals(fileName, eDoc.FileName);
		}

		public new void TestShouldSuspendValidation()
		{
			AssertEquals(false, fImporter.ShouldSuspendValidation_ForTestOnly);
		}

		public new void TestSupportsHistoryForDuplicatesPrevention()
		{
			AssertEquals(true, fImporter.SupportsHistoryForDuplicatesPrevention_ForTestOnly);
		}

		public new void TestDaysToKeepHistoryFor()
		{
			AssertEquals(7, fImporter.DaysToKeepHistoryFor_ForTestOnly);
		}

		public new void TestImportTypeForDuplicatesPrevention()
		{
			AssertEquals("GLJForADAW", fImporter.ImportTypeForDuplicatesPrevention_ForTestOnly);
		}

		public new void TestCheckImportHistoryInCurrentCompany()
		{
			AssertEquals(false, fImporter.ShouldCheckImportHistoryInCurrentCompany_ForTestOnly);
		}

		GLJournalHeaderForADAWCollection SetUpADAWCollection(string headerType = "H")
		{
			ObjectCreater.CreateTestPeriodsForEntireYear(2022);

			var collection = new GLJournalHeaderForADAWCollection(Factory, headerType);

			SetupGLJournalHeaderForADAW(collection.AddNew());
			SetupGLJournalHeaderForADAW(collection.AddNew());

			return collection;
		}

		void SetupGLJournalHeaderForADAW(GLJournalHeaderForADAW journalHeader)
		{
			journalHeader.JournalType = "GJL";
			journalHeader.PostPeriod = "202212";
			journalHeader.CompanyCode = "EDI";
			journalHeader.BranchCode = "BNE";
			journalHeader.JournalDescription = "Journal Description";

			var journalLine = journalHeader.GLJournalLines.AddNew();
			journalLine.CompanyCode = "EDI";
			journalLine.BranchCode = "BNE";
			journalLine.GLAccount = "1010.10.00";
			journalLine.DepartmentCode = "BRN";
			journalLine.Currency = "AUD";
			journalLine.LocalAmount = "100";
			journalLine.JournalLineDescription = "Line Description";

			journalLine = journalHeader.GLJournalLines.AddNew();
			journalLine.CompanyCode = "EDI";
			journalLine.BranchCode = "BNE";
			journalLine.GLAccount = "1010.20.00";
			journalLine.DepartmentCode = "BRN";
			journalLine.Currency = "AUD";
			journalLine.LocalAmount = "-100";
			journalLine.JournalLineDescription = "Line Description";
		}

		#region Upload GL Journal count

		protected override string UploadGLJournalCountFeatureCode => UsageFeatures.Codes.UploadGLJournalViaADAWCount;
		protected override string UploadGLJournalDetailsFeatureCode => UsageFeatures.Codes.UploadGLJournalViaADAWDetails;
		protected override string UploadGLJournalCountDescription => "Upload GL Journal via ADAW Count";
		protected override string UploadGLJournalDetailsDescription => "Upload GL Journal via ADAW Details";

		protected override void AssertCountForUploadGLJournalSucceed(IEnumerable<JObject> jObjects)
		{
			var uploadGLJournalCount = jObjects.First(json => json["FeatureCode"].ToString() == UploadGLJournalCountFeatureCode);
			AssertEquals(UsageFeatures.Modules.Accounting, uploadGLJournalCount["Module"].ToString());
			AssertEquals(UploadGLJournalCountDescription, uploadGLJournalCount["FeatureDescription"].ToString());
			AssertEquals(1, int.Parse(uploadGLJournalCount["CountOfCompanies"].ToString()));
			AssertEquals(1, int.Parse(uploadGLJournalCount["CountOfJournalTypes"].ToString()));

			var uploadGLJournalDetails = jObjects.Where(json => json["FeatureCode"].ToString() == UploadGLJournalDetailsFeatureCode);
			Assert(uploadGLJournalDetails.All(x => x["Module"].ToString() == UsageFeatures.Modules.Accounting));
			Assert(uploadGLJournalDetails.All(x => x["FeatureDescription"].ToString() == UploadGLJournalDetailsDescription));
			Assert(uploadGLJournalDetails.Any(x => x["JournalType"].ToString() == "GJL" && x["JournalCompanyCode"].ToString() == "EDI"));
		}

		protected override void AssertCountForUploadGLJournal(string fileName)
		{
			var ediMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_MessageType, SQLComparisonOperator.Equal, EDIMessageTypeList.Codes.UsageData));
			AssertNull("Pre-condition: no USG EDI message in the database", ediMessage);

			var collection = SetUpADAWCollection();
			var importer = new GLJournalForADAWDataImporterTestClass(collection);

			try
			{
				using (TextReader reader = new StreamReader(fileName))
				{
					importer.ImportDataToFactoryCore(reader, fileName, notification, out var additionalTransactionAction);
					importer.LastImportedJournal?.Factory.Save();
				}
			}
			finally
			{
				if (importer.ImportLockWithHash_ForTestOnly != null)
				{
					importer.ImportLockWithHash_ForTestOnly.Dispose();
				}
			}
		}

		#endregion

		GLJournalForADAWDataImporterTestClass fImporter;
		TestObjectCreator ObjectCreater;
		AccGLHeader GlHeader;
		AccGLHeader GlHeader2;

		protected override void SetUp()
		{
			base.SetUp();

			fImporter = new GLJournalForADAWDataImporterTestClass(new GLJournalHeaderForADAWCollection(Factory, "H"));
			ObjectCreater = new TestObjectCreator(Factory, true);
			GlHeader = ObjectCreater.CreateGLHeader("1010.10.00");
			GlHeader2 = ObjectCreater.CreateGLHeader("1010.20.00");
		}

		class GLJournalForADAWDataImporterTestClass : GLJournalForADAWDataImporter
		{
			public GLJournalForADAWDataImporterTestClass(IBusinessObjectCollection collection) : base(collection)
			{
			}

			public new IFlatFileConverter CreateConverter(INotifications notificationSubscriber)
			{
				return base.CreateConverter(notificationSubscriber);
			}

			public new bool ExtractToDataAdapter(IValueObject xsd, INotifications notifications)
			{
				return base.ExtractToDataAdapter(xsd, notifications);
			}

			public new bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
			{
				return base.ImportDataToFactoryCore(dataReader, attachmentFileName, notifications, out additionalTransactionActions);
			}

			public bool ShouldSuspendValidation_ForTestOnly => base.ShouldSuspendValidation;
			public bool SupportsHistoryForDuplicatesPrevention_ForTestOnly => SupportsHistoryForDuplicatesPrevention;
			public int DaysToKeepHistoryFor_ForTestOnly => DaysToKeepHistoryFor;
			public string ImportTypeForDuplicatesPrevention_ForTestOnly => ImportTypeForDuplicatesPrevention;
			public BusinessObjectFactoryProvider FactoryProvider_ForTestOnly => FactoryProvider;
			public bool ShouldCheckImportHistoryInCurrentCompany_ForTestOnly => ShouldCheckImportHistoryInCurrentCompany;
			public SqlApplicationLock ImportLockWithHash_ForTestOnly => ImportLockWithHash;
		}
	}
}
