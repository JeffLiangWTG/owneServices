using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CertificateRequestMessageBuilderTest : TestCaseWithFactory
	{
		public void TestMessageText()
		{
			var messageBuilder = new CertificateRequestMessageBuilder(new CertificateRequestDataForTesting(Factory));
			var certificateRequest = messageBuilder.GenerateMessage();
			AssertEquals("Certificate Request Message", ExpectedMessage, certificateRequest.EM_FormattedMessageText.TrimEnd());
		}

		string ExpectedMessage
		{
			get
			{
				ZStringBuilder builder = new ZStringBuilder();

				/*Message Header*/
				builder.Append("UNH+<<MSGNO PLACEHOLDER>>+SANCRT:D:97B:UN:CR0801");
				/*Message Identifier*/
				builder.Append("BGM+G::AQ++9");
				/*Discharge Port*/
				builder.Append("LOC+12+USIFR");
				/*Destination City*/
				builder.Append("LOC+8+RANIER");
				/*Cert Req Location*/
				builder.Append("LOC+91+CBR");
				/*Exporter Reference*/
				builder.Append("RFF+ABE:CERT REQ TEST 003");
				/*Notify Party Text*/
				builder.Append("FTX+AAG+++LINE 11111111111111111111111111111111111111111111111111:LINE 22222222222222222222222222222222222222222222222222:LINE 33333333333333333333333333333333333333333333333333:LINE 44444444444444444444444444444444444444444444444444:LINE 55555555555555555555555555555555555555555555555555");
				builder.Append("FTX+AAG+++LINE 6:LINE 77777777777777777777777777777777777777777777777777:LINE 88888888888888888888888888888888888888888888888888");
				/*Letter of Credit Text*/
				builder.Append("FTX+AAW+++LINE 11111111111111111111111111111111111111111111111111111111111111111:LINE 2:LINE 33333333333333333333333333333333333333333333333333333333333333333:LINE 44444444444444444444444444444444444444444444444444444444444444444");
				/*Separate Cert Container Ind*/
				builder.Append("GIS+Y::AQ:SC");
				/*Separate Cert Marks Ind*/
				builder.Append("GIS+Y::AQ:SM");
				/*Separate Cert Packer Ind*/
				builder.Append("GIS+Y::AQ:SP");
				/*Import Permit Nbr 1*/
				builder.Append("DOC+911+PERMIT1");
				/*Import Permit Date 1*/
				builder.Append("DTM+137:20091010:102");
				/*Import Permit Nbr 2*/
				builder.Append("DOC+911+PERMIT2");
				/*Import Permit Date 2*/
				builder.Append("DTM+137:20091010:102");
				/*Owner Exporter Nbr*/
				builder.Append("PNA+EX+EXPNUM");
				/*Consignee Name*/
				builder.Append("PNA+CN+123456++++10:CONSIGNEE NAME");
				/*Consignee Address*/
				builder.Append("ADR++5:ADDRESS LINE 1 ADDRESS LINE 2+LOS ANGELES+654321+US+:::CA");
				/*Consignee Contact Details*/
				builder.Append("CTA+CN");
				/*Consignee Phone*/
				builder.Append("COM+0212345678:TE");
				/*Consignee Represent Name*/
				builder.Append("CTA+AG+:FORWARDER NAME");
				/*Transport Details*/
				builder.Append("TDT+12+V1234+1++:::CARRIER NAME+++:::VESSEL NAME");
				/*Departure Date*/
				builder.Append("DTM+136:20091010:102");
				/*Certificate Line 1*/
				AppendCertificateLine(builder, 1);
				/*Certificate Line 2*/
				AppendCertificateLine(builder, 2);
				/*Message Trailer*/
				builder.Append("UNT+93+<<MSGNO PLACEHOLDER>>");

				return builder.ToStringWithNewLineBetweenAppends();
			}
		}

		void AppendCertificateLine(ZStringBuilder builder, int lineNum)
		{
			/*Line Identifier*/
			builder.Append("LIN+" + lineNum);
			/*Line Net Quantity*/
			builder.Append("MEA+AAA+SQ+TNE:2");
			/*Product Code*/
			builder.Append("PIA+5+XWHTVRGC:CC");
			/*Exp Def Product Desc*/
			builder.Append("IMD+++UHC:::DESCRIPTION LINE 1---------------->:DESCRIPTION LINE 2");
			/*Additional Product Desc*/
			builder.Append("IMD+++AD:::DESCRIPTION LINE 1---------------->:DESCRIPTION LINE 2");
			/*Extra Certificate 1*/
			builder.Append("DOC+852:::E1+:9");
			/*Extra Certificate 1*/
			builder.Append("DOC+852:::E2+:9");
			/*Packaging Details*/
			builder.Append("PAC+3+3+VR::AQ");
			//RFP Number 1
			/*RFP Number*/
			builder.Append("RFF+DM:RFPNUM1");
			/*RFP Line 1*/
			AppendRFPLine(builder, 1);
			/*RFP Line 2*/
			AppendRFPLine(builder, 2);
			//RFP Number 2
			/*RFP Number*/
			builder.Append("RFF+DM:RFPNUM2");
			/*RFP Line 1*/
			AppendRFPLine(builder, 1);
			/*RFP Line 2*/
			AppendRFPLine(builder, 2);
		}

		void AppendRFPLine(ZStringBuilder builder, int rfpLineNum)
		{
			/*RFP Line Number*/
			builder.Append("LIN+" + rfpLineNum + "+++1");
			/*RFP Line Net Quantity*/
			builder.Append("MEA+AAA+SQ+TNE:4");
			/*RFP Line Packaging Details*/
			builder.Append("PAC+2+3+VR::AQ");
			/*Container 1 - Num*/
			builder.Append("EQD+CN+CONT1");
			/*Container 1 - Seal*/
			builder.Append("SEL+SEAL1");
			/*Container 2 - Num*/
			builder.Append("EQD+CN+CONT2");
		}

		sealed class CertificateRequestDataForTesting : ICertificateRequestData
		{
			public CertificateRequestDataForTesting(BusinessObjectFactory factory)
			{
				this.factory = factory;
			}

			#region ICertificateRequestData Members

			public BusinessObject Object { get { return factory.New<QuarantineExDocHeader>(); } }
			public ZString CommodityType { get { return EXDOCCommodityCodesSingleChar.Codes.GrainsAndSeeds; } }
			public ZString DischargePort { get { return "USIFR"; } }
			public ZString DestinationCity { get { return "RANIER"; } }
			public ZString CertificateRequiredLocation { get { return "CBR"; } }
			public ZString ExporterCertificateReference { get { return "CERT REQ TEST 003"; } }

			public ZString NotifyPartyText
			{
				get
				{
					return "LINE 11111111111111111111111111111111111111111111111111"
						   + "LINE 22222222222222222222222222222222222222222222222222"
						   + "LINE 33333333333333333333333333333333333333333333333333"
						   + "LINE 44444444444444444444444444444444444444444444444444"
						   + "LINE 55555555555555555555555555555555555555555555555555"
						   + "LINE 6\r\n"
						   + "LINE 77777777777777777777777777777777777777777777777777"
						   + "LINE 88888888888888888888888888888888888888888888888888";
				}
			}

			public ZString LetterOfCreditText
			{
				get
				{
					return "LINE 11111111111111111111111111111111111111111111111111111111111111111"
						   + "LINE 2\r\n"
						   + "LINE 33333333333333333333333333333333333333333333333333333333333333333"
						   + "LINE 44444444444444444444444444444444444444444444444444444444444444444";
				}
			}

			public ZBool SeparateCertificateContainerInd { get { return true; } }
			public ZBool SeparateCertificateMarksInd { get { return true; } }
			public ZBool SeparateCertificatePackerInd { get { return true; } }

			public IEnumerable<IImportPermit> ImportPermits
			{
				get
				{
					for (int i = 1; i < 3; i++)
					{
						yield return new ImportPermitForTesting
						{
							PermitNumber = "PERMIT" + i,
							PermitDate = new ZDateTime(2009, 10, 10)
						};
					}
				}
			}

			public ZString OwnerExporterNumber { get { return "EXPNUM"; } }

			public OrgHeader Consignee
			{
				get
				{
					if (consignee == null)
					{
						consignee = factory.New<OrgHeader>();
						consignee.OH_FullName = "CONSIGNEE NAME";
						consignee.MainAddress.OA_Address1 = "ADDRESS LINE 1";
						consignee.MainAddress.OA_Address2 = "ADDRESS LINE 2";
						consignee.MainAddress.OA_City = "LOS ANGELES";
						consignee.MainAddress.OA_PostCode = "654321";
						consignee.OH_RL_NKClosestPort = "USLAX";
						consignee.MainAddress.OA_State = "CA";
						consignee.MainAddress.OA_Phone = "+02 (1234) 5678";
						var cusCode = consignee.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientID, "123456");
					}
					return consignee;
				}
			}

			OrgHeader consignee;

			public OrgHeader Forwarder
			{
				get
				{
					if (forwarder == null)
					{
						forwarder = factory.New<OrgHeader>();
						forwarder.OH_FullName = "FORWARDER NAME";
					}
					return forwarder;
				}
			}

			OrgHeader forwarder;

			public ZString TransportMode { get { return EXDOCTransportModeCodes.Codes.Sea; } }
			public ZString VoyageFlightNumber { get { return "V1234"; } }
			public ZString CarrierName { get { return "CARRIER NAME"; } }
			public ZString VesselName { get { return "VESSEL NAME"; } }
			public ZDateTime DepartureDate { get { return new ZDateTime(2009, 10, 10); } }

			public IEnumerable<ICertificateLine> CertificateLines
			{
				get
				{
					for (int i = 1; i <= 2; i++)
					{
						yield return new CertificateLineForTesting
						{
							LineNumber = i,
							NetQuantity = 2,
							NetQuantityUnit = "TNE",
							ProductCode = "XWHTVRGC",
							ProductDescription = "DESCRIPTION LINE 1---------------->DESCRIPTION LINE 2",
							AdditionalProductDescription = "DESCRIPTION LINE 1---------------->DESCRIPTION LINE 2",
							ExtraCertificates = new ZString[] { "E1", "E2" },
							PackQuantity = 3,
							PackType = "VR",
							RFPNumbers = new[] { new RFPNumberForTesting { RFPNumber = "RFPNUM1" }, new RFPNumberForTesting { RFPNumber = "RFPNUM2" } }
						};
					}
				}
			}

			#endregion

			readonly BusinessObjectFactory factory;
		}

		sealed class CertificateLineForTesting : ICertificateLine
		{
			public ZInt LineNumber { get; set; }
			public ZDecimal NetQuantity { get; set; }
			public ZString NetQuantityUnit { get; set; }
			public ZString ProductCode { get; set; }
			public ZString ProductDescription { get; set; }
			public ZString AdditionalProductDescription { get; set; }
			public ZString[] ExtraCertificates { get; set; }
			public ZDecimal PackQuantity { get; set; }
			public ZString PackType { get; set; }
			public IEnumerable<IRFPNumber> RFPNumbers { get; set; }
		}

		sealed class ImportPermitForTesting : IImportPermit
		{
			public ZString PermitNumber { get; set; }
			public ZDateTime PermitDate { get; set; }
		}

		sealed class RFPLineForTesting : IRFPLine
		{
			public ZInt RFPLineNumber { get; set; }
			public ZDecimal NetLineQuantity { get; set; }
			public ZString NetQuantityUnit { get; set; }
			public ZDecimal PackQuantity { get; set; }
			public ZString PackType { get; set; }
			public IEnumerable<IRFPContainer> Containers
			{
				get
				{
					for (int i = 1; i <= 2; i++)
					{
						yield return new RFPContainerForTesting
						{
							ContainerNumber = "CONT" + i,
							Seal = i == 1 ? "SEAL1" : string.Empty
						};
					}
				}
			}
		}

		sealed class RFPNumberForTesting : IRFPNumber
		{
			public ZString RFPNumber { get; set; }
			public IEnumerable<IRFPLine> RFPLines
			{
				get
				{
					for (int i = 1; i <= 2; i++)
					{
						yield return new RFPLineForTesting
						{
							RFPLineNumber = i,
							NetLineQuantity = 4,
							NetQuantityUnit = "TNE",
							PackQuantity = 2,
							PackType = "VR",
						};
					}
				}
			}
		}
	}
}
