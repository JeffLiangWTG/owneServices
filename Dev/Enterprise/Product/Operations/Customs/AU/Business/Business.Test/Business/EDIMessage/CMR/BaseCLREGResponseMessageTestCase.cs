using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class BaseCLREGResponseMessageTestCase : TestCaseWithFactory
	{
		public void TestGetWrappedObjectByRefNumber()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TESTCDE";

			var docMessageNumber = Guid.NewGuid().ToString("N");

			var addOn = Factory.New<GenAddOnColumn>();
			addOn.XA_ParentID = orgHeader.PK;
			addOn.XA_ParentTableCode = OrgHeaderSchema.Constants.Prefix;
			addOn.XA_Name = docMessageNumber;
			addOn.XA_Type = AddOnColumnDataType.Codes.String;
			addOn.XA_Data = docMessageNumber;

			var message = Factory.New<CMRCLREGRMessage>();
			message.EM_MessageText = @"UNH+000001+CUSRES:D:99B:UN'
BGM+961:::CLREGR+2F8D 6599 866A:001+11'
FTX+CCI++AAA3366766M'
NAD+MR+AAA374M::95'
RFF+ACW:CLREG'
RFF+AFM:9'
RFF+ABO:DOCMSGNUM::001'
DTM+310:20101221232955:204'
ERP+1'
ERC+ADVICE:80:95'
ERC+MS5202:6:95'
FTX+AAO+++THIS TRANSACTION WAS ACCEPTED WITH ERRORS AND/OR WARNINGS'
ERP+1'
ERC+ADVICE:80:95'
ERC+CL0378:6:95'
FTX+AAO+++CCID =AAA3366766M CREATED SUCCESSFULLY'
CNT+55:000'
UNT+18+000001'".Replace("\r\n", "").Replace("DOCMSGNUM", docMessageNumber);

			AssertEquals("Object should be found", orgHeader, message.GetWrappedObject());
		}
	}
}
