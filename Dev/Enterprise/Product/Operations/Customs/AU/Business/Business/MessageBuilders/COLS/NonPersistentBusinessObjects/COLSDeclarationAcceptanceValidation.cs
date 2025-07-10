using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by validation via reflection")]
	public sealed class COLSDeclarationAcceptanceValidation : ZValidation
	{
		public COLSDeclarationAcceptanceValidation(COLSDeclarationAcceptance parent) : base(parent)
		{
			this.parent = parent;
		}
		readonly COLSDeclarationAcceptance parent;

		public override Type AutoValidationType => typeof(COLSDeclarationAcceptanceValidation);

		public override void ValidateAll()
		{
			ValidateDeclarationAcceptance();
		}

		public void ValidateDeclarationAcceptance()
		{
			ValidateCalculatedProperty(parent.DeclarationAcceptanceInfo);
		}

		void CheckDeclarationAcceptance()
		{
			if (!parent.DeclarationAcceptance)
			{
				parent.DeclarationAcceptanceInfo.AddMessageError("You have not ticked the check box.");
			}
		}
	}
}
