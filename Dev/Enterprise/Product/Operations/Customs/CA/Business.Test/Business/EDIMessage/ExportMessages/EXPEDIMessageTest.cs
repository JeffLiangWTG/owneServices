using System;
using System.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business.MessageBuilders;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(EXPEDIMessage))]
	class EXPEDIMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGenerateEntryNumberWhenSendMessage()
		{
			var newFactory = new BusinessObjectFactory();
			var declaration = newFactory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = new G7ExportMessageWrapper(entry);
			var builder = new G7ExportMessageBuilder(wrapper, MessageSubTypes.Create);
			newFactory.Save();
			AssertEquals(ZString.Empty, entry.EntryNumber);

			builder.PopulateMessages();
			var query = new ZQuery();
			var newMessage = newFactory.LoadTop1<EDIMessage>(query);
			var messageObject = newMessage.EM_LinkedObject as CusEntryHeader;
			newFactory.Save();
			AssertEquals(ZString.Empty, entry.EntryNumber);
		}

		public void TestGenerateEntryNumberWhenSendMessage_GetEntryNumber()
		{
			var newFactory = new BusinessObjectFactory();
			var declaration = newFactory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			var wrapper = new G7ExportMessageWrapper(entry);
			var builder = new G7ExportMessageBuilder(wrapper, MessageSubTypes.Create);
			newFactory.Save();
			AssertEquals(ZString.Empty, entry.EntryNumber);

			builder.PopulateMessages();
			var query = new ZQuery();
			var newMessage = newFactory.LoadTop1<EDIMessage>(query);
			var messageObject = newMessage.EM_LinkedObject as CusEntryHeader;
			newMessage.EM_MessageInterpretation = "&lt;&lt;ENTRY NUMBER PLACE HOLDER&gt;&gt;";
			newFactory.Save();
			AssertEquals(ZString.Empty, entry.EntryNumber);
		}

		public void TestResetEntryNumberWhenMessageSaveFailed()
		{
			var newFactory = new BusinessObjectFactory();
			var company = GlbCompany.GetCurrentCompany(newFactory);
			var companyProxy = company.OrgProxy;
			var canada = newFactory.Load<RefCountry>(Core.Constants.CountryGuids.Canada);
			companyProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.ExportLicenceNumber, "EXPLIC", canada);
			var declaration = newFactory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry = declaration.CustomsEntryHeaders.AddNew();
			newFactory.Save();
			var newMessage = newFactory.New<EXPEDIMessageForTest>();
			newMessage.EM_LinkedObject = entry;
			newMessage.EM_MessageText = "&lt;&lt;ENTRY NUMBER PLACE HOLDER&gt;&gt;";
			newMessage.EM_MessageInterpretation = "&lt;&lt;ENTRY NUMBER PLACE HOLDER&gt;&gt;";
			AssertExceptionThrown<Exception>(() =>
			{
				newFactory.Save();
			});
			AssertEquals(ZString.Empty, entry.EntryNumber);
		}

		class EXPEDIMessageForTest : EXPEDIMessage
		{
			public EXPEDIMessageForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public override void OnSaving()
			{
				base.OnSaving();
				throw new Exception("Do not save.");
			}
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = (EDIMessage)GetNewBusinessObject();
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			result.EM_LinkUniqueID = entryHeader.PK;
			result.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			return result;
		}

		protected override bool CanPersistedObjectBeDeleted => false;

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<EXPEDIMessage>();
		}

		public virtual void TestDefaultValues()
		{
			AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.CAEXP, message.EM_ApplicationCode);
			AssertEquals("Interpretation should be visible", true, message.ShouldShowInterpretation);
		}

		[TestDate(2021, 05, 25, 15, 39, 42)]
		[UseSnapshotProtection]
		public void TestMessageAndEntryNumbersFilledIn()
		{
			ZString fountainKey = ZDateTime.Now.Year.ToString();
			fountainKey = fountainKey.SubstringSafe(fountainKey.Length - 1, 1);
			fountainKey = "EXPLIC" + fountainKey;

			var number = Env.NumberFountains.GetCAEntryNumberGeneratorFountain(fountainKey).PeekPreliminaryFormatted(Factory).Substring(2, 6);
			Factory.Save();
			AssertContains("MessageNumberFilledIn", "Message Number = 1 Entry Number =", message.EM_MessageText);
		}

		[UseSnapshotProtection]
		public override void TestFetchForLoad()
		{
			base.TestFetchForLoad();
		}

		[UseSnapshotProtection]
		public override void TestSaveAndDeleteBusinessObject()
		{
			base.TestSaveAndDeleteBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			var company = GlbCompany.GetCurrentCompany(Factory);
			var companyProxy = company.OrgProxy;
			var canada = Factory.Load<RefCountry>(Core.Constants.CountryGuids.Canada);
			companyProxy.CustomsCodes.AddNew(OrgCusCode.CACodeTypes.ExportLicenceNumber, "EXPLIC", canada);
			var jobDeclaration = Factory.New<JobDeclaration>();
			entryHeader = jobDeclaration.CustomsEntryHeaders.AddNew();
			message = (EDIMessage)GetNewBusinessObject();
			message.EM_LinkUniqueID = entryHeader.PK;
			message.EM_LinkTable = CusEntryHeaderSchema.Constants.TableName;
			message.EM_MessageText = "Message Number = " + EDIMessage.MessageNumberPlaceHolder + " Entry Number = " + EDIMessage.EntryNumberPlaceHolder;
		}
		protected EDIMessage message;
		CusEntryHeader entryHeader;
	}
}
