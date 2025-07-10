using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(TranshipmentDetails))]
	internal class TranshipmentDetailsTest : RTSTranshipmentDetailsTestCase
	{
		public override void TestPopulatePropertiesInConstructor()
		{
			RTSTranshipmentDetails details = GetNewRTSTranshipmentDetails(UPECusHAWB);
			AssertEquals("AUSYD", details.OriginPort);
			AssertEquals("ITMIL", details.DestinationPort);
			AssertEquals("Joe Schmoe", details.Name);
			AssertEquals("Lane1", details.Street);
			AssertEquals("Lane2", details.Street2);
			AssertEquals("Chatswood", details.City);
			AssertEquals("NSW", details.State);
			AssertEquals("AU", details.Country);
			AssertEquals("2222", details.PostCode);
		}

		protected override RTSTranshipmentDetails GetNewRTSTranshipmentDetails(UPECusHAWB uPECusHAWB)
		{
			return new TranshipmentDetails(uPECusHAWB);
		}

		protected override string ExpectedNoteDescription
		{
			get
			{
				return "Transhipment Note";
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
Consignee Name:      Joe Schmoe                                           JOohn                                             
Consignee Address:   Lane1                                                Botany Street                                     
                     Lane2                                                Mural Lane                                        
Consignee City:      Chatswood                                            SYDNEY                                            
Consignee State:     NSW                                                  NSW                                               
Consignee Post Code: 2222                                                 2100                                              
Consignee Country:   AU                                                   AU                                                ".TrimStart();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			UPECusHAWB.CS_ConsigneeName = "Joe Schmoe";
			UPECusHAWB.CS_ConsigneeStreet = "Lane1";
			UPECusHAWB.CS_ConsigneeStreet2 = "Lane2";
			UPECusHAWB.CS_ConsigneeCity = "Chatswood";
			UPECusHAWB.CS_ConsigneeState = "NSW";
			UPECusHAWB.CS_RN_NKConsigneeCountry = "AU";
			UPECusHAWB.CS_ConsigneePostcode = "2222";
		}
	}
}
