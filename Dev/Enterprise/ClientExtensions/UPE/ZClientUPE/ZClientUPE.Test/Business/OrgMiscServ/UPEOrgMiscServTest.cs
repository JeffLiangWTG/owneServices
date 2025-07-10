using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPEOrgMiscServ))]
	public class UPEOrgMiscServTest : EnterpriseBusinessObjectTestCase
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		public void TestCustomisationsLoadedForRegistry()
		{
			ErrorReporter.Clear();
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			Factory.New<UPEOrgMiscServ>();
			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		public void TestOM_CustomFlag4Info()
		{
			CreateUsers();
			using (Env.SetTemporaryUserContext(operationalUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				UPEOrgMiscServ miscServ = Factory.New<UPEOrgMiscServ>();
				Assert(miscServ.OM_CustomFlag4Info.ReadOnly);
				using (Env.SetTemporaryUserContext(nonOperationalUser.GS_LoginName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					Assert(!miscServ.OM_CustomFlag4Info.ReadOnly);
				}
			}
		}

		public void TestLOAReceivedAuthorisingUPStoClearGoods()
		{
			Header.MiscServ.OM_CustomAttrib3 = "N";
			Assert(!((UPEOrgMiscServ)Header.MiscServ).LOAReceivedAuthorisingUPStoClearGoods);
			Header.MiscServ.OM_CustomAttrib3 = "Y";
			Assert(((UPEOrgMiscServ)Header.MiscServ).LOAReceivedAuthorisingUPStoClearGoods);
		}

		public void TestIsPreReleaseFeeApplicable()
		{
			AssertEquals(Header.IsPreReleaseContactFeeApplicable, ((UPEOrgMiscServ)Header.MiscServ).IsPreReleaseFeeApplicable);
			Header.IsPreReleaseContactFeeApplicable = true;
			AssertEquals(Header.IsPreReleaseContactFeeApplicable, ((UPEOrgMiscServ)Header.MiscServ).IsPreReleaseFeeApplicable);
			((UPEOrgMiscServ)Header.MiscServ).IsPreReleaseFeeApplicable = false;
			AssertEquals(Header.IsPreReleaseContactFeeApplicable, ((UPEOrgMiscServ)Header.MiscServ).IsPreReleaseFeeApplicable);
		}

		void CreateUsers()
		{
			nonOperationalUser = Factory.New<GlbStaff>();
			nonOperationalUser.GS_Code = "ADM";
			nonOperationalUser.GS_LoginName = "administrator";
			nonOperationalUser.GS_FullName = "Administrator";
			nonOperationalUser.GS_IsController = true;
			Factory.Save();
			operationalUser = Factory.New<GlbStaff>();
			operationalUser.GS_Code = "non";
			operationalUser.GS_LoginName = "NonSysAamin";
			operationalUser.GS_FullName = "Not Administrator";
			operationalUser.GS_IsController = false;
			Factory.Save();
		}

		GlbStaff nonOperationalUser;
		GlbStaff operationalUser;

		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			return Header.MiscServ;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return Header.MiscServ;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Header.MiscServ;
		}

		UPEOrgHeader Header
		{
			get
			{
				if (fHeader == null)
				{
					fHeader = Factory.NewWithValidTestData<UPEOrgHeader>();
				}

				return fHeader;
			}
		}

		UPEOrgHeader fHeader;
	}
}
