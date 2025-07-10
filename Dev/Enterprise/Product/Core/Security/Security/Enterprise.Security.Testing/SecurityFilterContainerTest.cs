using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Security.Testing
{
	[TestedType(typeof(SecurityFilterContainer))]
	sealed class SecurityFilterContainerTest : NonPersistentBusinessObjectTestCase
	{
		SecurityFilterContainer filterContainer;

		SecurityFilterContainer FilterContainer
		{
			get
			{
				if (filterContainer == null)
				{
					filterContainer = new SecurityFilterContainer();
				}
				return filterContainer;
			}
		}

		public void TestLookupKeyValue()
		{
			CheckpointLookupKey lookupKey = new CheckpointLookupKey("a", Guid.NewGuid());
			FilterContainer.LookupKey = lookupKey;
			AssertEquals("LookupKey", lookupKey, FilterContainer.LookupKey);

			FilterContainer.LookupKey = CheckpointLookupKey.Empty;
			AssertEquals("LookupKey", CheckpointLookupKey.Empty, FilterContainer.LookupKey);
		}

		public void TestSchema()
		{
			AssertNotNull("The " + SecurityFilterContainer.Schema.LookupKeyValidationProxy + " property should exist.", FilterContainer.ZPropertyInfoHash[SecurityFilterContainer.Schema.LookupKeyValidationProxy]);
		}

		[ExpectNoExceptions]
		public void TestSettingLookupKeyCallsOnElementChanged()
		{
			CheckpointLookupKey lookupKey = new CheckpointLookupKey("a", Guid.NewGuid());
			var mockFilterContainer1 = new Mock<SecurityFilterContainer>();
			var mockFilterContainer2 = new Mock<SecurityFilterContainer>();
			var mockFilterContainer3 = new Mock<SecurityFilterContainer>();

			mockFilterContainer1.Protected().Setup("OnElementChanged");
			mockFilterContainer1.Object.LookupKey = lookupKey;
			mockFilterContainer1.Protected().Verify("OnElementChanged", Times.Exactly(1));
			mockFilterContainer1.VerifyAll();

			mockFilterContainer2.Protected().Setup("OnElementChanged");
			mockFilterContainer2.Object.LookupKey = lookupKey;
			mockFilterContainer2.Object.LookupKey = lookupKey;
			mockFilterContainer2.Protected().Verify("OnElementChanged", Times.Exactly(1));
			mockFilterContainer2.VerifyAll();

			mockFilterContainer3.Protected().Setup("OnElementChanged");
			mockFilterContainer3.Object.LookupKey = lookupKey;
			mockFilterContainer3.Object.LookupKey = CheckpointLookupKey.Empty;
			mockFilterContainer3.Protected().Verify("OnElementChanged", Times.Exactly(2));
			mockFilterContainer3.VerifyAll();
		}

		public void TestValidation()
		{
			FilterContainer.LookupKey = new CheckpointLookupKey("x");
			AssertHasError(FilterContainer.LookupKeyValidationProxyInfo, "Please select a valid Security Right.");

			var query = new ZQuery();
			query.AddToFilter(StmMenuItemSchema.SU_MenuType, "DOC");
			query.AddToFilter(StmMenuItemSchema.SU_IsSystemDefined, true);
			query.AddToFilter(StmMenuItemSchema.SU_BusinessContext, "RepSystemReports");
			var menuItem = Factory.LoadTop1<IStmMenuItem>(query);
			FilterContainer.LookupKey = new CheckpointLookupKey("Report", menuItem.PK.ToGuid());
			AssertNoErrors(FilterContainer.LookupKeyValidationProxyInfo);

			FilterContainer.LookupKey = new CheckpointLookupKey("");
			AssertNoErrors(FilterContainer.LookupKeyValidationProxyInfo);

			FilterContainer.LookupKey = Env.Security.ACAHouseDelete.LookupKey;
			AssertNoErrors(FilterContainer.LookupKeyValidationProxyInfo);
		}

		public void TestValidationSuspended()
		{
			using (FilterContainer.GetValidationSuspender())
			{
				FilterContainer.LookupKeyValidationProxyInfo.AddError("Test Error Message");
				FilterContainer.ValidateLookupKeyValidationProxy();
				Assert(!filterContainer.IsValidationSuspended);
				AssertEquals("Error - LookupKeyValidationProxy: Test Error Message", FilterContainer.Notifications.First().Message);

				FilterContainer.ValidateLookupKeyValidationProxy();
				Assert(!filterContainer.IsValidationSuspended);
				Assert(!FilterContainer.Notifications.HasMessageErrors());
			}
		}
	}
}
