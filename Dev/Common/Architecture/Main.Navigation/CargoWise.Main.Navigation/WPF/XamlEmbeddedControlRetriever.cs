using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Media;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Main.Navigation.WPF;

public class XamlEmbeddedControlRetriever
{
	public event EventHandler<RetrievedControlEventArgs> OnControlProcessing;

	public Func<DependencyObject, bool> ProcessElementDecider;

	List<Type> defaultControlContentSkip;
	public List<Type> ControlContentSkip
	{
		get
		{
			if (defaultControlContentSkip == null)
			{
				defaultControlContentSkip = new List<Type>() { typeof(MultilingualTextLabel) };
			}
			return defaultControlContentSkip;
		}
		set
		{
			defaultControlContentSkip = value;
		}
	}

#if DEBUG
	public static List<string> CheckMultilingualControl(DependencyObject control)
	{
		var errors = new List<string>();
		if (control == null)
		{
			return errors;
		}

		var xamlControlRetriever = new XamlEmbeddedControlRetriever();
		xamlControlRetriever.OnControlProcessing += (sender, e) =>
		{
			if (typeof(MultilingualTextLabel).IsAssignableFrom(sender.GetType()))
			{
				var multilingualWPFControl = sender as MultilingualTextLabel;
				if (!multilingualWPFControl.IsTranslatable || !multilingualWPFControl.IsVisible)
				{
					return;
				}

				var binding = multilingualWPFControl.GetBindingExpression(MultilingualTextLabel.MultilingualTextProperty);
				if (binding != null)
				{
					if (multilingualWPFControl.MultilingualText == null || typeof(NoResString).IsAssignableFrom(multilingualWPFControl.MultilingualText.GetType()))
					{
						errors.Add(string.Format(CultureInfo.InvariantCulture, "The MultilingualTextLabel in {0} has its MultilingualText property binds to a null or non-ResString object", e.ControlFullName));
					}
				}
			}
		};

		xamlControlRetriever.Retrieve(control);
		return errors;
	}

	const double allowDeviationValue = 1.0;

	public static List<string> CheckControlPosition(DependencyObject control)
	{
		var errors = new List<string>();

		if (control == null)
		{
			return errors;
		}

		var xamlControlRetriever = new XamlEmbeddedControlRetriever();
		xamlControlRetriever.OnControlProcessing += (sender, e) =>
		{
			var xamlControl = sender as FrameworkElement;
			if (xamlControl != null)
			{
				var dpObject = sender as DependencyObject;
				if (dpObject != null)
				{
					var parent = VisualTreeHelper.GetParent(dpObject) as FrameworkElement;
					if (xamlControl.IsVisible && parent != null && !ControlIsAllowedToGoOutsideBoundsOfParent(xamlControl, parent))
					{
						var vector = VisualTreeHelper.GetOffset(xamlControl);

						// because the method GetOffset has deviation, so cannot directly compare whether (positon.x + width > parent.width). The height is the same case.
						if ((vector.X < 0 || vector.Y < 0 || vector.X + xamlControl.ActualWidth - parent.ActualWidth > allowDeviationValue || vector.Y + xamlControl.ActualHeight - parent.ActualHeight > allowDeviationValue))
						{
							var errorMessage = string.Format(CultureInfo.InvariantCulture, "The control '{0}' is outside the bounds of the parent control: \r\n{1}", e.ControlFullName, GetControlDescriptionWithParentAndItsBounds(xamlControl));
							errors.Add(errorMessage);
						}
					}
				}
			}
		};

		xamlControlRetriever.Retrieve(control);

		return errors;
	}

	static string GetControlDescriptionWithParentAndItsBounds(DependencyObject control)
	{
		var description = string.Empty;
		if (control != null)
		{
			var visualParent = VisualTreeHelper.GetParent(control);
			description = string.Format(CultureInfo.InvariantCulture, "Control: {0} \r\n Visual Parent: {1}", GetControlDescription(control), GetControlDescription(visualParent));
		}

		return description;
	}

	static string GetControlDescription(DependencyObject control)
	{
		var description = string.Empty;
		var frameElement = control as FrameworkElement;
		if (frameElement != null)
		{
			var vector = VisualTreeHelper.GetOffset(frameElement);
			description = string.Format(CultureInfo.InvariantCulture, "{0} ({1}) >>> Bounds: {{X: {2}, Y: {3}, ActualWidth: {4}, ActualHeight: {5} }}",
																	frameElement.Name,
																	control.GetType().FullName,
																	vector.X,
																	vector.Y,
																	frameElement.ActualWidth,
																	frameElement.ActualHeight);
		}

		return description;
	}

	static bool ControlIsAllowedToGoOutsideBoundsOfParent(FrameworkElement frameworkElement, FrameworkElement parent)
	{
		if (frameworkElement != null && frameworkElement.TemplatedParent is Slider
			&& parent != null && parent.TemplatedParent is Slider
			|| WpfUtils.FindAncestor<Ribbon>(frameworkElement) != null)
		{
			return true;
		}

		return false;
	}
#endif

	public void Retrieve(DependencyObject control)
	{
		if (control == null)
		{
			return;
		}

		controlOccurTimes = new Dictionary<string, int>();
		controlFullName = new StringBuilder();
		RetrieveAllChildControls(control);
	}

	void RetrieveAllChildControls(DependencyObject control)
	{
		if (control == null)
		{
			return;
		}

		var shouldProcess = ProcessElementDecider == null || ProcessElementDecider(control);
		if (!shouldProcess)
		{
			return;
		}

		var controlName = processForEachControl(control);
		controlFullName.Append(controlName);

		var popupControl = control as Popup;
		if (popupControl != null)
		{
			var childPopupControl = popupControl.Child;
			if (childPopupControl != null)
			{
				RetrieveAllChildControls(childPopupControl);
			}
		}

		var dataTemplateControl = control as ItemsControl;
		if (dataTemplateControl != null)
		{
			var controlID = dataTemplateControl.Name;
			var dataTemplate = dataTemplateControl.ItemTemplate;
			loadControlWithinDataTemplate(dataTemplate, !string.IsNullOrEmpty(controlID) ? controlID : "");

			var templateSeparater = dataTemplateControl.ItemTemplateSelector;
			if (templateSeparater != null)
			{
				foreach (var propInfo in templateSeparater.GetType().GetProperties())
				{
					if (propInfo.PropertyType.IsAssignableFrom(typeof(DataTemplate)))
					{
						dataTemplate = propInfo.GetValue(templateSeparater) as DataTemplate;
						loadControlWithinDataTemplate(dataTemplate, propInfo.Name);
					}
				}
			}
		}

		if (!ControlContentSkip.Contains(control.GetType()))
		{
			for (var index = 0; index < VisualTreeHelper.GetChildrenCount(control); ++index)
			{
				var child = VisualTreeHelper.GetChild(control, index);
				RetrieveAllChildControls(child);
			}
		}

		if (controlFullName.Length >= controlName.Length)
		{
			controlFullName.Remove(controlFullName.Length - controlName.Length, controlName.Length);
		}
	}

	void loadControlWithinDataTemplate(DataTemplate dataTemplate, string appendDataTemlateInfo)
	{
		if (dataTemplate == null)
		{
			return;
		}

		if (!string.IsNullOrEmpty(appendDataTemlateInfo))
		{
			appendDataTemlateInfo += ";";
		}

		controlFullName.Append(appendDataTemlateInfo);

		var control = dataTemplate.LoadContent();
		RetrieveAllChildControls(control);

		if (!string.IsNullOrEmpty(appendDataTemlateInfo) && controlFullName.Length >= appendDataTemlateInfo.Length)
		{
			controlFullName.Remove(controlFullName.Length - appendDataTemlateInfo.Length, appendDataTemlateInfo.Length);
		}
	}

	string processForEachControl(DependencyObject control)
	{
		Argument.NotNull(control, nameof(control));

		var controlName = appendControlIndexIfOccurMoreThanOnce(controlFullName.ToString(), control.GetType().Name);
		var fullControlName = controlFullName.ToString() + controlName.TrimEnd('.');
		if (OnControlProcessing != null)
		{
			OnControlProcessing(control, new RetrievedControlEventArgs(fullControlName));
		}
		return controlName;
	}

	string appendControlIndexIfOccurMoreThanOnce(string pathName, string controlName)
	{
		string result = controlName;
		var fullName = pathName + controlName;
		if (controlOccurTimes.ContainsKey(fullName))
		{
			var occurTimes = controlOccurTimes[fullName] + 1;
			controlOccurTimes[fullName] = occurTimes;

			result += "`" + occurTimes;
		}
		else
		{
			controlOccurTimes[fullName] = 1;
		}
		return result + ".";
	}

	StringBuilder controlFullName = new StringBuilder();
	Dictionary<string, int> controlOccurTimes = new Dictionary<string, int>();
}

public class RetrievedControlEventArgs : EventArgs
{
	public RetrievedControlEventArgs(string controlFullName)
	{
		ControlFullName = controlFullName;
	}
	public string ControlFullName { get; set; }
}
