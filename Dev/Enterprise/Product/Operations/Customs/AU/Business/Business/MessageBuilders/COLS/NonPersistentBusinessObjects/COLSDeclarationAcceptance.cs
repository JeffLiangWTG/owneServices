using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class COLSDeclarationAcceptance : NonPersistentBusinessObject
	{
		public COLSDeclarationAcceptance()
		{
		}

		#region Property

		public ZBool DeclarationAcceptance
		{
			get { return declarationAcceptance; }
			set
			{
				SetNonPersistentPropertyValue(DeclarationAcceptanceInfo, ref declarationAcceptance, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDeclarationAcceptance();
				}
			}
		}
		ZBool declarationAcceptance;

		public ZPropertyInfo DeclarationAcceptanceInfo => GetZPropertyInfo(nameof(DeclarationAcceptance));

		#endregion

		#region Validation

		public COLSDeclarationAcceptanceValidation Validation => new COLSDeclarationAcceptanceValidation(this);

		#endregion
	}
}
