using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.DIF.Business.Testing
{
	sealed class DIFDocumentLookupsTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var difDocument = new DIFDocument(HostWrapper);
			AssertEquals("PGAs", typeof(PGACodes), difDocument.Lookups.PGAs.GetType());
		}

		public void TestEDocsList()
		{
			var declaration = (ICADIFHost)JobDeclaration;
			var docManagerSupport = (IDocManagerSupport)declaration;
			var storageMain = docManagerSupport.DocManagerInfo.MasterFactory.RetrieveExistingOrCreateStorageMain(JobDeclaration, "TST");
			storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "Example.txt", "DT1", false);
			storageMain.AddFileOrDocument(new byte[] { 1, 2, 3 }, "Example2.txt", "DT2", false);
			var hostWrapper = new DIFHostWrapper(declaration);
			var difDocument = hostWrapper.DISDocuments.AddNew();
			var lookups = new DIFDocumentLookups(difDocument);
			var list = lookups.EDocsList;
			AssertEquals(2, list.Count);
			AssertEquals("DT1-Example.txt", list[0].Code);
			AssertEquals("DT2-Example2.txt", list[1].Code);
		}

		public void TestDocumentTypes()
		{
			var difDocument = new DIFDocument(HostWrapper);
			difDocument.PGA = PGACodes.Codes.CNSC;
			AssertEquals("DocumentTypes Count", 0, difDocument.Lookups.DocumentTypes.Count);

			var newFactory = new BusinessObjectFactory();
			var startDate = ZDateTime.UtcToday.Date.AddMonths(-1);
			var endDate = ZDateTime.UtcToday.Date.AddMonths(1);
			var helper = new UniversalReferenceTestDataHelper(newFactory);
			var code = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, "0003", "0003 DESC", startDate, endDate);
			var attributeNamePGA = helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.CADocumentTypePGAType, "PGA", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CADocumentType, Core.Constants.CountryCodes.Canada);
			var attributePGA = helper.CreateNewOrGetExistingCusCodeListAttribute(code.PK, attributeNamePGA.ZXE_Name, PGACodes.Codes.CNSC);
			newFactory.Save();

			AssertEquals("DocumentTypes Count", 0, difDocument.Lookups.DocumentTypes.Count);

			newFactory = new BusinessObjectFactory();
			var newDec = new TestHelper(newFactory).GetJobDeclaration();
			difDocument = new DIFDocument(new DIFHostWrapper((ICADIFHost)newDec));
			difDocument.PGA = PGACodes.Codes.CNSC;
			AssertEquals("DocumentTypes Count", 1, difDocument.Lookups.DocumentTypes.Count);
		}

		public void TestBusinessNumbers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("CA"))
			{
				var orgCurrentCompany = GlbCompany.CurrentCompany.OrgProxy;
				var currentComCodes = orgCurrentCompany.CustomsCodes.Cast<OrgCusCode>();
				if (!currentComCodes.Any(code => code.OK_CodeType == "BRL"))
				{
					orgCurrentCompany.CustomsCodes.AddNew("BRL", "BRL00001", "CA");
				}

				if (!currentComCodes.Any(code => code.OK_CodeType == "BRC"))
				{
					orgCurrentCompany.CustomsCodes.AddNew("BRC", "BRC00001", "CA");
				}

				if (!currentComCodes.Any(code => code.OK_CodeType == "BRT"))
				{
					orgCurrentCompany.CustomsCodes.AddNew("BRT", "BRT00001", "CA");
				}

				if (!currentComCodes.Any(code => code.OK_CodeType == "BRP"))
				{
					orgCurrentCompany.CustomsCodes.AddNew("BRP", "BRP00001", "CA");
				}

				if (!currentComCodes.Any(code => code.OK_CodeType == "BRM"))
				{
					orgCurrentCompany.CustomsCodes.AddNew("BRM", "BRM00001", "CA");
				}

				if (!currentComCodes.Any(code => code.OK_CodeType == "BRB"))
				{
					orgCurrentCompany.CustomsCodes.AddNew("BRB", "BRB00001", "CA");
				}

				if (!currentComCodes.Any(code => code.OK_CodeType == "BNC"))
				{
					orgCurrentCompany.CustomsCodes.AddNew("BNC", "BNC00001", "CA");
				}

				if (!currentComCodes.Any(code => code.OK_CodeType == "CAI"))
				{
					orgCurrentCompany.CustomsCodes.AddNew("CAI", "CAI00001", "CA");
				}

				var importer = (JobDeclaration as BaseJobDeclaration).Importer;
				importer.OH_Code = "COM";
				var importerCodes = importer.CustomsCodes.Cast<OrgCusCode>();
				if (!importerCodes.Any(code => code.OK_CodeType == "BRL"))
				{
					importer.CustomsCodes.AddNew("BRL", "BRL00002", "CA");
				}

				if (!importerCodes.Any(code => code.OK_CodeType == "BRC"))
				{
					importer.CustomsCodes.AddNew("BRC", "BRC00002", "CA");
				}

				if (!importerCodes.Any(code => code.OK_CodeType == "BRT"))
				{
					importer.CustomsCodes.AddNew("BRT", "BRT00002", "CA");
				}

				if (!importerCodes.Any(code => code.OK_CodeType == "BRP"))
				{
					importer.CustomsCodes.AddNew("BRP", "BRP00002", "CA");
				}

				if (!importerCodes.Any(code => code.OK_CodeType == "BRM"))
				{
					importer.CustomsCodes.AddNew("BRM", "BRM00002", "CA");
				}

				if (!importerCodes.Any(code => code.OK_CodeType == "BRB"))
				{
					importer.CustomsCodes.AddNew("BRB", "BRB00002", "CA");
				}

				if (!importerCodes.Any(code => code.OK_CodeType == "BNC"))
				{
					importer.CustomsCodes.AddNew("BNC", "BNC00002", "CA");
				}

				if (!importerCodes.Any(code => code.OK_CodeType == "CAI"))
				{
					importer.CustomsCodes.AddNew("CAI", "CAI00002", "CA");
				}

				var difDocument = new DIFDocument(HostWrapper);

				Factory.Save();

				AssertEquals(16, difDocument.Lookups.BusinessNumbers.Count);
				Assert(difDocument.Lookups.BusinessNumbers.Contains(new CodeDescriptionPair("BRL00001", "BRL (Broker)")));
				Assert(difDocument.Lookups.BusinessNumbers.Contains(new CodeDescriptionPair("BRC00001", "BRC (Broker)")));
				Assert(difDocument.Lookups.BusinessNumbers.Contains(new CodeDescriptionPair("BRT00001", "BRT (Broker)")));
				Assert(difDocument.Lookups.BusinessNumbers.Contains(new CodeDescriptionPair("BRP00001", "BRP (Broker)")));
				Assert(difDocument.Lookups.BusinessNumbers.Contains(new CodeDescriptionPair("BRB00001", "BRB (Broker)")));
				Assert(difDocument.Lookups.BusinessNumbers.Contains(new CodeDescriptionPair("BNC00001", "BNC (Broker)")));
				Assert(difDocument.Lookups.BusinessNumbers.Contains(new CodeDescriptionPair("CAI00001", "CAI (Broker)")));

				Assert(difDocument.Lookups.BusinessNumbers.Contains(new CodeDescriptionPair("BRL00002", "BRL (Importer - COM)")));
				Assert(difDocument.Lookups.BusinessNumbers.Contains(new CodeDescriptionPair("BRC00002", "BRC (Importer - COM)")));
				Assert(difDocument.Lookups.BusinessNumbers.Contains(new CodeDescriptionPair("BRT00002", "BRT (Importer - COM)")));
				Assert(difDocument.Lookups.BusinessNumbers.Contains(new CodeDescriptionPair("BRP00002", "BRP (Importer - COM)")));
				Assert(difDocument.Lookups.BusinessNumbers.Contains(new CodeDescriptionPair("BRB00002", "BRB (Importer - COM)")));
				Assert(difDocument.Lookups.BusinessNumbers.Contains(new CodeDescriptionPair("BNC00002", "BNC (Importer - COM)")));
				Assert(difDocument.Lookups.BusinessNumbers.Contains(new CodeDescriptionPair("CAI00002", "CAI (Importer - COM)")));
			}
		}

		DIFHostWrapper hostWrapper;
		DIFHostWrapper HostWrapper => hostWrapper ?? (hostWrapper = new DIFHostWrapper((ICADIFHost)JobDeclaration));

		BusinessObject jobDeclaration;
		BusinessObject JobDeclaration => jobDeclaration ?? (jobDeclaration = new TestHelper(Factory).GetJobDeclaration());
	}
}
