using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Core.Forms
{
	/// <summary>
	/// Provides means of changing back color wih respect to color priorities.
	/// Color priority:
	///	* active
	///	* notification 
	///	* readonly
	///	* explicit
	///	 </summary>
	public class ActiveControlColorChanger : IDisposable
	{
		readonly Control control;
		readonly ColorState[] statesInPriorityOrder;

		bool selfChange;

		public ActiveControlColorChanger(Control control)
		{
			this.control = control;

			statesInPriorityOrder = new[]
			{
				new ColorState { ID = ColorID.Explicit, Color = control.BackColor },
				new ColorState { ID = ColorID.Notification },
				new ColorState { ID = ColorID.Valid, Color = Color.FromArgb(198, 236, 198) },
				new ColorState { ID = ColorID.ReadOnly, Color = SystemColors.Control },
				new ColorState { ID = ColorID.Active, Color = EnterpriseFormLookStrategy.SelectedControlColor },
				new ColorState { ID = ColorID.ControlBackColor, Color = control.BackColor, On = true }, // ultimate fallback
			};

			if (!DesignModeFinder.IsDesigning)
			{
				control.GotFocus += OnGotFocus;
				control.LostFocus += OnLostFocus;
				control.BackColorChanged += OnBackColorChanged;
			}
		}

		public void Dispose()
		{
			if (!DesignModeFinder.IsDesigning)
			{
				control.GotFocus -= OnGotFocus;
				control.LostFocus -= OnLostFocus;
				control.BackColorChanged -= OnBackColorChanged;
			}
		}

		public void SetNotificationBackColor(Color color)
		{
			SetState(ColorID.Notification, true, color);
		}

		public void ClearNotificationBackColor()
		{
			ClearState(ColorID.Notification);
		}

		internal void SetActiveBackColor()
		{
			SetState(ColorID.Active, true, null);
		}

		public void ClearActiveBackColor()
		{
			ClearState(ColorID.Active);
		}

		public void SetReadonlyColor()
		{
			SetReadonlyColor(true);
		}

		public void SetReadonlyColor(bool value)
		{
			SetState(ColorID.ReadOnly, value, null);
		}

		public void ClearReadonlyColor()
		{
			SetReadonlyColor(false);
		}

		public void ForceBackColor(Color color)
		{
			var state = GetExplicitState();
			state.Color = color;
			state.On = true;
			ChangeColor(color);
		}

		public void ResetForcedColor()
		{
			var state = GetExplicitState();
			state.On = false;

			if (control.Focused)
			{
				OnGotFocus(control, EventArgs.Empty);
			}
			else
			{
				OnLostFocus(control, EventArgs.Empty);
			}
		}

		ColorState GetExplicitState()
		{
			return Array.Find(statesInPriorityOrder, x => x.ID == ColorID.Explicit);
		}

		public void SetValidColorIfNeeded()
		{
			var colorMutator = control as IBackColorMutable;
			if (colorMutator != null && colorMutator.EnableValidStateColor)
			{
				SetState(ColorID.Valid, true, null);
			}
		}

		public void ClearValidColorIfNeeded()
		{
			var colorMutator = control as IBackColorMutable;
			if (colorMutator != null && colorMutator.EnableValidStateColor)
			{
				ClearState(ColorID.Valid);
			}
		}

		#region Implementation

		void OnGotFocus(object sender, EventArgs e)
		{
			SetActiveBackColor();
		}

		void OnLostFocus(object sender, EventArgs e)
		{
			ClearActiveBackColor();
		}

		void OnBackColorChanged(object sender, EventArgs e)
		{
			if (!selfChange)
			{
				SetState(ColorID.ControlBackColor, true, control.BackColor);
			}
		}

		void ChangeColor(Color color)
		{
			selfChange = true;
			control.BackColor = color;
			selfChange = false;
		}

		void SetState(ColorID id, bool on, Color? color)
		{
			var state = Array.Find(statesInPriorityOrder, x => x.ID == id);

			state.Color = color ?? state.Color;
			state.On = on;

			ApplyColor();
		}

		void ClearState(ColorID id)
		{
			SetState(id, false, null);
		}

		void ApplyColor()
		{
			var state = Array.Find(statesInPriorityOrder, x => x.On); // this will find explicit state as it is always on
			ChangeColor(state.Color);
		}

		#endregion

		enum ColorID
		{
			Explicit,
			Active,
			Notification,
			Valid,
			ReadOnly,
			ControlBackColor,
		}

		[DebuggerDisplay("ID={ID}")]
		class ColorState
		{
			public ColorID ID;
			public Color Color;
			public bool On;
		}
	}
}
