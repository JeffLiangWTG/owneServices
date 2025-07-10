using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(ApprovalCertificateInfoCollection))]
	sealed class ApprovalCertificateInfoCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<ApprovalCertificateInfo>
	{
		public void TestMaxCountValidation()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			var approvalCertificateInfos = entryInstruction.ApprovalCertificateInfos;
			approvalCertificateInfos.RemoveAndDeleteAll();

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			for (var i = 0; i < 15; i++)
			{
				approvalCertificateInfos.AddNew();
			}

			CombineAssertions(() =>
			{
				AssertEquals("Maximum allowed for Export", false, approvalCertificateInfos.HasErrors());
				AssertNoRowMessageError(approvalCertificateInfos.Last(), "The row count has exceeded the maximum limit. The maximum allowed number of rows is 15.");

				var info = approvalCertificateInfos.AddNew();
				AssertHasRowMessageError(info, "The row count has exceeded the maximum limit. The maximum allowed number of rows is 15.");
			});

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var lastIndex = approvalCertificateInfos.Count - 1;
			for (var i = lastIndex; i >= 10; i--)
			{
				AssertHasRowMessageError(approvalCertificateInfos[i], "The row count has exceeded the maximum limit. The maximum allowed number of rows is 10.");
				approvalCertificateInfos.RemoveAndDelete(approvalCertificateInfos[i]);
			}

			CombineAssertions(() =>
			{
				AssertEquals("Maximum allowed for Import", false, approvalCertificateInfos.HasErrors());
				AssertNoRowMessageError(approvalCertificateInfos.Last(), "The row count has exceeded the maximum limit. The maximum allowed number of rows is 10.");
			});
		}

		public void TestAddNewMaxCount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var testCollection = entryInstruction.ApprovalCertificateInfos;

			testCollection.RemoveAndDeleteAll();
			declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
			for (var i = 0; i < 14; i++)
			{
				testCollection.AddNew();
			}
			Assert($"[Export]Currently, collection has {testCollection.Count} elements", testCollection.AllowNew);
			testCollection.AddNew();
			Assert($"[Export]Currently, collection has {testCollection.Count} elements", !testCollection.AllowNew);

			testCollection.RemoveAndDeleteAll();
			declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			for (var i = 0; i < 9; i++)
			{
				testCollection.AddNew();
			}
			Assert($"[Import]Currently, collection has {testCollection.Count} elements", testCollection.AllowNew);
			testCollection.AddNew();
			Assert($"[Import]Currently, collection has {testCollection.Count} elements", !testCollection.AllowNew);
		}

		protected override CusSupportingInfoCollection<ApprovalCertificateInfo> GetCusSupportingInfoCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			return new ApprovalCertificateInfoCollection(entryInstruction);
		}
	}
}
