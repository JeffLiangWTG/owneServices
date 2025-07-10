using System;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	#region Time Edit Helper

	internal class ZTimeEditExHelper : ZTimeEditHelper
	{
		public ZTimeEditExHelper()
		{
			MaximumHours = 999;
		}

		public ZTimeEditExHelper(int maxHours)
		{
			MaximumHours = maxHours;
		}

		internal override int MaximumHours { get; }
	}

	#endregion

	internal class ZTimeEditExCore : ZTimeEditCore
	{
		internal ZTimeEditExCore(ZTimeEditEx editor) : base(editor)
		{
		}

		protected override ZTimeEditHelper GetNewHelper()
		{
			return new ZTimeEditExHelper((int)Math.Min(Math.Pow(10, Editor.HoursDigitCount) - 1, int.MaxValue));
		}
	}
}
