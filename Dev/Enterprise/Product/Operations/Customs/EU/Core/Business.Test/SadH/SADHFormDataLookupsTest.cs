using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.SADH;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.Business.Testing
{
	internal class SADHFormDataLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestEntryStyleList()
		{
			AssertEntryStyleLookupIsUsingSADHMessageType(MessageTypeList.Codes.Import);
			AssertEntryStyleLookupIsUsingSADHMessageType(MessageTypeList.Codes.Export);
			AssertEntryStyleLookupIsUsingSADHMessageType(MessageTypeList.Codes.MiscellaneousCustoms);
		}
		void AssertEntryStyleLookupIsUsingSADHMessageType(string messageType)
		{
			Declaration.JE_MessageType = messageType;
			SADHData.D1_MessageType = MessageTypeList.Codes.Import;
			AssertEquals(LookupIsUsingSADHMessageTypeMessage, typeof(CodeDescriptionPairList), SADHData.Lookups.EntryStyleList.GetType());
			SADHData.D1_MessageType = MessageTypeList.Codes.Export;
			AssertEquals(LookupIsUsingSADHMessageTypeMessage, typeof(CodeDescriptionPairList), SADHData.Lookups.EntryStyleList.GetType());
			SADHData.D1_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals(LookupIsUsingSADHMessageTypeMessage, typeof(CodeDescriptionPairList), SADHData.Lookups.EntryStyleList.GetType());
		}

		const string LookupIsUsingSADHMessageTypeMessage = "If the GB JobDeclaration's Lookups have changed, this method needs to be\r\n" +
														   "modified to test for the modified types that the Declaration now returns.";

		#region Implementation
		protected SADHFormData SADHData
		{
			get { return fSADHData ?? (fSADHData = new SADHFormData(Factory, Declaration)); }
		}
		SADHFormData fSADHData;

		protected JobDeclaration Declaration
		{
			get { return fDeclaration ?? (fDeclaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration fDeclaration;
		#endregion
	}
}
