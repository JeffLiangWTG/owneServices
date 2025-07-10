
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Media;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using NUnit.Framework.TestHelper;

namespace CargoWise.Main.Navigation.WPF.Test
{
	sealed class BindingErrorTraceListener : TraceListener
	{
		public BindingErrorTraceListener()
		{
			ForeEnableBypassRegistryCheck();
		}
		public List<string> Errors { get; } = new List<string>();
		readonly StringBuilder buffer = new StringBuilder();

		public override void Write(string message)
		{
			_ = buffer.Append(message);
		}

		[DebuggerStepThrough]
		public override void WriteLine(string message)
		{
			_ = buffer.Append(message);

			TraceCaught?.Invoke(buffer.ToString());
			Errors.Add(buffer.ToString());
			_ = buffer.Clear();
		}

		public event Action<string> TraceCaught;

		void ForeEnableBypassRegistryCheck()
		{
			var traceType = Type.GetType("MS.Internal.AvTrace, WindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", throwOnError: true);
			var isEnabledInRegistryField = traceType.GetField("_enabledInRegistry", BindingFlags.NonPublic | BindingFlags.Static);
			var valueAsObject = isEnabledInRegistryField.GetValue(null);
			var value = false;

			if (valueAsObject != null)
			{
				var valueAsNullable = (bool?)valueAsObject;
				value = valueAsNullable.HasValue && valueAsNullable.Value;
			}

			if (!value)
			{
				isEnabledInRegistryField.SetValue(null, true);

				var traceDataType = Type.GetType("MS.Internal.TraceData, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", throwOnError: true);
				var traceField = traceDataType.GetField("_avTrace", BindingFlags.Static | BindingFlags.NonPublic);
				var trace = traceField.GetValue(null);
				var initializeMethod = traceType.GetMethod("Initialize", BindingFlags.NonPublic | BindingFlags.Instance);
				initializeMethod.Invoke(trace, Array.Empty<object>());
			}
		}
	}

	[CLSCompliant(false)]
	[TestsSubclassesOf(typeof(ContentControl), typeof(TestExcludeWPFControlFromBasherAttribute), new String[] { "OxyPlot.Wpf.TrackerControl, OxyPlot.Wpf" }, ExcludeClientDlls = false)]
	public abstract class WPFControlBasherTest : TestCaseWithFactory
	{
		protected Type ControlToBashType => TestedTypeHelper.GetTestedType(GetType());

		public Control GetControlToBash()
		{
			var result = GetControlToBashCore();
			if (!ControlToBashType.IsInstanceOfType(result))
			{
				throw new InvalidOperationException("ControlToBashType must be consistent with the return value of GetControlToBash()");
			}
			return result;
		}

		#region Test no binding errors

		[GuiTest]
		[RequiresSTA]
		public void TestNoBindingErrors()
		{
			Assert(true);

			var window = new Window();

			var testForm = GetControlToBash();
			var traceListener = new BindingErrorTraceListener();
			Trace.AutoFlush = true;
			traceListener.Filter = new EventTypeFilter(SourceLevels.Warning);
			traceListener.TraceCaught += message => Assert(message, false);
			try
			{
				PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Error;
				PresentationTraceSources.DataBindingSource.Listeners.Add(traceListener);
				PresentationTraceSources.SetTraceLevel(testForm, PresentationTraceLevel.High);

				if (testForm is Window)
				{
					window = (Window)testForm;
				}
				else
				{
					window.Content = testForm;
				}

				window.Show();
				window.Close();
				Assert("should be no trace errors", traceListener.Errors.Count == 0);
				ReportExceptions();
			}
			finally
			{
				PresentationTraceSources.DataBindingSource.Listeners.Remove(traceListener);
			}
		}

		#endregion

		#region Test form is fully translatable

		[GuiTest]
		[RequiresSTA]
		public void TestFormIsFullyTranslatable()
		{
			Assert(true);

			var window = new Window();

			string report = WPFControlTranslatableBasherTest.BashTranslatable(
				this,
				() =>
				{
					var testForm = GetControlToBash();
					if (testForm is Window)
					{
						window = (Window)testForm;
					}
					else
					{
						window.Content = testForm;
					}

					window.Show();
					return testForm;
				});

			window.Close();

			if (!string.IsNullOrEmpty(report))
			{
				AddError(report);
			}

			ReportExceptions();
		}

		#endregion

		#region TestMultilingualTextLabelControlIsUsedForAllWPFControls

		List<string> nonWrappedComponents;

		[GuiTest]
		[RequiresSTA]
		public void TestLabelTextBlockIsWrappedWithMultilingualTextLabel()
		{
			nonWrappedComponents = new List<string>();
			var testForm = GetControlToBash();
			//remove the process decider when ribbon fully implements multilingual support
			//also remove the reference to System.Windows.Controls.Ribbon
			RetrieveAndProcessEachControl(testForm, XamlControlRetriever_OnControlProcessing, LabelTextBlockIsWrappedWithMultilingualTextLabel_ProcessElementDecider);

			AssertEquals(
					string.Format(
						"WPF Control {1} has non-wrapped or non-translatable controls:{0}{0}{2}",
					/*0*/ Environment.NewLine,
					/*1*/ testForm.GetType().FullName,
					/*2*/ string.Join(System.Environment.NewLine, nonWrappedComponents.ToArray())), 0, nonWrappedComponents.Count);
		}

		protected virtual bool LabelTextBlockIsWrappedWithMultilingualTextLabel_ProcessElementDecider(DependencyObject control)
		{
			return !(
				control is RibbonButton ||
				control is RibbonToggleButton ||
				control is RibbonGroup);
		}

		void XamlControlRetriever_OnControlProcessing(object sender, RetrievedControlEventArgs e)
		{
			var control = sender as DependencyObject;
			if (control == null)
			{
				return;
			}

			if (isNonTranslatableControl(control))
			{
				nonWrappedComponents.Add(formatComponentResolutionForUnWrapped(e.ControlFullName, control.GetType().Name));
			}
			if (typeof(MultilingualTextLabel).IsAssignableFrom(control.GetType()))
			{
				var multilingualControl = control as MultilingualTextLabel;

				if (isAddNonTranslatableForMultilingualControl(multilingualControl))
				{
					nonWrappedComponents.Add(formatComponentResolutionForUnTranslatable(e.ControlFullName, control.GetType().Name, multilingualControl.ContentString));
				}
				else if (!isMultilingualTextExistInResource(multilingualControl))
				{
					var key = ((ResourceString)multilingualControl.MultilingualText).ResourceKey;
					nonWrappedComponents.Add(formatComponentResolutionForResourceStringNotExist(e.ControlFullName, control.GetType().Name, key, multilingualControl.ContentString));
				}
			}
		}

		bool isNonTranslatableControl(DependencyObject control)
		{
			return control.GetType() == typeof(Label) || control.GetType() == typeof(TextBlock);
		}

		bool isAddNonTranslatableForMultilingualControl(MultilingualTextLabel control)
		{
			if (control.IsTranslatable)
			{
				var binding = control.GetBindingExpression(MultilingualTextLabel.MultilingualTextProperty);
				return (binding == null && control.MultilingualText == null) || typeof(NoResString).Equals(control.MultilingualText);
			}
			return false;
		}

		bool isMultilingualTextExistInResource(MultilingualTextLabel control)
		{
			if (control.MultilingualText != null && control.IsTranslatable)
			{
				return readonlyResourceStrings.AllKeys.Any(key => key == ((ResourceString)control.MultilingualText).ResourceKey);
			}
			return true;
		}

		string formatComponentResolutionForUnWrapped(string fullPath, string componentType)
		{
			return string.Format(
				"{0} in {1} must be wrapped with the MultilingualTextLabel control.",
				componentType, fullPath, System.Environment.NewLine);
		}

		string formatComponentResolutionForUnTranslatable(string fullPath, string componentType, string labelContent)
		{
			return string.Format(
				"{0} in {1} is not translatable, if you think the content is used for English only, please set its IsTranslatable to false. Content: {2}",
				componentType, fullPath, labelContent);
		}

		string formatComponentResolutionForResourceStringNotExist(string fullPath, string componentType, string key, string labelContent)
		{
			return string.Format(
				"{0} in {1} has a resource key: {2}, but it does not exist int the resource file. Consider bind it to its MultilingualText property. Content: {3}",
				componentType, fullPath, key, labelContent);
		}

		readonly ISimpleResourceStringCache readonlyResourceStrings = ResourceStringsResearch.Instance.GetSystemDefinedResourceStrings(Res.DefaultLanguage);

		#endregion

		#region TestDataContextEitherIsNotBusinessObjectOrImplementsINotifyPropertyChange

		List<string> componentsThatCanCauseMemoryLeaks;
		bool dataContextIsSpecifiedAtSomeLevel;

		[GuiTest]
		[RequiresSTA]
		public void TestDataContextEitherIsNotBusinessObjectOrImplementsINotifyPropertyChanged()
		{
			if (Attribute.IsDefined(GetType(), typeof(SuppressDataContextCheckAttribute)))
			{
				Assert(true);
				return;
			}

			componentsThatCanCauseMemoryLeaks = new List<string>();
			dataContextIsSpecifiedAtSomeLevel = false;
			var testForm = GetControlToBash();
			RetrieveAndProcessEachControl(testForm, XamlControlRetriever_OnControlProcessing_ForMemoryLeaks);

			AssertEquals(
				string.Format(
					@"WPF Control {0} has no specified DataContext.
					If you want the control to have DataContext, make sure it implements INotifyPropertyChanged.
					If you want the control to have no DataContext, decorate the test with the [SuppressDataContextCheck] attribute.",
					testForm.GetType().FullName),
				true, dataContextIsSpecifiedAtSomeLevel);

			AssertEquals(
				string.Format(
					"WPF Control {1} has controls bound to business objects not implementing INotifyPropertyChanged which may cause memory leaks (see https://support.microsoft.com/en-au/kb/938416 for detail):{0}{0}{2}",
					/*0*/ Environment.NewLine,
					/*1*/ testForm.GetType().FullName,
					/*2*/ string.Join(System.Environment.NewLine, componentsThatCanCauseMemoryLeaks.ToArray())),
				0, componentsThatCanCauseMemoryLeaks.Count);
		}

		void XamlControlRetriever_OnControlProcessing_ForMemoryLeaks(object sender, RetrievedControlEventArgs e)
		{
			var element = sender as FrameworkElement;
			if (element != null)
			{
				if (element.DataContext != null)
				{
					dataContextIsSpecifiedAtSomeLevel = true;
				}

				if (DoesControlRefersToDataContextWhichCanCauseMemoryLeaks(element))
				{
					componentsThatCanCauseMemoryLeaks.Add(FormatComponentResolutionForMemoryLeak(e.ControlFullName, element.GetType().Name, element.DataContext.GetType().Name));
				}
			}
		}

		bool DoesControlRefersToDataContextWhichCanCauseMemoryLeaks(FrameworkElement element)
		{
			return element != null && element.DataContext is BusinessObject && !(element.DataContext is INotifyPropertyChanged) && DoesDataContextDifferFromInheritedDataContext(element);
		}

		bool DoesDataContextDifferFromInheritedDataContext(FrameworkElement element)
		{
			var parent = FindParentFrameworkElement(element);
			return parent == null || parent.DataContext != element.DataContext;
		}

		public static FrameworkElement FindParentFrameworkElement(DependencyObject childElement)
		{
			var parentElement = VisualTreeHelper.GetParent(childElement);

			if (parentElement != null)
			{
				FrameworkElement parent = parentElement as FrameworkElement;
				return parent ?? FindParentFrameworkElement(parentElement);
			}
			return null;
		}

		string FormatComponentResolutionForMemoryLeak(string fullPath, string componentType, string dataContextType)
		{
			return string.Format(
				"{0} in {1} is bound to the {2} class, which is a business object not implementing INotifyPropertyChanged.",
				componentType, fullPath, dataContextType);
		}

		#endregion

		#region INotifications

		internal void AddError(string message)
		{
			Failures.Add(AddExtraDebuggingMessage(message));
		}

		#endregion

		#region Implementation

		void RetrieveAndProcessEachControl(Control testForm, EventHandler<RetrievedControlEventArgs> onControlProcessingHandler, Func<DependencyObject, bool> processElementDecider = null)
		{
			var window = new Window();
			if (testForm is Window)
			{
				window = (Window)testForm;
			}
			else
			{
				window.Content = testForm;
			}

			window.Show();
			var xamlControlRetriever = new XamlEmbeddedControlRetriever();
			xamlControlRetriever.OnControlProcessing += onControlProcessingHandler;
			xamlControlRetriever.ProcessElementDecider = processElementDecider;
			xamlControlRetriever.Retrieve(testForm);

			window.Close();
		}

		protected abstract Control GetControlToBashCore();

		protected virtual string AddExtraDebuggingMessage(string message)
		{
			return message;
		}

		protected internal string[] GetFailureMessages()
		{
			List<string> result = new List<string>();
			result.AddRange(Failures);
			return result.ToArray();
		}

		internal void ReportExceptions()
		{
			string errors = string.Join("<BR><BR>\r\n", GetFailureMessages());
			if (!string.IsNullOrEmpty(errors))
			{
				HtmlFail(errors + "<BR><BR>");
			}
		}

		readonly List<string> Failures = new List<string>();

		#endregion
	}
}
