using System;
using System.Globalization;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class RNSAutoSenderTest : TestCaseWithFactory
	{
		public void TestProcess()
		{
			CACustomsDataRegistry.Instance.CBSATestNetworkIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "111");
			CACustomsDataRegistry.Instance.MailBoxIDAppliesAllCountries.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "2222");
			CACustomsDataRegistry.Instance.TransmissionSite.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "333");
			var caCompany = Factory.NewWithValidTestData<GlbCompany>();
			caCompany.GC_Code = "CA1";
			caCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "AIR";
			caCompany.GC_OH_OrgProxy = org1.PK;
			var caBranch = caCompany.Branches.AddNew();
			caBranch.GB_Code = "AAA";
			var declaration = Factory.New<JobDeclaration>();
			var entryNumber = declaration.AdditionalReferenceNumbers.AddNew();
			entryNumber.CE_EntryNum = "CCN123 456";
			entryNumber.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			declaration.JE_GB = caBranch.PK;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MessageType = "REL";
			Factory.Save();
			var logger = new DummyLogger();
			var notification = new RNSUserNotificationWrapper(logger);
			new RNSAutoSender(entryHeader, notification).Process();
			AssertCollectionContains("RNS Auto Sending Log",
				string.Format(CultureInfo.InvariantCulture, "Information - Request RNS Status Query for Declaration {0} message queued for sending.",
				declaration.JE_DeclarationReference),
				logger);
		}
	}
}
