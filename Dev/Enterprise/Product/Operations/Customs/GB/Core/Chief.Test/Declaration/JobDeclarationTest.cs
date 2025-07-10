using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Declaration.Testing;
using Enterprise.Customs.GB.Chief.CusDec.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Chief.Declaration.Testing
{
	[TestedType(typeof(JobDeclaration))]
	public class JobDeclarationTest : JobDeclarationTests
	{
		public void TestAddInfoValidation()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			AssertType<GB.Business.Declaration.JobDeclarationValidation>(dec.AddInfoValidation);
		}

		public new void TestAllEntriesCleared()
		{
			BaseJobDeclaration declaration = (BaseJobDeclaration)GetNewBusinessObject();
			declaration.CustomsEntryHeaders.RemoveAndDeleteAll();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();

			AssertEquals("Empty collection", 0, declaration.CustomsEntryHeaders.Count);
			AssertEquals(false, declaration.HaveAllEntriesCleared);

			var mock = Factory.NewMoq<Business.Declaration.CusEntryHeader>();
			mock.Setup(m => m.ClearanceDate).Returns(new ZDateTime(2005, 2, 1));
			declaration.CustomsEntryHeaders.Add(mock.Object);
			AssertEquals(true, declaration.HaveAllEntriesCleared);

			mock = Factory.NewMoq<Business.Declaration.CusEntryHeader>();
			mock.Setup(m => m.ClearanceDate).Returns(ZDateTime.Empty);
			declaration.CustomsEntryHeaders.Add(mock.Object);
			AssertEquals(false, declaration.HaveAllEntriesCleared);
		}

		public void TestDefaultTerritoryIncludingSpecialCases()
		{
			const string serbria = "XS";
			const string xc = "XC";
			const string ic = "IC";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.EUCTY, MapDirectionList.Codes.OUT, "DES", true);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "RS", serbria, ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddYears(1), Core.Constants.CountryCodes.UnitedKingdom);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "ESCUE", xc, ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddYears(1), Core.Constants.CountryCodes.UnitedKingdom);
			helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "ESBBJ", ic, ZDateTime.MinSmallDateTimeValue, ZDateTime.Today.AddYears(1), Core.Constants.CountryCodes.UnitedKingdom);
			Factory.Save();

			JobDeclaration dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			dec.JE_RL_NKOrigin = "RSBEG";
			AssertEquals(serbria, dec.JE_GoodsOrigin);

			dec.JE_RL_NKOrigin = "ESCUE";
			AssertEquals(xc, dec.JE_GoodsOrigin);

			dec.JE_RL_NKFinalDestination = "ESBBJ";
			AssertEquals(ic, dec.JE_GoodsDestination);

			dec.JE_RL_NKOrigin = "ESMAD";
			AssertEquals(Core.Constants.CountryCodes.Spain, dec.JE_GoodsOrigin);
		}

		public void TestGoodsLocationIsPortAttributeGvmsArrived()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "Port Code");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.UnitedKingdom);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Port, "LON", "Test Port", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "GvmsArrived", "Gvms Arrived");
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			declaration.JE_TransportMode = "SEA";
			declaration.JE_EntrySubStyle = "D";
			declaration.JE_CHIEF_GoodsLocation = "";
			AssertEquals("D", declaration.JE_EntrySubStyle);

			declaration.JE_CHIEF_GoodsLocation = "LON";
			AssertEquals("A", declaration.JE_EntrySubStyle);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			declaration.CustomsEntryHeaders.AddNew();
			return declaration;
		}

		#region TestMergeOperation_GB_CHIEF

		protected override List<MergeScenario> PrepareMergeScenarios_ToTestMergeOperation(string countryCode)
		{
			const string CHF = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			const string EXP = MessageTypeList.Codes.Export;
			const string IMP = MessageTypeList.Codes.Import;

			var list = new List<MergeScenario>();

			#region Scenario_01_CHIEF

			var dec = CreateJobDeclaration_ToTestMergeOperation(IMP, CHF, 2);

			var invoice = dec.Invoices[0];
			invoice.AdditionalInfos.Add(CreateAdditionalInfo("IMH01"));

			var invLine = invoice.InvoiceLines[0];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("IMI01"));

			var scenario = new MergeScenario(dec);
			scenario.AssertMessage = "GB CHIEF test scenario 01";

			var entry = scenario.AddEntry("-11-12");
			entry.AddAdditionalInfo("IMH01");

			var entryLine = entry.AddEntryLine("11");
			entryLine.AddAdditionalInfo("IMI01");

			entryLine = entry.AddEntryLine("12");

			list.Add(scenario);

			#endregion  //End: "Scenario_01_CHIEF"

			#region Scenario_02_CHIEF

			dec = CreateJobDeclaration_ToTestMergeOperation(IMP, CHF, 2, 3);

			dec.AdditionalInfos.Add(CreateAdditionalInfo("IMH01"));
			dec.AdditionalInfos.Add(CreateAdditionalInfo("IMI01"));

			invoice = dec.Invoices[0];
			invoice.AdditionalInfos.Add(CreateAdditionalInfo("IMH02"));

			invLine = invoice.InvoiceLines[0];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("IMI02"));

			invLine = invoice.InvoiceLines[1];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("IMI03"));

			invoice = dec.Invoices[1];
			invoice.AdditionalInfos.Add(CreateAdditionalInfo("IMH03"));

			invLine = invoice.InvoiceLines[2];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("IMI01"));
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("IMI02"));

			scenario = new MergeScenario(dec);
			scenario.AssertMessage = "GB CHIEF test scenario 02";

			entry = scenario.AddEntry("-11-12");
			entry.AddAdditionalInfo("IMH01");
			entry.AddAdditionalInfo("IMI01");
			entry.AddAdditionalInfo("IMH02");

			entryLine = entry.AddEntryLine("11");
			entryLine.AddAdditionalInfo("IMI02");

			entryLine = entry.AddEntryLine("12");
			entryLine.AddAdditionalInfo("IMI03");

			entry = scenario.AddEntry("-24-25-26");
			entry.AddAdditionalInfo("IMH01");
			entry.AddAdditionalInfo("IMI01");
			entry.AddAdditionalInfo("IMH03");

			entryLine = entry.AddEntryLine("24");
			entryLine = entry.AddEntryLine("25");

			entryLine = entry.AddEntryLine("26");
			entryLine.AddAdditionalInfo("IMI01");
			entryLine.AddAdditionalInfo("IMI02");

			list.Add(scenario);

			#endregion  //End: "Scenario_02_CHIEF"

			#region Scenario_03_CHIEF

			dec = CreateJobDeclaration_ToTestMergeOperation(EXP, CHF, 1, 2, 3);
			dec.AdditionalInfos.Add(CreateAdditionalInfo("EXH03"));

			invoice = dec.Invoices[0];
			invoice.AdditionalInfos.Add(CreateAdditionalInfo("EXH01"));

			invLine = invoice.InvoiceLines[0];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI02"));

			invoice = dec.Invoices[1];
			invoice.AdditionalInfos.Add(CreateAdditionalInfo("EXH02"));

			invLine = invoice.InvoiceLines[0];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI01"));
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI03"));

			invLine = invoice.InvoiceLines[1];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI02"));

			invoice = dec.Invoices[2];
			invoice.AdditionalInfos.Add(CreateAdditionalInfo("EXH03"));

			invLine = invoice.InvoiceLines[0];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI02"));

			invLine = invoice.InvoiceLines[1];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI03"));

			invLine = invoice.InvoiceLines[2];
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI02"));
			invLine.AdditionalInfos.Add(CreateAdditionalInfo("EXI01"));

			scenario = new MergeScenario(dec);
			scenario.AssertMessage = "GB CHIEF test scenario 03";

			entry = scenario.AddEntry("-11");
			entry.AddAdditionalInfo("EXH03");
			entry.AddAdditionalInfo("EXH01");

			entryLine = entry.AddEntryLine("11");
			entryLine.AddAdditionalInfo("EXI02");

			entry = scenario.AddEntry("-24-25");
			entry.AddAdditionalInfo("EXH03");
			entry.AddAdditionalInfo("EXH02");

			entryLine = entry.AddEntryLine("24");
			entryLine.AddAdditionalInfo("EXI01");
			entryLine.AddAdditionalInfo("EXI03");

			entryLine = entry.AddEntryLine("25");
			entryLine.AddAdditionalInfo("EXI02");

			entry = scenario.AddEntry("-37-38-39");
			entry.AddAdditionalInfo("EXH03");

			entryLine = entry.AddEntryLine("37");
			entryLine.AddAdditionalInfo("EXI02");

			entryLine = entry.AddEntryLine("38");
			entryLine.AddAdditionalInfo("EXI03");

			entryLine = entry.AddEntryLine("39");
			entryLine.AddAdditionalInfo("EXI02");
			entryLine.AddAdditionalInfo("EXI01");

			list.Add(scenario);

			#endregion  //End: "Scenario_03_CHIEF"

			return list;
		}

		protected override EU.Business.Declaration.JobDeclaration CreateJobDeclaration_ToTestMergeOperation(string messageType, string applicationCode, int invLinesOnInvoice1, int invLinesOnInvoice2 = 0, int invLinesOnInvoice3 = 0)
		{
			var declaration = base.CreateJobDeclaration_ToTestMergeOperation(messageType, applicationCode, invLinesOnInvoice1, invLinesOnInvoice2, invLinesOnInvoice3);
			var dec = declaration as JobDeclaration;

			dec.JE_CustomsProfile = "BC1";
			dec.JE_ApplicationCode = applicationCode;
			dec.JE_DeclarationType = "IFD";

			dec.JE_CHIEF_GoodsLocation = "ZZZ";
			dec.SubLocation = "XXX";

			dec.JE_GBRouteOfEntry = "H";
			dec.JE_EntrySubStyle = "C";

			return dec;
		}

		protected override void Produce_EDI_Message_ToTestMergeOperation(MergeScenario scenario)
		{
			var msg = scenario.AssertMessage;
			var dec = scenario.JobDeclaration;

			DeclarationChosererTester.AddEoriIfNotExits(dec.Declarant.Header, "ABC");

			Factory.Save();

			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);

			var sender = (IDeclarationMessageSender)Activator.CreateInstance(
							ObjectFactory.GetType<Integration.Customs.GB.IDeclarationMessageSenderChooser>());

			sender.Send(dec, shutUp, new CusdecMessageFunction.New());

			Assert(msg, shutUp.SuccessfulSendOccured);
		}

		protected override void Check_EDI_Message_ToTestMergeOperation(MergeScenario scenario, Entry_ToTestMergeOperation expectedEntry, EU.Business.Declaration.CusEntryHeader entryHeader)
		{
			var msg = scenario.AssertMessage;
			var entryMessages = entryHeader.Messages;
			var msgText = entryMessages[0].EM_FormattedMessageText;

			Assert(!msgText.IsEmpty);

			var addInfoLookup = CreateAddInfoLookup_Header_ToTestMergeOperation(msgText);
			CompareAdditionalInfos_ToTestMergeOperation(msg, expectedEntry.AdditionalInfoList, addInfoLookup);

			var entryLineLookup = CreateEntryLineLookup(msgText);
			AssertEquals(msg + ": No of entry lines", expectedEntry.EntryLines.Count, entryLineLookup.Count);

			foreach (var expectedLine in expectedEntry.EntryLines)
			{
				var entryLine = FindCorrespondingEntryLine(expectedLine, entryLineLookup);
				Assert(msg + ": Entry line not found.", entryLine != null);

				addInfoLookup = CreateAddInfoLookup_ToTestMergeOperation(entryLine);
				CompareAdditionalInfos_ToTestMergeOperation(msg, expectedLine.AdditionalInfoList, addInfoLookup);
			}
		}

		Dictionary<string, string> CreateEntryLineLookup(string msgText)
		{
			var lookup = new Dictionary<string, string>();
			var list = new List<string>();

			var pos = msgText.IndexOf("UNS+D\r\n");
			var fin = msgText.IndexOf("UNS+S\r\n");
			var txt = msgText.Substring(pos, fin - pos);

			while (true)
			{
				pos = txt.IndexOf("CST++");
				if (pos > -1)
				{
					int endPos = txt.IndexOf("CST++", pos + 4);
					if (endPos > -1)
					{
						var lineText = txt.Substring(pos, endPos - pos);
						list.Add(lineText);
						txt = txt.Substring(endPos);
					}
					else
					{
						var lineText = txt.Substring(pos);
						list.Add(lineText);
						break;
					}
				}
				else
				{
					break;
				}
			}

			foreach (var str in list)
			{
				string key = null;
				pos = str.IndexOf("MOA+38:");

				if (pos > -1)
				{
					key = str.Substring(pos + 7, 2);
				}
				else
				{
					pos = str.IndexOf("MEA+AAR++KGM:");
					if (pos > -1)
					{
						key = str.Substring(pos + 13, 2);
					}
				}

				if (key != null)
				{
					if (!lookup.ContainsKey(key))
					{
						lookup.Add(key, str);
					}
				}
			}
			return lookup;
		}

		string FindCorrespondingEntryLine(EntryLine_ToTestMergeOperation expectedLine, Dictionary<string, string> entryLineLookup)
		{
			string entryLine = null;
			entryLineLookup.TryGetValue(expectedLine.lookupKey, out entryLine);
			return entryLine;
		}

		Dictionary<string, EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo> CreateAddInfoLookup_ToTestMergeOperation(string entryLine)
		{
			var lookup = new Dictionary<string, EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>();
			var list = new List<string>();
			var txt = entryLine;

			while (true)
			{
				var pos = txt.IndexOf("FTX+ACB++");
				if (pos > -1)
				{
					int endPos = txt.IndexOf("\r\n", pos);
					int endPos2 = txt.IndexOf("+", pos + 9);
					if (endPos2 < endPos)
					{
						endPos = endPos2;
					}
					var code = txt.Substring(pos + 9, endPos - pos - 9);
					list.Add(code);
					txt = txt.Substring(endPos);
				}
				else
				{
					break;
				}
			}

			foreach (var code in list)
			{
				if (!lookup.ContainsKey(code))
				{
					var addInfo = Factory.New<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>();
					Factory.EnqueueDelete(typeof(EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo), addInfo.PK);
					addInfo.CSI_Code = code;
					lookup.Add(code, addInfo);
				}
			}

			return lookup;
		}

		Dictionary<string, EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo> CreateAddInfoLookup_Header_ToTestMergeOperation(string msgText)
		{
			var lookup = new Dictionary<string, EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>();
			var list = new List<string>();

			var pos = msgText.IndexOf("UNS+D\r\n");
			var txt = msgText.Substring(0, pos);

			while (true)
			{
				pos = txt.IndexOf("FTX+ACB++");
				if (pos > -1)
				{
					int endPos = txt.IndexOf("\r\n", pos);
					var code = txt.Substring(pos + 9, endPos - pos - 9);
					list.Add(code);
					txt = txt.Substring(endPos);
				}
				else
				{
					break;
				}
			}

			foreach (var code in list)
			{
				if (!lookup.ContainsKey(code))
				{
					var addInfo = Factory.New<EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo>();
					Factory.EnqueueDelete(typeof(EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo), addInfo.PK);
					addInfo.CSI_Code = code;
					lookup.Add(code, addInfo);
				}
			}
			return lookup;
		}

		#endregion  //End: "TestMergeOperation_GB_CHIEF"

		#region TestClassTypes_GB_CHIEF

		public override void TestClassTypesBeingUsed()
		{
			TestClassTypesBeingUsed_Core(Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF);
		}

		protected override void InspectVariousObjects_ToTestClassTypes(EU.Business.Declaration.JobDeclaration dec)
		{
			const string CHF = "GB CHIEF";

			var invoice = dec.Invoices[0];
			var invLine = invoice.InvoiceLines[0];

			CheckClassType(CHF, typeof(JobDeclaration), dec);
			CheckClassType(CHF, typeof(JobComInvoiceHeader), invoice);
			CheckClassType(CHF, typeof(JobComInvoiceLine), invLine);

			var entry = dec.CustomsEntryHeaders[0];
			var entryLine = entry.AllEntryLines[0];

			CheckClassType(CHF, typeof(Business.Declaration.CusEntryHeader), entry);
			CheckClassType(CHF, typeof(Business.Declaration.CusEntryLine), entryLine);

			var gbEntry = entry as Business.Declaration.CusEntryHeader;
			var gbEntryLine = entryLine as Business.Declaration.CusEntryLine;

			var list = new List<object>();

			list.AddRange(dec.AdditionalInfos);
			list.AddRange(invoice.AdditionalInfos);
			list.AddRange(invLine.AdditionalInfos);
			list.AddRange(gbEntry.AdditionalInfos);
			list.AddRange(gbEntryLine.AdditionalInfos);

			foreach (var obj in list)
			{
				CheckClassType(CHF, typeof(Business.Declaration.MultiLineAddInfos.AdditionalInfo), obj);
			}

			list.Clear();
			list.AddRange(dec.SupportingDocuments);
			list.AddRange(invoice.SupportingDocuments);
			list.AddRange(invLine.SupportingDocuments);
			list.AddRange(gbEntry.SupportingDocuments);
			list.AddRange(gbEntryLine.SupportingDocuments);

			foreach (var obj in list)
			{
				CheckClassType(CHF, typeof(SupportingDocument), obj);
			}

			list.Clear();
			list.AddRange(dec.PreviousDocuments);
			list.AddRange(invoice.PreviousDocuments);
			list.AddRange(invLine.PreviousDocuments);
			list.AddRange(gbEntry.PreviousDocuments);
			list.AddRange(gbEntryLine.PreviousDocuments);

			foreach (var obj in list)
			{
				CheckClassType(CHF, typeof(PreviousDocument), obj);
			}
		}

		#endregion  //End: "TestClassTypes_GB_CHIEF"

		public void TestIsUCC5()
		{
			var declaration = GetNewBusinessObject() as JobDeclaration;
			CombineAssertions(() =>
			{
				AssertEquals("Disabled when business object is not JobDeclaration", false, declaration.Configuration.IsUCC5(Factory.New<DummyBusinessObject>()));

				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
				AssertEquals("Disabled for export", true, declaration.Configuration.IsUCC5(declaration));
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
				AssertEquals("Disabled for import", true, declaration.Configuration.IsUCC5(declaration));
				declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.ExWarehouse;
				AssertEquals("Disabled for other", false, declaration.Configuration.IsUCC5(declaration));
			});
		}

		public override void TestCustomsOfficeRequirementHelper()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.CHIEF;
			AssertType<Business.JobDeclarationCustomsOfficeRequirementHelper>(declaration.CustomsOfficeRequirementHelper);
		}

		public override void TestCustomsOfficeOfExit()
		{
			JobDeclaration decEcs = Factory.New<JobDeclaration>();
			decEcs.JE_MessageType = "EXP";
			AssertEquals("", decEcs.OfficeOfExit);
			decEcs.JE_CustomsOffice = "GB000001";
			AssertEquals("GB000001", decEcs.OfficeOfExit);

			decEcs.JE_MessageType = "1";
			decEcs.JE_ApplicationCode = "EMC";
			Assert(decEcs.IsEMCS);
		}
	}
}
