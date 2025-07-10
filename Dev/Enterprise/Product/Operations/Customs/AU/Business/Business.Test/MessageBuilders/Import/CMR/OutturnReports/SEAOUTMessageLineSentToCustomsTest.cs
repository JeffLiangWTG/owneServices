using CargoWise.EntityFramework.Testing;
using Enterprise.Edifact;
using Enterprise.Edifact.D99B.Messages.CUSCAR;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SEAOUTMessageLineSentToCustomsTest : TestCaseWithFactory
	{
		public void TestUniqueIdentifierMessageLineProperties()
		{
			var chSet = new UNOCCMRCharacterSet();
			var group7 = new SegmentGroup7();
			group7.Parse(chSet, @"CNI++:::I
RFF+AAQ:MSCU1234566
GID+1
RFF+ACU:NIL
GID+1
RFF+BH:321312
GID+1
RFF+MB:OBLESAU99999999
GIS+N:62:95
GIS+U:63:95
GIS+R:71:95
GIS+N:186:95
GIS+N:188:95
TDT+1
DTM+420:20090621:102
DTM+420:2329:401
GID+1
PAC+20
PAC+++PF:185:95
PAC+++LCL:67:95
".Replace("\r\n", "'"));

			var lineSentToCustoms = new SEAOUTMessageLineSentToCustoms(group7);
			AssertEquals(LineAction.Insert, lineSentToCustoms.LineAction);
			AssertEquals("CONTAINERNUMBER=MSCU1234566OCEANBILL=OBLESAU99999999HOUSEBILL=321312", lineSentToCustoms.UniqueIdentifier);
			AssertEquals(@"CNI
RFF+AAQ:MSCU1234566
GID+1
RFF+ACU:NIL
GID+1
RFF+BH:321312
GID+1
RFF+MB:OBLESAU99999999
GIS+N:62:95
GIS+U:63:95
GIS+R:71:95
GIS+N:186:95
GIS+N:188:95
TDT+1
DTM+420:20090621:102
DTM+420:2329:401
GID+1
PAC+20
PAC+++PF:185:95
PAC+++LCL:67:95
".Replace("\r\n", "'"), lineSentToCustoms.StringValue);
		}
	}
}
