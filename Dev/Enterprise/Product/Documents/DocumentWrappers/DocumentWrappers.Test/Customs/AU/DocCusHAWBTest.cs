using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	[TestedType(typeof(DocCusHAWB))]
	sealed class DocCusHAWBTest : DocumentWrapperTestCase
	{
		public void TestDeclaration()
		{
			CusHAWB.CS_JE_CustomsFormalEntry = Factory.NewWithValidTestData(typeof(JobDeclaration)).PK;
			CusHAWB.Declaration.JE_GoodsDescription = "GoodsDescription";
			AssertEquals("GoodsDescription", DocCusHAWB.Declaration.GoodsDescription);
		}

		#region Implementation

		ZString OldCountry;

		DocCusHAWB DocCusHAWB
		{
			get
			{
				if (fDocCusHAWB == null)
				{
					fDocCusHAWB = DocCusHAWB.New(CusHAWB, Factory);
				}
				return fDocCusHAWB;
			}
		}
		DocCusHAWB fDocCusHAWB;

		CusHAWB CusHAWB
		{
			get
			{
				if (fCusHAWB == null)
				{
					fCusHAWB = Factory.NewWithValidTestData<CusHAWB>();
				}
				return fCusHAWB;
			}
		}
		CusHAWB fCusHAWB;

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var result = DocCusHAWB.New(CusHAWB, Factory);
			return new DocumentWrapper[] { result };
		}

		protected override void SetUp()
		{
			base.SetUp();
			OldCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.SetCountry(OldCountry);
		}

		#endregion
	}
}
