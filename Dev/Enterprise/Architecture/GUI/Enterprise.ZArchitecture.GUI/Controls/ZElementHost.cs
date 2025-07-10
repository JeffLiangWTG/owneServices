using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	[ToolboxItem(false)]
	public class ZElementHost : KElementHost
	{
		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);

			if (lastForm != null)
			{
				lastForm.BeforePerformValidation -= Form_BeforePerformValidation;
			}

			var form = this.FindForm() as ZForm;
			if (form != null)
			{
				form.BeforePerformValidation += Form_BeforePerformValidation;
			}

			lastForm = form;
		}

		ZForm lastForm;

		void Form_BeforePerformValidation(object sender, EventArgs e)
		{
			UpdateFocusBinding();
		}

		void UpdateFocusBinding()
		{
			if (Child.IsKeyboardFocused || Child.IsKeyboardFocusWithin)
			{
				var element = Keyboard.FocusedElement;
				var textBlock = element as TextBlock;

				if (textBlock != null)
				{
					var expression = textBlock.GetBindingExpression(TextBlock.TextProperty);
					if (expression != null)
					{
						expression.UpdateSource();
						Keyboard.Focus(textBlock);
					}
				}

				var textBox = element as TextBox;
				if (textBox != null)
				{
					var start = textBox.SelectionStart;
					var length = textBox.SelectionLength;
					var expression = textBox.GetBindingExpression(TextBox.TextProperty);
					if (expression != null)
					{
						expression.UpdateSource();

						textBox.SelectionStart = start;
						textBox.SelectionLength = length;
					}
				}
			}
		}
	}
}
