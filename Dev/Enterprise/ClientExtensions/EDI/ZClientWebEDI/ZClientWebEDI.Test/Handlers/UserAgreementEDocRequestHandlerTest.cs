using System;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	[TestedType(typeof(UserAgreementEDocRequestHandler))]
	class UserAgreementEDocRequestHandlerTest : DataRequestHandlerTestCase<UserAgreementEDocRequestHelper>
	{
		public void TestGetBinaryData()
		{
			var bytes = RequestHandler.GetBinaryData();
			AssertEquals([1, 1, 1, 1], bytes);
		}

		public override void TestGetBinaryDataWithLock()
		{
			AssertNotNull("Nothing to lock in this class");
		}

		public void TestFileName()
		{
			AssertEquals("TAX INVOICE - AUS00335939 - JUSINTHKG (30-Jun-23).pdf", RequestHandler.FileName);
		}

		protected override DataRequestHandler<UserAgreementEDocRequestHelper> GetNewRequestHandler()
		{
			var result = new DummyUserAgreementEDocRequestHandler();
			result.QueryString.Add(DataRequestHelper.DataKey, agreement.PK.ToString());

			var tokenScope = new UserAgreementQueryToken();
			tokenScope.AgreementType = agreement.ERA_Type;
			var tokenInfo = new AccessTokenInfo(tokenScope.ToJson(), agreement.PK.ToGuid(), EdiUserAgreementSchema.Constants.TableName);
			var token = new TokenizedAccessControl().CreateLimitedToken(AccessTokenTypes.MyAccountUserAgreement, tokenInfo, TimeSpan.FromMinutes(10), maxUses: 1);

			result.QueryString.Add(UserAgreementEDocRequestHelper.TokenKey, token);
			result.QueryString.Add(UserAgreementEDocRequestHelper.UniqueKey, uniqueKey.ToString());

			AssertNotNull("Precondition", result.EDoc_Exposed);
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			agreement = Factory.NewWithValidTestData<EdiUserAgreement>();
			var edoc = agreement.DocManagerInfo.AddFileOrDocument([1, 1, 1, 1], "TAX INVOICE - AUS00335939 - JUSINTHKG (30-Jun-23).pdf", "INV");
			uniqueKey = edoc.UniqueKey;
			AssertEquals("Precondition", edoc, agreement.DocManagerInfo.AllEDocs.GetFromUniqueKey(uniqueKey.ToGuid()));
			agreement.DocManagerInfo.Save();
			Factory.Save();
		}

		ZGuid uniqueKey;

		EdiUserAgreement agreement;
		class DummyUserAgreementEDocRequestHandler : UserAgreementEDocRequestHandler
		{
			protected override BusinessObject[] GetNewBusinessObjects()
			{
				var userAgreement = new BusinessObjectFactory().Load<EdiUserAgreement>(PKs[0]);
				return [userAgreement];
			}

			public IeDoc EDoc_Exposed => EDoc;
		}
	}
}
