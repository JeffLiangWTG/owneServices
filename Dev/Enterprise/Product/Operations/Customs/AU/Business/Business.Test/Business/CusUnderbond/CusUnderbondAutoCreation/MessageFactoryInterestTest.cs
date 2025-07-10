namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	abstract class MessageFactoryInterestTest : CMRSeaCargoDepotTestCase
	{
		public abstract void TestIsInterested();

		#region Implementation

		protected CMRUBMREQRMessage IFTU723492ExpectedCargoAdvice
		{
			get
			{
				if (fIFTU723492ExpectedCargoAdvice == null)
				{
					fIFTU723492ExpectedCargoAdvice = Factory.New<CMRUBMREQRMessage>();
					fIFTU723492ExpectedCargoAdvice.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+A477 FA1B 3G5:1+32'
DTM+9:20050916142251736787:ZZZ'
FTX+AAH+++AAA374MU00000221/SYD1'
TDT+20+932++11++++7104673::11'
TDT+1++RAI'
LOC+5+9913C::95'
LOC+4+9912J::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:U00000221/SYD1::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:MOV'
DOC+1'
PAC+++LCL:67:95'
PAC+0000030++PK:185:95'
RFF+AAQ:IFTU7823492'
RFF+MB:CSCD002'
RFF+BH:L20059039'
PCI+28+NILMARKS'
UNT+21+000001'".Replace("\r\n", "");
				}

				return fIFTU723492ExpectedCargoAdvice;
			}
		}
		CMRUBMREQRMessage fIFTU723492ExpectedCargoAdvice;

		protected CMRUBMREQRMessage HAWBUnderbondApproval
		{
			get
			{
				if (fHAWBUnderbondApproval == null)
				{
					fHAWBUnderbondApproval = Factory.New<CMRUBMREQRMessage>();
					fHAWBUnderbondApproval.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+6I3A E5EG 165:1+32'
DTM+9:20050915163415647465:ZZZ'
DTM+132:20050908:102'
TDT+20+9980++6+QF::3'
TDT+1++ROA'
LOC+5+9938N::95'
LOC+4+9932A::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ANX:UNDERBOND APPROVAL'
RFF+ACD:MOV'
DOC+1'
PAC+++AIR:67:95'
PAC+0000020'
RFF+MWB:08122220063'
UNT+17+000001'".Replace("\r\n", "");
				}
				return fHAWBUnderbondApproval;
			}
		}
		CMRUBMREQRMessage fHAWBUnderbondApproval;

		protected CMRUBMREQRMessage HAWBExpectedCargoAdvice
		{
			get
			{
				if (fHAWBExpectedCargoAdvice == null)
				{
					fHAWBExpectedCargoAdvice = Factory.New<CMRUBMREQRMessage>();
					fHAWBExpectedCargoAdvice.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::UBMREQR+AA05 4IC3 B65:1+32'
DTM+9:20050915145517626106:ZZZ'
DTM+132:20050908:102'
FTX+AAH+++AAA447YL3020039M000050'
TDT+20+9980++6+QF::3'
TDT+1++ROA'
LOC+5+9938N::95'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L3020039M000050::1'
RFF+ANX:EXPECTED CARGO ARRIVAL ADVICE'
RFF+ACD:MOV'
DOC+1'
PAC+++AIR:67:95'
PAC+0000020'
RFF+MWB:08122220074'
UNT+19+000001'".Replace("\r\n", "");
				}
				return fHAWBExpectedCargoAdvice;
			}
		}
		CMRUBMREQRMessage fHAWBExpectedCargoAdvice;

		protected CMRCARSTMessage HAWBCargoStatusMessage
		{
			get
			{
				if (fHAWBCargoStatusMessage == null)
				{
					fHAWBCargoStatusMessage = Factory.New<CMRCARSTMessage>();
					fHAWBCargoStatusMessage.EM_MessageText = @"
UNH+000001+CUSRES:D:99B:UN'
BGM+34:::CARST+21A6 FB43 0AB5:1+8'
DTM+9:20050812082715331950:ZZZ'
DTM+132:20050722:102'
FTX+AHN+++CONSOLIDATED STATUS:CLEAR'
FTX+AHN+++CARGO REPORT SAC:NO'
TDT+20+270++6+QF::3'
LOC+12+AUSYD::6'
LOC+4+9914N::95'
NAD+MR+AAA374M::95'
NAD+UD+41065894724::95'
RFF+ABO:L3020037H0004/3::1'
RFF+MWB:08112347775'
RFF+HWB:AH3'
DOC+1'
PAC+0000200'
UNT+17+000001'".Replace("\r\n", "");
				}
				return fHAWBCargoStatusMessage;
			}
		}
		CMRCARSTMessage fHAWBCargoStatusMessage;

		#endregion
	}
}
