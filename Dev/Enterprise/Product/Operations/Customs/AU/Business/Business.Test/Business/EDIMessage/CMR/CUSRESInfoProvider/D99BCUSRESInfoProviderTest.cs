using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class D99BCUSRESInfoProviderTest : TestCaseWithFactory
	{
		public abstract void TestDocumentName();

		public void TestEntryNumber()
		{
			CUSRESMessage cUSRES = GetCUSRESMessage();
			D99BCUSRESInfoProvider infoProvider = GetInfoProvider(cUSRES);

			SegmentGroup3 group3 = cUSRES.Group3.InstantiateAChildAndAddItToChildrenCollection();
			RFFSegment rFF = group3.RFF.InstantiateAChildAndAddItToChildrenCollection();
			rFF.Reference.ReferenceFunctionCodeQualifier = ReferenceFunctionCodeQualifierList.CustomsDeclarationNumber;
			rFF.Reference.ReferenceIdentifier = "AAAANNPTY";
			AssertEquals("Entry Number", "AAAANNPTY", infoProvider.EntryNumber);
		}

		public void TestPaymentFinalisedDate()
		{
			CUSRESMessage cUSRES = GetCUSRESMessage();
			D99BCUSRESInfoProvider infoProvider = GetInfoProvider(cUSRES);

			DTMSegment dTM = cUSRES.DTM.InstantiateAChildAndAddItToChildrenCollection();
			dTM.DateTimePeriod.DateTimePeriodFunctionCodeQualifier = DateTimePeriodFunctionCodeQualifierList.PaymentDate;
			dTM.DateTimePeriod.DateTimePeriodValue = "20050204";
			AssertEquals("Payment Date", ExpectedPaymentFinalisedDate, infoProvider.PaymentFinalisedDate);
		}

		protected virtual ZDateTime ExpectedPaymentFinalisedDate
		{
			get
			{
				return new ZDateTime(2005, 2, 4);
			}
		}

		public void TestVersionNumber()
		{
			CUSRESMessage cUSRES = GetCUSRESMessage();
			D99BCUSRESInfoProvider infoProvider = GetInfoProvider(cUSRES);

			BGMSegment bGM = cUSRES.BGM.InstantiateAChildAndAddItToChildrenCollection();
			bGM.DocumentMessageIdentification.Version = "1";
			AssertEquals("VersionNumber", "1", infoProvider.VersionNumber);
		}

		protected abstract CUSRESMessage GetCUSRESMessage();
		protected abstract D99BCUSRESInfoProvider GetInfoProvider(CUSRESMessage cUSRES);
	}
}
