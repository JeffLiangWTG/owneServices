using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class EXDMessageBuilderTest : TestCaseWithFactory
	{
		public void TestConfirmingExportType()
		{
			var dec = JobDeclaration.New(Factory);
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec.JE_MessageSubType = JobDeclaration.MessageSubType.Confirming;
			var builder = new EXDMessageBuilder(dec, ExportDeclarationType.Confirming);
			AssertEquals("GIS field", true, builder.MessageText.Contains("GIS+Y:79:95'"));
		}

		public void TestConfirmedExportType()
		{
			var dec = JobDeclaration.New(Factory);
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec.JE_MessageSubType = JobDeclaration.MessageSubType.Confirming;
			var builder = new EXDMessageBuilder(dec, ExportDeclarationType.Confirmed);
			AssertEquals("GIS field", true, builder.MessageText.Contains("GIS+C:79:95'"));
		}

		public void TestNonConfirmingExportType()
		{
			var dec = JobDeclaration.New(Factory);
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec.JE_MessageSubType = JobDeclaration.MessageSubType.NonConfirming;
			var builder = new EXDMessageBuilder(dec, ExportDeclarationType.Original);
			AssertEquals("GIS field", true, builder.MessageText.Contains("GIS+N:79:95'"));
		}

		public void TestConstructor()
		{
			AssertNotNull("EXDMessageBuilder", new EXDMessageBuilder(Factory.New<JobDeclaration>(), ExportDeclarationType.Original));
		}

		public void TestAirLineCodeAndResponsibleAgencyCodeForSP_ST_AB()
		{
			transportMode = Core.Constants.TransportModes.Air;
			var testExportGoodsType = new string[] { JobDeclaration.ExportGoodsType.Stores, JobDeclaration.ExportGoodsType.SpareParts, JobDeclaration.ExportGoodsType.AccompaniedBaggage };

			foreach (var goodsType in testExportGoodsType)
			{
				var jobDec = GetTestJobDec(
					exportCargoTypeCode,
					goodsType,
					portOfLoadingCode,
					portOfLoadingCode,
					destinationCode,
					exportationDate,
					"Y",
					"Y",
					GlbCompany.CurrentCompany.GC_OH_OrgProxy,
					ZString.Empty,
					"Q F123",
					transportMode,
					Factory.LoadFromNaturalKey(typeof(OrgHeader), OrgHeaderSchema.OH_Code, "AUSEXP").PK,
					containerCount,
					packageCount,
					Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, invoiceCurrencyCode).PK,
					Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, fOBCurrencyCode).PK,
					fOBValue,
					unitOfQuantityCode,
					GoodsOwnerID
					);

				jobDec.SendDeclarationOriginal(ExportDeclarationType.Original);
				var generatedMessage = jobDec.Messages[0].EM_MessageText;
				AssertEquals("Message", true, generatedMessage.Contains("TDT+20+Q F123++6+QF::3"));
			}
		}

		public void TestAirLineCodeForGeneralTypes()
		{
			var jobDec = GetTestJobDec(
				exportCargoTypeCode,
				JobDeclaration.ExportGoodsType.GeneralConsignedCargo,
				portOfLoadingCode,
				portOfLoadingCode,
				destinationCode,
				exportationDate,
				"Y",
				"Y",
				GlbCompany.CurrentCompany.GC_OH_OrgProxy,
				ZString.Empty,
				"QF123",
				transportMode,
				Factory.LoadFromNaturalKey(typeof(OrgHeader), OrgHeaderSchema.OH_Code, "AUSEXP").PK,
				containerCount,
				packageCount,
				Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, invoiceCurrencyCode).PK,
				Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, fOBCurrencyCode).PK,
				fOBValue,
				unitOfQuantityCode,
				GoodsOwnerID
				);

			jobDec.SendDeclarationOriginal(ExportDeclarationType.Original);
			var generatedMessage = jobDec.Messages[0].EM_MessageText;
			AssertEquals("Message", false, generatedMessage.Contains("TDT+20+QF123++6+QF::3"));
		}

		public void TestVesselAndVoyageNumber()
		{
			AssetTDTisCorrect(JobDeclaration.ExportGoodsType.GeneralConsignedCargo, "Vessel/Voyage not present for OT", "TDT+20+++11'");
			AssetTDTisCorrect(JobDeclaration.ExportGoodsType.OwnPower, "Vessel/Voyage not present for OP", "TDT+20+++11'");
			AssetTDTisCorrect(JobDeclaration.ExportGoodsType.AccompaniedBaggage, "Vessel/Voyage is present for AB", "TDT+20+123S++11++++8811924::11");
			AssetTDTisCorrect(JobDeclaration.ExportGoodsType.SpareParts, "Vessel/Voyage is present for SP", "TDT+20+123S++11++++8811924::11");
			AssetTDTisCorrect(JobDeclaration.ExportGoodsType.Stores, "Vessel/Voyage is present for ST", "TDT+20+123S++11++++8811924::11");
		}

		public void TestUsingEntryLineToBuildMessage()
		{
			var jobDec = GetTestJobDec(
				exportCargoTypeCode,
				JobDeclaration.ExportGoodsType.GeneralConsignedCargo,
				portOfLoadingCode,
				portOfLoadingCode,
				destinationCode,
				exportationDate,
				"Y",
				"Y",
				GlbCompany.CurrentCompany.GC_OH_OrgProxy,
				ZString.Empty,
				"QF123",
				transportMode,
				Factory.LoadFromNaturalKey(typeof(OrgHeader), OrgHeaderSchema.OH_Code, "AUSEXP").PK,
				containerCount,
				packageCount,
				Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, invoiceCurrencyCode).PK,
				Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, fOBCurrencyCode).PK,
				fOBValue,
				unitOfQuantityCode,
				GoodsOwnerID
				);
			jobDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_LinePrice = 123456789012m;

			var merger = new LineMerger(jobDec);
			merger.DoMerge();
			AssertEquals("Precondition: One Customs Entry Header created", 1, jobDec.ActiveEntryHeaders.Count);
			AssertEquals("Precondition: EntryLine number", 1, jobDec.ActiveEntryHeaders[0].MergedLines.Count);
			AssertEquals("Precondition: EntryHeader invoiceCurrency", "AUD", jobDec.ActiveEntryHeaders[0].FOB.Currency.Code);
			AssertEquals("Precondition: EntryLine RandomLine", 123456789012m, jobDec.ActiveEntryHeaders[0].MergedLines[0].RandomLine.JI_LinePrice);

			jobDec.SendDeclarationOriginal(ExportDeclarationType.Original);
			var expectedResult = "'MOA+39::AUD'" + "'MOA+63:123456789012:AUD";
			var generatedMessage = jobDec.ActiveEntryHeaders[0].Messages[0].EM_MessageText;
			AssertEquals("Message", false, generatedMessage.Contains(expectedResult));
		}

		public void TestBuildMessage()
		{
			CreateTariff34060000();
			transportMode = Core.Constants.TransportModes.Air;

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var jobDec = GetTestJobDec(
				exportCargoTypeCode,
				exportGoodsTypeCode,
				portOfLoadingCode,
				dischargeCode,
				destinationCode,
				exportationDate,
				"Y",
				"Y",
				GlbCompany.CurrentCompany.GC_OH_OrgProxy,
				ZString.Empty,
				"QF123",
				transportMode,
				Factory.LoadFromNaturalKey(typeof(OrgHeader), OrgHeaderSchema.OH_Code, "AUSEXP").PK,
				containerCount,
				packageCount,
				Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, invoiceCurrencyCode).PK,
				Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, fOBCurrencyCode).PK,
				fOBValue,
				unitOfQuantityCode,
				GoodsOwnerID
				);
				jobDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_LinePrice = 123456789012m;
				jobDec.SendDeclarationOriginal(ExportDeclarationType.Original);
				var expectedResult = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:99B:UN'BGM+830:::EXD+" + jobDec.JE_DeclarationReference + "/DAT1:1+9'LOC+9+" + portOfLoadingCode + "::6'LOC+12+" + dischargeCode + "::6'LOC+28+" + destinationCode.Substring(0, 2) + "::6'DTM+129:" + exportationDate.ToString("yyyyMMdd") + ":102'GIS+N:79:95'GIS+N:107:95'GIS+N:141:95'RFF+AWH:O'PAC+++" + exportCargoTypeCode + ":67:95'PAC+++" + exportGoodsTypeCode + ":146:95'TDT+20+++6'NAD+CN+++AUSTRALIAN AIR EXPRESS++TULLAMARINE AIRPORT,'MOA+39::" + invoiceCurrencyCode + "'MOA+63:123456789012:" + fOBCurrencyCode + "'UNS+D'CST+1+I::95'FTX+AAA+++DESCRIPTION WITH A ?' CHARACTER'LOC+27++AU-NS::6'MEA+WT++KG:10'MEA+ABW++" + unitOfQuantityCode + ":10'MOA+63:123456789012'RFF+HS:34060000'RFF+EP:12345'RFF+AGM:1'UNS+S'CNT+11:" + packageCount + "'CNT+36:" + containerCount + "'UNT+{0}+<<MSGNO PLACEHOLDER>>'";
				expectedResult = string.Format(expectedResult, EXDMessageBuilder.CountSegments(expectedResult).ToString());
				AssertMultilineEquals("Message", expectedResult, jobDec.Messages[0].EM_MessageText, '\'');
			}
		}

		public void TestPortOfLoadingCodes()
		{
			CreateTariff34060000();
			transportMode = Core.Constants.TransportModes.Air;

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				foreach (var portOfLoadingCode in testLoadingPortCodes)
				{
					var jobDec = GetTestJobDec(
						exportCargoTypeCode,
						exportGoodsTypeCode,
						portOfLoadingCode,
						dischargeCode,
						destinationCode,
						exportationDate,
						"Y",
						"Y",
						GlbCompany.CurrentCompany.GC_OH_OrgProxy,
						ZString.Empty,
						"QF123",
						transportMode,
						Factory.LoadFromNaturalKey(typeof(OrgHeader), OrgHeaderSchema.OH_Code, "AUSEXP").PK,
						containerCount,
						packageCount,
						Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, invoiceCurrencyCode).PK,
						Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, fOBCurrencyCode).PK,
						fOBValue,
						unitOfQuantityCode,
						GoodsOwnerID
						);

					jobDec.SendDeclarationOriginal(ExportDeclarationType.Original);
					var expectedResult = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:99B:UN'BGM+830:::EXD+" + jobDec.JE_DeclarationReference + "/DAT1:1+9'LOC+9+" + portOfLoadingCode + "::6'LOC+12+" + dischargeCode + "::6'LOC+28+" + destinationCode.Substring(0, 2) + "::6'DTM+129:" + exportationDate.ToString("yyyyMMdd") + ":102'GIS+N:79:95'GIS+N:107:95'GIS+N:141:95'RFF+AWH:O'PAC+++" + exportCargoTypeCode + ":67:95'PAC+++" + exportGoodsTypeCode + ":146:95'TDT+20+++6'NAD+CN+++AUSTRALIAN AIR EXPRESS++TULLAMARINE AIRPORT,'MOA+39::" + invoiceCurrencyCode + "'MOA+63:0:" + fOBCurrencyCode + "'UNS+D'CST+1+I::95'FTX+AAA+++DESCRIPTION WITH A ?' CHARACTER'LOC+27++AU-NS::6'MEA+WT++KG:10'MEA+ABW++" + unitOfQuantityCode + ":10'MOA+63:0'RFF+HS:34060000'RFF+EP:12345'RFF+AGM:1'UNS+S'CNT+11:" + packageCount + "'CNT+36:" + containerCount + "'UNT+{0}+<<MSGNO PLACEHOLDER>>'";
					expectedResult = string.Format(expectedResult, EXDMessageBuilder.CountSegments(expectedResult).ToString());
					AssertMultilineEquals("Message", expectedResult, jobDec.Messages[0].EM_MessageText, '\'');
				}
			}
		}

		public void TestPortOfDestinationCodes()
		{
			CreateTariff34060000();
			transportMode = Core.Constants.TransportModes.Air;

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				foreach (var destinationCode in testDestinationPortCodes)
				{
					var jobDec = GetTestJobDec(
						exportCargoTypeCode,
						exportGoodsTypeCode,
						portOfLoadingCode,
						dischargeCode,
						destinationCode,
						exportationDate,
						"Y",
						"Y",
						GlbCompany.CurrentCompany.GC_OH_OrgProxy,
						ZString.Empty,
						"QF123",
						transportMode,
						Factory.LoadFromNaturalKey(typeof(OrgHeader), OrgHeaderSchema.OH_Code, "AUSEXP").PK,
						containerCount,
						packageCount,
						Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, invoiceCurrencyCode).PK,
						Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, fOBCurrencyCode).PK,
						fOBValue,
						unitOfQuantityCode,
						GoodsOwnerID
						);

					jobDec.SendDeclarationOriginal(ExportDeclarationType.Original);
					var expectedResult = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:99B:UN'BGM+830:::EXD+" + jobDec.JE_DeclarationReference + "/DAT1:1+9'LOC+9+" + portOfLoadingCode + "::6'LOC+12+" + dischargeCode + "::6'LOC+28+" + destinationCode.Substring(0, 2) + "::6'DTM+129:" + exportationDate.ToString("yyyyMMdd") + ":102'GIS+N:79:95'GIS+N:107:95'GIS+N:141:95'RFF+AWH:O'PAC+++" + exportCargoTypeCode + ":67:95'PAC+++" + exportGoodsTypeCode + ":146:95'TDT+20+++6'NAD+CN+++AUSTRALIAN AIR EXPRESS++TULLAMARINE AIRPORT,'MOA+39::" + invoiceCurrencyCode + "'MOA+63:0:" + fOBCurrencyCode + "'UNS+D'CST+1+I::95'FTX+AAA+++DESCRIPTION WITH A ?' CHARACTER'LOC+27++AU-NS::6'MEA+WT++KG:10'MEA+ABW++" + unitOfQuantityCode + ":10'MOA+63:0'RFF+HS:34060000'RFF+EP:12345'RFF+AGM:1'UNS+S'CNT+11:" + packageCount + "'CNT+36:" + containerCount + "'UNT+{0}+<<MSGNO PLACEHOLDER>>'";
					expectedResult = string.Format(expectedResult, EXDMessageBuilder.CountSegments(expectedResult).ToString());
					AssertMultilineEquals("Message", expectedResult, jobDec.Messages[0].EM_MessageText, '\'');
				}
			}
		}

		public void TestPackageCounts()
		{
			CreateTariff34060000();
			transportMode = Core.Constants.TransportModes.Air;

			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				foreach (short packageCount in packageCounts)
				{
					var jobDec = GetTestJobDec(
						exportCargoTypeCode,
						exportGoodsTypeCode,
						portOfLoadingCode,
						dischargeCode,
						destinationCode,
						exportationDate,
						"Y",
						"Y",
						GlbCompany.CurrentCompany.GC_OH_OrgProxy,
						ZString.Empty,
						"QF123",
						transportMode,
						Factory.LoadFromNaturalKey(typeof(OrgHeader), OrgHeaderSchema.OH_Code, "AUSEXP").PK,
						containerCount,
						packageCount,
						Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, invoiceCurrencyCode).PK,
						Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, fOBCurrencyCode).PK,
						fOBValue,
						unitOfQuantityCode,
						GoodsOwnerID
						);

					jobDec.SendDeclarationOriginal(ExportDeclarationType.Original);
					var expectedResult = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:99B:UN'BGM+830:::EXD+" + jobDec.JE_DeclarationReference + "/DAT1:1+9'LOC+9+" + portOfLoadingCode + "::6'LOC+12+" + dischargeCode + "::6'LOC+28+" + destinationCode.Substring(0, 2) + "::6'DTM+129:" + exportationDate.ToString("yyyyMMdd") + ":102'GIS+N:79:95'GIS+N:107:95'GIS+N:141:95'RFF+AWH:O'PAC+++" + exportCargoTypeCode + ":67:95'PAC+++" + exportGoodsTypeCode + ":146:95'TDT+20+++6'NAD+CN+++AUSTRALIAN AIR EXPRESS++TULLAMARINE AIRPORT,'MOA+39::" + invoiceCurrencyCode + "'MOA+63:0:" + fOBCurrencyCode + "'UNS+D'CST+1+I::95'FTX+AAA+++DESCRIPTION WITH A ?' CHARACTER'LOC+27++AU-NS::6'MEA+WT++KG:10'MEA+ABW++" + unitOfQuantityCode + ":10'MOA+63:0'RFF+HS:34060000'RFF+EP:12345'RFF+AGM:1'UNS+S'CNT+11:" + packageCount + "'CNT+36:" + containerCount + "'UNT+{0}+<<MSGNO PLACEHOLDER>>'";
					expectedResult = string.Format(expectedResult, EXDMessageBuilder.CountSegments(expectedResult).ToString());
					AssertMultilineEquals("MessagePackageCode=" + unitOfQuantityCode, expectedResult, jobDec.Messages[0].EM_MessageText, '\'');
				}
			}
		}

		public void TestBuildMessage_AHECC()
		{
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				transportMode = Core.Constants.TransportModes.Air;

				var jobDec = GetTestJobDec(
					exportCargoTypeCode,
					exportGoodsTypeCode,
					portOfLoadingCode,
					dischargeCode,
					destinationCode,
					exportationDate,
					"Y",
					"Y",
					GlbCompany.CurrentCompany.GC_OH_OrgProxy,
					ZString.Empty,
					"QF123",
					transportMode,
					Factory.LoadFromNaturalKey(typeof(OrgHeader), OrgHeaderSchema.OH_Code, "AUSEXP").PK,
					containerCount,
					packageCount,
					Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, invoiceCurrencyCode).PK,
					Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, fOBCurrencyCode).PK,
					fOBValue,
					unitOfQuantityCode,
					GoodsOwnerID
					);
				jobDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_LinePrice = 123456789012m;
				jobDec.SendDeclarationOriginal(ExportDeclarationType.Original);
				var expectedResult = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:99B:UN'BGM+830:::EXD+" + jobDec.JE_DeclarationReference + "/DAT1:1+9'LOC+9+" + portOfLoadingCode + "::6'LOC+12+" + dischargeCode + "::6'LOC+28+" + destinationCode.Substring(0, 2) + "::6'DTM+129:" + exportationDate.ToString("yyyyMMdd") + ":102'GIS+N:79:95'GIS+N:107:95'GIS+N:141:95'RFF+AWH:O'PAC+++" + exportCargoTypeCode + ":67:95'PAC+++" + exportGoodsTypeCode + ":146:95'TDT+20+++6'NAD+CN+++AUSTRALIAN AIR EXPRESS++TULLAMARINE AIRPORT,'MOA+39::" + invoiceCurrencyCode + "'MOA+63:123456789012:" + fOBCurrencyCode + "'UNS+D'CST+1+I::95'FTX+AAA+++DESCRIPTION WITH A ?' CHARACTER'LOC+27++AU-NS::6'MEA+WT++KG:10'MEA+ABW++" + unitOfQuantityCode + ":10'MOA+63:123456789012'RFF+HS:34060000'RFF+EP:12345'RFF+AGM:1'UNS+S'CNT+11:" + packageCount + "'CNT+36:" + containerCount + "'UNT+{0}+<<MSGNO PLACEHOLDER>>'";
				expectedResult = string.Format(expectedResult, EXDMessageBuilder.CountSegments(expectedResult).ToString());
				AssertMultilineEquals("Message", expectedResult, jobDec.Messages[0].EM_MessageText, '\'');
			}
		}

		public void TestPortOfLoadingCodes_AHECC()
		{
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				transportMode = Core.Constants.TransportModes.Air;

				foreach (var portOfLoadingCode in testLoadingPortCodes)
				{
					var jobDec = GetTestJobDec(
						exportCargoTypeCode,
						exportGoodsTypeCode,
						portOfLoadingCode,
						dischargeCode,
						destinationCode,
						exportationDate,
						"Y",
						"Y",
						GlbCompany.CurrentCompany.GC_OH_OrgProxy,
						ZString.Empty,
						"QF123",
						transportMode,
						Factory.LoadFromNaturalKey(typeof(OrgHeader), OrgHeaderSchema.OH_Code, "AUSEXP").PK,
						containerCount,
						packageCount,
						Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, invoiceCurrencyCode).PK,
						Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, fOBCurrencyCode).PK,
						fOBValue,
						unitOfQuantityCode,
						GoodsOwnerID
						);

					jobDec.SendDeclarationOriginal(ExportDeclarationType.Original);
					var expectedResult = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:99B:UN'BGM+830:::EXD+" + jobDec.JE_DeclarationReference + "/DAT1:1+9'LOC+9+" + portOfLoadingCode + "::6'LOC+12+" + dischargeCode + "::6'LOC+28+" + destinationCode.Substring(0, 2) + "::6'DTM+129:" + exportationDate.ToString("yyyyMMdd") + ":102'GIS+N:79:95'GIS+N:107:95'GIS+N:141:95'RFF+AWH:O'PAC+++" + exportCargoTypeCode + ":67:95'PAC+++" + exportGoodsTypeCode + ":146:95'TDT+20+++6'NAD+CN+++AUSTRALIAN AIR EXPRESS++TULLAMARINE AIRPORT,'MOA+39::" + invoiceCurrencyCode + "'MOA+63:0:" + fOBCurrencyCode + "'UNS+D'CST+1+I::95'FTX+AAA+++DESCRIPTION WITH A ?' CHARACTER'LOC+27++AU-NS::6'MEA+WT++KG:10'MEA+ABW++" + unitOfQuantityCode + ":10'MOA+63:0'RFF+HS:34060000'RFF+EP:12345'RFF+AGM:1'UNS+S'CNT+11:" + packageCount + "'CNT+36:" + containerCount + "'UNT+{0}+<<MSGNO PLACEHOLDER>>'";
					expectedResult = string.Format(expectedResult, EXDMessageBuilder.CountSegments(expectedResult).ToString());
					AssertMultilineEquals("Message", expectedResult, jobDec.Messages[0].EM_MessageText, '\'');
				}
			}
		}

		public void TestPortOfDestinationCodes_AHECC()
		{
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				transportMode = Core.Constants.TransportModes.Air;

				foreach (var destinationCode in testDestinationPortCodes)
				{
					var jobDec = GetTestJobDec(
						exportCargoTypeCode,
						exportGoodsTypeCode,
						portOfLoadingCode,
						dischargeCode,
						destinationCode,
						exportationDate,
						"Y",
						"Y",
						GlbCompany.CurrentCompany.GC_OH_OrgProxy,
						ZString.Empty,
						"QF123",
						transportMode,
						Factory.LoadFromNaturalKey(typeof(OrgHeader), OrgHeaderSchema.OH_Code, "AUSEXP").PK,
						containerCount,
						packageCount,
						Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, invoiceCurrencyCode).PK,
						Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, fOBCurrencyCode).PK,
						fOBValue,
						unitOfQuantityCode,
						GoodsOwnerID
						);

					jobDec.SendDeclarationOriginal(ExportDeclarationType.Original);
					var expectedResult = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:99B:UN'BGM+830:::EXD+" + jobDec.JE_DeclarationReference + "/DAT1:1+9'LOC+9+" + portOfLoadingCode + "::6'LOC+12+" + dischargeCode + "::6'LOC+28+" + destinationCode.Substring(0, 2) + "::6'DTM+129:" + exportationDate.ToString("yyyyMMdd") + ":102'GIS+N:79:95'GIS+N:107:95'GIS+N:141:95'RFF+AWH:O'PAC+++" + exportCargoTypeCode + ":67:95'PAC+++" + exportGoodsTypeCode + ":146:95'TDT+20+++6'NAD+CN+++AUSTRALIAN AIR EXPRESS++TULLAMARINE AIRPORT,'MOA+39::" + invoiceCurrencyCode + "'MOA+63:0:" + fOBCurrencyCode + "'UNS+D'CST+1+I::95'FTX+AAA+++DESCRIPTION WITH A ?' CHARACTER'LOC+27++AU-NS::6'MEA+WT++KG:10'MEA+ABW++" + unitOfQuantityCode + ":10'MOA+63:0'RFF+HS:34060000'RFF+EP:12345'RFF+AGM:1'UNS+S'CNT+11:" + packageCount + "'CNT+36:" + containerCount + "'UNT+{0}+<<MSGNO PLACEHOLDER>>'";
					expectedResult = string.Format(expectedResult, EXDMessageBuilder.CountSegments(expectedResult).ToString());
					AssertMultilineEquals("Message", expectedResult, jobDec.Messages[0].EM_MessageText, '\'');
				}
			}
		}

		public void TestPackageCounts_AHECC()
		{
			using (AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				transportMode = Core.Constants.TransportModes.Air;

				foreach (short packageCount in packageCounts)
				{
					var jobDec = GetTestJobDec(
						exportCargoTypeCode,
						exportGoodsTypeCode,
						portOfLoadingCode,
						dischargeCode,
						destinationCode,
						exportationDate,
						"Y",
						"Y",
						GlbCompany.CurrentCompany.GC_OH_OrgProxy,
						ZString.Empty,
						"QF123",
						transportMode,
						Factory.LoadFromNaturalKey(typeof(OrgHeader), OrgHeaderSchema.OH_Code, "AUSEXP").PK,
						containerCount,
						packageCount,
						Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, invoiceCurrencyCode).PK,
						Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, fOBCurrencyCode).PK,
						fOBValue,
						unitOfQuantityCode,
						GoodsOwnerID
						);

					jobDec.SendDeclarationOriginal(ExportDeclarationType.Original);
					var expectedResult = "UNH+<<MSGNO PLACEHOLDER>>+CUSDEC:D:99B:UN'BGM+830:::EXD+" + jobDec.JE_DeclarationReference + "/DAT1:1+9'LOC+9+" + portOfLoadingCode + "::6'LOC+12+" + dischargeCode + "::6'LOC+28+" + destinationCode.Substring(0, 2) + "::6'DTM+129:" + exportationDate.ToString("yyyyMMdd") + ":102'GIS+N:79:95'GIS+N:107:95'GIS+N:141:95'RFF+AWH:O'PAC+++" + exportCargoTypeCode + ":67:95'PAC+++" + exportGoodsTypeCode + ":146:95'TDT+20+++6'NAD+CN+++AUSTRALIAN AIR EXPRESS++TULLAMARINE AIRPORT,'MOA+39::" + invoiceCurrencyCode + "'MOA+63:0:" + fOBCurrencyCode + "'UNS+D'CST+1+I::95'FTX+AAA+++DESCRIPTION WITH A ?' CHARACTER'LOC+27++AU-NS::6'MEA+WT++KG:10'MEA+ABW++" + unitOfQuantityCode + ":10'MOA+63:0'RFF+HS:34060000'RFF+EP:12345'RFF+AGM:1'UNS+S'CNT+11:" + packageCount + "'CNT+36:" + containerCount + "'UNT+{0}+<<MSGNO PLACEHOLDER>>'";
					expectedResult = string.Format(expectedResult, EXDMessageBuilder.CountSegments(expectedResult).ToString());
					AssertMultilineEquals("MessagePackageCode=" + unitOfQuantityCode, expectedResult, jobDec.Messages[0].EM_MessageText, '\'');
				}
			}
		}

		public void TestDontSendLOCSegmentIfNoCountryOfOriginEntered()
		{
			var dec = GetTestJobDec();
			dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_CountryOfOrigin = ZString.Empty;
			dec.SendDeclarationOriginal(ExportDeclarationType.Original);
			Assert("CountryOfOriginNotIncluded", dec.Messages[0].EM_MessageText.IndexOf("LOC+27") == -1);
		}

		public void TestDontSendLOCSegmentIfNoStateEntered()
		{
			var dec = GetTestJobDec();
			dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_CountryOfOrigin = "AU";
			dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].JI_AUState = ZString.Empty;
			dec.SendDeclarationOriginal(ExportDeclarationType.Original);
			Assert("CountryOfOriginNotIncluded", dec.Messages[0].EM_MessageText.IndexOf("LOC+27") == -1);
		}

		public void TestLongImporterName()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = new string('A', 40);
			importer.OH_RL_NKClosestPort = "NZAKL";
			dec.JE_OH_Importer = importer.PK;
			var builder = new EXDMessageBuilder(dec, ExportDeclarationType.Original);

			Assert("Importer Name cut down to 35 characters", builder.MessageText.Contains("NAD+CN+++" + new string('A', 35) + "++AUCKLAND'"));
		}

		public void TestStripSpacesFromABN()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			var exporter = Factory.New<OrgHeader>();
			exporter.OH_FullName = "BOB";
			exporter.PrimaryRegistrationNumber.Number = "99 999 999 999";
			dec.JE_OH_Supplier = exporter.PK;
			var builder = new EXDMessageBuilder(dec, ExportDeclarationType.Original);

			Assert("ABN Spaces Stripped", builder.MessageText.Contains("NAD+GO+99999999999::95'"));
		}

		public void TestSendTotalWeightInTons()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			var line = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			line.JI_Weight = 100m;
			line.JI_WeightUQ = "T";
			var builder = new EXDMessageBuilder(dec, ExportDeclarationType.Original);

			Assert("Weight in Tons Included", builder.MessageText.Contains("MEA+WT++T:100'"));
		}

		public void TestDescriptionEndingInAColonIsEscaped()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			var line = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			line.JI_Description = "GOODS:";
			var builder = new EXDMessageBuilder(dec, ExportDeclarationType.Original);

			Assert("Escaped Correctly", builder.MessageText.Contains("FTX+AAA+++GOODS?:'"));
		}

		public void TestGetImporterCityFromCityFieldThenLOCO()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "Name";
			importer.MainAddress.OA_City = "New York";
			importer.OH_RL_NKClosestPort = "NZAKL";
			dec.JE_OH_Importer = importer.PK;

			var builder = new EXDMessageBuilder(dec, ExportDeclarationType.Original);
			Assert("City=New York", builder.MessageText.Contains("NAD+CN+++NAME++NEW YORK'"));

			importer.MainAddress.OA_City = ZString.Empty;

			builder = new EXDMessageBuilder(dec, ExportDeclarationType.Original);
			Assert("City=AUCKLAND", builder.MessageText.Contains("NAD+CN+++NAME++AUCKLAND'"));

			importer.OH_RL_NKClosestPort = ZString.Empty;

			builder = new EXDMessageBuilder(dec, ExportDeclarationType.Original);
			Assert("Importer Segment Not Entered", !builder.MessageText.Contains("NAD+CN"));
		}

		public void TestTrimDescriptionTo128Characters()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			var line = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			line.JI_Description = new ZString('A', 300);
			var builder = new EXDMessageBuilder(dec, ExportDeclarationType.Original);

			Assert("Only 128 Characters of Description Used", builder.MessageText.Contains("FTX+AAA+++" + new ZString('A', 128) + "'"));
		}

		public void TestFunnyCharactersInUNOCSegment()
		{
			var wantedDescription = "€™¢£¥©®[]LALA";
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			var line = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			line.JI_Description = wantedDescription;
			var builder = new EXDMessageBuilder(dec, ExportDeclarationType.Original);

			Assert("WantedDescriptionInMessage", builder.MessageText.Contains("FTX+AAA+++         LALA"));
		}

		public void TestDontSendCustomsQtyIfEmpty()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			var line = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			line.JI_CustomsQuantity = 100m;
			line.JI_CustomsUnitQty = ZString.Empty;
			var builder = new EXDMessageBuilder(dec, ExportDeclarationType.Original);

			Assert("NoCustomsQtySegmentIncluded", !builder.MessageText.Contains("MEA+ABW"));
		}

		public void TestDescriptionEndingInSingleQuote()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			var line = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			line.JI_Description = "HELLO'";
			var builder = new EXDMessageBuilder(dec, ExportDeclarationType.Original);

			Assert("Single Quote Correctly Escaped", builder.MessageText.Contains("FTX+AAA+++HELLO?''"));
		}

		public void TestPopulateWithDodgyGoodsType()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			dec.JE_ExportGoodsType = ZString.Empty;
			var builder = new EXDMessageBuilder(dec, ExportDeclarationType.Original);

			Assert("PAC Segment Not Included", !builder.MessageText.Contains("PAC+++:146:95'"));
		}

		public void TestPopulateWithDodgyCargoType()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			dec.JE_ContainerMode = "XXX";
			var builder = new EXDMessageBuilder(dec, ExportDeclarationType.Original);

			Assert("PAC Segment Not Included", !builder.MessageText.Contains("PAC+++:67:95'"));
		}

		public void TestDontPopulatePackagesForContainerisedOrBulk()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			dec.JE_TotalNoOfPacks = 10;
			dec.JE_ContainerCount = 10;

			dec.JE_ContainerMode = Core.Constants.ContainerModes.Containerised;
			var builder = new EXDMessageBuilder(dec, ExportDeclarationType.Original);
			Assert("Package Count Not Included", !builder.MessageText.Contains("CNT+11:10'"));

			dec.JE_ContainerMode = Core.Constants.ContainerModes.Bulk;
			builder = new EXDMessageBuilder(dec, ExportDeclarationType.Original);
			Assert("Package Count Not Included", !builder.MessageText.Contains("CNT+11:10'"));

			dec.JE_ContainerMode = Core.Constants.ContainerModes.Liquid;
			builder = new EXDMessageBuilder(dec, ExportDeclarationType.Original);
			Assert("Package Count Not Included", !builder.MessageText.Contains("CNT+11:10'"));
		}

		public void TestDontSendMOASegmentsIfNoCurrencySpecified()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			var header = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header.JZ_InvoiceAmount = 100m;
			header.JZ_RX_NKInvoice_Currency = ZString.Empty;

			header.JZ_IncoTerm = Core.Constants.IncoTerms.FreeOnBoard;
			var charge = header.Charges.AddNew();
			charge.J7_ChargeType = AUChargeCodeList.Codes.OtherCharges;
			charge.J7_Amount = 100m;

			var builder = new EXDMessageBuilder(dec, ExportDeclarationType.Original);

			Assert("No MOA Segments Included", !builder.MessageText.Contains("MOA+"));
		}

		public void TestIncludePrescribedGoodsIndicator()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			Assert("No To Prescribed Goods", new EXDMessageBuilder(dec, ExportDeclarationType.Original).MessageText.Contains("GIS+N:141:95'"));

			dec.AddInfo.ZA_PrescribedGoods_Hidden = true;
			Assert("Yes To Prescribed Goods", new EXDMessageBuilder(dec, ExportDeclarationType.Original).MessageText.Contains("GIS+Y:141:95'"));
		}

		public void TestPermitNumbers()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			var jobComInvHeader = dec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var jobComInvLine = jobComInvHeader.JobComInvoiceLines.AddNew();
			jobComInvHeader.AddInfo.ZA_PermitNumbers_Hidden = "ABC1,XYZ1";
			jobComInvLine.AddInfo.ZA_PermitNumbers_Hidden = "ABC2,XYZ2";
			Assert("Header and Line Level Permit Numbers", new EXDMessageBuilder(dec, ExportDeclarationType.Original).MessageText.Contains("RFF+EP:ABC2'RFF+EP:XYZ2'RFF+EP:ABC1'RFF+EP:XYZ1"));
		}

		void CreateTariff34060000()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Australia, Enterprise.Customs.Universal.Constants.TariffTypes.Export);
			var tariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Australia, expTariffType.PK, "34060000", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, taxOrFeeCode: "GST"
				, description: "CANDLES, TAPERS AND THE LIKE, WHETHER OR NOT COLOURED, PERFUMED OR DECORATED");
			helper.CreateTariffUOM(tariff1, Enterprise.Customs.Universal.Constants.UnitOfMeasureTypes.StatisticalUOMType, "KG");
		}

		protected override void SetUp()
		{
			base.SetUp();

			exportCargoTypeCode = "N";
			exportGoodsTypeCode = "OT";
			portOfLoadingCode = "AUSYD";
			dischargeCode = "NZAKL";
			destinationCode = "NZAKL";
			exportationDate = new DateTime(2003, 8, 20);
			invoiceCurrencyCode = "AUD";
			fOBCurrencyCode = "AUD";
			containerCount = 0;
			packageCount = 10;
			fOBValue = 1000;
			transportMode = Enterprise.Core.Constants.TransportModes.Sea;
			unitOfQuantityCode = "KG";
		}

		JobDeclaration GetTestJobDec()
		{
			return GetTestJobDec(
				exportCargoTypeCode,
				exportGoodsTypeCode,
				portOfLoadingCode,
				dischargeCode,
				destinationCode,
				exportationDate,
				"Y",
				"Y",
				GlbCompany.CurrentCompany.GC_OH_OrgProxy,
				ZString.Empty,
				"QF123",
				transportMode,
				Factory.LoadFromNaturalKey(typeof(OrgHeader), OrgHeaderSchema.OH_Code, "AUSEXP").PK,
				containerCount,
				packageCount,
				Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, invoiceCurrencyCode).PK,
				Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, fOBCurrencyCode).PK,
				fOBValue,
				unitOfQuantityCode,
				GoodsOwnerID
				);
		}

		JobDeclaration GetTestJobDec(string exportCargoTypeCode, string exportGoodsTypeCode, ZString portOfLoading, ZString discharge, ZString destination, DateTime exportDate, string exportGoodsType, string messageSubType, ZGuid supplier, string entryNumber, string voyageFlightNumber, string transportType, ZGuid importer, short containerCount, short packageCount, ZGuid invoiceCurrency, ZGuid fOBCurrency, decimal fOBAmount, string unitOfQuantity, string goodsOwnerID)
		{
			var jobDec = Factory.New<JobDeclaration>();
			var jobComInvHeader = jobDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var jobComInvLine = jobComInvHeader.JobComInvoiceLines.AddNew();

			jobDec.JE_OH_Supplier = supplier;
			jobDec.JE_OH_Importer = importer;
			//Set all the relavent values
			jobDec.JE_ExportGoodsType = exportGoodsTypeCode;
			jobDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			jobDec.JE_MessageSubType = messageSubType;
			jobDec.JE_TransportMode = transportType;
			jobDec.JE_ContainerMode = GetCargoIDType3Char(exportCargoTypeCode);
			//JobDec.JE_DeclarationReference = Env.NumberFountains.NZMessageReferenceNumber.GetNext().ToUpper();
			jobDec.JE_RL_NKPortOfLoading = portOfLoading;
			jobDec.JE_RL_NKPortOfArrival = discharge;
			jobDec.JE_RL_NKFinalDestination = destination;

			jobDec.JE_ExportDate = new ZDateTime(exportDate);
			jobDec.DeclarationNumber = entryNumber;
			jobDec.JE_VoyageFlightNo = voyageFlightNumber;

			jobDec.JE_ContainerCount = containerCount;
			jobDec.JE_TotalNoOfPacks = packageCount;

			jobComInvHeader.JZ_RX_NKInvoice_Currency = Factory.Load<RefCurrency>(invoiceCurrency).RX_Code;
			//TODO: What to do with this?
			//JobComInvHeader.JZ_Calc_FOBCurrency = FOBCurrency;
			//JobComInvHeader.JZ_Calc_FOBAmount = FOBAmount;

			jobComInvLine.JI_LineNo = 1;
			jobComInvLine.JI_Tariff = "3406.00.00";
			jobComInvLine.JI_TempImportNum = "1";
			jobComInvLine.JI_Description = "DESCRIPTION WITH A ' CHARACTER";
			jobComInvLine.JI_Weight = 10;
			jobComInvLine.JI_WeightUQ = unitOfQuantity;

			jobComInvLine.JI_CustomsQuantity = 10;

			jobComInvLine.AddInfo.ZA_PermitNumbers_Hidden = "12345";
			jobComInvLine.JI_CountryOfOrigin = "AU";
			jobComInvLine.JI_AUState = "NSW";

			jobDec.Supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.GSTCode, goodsOwnerID);
			return jobDec;
		}

		string exportCargoTypeCode;
		string exportGoodsTypeCode;
		string portOfLoadingCode;
		string dischargeCode;
		string destinationCode;
		DateTime exportationDate;
		string invoiceCurrencyCode;
		string fOBCurrencyCode;
		short containerCount;
		short packageCount;
		decimal fOBValue;
		string transportMode;
		string unitOfQuantityCode;
		const string GoodsOwnerID = "1234567890";

		string GetCargoIDType3Char(string code)
		{
			switch (code)
			{
				case "A":
					return "AIR";
				case "C":
					return "CNT";
				case "N":
					return "NCT";
				case "CO":
					return "COM";
				case "B":
					return "BLK";
			}
			return code;
		}

		readonly short[] packageCounts = { 1, 0, 10, 32767 };
		readonly string[] testDestinationPortCodes = { "AUSYD", "AUNTL", "NZAKL", "HKHKG" };
		readonly string[] testLoadingPortCodes = { "AUSYD", "AUNTL" };

		void AssetTDTisCorrect(string exportGoodsType, string message, string expectedTDT)
		{
			var testDec = GetTestJobDec(
					exportCargoTypeCode,
					exportGoodsType,
					portOfLoadingCode,
					portOfLoadingCode,
					destinationCode,
					exportationDate,
					"Y",
					"Y",
					GlbCompany.CurrentCompany.GC_OH_OrgProxy,
					ZString.Empty,
					"123S",
					Core.Constants.TransportModes.Sea,
					Factory.LoadFromNaturalKey(typeof(OrgHeader), OrgHeaderSchema.OH_Code, "AUSEXP").PK,
					containerCount,
					packageCount,
					Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, invoiceCurrencyCode).PK,
					Factory.LoadFromNaturalKey(typeof(RefCurrency), RefCurrencySchema.RX_Code, fOBCurrencyCode).PK,
					fOBValue,
					unitOfQuantityCode,
					GoodsOwnerID
					);
			testDec.JE_VesselName = RefVessel.LookupVesselByLloyds("8811924", Factory).RV_Code;
			testDec.SendDeclarationOriginal(ExportDeclarationType.Original);
			Assert(message, testDec.Messages[0].EM_MessageText.Contains(expectedTDT));
		}
	}
}
