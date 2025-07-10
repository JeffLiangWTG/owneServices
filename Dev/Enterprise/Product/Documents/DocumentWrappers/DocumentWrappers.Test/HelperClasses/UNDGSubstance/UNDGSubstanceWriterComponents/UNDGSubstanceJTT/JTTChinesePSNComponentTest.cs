using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class JTTChinesePSNComponentTest : TestCaseWithFactory
	{
		public void TestProperShippingNameIsChinese_CurrentLanguage()
		{
			var substance = Factory.New<UNDGSubstanceJTT>();
			substance.JTT_UNNO = "1234";
			substance.JTT_PSN = "English name";

			var chinesePSN = substance.Names.AddNew();
			chinesePSN.DAZ_Type = ViewUNDGAttributeLookups.TypeConstants.ProperShippingName;
			chinesePSN.DAZ_Descriptor = "罐头里的神奇豆子";

			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			undgDataItem.DI_DG = substance.PK;
			undgDataItem.LinkDefault(substance);

			Factory.Save();

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			var component = new JTTChinesePSNComponent() as IUNDGSummaryWriterComponent;
			var psn = component.Write(wrapper);

			AssertEquals("Should be English PSN", "English name", psn);

			using (Res.TemporarilySwitchLanguage(Core.Constants.Languages.ChineseSimplified))
			{
				psn = component.Write(wrapper);
				AssertEquals("Should be Chinese PSN", "罐头里的神奇豆子", psn);
			}
		}
		public void TestProperShippingNameIsChinese_StaffLanguage()
		{
			var substance = Factory.New<UNDGSubstanceJTT>();
			substance.JTT_UNNO = "1234";
			substance.JTT_PSN = "English name";

			var chinesePSN = substance.Names.AddNew();
			chinesePSN.DAZ_Type = ViewUNDGAttributeLookups.TypeConstants.ProperShippingName;
			chinesePSN.DAZ_Descriptor = "罐头里的神奇豆子";

			var undgDataItem = Factory.New<ForwardingUNDGDataItem>();
			undgDataItem.DI_DG = substance.PK;
			undgDataItem.LinkDefault(substance);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_LoginName = "PhyrexianObliterator";
			staff.GS_WorkingLanguage = Core.Constants.Languages.ChineseSimplified;

			Factory.Save();

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			var component = new JTTChinesePSNComponent() as IUNDGSummaryWriterComponent;
			var psn = component.Write(wrapper);

			AssertEquals("Should be English PSN", "English name", psn);

			var branchPK = EnvProxy.Instance.CurrentBranch.PK;
			var departmentPK = EnvProxy.Instance.CurrentDepartment.PK;
			using (EnvProxy.Instance.SetTemporaryUserContext(staff.GS_LoginName, branchPK, departmentPK))
			{
				psn = component.Write(wrapper);
				AssertEquals("Should be Chinese PSN", "罐头里的神奇豆子", psn);
			}
		}
	}
}
