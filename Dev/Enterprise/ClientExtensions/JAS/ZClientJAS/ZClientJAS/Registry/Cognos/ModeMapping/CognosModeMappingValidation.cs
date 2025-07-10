using System;

using CargoWise.EntityFramework;

namespace Enterprise.Client.JAS.Registry.Business
{
	public class CognosModeMappingValidation : ZValidation
	{
		public CognosModeMappingValidation(CognosModeMapping parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		public override Type AutoValidationType
		{
			get { return typeof(CognosModeMappingValidation); }
		}

		public override void ValidateAll()
		{
			ValidateSelectedMode();
		}

		public void ValidateSelectedMode()
		{
			ValidateCalculatedProperty(Parent.SelectedModeInfo);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Method called via reflection for test ref<https://devops.wisetechglobal.com/wtg/CargoWise/_git/Dev?path=%2FEnterprise%2FClientExtensions%2FJAS%2FZClientJAS%2FZClientJAS.Test%2FRegistry%2FCognos%2FModeMapping%2FCognosModeMappingValidationTest.cs&version=GBFGH%2FWI00621362%2FEnterprise.ZArchitecture.GUI.UserControls-Test-Extraction&line=27&lineEnd=27&lineStartColumn=4&lineEndColumn=88&lineStyle=plain&_a=contents>")]
		void CheckSelectedMode()
		{
			MandatoryValidation.CheckEntered(Parent.SelectedModeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.SelectedModeInfo, Parent.Modes);
		}

		readonly CognosModeMapping Parent;
	}
}
