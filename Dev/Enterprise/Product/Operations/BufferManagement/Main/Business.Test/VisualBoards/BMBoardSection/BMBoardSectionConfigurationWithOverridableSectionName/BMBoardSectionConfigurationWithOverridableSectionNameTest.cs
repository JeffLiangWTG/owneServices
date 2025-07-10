using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(TestSectionConfiguration))]
	public class BMBoardSectionConfigurationWithOverridableSectionNameTest : NonPersistentBusinessObjectTestCase
	{
		TestSectionConfiguration config;

		public void TestSectionName()
		{
			config.SectionNameIsOverridden = true;

			config.SectionNameOverride = "I'll override you!";
			AssertEquals("SectionName should equal to SectionNameOverride when SectionNameIsOverridden is set to true", "I'll override you!", config.SectionName);

			config.SectionNameOverride = "    Trim me!    ";
			AssertEquals("SectionName should equal to trimmed SectionNameOverride when SectionNameIsOverridden is set to true and SectionNameOverride needs triming", "Trim me!", config.SectionName);

			config.SectionNameOverride = "  ";
			AssertEquals("SectionName should equal to DefaultSectionName when SectionNameIsOverridden is set to true, but SectionNameOverride is whitespace", "Default Section Name", config.SectionName);

			config.SectionNameOverride = ZString.Empty;
			AssertEquals("SectionName should equal to DefaultSectionName when SectionNameIsOverridden is set to true, but SectionNameOverride is empty", "Default Section Name", config.SectionName);

			config.SectionNameIsOverridden = false;

			config.SectionNameOverride = "I'll override you!";
			AssertEquals("SectionName should equal to DefaultSectionName when SectionNameIsOverridden is set to false", "Default Section Name", config.SectionName);

			config.SectionNameOverride = "  ";
			AssertEquals("SectionName should equal to DefaultSectionName when SectionNameIsOverridden is set to false", "Default Section Name", config.SectionName);

			config.SectionNameOverride = ZString.Empty;
			AssertEquals("SectionName should equal to DefaultSectionName when SectionNameIsOverridden is set to false", "Default Section Name", config.SectionName);
		}

		public void TestSectionNameOverride_Updates_SectionName()
		{
			//CASE A
			config.SectionNameIsOverridden = true;
			using (var handlers = new EventHandlerTestHelper(config))
			{
				config.SectionNameOverride = "I'll override you!";

				handlers.AssertValueChangedEventFired("Case A: Assigning to SectionNameOverride should fire the event related to SectionName when overridden", nameof(config.SectionNameInfo));
				AssertEquals("Case A: SectionName should equal to SectionNameOverride when SectionNameIsOverridden is set to true", "I'll override you!", config.SectionName);

				handlers.AssertValueChangedEventFired("Case A: Assigning to SectionNameOverride should fire the event related to SectionNameOverride when overridden", nameof(config.SectionNameOverrideInfo));
				handlers.AssertValueChangedEventNotFired("Case A: Assigning to SectionNameOverride should never fire the event related to SectionNameIsOverridden", nameof(config.SectionNameIsOverriddenInfo));
			}

			//CASE B
			config.SectionNameIsOverridden = false;
			using (var handlers = new EventHandlerTestHelper(config))
			{
				config.SectionNameOverride = "I'll override you!";

				handlers.AssertValueChangedEventNotFired("Case B: Assigning to SectionNameOverride should not fire the event related to SectionName when not overridden", nameof(config.SectionNameInfo));
				AssertEquals("Case B: SectionName should equal to DefaultSectionName when SectionNameIsOverridden is set to false", "Default Section Name", config.SectionName);

				handlers.AssertValueChangedEventFired("Case B: Assigning to SectionNameOverride should fire the event related to SectionNameOverride when not overridden", nameof(config.SectionNameOverrideInfo));
				handlers.AssertValueChangedEventNotFired("Case B: Assigning to SectionNameOverride should never fire the event related to SectionNameIsOverridden", nameof(config.SectionNameIsOverriddenInfo));
			}
		}

		public void TestSectionNameIsOverridden_DoesNotUpdate_SectionNameOverride()
		{
			config.SectionNameIsOverridden = true;
			using (var handlers = new EventHandlerTestHelper(config))
			{
				config.SectionNameIsOverridden = false;
				handlers.AssertValueChangedEventFired("Switching to non overriding should fire the event related to SectionName", nameof(config.SectionNameInfo));
				handlers.AssertValueChangedEventNotFired("Switching to non overriding should not fire the event related to SectionNameOverride", nameof(config.SectionNameOverrideInfo));
				handlers.AssertValueChangedEventFired("Switching to non overriding should fire the event related to SectionNameIsOverridden", nameof(config.SectionNameIsOverriddenInfo));
			}

			config.SectionNameIsOverridden = false;
			using (var handlers = new EventHandlerTestHelper(config))
			{
				config.SectionNameIsOverridden = true;
				handlers.AssertValueChangedEventFired("Switching to overriding should fire the event related to SectionName", nameof(config.SectionNameInfo));
				handlers.AssertValueChangedEventNotFired("Switching to overriding should not fire the event related to SectionNameOverride", nameof(config.SectionNameOverrideInfo));
				handlers.AssertValueChangedEventFired("Switching to overriding should fire the event related to SectionNameIsOverridden", nameof(config.SectionNameIsOverriddenInfo));
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			config = new TestSectionConfiguration(Factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TestSectionConfiguration(Factory);
		}

		protected override IEnumerable<string> XmlMemberNames
		{
			get
			{
				yield return "SectionNameOverride";
				yield return "SectionNameIsOverridden";
			}
		}

		class TestSectionConfiguration : BMBoardSectionConfigurationWithOverridableSectionName<TestSectionConfigurationValidation>
		{
			public TestSectionConfiguration(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public override ZString DefaultSectionName
			{
				get
				{
					return "   Default Section Name   ";
				}
			}

			public override void CopyConfigurationPropertiesToNewSection(IBMBoardSection section)
			{
			}

			public override TestSectionConfigurationValidation GetNewValidation()
			{
				return new TestSectionConfigurationValidation(this);
			}
		}

		class TestSectionConfigurationValidation : BMBoardSectionConfigurationWithOverridableSectionNameValidation
		{
			public TestSectionConfigurationValidation(TestSectionConfiguration sectionConfiguration)
				: base(sectionConfiguration)
			{
				this.sectionConfiguration = sectionConfiguration;
			}

			readonly TestSectionConfiguration sectionConfiguration;

			#region Implementation

			public override void ValidateAll()
			{
			}

			public override Type AutoValidationType
			{
				get { return typeof(TestSectionConfigurationValidation); }
			}

			protected override IBoardSectionNameOverridable SectionConfiguration => sectionConfiguration;

			#endregion
		}

		#endregion
	}
}
