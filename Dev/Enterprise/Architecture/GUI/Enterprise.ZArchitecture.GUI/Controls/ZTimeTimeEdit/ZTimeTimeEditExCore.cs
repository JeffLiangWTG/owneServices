using System;

namespace Enterprise.ZArchitecture.GUI.Internal
{
	#region Time Edit Helper

	internal class ZTimeTimeEditExHelper : ZTimeTimeEditHelper
	{
		public ZTimeTimeEditExHelper()
		{
			MaximumHours = 999;
		}

		public ZTimeTimeEditExHelper(int maxHours)
		{
			MaximumHours = maxHours;
		}

		internal override int MaximumHours { get; }
	}

	#endregion

	internal class ZTimeTimeEditExCore : ZTimeTimeEditCore
	{
		internal ZTimeTimeEditExCore(ZTimeTimeEditEx editor) : base(editor)
		{
		}

		protected override ZTimeTimeEditHelper GetNewHelper()
		{
			return new ZTimeTimeEditExHelper((int)Math.Min(Math.Pow(10, Editor.HoursDigitCount) - 1, int.MaxValue));
		}
	}
}
