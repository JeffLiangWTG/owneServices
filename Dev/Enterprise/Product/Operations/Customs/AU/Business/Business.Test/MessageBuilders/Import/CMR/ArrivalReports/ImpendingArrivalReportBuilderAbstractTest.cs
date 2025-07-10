namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class ImpendingArrivalReportBuilderAbstractTest : ArrivalReportBuilderAbstractTest
	{
		public void TestLineActionCode()
		{
			Assert("LineActionCode", GeneratedMessage.Contains("FTX+LIN+I'"));
		}

		public void TestCargoDischargeIndicator()
		{
			Assert("CargoDischargeIndicator", GeneratedMessage.Contains("STS++Y:63:95'"));
		}

		public void TestLastOverseasPortOfDeparture()
		{
			Assert("LastOverseasPortOfDeparture", GeneratedMessage.Contains("LOC+125+NZAKL::6'"));
		}

		public virtual void TestEstimatedDateOfArrival()
		{
			Assert("EstimatedDateOfArrival", GeneratedMessage.Contains("DTM+132:20050124:102'"));
		}

		public virtual void TestLastDateOfDeparture()
		{
			Assert("LastDateOfDeparture", GeneratedMessage.Contains("DTM+" + DateTimeCodeQualifier + ":20050124:102'"));
		}

		public abstract void TestDischargeCTOID();
	}
}
