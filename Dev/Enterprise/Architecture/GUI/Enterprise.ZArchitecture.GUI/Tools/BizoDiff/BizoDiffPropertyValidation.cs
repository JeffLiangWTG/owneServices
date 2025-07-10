using System;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.DevTools
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Developer tool")]
	public sealed class BizoPropertyValidation : ZValidation
	{
		public BizoPropertyValidation(BizoProperty parent) : base(parent)
		{
			Parent = parent;
		}

		readonly BizoProperty Parent;

		public override Type AutoValidationType => typeof(BizoProperty);

		public override void ValidateAll()
		{
			if (Parent.NeedCompare && !Parent.IsSameValue)
			{
				Parent.ValueInfo.AddWarning("Different values found.");
			}
		}
	}
}
