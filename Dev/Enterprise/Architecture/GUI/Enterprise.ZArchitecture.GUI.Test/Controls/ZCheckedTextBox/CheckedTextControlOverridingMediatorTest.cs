using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI
{
	[TestedType(typeof(CheckedTextControlOverridingMediator))]
	sealed class CheckedTextControlOverridingMediatorTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDefaultValues()
		{
			AssertEquals("Default value for MaxLength should be...", 0, mediator.MaxLength);
			AssertEquals("Default value for TextOverride should be...", ZString.Empty, mediator.TextOverride);
			AssertEquals("Default value for PlaceholderText should be...", ZString.Empty, mediator.PlaceholderText);
			AssertEquals("Default value for TextIsOverridden should be...", false, mediator.TextIsOverridden);

			AssertEquals("Default value for ControlIsChecked should be...", false, mediator.ControlIsChecked);
			AssertEquals("Default value for ControlText should be...", ZString.Empty, mediator.ControlText);

			AssertEquals("Default value for UpdateTextOnChecked should be...", UpdateTextOnCheckedMode.ClearWhenPlaceholderTextIsTruncated, mediator.UpdateTextOnChecked);
		}

		#region Interaction between Mediator and Control

		public void TestBindingBetweenMediatorAndControl()
		{
			var mediatorControlTextHasChanged = false;
			var mediatorControlIsCheckedHasChanged = false;
			var mediatorMaxLengthHasChanged = false;
			var mediatorTextOverrideHasChanged = false;
			var mediatorPlaceholderTextHasChanged = false;
			var mediatorTextIsOverriddenHasChanged = false;
			mediator.ControlTextInfo.ValueChanged += delegate
			{ mediatorControlTextHasChanged = true; };
			mediator.ControlIsCheckedInfo.ValueChanged += delegate
			{ mediatorControlIsCheckedHasChanged = true; };
			mediator.MaxLengthInfo.ValueChanged += delegate
			{ mediatorMaxLengthHasChanged = true; };
			mediator.TextOverrideInfo.ValueChanged += delegate
			{ mediatorTextOverrideHasChanged = true; };
			mediator.PlaceholderTextInfo.ValueChanged += delegate
			{ mediatorPlaceholderTextHasChanged = true; };
			mediator.TextIsOverriddenInfo.ValueChanged += delegate
			{ mediatorTextIsOverriddenHasChanged = true; };

			control.Text = "Some Text";
			AssertEquals("Text should update Mediator.ControlText", true, mediatorControlTextHasChanged);
			AssertEquals("Mediator.ControlText should equal to Text", "Some Text", mediator.ControlText);

			control.Checked = true;
			AssertEquals("Checked should update Mediator.ControlIsChecked", true, mediatorControlIsCheckedHasChanged);
			AssertEquals("Mediator.ControlIsChecked should equal to Checked", true, mediator.ControlIsChecked);

			control.MaxLength = 100;
			AssertEquals("MaxLength should update Mediator.MaxLength", true, mediatorMaxLengthHasChanged);
			AssertEquals("Mediator.MaxLength should equal to MaxLength", 100, mediator.MaxLength);

			control.TextOverride = "I'll override you!";
			AssertEquals("TextOverride should update Mediator.TextOverride", true, mediatorTextOverrideHasChanged);
			AssertEquals("Mediator.TextOverride should equal to TextOverride", "I'll override you!", mediator.TextOverride);

			control.PlaceholderText = "Placeholder Text";
			AssertEquals("PlaceholderText should update Mediator.PlaceholderText", true, mediatorPlaceholderTextHasChanged);
			AssertEquals("Mediator.PlaceholderText should equal to PlaceholderText", "Placeholder Text", mediator.PlaceholderText);

			control.TextIsOverridden = true;
			AssertEquals("TextIsOverridden should update Mediator.TextIsOverridden", true, mediatorTextIsOverriddenHasChanged);
			AssertEquals("Mediator.TextIsOverridden should equal to TextIsOverridden", true, mediator.TextIsOverridden);

			var textHasChanged = false;
			var checkedHasChanged = false;
			var maxLengthHasChanged = false;
			var textOverrideHasChanged = false;
			var placeholderTextHasChanged = false;
			var textIsOverriddenHasChanged = false;
			control.TextChanged += delegate
			{ textHasChanged = true; };
			control.CheckedChanged += delegate
			{ checkedHasChanged = true; };
			control.MaxLengthChanged += delegate
			{ maxLengthHasChanged = true; };
			control.TextOverrideChanged += delegate
			{ textOverrideHasChanged = true; };
			control.PlaceholderTextChanged += delegate
			{ placeholderTextHasChanged = true; };
			control.TextIsOverriddenChanged += delegate
			{ textIsOverriddenHasChanged = true; };

			mediator.ControlText = "Another Text";
			AssertEquals("Mediator.ControlText should update Text", true, textHasChanged);
			AssertEquals("Text should equal to Mediator.ControlText", "Another Text", control.Text);

			mediator.ControlIsChecked = false;
			AssertEquals("Mediator.ControlIsChecked should update Checked", true, checkedHasChanged);
			AssertEquals("Checked should equal to Mediator.ControlIsChecked", false, control.Checked);

			mediator.MaxLength = 200;
			AssertEquals("Mediator.MaxLength should update MaxLength", true, maxLengthHasChanged);
			AssertEquals("MaxLength should equal to Mediator.MaxLength", 200, control.MaxLength);

			mediator.TextOverride = "I'll override the Universe!";
			AssertEquals("Mediator.TextOverride should update TextOverride", true, textOverrideHasChanged);
			AssertEquals("TextOverride should equal to Mediator.TextOverride", "I'll override the Universe!", control.TextOverride);

			mediator.PlaceholderText = "Another Placeholder Text";
			AssertEquals("Mediator.PlaceholderText should update PlaceholderText", true, placeholderTextHasChanged);
			AssertEquals("PlaceholderText should equal to Mediator.PlaceholderText", "Another Placeholder Text", control.PlaceholderText);

			mediator.TextIsOverridden = false;
			AssertEquals("Mediator.TextIsOverridden should update TextIsOverridden", true, textIsOverriddenHasChanged);
			AssertEquals("TextIsOverridden should equal to Mediator.TextIsOverridden", false, control.TextIsOverridden);
		}

		public void TestBindingUpdatesTextOnChecked()
		{
			control.UpdateTextOnChecked = UpdateTextOnCheckedMode.AlwaysClear;
			AssertEquals("Mediator.UpdateTextOnChecked should equal to UpdateTextOnChecked", UpdateTextOnCheckedMode.AlwaysClear, mediator.UpdateTextOnChecked);

			control.UpdateTextOnChecked = UpdateTextOnCheckedMode.ClearWhenPlaceholderTextIsTruncated;
			AssertEquals("Mediator.UpdateTextOnChecked should equal to UpdateTextOnChecked", UpdateTextOnCheckedMode.ClearWhenPlaceholderTextIsTruncated, mediator.UpdateTextOnChecked);
		}

		#endregion

		#region Business Layer

		public void TestMaxLength_DefinesAndUpdates_ControlText_WhenNotOverridden()
		{
			mediator.TextIsOverridden = false;
			mediator.PlaceholderText = "Placeholder Text";

			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.MaxLength = 10;
				handlers.AssertValueChangedEventFired("MaxLength should update ControlText when not overridden", nameof(mediator.ControlTextInfo));
				AssertEquals("ControlText should be set to truncated PlaceholderText when not overridden", "Placeho...", mediator.ControlText);
			}
		}

		public void TestMaxLength_DefinesAndUpdates_ControlText_WhenOverridden()
		{
			mediator.TextIsOverridden = true;
			mediator.TextOverride = "I'll override you!";
			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.MaxLength = 10;
				handlers.AssertValueChangedEventFired("MaxLength should update ControlText when overridden", nameof(mediator.ControlTextInfo));
				AssertEquals("ControlText should be truncated when overridden", "I'll overr", mediator.ControlText);
			}
		}

		public void TestMaxLength_DoesNotUpdateOtherProperties()
		{
			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.MaxLength = 10;
				handlers.AssertValueChangedEventNotFired("MaxLength should NOT update TextOverride", nameof(mediator.TextOverrideInfo));
				handlers.AssertValueChangedEventNotFired("MaxLength should NOT update PlaceholderText", nameof(mediator.PlaceholderTextInfo));
				handlers.AssertValueChangedEventNotFired("MaxLength should NOT update TextIsOverridden", nameof(mediator.TextIsOverriddenInfo));
				handlers.AssertValueChangedEventNotFired("MaxLength should NOT update ControlIsChecked", nameof(mediator.ControlIsCheckedInfo));
			}
		}

		public void TestUpdatesTextOnChecked()
		{
			mediator.PlaceholderText = "Placeholder Text";
			mediator.UpdateTextOnChecked = UpdateTextOnCheckedMode.ClearWhenPlaceholderTextIsTruncated;

			mediator.MaxLength = 0;
			AssertEquals("ShouldClearTextOnOChecked should be false when length is not limited", false, mediator.ShouldClearTextOnOChecked);

			mediator.MaxLength = 200;
			AssertEquals("ShouldClearTextOnOChecked should be false when PlaceholderText is short enough", false, mediator.ShouldClearTextOnOChecked);

			mediator.MaxLength = 10;
			AssertEquals("ShouldClearTextOnOChecked should be true when PlaceholderText is too long", true, mediator.ShouldClearTextOnOChecked);

			mediator.UpdateTextOnChecked = UpdateTextOnCheckedMode.AlwaysClear;
			mediator.MaxLength = 0;
			AssertEquals("ShouldClearTextOnOChecked should be true when UpdateTextOnChecked is set to AlwaysClear", true, mediator.ShouldClearTextOnOChecked);

			mediator.UpdateTextOnChecked = UpdateTextOnCheckedMode.Never;
			mediator.MaxLength = 0;
			AssertEquals("ShouldClearTextOnOChecked should be false when UpdateTextOnChecked is set to Never", false, mediator.ShouldClearTextOnOChecked);

			mediator.MaxLength = 200;
			AssertEquals("ShouldClearTextOnOChecked should be false when UpdateTextOnChecked is set to Never", false, mediator.ShouldClearTextOnOChecked);

			mediator.MaxLength = 10;
			AssertEquals("ShouldClearTextOnOChecked should be false when UpdateTextOnChecked is set to Never", false, mediator.ShouldClearTextOnOChecked);
		}

		public void TestTruncatedPlaceholderText()
		{
			mediator.PlaceholderText = "Placeholder Text";
			mediator.MaxLength = 0;
			AssertEquals("TruncatedPlaceholderText should equal to PlaceholderText when length is not limited", "Placeholder Text", mediator.TruncatedPlaceholderText);

			mediator.MaxLength = 200;
			AssertEquals("TruncatedPlaceholderText should equal to PlaceholderText when PlaceholderText is short enough", "Placeholder Text", mediator.TruncatedPlaceholderText);

			mediator.MaxLength = 10;
			AssertEquals("TruncatedPlaceholderText should equal to truncated PlaceholderText when PlaceholderText is too long", "Placeho...", mediator.TruncatedPlaceholderText);

			mediator.MaxLength = 3;
			AssertEquals("TruncatedPlaceholderText should be empty when truncated PlaceholderText does not make sense", ZString.Empty, mediator.TruncatedPlaceholderText);
		}

		public void TestTextOverride_IsTruncatedAutomatically()
		{
			mediator.TextIsOverridden = true;

			mediator.MaxLength = 0;
			mediator.TextOverride = "I'll override you!";
			AssertEquals("TextOverride should not be auto truncated when max length is unlimited", "I'll override you!", mediator.TextOverride);

			mediator.MaxLength = 200;
			mediator.TextOverride = "I'll override you!";
			AssertEquals("TextOverride should not be auto truncated when value is short enough", "I'll override you!", mediator.TextOverride);

			mediator.MaxLength = 7;
			mediator.TextOverride = "Another try!";
			AssertEquals("TextOverride should be auto truncated when value is too long", "Another", mediator.TextOverride);
		}

		public void TestTextOverride_DefinesAndUpdates_ControlText()
		{
			mediator.TextIsOverridden = true;
			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.TextOverride = "I'll override you!";
				AssertEquals("TextOverride should update ControlText when text is overridden", "I'll override you!", mediator.ControlText);
				handlers.AssertValueChangedEventFired("TextOverride should update ControlText and fire the event when text is overridden", nameof(mediator.ControlTextInfo));

				handlers.AssertValueChangedEventNotFired("TextOverride should NOT update TextIsOverridden", nameof(mediator.TextIsOverriddenInfo));
			}

			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.TextOverride = ZString.Empty;
				AssertEquals("TextOverride should update ControlText when text is overridden - even for empty strings", ZString.Empty, mediator.ControlText);
				handlers.AssertValueChangedEventFired("TextOverride should update ControlText and fire the event when text is overridden - even for empty strings", nameof(mediator.ControlTextInfo));

				handlers.AssertValueChangedEventNotFired("TextOverride should NOT update TextIsOverridden", nameof(mediator.TextIsOverriddenInfo));
			}

			mediator.TextIsOverridden = false;
			mediator.PlaceholderText = "Placeholder Text";
			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.TextOverride = "I'll override you!";
				handlers.AssertValueChangedEventFired("TextOverride should NOT update ControlText when text is not overridden", nameof(mediator.ControlTextInfo));
				AssertEquals("TextOverride should still store the assigned value when text is overridden", "I'll override you!", mediator.TextOverride);
				AssertEquals("ControlText should be set to (truncated) PlaceholderText when not overridden", "Placeholder Text", mediator.ControlText);

				handlers.AssertValueChangedEventNotFired("TextOverride should NOT update TextIsOverridden", nameof(mediator.TextIsOverriddenInfo));
			}
		}

		public void TestTextOverride_DoesNotUpdate_ControlIsChecked()
		{
			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.TextOverride = "I'll override you!";
				handlers.AssertValueChangedEventNotFired("TextOverride should NOT update ControlIsChecked", nameof(mediator.ControlIsCheckedInfo));
			}

			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.TextOverride = ZString.Empty;
				handlers.AssertValueChangedEventNotFired("TextOverride should NOT update ControlIsChecked", nameof(mediator.ControlIsCheckedInfo));
			}
		}

		public void TestPlaceholderText_DefinesAndUpdates_ControlText_WhenNotOverridden()
		{
			mediator.TextIsOverridden = false;
			AssertEquals("Precondition: not overridden", false, mediator.TextIsOverridden);

			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.PlaceholderText = "Placeholder Text";
				handlers.AssertValueChangedEventFired("PlaceholderText should update ControlText when not overridden", nameof(mediator.ControlTextInfo));
				AssertEquals("ControlText should be set to (truncated) PlaceholderText when not overridden", "Placeholder Text", mediator.ControlText);
			}
		}

		public void TestPlaceholderText_DoesNotUpdate_ControlText_And_OverriddenText_WhenOverridden()
		{
			mediator.TextIsOverridden = true;
			AssertEquals("Precondition: overridden", true, mediator.TextIsOverridden);

			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.PlaceholderText = "Placeholder Text";
				handlers.AssertValueChangedEventNotFired("PlaceholderText should NOT update ControlText when overridden", nameof(mediator.ControlTextInfo));
			}
		}

		public void TestTextIsOverridden_CanBeSetDirectly()
		{
			mediator.TextIsOverridden = true;
			AssertEquals("Precondition: overridden", true, mediator.TextIsOverridden);

			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.TextIsOverridden = false;
				AssertEquals("Set value should be applied", false, mediator.TextIsOverridden);
				handlers.AssertValueChangedEventFired("TextIsOverridden should be updated by setting its value directly", nameof(mediator.TextIsOverriddenInfo));
			}
		}

		public void TestTextIsOverridden_DefinesAndUpdates_ControlIsChecked()
		{
			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.TextIsOverridden = true;
				handlers.AssertValueChangedEventFired("TextIsOverridden should update ControlIsChecked", nameof(mediator.ControlIsCheckedInfo));
				AssertEquals("ControlIsChecked should be set to TextIsOverridden", true, mediator.ControlIsChecked);
			}

			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.TextIsOverridden = false;
				handlers.AssertValueChangedEventFired("TextIsOverridden should update ControlIsChecked", nameof(mediator.ControlIsCheckedInfo));
				AssertEquals("ControlIsChecked should be set to TextIsOverridden", false, mediator.ControlIsChecked);
			}
		}

		#endregion

		#region Control Layer

		public void TestControlText_DefinesAndUpdates_TextOverride()
		{
			mediator.TextOverride = "I'll override you!";

			mediator.ControlIsChecked = true;
			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.ControlText = "Control Text";
				handlers.AssertValueChangedEventFired("ControlText should update TextOverride when control is checked", nameof(mediator.TextOverrideInfo));
				AssertEquals("TextOverride should be set to ControlText when control is checked", "Control Text", mediator.TextOverride);
			}

			mediator.ControlIsChecked = false;
			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.ControlText = "Control Text";
				handlers.AssertValueChangedEventNotFired("ControlText should NOT update TextOverride when control is not checked", nameof(mediator.TextOverrideInfo));
			}
		}

		public void TestControlText_DoesNotUpdate_TextIsOverridden()
		{
			mediator.TextOverride = "I'll override you!";

			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.ControlText = "Control Text";
				handlers.AssertValueChangedEventNotFired("ControlText should NOT update TextIsOverridden", nameof(mediator.TextIsOverriddenInfo));
			}

			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.ControlText = ZString.Empty;
				handlers.AssertValueChangedEventNotFired("ControlText should NOT update TextIsOverridden", nameof(mediator.TextIsOverriddenInfo));
			}
		}

		public void TestControlIsChecked_DefinesAndUpdates_TextIsOverridden()
		{
			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.ControlIsChecked = true;
				handlers.AssertValueChangedEventFired("ControlIsChecked should update TextIsOverridden", nameof(mediator.TextIsOverriddenInfo));
				AssertEquals("TextIsOverridden should be set to ControlIsChecked", true, mediator.TextIsOverridden);
			}

			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.ControlIsChecked = false;

				handlers.AssertValueChangedEventFired("ControlIsChecked should update TextIsOverridden", nameof(mediator.TextIsOverriddenInfo));
				AssertEquals("TextIsOverridden should be set to ControlIsChecked", false, mediator.TextIsOverridden);
			}
		}

		public void TestControlIsChecked_DefinesAndUpdates_ControlText_WhenChecking()
		{
			mediator.PlaceholderText = "Placeholder Text";
			mediator.UpdateTextOnChecked = UpdateTextOnCheckedMode.ClearWhenPlaceholderTextIsTruncated;

			mediator.ControlIsChecked = false;
			mediator.MaxLength = 0;
			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.ControlIsChecked = true;
				AssertEquals("ShouldClearTextOnOChecked should be false when length is not limited", false, mediator.ShouldClearTextOnOChecked);

				handlers.AssertValueChangedEventFired("ControlText should be updated when switсhing to checked", nameof(mediator.ControlTextInfo));
				AssertEquals("ControlText should be defaulted with PlaceholderText when switсhing to checked and PlaceholderText is unlimited", "Placeholder Text", mediator.ControlText);
			}

			mediator.ControlIsChecked = false;
			mediator.MaxLength = 200;
			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.ControlIsChecked = true;
				AssertEquals("ShouldClearTextOnOChecked should be false when PlaceholderText is short enough", false, mediator.ShouldClearTextOnOChecked);

				handlers.AssertValueChangedEventFired("ControlText should be updated when switсhing to checked", nameof(mediator.ControlTextInfo));
				AssertEquals("ControlText should be defaulted with PlaceholderText when switсhing to checked and PlaceholderText is short enough", "Placeholder Text", mediator.ControlText);
			}

			mediator.ControlIsChecked = false;
			mediator.MaxLength = "Placeholder Text".Length;
			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.ControlIsChecked = true;
				AssertEquals("ShouldClearTextOnOChecked should be false when PlaceholderText is short enough", false, mediator.ShouldClearTextOnOChecked);

				handlers.AssertValueChangedEventFired("ControlText should be updated when switсhing to checked", nameof(mediator.ControlTextInfo));
				AssertEquals("ControlText should be defaulted with PlaceholderText when switсhing to checked and PlaceholderText is short enough", "Placeholder Text", mediator.ControlText);
			}

			mediator.ControlIsChecked = false;
			mediator.MaxLength = 10;
			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.ControlIsChecked = true;
				AssertEquals("ShouldClearTextOnOChecked should be true when PlaceholderText is too long", true, mediator.ShouldClearTextOnOChecked);

				handlers.AssertValueChangedEventFired("ControlText should be updated when switсhing to checked", nameof(mediator.ControlTextInfo));
				AssertEquals("ControlText should be cleared when switсhing to checked and PlaceholderText is too long", ZString.Empty, mediator.ControlText);
			}

			mediator.UpdateTextOnChecked = UpdateTextOnCheckedMode.AlwaysClear;

			mediator.MaxLength = 0;
			mediator.ControlIsChecked = false;
			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.ControlIsChecked = true;
				AssertEquals("ShouldClearTextOnOChecked should be true when UpdateTextOnChecked is set to AlwaysClear", true, mediator.ShouldClearTextOnOChecked);

				handlers.AssertValueChangedEventFired("ControlText should be updated when switсhing to checked", nameof(mediator.ControlTextInfo));
				AssertEquals("ControlText should be cleared when UpdateTextOnChecked is set to AlwaysClear", ZString.Empty, mediator.ControlText);
			}

			mediator.UpdateTextOnChecked = UpdateTextOnCheckedMode.Never;

			mediator.MaxLength = 0;
			mediator.ControlIsChecked = false;
			AssertEquals("Precondition: ControlText should equal to PlaceholderText", "Placeholder Text", mediator.ControlText);
			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.ControlIsChecked = true;
				AssertEquals("ShouldClearTextOnOChecked should be false when UpdateTextOnChecked is set to Never", false, mediator.ShouldClearTextOnOChecked);

				handlers.AssertValueChangedEventNotFired("ControlText should NOT be updated when switсhing to checked and UpdateTextOnChecked is set to Never", nameof(mediator.ControlTextInfo));
				AssertEquals("ControlText should keep the previous value when UpdateTextOnChecked is set to Never", "Placeholder Text", mediator.ControlText);
			}

			mediator.MaxLength = 200;
			mediator.ControlIsChecked = false;
			AssertEquals("Precondition: ControlText should equal to PlaceholderText", "Placeholder Text", mediator.ControlText);
			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.ControlIsChecked = true;
				AssertEquals("ShouldClearTextOnOChecked should be false when UpdateTextOnChecked is set to Never", false, mediator.ShouldClearTextOnOChecked);

				handlers.AssertValueChangedEventNotFired("ControlText should NOT be updated when switсhing to checked and UpdateTextOnChecked is set to Never", nameof(mediator.ControlTextInfo));
				AssertEquals("ControlText should keep the previous value when UpdateTextOnChecked is set to Never", "Placeholder Text", mediator.ControlText);
			}

			mediator.MaxLength = 10;
			mediator.ControlIsChecked = false;
			AssertEquals("Precondition: ControlText should equal to truncated PlaceholderText", "Placeho...", mediator.ControlText);
			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.ControlIsChecked = true;
				AssertEquals("ShouldClearTextOnOChecked should be false when UpdateTextOnChecked is set to Never", false, mediator.ShouldClearTextOnOChecked);

				handlers.AssertValueChangedEventNotFired("ControlText should NOT be updated when switсhing to checked and UpdateTextOnChecked is set to Never", nameof(mediator.ControlTextInfo));
				AssertEquals("ControlText should keep the previous value when UpdateTextOnChecked is set to Never", "Placeho...", mediator.ControlText);
			}
		}

		public void TestControlIsChecked_DefinesAndUpdates_ControlText_WhenUnchecking()
		{
			mediator.PlaceholderText = "Placeholder Text";
			mediator.ControlText = "Control Text";

			mediator.ControlIsChecked = true;
			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.ControlIsChecked = false;
				handlers.AssertValueChangedEventFired("ControlText should be updated when switсhing to unchecked", nameof(mediator.ControlTextInfo));
				AssertEquals("ControlText should be set to (truncated) PlaceholderText when switсhing to unchecked", "Placeholder Text", mediator.ControlText);
			}
		}

		public void TestControlIsChecked_DefinesAndUpdates_TextOverride_WhenCheckingAndUnchecking()
		{
			mediator.PlaceholderText = "Placeholder Text";
			mediator.ControlIsChecked = true;
			mediator.ControlText = "Control Text";
			AssertEquals("Precondition: TextOverride should be set to ControlText", "Control Text", mediator.TextOverride);

			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.ControlIsChecked = false;

				handlers.AssertValueChangedEventFired("ControlText should be updated when switсhing to unchecked", nameof(mediator.ControlTextInfo));
				AssertEquals("ControlText should be set to PlaceholderText when switсhing to unchecked (and PlaceholderText is unlimited)", "Placeholder Text", mediator.ControlText);

				handlers.AssertValueChangedEventFired("TextOverride should be updated when switсhing to checked", nameof(mediator.TextOverrideInfo));
				AssertEquals("TextOverride should be cleared when switсhing to unchecked", ZString.Empty, mediator.TextOverride);
			}

			using (var handlers = new EventHandlerTestHelper(mediator))
			{
				mediator.ControlIsChecked = true;

				handlers.AssertValueChangedEventFired("ControlText should be updated when switсhing to checked", nameof(mediator.ControlTextInfo));
				AssertEquals("ControlText should be defaulted with PlaceholderText when switсhing to checked (and PlaceholderText is unlimited)", "Placeholder Text", mediator.ControlText);

				handlers.AssertValueChangedEventFired("TextOverride should be updated when switсhing to checked", nameof(mediator.TextOverrideInfo));
				AssertEquals("TextOverride should be set to PlaceholderText when switсhing to checked (and PlaceholderText is unlimited)", "Placeholder Text", mediator.TextOverride);
			}
		}

		#endregion

		#region Implementation

		IOverridableTextBox control;
		CheckedTextControlOverridingMediator mediator;

		protected override void SetUp()
		{
			base.SetUp();
			var box = new ZOverridableTextBox();
			control = box;
			mediator = box.Mediator;
		}

		protected override void TearDown()
		{
			control.Dispose();
			base.TearDown();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new CheckedTextControlOverridingMediator(null);
		}

		#endregion
	}
}
