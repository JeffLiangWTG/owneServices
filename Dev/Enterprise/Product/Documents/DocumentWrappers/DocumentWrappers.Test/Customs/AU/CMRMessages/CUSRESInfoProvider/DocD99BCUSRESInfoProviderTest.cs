using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.Customs.AU.Testing
{
	abstract class DocD99BCUSRESInfoProviderTest : DocumentWrapperTest
	{
		public void TestEntryNumber()
		{
			SegmentGroup3 group3 = CUSRES.Group3.InstantiateAChildAndAddItToChildrenCollection();
			RFFSegment rFF = group3.RFF.InstantiateAChildAndAddItToChildrenCollection();
			rFF.Reference.ReferenceFunctionCodeQualifier = ReferenceFunctionCodeQualifierList.CustomsDeclarationNumber;
			rFF.Reference.ReferenceIdentifier = "AAAANNPTY";
			AssertEquals("Entry Number", "AAAANNPTY", Wrapper.EntryNumber);
		}

		public void TestVersionNumber()
		{
			BGMSegment bGM = CUSRES.BGM.InstantiateAChildAndAddItToChildrenCollection();
			bGM.DocumentMessageIdentification.Version = "1";
			AssertEquals("VersionNumber", "1", Wrapper.VersionNumber);
		}

		public virtual void TestPaymentFinalisedDate()
		{
			DTMSegment dTM = CUSRES.DTM.InstantiateAChildAndAddItToChildrenCollection();
			dTM.DateTimePeriod.DateTimePeriodFunctionCodeQualifier = DateTimePeriodFunctionCodeQualifierList.PaymentDate;
			dTM.DateTimePeriod.DateTimePeriodValue = "20050925";
			AssertEquals("Payment Date", new ZDateTime(2005, 09, 25), Wrapper.PaymentFinalisedDate);
		}

		protected override void SetUp()
		{
			StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			CUSRES = new CUSRESMessage();
			D99BCUSRESInfoProvider infoProvider = GetInfoProvider(CUSRES);
			Wrapper = GetWrapper(infoProvider, Factory);
			base.SetUp();
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.SetCountry(StoredCountry);
			base.TearDown();
		}

		CUSRESMessage CUSRES;
		protected DocD99BCUSRESInfoProvider Wrapper;
		protected ZString StoredCountry;
		protected abstract D99BCUSRESInfoProvider GetInfoProvider(CUSRESMessage cUSRES);
		protected abstract DocD99BCUSRESInfoProvider GetWrapper(D99BCUSRESInfoProvider infoProvider, BusinessObjectFactory factory);
	}
}
