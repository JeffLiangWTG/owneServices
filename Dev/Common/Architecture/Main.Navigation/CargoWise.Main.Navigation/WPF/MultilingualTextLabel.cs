using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CargoWise.Application;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.Main.Navigation.WPF;
public class MultilingualTextLabel : Label
{
	public MultilingualTextLabel()
	{
	}

	internal void RegisterMultilingualTextFromXAMLContent(UInt16 localizationAsmid, string localizationKeyPrefix)
	{
		if (GetBindingExpression(MultilingualTextLabel.MultilingualTextProperty) == null && IsTranslatable)
		{
			var caption = ContentString;
			if (localizationKeyPrefix != null && localizationKeyPrefix.Length < 200 && caption != null)
			{
				var key = XamlMarkupTranslator.Instance.CalculateKey(localizationKeyPrefix, caption);
				MultilingualText = ResString._GetMultilingualString(localizationAsmid, key, caption);
			}
		}
	}

	public bool InTranslationFeedBackMode
	{
		get { return (bool)(GetValue(InTranslationFeedBackModeProperty) ?? false); }
		set { SetValue(InTranslationFeedBackModeProperty, value); }
	}

	public bool IsTranslatable
	{
		get { return (bool)(GetValue(IsTranslatableProperty) ?? true); }
		set { SetValue(IsTranslatableProperty, value); }
	}

	public MultilingualString MultilingualText
	{
		get
		{
			var result = GetValue(MultilingualTextProperty);
			return result == null ? null : result as MultilingualString;
		}
		set
		{
			if (IsTranslatable && typeof(NoResString).Equals(value))
			{
				throw new InvalidOperationException("Trying to bind a string to MultilingualText property in a multilingual control.");
			}
			SetValue(MultilingualTextProperty, value);
		}
	}

	public static readonly DependencyProperty MultilingualTextProperty =
		DependencyProperty.RegisterAttached("MultilingualText", typeof(MultilingualString), typeof(MultilingualTextLabel));

	public static readonly DependencyProperty InTranslationFeedBackModeProperty =
		DependencyProperty.RegisterAttached("InTranslationFeedBackMode", typeof(bool), typeof(MultilingualTextLabel),
			new PropertyMetadata(false));

	public static readonly DependencyProperty IsTranslatableProperty =
		DependencyProperty.RegisterAttached("IsTranslatable", typeof(bool), typeof(MultilingualTextLabel),
			new PropertyMetadata(true));

	protected override void OnMouseEnter(MouseEventArgs e)
	{
		InTranslationFeedBackMode = TranslationFeedbackManager.InTranslationFeedbackMode();
		base.OnMouseEnter(e);
	}

	protected override void OnRender(DrawingContext drawingContext)
	{
		base.OnRender(drawingContext);
		if (MultilingualText != null && !HasContentBinding)
		{
			setContentString(this, MultilingualText.ToString(Res.CurrentLanguage));
		}
	}

	protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		base.OnPreviewMouseLeftButtonDown(e);

		bool isMultilingualResource = false;
		MultilingualString multiligualText = null;
		if (InTranslationFeedBackMode && IsTranslatable)
		{
			string caption = "";
			if (MultilingualText != null)
			{
				multiligualText = MultilingualText;
				isMultilingualResource = true;
			}
			else
			{
				caption = ContentString;
			}

			if (isMultilingualResource)
			{
				TranslationFeedbackManager.OpenFeedbackForm(multiligualText);
			}
			else
			{
				TranslationFeedbackManager.OpenFeedbackForm(caption);
			}

			e.Handled = true;
		}
		else
		{
			e.Handled = false;
		}
	}

	public string ContentString => getContentString(this);

	string getContentString(ContentControl control)
	{
		if (control == null)
		{
			return "";
		}

		var contentControl = control.Content as ContentControl;
		if (contentControl != null)
		{
			return getContentString(contentControl);
		}
		else if (control.Content is TextBlock)
		{
			return ((TextBlock)control.Content).Text;
		}
		else
		{
			return control.Content == null ? "" : control.Content.ToString();
		}
	}

	void setContentString(ContentControl control, string value)
	{
		if (control == null)
		{
			return;
		}

		var contentControl = control.Content as ContentControl;
		if (contentControl != null)
		{
			setContentString(contentControl, value);
		}
		else if (control.Content is TextBlock)
		{
			((TextBlock)control.Content).Text = value;
		}
		else
		{
			control.Content = value;
		}
	}

	bool HasContentBinding => hasContentBinding(this);

	bool hasContentBinding(ContentControl control)
	{
		if (control == null)
		{
			return false;
		}

		var contentControl = control.Content as ContentControl;
		if (contentControl != null)
		{
			return hasContentBinding(contentControl);
		}
		else if (control.Content is TextBlock)
		{
			return ((TextBlock)control.Content).GetBindingExpression(TextBlock.TextProperty) != null;
		}
		else
		{
			return control.GetBindingExpression(ContentProperty) != null;
		}
	}

	IWpfTranslationFeedbackManager translationFeedbackManager;
#if DEBUG
	public
#endif
	IWpfTranslationFeedbackManager TranslationFeedbackManager
	{
		get
		{
			if (translationFeedbackManager == null)
			{
				translationFeedbackManager = ObjectFactory.Get<IWpfTranslationFeedbackManager>();
			}
			return translationFeedbackManager;
		}
	}
}

public interface IWpfTranslationFeedbackManager
{
	bool InTranslationFeedbackMode();
	void OpenFeedbackForm(string caption);
	void OpenFeedbackForm(MultilingualString multilingualString);
#if DEBUG
	IDisposable EnableTranslationFeedbackModeForTest();
#endif
}
