using System;
using System.Collections.Generic;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.CustomsWare.Business.Testing
{
	public class CustomsWareIntegrationTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestCustomsWareSubmit()
		{
			var customsWareIntegration = new CustomsWareIntegrationTestClass();
			customsWareIntegration.ExecAPIResult = new XElement("TEST", new XElement("StatusCode", "0"));
			NUnit.Framework.Assert.That(customsWareIntegration.Execute(Declaration), Is.EqualTo("Submit Succeeded.").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(Declaration.LogsOfDeclarationOrShipment.MostRecentLogByEventTime(AutoEvents.CustomsCommenced), Is.Not.EqualTo(default(StmALog)), "customs clearance commenced event - should not be [null]");
			NUnit.Framework.Assert.That(declaration.JE_EntryStatus, Is.EqualTo(CustomsWareEntryStatusList.Codes.Submitted).Using(CustomComparers.TypeComparison), "Status Submitted");
			NUnit.Framework.Assert.That(Declaration.Messages.Count, Is.EqualTo(1));
			var message = Declaration.Messages[0];
			NUnit.Framework.Assert.That(message.EM_MessageText.Contains("<ConsignmentReference>DeclarationReference</ConsignmentReference>"), Is.EqualTo(true));
			NUnit.Framework.Assert.That(message.EM_ApplicationCode, Is.EqualTo(ApplicationCodeList.Codes.CustomsWare).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(message.EM_MessageType, Is.EqualTo(ApplicationCodeList.Codes.CustomsWare).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(message.EM_ReceiveTransmit, Is.EqualTo(EDIInterchange.Direction.Transmit).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(message.EM_Status, Is.EqualTo(EDIMessageStatusList.Codes.Sent).Using(CustomComparers.TypeComparison));
			customsWareIntegration.ExecAPIResult = new XElement("MessageBody", new XElement("ResponseList", new XElement("ResponseItem", new XElement("DataList", new XElement("DataItem", new XElement("DataMetaData", new XElement("StatusCode", "1")), new XElement("ErrorList", new XElement("ErrorItem", new XElement("ErrorIdentifier", "ErrorID"), new XElement("ErrorCode", "ErrorCode"), new XElement("ErrorDescription", "ErrorDescription"), new XElement("ErrorText", "ErrorText")), new XElement("ErrorItem", new XElement("ErrorIdentifier", "ErrorID1"), new XElement("ErrorCode", "ErrorCode1"), new XElement("ErrorDescription", "ErrorDescription1"), new XElement("ErrorText", "ErrorText1"))))))));
			NUnit.Framework.Assert.That(customsWareIntegration.Execute(Declaration), CustomConstraints.MultilineEquals("Status Code: 1\n\nError Identifier: ErrorID\nError Code: ErrorCode\nError Description: ErrorDescription\n\nError Identifier: ErrorID1\nError Code: ErrorCode1\nError Description: ErrorDescription1", '\n'), "Error Message");
			NUnit.Framework.Assert.That(Declaration.IsInDatabase, Is.EqualTo(true), "Saved");
		}

		[ExpectNoExceptions]
		public void TestSubmitIfNoErrorReturnedThenShowFullResponse()
		{
			var customsWareIntegration = new CustomsWareIntegrationTestClass();
			customsWareIntegration.ExecAPIResult = new XElement("Arbitrary", new XElement("Response", "Exception"));
			NUnit.Framework.Assert.That(customsWareIntegration.Execute(Declaration), Is.EqualTo("<Arbitrary>\r\n  <Response>Exception</Response>\r\n</Arbitrary>").Using(CustomComparers.TypeComparison), "Error Message");
		}

		[ExpectNoExceptions]
		public void TestSubmitWithEmptyUriAndCompany()
		{
			CustomsWareRegistry.Instance.URI.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			CustomsDataRegistry.Instance.CustomsWareCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			var integration = new CustomsWareIntegration();
			var submitResult = integration.Execute(Declaration);
			NUnit.Framework.Assert.That(submitResult, Is.EqualTo("Uri of Web Service should not be empty or invalid.\n" + "Company of Web Service should not be empty or invalid.\n" + "\nThis can be entered in the Registry: System > Registry > Customs > Integration > ABM.").Using(CustomComparers.TypeComparison));
		}

		#region Declaration
		BaseJobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<BaseJobDeclaration>();
					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					declaration.JE_DeclarationReference = "DeclarationReference";
				}

				return declaration;
			}
		}

		BaseJobDeclaration declaration;
		#endregion
		#region CustomsWareIntegrationTestClass
		class CustomsWareIntegrationTestClass : CustomsWareIntegration
		{
			protected override XElement Submit(ZString data)
			{
				return ExecAPIResult;
			}

			public XElement ExecAPIResult = new XElement("TEST");
			protected override ICollection<SettingDetail> SettingsToValidate
			{
				get
				{
					return new List<SettingDetail>();
				}
			}
		}
		#endregion
	}
}
