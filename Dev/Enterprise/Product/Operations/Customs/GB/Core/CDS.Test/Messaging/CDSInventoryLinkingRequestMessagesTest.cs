using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.Business.Common;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders;
using Enterprise.Customs.GB.Registry;
using GBCustomsDec = Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Testing.Messaging
{
	public class CDSInventoryLinkingRequestMessagesTest : TestCaseWithFactory
	{
		public void TestCDSExportInventoryLinkingAssociateRequestMessage()
		{
			CombineAssertions(() =>
			{
				TestScenarios(GbCusDecMessageFunctionsList.Codes.Associate, CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields);
				TestScenarios(GbCusDecMessageFunctionsList.Codes.Associate, CDSUCRAutomationSettingsList.Codes.SendOnlyASingleEntryReference);
				TestScenarios(GbCusDecMessageFunctionsList.Codes.Associate, CDSUCRAutomationSettingsList.Codes.NotForImports);
			});
		}

		public void TestCDSExportInventoryLinkingDisAssociateRequestMessage()
		{
			CombineAssertions(() =>
			{
				TestScenarios(GbCusDecMessageFunctionsList.Codes.Disassociate, CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields);
				TestScenarios(GbCusDecMessageFunctionsList.Codes.Disassociate, CDSUCRAutomationSettingsList.Codes.SendOnlyASingleEntryReference);
				TestScenarios(GbCusDecMessageFunctionsList.Codes.Disassociate, CDSUCRAutomationSettingsList.Codes.NotForImports);
			});
		}

		public void TestCDSExportInventoryLinkingDUCRQueryDeclarationRequestMessage()
		{
			CombineAssertions(() =>
			{
				TestScenarios(GbCusDecMessageFunctionsList.Codes.QueryDeclaration, CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields);
				TestScenarios(GbCusDecMessageFunctionsList.Codes.QueryDeclaration, CDSUCRAutomationSettingsList.Codes.SendOnlyASingleEntryReference);
				TestScenarios(GbCusDecMessageFunctionsList.Codes.QueryDeclaration, CDSUCRAutomationSettingsList.Codes.NotForImports);
			});
		}

		public void TestCDSExportInventoryLinkingArrivalAtLocationMovementRequestMessage()
		{
			CombineAssertions(() =>
			{
				TestScenarios(GbCusDecMessageFunctionsList.Codes.ArrivalAtLocation, CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields);
				TestScenarios(GbCusDecMessageFunctionsList.Codes.ArrivalAtLocation, CDSUCRAutomationSettingsList.Codes.SendOnlyASingleEntryReference);
				TestScenarios(GbCusDecMessageFunctionsList.Codes.ArrivalAtLocation, CDSUCRAutomationSettingsList.Codes.NotForImports);
			});
		}

		public void TestCDSExportInventoryLinkingDepartureRequestMessageBuilderRequestMessage()
		{
			CombineAssertions(() =>
			{
				TestScenarios(GbCusDecMessageFunctionsList.Codes.DepartureFromLocation, CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields);
				TestScenarios(GbCusDecMessageFunctionsList.Codes.DepartureFromLocation, CDSUCRAutomationSettingsList.Codes.SendOnlyASingleEntryReference);
				TestScenarios(GbCusDecMessageFunctionsList.Codes.DepartureFromLocation, CDSUCRAutomationSettingsList.Codes.NotForImports);
			});
		}

		public void TestCDSExportInventoryLinkingAnticipatedArrivalAtLocationRequestMessage()
		{
			CombineAssertions(() =>
			{
				TestScenarios(GbCusDecMessageFunctionsList.Codes.AnticipatedArrivalAtLocation, CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields);
				TestScenarios(GbCusDecMessageFunctionsList.Codes.AnticipatedArrivalAtLocation, CDSUCRAutomationSettingsList.Codes.SendOnlyASingleEntryReference);
				TestScenarios(GbCusDecMessageFunctionsList.Codes.AnticipatedArrivalAtLocation, CDSUCRAutomationSettingsList.Codes.NotForImports);
			});
		}

		public void TestCDSExportInventoryLinkingMUCRQueryDeclarationRequestMessage()
		{
			CombineAssertions(() =>
			{
				TestScenarios(CDSEDIMessageTypeList.Codes.MasterQueryDeclaration, CDSUCRAutomationSettingsList.Codes.SendOnlyASingleEntryReference);
				TestScenarios(CDSEDIMessageTypeList.Codes.MasterQueryDeclaration, CDSUCRAutomationSettingsList.Codes.NotForImports);
			});
		}

		#region Expected Results
		ZString ExpectedResultAssociateRequest(ZBool bSuffix, string option, string bgRef)
		{
			var results = string.Empty;
			if (option == CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				&& bSuffix)
			{
				results = @"<inventoryLinkingConsolidationRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
					<messageCode>EAC</messageCode>
					<masterUCR>MUCR</masterUCR>
					<ucrBlock>
						<ucr>6GB945390992000S00001410</ucr>
						<ucrPartNo>1</ucrPartNo>
						<ucrType>D</ucrType>
					</ucrBlock>
				</inventoryLinkingConsolidationRequest>";
			}
			else
			{
				results = (@"<inventoryLinkingConsolidationRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
					<messageCode>EAC</messageCode>
					<masterUCR>MUCR</masterUCR>
					<ucrBlock>
						<ucr>XXXX</ucr>		
						<ucrType>D</ucrType>
					</ucrBlock>
				</inventoryLinkingConsolidationRequest>").Replace("XXXX", bgRef);
			}
			return results;
		}

		ZString ExpectedResultDisAssociateRequest(ZBool bSuffix, string option, string bgRef)
		{
			var results = string.Empty;
			if (option == CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				&& bSuffix)
			{
				results = @"<inventoryLinkingConsolidationRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
						<messageCode>EAC</messageCode>	
						<ucrBlock>
							<ucr>6GB945390992000S00001410</ucr>
							<ucrPartNo>1</ucrPartNo>
							<ucrType>D</ucrType>
						</ucrBlock>
					</inventoryLinkingConsolidationRequest>";
			}
			else
			{
				results = (@"<inventoryLinkingConsolidationRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
					<messageCode>EAC</messageCode>	
					<ucrBlock>
						<ucr>XXXX</ucr>		
						<ucrType>D</ucrType>
					</ucrBlock>
				</inventoryLinkingConsolidationRequest>").Replace("XXXX", bgRef);
			}

			return results;
		}

		ZString ExpectedResultQueryDeclarationRequest(ZBool bSuffix, string option, string bgRef)
		{
			var results = string.Empty;
			if (option == CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				&& bSuffix)
			{
				results = @"<inventoryLinkingQueryRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">							
							<queryUCR>
								<ucr>6GB945390992000S00001410</ucr>
								<ucrPartNo>1</ucrPartNo>
								<ucrType>D</ucrType>
							</queryUCR>
						</inventoryLinkingQueryRequest>";
			}
			else
			{
				results = (@"<inventoryLinkingQueryRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">						
							<queryUCR>
								<ucr>XXXX</ucr>		
								<ucrType>D</ucrType>
							</queryUCR>
						</inventoryLinkingQueryRequest>").Replace("XXXX", bgRef);
			}

			return results;
		}

		ZString ExpectedResultQueryMovementDeclarationRequest(ZBool bSuffix, string option, string bgRef)
		{
			var results = string.Empty;
			if (option == CDSUCRAutomationSettingsList.Codes.SplitEntryReferenceIntoDucrAndPartFields
				&& bSuffix)
			{
				results = @"<ucrBlock xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
						<ucr>6GB945390992000S00001410</ucr>
						<ucrPartNo>1</ucrPartNo>
						<ucrType>D</ucrType>
					</ucrBlock>";
			}
			else
			{
				results = (@"<ucrBlock xmlns=""http://gov.uk/customs/inventoryLinking/v1"">
						<ucr>XXXX</ucr>						
						<ucrType>D</ucrType>
					</ucrBlock>").Replace("XXXX", bgRef);
			}

			return results;
		}

		ZString ExpectedResultMasterQueryDeclarationRequest(string bgRef)
		{
			return (@"<inventoryLinkingQueryRequest xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""http://gov.uk/customs/inventoryLinking/v1"">						
					<queryUCR>
						<ucr>XXXX</ucr>		
						<ucrType>M</ucrType>
					</queryUCR>
				</inventoryLinkingQueryRequest>").Replace("XXXX", bgRef);
		}

		ZString ReplaceXML(string xml, GBCustomsDec.CusEntryHeader entryHeader)
		{
			return xml.Replace(GBCustomsDec.CusEntryHeader.UCRReferencePlaceHolder, entryHeader.DeclarationUCR)
				.Replace(GBCustomsDec.CusEntryHeader.UCRPartPlaceHolder, entryHeader.DeclarationUCRPartSuffix)
				.Replace(GBCustomsDec.CusEntryHeader.BGMReferencePlaceHolderXmlFriendly, entryHeader.CH_BGMReference)
				.Replace(GBCustomsDec.CusEntryHeader.UCRReferencePlaceHolderXmlFriendly, entryHeader.DeclarationUCR)
				.Replace(GBCustomsDec.CusEntryHeader.UCRPartPlaceHolderXmlFriendly, entryHeader.DeclarationUCRPartSuffix);
		}

		ZString GetMessageBuilderXML(GBCustomsDec.CusEntryHeader entryHeader, ZString functionCode)
		{
			var cei = entryHeader.Declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = cei.PK;
			var ucrnumber = Factory.New<CusEntryNumber>();
			ucrnumber.CE_ParentID = entryHeader.PK;
			ucrnumber.CE_EntryType = CusEntryNumberTypes.Standard.UniqueConsignementReference;
			ucrnumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedKingdom;
			ucrnumber.CE_EntryNum = entryHeader.CH_BGMReference;

			var messageBuilderManager = new MessageBuilderManager();
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);
			var msgSendingObj = MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader);
			msgSendingObj.MessageType = functionCode;
			var errorCollector = new EU.Business.ErrorCollector();
			var msgFuncNew = new CusdecMessageFunction.New();
			var messageBuilder = messageBuilderManager.NewMessageBuilder(msgSendingObj, errorCollector, msgFuncNew);
			var actualXml = ReplaceXML(messageBuilder.Build(), entryHeader);

			return actualXml;
		}

		#endregion

		void TestScenarios(string functionCode, string option)
		{
			ZString actualResult = string.Empty;
			ZString expectedResult = string.Empty;

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			dec.JE_UCR = "6GB945390992000S00001410";

			var entryHeader = dec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_MasterUCR = "MUCR";

			ZString errorMessage = $"Failed function: {functionCode} ucr - #XXXX# for the option - {option}";

			GBCustomsDataRegistry.Instance.CDSDUCRAutomation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new CDSDUCRAutomationSettings { CDSDUCRAutomation = option });

			if (functionCode == GbCusDecMessageFunctionsList.Codes.Associate)
			{
				entryHeader.CH_BGMReference = "6GB945390992000S00001410/1";
				actualResult = GetMessageBuilderXML(entryHeader, functionCode);
				expectedResult = ExpectedResultAssociateRequest(entryHeader.CH_BGMReference.Split('/').Length > 1, option,
					entryHeader.CH_BGMReference);
				Assert(errorMessage.Replace("#XXXX#", entryHeader.CH_BGMReference), XNode.DeepEquals(NormalizeNamespaces(expectedResult), NormalizeNamespaces(actualResult)));

				entryHeader.CH_BGMReference = "6GB945390992000S00001410";
				actualResult = GetMessageBuilderXML(entryHeader, functionCode);
				expectedResult = ExpectedResultAssociateRequest(entryHeader.CH_BGMReference.Split('/').Length > 1, option,
					entryHeader.CH_BGMReference);
				Assert(errorMessage.Replace("#XXXX#", entryHeader.CH_BGMReference), XNode.DeepEquals(NormalizeNamespaces(expectedResult), NormalizeNamespaces(actualResult)));
			}
			else if (functionCode == GbCusDecMessageFunctionsList.Codes.Disassociate)
			{
				entryHeader.CH_BGMReference = "6GB945390992000S00001410/1";
				actualResult = GetMessageBuilderXML(entryHeader, functionCode);
				expectedResult = ExpectedResultDisAssociateRequest(entryHeader.CH_BGMReference.Split('/').Length > 1, option,
					entryHeader.CH_BGMReference);
				Assert(errorMessage.Replace("#XXXX#", entryHeader.CH_BGMReference), XNode.DeepEquals(NormalizeNamespaces(expectedResult), NormalizeNamespaces(actualResult)));

				entryHeader.CH_BGMReference = "6GB945390992000S00001410";
				actualResult = GetMessageBuilderXML(entryHeader, functionCode);
				expectedResult = ExpectedResultDisAssociateRequest(entryHeader.CH_BGMReference.Split('/').Length > 1, option,
					entryHeader.CH_BGMReference);
				Assert(errorMessage.Replace("#XXXX#", entryHeader.CH_BGMReference), XNode.DeepEquals(NormalizeNamespaces(expectedResult), NormalizeNamespaces(actualResult)));
			}
			else if (functionCode == GbCusDecMessageFunctionsList.Codes.QueryDeclaration)
			{
				entryHeader.CH_BGMReference = "6GB945390992000S00001410/1";
				actualResult = GetMessageBuilderXML(entryHeader, functionCode);
				expectedResult = ExpectedResultQueryDeclarationRequest(entryHeader.CH_BGMReference.Split('/').Length > 1, option,
					entryHeader.CH_BGMReference);
				Assert(errorMessage.Replace("#XXXX#", entryHeader.CH_BGMReference), XNode.DeepEquals(NormalizeNamespaces(expectedResult), NormalizeNamespaces(actualResult)));

				entryHeader.CH_BGMReference = "6GB945390992000S00001410";
				actualResult = GetMessageBuilderXML(entryHeader, functionCode);
				expectedResult = ExpectedResultQueryDeclarationRequest(entryHeader.CH_BGMReference.Split('/').Length > 1, option,
					entryHeader.CH_BGMReference);
				Assert(errorMessage.Replace("#XXXX#", entryHeader.CH_BGMReference), XNode.DeepEquals(NormalizeNamespaces(expectedResult), NormalizeNamespaces(actualResult)));
			}
			else if (functionCode == GbCusDecMessageFunctionsList.Codes.ArrivalAtLocation ||
				functionCode == GbCusDecMessageFunctionsList.Codes.AnticipatedArrivalAtLocation ||
				functionCode == GbCusDecMessageFunctionsList.Codes.DepartureFromLocation)
			{
				entryHeader.CH_BGMReference = "6GB945390992000S00001410/1";
				actualResult = GetMessageBuilderXML(entryHeader, functionCode);
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(actualResult);
				actualResult = doc.GetElementsByTagName("ucrBlock")[0].OuterXml;
				expectedResult = ExpectedResultQueryMovementDeclarationRequest(entryHeader.CH_BGMReference.Split('/').Length > 1, option,
					entryHeader.CH_BGMReference);
				Assert(errorMessage.Replace("#XXXX#", entryHeader.CH_BGMReference), XNode.DeepEquals(NormalizeNamespaces(expectedResult), NormalizeNamespaces(actualResult)));

				entryHeader.CH_BGMReference = "6GB945390992000S00001410";
				actualResult = GetMessageBuilderXML(entryHeader, functionCode);
				doc.LoadXml(actualResult);
				actualResult = doc.GetElementsByTagName("ucrBlock")[0].OuterXml;
				expectedResult = ExpectedResultQueryMovementDeclarationRequest(entryHeader.CH_BGMReference.Split('/').Length > 1, option,
					entryHeader.CH_BGMReference);
				Assert(errorMessage.Replace("#XXXX#", entryHeader.CH_BGMReference), XNode.DeepEquals(NormalizeNamespaces(expectedResult), NormalizeNamespaces(actualResult)));
			}
			else if (functionCode == CDSEDIMessageTypeList.Codes.MasterQueryDeclaration)
			{
				actualResult = GetMessageBuilderXML(entryHeader, functionCode);
				expectedResult = ExpectedResultMasterQueryDeclarationRequest(entryHeader.CH_MasterUCR);
				Assert(errorMessage.Replace("#XXXX#", entryHeader.CH_MasterUCR), XNode.DeepEquals(NormalizeNamespaces(expectedResult), NormalizeNamespaces(actualResult)));
			}
		}

		static XElement NormalizeNamespaces(ZString input, XNamespace defaultNamespace = null)
		{
			var element = XElement.Parse(input);
			return NormalizeNamespaces(element, defaultNamespace);
		}

		static XElement NormalizeNamespaces(XElement element, XNamespace defaultNamespace = null)
		{
			var currentNs = element.Name.Namespace;
			var effectiveNs = currentNs != XNamespace.None ? currentNs : defaultNamespace ?? XNamespace.None;

			return new XElement(
				effectiveNs + element.Name.LocalName,
				element.Attributes().Where(a => !a.IsNamespaceDeclaration),
				element.Nodes().Select(n =>
				{
					if (n is XElement child)
					{
						return NormalizeNamespaces(child, effectiveNs);
					}
					else if (n is XText text)
					{
						return new XText(text.Value.Trim());
					}
					else
					{
						return n;
					}
				})
			);
		}
	}
}
