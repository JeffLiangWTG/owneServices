using NUnit.Framework;

namespace Enterprise.Client.TNT.Testing
{
	class Exit2QuantumRecordFactoryTest : TestCase
	{
		public void TestNewRecord()
		{
			Exit2QuantumRecordFactory recordFactory = new Exit2QuantumRecordFactory();
			AssertEquals("The record should be of type:", typeof(Exit2QuantumShipmentRecord), recordFactory.NewRecord("BNE", Line).GetType());
		}

		const string Line = "03922639355 BNEMIA21017331SURPLUS BEARINGS P/L           U 2 49 RANDALL ST              ADDR2                          SLACKS CREEK                   QUEENSLAND                     AU 4127     0738087494  0738087494  MOORE                 PICKUP NAME                    PICKUP ADDR1                   PICKUP ADDR2                   PICKUP CITY                    PICKUP STATE                   NZ PICKUP   1234567890  9999999999  P_Contact             IQ ENGINEERING INC             8208 NW 30TH TERRACE                                          MIAMI                          FL                             US 33122    5924404     5924404     GEITSY GONZALEZ       DELIVERY NAME                  DELIVERY ADDR1                 DELIVERY ADDR2                 DELIVERY CITY                  DELIVERY STATE                 AU DELPCODE 123456789               DEL CONTACT           NS       450.00USD     1    13.100EX2                                                                                    .";
	}
}
