using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;
using IBaseJobDeclaration = Enterprise.Integration.Customs.IBaseJobDeclaration;
using ICommonShipment = Enterprise.Integration.Freight.ICommonShipment;

namespace Enterprise.Customs.Common.Testing
{
	class JobDeclarationFilterTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestForCompanyAndShipment()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(ICommonShipment)));
			Factory.Save();

			var cancelledDecInThisCountry = Factory.New<IBaseJobDeclaration>();
			cancelledDecInThisCountry.JE_GB = GlbBranch.CurrentBranch.PK;
			cancelledDecInThisCountry.JE_GC = GlbCompany.CurrentCompany.PK;
			cancelledDecInThisCountry.JE_JS = shipment.PK;
			cancelledDecInThisCountry.JE_IsCancelled = true;

			var decInOtherCountry = Factory.New<IBaseJobDeclaration>();
			decInOtherCountry.JE_GB = BranchInOtherCountry.PK;
			decInOtherCountry.JE_GC = BranchInOtherCountry.Company.PK;
			decInOtherCountry.JE_JS = shipment.PK;

			var filter = JobDeclarationFilter.ForCompanyAndShipment(true, GlbCompany.CurrentCompany, shipment.PK);
			var decs = Factory.Load<IBaseJobDeclaration>(filter);
			NUnit.Framework.Assert.That(decs.Length, Is.EqualTo(1), "One decs are linked to this country");
			NUnit.Framework.Assert.That(decs[0].JE_GB, Is.EqualTo(GlbBranch.CurrentBranch.PK), "Right Declaration Is Retrieved");

			filter = JobDeclarationFilter.ForCompanyAndShipment(false, GlbCompany.CurrentCompany, shipment.PK);
			decs = Factory.Load<IBaseJobDeclaration>(filter);
			NUnit.Framework.Assert.That(decs.Length, Is.EqualTo(0), "No dec is retrieved");

			var anotherFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var companyInAnotherFactory = anotherFactory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			var newBranchInAnotherFactory = companyInAnotherFactory.Branches.AddNew();
			newBranchInAnotherFactory.GB_Code = "YYY";
			newBranchInAnotherFactory.GB_RL_NKHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			var decInNewBranch = anotherFactory.New<IBaseJobDeclaration>();
			decInNewBranch.JE_GB = newBranchInAnotherFactory.PK;
			decInNewBranch.JE_GC = newBranchInAnotherFactory.Company.PK;
			decInNewBranch.JE_JS = shipment.PK;
			anotherFactory.Save();

			filter = JobDeclarationFilter.ForCompanyAndShipment(false, GlbCompany.CurrentCompany, shipment.PK);
			filter.ReLoadExistingRows = true;
			decs = Factory.Load<IBaseJobDeclaration>(filter);
			NUnit.Framework.Assert.That(decs.Length, Is.EqualTo(1), "No dec is retrieved");
		}

		[ExpectNoExceptions]
		public void TestFromShipmentInSGOnlyGetsTradeNetV4Declarations()
		{
			GlbCompany.CurrentCompany.SetCountry("SG");
			var shipment = Factory.New<ICommonShipment>();
			var declaration = Factory.New<IBaseJobDeclaration>();
			declaration.JE_GB = GlbBranch.CurrentBranch.PK;
			declaration.JE_GC = GlbCompany.CurrentCompany.PK;
			declaration.JE_JS = shipment.PK;
			declaration.JE_ApplicationCode = "";

			var filter = JobDeclarationFilter.ForCompanyAndShipment(true, GlbCompany.CurrentCompany, shipment.PK);
			var decs = Factory.Load<IBaseJobDeclaration>(filter);
			NUnit.Framework.Assert.That(decs.Length, Is.EqualTo(0), "For SG only TN4 decs should be selected");

			declaration.JE_ApplicationCode = "SG4";
			decs = Factory.Load<IBaseJobDeclaration>(filter);
			NUnit.Framework.Assert.That(decs.Length, Is.EqualTo(1), "For SG only TN4 decs should be selected");
			NUnit.Framework.Assert.That(decs[0].JE_ApplicationCode, Is.EqualTo("SG4").Using(CustomComparers.TypeComparison), "Correct TN4 Declaration Is Retrieved");
		}

		[ExpectNoExceptions]
		public void TestFromCountryCode()
		{
			var decInThisCountry = Factory.New<IBaseJobDeclaration>();
			decInThisCountry.JE_GB = GlbBranch.CurrentBranch.PK;
			decInThisCountry.JE_GC = GlbCompany.CurrentCompany.PK;
			decInThisCountry.JE_DeclarationReference = "ABC1111";

			var cancelledDecInThisCountry = Factory.New<IBaseJobDeclaration>();
			cancelledDecInThisCountry.JE_GB = BranchInCurrentCountryOnDifferentCompany.PK;
			cancelledDecInThisCountry.JE_GC = BranchInCurrentCountryOnDifferentCompany.Company.PK;
			cancelledDecInThisCountry.JE_DeclarationReference = "ABC1111";
			cancelledDecInThisCountry.JE_IsCancelled = true;

			var decInOtherCountry = Factory.New<IBaseJobDeclaration>();
			decInOtherCountry.JE_GB = BranchInOtherCountry.PK;
			decInOtherCountry.JE_GC = BranchInOtherCountry.Company.PK;
			decInOtherCountry.JE_DeclarationReference = "ABC2222";

			Factory.Save();

			var filter = JobDeclarationFilter.ForCountry(true, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Factory);

			var decs = Factory.Load<IBaseJobDeclaration>(filter);
			NUnit.Framework.Assert.That(decs.Length, Is.EqualTo(2), "Two decs are retrived");
			NUnit.Framework.Assert.That(decs.Select(x => x.PK), Is.EquivalentTo(new[] { decInThisCountry.PK, cancelledDecInThisCountry.PK }), "Right Declarations are Retrieved");
		}

		[ExpectNoExceptions]
		public void TestFromDeclarationReference()
		{
			var decInThisCountry = Factory.New<IBaseJobDeclaration>();
			decInThisCountry.JE_GB = GlbBranch.CurrentBranch.PK;
			decInThisCountry.JE_GC = GlbCompany.CurrentCompany.PK;
			decInThisCountry.JE_DeclarationReference = "ABC1111";

			var cancelledDecInThisCountry = Factory.New<IBaseJobDeclaration>();
			cancelledDecInThisCountry.JE_GB = GlbBranch.CurrentBranch.PK;
			cancelledDecInThisCountry.JE_GC = GlbCompany.CurrentCompany.PK;
			cancelledDecInThisCountry.JE_DeclarationReference = "ABC1111";
			cancelledDecInThisCountry.JE_IsCancelled = true;

			var decInOtherCountry = Factory.New<IBaseJobDeclaration>();
			decInOtherCountry.JE_GB = BranchInOtherCountry.PK;
			decInOtherCountry.JE_GC = BranchInOtherCountry.Company.PK;
			decInOtherCountry.JE_DeclarationReference = "ABC2222";

			var filter = JobDeclarationFilter.ForDeclarationReference(true, "ABC1111");

			var decs = Factory.Load<IBaseJobDeclaration>(filter);
			NUnit.Framework.Assert.That(decs.Length, Is.EqualTo(2), "Two decs are retrived");
			NUnit.Framework.Assert.That(decs[0].JE_GB, Is.EqualTo(GlbBranch.CurrentBranch.PK), "Right Declaration Is Retrieved");
			NUnit.Framework.Assert.That(decs[1].JE_GB, Is.EqualTo(GlbBranch.CurrentBranch.PK), "Right Declaration Is Retrieved");

			filter = JobDeclarationFilter.ForDeclarationReference(false, "ABC2222");
			decs = Factory.Load<IBaseJobDeclaration>(filter);
			NUnit.Framework.Assert.That(decs.Length, Is.EqualTo(0), "No decs with the number in this country are retrived");
		}

		[ExpectNoExceptions]
		public void TestFromDeclarationReferenceWhereBranchesAreScrewedUp()
		{
			var shipment = Factory.New<ICommonShipment>();

			var decInThisCountry = Factory.New<IBaseJobDeclaration>();
			decInThisCountry.JE_GB = GlbBranch.CurrentBranch.PK;
			decInThisCountry.JE_GC = GlbCompany.CurrentCompany.PK;
			decInThisCountry.JE_JS = shipment.PK;

			var decInOtherCountry = Factory.New<IBaseJobDeclaration>();
			decInOtherCountry.JE_GB = BranchInOtherCountry.PK;
			decInOtherCountry.JE_GC = BranchInOtherCountry.Company.PK;
			decInOtherCountry.JE_JS = shipment.PK;

			var decInCurrentCountryOnDifferentCompany = Factory.New<IBaseJobDeclaration>();
			decInCurrentCountryOnDifferentCompany.JE_GB = BranchInCurrentCountryOnDifferentCompany.PK;
			decInCurrentCountryOnDifferentCompany.JE_GC = BranchInCurrentCountryOnDifferentCompany.Company.PK;
			decInCurrentCountryOnDifferentCompany.JE_JS = shipment.PK;

			var filter = JobDeclarationFilter.ForDeclarationReference(false, ZString.Empty);

			var decs = Factory.Load<IBaseJobDeclaration>(filter);
			NUnit.Framework.Assert.That(decs.Length, Is.EqualTo(1), "Only one of decs is linked to this country");
			NUnit.Framework.Assert.That(decs[0].PK, Is.EqualTo(decInThisCountry.PK), "Right Declaration Is Retrieved");
		}

		[ExpectNoExceptions]
		public void TestGetEntryStatusQueryAllEntries()
		{
			var declaration0 = Factory.New<IBaseJobDeclaration>();

			var declaration1 = Factory.New<IBaseJobDeclaration>();

			var entryHeader0 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader0.CH_JE = declaration1.PK;
			entryHeader0.CH_EntryStatus = "6";

			var declaration2 = Factory.New<IBaseJobDeclaration>();
			var entryHeader1 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader1.CH_JE = declaration2.PK;
			entryHeader1.CH_EntryStatus = "6";
			var entryHeader2 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader2.CH_JE = declaration2.PK;
			entryHeader2.CH_EntryStatus = "1";

			var declaration3 = Factory.New<IBaseJobDeclaration>();
			var entryHeader3 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader3.CH_JE = declaration3.PK;
			entryHeader3.CH_EntryStatus = "6";
			var entryHeader4 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader4.CH_JE = declaration3.PK;
			entryHeader4.CH_EntryStatus = "6";

			var declaration4 = Factory.New<IBaseJobDeclaration>();
			var entryHeader5 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader5.CH_JE = declaration4.PK;
			entryHeader5.CH_EntryStatus = ZString.Empty;
			var entryHeader6 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader6.CH_JE = declaration4.PK;
			entryHeader6.CH_EntryStatus = ZString.Empty;

			var declaration5 = Factory.New<IBaseJobDeclaration>();
			var entryHeader7 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader7.CH_JE = declaration5.PK;
			entryHeader7.CH_EntryStatus = "6";

			var declaration6 = Factory.New<IBaseJobDeclaration>();
			var entryHeader8 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader8.CH_JE = declaration6.PK;
			entryHeader8.CH_EntryStatus = "1";

			Factory.Save();

			var filter = JobDeclarationFilter.GetEntryStatusQueryAllEntries(SQLComparisonOperator.Equal, "6");
			var filteredDecs = Factory.Load<IBaseJobDeclaration>(filter);
			NUnit.Framework.Assert.That(filteredDecs.Select(x => x.PK), Is.EquivalentTo(new[] { declaration1.PK, declaration3.PK, declaration5.PK }));

			filter = JobDeclarationFilter.GetEntryStatusQueryAllEntries(SQLComparisonOperator.Equal, "1");
			filteredDecs = Factory.Load<IBaseJobDeclaration>(filter);
			NUnit.Framework.Assert.That(filteredDecs.Select(x => x.PK), Is.EquivalentTo(new[] { declaration6.PK }));

			filter = JobDeclarationFilter.GetEntryStatusQueryAllEntries(SQLComparisonOperator.Equal, "NOT");
			filteredDecs = Factory.Load<IBaseJobDeclaration>(filter);
			NUnit.Framework.Assert.That(filteredDecs.Select(x => x.PK), Is.EquivalentTo(new[] { declaration4.PK, declaration0.PK }));

			filter = JobDeclarationFilter.GetEntryStatusQueryAllEntries(SQLComparisonOperator.Equal, "");
			filteredDecs = Factory.Load<IBaseJobDeclaration>(filter);
			NUnit.Framework.Assert.That(filteredDecs.Select(x => x.PK), Is.EquivalentTo(new[] { declaration4.PK, declaration0.PK }));
		}

		[ExpectNoExceptions]
		public void TestGetEntryPhaseStatusQueryAllEntries()
		{
			var declaration0 = Factory.New<IBaseJobDeclaration>();

			var declaration1 = Factory.New<IBaseJobDeclaration>();

			var entryHeader0 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader0.CH_JE = declaration1.PK;
			entryHeader0.CH_PhaseStatus = "6";

			var declaration2 = Factory.New<IBaseJobDeclaration>();
			var entryHeader1 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader1.CH_JE = declaration2.PK;
			entryHeader1.CH_PhaseStatus = "6";
			var entryHeader2 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader2.CH_JE = declaration2.PK;
			entryHeader2.CH_PhaseStatus = "1";

			var declaration3 = Factory.New<IBaseJobDeclaration>();
			var entryHeader3 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader3.CH_JE = declaration3.PK;
			entryHeader3.CH_PhaseStatus = "6";
			var entryHeader4 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader4.CH_JE = declaration3.PK;
			entryHeader4.CH_PhaseStatus = "6";

			var declaration4 = Factory.New<IBaseJobDeclaration>();
			var entryHeader5 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader5.CH_JE = declaration4.PK;
			entryHeader5.CH_PhaseStatus = ZString.Empty;
			var entryHeader6 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader6.CH_JE = declaration4.PK;
			entryHeader6.CH_PhaseStatus = ZString.Empty;

			var declaration5 = Factory.New<IBaseJobDeclaration>();
			var entryHeader7 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader7.CH_JE = declaration5.PK;
			entryHeader7.CH_PhaseStatus = "6";

			var declaration6 = Factory.New<IBaseJobDeclaration>();
			var entryHeader8 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader8.CH_JE = declaration6.PK;
			entryHeader8.CH_PhaseStatus = "1";

			Factory.Save();

			var filter = JobDeclarationFilter.GetEntryPhaseStatusQueryAllEntries(SQLComparisonOperator.Equal, "6");
			var filteredDecs = Factory.Load<IBaseJobDeclaration>(filter);
			NUnit.Framework.Assert.That(filteredDecs.Select(x => x.PK), Is.EquivalentTo(new[] { declaration1.PK, declaration3.PK, declaration5.PK }));

			filter = JobDeclarationFilter.GetEntryPhaseStatusQueryAllEntries(SQLComparisonOperator.Equal, "1");
			filteredDecs = Factory.Load<IBaseJobDeclaration>(filter);
			NUnit.Framework.Assert.That(filteredDecs.Select(x => x.PK), Is.EquivalentTo(new[] { declaration6.PK }));

			filter = JobDeclarationFilter.GetEntryPhaseStatusQueryAllEntries(SQLComparisonOperator.Equal, "NOT");
			filteredDecs = Factory.Load<IBaseJobDeclaration>(filter);
			NUnit.Framework.Assert.That(filteredDecs.Select(x => x.PK), Is.EquivalentTo(new[] { declaration4.PK, declaration0.PK }));

			filter = JobDeclarationFilter.GetEntryPhaseStatusQueryAllEntries(SQLComparisonOperator.Equal, "");
			filteredDecs = Factory.Load<IBaseJobDeclaration>(filter);
			NUnit.Framework.Assert.That(filteredDecs.Select(x => x.PK), Is.EquivalentTo(new[] { declaration4.PK, declaration0.PK }));
		}

		[ExpectNoExceptions]
		public void TestGetMessageStatusQueryAllEntries()
		{
			var declaration0 = Factory.New<IBaseJobDeclaration>();

			var declaration1 = Factory.New<IBaseJobDeclaration>();
			var entryHeader0 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader0.CH_JE = declaration1.PK;
			entryHeader0.CH_Status = "SNT";

			var declaration2 = Factory.New<IBaseJobDeclaration>();
			var entryHeader1 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader1.CH_JE = declaration2.PK;
			entryHeader1.CH_Status = "SNT";
			var entryHeader2 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader2.CH_JE = declaration2.PK;
			entryHeader2.CH_Status = "ACC";

			var declaration3 = Factory.New<IBaseJobDeclaration>();
			var entryHeader3 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader3.CH_JE = declaration3.PK;
			entryHeader3.CH_Status = "SNT";
			var entryHeader4 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader4.CH_JE = declaration3.PK;
			entryHeader4.CH_Status = "SNT";

			var declaration4 = Factory.New<IBaseJobDeclaration>();
			var entryHeader5 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader5.CH_JE = declaration4.PK;
			entryHeader5.CH_Status = ZString.Empty;
			var entryHeader6 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader6.CH_JE = declaration4.PK;
			entryHeader6.CH_Status = ZString.Empty;

			var declaration5 = Factory.New<IBaseJobDeclaration>();
			var entryHeader7 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader7.CH_JE = declaration5.PK;
			entryHeader7.CH_Status = "SNT";

			var declaration6 = Factory.New<IBaseJobDeclaration>();
			var entryHeader8 = Factory.New<Integration.Customs.ICusEntryHeader>();
			entryHeader8.CH_JE = declaration6.PK;
			entryHeader8.CH_Status = "ACC";

			Factory.Save();

			var filter = JobDeclarationFilter.GetMessageStatusQueryAllEntries(SQLComparisonOperator.Equal, "SNT");
			var filteredDecs = Factory.Load<IBaseJobDeclaration>(filter);
			NUnit.Framework.Assert.That(filteredDecs.Select(x => x.PK), Is.EquivalentTo(new[] { declaration1.PK, declaration3.PK, declaration5.PK }));

			filter = JobDeclarationFilter.GetMessageStatusQueryAllEntries(SQLComparisonOperator.Equal, "ACC");
			filteredDecs = Factory.Load<IBaseJobDeclaration>(filter);
			NUnit.Framework.Assert.That(filteredDecs.Select(x => x.PK), Is.EquivalentTo(new[] { declaration6.PK }));

			filter = JobDeclarationFilter.GetMessageStatusQueryAllEntries(SQLComparisonOperator.Equal, "NOT");
			filteredDecs = Factory.Load<IBaseJobDeclaration>(filter);
			NUnit.Framework.Assert.That(filteredDecs.Select(x => x.PK), Is.EquivalentTo(new[] { declaration4.PK, declaration0.PK }));

			filter = JobDeclarationFilter.GetMessageStatusQueryAllEntries(SQLComparisonOperator.Equal, "");
			filteredDecs = Factory.Load<IBaseJobDeclaration>(filter);
			NUnit.Framework.Assert.That(filteredDecs.Select(x => x.PK), Is.EquivalentTo(new[] { declaration4.PK, declaration0.PK }));
		}

		#region Implementation

		GlbBranch BranchInCurrentCountryOnDifferentCompany
		{
			get
			{
				if (fBranchInCurrentCountryOnDifferentCompany == null)
				{
					var company = Factory.NewWithValidTestData<GlbCompany>();
					company.GC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					company.GC_Code = "X" + company.GC_RN_NKCountryCode;
					fBranchInCurrentCountryOnDifferentCompany = Factory.NewWithValidTestData<GlbCompany>().Branches.AddNew();
					fBranchInCurrentCountryOnDifferentCompany.GB_Code = "Y" + company.GC_RN_NKCountryCode;
					fBranchInCurrentCountryOnDifferentCompany.GB_RL_NKHomePort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
				}
				return fBranchInCurrentCountryOnDifferentCompany;
			}
		}
		GlbBranch fBranchInCurrentCountryOnDifferentCompany;

		GlbBranch BranchInOtherCountry
		{
			get
			{
				if (fBranchInOtherCountry == null)
				{
					var company = Factory.NewWithValidTestData<GlbCompany>();
					company.GC_RN_NKCountryCode = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).RN_Code;
					company.GC_Code = "X" + company.GC_RN_NKCountryCode;
					fBranchInOtherCountry = company.Branches.AddNew();
					fBranchInOtherCountry.GB_Code = "Y" + company.GC_RN_NKCountryCode;
					fBranchInOtherCountry.GB_RL_NKHomePort = "XXYYY";
				}
				return fBranchInOtherCountry;
			}
		}
		GlbBranch fBranchInOtherCountry;

		#endregion
	}
}
