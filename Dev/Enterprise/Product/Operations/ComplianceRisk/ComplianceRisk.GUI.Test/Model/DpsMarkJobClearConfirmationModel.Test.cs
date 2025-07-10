using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.GUI.Tests
{
	[TestedType(typeof(DpsMarkJobClearConfirmationModel))]
	public class DpsMarkJobClearConfirmationModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestReason()
		{
			var model = new DpsMarkJobClearConfirmationModel();
			AssertEquals(200, model.ReasonInfo.MaxLength);
		}

		public void TestValidationReason()
		{
			var model = new DpsMarkJobClearConfirmationModel();
			var itemCollection = new RequireReasonForCLRItemCollection
			{
				new RequireReasonForCLRItem { Code = "CCC", Title = "Title1", IsMandatory = true }
			};

			var requireReasonWrapper = new RequireReasonForCLRWrapper(itemCollection)
			{
				RequireReasonForCLR = false
			};

			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForJobClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requireReasonWrapper))
			{
				model.RunPreSaveValidation();
				AssertNoErrors(model.ReasonInfo);
			}

			requireReasonWrapper.RequireReasonForCLR = true;
			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForJobClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requireReasonWrapper))
			{
				model.Reason = "";
				model.RunPreSaveValidation();
				AssertHasError(model.ReasonInfo, "Your organization requires you to enter a reason for clearing this record. Your reason will be recorded for audit purposes.");
			}

			model.Reason = "1";
			AssertHasError(model.ReasonInfo, "Clearing reason must be minimum two words and six characters long.");

			model.Reason = "1 2";
			AssertHasError(model.ReasonInfo, "Clearing reason must be minimum two words and six characters long.");

			model.Reason = "123456";
			AssertHasError(model.ReasonInfo, "Clearing reason must be minimum two words and six characters long.");

			model.Reason = "1234 |";
			AssertHasError(model.ReasonInfo, "Clearing reason must be minimum two words and six characters long.");
			AssertHasError(model.ReasonInfo, "Clearing reason should not contain characters of '|' or '='.");

			model.Reason = "12345 |";
			AssertHasError(model.ReasonInfo, "Clearing reason should not contain characters of '|' or '='.");

			model.Reason = "12345 =";
			AssertHasError(model.ReasonInfo, "Clearing reason should not contain characters of '|' or '='.");

			model.Reason = "12345 6";
			AssertNoErrors(model.ReasonInfo);
		}

		public void TestValidateCode()
		{
			var itemCollection = new RequireReasonForCLRItemCollection
			{
				new RequireReasonForCLRItem { Code = "CCC", Title = "Title1", IsMandatory = false }
			};

			var requireReasonWrapper = new RequireReasonForCLRWrapper(itemCollection)
			{
				RequireReasonForCLR = true
			};

			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForJobClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requireReasonWrapper))
			{
				var model = new DpsMarkJobClearConfirmationModel();
				model.Code = "CCC";
				AssertNoErrors(model.CodeInfo);

				model.Code = "AAA";
				AssertHasErrors("The Code is Invalid.", model.CodeInfo);
			}
		}

		public void TestCodeListIfRegistryOnButSecurityOff()
		{
			var rawValue = Env.Security.OrgDeniedPartyScreeningAllowOtherReasonForJobClear.IsAllowed;

			var itemCollection = new RequireReasonForCLRItemCollection
			{
				new RequireReasonForCLRItem { Code = "CCC", Title = "Title1", IsMandatory = true },
				new RequireReasonForCLRItem
				{
					Code = "DDD", Title = "Title2", ClearingReason = "", IsMandatory = false,
				}
			};

			var requireReasonWrapper = new RequireReasonForCLRWrapper(itemCollection)
			{
				RequireReasonForCLR = true
			};

			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForJobClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requireReasonWrapper))
			{
				try
				{
					Env.Security.OrgDeniedPartyScreeningAllowOtherReasonForJobClear.IsAllowed = false;

					var model = new DpsMarkJobClearConfirmationModel();
					AssertEquals("Code list should not contain 'OTH'.", 2, model.JobClearingReasonList.Count);
					AssertContainsExactElementsInAnyOrder(new[] { "CCC", "DDD" }, model.JobClearingReasonList.GetAllCodesZString());
				}
				finally
				{
					Env.Security.OrgDeniedPartyScreeningAllowOtherReasonForJobClear.IsAllowed = rawValue;
				}
			}
		}

		public void TestNoCodeInCodeListIfRegistryOffDespiteSecurityOn()
		{
			var rawValue = Env.Security.OrgDeniedPartyScreeningAllowOtherReasonForJobClear.IsAllowed;

			var itemCollection = new RequireReasonForCLRItemCollection
			{
				new RequireReasonForCLRItem { Code = "CCC", Title = "Title1", IsMandatory = true },
				new RequireReasonForCLRItem
				{
					Code = "DDD", Title = "Title2", ClearingReason = "", IsMandatory = false,
				}
			};

			var requireReasonWrapper = new RequireReasonForCLRWrapper(itemCollection)
			{
				RequireReasonForCLR = false
			};

			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForJobClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requireReasonWrapper))
			{
				try
				{
					Env.Security.OrgDeniedPartyScreeningAllowOtherReasonForJobClear.IsAllowed = true;

					var model = new DpsMarkJobClearConfirmationModel();
					AssertEquals("There should not be any Code in the Code list.", 0, model.JobClearingReasonList.Count);
				}
				finally
				{
					Env.Security.OrgDeniedPartyScreeningAllowOtherReasonForJobClear.IsAllowed = rawValue;
				}
			}
		}

		public void TestClearedReasonCorrespondsToCodeAndTitleWithRegistryOnAndSecurityRightOn()
		{
			var rawValue = Env.Security.OrgDeniedPartyScreeningAllowOtherReasonForJobClear.IsAllowed;

			var itemCollection = new RequireReasonForCLRItemCollection
			{
				new RequireReasonForCLRItem { Code = "CCC", Title = "Title1", IsMandatory = true },
				new RequireReasonForCLRItem
				{
					Code = "DDD", Title = "Title2", ClearingReason = "", IsMandatory = false,
				},
				new RequireReasonForCLRItem { Code = "EEE", Title = "Title3", ClearingReason = "Description 123" }
			};

			var requireReasonWrapper = new RequireReasonForCLRWrapper(itemCollection)
			{
				RequireReasonForCLR = true
			};

			using (OrganisationsDataRegistry.Instance.DeniedPartyScreeningRequireReasonForJobClearing.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, requireReasonWrapper))
			{
				var defaultClearedReason = StmEntityScreeningLogSchema.PJ_ClearedReason.SqlDbDefault.ToString();

				try
				{
					Env.Security.OrgDeniedPartyScreeningAllowOtherReasonForJobClear.IsAllowed = true;

					var model = new DpsMarkJobClearConfirmationModel();
					AssertEquals("Code list should contain 'OTH'.", 4, model.JobClearingReasonList.Count);
					AssertContainsExactElementsInAnyOrder(new[] { "CCC", "DDD", "EEE", "OTH" }, model.JobClearingReasonList.GetAllCodesZString());

					model.Code = "CCC";
					AssertCodeAndClearedReason("CCC", "Title1", false, "", model);

					model.Code = "DDD";
					AssertCodeAndClearedReason("DDD", "Title2", true, defaultClearedReason, model);

					model.Code = "EEE";
					AssertCodeAndClearedReason("EEE", "Title3", true, "Description 123", model);

					model.Code = "OTH";
					AssertCodeAndClearedReason("OTH", "Other", false, "", model);
				}
				finally
				{
					Env.Security.OrgDeniedPartyScreeningAllowOtherReasonForJobClear.IsAllowed = rawValue;
				}
			}
		}

		void AssertCodeAndClearedReason(string code, string title, bool reasonReadOnly, string reason, DpsMarkJobClearConfirmationModel model)
		{
			AssertEquals(code, model.Code);
			AssertEquals(title, model.JobClearingReasonList.GetDescriptionFromCode(model.Code));
			AssertEquals(reasonReadOnly, model.Reason_ReadOnly);
			AssertEquals(reason, model.Reason);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DpsMarkJobClearConfirmationModel()
			{
				RelatedJobsIDNotJCLOrCLR = Array.Empty<string>()
			};
		}
	}
}
