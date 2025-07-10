using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Input;
using CargoWise.Windows.UI;
using Enterprise.Core.Environment;

namespace Enterprise.ZArchitecture.GUI
{
	public partial class ZKeyStatusForm : KForm
	{
		public static ZKeyStatusForm Instance => instance ?? (instance = new ZKeyStatusForm());
		[SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Single instance")]
		static ZKeyStatusForm instance;

		ZKeyStatusForm()
		{
			InitializeComponent();
			SetBackgroundColor(zButtonCaps, Keyboard.IsKeyToggled(Key.CapsLock));
			SetBackgroundColor(zButtonCtrl, Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
			SetBackgroundColor(zButtonAlt, Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt));
			SetBackgroundColor(zButtonShift, Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift));
			zButtonReset.Click += ZButtonReset_Click;
			FormClosed += ZKeyStatusForm_FormClosed;
		}

		void ZButtonReset_Click(object sender, EventArgs e)
		{
			ResetKeyStatus();
		}

		void ZKeyStatusForm_FormClosed(object sender, FormClosedEventArgs e)
		{
			instance?.Dispose();
			instance = null;
		}

		void ResetKeyStatus()
		{
			var currentStates = GetKeyStates();
			if (currentStates != null)
			{
				var hasError = ResetKeyAndBackground(vk_CAPITAL, zButtonCaps, currentStates)
					|| ResetKeyAndBackground(vk_CONTROL, zButtonCtrl, currentStates)
					|| ResetKeyAndBackground(vk_ALT, zButtonAlt, currentStates)
					|| ResetKeyAndBackground(vk_SHIFT, zButtonShift, currentStates);

				if (hasError)
				{
					UserNotification.Instance.ShowError(setStateErrorMessage);
				}
			}
		}

		bool ResetKeyAndBackground(int index, ZButton button, byte[] currentStates)
		{
			var hasError = SetKeyState(index, currentStates, 0) == 0;
			if (!hasError)
			{
				SetBackgroundColor(button, false);
			}
			return hasError;
		}

		byte[] GetKeyStates()
		{
			var keystates = new byte[256];
			if (ZGrid.GetKeyboardState(keystates) == 0)
			{
				UserNotification.Instance.ShowError(getStateErrorMessage);
				return null;
			}
			return keystates;
		}

		int SetKeyState(int keyIndex, byte[] currentStates, byte newStatusByte)
		{
			currentStates[keyIndex] = newStatusByte;
			return ZGrid.SetKeyboardState(currentStates);
		}

		void SetBackgroundColor(ZButton button, bool state)
		{
			button.BackColor = state ? Color.Green : BackColor;
		}

#pragma warning disable IDE0044 //Conflicting warnings (conflict between IDE0044 and Res.GetString) suppressing the latest one, i.e., IDE0044
		string getStateErrorMessage = Res.GetString("0F9431E4-683C-4495-8408-D555E9580994", "Failed to get key states.");
		string setStateErrorMessage = Res.GetString("9D35F57C-6747-4080-9276-63E2BBDD56E0", "Failed to set key states.");
#pragma warning restore IDE0044 //Restoring IDE0044
		const int vk_CAPITAL = 0x14;
		const int vk_CONTROL = 0x11;
		const int vk_ALT = 0x12;
		const int vk_SHIFT = 0x10;
	}
}
