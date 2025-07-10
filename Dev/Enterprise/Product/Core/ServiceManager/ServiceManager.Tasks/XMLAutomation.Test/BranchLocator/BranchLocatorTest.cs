using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Xml.XsdVersion1;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ServiceManager.Tasks.XMLAutomation.Testing
{
	abstract class BranchLocatorTest : TestCaseWithFactory
	{
		public void TestFindDoesNotDigestNulls()
		{
			var testValueObject = GetTestObjectWrapper();
			AssertExceptionThrown(typeof(ArgumentNullException), () => BranchLocator.Find(null, null));
			AssertExceptionThrown(typeof(ArgumentNullException), () => BranchLocator.Find(new XmlInterchange(), null));
			AssertExceptionThrown(typeof(ArgumentNullException), () => BranchLocator.Find(null, testValueObject));
			AssertNoExceptionThrown(() => BranchLocator.Find(new XmlInterchange(), testValueObject));
			AssertExceptionThrown(typeof(ArgumentNullException), () => BranchLocator.Find(null, ValueObjectWithEmptyPorts));
			AssertNoExceptionThrown(() => BranchLocator.Find(new XmlInterchange(), ValueObjectWithEmptyPorts));
		}

		public void TestUseBranchFromInterchange()
		{
			ImportBranchRule rule = new ImportBranchRule();
			rule.UseBranchFromXml = true;
			rule.DefaultToBranchRelatedToOriginLoadPort = 0;
			rule.DefaultToBranchRelatedToDestinationDischargePort = 0;
			rule.FallbackRule = ImportBranchRule.FallbackCodes.DefaultToAny;

			ImportBranchRuleRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);

			BranchLocatorObjectWrapper valueObject = GetTestObjectWrapper();
			XmlInterchange interchange = new XmlInterchange();

			AssertEquals("SYD|True", BranchLocator.Find(interchange, ValueObjectWithEmptyPorts).ToString());

			AssertEquals("SYD|True", BranchLocator.Find(interchange, valueObject).ToString());

			interchange.InterchangeInfo.Target.BranchCode = "GDN";

			AssertEquals("GDN|True", BranchLocator.Find(interchange, valueObject).ToString());

			interchange.InterchangeInfo.Target.BranchCode = "HGK";
			AssertEquals("HGK|True", BranchLocator.Find(interchange, valueObject).ToString());

			interchange.InterchangeInfo.Target.BranchCode = "ZZZ";
			AssertEquals("SYD|True", BranchLocator.Find(interchange, valueObject).ToString());

			interchange.InterchangeInfo.Target.BranchCode = "CHD";
			AssertEquals("SYD|True", BranchLocator.Find(interchange, valueObject).ToString());
		}

		public void TestDefaultToBranchRelatedToOriginLoadPort()
		{
			ImportBranchRule rule = new ImportBranchRule();
			rule.UseBranchFromXml = false;
			rule.DefaultToBranchRelatedToOriginLoadPort = 1;
			rule.DefaultToBranchRelatedToDestinationDischargePort = 0;
			rule.FallbackRule = ImportBranchRule.FallbackCodes.DefaultToAny;

			ImportBranchRuleRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);

			XmlInterchange interchange = new XmlInterchange();

			AssertEquals("SYD|True", BranchLocator.Find(interchange, ValueObjectWithEmptyPorts).ToString());

			BranchLocatorObjectWrapper valueObject = GetTestObjectWrapper();

			AssertEquals("SYD|True", BranchLocator.Find(interchange, valueObject).ToString());

			valueObject.SetOriginOrLoadPort("PLGDN");

			AssertEquals("GDN|True", BranchLocator.Find(interchange, valueObject).ToString());

			valueObject.SetOriginOrLoadPort("HKHKG");
			AssertEquals("HGK|True", BranchLocator.Find(interchange, valueObject).ToString());

			valueObject.SetOriginOrLoadPort("CNSHA");
			AssertEquals("extra port for HGK", "HGK|True", BranchLocator.Find(interchange, valueObject).ToString());

			valueObject.SetOriginOrLoadPort("HKKWN");
			AssertEquals("fallback on port within the same country", "HGK|True", BranchLocator.Find(interchange, valueObject).ToString());

			valueObject.SetOriginOrLoadPort("ZZZ");
			AssertEquals("SYD|True", BranchLocator.Find(interchange, valueObject).ToString());

			valueObject.SetOriginOrLoadPort("INCHD");
			AssertEquals("SYD|True", BranchLocator.Find(interchange, valueObject).ToString());
		}

		public void TestDefaultToBranchRelatedToDestinationDischargePort()
		{
			ImportBranchRule rule = new ImportBranchRule();
			rule.UseBranchFromXml = false;
			rule.DefaultToBranchRelatedToOriginLoadPort = 0;
			rule.DefaultToBranchRelatedToDestinationDischargePort = 1;
			rule.FallbackRule = ImportBranchRule.FallbackCodes.DefaultToAny;

			ImportBranchRuleRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);

			XmlInterchange interchange = new XmlInterchange();

			AssertNotNull(BranchLocator.Find(interchange, ValueObjectWithEmptyPorts));

			BranchLocatorObjectWrapper valueObject = GetTestObjectWrapper();
			AssertNotNull(BranchLocator.Find(interchange, valueObject));

			valueObject.SetDestinationOrDischargePort("PLGDN");

			AssertEquals("GDN|True", BranchLocator.Find(interchange, valueObject).ToString());

			valueObject.SetDestinationOrDischargePort("HKHKG");
			AssertEquals("HGK|True", BranchLocator.Find(interchange, valueObject).ToString());

			valueObject.SetDestinationOrDischargePort("CNSHA");
			AssertEquals("extra port for HGK", "HGK|True", BranchLocator.Find(interchange, valueObject).ToString());

			valueObject.SetDestinationOrDischargePort("HKKWN");
			AssertEquals("fallback on port within the same country", "HGK|True", BranchLocator.Find(interchange, valueObject).ToString());

			valueObject.SetDestinationOrDischargePort("ZZZ");
			AssertNotNull(BranchLocator.Find(interchange, valueObject));

			valueObject.SetDestinationOrDischargePort("INCHD");
			AssertEquals("SYD|True", BranchLocator.Find(interchange, valueObject).ToString());
		}

		public void TestDefaultToAny()
		{
			AssertEquals("prerequisite - current branch is SYD", "SYD", GlbBranch.CurrentBranch.GB_Code);

			ImportBranchRule rule = new ImportBranchRule();
			rule.UseBranchFromXml = false;
			rule.DefaultToBranchRelatedToOriginLoadPort = 0;
			rule.DefaultToBranchRelatedToDestinationDischargePort = 0;
			rule.FallbackRule = ImportBranchRule.FallbackCodes.DefaultToAny;

			ImportBranchRuleRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);

			AssertEquals("SYD|True", BranchLocator.Find(new XmlInterchange(), ValueObjectWithEmptyPorts).ToString());
			AssertEquals("SYD|True", BranchLocator.Find(new XmlInterchange(), GetTestObjectWrapper()).ToString());
		}

		public void TestDoNotCreateShipment()
		{
			ImportBranchRule rule = new ImportBranchRule();
			rule.UseBranchFromXml = true;
			rule.DefaultToBranchRelatedToOriginLoadPort = 0;
			rule.DefaultToBranchRelatedToDestinationDischargePort = 0;
			rule.FallbackRule = ImportBranchRule.FallbackCodes.DoNotCreate;

			ImportBranchRuleRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);

			XmlInterchange interchange = new XmlInterchange();

			AssertEquals("|False", BranchLocator.Find(interchange, ValueObjectWithEmptyPorts).ToString());

			BranchLocatorObjectWrapper valueObject = GetTestObjectWrapper();
			AssertEquals("|False", BranchLocator.Find(interchange, valueObject).ToString());

			interchange.InterchangeInfo.Target.BranchCode = "GDN";

			AssertEquals("GDN|True", BranchLocator.Find(interchange, valueObject).ToString());

			interchange.InterchangeInfo.Target.BranchCode = "CHD";

			AssertEquals("|False", BranchLocator.Find(interchange, valueObject).ToString());
		}

		public void TestFallback()
		{
			ImportBranchRule rule = new ImportBranchRule();
			rule.UseBranchFromXml = true;
			rule.DefaultToBranchRelatedToOriginLoadPort = 1;
			rule.DefaultToBranchRelatedToDestinationDischargePort = 2;
			rule.FallbackRule = ImportBranchRule.FallbackCodes.DoNotCreate;

			ImportBranchRuleRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);

			BranchLocatorObjectWrapper valueObject = GetTestObjectWrapper();
			XmlInterchange interchange = new XmlInterchange();

			AssertEquals("|False", BranchLocator.Find(interchange, ValueObjectWithEmptyPorts).ToString());

			rule.FallbackRule = ImportBranchRule.FallbackCodes.DefaultToAny;

			ImportBranchRuleRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, rule);
			AssertEquals("SYD|True", BranchLocator.Find(interchange, GetTestObjectWrapperWithEmptyPorts()).ToString());

			valueObject = GetTestObjectWrapper();

			AssertEquals("SYD|True", BranchLocator.Find(interchange, valueObject).ToString());

			valueObject.SetDestinationOrDischargePort("HKHKG");
			AssertEquals("HGK|True", BranchLocator.Find(interchange, valueObject).ToString());

			valueObject.SetDestinationOrDischargePort("CNSHA");
			AssertEquals("extra port for HGK", "HGK|True", BranchLocator.Find(interchange, valueObject).ToString());

			valueObject.SetDestinationOrDischargePort("HKKWN");
			AssertEquals("fallback on port within the same country as Kowloon", "HGK|True", BranchLocator.Find(interchange, valueObject).ToString());

			valueObject.SetDestinationOrDischargePort("CNSTN");
			AssertEquals("allback on port within the same country as extra port for HGK", "HGK|True", BranchLocator.Find(interchange, valueObject).ToString());

			valueObject.SetDestinationOrDischargePort("PLGDN");
			AssertEquals("GDN|True", BranchLocator.Find(interchange, valueObject).ToString());

			valueObject.SetDestinationOrDischargePort("PLWRO");
			AssertEquals("fallback on port within the same country as Wroclaw", "GDN|True", BranchLocator.Find(interchange, valueObject).ToString());

			interchange.InterchangeInfo.Target.BranchCode = "HGK";
			AssertEquals("HGK|True", BranchLocator.Find(interchange, valueObject).ToString());

			branchHGK.GB_IsActive = false;
			Factory.Save();
			AssertNotEquals("ignores inactive branch", "HGK|True", BranchLocator.Find(interchange, valueObject).ToString());
		}

		#region Implementation

		protected abstract BranchLocatorObjectWrapper GetTestObjectWrapperWithEmptyPorts();
		protected abstract BranchLocatorObjectWrapper GetTestObjectWrapper();
		protected abstract StronglyTypedRegistryItem<ImportBranchRule> ImportBranchRuleRegistryItem { get; }

		GlbBranch branchHGK;

		protected BranchLocatorObjectWrapper ValueObjectWithEmptyPorts
		{
			get { return objectWrapperWithEmptyPorts ?? (objectWrapperWithEmptyPorts = GetTestObjectWrapperWithEmptyPorts()); }
		}
		BranchLocatorObjectWrapper objectWrapperWithEmptyPorts;

		protected override void SetUp()
		{
			base.SetUp();

			GlbCompany company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);

			XMLAutomationTestHelper.DeleteAllBranchesExceptCurrentBranch(company);

			SetupCompany();

			GlbBranch branchSYD = company.Branches[0];
			branchSYD.GB_RL_NKHomePort = "AUSYD";
			branchSYD.GB_Code = "SYD";

			GlbBranch branchGDN = company.Branches.AddNew();
			branchGDN.GB_RL_NKHomePort = "PLGDN";
			branchGDN.GB_Code = "GDN";

			branchHGK = company.Branches.AddNew();
			branchHGK.GB_RL_NKHomePort = "HKHKG";
			branchHGK.GB_Code = "HGK";
			branchHGK.ExtraPorts.AddNew().GY_RL_NKAdditionalBranchRelatedPort = "CNSHA";

			GlbBranch branchMFM = company.Branches.AddNew();
			branchMFM.GB_RL_NKHomePort = "MOMFM";
			branchMFM.GB_Code = "MFM";

			GlbBranch branchCHD = company.Branches.AddNew();
			branchCHD.GB_RL_NKHomePort = "INCHD";
			branchCHD.GB_Code = "CHD";
			branchCHD.GB_IsActive = false;

			Factory.Save();
		}

		void SetupCompany()
		{
			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
			if (orgProxy == null)
			{
				orgProxy = Factory.LoadTop1<OrgHeader>(new ZQuery());
				GlbCompany.CurrentCompany.GC_OH_OrgProxy = orgProxy.PK;
			}

			var portCodePatternMatch = Factory.New<OrgPatternMatchOverride>();
			portCodePatternMatch.OO_OH = orgProxy.PK;
			portCodePatternMatch.OO_ForeignCode = "ABC";
			portCodePatternMatch.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			portCodePatternMatch.OO_LocalGuid = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "USCHI")).PK;

			portCodePatternMatch = Factory.New<OrgPatternMatchOverride>();
			portCodePatternMatch.OO_OH = orgProxy.PK;
			portCodePatternMatch.OO_ForeignCode = "SYD";
			portCodePatternMatch.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Port;
			portCodePatternMatch.OO_LocalGuid = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "AUSYD")).PK;

			Factory.Save();
		}

		#endregion
	}
}
