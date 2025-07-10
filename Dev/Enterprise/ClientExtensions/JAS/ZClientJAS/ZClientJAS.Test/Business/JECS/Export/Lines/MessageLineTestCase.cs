using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal abstract class MessageLineTestCase : TestCaseWithFactory
	{
		public void TestLineType()
		{
			ZString lineType = Line.LineIdentifier.Left(Line.LineIdentifier.Length - JXCConstants.Version.Length);
			AssertEquals("Line type is not as expected", ExpectedLineType, lineType);
		}

		public void TestFieldCount()
		{
			ZString[] totalFields = Line.LineAsString.Split(JXCConstants.Delimiter);
			int fieldCountWithoutLineIdentifier = totalFields.Length - 1;
			AssertEquals(ExpectedFieldCount, fieldCountWithoutLineIdentifier);
		}

		protected MessageLine Line
		{
			get
			{
				if (fLine == null)
				{
					fLine = GetMessageLine();
				}

				return fLine;
			}

			set
			{
				fLine = value;
			}
		}

		MessageLine fLine;
		ZGuid initialProxyOrgPK;
		protected override void SetUp()
		{
			initialProxyOrgPK = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			base.SetUp();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "1234567890123456789012345678901234567890";
			org.MainAddress.OA_Address1 = "2345678901234567890123456789012345678901";
			org.MainAddress.OA_Address2 = "3456789012345678901234567890123456789012";
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = org.PK;
			Factory.Save();
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = initialProxyOrgPK;
		}

		#region Abstract
		protected abstract MessageLine GetMessageLine();
		protected abstract ZString ExpectedLineType { get; }

		protected abstract int ExpectedFieldCount { get; }
		#endregion
	}
}
