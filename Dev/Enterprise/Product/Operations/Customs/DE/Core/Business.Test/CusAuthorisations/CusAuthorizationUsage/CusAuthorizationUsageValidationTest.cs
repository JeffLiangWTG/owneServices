using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.Business.CusAuthorizationHeaderTypeList.Codes;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class CusAuthorizationUsageValidationTest : TestCaseWithFactory
	{
		public void TestCheckAGC_Code_Unique()
		{
			const string message = "has already been entered";
			cusAuthorizationUsage.AGC_Code = EndUse;
			var cusAuthorizationUsage2 = entryInstruction.CusAuthorizationUsages.AddNew();
			NUnit.Framework.Assert.Multiple(() =>
			{
				cusAuthorizationUsage2.AGC_Code = EndUse;
				AssertHasMessageErrorContaining("For Duplicate authorisation records", cusAuthorizationUsage2.AGC_CodeInfo, message);

				cusAuthorizationUsage2.AGC_Code = SimplifiedDeclaration;
				AssertNoMessageErrorContaining("For unique authorisation records", cusAuthorizationUsage2.AGC_CodeInfo, message);
			});
		}

		public void TestCheckAGC_Code_CheckCwpCw1Cw2AreMutuallyExclusive()
		{
			const string message = "The Authorization Types CWP, CW1 and CW2 are mutually exclusive.";

			NUnit.Framework.Assert.Multiple(() =>
			{
				AssertMessageErrorForConditions(expectMessageError: true, "CWP", "CW1", "CW2");
				AssertMessageErrorForConditions(expectMessageError: true, "CWP", "CW1");
				AssertMessageErrorForConditions(expectMessageError: true, "CW1", "CW2");
				AssertMessageErrorForConditions(expectMessageError: true, "CWP", "CW2");

				AssertMessageErrorForConditions(expectMessageError: false, "CWP");
				AssertMessageErrorForConditions(expectMessageError: false, "CW2");
				AssertMessageErrorForConditions(expectMessageError: false, "CW1", "CW1");
			});

			void AssertMessageErrorForConditions(bool expectMessageError, params string[] codes)
			{
				entryInstruction.CusAuthorizationUsages.RemoveAll();
				foreach (var code in codes)
				{
					var usage = entryInstruction.CusAuthorizationUsages.AddNew();
					usage.AGC_Code = code;
				}

				foreach (var usage in entryInstruction.CusAuthorizationUsages)
				{
					usage.Validation.ValidateAGC_Code();
					var assertMessage = $"For authorisation {usage.AGC_Code} when entry instruction has authorisations of {string.Join(", ", codes)}";
					if (expectMessageError)
					{
						AssertHasMessageError(assertMessage, usage.AGC_CodeInfo, message);
					}
					else
					{
						AssertNoMessageError(assertMessage, usage.AGC_CodeInfo, message);
					}
				}
			}
		}

		public void TestCheckAGC_OH_Owner_ValidateCusAuthorisationUsage()
		{
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.AZ;
			NUnit.Framework.Assert.Multiple(() =>
			{
				declaration.JE_DeclarantType = ZString.Empty;
				cusAuthorizationUsage.AGC_Code = EndUse;
				cusAuthorizationUsage.AGC_OH_Owner = declaration.RepresentativeOrgAddress.OA_OH;
				AssertHasMessageError("Has message error", cusAuthorizationUsage.AGC_OH_OwnerInfo, CusAuthorizationHelper.AuthHolderAndDeclarantToBeSame);

				cusAuthorizationUsage.AGC_OH_Owner = declaration.DeclarantOrgAddress.OA_OH;
				AssertNoMessageError("No message error", cusAuthorizationUsage.AGC_OH_OwnerInfo, CusAuthorizationHelper.AuthHolderAndDeclarantToBeSame);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.JE_OA_DeclarantAddress = Factory.New<OrgHeader>().MainAddress.PK;
			declaration.JE_OA_Representative = Factory.New<OrgHeader>().MainAddress.PK;
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
		}
		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusAuthorizationUsage cusAuthorizationUsage;
	}
}
