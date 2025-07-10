using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(RTSDetails))]
	internal class RTSDetailsTest : RTSTranshipmentDetailsTestCase
	{
		public override void TestPopulatePropertiesInConstructor()
		{
			RTSTranshipmentDetails details = GetNewRTSTranshipmentDetails(UPECusHAWB);
			AssertEquals("AUSYD", details.OriginPort);
			AssertEquals("ITMIL", details.DestinationPort);
			AssertEquals("John Doe", details.Name);
			AssertEquals("Street1", details.Street);
			AssertEquals("Street2", details.Street2);
			AssertEquals("Botany", details.City);
			AssertEquals("QLD", details.State);
			AssertEquals("US", details.Country);
			AssertEquals("1111", details.PostCode);
		}

		protected override RTSTranshipmentDetails GetNewRTSTranshipmentDetails(UPECusHAWB uPECusHAWB)
		{
			return new RTSDetails(uPECusHAWB);
		}

		protected override string ExpectedNoteDescription
		{
			get
			{
				return "RTS Note";
			}
		}

		protected override string ExpectedNoteReference
		{
			get
			{
				return @"
Fields               Original                                             Changed To                                        
======               ==================================================   ==================================================
Origin UNLOCO:       AUSYD                                                USCOL                                             
Destination UNLOCO:  ITMIL                                                AUMEL                                             
Consignor Name:      John Doe                                             JOohn                                             
Consignor Address:   Street1                                              Botany Street                                     
                     Street2                                              Mural Lane                                        
Consignor City:      Botany                                               SYDNEY                                            
Consignor State:     QLD                                                  NSW                                               
Consignor Post Code: 1111                                                 2100                                              
Consignor Country:   US                                                   AU                                                ".TrimStart();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			UPECusHAWB.CS_ConsignorName = "John Doe";
			UPECusHAWB.CS_ConsignorStreet = "Street1";
			UPECusHAWB.CS_ConsignorStreet2 = "Street2";
			UPECusHAWB.CS_ConsignorCity = "Botany";
			UPECusHAWB.CS_ConsignorState = "QLD";
			UPECusHAWB.CS_RN_NKConsignorCountry = "US";
			UPECusHAWB.CS_ConsignorPostcode = "1111";
		}
	}
}
