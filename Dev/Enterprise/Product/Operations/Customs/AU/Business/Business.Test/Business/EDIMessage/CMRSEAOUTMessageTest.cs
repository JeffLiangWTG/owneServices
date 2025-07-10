using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRSEAOUTMessage))]
	public class CMRSEAOUTMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestLineCollection()
		{
			var message = Factory.New<CMRSEAOUTMessage>();
			message.EM_MessageText = @"UNH+1+CUSCAR:D:99B:UN
BGM+263:::SEAOUT+O00000418/CMT1:3+4
NAD+VW+41065894724::95
TDT+20+3336++11++++7619410::11
LOC+4+9912J::95
CNI++:::D
RFF+AAQ:OCLU1233382
GID+1
RFF+ACU:SH
GIS+N:62:95
GIS+U:63:95
GIS+R:71:95
GIS+N:186:95
GIS+N:188:95
TDT+1
DTM+420:20131111:102
DTM+420:0031:401
GID+1
PAC+0
PAC+++FCL:67:95
FTX+AAA+++CONSOLIDATED CARGO
CNI++:::D
RFF+AAQ:OCLU1233398
GID+1
RFF+ACU:SH
GIS+N:62:95
GIS+U:63:95
GIS+R:71:95
GIS+N:186:95
GIS+N:188:95
TDT+1
DTM+420:20131111:102
DTM+420:0100:401
GID+1
PAC+0
PAC+++FCL:67:95
FTX+AAA+++CONSOLIDATED CARGO
CNI++:::I
RFF+AAQ:OCLU1233382
GID+1
RFF+ACU:NIL
GIS+Y:62:95
GIS+U:63:95
GIS+R:71:95
GIS+N:186:95
GIS+N:188:95
TDT+1
DTM+420:20131111:102
DTM+420:0031:401
GID+1
PAC+1
PAC+++YC:185:95
PAC+++FCL:67:95
FTX+AAA+++CONSOLIDATED CARGO
CNI++:::I
RFF+AAQ:OCLU1233398
GID+1
RFF+ACU:NIL
GIS+Y:62:95
GIS+U:63:95
GIS+R:71:95
GIS+N:186:95
GIS+N:188:95
TDT+1
DTM+420:20131111:102
DTM+420:0100:401
GID+1
PAC+1
PAC+++YC:185:95
PAC+++FCL:67:95
FTX+AAA+++CONSOLIDATED CARGO
UNT+72+1
".Replace("\r\n", "'");
			AssertEquals(4, message.LineCollection.Count());
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			EDIMessage result = (EDIMessage)GetNewBusinessObject();
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			return result;
		}

		protected override bool CanPersistedObjectBeDeleted => false;
	}
}
